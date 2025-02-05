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

namespace PL.Admin
{
    public partial class VolunteerListWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        #region Filtering and Sorting Properties

        // מקור נתונים עבור סינון לפי סוג קריאה (CallTypeEnum)
        public IEnumerable<CallTypeEnum> CallTypeOptions => Enum.GetValues(typeof(CallTypeEnum)).Cast<CallTypeEnum>();

        private CallTypeEnum _selectedCallType;
        public CallTypeEnum SelectedCallType
        {
            get => _selectedCallType;
            set
            {
                if (_selectedCallType != value)
                {
                    _selectedCallType = value;
                    OnPropertyChanged(nameof(SelectedCallType));
                    ApplyFilters();
                }
            }
        }

        // מקור נתונים עבור מיון לפי שדה (VolunteerInListField)
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
                    ApplyFilters();
                }
            }
        }

        // מקור נתונים עבור סינון לפי סטטוס פעיל/לא פעיל (All, Active, Inactive)
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
                    ApplyFilters();
                }
            }
        }

        #endregion

        // הבחירה של המתנדב הנבחר
        public VolunteerInList? SelectedVolunteer { get; set; }

        // הרשימה המוצגת של מתנדבים
        private ObservableCollection<VolunteerInList> _volunteers = new();
        public ObservableCollection<VolunteerInList> Volunteers
        {
            get => _volunteers;
            set
            {
                _volunteers = value;
                OnPropertyChanged(nameof(Volunteers));
            }
        }

        public VolunteerListWindow()
        {
            InitializeComponent();
            DataContext = this; // DataContext יהיה החלון הזה עצמו

            // אתחול ערכי ברירת מחדל
            SelectedCallType = CallTypeEnum.None;
            SelectedActiveFilter = "All";
            SelectedSortOption = VolunteerInListField.None;

            LoadVolunteerList(); // טעינת הרשימה הראשונית
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl?.Volunteer.AddObserver(ObserveVolunteerListChanges); // נרשמים למשקיף
            ObserveVolunteerListChanges(); // מבצע את הקריאה כדי להוריד את הרשימה המעודכנת
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Volunteer.RemoveObserver(ObserveVolunteerListChanges); // מסירים את המשקיף
        }

        /// <summary>
        /// פונקציית סינון ומיון שמתעדכנת בכל שינוי בהגדרות.
        /// </summary>
        private void ApplyFilters()
        {
            // --- סינון לפי סוג קריאה ---
            // אם נבחר ערך שונה מ־None, נשלח אותו, אחרת null (כל הערכים)
            CallTypeEnum? callTypeFilter = SelectedCallType != CallTypeEnum.None ? SelectedCallType : (CallTypeEnum?)null;

            // --- סינון לפי סטטוס פעיל/לא פעיל ---
            bool? isActiveFilter = null;
            if (!string.IsNullOrEmpty(SelectedActiveFilter))
            {
                if (SelectedActiveFilter.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    isActiveFilter = true;
                else if (SelectedActiveFilter.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                    isActiveFilter = false;
                // "All" או ערך אחר → נשאר null (כל המתנדבים)
            }

            // --- קריאה לרשימת המתנדבים עם הסינונים מה-BL ---
            // החתימה: RequestVolunteerList(bool? isActive, VolunteerInListField? sortField = null, CallTypeEnum? callTypeFilter = null)
            // עבור המיון בצד השרת נשלח null ונבצע את המיון כאן
            var filteredVolunteers = s_bl?.Volunteer.RequestVolunteerList(isActiveFilter, null, callTypeFilter)
                                      ?? new List<VolunteerInList>();

            // --- מיון לפי שדה (בצד לקוח) ---
            if (SelectedSortOption != VolunteerInListField.None)
            {
                switch (SelectedSortOption)
                {
                    case VolunteerInListField.Id:
                        filteredVolunteers = filteredVolunteers.OrderBy(v => v.Id).ToList();
                        break;
                    case VolunteerInListField.FullName:
                        filteredVolunteers = filteredVolunteers.OrderBy(v => v.FullName).ToList();
                        break;
                    case VolunteerInListField.Active:
                        filteredVolunteers = filteredVolunteers.OrderBy(v => v.Active).ToList();
                        break;
                    case VolunteerInListField.SumTreatedCalls:
                        filteredVolunteers = filteredVolunteers.OrderBy(v => v.SumTreatedCalls).ToList();
                        break;
                    case VolunteerInListField.SumCanceledCalls:
                        filteredVolunteers = filteredVolunteers.OrderBy(v => v.SumCanceledCalls).ToList();
                        break;
                    case VolunteerInListField.SumExpiredCalls:
                        filteredVolunteers = filteredVolunteers.OrderBy(v => v.SumExpiredCalls).ToList();
                        break;
                    case VolunteerInListField.CallIdInTreatment:
                        filteredVolunteers = filteredVolunteers.OrderBy(v => v.CallIdInTreatment).ToList();
                        break;
                    //case VolunteerInListField.CallType:
                    //    // שימוש ב-ToString למקרה שהמיון לפי enum צריך להיות לפי שמו
                    //    filteredVolunteers = filteredVolunteers.OrderBy(v => v.CallType.ToString()).ToList();
                    //    break;
                    default:
                        break;
                }
            }

            // --- עדכון הרשימה ---
            Volunteers = new ObservableCollection<VolunteerInList>(filteredVolunteers);
            // חשוב לעדכן גם את ה-PropertyChanged, מה שמבטיח את רענון ה-UI
            OnPropertyChanged(nameof(Volunteers));
        }

        /// <summary>
        /// טוען את רשימת המתנדבים הראשונית (ללא סינונים)
        /// </summary>
        private void LoadVolunteerList()
        {
            try
            {
                Volunteers.Clear(); // ניקוי הרשימה לפני הוספה חדשה

                var volunteerList = s_bl?.Volunteer.RequestVolunteerList(null) ?? new List<VolunteerInList>();

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

        /// <summary>
        /// Observer שמשדר עדכון כאשר רשימת המתנדבים משתנה.
        /// </summary>
        private void ObserveVolunteerListChanges()
        {
            LoadVolunteerList(); // טוען את הרשימה מחדש
        }

        #region פעולות UI

        // לחצן מחיקה
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

        /// <summary>
        /// מבצע מחיקת מתנדב ועדכון הרשימה.
        /// </summary>
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

        // לחצן הוספה
      private void ButtonAdd_Click(object sender, RoutedEventArgs e)
{

        new SingleVolunteerWindow().Show();
    }
    


        // לחיצה כפולה על פריט ברשימה לצפייה בפרטים
        private void lsvVolunteerList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedVolunteer != null)
                new SingleVolunteerWindow(SelectedVolunteer.Id).Show();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // אין צורך בפעולה מיוחדת כאן
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
