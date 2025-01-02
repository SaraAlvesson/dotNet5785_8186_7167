using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BO;
using static BO.Enums;

namespace PL.Admin
{
    /// <summary>
    /// Interaction logic for VolunteerListWindow.xaml
    /// </summary>
    public partial class VolunteerListWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        public VolunteerInListField Field { get; set; } = VolunteerInListField.None;

        public VolunteerListWindow()
        {
            InitializeComponent();
            LoadVolunteerList();
        }

        // הגדרת תכונת תלות
        public IEnumerable<BO.VolunteerInList> VolunteerList
        {
            get { return (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerListProperty); }
            set { SetValue(VolunteerListProperty, value); }
        }

        // קריאה לשכבת ה-BL כדי להוריד את הרשימה
        private void LoadVolunteerList()
        {
            // קריאה לשכבת ה-BL כדי להוריד את הרשימה
            var volunteers = s_bl.Volunteer.RequestVolunteerList;  // ודא שזה השם הנכון של הפונקציה בשכבת ה-BL
            SetValue(VolunteerListProperty, volunteers);  // עדכון תכונת התלות
        }
        public void OnFieldChanged(object sender, SelectionChangedEventArgs e)
        {
            // עדכון הערך של selectedVolunteerField בהתבסס על הבחירה ב-ComboBox
            var selectedVolunteerField = (BO.Enums.VolunteerInListField)((ComboBox)sender).SelectedItem;

            // ביצוע שאילתא מחדש על בסיס הבחירה
            if (selectedVolunteerField == BO.Enums.VolunteerInListField.None)
            {
                // אם לא נבחר שדה מסוים (None), מחזירים את כל המתנדבים ללא סינון
                VolunteerList = s_bl?.Volunteer.RequestVolunteerList(null);
            }
            else
            {
                // אם נבחר שדה, מבצעים את הסינון לפי השדה שנבחר
                VolunteerList = s_bl?.Volunteer.RequestVolunteerList(null, selectedVolunteerField);
            }
        }



        // הגדרת תכונת תלות עבור רשימת המתנדבים
        public static readonly DependencyProperty VolunteerListProperty =
            DependencyProperty.Register("VolunteerList", typeof(IEnumerable<BO.VolunteerInList>), typeof(VolunteerListWindow), new PropertyMetadata(null));
    }
}