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
            // פתיחת הקובץ עם FileShare.ReadWrite כך שתהיה גישה לקריאה ולכתיבה בו זמנית
            using (FileStream file = new(xmlFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                new XmlSerializer(typeof(List<T>)).Serialize(file, list);
            }
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

            using FileStream file = new(xmlFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            XmlSerializer x = new(typeof(List<T>));
            List<T> result = x.Deserialize(file) as List<T> ?? new();

            // רק אם זה קריאה חדשה נדפיס את הזמן
            if (typeof(T) == typeof(Call))
            {
                foreach (var item in result.Take(1)) // בודקים רק את הפריט הראשון
                {
                    if (item is Call call)
                    {
                        Console.WriteLine($"First Call OpenTime: {call.OpenTime}");
                    }
                }
            }

            return result;
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
            
            // יצירת קובץ חדש עם ערכים התחלתיים נכונים
            XElement rootElem = new("config");
            if (xmlFileName == "data-config.xml")
            {
                rootElem.Add(new XElement("NextCallId", "1000")); // מתחיל מ-1000
                rootElem.Add(new XElement("NextAssignmentId", "1")); // מתחיל מ-1
                rootElem.Add(new XElement("Clock", DateTime.Now.ToString()));
                rootElem.Add(new XElement("RiskRange", TimeSpan.Zero.ToString()));
            }
            else
            {
                rootElem = new(xmlFileName);
            }
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
        XElement root = LoadListFromXMLElement(xmlFileName);
        string? timeSpanStr = root.Element(elemName)?.Value;

        if (TimeSpan.TryParse(timeSpanStr, out TimeSpan timeSpan))
            return timeSpan;

        throw new FormatException($"Invalid TimeSpan value for {elemName} in {xmlFileName}");
    }

    public static void SetConfigTimeSpanVal(string xmlFileName, string elemName, TimeSpan elemVal)
    {
        XElement root = LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal.ToString());
        SaveListToXMLElement(root, xmlFileName);
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

            // טיפול בשדה מסוג DateTime? (nullable) עם ParseExact
            OpenTime = DateTime.TryParseExact((string?)s.Element("OpenTime"), "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime openTime)
                ? openTime
                : throw new FormatException("Invalid OpenTime"),

            // טיפול בשדה מסוג Enum? (nullable) - עם Enum.TryParse
            CallType = Enum.TryParse((string?)s.Element("CallType"), out CallType callType) ? callType : throw new FormatException("Invalid CallType"),

            VerbDesc = (string?)s.Element("VerbDesc"),  // אופציונלי, ניתן להחזיר null אם לא קיים

            // טיפול בשדה nullable DateTime? MaxTime
            MaxTime = DateTime.TryParse((string?)s.Element("MaxTime"), out DateTime maxTime) ? maxTime : (DateTime?)null
        };
    }



    #endregion

}