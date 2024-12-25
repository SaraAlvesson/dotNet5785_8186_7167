namespace BlImplementation;
using BlApi;
using BO;
using Helpers;
using static BO.Enums;

internal class AdminImplementation : IAdmin
{
    // DAL instance for data access
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    // Method to advance the system clock by a specified time unit
    public void UpdateClock(TimeUnitEnum unit)
    {
        DateTime newTime = ClockManager.Now; // Get the current time from ClockManager

        // Switch case to handle different time units
        switch (unit)
        {

            case TimeUnitEnum.SECOND:
                newTime = ClockManager.Now.AddSeconds(1);
                break;

            case TimeUnitEnum.MINUTE:
                newTime = ClockManager.Now.AddMinutes(1); // Advance by 1 minute
                break;
            case TimeUnitEnum.HOUR:
                newTime = ClockManager.Now.AddHours(1); // Advance by 1 hour
                break;
            case TimeUnitEnum.DAY:
                newTime = ClockManager.Now.AddDays(1); // Advance by 1 day
                break;
            case TimeUnitEnum.MONTH:
                newTime = ClockManager.Now.AddMonths(1); // Advance by 1 month
                break;
            case TimeUnitEnum.YEAR:
                newTime = ClockManager.Now.AddYears(1); // Advance by 1 year
                break;
                
            default:
                throw new ArgumentException("Invalid TimeUnit value"); // Handle invalid time unit
        }

        // Update the clock with the new time
        ClockManager.UpdateClock(newTime);
    }

    // Method to get the current system time
    public DateTime GetCurrentTime()
    {
        return ClockManager.Now; // Return the current time from ClockManager
    }

    // Method to get the configured risk time range
    public TimeSpan GetRiskTimeRange()
    {
        return _dal.config.RiskRange; // Return the current risk time range from the DAL configuration
    }

    // Method to initialize the database by resetting it and adding initial data
    public void InitializeDatabase()
    {
        ResetDatabase(); // First, reset the database
        DalTest.Initialization.Do(); // Add the initial data
        ClockManager.UpdateClock(ClockManager.Now); // Ensure the clock is updated after initialization
    }

    // Method to reset the database (clear all data and reset configurations)
    public void ResetDatabase()
    {
        _dal.ResetDB(); // Reset the database through the DAL
    }

    // Method to set a new risk time range configuration
    public void SetRiskTimeRange(TimeSpan maxRange)
    {
        _dal.config.RiskRange = maxRange; // Update the risk time range in the DAL configuration
    }
}
