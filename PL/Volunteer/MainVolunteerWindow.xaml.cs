using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using BO;
using static BO.Enums;

namespace PL.Volunteer
{
    public partial class MainVolunteerWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private DispatcherTimer _timer;
       




        // Events for observers
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? CallCompleted;
        public event EventHandler? CallCancelled;
        

        // Flag for handling async updates
        private volatile bool isUpdateInProgress = false;

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
            StartAutoRefresh();
        }
        private void StartAutoRefresh()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10) // רענון כל 5 שניות
            };
            _timer.Tick += (s, e) => ObserveVolunteerListChanges();
            _timer.Start();
        }
        private void ButtonChosenCall_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer?.VolunteerTakenCare == null && CurrentVolunteer?.Active == true)
            {
                try
                {
                    new Volunteer.ChooseCallWindow(CurrentVolunteer).Show();
                    RefreshVolunteerData(CurrentVolunteer.Id);
                    NotifyObservers();
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
                NotifyObservers();
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
                    if (isUpdateInProgress) return; // Skip if update is already in progress

                    isUpdateInProgress = true;
                    s_bl.Volunteer.UpdateVolunteerDetails(CurrentVolunteer.Id, CurrentVolunteer);

                    MessageBox.Show("Volunteer updated successfully.");

                    // Notify observers

                   
                    NotifyObservers(); // Notify observers after update

                    RefreshVolunteerData(CurrentVolunteer.Id);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating volunteer: {ex.Message}");
                }
                finally
                {
                    isUpdateInProgress = false; // Reset the flag after update is complete
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
                NotifyObservers(); // Notify observers after call completion
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
                    NotifyObservers(); // Notify observers after call cancellation
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
        private volatile bool isUpdatingView = false;

        private void ObserveVolunteerListChanges()
        {
            if (isUpdatingView) return; // אם יש כבר עדכון בתהליך, מתעלמים מהבקשה החדשה

            isUpdatingView = true;
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (CurrentVolunteer != null)
                    {
                        RefreshVolunteerData(CurrentVolunteer.Id);
                    }
                }
                finally
                {
                    isUpdatingView = false; // משחררים את הדגל לאחר סיום העדכון
                }
            });
        }

        private bool IsValidUpdate()
        {
            return !string.IsNullOrEmpty(CurrentVolunteer?.Email) && !string.IsNullOrEmpty(CurrentVolunteer?.Location);
        }

        private void NotifyObservers()
        {
            CallCompleted?.Invoke(this, EventArgs.Empty);
            CallCancelled?.Invoke(this, EventArgs.Empty);
        }

    }
}
