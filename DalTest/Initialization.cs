namespace DalTest;

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
    public static void Do(IDal dal)
    {
        s_dal = dal ?? throw new NullReferenceException("DAL object cannot be null!");
        Console.WriteLine("Resetting configuration values and data lists.");
        s_dal.ResetDB();
        Console.WriteLine("Initializing volunteers, calls, and assignments lists.");
        createVolunteers();
        createCalls();
        createAssignments();
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

        string GenerateRandomPassword()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            return new string(Enumerable.Range(0, 12)
                .Select(_ => chars[s_rand.Next(chars.Length)])
                .ToArray());
        }

        var volunteerCount = volunteerNames.Length;
        for (int i = 0; i < volunteerCount; i++)
        {
            int id;
            do
            {
                id = s_rand.Next(200000000, 400000000); // Generate unique ID
            } while (s_dal!.Volunteer.Read(id) != null);

            s_dal.Volunteer.Create(new Volunteer
            {
                Id = id,
                FullName = volunteerNames[i],
                PhoneNumber = $"05{s_rand.Next(10000000, 99999999)}",
                Email = $"volunteer{i + 1}@gmail.com",
                Location = addresses[i].Item1,
                MaxDistance = s_rand.Next(5, 20),
                Position = (i == 0) ? Position.Manager : Position.Volunteer,
                Latitude = addresses[i].Item2,
                Longitude = addresses[i].Item3,
                Active = true,
                DistanceType = DistanceType.AerialDistance,
                Password = GenerateRandomPassword(),
            });
        }
    }

    /// <summary>
    /// Creates a set of call records in the database.
    /// - Each call is assigned a random description, type, and location.
    /// - Open and maximum response times are set based on random offsets.
    /// </summary>
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

    /// <summary>
    /// Assigns volunteers to calls.
    /// - Ensures each volunteer is assigned to at least one call.
    /// - Generates random times for appointment and finish time within constraints.
    /// </summary>
    public static void createAssignments()
    {
        List<Call> allCalls = s_dal!.call.ReadAll()?.ToList();
        List<Volunteer> allVolunteers = s_dal!.Volunteer.ReadAll()?.ToList();

        if (allCalls?.Count == 0 || allVolunteers?.Count == 0)
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
