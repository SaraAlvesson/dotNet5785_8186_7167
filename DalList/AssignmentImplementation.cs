
namespace Dal;
using DalApi;
using DO;

/// <summary>
/// The AssignmentImplementation class provides the implementation for managing assignments in the data source.
/// It handles CRUD operations (Create, Read, Update, Delete) for the Assignment objects.
/// </summary>
internal class AssignmentImplementation : IAssignment
{
    /// <summary>
    /// Creates a new assignment by generating a unique Id and adding it to the assignments list.
    /// </summary>
    /// <param name="item">The assignment item to be created.</param>
    public void Create(Assignment item)
    {
        Assignment newItem = item with { Id = Config.NextAssignmentId}; // Creates new item with the next available ID
        DataSource.Assignments.Add(newItem); // Adds the new item to the list
    }

    /// <summary>
    /// Reads an assignment from the list by its Id.
    /// </summary>
    /// <param name="id">The Id of the assignment to be read.</param>
    /// <returns>The assignment with the matching Id, or null if not found.</returns>
    public Assignment? Read(int id)
    {
        return DataSource.Assignments.FirstOrDefault(obj => obj.Id == id); // Returns the assignment with the matching Id, or null if not found
    }

    /// <summary>
    /// Reads an assignment from the list by a custom filter function.
    /// </summary>
    /// <param name="filter">A filter function to match the assignment.</param>
    /// <returns>The first assignment that matches the filter, or null if no match is found.</returns>
    public Assignment? Read(Func<Assignment, bool> filter) // Stage 2
    => DataSource.Assignments.FirstOrDefault(filter); // Returns the first matching assignment based on the filter

    /// <summary>
    /// Reads all assignments, optionally filtered by a custom function.
    /// </summary>
    /// <param name="filter">An optional filter function to match assignments. If null, returns all assignments.</param>
    /// <returns>An IEnumerable of assignments that match the filter, or all assignments if no filter is provided.</returns>
    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null) // Stage 2
    => filter == null
        ? DataSource.Assignments.Select(obj => obj) // If no filter, returns all assignments
        : DataSource.Assignments.Where(filter); // Otherwise, returns assignments that match the filter

    /// <summary>
    /// Updates an existing assignment by first deleting the old one (if it exists) and then creating the new one.
    /// </summary>
    /// <param name="item">The assignment to be updated.</param>
    public void Update(Assignment item)
    {
        Delete(item.Id); // If the item exists, it is deleted first (throws an exception if not found)
        Create(item); // Adds the updated item to the list
    }

    /// <summary>
    /// Deletes an assignment from the list by its Id.
    /// </summary>
    /// <param name="id">The Id of the assignment to be deleted.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the assignment with the given Id is not found.</exception>
    public void Delete(int id)
    {
        Assignment? ToDelete = Read(id); // Retrieves the assignment to delete
        if (ToDelete != null) // Checks if the assignment exists
        {
            DataSource.Assignments.Remove(ToDelete); // Removes the assignment from the list
        }
        else // If the assignment does not exist
        {
            throw new DalDoesNotExistException($"Object with Id {id} not found"); // Throws an exception if not found
        }
    }

    /// <summary>
    /// Deletes all assignments from the list.
    /// </summary>
    public void DeleteAll()
    {
        DataSource.Assignments.Clear(); // Clears all items from the list
    }


}
