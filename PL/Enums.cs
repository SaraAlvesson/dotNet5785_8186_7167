using System.Collections;
using System.Collections.Generic;

namespace PL;

public class CallTypesCollection : IEnumerable<BO.Enums.CallTypeEnum>
{
    // יצירת המרת Enum לרשימה
    private static readonly BO.Enums.CallTypeEnum[] s_enums =
        (BO.Enums.CallTypeEnum[])Enum.GetValues(typeof(BO.Enums.CallTypeEnum));

    public IEnumerator<BO.Enums.CallTypeEnum> GetEnumerator()
    {
        return ((IEnumerable<BO.Enums.CallTypeEnum>)s_enums).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return s_enums.GetEnumerator();
    }
}
