using BO;
using System.Text.Json; // שים לב לשימוש ב- System.Text.Json במקום Newtonsoft.Json
using DalApi;
using DO;
using System.Net.Http;

namespace Helpers
{
    internal static class CallManager 
    {
        private static IDal s_dal = Factory.Get; //stage 4

        internal static ObserverManager Observers = new(); //stage 5 





        internal static BO.Enums.CalltStatusEnum CheckStatus(DO.Assignment doAssignment, DO.Call doCall, TimeSpan? riskTimeSpan)
        {
            // טיפול במקרה שבו doAssignment הוא null
            if (doAssignment == null)
            {
                return BO.Enums.CalltStatusEnum.OPEN; // מצב ברירת מחדל
            }

            // טיפול במקרה שבו doCall הוא null
            if (doCall == null)
            {
                return BO.Enums.CalltStatusEnum.OPEN; // מצב ברירת מחדל, בלי לזרוק חריגה
            }

            // לוגיקה קיימת
            if (doAssignment.VolunteerId == null ||
                doAssignment.FinishAppointmentType == FinishAppointmentType.CancelingAnAdministrator ||
                doAssignment.FinishAppointmentType == FinishAppointmentType.SelfCancellation)
            {
                if (doCall.MaxTime.HasValue && doCall.MaxTime.Value - DateTime.Now <= riskTimeSpan)
                {
                    return BO.Enums.CalltStatusEnum.CallAlmostOver; // פתוחה בסיכון
                }
                return BO.Enums.CalltStatusEnum.OPEN; // פתוחה
            }
            else if (doAssignment.FinishAppointmentType == FinishAppointmentType.WasTreated)
            {
                return BO.Enums.CalltStatusEnum.CLOSED; // סגורה
            }
            else if (doAssignment.FinishAppointmentType == FinishAppointmentType.CancellationHasExpired)
            {
                return BO.Enums.CalltStatusEnum.EXPIRED; // פג תוקף
            }
            else if (doAssignment.VolunteerId != null)
            {
                if (doCall.MaxTime.HasValue && doCall.MaxTime.Value - DateTime.Now <= riskTimeSpan)
                {
                    return BO.Enums.CalltStatusEnum.CallTreatmentAlmostOver; // בטיפול בסיכון
                }
                return BO.Enums.CalltStatusEnum.CallIsBeingTreated; // בטיפול
            }
            else
            {
                return BO.Enums.CalltStatusEnum.UNKNOWN; // מצב ברירת מחדל
            }
        }


        public static void checkCallLogic(BO.Call call)
        {
            // בדיקת יחס זמנים
            if (call.MaxFinishTime <= call.OpenTime)
                throw new InvalidCallLogicException("Max finish time must be later than open time.");

            // קריאה אסינכרונית לפונקציה בתוך Task.Run
            //Task.Run(async () =>
            //{
            //    if (!await IsValidAddressAsync(call.Address))
            //        throw new InvalidCallLogicException("Address is invalid or does not exist.");

            //    // בדיקת התאמה בין כתובת לקואורדינטות
            //    double[] GeolocationCoordinates = Tools.GetGeolocationCoordinates(call.Address);

            //    double expectedLongitude = GeolocationCoordinates[0];
            //    double expectedLatitude = GeolocationCoordinates[1];

            //    // השוואה בין שני ערכים מסוג double
            //    if (Math.Abs((double)call.Longitude - expectedLongitude) > 0.0001 || Math.Abs((double)call.Latitude - expectedLatitude) > 0.0001)
            //        throw new InvalidCallLogicException("Longitude and Latitude do not match the provided address.");
            //}).GetAwaiter().GetResult(); // מחכה שהמשימה תסתיים לפני המשך הקריאה
        }



        public static void checkCallFormat(BO.Call call)
        {
            // בדיקת מזהה
            if (call.Id <= 0)
                throw new InvalidCallFormatException("Call ID must be a positive integer.");

            // בדיקת זמן פתיחה
            if (call.OpenTime == default)
                throw new InvalidCallFormatException("Open time is not valid.");

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

        public static async Task<bool> IsValidAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return false;
            }

            try
            {
                var httpClient = new HttpClient();
                var apiKey = "ba9b0180f2cd4da1999edc1820855bdb";  // המפתח שלך
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={apiKey}";

                var response = await httpClient.GetStringAsync(url);

                // הפענוח של התשובה בתור JSON
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(response);
                var status = jsonResponse.GetProperty("status").GetString();

                // אם הסטטוס הוא "OK", אז הכתובת קיימת
                if (status == "OK")
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                // אם יש שגיאה בבקשה או במענה
                Console.WriteLine($"Error occurred: {ex.Message}");
            }

            return false; // אם לא מצאנו כתובת או קרתה שגיאה
        }




        public static void UpdateExpired()
        {
            // Step 1: Retrieve all calls where MaxTimeToEnd has passed and need handling
            var expiredCalls = s_dal.call.ReadAll(c => c.MaxTime < AdminManager.Now);

            // Step 2: Handle calls without assignments
            foreach (var call in expiredCalls)
            {
                var hasAssignment = s_dal.assignment.ReadAll(a => a.CallId == call.Id).Any();

                if (!hasAssignment)
                {
                    var newAssignment = new DO.Assignment
                    {
                        Id = 0,  // New ID will be generated
                        CallId = call.Id,
                        VolunteerId = 0,
                        AppointmentTime = AdminManager.Now,
                        FinishAppointmentType = DO.FinishAppointmentType.CancellationHasExpired
                    };
                    s_dal.assignment.Create(newAssignment);
                }
            }

            // Step 3: Update existing assignments with null FinishAppointmentTime
            var assignmentsToUpdate = s_dal.assignment.ReadAll(a => a.FinishAppointmentTime == null);

            foreach (var assignment in assignmentsToUpdate)
            {
                var call = expiredCalls.FirstOrDefault(c => c.Id == assignment.CallId);
                if (call != null)
                {
                    var updatedAssignment = assignment with
                    {
                        FinishAppointmentTime = AdminManager.Now,
                        FinishAppointmentType = DO.FinishAppointmentType.CancellationHasExpired
                    };
                    s_dal.assignment.Update(updatedAssignment);

                    // Notify specific item update
                    Observers.NotifyItemUpdated(updatedAssignment.Id);
                }
            }

            // Step 5: Notify observers for list update
            Observers.NotifyListUpdated();
        }

       
    }

   }