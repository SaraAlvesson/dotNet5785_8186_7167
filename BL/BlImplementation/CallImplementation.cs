
namespace BlImplementation;
using BlApi;
using BO;
using DalApi;
using static BO.Enums;
using DO;

internal class CallImplementation : ICall
{

    private readonly DalApi.IDal _dal = DalApi.Factory.Get();


    /// <summary>
    /// Returns the quantity of calls grouped by their status (e.g., open, closed).
    /// </summary>
    public int[] GetCallQuantitiesByStatus()
    {
        var calls = _dal.call.ReadAll; // Retrieve all calls from the data layer
        return Enum.GetValues(typeof(CallTypeEnum)) // Iterate over all status values
                   .Cast<CallStatusEnum>()
                   .Select(status => calls.Count(call => call.Status == status)) // Count the number of calls for each status
                   .ToArray();
    }

    /// <summary>
    /// Retrieves a list of calls, with optional filtering and sorting.
    /// </summary>
    public IEnumerable<CallInList> GetCallList(Enums.CallFieldEnum? filterField = null, object filterValue = null,  Enums.CallFieldEnum? sortField = null)
    {
        var calls = _dal.call.ReadAll; // Retrieve all calls from the data layer

        if (filterField.HasValue && filterValue != null)
        {
            calls = calls.Where(call => FilterCall(call, filterField.Value, filterValue)).ToList(); // Apply filtering if needed
        }

        if (sortField.HasValue)
        {
            calls = SortCalls(calls, sortField.Value).ToList(); // Apply sorting if needed
        }

        return calls.Select(call => new CallInList(call)); // Return the transformed call list
    }

    /// <summary>
    /// Filters the calls based on the specified field and value.
    /// </summary>
    private bool FilterCall(Call call, CallFieldEnum field, object value)
    {
        switch (field)
        {
            case CallFieldEnum.Status:
                return call.Status.Equals(value); // Filter by status
            case CallFieldEnum.Type:
                return call.Type.Equals(value); // Filter by type
            default:
                return false;
        }
    }

    /// <summary>
    /// Sorts the calls by the specified field.
    /// </summary>
    private IEnumerable<Call> SortCalls(IEnumerable<Call> calls, CallFieldEnum field)
    {
        switch (field)
        {
            case CallFieldEnum.CallNumber:
                return calls.OrderBy(call => call.CallNumber); // Sort by call number
            case CallFieldEnum.DateCreated:
                return calls.OrderBy(call => call.DateCreated); // Sort by creation date
            default:
                return calls; // No sorting if no field is provided
        }
    }

    /// <summary>
    /// Retrieves the details of a specific call by its ID.
    /// </summary>
    public Call GetCallDetails(int callId)
    {
        var call = _dataLayer.GetCallById(callId); // Fetch the call by ID
        if (call == null)
        {
            throw new Exception($"Call with ID {callId} not found"); // Throw an exception if not found
        }

        return new Call
        {
            CallId = call.CallId,
            CallNumber = call.CallNumber,
            Status = call.Status,
            Assignments = _dataLayer.GetAssignmentsForCall(callId) // Fetch related assignments
        };
    }

    /// <summary>
    /// Updates the details of an existing call.
    /// </summary>
    public void UpdateCall(Call call)
    {
        ValidateCall(call); // Validate the call before updating

        var callData = new CallData
        {
            CallId = call.CallId,
            CallNumber = call.CallNumber,
            Status = call.Status
        };

        _dataLayer.UpdateCall(callData); // Update the call in the data layer
    }

    /// <summary>
    /// Validates the provided call data (e.g., ensuring end time is after start time).
    /// </summary>
    private void ValidateCall(Call call)
    {
        if (call.EndTime < call.StartTime)
        {
            throw new Exception("End time must be greater than start time."); // Validation error for incorrect time
        }
        if (!IsValidAddress(call.Address))
        {
            throw new Exception("Invalid address format."); // Validation error for incorrect address format
        }
    }

    /// <summary>
    /// Validates the address format for the call.
    /// </summary>
    private bool IsValidAddress(string address)
    {
        return !string.IsNullOrWhiteSpace(address); // Address should not be null or empty
    }

    /// <summary>
    /// Deletes a call by its ID, only if it's open and not assigned.
    /// </summary>
    public void DeleteCall(int callId)
    {
        var call = _dataLayer.GetCallById(callId);
        if (call == null || call.Status != CallStatusEnum.Open || call.Assignment != null)
        {
            throw new Exception("Call cannot be deleted."); // Error if call cannot be deleted due to status or assignment
        }

        _dataLayer.DeleteCall(callId); // Delete the call from the data layer
    }

    /// <summary>
    /// Adds a new call to the system, ensuring no duplicate exists.
    /// </summary>
    public void AddCall(Call call)
    {
        if (_dataLayer.GetCallById(call.CallId) != null)
        {
            throw new Exception("Call already exists."); // Error if the call already exists
        }

        _dataLayer.AddCall(new CallData
        {
            CallId = call.CallId,
            CallNumber = call.CallNumber,
            Status = call.Status
        }); // Add the call to the data layer
    }

    /// <summary>
    /// Retrieves a list of closed calls assigned to a specific volunteer.
    /// </summary>
    public IEnumerable<ClosedCallInList> GetClosedCallsByVolunteer(string volunteerId, CallTypeEnum? callType = null, CallFieldEnum? sortField = null)
    {
        var calls = _dataLayer.GetClosedCallsByVolunteer(volunteerId);

        if (callType.HasValue)
        {
            calls = calls.Where(call => call.Type == callType).ToList(); // Apply type filter if needed
        }

        if (sortField.HasValue)
        {
            calls = SortClosedCalls(calls, sortField.Value).ToList(); // Apply sorting if needed
        }

        return calls.Select(call => new ClosedCallInList(call)); // Return the transformed closed call list
    }

    /// <summary>
    /// Sorts closed calls by a specific field.
    /// </summary>
    private IEnumerable<ClosedCall> SortClosedCalls(IEnumerable<ClosedCall> calls, CallFieldEnum sortField)
    {
        switch (sortField)
        {
            case CallFieldEnum.CallNumber:
                return calls.OrderBy(call => call.CallNumber); // Sort by call number
            default:
                return calls; // No sorting if no field is provided
        }
    }

    /// <summary>
    /// Retrieves a list of open calls assigned to a specific volunteer.
    /// </summary>
    public IEnumerable<OpenCallInList> GetOpenCallsForVolunteer(string volunteerId, CallTypeEnum? callType = null, CallFieldEnum? sortField = null)
    {
        var calls = _dataLayer.GetOpenCallsForVolunteer(volunteerId);

        if (callType.HasValue)
        {
            calls = calls.Where(call => call.Type == callType).ToList(); // Apply type filter if needed
        }

        if (sortField.HasValue)
        {
            calls = SortOpenCalls(calls, sortField.Value).ToList(); // Apply sorting if needed
        }

        return calls.Select(call => new OpenCallInList(call)); // Return the transformed open call list
    }

    /// <summary>
    /// Sorts open calls by a specific field.
    /// </summary>
    private IEnumerable<OpenCall> SortOpenCalls(IEnumerable<OpenCall> calls, CallFieldEnum sortField)
    {
        switch (sortField)
        {
            case CallFieldEnum.CallNumber:
                return calls.OrderBy(call => call.CallNumber); // Sort by call number
            default:
                return calls; // No sorting if no field is provided
        }
    }

    /// <summary>
    /// Marks a specific call assignment as completed.
    /// </summary>
    public void MarkCallAsCompleted(string volunteerId, int assignmentId)
    {
        var assignment = _dataLayer.GetAssignmentById(assignmentId);
        if (assignment == null || assignment.VolunteerId != volunteerId || assignment.Status != AssignmentStatusEnum.Open)
        {
            throw new Exception("Invalid assignment status."); // Error if the assignment is not valid for completion
        }

        assignment.Status = AssignmentStatusEnum.Completed; // Update the assignment status
        _dataLayer.UpdateAssignment(assignment); // Update the assignment in the data layer
    }

    /// <summary>
    /// Cancels the treatment for a specific call assignment.
    /// </summary>
    public void CancelCallTreatment(string volunteerId, int assignmentId)
    {
        var assignment = _dataLayer.GetAssignmentById(assignmentId);
        if (assignment == null || assignment.VolunteerId != volunteerId || assignment.Status == AssignmentStatusEnum.Completed)
        {
            throw new Exception("Invalid assignment status."); // Error if the assignment is invalid for cancellation
        }

        assignment.Status = AssignmentStatusEnum.Cancelled; // Update the assignment status to cancelled
        _dataLayer.UpdateAssignment(assignment); // Update the assignment in the data layer
    }

    /// <summary>
    /// Assigns a specific call to

}

