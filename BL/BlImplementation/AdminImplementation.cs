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



    // Method to initialize the database by resetting it and adding initial data
    // Method to initialize the database
    public void InitializeDatabase()
    {
        try
        {
            // Ensure DAL is initialized
            if (_dal == null)
            {
                throw new InvalidOperationException("The DAL object is not initialized.");
            }

            // Add the initial data
            DalTest.Initialization.Do();
            //AdminManager.UpdateClock(AdminManager.Now); // Ensure the clock is updated after initialization
            //AdminManager.MaxRange = AdminManager.MaxRange; // Reset MaxRange if needed
        }
        catch (Exception ex)
        {
            // Log detailed exception
          
            throw new InvalidOperationException("Error during initialization", ex);
        }
    }


    // Method to reset the database (clear all data and reset configurations)
    public void ResetDatabase()
    {
        // Ensure DAL is initialized
        if (_dal == null)
        {
            throw new InvalidOperationException("The DAL object is not initialized.");
        }

        // Reset the database through the DAL
      

        // Reinitialize data and configurations
        try
        {
            _dal.ResetDB();

        }
        catch (Exception ex)
        {
            // Handle reset failure, log or show message
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


    public void SetRiskTimeRange(TimeSpan maxRange)
    {
        AdminManager.MaxRange = (int)maxRange.TotalMinutes; // או TotalSeconds, TotalHours, תלוי מה אתה רוצה
    }

    // GetRiskTimeRange - מחזיר את הערך כ- TimeSpan (למשל, דקות)
    public TimeSpan GetRiskTimeRange()
    {
        return TimeSpan.FromMinutes(AdminManager.MaxRange); // ממיר בחזרה ל- TimeSpan (לדוג' דקות)
    }
}
