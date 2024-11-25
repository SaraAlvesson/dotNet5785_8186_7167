
using DalApi;

namespace Dal
{
    /// <summary>
    /// DalList class implements the IDal interface and provides access to 
    /// various data access components, including operations for Volunteers, Calls, Assignments, 
    /// and configuration management. It serves as a central hub for accessing the individual 
    /// implementations of these data operations.
    /// </summary>
    sealed public class DalList : IDal
    {
        // Provides access to the Volunteer data operations (CRUD).
        public IVolunteer Volunteer { get; } = new VolunteerImplementation();

        // Provides access to the Call data operations (CRUD).
        public ICall call { get; } = new CallImplementation();

        // Provides access to the Assignment data operations (CRUD).
        public IAssignment assignment { get; } = new AssignmentImplementation();

        // Provides access to configuration settings and operations.
        public IConfig config { get; } = new ConfigImplementation();

        /// <summary>
        /// Resets all the data stores and configuration to their initial state. 
        /// This method clears all the data in Volunteers, Calls, and Assignments, 
        /// and resets the system configuration.
        /// </summary>
        public void ResetDB()
        {
            Volunteer.DeleteAll(); // Deletes all volunteer records
            call.DeleteAll(); // Deletes all call records
            assignment.DeleteAll(); // Deletes all assignment records
            config.Reset(); // Resets the system configuration to its default state
        }
    }
}




