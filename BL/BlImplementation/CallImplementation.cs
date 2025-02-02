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

    public int GetNextId()
    {
        return _dal.config.NextCallId;  // קריאה לשכבת הנתונים
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
                             Status = Tools.callStatus(item.Id),
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


    public BO.Call readCallData(int ID)
    {

        var doCall = _dal.call.Read(ID) ??
            throw new BO.Exceptions.BlDoesNotExistException($"Call with ID={ID} does Not exist");
        IEnumerable<DO.Assignment> assignments = _dal.assignment.ReadAll();
        IEnumerable<DO.Volunteer> volunteers = _dal.Volunteer.ReadAll();
        List<BO.CallAssignInList> myAssignments = assignments.Where(a => a.CallId == ID).
            Select(a =>
            {
                return new CallAssignInList
                {
                    RealFinishTime = a.FinishAppointmentTime,
                    FinishAppointmentType = a.FinishAppointmentType==null?null:(FinishAppointmentTypeEnum)a.FinishAppointmentType,
                    OpenTime = a.AppointmentTime,
                    VolunteerId = a.VolunteerId,
                    VolunteerName = volunteers.Where(v => v.Id == a.VolunteerId).First().FullName,
                };
            }).ToList();
        return new BO.Call
        {
            Id = ID,
            CallType = doCall.CallType switch
            {
                DO.CallType.PreparingFood => BO.Enums.CallTypeEnum.PreparingFood,
                DO.CallType.TransportingFood => BO.Enums.CallTypeEnum.TransportingFood,
                DO.CallType.FixingEquipment => BO.Enums.CallTypeEnum.FixingEquipment,
                DO.CallType.ProvidingShelter => BO.Enums.CallTypeEnum.ProvidingShelter,
                DO.CallType.TransportAssistance => BO.Enums.CallTypeEnum.TransportAssistance,
                DO.CallType.MedicalAssistance => BO.Enums.CallTypeEnum.MedicalAssistance,
                DO.CallType.EmotionalSupport => BO.Enums.CallTypeEnum.EmotionalSupport,
                DO.CallType.PackingSupplies => BO.Enums.CallTypeEnum.PackingSupplies,
           

                _ => throw new FormatException("Unknown Call Type!")
            },
            Address = doCall.Adress,
            OpenTime = doCall.OpenTime,
            Latitude = doCall.Latitude,
            Longitude = doCall.Longitude,
            VerbDesc = doCall.VerbDesc,
            MaxFinishTime = doCall.MaxTime,
            CallStatus = Tools.callStatus(ID),
            CallAssignInLists = myAssignments
        };
    }



    //public BO.Call GetCallDetails(int callId)
    //{
    //    // קבלת פרטי הקריאה
    //    var call = _dal.call.Read(callId);

    //    // אם הקריאה לא קיימת, זורקים חריגה
    //    if (call == null)
    //    {
    //        throw new BO.Exceptions.BlDoesNotExistException($"Call with ID={callId} does not exist");
    //    }

    //    // קבלת כל ההקצאות של הקריאה
    //    var assignments = _dal.assignment.ReadAll(a => a.CallId == callId);

    //    // בחירת ההקצאה הרלוונטית ביותר (נניח האחרונה לפי זמן)
    //    var assignment = assignments.OrderByDescending(a => a.AppointmentTime).FirstOrDefault();

    //    // משתנה לטווח זמן סיכון (למשל מקונפיגורציה)
    //    TimeSpan riskTimeSpan = TimeSpan.FromMinutes(30); // דוגמה - ניתן לשנות לפי הצורך///////////////////////////////////////////////

    //    // יצירת האובייקט BO.Call
    //    return new BO.Call()
    //    {
    //        Id = call.Id,
    //        CallType = (BO.Enums.CallTypeEnum)call.CallType,
    //        VerbDesc = call.VerbDesc,
    //        Address = call.Adress,
    //        OpenTime = call.OpenTime,
    //        MaxFinishTime = call.MaxTime,
    //        Latitude = call.Latitude,
    //        Longitude = call.Longitude,
    //        // קריאה למתודת CheckStatus עם שלושה פרמטרים
    //        CallStatus = Tools.CheckStatus(assignment, call, riskTimeSpan),
    //        // יצירת רשימת הקצאות אם קיימת הקצאה
    //        CallAssignInLists = assignment == null
    //            ? null
    //            : new List<BO.CallAssignInList>
    //            {
    //            new BO.CallAssignInList
    //            {
    //                VolunteerId = assignment.VolunteerId,
    //                VolunteerName = GetVolunteerName(assignment.VolunteerId), // שימוש במתודה לקבלת שם מתנדב
    //                OpenTime = assignment.AppointmentTime,
    //                RealFinishTime = assignment.FinishAppointmentTime,
    //                FinishAppointmentType = (BO.Enums.FinishAppointmentTypeEnum)assignment.FinishAppointmentType
    //            }
    //            }
    //    };
    //}

    //// פונקציה למציאת שם מתנדב
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

            BO.Call c = readCallData(existingCall.Id);
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
                CallManager.Observers.NotifyItemUpdated(callId);  //stage 5
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



    //public void Create(BO.Call call)
    //{
    //    // Call a helper method to convert BO.Call to DO.Call
    //    DO.Call doCall = CallManager.HelpCreateUodate(call);
    //    try
    //    {
    //        // Attempt to create the new call in the database
    //        _dal.Call.Create(doCall);
    //    }
    //    catch (DO.DalAlreadyExistsException ex)
    //    {
    //        // If the call already exists, throw a BO exception
    //        throw new BO.BlAlreadyExistsException($"Call with ID={call.Id} already exists", ex);
    //    }
    //    CallManager.Observers.NotifyListUpdated();  //update list of calls  and obserervers etc.
    //}
    //internal static DO.Call HelpCreateUodate(BO.Call call)
    //{
    //    double[] cordinate = VolunteerManager.GetCoordinates(call.Address);  // Retrieves the coordinates based on the address. Throws an exception if the address is invalid.
    //    AdminImplementation admin = new();  // Creates an instance of AdminImplementation to access admin settings.

    //    // Checks if the MaxTimeToEnd is smaller than the OpeningTime, throws exception if true.
    //    if (call.MaxTimeToEnd < admin.GetClock() + admin.GetRiskRange())
    //        throw new BO.BlUserCantUpdateItemExeption("Max Time To End of Call can't be smaller than the Opening Time + risk range");

    //    if (call.MaxTimeToEnd < admin.GetClock())
    //        throw new BO.BlUserCantUpdateItemExeption("Max Time To End of Call can't be smaller than the Opening Time");

    //    // Returns a new DO.Call object with the updated values.
    //    return new()
    //    {
    //        Id = call.Id,
    //        TheCallType = (DO.CallType)call.TheCallType,  // Converts the call type to DO.CallType.
    //        VerbalDescription = call.VerbalDescription,  // Sets the verbal description.
    //        Address = call.Address,  // Sets the address.
    //        Latitude = cordinate[0],  // Sets the latitude.
    //        Longitude = cordinate[1],  // Sets the longitude.
    //        OpeningTime = call.OpeningTime,  // Sets the opening time.
    //        MaxTimeToEnd = call.MaxTimeToEnd  // Sets the max time to end.
    //    };
    //}

    //// שלב 2: בקשת רשומת הקריאה משכבת הנתונים
    //var existingCall = _dal.call.Read(v => v.Id != call.Id)
    //?? throw new DalDoesNotExistException($"Call with ID {call.Id} already exists.");

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
    //}).GetAwaiter().GetResult();  // מחכה לסיום לפני המשך הקריאה לפונקציות הבאו

    public void AddCall(BO.Call call)
    {

        try
        {
            // שלב 1: בדיקת תקינות הערכים (פורמט ולוגיקה)
            CallManager.checkCallFormat(call);
            CallManager.checkCallLogic(call);
            double[] cordinate = Tools.GetGeolocationCoordinates(call.Address);
           
            // שלב 4: המרת אובייקט BO.Call ל-DO.Call
            DO.Call newCall = new()  
            {
                Id = call.Id,
                OpenTime = call.OpenTime,
                MaxTime = call.MaxFinishTime, // ודא שהשדה תומך בערך null במסד הנתונים
                Longitude = cordinate[1],
                Latitude = cordinate[0],
                Adress = call.Address,
                CallType = (DO.CallType)call.CallType,
                VerbDesc = call.VerbDesc,
            };


            // שלב 5: עדכון הרשומה בשכבת הנתונים
            _dal.call.Create(newCall);
            CallManager.Observers.NotifyItemUpdated(newCall.Id);  //stage 5
            CallManager.Observers.NotifyListUpdated();  //stage 5
        }
        catch (Exception ex)
        {
            // טיפול בשגיאות אם יש
            throw new Exception($"Error occurred while adding call: {ex.Message}");
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
                                                   s.FinishAppointmentType == FinishAppointmentType.CancellationHasExpired ||
                                                   s.FinishAppointmentType == FinishAppointmentType.WasTreated))
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
                        let status = Tools.callStatus(call.Id)
                        where status == BO.Enums.CalltStatusEnum.OPEN || status == BO.Enums.CalltStatusEnum.CallAlmostOver
                        select new BO.OpenCallInList
                        {
                            Id = call.Id,
                            CallType = (BO.Enums.CallTypeEnum)call.CallType,
                            Address = call.Adress,
                            VerbDesc = call.VerbDesc,
                            OpenTime = call.OpenTime,
                            MaxFinishTime = call.MaxTime,
                            DistanceOfCall = Tools.CalculateDistance(volunteerLocation[0], volunteerLocation[1], (double)call.Latitude, (double)call.Longitude)
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


    //public void UpdateCallAsCompleted(int volunteerId, int assignmentId)
    //{
    //    try
    //    {
    //        // שליפת ההקצאה משכבת הנתונים
    //        var assignment = _dal.assignment.Read(a => a.VolunteerId == volunteerId && a.Id == assignmentId);

    //        if (assignment == null)
    //            throw new BO.Exceptions.BlDoesNotExistException("Assignment not found.");

    //        // בדיקת האם הקריאה קשורה למתנדב והאם היא פתוחה
    //        BO.Call call = readCallData(assignment.CallId);
    //        //call.CallStatus = BO.Enums.CalltStatusEnum.OPEN;//////////////////////////////////
    //        //if (call.CallStatus != BO.Enums.CalltStatusEnum.OPEN && call.CallStatus != BO.Enums.CalltStatusEnum.CallIsBeingTreated && call.CallStatus != BO.Enums.CalltStatusEnum.CallTreatmentAlmostOver && call.CallStatus != BO.Enums.CalltStatusEnum.CallAlmostOver)
    //        //    throw new BO.Exceptions.BlInvalidOperationException("Call is not open for completion.");
    //        if(call.CallStatus == BO.Enums.CalltStatusEnum.Canceled || call.CallStatus == BO.Enums.CalltStatusEnum.EXPIRED || call.CallStatus == BO.Enums.CalltStatusEnum.CLOSED)
    //            throw new BO.Exceptions.BlInvalidOperationException("Call is not open for completion.");
    //        //if (assignment.FinishAppointmentTime != null)
    //        //    throw new BO.Exceptions.BlInvalidOperationException("Assignment has already been completed.");

    //        // עדכון פרטי ההקצאה
    //        assignment = assignment with
    //        {
    //            FinishAppointmentTime = DateTime.Now,
    //            FinishAppointmentType = FinishAppointmentType.WasTreated
    //        };

    //        // ניסיון עדכון בשכבת הנתונים
    //        _dal.assignment.Update(assignment);
    //    }
    //    catch (DO.DalDoesNotExistException ex)
    //    {
    //        throw new BO.Exceptions.BlDoesNotExistException("Error updating volunteer assignment.", ex);///////////////////////////
    //    }

    //}
    public void UpdateCallAsCompleted(int volunteerId, int AssignmentId)
    {
        try
        {
            // Retrieve the assignment by ID
            var assignment = _dal.assignment.Read(AssignmentId);

            if (assignment == null)
                throw new InvalidOperationException($"Assignment with id={AssignmentId} does not exist.");

            BO.Call call = readCallData(assignment.CallId);

            // Check if the volunteer is not the one assigned to this assignment
            if (assignment.VolunteerId != volunteerId)
                throw new InvalidOperationException($"Volunteer with id={volunteerId} can't change this assignment to end.");

            // Check if the assignment has already ended
            if (assignment.FinishAppointmentTime.HasValue)
                throw new InvalidOperationException("This assignment already ended.");

            // Create a new assignment object with updated end time and end type
            DO.Assignment newAssign = assignment with { FinishAppointmentTime = AdminManager.Now, FinishAppointmentType = DO.FinishAppointmentType.WasTreated };

            // Attempt to update the assignment in the database
            _dal.assignment.Update(newAssign);

            CallManager.Observers.NotifyItemUpdated(assignment.CallId);  //update current call  and observers etc.
            CallManager.Observers.NotifyListUpdated();  //update list of calls and observers etc.
            VolunteersManager.Observers.NotifyItemUpdated(volunteerId);  //update current volunteer and observers etc.
            VolunteersManager.Observers.NotifyListUpdated();
        }
        catch (Exception ex)
        {
            // Handle exceptions
            throw new InvalidOperationException($"Error updating call as completed: {ex.Message}", ex);
        }
    }

    //public void UpdateToCancelCallTreatment(int Id, int assignmentId)
    //{
    //    try
    //    {
    //        // שליפת ההקצאה משכבת הנתונים
    //        var assignment = _dal.assignment.Read(a => a.VolunteerId == Id && a.Id == assignmentId);

    //        if (assignment == null)
    //            throw new BO.Exceptions.BlDoesNotExistException("Assignment not found.");

    //        // בדיקת האם הקריאה קשורה למתנדב והאם היא פתוחה
    //        BO.Call call = readCallData(assignment.CallId);

    //        if (!IsAdmin(Id)&& assignment.VolunteerId!=Id)
    //            throw new BO.Exceptions.BlInvalidOperationException("The call can be canceled only by the admin or the volunteer that the assignment was opened by.");

    //        if (call.CallStatus == BO.Enums.CalltStatusEnum.Canceled|| call.CallStatus == BO.Enums.CalltStatusEnum.EXPIRED)
    //           throw new BO.Exceptions.BlInvalidOperationException("Call cannot be canceled because its already expired or canceled.");



    //        // עדכון פרטי ההקצאה
    //        assignment = assignment with
    //        {
    //            FinishAppointmentTime = DateTime.Now,
    //            FinishAppointmentType = assignment.VolunteerId == Id
    //                ? FinishAppointmentType.SelfCancellation
    //                : FinishAppointmentType.CancelingAnAdministrator
    //        };

    //        // ניסיון עדכון בשכבת הנתונים
    //        _dal.assignment.Update(assignment);
    //    }
    //    catch (DO.DalDoesNotExistException ex)
    //    {
    //        throw new BO.Exceptions.BlDoesNotExistException("Error updating volunteer assignment.", ex);
    //    }
    //}
    public void UpdateToCancelCallTreatment(int RequesterId, int AssignmentId)
    {
        // Retrieve the assignment object based on its ID.
        var assignment = _dal.assignment.Read(AssignmentId);
        // Check if the assignment does not exist.
        if (assignment == null)
            throw new BO.Exceptions.BlDoesNotExistException($"Assignment with id={AssignmentId} does Not exist\"");

        BO.Call call = readCallData(assignment.CallId);
        // Retrieve the volunteer (asker) object based on the RequesterId.
        var asker = _dal.Volunteer.Read(RequesterId);

        // Check if the volunteer does not exist.
        if (asker == null)
            throw new BO.Exceptions.BlDoesNotExistException($"Volunteer with id={RequesterId} does Not exist\"");

        // Check if the volunteer is not authorized to cancel the assignment (either not their own or not a manager).
        if (assignment.VolunteerId != RequesterId && asker.Position != DO.Position.admin)
            throw new BO.Exceptions.CannotUpdateVolunteerException($"Volunteer with id={RequesterId} can't change this assignment to cancel");

        // Check if the assignment has already ended.
        if (assignment.FinishAppointmentTime.HasValue)
            throw new BO.Exceptions.BlCantBeErased("This assignment already ended");

        if (call.CallStatus != BO.Enums.CalltStatusEnum.CallIsBeingTreated && call.CallStatus != BO.Enums.CalltStatusEnum.CallTreatmentAlmostOver)
            throw new BO.Exceptions.CannotUpdateCallException($"You can only unassign if the call is currently in progress.");

        // Create a new assignment object with updated end time and end type based on role.
        DO.Assignment newAssign;
        if (asker.Position == DO.Position.admin && assignment.VolunteerId != RequesterId)
            newAssign = assignment with { FinishAppointmentTime = AdminManager.Now, FinishAppointmentType = DO.FinishAppointmentType.CancelingAnAdministrator };
        else
            newAssign = assignment with { FinishAppointmentTime = AdminManager.Now, FinishAppointmentType = DO.FinishAppointmentType.SelfCancellation };
        try
        {
            // Update the assignment in the data layer.
            _dal.assignment.Update(newAssign);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            // If updating fails, throw an exception indicating the assignment does not exist.
            throw new BO.Exceptions.BlDoesNotExistException($"Assignment with ID={AssignmentId} does not exist", ex);
        }
        CallManager.Observers.NotifyItemUpdated(call.Id);  //update current call  and obserervers etc.
        CallManager.Observers.NotifyListUpdated();  //update list of calls  and obserervers etc.
        VolunteersManager.Observers.NotifyItemUpdated(assignment.VolunteerId);  //update current call  and obserervers etc.
        VolunteersManager.Observers.NotifyListUpdated();  //update list of calls  and obserervers etc.
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
        BO.Call call = readCallData(callId);

        if (call == null)
            throw new InvalidOperationException("Call not found.");

        // בדיקת אם הקריאה לא טופלה ולא פג תוקפה
        if (call.CallStatus != BO.Enums.CalltStatusEnum.OPEN && call.CallStatus != BO.Enums.CalltStatusEnum.CallAlmostOver)
            throw new InvalidOperationException("Call has already been treated or expired.");

        // בדיקת אם קיימת הקצאה פתוחה על הקריאה
        var existingAssignments = _dal.assignment.Read(a => a.CallId == callId && a.FinishAppointmentTime == null);
        if (existingAssignments != null )
            throw new InvalidOperationException("Call is already assigned to a volunteer.");

            // יצירת הקצאה חדשה
            DO.Assignment newAssignment = new DO.Assignment
            {
                Id = 0,
                VolunteerId = volunteerId,
                CallId = callId,
                AppointmentTime = DateTime.Now, // זמן כניסה לטיפול
                FinishAppointmentTime = null,  // עדיין לא מעודכן
                FinishAppointmentType = null   // עדיין לא מעודכן
            };

        // ניסיון הוספה לשכבת הנתונים
        _dal.assignment.Create(newAssignment);

            
        // עדכון תצוגת הקריאות
        CallManager.Observers.NotifyItemUpdated(newAssignment.Id);
        CallManager.Observers.NotifyListUpdated();
    }
    catch (Exception ex)
    {
        // תרגום חריגות כלליות להודעה ברורה
        throw new Exception("Error occurred while assigning the call: " + ex.Message, ex);
    }
}


    //public void AssignCallToVolunteer(int volunteerId, int callId)
    //{
    //    try
    //    {
    //        // שליפת פרטי הקריאה
    //        BO.Call call = readCallData(callId);

    //        if (call == null)
    //            throw new BO.Exceptions.BlDoesNotExistException("Call not found.");

    //        // בדיקת אם הקריאה לא טופלה ולא פג תוקפה
    //        if (call.CallStatus != BO.Enums.CalltStatusEnum.OPEN)
    //            throw new BO.Exceptions.BlInvalidOperationException("Call has already been treated or expired.");

    //        // בדיקת אם קיימת הקצאה פתוחה על הקריאה
    //        var existingAssignments = _dal.assignment.Read(a => a.CallId == callId && a.FinishAppointmentTime == null);
    //        if (existingAssignments != null)
    //            throw new BO.Exceptions.BlInvalidOperationException("Call is already assigned to a volunteer.");

    //        // יצירת הקצאה חדשה
    //        DO.Assignment newAssignment = new DO.Assignment
    //        {
    //            VolunteerId = volunteerId,
    //            CallId = callId,
    //            AppointmentTime = DateTime.Now, // זמן כניסה לטיפול
    //            FinishAppointmentTime = null, // עדיין לא מעודכן
    //            FinishAppointmentType = null  // עדיין לא מעודכן
    //        };

    //        // ניסיון הוספה לשכבת הנתונים
    //        _dal.assignment.Create(newAssignment);
    //        CallManager.Observers.NotifyItemUpdated(newAssignment.Id);  //stage 5
    //        CallManager.Observers.NotifyListUpdated();  //stage 5
    //    }
    //    catch (DO.DalDoesNotExistException ex)
    //    {
    //        // החריגות נשארות רלוונטיות
    //        throw new BO.Exceptions.BlDoesNotExistException("Error assigning call to volunteer.", ex);
    //    }
    //    catch (BO.Exceptions.BlInvalidOperationException ex)
    //    {
    //        // חריגות שקשורות לפעולה לא חוקית
    //        throw new BO.Exceptions.BlInvalidOperationException("Error in assignment operation.", ex);
    //    }

    //}
    //public void UpdateAddress( int id,string Address)
    //{

    //   _dal.Volunteer.UpdateAddress(Address);   


    //}
}
