using System.Globalization;
using System.Windows.Data;
using static BO.Enums;
using System.Windows.Media;
using System.Windows;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace PL
{
    // קונברטר להמיר Id ל-True או False

    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result;
            return int.TryParse(value as string, out result) ? result : 0;
        }
    }

    public class PositionCollection : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string position)
            {
                switch (position)
                {
                    case "Admin":
                        return Brushes.Blue;

                    case "Volunteer":
                        return Brushes.Gray;
                    default:
                        return Brushes.Transparent;
                }
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not implemented.");
        }
    }

    // קונברטר להמיר CallTypeEnum לצבע
    public class CallTypeCollection : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CallTypeEnum callType)
            {
                switch (callType)
                {
                    case CallTypeEnum.PreparingFood:
                        return Brushes.Yellow;
                    case CallTypeEnum.TransportingFood:
                        return Brushes.Orange;
                    case CallTypeEnum.FixingEquipment:
                        return Brushes.Green;
                    case CallTypeEnum.ProvidingShelter:
                        return Brushes.Blue;
                    case CallTypeEnum.TransportAssistance:
                        return Brushes.Purple;
                    case CallTypeEnum.MedicalAssistance:
                        return Brushes.Red;
                    case CallTypeEnum.EmotionalSupport:
                        return Brushes.Pink;
                    case CallTypeEnum.PackingSupplies:
                        return Brushes.Brown;
                    case CallTypeEnum.None: // אם `none` הוא ערך תקני
                        return Brushes.Gray;
                    default:
                        return Brushes.Transparent; // ערך ברירת מחדל
                }
            }

            return Brushes.Transparent; // מקרה של ערך שאינו מ-`CallTypeEnum`
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not implemented.");
        }
    }
    public class DistanceTypeCollection : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string distanceType)
            {
                switch (distanceType)
                {
                    case "AerialDistance":
                        return Brushes.Cyan;
                    case "WalkingDistance":
                        return Brushes.GreenYellow;
                    case "DrivingDistance":
                        return Brushes.LightBlue;
                    default:
                        return Brushes.Transparent;
                }
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not implemented.");
        }
    }

    // קונברטר להמיר ערך ל-True אם הוא לא Null


    // קונברטר לבדיקת ערך "Update"
    public class ConvertObjIdToTF : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // בדיקה אם הטקסט הוא "Update"
            return value?.ToString() == "Update";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class UpdateToReadOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string buttonText = value as string;
            return buttonText == "Update";  // If it's an update, set IsReadOnly to true
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;  // Not needed for one-way binding
        }
    }


    public class UpdateToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string buttonText = value as string;
            return buttonText == "Update" ? Visibility.Visible : Visibility.Collapsed;  // Show only in update mode
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;  // Not needed for one-way binding
        }
    }
}

// הוספת הקולקציות החדשות

//public class PositionCollection: ObservableCollection<string>
//{
//    public PositionCollection()
//    {
//        this.Add("Manager");
//        this.Add("Assistant");
//        this.Add("Volunteer");
//    }
//}

//public class DistanceTypeCollection: ObservableCollection<string>
//{
//    public DistanceTypeCollection()
//    {
//        this.Add("AerialDistance");
//        this.Add("WalkingDistance");
//        this.Add("DrivingDistance");
//    }
//}



