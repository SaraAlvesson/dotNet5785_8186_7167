
namespace BO;

public class CallInList
{

    public int Id {  get; set; }    
    public int CallId {  get; set; }    
    public Enum CallType { get; set; }
    public DateTime OpenTime{ get; set; }
    public TimeSpan  sumTimeUnillFinish{ get; set; }  
    public string LastVolunteerName {  get; set; }  
    public TimeSpan SumAppointmentTime { get; set; }
    public Enum Status { get; set; }    
    public int SumAssignment { get; set; }
    public override string ToString() => this.ToStringProperty();

}
