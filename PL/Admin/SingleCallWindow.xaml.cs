using System;
using System.Windows;
using System.Windows.Input;
using System.Net.Mail;
using static BO.Enums;
using BO;
using System.ComponentModel;
using System.Windows.Controls;
using DO;

namespace PL.Admin
{
    public partial class SingleCallWindow : Window
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private CallFieldEnum _selectedCallField = CallFieldEnum.ID;
        public string ButtonText { get; set; }

        public CallFieldEnum SelectedFilter
        {
            get { return _selectedCallField; }
            set
            {
                if (_selectedCallField != value)
                {
                    _selectedCallField = value;
                    OnPropertyChanged(nameof(SelectedFilter));
                    UpdateCallList();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        public SingleCallWindow(int id = 0)
        {
            ButtonText = id == 0 ? "Add" : "Update";
            InitializeComponent();
            this.DataContext = this;

            try
            {
                if (id == 0)
                {
                    // קריאה חדשה, תעודת זהות מקבלת את הערך הרץ הבא
                    CurrentCall = new BO.Call
                    {

                        Id = s_bl.Call.GetNextId(),  // קריאה לפונקציה בשכבת הלוגיקה
                        OpenTime = DateTime.Now
                    }; 

                    }
                else
                {
                    CurrentCall = s_bl.Call.readCallData(id);
                    SetEditableFields(CurrentCall.CallStatus);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
          
            s_bl?.Call.AddObserver(CallListObserver);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
           
            s_bl?.Call.RemoveObserver(CallListObserver);
        }

        private void CallListObserver()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateCallList();
            });
        }

        public BO.Call CurrentCall
        {
            get { return (BO.Call)GetValue(CurrentCallProperty); }
            set { SetValue(CurrentCallProperty, value); }
        }

        public static readonly DependencyProperty CurrentCallProperty =
            DependencyProperty.Register("CurrentCall", typeof(BO.Call), typeof(SingleCallWindow), new PropertyMetadata(null));

        public bool IsEditable { get; private set; }
        public bool IsMaxTimeEditable { get; private set; }

        private void SetEditableFields(CalltStatusEnum status)
        {
            // ניתן לערוך את כל הפרטים אם הסטטוס הוא פתוחה או פתוחה בסיכון
            IsEditable = status == CalltStatusEnum.OPEN || status == CalltStatusEnum.CallAlmostOver;

            // ניתן לערוך רק את הזמן המקסימלי לסיום אם הסטטוס הוא בטיפול או בטיפול בסיכון
            IsMaxTimeEditable = status == CalltStatusEnum.CallIsBeingTreated || status == CalltStatusEnum.CallTreatmentAlmostOver;
        }

       

        private void UpdateCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            // אם הקריאה לא ניתנת לעריכה (היא סגורה או פג תוקפה)
            if (!IsEditable)
            {
                MessageBox.Show("You cannot edit this call because it's closed or expired.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // עדכון פרטי הקריאה בשכבה הלוגית
                s_bl.Call.UpdateCallDetails(CurrentCall);

                MessageBox.Show("Call updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCallList()
        {
            try
            {
                IEnumerable<BO.CallInList> call = queryCallList();
                CallInList = call;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading the calls list: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private IEnumerable<BO.CallInList> queryCallList()
        {
            IEnumerable<BO.CallInList> calls;

            switch (SelectedFilter)
            {
                case CallFieldEnum.ID:
                    calls = BlApi.Factory.Get().Call.GetCallList(CallFieldEnum.ID, null).OrderBy(v => v.Id);
                    break;
                case CallFieldEnum.CallId:
                    calls = BlApi.Factory.Get().Call.GetCallList(CallFieldEnum.CallId, null).OrderBy(v => v.CallId);
                    break;
                case CallFieldEnum.CallType:
                    calls = BlApi.Factory.Get().Call.GetCallList(CallFieldEnum.Status, null).OrderBy(v => v.CallType);
                    break;
                case CallFieldEnum.Status:
                    calls = BlApi.Factory.Get().Call.GetCallList(CallFieldEnum.Status, null).OrderBy(v => v.Status);
                    break;
                case CallFieldEnum.LastVolunteerName:
                    calls = BlApi.Factory.Get().Call.GetCallList(null, CallFieldEnum.LastVolunteerName).OrderBy(v => v.LastVolunteerName);
                    break;
                case CallFieldEnum.OpenTime:
                    calls = BlApi.Factory.Get().Call.GetCallList(CallFieldEnum.Status, null).OrderBy(v => v.OpenTime);
                    break;
                case CallFieldEnum.SumAssignment:
                    calls = BlApi.Factory.Get().Call.GetCallList(CallFieldEnum.Status, null).OrderBy(v => v.SumAssignment);
                    break;
                default:
                    calls = BlApi.Factory.Get().Call.GetCallList(null, null);
                    break;
            }

            return calls;
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

                if (ButtonText == "Add")
                {
                    s_bl.Call.AddCall(CurrentCall!); // הוספת קריאה חדשה
                    MessageBox.Show("Call added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    s_bl.Call.UpdateCallDetails(CurrentCall); // עדכון קריאה קיימת
                    MessageBox.Show("Call updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                UpdateCallList();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred: {ex.ToString()}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

      
    }
}
