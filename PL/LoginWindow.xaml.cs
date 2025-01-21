using System;
using System.Windows;
using System.Windows.Controls;
using BlApi; // Ensure the correct namespace for your BlApi

namespace PL
{
    public partial class LoginWindow : Window
    {
        private static readonly IBl s_bl = Factory.Get(); // Adjust the BlApi.Factory namespace if needed

        public int Username { get; set; }
        public string Password { get; set; } = string.Empty; // Initialize to avoid null reference

        public LoginWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string role = s_bl.Volunteer.Login(Username, Password);
                if (role == "admin")
                {
                    new MainWindow().Show();
                }
                else if (role == "volunteer")
                {
                    new Volunteer.MainVolunteerWindow(Username).Show();
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = ((PasswordBox)sender).Password;
        }
    }
}
