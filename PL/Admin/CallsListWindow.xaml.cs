using BO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static BO.Enums;

namespace PL.Admin
{
    /// <summary>
    /// Interaction logic for CallsListWindow.xaml
    /// </summary>
    public partial class CallsListWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        public BO.CallInList? SelectedCall { get; set; }
        private CallFieldEnum _selectedCallField = CallFieldEnum.CallId;

        public CallsListWindow()
        {
            InitializeComponent();
            Filteredcall = new ObservableCollection<CallInList>();
        }

        // Property for Call List
        public IEnumerable<CallInList> CallList
        {
            get => (IEnumerable<CallInList>)GetValue(CallListProperty);
            set
            {
                SetValue(CallListProperty, value ?? new List<CallInList>());
                OnPropertyChanged(nameof(CallList));
                FilterVolunteers(); // Filter the list whenever the CallList is updated
            }
        }

        // Event for PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // Helper for PropertyChanged event
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // DependencyProperty for CallList
        public static readonly DependencyProperty CallListProperty =
           DependencyProperty.Register(
               "CallList",
               typeof(IEnumerable<CallInList>),
               typeof(CallsListWindow),
               new PropertyMetadata(null));

        // Load window and subscribe to observer
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl?.Call.AddObserver(ObserveCallListChanges);
            ObserveCallListChanges();
        }

        // Unsubscribe observer on close
        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Call.RemoveObserver(ObserveCallListChanges);
        }

        // Observable collections for filtering
        public ObservableCollection<CallInList> Filteredcall { get; set; }

        // Selected call field for filtering
        public CallFieldEnum SelectedCallField
        {
            get => _selectedCallField;
            set
            {
                if (_selectedCallField != value)
                {
                    _selectedCallField = value;
                    FilterVolunteers(); // Filter the list when the field is updated
                }
            }
        }

        // Filter the call list based on the selected field
        private void FilterVolunteers()
        {
            Filteredcall.Clear();
            if (CallList != null)
            {
                foreach (var call in CallList.Where(call =>
                    SelectedCallField == CallFieldEnum.CallType && call.CallType == CallTypeEnum.None || // Example condition
                    SelectedCallField == CallFieldEnum.CallId && call.CallId == 1)) // Replace 1 with the desired ID
                {
                    Filteredcall.Add(call);
                }
            }
        }

        // Update Call List based on selected field
        public void UpdateCallList(CallFieldEnum? field)
        {
            try
            {
                // Filter and sort the call list using the ICall methods
                CallList = s_bl?.Call.GetCallList(
                    filter: field,
                    toFilter: null,  // Additional filter logic if needed
                    toSort: field) ?? new List<CallInList>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating call list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CallList = new List<CallInList>();  // In case of error, display empty list
            }
        }

        // Observe changes in call list
        private void ObserveCallListChanges()
        {
            UpdateCallList(_selectedCallField);
        }

        // Handle double-click on call in the list
        private void lsvCallList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCall != null)
            {
                new SingleCallWindow(SelectedCall.CallId).Show();
            }
        }

        // Add a new call when button is clicked
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            new SingleCallWindow().Show();
        }

        // Delete a call if allowed (open status and no assignments)
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCall != null && SelectedCall.Status == CalltStatusEnum.OPEN && SelectedCall.LastVolunteerName == null)
            {
                // Delete call
                s_bl?.Call.DeleteCall(SelectedCall.CallId);
                UpdateCallList(_selectedCallField);  // Refresh the list
            }
            else
            {
                MessageBox.Show("Cannot delete this call.");
            }
        }
    }
}
