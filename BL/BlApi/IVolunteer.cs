namespace BlApi;
using BO;
using static BO.Enums;

/// <summary>
/// 
/// </summary>
// Interface for the Volunteer service layer
public interface IVolunteer: IObservable
{
    /// <summary>
    /// Logs in a user by validating username and password.
    /// Returns the role of the user if credentials are valid.
    /// </summary>
    string Login(int username, string password);

    /// <summary>
    /// Requests a list of volunteers, optionally filtering by active status and sorting by a specific field.
    /// </summary>
    IEnumerable<VolunteerInList> RequestVolunteerList(
       bool? isActive,
       VolunteerInListField? sortField = null,
       CallTypeEnum? callTypeFilter = null);// הוספת פרמטר לסינון לפי סוג קריאה

    /// <summary>
    /// Requests the details of a volunteer by their unique ID.
    /// </summary>
    Volunteer RequestVolunteerDetails(int volunteerId);

    /// <summary>
    /// מעדכן את פרטי המתנדב (למשל: שינוי מיקום או פרטי קשר).
    /// </summary>
    /// <param name="requesterId">מזהה המשתמש המבצע את העדכון</param>
    /// <param name="volunteerDetails">אובייקט מתנדב עם הנתונים המעודכנים</param>
   void UpdateVolunteerDetails(int requesterId, BO.Volunteer volunteerDetails);

    /// <summary>
    /// Deletes a volunteer from the system by their ID.
    /// </summary>
    void DeleteVolunteer(int volunteerId);

    /// <summary>
    /// Adds a new volunteer to the system.
    /// </summary>
    void AddVolunteer(BO.Volunteer volunteerDetails);
    
}
