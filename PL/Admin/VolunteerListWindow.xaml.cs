using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BO;
using static BO.Enums;
using System.Windows.Threading;

namespace PL.Admin
{
    public partial class VolunteerListWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        #region Filtering and Sorting Properties

        public IEnumerable<CallTypeEnum> CallTypeOptions => Enum.GetValues(typeof(CallTypeEnum)).Cast<CallTypeEnum>();

        private CallTypeEnum _selectedCallType;
        private bool _isUpdating = false; 
        private bool _isInitializing = false; 

        public CallTypeEnum SelectedCallType
        {
            get => _selectedCallType;
            set
            {
                if (_selectedCallType != value)
                {
                    _selectedCallType = value;
                    OnPropertyChanged(nameof(SelectedCallType));
                    if (!_isInitializing) ApplyFilters();
                }
            }
        }

        public IEnumerable<VolunteerInListField> SortOptions => Enum.GetValues(typeof(VolunteerInListField)).Cast<VolunteerInListField>();

        private VolunteerInListField _selectedSortOption;
        public VolunteerInListField SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (_selectedSortOption != value)
                {
                    _selectedSortOption = value;
                    OnPropertyChanged(nameof(SelectedSortOption));
                    if (!_isInitializing) ApplyFilters();
                }
            }
        }

        public List<string> ActiveFilterOptions { get; } = new List<string>
        {
            "All",
            "Active",
            "Inactive"
        };

        private string _selectedActiveFilter;
        public string SelectedActiveFilter
        {
            get => _selectedActiveFilter;
            set
            {
                if (_selectedActiveFilter != value)
                {
                    _selectedActiveFilter = value;
                    OnPropertyChanged(nameof(SelectedActiveFilter));
                    if (!_isInitializing) ApplyFilters();
                }
            }
        }

        #endregion

        public VolunteerInList? SelectedVolunteer { get; set; }

        private ObservableCollection<VolunteerInList> _volunteers = new();
        public ObservableCollection<VolunteerInList> Volunteers
        {
            get => _volunteers;
            set
            {
                if (_volunteers != value)
                {
                    _volunteers = value;
                    OnPropertyChanged(nameof(Volunteers)); 
                }
            }
        }

        public VolunteerListWindow()
        {
            // אתחול ערכי ברירת מחדל לפני יצירת הממשק
            _selectedCallType = CallTypeEnum.None;
            _selectedActiveFilter = "All";
            _selectedSortOption = VolunteerInListField.None;

            InitializeComponent();
            DataContext = this;
            LoadVolunteerList();
            StartAutoRefresh();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl?.Volunteer.AddObserver(ObserveVolunteerListChanges);
            ObserveVolunteerListChanges();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Volunteer.RemoveObserver(ObserveVolunteerListChanges);
        }

        private void ApplyFilters()
        {
            if (_isUpdating) 
                return;

            _isUpdating = true; 

            // הגדרת סינון לפי קריאה וסוג הפעילות
            CallTypeEnum? callTypeFilter = SelectedCallType != CallTypeEnum.None ? SelectedCallType : (CallTypeEnum?)null;
            bool? isActiveFilter = SelectedActiveFilter switch
            {
                "Active" => true,
                "Inactive" => false,
                _ => null
            };

            // יצירת פעולת סינון ומיון
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    // שימוש באופציה של סינון בלבד (בלי קריאות נוספות)
                    var filteredVolunteers = s_bl?.Volunteer.RequestVolunteerList(isActiveFilter, null, callTypeFilter)
                                                      ?? new List<VolunteerInList>();

                    // סינון לפי מיון אם יש
                    if (SelectedSortOption != VolunteerInListField.None)
                    {
                        filteredVolunteers = SelectedSortOption switch
                        {
                            VolunteerInListField.Id => filteredVolunteers.OrderBy(v => v.Id).ToList(),
                            VolunteerInListField.FullName => filteredVolunteers.OrderBy(v => v.FullName).ToList(),
                            VolunteerInListField.Active => filteredVolunteers.OrderBy(v => v.Active).ToList(),
                            VolunteerInListField.SumTreatedCalls => filteredVolunteers.OrderBy(v => v.SumTreatedCalls).ToList(),
                            VolunteerInListField.SumCanceledCalls => filteredVolunteers.OrderBy(v => v.SumCanceledCalls).ToList(),
                            VolunteerInListField.SumExpiredCalls => filteredVolunteers.OrderBy(v => v.SumExpiredCalls).ToList(),
                            VolunteerInListField.CallIdInTreatment => filteredVolunteers.OrderBy(v => v.CallIdInTreatment).ToList(),
                            _ => filteredVolunteers
                        };
                    }

                    // עדכון הרשימה שהוזנה עם הערכים המסוננים והממוינים
                    Volunteers.Clear();
                    foreach (var volunteer in filteredVolunteers)
                    {
                        Volunteers.Add(volunteer);
                    }
                    OnPropertyChanged(nameof(Volunteers));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error applying filters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    _isUpdating = false; 
                }
            }), DispatcherPriority.Background);
        }



        private void LoadVolunteerList()
        {
            try
            {
                // טעינת כל המתנדבים בלי סינון
                var volunteerList = s_bl?.Volunteer.RequestVolunteerList(null, null, null);
                Volunteers.Clear();
                foreach (var volunteer in volunteerList)
                {
                    Volunteers.Add(volunteer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading volunteer list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private DispatcherTimer _timer;

        private void StartAutoRefresh()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5) 
            };
            _timer.Tick += (s, e) => ObserveVolunteerListChanges();
            _timer.Start();
        }

        private void ObserveVolunteerListChanges()
        {
            if (_isUpdating) return;

            _isUpdating = true;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                try
                {
                    // שימוש בפילטרים הנוכחיים
                    CallTypeEnum? callTypeFilter = SelectedCallType == CallTypeEnum.None ? null : SelectedCallType;
                    bool? isActiveFilter = SelectedActiveFilter switch
                    {
                        "Active" => true,
                        "Inactive" => false,
                        _ => null
                    };

                    var updatedVolunteers = s_bl?.Volunteer.RequestVolunteerList(isActiveFilter, null, callTypeFilter) ?? new List<VolunteerInList>();

                    // שימוש במיון הנוכחי
                    if (SelectedSortOption != VolunteerInListField.None)
                    {
                        updatedVolunteers = SelectedSortOption switch
                        {
                            VolunteerInListField.Id => updatedVolunteers.OrderBy(v => v.Id).ToList(),
                            VolunteerInListField.FullName => updatedVolunteers.OrderBy(v => v.FullName).ToList(),
                            VolunteerInListField.Active => updatedVolunteers.OrderBy(v => v.Active).ToList(),
                            VolunteerInListField.SumTreatedCalls => updatedVolunteers.OrderBy(v => v.SumTreatedCalls).ToList(),
                            VolunteerInListField.SumCanceledCalls => updatedVolunteers.OrderBy(v => v.SumCanceledCalls).ToList(),
                            VolunteerInListField.SumExpiredCalls => updatedVolunteers.OrderBy(v => v.SumExpiredCalls).ToList(),
                            VolunteerInListField.CallIdInTreatment => updatedVolunteers.OrderBy(v => v.CallIdInTreatment).ToList(),
                            _ => updatedVolunteers
                        };
                    }

                    Volunteers.Clear();
                    foreach (var volunteer in updatedVolunteers)
                    {
                        Volunteers.Add(volunteer);
                    }
                    OnPropertyChanged(nameof(Volunteers));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error observing volunteer list changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    _isUpdating = false;
                }
            }));
        }

        #region UI Actions

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int volunteerId)
            {
                var result = MessageBox.Show("Are you sure you want to delete this volunteer?",
                                             "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    DeleteVolunteer(volunteerId);
                }
            }
        }

        private void DeleteVolunteer(int volunteerId)
        {
            try
            {
                s_bl?.Volunteer.DeleteVolunteer(volunteerId);
                LoadVolunteerList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting the volunteer: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new SingleVolunteerWindow().Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lsvVolunteerList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is VolunteerInList selectedVolunteer)
            {
                try
                {
                    var singleVolunteerWindow = new SingleVolunteerWindow(selectedVolunteer.Id);
                    singleVolunteerWindow.Owner = this; 
                    singleVolunteerWindow.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening volunteer details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // No action needed here
        }

        private void SortField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CallTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void IsActiveFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
