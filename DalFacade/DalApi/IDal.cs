

namespace DalApi
{
    /// <summary>
    /// Main interface for accessing and managing the Data Access Layer (DAL).
    /// </summary>
    public interface IDal
    {
        IVolunteer Volunteer { get; } // Provides access to volunteer-related operations
        ICall call { get; } // Provides access to call-related operations
        IAssignment assignment { get; } // Provides access to assignment-related operations
        IConfig config { get; } // Provides access to configuration settings
        void ResetDB(); // Resets the entire database to its default state
    }
}
