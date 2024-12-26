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

        internal static BO.Enums.CalltStatusEnum CheckStatus(DO.Assignment doAssignment, DO.Call doCall, TimeSpan? riskTimeSpan)
        {
            // טיפול במקרה שבו doAssignment הוא null
            if (doAssignment == null)
            {
                return BO.Enums.CalltStatusEnum.UNKNOWN; // מצב ברירת מחדל
            }

            // טיפול במקרה שבו doCall הוא null (אם יש צורך)
            if (doCall == null)
            {
                throw new ArgumentNullException(nameof(doCall), "doCall cannot be null");
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
                return BO.Enums.CalltStatusEnum.UNKNOWN; // מצב ברירת מחדל
            }
        }


        public static void checkCallLogic(BO.Call call)
        {
            // בדיקת יחס זמנים
            if (call.MaxFinishTime <= call.OpenTime)
                throw new InvalidCallLogicException("Max finish time must be later than open time.");

            // בדיקת תקינות הכתובת
            if (!IsValidAddress(call.Address))
                throw new InvalidCallLogicException("Address is invalid or does not exist.");

            // בדיקת התאמה בין כתובת לקואורדינטות
            double[] GeolocationCoordinates = Tools.GetGeolocationCoordinates(call.Address);

            double expectedLongitude = GeolocationCoordinates[0];
            double expectedLatitude = GeolocationCoordinates[1];

            // השוואה בין שני ערכים מסוג double
            if (Math.Abs((double)call.Longitude - expectedLongitude) > 0.0001 || Math.Abs((double)call.Latitude - expectedLatitude) > 0.0001)
                throw new InvalidCallLogicException("Longitude and Latitude do not match the provided address.");

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

        public static bool IsValidAddress(string address)
        {
            // אם הכתובת ריקה או ארוכה מדי, נחזיר false
            if (string.IsNullOrWhiteSpace(address) || address.Length > 200)
                return false;

            try
            {
                // ניתן להשתמש ב-API של Google Maps או שירותים אחרים כדי לבדוק אם הכתובת קיימת
                // דוגמת שימוש ב-API של Google Maps
                var httpClient = new HttpClient();
                var apiKey = "your_api_key";  // המפתח שלך לשירות Google Maps או כל שירות אחר
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={apiKey}";
                var response = httpClient.GetStringAsync(url).Result;

                // אם התקבלה תשובה, נבדוק אם יש תוצאה, כך נוודא שהכתובת תקינה
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(response); // שימוש ב-System.Text.Json
                if (jsonResponse.GetProperty("status").GetString() == "OK")
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                // טיפול בחריגה במידה ויש
                Console.WriteLine($"Error validating address: {ex.Message}");
            }

            return false;  // אם הכתובת לא קיימת או לא הצלחנו לאמת אותה, נחזיר false
        }

        //  מתודת עדכון הקריאות הפתוחות שפג תוקפן
        //internal static void UpdateExpiredCalls()
        //{
        //    var calls = s_dal.call.ReadAll();
        //    // 1. נלך על כל הקריאות הפתוחות
        //    foreach (var call in calls) // Assuming _dal.Calls returns the list of calls
        //    {
        //        if (call.EndDate < ClockManager.Now) // 2. אם זמן הסיום עבר
        //        {
        //            // 3. קריאות שאין להן עדיין הקצאה
        //            if (call.Assignment == null)
        //            {
        //                var newAssignment = new Assignment
        //                {
        //                    CallId = call.Id,
        //                    EndDate = ClockManager.Now,
        //                    EndReason = "ביטול פג תוקף",
        //                    VolunteerId = 0 // ת.ז מתנדב
        //                };
        //                _dal.assignments.Add(newAssignment); // מוסיפים הקצאה חדשה
        //            }
        //            else if (call.Assignment.EndTreatmentDate == null) // 4. קריאות שיש להן הקצאה אך לא הסתיים טיפולן
        //            {
        //                call.Assignment.EndTreatmentDate = ClockManager.Now;
        //                call.Assignment.EndReason = "ביטול פג תוקף"; // עדכון סיום טיפול
        //                _dal.SaveChanges(); // שומרים את השינויים
        //            }

        //            // 5. שליחת הודעה למשקיפים על עדכון הקריאה הספציפית (אם יש)
        //            if (call.Observers != null && call.Observers.Count > 0)
        //            {
        //                foreach (var observer in call.Observers)
        //                {
        //                    observer?.Invoke(); // שולחים את ההודעה למשקיף
        //                }
        //            }
        //        }
        //    }

        //    // 6. שליחת הודעה על עדכון רשימת הקריאות
        //    CallListUpdated?.Invoke();
        //}

    }
}