
namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

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
        XElement volunteersRootElem = XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml);
        if (volunteersRootElem.Elements().Any(e => (int?)e.Element("Id") == item.Id))
            throw new DalAlreadyExistException($"Volunteer with ID={item.Id} already exists");
        volunteersRootElem.Add(new XElement("Volunteer", item));
        XMLTools.SaveListToXMLElement(volunteersRootElem, Config.s_volunteers_xml);
    }


    /// <summary>
    /// Deletes a volunteer record by its ID.
    /// If the volunteer with the given ID is found, it is removed from the list.
    /// Throws a DalDoesNotExistException if the volunteer with the given ID does not exist.
    /// </summary>
    public void Delete(int id)
    {
        XElement volunteersRootElem = XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml);
        XElement volunteerElem = volunteersRootElem.Elements().FirstOrDefault(e => (int?)e.Element("Id") == id);
        if (volunteerElem == null)
            throw new DalDoesNotExistException($"Volunteer with ID={id} not found");

        volunteerElem.Remove();
        XMLTools.SaveListToXMLElement(volunteersRootElem, Config.s_volunteers_xml);
    }


    /// <summary>
    /// Deletes all volunteer records from the list.
    /// </summary>
    public void DeleteAll()
    {
        XElement volunteersRootElem = XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml);
        volunteersRootElem.RemoveAll();
        XMLTools.SaveListToXMLElement(volunteersRootElem, Config.s_volunteers_xml);
    }


    /// <summary>
    /// Reads a volunteer record by its ID.
    /// Returns the volunteer if found, otherwise returns null.
    /// </summary>
    public Volunteer? Read(int id)
    {
        XElement? volunteertElem =
XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml).Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);
        return volunteertElem is null ? null : getVolunteer(volunteertElem);
        
    }

    /// <summary>
    /// Reads a volunteer record based on a given filter.
    /// Returns the first volunteer that matches the filter criteria, or null if no match is found.
    /// </summary>
    public Volunteer? Read(Func<Volunteer, bool> filter)
    {// Stage 2
        /* => DataSource.Volunteers.FirstOrDefault(filter)*/
        ; // Return the first volunteer matching the filter, or null if none matches
        return XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml).Elements().Select(s => getVolunteer(s)).FirstOrDefault(filter);
    }


    /// <summary>
    /// Reads all volunteer records, optionally filtered by a given condition.
    /// If no filter is provided, returns all volunteers.
    /// </summary>
    public IEnumerable<Volunteer> ReadAll(Func<Volunteer, bool>? filter = null) // Stage 2
    {
        //=> filter == null
        //    ? DataSource.Volunteers.Select(obj => obj) // Return all volunteers if no filter is provided
        //    : DataSource.Volunteers.Where(filter); // Return filtered volunteers based on the provided filter
        if (filter == null)
            return XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml).Elements().Select(s =>getVolunteer(s));
        else
            return XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml).Elements().Select(s => getVolunteer(s)).Where(filter);



    }


    /// <summary>
    /// Updates an existing volunteer record.
    /// Deletes the existing volunteer and then creates the new one in the list.
    /// </summary>
    public void Update(Volunteer item)
    {
        XElement volunteersRootElem = XMLTools.LoadListFromXMLElement(Config.s_volunteers_xml);
        XElement volunteerElem = volunteersRootElem.Elements().FirstOrDefault(e => (int?)e.Element("Id") == item.Id);
        if (volunteerElem == null)
            throw new DalDoesNotExistException($"Volunteer with ID={item.Id} not found");

        volunteerElem.Remove();
        volunteersRootElem.Add(new XElement("Volunteer", item));
        XMLTools.SaveListToXMLElement(volunteersRootElem, Config.s_volunteers_xml);
    }

    static Volunteer getVolunteer(XElement s)
    {
        return new DO.Volunteer()
        {

            Id = s.ToIntNullable("Id") ?? throw new FormatException("can't convert id"),//////////////
            FullName = (string?)s.Element("FullName") ?? "",
            PhoneNumber = (string?)s.Element("PhoneNumber") ?? "",
            Email = (string?)s.Element("Email") ?? "",
            Active = (bool?)s.Element("IsActive") ?? false,/////////////////////////////////
            DistanceType = Enum.TryParse((string?)s.Element("DistanceType"), out DistanceType result) ? result : default,
            Position = Enum.TryParse((string?)s.Element("Position"), out Position position) ? position : default,
            Password = (string?)s.Element("Password") ?? null,
            Location = (string?)s.Element("Location") ?? null,
            Latitude = (double?)s.Element("Latitude") ?? null,
            Longitude = (double?)s.Element("Longitude") ?? null,
            MaxDistance = (double?)s.Element("MaxDistance") ?? null



        };
    }
}
