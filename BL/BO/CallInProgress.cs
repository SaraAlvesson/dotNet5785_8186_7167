

namespace BO;

public class CallInProgress
{
    public int Id { get; init; } 
    public int CallId { get; init; }  
    public Enum CallType { get; set; }
    public string? VerbDesc { get; set; } = null;  
    public string CallAddress {  get; set; }    
    public DateTime OpenTime { get; set; }
    public DateTime MaxFinishTime { get; set; } 
    public DateTime StartAppointmentTime { get; set; }  
    public double DistanceOfCall { get; set; }
    public Enum Status {  get; init; }

    public override string ToString() => this.ToStringProperty();
}
