
namespace DO;
/// <summary>
/// Represents a volunteer with personal details, contact information, and location.
/// </summary>
/// <param name="Id"></param>
/// <param name="FullName"></param>
/// <param name="PhoneNumber"></param>
/// <param name="Email"></param>
/// <param name="Active"></param>
/// <param name="DistanceType"></param>
/// <param name="Position"></param>
/// <param name="Password"></param>
/// <param name="Location"></param>
/// <param name="Latitude"></param>
/// <param name="Longitude"></param>
/// <param name="MaxDistance"></param>

public record Volunteer
(
    int Id,
    string FullName,
    string PhoneNumber,
    string Email,
    bool Active,
    DistanceType DistanceType, // Cannot be null
    Position Position, // Cannot be null
    string? Password = null,
    string? Location = null,
    double? Latitude = null,
    double? Longitude = null,
    double? MaxDistance = null
)
{/// <summary>
 ///  Default constructor for the Volunteer.
 /// </summary>

    public Volunteer()
        : this(0, string.Empty, string.Empty, string.Empty, false, DistanceType.AerialDistance, Position.Manager) { }//ctor
}