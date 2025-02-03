using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BO;

namespace PL.Admin
{
    public partial class VolunteerListWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        public List<string> SelectedCallType { get; } = Enum.GetNames(typeof(BO.Enums.VolunteerInListField)).ToList();
        public BO.Enums.CallTypeEnum callType { get; set; } = BO.Enums.CallTypeEnum.None;

        public BO.VolunteerInList? SelectedVolunteer { get; set; }

        
        public VolunteerListWindow()
        {
            InitializeComponent();
            this.DataContext = this;  // DataContext יהיה חלון זה עצמו

            LoadVolunteerList(); // טוען את הרשימה הראשונית
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl?.Volunteer.AddObserver(ObserveVolunteerListChanges);  // נרשמים למשקיף
            ObserveVolunteerListChanges();  // מבצע את הקריאה כדי להוריד את הרשימה המעודכנת
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Volunteer.RemoveObserver(ObserveVolunteerListChanges);  // מסירים את המשקיף
        }

        private string _selectedCallType;
        public string SelectedFilter
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
        

        private void filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            BO.Enums.VolunteerInListField? volFilter = Enum.TryParse(SelectedFilter, out BO.Enums.VolunteerInListField parsedCallType)
                ? parsedCallType
                : (BO.Enums.VolunteerInListField?)null;

            BO.Enums.CallTypeEnum? sortField = Enum.TryParse(SelectedSortOption, out BO.Enums.CallTypeEnum parsedSortField)
                ? parsedSortField
                : (BO.Enums.CallTypeEnum?)null;

            // סינון הרשימה לפי הקריטריונים שנבחרו
            var filteredVolunteers = s_bl?.Volunteer.RequestVolunteerList(null) ?? new List<VolunteerInList>();

            if (volFilter.HasValue)
            {
                // סינון לפי שם מלא, אם volFilter הוא Enum
                filteredVolunteers = filteredVolunteers
                    .Where(v => v.FullName.Contains(volFilter.Value.ToString(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (sortField.HasValue)
            {
                // סינון לפי סוג הקריאה
                filteredVolunteers = filteredVolunteers.OrderBy(v => v.CallType).ToList();
            }

            // עדכון הרשימה
            Volunteers = new ObservableCollection<VolunteerInList>(filteredVolunteers);
        }

        // תכונת תלות לרשימת המתנדבים
        public ObservableCollection<VolunteerInList> Volunteers { get; set; } = new();

        // טעינת הרשימה
        private void LoadVolunteerList()
        {
            try
            {
                Volunteers.Clear();  // ניקוי הרשימה לפני הוספה חדשה

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

        private void ObserveVolunteerListChanges()
        {
            LoadVolunteerList();  // טוען את הרשימה מחדש
        }

        // מחיקת מתנדב
        private void DeleteVolunteer(int volunteerId)
        {
            try
            {
                var bl = BlApi.Factory.Get().Volunteer;
                bl.DeleteVolunteer(volunteerId); // מחיקת המתנדב
                LoadVolunteerList();  // עדכון הרשימה
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting the volunteer: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // לחצן מחיקה
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int volunteerId)
            {
                var result = MessageBox.Show("Are you sure you want to delete this volunteer?",
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DeleteVolunteer(volunteerId); // קריאה למחיקה
                }
            }
        }

        // לחצן הוספה
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            new SingleVolunteerWindow().Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Double-click on volunteer list to view details
        private void lsvVolunteerList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedVolunteer != null)
                new SingleVolunteerWindow(SelectedVolunteer.Id).Show();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
//using PL.privateVolunteer;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Input;

//namespace PL.Volunteer;

///// <summary>
///// Interaction logic for VolunteerListWindow.xaml
///// </summary>
//public partial class VolunteerListWindow : Window
//{
//    /// <summary>
//    /// Static instance of the business logic layer (BL).
//    /// </summary>
//    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

//    /// <summary>
//    /// Initializes a new instance of the <see cref="VolunteerListWindow"/> class.
//    /// </summary>
//    public VolunteerListWindow()
//    {
//        InitializeComponent();
//    }

//    /// <summary>
//    /// Dependency property for the VolunteerList, which is bound to the DataGrid in the XAML.
//    /// </summary>
//    public IEnumerable<BO.VolunteerInList> VolunteerList
//    {
//        get { return (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerListProperty); }
//        set { SetValue(VolunteerListProperty, value); }
//    }

//    /// <summary>
//    /// Registration of the dependency property for VolunteerList.
//    /// </summary>
//    public static readonly DependencyProperty VolunteerListProperty =
//        DependencyProperty.Register("VolunteerList", typeof(IEnumerable<BO.VolunteerInList>), typeof(VolunteerListWindow), new PropertyMetadata(null));

//    /// <summary>
//    /// Property to store the currently selected call type filter.
//    /// </summary>
//    public BO.CallType callType { get; set; } = BO.CallType.None;

//    /// <summary>
//    /// Event handler triggered when the call type ComboBox selection changes.
//    /// Updates the volunteer list based on the selected call type filter.
//    /// </summary>
//    private void CallTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
//    {
//        VolunteerList = helpReadAllVolunteer(callType);
//    }

//    /// <summary>
//    /// Refreshes the VolunteerList property with updated data.
//    /// </summary>
//    private void RefreshVolunteerList()
//    {
//        VolunteerList = helpReadAllVolunteer(callType);
//    }

//    /// <summary>
//    /// Helper method to fetch the list of volunteers based on the selected call type filter.
//    /// </summary>
//    /// <param name="callTypeHelp">The call type filter to apply.</param>
//    /// <returns>List of volunteers matching the filter.</returns>
//    private static IEnumerable<BO.VolunteerInList> helpReadAllVolunteer(BO.CallType callTypeHelp)
//    {
//        return (callTypeHelp == BO.CallType.None)
//            ? s_bl?.Volunteer.ReadAll(null, BO.FieldsVolunteerInList.Id, null)!
//            : s_bl?.Volunteer.ReadAll(null, BO.FieldsVolunteerInList.Id, callTypeHelp)!;
//    }

//    /// <summary>
//    /// Observer to refresh the volunteer list whenever there are updates.
//    /// </summary>
//    private void volunteerListObserver() => RefreshVolunteerList();

//    /// <summary>
//    /// Adds the observer when the window is loaded.
//    /// </summary>
//    private void Window_Loaded(object sender, RoutedEventArgs e)
//        => s_bl.Volunteer.AddObserver(volunteerListObserver);

//    /// <summary>
//    /// Removes the observer when the window is closed.
//    /// </summary>
//    private void Window_Closed(object sender, EventArgs e)
//        => s_bl.Volunteer.RemoveObserver(volunteerListObserver);

//    /// <summary>
//    /// Event handler for selection change in the DataGrid.
//    /// Placeholder for additional functionality if needed.
//    /// </summary>
//    private void DataGrid_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
//    {
//        // Placeholder
//    }

//    /// <summary>
//    /// Opens the VolunteerWindow to add a new volunteer when the Add button is clicked.
//    /// </summary>
//    private void btnAdd_Click(object sender, RoutedEventArgs e)
//    {
//        new VolunteerWindow().Show();
//    }

//    /// <summary>
//    /// Property to store the currently selected volunteer in the DataGrid.
//    /// </summary>
//    public BO.VolunteerInList? SelectedVolunteer { get; set; }

//    /// <summary>
//    /// Opens the VolunteerWindow for the selected volunteer when the user double-clicks a row in the DataGrid.
//    /// </summary>
//    private void lsvVolunteersList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
//    {
//        if (SelectedVolunteer != null)
//            new VolunteerWindow(SelectedVolunteer.Id).Show();
//    }

//    /// <summary>
//    /// Deletes the selected volunteer when the Delete button is clicked.
//    /// Shows a confirmation dialog before performing the delete operation.
//    /// </summary>
//    private void btnDelete_Click(object sender, RoutedEventArgs e)
//    {
//        if (sender is Button button && button.Tag is int volunteerId)
//        {
//            var result = MessageBox.Show(
//                "Are you sure you want to delete the volunteer?",
//                "Confirmation",
//                MessageBoxButton.YesNo,
//                MessageBoxImage.Question);

//            if (result != MessageBoxResult.Yes)
//            {
//                return;
//            }

//            try
//            {
//                // Close all related windows before deleting the volunteer
//                var windowsToClose = Application.Current.Windows
//                 .OfType<Window>()
//                 .Where(w =>
//                     w is MainVolunteerWindow mvw && mvw.CurrentVolunteer.Id == volunteerId ||
//                     w is SelectCallWindow scw && scw.CurrentVolunteer.Id == volunteerId ||
//                     w is CallHistoryWindow chw && chw.CurrentVolunteer.Id == volunteerId ||
//                     w is VolunteerWindow vw && vw.CurrentVolunteer.Id == volunteerId).ToList();

//                foreach (var window in windowsToClose)
//                {
//                    window.Close();
//                }

//                // Delete the volunteer from the database
//                s_bl.Volunteer.Delete(volunteerId);

//                MessageBox.Show("Volunteer deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//            }
//        }
//    }
//}