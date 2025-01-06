using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace PL
{
    class ConvertDistanceToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BO.Enums.DistanceTypeEnum Dis = (BO.Enums.DistanceTypeEnum)value;
            switch (Dis)
            {
                case BO.Enums.DistanceTypeEnum.AerialDistance:
                    return Brushes.Yellow;
                case BO.Enums.DistanceTypeEnum.WalkingDistance:
                    return Brushes.Orange;
                case BO.Enums.DistanceTypeEnum.DrivingDistance:
                    return Brushes.Green;
                default:
                    return Brushes.White;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class ConvertRoleToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BO.Enums.VolunteerTypeEnum Job = (BO.Enums.VolunteerTypeEnum)value;
            switch (Job)
            {
                case BO.Enums.VolunteerTypeEnum.admin:
                    return Brushes.Blue;
                case BO.Enums.VolunteerTypeEnum.volunteer:
                    return Brushes.Orange;
                default:
                    return Brushes.White;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ConvertUpdateToTrue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // אם ה-Id לא אפס (לא מצב הוספה), אז לא ניתן לשנות
            return value != null && (int)value != 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }


}
