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
                    double latitude = doVolunteer.Latitude ?? callTreat.Latitude;
                    double longitude = doVolunteer.Longitude ?? callTreat.Longitude;
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
                        DistanceOfCall = Tools.CalculateDistance(callTreat.Latitude, callTreat.Longitude, latitude, longitude),
                        //Status = (callTreat.MaxTime - ClockManager.Now <= s_dal.? BO.Enums.CalltStatusEnum.CallTreatmentAlmostOver : BO.Enums.CalltStatusEnum.CallIsBeingTreated),
                    };
                }
            }
            return null;
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




        internal static bool IsValidId(long id)
        {
            /// <summary>
            /// Validates an Israeli 9-digit ID using the Luhn algorithm.
            /// </summary>
            /// <param name="id">The ID number to validate.</param>
            /// <returns>True if the ID is valid, false otherwise.</returns>
            /// <remarks>
            /// This code was written with the assistance of ChatGPT, a language model developed by OpenAI.
            /// </remarks>

            // Check if ID is exactly 9 digits.
            if (id < 100000000 || id > 999999999)
            {
                
                return false;
                throw new BO.Exceptions.BlIdNotValid("ID not valid ");
            }

            // Luhn algorithm to calculate checksum for first 8 digits.
            int sum = 0;
            for (int i = 0; i < 8; i++)
            {
                int digit = (int)(id % 10);
                id /= 10;

                if (i % 2 == 0)
                {
                    sum += digit;  // Odd index: add digit.
                }
                else
                {
                    int doubled = digit * 2;
                    sum += doubled > 9 ? doubled - 9 : doubled;  // Even index: subtract 9 if doubled > 9.
                }
            }

            // Calculate and compare checksum.
            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit == (int)(id % 10);  // Valid if checksum matches.
        }

        internal static DO.Volunteer convertFormBOVolunteerToDo(BO.Volunteer BoVolunteer)
        {

            if (BoVolunteer.Location != null)
            {


                double[] cordintes = Tools.GetGeolocationCoordinates(BoVolunteer.Location);
                BoVolunteer.Latitude = cordintes[0];
                BoVolunteer.Longitude = cordintes[1];

            }
            else
            {
                BoVolunteer.Latitude = null;
                BoVolunteer.Longitude = null;
            }
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