using Dal;
using DalApi;
using Daltest;
using DO;

namespace DalTest
{
    internal class Program
    {
        // Static fields for DAL interfaces
        private static IVolunteer? s_dalVolunteer = new VolunteerImplementation(); // DAL for volunteers
        private static ICall? s_dalCall = new CallImplementation();       // DAL for calls
        private static IAssignment? s_dalAssignment = new AssignmentImplementation(); // DAL for assignments
        private static IConfig? s_dalConfig = new ConfigImplementation();         // DAL for configuration
        static void Main(string[] args)
        {
            try
            {
                RunMainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }

        private static void RunMainMenu()
        {
            while (true)
            {
                // Display the main menu options
                Console.WriteLine("\nMain Menu:");
                Console.WriteLine("1 - Volunteer Menu"); // Option to access the volunteer menu
                Console.WriteLine("2 - Call Menu");      // Option to access the call menu
                Console.WriteLine("3 - Assignment Menu"); // Option to access the assignment menu
                Console.WriteLine("4 - Config Menu");    // Option to access the configuration menu
                Console.WriteLine("5 - Initialize Database"); // Option to initialize the database
                Console.WriteLine("6 - Show Database");  // Option to display the database
                Console.WriteLine("7 - Reset Database"); // Option to reset the database
                Console.WriteLine("0 - Exit");           // Option to exit the program
                Console.Write("Enter your choice: ");

                // Parse user input and validate it
                if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > 7)
                {
                    Console.WriteLine("Invalid choice, try again."); // Notify user of invalid input
                    continue;
                }

                // Execute the appropriate action based on user choice
                switch (choice)
                {
                    case 1:
                        RunVolunteerMenu(); // Navigate to volunteer menu
                        break;
                    case 2:
                        RunCallMenu(); // Navigate to call menu
                        break;
                    case 3:
                        RunAssignmentMenu(); // Navigate to assignment menu
                        break;
                    case 4:
                        RunConfigMenu(); // Navigate to configuration menu
                        break;
                    case 5:
                        InitializeDatabase(); // Initialize the database
                        break;
                    case 6:
                        ShowDatabase(s_dalVolunteer, s_dalCall, s_dalAssignment); // Display the database contents
                        break;
                    case 7:
                        ResetDatabase(); // Reset the database to default state
                        break;
                    case 0:
                        return; // Exit the main menu and terminate the program
                }
            }
        }


        private static void RunVolunteerMenu()
        {
            while (true)
            {
                // Display the Volunteer Menu options
                Console.WriteLine("\nVolunteer Menu:");
                Console.WriteLine("1 - Create");           // Option to create a new volunteer
                Console.WriteLine("2 - Read");             // Option to read a specific volunteer by ID
                Console.WriteLine("3 - Read All");         // Option to read all volunteers
                Console.WriteLine("4 - Update");           // Option to update an existing volunteer
                Console.WriteLine("5 - Delete");           // Option to delete a specific volunteer by ID
                Console.WriteLine("6 - Delete All");       // Option to delete all volunteers
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
                            Volunteer v1 = GetVolunteerFromUser(); // Get new volunteer data from user
                            s_dalVolunteer.Create(v1);            // Create the volunteer in the database
                            Console.WriteLine("Volunteer created."); // Notify the user
                            break;
                        case 2:
                            Console.WriteLine("Enter ID to read:");
                            int id = int.Parse(Console.ReadLine());// Get the ID to read
                            Volunteer? VolToPrint = s_dalVolunteer?.Read(id);  // Fetch and display the volunteer data
                            PrintReadVolunteer(VolToPrint);
                            break;
                        case 3:
                            //Volunteer v = GetVolunteerFromUser(); // Get new volunteer data from user
                            List<Volunteer>? VolListToPrint = s_dalVolunteer?.ReadAll();
                            PrintVolunteerList(VolListToPrint); // Display all volunteers
                            break;
                        case 4:
                            Volunteer v2 = GetVolunteerFromUser(); // Get updated volunteer data from user
                            s_dalVolunteer.Update(v2);            // Update the volunteer in the database
                            Console.WriteLine("Volunteer updated."); // Notify the user
                            break;
                        case 5:
                            Console.WriteLine("Enter ID to read:");
                            int id1 = int.Parse(Console.ReadLine()); // Get the ID to delete
                            s_dalVolunteer.Delete(id1);            // Delete the volunteer from the database
                            Console.WriteLine("Volunteer deleted."); // Notify the user
                            break;
                        case 6:
                            s_dalVolunteer.DeleteAll();            // Delete all volunteers
                            Console.WriteLine("All volunteers deleted."); // Notify the user
                            break;
                        case 0:
                            return; // Exit to the Main Menu
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}"); // Handle exceptions and display error messages
                }
            }
        }

        private static void RunCallMenu()
        {
            while (true)
            {
                // Display the Call Menu options
                Console.WriteLine("\nCall Menu:");
                Console.WriteLine("1 - Create");           // Option to create a new call
                Console.WriteLine("2 - Read");             // Option to read a specific call by ID
                Console.WriteLine("3 - Read All");         // Option to read all calls
                Console.WriteLine("4 - Update");           // Option to update an existing call
                Console.WriteLine("5 - Delete");           // Option to delete a specific call by ID
                Console.WriteLine("6 - Delete All");       // Option to delete all calls
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
                            Call c1 = GetCallFromUser();      // Get new call data from user
                            s_dalCall?.Create(c1);           // Create the call in the database
                            Console.WriteLine("Call created."); // Notify the user
                            break;
                        case 2:
                            Console.WriteLine("Enter ID to read:");
                            int id = int.Parse(Console.ReadLine()); // Get the ID to read
                            Call? CallToPrint = s_dalCall?.Read(id); // Fetch and display the call data
                            PrintReadCall(CallToPrint);
                            break;
                        case 3:
                            //Call c = GetCallFromUser();  // Get new Call data from user
                            List<Call>? CallListToPrint = s_dalCall?.ReadAll();
                            PrintCallList(CallListToPrint); // Display all Calls
                            break;
                        case 4:
                            Call c2 = GetCallFromUser();      // Get updated call data from user
                            s_dalCall?.Update(c2);           // Update the call in the database
                            Console.WriteLine("Call updated."); // Notify the user
                            break;
                        case 5:
                            Console.WriteLine("Enter ID to read:");
                            int id1 = int.Parse(Console.ReadLine()); // Get the ID to delete
                            s_dalCall?.Delete(id1);           // Delete the call from the database
                            Console.WriteLine("Call deleted."); // Notify the user
                            break;
                        case 6:
                            s_dalCall?.DeleteAll();           // Delete all calls
                            Console.WriteLine("All calls deleted."); // Notify the user
                            break;
                        case 0:
                            return; // Exit to the Main Menu
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}"); // Handle exceptions and display error messages
                }
            }
        }


        private static void RunAssignmentMenu()
        {
            while (true)
            {
                Console.WriteLine("\nAssignment Menu:");
                Console.WriteLine("1 - Create");
                Console.WriteLine("2 - Read");
                Console.WriteLine("3 - Read All");
                Console.WriteLine("4 - Update");
                Console.WriteLine("5 - Delete");
                Console.WriteLine("6 - Delete All");
                Console.WriteLine("0 - Back to Main Menu");
                Console.Write("Enter your choice: ");

                if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > 6)
                {
                    Console.WriteLine("Invalid choice, try again.");
                    continue;
                }

                try
                {

                    switch (choice)
                    {
                        case 1:
                            Assignment a1 = GetAssignmentFromUser();
                            s_dalAssignment?.Create(a1);
                            Console.WriteLine("Assignment created.");
                            break;
                        case 2:
                            Console.WriteLine("Enter ID to read:");
                            int id = int.Parse(Console.ReadLine());
                            Assignment? AssignmentToPrint = s_dalAssignment?.Read(id);
                            PrintReadAssignment(AssignmentToPrint);
                            break;
                        case 3:
                            //Assignment a = GetAssignmentFromUser();  // Get new Assignment data from user
                            List<Assignment>? AssignmentListToPrint = s_dalAssignment?.ReadAll();
                            PrintAssignmentList(AssignmentListToPrint); // Display all Assignment
                            break;
                        case 4:
                            Assignment a2 = GetAssignmentFromUser();
                            s_dalAssignment?.Update(a2);
                            Console.WriteLine("Assignment updated.");
                            break;
                        case 5:
                            Console.WriteLine("Enter ID to read:");
                            int id1 = int.Parse(Console.ReadLine());
                            s_dalAssignment?.Delete(id1);
                            Console.WriteLine("Assignment deleted.");
                            break;
                        case 6:
                            s_dalAssignment?.DeleteAll();
                            Console.WriteLine("All assignments deleted.");
                            break;
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

        private static void RunConfigMenu()
        {
            while (true)
            {
                Console.WriteLine("\nConfig Menu:");
                Console.WriteLine("1 - Advance Clock by Minutes");
                Console.WriteLine("2 - Advance Clock by Hours");
                Console.WriteLine("3 - Advance Clock by Seconds");
                Console.WriteLine("4 - Show Current Clock");
                Console.WriteLine("5 - Update Risk Range");
                Console.WriteLine("6 - Reset Configurations");
                Console.WriteLine("7 - Show Current Value of a Configuration Variable");
                Console.WriteLine("0 - Back to Main Menu");
                Console.Write("Enter your choice: ");

                if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > 7)
                {
                    Console.WriteLine("Invalid choice, try again.");
                    continue;
                }

                try
                {
                    switch (choice)
                    {
                        case 1:
                            Console.Write("Enter minutes to advance: ");
                            int minutes = int.Parse(Console.ReadLine());
                            s_dalConfig.Clock = s_dalConfig.Clock.AddMinutes(minutes);
                            Console.WriteLine($"Clock advanced by {minutes} minutes.");
                            break;
                        case 2:
                            Console.Write("Enter hours to advance: ");
                            int hours = int.Parse(Console.ReadLine());
                            s_dalConfig.Clock = s_dalConfig.Clock.AddHours(hours);
                            Console.WriteLine($"Clock advanced by {hours} hours.");
                            break;
                        case 3:
                            Console.Write("Enter seconds to advance: ");
                            int seconds = int.Parse(Console.ReadLine());
                            s_dalConfig.Clock = s_dalConfig.Clock.AddSeconds(seconds);
                            Console.WriteLine($"Clock advanced by {seconds} seconds.");
                            break;
                        case 4:
                            Console.WriteLine($"Current Clock: {s_dalConfig.Clock}");
                            break;
                        case 5:
                            Console.Write("Enter new Risk Range (hours): ");
                            int riskHours = int.Parse(Console.ReadLine());
                            s_dalConfig.RiskRange = TimeSpan.FromHours(riskHours);
                            Console.WriteLine($"Risk Range updated to {riskHours} hours.");
                            break;
                        case 6:
                            s_dalConfig.Reset();
                            Console.WriteLine("Configurations reset to default.");
                            break;
                        case 7:
                            ShowConfigVariable();
                            break;
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

        private static void ShowConfigVariable()
        {
            Console.WriteLine("\nAvailable Configuration Variables:");
            Console.WriteLine("1 - Clock");
            Console.WriteLine("2 - Risk Range");
            Console.Write("Enter the number of the variable to view: ");

            if (!int.TryParse(Console.ReadLine(), out int variableChoice) || variableChoice < 1 || variableChoice > 2)
            {
                Console.WriteLine("Invalid choice, returning to Config Menu.");
                return;
            }

            switch (variableChoice)
            {
                case 1:
                    Console.WriteLine($"Current Clock: {s_dalConfig.Clock}");
                    break;
                case 2:
                    Console.WriteLine($"Current Risk Range: {s_dalConfig.RiskRange}");
                    break;
            }
        }


        private static void InitializeDatabase()///////////////////////////////////
        {
            Initialization.Do(s_dalVolunteer, s_dalCall, s_dalAssignment, s_dalConfig);
            Console.WriteLine("Database initialized.");
        }

        private static void ResetDatabase()///////////////////////////////
        {
            s_dalVolunteer.DeleteAll();
            s_dalCall.DeleteAll();
            s_dalAssignment.DeleteAll();
            s_dalConfig.Reset();
            Console.WriteLine("Database and configurations reset.");
        }
        public static Volunteer GetVolunteerFromUser()
        {

            Console.WriteLine("Enter Volunteer Details:");


            Console.Write("ID: ");
            string id = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or empty");
            }
            else
            {

                Console.Write("Full Name: ");
                string fullName = Console.ReadLine();

                Console.Write("Phone Number: ");
                string phoneNumber = Console.ReadLine();

                Console.Write("Email: ");
                string email = Console.ReadLine();

                Console.Write("Password: ");
                string password = Console.ReadLine();

                Console.Write("Active (true/false): ");
                bool active = bool.Parse(Console.ReadLine());

                Console.Write("Distance Type: ( AerialDistance/walkingDistance/drivingdistance): ");
                DistanceType distanceType = Enum.Parse<DistanceType>(Console.ReadLine(), true);

                Console.Write("Position: (Manager/volunteer): ");
                Position position = Enum.Parse<Position>(Console.ReadLine(), true);

                Console.Write("Location: ");
                string location = Console.ReadLine();

                Console.Write("Latitude (optional, press Enter to skip): ");
                string latitudeInput = Console.ReadLine();
                double? latitude = string.IsNullOrWhiteSpace(latitudeInput) ? null : double.Parse(latitudeInput);

                Console.Write("Longitude (optional, press Enter to skip): ");
                string longitudeInput = Console.ReadLine();
                double? longitude = string.IsNullOrWhiteSpace(longitudeInput) ? null : double.Parse(longitudeInput);

                Console.Write("Max Distance (optional, press Enter to skip): ");
                string maxDistanceInput = Console.ReadLine();
                double? maxDistance = string.IsNullOrWhiteSpace(maxDistanceInput) ? null : double.Parse(maxDistanceInput);

                return new Volunteer
                {

                    Id = int.Parse(id),
                    FullName = fullName,
                    PhoneNumber = phoneNumber,
                    Email = email,
                    Password = password,
                    Active = active,
                    DistanceType = distanceType,
                    Position = position,
                    Location = location,
                    Latitude = latitude,
                    Longitude = longitude,
                    MaxDistance = maxDistance
                };
            }
            //}
        }
        public static Call GetCallFromUser()
        {
            Console.WriteLine("Enter Call Details:");

            Console.Write("ID: ");
            int id = int.Parse(Console.ReadLine());

            Console.Write("Address: ");
            string address = Console.ReadLine();

            Console.Write("Latitude: ");
            double latitude = double.Parse(Console.ReadLine());

            Console.Write("Longitude: ");
            double longitude = double.Parse(Console.ReadLine());

            Console.Write("Open Time (yyyy-MM-dd HH:mm:ss): ");
            DateTime openTime = DateTime.Parse(Console.ReadLine());

            Console.Write("Call Type ( PreparingFood/TransportingFood/FixingEquipment/ProvidingShelter/TransportAssistance/MedicalAssistance/EmotionalSupport/PackingSupplies): ");
            CallType callType = Enum.Parse<CallType>(Console.ReadLine(), true);

            Console.Write("Verbal Description (optional, press Enter to skip): ");
            string? verbDesc = Console.ReadLine();
            verbDesc = string.IsNullOrWhiteSpace(verbDesc) ? null : verbDesc;

            Console.Write("Max Time (yyyy-MM-dd HH:mm:ss): ");
            DateTime maxTime = DateTime.Parse(Console.ReadLine());

            return new Call
            {
                Id = id,
                Adress = address,
                Latitude = latitude,
                Longitude = longitude,
                OpenTime = openTime,
                CallType = callType,
                VerbDesc = verbDesc,
                MaxTime = maxTime
            };
        }
        public static Assignment GetAssignmentFromUser()
        {
            Console.WriteLine("Enter Assignment Details:");

            Console.Write("ID: ");
            int id = int.Parse(Console.ReadLine());

            Console.Write("Call ID: ");
            int callId = int.Parse(Console.ReadLine());

            Console.Write("Volunteer ID: ");
            int volunteerId = int.Parse(Console.ReadLine());

            Console.Write("Appointment Time (yyyy-MM-dd HH:mm:ss): ");
            DateTime appointmentTime = DateTime.Parse(Console.ReadLine());

            Console.Write("Finish Appointment Time (optional, press Enter to skip): ");
            string finishAppointmentTimeInput = Console.ReadLine();
            DateTime? finishAppointmentTime = string.IsNullOrWhiteSpace(finishAppointmentTimeInput)
                ? null
                : DateTime.Parse(finishAppointmentTimeInput);

            Console.Write("Finish Appointment Type (optional, press Enter to skip): ");
            string finishAppointmentTypeInput = Console.ReadLine();
            Enum? finishAppointmentType = string.IsNullOrWhiteSpace(finishAppointmentTypeInput)
                ? null
                : Enum.Parse<FinishAppointmentType>(finishAppointmentTypeInput, true);

            return new Assignment
            {
                Id = id,
                CallId = callId,
                VolunteerId = volunteerId,
                AppointmentTime = appointmentTime,
                FinishAppointmentTime = finishAppointmentTime,
                FinishAppointmentType = finishAppointmentType
            };
        }
        private static int volunteerRead()
        {
            Console.WriteLine("Enter volunteer ID:");  // Prompt the user to enter the volunteer ID
            int id;  // Declare a variable to hold the volunteer ID
            while (!int.TryParse(Console.ReadLine(), out id))  // Check if the input is a valid integer
                Console.WriteLine("Invalid input. Please enter a valid ID:");  // Prompt again if the input is invalid

            Volunteer? volunteer = s_dalVolunteer?.Read(id);  // Try to read the volunteer with the provided ID

            if (volunteer != null)  // If a volunteer is found, display their information
                Console.WriteLine(volunteer);
            else  // If no volunteer is found, inform the user
                Console.WriteLine("Volunteer not found.");
            return id;
        }
        public static void PrintReadVolunteer(Volunteer v)
        {
            if (v == null)
                Console.WriteLine("ID is not found");
            else
                Console.WriteLine(v);
        }
        public static void PrintReadCall(Call c)
        {

            if (c == null)
                Console.WriteLine("ID is not found");
            else
                Console.WriteLine(c);
        }
        public static void PrintReadAssignment(Assignment a)
        {
            if (a == null)
                Console.WriteLine("ID is not found");
            else
                Console.WriteLine(a);
        }
        public static void PrintVolunteerList(List<Volunteer> volList)
        {
            if (volList == null)
                Console.WriteLine("Volunteer list is empty");
            else
                foreach (Volunteer vol in volList)  // Iterate through each volunteer in the list
                    Console.WriteLine(vol);  // Print each volunteer's information
        }

        public static void PrintCallList(List<Call> callList)
        {
            if (callList == null)
                Console.WriteLine("Call list is empty");
            else
                foreach (Call call in callList)  // Iterate through each volunteer in the list
                    Console.WriteLine(call);  // Print each volunteer's information
        }

        public static void PrintAssignmentList(List<Assignment> assigList)
        {
            if (assigList == null)
                Console.WriteLine("Assignment List is empty");
            else
                foreach (Assignment asig in assigList)  // Iterate through each volunteer in the list
                    Console.WriteLine(asig);  // Print each volunteer's information
        }
        public static void ShowDatabase(IVolunteer? s_dalVolunteer, ICall? s_dalCall, IAssignment? s_dalAssignment)
        {

            PrintCallList(s_dalCall.ReadAll());
            PrintAssignmentList(s_dalAssignment.ReadAll());
            PrintVolunteerList(s_dalVolunteer.ReadAll());


        }
    }


}

