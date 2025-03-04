using BO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PL.Volunteer
{
    public partial class ChooseCallWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private BO.Volunteer CurrentVolunteer;

        // דגלים למניעת עדכונים כפולים
        private volatile bool _isUpdatingCalls = false;
        private volatile bool _isUpdatingLocation = false;

        public List<string> CallTypes { get; } = Enum.GetNames(typeof(BO.Enums.CallTypeEnum)).ToList();
        public List<string> SortOptions { get; } = Enum.GetNames(typeof(BO.Enums.OpenCallEnum)).ToList();

        private string _selectedTypeFilter;
        public string SelectedTypeFilter
        {
            get => _selectedTypeFilter;
            set
            {
                _selectedTypeFilter = value;
                OnPropertyChanged();
                LoadCalls(); // ריענון הקריאות לאחר שינוי הפילטר
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl?.Call.AddObserver(ObserveCallListChanges);
            ObserveCallListChanges();
        }

        private string _addressFilter;
        public string AddressFilter
        {
            get => _addressFilter;
            set
            {
                _addressFilter = value;
                OnPropertyChanged();
                LoadCalls(); // ריענון הקריאות לאחר שינוי הפילטר
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

        public ChooseCallWindow(BO.Volunteer volunteer)
        {
            if (volunteer == null)
            {
                MessageBox.Show("Volunteer is null. Please ensure valid data is passed.");
                return;
            }

            InitializeComponent();
            DataContext = this; // קישור ה-DataContext

            CurrentVolunteer = volunteer;

            // התחברות לאירועים של סיום וביטול קריאה מ- MainVolunteerWindow
            var mainVolunteerWindow = Application.Current.Windows.OfType<MainVolunteerWindow>().FirstOrDefault();
            if (mainVolunteerWindow != null)
            {
                mainVolunteerWindow.CallCompleted += (s, e) => LoadCalls(); // טוען קריאות מחדש
                mainVolunteerWindow.CallCancelled += (s, e) => LoadCalls(); // טוען קריאות מחדש
            }

            textrr.SetBinding(TextBox.TextProperty, new Binding("CurrentVolunteer.Location")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            LoadCalls(); // טוען את הקריאות הראשוניות
        }

        private async void LoadCalls()
        {
            if (_isUpdatingCalls) return;
            _isUpdatingCalls = true;

            try
            {
                BO.Enums.CallTypeEnum? callTypeEnum = null;
                if (!string.IsNullOrEmpty(SelectedTypeFilter) && SelectedTypeFilter != "None" &&
                    Enum.TryParse<BO.Enums.CallTypeEnum>(SelectedTypeFilter, out var tempCallType))
                {
                    callTypeEnum = tempCallType;
                }

                BO.Enums.OpenCallEnum? openCallEnum = null;
                if (!string.IsNullOrEmpty(AddressFilter) && AddressFilter != "None" &&
                    Enum.TryParse<BO.Enums.OpenCallEnum>(AddressFilter, out var tempOpenCall))
                {
                    openCallEnum = tempOpenCall;
                }

                var calls = await s_bl.Call.GetOpenCallInListsAsync(CurrentVolunteer.Id, callTypeEnum, openCallEnum);

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
            finally
            {
                _isUpdatingCalls = false;
            }
        }

        private async Task ApplyFilters()
        {
            BO.Enums.CallTypeEnum? callTypeFilter = string.IsNullOrEmpty(SelectedCallType) || SelectedCallType == "None"
                ? (BO.Enums.CallTypeEnum?)null
                : Enum.TryParse(SelectedCallType, out BO.Enums.CallTypeEnum parsedCallType)
                    ? parsedCallType
                    : (BO.Enums.CallTypeEnum?)null;

            BO.Enums.OpenCallEnum? openCallFilter = string.IsNullOrEmpty(SelectedSortOption) || SelectedSortOption == "None"
                ? (BO.Enums.OpenCallEnum?)null
                : Enum.TryParse(SelectedSortOption, out BO.Enums.OpenCallEnum parsedOpenCall)
                    ? parsedOpenCall
                    : (BO.Enums.OpenCallEnum?)null;

            var filteredCalls = await s_bl.Call.GetOpenCallInListsAsync(CurrentVolunteer.Id, callTypeFilter, openCallFilter);
            Calls.Clear();
            foreach (var call in filteredCalls)
            {
                Calls.Add(call);
            }
        }


        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is OpenCallInList selectedCall)
            {
                ChooseCall(selectedCall);
            }
            Close();
        }

        private void ChooseCall(OpenCallInList selectedCall)
        {
            if (selectedCall != null)
            {
                try
                {
                    if (CurrentVolunteer == null)
                    {
                        MessageBox.Show("Error: No volunteer is currently selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    s_bl.Call.AssignCallToVolunteer(CurrentVolunteer.Id, selectedCall.Id);
                    MessageBox.Show($"You have selected Call ID {selectedCall.Id} for handling.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadCalls();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Error: No call selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void FilterAddressTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadCalls();
        }

        private void CallTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private string _selectedCallType;
        public string SelectedCallType
        {
            get => _selectedCallType;
            set
            {
                _selectedCallType = value;
                OnPropertyChanged();
                ApplyFilters();
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
                ApplyFilters();
            }
        }

        public ObservableCollection<OpenCallInList> Calls { get; } = new ObservableCollection<OpenCallInList>();

        private void CallsDataGrid_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var selectedCall = (OpenCallInList)CallsDataGrid.SelectedItem;
            if (selectedCall != null)
            {
                SelectedCallDetails = selectedCall.VerbDesc;
            }
            else
            {
                SelectedCallDetails = "No call selected";
            }
        }

        private void update_click(object sender, RoutedEventArgs e)
        {
            if (_isUpdatingLocation) return; // אם כבר מתבצע עדכון, לא נבצע עדכון נוסף
            _isUpdatingLocation = true;

            try
            {
                CurrentVolunteer.Location = textrr.Text;

                s_bl.Volunteer.UpdateVolunteerDetails(CurrentVolunteer.Id, CurrentVolunteer);
                OnPropertyChanged(nameof(CurrentVolunteer.Location));

                LoadCalls();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating address: {ex.Message}");
            }
            finally
            {
                _isUpdatingLocation = false;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Call.RemoveObserver(ObserveCallListChanges);
        }

        private void ObserveCallListChanges()
        {
            if (_isUpdatingCalls) return; // אם כבר מתבצע עדכון, לא נבצע עדכון נוסף
            _isUpdatingCalls = true;

            // עדכון התצוגה באמצעות Dispatcher
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadCalls();
            }));

            _isUpdatingCalls = false;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textrr_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
