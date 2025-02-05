using Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

/// <summary>
/// The AssignmentImplementation class provides the implementation for managing assignments in the data source.
/// It handles CRUD operations (Create, Read, Update, Delete) for the Assignment objects.
/// </summary>
internal class AssignmentImplementation : IAssignment
{
    /// <summary>
    /// Loads the list of assignments from the XML file.
    /// </summary>
    private List<Assignment> LoadAssignments() => XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);

    /// <summary>
    /// Saves the list of assignments back to the XML file.
    /// </summary>
    /// <param name="assignments">The list of assignments to save.</param>
    private void SaveAssignments(List<Assignment> assignments) => XMLTools.SaveListToXMLSerializer(assignments, Config.s_assignments_xml);

    /// <summary>
    /// Creates a new assignment by generating a unique Id and adding it to the assignments list.
    /// </summary>
    /// <param name="item">The assignment item to be created.</param>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public void Create(Assignment item)
    {
        List<Assignment> assignments = LoadAssignments();


        int nextId = Config.NextAssignmentId;
        Assignment copy = item with { Id = nextId };
        assignments.Add(copy);
        SaveAssignments(assignments);
    }

    /// <summary>
    /// Reads an assignment from the list by its Id.
    /// </summary>
    /// <param name="id">The Id of the assignment to be read.</param>
    /// <returns>The assignment with the matching Id, or null if not found.</returns>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public Assignment? Read(int id)
    {
        List<Assignment> assignments = LoadAssignments();
        return assignments.FirstOrDefault(a => a.Id == id);
    }

    /// <summary>
    /// Reads an assignment from the list by a custom filter function.
    /// </summary>
    /// <param name="filter">A filter function to match the assignment.</param>
    /// <returns>The first assignment that matches the filter, or null if no match is found.</returns>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public Assignment? Read(Func<Assignment, bool> filter)
    {
        List<Assignment> assignments = LoadAssignments();
        return assignments.FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads all assignments, optionally filtered by a custom function.
    /// </summary>
    /// <param name="filter">An optional filter function to match assignments. If null, returns all assignments.</param>
    /// <returns>An IEnumerable of assignments that match the filter, or all assignments if no filter is provided.</returns>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null)
    {
        List<Assignment> assignments = LoadAssignments();

        return filter == null ? assignments : assignments.Where(filter);
    }

    /// <summary>
    /// Updates an existing assignment by first finding and replacing the old one.
    /// </summary>
    /// <param name="item">The assignment to be updated.</param>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public void Update(Assignment item)
    {
        List<Assignment> assignments = LoadAssignments();
        Assignment? existing = assignments.FirstOrDefault(a => a.Id == item.Id);

        if (existing == null)
            throw new DalDoesNotExistException($"Assignment with ID={item.Id} does not exist.");

        assignments.Remove(existing);
        assignments.Add(item);
        SaveAssignments(assignments);
    }

    /// <summary>
    /// Deletes an assignment from the list by its Id.
    /// </summary>
    /// <param name="id">The Id of the assignment to be deleted.</param>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public void Delete(int id)
    {
        List<Assignment> assignments = LoadAssignments();

        if (assignments.RemoveAll(a => a.Id == id) == 0)
            throw new DalDoesNotExistException($"Assignment with ID={id} does not exist.");

        SaveAssignments(assignments);
    }

    /// <summary>
    /// Deletes all assignments from the list.
    /// </summary>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public void DeleteAll()
    {
        SaveAssignments(new List<Assignment>());
    }
}
