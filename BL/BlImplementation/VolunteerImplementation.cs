
namespace BlImplementation;
using BlApi;
using BO;


internal class VolunteerImplementation : IVolunteer

{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    public string Login(string username, string password)
    {

        var volunteer = _dal.Volunteer.ReadAll(v => v.FullName == username).FirstOrDefault();
        if (volunteer != null)
        {
            if (volunteer.Password != password)
                throw new ArgumentException("Incorrect password");

            return ((Enums.VolunteerTypeEnum)volunteer.Position).ToString();
        }
        throw new ArgumentException($"Username {username} not found");

    }
    public IEnumerable<BO.VolunteerInList> RequestVolunteerList(bool? isActive = null, VolunteerInList? sortField = null)
    {
        var volunteers = _dal.Volunteer.ReadAll().Select(v => new BO.VolunteerInList
        {
            Id = v.Id,
            FullName = v.FullName,
            Active = v.Active,



        });
        if (isActive == null && sortField==null)
        {
            return volunteers.OrderBy(v => v.Id);
        }
        else
        if (isActive == null && sortField != null)

        {
              return (volunteers.OrderBy(v => v.CallType));
        }
        if (isActive != null && sortField == null)
        {
            volunteers = volunteers.Where(v => v.Active == isActive);
            return volunteers.OrderBy(v => v.Id);
        }
        else
        {
            volunteers = volunteers.Where(v => v.Active == isActive);
            return (volunteers.OrderBy(v => v.CallType));
        }
       

    }
    //    public BO.Volunteer RequestVolunteerDetails(int volunteerId)
    //{
    //    try
    //    {
    //        // שליפת פרטי מתנדב משכבת הנתונים
    //        var volunteer = _dal.Volunteer.Read(volunteerId)
    //            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} not found.");

    //        // שליפת קריאה בטיפול, אם קיימת
    //        var call = _dal.call.ReadAll().FirstOrDefault(c => c.Id == volunteerId);

    //        // החזרת האובייקט הלוגי "מתנדב" עם קריאה בטיפול (אם קיימת)
    //        return new BO.Volunteer
    //        {
    //            Id = volunteer.Id,
    //            FullName = volunteer.FullName,
    //            Email = volunteer.Email,
    //            VolunteerTakenCare = call == null
    //                ? null
    //                : new BO.CallInProgress
    //                {
    //                    Id = call.Id,
    //                    CallType = (BO.Enums.CallTypeEnum)call.CallType, // המרה ישירה ל-Enum
    //                    VerbDesc = call.VerbDesc,
    //                    CallAddress = call.Adress, // תיקון שם שדה אם צריך
    //                    OpenTime = call.OpenTime,
    //                    MaxFinishTime = (DateTime)call.MaxTime,
    //                    StartAppointmentTime = call.OpenTime,
    //                    DistanceOfCall = call.Latitude, //longtitude and latitude
    //                    Status = ((BO.Enums.CallStatusEnum)(DO.FinishAppointmentType)Assignment.FinishAppointmentType).ToString()
    //                },
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        throw new ArgumentException("Error retrieving volunteer details.", ex);
    //    }
    //}

    public BO.Volunteer RequestVolunteerDetails(int volunteerId)
    {
        try
        {
            // שליפת פרטי מתנדב משכבת הנתונים
            var volunteer = _dal.Volunteer.Read(volunteerId)
                ?? throw new BO.DoesNotExistException($"Volunteer with ID {volunteerId} not found.");

            // שליפת קריאה בטיפול, אם קיימת
            var call = _dal.call.ReadAll().FirstOrDefault(c => c.Id == volunteerId);
            var assignment = _dal.assignment.ReadAll().FirstOrDefault(c => c.FinishAppointmentType = null

            // החזרת האובייקט הלוגי "מתנדב" עם קריאה בטיפול (אם קיימת)
            return new BO.Volunteer
            {
                Id = volunteer.Id,
                FullName = volunteer.FullName,
                Email = volunteer.Email,
                VolunteerTakenCare = call == null
                    ? null
                    : new BO.CallInProgress
                    {
                        Id = call.Id,
                        CallType = (BO.Enums.CallTypeEnum)call.CallType, // המרה ישירה ל-Enum
                        VerbDesc = call.VerbDesc,
                        CallAddress = call.Adress, // תיקון שם שדה אם צריך
                        OpenTime = call.OpenTime,
                        MaxFinishTime = (DateTime)call.MaxTime,
                        StartAppointmentTime = call.OpenTime,
                        DistanceOfCall = call.Latitude,////longtitude and latitude
                        Status = (Enums.CallStatusEnum)Assignment.FinishAppointmentType
                    }


            };
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Error retrieving volunteer details.", ex);
        }

    }

    public void UpdateVolunteerDetails(int volunteerId, BO.Volunteer volunteerDetails)
    {
        try
        {
            // שלב 1: בקשת רשומת המתנדב משכבת הנתונים
            var existingVolunteer = _dal.Volunteer.ReadAll(v => v.Id == volunteerId).FirstOrDefault()
                ?? throw new BlDoesNotExistException("Volunteer not found.");

            // שלב 2: בדיקה אם המבקש לעדכן הוא המנהל או המתנדב עצמו
            if (volunteerDetails.Id != volunteerId && !IsAdmin(volunteerDetails.Id))
                throw new UnauthorizedAccessException("Unauthorized to update volunteer details.");

            // שלב 3: בדיקת ערכים מבחינת פורמט
            if (!(Helpers.Tools.IsValidEmail(volunteerDetails.Email)))
                throw new ArgumentException("Invalid email format.");
            if (!(Helpers.Tools.IsValidId(volunteerDetails.Id)))
                throw new ArgumentException("Invalid ID format.");
            if (!(Helpers.Tools.IsValidAddress(volunteerDetails.Address)))
                throw new ArgumentException("Invalid address.");

            // שלב 4: בדיקה אם מותר לשנות תפקיד (רק למנהל)
            if ((Enums.VolunteerTypeEnum)existingVolunteer.Position != volunteerDetails.position && !IsAdmin(volunteerDetails.Id))

                throw new UnauthorizedAccessException("Only admins can update roles.");

            // שלב 5: עדכון רשומת המתנדב בערכים החדשים
            existingVolunteer.FullName = volunteerDetails.FullName;
            existingVolunteer.Email = volunteerDetails.Email;
            existingVolunteer.Active = volunteerDetails.Active;
            existingVolunteer.Position = volunteerDetails.position;
            existingVolunteer.Latitude = volunteerDetails.Latitude;
            existingVolunteer.Longitude = volunteerDetails.Longitude;

            // שלב 6: עדכון הרשומה בשכבת הנתונים
            _dal.Volunteer.Update(existingVolunteer);
        }
        catch (DalApi.DalException ex)
        {
            // טיפול בחריגות וזריקתן מחדש עם מידע ברור לשכבת התצוגה
            throw new ArgumentException("Error updating volunteer details.", ex);
        }
    }



    public void UpdateVolunteerDetails(int volunteerId, BO.Volunteer volunteerDetails)
    {
        

        var existingVolunteer = _dal.Volunteers.FirstOrDefault(v => v.Id == volunteer.Id)
            ?? throw new KeyNotFoundException("Volunteer not found.");

        if (requesterId != existingVolunteer.Id && !IsAdmin(requesterId))
            throw new UnauthorizedAccessException("Unauthorized to update volunteer details.");

        if (existingVolunteer.Role != volunteer.Role && !IsAdmin(requesterId))
            throw new UnauthorizedAccessException("Only admins can update roles.");

        existingVolunteer.Name = volunteer.Name;
        existingVolunteer.Email = volunteer.Email;
        existingVolunteer.IsActive = volunteer.IsActive;

        _dal.Volunteers.Update(existingVolunteer);
    }

    public void DeleteVolunteer(int volunteerId)
    {
        var volunteer = _dal.Volunteers.FirstOrDefault(v => v.Id == id)
            ?? throw new KeyNotFoundException("Volunteer not found.");

        if (_dal.Calls.Any(c => c.VolunteerId == id))
            throw new InvalidOperationException("Cannot delete a volunteer with active or past calls.");

        _dal.Volunteers.Remove(volunteer);
    }

    private void ValidateVolunteer(Volunteer volunteer)
    {
        if (string.IsNullOrEmpty(volunteer.FullName) || string.IsNullOrEmpty(volunteer.Email))
            throw new ArgumentException("Volunteer name and email must not be empty.");
    }

    private bool IsAdmin(int id)
    {
        var volunteer = _dal.Volunteers.FirstOrDefault(v => v.Id == id);
        return volunteer?.Role == "Admin";
    }
    public void AddVolunteer(Volunteer volunteerDetails)
    {



    }
}


//    internal class VolunteerImplementation : IVolunteer
//    {
//    private readonly DalApi.IDal _dal = DalApi.Factory.Get;





//        public string Login(string userName, string password)
//        {
//            // בודק אם המשתמש קיים ומחזיר את התפקיד, אחרת זורק חריגה
//            /*var user = _dal.Volunteer.ReadAll()*/;
//            if (_dal.Volunteer == null || _dal.Volunteer. != password)
//            {
//                throw new DalDoesNotExistException("הסיסמה שגויה או שהמשתמש לא קיים.");
//            }
//            return user.Position;
//        }

//        public List<VolunteerInList> GetVolunteersList(bool? isActive, VolunteerSortField? sortField)
//        {
//            var volunteers = _dataSource.GetAllVolunteers();

//            // סינון לפי סטטוס מתנדב (פעיל / לא פעיל)
//            if (isActive.HasValue)
//            {
//                volunteers = volunteers.Where(v => v.IsActive == isActive.Value).ToList();
//            }

//            // מיון לפי שדה מסוים (ת.ז או שדה אחר)
//            if (sortField.HasValue)
//            {
//                switch (sortField.Value)
//                {
//                    case VolunteerSortField.Taz:
//                        volunteers = volunteers.OrderBy(v => v.Taz).ToList();
//                        break;
//                        // הוסף מקרים נוספים אם צריך למיין לפי שדות אחרים
//                }
//            }

//            return volunteers;
//        }

//        public Volunteer GetVolunteerDetails(int id)
//        {
//            var volunteer = _dataSource.GetVolunteerByTaz(id);
//            if (volunteer == null)
//            {
//                throw new VolunteerNotFoundException($"לא נמצא מתנדב עם ת.ז {id}");
//            }
//            return volunteer;
//        }

//        public void UpdateVolunteerDetails(int id, Volunteer updatedVolunteer)
//        {
//            var existingVolunteer = _dataSource.GetVolunteerByTaz(id);
//            if (existingVolunteer == null)
//            {
//                throw new VolunteerNotFoundException($"לא נמצא מתנדב עם ת.ז {id}");
//            }

//            // תבדוק אם המתנדב יכול לעדכן את הערכים (המנהל או המתנדב עצמו)
//            if (updatedVolunteer.Taz != id)
//            {
//                throw new UnauthorizedAccessException("אינך מורשה לעדכן את המתנדב הזה.");
//            }

//            // כאן תוכל להוסיף בדיקות פורמט של הערכים לפני עדכון

//            _dataSource.UpdateVolunteer(id, updatedVolunteer);
//        }

//        public void DeleteVolunteer(int id)
//        {
//            var volunteer = _dataSource.GetVolunteerByTaz(id);
//            if (volunteer == null)
//            {
//                throw new VolunteerNotFoundException($"לא נמצא מתנדב עם ת.ז {id}");
//            }

//            if (volunteer.IsHandlingCalls)
//            {
//                throw new VolunteerCannotBeDeletedException("לא ניתן למחוק את המתנדב כי הוא מטפל בקריאה.");
//            }

//            _dataSource.DeleteVolunteer(id);
//        }

//        public void AddVolunteer(Volunteer newVolunteer)
//        {
//            // כאן תוכל לבדוק את תקינות הערכים כמו בפונקציות הקודמות
//            if (_dataSource.GetVolunteerByTaz(newVolunteer.Taz) != null)
//            {
//                throw new VolunteerAlreadyExistsException("מתנדב עם ת.ז זו כבר קיים.");
//            }

//            _dataSource.AddVolunteer(newVolunteer);
//        }
//    }
//}

//internal class VolunteerImplementation:IVolunteer
//{
//    // Implementation of IVolunteer interface

//        /// <summary>
//        /// Logs in a user by validating the username and password.
//        /// If valid, returns the user's role, e.g., "Admin".
//        /// Throws an exception if credentials are incorrect.
//        /// </summary>
//        public string Login(string username, string password)
//        {
//            // Check user credentials from the data layer (simulated here)
//            if (username=)
//            {
//                return "Admin"; // Example, return the role of the user
//            }
//            else
//            {
//                throw new Exception("Invalid username or password.");
//            }
//        }

//        /// <summary>
//        /// Requests a list of volunteers, filtered by active status (optional) and sorted by a specified field (optional).
//        /// </summary>
//        public List<VolunteerInList> RequestVolunteerList(bool? isActive, VolunteerField? sortField)
//        {
//            // Fetch data from the data layer (simulated here)
//            List<VolunteerInList> volunteers = new List<VolunteerInList>
//        {
//            new VolunteerInList { Id = 1, Name = "John Doe", IsActive = true },
//            new VolunteerInList { Id = 2, Name = "Jane Smith", IsActive = false }
//        };

//            // Filter based on isActive (if provided)
//            if (isActive.HasValue)
//            {
//                volunteers = volunteers.Where(v => v.IsActive == isActive.Value).ToList();
//            }

//            // Sort the list based on the sortField (if provided)
//            if (sortField.HasValue)
//            {
//                switch (sortField.Value)
//                {
//                    case VolunteerField.Name:
//                        volunteers = volunteers.OrderBy(v => v.Name).ToList();
//                        break;
//                    case VolunteerField.Id:
//                        volunteers = volunteers.OrderBy(v => v.Id).ToList();
//                        break;
//                }
//            }

//            return volunteers;
//        }

//        /// <summary>
//        /// Requests detailed information of a volunteer based on their ID.
//        /// </summary>
//        public Volunteer RequestVolunteerDetails(int volunteerId)
//        {
//            // Simulate fetching volunteer details from the data layer
//            if (volunteerId == 1)
//            {
//                return new Volunteer { Id = 1, Name = "John Doe", Phone = "1234567890" };
//            }
//            else
//            {
//                throw new Exception("Volunteer not found.");
//            }
//        }

//        /// <summary>
//        /// Updates the details of an existing volunteer.
//        /// Validates input and applies changes to the volunteer's data.
//        /// </summary>
//        public void UpdateVolunteerDetails(int volunteerId, Volunteer volunteerDetails)
//        {
//            // Simulate updating volunteer details in the data layer
//            if (volunteerId == volunteerDetails.Id)
//            {
//                // Perform validation (e.g., email format, ID number)
//                if (string.IsNullOrEmpty(volunteerDetails.Name))
//                {
//                    throw new Exception("Invalid volunteer data.");
//                }

//                // Assume successful update in the database
//                Console.WriteLine("Volunteer details updated successfully.");
//            }
//            else
//            {
//                throw new Exception("Volunteer not found.");
//            }
//        }

//        /// <summary>
//        /// Deletes a volunteer from the system by their ID.
//        /// If the volunteer is handling a call, deletion will not be allowed.
//        /// </summary>
//        public void DeleteVolunteer(int volunteerId)
//        {
//            // Simulate checking if the volunteer can be deleted
//            if (volunteerId == 1) // Example, assume volunteer ID 1 is active
//            {
//                throw new Exception("Volunteer is currently handling a call and cannot be deleted.");
//            }

//            // Perform deletion from the data layer (simulated here)
//            Console.WriteLine("Volunteer deleted successfully.");
//        }

//        /// <summary>
//        /// Adds a new volunteer to the system.
//        /// Validates the input data and inserts it into the system.
//        /// </summary>
//        public void AddVolunteer(Volunteer volunteerDetails)
//        {
//            // Validate the volunteer details
//            if (string.IsNullOrEmpty(volunteerDetails.Name))
//            {
//                throw new Exception("Invalid volunteer data.");
//            }

//            // Simulate adding volunteer to the data layer
//            Console.WriteLine("Volunteer added successfully.");
//        }
//    }


