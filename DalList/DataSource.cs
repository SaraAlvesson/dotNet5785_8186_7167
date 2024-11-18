

namespace Dal
{
    // Static class to hold the data source for various collections
    internal static class DataSource
    {
        // A list of Volunteers, initially empty, to store volunteer data
        internal static List<DO.Volunteer> Volunteers { get; } = new();

        // A list of Calls, initially empty, to store call data
        internal static List<DO.Call> Calls { get; } = new();

        // A list of Assignments, initially empty, to store assignment data
        internal static List<DO.Assignment> Assignments { get; } = new();
    }
}
