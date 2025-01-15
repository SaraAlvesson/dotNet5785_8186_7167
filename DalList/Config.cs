namespace Dal;

/// <summary>
///Static class for configuration settings
/// </summary>
public static class Config
{
    internal const int StartCallId = 1000; // Starting value for call IDs
    private static int s_nextCallId = StartCallId; // Tracks the next available call ID
    internal static int NextCallId { get => s_nextCallId++; } // Property to get the next call ID and increment

    internal const int StartAssignmentId = 10000; // Starting value for assignment IDs
    private static int s_nextAssignmentId = StartAssignmentId; // Tracks the next available assignment ID
    internal static int NextAssignmentId { get => s_nextAssignmentId++; } // Property to get the next assignment ID and increment

    internal static DateTime Clock { get; set; } = DateTime.Now; // Property for the current date and time
    internal static TimeSpan RiskRange { get; set; } = TimeSpan.Zero; // Property for the risk range duration

    /// <summary>
    ///Method to reset configuration settings
    /// </summary>
    internal static void Reset()
    {
        s_nextAssignmentId = StartAssignmentId; // Reset the assignment ID counter
        s_nextCallId = StartCallId; // Reset the call ID counter
        Clock = DateTime.Now; // Reset the clock to the current time
        RiskRange = TimeSpan.Zero; // Reset the risk range to zero
    }
}