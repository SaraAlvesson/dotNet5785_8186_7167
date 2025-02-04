using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using BO;
using static BO.Enums;

namespace PL.Volunteer
{
    public partial class MainVolunteerWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        // Events for observers
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? CallCompleted;
        public event EventHandler? CallCancelled;
        public event EventHandler? VolunteerUpdated;

        // Property change notification
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<VolunteerTypeEnum> PositionCollection { get; set; }

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
            VolunteerUpdated?.Invoke(this, EventArgs.Empty);
        }

        // Dependency properties
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
            set
            {
                SetValue(CurrentVolunteerProperty, value);
                OnPropertyChanged(nameof(CurrentVolunteer)); // Ensure property change notification
            }
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
            this.DataContext = this;
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
            if (CurrentVolunteer?.VolunteerTakenCare == null && CurrentVolunteer?.Active == true)
            {
                try
                {
                    new Volunteer.ChooseCallWindow(CurrentVolunteer).Show();
                    RefreshVolunteerData(CurrentVolunteer.Id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error choosing call: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("You cannot choose a call at the moment. You are either already handling a call or not active.");

            }
        }

        private void RefreshVolunteerData(int volunteerId)
        {
            try
            {
                CurrentVolunteer = s_bl.Volunteer.RequestVolunteerDetails(volunteerId);
                OnPropertyChanged(nameof(CurrentVolunteer)); // Ensure property change notification
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing volunteer data: {ex.Message}");
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

                    // Notify observers
                    OnVolunteerUpdated();

                    RefreshVolunteerData(CurrentVolunteer.Id);
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl?.Volunteer.AddObserver(ObserveVolunteerListChanges);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Volunteer.RemoveObserver(ObserveVolunteerListChanges);
        }

        private void ObserveVolunteerListChanges()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (CurrentVolunteer != null)
                {
                    RefreshVolunteerData(CurrentVolunteer.Id);
                }
            });
        }

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
                if (CurrentVolunteer?.VolunteerTakenCare == null)
                {
                    MessageBox.Show("Error: No call is associated with this volunteer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Mark call as completed
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
                    s_bl.Call.GetVolunteerClosedCalls(CurrentVolunteer.Id, null, null);
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
                if (CurrentVolunteer?.VolunteerTakenCare != null)
                {
                    s_bl.Call.UpdateToCancelCallTreatment(CurrentVolunteer.Id, CurrentVolunteer.VolunteerTakenCare.Id);
                    MessageBox.Show("Call canceled successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    OnCallCancelled();
                    RefreshVolunteerData(CurrentVolunteer.Id);
                }
                else
                {
                    MessageBox.Show("Error: No call is associated with this volunteer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidUpdate()
        {
            return !string.IsNullOrEmpty(CurrentVolunteer?.Email) && !string.IsNullOrEmpty(CurrentVolunteer?.Location);
        }
    }
}
