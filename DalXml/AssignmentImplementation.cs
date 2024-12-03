

namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

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
        List<Assignment> Assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        int nextId = Config.NextAssignmentId;
        Assignment copy = item with { Id = nextId };
        Assignments.Add(copy);
        XMLTools.SaveListToXMLSerializer(Assignments, Config.s_assignments_xml);
    }

    /// <summary>
    /// Reads an assignment from the list by its Id.
    /// </summary>
    /// <param name="id">The Id of the assignment to be read.</param>
    /// <returns>The assignment with the matching Id, or null if not found.</returns>
    public Assignment? Read(int id)
    {
        List<Assignment> Assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        //XMLTools.SaveListToXMLSerializer(Assignments, Config.s_assignments_xml);
        return Assignments.FirstOrDefault(obj => obj.Id == id); // Returns the assignment with the matching Id, or null if not found  
    }

    /// <summary>
    /// Reads an assignment from the list by a custom filter function.
    /// </summary>
    /// <param name="filter">A filter function to match the assignment.</param>
    /// <returns>The first assignment that matches the filter, or null if no match is found.</returns>
    public Assignment? Read(Func<Assignment, bool> filter)// Stage 2
    {
        List<Assignment> Assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        //XMLTools.SaveListToXMLSerializer(Assignments, Config.s_assignments_xml);
        return Assignments.FirstOrDefault(filter); // Returns the first matching assignment based on the filter

    }

    /// <summary>
    /// Reads all assignments, optionally filtered by a custom function.
    /// </summary>
    /// <param name="filter">An optional filter function to match assignments. If null, returns all assignments.</param>
    /// <returns>An IEnumerable of assignments that match the filter, or all assignments if no filter is provided.</returns>
    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null) // Stage 2
    {
        List<Assignment> Assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        if (filter == null)
        {
            XMLTools.SaveListToXMLSerializer(Assignments, Config.s_assignments_xml);
            return Assignments.Select(obj => obj); // If no filter, returns all assignments
        }
        else
        {
            XMLTools.SaveListToXMLSerializer(Assignments, Config.s_assignments_xml);
            return Assignments.Where(filter); // Otherwise, returns assignments that match the filter
        }

    }
    /// <summary>
    /// Updates an existing assignment by first deleting the old one (if it exists) and then creating the new one.
    /// </summary>
    /// <param name="item">The assignment to be updated.</param>
    public void Update(Assignment item)
    {
        List<Assignment> Assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        if (Assignments.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Assignment with ID={item.Id} does Not exist");
        Assignments.Add(item);
        XMLTools.SaveListToXMLSerializer(Assignments, Config.s_assignments_xml);
    }
    /// <summary>
    /// Deletes an assignment from the list by its Id.
    /// </summary>
    /// <param name="id">The Id of the assignment to be deleted.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the assignment with the given Id is not found.</exception>
    public void Delete(int id)
    {
        List<Assignment> Assignments = XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
        if (Assignments.RemoveAll(it => it.Id == id) == 0)
            throw new DalDoesNotExistException($"Assignment with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(Assignments, Config.s_assignments_xml);
    }
    /// <summary>
    /// Deletes all assignments from the list.
    /// </summary>
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Assignment>(), Config.s_assignments_xml);
    }
}
//using Dal;
//using DalApi;
//using DO;
//using System.Xml.Linq;

//internal class AssignmentImplementation : IAssignment
//{
//    private List<Assignment> LoadAssignments()
//    {
//        return XMLTools.LoadListFromXMLSerializer<Assignment>(Config.s_assignments_xml);
//    }

//    private void SaveAssignments(List<Assignment> assignments)
//    {
//        XMLTools.SaveListToXMLSerializer(assignments, Config.s_assignments_xml);
//    }

//    public void Create(Assignment item)
//    {
//        List<Assignment> assignments = LoadAssignments();
//        int nextId = Config.NextAssignmentId;
//        Assignment copy = item with { Id = nextId };
//        assignments.Add(copy);
//        SaveAssignments(assignments);
//    }

//    public Assignment? Read(int id)
//    {
//        List<Assignment> assignments = LoadAssignments();
//        return assignments.FirstOrDefault(obj => obj.Id == id); // Return the first matching assignment or null
//    }

//    public Assignment? Read(Func<Assignment, bool> filter)
//    {
//        List<Assignment> assignments = LoadAssignments();
//        return assignments.FirstOrDefault(filter); // Return the first matching assignment based on the filter
//    }

//    public IEnumerable<Assignment> ReadAll(Func<Assignment, bool>? filter = null)
//    {
//        List<Assignment> assignments = LoadAssignments();
//        return filter == null ? assignments : assignments.Where(filter); // Return filtered or all assignments
//    }

//    public void Update(Assignment item)
//    {
//        List<Assignment> assignments = LoadAssignments();
//        Assignment? existingAssignment = assignments.FirstOrDefault(it => it.Id == item.Id);
//        if (existingAssignment == null)
//            throw new DalDoesNotExistException($"Assignment with ID={item.Id} does Not exist");

//        assignments.Remove(existingAssignment);
//        assignments.Add(item);
//        SaveAssignments(assignments);
//    }

//    public void Delete(int id)
//    {
//        List<Assignment> assignments = LoadAssignments();
//        if (assignments.RemoveAll(it => it.Id == id) == 0)
//            throw new DalDoesNotExistException($"Assignment with ID={id} does Not exist");

//        SaveAssignments(assignments);
//    }

//    public void DeleteAll()
//    {
//        SaveAssignments(new List<Assignment>());
//    }
//}
