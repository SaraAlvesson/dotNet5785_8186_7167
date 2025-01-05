using System.Collections;

namespace PL;

public class Enums  // וודא שהמחלקה Enums היא public אם אתה צריך לגשת אליה משם
{
    public class CallTypeCollection : IEnumerable, IEnumerable<BO.Enums.CallTypeEnum>  // מימוש הגנרי ולא גנרי
    {
        // המרת הערכים של ה-Enum ל-IEnumerable גנרי
        static readonly BO.Enums.CallTypeEnum[] s_enums =
            (BO.Enums.CallTypeEnum[])Enum.GetValues(typeof(BO.Enums.CallTypeEnum));

        // מימוש המתודה GetEnumerator של IEnumerable הגנרי
        public IEnumerator<BO.Enums.CallTypeEnum> GetEnumerator()
        {
            return ((IEnumerable<BO.Enums.CallTypeEnum>)s_enums).GetEnumerator();
        }

        // מימוש המתודה GetEnumerator של IEnumerable הכללי
        IEnumerator IEnumerable.GetEnumerator()
        {
            return s_enums.GetEnumerator();  // שימוש ב-GetEnumerator של המערך באופן ישיר
        }
    }
}
