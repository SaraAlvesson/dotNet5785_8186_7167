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
    internal static class  VoluneerManager
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

        internal static bool checkVolunteerLogic(BO.Volunteer volunteer)
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