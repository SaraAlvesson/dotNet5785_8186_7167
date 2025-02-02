
using BO;
using DalApi;
using DO;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace Helpers
{
    internal static class Tools
    {
        private static IDal s_dal = DalApi.Factory.Get; //stage 4
        internal static BO.Enums.CalltStatusEnum callStatus(int ID)
        {
            DO.Call? c = s_dal.call.Read(ID);

            if (c == null)
                throw new DO.DalDoesNotExistException($"call with ID: {ID} doesn't exist!");

            DO.Assignment? a = s_dal.assignment.Read(item => item.CallId == ID);
            if (a == null)
                if (s_dal.config.Clock - c.OpenTime > s_dal.config.RiskRange)
                    return BO.Enums.CalltStatusEnum.CallAlmostOver;
                else
                    return BO.Enums.CalltStatusEnum.OPEN;

            if (a.FinishAppointmentType is null)
                if (s_dal.config.Clock - a.AppointmentTime > s_dal.config.RiskRange)
                    return BO.Enums.CalltStatusEnum.CallTreatmentAlmostOver;
                else
                    return BO.Enums.CalltStatusEnum.CallIsBeingTreated;

            if (a.FinishAppointmentType == DO.FinishAppointmentType.CancellationHasExpired)
                return BO.Enums.CalltStatusEnum.EXPIRED;

            if (a.FinishAppointmentType == DO.FinishAppointmentType.WasTreated)
                return BO.Enums.CalltStatusEnum.CLOSED;

            if (a.FinishAppointmentType == DO.FinishAppointmentType.SelfCancellation || a.FinishAppointmentType == DO.FinishAppointmentType.CancelingAnAdministrator)
                return BO.Enums.CalltStatusEnum.Canceled;

            else
                return BO.Enums.CalltStatusEnum.UNKNOWN;

        }

        //internal static BO.Enums.CalltStatusEnum CheckStatus(DO.Assignment doAssignment, DO.Call doCall, TimeSpan? riskTimeSpan)
        //{
        //    // טיפול במקרה שבו doAssignment הוא null
        //    if (doAssignment == null || doCall == null)
        //    {
        //        return BO.Enums.CalltStatusEnum.UNKNOWN; // לא ניתן להחזיר מצב אם הנתונים חסרים
        //    }

        //    // אם הקריאה בטיפול של מתנדב מסוים
        //    if (doAssignment.VolunteerId != null)
        //    {
        //        // אם הקריאה מתקרבת לסיום (15 שעות לפני סיום)
        //        if (doCall.MaxTime.HasValue)
        //        {
        //            // חישוב הזמן שנשאר עד ל-MaxTime
        //            TimeSpan timeRemaining = doCall.MaxTime.Value - DateTime.Now;

        //            // בדיקה אם הזמן שנשאר הוא 15 שעות או פחות
        //            if (timeRemaining <= riskTimeSpan)
        //            {
        //                return BO.Enums.CalltStatusEnum.CallTreatmentAlmostOver; // קריאה בטיפול שמתקרבת לסיום - בטיפול בסיכון
        //            }
        //        }

        //        return BO.Enums.CalltStatusEnum.CallIsBeingTreated; // בטיפול - יש מתנדב שטיפל בה
        //    }

        //    return BO.Enums.CalltStatusEnum.UNKNOWN; // מצב ברירת מחדל אם לא בטיפול
        //}
        //internal static BO.Enums.CalltStatusEnum CheckStatusCalls(DO.Assignment doAssignment, DO.Call doCall, TimeSpan? riskTimeSpan)
        //{
        //    // טיפול במקרה שבו doAssignment או doCall הם null
        //    if (doAssignment == null || doCall == null)
        //    {
        //        return BO.Enums.CalltStatusEnum.OPEN; // לא ניתן להחזיר מצב אם הנתונים חסרים
        //    }

        //    // אם הקריאה לא נמצאת בטיפול כרגע
        //    if (doAssignment.VolunteerId == null)
        //    {
        //        // אם הקריאה מתקרבת לזמן סיום הדרוש לה
        //        if (doCall.MaxTime.HasValue)
        //        {
        //            TimeSpan timeRemaining = doCall.MaxTime.Value - DateTime.Now;

        //            if (timeRemaining <= TimeSpan.Zero) // הזמן עבר
        //            {
        //                return BO.Enums.CalltStatusEnum.OPEN; // פג תוקף
        //            }
        //            else if (timeRemaining <= riskTimeSpan) // זמן קרוב מאוד לסיום
        //            {
        //                return BO.Enums.CalltStatusEnum.OPEN; // פתוחה בסיכון
        //            }
        //        }

        //        return BO.Enums.CalltStatusEnum.OPEN; // קריאה פתוחה
        //    }

        //    // אם הקריאה בטיפול כרגע על ידי מתנדב
        //    if (doCall.MaxTime.HasValue)
        //    {
        //        TimeSpan timeRemaining = doCall.MaxTime.Value - DateTime.Now;

        //        if (timeRemaining <= TimeSpan.Zero) // הזמן עבר
        //        {
        //            return BO.Enums.CalltStatusEnum.OPEN; // פג תוקף גם אם היא בטיפול
        //        }
        //        else if (timeRemaining <= riskTimeSpan) // זמן קרוב מאוד לסיום
        //        {
        //            return BO.Enums.CalltStatusEnum.OPEN; // בטיפול בסיכון
        //        }
        //    }

        //    return BO.Enums.CalltStatusEnum.OPEN; // קריאה בטיפול
        //}


        // The generic method works for any object, returning a string of its properties
        //public static BO.CallStatus GetCallStatus(DO.Call doCall)
        //{
        //    if (doCall.MaxTimeToClose < _dal.Config.Clock)
        //        return BO.CallStatus.Expired;
        //    var lastAssignment = _dal.Assignment.ReadAll(ass => ass.CallId == doCall.Id).OrderByDescending(a => a.TimeStart).FirstOrDefault();

        //    if (lastAssignment == null)
        //    {
        //        if (IsInRisk(doCall!))
        //            return BO.CallStatus.OpenRisk;
        //        else return BO.CallStatus.Open;
        //    }
        //    if (lastAssignment.TypeEndTreat.ToString() == "Treated")
        //    {
        //        return BO.CallStatus.Closed;
        //    }
        //    if (lastAssignment.TypeEndTreat == null)
        //    {
        //        if (IsInRisk(doCall!))
        //            return BO.CallStatus.InProgressRisk;
        //        else return BO.CallStatus.InProgress;
        //    }
        //    return BO.CallStatus.Closed;//default
        //}

        public static string ToStringProperty<T>(this T t)
        {
            string str = "";
            if (t != null)
            {
                // Using reflection to get properties of the object
                foreach (PropertyInfo item in t.GetType().GetProperties())
                {
                    var value = item.GetValue(t, null);
                    if (value != null)
                    {
                        if (value is System.Collections.IEnumerable enumerable && !(value is string))
                        {
                            str += $"\n{item.Name}: [";
                            foreach (var subItem in enumerable)
                            {
                                str += subItem + ", ";
                            }
                            str = str.TrimEnd(',', ' ') + "]";
                        }
                        else
                        {
                            str += $"\n{item.Name}: {value}";
                        }
                    }
                }
            }
            return str;
        }

        /// <summary>
        /// Retrieves geolocation coordinates (latitude and longitude) for a given address using an external geocoding API.
        /// </summary>
        /// <param name="address">Address to search for</param>
        /// <returns>Array containing latitude and longitude</returns>
        /// <remarks>
        /// Written with the help of ChatGPT from OpenAI (https://openai.com).
        /// </remarks>
        internal static double[] GetGeolocationCoordinates(string address)
        {
            // בדיקת תקינות של הכתובת
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException("Address cannot be empty or null.", nameof(address));
            }

            string apiKey = "678694850f91d165965268skuda91dd"; // המפתח שלך
            string requestUrl = $"https://geocode.maps.co/search?q={Uri.EscapeDataString(address)}&api_key={apiKey}"; // URL של Forward Geocoding

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // שליחת הבקשה לשרת
                    HttpResponseMessage response = client.GetAsync(requestUrl).GetAwaiter().GetResult();

                    // בדיקה אם התשובה תקינה
                    if (!response.IsSuccessStatusCode)
                    {
                        string errorContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        throw new Exception($"Request failed with status: {response.StatusCode}, details: {errorContent}");
                    }

                    // קריאת התשובה
                    string jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    // ניתוח התשובה
                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var locationData = JsonSerializer.Deserialize<LocationResult[]>(jsonResponse, jsonOptions);

                    if (locationData == null || locationData.Length == 0)
                    {
                        throw new Exception("No geolocation data found for the given address.");
                    }

                    // החזרת קווי הרוחב והאורך
                    if (double.TryParse(locationData[0].Lat, out var lat) && double.TryParse(locationData[0].Lon, out var lon))
                    {
                        return new double[] { lat, lon };
                    }
                    else
                    {
                        throw new Exception("Invalid latitude or longitude.");
                    }
                }
                catch (HttpRequestException ex)
                {
                    throw new Exception("Error sending web request: " + ex.Message);
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred: " + ex.Message);
                }
            }
        }



        /// <summary>
        /// Represents a location result returned by the geocoding service.
        /// </summary>
        /// <remarks>
        /// This class was created with the help of ChatGPT from OpenAI (https://openai.com).
        /// </remarks>
        public class LocationResult
        {
            /// <summary>
            /// Latitude of the location.
            /// </summary>
            public string Lat { get; set; }

            /// <summary>
            /// Longitude of the location.
            /// </summary>
            public string Lon { get; set; }

            // Add additional properties if needed (e.g., address, city, country)
        }
        /// <summary>
        /// Calculates the distance between two geographical points using Haversine formula.
        /// </summary>
        /// <param name="lat1">Latitude of the first point.</param>
        /// <param name="lon1">Longitude of the first point.</param>
        /// <param name="lat2">Latitude of the second point.</param>
        /// <param name="lon2">Longitude of the second point.</param>
        /// <returns>The distance between the two points in kilometers.</returns>
        /// <remarks>
        /// Code by ChatGPT (OpenAI).
        /// </remarks>
        public static double CalculateDistance(double lat1=0, double lon1=0, double lat2=0, double lon2=0)
        {
            const double R = 6371; // Radius of the Earth in kilometers

            double lat1Rad = ToRadians(lat1);
            double lon1Rad = ToRadians(lon1);
            double lat2Rad = ToRadians(lat2);
            double lon2Rad = ToRadians(lon2);

            double dLat = lat2Rad - lat1Rad;
            double dLon = lon2Rad - lon1Rad;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // Distance in kilometers
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">The degree value to convert.</param>
        /// <returns>The corresponding value in radians.</returns>
        private static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
        /// <summary>
        /// Determines the type of distance based on the given distance in kilometers.
        /// </summary>
        /// <param name="distanceInKm">Distance in kilometers.</param>
        /// <returns>Distance type: WalkingDistance, DrivingDistance, or AirDistance.</returns>
        /// <author>ChatGPT, OpenAI</author>

        public static DO.DistanceType GetDistanceType(double distanceInKm)
        {
            // Thresholds for categorizing distances
            const double walkingDistanceThreshold = 3.0;  // <= 3 km for WalkingDistance
            const double drivingDistanceThreshold = 50.0; // <= 50 km for DrivingDistance
            const double airDistanceThreshold = 1000.0;   // <= 1000 km for AirDistance

            // Check if the distance is within walking range
            if (distanceInKm <= walkingDistanceThreshold)
            {
                return DO.DistanceType.WalkingDistance; // Walking distance for <= 3 km
            }
            // Check if the distance is within driving range
            else if (distanceInKm <= drivingDistanceThreshold)
            {
                return DO.DistanceType.DrivingDistance; // Driving distance for <= 50 km
            }
            // Check if the distance is within aerial range
            else if (distanceInKm <= airDistanceThreshold)
            {
                return DO.DistanceType.AerialDistance; // Air distance for <= 1000 km
            }
            else
            {
                // Default to AerialDistance for greater than 1000 km
                return DO.DistanceType.AerialDistance;
            }
        }

        internal static double CalculateDistance(double v1, double v2, double latitude, double? longitude)
        {
            throw new NotImplementedException();
        }
    }
}
