namespace DalTest;
using Dal;
using DalApi;
using DalTest;
using DO;

/// <summary>
/// This program serves as the main interface for interacting with the DAL (Data Access Layer).
/// It provides various menus for managing volunteers, calls, assignments, configurations, and database operations.
/// </summary>
internal class Program
{
    // Static field for accessing the DAL interface
    /*static readonly IDal s_dal = new DalList(); *///stage 2
    //static readonly IDal s_dal = new DalXml(); //stage 3
    static readonly IDal s_dal = Factory.Get; //stage 4


    /// <summary>
    /// The main entry point of the application.
    /// Initializes the database and launches the main menu.
    /// </summary>
    /// <param name="args">Command-line arguments (not used).</param>
    static void Main(string[] args)
    {
        try
        {
            // Initialize the database using the DAL instance
            //Initialization.Do(s_dal);//stage 2
            Initialization.Do(); //stage 4


            // Run the main menu
            RunMainMenu();
        }
        catch (Exception ex)
        {
            // Handle any errors that occur during initialization or execution
            Console.WriteLine($"Error occurred: {ex.Message}");
        }
    }

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
                    RunCallMenu(); // Navigate to call menu (not implemented in this snippet)
                    break;
                case 3:
                    RunAssignmentMenu(); // Navigate to assignment menu (not implemented in this snippet)
                    break;
                case 4:
                    RunConfigMenu(); // Navigate to configuration menu (not implemented in this snippet)
                    break;
                case 5:
                    InitializeDatabase(); // Initialize the database (not implemented in this snippet)
                    break;
                case 6:
                    ShowDatabase(s_dal); // Display the database contents (not implemented in this snippet)
                    break;
                case 7:
                    ResetDatabase(); // Reset the database to default state (not implemented in this snippet)
                    break;
                case 0:
                    return; // Exit the main menu and terminate the program
            }
        }
    }

    /// <summary>
    /// Displays and manages the Volunteer Menu options.
    /// Allows CRUD operations for managing volunteers in the database.
    /// </summary>
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
                        // Get new volunteer data from user
                        Volunteer v1 = GetVolunteerFromUser();
                        s_dal.Volunteer.Create(v1); // Create the volunteer in the database
                        Console.WriteLine("Volunteer created."); // Notify the user
                        break;

                    case 2:
                        Console.WriteLine("Enter ID to read:");
                        int id = int.Parse(Console.ReadLine()); // Get the ID to read
                        Volunteer? VolToPrint = s_dal.Volunteer?.Read(id);  // Fetch and display the volunteer data
                        PrintReadVolunteer(VolToPrint); // Print details of the volunteer
                        break;

                    case 3:
                        // Retrieve a list of all volunteers
                        List<Volunteer>? VolListToPrint = s_dal.Volunteer?.ReadAll(volunteer => true)?.ToList();
                        PrintVolunteerList(VolListToPrint); // Display all volunteers
                        break;

                    case 4:
                        // Get updated volunteer data from user
                        Volunteer v2 = GetVolunteerFromUser();
                        s_dal.Volunteer?.Update(v2); // Update the volunteer in the database
                        Console.WriteLine("Volunteer updated."); // Notify the user
                        break;

                    case 5:
                        Console.WriteLine("Enter ID to delete:");
                        int id1 = int.Parse(Console.ReadLine()); // Get the ID to delete
                        s_dal.Volunteer?.Delete(id1); // Delete the volunteer from the database
                        Console.WriteLine("Volunteer deleted."); // Notify the user
                        break;

                    case 6:
                        s_dal.Volunteer?.DeleteAll(); // Delete all volunteers
                        Console.WriteLine("All volunteers deleted."); // Notify the user
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


    /// <summary>
    /// Runs the Call Menu, allowing the user to manage calls.
    /// Users can create, read, update, or delete calls.
    /// </summary>
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
                continue; // Restart the loop for valid input
            }

            try
            {
                // Execute the appropriate action based on user choice
                switch (choice)
                {
                    case 1:
                        // Create a new call
                        Call c1 = GetCallFromUser();      // Prompt the user for call details
                        s_dal.call?.Create(c1);           // Add the call to the database
                        Console.WriteLine("Call created."); // Confirm to the user
                        break;
                    case 2:
                        // Read a specific call by ID
                        Console.WriteLine("Enter ID to read:");
                        int id = int.Parse(Console.ReadLine()); // Read user input for ID
                        Call? CallToPrint = s_dal.call?.Read(id); // Retrieve the call from the database
                        PrintReadCall(CallToPrint);             // Display the call details
                        break;
                    case 3:
                        // Read all calls
                        List<Call>? CallListToPrint = s_dal.call?.ReadAll(Call => true)?.ToList(); // Fetch all calls
                        PrintCallList(CallListToPrint); // Display all calls
                        break;
                    case 4:
                        // Update an existing call
                        Call c2 = GetCallFromUser();      // Prompt the user for updated call details
                        s_dal.call?.Update(c2);           // Update the call in the database
                        Console.WriteLine("Call updated."); // Confirm to the user
                        break;
                    case 5:
                        // Delete a specific call by ID
                        Console.WriteLine("Enter ID to delete:");
                        int id1 = int.Parse(Console.ReadLine()); // Read user input for ID
                        s_dal.call?.Delete(id1);           // Delete the call from the database
                        Console.WriteLine("Call deleted."); // Confirm to the user
                        break;
                    case 6:
                        // Delete all calls
                        s_dal.call?.DeleteAll();           // Remove all calls from the database
                        Console.WriteLine("All calls deleted."); // Confirm to the user
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

    /// <summary>
    /// Runs the Assignment Menu, allowing the user to manage assignments.
    /// Users can create, read, update, or delete assignments.
    /// </summary>
    private static void RunAssignmentMenu()
    {
        while (true)
        {
            // Display the Assignment Menu options
            Console.WriteLine("\nAssignment Menu:");
            Console.WriteLine("1 - Create");           // Option to create a new assignment
            Console.WriteLine("2 - Read");             // Option to read a specific assignment by ID
            Console.WriteLine("3 - Read All");         // Option to read all assignments
            Console.WriteLine("4 - Update");           // Option to update an existing assignment
            Console.WriteLine("5 - Delete");           // Option to delete a specific assignment by ID
            Console.WriteLine("6 - Delete All");       // Option to delete all assignments
            Console.WriteLine("0 - Back to Main Menu");// Option to return to the Main Menu
            Console.Write("Enter your choice: ");

            // Parse user input and validate it
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > 6)
            {
                Console.WriteLine("Invalid choice, try again."); // Notify user of invalid input
                continue; // Restart the loop for valid input
            }

            try
            {
                // Execute the appropriate action based on user choice
                switch (choice)
                {
                    case 1:
                        // Create a new assignment
                        Assignment a1 = GetAssignmentFromUser(); // Prompt the user for assignment details
                        s_dal.assignment?.Create(a1);           // Add the assignment to the database
                        Console.WriteLine("Assignment created."); // Confirm to the user
                        break;
                    case 2:
                        // Read a specific assignment by ID
                        Console.WriteLine("Enter ID to read:");
                        int id = int.Parse(Console.ReadLine()); // Read user input for ID
                        Assignment? AssignmentToPrint = s_dal.assignment?.Read(id); // Retrieve the assignment from the database
                        PrintReadAssignment(AssignmentToPrint); // Display the assignment details
                        break;
                    case 3:
                        // Read all assignments
                        List<Assignment>? AssignmentListToPrint = s_dal.assignment?.ReadAll(Assignment => true)?.ToList(); // Fetch all assignments
                        PrintAssignmentList(AssignmentListToPrint); // Display all assignments
                        break;
                    case 4:
                        // Update an existing assignment
                        Assignment a2 = GetAssignmentFromUser(); // Prompt the user for updated assignment details
                        s_dal.assignment?.Update(a2);           // Update the assignment in the database
                        Console.WriteLine("Assignment updated."); // Confirm to the user
                        break;
                    case 5:
                        // Delete a specific assignment by ID
                        Console.WriteLine("Enter ID to delete:");
                        int id1 = int.Parse(Console.ReadLine()); // Read user input for ID
                        s_dal.assignment?.Delete(id1);           // Delete the assignment from the database
                        Console.WriteLine("Assignment deleted."); // Confirm to the user
                        break;
                    case 6:
                        // Delete all assignments
                        s_dal.assignment?.DeleteAll();           // Remove all assignments from the database
                        Console.WriteLine("All assignments deleted."); // Confirm to the user
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


    /// <summary>
    /// Displays the configuration menu and handles user input for clock and configuration updates.
    /// </summary>
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
                        s_dal.config.Clock = s_dal.config.Clock.AddMinutes(minutes);
                        Console.WriteLine($"Clock advanced by {minutes} minutes.");
                        break;
                    case 2:
                        Console.Write("Enter hours to advance: ");
                        int hours = int.Parse(Console.ReadLine());
                        s_dal.config.Clock = s_dal.config.Clock.AddHours(hours);
                        Console.WriteLine($"Clock advanced by {hours} hours.");
                        break;
                    case 3:
                        Console.Write("Enter seconds to advance: ");
                        int seconds = int.Parse(Console.ReadLine());
                        s_dal.config.Clock = s_dal.config.Clock.AddSeconds(seconds);
                        Console.WriteLine($"Clock advanced by {seconds} seconds.");
                        break;
                    case 4:
                        Console.WriteLine($"Current Clock: {s_dal.config.Clock}");
                        break;
                    case 5:
                        Console.Write("Enter new Risk Range (hours): ");
                        int riskHours = int.Parse(Console.ReadLine());
                        s_dal.config.RiskRange = TimeSpan.FromHours(riskHours);
                        Console.WriteLine($"Risk Range updated to {riskHours} hours.");
                        break;
                    case 6:
                        s_dal.config.Reset();
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
    // End of RunConfigMenu method.

    /// <summary>
    /// Displays the current value of a specific configuration variable.
    /// </summary>
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
                Console.WriteLine($"Current Clock: {s_dal.config.Clock}");
                break;
            case 2:
                Console.WriteLine($"Current Risk Range: {s_dal.config.RiskRange}");
                break;
        }
    }
    // End of ShowConfigVariable method.

    /// <summary>
    /// Initializes the database by calling the initialization function.
    /// </summary>
    private static void InitializeDatabase()
    {
        Initialization.Do();
        Console.WriteLine("Database initialized.");
    }
    // End of InitializeDatabase method.

    /// <summary>
    /// Resets the database and all configurations to their default state.
    /// </summary>
    private static void ResetDatabase()
    {
        s_dal.Volunteer.DeleteAll();
        s_dal.call.DeleteAll();
        s_dal.assignment.DeleteAll();
        s_dal.config.Reset();
        Console.WriteLine("Database and configurations reset.");
    }
    // End of ResetDatabase method.

    /// <summary>
    /// Prompts the user for volunteer details and creates a new Volunteer object.
    /// </summary>
    /// <returns>A new Volunteer object with the provided details.</returns>
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
            string? fullName = Console.ReadLine();

            Console.Write("Phone Number: ");
            string? phoneNumber = Console.ReadLine();

            Console.Write("Email: ");
            string? email = Console.ReadLine();

            Console.Write("Password: ");
            string? password = Console.ReadLine();

            Console.Write("Active (true/false): ");
            bool active = bool.Parse(Console.ReadLine());

            Console.Write("Distance Type: ( AerialDistance/walkingDistance/drivingdistance): ");
            DistanceType distanceType = Enum.Parse<DistanceType>(Console.ReadLine(), true);

            Console.Write("Position: (Manager/volunteer): ");
            Position position = Enum.Parse<Position>(Console.ReadLine(), true);

            Console.Write("Location: ");
            string location = Console.ReadLine();

            Console.Write("Latitude (optional, press Enter to skip): ");
            string? latitudeInput = Console.ReadLine();
            double? latitude = string.IsNullOrWhiteSpace(latitudeInput) ? null : double.Parse(latitudeInput);

            Console.Write("Longitude (optional, press Enter to skip): ");
            string? longitudeInput = Console.ReadLine();
            double? longitude = string.IsNullOrWhiteSpace(longitudeInput) ? null : double.Parse(longitudeInput);

            Console.Write("Max Distance (optional, press Enter to skip): ");
            string? maxDistanceInput = Console.ReadLine();
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
    }
    /// <summary>
    /// Prompts the user for call details and creates a new Call object.
    /// </summary>
    /// <returns>A new Call object with the provided details.</returns>
    public static Call GetCallFromUser()
    {
        Console.WriteLine("Enter Call Details:");

        Console.Write("ID: ");
        int id = int.Parse(Console.ReadLine());

        Console.Write("Address: ");
        string? address = Console.ReadLine();

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
    // End of GetCallFromUser method.

    /// <summary>
    /// Prompts the user for assignment details and creates a new Assignment object.
    /// </summary>
    /// <returns>A new Assignment object with the provided details.</returns>
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
    // End of GetAssignmentFromUser method.

    /// <summary>
    /// Reads a volunteer ID from the user and attempts to retrieve and display the corresponding volunteer details.
    /// </summary>
    /// <returns>The volunteer ID entered by the user.</returns>
    private static int volunteerRead()
    {
        Console.WriteLine("Enter volunteer ID:");  // Prompt the user to enter the volunteer ID
        int id;  // Declare a variable to hold the volunteer ID
        while (!int.TryParse(Console.ReadLine(), out id))  // Check if the input is a valid integer
            Console.WriteLine("Invalid input. Please enter a valid ID:");  // Prompt again if the input is invalid

        Volunteer? volunteer = s_dal.Volunteer?.Read(id);  // Try to read the volunteer with the provided ID

        if (volunteer != null)  // If a volunteer is found, display their information
            Console.WriteLine(volunteer);
        else  // If no volunteer is found, inform the user
            Console.WriteLine("Volunteer not found.");
        return id;
    }

    /// <summary>
    /// Prints the details of a Volunteer object, or indicates if the ID is not found.
    /// </summary>
    /// <param name="v">The Volunteer object to print.</param>
    public static void PrintReadVolunteer(Volunteer v)
    {
        if (v == null)
            Console.WriteLine("ID is not found");
        else
            Console.WriteLine(v);
    }

    /// <summary>
    /// Prints the details of a Call object, or indicates if the ID is not found.
    /// </summary>
    /// <param name="c">The Call object to print.</param>
    public static void PrintReadCall(Call c)
    {
        if (c == null)
            Console.WriteLine("ID is not found");
        else
            Console.WriteLine(c);
    }

    /// <summary>
    /// Prints the details of an Assignment object, or indicates if the ID is not found.
    /// </summary>
    /// <param name="a">The Assignment object to print.</param>
    public static void PrintReadAssignment(Assignment a)
    {
        if (a == null)
            Console.WriteLine("ID is not found");
        else
            Console.WriteLine(a);
    }

    /// <summary>
    /// Prints a list of Volunteer objects. If the list is null or empty, displays a corresponding message.
    /// </summary>
    /// <param name="volList">The list of Volunteer objects to print.</param>
    public static void PrintVolunteerList(List<Volunteer> volList)
    {
        if (volList == null)
            Console.WriteLine("Volunteer list is empty");
        else
            foreach (Volunteer vol in volList)  // Iterate through each volunteer in the list
                Console.WriteLine(vol);  // Print each volunteer's information
    }

    /// <summary>
    /// Prints a list of Call objects. If the list is null or empty, displays a corresponding message.
    /// </summary>
    /// <param name="callList">The list of Call objects to print.</param>
    public static void PrintCallList(List<Call> callList)
    {
        if (callList == null)
            Console.WriteLine("Call list is empty");
        else
            foreach (Call call in callList)  // Iterate through each call in the list
                Console.WriteLine(call);  // Print each call's information
    }

    /// <summary>
    /// Prints a list of Assignment objects. If the list is null or empty, displays a corresponding message.
    /// </summary>
    /// <param name="assigList">The list of Assignment objects to print.</param>
    public static void PrintAssignmentList(List<Assignment> assigList)
    {
        if (assigList == null)
            Console.WriteLine("Assignment List is empty");
        else
            foreach (Assignment asig in assigList)  // Iterate through each assignment in the list
                Console.WriteLine(asig);  // Print each assignment's information
    }

    /// <summary>
    /// Displays the contents of the database, including calls, assignments, and volunteers.
    /// </summary>
    /// <param name="s_dal">The data access layer to retrieve data from.</param>
    public static void ShowDatabase(IDal s_dal)
    {
        PrintCallList(s_dal.call.ReadAll().ToList()); // Prints call list
        PrintAssignmentList(s_dal.assignment.ReadAll().ToList()); // Prints assignment list
        PrintVolunteerList(s_dal.Volunteer.ReadAll().ToList()); // Prints volunteer list
    }
}
// End of ShowDatabase method.
