using System;
using System.Collections.Generic;
using BlApi;
using BO;

class Program
{
    // שדה הממשק לשכבת ה-BL
    static readonly IBl s_bl = BlApi.Factory.Get();

    // הצגת תפריט ראשי
    private static void ShowMainMenu()
    {
        Console.Clear();
        Console.WriteLine("תפריט ראשי:");
        Console.WriteLine("1. ניהול סטודנטים");
        Console.WriteLine("2. ניהול קריאות");
        Console.WriteLine("3. ניהול מתנדבים");
        Console.WriteLine("4. יציאה");
    }

    // תפריט ניהול סטודנטים
    private static void ShowStudentMenu()
    {
        bool continueStudentMenu = true;
        while (continueStudentMenu)
        {
            Console.Clear();
            Console.WriteLine("תפריט ניהול סטודנטים:");
            Console.WriteLine("1. קריאה לסטודנט");
            Console.WriteLine("2. הוספת סטודנט");
            Console.WriteLine("3. עדכון סטודנט");
            Console.WriteLine("4. מחיקת סטודנט");
            Console.WriteLine("5. יציאה לתפריט ראשי");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    HandleStudentRead();
                    break;
                case "2":
                    HandleStudentAdd();
                    break;
                case "3":
                    HandleStudentUpdate();
                    break;
                case "4":
                    HandleStudentDelete();
                    break;
                case "5":
                    continueStudentMenu = false;
                    break;
                default:
                    Console.WriteLine("בחירה לא תקפה.");
                    break;
            }
        }
    }

    // טיפול בקריאה לסטודנט
    private static void HandleStudentRead()
    {
        Console.Write("הכנס ID של הסטודנט: ");
        string input = Console.ReadLine();
        if (int.TryParse(input, out int studentId))
        {
            try
            {
                BO.Student? student = s_bl.Student.Read(studentId);
                if (student != null)
                    Console.WriteLine(student);
                else
                    Console.WriteLine("הסטודנט לא נמצא.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"שגיאה: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("הזנת ID לא תקינה.");
        }
    }

    // טיפול בהוספת סטודנט
    private static void HandleStudentAdd()
    {
        Console.Write("הכנס שם של הסטודנט: ");
        string name = Console.ReadLine();
        Console.Write("הכנס גיל של הסטודנט: ");
        string ageInput = Console.ReadLine();

        if (int.TryParse(ageInput, out int age))
        {
            BO.Student newStudent = new BO.Student { Name = name, Age = age };
            try
            {
                s_bl.Student.Add(newStudent);
                Console.WriteLine("הסטודנט נוסף בהצלחה.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"שגיאה בהוספת סטודנט: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("הזנת גיל לא תקינה.");
        }
    }

    // טיפול בעדכון סטודנט
    private static void HandleStudentUpdate()
    {
        Console.Write("הכנס ID של הסטודנט לעדכון: ");
        string input = Console.ReadLine();
        if (int.TryParse(input, out int studentId))
        {
            Console.Write("הכנס שם חדש של הסטודנט: ");
            string name = Console.ReadLine();
            Console.Write("הכנס גיל חדש של הסטודנט: ");
            string ageInput = Console.ReadLine();

            if (int.TryParse(ageInput, out int age))
            {
                BO.Student updatedStudent = new BO.Student { ID = studentId, Name = name, Age = age };
                try
                {
                    s_bl.Student.Update(updatedStudent);
                    Console.WriteLine("הסטודנט עודכן בהצלחה.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"שגיאה בעדכון סטודנט: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("הזנת גיל לא תקינה.");
            }
        }
        else
        {
            Console.WriteLine("הזנת ID לא תקינה.");
        }
    }

    // טיפול במחיקת סטודנט
    private static void HandleStudentDelete()
    {
        Console.Write("הכנס ID של הסטודנט למחיקה: ");
        string input = Console.ReadLine();
        if (int.TryParse(input, out int studentId))
        {
            try
            {
                s_bl.Student.Delete(studentId);
                Console.WriteLine("הסטודנט נמחק בהצלחה.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"שגיאה במחיקת סטודנט: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("הזנת ID לא תקינה.");
        }
    }

    // תפריט ניהול קריאות
    private static void ShowCallMenu()
    {
        bool continueCallMenu = true;
        while (continueCallMenu)
        {
            Console.Clear();
            Console.WriteLine("תפריט ניהול קריאות:");
            Console.WriteLine("1. קריאה לרשימת קריאות");
            Console.WriteLine("2. יציאה לתפריט ראשי");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    HandleCallList();
                    break;
                case "2":
                    continueCallMenu = false;
                    break;
                default:
                    Console.WriteLine("בחירה לא תקפה.");
                    break;
            }
        }
    }

    // טיפול ברשימת קריאות
    private static void HandleCallList()
    {
        try
        {
            var callList = s_bl.Call.GetCallList(null, null, null);
            foreach (var call in callList)
            {
                Console.WriteLine(call);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"שגיאה: {ex.Message}");
        }
    }

    // תפריט ניהול מתנדבים
    private static void ShowVolunteerMenu()
    {
        bool continueVolunteerMenu = true;
        while (continueVolunteerMenu)
        {
            Console.Clear();
            Console.WriteLine("תפריט ניהול מתנדבים:");
            Console.WriteLine("1. קריאה לרשימת מתנדבים");
            Console.WriteLine("2. הוספת מתנדב");
            Console.WriteLine("3. עדכון מתנדב");
            Console.WriteLine("4. מחיקת מתנדב");
            Console.WriteLine("5. יציאה לתפריט ראשי");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    HandleVolunteerList();
                    break;
                case "2":
                    HandleVolunteerAdd();
                    break;
                case "3":
                    HandleVolunteerUpdate();
                    break;
                case "4":
                    HandleVolunteerDelete();
                    break;
                case "5":
                    continueVolunteerMenu = false;
                    break;
                default:
                    Console.WriteLine("בחירה לא תקפה.");
                    break;
            }
        }
    }

    // טיפול ברשימת מתנדבים
    private static void HandleVolunteerList()
    {
        try
        {
            var volunteers = s_bl.Volunteer.RequestVolunteerList(true);
            foreach (var volunteer in volunteers)
            {
                Console.WriteLine(volunteer);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"שגיאה: {ex.Message}");
        }
    }

    // טיפול בהוספת מתנדב
    private static void HandleVolunteerAdd()
    {
        Console.Write("הכנס שם של המתנדב: ");
        string name = Console.ReadLine();
        try
        {
            s_bl.Volunteer.Add(new BO.Volunteer { Name = name });
            Console.WriteLine("המתנדב נוסף בהצלחה.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"שגיאה בהוספת מתנדב: {ex.Message}");
        }
    }

    // טיפול בעדכון מתנדב
    private static void HandleVolunteerUpdate()
    {
        Console.Write("הכנס ID של המתנדב לעדכון: ");
        string input = Console.ReadLine();
        if (int.TryParse(input, out int volunteerId))
        {
            Console.Write("הכנס שם חדש של המתנדב: ");
            string name = Console.ReadLine();
            try
            {
                s_bl.Volunteer.Update(new BO.Volunteer { ID = volunteerId, Name = name });
                Console.WriteLine("המתנדב עודכן בהצלחה.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"שגיאה בעדכון מתנדב: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("הזנת ID לא תקינה.");
        }
    }

    // טיפול במחיקת מתנדב
    private static void HandleVolunteerDelete()
    {
        Console.Write("הכנס ID של המתנדב למחיקה: ");
        string input = Console.ReadLine();
        if (int.TryParse(input, out int volunteerId))
        {
            try
            {
                s_bl.Volunteer.Delete(volunteerId);
                Console.WriteLine("המתנדב נמחק בהצלחה.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"שגיאה במחיקת מתנדב: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("הזנת ID לא תקינה.");
        }
    }

    // הפעלת התוכנית
    static void Main(string[] args)
    {
        bool continueProgram = true;
        while (continueProgram)
        {
            ShowMainMenu();
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowStudentMenu();
                    break;
                case "2":
                    ShowCallMenu();
                    break;
                case "3":
                    ShowVolunteerMenu();
                    break;


