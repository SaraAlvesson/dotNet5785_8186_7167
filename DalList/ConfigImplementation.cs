using System.Runtime.CompilerServices;
using DalApi;
namespace Dal
{
    /// <summary>
    /// Class implementing the IConfig interface
    /// </summary>
    internal class ConfigImplementation : IConfig
    {

      

        public DateTime Clock
        { 
            get => Config.Clock;
            set => Config.Clock = value;
        }

        public TimeSpan RiskRange
        {
            get => Config.RiskRange;
            set => Config.RiskRange = value;
        }

        public int NextCallId => Config.NextCallId; // Implements the property

        public int NextAssignmentId => Config.NextAssignmentId; // Implements the property


        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        public void Reset()
        {
            Config.Reset();
        }
    }
}
