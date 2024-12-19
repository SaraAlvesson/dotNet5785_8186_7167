
namespace BlImplementation;
using BlApi;
using BO;
using DO;

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
        if (isActive == null && sortField == null)
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
  

    public BO.Volunteer RequestVolunteerDetails(int volunteerId)
    {
        try
        {
            // שליפת פרטי מתנדב משכבת הנתונים
            //var volunteer = _dal.Volunteer.Read(volunteerId)
            //    ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} not found.");
            DO.Volunteer volunteer = _dal.Volunteer.Read(volunteerId) ?? throw new BO.Exceptions.BlDoesNotExistException($"Volunteer with ID {volunteerId} not found.");
            // שליפת קריאה בטיפול, אם קיימת
            var call = _dal.call.ReadAll().FirstOrDefault(c => c.Id == volunteerId);
            //var assignment = _dal.assignment.ReadAll().FirstOrDefault(c => c.FinishAppointmentType = null

            // החזרת האובייקט הלוגי "מתנדב" עם קריאה בטיפול (אם קיימת)
            return new BO.Volunteer()
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
                        CallAddress = call.Address, // תיקון שם שדה אם צריך
                        OpenTime = call.OpenTime,
                        MaxFinishTime = (DateTime)call.MaxTime,
                        StartAppointmentTime = call.OpenTime,
                        DistanceOfCall = call.Latitude,////longtitude and latitude
                        Status = (Enums.CallStatusEnum)Assignment.
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
            ?? throw new KeyNotaFoundException("Volunteer not found.");

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
//namespace BlImplementation;
//    using BlApi;
//using BO;
//    internal class VolunteerImplementation : IVolunteer
//    {
//        private readonly DalApi.IDal _dal = DalApi.Factory.Get;

//        // פונקציה לביצוע התחברות של המתנדב לפי שם משתמש וסיסמה
//        public string Login(string username, string password)
//        {
//            var volunteer = _dal.Volunteer.ReadAll(v => v.FullName == username).FirstOrDefault();
//            if (volunteer != null)
//            {
//                if (volunteer.Password != password)
//                    throw new ArgumentException("Incorrect password");

//                return ((Enums.VolunteerTypeEnum)volunteer.Position).ToString();
//            }
//            throw new ArgumentException($"Username {username} not found");
//        }

//        // פונקציה להחזרת רשימה של מתנדבים, עם סינונים ואופציות מיון
//        public IEnumerable<BO.VolunteerInList> RequestVolunteerList(bool? isActive = null, VolunteerInList? sortField = null)
//        {
//            var volunteers = _dal.Volunteer.ReadAll()
//                .Select(v => new BO.VolunteerInList
//                {
//                    Id = v.Id,
//                    FullName = v.FullName,
//                    Active = v.Active,
//                }).AsQueryable();

//            // סינון לפי סטטוס פעיל אם הוגדר
//            if (isActive.HasValue)
//            {
//                volunteers = volunteers.Where(v => v.Active == isActive.Value);
//            }

//            // מיון לפי השדה המבוקש
//            if (sortField.HasValue)
//            {
//                switch (sortField.Value)
//                {
//                    case VolunteerInList.FullName:
//                        volunteers = volunteers.OrderBy(v => v.FullName);
//                        break;
//                    case VolunteerInList.Active:
//                        volunteers = volunteers.OrderBy(v => v.Active);
//                        break;
//                        // אפשר להוסיף מקרים נוספים לפי שדות נוספים של המתנדב
//                }
//            }

//            return volunteers.ToList();
//        }

//        // פונקציה לבקשת פרטי מתנדב ספציפי לפי מזהה
//        public BO.Volunteer RequestVolunteerDetails(int volunteerId)
//        {
//            var volunteer = _dal.Volunteer.ReadAll(v => v.Id == volunteerId).FirstOrDefault();
//            if (volunteer == null)
//                throw new ArgumentException($"Volunteer with id {volunteerId} not found");

//            return new BO.Volunteer
//            {
//                Id = volunteer.Id,
//                FullName = volunteer.FullName,
//                Position = (Enums.VolunteerTypeEnum)volunteer.Position,
//                Active = volunteer.Active
//            };
//        }

//        // פונקציה לעדכון פרטי מתנדב
//        public void UpdateVolunteerDetails(BO.Volunteer volunteer)
//        {
//            var existingVolunteer = _dal.Volunteer.ReadAll(v => v.Id == volunteer.Id).FirstOrDefault();
//            if (existingVolunteer == null)
//                throw new ArgumentException($"Volunteer with id {volunteer.Id} not found");

//            // עדכון פרטי המתנדב במערכת
//            existingVolunteer.FullName = volunteer.FullName;
//            existingVolunteer.Position = (DalApi.Enums.VolunteerTypeEnum)volunteer.Position;
//            existingVolunteer.Active = volunteer.Active;

//            _dal.Volunteer.Update(existingVolunteer);
//        }

//   public void DeleteVolunteer(int volunteerId)
//        {
//            var volunteer = _dal.Volunteers.FirstOrDefault(v => v.Id == id)
//                ?? throw new KeyNotaFoundException("Volunteer not found.");

//            if (_dal.Calls.Any(c => c.VolunteerId == id))
//                throw new InvalidOperationException("Cannot delete a volunteer with active or past calls.");

//            _dal.Volunteers.Remove(volunteer);
//        }

//        private void ValidateVolunteer(Volunteer volunteer)
//        {
//            if (string.IsNullOrEmpty(volunteer.FullName) || string.IsNullOrEmpty(volunteer.Email))
//                throw new ArgumentException("Volunteer name and email must not be empty.");
//        }

//        private bool IsAdmin(int id)
//        {
//            var volunteer = _dal.Volunteers.FirstOrDefault(v => v.Id == id);
//            return volunteer?.Role == "Admin";
//        }
//        public void AddVolunteer(Volunteer volunteerDetails)
//        {

//        }
//    }


//namespace BlImplementation;
//    using BlApi;
//    using BO;

//    internal class VolunteerImplementation : IVolunteer
//    {
//        private readonly DalApi.IDal _dal = DalApi.Factory.Get;

//    // פונקציה לביצוע התחברות של מתנדב עם שם משתמש וסיסמה
//    public string Login(string username, string password)
//    {

//        var volunteer = _dal.Volunteer.ReadAll(v => v.FullName == username).FirstOrDefault();
//        if (volunteer != null)
//        {
//            if (volunteer.Password != password)
//                throw new ArgumentException("Incorrect password");

//            return ((Enums.VolunteerTypeEnum)volunteer.Position).ToString();
//        }
//        throw new ArgumentException($"Username {username} not found");

//    }
//    //פונקציה להחזרת רשימת מתנדבים, עם אפשרות לסינון לפי מצב פעיל ומיון
//    public IEnumerable<VolunteerInList> RequestVolunteerList(bool? isActive = null, VolunteerInList? sortField = null)
//    {
//        var volunteers = _dal.Volunteer.ReadAll()
//            .Select(v => new VolunteerInList
//            {
//                Id = v.Id,
//                FullName = v.FullName,
//                Active = v.Active,
//            }).AsQueryable();

//        // סינון לפי מצב פעיל אם נתון
//        if (isActive.HasValue)
//        {
//            volunteers = volunteers.Where(v => v.Active == isActive.Value);
//        }

//        // מיון לפי השדה המבוקש
//        if (sortField.HasValue)
//        {
//            switch (sortField.Value)
//            {
//                case VolunteerInList.FullName:
//                    volunteers = volunteers.OrderBy(v => v.FullName);
//                    break;
//                case VolunteerInList.Active:
//                    volunteers = volunteers.OrderBy(v => v.Active);
//                    break;
//                    // ניתן להוסיף יותר מקרים למיון לפי שדות נוספים
//            }
//        }

//        return volunteers.ToList();
//    }



//   // פונקציה לבקשה לפרטי מתנדב לפי מזהה
//     public BO.Volunteer RequestVolunteerDetails(int volunteerId)
//    {
//        try
//        {
//            // שליפת פרטי מתנדב משכבת הנתונים
//            var volunteer = _dal.Volunteer.Read(volunteerId)
//                ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} not found.");

//            // שליפת קריאה בטיפול, אם קיימת
//            var call = _dal.call.ReadAll().FirstOrDefault(c => c.Id == volunteerId);

//            // החזרת האובייקט הלוגי "מתנדב" עם קריאה בטיפול (אם קיימת)
//            return new BO.Volunteer
//            {
//                Id = volunteer.Id,
//                FullName = volunteer.FullName,
//                Email = volunteer.Email,
//                VolunteerTakenCare = call == null
//                    ? null
//                    : new BO.CallInProgress
//                    {
//                        Id = call.Id,
//                        CallType = (BO.Enums.CallTypeEnum)call.CallType, // המרה ישירה ל-Enum
//                        VerbDesc = call.VerbDesc,
//                        CallAddress = call.Adress, // תיקון שם שדה אם צריך
//                        OpenTime = call.OpenTime,
//                        MaxFinishTime = (DateTime)call.MaxTime,
//                        StartAppointmentTime = call.OpenTime,
//                        DistanceOfCall = call.Latitude, //longtitude and latitude
//                        Status = ((BO.Enums.CallStatusEnum)(DO.FinishAppointmentType)Assignment.FinishAppointmentType).ToString()
//                    },
//            };
//        }
//        catch (Exception ex)
//        {
//            throw new ArgumentException("Error retrieving volunteer details.", ex);
//        }
//    }
//    //// פונקציה לבקשת פרטי מתנדב ספציפי לפי מזהה
//    // public BO.Volunteer RequestVolunteerDetails(int volunteerId)
//    // {
//    //     var volunteer = _dal.Volunteer.ReadAll(v => v.Id == volunteerId).FirstOrDefault();
//    //     if (volunteer == null)
//    //         throw new ArgumentException($"Volunteer with id {volunteerId} not found");

//    //     return new BO.Volunteer
//    //     {
//    //         Id = volunteer.Id,
//    //         FullName = volunteer.FullName,
//    //         Position = (Enums.VolunteerTypeEnum)volunteer.Position,
//    //         Active = volunteer.Active
//    //     };
//    // }

//    //  פונקציה לעדכון פרטי מתנדב
//    // פונקציה לעדכון פרטי מתנדב
//    public void UpdateVolunteerDetails(BO.Volunteer volunteer)
//    {
//        var existingVolunteer = _dal.Volunteer.ReadAll(v => v.Id == volunteer.Id).FirstOrDefault();
//        if (existingVolunteer == null)
//            throw new ArgumentException($"Volunteer with id {volunteer.Id} not found");

//        // עדכון פרטי המתנדב במערכת
//        existingVolunteer.Volunteer.FullName = volunteer.FullName;
//        existingVolunteer.Position = (DalApi.Enums.VolunteerTypeEnum)volunteer.Position;
//        existingVolunteer.Active = volunteer.Active;

//        _dal.Volunteer.Update(existingVolunteer);
//    }


//    //פונקציה למחיקת מתנדב
//    public void DeleteVolunteer(int volunteerId)
//        {
//            var volunteer = _dal.Volunteer.ReadAll(v => v.Id == volunteerId).FirstOrDefault();
//            if (volunteer == null)
//                throw new ArgumentException($"Volunteer with id {volunteerId} not found");

//            _dal.Volunteer.Delete(volunteerId);
//        }

//        // פונקציה להוספת מתנדב חדש
//        public void AddVolunteer(Volunteer volunteerDetails)
//        {
//            var newVolunteer = new DalApi.DTO.Volunteer
//            {
//                FullName = volunteerDetails.FullName,
//                Email = volunteerDetails.Email,
//                Active = volunteerDetails.Active,
//                Position = (int)volunteerDetails.Position,
//                Latitude = volunteerDetails.Latitude,
//                Longitude = volunteerDetails.Longitude
//            };

//            _dal.Volunteer.Create(newVolunteer);
//        }
//    }


