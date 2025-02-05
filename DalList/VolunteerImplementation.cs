namespace Dal;

using System.Runtime.CompilerServices;
using DalApi;
using DO;

/// <summary>
/// Implementation of the IVolunteer interface for managing volunteer data.
/// This class handles the operations for creating, reading, updating, and deleting volunteer records.
/// </summary>
internal class VolunteerImplementation : IVolunteer
{
    /// <summary>
    /// Creates a new volunteer record.
    /// Adds the volunteer to the list if no existing volunteer has the same ID.
    /// Throws a DalAlreadyExistException if the volunteer with the same ID already exists.
    /// </summary>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public void Create(Volunteer item)
    {
        if (Read(item.Id) == null) // אם המתנדב לא קיים כבר
        {
            DataSource.Volunteers.Add(item); // הוסף את המתנדב החדש
            Console.WriteLine($"Volunteer with ID {item.Id} created.");
        }
        else // אם המתנדב כבר קיים
        {
            throw new DalAlreadyExistException($"Volunteer with ID={item.Id} already exists"); // זרוק חריגה אם המתנדב כבר קיים
        }
    }


    /// <summary>
    /// Deletes a volunteer record by its ID.
    /// If the volunteer with the given ID is found, it is removed from the list.
    /// Throws a DalDoesNotExistException if the volunteer with the given ID does not exist.
    /// </summary>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]


    public void Delete(int id)
    {
        Volunteer? ToDelete = Read(id); // קבלת המתנדב למחיקה
        if (ToDelete != null) // אם המתנדב קיים
        {
            DataSource.Volunteers.Remove(ToDelete); // הסרת המתנדב מהרשימה
            Console.WriteLine($"Volunteer with ID {id} deleted.");
        }
        else // אם המתנדב לא קיים
        {
            throw new DalDoesNotExistException($"Object with Id {id} not found"); // זרוק חריגה אם המתנדב לא נמצא
        }
    }


    /// <summary>
    /// Deletes all volunteer records from the list.
    /// </summary>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public void DeleteAll()
    {
        DataSource.Volunteers.Clear(); // Clear the list of all volunteers
    }

    /// <summary>
    /// Reads a volunteer record by its ID.
    /// Returns the volunteer if found, otherwise returns null.
    /// </summary>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public Volunteer? Read(int id)
    {
        return DataSource.Volunteers.FirstOrDefault(obj => obj.Id == id); // Return volunteer with the matching ID, or null if not found
    }

    /// <summary>
    /// Reads a volunteer record based on a given filter.
    /// Returns the first volunteer that matches the filter criteria, or null if no match is found.
    /// </summary>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public Volunteer? Read(Func<Volunteer, bool> filter) // Stage 2
        => DataSource.Volunteers.FirstOrDefault(filter); // Return the first volunteer matching the filter, or null if none matches

    /// <summary>
    /// Reads all volunteer records, optionally filtered by a given condition.
    /// If no filter is provided, returns all volunteers.
    /// </summary>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public IEnumerable<Volunteer> ReadAll(Func<Volunteer, bool>? filter = null) // Stage 2
        => filter == null
            ? DataSource.Volunteers.Select(obj => obj) // Return all volunteers if no filter is provided
            : DataSource.Volunteers.Where(filter); // Return filtered volunteers based on the provided filter

    /// <summary>
    /// Updates an existing volunteer record.
    /// Deletes the existing volunteer and then creates the new one in the list.
    /// </summary>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public void Update(Volunteer item)
    {
        try
        {
            // שלב 1: מחיקת המתנדב הקיים
            Delete(item.Id); // אם המתנדב קיים, הוא יימחק

            // שלב 2: הוספת המתנדב החדש
            Create(item); // המתנדב החדש ייווסף
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw; // זרוק את החריגה במידה ו-Delete נכשל
        }
        catch (DalAlreadyExistException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw; // זרוק את החריגה במידה ו-Create נכשל
        }
    }

}
