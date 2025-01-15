using DalApi;
namespace Dal;

/// <summary>
///Class implementing the IConfig interface
/// </summary>
internal class ConfigImplementation : IConfig
{
    /// <summary>
    ///Property to get or set the Clock value
    /// </summary>
    public DateTime Clock
    {
        get => Config.Clock; // Retrieves the clock from the Config class
        set => Config.Clock = value; // Sets the clock value in the Config class
    }

    /// <summary>
    /// Property to get or set the RiskRange value
    /// </summary>
    public TimeSpan RiskRange
    {
        get => Config.RiskRange; // Retrieves the risk range from the Config class
        set => Config.RiskRange = value; // Sets the risk range value in the Config class
    }
    public int NextCallId => Config.NextCallId; // Implements the property
    public int NextAssignmentId => Config.NextAssignmentId; // Implements the property
    /// <summary>
    ///Method to reset the configuration settings
    /// </summary>
    public void Reset()
    {
        Config.Reset(); // Calls the Reset method of the Config class
    }
}
