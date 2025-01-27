using System;
using System.Windows;
using System.Windows.Controls;
using BlApi; // Ensure the correct namespace for your BlApi

namespace PL
{
    public partial class LoginWindow : Window
    {
        private static readonly IBl s_bl = Factory.Get(); // Adjust the BlApi.Factory namespace if needed

        private static bool IsAdminLoggedIn = false; // Ensure only one admin can log in at a time

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
                // Authenticate user and get their role
                string role = s_bl.Volunteer.Login(Username, Password);

                if (role == "admin")
                {
                    if (IsAdminLoggedIn)
                    {
                        MessageBox.Show("An admin is already logged in.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    IsAdminLoggedIn = true; // Mark admin as logged in

                    // Ask admin to choose screen
                    var result = MessageBox.Show("Do you want to enter the Admin screen? (Click 'No' for Volunteer screen)",
                                                 "Choose Role", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        new MainWindow().Show(); // Admin screen
                    }
                    else
                    {
                        new Volunteer.MainVolunteerWindow(Username).Show(); // Volunteer screen
                    }
                }
                else if (role == "volunteer")
                {
                    new Volunteer.MainVolunteerWindow(Username).Show(); // Volunteer screen
                }

                this.Hide(); // Keep login screen accessible
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

        public static void LogoutAdmin()
        {
            IsAdminLoggedIn = false; // Mark admin as logged out
        }
    }
}
