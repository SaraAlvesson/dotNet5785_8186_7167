using System;
using System.Globalization;
using System.Windows.Data;

namespace PL.Converters
{
    public class TimeSpanToCustomFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                // Extract hours, minutes and seconds without including days
                int hours = timeSpan.Hours;
                int minutes = timeSpan.Minutes;
                int seconds = timeSpan.Seconds;
                
                return $"{timeSpan.Days} Days and {hours:D2}:{minutes:D2}:{seconds:D2}";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
