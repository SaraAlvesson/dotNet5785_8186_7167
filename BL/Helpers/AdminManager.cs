using System.Runtime.CompilerServices;

namespace Helpers;

/// <summary>
/// Internal BL manager for all application's clock logic policies.
/// </summary>
internal static class AdminManager // Stage 4
{
    #region Stage 4

    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get; // Stage 4

    #endregion Stage 4

    #region Stage 5

    /// <summary>
    /// Event for notifying observers when the configuration is updated.
    /// </summary>
    internal static event Action? ConfigUpdatedObservers; // Prepared for stage 5 - for config update observers

    /// <summary>
    /// Event for notifying observers when the clock is updated.
    /// </summary>
    internal static event Action? ClockUpdatedObservers; // Prepared for stage 5 - for clock update observers

    #endregion Stage 5

    #region Stage 4

    /// <summary>
    /// Property for providing or setting the current configuration variable value 
    /// for any BL class that may need it.
    /// </summary>
    internal static TimeSpan RiskRange
    {
        get => s_dal.config.RiskRange;
        set
        {
            lock (BlMutex) // Ensuring thread safety
                s_dal.config.RiskRange = value;

            ConfigUpdatedObservers?.Invoke(); // Stage 5 - Notify observers
        }
    }

    /// <summary>
    /// Property for providing the current application's clock value for any BL class that may need it.
    /// </summary>
    internal static DateTime Now => s_dal.config.Clock; // Stage 4

    /// <summary>
    /// Resets the database and updates relevant values.
    /// </summary>
    internal static void ResetDB() // Stage 4
    {
        lock (BlMutex) // Stage 7 - Ensuring thread safety
        {
            s_dal.ResetDB();
            AdminManager.UpdateClock(AdminManager.Now); // Stage 5 - Ensures the UI label updates
            AdminManager.RiskRange = AdminManager.RiskRange; // Stage 5 - Updates the UI
        }
    }

    /// <summary>
    /// Initializes the database with test data and updates relevant values.
    /// </summary>
    internal static void InitializeDB() // Stage 4
    {
        lock (BlMutex) // Stage 7 - Ensuring thread safety
        {
            DalTest.Initialization.Do();
            AdminManager.UpdateClock(AdminManager.Now); // Stage 5 - Ensures the UI label updates
            AdminManager.RiskRange = AdminManager.RiskRange; // Stage 5 - Updates the UI
        }
    }

    private static Task? _periodicTask = null;

    /// <summary>
    /// Updates the application's clock and notifies observers.
    /// </summary>
    /// <param name="newClock">The updated clock value.</param>
    internal static void UpdateClock(DateTime newClock) // Stage 4-7
    {
        Console.WriteLine($"UpdateClock called with time: {newClock}");

        lock (BlMutex) // Ensuring thread safety
            s_dal.config.Clock = newClock; // Stage 4 - Updates clock in configuration

        if (_periodicTask is null || _periodicTask.IsCompleted) // Stage 7
            _periodicTask = Task.Run(() => CallManager.UpdateExpired());

        // Notifies all observers about the clock update
        Console.WriteLine("ClockUpdatedObservers invoked");

        ClockUpdatedObservers?.Invoke(); // Prepared for stage 5
    }

    #endregion Stage 4

    #region Stage 7 base

    /// <summary>
    /// Mutex for ensuring mutual exclusion while the simulator is running.
    /// </summary>
    internal static readonly object BlMutex = new(); // Used for synchronization

    /// <summary>
    /// The thread that runs the simulator.
    /// </summary>
    private static volatile Thread? s_thread;

    /// <summary>
    /// The interval for clock updates in minutes per second (default is 1, set in Start()).
    /// </summary>
    private static int s_interval = 1;

    /// <summary>
    /// Flag indicating whether the simulator is running.
    /// </summary>
    private static volatile bool s_stop = false;

    /// <summary>
    /// Throws an exception if the simulator is currently running.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] // Stage 7
    public static void ThrowOnSimulatorIsRunning()
    {
        if (s_thread is not null)
            throw new BO.Exceptions.BLTemporaryNotAvailableException("Cannot perform the operation since the simulator is running.");
    }

    /// <summary>
    /// Starts the simulator with the given interval.
    /// </summary>
    /// <param name="interval">The interval for clock updates.</param>
    [MethodImpl(MethodImplOptions.Synchronized)] // Stage 7
    internal static void Start(int interval)
    {
        if (s_thread is null)
        {
            s_interval = interval;
            s_stop = false;
            s_thread = new(clockRunner) { Name = "ClockRunner" };
            s_thread.Start();
        }
    }

    /// <summary>
    /// Stops the simulator.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] // Stage 7
    internal static void Stop()
    {
        if (s_thread is not null)
        {
            s_stop = true;
            s_thread.Interrupt(); // Wakes up a sleeping thread
            s_thread.Name = "ClockRunner stopped";
            s_thread = null;
        }
    }

    private static Task? _simulateTask = null;

    /// <summary>
    /// The main function running in the simulator thread.
    /// Updates the clock and triggers simulations periodically.
    /// </summary>
    private static void clockRunner()
    {
        while (!s_stop)
        {
            UpdateClock(Now.AddMinutes(s_interval));

            if (_simulateTask is null || _simulateTask.IsCompleted) // Stage 7
                _simulateTask = Task.Run(() => VolunteersManager.SimulateAssignForVolunteer());

            try
            {
                Thread.Sleep(1000); // Waits for 1 second before the next update
            }
            catch (ThreadInterruptedException) { }
        }
    }

    #endregion Stage 7 base
}
