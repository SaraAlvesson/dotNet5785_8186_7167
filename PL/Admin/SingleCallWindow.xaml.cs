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

namespace PL.Admin
{
    public partial class SingleCallWindow : Window, INotifyPropertyChanged
    {
       
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
            get => CurrentCall!=null&& (CurrentCall?.CallStatus == CalltStatusEnum.OPEN ||
                   CurrentCall?.CallStatus == CalltStatusEnum.CallAlmostOver);
        }

        public bool CanEdit 
        { 
            get=>CurrentCall!=null && (CurrentCall?.CallStatus == CalltStatusEnum.OPEN ||
                   CurrentCall?.CallStatus == CalltStatusEnum.CallAlmostOver|| CurrentCall?.CallStatus==CalltStatusEnum.CallIsBeingTreated || CurrentCall?.CallStatus==CalltStatusEnum.CallTreatmentAlmostOver);
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




        public bool CanEditMaxFinishTime
        {
            get
            {
                if (CurrentCall == null)
                    return false;

                return CurrentCall.CallStatus == CalltStatusEnum.CallIsBeingTreated ||
                       CurrentCall.CallStatus == CalltStatusEnum.CallTreatmentAlmostOver|| CurrentCall.CallStatus == CalltStatusEnum.OPEN|| CurrentCall.CallStatus == CalltStatusEnum.CallAlmostOver;
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
                if (CurrentCall == null)
                {
                    MessageBox.Show("No call data available", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // בדיקות שדות חסרים או לא תקינים ב-CurrentCall
                if (CurrentCall.CallType == null)
                {
                    MessageBox.Show("Call type is required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrEmpty(CurrentCall.Address))
                {
                    MessageBox.Show("Address is required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (CurrentCall.OpenTime == null)
                {
                    MessageBox.Show("Open time is required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (CurrentCall.MaxFinishTime == null)
                {
                    MessageBox.Show("Max finish time is required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // אם כל השדות תקינים
                if (ButtonText == "Add")
                {
                    // לוודא שלא מתבצע עדכון אם מדובר בהוספה
                    s_bl.Call.AddCallAsync(CurrentCall); // הוספת קריאה חדשה
                    MessageBox.Show("Call added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                else if (ButtonText == "Update")
                {
                    { 
                        // כאן מטפלים רק בעדכון, לוודא לא קוראים לעדכון אם הכפתור לא מתכוון לעדכן
                        //if (!IsEditable())
                        //{
                        //    MessageBox.Show("You cannot edit this call because it is either not open or currently in progress.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        //    return;
                        //}

                        s_bl.Call.UpdateCallDetails(CurrentCall); // עדכון קריאה קיימת
                        MessageBox.Show("Call updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close();
                    }
                }

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

        private IEnumerable<BO.CallInList> queryCallList()
        {
            IEnumerable<BO.CallInList> calls;

            switch (SelectedFilter)
            {
                case CallFieldEnum.ID:
                    calls = s_bl.Call.GetCallList(CallFieldEnum.ID, null).OrderBy(v => v.Id);
                    break;
                case CallFieldEnum.CallId:
                    calls = s_bl.Call.GetCallList(CallFieldEnum.CallId, null).OrderBy(v => v.CallId);
                    break;
                case CallFieldEnum.Status:
                    calls = s_bl.Call.GetCallList(CallFieldEnum.Status, null).OrderBy(v => v.Status);
                    break;
                case CallFieldEnum.LastVolunteerName:
                    calls = s_bl.Call.GetCallList(null, CallFieldEnum.LastVolunteerName).OrderBy(v => v.LastVolunteerName);
                    break;
                case CallFieldEnum.OpenTime:
                    calls = s_bl.Call.GetCallList(CallFieldEnum.OpenTime, null).OrderBy(v => v.OpenTime);
                    break;
                case CallFieldEnum.SumAssignment:
                    calls = s_bl.Call.GetCallList(CallFieldEnum.SumAssignment, null).OrderBy(v => v.SumAssignment);
                    break;
                default:
                    calls = s_bl.Call.GetCallList(null, null);
                    break;
            }

            return calls;
        }

        // Automatically refresh list of assignments whenever current call is updated
        private void CallListObserver()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                // אם יש עדכון שממתין, לא נבצע עדכון נוסף
                if (isUpdating)
                    return;

                isUpdating = true;
                try
                {
                    // הוספתי קריאה ל-Dispatcher.Invoke לעדכון סטטוס בצורה בטוחה
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

     

       
    }
}
