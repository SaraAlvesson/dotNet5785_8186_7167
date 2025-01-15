namespace Dal;
using DalApi;
using DO;

/// <summary>
/// The CallImplementation class provides the implementation for managing calls in the data source.
/// It handles CRUD operations (Create, Read, Update, Delete) for the Call objects.
/// </summary>
internal class CallImplementation : ICall
{
    /// <summary>
    /// Creates a new call by generating a unique Id and adding it to the calls list.
    /// </summary>
    /// <param name="item">The call item to be created.</param>
    public void Create(Call item)
    {
        Call newItem = item with { Id = Config.NextCallId }; // Creates new item with the next available ID
        DataSource.Calls.Add(newItem); // Adds the new item to the list
    }

    /// <summary>
    /// Reads a call from the list by its Id.
    /// </summary>
    /// <param name="id">The Id of the call to be read.</param>
    /// <returns>The call with the matching Id, or null if not found.</returns>
    public Call? Read(int id)
    {
        return DataSource.Calls.FirstOrDefault(obj => obj.Id == id); // Returns the call with the matching Id, or null if not found
    }

    /// <summary>
    /// Reads a call from the list by a custom filter function.
    /// </summary>
    /// <param name="filter">A filter function to match the call.</param>
    /// <returns>The first call that matches the filter, or null if no match is found.</returns>
    public Call? Read(Func<Call, bool> filter) // Stage 2
    => DataSource.Calls.FirstOrDefault(filter); // Returns the first matching call based on the filter

    /// <summary>
    /// Reads all calls, optionally filtered by a custom function.
    /// </summary>
    /// <param name="filter">An optional filter function to match calls. If null, returns all calls.</param>
    /// <returns>An IEnumerable of calls that match the filter, or all calls if no filter is provided.</returns>
    public IEnumerable<Call> ReadAll(Func<Call, bool>? filter = null) // Stage 2
    => filter == null
        ? DataSource.Calls.Select(obj => obj) // If no filter, returns all calls
        : DataSource.Calls.Where(filter); // Otherwise, returns calls that match the filter

    /// <summary>
    /// Updates an existing call by first deleting the old one (if it exists) and then creating the new one.
    /// </summary>
    /// <param name="item">The call to be updated.</param>
    public void Update(Call item)
    {
        Delete(item.Id); // If the item exists, it is deleted first (throws an exception if not found)
        Create(item); // Adds the updated item to the list
    }

    /// <summary>
    /// Deletes a call from the list by its Id.
    /// </summary>
    /// <param name="id">The Id of the call to be deleted.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the call with the given Id is not found.</exception>
    public void Delete(int id)
    {
        Call? ToDelete = Read(id); // Retrieves the call to delete
        if (ToDelete != null) // Checks if the call exists
        {
            DataSource.Calls.Remove(ToDelete); // Removes the call from the list
        }
        else // If the call does not exist
        {
            throw new DalDoesNotExistException($"Object with Id {id} not found"); // Throws an exception if not found
        }
    }

    /// <summary>
    /// Deletes all calls from the list.
    /// </summary>
    public void DeleteAll()
    {
        DataSource.Calls.Clear(); // Clears all items from the list
    }
}
