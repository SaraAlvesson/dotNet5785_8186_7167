using BO;
using System.Text.Json; // שים לב לשימוש ב- System.Text.Json במקום Newtonsoft.Json
using DalApi;
using DO;
using System.Net.Http;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading.Tasks;
using static BO.Enums;


namespace Helpers
{
    internal static class CallManager
    {
        private static IDal s_dal = Factory.Get; //stage 4

        internal static ObserverManager Observers = new(); //stage 5 




        internal static BO.Enums.FinishAppointmentTypeEnum? ConvertEndType(DO.FinishAppointmentType? finishAppointment)
        {
            if (finishAppointment == null)
            {
                // אם finishAppointment הוא null, נחזיר null
                return null;
            }

            switch (finishAppointment.Value)
            {
                case DO.FinishAppointmentType.WasTreated:
                    return BO.Enums.FinishAppointmentTypeEnum.WasTreated;
                case DO.FinishAppointmentType.SelfCancellation:
                    return BO.Enums.FinishAppointmentTypeEnum.SelfCancellation;
                case DO.FinishAppointmentType.CancelingAnAdministrator:
                    return BO.Enums.FinishAppointmentTypeEnum.CancelingAnAdministrator;
                case DO.FinishAppointmentType.CancellationHasExpired:
                    return BO.Enums.FinishAppointmentTypeEnum.CancellationHasExpired;
                default:
                    throw new ArgumentException("Invalid finish appointment type", nameof(finishAppointment));
            }
        }


        public static void checkCallLogic(BO.Call call)
        {
            // בדיקת יחס זמנים
            if (call.MaxFinishTime != null && call.MaxFinishTime <= call.OpenTime)
                throw new InvalidCallLogicException("Max finish time must be later than open time.");
        }




        public static void checkCallFormat(BO.Call call)
        {
            // בדיקת מזהה
            if (call.Id <= 0)
                throw new InvalidCallFormatException("Call ID must be a positive integer.");

            // בדיקת זמן פתיחה
            if (call.OpenTime == default)
                throw new InvalidCallFormatException("Open time is not valid.");

            // בדיקת כתובת
            if (Tools.IsAddressValid(call.Address))
                throw new InvalidCallFormatException("Address cannot be empty.");

            if (call.Address.Length > 200)
                throw new InvalidCallFormatException("Address exceeds the maximum length of 200 characters.");

            // בדיקת אורך ורוחב
            if (call.Longitude < -180 || call.Longitude > 180)
                throw new InvalidCallFormatException("Longitude must be between -180 and 180 degrees.");

            if (call.Latitude < -90 || call.Latitude > 90)
                throw new InvalidCallFormatException("Latitude must be between -90 and 90 degrees.");
        }


        public static async Task<bool> IsValidAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return false;
            }

            try
            {
                var httpClient = new HttpClient();
                var apiKey = "ba9b0180f2cd4da1999edc1820855bdb";  // המפתח שלך
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={apiKey}";

                var response = await httpClient.GetStringAsync(url);

                // הפענוח של התשובה בתור JSON
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(response);
                var status = jsonResponse.GetProperty("status").GetString();

                // אם הסטטוס הוא "OK", אז הכתובת קיימת
                if (status == "OK")
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                // אם יש שגיאה בבקשה או במענה
                Console.WriteLine($"Error occurred: {ex.Message}");
            }

            return false; // אם לא מצאנו כתובת או קרתה שגיאה
        }







        public static void UpdateExpired()
        {
            // Step 1: Retrieve all calls where MaxTimeToEnd has passed and need handling
            var expiredCalls = s_dal.call.ReadAll(c => c.MaxTime < AdminManager.Now);

            // Step 2: Handle calls without assignments
            foreach (var call in expiredCalls)
            {
                var hasAssignment = s_dal.assignment.ReadAll(a => a.CallId == call.Id).Any();

                if (!hasAssignment)
                {
                    var newAssignment = new DO.Assignment
                    {
                        Id = 0,  // New ID will be generated
                        CallId = call.Id,
                        VolunteerId = 0,
                        AppointmentTime = AdminManager.Now,
                        FinishAppointmentType = DO.FinishAppointmentType.CancellationHasExpired
                    };
                    s_dal.assignment.Create(newAssignment);
                }
            }

            // Step 3: Update existing assignments with null FinishAppointmentTime
            var assignmentsToUpdate = s_dal.assignment.ReadAll(a => a.FinishAppointmentTime == null);

            foreach (var assignment in assignmentsToUpdate)
            {
                var call = expiredCalls.FirstOrDefault(c => c.Id == assignment.CallId);
                if (call != null)
                {
                    var updatedAssignment = assignment with
                    {
                        FinishAppointmentTime = AdminManager.Now,
                        FinishAppointmentType = DO.FinishAppointmentType.CancellationHasExpired
                    };
                    s_dal.assignment.Update(updatedAssignment);

                    // Notify specific item update
                    Observers.NotifyItemUpdated(updatedAssignment.Id);
                }
            }

            // Step 5: Notify observers for list update
            Observers.NotifyListUpdated();
        }




        #region
        public static void UpdateToCancelCallTreatment(int RequesterId, int AssignmentId)
        {
            // Retrieve the assignment object based on its ID.
            var assignment = s_dal.assignment.Read(AssignmentId);
            // Check if the assignment does not exist.
            if (assignment == null)
                throw new BO.Exceptions.BlDoesNotExistException($"Assignment with id={AssignmentId} does Not exist\"");

            BO.Call call = readCallData(assignment.CallId);
            // Retrieve the volunteer (asker) object based on the RequesterId.
            var asker = s_dal.Volunteer.Read(RequesterId);

            // Check if the volunteer does not exist.
            if (asker == null)
                throw new BO.Exceptions.BlDoesNotExistException($"Volunteer with id={RequesterId} does Not exist\"");

            // Check if the volunteer is not authorized to cancel the assignment (either not their own or not a manager).
            if (assignment.VolunteerId != RequesterId && asker.Position != DO.Position.admin)
                throw new BO.Exceptions.CannotUpdateVolunteerException($"Volunteer with id={RequesterId} can't change this assignment to cancel");

            // Check if the assignment has already ended.
            if (assignment.FinishAppointmentTime.HasValue)
                throw new BO.Exceptions.BlCantBeErased("This assignment already ended");

            if (call.CallStatus != BO.Enums.CalltStatusEnum.CallIsBeingTreated && call.CallStatus != BO.Enums.CalltStatusEnum.CallTreatmentAlmostOver)
                throw new BO.Exceptions.CannotUpdateCallException($"You can only unassign if the call is currently in progress.");

            // Create a new assignment object with updated end time and end type based on role.
            DO.Assignment newAssign;
            if (asker.Position == DO.Position.admin && assignment.VolunteerId != RequesterId)
                newAssign = assignment with { FinishAppointmentTime = AdminManager.Now, FinishAppointmentType = DO.FinishAppointmentType.CancelingAnAdministrator };
            else
                newAssign = assignment with { FinishAppointmentTime = AdminManager.Now, FinishAppointmentType = DO.FinishAppointmentType.SelfCancellation };
            try
            {
                // Update the assignment in the data layer.
                s_dal.assignment.Update(newAssign);
            }
            catch (DO.DalDoesNotExistException ex)
            {
                // If updating fails, throw an exception indicating the assignment does not exist.
                throw new BO.Exceptions.BlDoesNotExistException($"Assignment with ID={AssignmentId} does not exist", ex);
            }
            CallManager.Observers.NotifyItemUpdated(call.Id);  //update current call  and obserervers etc.
            CallManager.Observers.NotifyListUpdated();  //update list of calls  and obserervers etc.
            VolunteersManager.Observers.NotifyItemUpdated(assignment.VolunteerId);  //update current call  and obserervers etc.
            VolunteersManager.Observers.NotifyListUpdated();  //update list of calls  and obserervers etc.
        }
        public static void AssignCallToVolunteer(int volunteerId, int callId)
        {
            try
            {
                // שליפת פרטי הקריאה
                BO.Call call = readCallData(callId);

                if (call == null)
                    throw new InvalidOperationException("Call not found.");

                // בדיקת אם הקריאה לא טופלה ולא פג תוקפה
                if (call.CallStatus != BO.Enums.CalltStatusEnum.OPEN && call.CallStatus != BO.Enums.CalltStatusEnum.CallAlmostOver)
                    throw new InvalidOperationException("Call has already been treated or expired.");

                // בדיקת אם קיימת הקצאה פתוחה על הקריאה
                var existingAssignments = s_dal.assignment.Read(a => a.CallId == callId && a.FinishAppointmentTime == null);
                if (existingAssignments != null)
                    throw new InvalidOperationException("Call is already assigned to a volunteer.");

                // יצירת הקצאה חדשה
                DO.Assignment newAssignment = new DO.Assignment
                {
                    Id = 0,
                    VolunteerId = volunteerId,
                    CallId = callId,
                    AppointmentTime = DateTime.Now, // זמן כניסה לטיפול
                    FinishAppointmentTime = null,  // עדיין לא מעודכן
                    FinishAppointmentType = null   // עדיין לא מעודכן
                };

                // ניסיון הוספה לשכבת הנתונים
                s_dal.assignment.Create(newAssignment);


                // עדכון תצוגת הקריאות
                CallManager.Observers.NotifyItemUpdated(newAssignment.Id);
                CallManager.Observers.NotifyListUpdated();
            }
            catch (Exception ex)
            {
                // תרגום חריגות כלליות להודעה ברורה
                throw new Exception("Error occurred while assigning the call: " + ex.Message, ex);
            }
        }

        public static void UpdateCallAsCompleted(int volunteerId, int AssignmentId)
        {
            AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
            try
            {
                // Retrieve the assignment by ID
                var assignment = s_dal.assignment.Read(AssignmentId);

                if (assignment == null)
                    throw new InvalidOperationException($"Assignment with id={AssignmentId} does not exist.");

                BO.Call call = readCallData(assignment.CallId);

                // Check if the volunteer is not the one assigned to this assignment
                if (assignment.VolunteerId != volunteerId)
                    throw new InvalidOperationException($"Volunteer with id={volunteerId} can't change this assignment to end.");

                // Check if the assignment has already ended
                if (assignment.FinishAppointmentTime.HasValue)
                    throw new InvalidOperationException("This assignment already ended.");

                // Create a new assignment object with updated end time and end type
                DO.Assignment newAssign = assignment with { FinishAppointmentTime = AdminManager.Now, FinishAppointmentType = DO.FinishAppointmentType.WasTreated };

                // Attempt to update the assignment in the database
                s_dal.assignment.Update(newAssign);

                CallManager.Observers.NotifyItemUpdated(assignment.CallId);  //update current call  and observers etc.
                CallManager.Observers.NotifyListUpdated();  //update list of calls and observers etc.
                VolunteersManager.Observers.NotifyItemUpdated(volunteerId);  //update current volunteer and observers etc.
                VolunteersManager.Observers.NotifyListUpdated();
            }
            catch (Exception ex)
            {
                // Handle exceptions
                throw new InvalidOperationException($"Error updating call as completed: {ex.Message}", ex);
            }
        }
        public static IEnumerable<BO.OpenCallInList> GetOpenCallInLists(
     int volunteerId,
     BO.Enums.CallTypeEnum? filter = null,
     BO.Enums.OpenCallEnum? toSort = null)
        {
            // שליפת רשימות הקריאות והשיוכים
            var listCall = s_dal.call.ReadAll();
            var listAssignment = s_dal.assignment.ReadAll();

            if (!listCall.Any())
            {
                throw new Exception("No calls found in the database.");
            }

            if (!listAssignment.Any())
            {
                throw new Exception("No assignments found in the database.");
            }

            var volunteer = s_dal.Volunteer.Read(v => v.Id == volunteerId);

            if (volunteer == null)
            {
                throw new ArgumentException("Volunteer not found.");
            }

            // שליפת מיקום המתנדב
            string volunteerAddress = volunteer.Location;

            if (string.IsNullOrWhiteSpace(volunteerAddress))
            {
                throw new ArgumentException("Volunteer location is not provided.");
            }

            // קריאה אסינכרונית לפעולה
            double[] volunteerLocation;
            try
            {
                volunteerLocation = Tools.GetGeolocationCoordinates(volunteerAddress);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch volunteer location: {ex.Message}");
            }

            if (volunteerLocation == null || volunteerLocation.Length != 2)
            {
                throw new Exception("Invalid location data received for the volunteer.");
            }

            // סינון הקריאות הפתוחות לפי סטטוס
            var openCalls = from call in listCall
                            let assignment = listAssignment.FirstOrDefault(a => a.CallId == call.Id)
                            let status = Tools.callStatus(call.Id)
                            where status == BO.Enums.CalltStatusEnum.OPEN || status == BO.Enums.CalltStatusEnum.CallAlmostOver
                            select new BO.OpenCallInList
                            {
                                Id = call.Id,
                                CallType = (BO.Enums.CallTypeEnum)call.CallType,
                                Address = call.Adress,
                                VerbDesc = call.VerbDesc,
                                OpenTime = call.OpenTime,
                                MaxFinishTime = call.MaxTime,
                                DistanceOfCall = Tools.CalculateDistance(volunteerLocation[0], volunteerLocation[1], (double)call.Latitude, (double)call.Longitude)
                            };

            // סינון לפי סוג הקריאה אם הוזן
            if (filter.HasValue)
            {
                openCalls = openCalls.Where(call => call.CallType == filter.Value);
            }

            // מיון הקריאות לפי השדה שהוזן
            openCalls = toSort switch
            {
                BO.Enums.OpenCallEnum.Id => openCalls.OrderBy(call => call.Id),
                BO.Enums.OpenCallEnum.Address => openCalls.OrderBy(call => call.Address),
                BO.Enums.OpenCallEnum.OpenTime => openCalls.OrderBy(call => call.OpenTime),
                BO.Enums.OpenCallEnum.MaxFinishTime => openCalls.OrderBy(call => call.MaxFinishTime),
                BO.Enums.OpenCallEnum.DistanceOfCall => openCalls.OrderBy(call => call.DistanceOfCall),
                _ => openCalls.OrderBy(call => call.Id)
            };

            // החזרת הרשימה הממוינת
            return openCalls.ToList();
        }
        public static BO.Call readCallData(int ID)
        {

            var doCall = s_dal.call.Read(ID) ??
                throw new BO.Exceptions.BlDoesNotExistException($"Call with ID={ID} does Not exist");
            IEnumerable<DO.Assignment> assignments = s_dal.assignment.ReadAll();
            IEnumerable<DO.Volunteer> volunteers = s_dal.Volunteer.ReadAll();
            List<BO.CallAssignInList> myAssignments = assignments.Where(a => a.CallId == ID).
                Select(a =>
                {
                    return new CallAssignInList
                    {
                        RealFinishTime = a.FinishAppointmentTime,
                        FinishAppointmentType = a.FinishAppointmentType == null ? null : (FinishAppointmentTypeEnum)a.FinishAppointmentType,
                        OpenTime = a.AppointmentTime,
                        VolunteerId = a.VolunteerId,
                        VolunteerName = volunteers.Where(v => v.Id == a.VolunteerId).First().FullName,
                    };
                }).ToList();
            return new BO.Call
            {
                Id = ID,
                CallType = doCall.CallType switch
                {
                    DO.CallType.PreparingFood => BO.Enums.CallTypeEnum.PreparingFood,
                    DO.CallType.TransportingFood => BO.Enums.CallTypeEnum.TransportingFood,
                    DO.CallType.FixingEquipment => BO.Enums.CallTypeEnum.FixingEquipment,
                    DO.CallType.ProvidingShelter => BO.Enums.CallTypeEnum.ProvidingShelter,
                    DO.CallType.TransportAssistance => BO.Enums.CallTypeEnum.TransportAssistance,
                    DO.CallType.MedicalAssistance => BO.Enums.CallTypeEnum.MedicalAssistance,
                    DO.CallType.EmotionalSupport => BO.Enums.CallTypeEnum.EmotionalSupport,
                    DO.CallType.PackingSupplies => BO.Enums.CallTypeEnum.PackingSupplies,


                    _ => throw new FormatException("Unknown Call Type!")
                },
                Address = doCall.Adress,
                OpenTime = doCall.OpenTime,
                Latitude = doCall.Latitude,
                Longitude = doCall.Longitude,
                VerbDesc = doCall.VerbDesc,
                MaxFinishTime = doCall.MaxTime,
                CallStatus = Tools.callStatus(ID),
                CallAssignInLists = myAssignments
            };
        }
        #endregion


    }
}