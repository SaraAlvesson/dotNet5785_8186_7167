

using DalApi;

namespace Dal
{
    // Implementation of IConfig interface for handling configuration settings
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

        // Method to reset the configuration to its default settings
        public void Reset()
        {
            Config.Reset(); // Call the Reset method of Config to revert to default settings
        }
    }
}
