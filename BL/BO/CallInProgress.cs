using Helpers;

namespace BO
{
    public class CallInProgress
    {
        // מספר מזהה של ישות ההקצאה
        public int Id { get; init; }

        // מספר מזהה רץ של ישות הקריאה
        public int CallId { get; init; }

        // סוג הקריאה (Enum)
        public Enum CallType { get; set; }

        // תיאור מילולי (יכול להיות null)
        public string? VerbDesc { get; set; } = null;

        // כתובת מלאה של הקריאה
        public string CallAddress { get; set; }

        // זמן פתיחה של הקריאה
        public DateTime OpenTime { get; set; }

        // זמן מקסימלי לסיום הקריאה (יכול להיות null)
        public DateTime? MaxFinishTime { get; set; }  // שים לב לשינוי ל- DateTime?

        // זמן כניסה לטיפול (לא יכול להיות null)
        public DateTime StartAppointmentTime { get; set; }

        // מרחק קריאה מהמתנדב המטפל
        public double DistanceOfCall { get; set; }

        // סטטוס הקריאה
        public Enum Status { get; init; }

        // המרה לסטראינג (השתמש במתודה שמבצע את ההמרה שלך)
        public override string ToString() => this.ToStringProperty();
    }
}
