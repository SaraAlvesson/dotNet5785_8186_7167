
namespace BO;

public class OpenCallInList
{

    public int Id {  get; init; }    
    public Enum CallType { get; set; }

    public string VerbDesc { get; set; }    
    public string Address {  get; set; }    
    public DateTime OpenTime { get; set; } 
    public DateTime MaxFinishTime { get; set; }
    public string DistanceOfCall { get; set; }
    public override string ToString() => this.ToStringProperty();

}
