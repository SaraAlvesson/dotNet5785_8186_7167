using DO;
using Helpers;
using static BO.Enums;

namespace BO;

public class ClosedCallInList
{
    public int Id { get; init; } // מספר מזהה רץ של ישות הקריאה
    public string Address { get; set; } // כתובת מלאה של הקריאה
    public CallTypeEnum CallType { get; set; } // סוג הקריאה
    public DateTime OpenTime { get; set; } // זמן פתיחה (לא יכול להיות null)
    public DateTime? TreatmentStartTime { get; set; } // זמן כניסה לטיפול
    public DateTime? RealFinishTime { get; set; } // זמן סיום הטיפול בפועל
    public FinishAppointmentTypeEnum? FinishAppointmentType { get; set; } // סוג סיום הטיפול

    public override string ToString() => this.ToStringProperty();
}
