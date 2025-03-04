using BlApi;
using BlImplementation;
using BO;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static BO.Enums;
using System.Web;
using static BO.Exceptions;
using Helpers;
using static Helpers.Tools;
using BL.Helpers;

namespace BL.Helpers
{
    internal static class VolunteerManager
    {
        private static readonly object _lockObject = new();

        internal static ObserverManager Observers = new(); //stage 5 

        private static IDal s_dal = DalApi.Factory.Get; //stage 4

        internal static BO.CallInProgress? GetCallIn(DO.Volunteer doVolunteer)
        {
            lock (_lockObject) //stage 7
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
                            DistanceOfCall = Tools.CalculateDistance(latitude, longitude, callTreat.Latitude, callTreat.Longitude)
                        };
                    }
                }
                return null;
            }
        }

        internal static bool CheckVolunteerLogic(BO.Volunteer volunteer)
        {
            return checkVolunteerEmail(volunteer) && IsPhoneNumberValid(volunteer) && IsValidId(volunteer.Id);
        }

        internal static bool checkVolunteerEmail(BO.Volunteer volunteer)
        {
            if (string.IsNullOrEmpty(volunteer.Email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(volunteer.Email);
                return addr.Address == volunteer.Email;
            }
            catch
            {
                return false;
            }
        }

        internal static BO.Volunteer RequestVolunteerDetails(int volunteerId)
        {
            try
            {
                // ????? ?? ?????? ?-DAL ?-lock
                lock (AdminManager.BlMutex)
                {
                    // ????? ???? ??????
                    DO.Volunteer volunteer = s_dal.Volunteer.Read(volunteerId)
                        ?? throw new BO.Exceptions.BlDoesNotExistException($"Volunteer with ID {volunteerId} not found.");

                    // ????? ?? ??????? ?? ??????
                    var assignments = s_dal.assignment.ReadAll(a => a.VolunteerId == volunteerId);

                    // ????? ??????? ???????
                    var ongoingAssignments = assignments.Where(a =>
                    {
                        DO.Call call = s_dal.call.Read(a.CallId);
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

        internal static void UpdateVolunteerDetails(int requesterId, BO.Volunteer volunteerDetails)
        {
            AdminManager.ThrowOnSimulatorIsRunning();  // בדיקה אם הסימולטור רץ

            try
            {
                lock (AdminManager.BlMutex) // מונע בעיות בתחרות על הנתונים
                {
                    // שליפת המתנדב הקיים
                    var existingVolunteer = s_dal.Volunteer.ReadAll(v => v.Id == volunteerDetails.Id).FirstOrDefault()
                        ?? throw new DalDoesNotExistException($"Volunteer with ID {volunteerDetails.Id} not found.");

                    Console.WriteLine("Existing volunteer found.");

                    // בדיקת הרשאה - האם זה המתנדב עצמו או מנהל?
                    bool isAdmin = IsAdmin(requesterId);
                    if (requesterId != volunteerDetails.Id && !isAdmin)
                    {
                        throw new UnauthorizedAccessException("Unauthorized to update volunteer details.");
                    }

                    Console.WriteLine("Access authorization passed.");

                    // בדיקות ולידציה
                    if (!VolunteerManager.checkVolunteerEmail(volunteerDetails))
                        throw new BlEmailNotCorrect("Invalid email format.");
                    if (!VolunteerManager.IsValidId(volunteerDetails.Id))
                        throw new BlIdNotValid("Invalid ID format.");
                    if (!VolunteerManager.IsPhoneNumberValid(volunteerDetails))
                        throw new BlPhoneNumberNotCorrect("Invalid phone number format.");

                    Console.WriteLine("Format checks passed.");

                    if (volunteerDetails.Latitude == null || volunteerDetails.Longitude == null)
                        throw new BlInvalidLocationException("Location must include valid latitude and longitude.");

                    Console.WriteLine("Location checks passed.");

                    // רק מנהל יכול לשנות תפקידים
                    if ((Enums.VolunteerTypeEnum)existingVolunteer.Position != volunteerDetails.Position && !isAdmin)
                    {
                        throw new BlUnauthorizedAccessException("Only admins can update the position.");
                    }


                    Console.WriteLine("Position update authorized.");

                    double[]? coordinates = null;

                    // אם הכתובת לא השתנתה, שומרים את הקואורדינטות הקיימות
                    if (existingVolunteer.Location == volunteerDetails.Location && existingVolunteer.Latitude != null && existingVolunteer.Longitude != null)
                    {
                        coordinates = new double[] { (double)existingVolunteer.Latitude, (double)existingVolunteer.Longitude };
                    }

                    // יצירת אובייקט חדש עם נתוני המתנדב המעודכנים
                    DO.Volunteer updatedVolunteer = new DO.Volunteer
                    {
                        Id = volunteerDetails.Id,
                        FullName = volunteerDetails.FullName,
                        PhoneNumber = volunteerDetails.PhoneNumber,
                        Password = volunteerDetails.Password, // צריך לוודא שזו סיסמה מוצפנת
                        Location = volunteerDetails.Location,
                        Email = volunteerDetails.Email,
                        Active = volunteerDetails.Active,
                        DistanceType = (DO.DistanceType)volunteerDetails.DistanceType,
                        Position = (DO.Position)volunteerDetails.Position,
                        Latitude = coordinates != null ? coordinates[0] : null,
                        Longitude = coordinates != null ? coordinates[1] : null,
                        MaxDistance = volunteerDetails.MaxDistance,
                    };

                    Console.WriteLine("Volunteer data mapped to DO object.");

                    // עדכון במסד הנתונים
                    s_dal.Volunteer.Update(updatedVolunteer);

                    // אם הכתובת השתנתה, יש לעדכן קואורדינטות חדשות
                    if (coordinates == null)
                    {
                        _ = VolunteerManager.GetCoordinates(updatedVolunteer);
                    }

                    Console.WriteLine("Volunteer updated successfully in database.");

                    // עדכון לצופים (Observers)
                    VolunteerManager.Observers.NotifyItemUpdated(updatedVolunteer.Id);
                    VolunteerManager.Observers.NotifyListUpdated();

                    Console.WriteLine("Observers notified.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during update.");
                throw new CannotUpdateVolunteerException("An error occurred while updating the volunteer details.", ex);
            }
        }
        internal static async Task GetCoordinates(DO.Volunteer doVolunteer)
        {
            if (doVolunteer.Location is not null)  // Check if address is not null
            {
                double[]? loc = await Tools.GetGeolocationCoordinatesAsync(doVolunteer.Location);  // Get coordinates using geocoding API
                if (loc is not null)
                    doVolunteer = doVolunteer with { Latitude = loc[0], Longitude = loc[1] };  // Update volunteer with latitude and longitude
                else
                    doVolunteer = doVolunteer with { Latitude = null, Longitude = null };  // Set coordinates to null if not found
                lock (AdminManager.BlMutex)  // Lock to prevent concurrent access
                    s_dal.Volunteer.Update(doVolunteer);  // Update volunteer in the database
                Observers.NotifyListUpdated();  // Notify observers of list update
                Observers.NotifyItemUpdated(doVolunteer.Id);  // Notify observers of item update
            }
        }

        internal static bool IsAdmin(int id)
        {
            lock (AdminManager.BlMutex)  // ????? ?-lock
            {
                var volunteer = s_dal.Volunteer.Read(v => v.Id == id);
                if (volunteer != null && volunteer.Position == DO.Position.admin)
                    return true;

                return false;
            }
        }
        internal static bool IsValidId(int id)
        {
            return id.ToString().Length == 9 && id > 0;
        }

        internal static bool IsPhoneNumberValid(BO.Volunteer volunteer)
        {
            if (string.IsNullOrEmpty(volunteer.PhoneNumber))
                return false;

            string phonePattern = @"^0[1-9]\d{8}$";
            return Regex.IsMatch(volunteer.PhoneNumber, phonePattern);
        }

        internal static void SimulateAssignForVolunteer()
        {
            // Simulation logic for volunteer assignment
        }
    }
}
