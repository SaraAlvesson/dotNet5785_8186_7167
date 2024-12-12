
namespace BlImplementation;
using BlApi;
using static BO.Enums;

internal class AdminImplementation : IAdmin
{
    private DateTime systemClock;
    private TimeSpan riskTimeRange;

    public AdminImplementation()
    {
        systemClock = DateTime.Now;  // Initial system time
        riskTimeRange = TimeSpan.FromHours(24);  // Default risk time range
    }

    /// <summary>
    /// Gets the current system time.
    /// </summary>
    /// <returns>Current system time as a DateTime value.</returns>
    public DateTime GetCurrentTime()
    {
        return systemClock;
    }

    /// <summary>
    /// Advances the system clock by the specified time unit (minute, hour, day, month, or year).
    /// </summary>
    /// <param name="timeUnit">The time unit to advance the clock (minute, hour, day, month, or year).</param>
    public void AdvanceClock(TimeUnitEnum timeUnit)
    {
        switch (timeUnit)
        {
            case TimeUnitEnum.MINUTE:
                systemClock = systemClock.AddMinutes(1);
                break;
            case TimeUnitEnum.HOUR:
                systemClock = systemClock.AddHours(1);
                break;
            case TimeUnitEnum.DAY:
                systemClock = systemClock.AddDays(1);
                break;
            case TimeUnitEnum.MONTH:
                systemClock = systemClock.AddMonths(1);
                break;
            case TimeUnitEnum.YEAR:
                systemClock = systemClock.AddYears(1);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(timeUnit), "Invalid time unit.");
        }

        ClockManager.UpdateClock(systemClock);
    }

    /// <summary>
    /// Gets the current risk time range configuration.
    /// </summary>
    /// <returns>The current risk time range as a TimeSpan.</returns>
    public TimeSpan GetRiskTimeRange()
    {
        return riskTimeRange;
    }

    /// <summary>
    /// Sets the risk time range configuration.
    /// </summary>
    /// <param name="riskTimeRange">The TimeSpan value to set as the new risk time range.</param>
    public void SetRiskTimeRange(TimeSpan riskTimeRange)
    {
        this.riskTimeRange = riskTimeRange;
    }

    /// <summary>
    /// Resets the database configuration to its initial state, clearing all data.
    /// </summary>
    public void ResetDatabase()
    {
        // Reset configuration and clear data
        riskTimeRange = TimeSpan.FromHours(24);  // Default risk time range
        systemClock = DateTime.Now;  // Reset system time

        // Clear any data in the database (entities, configurations)
        Database.ClearAllData();
    }

    /// <summary>
    /// Initializes the database with default data values.
    /// </summary>
    public void InitializeDatabase()
    {
        ResetDatabase();  // Reset before initializing

        // Add default data entries for entities and configurations
        Database.AddDefaultEntities();
    }
}