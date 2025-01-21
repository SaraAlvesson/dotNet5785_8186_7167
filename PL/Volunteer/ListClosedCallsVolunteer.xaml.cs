using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace PL.Volunteer
{
    public partial class ListClosedCallsVolunteer : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        private List<BO.ClosedCallInList> _closedCalls;
        public List<BO.ClosedCallInList> ClosedCalls
        {
            get => _closedCalls;
            set
            {
                _closedCalls = value;
                OnPropertyChanged();
            }
        }

        public List<string> CallTypes { get; } = Enum.GetNames(typeof(BO.Enums.CallTypeEnum)).ToList();
        public List<string> SortOptions { get; } = Enum.GetNames(typeof(BO.Enums.ClosedCallFieldEnum)).ToList();

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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl?.Volunteer.AddObserver(ObserveCallsListChanges);  // נרשמים למשקיף
            ObserveCallsListChanges();  // מבצע את הקריאה כדי להוריד את הרשימה המעודכנת
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Volunteer.RemoveObserver(ObserveCallsListChanges);  // מסירים את המשקיף
        }

        private void ObserveCallsListChanges()
        {
            LoadClosedCalls();  // טוען את הרשימה מחדש
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

        private readonly int _volunteerId;

        public ListClosedCallsVolunteer(int volunteerId)
        {
            InitializeComponent();

            _volunteerId = volunteerId;
            ClosedCalls = LoadClosedCalls();
        }

        private List<BO.ClosedCallInList> LoadClosedCalls()
        {
            return s_bl.Call.GetVolunteerClosedCalls(_volunteerId, null, null)?.ToList() ?? new List<BO.ClosedCallInList>();
        }

        private void ApplyFilters()
        {
            BO.Enums.CallTypeEnum? callTypeFilter = Enum.TryParse(SelectedCallType, out BO.Enums.CallTypeEnum parsedCallType)
                ? parsedCallType
                : (BO.Enums.CallTypeEnum?)null;

            BO.Enums.ClosedCallFieldEnum? sortField = Enum.TryParse(SelectedSortOption, out BO.Enums.ClosedCallFieldEnum parsedSortField)
                ? parsedSortField
                : (BO.Enums.ClosedCallFieldEnum?)null;

            ClosedCalls = s_bl.Call.GetVolunteerClosedCalls(_volunteerId, callTypeFilter, sortField)?.ToList();
        }

        private void CallTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
