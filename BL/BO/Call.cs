
namespace BO;

public class Call
{

    public int Id { get; init; } 
    public Enum CallType { get; set; }    
    public string VerbDesc { get; set; } 
    public string Address { get; set; }    
    public double Latitude { get; set; }     
    public double Longitude { get; set; }
    public DateTime OpenTime{ get; set; }

    public DateTime MaxFinishTime{ get; set; }
    public Enum Status{ get; set; }
    public List<BO.CallAssignInList> callAssignInLists { get; set; }    

    public override string ToString() => this.ToStringProperty();



}
