

using System.Globalization;
using System.Windows.Data;
using static BO.Enums;
using System.Windows.Media;

namespace PL;

public class ConvertIdToTF : IValueConverter

{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value.ToString() == "Update")
            return true;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

  

public class CallTypeCollection : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is CallTypeEnum callType)
        {
            // Map each enum value to a specific color.
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
                case CallTypeEnum.none:
                    return Brushes.Gray;
                default:
                    return Brushes.Transparent;
            }
        }

        return Brushes.Transparent; // Fallback color.
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ConvertBack is not implemented.");
    }

}



}

 
	
    

