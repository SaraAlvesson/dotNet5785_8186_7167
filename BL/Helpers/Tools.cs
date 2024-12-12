
namespace Helpers;

internal static class Tools
{
    public static string ToStringProperty<T>(this T t);
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    public static bool IsValidId(int id)
    {
        string idStr = id.ToString("D9"); // ת.ז. צריכה להיות באורך 9 ספרות
        int sum = 0;

        for (int i = 0; i < idStr.Length; i++)
        {
            int digit = (idStr[i] - '0') * ((i % 2) + 1);
            sum += (digit > 9) ? digit - 9 : digit;
        }

        return sum % 10 == 0;
    }
    public static bool IsValidAddress(string address)
    {
        return !string.IsNullOrWhiteSpace(address) && address.Length > 5;
    }



}

