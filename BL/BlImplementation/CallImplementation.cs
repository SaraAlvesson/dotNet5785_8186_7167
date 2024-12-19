namespace BlImplementation;
 using BlApi;
using BO;
using Helpers;
using static BO.Exceptions;

internal class CallImplementation : ICall
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    public int[] GetCallsCountByStatus()
    {
        // שליפת כל הקריאות מה-DAL
        var calls = _dal.call.ReadAll(); // מניחים שיש פונקציה כזו
        // קיבוץ הקריאות לפי סטטוס
        var groupedCalls = calls

            .GroupBy(calls => (int)calls.CallStatus) // קיבוץ לפי ערך מספרי של הסטטוס
            .ToDictionary(group => group.Key, group => group.Count());

        // יצירת מערך עם כמות לכל סטטוס לפי אינדקס
        int maxStatusValue = Enum.GetValues(typeof(CallStatusEnum)).Cast<int>().Max();
        int[] result = new int[maxStatusValue + 1]; // יצירת מערך בגודל המתאים

        foreach (var group in groupedCalls)
        {
            result[group.Key] = group.Value; // הכנסת הכמות למיקום המתאים
        }

        return result;
    }


    // המתודה שתחזיר את רשימת הקריאות, תומכת גם במיון וסינון
    public IEnumerable<CallInList> GetCallList(
          Enums.CallFieldEnum? filterField = null,
          object filterValue = null,
          Enums.CallFieldEnum? sortField = null)
        {
            // שליפת כל הקריאות
            var calls = _dal.call.ReadAll()
                .GroupBy(c => c.Id) // קיבוץ לפי CallId כדי להבטיח קריאה אחת עם ההקצאה האחרונה
                .Select(callGroup => callGroup.OrderByDescending(c => c.OpenTime).First()) // הקריאה האחרונה
                .Select(c => new BO.CallInList
                {
                    Id = c.Id,
                    CallId = c.Id,
                    CallType = (Enums.CallType)c.CallType,
                    OpenTime = c.OpenTime,
                    SumTimeUntilFinish = c.MaxTime.HasValue ? (c.MaxTime - c.OpenTime) : (TimeSpan?)null, // הזמן שנותר לסיום הקריאה
                    LastVolunteerName = _dal.assignment.ReadAll()
                        .Where(a => a.CallId == c.Id)
                        .OrderByDescending(a => a.AppointmentTime)
                        .Select(a => a.FinishAppointmentTime.HasValue ? $"Volunteer {a.VolunteerId}" : "No Volunteer")
                        .FirstOrDefault() ?? "No Volunteer",
                    SumAppointmentTime = GetTotalAppointmentTime(c.Id), // חישוב זמן ההקצאות
                    Status = (Enums.CallStatusEnum)c.CallType, // הצגת סטטוס הקריאה
                    SumAssignment = _dal.assignment.ReadAll().Count(a => a.CallId == c.Id)
                }).ToList();

            // סינון
            if (filterField.HasValue && filterValue != null)
            {
                calls = calls.Where(call =>
                    GetFieldValue(call, filterField.Value)?.Equals(filterValue) ?? false).ToList();
            }

            // מיון
            if (sortField.HasValue)
            {
                calls = calls.OrderBy(call => GetFieldValue(call, sortField.Value)).ToList();
            }
            else
            {
                // מיון ברירת מחדל לפי מספר קריאה
                calls = calls.OrderBy(call => call.CallId).ToList();
            }

            return calls;
        }

        //// פונקציה לחישוב זמן ההקצאות
        //private TimeSpan GetTotalAppointmentTime(int callId)
        //{
        //    TimeSpan totalTime = TimeSpan.Zero;
        //    foreach (var a in _dal.assignment.ReadAll().Where(a => a.CallId == callId))
        //    {
        //        if (a.FinishAppointmentTime.HasValue)
        //        {
        //            totalTime += a.FinishAppointmentTime.Value - a.AppointmentTime;
        //        }
        //    }
        //    return totalTime;
        //}

        public BO.Call GetCallDetails(int callId)
        {
            // שליפת הקריאה משכבת הנתונים
            var call = _dal.call.Read(callId);
            if (call == null)
            {
                throw new Exception($"Call with ID {callId} not found.");
            }

            // שליפת ההקצאות משכבת הנתונים
            var assignments = _dal.assignment.ReadAll()
                .Where(a => a.CallId == call.Id) // מסנן את ההקצאות לפי מזהה הקריאה
                .ToList();

            // בדיקה אם יש הקצאה רלוונטית
            var relevantAssignment = assignments.FirstOrDefault();
            if (relevantAssignment == null)
            {
                throw new Exception($"No assignments found for Call ID {callId}.");
            }

            // קביעת סטטוס הקריאה לפי FinishAppointmentType
            Enums.CallStatusEnum callStatus;
            switch (relevantAssignment.FinishAppointmentType)
            {
                case FinishAppointmentType.WasTreated:
                    callStatus = Enums.CallStatusEnum.CLOSED;
                    break;

                case FinishAppointmentType.SelfCancellation:
                    callStatus = Enums.CallStatusEnum.CLOSED;
                    break;

                case FinishAppointmentType.CancelingAnAdministrator:
                    callStatus = Enums.CallStatusEnum.CLOSED;
                    break;

                case FinishAppointmentType.CancellationHasExpired:
                    callStatus = Enums.CallStatusEnum.EXPIRED;
                    break;

                default:
                    callStatus = Enums.CallStatusEnum.OPEN;
                    break;
            }

            // יצירת אובייקט BO.Call
            return new BO.Call
            {
                Id = call.Id,
                CallType = (Enums.CallType)call.CallType,
                VerbDesc = call.VerbDesc,
                Address = call.Address,
                Latitude = call.Latitude,
                Longitude = call.Longitude,
                OpenTime = call.OpenTime,
                MaxFinishTime = call.MaxTime,
                CallStatusEnum = callStatus, // כאן אנחנו מוסיפים את הסטטוס שחושב
                CallAssignInLists = assignments.Select(a => new BO.CallAssignInList
                {
                    VolunteerId = a.VolunteerId,
                    OpenTime = a.AppointmentTime,
                    RealFinishTime = a.FinishAppointmentTime,
                    FinishTreatmentType = a.FinishAppointmentType // לא נשכח את סוג סיום הטיפול
                }).ToList()
            };
        }

        public void UpdateCall(BO.Call call)
        {
            // בדיקות תקינות
            if (call.MaxFinishTime < call.OpenTime)
            {
                throw new Exception("MaxFinishTime must be greater than OpenTime.");
            }

            // שלב 6: עדכון קווי האורך והרוחב במקרה של שינוי כתובת
            var coordinates = Tools.GetGeolocationCoordinates(call.Address); // מניחים שיש פונקציה כזו

            // בדיקת תקינות כתובת (אם יש צורך, למשל, באמצעות API של כתובת)
            if (string.IsNullOrWhiteSpace(call.Address) || call.Latitude != coordinates[0] || call.Longitude != coordinates[1])
            {
                throw new Exception("Invalid address or coordinates.");
            }

            // המרת BO.Call ל-DO.Call
            var doCall = new DO.Call
            {
                Id = call.Id,
                VerbDesc = call.VerbDesc,
                Address = call.Address,
                Latitude = call.Latitude,
                Longitude = call.Longitude,
                OpenTime = call.OpenTime,
                MaxTime = call.MaxFinishTime,
            };

            // שליפת הקריאה מה-DAL
            var existingCall = _dal.call.Read(call.Id);
            if (existingCall == null)
            {
                throw new Exception($"Call with ID {call.Id} not found.");
            }

            // ניסיון לבצע עדכון בשכבת הנתונים
            try
            {
                _dal.call.Update(doCall);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update call with ID {call.Id}.", ex);
            }
        }

    // מחיקת קריאה
    public void DeleteCall(int callId)
    {
        try
        {
            BO.Call c = CallManager.GetCall(callId);
            if (c.CallStatus == CallStatus.AtRisk || c.CallStatus == CallStatus.open)
            {
                _dal.call.Delete(callId);
            }
            else
                throw new BlDoesNotExistException();
        }
        catch (Exception ex)
        {
            throw new BlDeletionImpossible();
        }

    }

    public void AddCall(BO.Call call)
        {
            // בדיקת תקינות זמן סיום מקסימלי
            if (call.MaxFinishTime < call.OpenTime)
            {
                throw new Exception("MaxFinishTime must be greater than OpenTime.");
            }

            // שלב 6: עדכון קווי האורך והרוחב במקרה של שינוי כתובת
            var coordinates = Tools.GetGeolocationCoordinates(call.Address); // מניחים שיש פונקציה כזו

            // בדיקת תקינות כתובת ו/או קואורדינטות
            if (string.IsNullOrWhiteSpace(call.Address))
            {
                throw new Exception("Address cannot be empty or null.");
            }

            if (coordinates == null || coordinates.Length != 2 ||
                call.Latitude != coordinates[0] || call.Longitude != coordinates[1])
            {
                throw new Exception("Invalid address or coordinates.");
            }

            // המרת BO.Call ל-DO.Call
            var doCall = new DO.Call
            {
                Id = call.Id,
                VerbDesc = call.VerbDesc,
                Address = call.Address,
                Latitude = call.Latitude,
                Longitude = call.Longitude,
                OpenTime = call.OpenTime,
                MaxTime = call.MaxFinishTime,
            };

            // ניסיון להוסיף את הקריאה
            try
            {
                _dal.call.Create(doCall);
            }
            catch (Exception ex)
            {
                // לוג של שגיאה לפני זריקת החריגה
                // ניתן להוסיף כאן לוגים נוספים לפי הצורך
                throw new Exception("Failed to add new call.", ex);
            }
        }


        public IEnumerable<ClosedCallInList> GetClosedCallsByVolunteer(
     string volunteerId,
     Enums.CallType? callType = null,
     Enums.CallFieldEnum? sortField = null
 )
        {
            // שליפת כל ההקצאות של המתנדב
            var assignments = _dal.assignment.ReadAll()
                .Where(a => a.VolunteerId == volunteerId && a.FinishAppointmentTime.HasValue)
                .ToList();

            // סינון לפי סוג הקריאה אם נדרש
            if (callType.HasValue)
            {
                assignments = assignments.Where(a => (Enums.CallType)a.Call.CallType == callType.Value).ToList();
            }

            // יצירת רשימת קריאות סגורות
            var closedCalls = assignments.Select(a => new BO.ClosedCallInList
            {
                CallId = a.CallId,
                VolunteerId = a.VolunteerId,
                FinishTreatmentTime = a.FinishAppointmentTime,
                Status = (Enums.CallStatusEnum)a.Call.FinishAppointmentType // מציין את סוג סיום הקריאה
            }).ToList();

            // מיון לפי השדה שנבחר אם נדרש
            if (sortField.HasValue)
            {
                closedCalls = closedCalls.OrderBy(call => GetFieldValue(call, sortField.Value)).ToList();
            }

            return closedCalls;
        }

        public IEnumerable<OpenCallInList> GetOpenCallsForVolunteer(
            string volunteerId,
            Enums.CallType? callType = null,
            Enums.CallFieldEnum? sortField = null
        )
        {
            // שליפת כל הקריאות הפתוחות
            var calls = _dal.call.ReadAll()
                .Where(c => c.Status == Enums.CallStatusEnum.OPEN)
                .ToList();

            // סינון לפי סוג הקריאה אם נדרש
            if (callType.HasValue)
            {
                calls = calls.Where(c => (Enums.CallType)c.CallType == callType.Value).ToList();
            }

            // יצירת רשימת קריאות פתוחות שניתן להקצות למתנדב
            var openCalls = calls.Select(c => new BO.OpenCallInList
            {
                CallId = c.Id,
                CallType = (Enums.CallType)c.CallType,
                OpenTime = c.OpenTime,
                MaxFinishTime = c.MaxTime
            }).ToList();

            // מיון לפי השדה שנבחר אם נדרש
            if (sortField.HasValue)
            {
                openCalls = openCalls.OrderBy(call => GetFieldValue(call, sortField.Value)).ToList();
            }

            return openCalls;
        }

        public void MarkCallAsCompleted(string volunteerId, int assignmentId)
        {
            // שליפת המשימה מה-DAL
            var assignment = _dal.assignment.Read(assignmentId);
            if (assignment == null)
            {
                throw new Exception($"Assignment with ID {assignmentId} not found.");
            }

            // בדיקת סטטוס הקריאה
            var call = _dal.call.Read(assignment.CallId);
            if (call == null || call.Status != Enums.CallStatusEnum.OPEN)
            {
                throw new Exception("Call is not open and cannot be marked as completed.");
            }

            // עדכון סיום הטיפול במשימה
            assignment.FinishAppointmentTime = DateTime.Now;
            assignment.FinishAppointmentType = FinishAppointmentType.WasTreated;

            // עדכון בשכבת הנתונים
            _dal.assignment.Update(assignment);

            // עדכון סטטוס הקריאה
            call.Status = Enums.CallStatusEnum.CLOSED;
            _dal.call.Update(call);
        }

        public void CancelCallTreatment(string volunteerId, int assignmentId)
        {
            // שליפת המשימה מה-DAL
            var assignment = _dal.assignment.Read(assignmentId);
            if (assignment == null)
            {
                throw new Exception($"Assignment with ID {assignmentId} not found.");
            }

            // בדיקת סטטוס הקריאה
            var call = _dal.call.Read(assignment.CallId);
            if (call == null || call.Status != Enums.CallStatusEnum.OPEN)
            {
                throw new Exception("Call is not open and cannot be canceled.");
            }

            // עדכון ביטול הטיפול במשימה
            assignment.FinishAppointmentTime = DateTime.Now;
            assignment.FinishAppointmentType = FinishAppointmentType.SelfCancellation;

            // עדכון בשכבת הנתונים
            _dal.assignment.Update(assignment);

            // עדכון סטטוס הקריאה
            call.Status = Enums.CallStatusEnum.CLOSED;
            _dal.call.Update(call);
        }

        public void AssignCallToVolunteer(string volunteerId, int callId)
        {
            // שליפת הקריאה מה-DAL
            var call = _dal.call.Read(callId);
            if (call == null)
            {
                throw new Exception($"Call with ID {callId} not found.");
            }

            // בדיקת אם הקריאה פתוחה
            if (call.Status != Enums.CallStatusEnum.OPEN)
            {
                throw new Exception("Call is not open and cannot be assigned.");
            }

            // יצירת הקצאה חדשה למתנדב
            var assignment = new DO.Assignment
            {
                VolunteerId = volunteerId,
                CallId = callId,
                AppointmentTime = DateTime.Now
            };

            // הוספת ההקצאה לשכבת הנתונים
            try
            {
                _dal.assignment.Create(assignment);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to assign call to volunteer.", ex);
            }

            // עדכון סטטוס הקריאה
            call.Status = Enums.CallStatusEnum.ASSIGNED;
            _dal.call.Update(call);
        }

        public bool IsFieldEqual(object entity, Enums.CallFieldEnum field, object value)
        {
            // בדיקת אם הערך בשדה שווה לערך המבוקש
            return GetFieldValue(entity, field)?.Equals(value) ?? false;
        }

        public object GetFieldValue(object entity, Enums.CallFieldEnum field)
        {
            // שליפת הערך של השדה המבוקש
            switch (field)
            {
                case Enums.CallFieldEnum.CallId:
                    return ((BO.CallInList)entity).CallId;
                case Enums.CallFieldEnum.OpenTime:
                    return ((BO.CallInList)entity).OpenTime;
                case Enums.CallFieldEnum.Status:
                    return ((BO.CallInList)entity).Status;
                // הוספה של שדות נוספים לפי הצורך
                default:
                    throw new Exception($"Unknown field {field}.");
            }
        }

    }















