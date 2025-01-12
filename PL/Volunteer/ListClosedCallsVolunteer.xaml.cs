using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PL.Volunteer
{
    /// <summary>
    /// Interaction logic for ListClosedCallsVolunteer.xaml
    /// </summary>
    public partial class ListClosedCallsVolunteer : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get(); // גישה ל-BL
      
        public List<BO.ClosedCallInList> ClosedCalls { get; set; } // רשימת הקריאות הסגורות

        public string SelectedCallType { get; set; }
        public string SelectedSortOption { get; set; }
        private readonly int _volunteerId; // מזהה המתנדב

        public ListClosedCallsVolunteer(int volunteerId)
        {
            InitializeComponent();

            _volunteerId = volunteerId;
 
            // טעינת רשימת הקריאות ממקור נתונים
            ClosedCalls = LoadClosedCalls();
           
        }

        private List<BO.ClosedCallInList> LoadClosedCalls()
        {
            // שימוש ב-BL לטעינת קריאות סגורות
            return s_bl.Call.GetVolunteerClosedCalls(_volunteerId, null, null)?.ToList() ?? new List<BO.ClosedCallInList>();
        }

        private void CallTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // סינון הקריאות לפי סוג הקריאה
            BO.Enums.CallTypeEnum? filter = Enum.TryParse(SelectedCallType, out BO.Enums.CallTypeEnum parsedFilter)
                ? parsedFilter
                : (BO.Enums.CallTypeEnum?)null;

            ClosedCallsDataGrid.ItemsSource = s_bl.Call.GetVolunteerClosedCalls(_volunteerId, filter, null)?.ToList();
        }

        private void SortByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // מיון הקריאות לפי השדה שנבחר
            BO.Enums.ClosedCallFieldEnum? sortField = Enum.TryParse(SelectedSortOption, out BO.Enums.ClosedCallFieldEnum parsedSortField)
                ? parsedSortField
                : (BO.Enums.ClosedCallFieldEnum?)null;

            ClosedCallsDataGrid.ItemsSource = s_bl.Call.GetVolunteerClosedCalls(_volunteerId, null, sortField)?.ToList();
        }
    }
}
