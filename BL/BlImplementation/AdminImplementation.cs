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
        AdminManager.UpdateClock(newTime);
    }

    // Method to get the current system time
    public DateTime GetCurrentTime()
    {
        return AdminManager.Now; // Return the current time from ClockManager
    }

    // Method to get the configured risk time range
    public TimeSpan GetRiskTimeRange()
    {
        return AdminManager.MaxRange; // Return the current risk time range from the DAL configuration
    }

    // Method to initialize the database by resetting it and adding initial data
    public void InitializeDatabase()
    {
        ResetDatabase(); // First, reset the database
        DalTest.Initialization.Do(); // Add the initial data
        AdminManager.UpdateClock(AdminManager.Now); // Ensure the clock is updated after initialization
        DalTest.Initialization.Do();
        AdminManager.UpdateClock(AdminManager.Now);
        AdminManager.MaxRange = AdminManager.MaxRange;

    }

    // Method to reset the database (clear all data and reset configurations)
    public void ResetDatabase()
    {
        _dal.ResetDB(); // Reset the database through the DAL
        DalTest.Initialization.Do();
        AdminManager.UpdateClock(AdminManager.Now);
        AdminManager.MaxRange = AdminManager.MaxRange;

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



    // Method to set a new risk time range configuration
    public void SetMaxRange(int maxRange) => AdminManager.MaxRange = maxRange;



}
