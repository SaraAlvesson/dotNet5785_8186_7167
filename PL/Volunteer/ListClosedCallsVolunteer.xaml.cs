using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading; // הוספת using עבור DispatcherTimer

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

        // דגלים למניעת עדכונים כפולים
        private volatile bool _isUpdatingCallList = false;

        private DispatcherTimer _simulatorUpdateTimer;
        private bool _isSimulatorRunning = false;

        public ListClosedCallsVolunteer(int volunteerId)
        {
            InitializeComponent();

            _volunteerId = volunteerId;
            LoadClosedCalls();
        }

        private void LoadClosedCalls()
        {
            ClosedCalls = s_bl.Call.GetVolunteerClosedCalls(_volunteerId, null, null)?.ToList() ?? new List<BO.ClosedCallInList>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl?.Volunteer.AddObserver(ObserveCallsListChanges);
            ObserveCallsListChanges();

            // הגדרת טיימר לעדכון בזמן הסימולטור
            _simulatorUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _simulatorUpdateTimer.Tick += SimulatorUpdateTimer_Tick;
            _simulatorUpdateTimer.Start();
        }

        private void SimulatorUpdateTimer_Tick(object sender, EventArgs e)
        {
            // בדיקה אם הסימולטור פועל
            bool currentSimulatorState = Application.Current.MainWindow is MainWindow mainWindow && mainWindow.IsSimulatorRunning;

            if (currentSimulatorState != _isSimulatorRunning)
            {
                _isSimulatorRunning = currentSimulatorState;

                if (_isSimulatorRunning)
                {
                    // הפעלת עדכון מיידי כאשר הסימולטור מתחיל
                    ObserveCallsListChanges();
                }
            }

            // עדכון רק אם הסימולטור פועל
            if (_isSimulatorRunning)
            {
                ObserveCallsListChanges();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Volunteer.RemoveObserver(ObserveCallsListChanges);

            // עצירת הטיימר
            _simulatorUpdateTimer?.Stop();
            _simulatorUpdateTimer = null;
        }

        private void ObserveCallsListChanges()
        {
            // מניעת עדכונים כפולים
            if (_isUpdatingCallList)
                return;

            _isUpdatingCallList = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    // קבלת רשימת הקריאות הסגורות העדכנית
                    var updatedClosedCalls = s_bl.Call.GetVolunteerClosedCalls(_volunteerId, null, null)?.ToList()
                                             ?? new List<BO.ClosedCallInList>();

                    // עדכון הרשימה רק אם יש שינוי
                    if (!AreListsEqual(ClosedCalls, updatedClosedCalls))
                    {
                        ClosedCalls = updatedClosedCalls;
                    }
                }
                catch (Exception ex)
                {
                    // טיפול בשגיאות אפשריות
                    MessageBox.Show($"שגיאה בעדכון רשימת קריאות סגורות: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    _isUpdatingCallList = false;
                }
            }));
        }

        // השוואת רשימות קריאות סגורות
        private bool AreListsEqual(List<BO.ClosedCallInList> list1, List<BO.ClosedCallInList> list2)
        {
            if (list1 == null && list2 == null) return true;
            if (list1 == null || list2 == null) return false;
            if (list1.Count != list2.Count) return false;

            return list1.SequenceEqual(list2, new ClosedCallInListComparer());
        }

        // מחלקת השוואה מותאמת לקריאות סגורות
        private class ClosedCallInListComparer : IEqualityComparer<BO.ClosedCallInList>
        {
            public bool Equals(BO.ClosedCallInList x, BO.ClosedCallInList y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;

                return x.Id == y.Id &&
                       x.CallType == y.CallType &&
                       x.Address == y.Address &&
                       x.OpenTime == y.OpenTime &&
                       x.TreatmentStartTime == y.TreatmentStartTime &&
                       x.RealFinishTime == y.RealFinishTime;
            }

            public int GetHashCode(BO.ClosedCallInList obj)
            {
                return HashCode.Combine(obj.Id, obj.CallType, obj.Address, obj.OpenTime, obj.TreatmentStartTime, obj.RealFinishTime);
            }
        }

        private void ApplyFilters()
        {
            BO.Enums.CallTypeEnum? callTypeFilter = null;

            if (!string.IsNullOrEmpty(SelectedCallType) && SelectedCallType != "None")
            {
                if (Enum.TryParse(SelectedCallType, out BO.Enums.CallTypeEnum parsedCallType))
                {
                    callTypeFilter = parsedCallType;
                }
            }

            BO.Enums.ClosedCallFieldEnum? sortField = null;

            if (!string.IsNullOrEmpty(SelectedSortOption) && SelectedSortOption != "None")
            {
                if (Enum.TryParse(SelectedSortOption, out BO.Enums.ClosedCallFieldEnum parsedSortField))
                {
                    sortField = parsedSortField;
                }
            }

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

        private void ClosedCallsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
