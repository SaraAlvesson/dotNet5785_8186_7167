using BO;
using System.Text.RegularExpressions;
using DalApi;
using DO;
namespace Helpers;

internal static class CallManager

{
    
    internal static BO.Call GetCall(int id)
    {
         DalApi.IDal s_dal = DalApi.Factory.Get;

         var call = s_dal.call.Read(id);
         var assignment = s_dal.assignment.Read(id);
         var volunteer=s_dal.Volunteer.Read(id);

        if (call == null)   
        throw new ArgumentNullException($"Call with ID {id} does not exist.");

        BO.CallAssignInList CallAssiInList = GetCallAssignInList(assignment, volunteer.FullName);
        BO.CallInList callList

        var assignments = s_dal.assignment.ReadAll(a => a.CallId == call.Id)
            .Select(a => CallAssiInList);

        return new BO.Call
        {
            Id = call.Id,
            CallType = (BO.CallType)call.CallType,
            Address = call.Address,
            Latitude = call.Latitude,
            Longitude = call.Longitude,
            OpenTime = call.OpenTime,
            MaxTime = call.MaxTime,
            VerbDesc = call.VerbDesc,
            CallAssignInLists = assignments == null ? null : assignments.ToList()
            CallStatus=
        };
    }
    internal static BO.CallAssignInList GetCallAssignInList(DO.Assignment doAssignment, string volunteerName)
    {
        if(volunteerName == null) throw new ArgumentNullException(nameof(volunteerName));
        return new CallAssignInList
        {
            VolunteerId = doAssignment.VolunteerId,
            VolunteerName = volunteerName,
            AppointmentTime = doAssignment.AppointmentTime,
            FinishAppointmentTime = doAssignment.FinishAppointmentTime,
            FinishAppointmentType = (BO.FinishAppointmentType)doAssignment.FinishAppointmentType

        };
    }


    private static IDal s_dal = Factory.Get; //stage 4

    internal static BO.CallInProgress GetCallInProgress(DO.Call doCall, DateTime entryTime, double distanceFromVolunteer)

    {

        return new BO.CallInProgress

        {

            Id = doCall.Id,

            CallId = doCall.Id,

            CallType = (BO.Enums.CallType)doCall.CallType,

            VerbDesc = doCall.VerbDesc,

            CallAddress = doCall.Address,

            //MaxFinishTime = doCall.MaxFinishTime,

            //StartAppointmentTime = doCall.StartAppointmentTime,

            DistanceOfCall = distanceFromVolunteer,

            StartAppointmentTime = doCall.OpenTime

        };

    }

    internal static BO.ClosedCallInList GetClosedCallInList(DO.Call doCall, DO.Assignment? doAssignment)
    {
        // אם לא קיים Assignment, זרוק חריגה
        if (doAssignment == null)
        {
            throw new Exception($"Assignment missing for Call ID {doCall.Id}");
        }

        // החזרת אובייקט BO.ClosedCallInList שמממש את פרטי הקריאה והטיפול
        return new BO.ClosedCallInList
        {
            Id = doCall.Id,  // מזהה הקריאה
            Address = doCall.Address,  // כתובת הקריאה
            CallType = (BO.Enums.CallType)doCall.CallType,  // המרה של סוג הקריאה מ-DO ל-BO
            OpenTime = doCall.OpenTime,  // זמן פתיחה של הקריאה
            TreatmentStartTime = doAssignment.AppointmentTime,  // זמן תחילת טיפול
            RealFinishTime = doAssignment.FinishAppointmentTime,  // זמן סיום טיפול
            FinishAppointmentType = doAssignment.FinishAppointmentType,

        };
    }


  

    internal static BO.OpenCallInList GetOpenCallInList(DO.Call doCall, double distanceFromVolunteer)

    {

        return new BO.OpenCallInList

        {

            Id = doCall.Id,

            CallType = (BO.Enums.CallType)doCall.CallType,

            VerbDesc = doCall.VerbDesc,

            Address = doCall.Address,

            OpenTime = doCall.OpenTime,

            MaxFinishTime = doCall.MaxTime,

            DistanceOfCall = distanceFromVolunteer

        };

    }
    internal static BO.CallInList GetCallInList(DO.Call doCall, double distanceFromVolunteer)

    {

        return new BO.CallInList

        {

            Id = doCall.Id,

            CallType = (BO.Enums.CallType)doCall.CallType,

            VerbDesc = doCall.VerbDesc,

            Address = doCall.Address,

            OpenTime = doCall.OpenTime,

            MaxFinishTime = doCall.MaxTime,

            DistanceOfCall = distanceFromVolunteer

        };

    }

    private static void ValidateCallData(DO.Call doCall)

    {

        if (string.IsNullOrWhiteSpace(doCall.VerbDesc))

            throw new ArgumentException("Description cannot be null or empty.");



        if (string.IsNullOrWhiteSpace(doCall.Address))

            throw new ArgumentException("FullAddress cannot be null or empty.");



        if (doCall.Latitude < -90 || doCall.Latitude > 90)

            throw new ArgumentOutOfRangeException(nameof(doCall.Latitude), "Latitude must be between -90 and 90.");



        if (doCall.Longitude < -180 || doCall.Longitude > 180)

            throw new ArgumentOutOfRangeException(nameof(doCall.Longitude), "Longitude must be between -180 and 180.");



        if (doCall.MaxTime.HasValue && doCall.MaxTime <= doCall.OpenTime)

            throw new ArgumentException("MaxTimeToClose must be later than TimeOpened.");

    }



    internal static BO.Call ConvertDOToBO(DO.Call doCall)

    {

        // קריאה לפונקציית העזר לבדיקת תקינות

        ValidateCallData(doCall);



        // המרה ל־BO

        return new BO.Call

        {

            Id = doCall.Id,

            CallType = (BO.CallType)doCall.CallType,

            VerbDesc = doCall.VerbDesc,

            Address = doCall.Address,

            Latitude = doCall.Latitude,

            Longitude = doCall.Longitude,

            OpenTime = doCall.OpenTime,

            MaxTime = doCall.MaxTime

        };

    }
}
