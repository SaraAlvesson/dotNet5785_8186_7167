
namespace BlImplementation;
using BlApi;
using BO;
internal class CallImplementation :ICall
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    public int[] GetCallQuantitiesByStatus()
    {
        // קבלת כל הקריאות משכבת הנתונים
        var assignments = _dal.assignment.ReadAll();

        // קיבוץ הקריאות לפי סטטוס
        var groupedCalls = assignments
            .GroupBy(assignment => assignment.FinishAppointmentType) // הנחה: FinishAppointmentType תואם ל-BO.Enums.CallStatusEnum
            .ToDictionary(group => group.Key, group => group.Count());

        // יצירת מערך שבו כל סטטוס ב-enum מקבל את הכמות שלו, או 0 אם אין
        return Enum.GetValues(typeof(BO.Enums.CallStatusEnum))
                   .Cast<BO.Enums.CallStatusEnum>()
                   .Select(status => groupedCalls.TryGetValue(status, out var count) ? count : 0)
                   .ToArray();
    }


    IEnumerable<CallInList> GetCallList
     (
         Enums.CallFieldEnum? filterField = null,
         object filterValue = null,
         Enums.CallFieldEnum? sortField = null
     )
    {
        // שליפת כל הקריאות מהשכבה הנתונים
        var calls = _dal.call.ReadAll()
            .GroupBy(c => c.Id) // קיבוץ לפי CallId להבטחת קריאה אחת עם ההקצאה האחרונה
            .Select(group => group.OrderByDescending(c => c.AssignmentDate).First()) // בחירת ההקצאה האחרונה
            .Select(c => new BO.CallInList
            {
                Id = c.Id,
                CallId = c.CallId,
                CallType = (Enums.CallTypeEnum)c.CallType,
                OpenTime = c.OpenTime,
                SumTimeUntilFinish = c.FinishTime - c.OpenTime,
                LastVolunteerName = c.LastVolunteer?.Name ?? "No Volunteer",
                SumAppointmentTime = group.Sum(a => a.Duration),
                Status = (Enums.CallStatusEnum)c.Status,
                SumAssignment = group.Count()
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


    public DO.Call GetCallDetails(int callId)
    {
        return _dal.call.Read(callId); // Retrieve call details by ID
    }


    public void UpdateCall(DO.Call call)
    {
        _dal.call.Update(call); // Update the call in the data layer
    }

    public void DeleteCall(int callId)
    {
        _dal.call.Delete(callId); // Delete the call by ID
    }

    public void AddCall(DO.Call call)
    {
        _dal.call.Update(call); // Check if the call already exists
    }


    IEnumerable<ClosedCallInList> GetClosedCallsByVolunteer(
      string volunteerId,
      Enums.CallTypeEnum? callType = null,
      Enums.CallFieldEnum? sortField = null
     )
    {
        var calls = _dal.call.ReadAll().Where(call => call.VolunteerId == volunteerId && call.Status == CallStatusEnum.Closed).ToList();

        if (callType.HasValue)
        {
            calls = calls.Where(call => call.Type == callType.Value).ToList();
        }

        if (sortField.HasValue)
        {
            calls = SortCalls(calls, sortField.Value).ToList();
        }

        return calls.Select(call => new ClosedCallInList(call)); // Return the list of closed calls handled by the volunteer
    }


    IEnumerable<OpenCallInList> GetOpenCallsForVolunteer(
         string volunteerId,
         Enums.CallTypeEnum? callType = null,
         Enums.CallFieldEnum? sortField = null
     )
    {
        var calls = _dal.call.ReadAll().Where(call => call.VolunteerId == volunteerId && call.Status == CallStatusEnum.Open).ToList();

        if (callType.HasValue)
        {
            calls = calls.Where(call => call.Type == callType.Value).ToList();
        }

        if (sortField.HasValue)
        {
            calls = SortCalls(calls, sortField.Value).ToList();
        }

        return calls.Select(call => new OpenCallInList(call)); // Return the list of open calls available for the volunteer
    }

    public void MarkCallAsCompleted(string volunteerId, int assignmentId)
    {
        var call = _dal.call.ReadAll().FirstOrDefault(c => c.VolunteerId == volunteerId && c.AssignmentId == assignmentId);
        if (call != null)
        {
            call.Status = CallStatusEnum.Completed;
            _dal.Call.Update(call); // Update the call status to "Completed"
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

