using BO;
using System.Text.Json; // שים לב לשימוש ב- System.Text.Json במקום Newtonsoft.Json
using DalApi;
using DO;
using System.Net.Http;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading.Tasks;


namespace Helpers
{
    internal static class CallManager 
    {
        private static IDal s_dal = Factory.Get; //stage 4

        internal static ObserverManager Observers = new(); //stage 5 


      

        internal static BO.Enums.FinishAppointmentTypeEnum? ConvertEndType(DO.FinishAppointmentType? finishAppointment)
        {
            if (finishAppointment == null)
            {
                // אם finishAppointment הוא null, נחזיר null
                return null;
            }

            switch (finishAppointment.Value)
            {
                case DO.FinishAppointmentType.WasTreated:
                    return BO.Enums.FinishAppointmentTypeEnum.WasTreated;
                case DO.FinishAppointmentType.SelfCancellation:
                    return BO.Enums.FinishAppointmentTypeEnum.SelfCancellation;
                case DO.FinishAppointmentType.CancelingAnAdministrator:
                    return BO.Enums.FinishAppointmentTypeEnum.CancelingAnAdministrator;
                case DO.FinishAppointmentType.CancellationHasExpired:
                    return BO.Enums.FinishAppointmentTypeEnum.CancellationHasExpired;
                default:
                    throw new ArgumentException("Invalid finish appointment type", nameof(finishAppointment));
            }
        }


        public static void checkCallLogic(BO.Call call)
        {
            // בדיקת יחס זמנים
            if (call.MaxFinishTime != null && call.MaxFinishTime <= call.OpenTime)
                throw new InvalidCallLogicException("Max finish time must be later than open time.");
        }



        public static bool IsAddressValid(string address)
        {
            // ודא שהכנסת את המפתח שלך כאן
            string ApiKey = "YOUR_GOOGLE_MAPS_API_KEY";  // הכנס את ה-API Key שלך כאן

            using (HttpClient client = new HttpClient())
            {
                string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={ApiKey}";

                try
                {
                    // שליחת הבקשה ל-Google Geocoding API
                    HttpResponseMessage response = client.GetAsync(url).Result; // סינכרונית
                    response.EnsureSuccessStatusCode(); // אם לא הצליחה, תזרוק שגיאה
                    string responseBody = response.Content.ReadAsStringAsync().Result; // סינכרונית

                    // השתמש ב-System.Text.Json כדי לנתח את התשובה
                    using (JsonDocument jsonDoc = JsonDocument.Parse(responseBody))
                    {
                        // הדפסת התשובה כולה כדי לבדוק
                        Console.WriteLine("API Response: " + responseBody);

                        var status = jsonDoc.RootElement.GetProperty("status").GetString();
                        Console.WriteLine("Status: " + status);  // הדפס את הסטטוס

                        // אם הסטטוס הוא "OK", הכתובת חוקית
                        return status == "OK";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false; // טיפול בשגיאות ברשת או בשגיאות של ה-API
                }
            }
        }








        public static void checkCallFormat(BO.Call call)
        {
            // בדיקת מזהה
            if (call.Id <= 0)
                throw new InvalidCallFormatException("Call ID must be a positive integer.");

            // בדיקת זמן פתיחה
            if (call.OpenTime == default)
                throw new InvalidCallFormatException("Open time is not valid.");

            // בדיקת כתובת
            if (string.IsNullOrWhiteSpace(call.Address))
                throw new InvalidCallFormatException("Address cannot be empty.");

            if (call.Address.Length > 200)
                throw new InvalidCallFormatException("Address exceeds the maximum length of 200 characters.");

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