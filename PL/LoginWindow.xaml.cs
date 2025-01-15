using System;
using System.Windows;
using System.Windows.Controls;

namespace PL
{
    public partial class LoginWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public int Username { get; set; }
        public string Password { get; set; }

        public LoginWindow()
        {
            InitializeComponent();
            DataContext = this; // הגדרת הקשר הנתונים בין ה-XAML והקוד האחורי
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // קריאה למתודה לבדיקת פרטי ההתחברות
                string role = s_bl.Volunteer.Login(Username, Password);

                // מעבר למסך הבא בהתאם לסוג המשתמש
                if (role == "admin")
                {
                    new MainWindow().Show();
                }
                else if (role == "volunteer")
                {
                    // שליחה של תעודת הזהות למסך המתנדב
                    new Volunteer.MainVolunteerWindow(Username).Show();
                }

                // סגירת חלון ההתחברות
                this.Close();
            }
            catch (Exception ex)
            {
                // תצוגת הודעת שגיאה אם שם המשתמש או הסיסמה לא נכונים
                MessageBox.Show($"Login failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = ((PasswordBox)sender).Password; // עדכון הסיסמה מתוך PasswordBox
        }
    }
}
