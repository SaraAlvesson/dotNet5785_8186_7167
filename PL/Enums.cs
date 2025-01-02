using System.Collections;  // עבור IEnumerable

namespace PL
{
    public class Enums  // וודא שהמחלקה Enums היא public אם אתה צריך לגשת אליה משם
    {
        public class VolunteerCollection : IEnumerable<BO.Enums.VolunteerInListField>
        {
            // המרת הערכים של ה-Enum ל-IEnumerable גנרי
            private static readonly BO.Enums.VolunteerInListField[] s_enums =
                (BO.Enums.VolunteerInListField[])Enum.GetValues(typeof(BO.Enums.VolunteerInListField));

            // מימוש המתודה GetEnumerator של IEnumerable גנרי
            public IEnumerator<BO.Enums.VolunteerInListField> GetEnumerator() => ((IEnumerable<BO.Enums.VolunteerInListField>)s_enums).GetEnumerator();

            // מימוש המתודה GetEnumerator של IEnumerable לא גנרי
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
