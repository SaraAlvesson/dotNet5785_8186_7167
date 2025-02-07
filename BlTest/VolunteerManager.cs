using BlApi;
using BlImplementation;
using BO;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static BO.Enums;
using System.Web;
using static BO.Exceptions;
using static Helpers.Tools;

namespace Helpers
{
    internal static class VolunteersManager
    {
        internal static ObserverManager Observers = new(); //stage 5 

        private static IDal s_dal = DalApi.Factory.Get; //stage 4

        internal static BO.CallInProgress? GetCallIn(DO.Volunteer doVolunteer)
        {
            lock (AdminManager.BlMutex) //stage 7
            {
                var call = s_dal.assignment.ReadAll(ass => ass.VolunteerId == doVolunteer.Id).ToList();
                DO.Assignment? assignmentTreat = call.Find(ass => ass.FinishAppointmentType == null);

                if (assignmentTreat != null)
                {
                    DO.Call? callTreat = s_dal.call.Read(c => c.Id == assignmentTreat.CallId);

                    if (callTreat != null)
                    {
                        double latitude = (double)(doVolunteer.Latitude ?? callTreat.Latitude);
                        double longitude = (double)(doVolunteer.Longitude ?? callTreat.Longitude);
                        return new()
                        {
                            Id = assignmentTreat.Id,
                            CallId = assignmentTreat.CallId,
                            CallType = (BO.Enums.CallTypeEnum)callTreat.CallType,
                            VerbDesc = callTreat.VerbDesc,
                            CallAddress = callTreat.Adress,
                            OpenTime = callTreat.OpenTime,
                            MaxFinishTime = callTreat.MaxTime,
                            StartAppointmentTime = assignmentTreat.AppointmentTime,
                            DistanceOfCall = Tools.CalculateDistance((double)callTreat.Latitude, (double)callTreat.Longitude, latitude, longitude),
                            //Status = (callTreat.MaxTime - ClockManager.Now <= s_dal.? BO.Enums.CalltStatusEnum.CallTreatmentAlmostOver : BO.Enums.CalltStatusEnum.CallIsBeingTreated),
                        };
                    }
                }
                return null;
            }
        }

        internal static bool IsPhoneNumberValid(BO.Volunteer volunteer)
        {
            // וידוא שלא מדובר במחרוזת ריקה או null
            if (string.IsNullOrEmpty(volunteer.PhoneNumber))
                throw new BO.Exceptions.BlPhoneNumberNotCorrect($"The PhoneNumber: {volunteer.PhoneNumber} is incorrect (empty or null).");

            // וידוא שכל התווים הם ספרות בלבד
            if (!volunteer.PhoneNumber.All(char.IsDigit))
                throw new BO.Exceptions.BlPhoneNumberNotCorrect($"The PhoneNumber: {volunteer.PhoneNumber} contains invalid characters.");

            // וידוא שהאורך מתאים
            if (volunteer.PhoneNumber.Length < 10 || volunteer.PhoneNumber.Length > 10)
                throw new BO.Exceptions.BlPhoneNumberNotCorrect($"The PhoneNumber: {volunteer.PhoneNumber} has an invalid length.");

            // אם כל הבדיקות עברו בהצלחה
            return true;
        }

        internal static bool checkVolunteerEmail(BO.Volunteer volunteer)
        {
            if (string.IsNullOrEmpty(volunteer.Email) || volunteer.Email.Count(c => c == '@') != 1)
            {
                throw new BO.Exceptions.BlEmailNotCorrect($"email :{volunteer.Email} Is incorrect ");
            }

            // תבנית המייל שתומכת בסיומות ישראליות וגם סיומות בינלאומיות
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.(co\.il|gov\.il|ac\.il|org\.il|net\.il|k12\.il|muni\.il|com|org|net|edu|info|biz|us|uk|ca|de|fr|jp|au|in|cn|ru)$";

            if (!Regex.IsMatch(volunteer.Email, pattern))
            {
                throw new BO.Exceptions.BlEmailNotCorrect($"email :{volunteer.Email} Is incorrect ");
            }
            else
            {
                return true;
            }
        }

        internal static bool IsValidId(int id)
        {
            // בדיקה אם תעודת הזהות היא בת 9 ספרות
            if (id < 100000000 || id > 999999999)
            {
                return false;
            }

            // אם ה-ID תקין, מחזירים true
            return true;
        }

        // A constant shift value for encryption and decryption (simple Caesar Cipher)
        private static readonly int shift = 3;

        /// <summary>
        /// Encrypts the given password by shifting each character's ASCII value by the specified shift.
        /// </summary>
        /// <param name="password">The password to be encrypted.</param>
        /// <returns>The encrypted password as a string.</returns>
        /// <remarks>
        /// This function was created with the assistance of ChatGPT, a language model developed by OpenAI.
        /// </remarks>

        /// <summary>
        /// Decrypts the given encrypted password by reversing the encryption process.
        /// </summary>
        /// <param name="encryptedPassword">The encrypted password to be decrypted.</param>
        /// <returns>The decrypted (original) password as a string.</returns>
        /// <remarks>
        /// This function was created with the assistance of ChatGPT, a language model developed by OpenAI.
        /// </remarks>
        public static string Decrypt(string encryptedPassword)
        {
            // Convert the encrypted password string into a character array for manipulation
            char[] buffer = encryptedPassword.ToCharArray();

            // Loop through each character in the encrypted password
            for (int i = 0; i < buffer.Length; i++)
            {
                // Get the current character
                char c = buffer[i];

                // Reverse the shift of the ASCII value by subtracting the constant shift value
                buffer[i] = (char)((int)c - shift);
            }

            // Return the decrypted password as a new string
            return new string(buffer);
        }

        /// <summary>
        /// Checks if the password is at least 6 characters, contains an uppercase letter and a digit.
        /// </summary>
        /// <param name="password">Password to check</param>
        /// <returns>true if strong, false otherwise</returns>
        /// <remarks>Written with the help of ChatGPT (https://openai.com)</remarks>
    
#region Simulation

// Static random number generator used throughout the simulation
private static readonly Random s_rand = new();

        // Counter to track the number of simulator threads
        private static int s_simulatorCounter = 0;

        // Counter to randomly choose volunteers every iteration
        private static int s_Counter = 0; // for the func to choose completely random volunteers every iteration

        /// <summary>
        /// Simulates the assignment of volunteers to calls. It processes all active volunteers and assigns them calls.
        /// </summary>
        internal static void SimulateAssignForVolunteer()
        {
            // Set the thread's name for identification purposes
            Thread.CurrentThread.Name = $"Simulator{++s_simulatorCounter}";

            // Declare a list to store active volunteers
            IEnumerable<DO.Volunteer> DoVolList;

            // Locking the critical section to ensure thread safety while accessing the volunteer data
            lock (AdminManager.BlMutex) //stage 7
                DoVolList = s_dal.Volunteer.ReadAll(v => v.Active == true).ToList();

            // Loop through each active volunteer
            foreach (DO.Volunteer doVolunteer in DoVolList)
            {
                // Check if the volunteer currently has an ongoing call
                CallInProgress? currentCall = GetCallIn(doVolunteer);

                // Increment the global counter for tracking iterations
                s_Counter++;

                // If the volunteer doesn't have an active call
                if (currentCall == null)
                {
                    // Every 3rd iteration, try to assign an open call to the volunteer
                    if (s_Counter % 3 == 0)
                    {
                        // Get the list of open calls for the volunteer
                        var availableCalls = CallManager.GetOpenCallInLists(doVolunteer.Id, null, null);
                        int cntOpenCall = availableCalls.Count();

                        // If there are open calls available, randomly choose one to assign
                        if (cntOpenCall != 0)
                        {
                            int callId = availableCalls.Skip(s_rand.Next(0, cntOpenCall)).First()!.Id;
                            // Assign the selected call to the volunteer
                            CallManager.AssignCallToVolunteer(doVolunteer.Id, callId);
                        }
                    }
                }
                else
                {
                    // Calculate the maximum time the volunteer can be busy with the current call
                    DateTime maxTime = currentCall.OpenTime.AddHours(currentCall.DistanceOfCall).AddMonths(1);

                    // If the current time exceeds the maximum time, end the treatment
                    if (AdminManager.Now > maxTime)
                    {
                        CallManager.UpdateCallAsCompleted(doVolunteer.Id, currentCall.Id);
                    }
                    // Every 10th iteration, cancel the treatment for the current volunteer
                    else if (s_Counter % 10 == 0)
                    {
                        CallManager.UpdateToCancelCallTreatment(doVolunteer.Id, currentCall.Id);
                    }
                }
            }
        }
    #endregion

  
        #region Implementation
        public static string Login(int username, string password)
        {
            try
            {
                // עטיפת שליפת המתנדב בבלוק נעילה כדי להבטיח שלמות הנתונים
                lock (AdminManager.BlMutex)
                {
                    // שלב 1: שליפת המתנדב לפי מזהה
                    var volunteer = s_dal.Volunteer.ReadAll(v => v.Id == username).FirstOrDefault();

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
                }

                // שלב 5: אם המשתמש לא נמצא
                throw new BlDoesNotExistException($"Username {username} not found");
            }
            catch (Exception ex)
            {
                // טיפול בחריגות
                throw new Exception("An error occurred during the login process: " + ex.Message, ex);
            }
        }

        public static IEnumerable<VolunteerInList> RequestVolunteerList(
         bool? isActive,
         VolunteerInListField? sortField = null,
         CallTypeEnum? callTypeFilter = null)  // הוספת פילטר לסוג קריאה
        {
            try
            {
                IEnumerable<DO.Volunteer> volunteers;

                // נעילה סביב קריאה ל-DAL
                lock (AdminManager.BlMutex)
                {
                    volunteers = s_dal.Volunteer.ReadAll();
                }

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



        public static BO.Volunteer RequestVolunteerDetails(int volunteerId)
        {
            try
            {
                // עטיפת כל הפניות ל-DAL ב-lock
                lock (AdminManager.BlMutex)
                {
                    // שליפת פרטי המתנדב
                    DO.Volunteer volunteer = s_dal.Volunteer.Read(volunteerId)
                        ?? throw new BO.Exceptions.BlDoesNotExistException($"Volunteer with ID {volunteerId} not found.");

                    // שליפת כל ההקצאות של המתנדב
                    var assignments = s_dal.assignment.ReadAll(a => a.VolunteerId == volunteerId);

                    // סינון הקריאות שבתהליך
                    var ongoingAssignments = assignments.Where(a =>
                    {
                        DO.Call call = s_dal.call.Read(a.CallId);
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
                        SumCalls = assignments.Where(a => a.FinishAppointmentType == FinishAppointmentType.WasTreated).Count(),
                        VolunteerTakenCare = ongoingAssignments.Select(activeAssignment =>
                        {
                            var activeCall = s_dal.call.Read(activeAssignment.CallId);
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


        public static void UpdateVolunteerDetails(int requesterId, BO.Volunteer volunteerDetails)
        {
            AdminManager.ThrowOnSimulatorIsRunning();  // stage 7
            try
            {
                lock (AdminManager.BlMutex)  // נעילה על קריאות ושינויים
                {
                    // שלב 1: בקשת רשומת המתנדב משכבת הנתונים
                    var existingVolunteer = s_dal.Volunteer.ReadAll(v => v.Id == volunteerDetails.Id).FirstOrDefault()
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
                    s_dal.Volunteer.Update(newVolunteer);

                    // שלב 8: קריאה חוזרת לרשומה על מנת לוודא שהיא אכן עודכנה
                    var updatedVolunteer = s_dal.Volunteer.Read(v => v.Id == newVolunteer.Id);
                    if (updatedVolunteer == null)
                    {
                        throw new Exception("Volunteer update failed, no record found after update.");
                    }

                    Console.WriteLine("Volunteer updated successfully in database.");

                    VolunteersManager.Observers.NotifyItemUpdated(newVolunteer.Id);  // stage 5
                    VolunteersManager.Observers.NotifyListUpdated();  // stage 5
                    Console.WriteLine("Observers notified.");
                }
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

        private static bool IsAdmin(int id)
        {
            lock (AdminManager.BlMutex)  // עטיפה ב-lock
            {
                var volunteer = s_dal.Volunteer.Read(v => v.Id == id);
                if (volunteer != null && volunteer.Position == DO.Position.admin)
                    return true;

                return false;
            }
        }



        public static void DeleteVolunteer(int volunteerId)
        {
            AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
            try
            {
                // שלב 1: בדוק אם המתנדב מטפל בקריאה פעילה
                lock (AdminManager.BlMutex)  // עטיפה ב-lock עבור קריאת ה-DAL
                {
                    var assignments = s_dal.assignment.ReadAll(a => a.VolunteerId == volunteerId).ToList();
                    var activeAssignment = assignments.FirstOrDefault(a => a.FinishAppointmentType == null); // אם יש הקצאה פעילה

                    if (activeAssignment != null)
                    {
                        // אם המתנדב מטפל בהקצאה פעילה, יש לזרוק חריגה
                        throw new BlCantBeErased($"Volunteer with id {volunteerId} cannot be erased because they are currently handling a call.");
                    }
                }

                // שלב 2: ניסיון למחוק את המתנדב אם הוא לא מטפל בהקצאה פעילה
                lock (AdminManager.BlMutex)  // עטיפה ב-lock עבור קריאת ה-DAL למחיקה
                {
                    s_dal.Volunteer.Delete(volunteerId); // מנסה למחוק את המתנדב
                }

                VolunteersManager.Observers.NotifyItemUpdated(volunteerId);  //stage 5
                VolunteersManager.Observers.NotifyListUpdated(); //stage 5
            }
            catch (DO.DalDoesNotExistException ex)
            {
                // טיפול בחריגה במקרה של מתנדב שלא נמצא
                throw new BlDoesNotExistException($"Volunteer with id {volunteerId} not found.", ex);
            }
        }





        public static void AddVolunteer(BO.Volunteer volunteer)
        {
            AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

            DO.Volunteer newVolunteer = new()
            {
                Id = volunteer.Id,
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
                // עטיפה ב-lock עבור קריאה ל-DAL (יצירת המתנדב)
                lock (AdminManager.BlMutex)
                {
                    s_dal.Volunteer.Create(newVolunteer);
                }

                VolunteersManager.Observers.NotifyItemUpdated(newVolunteer.Id);  // stage 5
                VolunteersManager.Observers.NotifyListUpdated(); // stage 5   
            }
            catch (DO.DalAlreadyExistException ex)
            {
                throw new BLAlreadyExistException($"Volunteer with id {volunteer.Id} already exists");
            }
        }
    }

#endregion



}
