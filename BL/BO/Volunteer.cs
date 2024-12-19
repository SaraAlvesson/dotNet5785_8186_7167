using DO;
using Helpers;
using static BO.Enums;

namespace BO;
public class Volunteer
{
    public int Id { get; init; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string? Password { get; set; } = null;
    public string? Address { get; set; } = null;
    public double? Latitude { get; set; } = null;
    public double? Longitude { get; set; } = null;
    public VolunteerTypeEnum Position { get; set; } // Updated casing
    public bool Active { get; set; }
    public double? MaxDistance { get; set; } = null;
    public DistanceType DistanceType { get; set; } // Updated to specific enum
    public int SumCalls { get; set; } = 0; // Added for tracking total calls
    public int SumCanceled { get; set; } = 0;
    public int SumExpired { get; set; } = 0;
    public CallInProgress? VolunteerTakenCare { get; set; } = null; // Added nullability for clarity

    public override string ToString() => this.ToStringProperty();
}
