using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using BO;
using DO;
using static BO.Enums;
using System.Windows.Threading;
using System.Runtime.CompilerServices;

namespace PL.Admin
{
    public partial class SingleCallWindow : Window, INotifyPropertyChanged
    {
        private bool _isSimulatorRunning;

        public bool IsSimulatorRunning
        {
            get { return _isSimulatorRunning; }
            set
            {
                if (_isSimulatorRunning != value)
                {
                    _isSimulatorRunning = value;
                    OnPropertyChanged(nameof(IsSimulatorRunning)); // זה יעדכן את ה-binding
                }
            }
        }
        // דגלים למניעת עדכונים מרובים
        private CallFieldEnum _selectedCallField = CallFieldEnum.ID;
        private volatile bool isUpdating = false;

        public CallFieldEnum SelectedFilter
        {
            get { return _selectedCallField; }
            set
            {
                if (_selectedCallField != value)
                {
                    _selectedCallField = value;
                    OnPropertyChanged(nameof(SelectedFilter));
                    OnPropertyChanged(nameof(IsEditable)); // עדכון שדות ה-UI
                    OnPropertyChanged(nameof(CanEditMaxFinishTime)); // להפעיל עדכון גם ל-MaxFinishTime
                    OnPropertyChanged(nameof(CanEdit));
                    UpdateCallList();
                }
            }
        }

        public bool IsEditable
        {
            get => CurrentCall != null && (CurrentCall?.CallStatus == CalltStatusEnum.OPEN ||
                   CurrentCall?.CallStatus == CalltStatusEnum.CallAlmostOver);
        }

        public bool CanEdit
        {
            get => CurrentCall != null && (CurrentCall?.CallStatus == CalltStatusEnum.OPEN ||
                   CurrentCall?.CallStatus == CalltStatusEnum.CallAlmostOver || CurrentCall?.CallStatus == CalltStatusEnum.CallIsBeingTreated || CurrentCall?.CallStatus == CalltStatusEnum.CallTreatmentAlmostOver);
        }

        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        public string ButtonText { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private BO.Call _currentCall;

        public BO.Call CurrentCall
        {
            get { return _currentCall; }
            set
            {
                _currentCall = value;
                OnPropertyChanged(nameof(CurrentCall));
                OnPropertyChanged(nameof(IsEditable)); // עדכון שדות UI
                OnPropertyChanged(nameof(CanEditMaxFinishTime)); // להפעיל עדכון גם ל-MaxFinishTime
                OnPropertyChanged(nameof(CanEdit));
            }
        }
        private DateTime? _maxFinishTime; // שדה שנשמר את הזמן המקסימ
        public DateTime? MaxFinishTime
        {
            get => _maxFinishTime;
            set
            {
                if (_maxFinishTime != value)
                {
                    _maxFinishTime = value;
                    OnPropertyChanged(nameof(MaxFinishTime)); // עדכון ה-UI

                    // עדכון הסטטוס של הקריאה בהתאם לזמן המקסימלי
                    try
                    {
                        // עדכון הקריאה מחדש לאחר שינוי זמן הסיום המקסימלי
                        CurrentCall = s_bl.Call.readCallData(CurrentCall.Id); // קריאה מחדש לשרת
                        OnPropertyChanged(nameof(CurrentCall)); // עדכון ה-UI על כל האובייקט

                        // עדכון שדות UI אם יש צורך
                        LoadCurrentCallDetails();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating call status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        public int SelectedHour { get; set; } = 0;  // משתנה לשעה שנבחרה
        public int SelectedMinute { get; set; } = 0; // משתנה לדקה שנבחרה

        private void OnSetMaxFinishTimeClick(object sender, RoutedEventArgs e)
        {
            // תאריך שנבחר ב-DatePicker
            DateTime? selectedDate = CurrentCall.MaxFinishTime; // נניח שמדובר בתאריך מ-Binding

            if (selectedDate.HasValue)
            {
                // הוספת שעה ודקה לתאריך
                DateTime finalDateTime = selectedDate.Value
                                          .AddHours(SelectedHour)
                                          .AddMinutes(SelectedMinute);

                // עדכון ה-MaxFinishTime עם הזמן החדש
                CurrentCall.MaxFinishTime = finalDateTime;
            }
        }

        public bool CanEditMaxFinishTime
        {
            get
            {
                if (CurrentCall == null)
                    return false;

                return CurrentCall.CallStatus == CalltStatusEnum.CallIsBeingTreated ||
                       CurrentCall.CallStatus == CalltStatusEnum.CallTreatmentAlmostOver || CurrentCall.CallStatus == CalltStatusEnum.OPEN || CurrentCall.CallStatus == CalltStatusEnum.CallAlmostOver;
            }
        }

        private ObservableCollection<CallAssignInList> _callAssignInLists;
        public ObservableCollection<CallAssignInList> CallAssignInLists
        {
            get { return _callAssignInLists; }
            set
            {
                _callAssignInLists = value;
                OnPropertyChanged(nameof(CallAssignInLists));
            }
        }

        public SingleCallWindow(int id = 0)
        {
            ButtonText = id == 0 ? "Add" : "Update";
            InitializeComponent();
            this.DataContext = this;

            try
            {
                if (id == 0)
                {
                    // קריאה חדשה
                    CurrentCall = new BO.Call
                    {
                        Id = s_bl.Call.GetNextId(), // קבלת המספר הבא רק אם זו קריאה חדשה
                        OpenTime = DateTime.Now,
                    };
                }
                else
                {
                    CurrentCall = s_bl.Call.readCallData(id);
                    LoadCurrentCallDetails(); // טוען פרטים נוספים של הקריאה רק אם זו לא קריאה חדשה
                }

                // אתחול טיימר רק אם זו לא קריאה חדשה
                if (id != 0)
                {
                    _timer = new DispatcherTimer();
                    _timer.Interval = TimeSpan.FromSeconds(1);
                    _timer.Tick += Timer_Tick;
                    _timer.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DispatcherTimer _timer;
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (CurrentCall?.Id > 0) // בודק שיש קריאה קיימת
            {
                try
                {
                    LoadCurrentCallDetails();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating call details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadCurrentCallDetails()
        {
            if (CurrentCall?.Id > 0) // רק אם יש קריאה קיימת
            {
                try
                {
                    if (IsEditable || CanEditMaxFinishTime) // אם הקריאה ניתנת לעריכה, לא נעדכן מהשרת!
                        return;

                    var updatedCall = s_bl.Call.readCallData(CurrentCall.Id);
                    if (updatedCall != null)
                    {
                        CurrentCall = updatedCall;
                        CallAssignInLists = new ObservableCollection<CallAssignInList>(updatedCall.CallAssignInLists ?? new List<CallAssignInList>());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading call details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Notify UI of property changes
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ButtonAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                // עדכון הסטטוס למקסימום זמן סיום
                if (ButtonText == "Add")
                {
                    s_bl.Call.AddCallAsync(CurrentCall); // הוספת קריאה חדשה
                    MessageBox.Show("Call added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                else if (ButtonText == "Update")
                {
                    s_bl.Call.UpdateCallDetails(CurrentCall); // עדכון קריאה קיימת
                    MessageBox.Show("Call updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }

                // לאחר עדכון, נוודא שהסטטוס יתעדכן ושלא יתבצע עדכון נוסף
                UpdateCallList();
            }
            catch (InvalidCallLogicException ex)
            {
                MessageBox.Show($"Logic error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidCallFormatException ex)
            {
                MessageBox.Show($"Logic error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCallList()
        {
            if (CurrentCall?.Id > 0) // רק אם יש קריאה קיימת
            {
                try
                {
                    var updatedCall = s_bl.Call.readCallData(CurrentCall.Id);
                    if (updatedCall != null)
                    {
                        CurrentCall = updatedCall;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public IEnumerable<BO.CallInList> CallInList
        {
            get { return (IEnumerable<BO.CallInList>)GetValue(callInListFieldListProperty); }
            set { SetValue(callInListFieldListProperty, value); }
        }

        public static readonly DependencyProperty callInListFieldListProperty =
           DependencyProperty.Register(
               "CallInList",
               typeof(IEnumerable<BO.CallInList>),
               typeof(SingleCallWindow),
               new PropertyMetadata(null));

        // Observer update call list and close window after status update
        private void CallListObserver()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (isUpdating)
                    return;

                isUpdating = true;
                try
                {
                    UpdateCallList();
                }
                finally
                {
                    isUpdating = false;
                }
            }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl?.Call.AddObserver(CallListObserver);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Call.RemoveObserver(CallListObserver);
        }

        private void MaxFinishHourComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
