using BO;
using DalApi;
using DO;
using System.Net;
using System.Reflection;
using System.Text.Json;
using static BO.Exceptions;


namespace Helpers
{
    public  static class Tools
    {
        private static IDal s_dal = DalApi.Factory.Get;

        internal static BO.Enums.CalltStatusEnum callStatus(int ID)
        {
            lock (AdminManager.BlMutex) // Using lock for DAL operations
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
        }

        public static string ToStringProperty<T>(this T t)
        {
            string str = "";
            if (t != null)
            {
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

        public static async Task<double[]> GetGeolocationCoordinatesAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException("Address cannot be empty or null.", nameof(address));
            }

            string apiKey = "678694850f91d165965268skuda91dd"; // your API key
            string requestUrl = $"https://geocode.maps.co/search?q={Uri.EscapeDataString(address)}&api_key={apiKey}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(requestUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Request failed with status: {response.StatusCode}, details: {errorContent}");
                    }

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var locationData = JsonSerializer.Deserialize<LocationResult[]>(jsonResponse, jsonOptions);

                    if (locationData == null || locationData.Length == 0)
                    {
                        throw new Exception("No geolocation data found for the given address.");
                    }

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


        public static async Task<bool> IsAddressValidAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new InvalidAddressFormatException("Address cannot be empty or null.");
            }

            var addressParts = address.Split(',');

            if (addressParts.Length < 3)
            {
                throw new InvalidAddressFormatException("Address must contain at least street, city, and country.");
            }

            string street = addressParts[0].Trim();
            string city = addressParts.Length > 1 ? addressParts[1].Trim() : string.Empty;
            string country = addressParts.Length > 2 ? addressParts[2].Trim() : string.Empty;

            if (string.IsNullOrEmpty(street) || string.IsNullOrEmpty(city) || string.IsNullOrEmpty(country))
            {
                throw new InvalidAddressFormatException("Address is missing essential parts like street, city, or country.");
            }

            string apiKey = "678694850f91d165965268skuda91dd";
            string requestUrl = $"https://geocode.maps.co/search?q={Uri.EscapeDataString(address)}&api_key={apiKey}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(requestUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var locationData = JsonSerializer.Deserialize<LocationResult[]>(jsonResponse, jsonOptions);

                        if (locationData != null && locationData.Length > 0)
                        {
                            return true;
                        }
                        else
                        {
                            throw new InvalidGeolocationException("No geolocation data found for the provided address.");
                        }
                    }
                    else
                    {
                        throw new InvalidGeolocationException($"Failed to retrieve data for the address. Status code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidGeolocationException("An error occurred while verifying the geolocation: " + ex.Message);
                }
            }
        }


        public static bool IsValidAddress(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return false;

            // בדיקה בסיסית לכתובת תקינה
            // ניתן להרחיב את הבדיקה בהתאם לדרישות הספציפיות
            return address.Length >= 5 && address.Contains(' ');
        }

        public class LocationResult
        {
            public string Lat { get; set; }
            public string Lon { get; set; }
        }

        public static double CalculateDistance(double lat1 = 0, double lon1 = 0, double lat2 = 0, double lon2 = 0)
        {
            const double R = 6371;

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

            return R * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        public static DO.DistanceType GetDistanceType(double distanceInKm)
        {
            const double walkingDistanceThreshold = 3.0;
            const double drivingDistanceThreshold = 50.0;
            const double airDistanceThreshold = 1000.0;

            if (distanceInKm <= walkingDistanceThreshold)
                return DO.DistanceType.WalkingDistance;
            else if (distanceInKm <= drivingDistanceThreshold)
                return DO.DistanceType.DrivingDistance;
            else
                return DO.DistanceType.AerialDistance;
        }
    }
}
