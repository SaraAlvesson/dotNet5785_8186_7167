namespace Dal;

using DalApi;
using DO;
using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

static class XMLTools
{
    const string s_xmlDir = @"..\xml\";
    static XMLTools()
    {
        if (!Directory.Exists(s_xmlDir))
            Directory.CreateDirectory(s_xmlDir);
    }

    #region SaveLoadWithXMLSerializer
    public static void SaveListToXMLSerializer<T>(List<T> list, string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            using FileStream file = new(xmlFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            new XmlSerializer(typeof(List<T>)).Serialize(file, list);
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }
    public static List<T> LoadListFromXMLSerializer<T>(string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            if (!File.Exists(xmlFilePath)) return new();
            using FileStream file = new(xmlFilePath, FileMode.Open);
            XmlSerializer x = new(typeof(List<T>));
            return x.Deserialize(file) as List<T> ?? new();
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {xmlFilePath}, {ex.Message}");
        }
    }
    #endregion

    #region SaveLoadWithXElement
    public static void SaveListToXMLElement(XElement rootElem, string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            rootElem.Save(xmlFilePath);
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }
    public static XElement LoadListFromXMLElement(string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            if (File.Exists(xmlFilePath))
                return XElement.Load(xmlFilePath);
            XElement rootElem = new(xmlFileName);
            rootElem.Save(xmlFilePath);
            return rootElem;
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }
    #endregion

    #region XmlConfig
    public static int AndIncreaseConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int nextId = root.ToIntNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        root.Element(elemName)?.SetValue((nextId + 1).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
        return nextId;
    }
    public static TimeSpan GetConfigTimeSpanVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        TimeSpan dt = root.ToSpanTimeNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return dt;
    }
    public static int GetConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int num = root.ToIntNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }
    public static int GetAndIncreaseConfigIntVal(string xmlFileName, string elemName)//////////////
    {
        return (AndIncreaseConfigIntVal(xmlFileName, elemName));
    }
    //XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
    //int num = root.ToIntNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
    //return (num+1);

    public static DateTime GetConfigDateVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        DateTime dt = root.ToDateTimeNullable(elemName) ?? throw new FormatException($"can't convert:  {xmlFileName}, {elemName}");
        return dt;
    }
    public static void SetConfigIntVal(string xmlFileName, string elemName, int elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    public static void SetConfigDateVal(string xmlFileName, string elemName, DateTime elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    public static void SetConfigSpanTimeVal(string xmlFileName, string elemName, TimeSpan elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }


    #endregion


    #region ExtensionFuctions
    public static T? ToEnumNullable<T>(this XElement element, string name) where T : struct, Enum =>
        Enum.TryParse<T>((string?)element.Element(name), out var result) ? (T?)result : null;
    public static DateTime? ToDateTimeNullable(this XElement element, string name) =>
        DateTime.TryParse((string?)element.Element(name), out var result) ? (DateTime?)result : null;
    public static double? ToDoubleNullable(this XElement element, string name) =>
        double.TryParse((string?)element.Element(name), out var result) ? (double?)result : null;
    public static int? ToIntNullable(this XElement element, string name) =>
        int.TryParse((string?)element.Element(name), out var result) ? (int?)result : null;
    public static TimeSpan? ToSpanTimeNullable(this XElement element, string name) =>
       TimeSpan.TryParse((string?)element.Element(name), out var result) ? (TimeSpan?)result : null;





    static Assignment getAssignment(XElement s)
    {
        return new DO.Assignment()
        {
            Id = s.ToIntNullable("Id") ?? throw new FormatException("Can't convert Id"),

            // המרה ל-CallId ו-VolunteerId, אם לא מצליח, נזרוק חריגה
            CallId = int.Parse((string?)s.Element("CallId") ?? throw new FormatException("CallId is missing")),
            VolunteerId = int.Parse((string?)s.Element("VolunteerId") ?? throw new FormatException("VolunteerId is missing")),

            // טיפול ב-AppointmentTime כ- DateTime
            AppointmentTime = DateTime.TryParse((string?)s.Element("AppointmentTime"), out DateTime appointmentTime) ? appointmentTime : throw new FormatException("Invalid AppointmentTime"),

            // טיפול ב-FinishAppointmentTime כ- DateTime? (nullable)
            FinishAppointmentTime = DateTime.TryParse((string?)s.Element("FinishAppointmentTime"), out DateTime finishTime)
                ? finishTime
                : (DateTime?)null,  // נשאיר את השדה כ-null אם אין ערך ב-XML

            // אם הערך ב-XML לא תקין, נשאיר את FinishAppointmentType כ-default של Enum?
            FinishAppointmentType = Enum.TryParse((string?)s.Element("FinishAppointmentType"), out FinishAppointmentType finishType)
                ? finishType
                : default
        };
    }



    static Call getCall(XElement s)
    {
        return new DO.Call()
        {
            Id = s.ToIntNullable("Id") ?? throw new FormatException("Can't convert Id"),
            Adress = (string?)s.Element("Adress") ?? throw new FormatException("Adress is missing"),
            Latitude = (double?)s.Element("Latitude") ?? throw new FormatException("Latitude is missing"),
            Longitude = (double?)s.Element("Longitude") ?? throw new FormatException("Longitude is missing"),

            // טיפול בשדה מסוג DateTime? (nullable)
            OpenTime = DateTime.TryParse((string?)s.Element("OpenTime"), out DateTime openTime) ? openTime : throw new FormatException("Invalid OpenTime"),

            // טיפול בשדה מסוג Enum? (nullable) - עם Enum.TryParse
            CallType = Enum.TryParse((string?)s.Element("CallType"), out CallType callType) ? callType : throw new FormatException("Invalid CallType"),

            VerbDesc = (string?)s.Element("VerbDesc"),  // אופציונלי, ניתן להחזיר null אם לא קיים

            // טיפול בשדה nullable DateTime? MaxTime
            MaxTime = DateTime.TryParse((string?)s.Element("MaxTime"), out DateTime maxTime) ? maxTime : (DateTime?)null
        };
    }


    #endregion

}