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
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        DateTime newTime = AdminManager.Now; // Get the current time from ClockManager

        // Switch case to handle different time units
        switch (unit)
        {
            case TimeUnitEnum.SECOND:
                newTime = AdminManager.Now.AddSeconds(1);
                break;
            case TimeUnitEnum.MINUTE:
                newTime = AdminManager.Now.AddMinutes(1); // Advance by 1 minute
                break;
            case TimeUnitEnum.HOUR:
                newTime = AdminManager.Now.AddHours(1); // Advance by 1 hour
                break;
            case TimeUnitEnum.DAY:
                newTime = AdminManager.Now.AddDays(1); // Advance by 1 day
                break;
            case TimeUnitEnum.MONTH:
                newTime = AdminManager.Now.AddMonths(1); // Advance by 1 month
                break;
            case TimeUnitEnum.YEAR:
                newTime = AdminManager.Now.AddYears(1); // Advance by 1 year
                break;
            default:
                throw new ArgumentException("Invalid TimeUnit value"); // Handle invalid time unit
        }

        // Update the clock with the new time
        lock (AdminManager.BlMutex)  // עוטף את הפעולה בנעילה
        {
            AdminManager.UpdateClock(newTime); // Update the clock
        }
    }

    // Method to get the current system time
    public DateTime GetCurrentTime()
    {
        return AdminManager.Now; // Return the current time from ClockManager
    }

    // Method to initialize the database by resetting it and adding initial data
    public void InitializeDatabase()
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        try
        {
            // Ensure DAL is initialized
            if (_dal == null)
            {
                throw new InvalidOperationException("The DAL object is not initialized.");
            }

            // Add the initial data
            lock (AdminManager.BlMutex)  // עוטף את הפעולה בנעילה
            {
                DalTest.Initialization.Do();  // Initialize the database
                AdminManager.UpdateClock(DateTime.Now);  // Set system clock to current time
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error during initialization", ex);
        }
    }

    // Method to reset the database (clear all data and reset configurations)
    public void ResetDatabase()
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        // Ensure DAL is initialized
        if (_dal == null)
        {
            throw new InvalidOperationException("The DAL object is not initialized.");
        }

        // Reset the database through the DAL
        try
        {
            lock (AdminManager.BlMutex)  // עוטף את הפעולה בנעילה
            {
                _dal.ResetDB();  // Reset the database
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error during reset", ex);
        }
    }

    #region Stage 5
    public void AddClockObserver(Action clockObserver) =>
    AdminManager.ClockUpdatedObservers += clockObserver;
    public void RemoveClockObserver(Action clockObserver) =>
    AdminManager.ClockUpdatedObservers -= clockObserver;
    public void AddConfigObserver(Action configObserver) =>
   AdminManager.ConfigUpdatedObservers += configObserver;
    public void RemoveConfigObserver(Action configObserver) =>
    AdminManager.ConfigUpdatedObservers -= configObserver;
    #endregion Stage 5

    /// <summary>
    /// Sets a new risk range value in the DAL.
    /// </summary>
    /// <param name="riskRange">The new risk range value to set.</param>
    public void SetRiskTimeRange(TimeSpan riskRange)
    {
        AdminManager.ThrowOnSimulatorIsRunning();
        AdminManager.RiskRange = riskRange; // Set the new risk range in the DAL configuration.
    }

    /// <summary>
    /// Retrieves the risk range from the DAL.
    /// </summary>
    /// <returns>The time span representing the risk range.</returns>
    public TimeSpan GetRiskTimeRange()
    {
        return AdminManager.RiskRange; // Retrieve the risk range configuration from the DAL.
    }

    /// <summary>
    /// Starts the simulator with a specified interval.
    /// </summary>
    /// <param name="interval">The time interval for the simulator updates.</param>
    public void StartSimulator(int interval) // Stage 7
    {
        AdminManager.ThrowOnSimulatorIsRunning(); // Ensures the simulator is not already running
        AdminManager.Start(interval); // Starts the simulator with the given interval
    }

    /// <summary>
    /// Stops the simulator.
    /// </summary>
    public void StopSimulator()
        => AdminManager.Stop(); // Stops the simulator
}
