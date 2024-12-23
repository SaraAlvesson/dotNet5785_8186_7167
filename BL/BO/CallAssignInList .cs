using Helpers;
using static BO.Enums;

namespace BO
{
    public class CallAssignInList
    {
        // ת.ז מתנדב (יכול להיות null במקרה של הקצאה שלא הושלמה)
        public int? VolunteerId { get; init; }

        // שם מתנדב (יכול להיות null במקרה של הקצאה שלא הושלמה)
        public string? VolunteerName { get; set; }

        // זמן כניסה לטיפול (חובה)
        public DateTime OpenTime { get; set; }

        // זמן סיום הטיפול בפועל (יכול להיות null אם לא הסתיים)
        public DateTime? RealFinishTime { get; set; }

        // סוג סיום טיפול (יכול להיות null אם לא הושלם טיפול)
        public FinishAppointmentTypeEnum FinishAppointmentType { get; set; }

        // הצגת פרטי ההקצאה כמחרוזת
        public override string ToString() => this.ToStringProperty();
    }
}
