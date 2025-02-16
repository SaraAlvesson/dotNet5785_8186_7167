namespace DalApi
{
    /// <summary>
    /// Defines configuration settings for the system.
    /// </summary>
    public interface IConfig
    {
        DateTime Clock { get; set; } // Current system clock (date and time)
        TimeSpan RiskRange { get; set; } // Allowable risk time range
        void Reset(); // Resets the configuration to default values

        int NextCallId { get; } // Add this property
        int NextAssignmentId { get; } // Add this property
    }
}
