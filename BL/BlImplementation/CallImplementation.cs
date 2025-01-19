namespace BlImplementation;
using BlApi;
using BO;

using DO;
using Helpers;
using static BO.Enums;
using static BO.Exceptions;

internal class CallImplementation : ICall
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;


    #region Stage 5
    public void AddObserver(Action listObserver) =>
    CallManager.Observers.AddListObserver(listObserver); //stage 5
    public void AddObserver(int id, Action observer) =>
    CallManager.Observers.AddObserver(id, observer); //stage 5
    public void RemoveObserver(Action listObserver) =>
    CallManager.Observers.RemoveListObserver(listObserver); //stage 5
    public void RemoveObserver(int id, Action observer) =>
    CallManager.Observers.RemoveObserver(id, observer); //stage 5
    #endregion Stage 5
    public IEnumerable<int> CallsAmount()
    {
        // שליפת כל הקריאות
        var allCalls = GetCallList(null, null, null);

        // קיבוץ וספירה לפי סטטוס, תוך שימוש בערך המספרי של ה-Enum
        var grouped = allCalls
            .GroupBy(call => (int)call.Status)  // המרת הסטטוס לערך המספרי של ה-Enum
            .ToDictionary(group => group.Key, group => group.Count());

        // יצירת מערך בגודל ה-Enum, ומילויו בכמויות לפי הסטטוס
        var enumLength = Enum.GetValues(typeof(BO.Enums.CalltStatusEnum)).Length;

        // מוודא שהאינדקסים תואמים לערכים של ה-Enum וממלא את המערך
        var result = Enumerable.Range(0, enumLength)
                               .Select(index => grouped.GetValueOrDefault(index, 0))  // השגת הערך או 0 אם לא קיים
                               .ToArray();

        // הדפסת ערכים לצורכי ניפוי בעיות (אם יש צורך)
        for (int i = 0; i < enumLength; i++)
        {
            Console.WriteLine($"Status {i}: {result[i]} calls");
        }

        return result;
    }



    public IEnumerable<BO.CallInList> GetCallList(BO.Enums.CallFieldEnum? filter, object? toFilter, BO.Enums.CallFieldEnum? toSort)
    {
        var listCall = _dal.call.ReadAll();
        var listAssignment = _dal.assignment.ReadAll();

        // יצירת השאילתה הראשונית
        var callInList = from item in listCall
                         let assignments = listAssignment.Where(s => s.CallId == item.Id).OrderByDescending(s => s.AppointmentTime).ToList()
                         let assignment = assignments.FirstOrDefault() // לוקחים את הראשון
                         let volunteer = assignment != null ? _dal.Volunteer.Read(assignment.VolunteerId) : null
                         let TempTimeToEnd = item.MaxTime - AdminManager.Now
                         select new BO.CallInList
                         {
                             Id = assignment?.Id,
                             CallId = item.Id,
                             CallType = (BO.Enums.CallTypeEnum)item.CallType,
                             OpenTime = item.OpenTime,
                             SumTimeUntilFinish = TempTimeToEnd > TimeSpan.Zero ? TempTimeToEnd : null,
                             LastVolunteerName = volunteer?.FullName,
                             SumAppointmentTime = assignment != null && assignment.FinishAppointmentTime.HasValue
                                 ? assignment.FinishAppointmentTime.Value - item.OpenTime
                                 : null,
                             Status = Tools.CheckStatusCalls(assignment, item, null),
                             SumAssignment = assignments.Count() // ספירה מתוך הרשימה המלאה של Assignment
                         };


        // סינון לפי filter ו-toFilter אם יש
        if (filter.HasValue && toFilter != null)
        {
            callInList = callInList.Where(call =>
            {
                return filter switch
                {
                    BO.Enums.CallFieldEnum.ID => call.Id?.Equals(toFilter) == true,
                    BO.Enums.CallFieldEnum.CallId => call.CallId.Equals(toFilter),
                    BO.Enums.CallFieldEnum.CallType => call.CallType.Equals(toFilter),
                    BO.Enums.CallFieldEnum.OpenTime => call.OpenTime.Equals(toFilter),
                    BO.Enums.CallFieldEnum.SumTimeUntilFinish => call.SumTimeUntilFinish?.Equals(toFilter) == true,
                    BO.Enums.CallFieldEnum.LastVolunteerName => call.LastVolunteerName?.Equals(toFilter) == true,
                    BO.Enums.CallFieldEnum.SumAppointmentTime => call.SumAppointmentTime?.Equals(toFilter) == true,
                    BO.Enums.CallFieldEnum.Status => call.Status.Equals(toFilter),
                    BO.Enums.CallFieldEnum.SumAssignment => call.SumAssignment.Equals(toFilter),
                    _ => true
                };
            });
        }

        // מיון אם יש
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
            // מיון ברירת מחדל לפי CallId
            callInList = callInList.OrderBy(call => call.CallId);
        }

        return callInList.ToList();
    }



    public BO.Call GetCallDetails(int callId)
    {
        // קבלת פרטי הקריאה
        var call = _dal.call.Read(callId);

        // אם הקריאה לא קיימת, זורקים חריגה
        if (call == null)
        {
            throw new BO.Exceptions.BlDoesNotExistException($"Call with ID={callId} does not exist");
        }

        // קבלת כל ההקצאות של הקריאה
        var assignments = _dal.assignment.ReadAll(a => a.CallId == callId);

        // בחירת ההקצאה הרלוונטית ביותר (נניח האחרונה לפי זמן)
        var assignment = assignments.OrderByDescending(a => a.AppointmentTime).FirstOrDefault();

        // משתנה לטווח זמן סיכון (למשל מקונפיגורציה)
        TimeSpan riskTimeSpan = TimeSpan.FromMinutes(30); // דוגמה - ניתן לשנות לפי הצורך///////////////////////////////////////////////

        // יצירת האובייקט BO.Call
        return new BO.Call()
        {
            Id = call.Id,
            CallType = (BO.Enums.CallTypeEnum)call.CallType,
            VerbDesc = call.VerbDesc,
            Address = call.Adress,
            OpenTime = call.OpenTime,
            MaxFinishTime = call.MaxTime,
            Latitude = call.Latitude,
            Longitude = call.Longitude,
            // קריאה למתודת CheckStatus עם שלושה פרמטרים
            CallStatus = Tools.CheckStatus(assignment, call, riskTimeSpan),
            // יצירת רשימת הקצאות אם קיימת הקצאה
            CallAssignInLists = assignment == null
                ? null
                : new List<BO.CallAssignInList>
                {
                new BO.CallAssignInList
                {
                    VolunteerId = assignment.VolunteerId,
                    VolunteerName = GetVolunteerName(assignment.VolunteerId), // שימוש במתודה לקבלת שם מתנדב
                    OpenTime = assignment.AppointmentTime,
                    RealFinishTime = assignment.FinishAppointmentTime,
                    FinishAppointmentType = (BO.Enums.FinishAppointmentTypeEnum)assignment.FinishAppointmentType
                }
                }
        };
    }

    // פונקציה למציאת שם מתנדב
    private string GetVolunteerName(int? volunteerId)
    {
        if (!volunteerId.HasValue)
            return null;
        var volunteer = _dal.Volunteer.Read(v => v.Id == volunteerId);
        return volunteer?.FullName; // מחזיר את שם המתנדב או null אם לא נמצא
    }


    public void UpdateCallDetails(BO.Call callDetails)
    {
        try
        {
            // שלב 1: בדיקת תקינות הערכים (פורמט ולוגיקה)
            CallManager.checkCallFormat(callDetails);
            CallManager.checkCallLogic(callDetails);

            // שלב 2: בקשת רשומת הקריאה משכבת הנתונים
            var existingCall = _dal.call.Read(v => v.Id == callDetails.Id)
                ?? throw new DalDoesNotExistException($"Call with ID {callDetails.Id} not found.");

            //// שלב 3: בדיקת כתובת ועדכון קואורדינטות
            //bool isValidAddress = CallManager.IsValidAddressAsync(callDetails.Address).Result;  // קריאה סנכרונית לפונקציה אסינכרונית
            //if (!isValidAddress)
            //{
            //    throw new InvalidCallFormatException("Invalid address provided.");
            //}

            //// עדכון אורך ורוחב לפי הכתובת
            //double[] GeolocationCoordinates = Tools.GetGeolocationCoordinates(callDetails.Address);

            //callDetails.Longitude = GeolocationCoordinates[0];
            //callDetails.Latitude = GeolocationCoordinates[1];

            // שלב 4: המרת אובייקט BO.Call ל-DO.Call
            DO.Call newCall = new()
            {
                Id = callDetails.Id,
                OpenTime = callDetails.OpenTime,
                MaxTime = callDetails.MaxFinishTime,
                Longitude = (double)callDetails.Longitude,
                Latitude = (double)callDetails.Latitude,
                Adress = callDetails.Address,
                CallType = (DO.CallType)callDetails.CallType,
                VerbDesc = callDetails.VerbDesc,
            };

            // שלב 5: עדכון הרשומה בשכבת הנתונים
            _dal.call.Update(newCall);
            CallManager.Observers.NotifyItemUpdated(newCall.Id);  //stage 5
            CallManager.Observers.NotifyListUpdated();  //stage 5

        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new CannotUpdateCallException($"Error updating call details for ID: {callDetails.Id}.", ex);
        }
        catch (InvalidCallFormatException ex)
        {
            throw new CannotUpdateCallException("Invalid call details provided.", ex);
        }
    }

    public void DeleteCall(int callId)
    {
        try
        {
            // שלב 1: בקשת הקריאה משכבת הנתונים
            var existingCall = _dal.call.Read(v => v.Id == callId)
                ?? throw new DalDoesNotExistException("Call not found.");

            BO.Call c = GetCallDetails(existingCall.Id);
            var assignment = _dal.assignment.Read(a => a.CallId == callId);

            // שלב 2: בדיקת סטטוס הקריאה והתאמת התנאים למחיקה
            Console.WriteLine($"Call status: {c.CallStatus}");  // הדפסת סטטוס לקריאה
            if (c.CallStatus != Enums.CalltStatusEnum.OPEN)
                throw new BLDeletionImpossible("Only open calls can be deleted.");

            // שלב 3: בדיקה אם הקריאה הוקצתה למתנדב
            Console.WriteLine($"Assignment VolunteerId: {assignment?.VolunteerId}");  // הדפסת מזהה המתנדב אם קיים
            if (assignment?.VolunteerId != null)
                throw new BLDeletionImpossible("Cannot delete call as it has been assigned to a volunteer.");

            // שלב 4: ניסיון מחיקת הקריאה משכבת הנתונים
            try
            {
                Console.WriteLine($"Attempting to delete call with ID {callId}");
                _dal.call.Delete(callId);
                CallManager.Observers.NotifyListUpdated();  //stage 5  
            }
            catch (DO.DalDeletionImpossible ex)
            {
                // שלב 5: אם יש בעיה במחיקה בשכבת הנתונים, זריקת חריגה מתאימה לכיוון שכבת התצוגה
                Console.WriteLine($"Error deleting call from DAL: {ex.Message}");
                throw new ArgumentException("Error deleting call from data layer.", ex);
            }
        }
        catch (Exception ex)
        {
            // שלב 6: טיפול בחריגות וזריקתן מחדש עם מידע ברור לשכבת התצוגה
            Console.WriteLine($"Error processing delete call request for call ID {callId}: {ex.Message}");
            throw new ArgumentException($"Error processing delete call request for call ID {callId}.", ex);
        }
    }




    public void AddCall(BO.Call call)
    {

        try
        {
            // שלב 1: בדיקת תקינות הערכים (פורמט ולוגיקה)
            CallManager.checkCallFormat(call);
            CallManager.checkCallLogic(call);

            // שלב 2: בקשת רשומת הקריאה משכבת הנתונים
            var existingCall = _dal.call.Read(v => v.Id != call.Id)
                ?? throw new DalDoesNotExistException($"Call with ID {call.Id} already exists.");

            // שלב 3: בדיקת כתובת ועדכון קואורדינטות
            //Task.Run(async () =>
            //{
            //    bool isValidAddress = await CallManager.IsValidAddressAsync(call.Address); // קריאה אסינכרונית לפונקציה
            //    if (!isValidAddress)
            //    {
            //        throw new InvalidCallFormatException("Invalid address provided.");
            //    }

            //    // עדכון אורך ורוחב לפי הכתובת
            //    double[] GeolocationCoordinates = Tools.GetGeolocationCoordinates(call.Address);

            //    call.Longitude = GeolocationCoordinates[0];
            //    call.Latitude = GeolocationCoordinates[1];
            //}).GetAwaiter().GetResult();  // מחכה לסיום לפני המשך הקריאה לפונקציות הבאות

            // שלב 4: המרת אובייקט BO.Call ל-DO.Call
            DO.Call newCall = new()
            {
                Id = call.Id,
                OpenTime = call.OpenTime,
                MaxTime = call.MaxFinishTime,
                Longitude = (double)call.Longitude,
                Latitude = (double)call.Latitude,
                Adress = call.Address,
                CallType = (DO.CallType)call.CallType,
                VerbDesc = call.VerbDesc,
            };

            // שלב 5: עדכון הרשומה בשכבת הנתונים
            _dal.call.Create(newCall);
            CallManager.Observers.NotifyListUpdated(); //stage 5  
        }
        catch (Exception ex)
        {
            // טיפול בשגיאות אם יש
            Console.WriteLine($"Error occurred while adding call: {ex.Message}");
        }
    }


    public IEnumerable<BO.ClosedCallInList> GetVolunteerClosedCalls(int volunteerId, BO.Enums.CallTypeEnum? filter, BO.Enums.ClosedCallFieldEnum? toSort)
    {
        var listAssignment = _dal.assignment.ReadAll(a => a.VolunteerId == volunteerId);
        var listCall = _dal.call.ReadAll();

        var closedCalls = from item in listCall
                          let assignment = listAssignment
                              .FirstOrDefault(s => s.CallId == item.Id && s.VolunteerId == volunteerId &&
                                                  (s.FinishAppointmentType == FinishAppointmentType.SelfCancellation ||
                                                   s.FinishAppointmentType == FinishAppointmentType.CancelingAnAdministrator ||
                                                   s.FinishAppointmentType == FinishAppointmentType.CancellationHasExpired))
                          where assignment != null
                          select new BO.ClosedCallInList
                          {
                              Id = item.Id,
                              Address = item.Adress,
                              CallType = (BO.Enums.CallTypeEnum)item.CallType,
                              OpenTime = item.OpenTime,
                              TreatmentStartTime = assignment.AppointmentTime,
                              RealFinishTime = assignment.FinishAppointmentTime,
                              FinishAppointmentType = (BO.Enums.FinishAppointmentTypeEnum?)assignment.FinishAppointmentType,
                          };

        // סינון אם נדרש לפי סוג הקריאה
        if (filter.HasValue)
        {
            closedCalls = closedCalls.Where(call => call.CallType == filter.Value);
        }

        // סינון ומיון לפי השדה שנבחר
        if (toSort.HasValue)
        {
            closedCalls = toSort switch
            {
                BO.Enums.ClosedCallFieldEnum.ID => closedCalls.OrderBy(call => call.Id),
                BO.Enums.ClosedCallFieldEnum.Address => closedCalls.OrderBy(call => call.Address),
                BO.Enums.ClosedCallFieldEnum.CallType => closedCalls.OrderBy(call => call.CallType),
                BO.Enums.ClosedCallFieldEnum.OpenTime => closedCalls.OrderBy(call => call.OpenTime),
                BO.Enums.ClosedCallFieldEnum.TreatmentStartTime => closedCalls.OrderBy(call => call.TreatmentStartTime),
                BO.Enums.ClosedCallFieldEnum.RealFinishTime => closedCalls.OrderBy(call => call.RealFinishTime),
                BO.Enums.ClosedCallFieldEnum.FinishAppointmentType => closedCalls.OrderBy(call => call.FinishAppointmentType),
                _ => closedCalls
            };
        }
        else
        {
            closedCalls = closedCalls.OrderBy(call => call.Id);
        }

        return closedCalls;
    }



    public IEnumerable<BO.OpenCallInList> GetOpenCallInLists(
     int volunteerId,
     BO.Enums.CallTypeEnum? filter = null,
     BO.Enums.OpenCallEnum? toSort = null)
    {
        // שליפת רשימות הקריאות והשיוכים
        var listCall = _dal.call.ReadAll();
        var listAssignment = _dal.assignment.ReadAll();

        if (!listCall.Any())
        {
            throw new Exception("No calls found in the database.");
        }

        if (!listAssignment.Any())
        {
            throw new Exception("No assignments found in the database.");
        }

        var volunteer = _dal.Volunteer.Read(v => v.Id == volunteerId);

        if (volunteer == null)
        {
            throw new ArgumentException("Volunteer not found.");
        }

        // שליפת מיקום המתנדב
        string volunteerAddress = volunteer.Location;

        if (string.IsNullOrWhiteSpace(volunteerAddress))
        {
            throw new ArgumentException("Volunteer location is not provided.");
        }

        // קריאה אסינכרונית לפעולה
        double[] volunteerLocation;
        try
        {
            volunteerLocation = Tools.GetGeolocationCoordinates(volunteerAddress);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to fetch volunteer location: {ex.Message}");
        }

        if (volunteerLocation == null || volunteerLocation.Length != 2)
        {
            throw new Exception("Invalid location data received for the volunteer.");
        }

        // סינון הקריאות הפתוחות לפי סטטוס
        var openCalls = from call in listCall
                        let assignment = listAssignment.FirstOrDefault(a => a.CallId == call.Id)
                        let status = Tools.CheckStatusCalls(assignment, call, null)
                        where status == BO.Enums.CalltStatusEnum.OPEN || status == BO.Enums.CalltStatusEnum.CallAlmostOver
                        select new BO.OpenCallInList
                        {
                            Id = call.Id,
                            CallType = (BO.Enums.CallTypeEnum)call.CallType,
                            Address = call.Adress,
                            OpenTime = call.OpenTime,
                            MaxFinishTime = call.MaxTime,
                            DistanceOfCall = Tools.CalculateDistance(volunteerLocation[0], volunteerLocation[1], call.Latitude, call.Longitude)
                        };

        // סינון לפי סוג הקריאה אם הוזן
        if (filter.HasValue)
        {
            openCalls = openCalls.Where(call => call.CallType == filter.Value);
        }

        // מיון הקריאות לפי השדה שהוזן
        openCalls = toSort switch
        {
            BO.Enums.OpenCallEnum.Id => openCalls.OrderBy(call => call.Id),
            BO.Enums.OpenCallEnum.CallType => openCalls.OrderBy(call => call.CallType),
            BO.Enums.OpenCallEnum.Address => openCalls.OrderBy(call => call.Address),
            BO.Enums.OpenCallEnum.OpenTime => openCalls.OrderBy(call => call.OpenTime),
            BO.Enums.OpenCallEnum.MaxFinishTime => openCalls.OrderBy(call => call.MaxFinishTime),
            BO.Enums.OpenCallEnum.DistanceOfCall => openCalls.OrderBy(call => call.DistanceOfCall),
            _ => openCalls.OrderBy(call => call.Id)
        };

        // החזרת הרשימה הממוינת
        return openCalls.ToList();
    }


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

            if (assignment.FinishAppointmentTime != null)
                throw new BO.Exceptions.BlInvalidOperationException("Assignment has already been completed.");

            // עדכון פרטי ההקצאה
            assignment = assignment with
            {
                FinishAppointmentTime = DateTime.Now,
                FinishAppointmentType = FinishAppointmentType.WasTreated
            };

            // ניסיון עדכון בשכבת הנתונים
            _dal.assignment.Update(assignment);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.Exceptions.BlDoesNotExistException("Error updating volunteer assignment.", ex);///////////////////////////
        }

    }

    public void UpdateToCancelCallTreatment(int Id, int assignmentId)
    {
        try
        {
            // שליפת ההקצאה משכבת הנתונים
            var assignment = _dal.assignment.Read(a => a.VolunteerId == Id && a.Id == assignmentId);

            if (assignment == null)
                throw new BO.Exceptions.BlDoesNotExistException("Assignment not found.");

            // בדיקת האם הקריאה קשורה למתנדב והאם היא פתוחה
            BO.Call call = GetCallDetails(assignment.CallId);

            if (!IsAdmin(Id) && assignment.VolunteerId != Id)
                throw new BO.Exceptions.BlInvalidOperationException("The call can be canceled only by the admin or the volunteer that the assignment was opened by.");

            if (call.CallStatus != BO.Enums.CalltStatusEnum.OPEN)
                throw new BO.Exceptions.BlInvalidOperationException("Call is not open for completion.");

            if (assignment.FinishAppointmentTime != null)
                throw new BO.Exceptions.BlInvalidOperationException("Assignment has already been completed.");

            // עדכון פרטי ההקצאה
            assignment = assignment with
            {
                FinishAppointmentTime = DateTime.Now,
                FinishAppointmentType = assignment.VolunteerId == Id
                    ? FinishAppointmentType.SelfCancellation
                    : FinishAppointmentType.CancelingAnAdministrator
            };

            // ניסיון עדכון בשכבת הנתונים
            _dal.assignment.Update(assignment);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.Exceptions.BlDoesNotExistException("Error updating volunteer assignment.", ex);
        }
    }

    private bool IsAdmin(int id)
    {
        var volunteer = _dal.Volunteer.Read(v => v.Id == id);
        if (volunteer.Position == DO.Position.admin)
            return true;
        return false;
    }


    public void AssignCallToVolunteer(int volunteerId, int callId)
    {
        try
        {
            // שליפת פרטי הקריאה
            BO.Call call = GetCallDetails(callId);

            if (call == null)
                throw new BO.Exceptions.BlDoesNotExistException("Call not found.");

            // בדיקת אם הקריאה לא טופלה ולא פג תוקפה
            if (call.CallStatus != BO.Enums.CalltStatusEnum.OPEN)
                throw new BO.Exceptions.BlInvalidOperationException("Call has already been treated or expired.");

            // בדיקת אם קיימת הקצאה פתוחה על הקריאה
            var existingAssignments = _dal.assignment.Read(a => a.CallId == callId && a.FinishAppointmentTime == null);
            if (existingAssignments != null)
                throw new BO.Exceptions.BlInvalidOperationException("Call is already assigned to a volunteer.");

            // יצירת הקצאה חדשה
            DO.Assignment newAssignment = new DO.Assignment
            {
                VolunteerId = volunteerId,
                CallId = callId,
                AppointmentTime = DateTime.Now, // זמן כניסה לטיפול
                FinishAppointmentTime = null, // עדיין לא מעודכן
                FinishAppointmentType = null  // עדיין לא מעודכן
            };

            // ניסיון הוספה לשכבת הנתונים
            _dal.assignment.Create(newAssignment);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            // החריגות נשארות רלוונטיות
            throw new BO.Exceptions.BlDoesNotExistException("Error assigning call to volunteer.", ex);
        }
        catch (BO.Exceptions.BlInvalidOperationException ex)
        {
            // חריגות שקשורות לפעולה לא חוקית
            throw new BO.Exceptions.BlInvalidOperationException("Error in assignment operation.", ex);
        }

    }


}
