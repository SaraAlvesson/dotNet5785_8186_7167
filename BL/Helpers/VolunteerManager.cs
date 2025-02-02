using BlApi;
using BlImplementation;
using BO;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static BO.Exceptions;

namespace Helpers
{
    internal static class VolunteersManager
    {

        internal static ObserverManager Observers = new(); //stage 5 

        private static IDal s_dal = DalApi.Factory.Get; //stage 4
        /// <summary>
        /// convert volunteer form Do to volunteerINList  from BO
        /// </summary>
        /// <param name="doVolunteer"> get the volunteer to return </param>
        /// <returns> the BO vlounteerInList  </returns>
        internal static BO.VolunteerInList convertDOToBOInList(DO.Volunteer doVolunteer)
        {
            var call = s_dal.assignment.ReadAll(ass => ass.VolunteerId == doVolunteer.Id).ToList();
            int sumCalls = call.Count(ass => ass.FinishAppointmentType == DO.FinishAppointmentType.WasTreated);
            int sumCanceld = call.Count(ass => ass.FinishAppointmentType == DO.FinishAppointmentType.CancelingAnAdministrator);
            int sumExpired = call.Count(ass => ass.FinishAppointmentType == DO.FinishAppointmentType.CancellationHasExpired);
            //int? idCall = call.Count(ass => ass.finishTreatment == null);
            return new()
            {
                Id = doVolunteer.Id,
                FullName = doVolunteer.FullName,
                Active = doVolunteer.Active,
                SumTreatedCalls = sumCalls,
                SumCanceledCalls = sumCanceld,
                SumExpiredCalls = sumExpired,
                //CallId = idCall,
            };
        }
        /// <summary>
        /// get volunteer from do and convert it to Bo volunteer 
        /// </summary>
        /// <param name = "doVolunteer" > the Dovolunteer</param>
        /// <returns>the bo vlounteer</returns>
        internal static BO.Volunteer convertDOToBOVolunteer(DO.Volunteer doVolunteer)
        {
            var call = s_dal.assignment.ReadAll(ass => ass.VolunteerId == doVolunteer.Id).ToList();
            int sumCalls = call.Count(ass => ass.FinishAppointmentType == DO.FinishAppointmentType.WasTreated);
            int sumCanceld = call.Count(ass => ass.FinishAppointmentType == DO.FinishAppointmentType.CancelingAnAdministrator);
            int sumExpired = call.Count(ass => ass.FinishAppointmentType == DO.FinishAppointmentType.CancellationHasExpired);
            //int? idCall = call.Count(ass => ass.finishTreatment == null);
            CallInProgress? c = GetCallIn(doVolunteer);
            return new BO.Volunteer()
            {
                Id = doVolunteer.Id,
                FullName = doVolunteer.FullName,
                PhoneNumber = doVolunteer.PhoneNumber,
                Email = doVolunteer.Email,
                Password = doVolunteer.Password != null ? Decrypt(doVolunteer.Password) : null,
                Location = doVolunteer.Location,
                Latitude = doVolunteer.Latitude,
                Longitude = doVolunteer.Longitude,
                Position = (BO.Enums.VolunteerTypeEnum)doVolunteer.Position,
                Active = doVolunteer.Active,
                SumCalls = sumCalls,
                SumCanceled = sumCanceld,
                SumExpired = sumExpired,
                MaxDistance = doVolunteer.MaxDistance,
                DistanceType = (BO.Enums.DistanceTypeEnum)doVolunteer.DistanceType,
                VolunteerTakenCare = c

            };
        }
        ///// <summary>
        ///// get volunteer and return Call in prgers if there is one 
        ///// </summary>
        ///// <param name="doVolunteer"> the volunteer we wnat to check if there is </param>
        ///// <returns>callin progerss th this spsifiec volunteer </returns>
        internal static BO.CallInProgress? GetCallIn(DO.Volunteer doVolunteer)
        {

            var call = s_dal.assignment.ReadAll(ass => ass.VolunteerId == doVolunteer.Id).ToList();
            DO.Assignment? assignmentTreat = call.Find(ass => ass.FinishAppointmentType == null);

            if (assignmentTreat != null)
            {
                DO.Call? callTreat = s_dal.call.Read(c => c.Id == assignmentTreat.CallId);

                if (callTreat != null)
                {
                    double latitude = (double)(doVolunteer.Latitude ?? callTreat.Latitude);
                    double longitude = (double)(doVolunteer.Longitude ?? callTreat.Longitude);
                    return new()
                    {
                        Id = assignmentTreat.Id,
                        CallId = assignmentTreat.CallId,
                        CallType = (BO.Enums.CallTypeEnum)callTreat.CallType,
                        VerbDesc = callTreat.VerbDesc,
                        CallAddress = callTreat.Adress,
                        OpenTime = callTreat.OpenTime,
                        MaxFinishTime = callTreat.MaxTime,
                        StartAppointmentTime = assignmentTreat.AppointmentTime,
                        DistanceOfCall = Tools.CalculateDistance((double)callTreat.Latitude, (double)callTreat.Longitude, latitude, longitude),
                        //Status = (callTreat.MaxTime - ClockManager.Now <= s_dal.? BO.Enums.CalltStatusEnum.CallTreatmentAlmostOver : BO.Enums.CalltStatusEnum.CallIsBeingTreated),
                    };
                }
            }
            return null;
        }

        private const string ApiKey = "YOUR_GOOGLE_MAPS_API_KEY";

        public static bool IsAddressValid(string address)
        {

            using (HttpClient client = new HttpClient())
            {
                string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={ApiKey}";

                try
                {
                    HttpResponseMessage response = client.GetAsync(url).Result; // סינכרונית
                    response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().Result; // סינכרונית

                    // השתמש ב-System.Text.Json
                    using (JsonDocument jsonDoc = JsonDocument.Parse(responseBody))
                    {
                        var status = jsonDoc.RootElement.GetProperty("status").GetString();

                        return status == "OK"; // אם הסטטוס הוא "OK", הכתובת חוקית
                    }
                }
                catch (Exception)
                {
                    return false; // טיפול בשגיאות ברשת או בשגיאות של ה-API
                }
            }
        }


        internal static bool IsPhoneNumberValid(BO.Volunteer volunteer)
        {
            // וידוא שלא מדובר במחרוזת ריקה או null
            if (string.IsNullOrEmpty(volunteer.PhoneNumber))
                throw new BO.Exceptions.BlPhoneNumberNotCorrect($"The PhoneNumber: {volunteer.PhoneNumber} is incorrect (empty or null).");

            // וידוא שכל התווים הם ספרות בלבד
            if (!volunteer.PhoneNumber.All(char.IsDigit))
                throw new BO.Exceptions.BlPhoneNumberNotCorrect($"The PhoneNumber: {volunteer.PhoneNumber} contains invalid characters.");

            // וידוא שהאורך מתאים
            if (volunteer.PhoneNumber.Length < 10 || volunteer.PhoneNumber.Length > 10)
                throw new BO.Exceptions.BlPhoneNumberNotCorrect($"The PhoneNumber: {volunteer.PhoneNumber} has an invalid length.");

            // אם כל הבדיקות עברו בהצלחה
            return true;
        }

        internal static bool checkVolunteerLocation(BO.Volunteer volunteer)
        {
            if (volunteer.MaxDistance < 0)
                throw new BlMaxDistanceNotCorrect("ERROR- Max Distance cannot be negative ");
            else
                return true;
        }


       
        



        internal static bool checkVolunteerEmail(BO.Volunteer volunteer)
        {
            if (string.IsNullOrEmpty(volunteer.Email) || volunteer.Email.Count(c => c == '@') != 1)
            {
                throw new BO.Exceptions.BlEmailNotCorrect($"email :{volunteer.Email} Is incorrect ");

            }
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (!Regex.IsMatch(volunteer.Email, pattern))
            {
                throw new BO.Exceptions.BlEmailNotCorrect($"email :{volunteer.Email}  Is incorrect  ");
            }
            else
                return true;
        }




        internal static bool IsValidId(int id)
        {
            // בדיקה אם תעודת הזהות היא בת 9 ספרות
            if (id < 100000000 || id > 999999999)
            {
                return false;
            }

            // אם ה-ID תקין, מחזירים true
            return true;
        }


        //// אלגוריתם לונה לבדוק את תקינות תעודת הזהות
        //int sum = 0;
        //long idCopy = id; // העתק של תעודת הזהות, כדי שנוכל לעבוד עם כל ספרה בנפרד

        //for (int i = 0; i < 8; i++)
        //{
        //    int digit = (int)(idCopy % 10);
        //    idCopy /= 10;

        //    // אם אנחנו במקום זוגי (ממקום 2 ומעלה), הכפל את הספרה ב-2
        //    if (i % 2 == 0)
        //    {
        //        sum += digit; // הוספת ספרה זוגית כפי שהיא
        //    }
        //    else
        //    {
        //        int doubled = digit * 2;
        //        sum += doubled > 9 ? doubled - 9 : doubled; // אם התוצאה מעל 9, חיסור 9
        //    }
        //}

        //// חישוב ספרת הביקורת
        //int checkDigit = (10 - (sum % 10)) % 10;

        //// השוואה בין ספרת הביקורת לבין הספרה האחרונה בתעודה
        //return checkDigit == (int)(id % 10); // אם התוצאה תואמת, תעודת הזהות תקינה
        internal static DO.Volunteer convertFormBOVolunteerToDoAsync(BO.Volunteer BoVolunteer)
        {
            if (BoVolunteer.Location != null)
            {
                // שימוש באסינכרוניות
                double[] coordinates = Tools.GetGeolocationCoordinates(BoVolunteer.Location);
                BoVolunteer.Latitude = coordinates[0];
                BoVolunteer.Longitude = coordinates[1];
            }
            else
            {
                BoVolunteer.Latitude = null;
                BoVolunteer.Longitude = null;
            }

            // יצירת אובייקט DO.Volunteer
            DO.Volunteer doVl = new(
                Id: BoVolunteer.Id,
                FullName: BoVolunteer.FullName,
                PhoneNumber: BoVolunteer.PhoneNumber,
                Email: BoVolunteer.Email,
                Active: BoVolunteer.Active,
                Position: (DO.Position)BoVolunteer.Position,
                DistanceType: (DO.DistanceType)BoVolunteer.DistanceType,
                Password: BoVolunteer.Password != null ? Encrypt(BoVolunteer.Password) : null,
                Location: BoVolunteer.Location,
                Latitude: BoVolunteer.Latitude,
                Longitude: BoVolunteer.Longitude,
                MaxDistance: BoVolunteer.MaxDistance
            );

            return doVl;
        }

        // A constant shift value for encryption and decryption (simple Caesar Cipher)
        private static readonly int shift = 3;
        /// <summary>
        /// Encrypts the given password by shifting each character's ASCII value by the specified shift.
        /// </summary>
        /// <param name="password">The password to be encrypted.</param>
        /// <returns>The encrypted password as a string.</returns>
        /// <remarks>
        /// This function was created with the assistance of ChatGPT, a language model developed by OpenAI.
        /// </remarks>
        public static string Encrypt(string password)
        {
            // Convert the password string into a character array for manipulation
            char[] buffer = password.ToCharArray();

            // Loop through each character in the password
            for (int i = 0; i < buffer.Length; i++)
            {
                // Get the current character
                char c = buffer[i];

                // Shift the ASCII value of the character by the constant shift value
                buffer[i] = (char)((int)c + shift);
            }

            // Return the encrypted password as a new string
            return new string(buffer);
        }

        /// <summary>
        /// Decrypts the given encrypted password by reversing the encryption process.
        /// </summary>
        /// <param name="encryptedPassword">The encrypted password to be decrypted.</param>
        /// <returns>The decrypted (original) password as a string.</returns>
        /// <remarks>
        /// This function was created with the assistance of ChatGPT, a language model developed by OpenAI.
        /// </remarks>
        public static string Decrypt(string encryptedPassword)
        {
            // Convert the encrypted password string into a character array for manipulation
            char[] buffer = encryptedPassword.ToCharArray();

            // Loop through each character in the encrypted password
            for (int i = 0; i < buffer.Length; i++)
            {
                // Get the current character
                char c = buffer[i];

                // Reverse the shift of the ASCII value by subtracting the constant shift value
                buffer[i] = (char)((int)c - shift);
            }

            // Return the decrypted password as a new string
            return new string(buffer);
        }
        /// <summary>
        /// Checks if the password is at least 6 characters, contains an uppercase letter and a digit.
        /// </summary>
        /// <param name="password">Password to check</param>
        /// <returns>true if strong, false otherwise</returns>
        /// <remarks>Written with the help of ChatGPT (https://openai.com)</remarks>
        static bool IsStrongPassword(string password)
        {
            // Must be at least 6 characters
            if (password.Length < 6)
            { 
                return false;
                throw new BO.Exceptions.BlPasswordNotValid($" The password :{password!} needs to contain at least  8 digits, Uppercase letter and number");
            }

            // Must contain at least one uppercase letter and one digit
            return password.Any(char.IsUpper) && password.Any(char.IsDigit);
        }






    }

}