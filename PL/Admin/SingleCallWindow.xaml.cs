using System;
using System.Windows;
using System.Windows.Input;
using System.Net.Mail;
using static BO.Enums;

namespace PL.Admin
{
    public partial class SingleCallWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public SingleCallWindow(int id = 0)
        {
            InitializeComponent();

            if (id != 0)
            {
                CurrentCall = s_bl.Call.GetCallDetails(id);
                SetEditableFields(CurrentCall.CallStatus);
            }
        }

        public BO.Call CurrentCall
        {
            get { return (BO.Call)GetValue(CurrentCallProperty); }
            set { SetValue(CurrentCallProperty, value); }
        }

        public static readonly DependencyProperty CurrentCallProperty =
            DependencyProperty.Register("CurrentCall", typeof(BO.Call), typeof(SingleCallWindow), new PropertyMetadata(null));

        public bool IsEditable { get; private set; }
        public bool IsMaxTimeEditable { get; private set; }

        private void SetEditableFields(CalltStatusEnum status)
        {
            // ניתן לערוך את כל הפרטים אם הסטטוס הוא פתוחה או פתוחה בסיכון
            IsEditable = status == CalltStatusEnum.OPEN || status == CalltStatusEnum.CallAlmostOver;

            // ניתן לערוך רק את הזמן המקסימלי לסיום אם הסטטוס הוא בטיפול או בטיפול בסיכון
            IsMaxTimeEditable = status == CalltStatusEnum.CallIsBeingTreated || status == CalltStatusEnum.CallTreatmentAlmostOver;
        }

        private void UpdateCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            // אם הקריאה לא ניתנת לעריכה (היא סגורה או פג תוקפה)
            if (!IsEditable)
            {
                MessageBox.Show("You cannot edit this call because it's closed or expired.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // עדכון פרטי הקריאה בשכבה הלוגית
                s_bl.Call.UpdateCallDetails(CurrentCall);

                // שליחת אימייל למתנדבים במרחק מתאים לאחר עדכון הקריאה
              //  SendEmailToVolunteers(CurrentCall);

                MessageBox.Show("Call updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            // כאן תוכל להוסיף את הבדיקות של הפורמט (למשל אם הכתובת חוקית או אם יש שדות חובה)
            return true;
        }

        //private void SendEmailToVolunteers(BO.Call newCall)
        //{
        //    // חישוב המיקום הגיאוגרפי של הקריאה
        //    var callLocation = GetGeolocationCoordinates(newCall.Address); // הפונקציה הזו תחזיר מיקום גיאוגרפי

        //    // קבלת כל המתנדבים מתוך הרשימה
        //    var volunteers = s_bl.Volunteer.RequestVolunteerList();

        //    // סינון המתנדבים על פי המרחק
        //    foreach (var volunteer in volunteers)
        //    {
        //        var volunteerLocation = volunteer.location; // הכתוב של המתנדב (קו רוחב/אורך)

        //        // חישוב המרחק בין מיקום הקריאה למיקום המתנדב
        //        double distance = CalculateDistance(callLocation, volunteerLocation);

        //        // אם המרחק בטווח הקטן או שווה למרחק המוגדר
        //        if (distance <= 10.0) // 10.0 קילומטרים לדוגמה
        //        {
        //            SendEmail(volunteer.email, newCall); // שליחת האימייל
        //        }
        //    }
        //}

        //// פונקציה לחישוב המרחק בין שתי נקודות גיאוגרפיות
        //private double CalculateDistance(Location loc1, Location loc2)
        //{
        //    const double EarthRadius = 6371.0; // קילומטרים
        //    double lat1 = loc1.Latitude;
        //    double lon1 = loc1.Longitude;
        //    double lat2 = loc2.Latitude;
        //    double lon2 = loc2.Longitude;

        //    double latDistance = ToRadians(lat2 - lat1);
        //    double lonDistance = ToRadians(lon2 - lon1);

        //    double a = Math.Sin(latDistance / 2) * Math.Sin(latDistance / 2) +
        //               Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
        //               Math.Sin(lonDistance / 2) * Math.Sin(lonDistance / 2);

        //    double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        //    return EarthRadius * c; // המרחק בקילומטרים
        //}

        private double ToRadians(double angle)
        {
            return angle * (Math.PI / 180);
        }

        private void SendEmail(string email, BO.Call newCall)
        {
            try
            {
                // הגדרת אימייל
                var mail = new MailMessage();
                mail.From = new MailAddress("no-reply@yourcompany.com");
                mail.To.Add(email);
                mail.Subject = $"Call updated - {newCall.CallType}";
                mail.Body = $"Hello, \n\nThe details of a call have been updated: \n\n" +
                            $"Address: {newCall.Address} \n" +
                            $"Description: {newCall.VerbDesc} \n\n" +
                            $"Please log into the system and select whether you would like to handle it.";

                // שליחת האימייל
                var smtpClient = new SmtpClient("smtp.yourserver.com");
                smtpClient.Send(mail);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending email: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
