namespace BlImplementation;
using BlApi;
using BO;
using DO;
using Helpers;
using BL.Helpers;
using static BO.Enums;
using static BO.Exceptions;
using DalApi;

internal class CallImplementation : BlApi.ICall
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
        var allCalls = GetCallList(null, null, null,null);

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


    public IEnumerable<BO.CallInList> GetCallList(BO.Enums.CallFieldEnum? filter, object? toFilter, BO.Enums.CallFieldEnum? toSort, BO.Enums.CalltStatusEnum? toSort2)
    {
        return CallManager.GetCallList(filter, toFilter, toSort,toSort2);
    }


    public BO.Call readCallData(int ID)
    {
        return CallManager.readCallData(ID);
    }

    public void UpdateCallDetails(BO.Call callDetails)
    {
        CallManager.UpdateCallDetails(callDetails);
    }
    public void DeleteCall(int callId)
    {
        CallManager.DeleteCall(callId);
    }



    public void AddCallAsync(BO.Call call)
    {
        CallManager.AddCallAsync(call);
    }

    // מתודה אסינכרונית שתעדכן את הקואורדינטות לאחר קבלתן מהרשת
    public async Task UpdateCallWithCoordinatesAsync(DO.Call oldCall)
    {
        try
        {
            Console.WriteLine($"Fetching coordinates for call ID: {oldCall.Id}");

            double[] coordinates = await Tools.GetGeolocationCoordinatesAsync(oldCall.Adress);
            Console.WriteLine($"Coordinates retrieved: Lat={coordinates[0]}, Lon={coordinates[1]}");

            // יצירת אובייקט חדש עם הערכים הישנים + הקואורדינטות החדשות
            DO.Call updatedCall = new()
            {
                Id = oldCall.Id,
                OpenTime = oldCall.OpenTime,
                MaxTime = oldCall.MaxTime,
                Longitude = coordinates[1],
                Latitude = coordinates[0],
                Adress = oldCall.Adress,
                CallType = oldCall.CallType,
                VerbDesc = oldCall.VerbDesc
            };

            // עדכון ה-DAL עם האובייקט החדש
            lock (AdminManager.BlMutex)
            {

                _dal.call.Update(updatedCall);
                Console.WriteLine($"Call updated in DAL with coordinates: {updatedCall.Id}");
            }

            CallManager.Observers.NotifyItemUpdated(updatedCall.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateCallWithCoordinatesAsync: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }

    // מתודה אסינכרונית שתעדכן את הקואורדינטות לאחר קבלתן מהרשת

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
        await UpdateCallDistancesAsync(openCallsList, volunteer);

        // **סינון הקריאות לפי MaxDistance**
        openCallsList = openCallsList
            .Where(call => call.DistanceOfCall <= volunteer.MaxDistance)
            .ToList();

        return openCallsList;
    }

    // מתודה אסינכרונית לחישוב המרחק ועדכון הישות ב-DAL
    private async Task UpdateCallDistancesAsync(List<BO.OpenCallInList> openCalls, DO.Volunteer volunteer)
    {
        try
        {
            double[] volunteerLocation = await Tools.GetGeolocationCoordinatesAsync(volunteer.Location);

            if (volunteerLocation == null || volunteerLocation.Length != 2)
                throw new Exception("Invalid location data received for the volunteer.");

            foreach (var openCall in openCalls)
            {
                var doCall = _dal.call.Read(c => c.Id == openCall.Id);
                if (doCall != null)
                {
                    openCall.DistanceOfCall = Tools.CalculateDistance(
                        volunteerLocation[0], volunteerLocation[1],
                        (double)doCall.Latitude, (double)doCall.Longitude);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to update call distances: {ex.Message}");
        }
    }



    public void UpdateCallAsCompleted(int volunteerId, int AssignmentId)
    {
        CallManager.UpdateCallAsCompleted(volunteerId, AssignmentId);
    }



    public void UpdateToCancelCallTreatment(int RequesterId, int AssignmentId)
    {
        CallManager.UpdateToCancelCallTreatment(RequesterId, AssignmentId);
    }


    public void AssignCallToVolunteer(int volunteerId, int callId)
    {
        CallManager.AssignCallToVolunteer(volunteerId, callId);
    }
}
