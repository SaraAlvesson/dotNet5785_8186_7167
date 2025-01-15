using BO;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static BO.Enums;

namespace PL.Admin
{
    public partial class CallsListWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private CallTypeEnum _selectedCallType = CallTypeEnum.None;
        private CallFieldEnum _selectedCallField = CallFieldEnum.None;
       
        public BO.CallInList? SelectedCall { get; set; }

       
        public CallsListWindow()
        {
            InitializeComponent();
           
            LoadCallList();
        }

        // רשימת קריאות
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl?.Call.AddObserver(ObserveCallListChanges);
            ObserveCallListChanges();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Call.RemoveObserver(ObserveCallListChanges);
        }
        public IEnumerable<CallInList> callList
        {
            get => (IEnumerable<CallInList>)GetValue(CallListProperty);
            set
            {
                SetValue(CallListProperty, value ?? new List<CallInList>());  // טיפול במקרה של null
                OnPropertyChanged(nameof(callList));
            }
        }
        // רשימת קריאות מסוננות
        public ObservableCollection<CallInList> FilteredCalls { get; set; } = new();
        public ObservableCollection<CallInList> CallList { get; set; } = new();

        public CallFieldEnum SelectedCallField
        {
            get => _selectedCallField;
            set
            {
                if (_selectedCallField != value)
                {
                    _selectedCallField = value;
                    OnFieldChanged();
                }
            }
        }
        public void OnFieldChanged()
        {
            UpdateCallList(_selectedCallField);
        }


        public static readonly DependencyProperty CallListProperty =
           DependencyProperty.Register(
               "CallList",
               typeof(IEnumerable<CallInList>),
               typeof(CallsListWindow),
               new PropertyMetadata(null));


        public BO.Enums.CallTypeEnum SelectedCallType
        {
            get => _selectedCallType;
            set
            {
                if (_selectedCallType != value)
                {
                    _selectedCallType = value;
                    //OnPropertyChanged(); // עידכון הממשק
                    FilterCalls(); // סינון הרשימה
                }
            }
        }

        private void FilterCalls()
        {
            FilteredCalls.Clear();
            foreach (var call in CallList)
            {
                if (SelectedCallField == CallFieldEnum.CallType && call.CallType == CallTypeEnum.None)
                {
                    FilteredCalls.Add(call);
                }
                else if (SelectedCallField == CallFieldEnum.CallId)
                {
                    FilteredCalls.Add(call);
                }
            }
        }

        private void LoadCallList()
        {
            UpdateCallList(null);  // טוען את הרשימה הראשונית ללא סינון
        }
        public void UpdateCallList(CallFieldEnum? field)
        {
            try
            {
                var calls = field == CallFieldEnum.CallId
           ? s_bl?.Call.GetCallList(null) ?? new List<CallInList>()
           : s_bl?.Call.GetCallList(null, field) ?? new List<CallInList>();

                callList = calls;  // עדכון callList
                FilteredCalls.Clear();
                foreach (var call in calls)
                {
                    FilteredCalls.Add(call);  // עדכון FilteredCalls
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating call list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                callList = new List<CallInList>();  // רשימה ריקה במקרה של כשל
            }
        }

        private void ObserveVolunteerListChanges()
        {
            UpdateCallList(_selectedCallField);
        }

        private void ObserveCallListChanges()
        {
            LoadCallList();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void lsvCallList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCall != null)
            {
                new SingleCallWindow(SelectedCall.CallId).Show();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            new SingleCallWindow().Show();
        }
    }
}
