
using DalApi;

namespace Dal;

internal static class Config
{
    // Set runner id to calling entity
    internal const int StartCallId = 1000;
    private static int NextCallId = StartCallId;
    internal static int FuncNextCallId { get => NextCallId++; }

    // Setting a runner ID to a volunteer entity
    internal const int StartVolunteerId = 1000;

    // Setting a runner ID to an assignment entity
    internal const int StartAssignmentId = 1000;
    private static int NextAssignmentId = StartAssignmentId;
    internal static int FuncNextAssignmentId { get => NextAssignmentId++; }

    // Additional variables according to the system configuration variables
    internal static DateTime Clock { get; set; } = DateTime.Now;

    // "risk time" for calls approaching end time
    internal static TimeSpan RiskRange { get; set; } = TimeSpan.FromHours(1);

    // Function to reset the values ​​to the initial ones
    internal static void Reset()
    {
        NextCallId = StartCallId;
        NextAssignmentId = StartAssignmentId;

        // Additional configuration variables to reset
        Clock = DateTime.Now;
        RiskRange = TimeSpan.FromHours(1);
    }
}
