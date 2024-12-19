using DO;
using Helpers;


namespace BO;

public class ClosedCallInList
{
    public int Id { get; init; } // מספר מזהה רץ של ישות הקריאה
    public string Address { get; set; } // כתובת מלאה של הקריאה
    public CallType CallType { get; set; } // סוג הקריאה
    public DateTime OpenTime { get; set; } // זמן פתיחה (לא יכול להיות null)
    public DateTime AppointmentTime { get; set; } // זמן כניסה לטיפול
    public DateTime? FinishAppointmentTime { get; set; } // זמן סיום הטיפול בפועל
    public FinishAppointmentType? FinishAppointmentType { get; set; } // סוג סיום הטיפול

    public override string ToString() => this.ToStringProperty();
}
