namespace Dal;
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
    public void Create(Volunteer item)
    {
        if (Read(item.Id) == null) // Check if the volunteer with the given ID doesn't already exist
            DataSource.Volunteers.Add(item); // Add the new volunteer to the list
        else // Volunteer with the same ID already exists
            throw new DalAlreadyExistException($"Volunteer with ID={item.Id} already exists"); // Throw exception
    }

    /// <summary>
    /// Deletes a volunteer record by its ID.
    /// If the volunteer with the given ID is found, it is removed from the list.
    /// Throws a DalDoesNotExistException if the volunteer with the given ID does not exist.
    /// </summary>
    public void Delete(int id)
    {
        Volunteer? ToDelete = Read(id); // Get the volunteer with the given ID to delete
        if (ToDelete != null) // Check if the volunteer exists
        {
            DataSource.Volunteers.Remove(ToDelete); // Remove the volunteer from the list
        }
        else // If volunteer with the given ID does not exist
        {
            throw new DalDoesNotExistException($"Object with Id {id} not found"); // Throw exception
        }
    }

    /// <summary>
    /// Deletes all volunteer records from the list.
    /// </summary>
    public void DeleteAll()
    {
        DataSource.Volunteers.Clear(); // Clear the list of all volunteers
    }

    /// <summary>
    /// Reads a volunteer record by its ID.
    /// Returns the volunteer if found, otherwise returns null.
    /// </summary>
    public Volunteer? Read(int id)
    {
        return DataSource.Volunteers.FirstOrDefault(obj => obj.Id == id); // Return volunteer with the matching ID, or null if not found
    }

    /// <summary>
    /// Reads a volunteer record based on a given filter.
    /// Returns the first volunteer that matches the filter criteria, or null if no match is found.
    /// </summary>
    public Volunteer? Read(Func<Volunteer, bool> filter) // Stage 2
        => DataSource.Volunteers.FirstOrDefault(filter); // Return the first volunteer matching the filter, or null if none matches

    /// <summary>
    /// Reads all volunteer records, optionally filtered by a given condition.
    /// If no filter is provided, returns all volunteers.
    /// </summary>
    public IEnumerable<Volunteer> ReadAll(Func<Volunteer, bool>? filter = null) // Stage 2
        => filter == null
            ? DataSource.Volunteers.Select(obj => obj) // Return all volunteers if no filter is provided
            : DataSource.Volunteers.Where(filter); // Return filtered volunteers based on the provided filter

    /// <summary>
    /// Updates an existing volunteer record.
    /// Deletes the existing volunteer and then creates the new one in the list.
    /// </summary>
    public void Update(Volunteer item)
    {
        Delete(item.Id); // Delete the existing volunteer if it exists
        Create(item); // Add the updated volunteer to the list
    }
}
