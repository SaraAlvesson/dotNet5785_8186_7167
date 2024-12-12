
using static BO.Enums;

namespace BO;
public class Volunteer
{
    public int Id { get; init; }
    public  string FullName { get; set; }
    public  string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string? Password { get; set; }=null;
    public string? Address { get; set; }=null;
    public double? Latitude { get; set; }=null;
    public double? Longitude { get; set; }=null ;
    public VolunteerTypeEnum position { get; set; }
    public bool Active { get; set; }
    public double? MaxDistance { get; set; } = null;
    public Enum DistanceType { get; set; }
    //public  readonly int SumCalls { get; set; }    
    public int SumCanceled { get; set; } = 0;
    public int SumExpired { get; set; } = 0;
    public CallInProgress VolunteerTakenCare { get; set; }   

    public override string ToString() => this.ToStringProperty();
}

