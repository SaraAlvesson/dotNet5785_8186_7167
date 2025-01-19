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
        new Tuple<string, double, double>("Rothschild Boulevard, Tel Aviv, Israel", 32.065500, 34.767028),
new Tuple<string, double, double>("Tel Aviv Port, Tel Aviv, Israel", 32.099791, 34.769077),
new Tuple<string, double, double>("Jaffa, Tel Aviv, Israel", 32.055823, 34.759151),
new Tuple<string, double, double>("Neve Tzedek, Tel Aviv, Israel", 32.065764, 34.770022),
new Tuple<string, double, double>("Habima Theatre, Tel Aviv, Israel", 32.073743, 34.770498),
new Tuple<string, double, double>("Carmel Market, Tel Aviv, Israel", 32.065013, 34.768370),
new Tuple<string, double, double>("Dizengoff Square, Tel Aviv, Israel", 32.078323, 34.770340),
new Tuple<string, double, double>("Yarkon Park, Tel Aviv, Israel", 32.087573, 34.781904),
new Tuple<string, double, double>("Tel Aviv Museum of Art, Tel Aviv, Israel", 32.073418, 34.781046),
new Tuple<string, double, double>("Florentin, Tel Aviv, Israel", 32.054186, 34.759738),
new Tuple<string, double, double>("Jaffa Port, Tel Aviv, Israel", 32.053704, 34.756671),
new Tuple<string, double, double>("Rabin Square, Tel Aviv, Israel", 32.078570, 34.768721),
new Tuple<string, double, double>("Levinsky Market, Tel Aviv, Israel", 32.061425, 34.766412),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Azrieli Center, Tel Aviv, Israel", 32.073700, 34.791202),

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
       new Tuple<string, double, double>("Ben Yehuda Street, Tel Aviv, Israel", 32.065500, 34.767028),
new Tuple<string, double, double>("Tel Aviv Port, Tel Aviv, Israel", 32.099791, 34.769077),
new Tuple<string, double, double>("Jaffa Road, Tel Aviv, Israel", 32.055823, 34.759151),
new Tuple<string, double, double>("Neve Tzedek, Tel Aviv, Israel", 32.065764, 34.770022),
new Tuple<string, double, double>("Habima Theatre, Tel Aviv, Israel", 32.073743, 34.770498),
new Tuple<string, double, double>("Carmel Market, Tel Aviv, Israel", 32.065013, 34.768370),
new Tuple<string, double, double>("Dizengoff Square, Tel Aviv, Israel", 32.078323, 34.770340),
new Tuple<string, double, double>("Yarkon Park, Tel Aviv, Israel", 32.087573, 34.781904),
new Tuple<string, double, double>("Tel Aviv Museum of Art, Tel Aviv, Israel", 32.073418, 34.781046),
new Tuple<string, double, double>("Florentin, Tel Aviv, Israel", 32.054186, 34.759738),
new Tuple<string, double, double>("Jaffa Port, Tel Aviv, Israel", 32.053704, 34.756671),
new Tuple<string, double, double>("Rabin Square, Tel Aviv, Israel", 32.078570, 34.768721),
new Tuple<string, double, double>("Levinsky Market, Tel Aviv, Israel", 32.061425, 34.766412),
new Tuple<string, double, double>("Gordon Beach, Tel Aviv, Israel", 32.071299, 34.767122),
new Tuple<string, double, double>("Azrieli Center, Tel Aviv, Israel", 32.073700, 34.791202),
new Tuple<string, double, double>("Tel Aviv Cinematheque, Tel Aviv, Israel", 32.070856, 34.772402),
new Tuple<string, double, double>("Rothschild Boulevard, Tel Aviv, Israel", 32.065500, 34.767028),
new Tuple<string, double, double>("Neve Tzedek, Tel Aviv, Israel", 32.065764, 34.770022),
new Tuple<string, double, double>("Carmel Market, Tel Aviv, Israel", 32.065013, 34.768370),
new Tuple<string, double, double>("Teddy Stadium, Tel Aviv, Israel", 32.065713, 34.769120),
new Tuple<string, double, double>("Jaffa, Tel Aviv, Israel", 32.050830, 34.752780),
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
new Tuple<string, double, double>("Jaffa Flea Market, Tel Aviv, Israel", 32.053333, 34.757233),
new Tuple<string, double, double>("Ramat Gan, Tel Aviv, Israel", 32.073208, 34.817878),
new Tuple<string, double, double>("Tel Aviv University, Tel Aviv, Israel", 32.113120, 34.804338),
new Tuple<string, double, double>("Hilton Beach, Tel Aviv, Israel", 32.095183, 34.768234),
new Tuple<string, double, double>("Kikar Hamedina, Tel Aviv, Israel", 32.078298, 34.786209),
new Tuple<string, double, double>("Azrieli Sarona Tower, Tel Aviv, Israel", 32.073719, 34.785553),
new Tuple<string, double, double>("Tel Aviv-Yafo, Tel Aviv, Israel", 32.054166, 34.765746),
new Tuple<string, double, double>("Alma Beach, Tel Aviv, Israel", 32.073487, 34.758676),
new Tuple<string, double, double>("Gan Meir, Tel Aviv, Israel", 32.070504, 34.776377),
new Tuple<string, double, double>("Shenkin Street, Tel Aviv, Israel", 32.065034, 34.770086),
new Tuple<string, double, double>("Palmach Street, Tel Aviv, Israel", 32.070043, 34.767118),
new Tuple<string, double, double>("Jaffa Clock Tower, Tel Aviv, Israel", 32.053076, 34.759602),

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
                    case 0:
                        finish = FinishAppointmentType.WasTreated;
                        finishTime = s_dal.config.Clock.AddMinutes(s_rand.Next(5, 31)); // הוספת אקראי בין 5 ל-30 דקות
                        break;
                    case 1:
                        finish = FinishAppointmentType.SelfCancellation;
                        finishTime = s_dal.config.Clock.AddMinutes(s_rand.Next(5, 31)); // הוספת אקראי בין 5 ל-30 דקות
                        break;
                    case 2:
                        finish = FinishAppointmentType.CancelingAnAdministrator;
                        finishTime = s_dal.config.Clock.AddMinutes(s_rand.Next(5, 31)); // הוספת אקראי בין 5 ל-30 דקות
                        break;
                    case 4:
                        finish = FinishAppointmentType.CancellationHasExpired;
                        finishTime = callToAssig.MaxTime?.AddMinutes(s_rand.Next(5, 31)); // הוספת אקראי בין 5 ל-30 דקות
                        break;
                }
            }

            // יצירת הקצאה
            int newAssignmentId = s_dal.config.NextAssignmentId;
            s_dal!.assignment.Create(new Assignment
            {
                Id = newAssignmentId,
                CallId = callToAssig.Id,
                VolunteerId = volunteerToAssig.Id,
                AppointmentTime = s_dal!.config.Clock.AddMinutes(s_rand.Next(5, 31)), // הוספת אקראי לזמן תחילת הייעוץ
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
