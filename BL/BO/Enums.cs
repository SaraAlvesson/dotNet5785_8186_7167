

namespace BO;

public class Enums
{
    #region TimeUnitEnum
    /// <summary>
    /// Enumeration for time units used in the Admin service.
    /// </summary>
    public enum TimeUnitEnum
    {
        MINUTE,   // דקה
        HOUR,     // שעה
        DAY,      // יום
        MONTH,    // חודש
        YEAR      // שנה
    }
    #endregion

    #region CallStatusEnum
    /// <summary>
    /// Enumeration for different call statuses.
    /// </summary>
    public enum CallStatusEnum
    {
        OPEN,        // פתוחה
        IN_PROGRESS, // בתהליך
        COMPLETED,   // הושלמה
        CLOSED       // סגורה
    }
    #endregion

    #region CallTypeEnum
    /// <summary>
    /// Enumeration for different call types.
    /// </summary>
    public enum CallTypeEnum
    {
        EMERGENCY,    // חירום
        NON_EMERGENCY  // לא חירום
    }
    #endregion

    #region CallFieldEnum
    /// <summary>
    /// Enumeration for fields that can be used to filter or sort call lists.
    /// </summary>
    public enum CallFieldEnum
    {
        ID,         // מזהה קריאה
        STATUS,     // סטטוס
        TYPE,       // סוג
        DATE,       // תאריך
        VOLUNTEER   // מתנדב
    }
    #endregion

    #region VolunteerTypeEnum
    /// <summary>
    /// Enumeration for volunteer types (e.g., admin, regular).
    /// </summary>
    public enum VolunteerTypeEnum
    {
        ADMIN,   // מנהל
        VOLUNTEER  // רגיל
    }
    #endregion

    #region RiskLevelEnum
    /// <summary>
    /// Enumeration for different risk levels associated with calls or actions.
    /// </summary>
    public enum RiskLevelEnum
    {
        LOW,    // סיכון נמוך
        MEDIUM, // סיכון בינוני
        HIGH    // סיכון גבוה
    }
    #endregion

    #region VolunteerStatusEnum
    /// <summary>
    /// Enumeration for volunteer status types.
    /// </summary>
    public enum VolunteerStatusEnum
    {
        ACTIVE,   // פעיל
        INACTIVE, // לא פעיל
        PENDING   // ממתין
    }
    #endregion

    #region AssignmentStatusEnum
    /// <summary>
    /// Enumeration for the assignment status types.
    /// </summary>
    public enum AssignmentStatusEnum
    {
        ASSIGNED,   // הוקצתה
        COMPLETED,  // הושלמה
        CANCELLED   // בוטלה
    }
    #endregion
}


