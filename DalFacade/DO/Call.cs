
namespace DO;
/// <summary>
/// 
/// </summary>
/// <param name="Id"></param>
/// <param name="Adress"></param>
/// <param name="Latitude"></param>
/// <param name="Longitude"></param>
/// <param name="OpenTime"></param>
/// <param name="CallType"></param>
/// <param name="VerbDesc"></param>
/// <param name="MaxTime"></param>
public record Call
(
    int Id,
    string Adress,
    double Latitude,
    double Longitude,
    DateTime OpenTime,
    CallType CallType,
    string? VerbDesc = null,
    /// לא יכול להיות פה null
    DateTime? MaxTime = null
)
{
    /// <summary>
    /// Default constructor for the Call record.
    /// </summary>
    public Call()
        : this(0, string.Empty, 0.0, 0.0, DateTime.MinValue, CallType.PreparingFood) { }
}
