namespace DalTest;
using Dal;

using DalApi;
using DO;
using System;

/// <summary>
/// Handles initialization and setup of data in the DAL.
/// This includes resetting the database and populating it with initial data.
/// </summary>
public static class Initialization
{
    private static readonly Random s_rand = new(); // Random generator for generating unique data
    private static IDal? s_dal;

    /// <summary>
    /// Main entry point for initializing the DAL.
    /// - Resets the database to its default state.
    /// - Populates the database with volunteers, calls, and assignments.
    /// </summary>
    /// <param name="dal">The IDal implementation to use for data access.</param>
    /// <exception cref="NullReferenceException">Thrown if the provided DAL is null.</exception>
    public static void Do()
    {
        //s_dal = dal ?? throw new NullReferenceException("DAL object can not be null!"); //stage 2
        s_dal = DalApi.Factory.Get; //stage 4
        Console.WriteLine("Resetting configuration values and data lists.");
        s_dal.ResetDB();
        Console.WriteLine("Initializing volunteers, calls, and assignments lists.");
        createVolunteers();
        CreateCalls();
        createAssignment();
    }

    /// <summary>
    /// Creates a predefined list of volunteers and adds them to the database.
    /// - Each volunteer gets a unique ID, random phone number, and other properties.
    /// - Volunteers are distributed among predefined locations across Israel.
    /// </summary>


    private static void createVolunteers()
    {
        string[] volunteerNames = { "Yosef Cohen", "Shmuel Levi", "Yaakov Goldstein", "Moshe Friedman", "Avraham Stein",
                               "Daniel Green", "David Weiss", "Yonatan Rubin", "Hanan Levy", "Eli Karp",
                               "Uzi Sharoni", "Shimon Ben-David", "Matan Shalev", "Asher Tzukrel", "Oren Regev" };

        var addresses = new List<Tuple<string, double, double>>()
    {
        new Tuple<string, double, double>("Tel Aviv Port, Tel Aviv, Israel", 32.099791, 34.769077),
new Tuple<string, double, double>("Tel Aviv Port, Tel Aviv, Israel", 32.099791, 34.769077),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Neve Tzedek, Tel Aviv, Israel", 32.065764, 34.770022),
new Tuple<string, double, double>("Tel Aviv Port, Tel Aviv, Israel", 32.099791, 34.769077),
new Tuple<string, double, double>("Carmel Market, Tel Aviv, Israel", 32.065013, 34.768370),
new Tuple<string, double, double>("Dizengoff Square, Tel Aviv, Israel", 32.078323, 34.770340),
new Tuple<string, double, double>("Yarkon Park, Tel Aviv, Israel", 32.087573, 34.781904),
new Tuple<string, double, double>("Tel Aviv Museum of Art, Tel Aviv, Israel", 32.073418, 34.781046),
new Tuple<string, double, double>("Florentin, Tel Aviv, Israel", 32.054186, 34.759738),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Rabin Square, Tel Aviv, Israel", 32.078570, 34.768721),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),

    };

        string GenerateRandomPassword()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            return new string(Enumerable.Range(0, 12)
                .Select(_ => chars[s_rand.Next(chars.Length)])
                .ToArray());
        }

        // מנגנון יצירת IDs ייחודיים
        var usedIds = new HashSet<int>();
        int GenerateUniqueId()
        {
            int id;
            do
            {
                id = s_rand.Next(100000000, 999999999); // מספר בן 9 ספרות
            } while (usedIds.Contains(id));
            usedIds.Add(id);
            return id;
        }

        for (int i = 0; i < volunteerNames.Length; i++)
        {
            int id = GenerateUniqueId();

            // החלטת על סוג המרחק לפי MaxDistance
            DistanceType distanceType = DistanceType.AerialDistance;
            int maxDistance = s_rand.Next(5, 20);

            if (maxDistance < 5)
            {
                distanceType = DistanceType.WalkingDistance;
            }
            else if (maxDistance >= 5 && maxDistance <= 15)
            {
                distanceType = DistanceType.DrivingDistance;
            }

            try
            {
                s_dal.Volunteer.Create(new Volunteer
                {
                    Id = id,
                    FullName = volunteerNames[i],
                    PhoneNumber = $"05{s_rand.Next(10000000, 99999999)}",
                    Email = $"volunteer{i + 1}@gmail.com",
                    Location = addresses[i].Item1,
                    MaxDistance = maxDistance,
                    Position = (i == 0 || i == 1) ? Position.admin : Position.volunteer,
                    Latitude = addresses[i].Item2,
                    Longitude = addresses[i].Item3,
                    Active = s_rand.Next(0, 2) == 1,
                    DistanceType = distanceType,
                    Password = GenerateRandomPassword(),
                });

                Console.WriteLine($"Volunteer {i + 1} created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create volunteer {i + 1}: {ex.Message}");
            }
        }
    }


    /// <summary>
    /// Creates a set of call records in the databasשe.
    /// - Each call is assigned a random description, type, and location.
    /// - Open and maximum response times are set based on random offsets.
    /// </summary>
    private static void CreateCalls()
    {
        // List of addresses for volunteers in Jerusalem with precise latitudes and longitudes
        var callAddresses = new List<Tuple<string, double, double>>
    {
           new Tuple<string, double, double>("Ben Yehuda Street, Tel Aviv, Israel", 32.065500, 34.767028),
new Tuple<string, double, double>("Tel Aviv Port, Tel Aviv, Israel", 32.099791, 34.769077),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Neve Tzedek, Tel Aviv, Israel", 32.065764, 34.770022),
new Tuple<string, double, double>("Tel Aviv Port, Tel Aviv, Israel", 32.099791, 34.769077),
new Tuple<string, double, double>("Carmel Market, Tel Aviv, Israel", 32.065013, 34.768370),
new Tuple<string, double, double>("Dizengoff Square, Tel Aviv, Israel", 32.078323, 34.770340),
new Tuple<string, double, double>("Yarkon Park, Tel Aviv, Israel", 32.087573, 34.781904),
new Tuple<string, double, double>("Tel Aviv Museum of Art, Tel Aviv, Israel", 32.073418, 34.781046),
new Tuple<string, double, double>("Florentin, Tel Aviv, Israel", 32.054186, 34.759738),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Tel Aviv Cinematheque, Tel Aviv, Israel", 32.070856, 34.772402),
new Tuple<string, double, double>("Tel Aviv Port, Tel Aviv, Israel", 32.099791, 34.769077),
new Tuple<string, double, double>("Neve Tzedek, Tel Aviv, Israel", 32.065764, 34.770022),
new Tuple<string, double, double>("Carmel Market, Tel Aviv, Israel", 32.065013, 34.768370),
new Tuple<string, double, double>("Teddy Stadium, Tel Aviv, Israel", 32.065713, 34.769120),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Shalom Meir Tower, Tel Aviv, Israel", 32.063725, 34.774329),
new Tuple<string, double, double>("Old Jaffa, Tel Aviv, Israel", 32.053241, 34.759167),
new Tuple<string, double, double>("Tel Aviv Beach, Tel Aviv, Israel", 32.071535, 34.767631),
new Tuple<string, double, double>("Ramat Aviv, Tel Aviv, Israel", 32.115225, 34.803378),
new Tuple<string, double, double>("Sheinkin Street, Tel Aviv, Israel", 32.065034, 34.770086),
new Tuple<string, double, double>("Bauhaus Center, Tel Aviv, Israel", 32.071024, 34.767232),
new Tuple<string, double, double>("Neve Tzedek, Tel Aviv, Israel", 32.065764, 34.770022),
new Tuple<string, double, double>("Bograshov Street, Tel Aviv, Israel", 32.070603, 34.768185),
new Tuple<string, double, double>("Dizengoff Center, Tel Aviv, Israel", 32.075389, 34.771455),
new Tuple<string, double, double>("Givatayim, Tel Aviv, Israel", 32.070946, 34.783580),
new Tuple<string, double, double>("Kikar Hamedina, Tel Aviv, Israel", 32.078298, 34.786209),
new Tuple<string, double, double>("Sarona Market, Tel Aviv, Israel", 32.073158, 34.782633),
new Tuple<string, double, double>("Shuk HaCarmel, Tel Aviv, Israel", 32.065013, 34.768370),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Ramat Gan, Tel Aviv, Israel", 32.073208, 34.817878),
new Tuple<string, double, double>("Tel Aviv University, Tel Aviv, Israel", 32.113120, 34.804338),
new Tuple<string, double, double>("Hilton Beach, Tel Aviv, Israel", 32.095183, 34.768234),
new Tuple<string, double, double>("Kikar Hamedina, Tel Aviv, Israel", 32.078298, 34.786209),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Tel Aviv-Yafo, Tel Aviv, Israel", 32.054166, 34.765746),
new Tuple<string, double, double>("Alma Beach, Tel Aviv, Israel", 32.073487, 34.758676),
new Tuple<string, double, double>("Gan Meir, Tel Aviv, Israel", 32.070504, 34.776377),
new Tuple<string, double, double>("Shenkin Street, Tel Aviv, Israel", 32.065034, 34.770086),
new Tuple<string, double, double>("Palmach Street, Tel Aviv, Israel", 32.070043, 34.767118),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),


        // Add more addresses as needed...
    };

        if (callAddresses.Count == 0)
        {
            throw new InvalidOperationException("The call addresses list is empty. Cannot create calls.");
        }
        // הזמן הראשוני בתחילת הלולאה
        for (int i = 0; i < 70; i++)
        {
            try
            {
                // Randomly select a call type
                CallType callType = (CallType)s_rand.Next(0, Enum.GetValues(typeof(CallType)).Length);

                // Generate a description based on the call type
                string description = callType switch
                {
                    CallType.PreparingFood => "Cooking or assembling meals for those in need.",
                    CallType.TransportingFood => "Delivering prepared food to designated locations.",
                    CallType.FixingEquipment => "Repairing essential tools or equipment.",
                    CallType.ProvidingShelter => "Arranging or offering temporary accommodation.",
                    CallType.TransportAssistance => "Helping with vehicle issues or emergency rides.",
                    CallType.MedicalAssistance => "Delivering medical supplies or offering first aid.",
                    CallType.EmotionalSupport => "Providing mental health support through conversations.",
                    CallType.PackingSupplies => "Organizing and packing necessary supplies for distribution.",
                    _ => "General assistance."
                };

                // Generate call ID
                int callId = s_dal.config.NextCallId;

                // Select a random call address
                var selectedAddress = callAddresses[s_rand.Next(callAddresses.Count)];

                // Generate opening time (with random variation)
                DateTime now = DateTime.Now;  // Get the current time for each iteration

                // Generate opening time (either today or up to two weeks ago)
                int randDays = s_rand.Next(0, 14); // Between 0 and 14 days

                // Randomly decide whether to subtract hours or days
                DateTime openTime;
                if (s_rand.Next(0, 2) == 0) // 50% chance to use hours, 50% for days
                {
                    int randHours = s_rand.Next(1, 24); // Between 1 and 24 hours
                    openTime = now.AddHours(-randHours);
                }
                else
                {
                    openTime = now.AddDays(-randDays); // Between 0 and 14 days ago
                }

                // Optionally, add random minutes and seconds for more variety
                openTime = openTime.AddMinutes(-s_rand.Next(0, 60))
                                   .AddSeconds(-s_rand.Next(0, 60));

                // Print the opening time for each call (for debugging purposes)
                Console.WriteLine($"Call ID: {callId}, Opening Time: {openTime}");

                // Ensure maxTime is initialized if it wasn't done above
                DateTime? maxTime = null;

                // Generate max time with 70% chance
                if (s_rand.NextDouble() < 0.7) // 70% chance to have a maxTime
                {
                    int hoursOffset = s_rand.Next(1, 24); // Offset by 1 to 24 hours
                    maxTime = openTime.AddHours(hoursOffset);

                    // Ensure maxTime is valid
                    if (maxTime <= openTime)
                    {
                        throw new ArgumentOutOfRangeException(nameof(maxTime), "MaxTime must be greater than OpenTime.");
                    }
                }
                else
                {
                    // If maxTime wasn't generated by the 70% chance, set it to a value.
                    maxTime = openTime.AddHours(1); // Set a default MaxTime
                }

                // Create a new call
                Call newCall = new Call
                {
                    Id = callId,
                    VerbDesc = description,
                    Adress = selectedAddress.Item1,
                    OpenTime = openTime,
                    MaxTime = maxTime,
                    Latitude = selectedAddress.Item2,
                    Longitude = selectedAddress.Item3,
                    CallType = callType,
                };

                // Insert the new call into the database
                s_dal!.call.Create(newCall);

                Console.WriteLine($"Created call ID: {callId} at {selectedAddress.Item1}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create call {i + 1}: {ex.Message}");
            }
        }

    }
    private static void createAssignment()
    {
        var allCalls = s_dal!.call.ReadAll();
        var allVolunteers = s_dal!.Volunteer.ReadAll();

        // Remove the first two volunteers from the list
        var availableVolunteers = allVolunteers.Skip(2).ToList();

        // Keep track of assignments per volunteer and calls in treatment
        var assignmentsPerVolunteer = new Dictionary<int, List<int>>(); // volunteerId -> list of callIds
        var callsInTreatment = new HashSet<int>(); // callIds currently in treatment
        var assignedCalls = new HashSet<int>(); // all assigned calls

        // Get the current system time
        DateTime currentTime = DateTime.Now;

        // Select 5 volunteers who will have multiple assignments
        var specialVolunteers = availableVolunteers.Take(5).ToList();
        var regularVolunteers = availableVolunteers.Skip(5).ToList();

        // First, create assignments for special volunteers (with multiple calls)
        foreach (var volunteer in specialVolunteers)
        {
            assignmentsPerVolunteer[volunteer.Id] = new List<int>();

            // Create one active treatment
            var activeCall = allCalls.FirstOrDefault(c =>
                !assignedCalls.Contains(c.Id) &&
                c.MaxTime > currentTime);

            if (activeCall != null)
            {
                DateTime treatmentStartTime = activeCall.OpenTime.AddHours(s_rand.Next(1, 24));

                s_dal!.assignment.Create(new Assignment(0, activeCall.Id, volunteer.Id,
                    treatmentStartTime, null, null));

                assignmentsPerVolunteer[volunteer.Id].Add(activeCall.Id);
                assignedCalls.Add(activeCall.Id);
                callsInTreatment.Add(activeCall.Id);
            }

            // Create 3 additional completed/expired assignments
            for (int i = 0; i < 3; i++)
            {
                var call = allCalls.FirstOrDefault(c =>
                    !assignedCalls.Contains(c.Id));

                if (call != null)
                {
                    DateTime treatmentStartTime = call.OpenTime.AddHours(s_rand.Next(1, 24));
                    DateTime? treatmentEndTime;
                    FinishAppointmentType? status;

                    // Alternate between treated and expired
                    if (call.MaxTime <= currentTime)
                    {
                        status = FinishAppointmentType.CancellationHasExpired;
                        treatmentEndTime = call.MaxTime;
                    }
                    else
                    {
                        status = FinishAppointmentType.WasTreated;
                        treatmentEndTime = treatmentStartTime.AddHours(s_rand.Next(1, 24));
                        if (call.MaxTime.HasValue && treatmentEndTime > call.MaxTime.Value)
                        {
                            treatmentEndTime = call.MaxTime.Value.AddHours(-1);
                        }
                    }

                    s_dal!.assignment.Create(new Assignment(0, call.Id, volunteer.Id,
                        treatmentStartTime, treatmentEndTime, status));

                    assignmentsPerVolunteer[volunteer.Id].Add(call.Id);
                    assignedCalls.Add(call.Id);
                }
            }
        }

        // Create single assignments for regular volunteers
        foreach (var volunteer in regularVolunteers)
        {
            var call = allCalls.FirstOrDefault(c =>
                !assignedCalls.Contains(c.Id));

            if (call != null)
            {
                DateTime treatmentStartTime = call.OpenTime.AddHours(s_rand.Next(1, 24));
                DateTime? treatmentEndTime;
                FinishAppointmentType? status;

                if (call.MaxTime <= currentTime)
                {
                    status = FinishAppointmentType.CancellationHasExpired;
                    treatmentEndTime = call.MaxTime;
                }
                else if (s_rand.Next(2) == 0)
                {
                    status = FinishAppointmentType.WasTreated;
                    treatmentEndTime = treatmentStartTime.AddHours(s_rand.Next(1, 24));
                    if (call.MaxTime.HasValue && treatmentEndTime > call.MaxTime.Value)
                    {
                        treatmentEndTime = call.MaxTime.Value.AddHours(-1);
                    }
                }
                else
                {
                    status = null;
                    treatmentEndTime = null;
                }

                s_dal!.assignment.Create(new Assignment(0, call.Id, volunteer.Id,
                    treatmentStartTime, treatmentEndTime, status));

                assignedCalls.Add(call.Id);
            }
        }
    }




}

