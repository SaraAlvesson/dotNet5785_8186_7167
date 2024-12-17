using Helpers;
using static BO.Enums;

namespace BO;

public class OpenCallInList
{
    public int Id { get; init; } // מספר מזהה רץ של ישות הקריאה
    public CallTypeEnum CallType { get; set; } // סוג הקריאה
    public string? VerbDesc { get; set; } // תיאור מילולי (יכול להיות null)
    public string Address { get; set; } // כתובת מלאה של הקריאה
    public DateTime OpenTime { get; set; } // זמן פתיחה
    public DateTime? MaxFinishTime { get; set; } // זמן מקסימלי לסיום הקריאה
    public double DistanceOfCall { get; set; } // מרחק הקריאה מהמתנדב

    public override string ToString() => this.ToStringProperty();
}
