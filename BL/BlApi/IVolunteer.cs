
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
    string Login(string username, string password);

    /// <summary>
    /// Requests a list of volunteers, optionally filtering by active status and sorting by a specific field.
    /// </summary>
    IEnumerable<VolunteerInList> RequestVolunteerList(bool? isActive, VolunteerInListField? sortField=null);

    /// <summary>
    /// Requests the details of a volunteer by their unique ID.
    /// </summary>
    Volunteer RequestVolunteerDetails(int volunteerId);

    /// <summary>
    /// Updates the details of an existing volunteer based on their ID.
    /// </summary>
    void UpdateVolunteerDetails(int volunteerId, Volunteer volunteerDetails);

    /// <summary>
    /// Deletes a volunteer from the system by their ID.
    /// </summary>
    void DeleteVolunteer(int volunteerId);

    /// <summary>
    /// Adds a new volunteer to the system.
    /// </summary>
    void AddVolunteer(BO.Volunteer volunteerDetails);
    IEnumerable<VolunteerInList> RequestVolunteerList();
}

