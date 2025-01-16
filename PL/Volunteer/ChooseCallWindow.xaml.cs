using BO;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;

namespace PL.Volunteer
{
    public partial class ChooseCallWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private BO.Volunteer CurrentVolunteer;

        public ObservableCollection<OpenCallInList> Calls { get; set; } = new ObservableCollection<OpenCallInList>();

        private string _selectedTypeFilter;
        public string SelectedTypeFilter
        {
            get => _selectedTypeFilter;
            set
            {
                _selectedTypeFilter = value;
                OnPropertyChanged();
                LoadCallsAsync();
            }
        }

        private string _addressFilter;
        public string AddressFilter
        {
            get => _addressFilter;
            set
            {
                _addressFilter = value;
                OnPropertyChanged();
                LoadCallsAsync();
            }
        }

        private string _selectedSortOption;
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                _selectedSortOption = value;
                OnPropertyChanged();
                LoadCallsAsync();
            }
        }

        private string _newAddress;
        public string NewAddress
        {
            get => _newAddress;
            set
            {
                _newAddress = value;
                OnPropertyChanged();
            }
        }

        private string _selectedCallDetails;
        public string SelectedCallDetails
        {
            get => _selectedCallDetails;
            set
            {
                _selectedCallDetails = value;
                OnPropertyChanged();
            }
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

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadCallsAsync();
        }

        private async Task LoadCallsAsync()
        {
            try
            {
                BO.Enums.CallTypeEnum? callTypeEnum = null;
                if (!string.IsNullOrEmpty(SelectedTypeFilter) && Enum.TryParse<BO.Enums.CallTypeEnum>(SelectedTypeFilter, out var tempCallType))
                {
                    callTypeEnum = tempCallType;
                }

                BO.Enums.OpenCallEnum? openCallEnum = null;
                if (!string.IsNullOrEmpty(AddressFilter) && Enum.TryParse<BO.Enums.OpenCallEnum>(AddressFilter, out var tempOpenCall))
                {
                    openCallEnum = tempOpenCall;
                }

                var calls = await s_bl.Call.GetVolunteerOpenCallsAsync(
                    CurrentVolunteer.Id,
                    callTypeEnum,
                    openCallEnum
                );

                Calls.Clear();
                foreach (var call in calls)
                {
                    Calls.Add(call);
                }
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
                    LoadCallsAsync();
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
                    LoadCallsAsync();
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

        private void FilterAddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadCallsAsync();
        }

        private void CallsDataGrid_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;

            if (dataGrid != null && dataGrid.SelectedItem is OpenCallInList selectedCall)
            {
                SelectedCallDetails = selectedCall.VerbDesc;
            }
        }

        private void CallDetailsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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
