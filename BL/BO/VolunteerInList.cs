
namespace BO;

public class VolunteerInList
{

    public int Id { get; init; } 
    public string FullName { get; set; }   
    public bool Active { get; set; }   
    public int SumTreatedCalls { get; set; }   
    public int SumCanceledCalls {  get; set; } 
    public int SumExspiredCalls {  get; set; } 
    public int CallIdInTreatment {  get; set; }    
    public Enum CallType { get; set; }

    public override string ToString() => this.ToStringProperty();

}
