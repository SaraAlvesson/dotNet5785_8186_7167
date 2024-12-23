//using System.Net;
//using System.Reflection;
//using System.Text.Json;

//namespace Helpers
//{
//    internal static class Tools
//    {
//        // מתודת עזר להמיר אובייקט למיתר
//        public static string ToStringProperty<T>(this T t)
//        {
//            return t?.ToString() ?? string.Empty;
//        }

//        // בדיקה אם כתובת אימייל תקינה
//        public static bool IsValidEmail(string email)
//        {
//            if (string.IsNullOrWhiteSpace(email))
//                return false;

//            try
//            {
//                var addr = new System.Net.Mail.MailAddress(email);
//                return addr.Address == email;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        // בדיקה אם מספר תעודת זהות תקין
//        public static bool IsValidId(int id)
//        {
//            string idStr = id.ToString("D9"); // ת.ז. צריכה להיות באורך 9 ספרות
//            int sum = 0;

//            for (int i = 0; i < idStr.Length; i++)
//            {
//                int digit = (idStr[i] - '0') * ((i % 2) + 1);
//                sum += (digit > 9) ? digit - 9 : digit;
//            }

//            return sum % 10 == 0;
//        }

//        // בדיקה אם כתובת תקינה (לפחות 5 תווים)
//        public static bool IsValidAddress(string address)
//        {
//            return !string.IsNullOrWhiteSpace(address) && address.Length > 5;
//        }
//    }
//}
using BO;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace Helpers
{
    internal static class Tools
    {


        // The generic method works for any object, returning a string of its properties
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
                        str += "\n" + item.Name + ": " + value;
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
            // Check if the address is valid
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException("Address cannot be empty or null.", nameof(address));
            }

            string apiKey = "67609238e7135923908907oxh46a576";
            string requestUrl = $"https://geocode.maps.co/search?q={Uri.EscapeDataString(address)}&api_key={apiKey}";
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(requestUrl);
            webRequest.Method = "GET";

            try
            {
                // Send request and get response
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    if (webResponse.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"Request failed with status: {webResponse.StatusCode}");
                    }

                    using (StreamReader responseReader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        string jsonResponse = responseReader.ReadToEnd();
                        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var locationData = JsonSerializer.Deserialize<LocationResult[]>(jsonResponse, jsonOptions);

                        if (locationData == null || locationData.Length == 0)
                        {
                            throw new Exceptions.AddressDoesNotExistException("No geolocation data found for the given address.");
                        }

                        // Return latitude and longitude
                        return new double[] { double.Parse(locationData[0].Lat), double.Parse(locationData[0].Lon) };
                    }
                }
            }
            catch (WebException ex)
            {
                throw new Exception("Error sending web request: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
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
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
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

            if (distanceInKm <= walkingDistanceThreshold)
            {
                return DO.DistanceType.WalkingDistance; // Walking distance for <= 3 km
            }
            else if (distanceInKm <= drivingDistanceThreshold)
            {
                return DO.DistanceType.DrivingDistance; // Driving distance for <= 50 km
            }
            else if (distanceInKm <= airDistanceThreshold)
            {
                return DO.DistanceType.AerialDistance; // Air distance for <= 1000 km
            }
            else
            {
                return DO.DistanceType.AerialDistance; // Default to AirDistance for greater than 1000 km
            }
        }

    }
}
