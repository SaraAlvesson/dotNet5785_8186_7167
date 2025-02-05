using System.Collections;
using System.Collections.ObjectModel;

namespace PL;

internal class CallTypeCollection : IEnumerable
{
    static readonly IEnumerable<BO.Enums.CallTypeEnum> s_enums = (Enum.GetValues(typeof(BO.Enums.CallTypeEnum)) as IEnumerable<BO.Enums.CallTypeEnum>)!;
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

internal class PositionCollection : ObservableCollection<BO.Enums.VolunteerTypeEnum>
{
    public PositionCollection()
    {
        // הוספת ערכים באופן ידני או אוטומטי
        foreach (var value in Enum.GetValues(typeof(BO.Enums.VolunteerTypeEnum)).Cast<BO.Enums.VolunteerTypeEnum>())
        {
            this.Add(value);
        }
    }
}

internal class DistanceTypeCollection : IEnumerable
{
    static readonly IEnumerable<BO.Enums.DistanceTypeEnum> s_enums = (Enum.GetValues(typeof(BO.Enums.DistanceTypeEnum)) as IEnumerable<BO.Enums.DistanceTypeEnum>)!;
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

internal class StatusTypeCollection : IEnumerable
{
    static readonly IEnumerable<BO.Enums.CalltStatusEnum> s_enums = (Enum.GetValues(typeof(BO.Enums.CalltStatusEnum)) as IEnumerable<BO.Enums.CalltStatusEnum>)!;
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}
