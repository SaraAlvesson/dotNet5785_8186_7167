using DalApi;

namespace Dal;
/// <summary>
/// The Config class is responsible for managing the configuration settings of the system, 
/// including the generation of unique IDs for calls, volunteers, and assignments, as well as 
/// other system-wide settings like the system clock and risk range for calls.
/// </summary>
internal static class Config
{
    // Set runner id to calling entity
    internal const int StartCallId = 1000; // Starting ID for calls
    private static int NextCallId = StartCallId; // Stores the next call ID
    internal static int FuncNextCallId { get => NextCallId++; } // Returns the next available call ID and increments it

    // Setting a runner ID to a volunteer entity
    internal const int StartVolunteerId = 1000; // Starting ID for volunteers

    // Setting a runner ID to an assignment entity
    internal const int StartAssignmentId = 1000; // Starting ID for assignments
    private static int NextAssignmentId = StartAssignmentId; // Stores the next assignment ID
    internal static int FuncNextAssignmentId { get => NextAssignmentId++; } // Returns the next available assignment ID and increments it

    // Additional variables according to the system configuration variables
    internal static DateTime Clock { get; set; } = DateTime.Now; // System clock, initialized to current date and time

    // "Risk time" for calls approaching end time
    internal static TimeSpan RiskRange { get; set; } = TimeSpan.FromHours(1); // Time window (1 hour) before call end is considered at risk

    /// <summary>
    /// Resets the configuration values to their initial states.
    /// This includes resetting the call and assignment IDs and other configuration settings.
    /// </summary>
    internal static void Reset()
    {
        NextCallId = StartCallId; // Resets the next call ID to the starting value
        NextAssignmentId = StartAssignmentId; // Resets the next assignment ID to the starting value

        // Additional configuration variables are reset to their initial values
        Clock = DateTime.Now; // Resets the system clock to the current time
        RiskRange = TimeSpan.FromHours(1); // Resets the risk range to 1 hour
    }
}
