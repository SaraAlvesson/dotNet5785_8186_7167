namespace BlImplementation;
using BlApi;
using BO;
using Helpers;
using static BO.Enums;

internal class AdminImplementation : IAdmin
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;
    public void UpdateClock(TimeUnitEnum unit)
    {
        DateTime newTime = ClockManager.Now;

        switch (unit)
        {
            case TimeUnitEnum.MINUTE:
                newTime = ClockManager.Now.AddMinutes(1);
                break;
            case TimeUnitEnum.HOUR:
                newTime = ClockManager.Now.AddHours(1);
                break;
            case TimeUnitEnum.DAY:
                newTime = ClockManager.Now.AddDays(1);
                break;
            case TimeUnitEnum.MONTH:
                newTime = ClockManager.Now.AddMonths(1);
                break;
            case TimeUnitEnum.YEAR:
                newTime = ClockManager.Now.AddYears(1);
                break;
            default:
                throw new ArgumentException("Invalid TimeUnit value");
        }

        ClockManager.UpdateClock(newTime);
    }

    public DateTime GetCurrentTime()
    {
        return ClockManager.Now;
    }

    public TimeSpan GetRiskTimeRange()
    {
        return _dal.config.RiskRange;
    }
    public void InitializeDatabase()
    {
        ResetDatabase();
        DalTest.Initialization.Do();
        ClockManager.UpdateClock(ClockManager.Now);
    }

    public void ResetDatabase()
    {
        _dal.ResetDB();
    }

    public void SetRiskTimeRange(TimeSpan maxRange)
    {
        _dal.config.RiskRange = maxRange;
    }

   

}
