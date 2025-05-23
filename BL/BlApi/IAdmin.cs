

namespace BlApi;
using BO;
/// <summary>
/// 
/// </summary>
public interface IAdmin
{



    #region Stage 5
    void AddConfigObserver(Action configObserver);
    void RemoveConfigObserver(Action configObserver);
    void AddClockObserver(Action clockObserver);
    void RemoveClockObserver(Action clockObserver);
    #endregion Stage 5

    /// <summary>
    /// Gets the current system time.
    /// </summary>
    /// <returns>Current system time as a DateTime value.</returns>
    DateTime GetCurrentTime();

    /// <summary>
    /// Advances the system clock by the specified time unit (minute, hour, day, month, or year).
    /// </summary>
    /// <param name="timeUnit">The time unit to advance the clock (minute, hour, day, month, or year).</param>
    void UpdateClock(Enums.TimeUnitEnum timeUnit);

    /// <summary>
    /// Gets the current risk time range configuration.
    /// </summary>
    /// <returns>The current risk time range as a TimeSpan.</returns>y
    TimeSpan GetRiskTimeRange();

    /// <summary>
    /// Sets the risk time range configuration.
    /// </summary>
    /// <param name="riskTimeRange">The TimeSpan value to set as the new risk time range.</param>
    void SetRiskTimeRange(TimeSpan riskTimeRange);

    /// <summary>
    /// Resets the database configuration to its initial state, clearing all data.
    /// </summary>
    void ResetDatabase();

    /// <summary>
    /// Initializes the database with default data values.
    /// </summary>
    void InitializeDatabase();
    void StartSimulator(int interval); //stage 7
    void StopSimulator(); //stage 7

}
