using DalApi;

namespace Dal
{
    /// <summary>
    /// Implementation of the IConfig interface for handling configuration settings in the system.
    /// This class provides methods to interact with system configuration, 
    /// such as getting and setting the system clock, adjusting the risk range, 
    /// and resetting the configuration to its default state.
    /// </summary>
    internal class ConfigImplementation : IConfig
    {
        // Property to get or set the current system clock
        public DateTime Clock
        {
            get => Config.Clock; // Get the current value of the Clock from Config
            set => Config.Clock = value; // Set the new value for Clock in Config
        }

        // Property to get or set the risk range for the system
        public TimeSpan RiskRange
        {
            get => Config.RiskRange; // Get the current value of the RiskRange from Config
            set => Config.RiskRange = value; // Set the new value for RiskRange in Config
        }

        /// <summary>
        /// Resets the configuration to its default settings, reverting values 
        /// like Clock and RiskRange to their initial state as defined in the Config class.
        /// </summary>
        public void Reset()
        {
            Config.Reset(); // Call the Reset method of Config to revert to default settings
        }
    }
}
