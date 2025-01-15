// Converter for IsReadOnly
using System.Globalization;
using System.Windows.Data;
using System.Windows;

public class ConvertUpdateToReadOnly : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string? buttonText = value as string;
        return buttonText == "Update"; //can modify only if it is for update
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

//if there is no current call, the currentCall details will not be displayed
public class CallInProgressToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // if CurrentCallInProgress is null, return Collapsed (hide), and if not null Visible
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

//If the role is "Volunteer", they can't modify their role
public class RoleToEditableConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BO.Enums.VolunteerTypeEnum role)
        {
            if (role == BO.Enums.VolunteerTypeEnum.volunteer)
            {
                return false; //If the role is 'Volunteer', the area is not editable.
            }
        }

        //If the value is null or of another type, make the area editable (true).
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

//If the role is "Volunteer", they can't modify the Id
public class VolunteerRoleToEnabledConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Check if the value is a "Volunteer" role
        if (value is BO.Enums.VolunteerTypeEnum role && role == BO.Enums.VolunteerTypeEnum.volunteer)
        {
            // If the role is "Volunteer", disable the field (return false)
            return false;
        }

        // Otherwise, enable the field (return true)
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
