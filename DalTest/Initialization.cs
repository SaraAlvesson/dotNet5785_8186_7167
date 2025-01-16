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
        createCalls();
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
        new Tuple<string, double, double>("Western Wall, Jerusalem, Israel", 31.776694, 35.234524),
        new Tuple<string, double, double>("Jaffa Gate, Jerusalem, Israel", 31.783499, 35.233924),
        new Tuple<string, double, double>("Yad Vashem, Jerusalem, Israel", 31.774387, 35.175800),
        new Tuple<string, double, double>("Mount of Olives, Jerusalem, Israel", 31.776772, 35.261929),
        new Tuple<string, double, double>("Israel Museum, Jerusalem, Israel", 31.776723, 35.213707),
        new Tuple<string, double, double>("Mahane Yehuda Market, Jerusalem, Israel", 31.774524, 35.220499),
        new Tuple<string, double, double>("Old City, Jerusalem, Israel", 31.768319, 35.213745),
        new Tuple<string, double, double>("Church of the Holy Sepulchre, Jerusalem, Israel", 31.778159, 35.230828),
        new Tuple<string, double, double>("The Knesset, Jerusalem, Israel", 31.768898, 35.225121),
        new Tuple<string, double, double>("Mount Herzl, Jerusalem, Israel", 31.749554, 35.211941),
        new Tuple<string, double, double>("Zion Gate, Jerusalem, Israel", 31.773584, 35.233090),
        new Tuple<string, double, double>("Sacher Park, Jerusalem, Israel", 31.760126, 35.217681),
        new Tuple<string, double, double>("Ein Karem, Jerusalem, Israel", 31.762759, 35.167712),
        new Tuple<string, double, double>("Hebron Road, Jerusalem, Israel", 31.735084, 35.222445),
        new Tuple<string, double, double>("Montefiore Windmill, Jerusalem, Israel", 31.766133, 35.235729)
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
    private static void createCalls()
    {
        // List of addresses for volunteers in Jerusalem with precise latitudes and longitudes
        var callAddresses = new List<Tuple<string, double, double>>()
    {
            new Tuple<string, double, double>("Ben Yehuda Street, Jerusalem, Israel", 31.768004, 35.213710),
 new Tuple<string, double, double>("Mamilla Mall, Jerusalem, Israel", 31.776788, 35.233570),
 new Tuple<string, double, double>("Jaffa Road, Jerusalem, Israel", 31.772611, 35.227500),
 new Tuple<string, double, double>("Safra Square, Jerusalem, Israel", 31.768594, 35.211436),
 new Tuple<string, double, double>("Sultan Suleiman Street, Jerusalem, Israel", 31.781348, 35.224762),
 new Tuple<string, double, double>("King David Street, Jerusalem, Israel", 31.766828, 35.216717),
 new Tuple<string, double, double>("Davidka Square, Jerusalem, Israel", 31.763445, 35.211122),
 new Tuple<string, double, double>("Rehavia, Jerusalem, Israel", 31.770648, 35.207551),
 new Tuple<string, double, double>("Shimon Peres Park, Jerusalem, Israel", 31.758640, 35.220455),
 new Tuple<string, double, double>("Mount Scopus, Jerusalem, Israel", 31.784407, 35.235450),
 new Tuple<string, double, double>("Talpiot, Jerusalem, Israel", 31.747506, 35.207860),
 new Tuple<string, double, double>("Hebron Road, Jerusalem, Israel", 31.740900, 35.220900),
 new Tuple<string, double, double>("Ein Kerem, Jerusalem, Israel", 31.762911, 35.161344),
 new Tuple<string, double, double>("Givat Ram, Jerusalem, Israel", 31.783134, 35.204942),
 new Tuple<string, double, double>("Yemin Moshe, Jerusalem, Israel", 31.761394, 35.234779),
 new Tuple<string, double, double>("Zion Square, Jerusalem, Israel", 31.766071, 35.222359),
 new Tuple<string, double, double>("Sacher Park, Jerusalem, Israel", 31.760126, 35.217681),
 new Tuple<string, double, double>("Mount Herzl, Jerusalem, Israel", 31.749554, 35.211941),
 new Tuple<string, double, double>("City Center, Jerusalem, Israel", 31.768245, 35.213458),
 new Tuple<string, double, double>("Mahane Yehuda Market, Jerusalem, Israel", 31.774524, 35.220499),
 new Tuple<string, double, double>("Old City, Jerusalem, Israel", 31.768319, 35.213745),
 new Tuple<string, double, double>("Church of the Holy Sepulchre, Jerusalem, Israel", 31.778159, 35.230828),
 new Tuple<string, double, double>("The Knesset, Jerusalem, Israel", 31.768898, 35.225121),
 new Tuple<string, double, double>("Yad Vashem, Jerusalem, Israel", 31.774387, 35.175800),
 new Tuple<string, double, double>("Western Wall, Jerusalem, Israel", 31.776694, 35.234524),
 new Tuple<string, double, double>("Montefiore Windmill, Jerusalem, Israel", 31.766133, 35.235729),
 new Tuple<string, double, double>("Ein Karem, Jerusalem, Israel", 31.762759, 35.167712),
 new Tuple<string, double, double>("Zion Gate, Jerusalem, Israel", 31.773584, 35.233090),
 new Tuple<string, double, double>("Rehavia, Jerusalem, Israel", 31.770214, 35.210789),
 new Tuple<string, double, double>("Binyanei Hauma, Jerusalem, Israel", 31.764568, 35.198928),
 new Tuple<string, double, double>("Jerusalem Boulevard, Jerusalem, Israel", 31.772078, 35.229609),
 new Tuple<string, double, double>("Hanevi'im Street, Jerusalem, Israel", 31.783268, 35.215372),
 new Tuple<string, double, double>("Givat Shaul, Jerusalem, Israel", 31.747872, 35.206056),
 new Tuple<string, double, double>("Nachlaot, Jerusalem, Israel", 31.771530, 35.217560),
 new Tuple<string, double, double>("Gilo, Jerusalem, Israel", 31.748887, 35.170544),
 new Tuple<string, double, double>("Mount of Olives, Jerusalem, Israel", 31.776772, 35.261929),
 new Tuple<string, double, double>("Emek Refaim, Jerusalem, Israel", 31.764424, 35.206598),
 new Tuple<string, double, double>("Beit Hakerem, Jerusalem, Israel", 31.758071, 35.185407),
 new Tuple<string, double, double>("French Hill, Jerusalem, Israel", 31.788697, 35.232106),
 new Tuple<string, double, double>("Armon Hanatziv, Jerusalem, Israel", 31.748263, 35.227831),
 new Tuple<string, double, double>("Teddy Stadium, Jerusalem, Israel", 31.760424, 35.188991),
 new Tuple<string, double, double>("Talpiot Industrial Zone, Jerusalem, Israel", 31.748036, 35.214887),
 new Tuple<string, double, double>("Shmuel Hanavi Street, Jerusalem, Israel", 31.779032, 35.215924),
 new Tuple<string, double, double>("Kiryat Moshe, Jerusalem, Israel", 31.770064, 35.203555),
 new Tuple<string, double, double>("Jabotinsky Street, Jerusalem, Israel", 31.759996, 35.201378),
 new Tuple<string, double, double>("Kedem Street, Jerusalem, Israel", 31.772394, 35.231221),
 new Tuple<string, double, double>("Tzefaniah Street, Jerusalem, Israel", 31.759870, 35.205675),
 new Tuple<string, double, double>("Alrov Mamilla Mall, Jerusalem, Israel", 31.776948, 35.233613)

    };

        // Validate addresses list
        if (callAddresses.Count == 0)
        {
            throw new InvalidOperationException("The call addresses list is empty. Cannot create calls.");
        }

        // Generate 50 calls
        for (int i = 0; i < 50; i++)
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

                // Generate opening time and max time
                DateTime openTime = s_dal.config.Clock.AddHours(-s_rand.Next(1, 6));
                DateTime? maxTime = (s_rand.NextDouble() > 0.3)
                    ? (DateTime?)openTime.AddHours(s_rand.Next(2, 5))
                    : null;

                // Ensure expired calls are handled
                if (maxTime == null && s_rand.NextDouble() < 0.1)
                {
                    maxTime = openTime.AddHours(-s_rand.Next(6, 24));
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


    // <summary>
    // Assigns volunteers to calls.
    // - Ensures each volunteer is assigned to at least one call.
    // - Generates random times for appointment and finish time within constraints.
    // </summary>
    private static void createAssignment()
    {
        // אתחול רשימות למעקב
        Dictionary<int, int> volunteerCallCount = new Dictionary<int, int>();
        List<int> volunteersWithoutCalls = new List<int>(s_dal!.Volunteer.ReadAll().Select(v => v.Id));
        int totalVolunteers = s_dal!.Volunteer.ReadAll().Count();

        // לולאה ליצירת 50 הקצאות
        for (int i = 0; i < 50; i++)
        {
            Volunteer volunteerToAssig;

            // בחירת מתנדב
            if (volunteersWithoutCalls.Count > 0)
            {
                int randIndex = s_rand.Next(volunteersWithoutCalls.Count);
                int volunteerId = volunteersWithoutCalls[randIndex];
                volunteerToAssig = s_dal!.Volunteer.ReadAll().First(v => v.Id == volunteerId);
                volunteersWithoutCalls.RemoveAt(randIndex);
            }
            else
            {
                int randVolunteer = s_rand.Next(totalVolunteers);
                volunteerToAssig = s_dal!.Volunteer.ReadAll().ElementAt(randVolunteer);
            }

            // עדכון המעקב
            if (!volunteerCallCount.ContainsKey(volunteerToAssig.Id))
            {
                volunteerCallCount[volunteerToAssig.Id] = 0;
            }

            // הגרלת קריאה
            Call callToAssig = null;
            int attempts = 0;
            while (attempts < 10)
            {
                int randCall = s_rand.Next(s_dal!.call.ReadAll().Count());
                callToAssig = s_dal.call.ReadAll().ElementAt(randCall);
                if (callToAssig.OpenTime <= s_dal!.config.Clock) break;
                attempts++;
            }
            if (callToAssig == null || callToAssig.OpenTime > s_dal!.config.Clock)
            {
                Console.WriteLine("No suitable call found after 10 attempts.");
                continue;
            }

            // יצירת סיום
            FinishAppointmentType? finish = null;
            DateTime? finishTime = null;

            if (callToAssig.MaxTime != null && callToAssig.MaxTime <= s_dal.config.Clock)
            {
                finish = FinishAppointmentType.CancellationHasExpired;
            }
            else
            {
                int randFinish = s_rand.Next(0, 5);
                switch (randFinish)
                {
                    case 0: finish = FinishAppointmentType.WasTreated; finishTime = s_dal.config.Clock; break;
                    case 1: finish = FinishAppointmentType.SelfCancellation; finishTime = s_dal.config.Clock; break;
                    case 2: finish = FinishAppointmentType.CancelingAnAdministrator; finishTime = s_dal.config.Clock; break;
                    case 4: finish = FinishAppointmentType.WasTreated; finishTime = callToAssig.MaxTime?.AddMinutes(10); break;
                }
            }


            // יצירת הקצאה
            int newAssignmentId = s_dal.config.NextAssignmentId;
            s_dal!.assignment.Create(new Assignment
            {
                Id = newAssignmentId,
                CallId = callToAssig.Id,
                VolunteerId = volunteerToAssig.Id,
                AppointmentTime = s_dal!.config.Clock,
                FinishAppointmentTime = finishTime,
                FinishAppointmentType = finish,
            });

            volunteerCallCount[volunteerToAssig.Id]++;
            if (volunteerCallCount[volunteerToAssig.Id] > 5)
            {
                volunteerCallCount.Remove(volunteerToAssig.Id);
            }
        }
    }


}
