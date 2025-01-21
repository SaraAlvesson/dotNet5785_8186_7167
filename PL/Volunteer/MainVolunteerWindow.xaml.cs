using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BO;
using PL.Admin;
using static BO.Enums;

namespace PL.Volunteer
{
    /// <summary>
    /// Interaction logic for MainVolunteerWindow.xaml
    /// </summary>
    public partial class MainVolunteerWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // VolunteerInList property (DependencyProperty)
        public IEnumerable<BO.Volunteer> Volunteer
        {
            get { return (IEnumerable<BO.Volunteer>)GetValue(VolunteerFieldListProperty); }
            set { SetValue(VolunteerFieldListProperty, value); }
        }

        public static readonly DependencyProperty VolunteerFieldListProperty =
           DependencyProperty.Register(
               "Volunteer",
               typeof(IEnumerable<BO.Volunteer>),
               typeof(MainVolunteerWindow),
               new PropertyMetadata(null));

        // CurrentVolunteer property (DependencyProperty)
        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set { SetValue(CurrentVolunteerProperty, value); }
        }

        public static readonly DependencyProperty CurrentVolunteerProperty =
            DependencyProperty.Register("CurrentVolunteer", typeof(BO.Volunteer), typeof(MainVolunteerWindow), new PropertyMetadata(null));

        // CurrentCall property (DependencyProperty)
        public BO.CallInProgress? CurrentCall
        {
            get { return (BO.CallInProgress?)GetValue(CurrentCallProperty); }
            set { SetValue(CurrentCallProperty, value); }
        }

        public static readonly DependencyProperty CurrentCallProperty =
            DependencyProperty.Register("CurrentCall", typeof(BO.CallInProgress), typeof(MainVolunteerWindow), new PropertyMetadata(null));

        public MainVolunteerWindow(int id)
        {
            InitializeComponent();
           

            try
            {
                // Fetch and assign the current volunteer details (replace with appropriate ID)
                CurrentVolunteer = s_bl.Volunteer.RequestVolunteerDetails(id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching volunteer details: {ex.Message}");
            }
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer == null)
            {
                MessageBox.Show("No volunteer selected.");
                return;
            }

            if (IsValidUpdate())
            {
                try
                {
                    s_bl.Volunteer.UpdateVolunteerDetails(CurrentVolunteer.Id, CurrentVolunteer);
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
            if (CurrentVolunteer.VolunteerTakenCare != null)
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
                    s_bl.Call.UpdateToCancelCallTreatment(CurrentVolunteer.Id, CurrentVolunteer.VolunteerTakenCare.Id);
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
              if (CurrentVolunteer.VolunteerTakenCare==null && CurrentVolunteer.Active)
            {
                try
                {
                    new Volunteer.ChooseCallWindow(CurrentVolunteer).Show();
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
                    // Open new window for displaying history
                    new ListClosedCallsVolunteer(CurrentVolunteer.Id).Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error retrieving history: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("No volunteer selected.");
            }
        }

        private bool IsValidUpdate()
        {
            return !string.IsNullOrEmpty(CurrentVolunteer?.Email) && !string.IsNullOrEmpty(CurrentVolunteer?.Location);
        }
    }
}
