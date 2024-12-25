using BO;
using System.Text.Json; // שים לב לשימוש ב- System.Text.Json במקום Newtonsoft.Json
using DalApi;
using DO;
namespace Helpers;

namespace Helpers
{
    internal static class CallManager
    {
        private static IDal s_dal = Factory.Get; //stage 4

    internal static BO.CallInProgress GetCallInProgress(DO.Call doCall, DateTime entryTime, double distanceFromVolunteer)

    {

        return new BO.CallInProgress

        {

            Id = doCall.Id,

            CallId = doCall.Id,

            CallType = (BO.CallType)doCall.Type,

            Description = doCall.Description,

            FullAddress = doCall.FullAddress,

            MaxCompletionTime = doCall.MaxTimeToClose,

            EntryTime = entryTime,

            DistanceFromVolunteer = distanceFromVolunteer,

            OpeningTime = doCall.TimeOpened

        };

    }
   
    internal static BO.ClosedCallInList GetClosedCallInList(DO.Call doCall, DO.Assignment? doAssignment)

    {

        public static void checkCallLogic(BO.Call call)
        {
            // בדיקת יחס זמנים
            if (call.MaxFinishTime <= call.OpenTime)
                throw new InvalidCallLogicException("Max finish time must be later than open time.");

            // בדיקת תקינות הכתובת
            if (!IsValidAddress(call.Address))
                throw new InvalidCallLogicException("Address is invalid or does not exist.");

            CallType = (BO.CallType)doCall.Type,

            FullAddress = doCall.FullAddress,

            OpeningTime = doCall.TimeOpened,

            EntryTime = doAssignment?.TimeStart ?? throw new BO.BlWrongItemtException($"Assignment missing for Call ID {doCall.Id}"),

            CompletionTime = doAssignment?.TimeEnd,

            CompletionType = doAssignment?.TypeEndTreat.HasValue == true

             ? (BO.AssignmentCompletionType?)doAssignment.TypeEndTreat.Value

            // בדיקת זמן מקסימלי
            if (call.MaxFinishTime == default)
                throw new InvalidCallFormatException("Max finish time is not valid.");

            // בדיקת כתובת
            if (string.IsNullOrWhiteSpace(call.Address) || call.Address.Length > 200)
                throw new InvalidCallFormatException("Address is either empty or exceeds the maximum length (200 characters).");

            // בדיקת אורך ורוחב
            if (call.Longitude < -180 || call.Longitude > 180)
                throw new InvalidCallFormatException("Longitude must be between -180 and 180 degrees.");

            if (call.Latitude < -90 || call.Latitude > 90)
                throw new InvalidCallFormatException("Latitude must be between -90 and 90 degrees.");
        }

    internal static CallAssignmentInList GetCallAssignmentInList(DO.Assignment doAssignment, string volunteerName)

            try
            {
                // ניתן להשתמש ב-API של Google Maps או שירותים אחרים כדי לבדוק אם הכתובת קיימת
                // דוגמת שימוש ב-API של Google Maps
                var httpClient = new HttpClient();
                var apiKey = "your_api_key";  // המפתח שלך לשירות Google Maps או כל שירות אחר
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={apiKey}";
                var response = httpClient.GetStringAsync(url).Result;

        return new CallAssignmentInList

        {

            VolunteerId = doAssignment.VolunteerId,

            VolunteerName = volunteerName,

            StartTime = doAssignment.TimeStart,

            EndTime = doAssignment.TimeEnd,

            CompletionType = doAssignment.TypeEndTreat.HasValue

                ? (BO.AssignmentCompletionType?)doAssignment.TypeEndTreat.Value

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

    private static void ValidateCallData(DO.Call doCall)

    {

        if (string.IsNullOrWhiteSpace(doCall.Description))

            throw new ArgumentException("Description cannot be null or empty.");



        if (string.IsNullOrWhiteSpace(doCall.FullAddress))

            throw new ArgumentException("FullAddress cannot be null or empty.");



        if (doCall.Latitude < -90 || doCall.Latitude > 90)

            throw new ArgumentOutOfRangeException(nameof(doCall.Latitude), "Latitude must be between -90 and 90.");



        if (doCall.Longitude < -180 || doCall.Longitude > 180)

            throw new ArgumentOutOfRangeException(nameof(doCall.Longitude), "Longitude must be between -180 and 180.");



        if (doCall.MaxTimeToClose.HasValue && doCall.MaxTimeToClose <= doCall.TimeOpened)

            throw new ArgumentException("MaxTimeToClose must be later than TimeOpened.");

    }



    //internal static BO.Call ConvertDOToBO(DO.Call doCall)

    //{

    //    // קריאה לפונקציית העזר לבדיקת תקינות

    //    ValidateCallData(doCall);
    //    // המרה ל־BO

    //    return new BO.Call

    //    {

    //        Id = doCall.Id,

    //        CallType = (BO.Enums.CallTypeEnum)doCall.CallType,

    //        VerbDesc = doCall.VerbDesc,

    //        Address = doCall.Adress,

    //        Latitude = doCall.Latitude,

    //        Longitude = doCall.Longitude,

    //        OpenTime = doCall.OpenTime,

    //        MaxFinishTime = doCall.MaxTime,




    //    };
    //}
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
}


