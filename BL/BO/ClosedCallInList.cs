

namespace BO;

public class ClosedCallInList
{
    public int Id {  get; init; }    
    public string Address { get; set; } 
    public Enum CallType { get; set; }
    public DateTime? OpenTime { get; set; }=null;
    public DateTime RealFinishTime { get; set;}
    public Enum FinishTreatmentType { get; set; }

    public override string ToString() => this.ToStringProperty();


}
