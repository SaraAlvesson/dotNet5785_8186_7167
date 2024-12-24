using BO;
using System.Text.RegularExpressions;
using DalApi;
using DO;
using static BO.Exceptions;
namespace Helpers;

internal static class CallManager

{

    private static IDal s_dal = Factory.Get; //stage 4

    internal static BO.CallInProgress GetCallInProgress(DO.Call doCall, DateTime entryTime, double distanceFromVolunteer)

    {

        return new BO.CallInProgress

        {

            Id = doCall.Id,

            CallId = doCall.Id,

            CallType = (BO.Enums.CallTypeEnum)doCall.CallType,

            VerbDesc = doCall.VerbDesc,

            CallAddress = doCall.Adress,

            MaxFinishTime = doCall.MaxTime,

            OpenTime = entryTime,

            DistanceOfCall = distanceFromVolunteer,

            StartAppointmentTime = doCall.OpenTime

        };

    }
   
    internal static BO.ClosedCallInList GetClosedCallInList(DO.Call doCall, DO.Assignment? doAssignment)

    {

        return new BO.ClosedCallInList

        {

            Id = doCall.Id,

            CallType = (BO.Enums.CallTypeEnum)doCall.CallType,

            Address = doCall.Adress,

            OpenTime = doCall.OpenTime,

            TreatmentStartTime = doAssignment?.AppointmentTime ?? throw new BO.BlWrongItemtException($"Assignment missing for Call ID {doCall.Id}"),

            RealFinishTime = doAssignment?.FinishAppointmentTime,

            FinishAppointmentType = doAssignment?.FinishAppointmentType.HasValue == true

             ? (BO.Enums.FinishAppointmentTypeEnum?)doAssignment.FinishAppointmentType.Value

             : null



        };

    }

    internal static CallAssignInList GetCallAssignmentInList(DO.Assignment doAssignment, string volunteerName)

    {

        return new CallAssignInList

        {

            VolunteerId = doAssignment.VolunteerId,

            VolunteerName = volunteerName,

            OpenTime = doAssignment.AppointmentTime,

            RealFinishTime = doAssignment.FinishAppointmentTime,

            FinishAppointmentType = doAssignment.FinishAppointmentType.HasValue

                ? (BO.Enums.FinishAppointmentTypeEnum?)doAssignment.ty//.Value

                : null

        };

    }

    internal static BO.OpenCallInList GetOpenCallInList(DO.Call doCall, double distanceFromVolunteer)

    {

        return new BO.OpenCallInList

        {

            Id = doCall.Id,

            CallType = (BO.Enums.CallTypeEnum)doCall.CallType,

            VerbDesc = doCall.VerbDesc,

            Address = doCall.Adress,

            OpenTime = doCall.OpenTime,

            MaxFinishTime = doCall.MaxTime,

            DistanceOfCall = distanceFromVolunteer

        };

    }

   
    public static void checkCallAdress(BO.Call doCall)

    {

        if (string.IsNullOrWhiteSpace(doCall.VerbDesc))

            throw new ArgumentException("Description cannot be null or empty.");



        if (string.IsNullOrWhiteSpace(doCall.Address))

            throw new ArgumentException("Full Address cannot be null or empty.");



        if (doCall.Latitude < -90 || doCall.Latitude > 90)

            throw new ArgumentOutOfRangeException(nameof(doCall.Latitude), "Latitude must be between -90 and 90.");



        if (doCall.Longitude < -180 || doCall.Longitude > 180)

            throw new ArgumentOutOfRangeException(nameof(doCall.Longitude), "Longitude must be between -180 and 180.");



        if (doCall.MaxFinishTime.HasValue && doCall.MaxFinishTime <= doCall.OpenTime)

            throw new ArgumentException("MaxTime  must be later than the open time.");

    }



    internal static BO.Call ConvertDOToBO(DO.Call doCall)

    {

        return new BO.Call

        {

            Id = doCall.Id,

            CallType = (BO.Enums.CallTypeEnum)doCall.CallType,

            VerbDesc = doCall.VerbDesc,

            Address = doCall.Adress,

            Latitude = doCall.Latitude,

            Longitude = doCall.Longitude,

            OpenTime = doCall.OpenTime,

            MaxFinishTime = doCall.MaxTime,




        };
    }
    internal static BO.Enums.CalltStatusEnum CheckStatus(DO.Assignment doAssignment,DO.Call doCall)
         {
        if (doAssignment.VolunteerId == null || doAssignment.FinishAppointmentType == FinishAppointmentType.CancelingAnAdministrator
            || doAssignment.FinishAppointmentType == FinishAppointmentType.SelfCancellation)
            return BO.Enums.CalltStatusEnum.OPEN;
        else if (doAssignment.VolunteerId != null)
            return BO.Enums.CalltStatusEnum.CallIsBeingTreated;
        else if (doAssignment.FinishAppointmentType == FinishAppointmentType.WasTreated)
            return BO.Enums.CalltStatusEnum.CLOSED;
        else if (doAssignment.VolunteerId == null || doAssignment.FinishAppointmentType == FinishAppointmentType.CancellationHasExpired)
            return BO.Enums.CalltStatusEnum.EXPIRED;
        // else if (doCall.MaxTime)////////////////////////////////////////////////////////////////////////////////////
        ///////  return;////////////////////////////////
        else return;
       
         }


    
}
