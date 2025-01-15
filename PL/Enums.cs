using System.Collections;

namespace PL;

internal class CallTypeCollection : IEnumerable
{
    static readonly IEnumerable<BO.Enums.CallTypeEnum> s_enums = (Enum.GetValues(typeof(BO.Enums.CallTypeEnum)) as IEnumerable<BO.Enums.CallTypeEnum>)!;
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

internal class PositionCollection : IEnumerable
{
    static readonly IEnumerable<BO.Enums.VolunteerTypeEnum> s_enums = (Enum.GetValues(typeof(BO.Enums.VolunteerTypeEnum)) as IEnumerable<BO.Enums.VolunteerTypeEnum>)!;
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}

internal class DistanceTypeCollection : IEnumerable
{
    static readonly IEnumerable<BO.Enums.DistanceTypeEnum> s_enums = (Enum.GetValues(typeof(BO.Enums.DistanceTypeEnum)) as IEnumerable<BO.Enums.DistanceTypeEnum>)!;
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
}