using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace PL.Volunteer
{
    public partial class ChooseCallWindow : Window
    {
        public ObservableCollection<OpenCallInList> Calls { get; set; } = new ObservableCollection<OpenCallInList>();

        public ChooseCallWindow()
        {
            InitializeComponent();
            DataContext = this;

            // טוען רשימת קריאות לדוגמה
            LoadCalls();
        }

        private void LoadCalls()
        {
            // כאן יש לטעון את רשימת הקריאות ממקור נתונים אמיתי (לדוגמה, API או מאגר נתונים)
            // לדוגמה, מוסיפים קריאות מדומות
            Calls.Add(new OpenCallInList { Id = 1, Type = "חירום", Address = "רחוב דוגמה 1, תל אביב", Distance = 3.5 });
            Calls.Add(new OpenCallInList { Id = 2, Type = "סיוע", Address = "רחוב דוגמה 2, חיפה", Distance = 7.2 });
            Calls.Add(new OpenCallInList { Id = 3, Type = "ליווי", Address = "רחוב דוגמה 3, ירושלים", Distance = 12.4 });

            CallsDataGrid.ItemsSource = Calls;
        }

        private void ChooseCall_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int callId)
            {
                var selectedCall = Calls.FirstOrDefault(c => c.Id == callId);
                if (selectedCall != null)
                {
                    // פעולה לבחירת קריאה
                    MessageBox.Show($"בחרת קריאה {selectedCall.Id}: {selectedCall.Type} בכתובת {selectedCall.Address}", "אישור");

                    // הסרת הקריאה מהטבלה
                    Calls.Remove(selectedCall);
                }
            }
        }

        private void UpdateAddress_Click(object sender, RoutedEventArgs e)
        {
            string newAddress = NewAddressTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(newAddress))
            {
                // פעולה לעדכון כתובת (לדוגמה, עדכון הכתובת במערכת)
                MessageBox.Show($"הכתובת עודכנה ל: {newAddress}", "עדכון כתובת");
                // אפשר לקרוא כאן לשיטה שמרעננת את רשימת הקריאות לפי הכתובת החדשה
            }
            else
            {
                MessageBox.Show("אנא הזן כתובת חוקית.", "שגיאה");
            }
        }

        private void CallsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CallsDataGrid.SelectedItem is OpenCallInList selectedCall)
            {
                // עדכון תיאור מילולי
                CallDetailsTextBox.Text = $"סוג קריאה: {selectedCall.Type}\nכתובת: {selectedCall.Address}\nמרחק: {selectedCall.Distance} ק״מ";

                // הצגת מיקומים במפה (להשלים לפי שימוש במפה מתאימה)
                // UpdateMap(selectedCall);
            }
        }
    }

    public class OpenCallInList
    {
        public int Id { get; set; } // מזהה ייחודי
        public string Type { get; set; } // סוג הקריאה
        public string Address { get; set; } // כתובת
        public double Distance { get; set; } // מרחק מכתובת המתנדב
    }
}
