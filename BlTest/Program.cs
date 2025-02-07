using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using BlApi;
using BO;
using DalApi;
using DO;
using static BO.Enums;

internal class Program
{
    // שדה הממשק לשכבת ה-BL
    static readonly IBl s_bl = BlApi.Factory.Get();
    static void Main(string[] args)
    {
        try
        {
            RunMainMenu();
        }
        catch (Exception ex)
        { Console.WriteLine($"Error occurred: {ex.Message}"); }

    } // הצגת תפריט ראשי



    /// <summary>
    /// Displays and manages the main menu options.
    /// Allows the user to navigate to different functional menus or perform database operations.
    /// </summary>
    private static void RunMainMenu()
    {
        while (true)
        {
            // Display the main menu options
            Console.WriteLine("\nMain Menu:");
            Console.WriteLine("1 - Admin Menu");      // Option to access the call menu
            Console.WriteLine("2 - Volunteer Menu"); // Option to access the volunteer menu
            Console.WriteLine("3 - Call Menu");      // Option to access the call menu
            Console.WriteLine("0 - Exit");           // Option to exit the program
            Console.Write("Enter your choice: ");

            // Parse user input and validate it
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > 7)
            {
                Console.WriteLine("Invalid choice, try again."); // Notify user of invalid input
                continue;
            }
            try
            {

                // Execute the appropriate action based on user choice
                switch (choice)
                {
                    case 1:
                        RunAdminMenu(); // Navigate to volunteer menu
                        break;
                    case 2:
                        RunVolunteerMenu(); // Navigate to call menu (not implemented in this snippet)
                        break;
                    case 3:
                        RunCallMenu(); // Navigate to assignment menu (not implemented in this snippet)
                        break;
                    case 0:
                        return; // Exit the main menu and terminate the program
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

    }

    // תפריט ניהול מנהלים

    private static void RunAdminMenu()
    {
        while (true)
        {
            // Display the Volunteer Menu options
            Console.WriteLine("\nAdmin Menu:");
            Console.WriteLine("1 - Current Time");           // Option to create a new volunteer
            Console.WriteLine("2 - Update Clock");             // Option to read a specific volunteer by ID
            Console.WriteLine("3 - Get RiskTime Range");         // Option to read all volunteers
            Console.WriteLine("4 - Set RiskTime Range");           // Option to update an existing volunteer
            Console.WriteLine("5 - Reset Database");           // Option to delete a specific volunteer by ID
            Console.WriteLine("6 - Initialize Database");       // Option to delete all volunteers
            Console.WriteLine("0 - Back to Main Menu");// Option to return to the Main Menu
            Console.Write("Enter your choice: ");

            // Parse user input and validate it
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > 6)
            {
                Console.WriteLine("Invalid choice, try again."); // Notify user of invalid input
                continue;
            }

            try
            {
                // Execute the appropriate action based on user choice
                switch (choice)
                {
                    case 1:
                        // current time
                        Console.WriteLine("Current Time:"); // Notify the user
                        Console.WriteLine(s_bl.Admin.GetCurrentTime()); // Create the volunteer in the database
                        break;

                    case 2://updare clock

                        Console.WriteLine("Enter time unit ( SECOND, MINUTE, HOUR, DAY, MONTH, YEAR):");

                        string userInput = Console.ReadLine();

                        // המרת הקלט ל-Enum
                        Enums.TimeUnitEnum timeUnit = (Enums.TimeUnitEnum)Enum.Parse(typeof(Enums.TimeUnitEnum), userInput.ToUpper());

                        // קריאה לפונקציה עם הערך ב-Enum
                        s_bl.Admin.UpdateClock(timeUnit);
                        Console.WriteLine("Clock updated successfully!");


                        break;

                    case 3:
                        Console.WriteLine("Risk Range:");
                        Console.WriteLine(s_bl.Admin.GetRiskTimeRange()); // Create the volunteer in the database

                        break;

                    case 4:
                        Console.WriteLine("Set Risk Range (e.g., 02:00:00 for 2 hours):");
                        try
                        {
                            string userInputR = Console.ReadLine();
                            if (TimeSpan.TryParse(userInputR, out TimeSpan riskTimeRange))
                            {
                                // Assuming SetRiskTimeRange is the correct method to apply the range
                                s_bl.Admin.SetRiskTimeRange(riskTimeRange);
                                Console.WriteLine($"Risk range successfully set to: {riskTimeRange}");
                            }
                            else
                            {
                                Console.WriteLine("Invalid input. Please enter a valid time range (e.g., 02:00:00).");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred: {ex.Message}");
                        }
                        break;


                    case 5:
                        Console.WriteLine("Reset Database:");
                        s_bl.Admin.ResetDatabase();

                        break;

                    case 6:
                        Console.WriteLine("Initialize Database:");
                        s_bl.Admin.InitializeDatabase();
                        break;

                    case 0:
                        return; // Exit to the Main Menu
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and display error messages
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static void RunVolunteerMenu()
    {
        while (true)
        {
            // Display the Volunteer Menu options
            Console.WriteLine("\nVolunteer Menu:");
            Console.WriteLine("1 - Login");           // Option to create a new volunteer
            Console.WriteLine("2 - Request Volunteer List");             // Option to read a specific volunteer by ID
            Console.WriteLine("3 - Request Volunteer Details");         // Option to read all volunteers
            Console.WriteLine("4 - UpdateVolunteer Details");           // Option to update an existing volunteer
            Console.WriteLine("5 - Delete Volunteer");           // Option to delete a specific volunteer by ID
            Console.WriteLine("6 - Add Volunteer");       // Option to delete all volunteers
            Console.WriteLine("0 - Back to Main Menu");// Option to return to the Main Menu
            Console.Write("Enter your choice: ");

            // Parse user input and validate it
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > 6)
            {
                Console.WriteLine("Invalid choice, try again."); // Notify user of invalid input
                continue;
            }

            try
            {
                // Execute the appropriate action based on user choice
                switch (choice)
                {
                    case 1:
                        // current time
                        Console.WriteLine("Login:"); // Notify the user
                        Console.WriteLine("Enter username and password :"); // Notify the user
                        int username = int.Parse(Console.ReadLine());
                        string password = Console.ReadLine();
                        s_bl.Volunteer.Login(username, password); // Create the volunteer in the database
                        break;

                    case 2:

                        Console.WriteLine("Enter Call Type (PreparingFood, TransportingFood, FixingEquipment, ProvidingShelter, TransportAssistance, MedicalAssistance, PackingSupplies, none, or 'null'):");
                        Console.WriteLine(" and enter if volunteer is active or not (0, 1, or 'null'):");

                        // קריאת הקלט מהמשתמש
                        string? callTypeInput = Console.ReadLine();
                        string? isActiveInput = Console.ReadLine();

                        // המרת isActive ל-bool? (אם הוזן "null", התוצאה תהיה null)
                        bool? isActive = isActiveInput?.ToLower() == "null"
                            ? null
                            : isActiveInput == "1";

                        // המרת callType ל-VolunteerInList? (אם הוזן "null", התוצאה תהיה null)
                        VolunteerInListField? sortField = callTypeInput?.ToLower() == "null"
                            ? null
                            : Enum.TryParse(typeof(VolunteerInListField), callTypeInput, true, out var result)
                                ? (VolunteerInListField?)result
                                : null;

                        // קריאה לפונקציה עם הערכים המעובדים
                        IEnumerable<VolunteerInList> VolunteerInList = s_bl.Volunteer.RequestVolunteerList(isActive, sortField);

                        Console.WriteLine("Volunteer List:");
                        if (VolunteerInList == null)
                            Console.WriteLine("volunteers list is empty.");
                        else
                        {
                            // הצגת התוצאה
                            foreach (var item in VolunteerInList)
                            {
                                Console.WriteLine(item);
                            }
                        }
                        break;

                    case 3:
                        Console.WriteLine("Enter Volunteer Id:");
                        int idUser = int.Parse(Console.ReadLine());
                        BO.Volunteer v = s_bl.Volunteer.RequestVolunteerDetails(idUser);
                        Console.WriteLine("Volunteer Details:");
                        Console.WriteLine(v);
                        break;

                    case 4:
                        Console.WriteLine("Enter your id and the volunteer you want to update ");
                        int id = int.Parse(Console.ReadLine());
                        BO.Volunteer vol = GetVolunteerFromUser();
                        s_bl.Volunteer.UpdateVolunteerDetails(id, vol);
                        Console.WriteLine("Volunteer updated successfully ");

                        break;


                    case 5:
                        Console.WriteLine("Enter id of volunteer you want to delete");
                        int Volunteerid = int.Parse(Console.ReadLine());
                        s_bl.Volunteer.DeleteVolunteer(Volunteerid);
                        Console.WriteLine("Volunteer Deleted:");

                        break;

                    case 6:

                        BO.Volunteer VolunteeridToAdd = GetVolunteerFromUser();
                        s_bl.Volunteer.AddVolunteer(VolunteeridToAdd);
                        Console.WriteLine("Volunteer Added:");
                        break;
                    case 0:
                        return; // Exit to the Main Menu
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and display error messages
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
    public static BO.Volunteer GetVolunteerFromUser()
    {
        Console.WriteLine("Enter Volunteer Details:");

        Console.Write("ID: ");
        int Id = int.Parse(Console.ReadLine());

        Console.Write("FullName: ");
        string FullName = Console.ReadLine();

        Console.Write("PhoneNumber: ");
        string PhoneNumber = Console.ReadLine();

        Console.Write("Email: ");
        string Email = Console.ReadLine();

        Console.Write("Password: ");
        string Password = Console.ReadLine();

        Console.Write("Location: ");
        string Location = Console.ReadLine();

        Console.Write("Latitude: ");
        double Latitude = double.Parse(Console.ReadLine());

        Console.Write("Longitude: ");
        double longitude = double.Parse(Console.ReadLine());

        Console.Write("Position: ");
        string Position = Console.ReadLine();
        Enums.VolunteerTypeEnum P = (Enums.VolunteerTypeEnum)Enum.Parse(typeof(Enums.VolunteerTypeEnum), Position.ToUpper());

        Console.Write("Active: ");
        bool Active = bool.Parse(Console.ReadLine());


        Console.Write("MaxDistance: ");
        double MaxDistance = double.Parse(Console.ReadLine());

        Console.Write("Distance Type: ");
        string DistanceType = Console.ReadLine();
        Enums.DistanceTypeEnum d = (Enums.DistanceTypeEnum)Enum.Parse(typeof(Enums.DistanceTypeEnum), DistanceType.ToUpper());

        Console.Write("SumCalls: ");
        int SumCalls = int.Parse(Console.ReadLine());

        Console.Write("SumCanceled: ");
        int SumCanceled = int.Parse(Console.ReadLine());

        Console.Write("SumExpired: ");
        int SumExpired = int.Parse(Console.ReadLine());

        Console.Write("VolunteerTakenCare: ");
        CallInProgress VolunteerTakenCare = CallInProgress();

        return new BO.Volunteer
        {
            Id = Id,
            FullName = FullName,
            PhoneNumber = PhoneNumber,
            Email = Email,
            Password = Password,
            Location = Location,
            Latitude = Latitude,
            Longitude = longitude,
            Position = P,
            Active = Active,
            MaxDistance = MaxDistance,
            DistanceType = d,
            SumCalls = SumCalls,
            SumCanceled = SumCanceled,
            SumExpired = SumExpired,
            VolunteerTakenCare = VolunteerTakenCare
        };
    }
    public static CallInProgress CallInProgress()
    {
        Console.WriteLine("Enter Volunteer call in progress:");

        Console.Write("ID: ");
        int Id = int.Parse(Console.ReadLine());

        Console.Write("CallId: ");
        int CallId = int.Parse(Console.ReadLine());

        Console.Write("CallType: ");
        string CallType = Console.ReadLine();
        Enums.CallTypeEnum cType = (Enums.CallTypeEnum)Enum.Parse(typeof(Enums.CallTypeEnum), CallType.ToUpper());

        Console.Write("VerbDesc: ");
        string VerbDesc = Console.ReadLine();

        Console.Write("CallAddress: ");
        string CallAddress = Console.ReadLine();

        Console.Write("OpenTime: ");
        DateTime OpenTime = DateTime.Parse(Console.ReadLine());

        Console.Write("Max FinishTime: ");
        DateTime MaxFinishTime = DateTime.Parse(Console.ReadLine());

        Console.Write("Start Appointment Time: ");
        DateTime StartAppointmentTime = DateTime.Parse(Console.ReadLine());

        Console.Write("Distance O fCall: ");
        double DistanceOfCall = double.Parse(Console.ReadLine());

        Console.Write("Position: ");
        string Status = Console.ReadLine();
        Enums.StatusCallInProgressEnum st = (Enums.StatusCallInProgressEnum)Enum.Parse(typeof(Enums.StatusCallInProgressEnum), Status.ToUpper());


        return new BO.CallInProgress
        {
            Id = Id,
            CallId = CallId,
            CallType = cType,
            VerbDesc = VerbDesc,
            CallAddress = CallAddress,
            OpenTime = OpenTime,
            MaxFinishTime = MaxFinishTime,
            StartAppointmentTime = StartAppointmentTime,
            DistanceOfCall = DistanceOfCall,
            Status = st,

        };
    }

    // תפריט ניהול קריאות
    private static void RunCallMenu()
    {
        while (true)
        {
            // Display the Volunteer Menu options
            Console.WriteLine("\nCall Menu:");
            Console.WriteLine("1 - Calls Amount ");           // Option to create a new volunteer
            Console.WriteLine("2 - Call List ");             // Option to read a specific volunteer by ID
            Console.WriteLine("3 - Call Details");         // Option to read all volunteers
            Console.WriteLine("4 - Update Call Details");           // Option to update an existing volunteer
            Console.WriteLine("5 - Delete Call");
            Console.WriteLine("6 - Add Call");// Option to delete a specific volunteer by ID
            Console.WriteLine("7 - Volunteer Closed Calls");       // Option to delete all volunteers
            Console.WriteLine("8 - Volunteer Open Calls");
            Console.WriteLine("9 - Update Call As Completed");
            Console.WriteLine("10 - Update To Cancel Call Treatment");
            Console.WriteLine("11- Assign Call To Volunteer");
            Console.WriteLine("0 - Back to Main Menu");// Option to return to the Main Menu
            Console.Write("Enter your choice: ");

            // Parse user input and validate it

            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > 12)
            {
                Console.WriteLine("Invalid choice, try again."); // Notify user of invalid input
                continue;
            }

            try
            {
                // Execute the appropriate action based on user choice
                switch (choice)
                {
                    case 1:
                        Console.WriteLine("Calls Amount:"); // Notify the user
                        IEnumerable<int> CallsAmount = s_bl.Call.CallsAmount();
                        break;

                    case 2:
                        Console.WriteLine("Enter Call Field to filter (ID, CallId, CallType, OpenTime, SumTimeUntilFinish, LastVolunteerName, SumAppointmentTime, Status, SumAssignment):");
                        string? filterInput = Console.ReadLine();

                        Console.WriteLine("Enter filter value:");
                        string? toFilterInput = Console.ReadLine();

                        Console.WriteLine("Enter Call Field to sort (ID, CallId, CallType, OpenTime, SumTimeUntilFinish, LastVolunteerName, SumAppointmentTime, Status, SumAssignment):");
                        string? sortInput = Console.ReadLine();

                        Enums.CallFieldEnum? filterEnum = Enum.TryParse(typeof(Enums.CallFieldEnum), filterInput, true, out var filterResult)
                            ? (Enums.CallFieldEnum?)filterResult
                            : null;

                        Enums.CallFieldEnum? sortEnum = Enum.TryParse(typeof(Enums.CallFieldEnum), sortInput, true, out var sortResult)
                            ? (Enums.CallFieldEnum?)sortResult
                            : null;

                        object? filterValue = toFilterInput;

                        IEnumerable<BO.CallInList> callList = s_bl.Call.GetCallList(filterEnum, filterValue, sortEnum);

                        Console.WriteLine("Call in List:");
                        foreach (var item in callList)
                        {
                            Console.WriteLine(item);
                        }

                        break;

                    case 3:
                        Console.WriteLine("Enter Call Id:");
                        int idCall = int.Parse(Console.ReadLine());
                        BO.Call c = s_bl.Call.readCallData(idCall);
                        Console.WriteLine("Call Details:");
                        Console.WriteLine(c);
                        break;

                    case 4:
                        BO.Call call = GetCallFromUser();
                        s_bl.Call.UpdateCallDetails(call);
                        Console.WriteLine("Call updated successfully ");
                        break;

                    case 5:
                        Console.WriteLine("Enter id of Call you want to delete");
                        int callId = int.Parse(Console.ReadLine());
                        s_bl.Call.DeleteCall(callId);
                        Console.WriteLine("Call Deleted:");
                        break;

                    case 6:
                        BO.Call CallidToAdd = GetCallFromUser();
                        s_bl.Call.AddCallAsync(CallidToAdd);
                        Console.WriteLine("Call Added:");
                        break;
                    case 7:
                        Console.WriteLine("Volunteer ID: ");
                        int volunteerId = int.Parse(Console.ReadLine());  // Renamed to avoid conflict

                        Console.WriteLine("Enter Call Type to filter (PreparingFood, EmotionalSupport, TransportingFood, FixingEquipment, ProvidingShelter, TransportAssistance, MedicalAssistance, PackingSupplies, none, or 'null'):");
                        string? callTypeInput = Console.ReadLine();  // Renamed to avoid conflict

                        Console.WriteLine("Enter close Call Field to sort (ID, CallType, VerbDesc, Address, OpenTime, MaxFinishTime, DistanceOfCall):");
                        string? sortFieldInput = Console.ReadLine();  // Renamed to avoid conflict

                        // Converting the call type input to enum
                        Enums.CallTypeEnum? callTypeEnum = callTypeInput?.ToLower() == "null"
                            ? null
                            : Enum.TryParse(typeof(Enums.CallTypeEnum), callTypeInput, true, out var callTypeResult)
                                ? (Enums.CallTypeEnum?)callTypeResult
                                : null;

                        // Converting the sort field input to enum
                        Enums.ClosedCallFieldEnum? sortFieldEnum = sortFieldInput?.ToLower() == "null"
                            ? null
                            : Enum.TryParse(typeof(Enums.ClosedCallFieldEnum), sortFieldInput, true, out var sortFieldResult)
                                ? (Enums.ClosedCallFieldEnum?)sortFieldResult
                                : null;

                        var closedCalls = s_bl.Call.GetVolunteerClosedCalls(volunteerId, callTypeEnum, sortFieldEnum);  // Renamed to match method signature

                        Console.WriteLine("Closed calls: ");
                        if (!closedCalls.Any())
                        {
                            Console.WriteLine("No closed calls.");
                        }
                        else
                        {
                            foreach (var closedCall in closedCalls)  // Renamed to avoid conflict with 'call'
                            {
                                Console.WriteLine(closedCall);
                            }
                        }

                        break;

                    case 8:
                        Console.WriteLine("Volunteer ID: ");
                        int volunteerIdOpen = int.Parse(Console.ReadLine());  // Renamed to avoid conflict

                        Console.WriteLine("Enter Call Type to filter (PreparingFood, EmotionalSupport, TransportingFood, FixingEquipment, ProvidingShelter, TransportAssistance, MedicalAssistance, PackingSupplies, none, or 'null'):");
                        string? callTypeInputOpen = Console.ReadLine();  // Renamed to avoid conflict

                        Console.WriteLine("Enter Open Call Field to sort (ID, CallType, VerbDesc, Address, OpenTime, MaxFinishTime, DistanceOfCall):");
                        string? sortFieldInputOpen = Console.ReadLine();  // Renamed to avoid conflict

                        // Converting the call type input to enum
                        Enums.CallTypeEnum? callTypeEnumOpen = callTypeInputOpen?.ToLower() == "null"
                            ? null
                            : Enum.TryParse(typeof(Enums.CallTypeEnum), callTypeInputOpen, true, out var callTypeResultOpen)
                                ? (Enums.CallTypeEnum?)callTypeResultOpen
                                : null;

                        // Converting the sort field input to enum
                        Enums.OpenCallEnum? sortFieldEnumOpen = sortFieldInputOpen?.ToLower() == "null"
                            ? null
                            : Enum.TryParse(typeof(Enums.OpenCallEnum), sortFieldInputOpen, true, out var sortFieldResultOpen)
                                ? (Enums.OpenCallEnum?)sortFieldResultOpen
                                : null;

                        var openCalls = s_bl.Call.GetOpenCallInListsAsync(volunteerIdOpen, callTypeEnumOpen, sortFieldEnumOpen);  // Renamed to match method signature

                        Console.WriteLine("Open calls: ");
                        if (!openCalls.Any())
                        {
                            Console.WriteLine("No open calls.");
                        }
                        else
                        {
                            foreach (var openCall in openCalls)  // Renamed to avoid conflict with 'call'
                            {
                                Console.WriteLine(openCall);
                            }
                        }

                        break;


                    case 9:
                        Console.WriteLine("Enter volunteer Id and assignment Id of the call you want to update as completed");

                        int volunteerid = int.Parse(Console.ReadLine());

                        int assignmentId = int.Parse(Console.ReadLine());

                        s_bl.Call.UpdateCallAsCompleted(volunteerid, assignmentId);

                        Console.WriteLine("Call Updated as completed");
                        break;

                    case 10:
                        Console.WriteLine("Enter volunteer Id and assignment Id of the call you want to Update to Cancel Call Treatment");

                        int id = int.Parse(Console.ReadLine());

                        int assigId = int.Parse(Console.ReadLine());

                        s_bl.Call.UpdateToCancelCallTreatment(id, assigId);

                        Console.WriteLine("Call Updated to Canceled Call Treatment");
                        break;
                    case 11:
                        Console.WriteLine("Enter your Id and the call you want to take care");

                        int idv = int.Parse(Console.ReadLine());

                        int assigIdv = int.Parse(Console.ReadLine());
                        Console.WriteLine(" Available assignments for you:");
                        s_bl.Call.AssignCallToVolunteer(idv, assigIdv);

                        break;

                    // Add other cases as needed...
                    case 0:
                        return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }




    public static BO.Call GetCallFromUser()

    {

        Console.WriteLine("Enter Call Details you want to add:");
        Console.Write("ID: ");
        int Id = int.Parse(Console.ReadLine());

        Console.Write("Call Type: ");
        string CallType = Console.ReadLine();
        Enums.CallTypeEnum c = (Enums.CallTypeEnum)Enum.Parse(typeof(Enums.CallTypeEnum), CallType.ToUpper());

        Console.Write("Verb Description: ");
        string VerbDesc = Console.ReadLine();

        Console.Write("Address: ");
        string Address = Console.ReadLine();

        Console.Write("Latitude: ");
        double Latitude = double.Parse(Console.ReadLine());

        Console.Write("Longitude: ");
        double longitude = double.Parse(Console.ReadLine());

        Console.Write("Open Time: ");
        DateTime OpenTime = DateTime.Parse(Console.ReadLine());


        Console.Write("Max Finish Time: ");
        DateTime MaxFinishTime = DateTime.Parse(Console.ReadLine());


        Console.Write("Call Status: ");
        string CallStatus = Console.ReadLine();
        Enums.CalltStatusEnum s = (Enums.CalltStatusEnum)Enum.Parse(typeof(Enums.CalltStatusEnum), CallStatus.ToUpper());


        return new BO.Call
        {
            Id = Id,
            CallType = c,
            VerbDesc = VerbDesc,
            Address = Address,
            Latitude = Latitude,
            Longitude = longitude,
            OpenTime = OpenTime,
            MaxFinishTime = MaxFinishTime,
            CallStatus = s,

        };





    }

}






