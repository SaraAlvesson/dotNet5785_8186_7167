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

        // רשימות לסוגי קריאות ואופציות מיון
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

            // קישור טקסט-בוקס ל-Location במודל
            textrr.SetBinding(TextBox.TextProperty, new Binding("CurrentVolunteer.Location")
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            LoadCalls();
        }

        private void LoadCalls()
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

                var calls = s_bl.Call.GetOpenCallInLists(
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




        private void ChooseCall(OpenCallInList selectedCall)
        {
            if (selectedCall != null)
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

        private void ApplyFilters()
        {
            BO.Enums.CallTypeEnum? callTypeFilter = Enum.TryParse(SelectedCallType, out BO.Enums.CallTypeEnum parsedCallType)
                ? parsedCallType
                : (BO.Enums.CallTypeEnum?)null;

            BO.Enums.OpenCallEnum? openCallFilter = Enum.TryParse(SelectedSortOption, out BO.Enums.OpenCallEnum parsedOpenCall)
                ? parsedOpenCall
                : (BO.Enums.OpenCallEnum?)null;

            var filteredCalls = s_bl.Call.GetOpenCallInLists(CurrentVolunteer.Id, callTypeFilter, openCallFilter);
            Calls.Clear();
            foreach (var call in filteredCalls)
            {
                Calls.Add(call);
            }
        }

        public ObservableCollection<OpenCallInList> Calls { get; } = new ObservableCollection<OpenCallInList>();

        private void CallsDataGrid_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var selectedCall = (OpenCallInList)CallsDataGrid.SelectedItem;
            if (selectedCall != null)
            {
                SelectedCallDetails = selectedCall.VerbDesc; // עדכון התיאור המילולי
            }
            else
            {
                SelectedCallDetails = "No call selected"; // הודעת ברירת מחדל כאשר אין קריאה נבחרת
            }
        }

        private void update_click(object sender, RoutedEventArgs e)
        {
            try
            {
                // עדכון הכתובת לפי הערך מה-TextBox
                CurrentVolunteer.Location = textrr.Text;

                // עדכון במערכת
                s_bl.Volunteer.UpdateVolunteerDetails(CurrentVolunteer.Id, CurrentVolunteer);

                // עדכון ה-PropertyChanged כדי להפעיל את ה-Binding
                OnPropertyChanged(nameof(CurrentVolunteer.Location));

                // ריענון הקריאות לאחר עדכון הכתובת
                LoadCalls();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating address: {ex.Message}");
            }
        }

      
        private void textrr_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}
