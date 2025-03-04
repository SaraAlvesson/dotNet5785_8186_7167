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
            lock (AdminManager.BlMutex)
            {

                var volunteer = _dal.Volunteer.ReadAll(v => v.Id == username).FirstOrDefault();

                if (volunteer != null)
                {

                    string decodedPassword = HttpUtility.HtmlDecode(volunteer.Password);

                    string inputPassword = HttpUtility.HtmlDecode(password);

                    if (decodedPassword != inputPassword)
                    {
                        throw new BlPasswordNotValid("Incorrect password");
                    }


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
 CallTypeEnum? callTypeFilter = null)
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


            if (callTypeFilter.HasValue)
            {
                volunteerList = volunteerList.Where(v => v.CallType == callTypeFilter.Value).ToList();
            }


            volunteerList = sortField switch
            {
                VolunteerInListField.FullName => volunteerList.OrderBy(v => v.FullName).ThenBy(v => v.Id).ToList(),
                VolunteerInListField.Active => volunteerList.OrderBy(v => v.Active).ThenBy(v => v.Id).ToList(),
                VolunteerInListField.SumTreatedCalls => volunteerList.OrderBy(v => v.SumTreatedCalls).ThenBy(v => v.Id).ToList(),
                VolunteerInListField.SumCanceledCalls => volunteerList.OrderBy(v => v.SumCanceledCalls).ThenBy(v => v.Id).ToList(),
                VolunteerInListField.SumExpiredCalls => volunteerList.OrderBy(v => v.SumExpiredCalls).ThenBy(v => v.Id).ToList(),
                VolunteerInListField.CallIdInTreatment => volunteerList.OrderBy(v => v.CallIdInTreatment ?? -1).ThenBy(v => v.Id).ToList(),
                _ => volunteerList.OrderBy(v => v.Id).ToList(),
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
        return VolunteerManager.RequestVolunteerDetails(volunteerId);
    }


    public void UpdateVolunteerDetails(int requesterId, BO.Volunteer volunteerDetails)
    {
        VolunteerManager.UpdateVolunteerDetails(requesterId, volunteerDetails);
    }





    public void DeleteVolunteer(int volunteerId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        try
        {

            lock (AdminManager.BlMutex)
            {
                var assignments = _dal.assignment.ReadAll(a => a.VolunteerId == volunteerId).ToList();
                var activeAssignment = assignments.FirstOrDefault(a => a.FinishAppointmentType == null);

                if (activeAssignment != null)
                {

                    throw new BlCantBeErased($"Volunteer with id {volunteerId} cannot be erased because they are currently handling a call.");
                }
            }


            lock (AdminManager.BlMutex)
            {
                _dal.Volunteer.Delete(volunteerId);
            }

            VolunteerManager.Observers.NotifyItemUpdated(volunteerId);  //stage 5
            VolunteerManager.Observers.NotifyListUpdated(); //stage 5
        }
        catch (DO.DalDoesNotExistException ex)
        {

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
            Location=volunteer.Location

        };

        if (!VolunteerManager.checkVolunteerEmail(volunteer))
            throw new BlEmailNotCorrect("Invalid Email format.");
        if (!(VolunteerManager.IsValidId(volunteer.Id)))
            throw new BlIdNotValid("Invalid ID format.");
        if (!(VolunteerManager.IsPhoneNumberValid(volunteer)))
            throw new BlPhoneNumberNotCorrect("Invalid PhoneNumber format.");

        try
        {
            // ????? ????? ?-DAL ??? ????? ???????? ?????
            lock (AdminManager.BlMutex)
            {
                _dal.Volunteer.Create(newVolunteer);
            }

            VolunteerManager.Observers.NotifyItemUpdated(newVolunteer.Id);  // stage 5
            VolunteerManager.Observers.NotifyListUpdated(); // stage 5   
        }
        catch (DO.DalAlreadyExistException)
        {
            throw new BLAlreadyExistException($"Volunteer with id {volunteer.Id} already exists");
        }
    }
}
