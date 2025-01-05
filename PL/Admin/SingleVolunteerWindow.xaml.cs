using System;
using System.Windows;
using static BO.Enums;

namespace PL.Admin
{
    /// <summary>
    /// Interaction logic for SingleVolunteerWindow.xaml
    /// </summary>
    public partial class SingleVolunteerWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private VolunteerInListField _selectedVolunteerField = VolunteerInListField.None;
        // תכונה (Property) שמייצגת את הטקסט שעל הכפתור
        public string ButtonText { get; set; }

        // קונסטרוקטור של החלון
        public SingleVolunteerWindow(int id=0)
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl?.Volunteer.AddObserver(ObserveVolunteerListChanges);  // נרשמים למשקיף
            ObserveVolunteerListChanges();  // מבצע את הקריאה כדי להוריד את הרשימה המעודכנת
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Volunteer.RemoveObserver(ObserveVolunteerListChanges);  // מסירים את המשקיף
        }
        private void ObserveVolunteerListChanges()
        {
           PL.Admin.VolunteerListWindow.UpdateVolunteerList(_selectedVolunteerField);
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

        }

        private void btnAddUpdate(object sender, RoutedEventArgs e)
        {
            if (ButtonText == "Add")
            {
                s_bl.Volunteer.AddVolunteer(CurrentVolunteer!);
                MessageBox.Show("Volunteer added succesfully");
                Window_Closed(SingleVolunteerWindow)
            }
            else
                s_bl.Volunteer.UpdateVolunteerDetails(CurrentVolunteer.Id, CurrentVolunteer);


        }
    }
}