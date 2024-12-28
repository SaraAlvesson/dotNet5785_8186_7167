

namespace BO;

public class Enums
{
    #region TimeUnitEnum
    /// <summary>
    /// Enumeration for time units used in the Admin service.
    /// </summary>
    public enum TimeUnitEnum
    {   SECOND,
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
    public enum CalltStatusEnum
    {
        OPEN,        // פתוחה
        EXPIRED, // פג תוקף
        CLOSED,       // סגורה
        CallIsBeingTreated,    // בטיפול
        CallAlmostOver, // פתוחה בסיכון
        CallTreatmentAlmostOver,// בטיפול בסיכון
         UNKNOWN  // מצב ברירת מחדל

    }
    #endregion
    /// <summary>
    /// Defines the possible outcomes for completing or terminating an appointment.
    /// </summary>
    public enum FinishAppointmentTypeEnum
    {
        WasTreated, // The task or appointment was successfully completed.
        SelfCancellation, // The appointment was canceled by the user.
        CancelingAnAdministrator, // An administrator canceled the appointment.
        CancellationHasExpired // The appointment was automatically canceled due to expiration.
    }
    public enum CallTypeEnum
    {
        PreparingFood, // Cooking or assembling meals for those in need.
        TransportingFood, // Delivering prepared food to designated locations.
        FixingEquipment, // Repairing essential tools or equipment.
        ProvidingShelter, // Arranging or offering temporary accommodation.
        TransportAssistance, // Helping with vehicle issues or emergency rides.
        MedicalAssistance, // Delivering medical supplies or offering first aid.
        EmotionalSupport, // Providing mental health support through conversations.
        PackingSupplies,// Organizing and packing necessary supplies for distribution.
        none
    }



    #region EmergencyTypeEnum
    // <summary>
    // Enumeration for different call types.
    // </summary>
        public enum StatusCallInProgressEnum
    {
        CallIsBeingTreated,    // בטיפול
        CallAlmostOver  //סיכון
    }
    #endregion


    /// <summary>
    /// Enumeration for fields that can be used to filter or sort call lists.
    /// </summary>
     #region ClosedCallFieldEnum
    public enum ClosedCallFieldEnum
    {
        ID,         // מזהה קריאה
        Address,     // סטטוס
        CallType,       // סוג
        OpenTime,       // תאריך
        TreatmentStartTime,  // מתנדב
        RealFinishTime,
        FinishAppointmentType,
    }
    #endregion
    public enum CallFieldEnum
    {
        ID,         // מזהה קריאה
        CallId,     // סטטוס
        CallType,       // סוג
        OpenTime,       // תאריך
        SumTimeUntilFinish,  // מתנדב
        LastVolunteerName,
        SumAppointmentTime,
        Status,
        SumAssignment,
    }
    public enum OpenCallEnum
    {
        Id,         // מזהה קריאה
        CallType,     // סטטוס
        VerbDesc,       // סוג
        Address,       // תאריך
        OpenTime,  // מתנדב
        MaxFinishTime,
        DistanceOfCall,

    }
    public enum VolunteerInListField
    {
      Id ,
     FullName,
     Active ,
     SumTreatedCalls,
     SumCanceledCalls,
     SumExpiredCalls,
     CallIdInTreatment ,
     CallType 
}


    #region VolunteerTypeEnum
    /// <summary>
    /// Enumeration for volunteer types (e.g., admin, regular).
    /// </summary>
    public enum VolunteerTypeEnum
    {
        admin,   // מנהל
        volunteer  // רגיל
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
    public enum DistanceTypeEnum
    {
        AerialDistance, // Straight-line distance (as the crow flies).
        WalkingDistance, // Distance calculated for pedestrian routes.
        DrivingDistance // Distance calculated for vehicular routes.
    }

    public class Position
    {
    }
}


