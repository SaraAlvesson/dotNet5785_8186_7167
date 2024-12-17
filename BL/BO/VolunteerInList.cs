using Helpers;
using static BO.Enums;

namespace BO;

public class VolunteerInList
{
    public int Id { get; init; } // ת.ז. מתנדב
    public string FullName { get; set; } // שם מלא
    public bool Active { get; set; } // פעיל
    public int SumTreatedCalls { get; set; } // סך קריאות שטופלו
    public int SumCanceledCalls { get; set; } // סך קריאות שבוטלו
    public int SumExpiredCalls { get; set; } // סך קריאות שפג תוקפן
    public int? CallIdInTreatment { get; set; } // מספר מזהה של הקריאה שבטיפולו (יכול להיות null)
    public CallTypeEnum CallType { get; set; } // סוג הקריאה שבטיפולו

    public override string ToString() => this.ToStringProperty();
}
