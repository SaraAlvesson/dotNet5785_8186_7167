namespace BlImplementation;
using BlApi;
using BO;
using DO;
using Helpers;
using BL.Helpers;
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
        lock (AdminManager.BlMutex) // 🔒 הוספת נעילה למניעת קריאות מקבילות
        {
            return _dal.config.NextCallId;
        }
    }


    public IEnumerable<BO.CallInList> GetCallList(BO.Enums.CallFieldEnum? filter, object? toFilter, BO.Enums.CallFieldEnum? toSort)
    {
        lock (AdminManager.BlMutex) // 🔒 הוספת נעילה לקריאות מה-DAL
        {
            var listCall = _dal.call.ReadAll();
            var listAssignment = _dal.assignment.ReadAll();

            var callInList = from item in listCall
                             let assignments = listAssignment.Where(s => s.CallId == item.Id).OrderByDescending(s => s.AppointmentTime).ToList()
                             let assignment = assignments.FirstOrDefault()
                             let volunteer = assignment != null ? _dal.Volunteer.Read(assignment.VolunteerId) : null
                             let TempTimeToEnd = item.MaxTime - AdminManager.Now
                             select new BO.CallInList
                             {
                                 Id = assignment?.Id,
                                 CallId = item.Id,
                                 CallType = (BO.Enums.CallTypeEnum)item.CallType,
                                 OpenTime = item.OpenTime,
                                 SumTimeUntilFinish = item.MaxTime > AdminManager.Now
                                     ? item.MaxTime - AdminManager.Now
                                     : null,
                                 LastVolunteerName = volunteer?.FullName,
                                 SumAppointmentTime = assignment != null && assignment.FinishAppointmentTime.HasValue
                                     ? assignment.FinishAppointmentTime.Value - item.OpenTime
                                     : null,
                                 Status = Tools.callStatus(item.Id),
                                 SumAssignment = assignments.Count(s => s.CallId == item.Id)
                             };

            if (filter.HasValue && toFilter != null)
            {
                callInList = callInList.Where(call =>
                {
                    return filter switch
                    {
                        BO.Enums.CallFieldEnum.ID => call.Id?.Equals(toFilter) == true,
                        BO.Enums.CallFieldEnum.CallId => call.CallId.Equals(toFilter),
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

            if (toSort.HasValue)
            {
                callInList = toSort switch
                {
                    BO.Enums.CallFieldEnum.ID => callInList.OrderBy(call => call.Id),
                    BO.Enums.CallFieldEnum.CallId => callInList.OrderBy(call => call.CallId),
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

            return callInList.ToList();
        }
    }


    public BO.Call readCallData(int ID)
    {
        lock (AdminManager.BlMutex) // 🔒 נעילה למניעת קריאות מקבילות
        {
            var doCall = _dal.call.Read(ID) ??
                throw new BO.Exceptions.BlDoesNotExistException($"Call with ID={ID} does Not exist");

            IEnumerable<DO.Assignment> assignments = _dal.assignment.ReadAll();
            IEnumerable<DO.Volunteer> volunteers = _dal.Volunteer.ReadAll();

            List<BO.CallAssignInList> myAssignments = assignments
                .Where(a => a.CallId == ID)
                .Select(a => new BO.CallAssignInList
                {
                    RealFinishTime = a.FinishAppointmentTime,
                    FinishAppointmentType = a.FinishAppointmentType == null ? null : (BO.Enums.FinishAppointmentTypeEnum)a.FinishAppointmentType,
                    StartAppointment = a.AppointmentTime,
                    VolunteerId = a.VolunteerId,
                    VolunteerName = volunteers.FirstOrDefault(v => v.Id == a.VolunteerId)?.FullName ?? "Unknown"
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
    }

    public void UpdateCallDetails(BO.Call callDetails)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  // stage 7

        try
        {
            // שלב 1: בדיקת תקינות הערכים (פורמט ולוגיקה)
            CallManager.CheckCallFormat(callDetails);
            CallManager.checkCallLogic(callDetails);

            lock (AdminManager.BlMutex) // stage 7
            {
                // שלב 2: בקשת רשומת הקריאה משכבת הנתונים
                var existingCall = _dal.call.Read(v => v.Id == callDetails.Id)
                    ?? throw new DalDoesNotExistException($"Call with ID {callDetails.Id} not found.");

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
            }

            CallManager.Observers.NotifyItemUpdated(callDetails.Id);  // stage 5
            CallManager.Observers.NotifyListUpdated();  // stage 5
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
        AdminManager.ThrowOnSimulatorIsRunning();  // stage 7
        try
        {
            // הגדרת המשתנים מחוץ לבלוק הנעילה
            BO.Call c = null;
            DO.Assignment assignment = null;

            // שלב 1: בקשת הקריאה משכבת הנתונים עם נעילה
            lock (AdminManager.BlMutex)  // stage 7
            {
                var existingCall = _dal.call.Read(v => v.Id == callId)
                    ?? throw new DalDoesNotExistException("Call not found.");

                c = readCallData(existingCall.Id);  // שמירה במשתנה מחוץ לבלוק
                assignment = _dal.assignment.Read(a => a.CallId == callId);  // שמירה במשתנה מחוץ לבלוק
            }

            // שלב 2: בדיקת סטטוס הקריאה והתאמת התנאים למחיקה
            Console.WriteLine($"Call status: {c.CallStatus}");  // הדפסת סטטוס לקריאה
            if (c.CallStatus != Enums.CalltStatusEnum.OPEN && c.CallStatus != Enums.CalltStatusEnum.CallAlmostOver)
            {
                throw new BLDeletionImpossible("Only open calls can be deleted.");
            }

            // שלב 3: בדיקה אם הקריאה הוקצתה למתנדב
            if (assignment?.VolunteerId != null)
            {
                Console.WriteLine($"Assignment VolunteerId: {assignment.VolunteerId}");  // הדפסת מזהה המתנדב אם קיים
                throw new BLDeletionImpossible("Cannot delete call as it has been assigned to a volunteer.");
            }

            // שלב 4: ניסיון מחיקת הקריאה משכבת הנתונים עם נעילה
            lock (AdminManager.BlMutex)  // stage 7
            {
                Console.WriteLine($"Attempting to delete call with ID {callId}");
                _dal.call.Delete(callId);
                CallManager.Observers.NotifyItemUpdated(callId);  // stage 5
                CallManager.Observers.NotifyListUpdated();  // stage 5
            }
        }
        catch (DO.DalDeletionImpossible ex)
        {
            // שלב 5: אם יש בעיה במחיקה בשכבת הנתונים, זריקת חריגה מתאימה לכיוון שכבת התצוגה
            Console.WriteLine($"Error deleting call from DAL: {ex.Message}");
            throw new ArgumentException("Error deleting call from data layer.", ex);
        }
        catch (Exception ex)
        {
            // שלב 6: טיפול בחריגות וזריקתן מחדש עם מידע ברור לשכבת התצוגה
            Console.WriteLine($"Error processing delete call request for call ID {callId}: {ex.Message}");
            throw new ArgumentException($"Error processing delete call request for call ID {callId}.", ex);
        }
    }



    public void AddCallAsync(BO.Call call)
    {
       CallManager.AddCallAsync(call);
    }

   

    public IEnumerable<BO.ClosedCallInList> GetVolunteerClosedCalls(int volunteerId, BO.Enums.CallTypeEnum? filter, BO.Enums.ClosedCallFieldEnum? toSort)
    {
        // נעילה סביב קריאות ל-DAL
        lock (AdminManager.BlMutex)
        {
            // קריאות ל-DAL
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
    }

    public async Task<IEnumerable<BO.OpenCallInList>> GetOpenCallInListsAsync(
       int volunteerId,
     BO.Enums.CallTypeEnum? filter = null,
     BO.Enums.OpenCallEnum? toSort = null)
    {
        IEnumerable<DO.Call> listCall;
        IEnumerable<DO.Assignment> listAssignment;
        DO.Volunteer volunteer;

        lock (AdminManager.BlMutex)
        {
            listCall = _dal.call.ReadAll();
            listAssignment = _dal.assignment.ReadAll();

            if (!listCall.Any())
                throw new Exception("No calls found in the database.");

            if (!listAssignment.Any())
                throw new Exception("No assignments found in the database.");

            volunteer = _dal.Volunteer.Read(v => v.Id == volunteerId)
                ?? throw new ArgumentException("Volunteer not found.");
        }

        string volunteerAddress = volunteer.Location;
        if (string.IsNullOrWhiteSpace(volunteerAddress))
            throw new ArgumentException("Volunteer location is not provided.");

        // יצירת רשימת BO.OpenCallInList ללא חישוב DistanceOfCall
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
                            MaxFinishTime = call.MaxTime
                        };

        if (filter.HasValue)
            openCalls = openCalls.Where(call => call.CallType == filter.Value);

        openCalls = toSort switch
        {
            BO.Enums.OpenCallEnum.Id => openCalls.OrderBy(call => call.Id),
            BO.Enums.OpenCallEnum.Address => openCalls.OrderBy(call => call.Address),
            BO.Enums.OpenCallEnum.OpenTime => openCalls.OrderBy(call => call.OpenTime),
            BO.Enums.OpenCallEnum.MaxFinishTime => openCalls.OrderBy(call => call.MaxFinishTime),
            _ => openCalls.OrderBy(call => call.Id)
        };

        var openCallsList = openCalls.ToList();

        // חישוב המרחקים מחוץ לבלוק הנעילה
        await UpdateCallDistancesAsync(openCallsList, volunteerAddress);

        return openCallsList;
    }

// מתודה אסינכרונית לחישוב המרחק ועדכון הישות ב-DAL
private async Task UpdateCallDistancesAsync(List<BO.OpenCallInList> openCalls, string volunteerAddress)
{
    try
    {
        double[] volunteerLocation = await Tools.GetGeolocationCoordinatesAsync(volunteerAddress);

            if (volunteerLocation == null || volunteerLocation.Length != 2)
                throw new Exception("Invalid location data received for the volunteer.");

            foreach (var openCall in openCalls)
            {
                // קריאת הישות המקורית מסוג DO.Call לפי המזהה
                var doCall = _dal.call.Read(c => c.Id == openCall.Id);
                if (doCall != null)
                {
                    // שימוש בשדות Latitude ו-Longitude של ה-DO.Call לחישוב המרחק
                    openCall.DistanceOfCall = Tools.CalculateDistance(
                    volunteerLocation[0], volunteerLocation[1],
                    (double)doCall.Latitude, (double)doCall.Longitude);

                    // עדכון הישות ב-DAL
                    _dal.call.Update(doCall);
                }
            }
        }
        catch (Exception ex)
        {
            // ניתן להוסיף לוג מתאים כאן
            Console.WriteLine($"Failed to update call distances: {ex.Message}");
        }
    }



    //public async<BO.Call> CalculateDistanceAndAssignVolunteer(int callId, int volunteerId)
    //{
    //    lock (AdminManager.BlMutex)
    //    {
    //        var call = _dal.call.Read(callId);
    //        var volunteer = _dal.Volunteer.Read(volunteerId);

    //        if (call == null || volunteer == null)
    //            throw new BlDoesNotExistException("Call or Volunteer not found");

    //        string volunteerAddress = volunteer.Location;
    //        string callAddress = call.Adress;

    //        // הוספת await כאן
    //        double[] volunteerLocation =Tools.GetGeolocationCoordinatesAsync(volunteerAddress);
    //        double[] callLocation = Tools.GetGeolocationCoordinatesAsync(callAddress);

    //        // ... (שאר הקוד)
    //    }
    //}

    public void UpdateCallAsCompleted(int volunteerId, int AssignmentId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        try
        {
            // Locking the block to ensure thread safety for DAL operations
            lock (AdminManager.BlMutex)  //stage 7
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

                // Notify observers within the lock to ensure thread safety
                CallManager.Observers.NotifyItemUpdated(assignment.CallId);  //update current call and observers etc.
                CallManager.Observers.NotifyListUpdated();  //update list of calls and observers etc.
                VolunteerManager.Observers.NotifyItemUpdated(volunteerId);  //update current volunteer and observers etc.
                VolunteerManager.Observers.NotifyListUpdated();
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions
            throw new InvalidOperationException($"Error updating call as completed: {ex.Message}", ex);
        }
    }



    public void UpdateToCancelCallTreatment(int RequesterId, int AssignmentId)
    {
        CallManager.UpdateToCancelCallTreatment(RequesterId, AssignmentId);
    }


    public  void AssignCallToVolunteer(int volunteerId, int callId)
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
            if (existingAssignments != null)
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

            // עטיפת פנייה ל DAL בבלוק נעילה
            lock (AdminManager.BlMutex)
            {
                // ניסיון הוספה לשכבת הנתונים
                _dal.assignment.Create(newAssignment);
            }

    public void AssignCallToVolunteer(int volunteerId, int callId)
    {
        CallManager.AssignCallToVolunteer(volunteerId , callId);
    }
}
