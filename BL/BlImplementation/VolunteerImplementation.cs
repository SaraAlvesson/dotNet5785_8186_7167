namespace BlImplementation;
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
    VolunteersManager.Observers.AddListObserver(listObserver); //stage 5
    public void AddObserver(int id, Action observer) =>
    VolunteersManager.Observers.AddObserver(id, observer); //stage 5
    public void RemoveObserver(Action listObserver) =>
    VolunteersManager.Observers.RemoveListObserver(listObserver); //stage 5
    public void RemoveObserver(int id, Action observer) =>
   VolunteersManager.Observers.RemoveObserver(id, observer); //stage 5
    #endregion Stage 5


  
    public string Login(int username, string password)
    {
        // שלב 1: שליפת המתנדב לפי מזהה
        var volunteer = _dal.Volunteer.ReadAll(v => v.Id == username).FirstOrDefault();

        if (volunteer != null)
        {
            // שלב 2: פענוח סיסמת המתנדב מה-XML
            string decodedPassword = HttpUtility.HtmlDecode(volunteer.Password);

            // שלב 3: פענוח סיסמת הקלט של המשתמש (למקרה שהיא מקודדת)
            string inputPassword = HttpUtility.HtmlDecode(password);

            // השוואת הסיסמאות לאחר הפענוח
            if (decodedPassword != inputPassword)
            {
                throw new BlPasswordNotValid("Incorrect password");
            }

            // שלב 4: המרת תפקיד
            if (Enum.TryParse<BO.Enums.VolunteerTypeEnum>(volunteer.Position.ToString(), out var volunteerType))
            {
                return volunteerType.ToString();
            }
            else
            {
                throw new Exception("Failed to map volunteer position to BO.Enums.VolunteerTypeEnum.");
            }
        }

        // שלב 5: אם המשתמש לא נמצא
        throw new BlDoesNotExistException($"Username {username} not found");
    }
    public IEnumerable<VolunteerInList> RequestVolunteerList(
      bool? isActive,
      VolunteerInListField? sortField = null,
      CallTypeEnum? callTypeFilter = null)  // הוספת פילטר לסוג קריאה
    {
        try
        {
            var volunteers = _dal.Volunteer.ReadAll();

            // סינון לפי פעילות
            if (isActive.HasValue)
            {
                volunteers = volunteers.Where(v => v.Active == isActive.Value).ToList();
            }

            // הבאת הנתונים לאחר הסינון
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

            // סינון לפי סוג קריאה אם יש צורך
            if (callTypeFilter.HasValue)
            {
                volunteerList = volunteerList.Where(v => v.CallType == callTypeFilter.Value).ToList();
            }

            // מיון
            volunteerList = sortField switch
            {
                VolunteerInListField.FullName => volunteerList.OrderBy(v => v.FullName).ThenBy(v => v.Id).ToList(),
                VolunteerInListField.Active => volunteerList.OrderBy(v => v.Active).ThenBy(v => v.Id).ToList(),
                VolunteerInListField.SumTreatedCalls => volunteerList.OrderBy(v => v.SumTreatedCalls).ThenBy(v => v.Id).ToList(),
                VolunteerInListField.SumCanceledCalls => volunteerList.OrderBy(v => v.SumCanceledCalls).ThenBy(v => v.Id).ToList(),
                VolunteerInListField.SumExpiredCalls => volunteerList.OrderBy(v => v.SumExpiredCalls).ThenBy(v => v.Id).ToList(),
                VolunteerInListField.CallIdInTreatment => volunteerList.OrderBy(v => v.CallIdInTreatment ?? -1).ThenBy(v => v.Id).ToList(),
                _ => volunteerList.OrderBy(v => v.Id).ToList(), // ברירת מחדל
            };

            return volunteerList;
        }
        catch (Exception ex)
        {
            throw new BO.Exceptions.BlDoesNotExistException("Error retrieving volunteer list.", ex);
        }
    }


    //   public IEnumerable<VolunteerInList> RequestVolunteerList(
    //    bool? isActive,
    //    VolunteerInListField? sortField = null)
    //{
    //    try
    //    {
    //        var volunteers = _dal.Volunteer.ReadAll();

    //        if (isActive.HasValue)
    //        {
    //            volunteers = volunteers.Where(v => v.Active == isActive.Value).ToList();
    //            Console.WriteLine($"Active filter applied: {isActive.Value}");
    //        }

    //        if (!volunteers.Any())
    //        {
    //            Console.WriteLine("No volunteers found after applying active filter.");
    //        }

    //        var volunteerDetailsMap = volunteers
    //            .Select(v => RequestVolunteerDetails(v.Id))
    //            .ToDictionary(d => d.Id);

    //        var volunteerList = volunteerDetailsMap.Values.Select(details => new VolunteerInList
    //        {
    //            Id = details.Id,
    //            FullName = details.FullName,
    //            Active = details.Active,
    //            SumTreatedCalls = details.SumCalls,
    //            SumCanceledCalls = details.SumCanceled,
    //            SumExpiredCalls = details.SumExpired,
    //            CallIdInTreatment = details.VolunteerTakenCare?.CallId,
    //            CallType = details.VolunteerTakenCare?.CallType ?? default(CallTypeEnum)
    //        }).ToList();

    //        if (callTypeFilter.HasValue)
    //        {
    //            volunteerList = volunteerList.Where(v => v.CallType == callTypeFilter.Value).ToList();
    //            Console.WriteLine($"Call type filter applied: {callTypeFilter.Value}");
    //        }

    //        if (!volunteerList.Any())
    //        {
    //            Console.WriteLine("No volunteers found after applying call type filter.");
    //        }

    //        if (sortField.HasValue)
    //        {
    //            Console.WriteLine($"Sorting by: {sortField}");
    //            switch (sortField)
    //            {
    //                case VolunteerInListField.FullName:
    //                    volunteerList = volunteerList.OrderBy(v => v.FullName).ThenBy(v => v.Id).ToList();
    //                    break;
    //                case VolunteerInListField.Active:
    //                    volunteerList = volunteerList.OrderBy(v => v.Active).ThenBy(v => v.Id).ToList();
    //                    break;
    //                case VolunteerInListField.SumTreatedCalls:
    //                    volunteerList = volunteerList.OrderBy(v => v.SumTreatedCalls).ThenBy(v => v.Id).ToList();
    //                    break;
    //                case VolunteerInListField.SumCanceledCalls:
    //                    volunteerList = volunteerList.OrderBy(v => v.SumCanceledCalls).ThenBy(v => v.Id).ToList();
    //                    break;
    //                case VolunteerInListField.SumExpiredCalls:
    //                    volunteerList = volunteerList.OrderBy(v => v.SumExpiredCalls).ThenBy(v => v.Id).ToList();
    //                    break;
    //                case VolunteerInListField.CallIdInTreatment:
    //                    volunteerList = volunteerList.OrderBy(v => v.CallIdInTreatment ?? -1).ThenBy(v => v.Id).ToList();
    //                    break;
    //                case VolunteerInListField.CallType:
    //                    volunteerList = volunteerList.OrderBy(v => v.CallType).ThenBy(v => v.Id).ToList();
    //                    break;
    //                default:
    //                    volunteerList = volunteerList.OrderBy(v => v.Id).ToList();
    //                    break;
    //            }
    //        }
    //        else
    //        {
    //            volunteerList = volunteerList.OrderBy(v => v.Id).ToList();
    //        }

    //        return volunteerList;
    //    }
    //    catch (Exception ex)
    //    {
    //        throw new BO.Exceptions.BlDoesNotExistException("Error retrieving volunteer list.", ex);
    //    }
    //}


    public BO.Volunteer RequestVolunteerDetails(int volunteerId)
    {
        try
        {
            // שליפת פרטי המתנדב משכבת הנתונים
            DO.Volunteer volunteer = _dal.Volunteer.Read(volunteerId)
                ?? throw new BO.Exceptions.BlDoesNotExistException($"Volunteer with ID {volunteerId} not found.");

            // שליפת כל ההקצאות של המתנדב
            var assignments = _dal.assignment.ReadAll(a => a.VolunteerId == volunteerId);

            // סינון הקריאות שבתהליך (בטיפול או בטיפול בסיכון בלבד)
            var ongoingAssignments = assignments.Where(a =>
            {
                DO.Call call = _dal.call.Read(a.CallId);
                var status = Tools.callStatus(call.Id);
                return status == BO.Enums.CalltStatusEnum.CallIsBeingTreated ||
                       status == BO.Enums.CalltStatusEnum.CallTreatmentAlmostOver;
            });


            // יצירת אובייקט לוגי "מתנדב" והחזרת הקריאות שבתהליך בלבד
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
                SumCalls = assignments.Where(a => a.FinishAppointmentType == FinishAppointmentType.WasTreated).Count(), // ספירת הקריאות שבתהליך
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
                }).FirstOrDefault() // אם יש קריאה אחת לפחות בטיפול
            };
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


    //public void UpdateVolunteerDetails(int volunteerId, BO.Volunteer volunteerDetails)
    //{
    //    try
    //    {
    //        // שלב 1: בקשת רשומת המתנדב משכבת הנתונים
    //        var existingVolunteer = _dal.Volunteer.ReadAll(v => v.Id == volunteerId).FirstOrDefault()
    //            ?? throw new DalDoesNotExistException("Volunteer not found.");

    //        // שלב 2: בדיקה אם המבקש לעדכן הוא המנהל או המתנדב עצמו
    //        if (volunteerDetails.Id != volunteerId && !IsAdmin(volunteerDetails.Id))
    //            throw new UnauthorizedAccessException("Unauthorized to update volunteer details.");

    //        // שלב 3: בדיקת ערכים מבחינת פורמט
    //        if (!(Helpers.VolunteersManager.checkVolunteerEmail(volunteerDetails)))
    //            throw new BlEmailNotCorrect("Invalid Email format.");
    //        if (!(Helpers.VolunteersManager.IsValidId(volunteerDetails.Id)))
    //            throw new BlIdNotValid("Invalid ID format.");
    //        if (!(Helpers.VolunteersManager.IsPhoneNumberValid(volunteerDetails)))
    //            throw new BlPhoneNumberNotCorrect("Invalid PhoneNumber format.");

    //        // שלב 4: בדיקה אם מותר לשנות תפקיד (רק למנהל)
    //        if ((Enums.VolunteerTypeEnum)existingVolunteer.Position != volunteerDetails.Position && !IsAdmin(volunteerDetails.Id))

    //            throw new BlUnauthorizedAccessException("Only admins can update the Positions.");

    //        // שלב 5: עדכון רשומת המתנדב בערכים החדשים


    //        DO.Volunteer newVolunteer = new()

    //        {
    //            FullName = volunteerDetails.FullName,
    //            PhoneNumber = volunteerDetails.PhoneNumber,
    //            Email = volunteerDetails.Email,
    //            Active = volunteerDetails.Active,
    //            DistanceType = (DO.DistanceType)volunteerDetails.DistanceType,
    //            Position = (DO.Position)volunteerDetails.Position,
    //            Latitude = volunteerDetails.Latitude,
    //            Longitude = volunteerDetails.Longitude,
    //            MaxDistance = volunteerDetails.MaxDistance,

    //        };
    //        // שלב 6: עדכון הרשומה בשכבת הנתונים
    //        _dal.Volunteer.Update(newVolunteer);
    //    }
    //    catch (DO.DalDoesNotExistException ex)
    //    {  // טיפול בחריגות וזריקתן מחדש עם מידע ברור לשכבת התצוגה
    //        throw new CannotUpdateVolunteerException("Error updating volunteer details.", ex);
    //    }




    //}
    public void UpdateVolunteerDetails(int requesterId, BO.Volunteer volunteerDetails)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        try
        {
            // שלב 1: בקשת רשומת המתנדב משכבת הנתונים
            var existingVolunteer = _dal.Volunteer.ReadAll(v => v.Id == volunteerDetails.Id).FirstOrDefault()
                ?? throw new DalDoesNotExistException($"Volunteer with ID {volunteerDetails.Id} not found.");

            Console.WriteLine("Existing volunteer found.");

            // שלב 2: בדיקה אם המבקש לעדכן הוא המנהל או המתנדב עצמו
            if (requesterId != volunteerDetails.Id && IsAdmin(requesterId))
            {
                throw new UnauthorizedAccessException("Unauthorized to update volunteer details.");
            }

            Console.WriteLine("Access authorization passed.");

            // שלב 3: בדיקת ערכים מבחינת פורמט
            if (!Helpers.VolunteersManager.checkVolunteerEmail(volunteerDetails))
                throw new BlEmailNotCorrect("Invalid email format.");
            if (!Helpers.VolunteersManager.IsPhoneNumberValid(volunteerDetails))
                throw new BlPhoneNumberNotCorrect("Invalid phone number format.");
            if (!Helpers.VolunteersManager.IsValidId(volunteerDetails.Id))
                throw new BlIdNotValid("Invalid ID format.");

            Console.WriteLine("Format checks passed.");

            // שלב 4: בדיקה לוגית של הערכים
            if (volunteerDetails.Latitude == null || volunteerDetails.Longitude == null)
                throw new BlInvalidLocationException("Location must include valid latitude and longitude.");

            Console.WriteLine("Location checks passed.");

            if ((Enums.VolunteerTypeEnum)existingVolunteer.Position != volunteerDetails.Position && !IsAdmin(volunteerDetails.Id))
            {
                throw new BlUnauthorizedAccessException("Only admins can update the position.");
            }

            Console.WriteLine("Position update authorized.");

            // שלב 6: העברת נתונים מ-BO ל-DO
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
                Position = (DO.Position)volunteerDetails.Position,  // חשוב לוודא ש-Position כאן לא משאיר ערך לא מעודכן
                Latitude = volunteerDetails.Latitude,
                Longitude = volunteerDetails.Longitude,
                MaxDistance = volunteerDetails.MaxDistance,
            };

            Console.WriteLine("Volunteer data mapped to DO object.");

            // שלב 7: עדכון רשומת המתנדב בשכבת הנתונים
            _dal.Volunteer.Update(newVolunteer);

            // שלב 8: קריאה חוזרת לרשומה על מנת לוודא שהיא אכן עודכנה
            var updatedVolunteer = _dal.Volunteer.Read(v => v.Id == newVolunteer.Id);
            if (updatedVolunteer == null)
            {
                throw new Exception("Volunteer update failed, no record found after update.");
            }

            Console.WriteLine("Volunteer updated successfully in database.");

            VolunteersManager.Observers.NotifyItemUpdated(newVolunteer.Id);  // stage 5
            VolunteersManager.Observers.NotifyListUpdated();  // stage 5
            Console.WriteLine("Observers notified.");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine(ex.Message);
            throw;  // מיידע על חריגה ספציפית
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine(ex.Message);
            throw;  // מיידע על חריגה ספציפית
        }
        catch (BlEmailNotCorrect ex)
        {
            Console.WriteLine(ex.Message);
            throw;  // מיידע על חריגה ספציפית
        }
        catch (BlPhoneNumberNotCorrect ex)
        {
            Console.WriteLine(ex.Message);
            throw;  // מיידע על חריגה ספציפית
        }
        catch (BlIdNotValid ex)
        {
            Console.WriteLine(ex.Message);
            throw;  // מיידע על חריגה ספציפית
        }
        catch (BlInvalidLocationException ex)
        {
            Console.WriteLine(ex.Message);
            throw;  // מיידע על חריגה ספציפית
        }
        catch (BlUnauthorizedAccessException ex)
        {
            Console.WriteLine(ex.Message);
            throw;  // מיידע על חריגה ספציפית
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred during update.");
            throw new CannotUpdateVolunteerException("An error occurred while updating the volunteer details.", ex);
        }
    }
    private bool IsAdmin(int id)
    {
        var volunteer = _dal.Volunteer.Read(v => v.Id == id);
        if (volunteer != null && volunteer.Position == DO.Position.admin)
            return true;

        return false;
    }
 

    public void DeleteVolunteer(int volunteerId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        try
        {
            // שלב 1: בדוק אם המתנדב מטפל בקריאה פעילה
            var assignments = _dal.assignment.ReadAll(a => a.VolunteerId == volunteerId).ToList();
            var activeAssignment = assignments.FirstOrDefault(a => a.FinishAppointmentType == null); // אם יש הקצאה פעילה

            if (activeAssignment != null)
            {
                // אם המתנדב מטפל בהקצאה פעילה, יש לזרוק חריגה
                throw new BlCantBeErased($"Volunteer with id {volunteerId} cannot be erased because they are currently handling a call.");
            }

            // שלב 2: ניסיון למחוק את המתנדב אם הוא לא מטפל בהקצאה פעילה
            _dal.Volunteer.Delete(volunteerId); // מנסה למחוק את המתנדב 
            VolunteersManager.Observers.NotifyItemUpdated(volunteerId);  //stage 5
            VolunteersManager.Observers.NotifyListUpdated(); //stage
        }
        catch (DO.DalDoesNotExistException ex)
        {
            // טיפול בחריגה במקרה של מתנדב שלא נמצא
            throw new BlDoesNotExistException($"Volunteer with id {volunteerId} not found.", ex);
        }
    }





    public void AddVolunteer(BO.Volunteer volunteer)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        DO.Volunteer newVolunteer = new()

        {   Id = volunteer.Id,
            FullName = volunteer.FullName,
            PhoneNumber = volunteer.PhoneNumber,
            Password = volunteer.Password,      
            Location = volunteer.Location,
            Email = volunteer.Email,
            Active = volunteer.Active,
            DistanceType = (DO.DistanceType)volunteer.DistanceType,
            Position = (DO.Position)volunteer.Position,
            Latitude = volunteer.Latitude,
            Longitude = volunteer.Longitude,
            MaxDistance = volunteer.MaxDistance,

        };

        if (!(Helpers.VolunteersManager.checkVolunteerEmail(volunteer)))
            throw new BlEmailNotCorrect("Invalid Email format.");
        if (!(Helpers.VolunteersManager.IsValidId(volunteer.Id)))
            throw new BlIdNotValid("Invalid ID format.");
        if (!(Helpers.VolunteersManager.IsPhoneNumberValid(volunteer)))
            throw new BlPhoneNumberNotCorrect("Invalid PhoneNumber format.");
        if (!(Helpers.Tools.IsAddressValid(volunteer.Location)))
            throw new BlPhoneNumberNotCorrect("Invalid Location format.");
        try
        {
            _dal.Volunteer.Create(newVolunteer);
            VolunteersManager.Observers.NotifyItemUpdated(newVolunteer.Id);  // stage 5
            VolunteersManager.Observers.NotifyListUpdated(); //stage 5   
        }
        catch (DO.DalAlreadyExistException ex)
        {
            throw new BLAlreadyExistException($"Volunteer with id {volunteer.Id} already exists");
        }

    }

    
}


