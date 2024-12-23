namespace BlApi;
using BO;
/// <summary>
/// 
/// </summary>
public interface ICall
{
    /// <summary>
    /// מבצע חישוב של כמות הקריאות לכל מצב (Status), ומחזיר מערך עם הכמויות עבור כל מצב.
    /// </summary>
    /// <returns>מערך של אינטים המייצג את הכמויות של קריאות לכל מצב</returns>
    int[] GetCallQuantitiesByStatus();
    /// <summary>
    /// מחזיר רשימה של קריאות בהתבסס על פילטרים אפשריים ומיון.
    /// </summary>
    /// <param name="filterField">שדה לפילטר (למשל: מצב הקריאה, סוג הקריאה)</param>
    /// <param name="filterValue">ערך הפילטר (למשל: 'Open' עבור קריאות פתוחות)</param>
    /// <param name="sortField">שדה למיון הקריאות (למשל: מספר הקריאה או תאריך יצירה)</param>
    /// <returns>רשימה של קריאות לאחר סינון ומיון, אם נדרש</returns>
    IEnumerable<CallInList> GetCallList
    (
        Enums.CallFieldEnum? filterField = null,
        object filterValue = null,
        Enums.CallFieldEnum? sortField = null
    );
    /// <summary>
    /// מחזיר את פרטי הקריאה עבור מזהה קריאה נתון.
    /// </summary>
    /// <param name="callId">מזהה הקריאה</param>
    /// <returns>פרטי הקריאה כולל פרטי המשימות הקשורות לקריאה</returns>
   DO. Call GetCallDetails(int callId);
    /// <summary>
    /// מעדכן את פרטי הקריאה (למשל: שינוי מצב או מספר קריאה).
    /// </summary>
    /// <param name="call">אובייקט קריאה עם הנתונים המעודכנים</param>
    void UpdateCall(BO.Call call);
    /// <summary>
    /// מוחק קריאה לפי מזהה קריאה. ניתן למחוק קריאות שנמצאות במצב 'Open' ואין להן משימות משויכות.
    /// </summary>
    /// <param name="callId">מזהה הקריאה למחיקה</param>
    void DeleteCall(int callId);
    /// <summary>
    /// מוסיף קריאה חדשה למערכת. אם קיימת קריאה עם אותו מזהה, לא תתבצע ההוספה.
    /// </summary>
    /// <param name="call">הקריאה להוספה</param>
    void AddCall(BO.Call call);
    /// <summary>
    /// מחזיר רשימה של קריאות סגורות שטופלו על ידי מתנדב, עם אפשרות לסינון ומיון.
    /// </summary>
    /// <param name="volunteerId">מזהה המתנדב</param>
    /// <param name="callType">סוג הקריאה (למשל: חירום או כללי)</param>
    /// <param name="sortField">שדה למיון הקריאות</param>
    /// <returns>רשימה של קריאות סגורות שטופלו על ידי המתנדב</returns>
    IEnumerable<ClosedCallInList> GetClosedCallsByVolunteer(
        string volunteerId,
        Enums.CallTypeEnum? callType = null,
        Enums.CallFieldEnum? sortField = null
    );
    /// <summary>
    /// מחזיר רשימה של קריאות פתוחות שניתן לבחור לעבודה על ידי מתנדב, עם אפשרות לסינון ומיון.
    /// </summary>
    /// <param name="volunteerId">מזהה המתנדב</param>
    /// <param name="callType">סוג הקריאה (למשל: חירום או כללי)</param>
    /// <param name="sortField">שדה למיון הקריאות</param>
    /// <returns>רשימה של קריאות פתוחות שניתן לבחור לעבודה על ידי המתנדב</returns>
    IEnumerable<OpenCallInList> GetOpenCallsForVolunteer(
        string volunteerId,
        Enums.CallTypeEnum? callType = null,
        Enums.CallFieldEnum? sortField = null
    );
    /// <summary>
    /// מעדכן את מצב הקריאה לסיום טיפול עבור משימה נתונה.
    /// </summary>
    /// <param name="volunteerId">מזהה המתנדב</param>
    /// <param name="assignmentId">מזהה המשימה</param>
    void MarkCallAsCompleted(string volunteerId, int assignmentId);
    /// <summary>
    /// מעדכן את מצב הקריאה לביטול טיפול עבור משימה נתונה.
    /// </summary>
    /// <param name="volunteerId">מזהה המתנדב</param>
    /// <param name="assignmentId">מזהה המשימה</param>
    void CancelCallTreatment(string volunteerId, int assignmentId);
    /// <summary>
    /// מקצה קריאה למתנדב מסוים לעבודה על קריאה נתונה.
    /// </summary>
    /// <param name="volunteerId">מזהה המתנדב</param>
    /// <param name="callId">מזהה הקריאה</param>
    void AssignCallToVolunteer(string volunteerId, int callId);
    bool IsFieldEqual(object entity, Enums.CallFieldEnum field, object value);
    object GetFieldValue(object entity, Enums.CallFieldEnum field);
}