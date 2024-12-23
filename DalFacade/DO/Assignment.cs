

namespace DO;
/// <summary>
/// Represents an assignment for a volunteer to a call, including appointment times and status.
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
FinishAppointmentType? FinishAppointmentType = null

)
{/// <summary>
///  Default constructor for Assignment.
/// </summary>
    public Assignment() : this(0,0,0,DateTime.MinValue) { }//ctor

}
