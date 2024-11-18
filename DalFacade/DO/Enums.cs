
namespace DO;
/// <summary>
/// 
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

public enum Position
{
    Volunteer,
    Manager
}
public enum FinishAppointmentType
{
    WasTreated,
    SelfCancellation,
    CancelingAnAdministrator,
    CancellationHasExpired

}
public enum DistanceType
{
    AerialDistance,
    walkingDistance, 
    drivingdistance
}