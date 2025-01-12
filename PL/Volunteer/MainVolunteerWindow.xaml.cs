using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BO;
using static BO.Enums;

namespace PL.Volunteer
{
    /// <summary>
    /// Interaction logic for MainVolunteerWindow.xaml
    /// </summary>
    public partial class MainVolunteerWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        public MainVolunteerWindow()
        {
            InitializeComponent();
        }

        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set { SetValue(CurrentVolunteerProperty, value); }
        }

        public static readonly DependencyProperty CurrentVolunteerProperty =
            DependencyProperty.Register("CurrentVolunteer", typeof(BO.Volunteer), typeof(MainVolunteerWindow), new PropertyMetadata(null));

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            // Verify if the update can be performed (validation)
            if (CurrentVolunteer != null && IsValidUpdate())
            {
                try
                {
                    s_bl.Volunteer.UpdateVolunteerDetails(CurrentVolunteer.Id,CurrentVolunteer);
                    MessageBox.Show("Volunteer updated successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating volunteer: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please fill in all required fields correctly.");
            }
        }

        private void ButtonComplete_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer?.VolunteerTakenCare != null)
            {
                try
                {
                    s_bl.Call.UpdateCallAsCompleted(CurrentVolunteer.Id, CurrentVolunteer.VolunteerTakenCare.Id);
                    MessageBox.Show("Call completed successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error completing call: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("No active call to complete.");
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer?.VolunteerTakenCare != null)
            {
                try
                {
                    s_bl.Call.UpdateToCancelCallTreatment(CurrentVolunteer.VolunteerTakenCare.CallId, CurrentVolunteer.VolunteerTakenCare.Id);
                    MessageBox.Show("Call canceled successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error canceling call: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("No active call to cancel.");
            }
        }

        private void ButtonChosenCall_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer != null && CurrentVolunteer.Active)
            {
                try
                {
                    var chosenCall = s_bl.Call.GetCallDetails(CurrentVolunteer.Id);
                    MessageBox.Show($"Chosen call: {chosenCall.Id}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error choosing call: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("You cannot choose a call at this time.");
            }
        }

        private void ButtonHistory_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer != null)
            {
                try
                {
                    var callHistory = s_bl.Call.GetVolunteerClosedCalls(CurrentVolunteer.Id, null, null);

                    // פתיחת חלון חדש עם מזהה המתנדב
                    new ListClosedCallsVolunteer(CurrentVolunteer.Id).Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error retrieving history: {ex.Message}");
                }
            }
        }


        // Validate if the update is correct (add necessary conditions here)
        private bool IsValidUpdate()
        {
            // Example: Validate email format, address, and other necessary fields
            return !string.IsNullOrEmpty(CurrentVolunteer?.Email) && !string.IsNullOrEmpty(CurrentVolunteer?.Location);
        }
    }
}
