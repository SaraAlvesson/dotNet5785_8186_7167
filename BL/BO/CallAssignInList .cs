
namespace BO;

public class CallAssignInList
{
    public int  VolunteerId {  get; init ;}  
    public string VolunteerName {  get; set;}   

    public DateTime OpenTime  { get; set;}

    public DateTime RealFinishTime { get; set;}

    public Enum FinishTreatmentType { get; set; }

    public override string ToString() => this.ToStringProperty();

}
