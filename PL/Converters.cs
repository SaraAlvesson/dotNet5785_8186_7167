// Converter for IsReadOnly
using System.Globalization;
using System.Windows.Data;
using System.Windows;
namespace PL;
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

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
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
//public class VolunteerRoleToEnabledConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        // Check if the value is a "Volunteer" role
//        if (value is BO.Enums.VolunteerTypeEnum role && role == BO.Enums..volunteer)
//        {
//            // If the role is "Volunteer", disable the field (return false)
//            return false;
//        }

//        // Otherwise, enable the field (return true)
//        return true;
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        throw new NotImplementedException();
//    }
//}

public class CallStatusToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BO.Enums.CalltStatusEnum status)
        {
            // מחזיר Visibility.Visible רק אם הסטטוס הוא OPEN או CallAlmostOver
            if (status == BO.Enums.CalltStatusEnum.OPEN || status == BO.Enums.CalltStatusEnum.CallAlmostOver)
            {
                return Visibility.Visible;
            }
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is Visibility && (Visibility)value == Visibility.Visible);
    }
}


