using DO;
using Helpers;


namespace BO
{
    public class Call
    {
        // מספר מזהה רץ של ישות הקריאה - חייב להיות מספר שלם (לא יכול להיות null)
        public int Id { get; init; }

        // סוג הקריאה - חייב להיות מוגדר כ-enum, לא יכול להיות null
        public CallType CallType { get; set; }

        // תיאור מילולי - יכול להיות null
        public string? VerbDesc { get; set; }

        // כתובת מלאה של הקריאה - חייבת להיות כתובת תקינה, יכולה להיות null
        public string Address { get; set; }

        // קו רוחב - מספק מידע על המקום, יכול להיות null במקרה שאין כתובת
        public double Latitude { get; set; }

        // קו אורך - מספק מידע על המקום, יכול להיות null במקרה שאין כתובת
        public double Longitude { get; set; }

        // זמן פתיחה - חייב להיות מועד פתיחה לתהליך הקריאה
        public DateTime OpenTime { get; set; }

        // זמן מקסימלי לסיום הקריאה - יכול להיות null במקרה של קריאה פתוחה או שבוטלה
        public DateTime? MaxTime { get; set; }

        // סטטוס הקריאה - מחושב על פי סוג סיום הטיפול, זמן מקסימלי לסיום והזמן הנוכחי
        public CallStatus CallStatus { get; set; }

        // רשימת ההקצאות עבור הקריאה - אם אין הקצאות, יהיה null
        public List<BO.CallAssignInList>? CallAssignInLists { get; set; }

        // הצגת פרטי הקריאה כמחרוזת
        public override string ToString() => this.ToStringProperty();
    }
}
