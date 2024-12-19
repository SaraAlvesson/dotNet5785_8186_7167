
namespace BlImplementation;
using BlApi;
using BO;
using Helpers;

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
            var volunteer = _dal.Volunteer.Read(volunteerId)
                ?? throw new Exception($"Volunteer with ID {volunteerId} not found.");

            // שליפת קריאה בטיפול, אם קיימת
            var call = _dal.call.ReadAll().FirstOrDefault(c => c.Id == volunteerId);
            var assignment = _dal.assignment.ReadAll().FirstOrDefault(a => a.FinishAppointmentType!=null);

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
                        CallAddress = call.Address, // תיקון שם שדה אם צריך
                        OpenTime = call.OpenTime,
                        MaxFinishTime = (DateTime)call.MaxTime,
                        StartAppointmentTime = call.OpenTime,
                        DistanceOfCall = call.Latitude,////longtitude and latitude
                        Status = (Enums.CallStatusEnum)assignment.FinishAppointmentType
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
                ?? throw new DalApi.DalConfigException("Volunteer not found.");

            // שלב 2: בדיקה אם המבקש לעדכן הוא המנהל או המתנדב עצמו
            if (volunteerDetails.Id != volunteerId && !IsAdmin(volunteerDetails.Id))
                throw new UnauthorizedAccessException("Unauthorized to update volunteer details.");

            // שלב 3: בדיקת ערכים מבחינת פורמט
            VolunteerManager.checkeVolunteerFormat(volunteerDetails);

            // שלב 4: בדיקת ערכים מבחינה לוגית (למשל, ת.ז תקינה, כתובת אמיתית וכו')
            VolunteerManager.checkeVolunteerlogic(volunteerDetails);

            // שלב 5: בדיקה אם מותר לשנות תפקיד (רק למנהל)
            if ((Enums.VolunteerTypeEnum)existingVolunteer.Position != volunteerDetails.Position && !IsAdmin(volunteerDetails.Id))
                throw new UnauthorizedAccessException("Only admins can update roles.");

            // שלב 6: עדכון קווי האורך והרוחב במקרה של שינוי כתובת
            if (volunteerDetails.Address != existingVolunteer.Location)
            {
                var coordinates = Tools.GetGeolocationCoordinates(volunteerDetails.Address); // מניחים שיש פונקציה כזו
                volunteerDetails.Latitude = coordinates[0];
                volunteerDetails.Longitude = coordinates[1];
            }

            // שלב 7: המרת BO.Volunteer ל-DO.Volunteer
            DO.Volunteer updatedVolunteer = VolunteerManager.convertFormBOVolunteerToDo(volunteerDetails);

            // שלב 8: עדכון הרשומה בשכבת הנתונים
            _dal.Volunteer.Update(updatedVolunteer);
        }
        catch (DalApi.DalConfigException ex)
        {
            // טיפול בחריגות וזריקתן מחדש עם מידע ברור לשכבת התצוגה
            throw new ArgumentException("Error updating volunteer details in data layer.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            // טיפול במקרה של הרשאות לא מספיקות
            throw new ArgumentException("Unauthorized access while updating volunteer details.", ex);
        }
        catch (Exception ex)
        {
            // טיפול בחריגות כלליות
            throw new ArgumentException("Unexpected error while updating volunteer details.", ex);
        }
    }

    private bool IsAdmin(int id)
    {
        var volunteer = _dal.Volunteer.ReadAll(v => v.Id == id).FirstOrDefault();
        return ((Enums.VolunteerTypeEnum)volunteer.Position).ToString() == "ADMIN";
    }
    public void DeleteVolunteer(int volunteerId)
    {
        // חיפוש המתנדב על פי ת.ז.
        var volunteer = _dal.Volunteer.ReadAll(v => v.Id == volunteerId).FirstOrDefault();

        // אם המתנדב לא נמצא, זרוק חריגה מתאימה
        if (volunteer == null)
            throw new ArgumentException($"Volunteer with id {volunteerId} not found");

        // בדוק אם המתנדב לא טיפל בשום קריאה (מטפל או טיפל)
        var activeCalls = _dal.Volunteer.ReadAll(v => v.Id == volunteerId && v.Active == true);
        if (activeCalls.Any())  // אם יש קריאות שלא נסגרו
        {
            throw new InvalidOperationException($"Volunteer with id {volunteerId} is currently handling calls and cannot be deleted.");
        }

        // אם המתנדב לא מטפל כרגע בקריאות, מחק אותו
        _dal.Volunteer.Delete(volunteerId);
    }

    //  פונקציה להוספת מתנדב חדש
    public void AddVolunteer(BO.Volunteer volunteerDetails)
    {
        // בדוק תקינות הערכים מבחינת פורמט לוגי ופורמטי
        if (string.IsNullOrEmpty(volunteerDetails.FullName))
            throw new ArgumentException("Full name is required.");

        VolunteerManager.checkeVolunteerFormat(volunteerDetails);
        // יצירת אובייקט חדש מטיפוס ישות נתונים (DalApi.Volunteer)
        DO.Volunteer v = VolunteerManager.convertFormBOVolunteerToDo(volunteerDetails);

        // בדוק אם כבר קיים מתנדב עם ת.ז. זהה
        var existingVolunteer = _dal.Volunteer.ReadAll(v => v.Id == volunteerDetails.Id).FirstOrDefault();
        if (existingVolunteer != null)
            throw new InvalidOperationException($"Volunteer with id {volunteerDetails.Id} already exists.");

        // הוסף את המתנדב לשכבת הנתונים
        _dal.Volunteer.Create(v);
    }

}






