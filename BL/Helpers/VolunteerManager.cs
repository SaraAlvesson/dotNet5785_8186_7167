using BO;
using DalApi;
using Helpers;
using System.Text.RegularExpressions;
using static BO.Exceptions;

internal static class VolunteerManager
{
    private static IDal _dal = Factory.Get;


    internal static void checkeVolunteerFormat(BO.Volunteer volunteer)
    {
        if (string.IsNullOrEmpty(volunteer.Email) || volunteer.Email.Count(c => c == '@') != 1)
        {
            throw new Exception($"email :{volunteer.Email} have problem with format ");
        }

        string pattern = @"^[a-zA-Z0-9.]+@[a-zA-Z0-9]+\.[a-zA-Z0-9]+\.com$";

        if (!Regex.IsMatch(volunteer.Email, pattern))
        {
            throw new Exception($"email :{volunteer.Email} have problem with format ");
        }
        if (string.IsNullOrEmpty(volunteer.PhoneNumber) || volunteer.PhoneNumber.Length < 8 || volunteer.PhoneNumber.Length > 9 || !(volunteer.PhoneNumber.All(char.IsDigit)))
            throw new Exception($"phone :{volunteer.PhoneNumber} have problem with format ");

        if (volunteer.MaxDistance < 0)
            throw new Exception("Max Distance can't be negavite ");
    }
    internal static void checkeVolunteerlogic(BO.Volunteer volunteer)
    {
        if (!(IsValidId(volunteer.Id)))
            throw new Exception("the id isnt vaild ");
        if (volunteer != null && !IsStrongPassword(volunteer.Password!))
            throw new Exception($" this pasword :{volunteer.Password!} doent have at least 6 characters, contains an uppercase letter and a digit");
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
    private static string Encrypt(string password)
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
    private static string Decrypt(string encryptedPassword)
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
        if (password.Length < 6) return false;

        // Must contain at least one uppercase letter and one digit
        return password.Any(char.IsUpper) && password.Any(char.IsDigit);
    }
   
    internal static DO.Volunteer convertFormBOVolunteerToDo(BO.Volunteer BoVolunteer)
    {

        if (BoVolunteer.Address != null)
        {


            double[] cordintes = Tools.GetGeolocationCoordinates(BoVolunteer.Address);
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
           Location: BoVolunteer.Address,
          Latitude: BoVolunteer.Latitude,
          Longitude: BoVolunteer.Longitude,
         MaxDistance: BoVolunteer.MaxDistance

                    );
        return doVl;
    }
   

}