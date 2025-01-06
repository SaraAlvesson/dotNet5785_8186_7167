using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BO;
using static BO.Enums;

namespace PL.Admin
{
    /// <summary>
    /// Interaction logic for SingleVolunteerWindow.xaml
    /// </summary>
    public partial class SingleVolunteerWindow : Window, INotifyPropertyChanged

    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private VolunteerInListField _selectedVolunteerField = VolunteerInListField.None;
        // תכונה (Property) שמייצגת את הטקסט שעל הכפתור
        public string ButtonText { get; set; }

        public VolunteerInListField SelectedFilter
        {
            get { return _selectedVolunteerField; }
            set
            {
                if (_selectedVolunteerField != value)
                {
                    _selectedVolunteerField = value;
                    OnPropertyChanged(nameof(SelectedFilter));  // Notify the UI of the property change
                    UpdateVolunteerList();  // Update the list when the filter changes
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        // VolunteerInList property (DependencyProperty)
        public IEnumerable<BO.VolunteerInList> VolunteerInList
        {
            get { return (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerInListFieldListProperty); }
            set { SetValue(VolunteerInListFieldListProperty, value); }
        }

        public static readonly DependencyProperty VolunteerInListFieldListProperty =
           DependencyProperty.Register(
               "VolunteerInList",
               typeof(IEnumerable<BO.VolunteerInList>),
               typeof(SingleVolunteerWindow),
               new PropertyMetadata(null));



        // קונסטרוקטור של החלון
        public SingleVolunteerWindow(int id = 0)
        {

            // קביעת הטקסט של הכפתור לפי ה-id
            ButtonText = id == 0 ? "Add" : "Update";

            InitializeComponent();

            // חיבור התכונה למנגנון Binding, כדי שה-XAML יקבל את הערך שלה
            this.DataContext = this;
            try
            {
                if (id == 0)
                {
                    // אם מדובר בהוספה, יוצר אובייקט חדש עם ערכים ברירת מחדל
                    CurrentVolunteer = new BO.Volunteer(); // BO - namespace שלך
                }
                else
                {
                    // אם מדובר בעדכון, קורא ל- BL כדי להביא את הישות לפי ה-Id
                    CurrentVolunteer = s_bl.Volunteer.RequestVolunteerDetails(id); // YourBL הוא מחלקת ה- BL שלך
                }
            }
            catch (Exception ex)
            {
                // טיפול בחריגות (למשל, במקרה שה-Id לא נמצא או יש שגיאה בטעינה)
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }



        }

        private void UpdateVolunteerList()
        {
            try
            {
                IEnumerable<BO.VolunteerInList> volunteers = queryVolunteerList();
                VolunteerInList = volunteers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading the volunteer list: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BlApi.Factory.Get().Volunteer.AddObserver(volunteerListObserver);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            BlApi.Factory.Get().Volunteer.RemoveObserver(volunteerListObserver);
        }

        // Observer method to refresh volunteer list
        private void volunteerListObserver()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateVolunteerList();  // Refresh the volunteer list when notified of changes
            });
        }
        // Filtering logic based on the selected filter
        private IEnumerable<BO.VolunteerInList> queryVolunteerList()
        {
            IEnumerable<BO.VolunteerInList> volunteers;

            switch (SelectedFilter)
            {
                case VolunteerInListField.Id:
                    volunteers = BlApi.Factory.Get().Volunteer.RequestVolunteerList(null, VolunteerInListField.Id).OrderBy(v => v.Id);
                    break;
                case VolunteerInListField.FullName:
                    volunteers = BlApi.Factory.Get().Volunteer.RequestVolunteerList(null, VolunteerInListField.FullName).OrderBy(v => v.FullName);
                    break;
                case VolunteerInListField.Active:
                    volunteers = BlApi.Factory.Get().Volunteer.RequestVolunteerList(true, VolunteerInListField.Active).Where(v => v.Active);
                    break;
                case VolunteerInListField.None:  // No filter (default)
                    volunteers = BlApi.Factory.Get().Volunteer.RequestVolunteerList(null, null);
                    break;
                default:
                    volunteers = BlApi.Factory.Get().Volunteer.RequestVolunteerList(null, null);
                    break;
            }

            return volunteers;
        }

        // פעולה שנקראת כאשר הכפתור נלחץ

        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set { SetValue(CurrentVolunteerProperty, value); }
        }

        public static readonly DependencyProperty CurrentVolunteerProperty =
            DependencyProperty.Register("CurrentVolunteer", typeof(BO.Volunteer), typeof(SingleVolunteerWindow), new PropertyMetadata(null));

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Handle selection change and update the SelectedFilter property
            if (sender is ComboBox comboBox && comboBox.SelectedItem is VolunteerInListField selectedFilter)
            {
                SelectedFilter = selectedFilter; // Update the SelectedFilter property
            }
        }

        private void btnAddUpdate(object sender, RoutedEventArgs e)
        {
            if (ButtonText == "Add")
            {
                try
                {
                    s_bl.Volunteer.AddVolunteer(CurrentVolunteer!);
                    MessageBox.Show("Volunteer added succesfully");
                    this.Window_Closed(sender, e);
                }
                catch (Exception ex)
                { MessageBox.Show("$Error accured while adding new Volunteer"); }
            }
            else
            {
                try
                {
                    s_bl.Volunteer.UpdateVolunteerDetails(CurrentVolunteer.Id, CurrentVolunteer);
                    MessageBox.Show("Volunteer updated succesfully");
                    this.Window_Closed(sender, e);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error accured while updating Volunteer");
                }


            }


        }
    }
}
