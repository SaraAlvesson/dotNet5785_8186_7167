namespace DalTest;

using DalApi;
using DO;
using System;

public static class Initialization
{
    //// Static fields to hold references to the DAL interfaces
    //private static ICall? s_dalCall;
    //private static IAssignment? s_dalAssignment;
    //private static IVolunteer? s_dalVolunteer;
    //private static IConfig? s_dalConfig;
    private static readonly Random s_rand = new(); // Random generator for creating random data
    private static IDal? s_dal;

    // Main initialization method
    public static void Do(IDal dal)
    {
        // Validate and set the interfaces
        //s_dalVolunteer = dalVolunteer ?? throw new NullReferenceException("DAL cannot be null!");
        //s_dalCall = dalCall ?? throw new NullReferenceException("DAL cannot be null!");
        //s_dalAssignment = dalAssignment ?? throw new NullReferenceException("DAL cannot be null!");
        //s_dalConfig = dalConfig ?? throw new NullReferenceException("DAL cannot be null!");
        s_dal=dal?? throw new NullReferenceException("DAL object can not be null!");
        // Reset all data and configurations
        Console.WriteLine("Reset Configuration values and List values");
        //s_dalConfig.Reset();
        //s_dalAssignment.DeleteAll();
        //s_dalVolunteer.DeleteAll();
        //s_dalCall.DeleteAll();
        s_dal.ResetDB();    
        // Populate volunteers, calls, and assignments
        Console.WriteLine("Initializing Volunteers list, Calls list, Assignment list");
        createVolunteers();
        createCalls();
        createAssignments();
    }

    // Creates a list of volunteers and populates the DAL
    private static void createVolunteers()
    {
        // Array of volunteer names
        string[] volunteerNames = new string[] { "Yosef Cohen", "Shmuel Levi", "Yaakov Goldstein", "Moshe Friedman",
                                                 "Avraham Stein", "Daniel Green", "David Weiss", "Yonatan Rubin",
                                                 "Hanan Levy", "Eli Karp", "Uzi Sharoni", "Shimon Ben-David",
                                                 "Matan Shalev", "Asher Tzukrel", "Oren Regev"};

        // List of addresses with their latitude and longitude
        var addresses = new List<Tuple<string, double, double>>()
        {
          new Tuple<string, double, double>("Rothschild Boulevard, Tel Aviv, Israel", 32.0853, 34.7818),
          new Tuple<string, double, double>("Jaffa Gate, Jerusalem, Israel", 31.7683, 35.2137),
          new Tuple<string, double, double>("Baha'i Gardens, Haifa, Israel", 32.8140, 34.9896),
          new Tuple<string, double, double>("Dead Sea, Israel", 31.5590, 35.4732),
          new Tuple<string, double, double>("Masada National Park, Israel", 31.3156, 35.3533),
          new Tuple<string, double, double>("Eilat Coral Beach, Eilat, Israel", 29.5015, 34.9166),
          new Tuple<string, double, double>("Yad Vashem, Jerusalem, Israel", 31.7744, 35.1758),
          new Tuple<string, double, double>("Carmel Market, Tel Aviv, Israel", 32.0681, 34.7705),
          new Tuple<string, double, double>("Sea of Galilee, Tiberias, Israel", 32.7959, 35.5312),
          new Tuple<string, double, double>("Ramon Crater, Mitzpe Ramon, Israel", 30.6092, 34.8015),
          new Tuple<string, double, double>("Western Wall, Jerusalem, Israel", 31.7767, 35.2345),
          new Tuple<string, double, double>("Caesarea National Park, Israel", 32.4999, 34.8957),
          new Tuple<string, double, double>("Tel Aviv Port, Tel Aviv, Israel", 32.0970, 34.7743),
          new Tuple<string, double, double>("Timna Park, Eilat, Israel", 29.7878, 34.9491),
          new Tuple<string, double, double>("Banias Nature Reserve, Golan Heights, Israel", 33.2473, 35.6931),
        };

        // Function to generate random secure passwords
        string GenerateRandomPassword()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            return new string(Enumerable.Range(0, 12)
                .Select(_ => chars[s_rand.Next(chars.Length)])
                .ToArray());
        }

        // Create volunteers
        var volunteerCount = volunteerNames.Length;
        for (int i = 0; i < volunteerCount; i++)
        {
            int id;
            do
            {
                id = s_rand.Next(200000000, 400000000); // Generate a valid ID
            } while (s_dal!.Volunteer.Read(id) != null); // Ensure the ID is unique

            s_dal.Volunteer.Create(new Volunteer
            {
                Id = id,
                FullName = volunteerNames[i],
                PhoneNumber = $"05{s_rand.Next(10000000, 99999999)}", // Generate a valid phone number
                Email = $"volunteer{i + 1}@gmail.com",
                Location = addresses[i].Item1,
                MaxDistance = s_rand.Next(5, 20),
                Position = (i == 0) ? Position.Manager : Position.Volunteer, // First volunteer is a manager
                Latitude = addresses[i].Item2,
                Longitude = addresses[i].Item3,
                Active = true,
                DistanceType = DistanceType.AerialDistance,
                Password = GenerateRandomPassword(),
            });
        }
    }

    // Creates a list of calls and populates the DAL
    private static void createCalls()
    {
        string[] callDescriptions = { "Emergency food delivery", "Fixing equipment", "Medical assistance required",
                                       "Providing shelter to families in need" };

        for (int i = 0; i < 50; i++)
        {
            CallType callType = (CallType)s_rand.Next(0, Enum.GetValues(typeof(CallType)).Length);
            string description = callDescriptions[s_rand.Next(callDescriptions.Length)];
            string[] cities = { "Tel Aviv", "Jerusalem", "Haifa", "Eilat", "Beer Sheva", "Rishon Lezion", "Ashdod" };
            string address = $"{s_rand.Next(1, 100)} {cities[s_rand.Next(cities.Length)]} St.";

            s_dal!.call.Create(new Call
            {
                VerbDesc = description,
                Adress = address,
                OpenTime = s_dal.config.Clock.AddHours(-s_rand.Next(1, 6)),
                MaxTime = s_dal.config.Clock.AddHours(s_rand.Next(2, 10)),
                Latitude = s_rand.NextDouble() * 180 - 90,
                Longitude = s_rand.NextDouble() * 360 - 180,
                CallType = callType,
            });
        }
    }

    // Creates assignments between volunteers and calls
    
    public static void createAssignments()
    {
        List<Call> allCalls = s_dal!.call.ReadAll();
        List<Volunteer> allVolunteers = s_dal!.Volunteer.ReadAll();

        if (allCalls.Count == 0 || allVolunteers.Count == 0)
        {
            Console.WriteLine("No calls or volunteers available for assignment.");
            return;
        }

        int allocationCount = 50; // Number of assignments to create
        List<Volunteer> volunteersWithNoAssignments = new List<Volunteer>(allVolunteers);

        for (int i = 0; i < allocationCount; i++)
        {
            int callIndex = s_rand.Next(0, allCalls.Count);
            Call call = allCalls[callIndex];

            Volunteer volunteer;
            if (volunteersWithNoAssignments.Count > 0)
            {
                int volunteerIndex = s_rand.Next(0, volunteersWithNoAssignments.Count);
                volunteer = volunteersWithNoAssignments[volunteerIndex];
                volunteersWithNoAssignments.RemoveAt(volunteerIndex);
            }
            else
            {
                int volunteerIndex = s_rand.Next(0, allVolunteers.Count);
                volunteer = allVolunteers[volunteerIndex];
            }

            if (call.MaxTime.HasValue)
            {
                DateTime entryTime = call.OpenTime.AddMinutes(s_rand.Next(1, call.MaxTime.Value.Minute));
                DateTime finishTime = entryTime.AddMinutes(s_rand.Next(30, 120));

                FinishAppointmentType finishType = finishTime <= call.MaxTime ?
                                                   FinishAppointmentType.WasTreated :
                                                   FinishAppointmentType.CancellationHasExpired;

                s_dal!.assignment.Create(new Assignment
                {
                    CallId = call.Id,
                    VolunteerId = volunteer.Id,
                    AppointmentTime = entryTime,
                    FinishAppointmentTime = finishTime,
                    FinishAppointmentType = finishType,
                });
            }
        }
    }
}
