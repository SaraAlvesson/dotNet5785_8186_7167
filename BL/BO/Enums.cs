



using DO;
using Helpers;

namespace BO;


public enum Position
{
    Manager,
    Volunteer
}


public enum DistanceType
{
    WalkingDistance,
    DrivingDistance,
    AerialDistance
}

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

public enum FinishAppointmentType
{
    TakenCareOf,
    SelfCancellation,
    ManagerCancellation,
    OutDatedCallCancellation
}

public enum TreatmentStatus 
{ 
    InTreatment,
    AtRisk
}

public enum CallStatus 
{
    open,
    closed,
    InTreatment,
    AtRisk,
    InTreatmenAtRisk, 
    OutDatedCall 
}

public enum TimeUnit
{
    year,
    month,
    day,
    hour,
    minute,
    second
}


public enum ClosedCallSortBy
{
    CallId,
    CallType,
    FullAddress,
    OpenTime,
    TreatmentEndTime,
    TreatmentStartTime,
    Finish
}



public enum VolunteerSortBy
{
    VolunteerId,
    FullName,
    HandledCallsCount,
    CanceledCallsCount,
    ExpiredCallsCount,
    CurrentCallId,
    CurrentCallType
}

public enum CallSortBy
{
    VolunteerId,
    FullName,
    HandledCallsCount,
    CanceledCallsCount,
    ExpiredCallsCount,
    CurrentCallId,
    CurrentCallType
}

public enum UpdateVolunteerDBAction 
{ 
    delete
    , update }



