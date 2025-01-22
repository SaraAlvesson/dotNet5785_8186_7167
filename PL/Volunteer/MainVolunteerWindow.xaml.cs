using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using BO;

namespace PL.Volunteer
{
    public partial class MainVolunteerWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        // אירועים לצופים
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? CallCompleted;
        public event EventHandler? CallCancelled;
        public event EventHandler? VolunteerUpdated;  // אירוע נוסף לצופים

        // חיבור לפונקציות שמבצעות עדכון במודל
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnCallCompleted()
        {
            CallCompleted?.Invoke(this, EventArgs.Empty);
        }

        protected void OnCallCancelled()
        {
            CallCancelled?.Invoke(this, EventArgs.Empty);
        }

        protected void OnVolunteerUpdated()
        {
            VolunteerUpdated?.Invoke(this, EventArgs.Empty);  // הפעלת צופים
        }

        // מאפייני המודל
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

        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set { SetValue(CurrentVolunteerProperty, value); }
        }

        public static readonly DependencyProperty CurrentVolunteerProperty =
            DependencyProperty.Register("CurrentVolunteer", typeof(BO.Volunteer), typeof(MainVolunteerWindow), new PropertyMetadata(null));

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
                RefreshVolunteerData(id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching volunteer details: {ex.Message}");
            }
        }
        private void ButtonChosenCall_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer.VolunteerTakenCare == null && CurrentVolunteer.Active)
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
        private void RefreshVolunteerData(int volunteerId)
        {
            try
            {
                CurrentVolunteer = s_bl.Volunteer.RequestVolunteerDetails(volunteerId);
                OnPropertyChanged(nameof(CurrentVolunteer));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing volunteer data: {ex.Message}");
            }
        }

        // עדכון מתנדב
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
                    RefreshVolunteerData(CurrentVolunteer.Id);
                    OnVolunteerUpdated();  // צופה למצב העדכון
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

        // טיפול בסיום טיפול
        private void ButtonComplete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to end your treatment for this call?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                s_bl.Call.UpdateCallAsCompleted(CurrentVolunteer.Id, CurrentVolunteer.VolunteerTakenCare.Id);
                MessageBox.Show("Call ended successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                OnCallCompleted();
                RefreshVolunteerData(CurrentVolunteer.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {

            var result = MessageBox.Show(
                "Are you sure you want to cancel your treatment for this call?",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                s_bl.Call.UpdateToCancelCallTreatment(CurrentVolunteer.Id, CurrentVolunteer.VolunteerTakenCare.Id);
                MessageBox.Show("Call canceled successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        //                OnCallCancelled();
        //                RefreshVolunteerData(CurrentVolunteer.Id);
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }

        //        // תוקף העדכון

        //    }
        //}
    


       
       

        private bool IsValidUpdate()
        {
            return !string.IsNullOrEmpty(CurrentVolunteer?.Email) && !string.IsNullOrEmpty(CurrentVolunteer?.Location);
        }

    }
}

