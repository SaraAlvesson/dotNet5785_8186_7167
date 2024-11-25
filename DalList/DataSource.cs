namespace Dal
{
    /// <summary>
    /// Static class to hold the data source for various collections, such as Volunteers, Calls, and Assignments.
    /// This class is used as the centralized location for storing in-memory data during the runtime of the application.
    /// </summary>
    internal static class DataSource
    {
        // A list of Volunteers, initially empty, to store volunteer data.
        // This list will be populated and accessed throughout the application for volunteer-related operations.
        internal static List<DO.Volunteer> Volunteers { get; } = new();

        // A list of Calls, initially empty, to store call data.
        // This list will be populated and accessed for handling call-related operations.
        internal static List<DO.Call> Calls { get; } = new();

        // A list of Assignments, initially empty, to store assignment data.
        // This list will be used to store and retrieve assignment records for the system.
        internal static List<DO.Assignment> Assignments { get; } = new();
    }
}
