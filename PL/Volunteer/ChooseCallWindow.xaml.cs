using BO;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL.Volunteer
{
    public partial class ChooseCallWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private BO.Volunteer CurrentVolunteer;

        public ObservableCollection<OpenCallInList> Calls { get; set; } = new();
        private string _selectedTypeFilter;
        public string SelectedTypeFilter
        {
            get => _selectedTypeFilter;
            set { _selectedTypeFilter = value; OnPropertyChanged(); LoadCalls(); }
        }

        private string _addressFilter;
        public string AddressFilter
        {
            get => _addressFilter;
            set { _addressFilter = value; OnPropertyChanged(); LoadCalls(); }
        }

        private string _selectedSortOption;
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set { _selectedSortOption = value; OnPropertyChanged(); LoadCalls(); }
        }

        private string _newAddress;
        public string NewAddress
        {
            get => _newAddress;
            set { _newAddress = value; OnPropertyChanged(); }
        }

        private string _selectedCallDetails;
        public string SelectedCallDetails
        {
            get => _selectedCallDetails;
            set { _selectedCallDetails = value; OnPropertyChanged(); }
        }

        public ICommand ChooseCallCommand { get; }
        public ICommand UpdateAddressCommand { get; }

        public ChooseCallWindow(BO.Volunteer volunteer)
        {
            InitializeComponent();
            DataContext = this;

            CurrentVolunteer = volunteer;

            ChooseCallCommand = new RelayCommand(ChooseCall);
            UpdateAddressCommand = new RelayCommand(UpdateAddress);

            LoadCalls(); // טוען את הקריאות
        }

        private void LoadCalls()
        {
            try
            {
                var calls = s_bl.Call.GetVolunteerOpenCalls(
                    CurrentVolunteer.Id,
                    null,
                    null
                );

                Calls.Clear();
                foreach (var call in calls)
                    Calls.Add(call);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading calls: {ex.Message}");
            }
        }

        private void ChooseCall(object parameter)
        {
            if (parameter is OpenCallInList selectedCall)
            {
                try
                {
                    s_bl.Call.AssignCallToVolunteer(CurrentVolunteer.Id, selectedCall.Id);
                    MessageBox.Show($"You have selected Call ID {selectedCall.Id} for handling.");
                    LoadCalls();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error assigning call: {ex.Message}");
                }
            }
        }

        private void UpdateAddress(object parameter)
        {
            if (!string.IsNullOrEmpty(NewAddress))
            {
                try
                {
                    CurrentVolunteer.Location = NewAddress;
                    s_bl.Volunteer.UpdateVolunteerDetails(CurrentVolunteer.Id, CurrentVolunteer);
                    LoadCalls();
                    MessageBox.Show("Address updated successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating address: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid address.");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // מתודה שמטפלת בשינוי טקסט בשדה כתובת סינון
        private void FilterAddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // הוסף לוגיקה לסינון הקריאות לפי הכתובת שהוזנה
        }

        // מתודה שמטפלת בשינוי הבחירה ברשימת הקריאות
        private void CallsDataGrid_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            // קבלת ה-DataGrid מה- sender
            var dataGrid = sender as DataGrid;

            if (dataGrid != null && dataGrid.SelectedItem is OpenCallInList selectedCall)
            {
                // הצגת הפירוט המילולי של הקריאה
                SelectedCallDetails = selectedCall.VerbDesc;

                // עדכן את המפה עם מיקום הקריאה, אם יש לך את הנתונים הדרושים
            }
        }


        // מתודה שמטפלת בשינוי טקסט בפרטי הקריאה שנבחרה
        private void CallDetailsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // אפשר לעדכן כאן את פרטי הקריאה במידת הצורך
        }

        // מתודה לעדכון הכתובת
        private void UpdateAddress()
        {
            // עדכון כתובת המתנדב
        }

        public class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Predicate<object> _canExecute;

            public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

            public void Execute(object parameter) => _execute(parameter);

            public event EventHandler CanExecuteChanged;
            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
