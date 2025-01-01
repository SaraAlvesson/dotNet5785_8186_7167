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

namespace PL.Admin
{
    /// <summary>
    /// Interaction logic for VolunteerListWindow.xaml
    /// </summary>
    public partial class VolunteerListWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

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

        // הגדרת תכונת תלות עבור רשימת המתנדבים
        public static readonly DependencyProperty VolunteerListProperty =
            DependencyProperty.Register("VolunteerList", typeof(IEnumerable<BO.VolunteerInList>), typeof(VolunteerListWindow), new PropertyMetadata(null));
    }
}