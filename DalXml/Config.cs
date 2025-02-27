namespace Dal;

/// <summary>
/// A static class that manages configuration data for the application.
/// This includes tracking IDs, managing the clock, and handling configuration values
/// stored in XML files.
/// </summary>
public static class Config
{
    // File paths for storing configuration and data information.
    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_calls_xml = "calls.xml";
    internal const string s_assignments_xml = "assignments.xml";
    internal const string s_volunteers_xml = "volunteers.xml";

    /// <summary>
    /// Gets the next available Call ID from the configuration file and increments its value.
    /// The ID is unique for each call and is auto-incremented.
    /// </summary>
   public static int NextCallId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextCallId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextCallId", value);
    }

    /// <summary>
    /// Gets the next available Assignment ID from the configuration file and increments its value.
    /// The ID is unique for each assignment and is auto-incremented.
    /// </summary>
    public static int NextAssignmentId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextAssignmentId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextAssignmentId", value);
    }

    /// <summary>
    /// Gets or sets the current system clock used by the application.
    /// The value is stored and retrieved from the configuration file.
    /// </summary>
    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    /// <summary>
    /// Gets or sets the Risk Range used for defining the time span
    /// in which certain operations are considered at risk.
    /// The value is stored and retrieved from the configuration file.
    /// </summary>
    public static TimeSpan RiskRange
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "RiskRange"); // קורא את הערך מה-XML
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "RiskRange", value); // מעדכן את הערך ב-XML
    }


    /// <summary>
    /// Resets all configurable parameters to their default values.
    /// This includes resetting IDs, setting the clock to the current time,
    /// and clearing the risk range.
    /// </summary>
    internal static void Reset()
    {
        try
        {
            NextCallId = 1000; // קריאות מתחילות מ-1000
            NextAssignmentId = 1; // הקצאות מתחילות מ-1
            Clock = DateTime.Now;
            RiskRange = TimeSpan.Zero;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resetting configuration: {ex.Message}");
            throw;
        }
    }

}
