using System;
using System.Windows;

namespace PL.Admin
{
    /// <summary>
    /// Interaction logic for SingleVolunteerWindow.xaml
    /// </summary>
    public partial class SingleVolunteerWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        // תכונה (Property) שמייצגת את הטקסט שעל הכפתור
        public string ButtonText { get; set; }

        // קונסטרוקטור של החלון
        public SingleVolunteerWindow(int id=0)
        {

            // קביעת הטקסט של הכפתור לפי ה-id
            ButtonText = id == 0 ? "Add" : "Update";
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
            InitializeComponent();

        }

        // פעולה שנקראת כאשר הכפתור נלחץ
        private void btnAddUpdate(object sender, RoutedEventArgs e)
        {
            // פעולה להוספה או עדכון של הישות
        }
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
    }
}