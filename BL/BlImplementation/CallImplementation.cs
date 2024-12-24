
namespace BlImplementation;
using BlApi;
using BO;
using DalApi;
using DO;
using Helpers;
using static BO.Enums;
using static BO.Exceptions;

internal class CallImplementation :ICall
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;
    public IEnumerable<int> CallsAmount()
    {    // שליפת כל הקריאות באמצעות ReadAll
        var allCalls = GetCallList(null, null, null);

        // קיבוץ וספירה לפי סטטוס, תוך שימוש בערך המספרי של ה-Enum
        var grouped = allCalls
            .GroupBy(call => call.Status)
            .ToDictionary(group => group.Key, group => group.Count());

        // יצירת מערך בגודל ה-Enum, ומילויו בכמויות לפי הסטטוס
        return Enumerable.Range(0, Enum.GetValues(typeof(BO.Enums.CalltStatusEnum)).Length)
                         .Select(index => grouped.ContainsKey((Enums.CalltStatusEnum)index) ? grouped[(Enums.CalltStatusEnum)index] : 0)
                         .ToArray(); // ממיר את התוצאה למערך int[]
    }


    public IEnumerable<BO.CallInList> GetCallList(BO.Enums.CallFieldEnum? filter, object? toFilter, BO. Enums.CallFieldEnum? toSort)
    {
        var listCall = _dal.call.ReadAll();
        var listAssignment = _dal.assignment.ReadAll();
        var callInList = from item in listCall
                         let assignment = listAssignment.Where(s => s.CallId == item.Id).OrderByDescending(s => s.AppointmentTime).FirstOrDefault()
                         let volunteer = assignment != null ? _dal.Volunteer.Read(assignment.VolunteerId) : null
                         let TempTimeToEnd = item.MaxTime - (ClockManager.Now)
                         select new BO.CallInList
                         {
                             Id = assignment != null ? assignment.Id : null,
                             CallId = item.Id,
                             CallType = (BO.Enums.CallTypeEnum)item.CallType,
                             OpenTime = item.OpenTime,
                             SumTimeUntilFinish = TempTimeToEnd > TimeSpan.Zero ? TempTimeToEnd : null,
                             LastVolunteerName = volunteer != null ? volunteer.FullName : null,
                             SumAppointmentTime = assignment != null ? (assignment.FinishAppointmentType != null ? assignment.FinishAppointmentTime - item.OpenTime : null) : null,
                             Status = CallManager.CheckStatus(assignment, item),
                             SumAssignment = listAssignment.Where(s => s.CallId == item.Id).Count()
                         };


        if (filter.HasValue)
        {
            callInList = callInList.Where(call =>
            {
                return filter switch
                {
                    BO.Enums.CallFieldEnum.ID => call.Id.Equals(toFilter),
                    BO.Enums.CallFieldEnum.CallId => call.CallId.Equals(toFilter),
                    BO.Enums.CallFieldEnum.CallType => call.CallType.Equals(toFilter),
                    BO.Enums.CallFieldEnum.OpenTime => call.OpenTime.Equals(toFilter),
                    BO.Enums.CallFieldEnum.SumTimeUntilFinish => call.SumTimeUntilFinish.Equals(toFilter),
                    BO.Enums.CallFieldEnum.LastVolunteerName => call.LastVolunteerName != null && call.LastVolunteerName.Equals(toFilter),
                    BO.Enums.CallFieldEnum.SumAppointmentTime => call.SumAppointmentTime.Equals(toFilter),
                    BO.Enums.CallFieldEnum.Status => call.Status.Equals(toFilter),
                    BO.Enums.CallFieldEnum.SumAssignment => call.SumAssignment.Equals(toFilter),
                    _ => true
                };
            });

        }

        if (toSort.HasValue)
        {
            callInList = toSort switch
            {
                BO.Enums.CallFieldEnum.ID => callInList.OrderBy(call => call.Id),
                BO.Enums.CallFieldEnum.CallId => callInList.OrderBy(call => call.CallId),
                BO.Enums.CallFieldEnum.CallType => callInList.OrderBy(call => call.CallType),
                BO.Enums.CallFieldEnum.OpenTime => callInList.OrderBy(call => call.OpenTime),
                BO.Enums.CallFieldEnum.SumTimeUntilFinish => callInList.OrderBy(call => call.SumTimeUntilFinish),
                BO.Enums.CallFieldEnum.LastVolunteerName => callInList.OrderBy(call => call.LastVolunteerName),
                BO.Enums.CallFieldEnum.SumAppointmentTime => callInList.OrderBy(call => call.SumAppointmentTime),
                BO.Enums.CallFieldEnum.Status => callInList.OrderBy(call => call.Status),
                BO.Enums.CallFieldEnum.SumAssignment => callInList.OrderBy(call => call.SumAssignment),
                _ => callInList
            };
        }
        else
        {
            callInList = callInList.OrderBy(call => call.CallId);
        }
        return callInList;
    }

    public BO.Call GetCallDetails(int callId)
    {
        var Call = _dal.call.Read(callId);
        if (Call == null)
        {
            throw new BO.Exceptions.BlDoesNotExistException($"Volunteer with ID={callId} does not exist");
        }
        var assignment = _dal.assignment.ReadAll(a => a.CallId == callId).Select(a => a.FinishAppointmentTime);//////////////////////////////////////////////////////
        if (assignment != null )
        {
            BO.Call c = CallManager.CheckStatus(assignment, Call);

            {
                return new BO.Call()
                {
                    Id = Call.Id,
                    CallType = (BO.Enums.CallTypeEnum)Call.CallType,
                    VerbDesc = Call.VerbDesc,
                    Address = Call.Adress,
                    OpenTime = Call.OpenTime,
                    MaxFinishTime = Call.MaxTime,
                    Latitude = Call.Latitude,
                    Longitude = Call.Longitude,
                    CallStatus = c.CallStatus,
                    CallAssignInLists = assignment == null
                        ? null
                        : new BO.CallAssignInList
                        {
                            VolunteerId = assignment.volunteer,
                            VolunteerName = assignment,
                            OpenTime = assignment.AppointmentTime, // המרה ישירה ל-Enum
                            RealFinishTime = assignment.FinishAppointmentTime,
                            FinishAppointmentType = (BO.Enums.FinishAppointmentTypeEnum)assignment.FinishAppointmentType // תיקון שם שדה אם צריך

                        }

                };
            }
            
        }
    }


    public void UpdateCallDetails(BO.Call callDetails)
    {
        try
        {
            // שלב 1: בקשת רשומת הקריאה משכבת הנתונים
            var existingCall = _dal.call.ReadAll(v => v.Id == callDetails.Id).FirstOrDefault()
                ?? throw new DalDoesNotExistException("Call not found.");

            // שלב 2: בדיקת תקינות הערכים (פורמט ולוגיקה)

             CallManager.checkCallAdress(callDetails);

            
            // שלב 4: המרת אובייקט BO.Call ל-DO.Call
            DO.Call newCall = new()
            {
                Id = callDetails.Id,
                OpenTime = callDetails.OpenTime,
                MaxTime = callDetails.MaxFinishTime,
                Longitude = (double)callDetails.Longitude,
                CallType=(DO.CallType)callDetails.CallType,
                VerbDesc = callDetails.VerbDesc,
                Latitude = (double)callDetails.Latitude,
                Adress = callDetails.Address,
            };

            // שלב 5: עדכון הרשומה בשכבת הנתונים
            _dal.call.Update(newCall);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            // טיפול בחריגות וזריקתן מחדש עם מידע ברור לשכבת התצוגה
            throw new CannotUpdateCallException("Error updating call details.", ex);
        }
       
    }


    public void DeleteCall(int callId)
    {
        try
        {
            // שלב 1: בקשת הקריאה משכבת הנתונים
            var existingCall = _dal.call.ReadAll(v => v.Id == callId).FirstOrDefault()
                ?? throw new DalDoesNotExistException("Call not found.");
            var assignment=_dal.assignment.Read(a=>a.CallId==callId);
            // שלב 2: בדיקת סטטוס הקריאה והתאמת התנאים למחיקה
            if ( existingCall.status != Enums.CalltStatusEnum.OPEN  )
                throw new BLDeletionImpossible("Only open calls can be deleted.");

            // שלב 3: בדיקה אם הקריאה הוקצתה למתנדב
            if (assignment.VolunteerId != null)
                throw new BLDeletionImpossible("Cannot delete call as it has been assigned to a volunteer.");

            // שלב 4: ניסיון מחיקת הקריאה משכבת הנתונים
            try
            {
                _dal.call.Delete(callId);
            }
            catch (DO.DalDeletionImpossible ex)
            {
                // שלב 5: אם יש בעיה במחיקה בשכבת הנתונים, זריקת חריגה מתאימה לכיוון שכבת התצוגה
                throw new ArgumentException("Error deleting call from data layer.", ex);
            }
        }
        catch (Exception ex)
        {
            // שלב 6: טיפול בחריגות וזריקתן מחדש עם מידע ברור לשכבת התצוגה
            throw new ArgumentException("Error processing delete call request.", ex);
        }
    }


    public void AddCall(BO.Call call)
    {
        _dal.call.Update(call); // Check if the call already exists
    }

    public IEnumerable<BO.ClosedCallInList> GetVolunteerClosedCalls(int volunteerId, BO.Enums.CallTypeEnum? filter, BO.Enums. ClosedCallFieldEnum? toSort)
    {
        // קריאות וקריאות מוקצות מתוך שכבת ה-DAL
        var listCall = _dal.call.ReadAll();
        var listAssignment = _dal.assignment.ReadAll();

        // סינון קריאות סגורות שטופלו על ידי מתנדב מסוים
        var closedCalls = from item in listCall
                          let assignment = listAssignment
                              .FirstOrDefault(s => s.CallId == item.Id && s.VolunteerId == volunteerId && s.FinishAppointmentType != null)
                          where assignment != null
                          select new BO.ClosedCallInList
                          {
                              Id = item.Id,
                              Address = item.Adress,
                              CallType = (BO.Enums.CallTypeEnum)item.CallType,
                              OpenTime = item.OpenTime,
                              TreatmentStartTime = assignment.AppointmentTime,
                              RealFinishTime = assignment.FinishAppointmentTime,
                              FinishAppointmentType = (BO.Enums.FinishAppointmentTypeEnum?)assignment.FinishAppointmentType
                          };

        // סינון לפי סוג קריאה (אם filter != null)
        if (filter.HasValue)
        {
            closedCalls = closedCalls.Where(call => call.CallType == filter.Value);
        }

        // אם filter == null, לא מסננים לפי סוג קריאה, מחזירים את כל הקריאות הסגורות של המתנדב

        // מיון הרשימה (אם toSort != null)
        if (toSort.HasValue)
        {
            closedCalls = toSort switch
            {
                BO.Enums.ClosedCallFieldEnum.ID => closedCalls.OrderBy(call => call.Id),
                BO.Enums.ClosedCallFieldEnum.Address => closedCalls.OrderBy(call => call.Address),  // מיון לפי כתובת
                BO.Enums.ClosedCallFieldEnum.CallType => closedCalls.OrderBy(call => call.CallType),
                BO.Enums.ClosedCallFieldEnum.OpenTime => closedCalls.OrderBy(call => call.TreatmentStartTime),
                BO.Enums.ClosedCallFieldEnum.TreatmentStartTime => closedCalls.OrderBy(call => call.RealFinishTime),
                BO.Enums.ClosedCallFieldEnum.RealFinishTime => closedCalls.OrderBy(call => call.FinishAppointmentType),
                BO.Enums.ClosedCallFieldEnum.FinishAppointmentType => closedCalls.OrderBy(call => call.OpenTime.Date), // מיון לפי תאריך פתיחה

                _ => closedCalls
            };
        }
        else
        {
            // ברירת מחדל למיון לפי ID
            closedCalls = closedCalls.OrderBy(call => call.Id);
        }

        return closedCalls;
    }

    public IEnumerable<BO.OpenCallInList> GetVolunteerOpenCalls(
    int volunteerId,
    BO.Enums.CallTypeEnum? filter,
    BO.Enums.OpenCallEnum? toSort)
    {
        // קריאות מתוך שכבת ה-DAL
        var c = _dal.call.ReadAll();
        var allCalls = GetCallList(null, null, null);
        var calls=_dal.call.ReadAll(c=>c.Id==volunteerId);

        // קיבוץ וספירה לפי סטטוס, תוך שימוש בערך המספרי של ה-Enum
        
        var listAssignment = _dal.assignment.ReadAll();
      
        // מיקום המתנדב (מתוך פרטי המתנדב)
        //var volunteerLocation = _dal.Volunteer.GetLocation(volunteerId);

        // סינון קריאות פתוחות או פתוחות בסיכון
        var openCalls = from item in c
                        where item.Status == CalltStatusEnum.OPEN || item.Status == CalltStatusEnum.CallAlmostOver
                        select new BO.OpenCallInList
                        {
                            Id = (int)item.Id,
                            CallType = (BO.Enums.CallTypeEnum)item.CallType,
                            VerbDesc = item.ver,
                            Address = call.Adress,
                            OpenTime = item.OpenTime,
                            MaxFinishTime = item.MaxFinishTime,
                            DistanceOfCall = CalculateDistance(volunteerLocation, call.Location) // חישוב מרחק
                        };

        // סינון לפי סוג הקריאה (אם filter != null)
        if (filter.HasValue)
        {
            openCalls = openCalls.Where(call => call.CallType == filter.Value);
        }

        // מיון הרשימה (אם toSort != null)
        if (toSort.HasValue)
        {
            openCalls = toSort switch
            {
                BO.Enums.OpenCallEnum.Id => openCalls.OrderBy(call => call.Id),
                BO.Enums.OpenCallEnum.CallType => openCalls.OrderBy(call => call.CallType),
                BO.Enums.OpenCallEnum.VerbDesc => openCalls.OrderBy(call => call.VerbDesc),
                BO.Enums.OpenCallEnum.Address => openCalls.OrderBy(call => call.Address),
                BO.Enums.OpenCallEnum.OpenTime => openCalls.OrderBy(call => call.OpenTime),
                BO.Enums.OpenCallEnum.MaxFinishTime => openCalls.OrderBy(call => call.MaxFinishTime),
                BO.Enums.OpenCallEnum.DistanceOfCall => openCalls.OrderBy(call => call.DistanceOfCall),
                _ => openCalls
            };
        }
        else
        {
            // ברירת מחדל למיון לפי ID
            openCalls = openCalls.OrderBy(call => call.Id);
        }

        return openCalls;
    }

    // פונקציה לחישוב מרחק בין שני מיקומים
    private double CalculateDistance(Location location1, Location location2)
    {
        // מימוש חישוב מרחק (לדוגמה, שימוש בנוסחת Haversine)
        double latDiff = location1.Latitude - location2.Latitude;
        double lonDiff = location1.Longitude - location2.Longitude;
        return Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff); // דוגמה בלבד
    }



    //IEnumerable<OpenCallInList> GetOpenCallsForVolunteer(
    //     string volunteerId,
    //     Enums.CallTypeEnum? callType = null,
    //     Enums.CallFieldEnum? sortField = null
    // )
    //{
    //    var calls = _dal.call.ReadAll().Where(call => call.VolunteerId == volunteerId && call.Status == CallStatusEnum.Open).ToList();

    //    if (callType.HasValue)
    //    {
    //        calls = calls.Where(call => call.Type == callType.Value).ToList();
    //    }

    //    if (sortField.HasValue)
    //    {
    //        calls = SortCalls(calls, sortField.Value).ToList();
    //    }

    //    return calls.Select(call => new OpenCallInList(call)); // Return the list of open calls available for the volunteer
    //}

    //public void UpdetedCallAsCompleted(int volunteerId, int assignmentId)
    //{

    //    var assignment = _dal.assignment.Read(a => a.VolunteerId == volunteerId && a.Id == assignmentId);
    //    if (assignment != null)
    //    {
    //        BO.Call call = GetCallDetails(assignment.CallId);
    //        if (call.CallStatus == BO.Enums.CalltStatusEnum.OPEN)
    //        {
    //            try
    //            {
    //                assignment.FinishAppointmentType = DateTime.Now;
    //                assignment.FinishAppointmentType = DO.
    //                 _dal.assignment.Update(assignment);
    //            }
    //            catch (DO.DalDoesNotExistException ex)
    //            { throw new BO.Exceptions.BlDoesNotExistException("Error updating volunteer details.", ex); }




    //        }

    //    }
    //}
    public void UpdateCallAsCompleted(int volunteerId, int assignmentId)
    {
        try
        {
            // שליפת ההקצאה משכבת הנתונים
            var assignment = _dal.assignment.Read(a => a.VolunteerId == volunteerId && a.Id == assignmentId);

            if (assignment == null)
                throw new BO.Exceptions.BlDoesNotExistException("Assignment not found.");

            // בדיקת האם הקריאה קשורה למתנדב והאם היא פתוחה
            BO.Call call = GetCallDetails(assignment.CallId);
            if (call.CallStatus != BO.Enums.CalltStatusEnum.OPEN)
                throw new BO.Exceptions.BlInvalidOperationException("Call is not open for completion.");

            if (assignment.FinishAppointmentType != null)
                throw new BO.Exceptions.BlInvalidOperationException("Assignment has already been completed.");

            // עדכון פרטי ההקצאה
            assignment.FinishAppointmentType = DateTime.Now;
            assignment.Status = BO.Enums.AssignmentStatus.Completed; // הנחה שסוג הסיום "טופלה"

            // ניסיון עדכון בשכבת הנתונים
            _dal.assignment.Update(assignment);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.Exceptions.BlDoesNotExistException("Error updating volunteer assignment. Assignment not found.", ex);
        }
        catch (Exception ex)
        {
            throw new BO.Exceptions.BlGeneralException("An unexpected error occurred.", ex);
        }
    }

    public void CancelCallTreatment(string volunteerId, int assignmentId)
    {
        var call = _dal.call.ReadAll().FirstOrDefault(c => c.VolunteerId == volunteerId && c.AssignmentId == assignmentId);
        if (call != null)
        {
           
            call.Status = CallStatusEnum.Cancelled;
            _dal.Call.Update(call); // Update the call status to "Cancelled"
        }
    }

    public void AssignCallToVolunteer(string volunteerId, int callId)
    {
        var call = _dal.call.Read(callId);
        if (call != null)
        {
            call.VolunteerId = volunteerId;
            call.Status = CallStatusEnum.Assigned;
            _dal.Call.Update(call);
        }
    }
    private bool IsFieldEqual(object entity, Enums.CallFieldEnum field, object value)
    {
        // שליפת מידע על השדה באובייקט לפי השם המועבר
        var property = entity.GetType().GetProperty(field.ToString());
        if (property == null)
            return false; // אם השדה לא קיים

        // שליפת הערך של השדה מהאובייקט
        var fieldValue = property.GetValue(entity);

        // השוואה בין הערכים
        return fieldValue != null && fieldValue.Equals(value);
    }
    private object GetFieldValue(object entity, Enums.CallFieldEnum field)
    {
        // שליפת מידע על השדה באובייקט לפי השם המועבר
        var property = entity.GetType().GetProperty(field.ToString());
        return property?.GetValue(entity); // החזרת הערך או null אם השדה לא קיים
    }

}

