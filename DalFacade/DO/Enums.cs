
namespace DO;

/// <summary>
/// Defines the types of calls or tasks that can be assigned.
/// </summary>
public enum CallType
{
    PreparingFood, // Cooking or assembling meals for those in need.
    TransportingFood, // Delivering prepared food to designated locations.
    FixingEquipment, // Repairing essential tools or equipment.
    ProvidingShelter, // Arranging or offering temporary accommodation.
    TransportAssistance, // Helping with vehicle issues or emergency rides.
    MedicalAssistance, // Delivering medical supplies or offering first aid.
    EmotionalSupport, // Providing mental health support through conversations.
    PackingSupplies // Organizing and packing necessary supplies for distribution.
}

/// <summary>
/// Represents the position of a person in the organization.
/// </summary>
public enum Position
{
    Volunteer, // A person offering their time and services without payment.
    Manager // A person responsible for overseeing tasks and volunteers.
}

/// <summary>
/// Defines the possible outcomes for completing or terminating an appointment.
/// </summary>
public enum FinishAppointmentType
{
    WasTreated, // The task or appointment was successfully completed.
    SelfCancellation, // The appointment was canceled by the user.
    CancelingAnAdministrator, // An administrator canceled the appointment.
    CancellationHasExpired // The appointment was automatically canceled due to expiration.
}

/// <summary>
/// Specifies the type of distance measurement used.
/// </summary>
public enum DistanceType
{
    AerialDistance, // Straight-line distance (as the crow flies).
    WalkingDistance, // Distance calculated for pedestrian routes.
    DrivingDistance // Distance calculated for vehicular routes.
}
