namespace BlImplementation;

using BL.Helpers;
using BlApi;
using BO;
using DO;
using Helpers;
using System;
using System.Web;
using static BO.Enums;
using static BO.Exceptions;

internal class VolunteerImplementation : IVolunteer

{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;


    #region Stage 5
    public void AddObserver(Action listObserver) =>
    VolunteerManager.Observers.AddListObserver(listObserver); //stage 5
    public void AddObserver(int id, Action observer) =>
    VolunteerManager.Observers.AddObserver(id, observer); //stage 5
    public void RemoveObserver(Action listObserver) =>
    VolunteerManager.Observers.RemoveListObserver(listObserver); //stage 5
    public void RemoveObserver(int id, Action observer) =>
   VolunteerManager.Observers.RemoveObserver(id, observer); //stage 5
    #endregion Stage 5



    public string Login(int username, string password)
    {
        try
        {
            // ????? ????? ?????? ????? ????? ??? ?????? ????? ???????
            lock (AdminManager.BlMutex)
            {
                // ??? 1: ????? ?????? ??? ????
                var volunteer = _dal.Volunteer.ReadAll(v => v.Id == username).FirstOrDefault();

                if (volunteer != null)
                {
                    // ??? 2: ????? ????? ?????? ??-XML
                    string decodedPassword = HttpUtility.HtmlDecode(volunteer.Password);

                    // ??? 3: ????? ????? ???? ?? ?????? (????? ???? ??????)
                    string inputPassword = HttpUtility.HtmlDecode(password);

                    // ?????? ???????? ???? ??????
                    if (decodedPassword != inputPassword)
                    {
                        throw new BlPasswordNotValid("Incorrect password");
                    }

                    // ??? 4: ???? ?????
                    if (Enum.TryParse<BO.Enums.VolunteerTypeEnum>(volunteer.Position.ToString(), out var volunteerType))
                    {
                        return volunteerType.ToString();
                    }
                    else
                    {
                        throw new Exception("Failed to map volunteer position to BO.Enums.VolunteerTypeEnum.");
                    }
                }
            }

            // ??? 5: ?? ?????? ?? ????
            throw new BlDoesNotExistException($"Username {username} not found");
        }
        catch (Exception ex)
        {
            // ????? ???????
            throw new Exception("An error occurred during the login process: " + ex.Message, ex);
        }
    }

    public IEnumerable<VolunteerInList> RequestVolunteerList(
     bool? isActive,
     VolunteerInListField? sortField = null,
     CallTypeEnum? callTypeFilter = null)  // ????? ????? ???? ?????
    {
        try
        {
            IEnumerable<DO.Volunteer> volunteers;

            // ????? ???? ????? ?-DAL
            lock (AdminManager.BlMutex)
            {
                volunteers = _dal.Volunteer.ReadAll();
            }

            // ????? ??? ??????
            if (isActive.HasValue)
            {
                volunteers = volunteers.Where(v => v.Active == isActive.Value).ToList();
            }

            // ???? ??????? ???? ??????
            var volunteerDetailsMap = volunteers
                .Select(v => RequestVolunteerDetails(v.Id))
                .ToDictionary(d => d.Id);

            var volunteerList = volunteerDetailsMap.Values.Select(details => new VolunteerInList
            {
                Id = details.Id,
                FullName = details.FullName,
                Active = details.Active,
                SumTreatedCalls = details.SumCalls,
                SumCanceledCalls = details.SumCanceled,
                SumExpiredCalls = details.SumExpired,
                CallIdInTreatment = details.VolunteerTakenCare?.CallId,
                CallType = details.VolunteerTakenCare?.CallType ?? default(CallTypeEnum)
            }).ToList();

            // ????? ??? ??? ????? ?? ?? ????
            if (callTypeFilter.HasValue)
            {
                volunteerList = volunteerList.Where(v => v.CallType == callTypeFilter.Value).ToList();
            }

            // ????
            volunteerList = sortField switch
            {
                VolunteerInListField.FullName => volunteerList.OrderBy(v => v.FullName).ThenBy(v => v.Id).ToList(),
                VolunteerInListField.Active => volunteerList.OrderBy(v => v.Active).ThenBy(v => v.Id).ToList(),
                VolunteerInListField.SumTreatedCalls => volunteerList.OrderBy(v => v.SumTreatedCalls).ThenBy(v => v.Id).ToList(),
                VolunteerInListField.SumCanceledCalls => volunteerList.OrderBy(v => v.SumCanceledCalls).ThenBy(v => v.Id).ToList(),
                VolunteerInListField.SumExpiredCalls => volunteerList.OrderBy(v => v.SumExpiredCalls).ThenBy(v => v.Id).ToList(),
                VolunteerInListField.CallIdInTreatment => volunteerList.OrderBy(v => v.CallIdInTreatment ?? -1).ThenBy(v => v.Id).ToList(),
                _ => volunteerList.OrderBy(v => v.Id).ToList(), // ????? ????
            };

            return volunteerList;
        }
        catch (Exception ex)
        {
            throw new BO.Exceptions.BlDoesNotExistException("Error retrieving volunteer list.", ex);
        }
    }



    public BO.Volunteer RequestVolunteerDetails(int volunteerId)
    {
        try
        {
            // ????? ?? ?????? ?-DAL ?-lock
            lock (AdminManager.BlMutex)
            {
                // ????? ???? ??????
                DO.Volunteer volunteer = _dal.Volunteer.Read(volunteerId)
                    ?? throw new BO.Exceptions.BlDoesNotExistException($"Volunteer with ID {volunteerId} not found.");

                // ????? ?? ??????? ?? ??????
                var assignments = _dal.assignment.ReadAll(a => a.VolunteerId == volunteerId);

                // ????? ??????? ???????
                var ongoingAssignments = assignments.Where(a =>
                {
                    DO.Call call = _dal.call.Read(a.CallId);
                    var status = Tools.callStatus(call.Id);
                    return status == BO.Enums.CalltStatusEnum.CallIsBeingTreated ||
                           status == BO.Enums.CalltStatusEnum.CallTreatmentAlmostOver;
                });

                // ????? ??????? ???? "?????" ?????? ??????? ??????? ????
                return new BO.Volunteer()
                {
                    Id = volunteer.Id,
                    FullName = volunteer.FullName,
                    PhoneNumber = volunteer.PhoneNumber,
                    Email = volunteer.Email,
                    Password = volunteer.Password,
                    Location = volunteer.Location,
                    Latitude = volunteer.Latitude,
                    Longitude = volunteer.Longitude,
                    Position = (BO.Enums.VolunteerTypeEnum)volunteer.Position,
                    Active = volunteer.Active,
                    MaxDistance = volunteer.MaxDistance,
                    DistanceType = (BO.Enums.DistanceTypeEnum)volunteer.DistanceType,
                    SumCanceled = assignments.Where(a => a.FinishAppointmentType == FinishAppointmentType.SelfCancellation || a.FinishAppointmentType == FinishAppointmentType.CancelingAnAdministrator).Count(),
                    SumExpired = assignments.Where(a => a.FinishAppointmentType == FinishAppointmentType.CancellationHasExpired).Count(),
                    SumCalls = assignments.Where(a => a.FinishAppointmentType == FinishAppointmentType.WasTreated).Count(),
                    VolunteerTakenCare = ongoingAssignments.Select(activeAssignment =>
                    {
                        var activeCall = _dal.call.Read(activeAssignment.CallId);
                        return new BO.CallInProgress
                        {
                            Id = activeAssignment.Id,
                            CallId = activeCall.Id,
                            CallType = (BO.Enums.CallTypeEnum)activeCall.CallType,
                            VerbDesc = activeCall.VerbDesc,
                            CallAddress = activeCall.Adress,
                            OpenTime = activeCall.OpenTime,
                            MaxFinishTime = activeCall.MaxTime ?? DateTime.MinValue,
                            StartAppointmentTime = activeAssignment.AppointmentTime,
                            DistanceOfCall = Tools.CalculateDistance(
                                activeCall.Latitude,
                                activeCall.Longitude,
                                volunteer.Latitude ?? 0,
                                volunteer.Longitude ?? 0
                            ),
                        };
                    }).FirstOrDefault() // ?? ?? ????? ??? ????? ??????
                };
            }
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.Exceptions.BlDoesNotExistException("Error retrieving volunteer details.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while retrieving volunteer details.", ex);
        }
    }


    public void UpdateVolunteerDetails(int requesterId, BO.Volunteer volunteerDetails)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  // stage 7
        try
        {
            lock (AdminManager.BlMutex)  // ????? ?? ?????? ????????
            {
                // ??? 1: ???? ????? ?????? ????? ???????
                var existingVolunteer = _dal.Volunteer.ReadAll(v => v.Id == volunteerDetails.Id).FirstOrDefault()
                    ?? throw new DalDoesNotExistException($"Volunteer with ID {volunteerDetails.Id} not found.");

                Console.WriteLine("Existing volunteer found.");

                // ??? 2: ????? ?? ????? ????? ??? ????? ?? ?????? ????
                if (requesterId != volunteerDetails.Id && IsAdmin(requesterId))
                {
                    throw new UnauthorizedAccessException("Unauthorized to update volunteer details.");
                }

                Console.WriteLine("Access authorization passed.");

                // ??? 3: ????? ????? ?????? ?????
                if (!VolunteerManager.checkVolunteerEmail(volunteerDetails))
                    throw new BlEmailNotCorrect("Invalid email format.");
                if (!VolunteerManager.IsValidId(volunteerDetails.Id))
                    throw new BlIdNotValid("Invalid ID format.");
                if (!VolunteerManager.IsPhoneNumberValid(volunteerDetails))
                    throw new BlPhoneNumberNotCorrect("Invalid phone number format.");
                if (!VolunteerManager.IsValidId(volunteerDetails.Id))
                    throw new BlIdNotValid("Invalid ID format.");

                Console.WriteLine("Format checks passed.");

                // ??? 4: ????? ????? ?? ??????
                if (volunteerDetails.Latitude == null || volunteerDetails.Longitude == null)
                    throw new BlInvalidLocationException("Location must include valid latitude and longitude.");

                Console.WriteLine("Location checks passed.");

                if ((Enums.VolunteerTypeEnum)existingVolunteer.Position != volunteerDetails.Position && !IsAdmin(volunteerDetails.Id))
                {
                    throw new BlUnauthorizedAccessException("Only admins can update the position.");
                }

                Console.WriteLine("Position update authorized.");

                // ??? 6: ????? ?????? ?-BO ?-DO
                DO.Volunteer newVolunteer = new DO.Volunteer
                {
                    Id = volunteerDetails.Id,
                    FullName = volunteerDetails.FullName,
                    PhoneNumber = volunteerDetails.PhoneNumber,
                    Password = volunteerDetails.Password,
                    Location = volunteerDetails.Location,
                    Email = volunteerDetails.Email,
                    Active = volunteerDetails.Active,
                    DistanceType = (DO.DistanceType)volunteerDetails.DistanceType,
                    Position = (DO.Position)volunteerDetails.Position,  // ???? ????? ?-Position ??? ?? ????? ??? ?? ??????
                    Latitude = volunteerDetails.Latitude,
                    Longitude = volunteerDetails.Longitude,
                    MaxDistance = volunteerDetails.MaxDistance,
                };

                Console.WriteLine("Volunteer data mapped to DO object.");

                // ??? 7: ????? ????? ?????? ????? ???????
                _dal.Volunteer.Update(newVolunteer);

                // ??? 8: ????? ????? ?????? ?? ??? ????? ???? ??? ??????
                var updatedVolunteer = _dal.Volunteer.Read(v => v.Id == newVolunteer.Id);
                if (updatedVolunteer == null)
                {
                    throw new Exception("Volunteer update failed, no record found after update.");
                }

                Console.WriteLine("Volunteer updated successfully in database.");

                VolunteerManager.Observers.NotifyItemUpdated(newVolunteer.Id);  // stage 5
                VolunteerManager.Observers.NotifyListUpdated();  // stage 5
                Console.WriteLine("Observers notified.");
            }
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine(ex.Message);
            throw;  // ????? ?? ????? ???????
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine(ex.Message);
            throw;  // ????? ?? ????? ???????
        }
        catch (BlEmailNotCorrect ex)
        {
            Console.WriteLine(ex.Message);
            throw;  // ????? ?? ????? ???????
        }
        catch (BlPhoneNumberNotCorrect ex)
        {
            Console.WriteLine(ex.Message);
            throw;  // ????? ?? ????? ???????
        }
        catch (BlIdNotValid ex)
        {
            Console.WriteLine(ex.Message);
            throw;  // ????? ?? ????? ???????
        }
        catch (BlInvalidLocationException ex)
        {
            Console.WriteLine(ex.Message);
            throw;  // ????? ?? ????? ???????
        }
        catch (BlUnauthorizedAccessException ex)
        {
            Console.WriteLine(ex.Message);
            throw;  // ????? ?? ????? ???????
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred during update.");
            throw new CannotUpdateVolunteerException("An error occurred while updating the volunteer details.", ex);
        }
    }

    private bool IsAdmin(int id)
    {
        lock (AdminManager.BlMutex)  // ????? ?-lock
        {
            var volunteer = _dal.Volunteer.Read(v => v.Id == id);
            if (volunteer != null && volunteer.Position == DO.Position.admin)
                return true;

            return false;
        }
    }



    public void DeleteVolunteer(int volunteerId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        try
        {
            // ??? 1: ???? ?? ?????? ???? ?????? ?????
            lock (AdminManager.BlMutex)  // ????? ?-lock ???? ????? ?-DAL
            {
                var assignments = _dal.assignment.ReadAll(a => a.VolunteerId == volunteerId).ToList();
                var activeAssignment = assignments.FirstOrDefault(a => a.FinishAppointmentType == null); // ?? ?? ????? ?????

                if (activeAssignment != null)
                {
                    // ?? ?????? ???? ?????? ?????, ?? ????? ?????
                    throw new BlCantBeErased($"Volunteer with id {volunteerId} cannot be erased because they are currently handling a call.");
                }
            }

            // ??? 2: ?????? ????? ?? ?????? ?? ??? ?? ???? ?????? ?????
            lock (AdminManager.BlMutex)  // ????? ?-lock ???? ????? ?-DAL ??????
            {
                _dal.Volunteer.Delete(volunteerId); // ???? ????? ?? ??????
            }

            VolunteerManager.Observers.NotifyItemUpdated(volunteerId);  //stage 5
            VolunteerManager.Observers.NotifyListUpdated(); //stage 5
        }
        catch (DO.DalDoesNotExistException ex)
        {
            // ????? ?????? ????? ?? ????? ??? ????
            throw new BlDoesNotExistException($"Volunteer with id {volunteerId} not found.", ex);
        }
    }





    public void AddVolunteer(BO.Volunteer volunteer)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        DO.Volunteer newVolunteer = new()
        {
            Id = volunteer.Id,
            FullName = volunteer.FullName,
            PhoneNumber = volunteer.PhoneNumber,
            Password = volunteer.Password,
            Email = volunteer.Email,
            Active = volunteer.Active,
            DistanceType = (DO.DistanceType)volunteer.DistanceType,
            Position = (DO.Position)volunteer.Position,
            MaxDistance = volunteer.MaxDistance,
            // ?? ???? ?? Location, Latitude, Longitude ??? ?? ?? ??????? ????
        };

        if (!VolunteerManager.checkVolunteerEmail(volunteer))
            throw new BlEmailNotCorrect("Invalid Email format.");
        if (!(VolunteersManager.IsValidId(volunteer.Id)))
            throw new BlIdNotValid("Invalid ID format.");
        if (!(VolunteersManager.IsPhoneNumberValid(volunteer)))
            throw new BlPhoneNumberNotCorrect("Invalid PhoneNumber format.");

        try
        {
            // ????? ????? ?-DAL ??? ????? ???????? ?????
            lock (AdminManager.BlMutex)
            {
                _dal.Volunteer.Create(newVolunteer);
            }

            VolunteersManager.Observers.NotifyItemUpdated(newVolunteer.Id);  // stage 5
            VolunteersManager.Observers.NotifyListUpdated(); // stage 5   
        }
        catch (DO.DalAlreadyExistException)
        {
            throw new BLAlreadyExistException($"Volunteer with id {volunteer.Id} already exists");
        }
    }
}
