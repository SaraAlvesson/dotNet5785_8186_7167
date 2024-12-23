using Helpers;
using static BO.Enums;

namespace BO
{
    public class CallInList
    {
        // מספר מזהה של ישות ההקצאה - יכול להיות null אם לא נעשתה הקצאה
        public int? Id { get; set; }  // תיקון: שדה זה nullable

        // מספר מזהה רץ של ישות הקריאה - חובה ולקוח מ-DO.Call
        public int CallId { get; set; }

        // סוג הקריאה - חייב להיות מסוג Enum, לקוח מ-DO.Call
        public CallTypeEnum CallType { get; set; }

        // זמן פתיחה של הקריאה - לקוח מ-DO.Call
        public DateTime OpenTime { get; set; }

        // סך הזמן שנותר לסיום הקריאה - TimeSpan מחושב על פי הזמן הנותר
        public TimeSpan? SumTimeUntilFinish { get; set; }  // יכול להיות null אם אין זמן מקסימלי לסיום

        // שם המתנדב האחרון שהוקצה לקריאה - יכול להיות null אם לא הוקצה מתנדב
        public string? LastVolunteerName { get; set; }

        // סך זמן השלמת הטיפול - TimeSpan של הזמן שחלף מהזמן הפתיחה ועד סיום הקריאה
        public TimeSpan? SumAppointmentTime { get; set; }  // יכול להיות null אם הקריאה לא הושלמה

        // סטטוס הקריאה - מחושב על פי סטטוס הסיום, זמן מקסימלי, והזמן הנוכחי
        public CalltStatusEnum Status { get; set; }

        // סך הקצאות עבור הקריאה - מספר ההקצאות שנעשו לקריאה (כולל בוטלו)
        public int SumAssignment { get; set; }

        // הצגת פרטי הקריאה כמחרוזת
        public override string ToString() => this.ToStringProperty();
    }
}
