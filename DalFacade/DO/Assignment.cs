

namespace DO;
/// <summary>
/// 
/// </summary>
/// <param name="Id"></param>
/// <param name="CallId"></param>
/// <param name="VolunteerId"></param>
/// <param name="AppointmentTime"></param>
/// <param name="FinishAppointmentTime"></param>
/// <param name="FinishAppointmentType"></param>
public record Assignment
(
int Id,
int CallId,
int VolunteerId,
DateTime AppointmentTime,
DateTime? FinishAppointmentTime = null,
Enum? FinishAppointmentType = null

)
{
    public Assignment() : this(0,0,0,DateTime.MinValue) { }

}
