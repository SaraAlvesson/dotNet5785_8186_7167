namespace BlImplementation;
using BlApi;
using BO;
using DO;
using Helpers;
using System.Collections.Generic;
using System.Text;
using static BO.Exceptions;

internal class VolunteerImplementation : IVolunteer

{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    //public string Login(string username, string password)
    //{

    //    var volunteer = _dal.Volunteer.ReadAll(v => v.FullName == username).FirstOrDefault();
    //    if (volunteer != null)
    //    {
    //        if (volunteer.Password != password)
    //            throw new BlPasswordNotValid("Incorrect password");

    //        return ((Enums.VolunteerTypeEnum)volunteer.Position).ToString();
    //    }
    //    throw new BlDoesNotExistException($"Username {username} not found");

    //}
    public string Login(string username, string password)
    {
        // שלב 1: שליפת המתנדב עם שם משתמש (יכול להיות גם לפי מזהה ייחודי)
        var volunteer = _dal.Volunteer.ReadAll(v => v.FullName == username).FirstOrDefault(); // שים לב כאן שהשתמשתי ב-Username במקום FullName

        if (volunteer != null)
        {
            // שלב 2: השוואת הסיסמאות
            if (volunteer.Password != password) // יש להשתמש במתודה השוואה שמבצעת הצפנה
                throw new BlPasswordNotValid("Incorrect password");

            // שלב 3: החזרת תפקיד המשתמש
            return ((Enums.VolunteerTypeEnum)volunteer.Position).ToString();
        }

        // שלב 4: אם המשתמש לא נמצא
        throw new BlDoesNotExistException($"Username {username} not found");
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



    //public BO.Volunteer RequestVolunteerDetails(int volunteerId)
    //{
    //    try
    //    {
    //        // שליפת פרטי המתנדב משכבת הנתונים
    //        DO.Volunteer volunteer = _dal.Volunteer.Read(volunteerId)
    //            ?? throw new BO.Exceptions.BlDoesNotExistException($"Volunteer with ID {volunteerId} not found.");

    //        // שליפת כל ההקצותות של המתנדב
    //        var assignments = _dal.assignment.ReadAll(a => a.VolunteerId == volunteerId);

    //        // שליפת קריאה בטיפול (אם קיימת)
    //        var activeAssignment = assignments.FirstOrDefault(a => a.FinishAppointmentType == null);
    //        var activeCall = activeAssignment != null ? _dal.call.Read(c => c.Id == activeAssignment.CallId) : null;

    //        // חישוב מספר הקריאות שהושלמו, בוטלו או פגו
    //        int sumCalls = assignments.Count(a=>a.VolunteerId==volunteerId && (BO.Enums.FinishAppointmentTypeEnum)a.FinishAppointmentType==BO.Enums.FinishAppointmentTypeEnum.WasTreated);
    //        int sumCanceled = assignments.Count(a => (BO.Enums.FinishAppointmentTypeEnum)a.FinishAppointmentType == BO.Enums.FinishAppointmentTypeEnum.SelfCancellation);
    //        int sumExpired = assignments.Count(a => (BO.Enums.FinishAppointmentTypeEnum)a.FinishAppointmentType == Enums.FinishAppointmentTypeEnum.CancellationHasExpired);

    //        // החזרת האובייקט הלוגי "מתנדב" עם קריאה בטיפול (אם קיימת) ונתוני הסיכום
    //        return new BO.Volunteer()
    //        {
    //            Id = volunteer.Id,
    //            FullName = volunteer.FullName,
    //            PhoneNumber = volunteer.PhoneNumber,
    //            Email = volunteer.Email,
    //            Password = volunteer.Password,
    //            Location = volunteer.Location,
    //            Latitude = volunteer.Latitude,
    //            Longitude = volunteer.Longitude,
    //            Position = (BO.Enums.VolunteerTypeEnum)volunteer.Position,
    //            Active = volunteer.Active,
    //            MaxDistance = volunteer.MaxDistance,
    //            DistanceType = (BO.Enums.DistanceTypeEnum)volunteer.DistanceType,
    //            SumCalls = sumCalls, // מספר הקריאות
    //            SumCanceled = sumCanceled, // מספר הקריאות שבוטלו
    //            SumExpired = sumExpired, // מספר הקריאות שפגו

    //            // אם יש קריאה בטיפול, החזרת פרטי הקריאה
    //            VolunteerTakenCare = activeCall == null
    //                ? null
    //                : new BO.CallInProgress
    //                {
    //                    Id = activeCall.Id,
    //                    CallId = activeCall.Id,
    //                    CallType = (BO.Enums.CallTypeEnum)activeCall.CallType, // המרה ישירה ל-Enum
    //                    VerbDesc = activeCall.VerbDesc,
    //                    CallAddress = activeCall.Adress, // תיקון שם שדה אם צריך
    //                    OpenTime = activeCall.OpenTime,
    //                    MaxFinishTime = (DateTime)activeCall.MaxTime,
    //                    StartAppointmentTime = activeCall.OpenTime,
    //                    DistanceOfCall = activeCall.Latitude, // כאן ניתן לשלב את החישוב של המרחק אם יש צורך
    //                }
    //        };
    //    }
    //    catch (DO.DalDoesNotExistException ex)
    //    {
    //        // טיפול בחריגה במקרה שהמתנדב לא נמצא בשכבת הנתונים
    //        throw new BlDoesNotExistException("Error retrieving volunteer details.", ex);
    //    }
    //}
    public BO.Volunteer RequestVolunteerDetails(int volunteerId)
    {
        try
        {
            // שליפת פרטי המתנדב משכבת הנתונים
            DO.Volunteer volunteer = _dal.Volunteer.Read(volunteerId)
                ?? throw new BO.Exceptions.BlDoesNotExistException($"Volunteer with ID {volunteerId} not found.");

            // שליפת כל ההקצותות של המתנדב
            var assignments = _dal.assignment.ReadAll(a => a.VolunteerId == volunteerId);

            // שליפת קריאה בטיפול (אם קיימת)
            var activeAssignment = assignments.FirstOrDefault(a => a.FinishAppointmentType == null);
            var activeCall = activeAssignment != null ? _dal.call.Read(c => c.Id == activeAssignment.CallId) : null;

            // חישוב מספר הקריאות שהושלמו, בוטלו או פגו
            int sumCalls = assignments.Count(a => a.VolunteerId == volunteerId && (BO.Enums.FinishAppointmentTypeEnum)a.FinishAppointmentType == BO.Enums.FinishAppointmentTypeEnum.WasTreated);
            int sumCanceled = assignments.Count(a => (BO.Enums.FinishAppointmentTypeEnum)a.FinishAppointmentType == BO.Enums.FinishAppointmentTypeEnum.SelfCancellation);
            int sumExpired = assignments.Count(a => (BO.Enums.FinishAppointmentTypeEnum)a.FinishAppointmentType == Enums.FinishAppointmentTypeEnum.CancellationHasExpired);
            // החזרת האובייקט הלוגי "מתנדב" עם קריאה בטיפול (אם קיימת) ונתוני הסיכום
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
                SumCalls = sumCalls, // מספר הקריאות
                SumCanceled = sumCanceled, // מספר הקריאות שבוטלו
                SumExpired = sumExpired, // מספר הקריאות שפגו

                // אם יש קריאה בטיפול, החזרת פרטי הקריאה
                VolunteerTakenCare = activeCall == null
                    ? null
                    : new BO.CallInProgress
                    {
                        Id = activeCall.Id,
                        CallId = activeCall.Id,
                        CallType = (BO.Enums.CallTypeEnum)activeCall.CallType, // המרה ישירה ל-Enum
                        VerbDesc = activeCall.VerbDesc,
                        CallAddress = activeCall.Adress, // תיקון שם שדה אם צריך
                        OpenTime = activeCall.OpenTime,
                        MaxFinishTime = (DateTime)activeCall.MaxTime,
                        StartAppointmentTime = activeAssignment.AppointmentTime, // חיבור למועד ההתחלה של ההקצאה
                        DistanceOfCall = Tools.CalculateDistance(activeCall.Latitude, activeCall.Longitude, volunteer.Latitude ?? 0, volunteer.Longitude ?? 0),
                    }
            };
        }
        catch (DO.DalDoesNotExistException ex)
        {
            // טיפול בחריגה במקרה שהמתנדב לא נמצא בשכבת הנתונים
            throw new BO.Exceptions.BlDoesNotExistException("Error retrieving volunteer details.", ex);
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
        try
        {
            // שלב 1: בקשת רשומת המתנדב משכבת הנתונים
            var existingVolunteer = _dal.Volunteer.ReadAll(v => v.Id == volunteerDetails.Id).FirstOrDefault()
                ?? throw new DalDoesNotExistException($"Volunteer with ID {volunteerDetails.Id} not found.");

            // שלב 2: בדיקה אם המבקש לעדכן הוא המנהל או המתנדב עצמו
            if (requesterId != volunteerDetails.Id && !IsAdmin(requesterId))
            {
                throw new UnauthorizedAccessException("Unauthorized to update volunteer details.");
            }

            // שלב 3: בדיקת ערכים מבחינת פורמט
            if (!Helpers.VolunteersManager.checkVolunteerEmail(volunteerDetails))
                throw new BlEmailNotCorrect("Invalid email format.");
            if (!Helpers.VolunteersManager.IsPhoneNumberValid(volunteerDetails))
                throw new BlPhoneNumberNotCorrect("Invalid phone number format.");
            if (!Helpers.VolunteersManager.IsValidId(volunteerDetails.Id))
                throw new BlIdNotValid("Invalid ID format.");

            // שלב 4: בדיקה לוגית של הערכים
            if (volunteerDetails.Latitude == null || volunteerDetails.Longitude == null)
                throw new BlInvalidLocationException("Location must include valid latitude and longitude.");

            // שלב 5: בדיקה אם מותר לשנות תפקיד
            if ((Enums.VolunteerTypeEnum)existingVolunteer.Position != volunteerDetails.Position && !IsAdmin(volunteerDetails.Id))
                throw new BlUnauthorizedAccessException("Only admins can update the position.");

            // שלב 6: העברת נתונים מ-BO ל-DO
            DO.Volunteer newVolunteer = new DO.Volunteer
            {
                Id = volunteerDetails.Id,
                FullName = volunteerDetails.FullName,
                PhoneNumber = volunteerDetails.PhoneNumber,
                Email = volunteerDetails.Email,
                Active = volunteerDetails.Active,
                DistanceType = (DO.DistanceType)volunteerDetails.DistanceType,
                Position = (DO.Position)volunteerDetails.Position,
                Latitude = volunteerDetails.Latitude,
                Longitude = volunteerDetails.Longitude,
                MaxDistance = volunteerDetails.MaxDistance,
            };

            // שלב 7: עדכון רשומת המתנדב בשכבת הנתונים
            _dal.Volunteer.Update(newVolunteer);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BlDoesNotExistException("Error updating volunteer details.", ex);
        }
        catch (Exception ex)
        {
            // טיפול כללי בחריגות
            throw new CannotUpdateVolunteerException("An error occurred while updating the volunteer details.", ex);
        }
    }


    private bool IsAdmin(int id)
    {
        var volunteer = _dal.Volunteer.Read(v => v.Id == id);
        if (volunteer.Position == Position.Manager)
            return true;
        return false;
    }




    public void DeleteVolunteer(int volunteerId)
    {
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
        }
        catch (DO.DalDoesNotExistException ex)
        {
            // טיפול בחריגה במקרה של מתנדב שלא נמצא
            throw new BlDoesNotExistException($"Volunteer with id {volunteerId} not found.", ex);
        }
    }





    public void AddVolunteer(BO.Volunteer volunteer)
    {

        if (!(Helpers.VolunteersManager.checkVolunteerEmail(volunteer)))
            throw new BlEmailNotCorrect("Invalid Email format.");
        if (!(Helpers.VolunteersManager.IsValidId(volunteer.Id)))
            throw new BlIdNotValid("Invalid ID format.");
        if (!(Helpers.VolunteersManager.IsPhoneNumberValid(volunteer)))
            throw new BlPhoneNumberNotCorrect("Invalid PhoneNumber format.");
        DO.Volunteer newVolunteer = new()

        {
            FullName = volunteer.FullName,
            PhoneNumber = volunteer.PhoneNumber,
            Email = volunteer.Email,
            Active = volunteer.Active,
            DistanceType = (DO.DistanceType)volunteer.DistanceType,
            Position = (DO.Position)volunteer.Position,
            Latitude = volunteer.Latitude,
            Longitude = volunteer.Longitude,
            MaxDistance = volunteer.MaxDistance,

        };
        try
        {
            _dal.Volunteer.Create(newVolunteer);
        }
        catch (DO.DalAlreadyExistException ex)
        {
            throw new BLAlreadyExistException($"Volunteer with id {volunteer.Id} already exists");
        }

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
//        }
//    }
