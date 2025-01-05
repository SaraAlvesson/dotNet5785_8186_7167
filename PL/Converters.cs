using System.Globalization;
using System.Windows.Data;
using static BO.Enums;
using System.Windows.Media;
using System.Windows;
using System.Collections.ObjectModel;

namespace PL
{
    // קונברטר להמיר Id ל-True או False
    public class IdToReadOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // אם ה-Id הוא ברירת מחדל (למשל, Guid.Empty או null), הפקד יהיה "לא מאופשר"
            if (value == null || value.Equals(Guid.Empty))
            {
                return false;  // מצבים של הוספה: ניתן להקליד
            }
            else
            {
                return true;  // מצבים של עדכון: לא ניתן לשנות
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // לא נדרש במקרה הזה
            return null;
        }
    }
    public class PositionCollectionConverter : IValueConverter
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
    public class CallTypeConvertor : IValueConverter
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
                    case CallTypeEnum.none: // אם `none` הוא ערך תקני
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
    public class DistanceTypeCollectionConverter : IValueConverter
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
    public class ConvertUpdateToTrueKey : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // אם ה-value הוא null, החזר false, אחרת החזר true
            return value == null ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // קונברטר לבדיקת ערך "Update"
    public class ConvertUpdateToTrue : IValueConverter
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

    // הוספת הקולקציות החדשות

    public class PositionCollection: ObservableCollection<string>
    {
        public PositionCollection()
        {
            this.Add("Manager");
            this.Add("Assistant");
            this.Add("Volunteer");
        }
    }

    public class DistanceTypeCollection: ObservableCollection<string>
    {
        public DistanceTypeCollection()
        {
            this.Add("AerialDistance");
            this.Add("WalkingDistance");
            this.Add("DrivingDistance");
        }
    }
    public class CallTypeCollection : ObservableCollection<string>
    {
        public CallTypeCollection()
        {
            this.Add("PreparingFood");
            this.Add("TransportingFood");
            this.Add("FixingEquipment");
            this.Add("ProvidingShelter");
            this.Add("TransportAssistance");
            this.Add("MedicalAssistance");
            this.Add("EmotionalSupport");
            this.Add("PackingSupplies");
        }
    }


    public class IdToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // אם ה-Id הוא ברירת מחדל, הפקד יהיה Visible במצב הוספה
            return value == null || value.Equals(Guid.Empty) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // לא נדרש במקרה הזה
            return null;
        }
    }

}
