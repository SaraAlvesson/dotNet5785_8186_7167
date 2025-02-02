using BO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Input;
using static BO.Enums;
using System.Net.Mail; // הוספת ספריית שליחה אימייל

namespace PL.Admin
{
    public partial class CallsListWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        public List<string> CallFieldsSort { get; } = Enum.GetNames(typeof(BO.Enums.CallTypeEnum)).ToList();
        public List<string> SortOptionsCall { get; } = Enum.GetNames(typeof(BO.Enums.CallFieldEnum)).ToList();

        private string _selectedCallType;
        public string SelectedCallType
        {
            get => _selectedCallType;
            set
            {
                if (_selectedCallType != value)
                {
                    _selectedCallType = value;
                    OnPropertyChanged();
                    UpdateCallList();  // עדכון הרשימה לפי הבחירה החדשה בסינון
                }
            }
        }

        private string _selectedSortOption;
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (_selectedSortOption != value)
                {
                    _selectedSortOption = value;
                    OnPropertyChanged();
                    UpdateCallList();  // עדכון הרשימה לפי הבחירה החדשה במיון
                }
            }
        }
        public ICommand DeleteCallCommand { get; private set; }
        public ObservableCollection<CallInList> FilteredCalls { get; set; } = new ObservableCollection<CallInList>();

        public CallsListWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            DeleteCallCommand = new RelayCommand<CallInList>(DeleteCall, CanDeleteCall);
            LoadCallList();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl?.Call.AddObserver(callListObserver);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Call.RemoveObserver(callListObserver);
        }
        public BO.CallInList? SelectedCurrentCall { get; set; }

        private static IEnumerable<CallInList> ReadAllCalls(BO.Enums.CallTypeEnum? callTypeFilter, BO.Enums.CallFieldEnum? sortFieldFilter)
        {
            try
            {
                return s_bl?.Call.GetCallList(sortFieldFilter, callTypeFilter, null) ?? Enumerable.Empty<CallInList>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return Enumerable.Empty<CallInList>();
            }
        }


        public void UpdateCallList()
        {
            try
            {
                BO.Enums.CallTypeEnum? callTypeFilter = Enum.TryParse(SelectedCallType, out BO.Enums.CallTypeEnum parsedCallType)
                    ? parsedCallType
                    : (BO.Enums.CallTypeEnum?)null;

                BO.Enums.CallFieldEnum? sortFieldFilter = Enum.TryParse(SelectedSortOption, out BO.Enums.CallFieldEnum parsedSortField)
                    ? parsedSortField
                    : (BO.Enums.CallFieldEnum?)null;

                // קבלת כל הקריאות עם הסינון והמיון שבחרת
                var updatedCalls = ReadAllCalls(callTypeFilter, sortFieldFilter);

                // עדכון הרשימה
                FilteredCalls.Clear();
                foreach (var call in updatedCalls)
                {
                    FilteredCalls.Add(call);  // הוסף את הקריאות החדשות לרשימה
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating the call list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCallList()
        {
            UpdateCallList();  // טוען את הרשימה הראשונית ללא סינון
        }
        public BO.CallInList? SelectedCall { get; set; }
        private void ObserveCallListChanges()
        {
            LoadCallList();
        }

        private void callListObserver() => RefreshCallList();

        private void RefreshCallList()
        {
            var callTypeFilter = Enum.TryParse(SelectedCallType, out BO.Enums.CallTypeEnum parsedCallType)
                ? parsedCallType
                : (BO.Enums.CallTypeEnum?)null;

            var sortFieldFilter = Enum.TryParse(SelectedSortOption, out BO.Enums.CallFieldEnum parsedSortField)
                ? parsedSortField
                : (BO.Enums.CallFieldEnum?)null;

            var updatedCalls = ReadAllCalls(callTypeFilter, sortFieldFilter);

            FilteredCalls.Clear();
            foreach (var call in updatedCalls)
            {
                FilteredCalls.Add(call);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = null)
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

        private void FilterByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCallList();  // עדכון הרשימה לפי הבחירה בסינון
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            new SingleCallWindow().Show();
        }

        private void SortByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCallList();  // עדכון הרשימה לפי הבחירה במיון
        }

        // פונקציה לשליחת אימייל למתנדב במקרה של ביטול ההקצאה
        private void SendCancellationEmail(string volunteerEmail, string callDetails)
        {
            try
            {
                var mailMessage = new MailMessage("admin@company.com", volunteerEmail)
                {
                    Subject = "הקצאה בוטלה",
                    Body = $"הקריאה הבאה בוטלה: {callDetails}"
                };
                var smtpClient = new SmtpClient("smtp.company.com")
                {
                    Port = 587,
                    Credentials = new System.Net.NetworkCredential("admin@company.com", "password"),
                    EnableSsl = true
                };
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending email: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //// פונקציה לביטול ההקצאה של קריאה
        //private void CancelAssignment(CallInList call)
        //{
        //    if (call.Status == BO.Enums.CalltStatusEnum.CallIsBeingTreated && call.LastVolunteerName != null)
        //    {
        //        // שלח את האימייל למתנדב
        //       // SendCancellationEmail(call..Email, $"קריאה מספר {call.CallId} בוטלה.");
        //        //call.AssignedVolunteer = null; // עדכון הקריאה לביטול ההקצאה

        //        UpdateCallList(); // עדכון הרשימה אחרי ביטול ההקצאה
        //    }
        //    else
        //    {
        //        MessageBox.Show("לא ניתן לבטל הקצאה לקריאה זו.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Warning);
        //    }
        //}

        // פונקציה למחיקת קריאה

        public bool IsDeletable
        {
            get
            {
                return SelectedCall != null &&
                       SelectedCall.Status == BO.Enums.CalltStatusEnum.OPEN &&
                       string.IsNullOrEmpty(SelectedCall.LastVolunteerName);  // קריאה פתוחה ולא הוקצתה למתנדב
            }
        }


        private void DeleteCall(CallInList call)
        {
            if (call != null)
            {
                if (call.Status == BO.Enums.CalltStatusEnum.OPEN && string.IsNullOrEmpty(call.LastVolunteerName))
                {
                    s_bl.Call.DeleteCall(call.CallId);  // מחיקת הקריאה ממאגר
                    UpdateCallList();  // עדכון הרשימה לאחר המחיקה
                }
                else
                {
                    MessageBox.Show("לא ניתן למחוק קריאה בסטטוס זה או אם היא הוקצתה למתנדב.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private bool CanDeleteCall(CallInList call)
        {
            return call != null && call.Status == BO.Enums.CalltStatusEnum.OPEN && string.IsNullOrEmpty(call.LastVolunteerName);
        }

        public class RelayCommand<T> : ICommand
        {
            private readonly Action<T> _execute;
            private readonly Func<T, bool> _canExecute;

            public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter)
            {
                return _canExecute == null || _canExecute((T)parameter);
            }

            public void Execute(object parameter)
            {
                _execute((T)parameter);
            }

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }
        }
    }
}

    

