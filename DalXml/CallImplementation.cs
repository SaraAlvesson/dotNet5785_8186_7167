

namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

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
    /// 

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Call item)
    {
        XElement calls = XMLTools.LoadListFromXMLElement(Config.s_calls_xml);
        calls.Add(createCallElement(item));
        XMLTools.SaveListToXMLElement(calls, Config.s_calls_xml);
    }
    static XElement createCallElement(Call item)
    {
        return new XElement("Call",
            new XElement("Id", Config.NextAssignmentId),
            new XElement("CallType", item.CallType),
            new XElement("VerbDesc", item.VerbDesc),
            new XElement("Adress", item.Adress),
            new XElement("Latitude", item.Latitude),
            new XElement("Longitude", item.Longitude),
            new XElement("OpenTime", item.OpenTime.ToString("yyyy-MM-ddTHH:mm:ss")), // מבלי לכלול אזור זמן
            new XElement("MaxTime", item.MaxTime?.ToString("yyyy-MM-ddTHH:mm:ss")) // גם כאן
        );
    }


    //public void Create(Call item)
    //{
    //    List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
    //    int nextId = Config.NextAssignmentId;
    //    Call newItem = item with { Id = nextId }; // Creates new item with the next available ID
    //    Calls.Add(newItem); // Adds the new item to the list
    //    XMLTools.SaveListToXMLSerializer(Calls, Config.s_calls_xml);
    //}

    /// <summary>
    /// Reads a call from the list by its Id.
    /// </summary>
    /// <param name="id">The Id of the call to be read.</param>
    /// <returns>The call with the matching Id, or null if not found.</returns>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public Call Read(int id)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
     
        return Calls.FirstOrDefault(obj => obj.Id == id); // Returns the Calls with the matching Id, or null if not found
    }

    /// <summary>
    /// Reads a call from the list by a custom filter function.
    /// </summary>
    /// <param name="filter">A filter function to match the call.</param>
    /// <returns>The first call that matches the filter, or null if no match is found.</returns>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public Call? Read(Func<Call, bool> filter) // Stage 2
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        XMLTools.SaveListToXMLSerializer(Calls, Config.s_calls_xml);
        return Calls.FirstOrDefault(filter); // Returns the first matching assignment based on the filter
    }
    /// <summary>
    /// Reads all calls, optionally filtered by a custom function.
    /// </summary>
    /// <param name="filter">An optional filter function to match calls. If null, returns all calls.</param>
    /// <returns>An IEnumerable of calls that match the filter, or all calls if no filter is provided.</returns>
    public IEnumerable<Call> ReadAll(Func<Call, bool>? filter = null)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);

        if (filter == null)
        {
            return Calls; // No need to save the list back to XML if no filter is applied
        }

        return Calls.Where(filter); // If there's a filter, apply it
    }
    /// <summary>
    /// Updates an existing call by first deleting the old one (if it exists) and then creating the new one.
    /// </summary>
    /// <param name="item">The call to be updated.</param>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public void Update(Call item)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        if (Calls.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Calls with ID={item.Id} does Not exist");
        Calls.Add(item);
        XMLTools.SaveListToXMLSerializer(Calls, Config.s_calls_xml);
    }

    /// <summary>
    /// Deletes a call from the list by its Id.
    /// </summary>
    /// <param name="id">The Id of the call to be deleted.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the call with the given Id is not found.</exception>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public void Delete(int id)
    {
        List<Call> Calls = XMLTools.LoadListFromXMLSerializer<Call>(Config.s_calls_xml);
        if (Calls.RemoveAll(it => it.Id == id) == 0)
            throw new DalDoesNotExistException($"Calls with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(Calls, Config.s_calls_xml);
    }

    /// <summary>
    /// Deletes all calls from the list.
    /// </summary>
    /// 
    [MethodImpl(MethodImplOptions.Synchronized)]

    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Call>(), Config.s_calls_xml);
    }
}
