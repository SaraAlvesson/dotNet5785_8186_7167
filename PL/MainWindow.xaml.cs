using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BlApi;
using DalApi;
using PL.Admin;

namespace PL
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();


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
        private volatile DispatcherOperation? _observerOperation = null; //stage 7
        private volatile DispatcherOperation? _observerOperation2 = null; //stage 7
        private volatile DispatcherOperation? _observerOperation3 = null; //stage 7

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public int Interval { get; set; }


        private Action clockObserver;
        private Action configObserver;
        private volatile bool clockFlag = false;
        private volatile bool configFlag = false;

        public MainWindow()
        {
            InitializeComponent();
            Interval = 1000;
            this.DataContext = this;
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        // שלב 8: סגירת הסימולטור בעת סגירת החלון
        protected override void OnClosed(EventArgs e)
        {
            if (IsSimulatorRunning)
            {
                StopSimulator();
            }
            base.OnClosed(e);
        }

        public void StopSimulator()
        {
            // עצירת הסימולטור
            s_bl.Admin.StopSimulator();
            IsSimulatorRunning = false;

            // שלב 9: החזרת מצב הפקדים לאחר עצירת הסימולטור
            textrr.IsEnabled = true;

            // אם יש שדות אחרים שצריך להחזיר אליהם את האפשרות לשימוש
            // הוסף אותם כאן
        }
        private void callAmountsObserver()
        {
            if (_observerOperation3 is null || _observerOperation3.Status == DispatcherOperationStatus.Completed)
                _observerOperation3 = Dispatcher.BeginInvoke(() =>
                {
                    RefreshCallAmounts();
                });
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("clockObserver activated - Time is updating");

            CurrentTime = s_bl.Admin.GetCurrentTime();
            RiskRange = s_bl.Admin.GetRiskTimeRange();
            RefreshCallAmounts();
            s_bl.Call.AddObserver(callAmountsObserver);

            clockObserver = () =>
            {
                Console.WriteLine("clockObserver activated"); // בדיקה אם זה בכלל רץ
                if (!clockFlag)
                {
                    clockFlag = true;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        DateTime newTime = s_bl.Admin.GetCurrentTime();
                        Console.WriteLine($"Previous Time: {CurrentTime}");
                        Console.WriteLine($"New Time: {newTime}");
                        CurrentTime = newTime; // עדכון השעה
                        clockFlag = false;
                    }));
                }
            };

            configObserver = () =>
            {
                if (!configFlag)
                {
                    configFlag = true;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        RiskRange = s_bl.Admin.GetRiskTimeRange();
                        configFlag = false;
                    }));
                }
            };

            s_bl.Admin.AddClockObserver(clockObserver);
            Console.WriteLine("clockObserver was added.");

            s_bl.Admin.AddConfigObserver(configObserver);
        }


        private void CloseAllOtherWindows()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window != this)
                {
                    window.Close();
                }
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
            s_bl.Call.RemoveObserver(callAmountsObserver);
        }


        public DateTime CurrentTime
        {
            get { return (DateTime)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }
        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

        private TimeSpan RiskRange
        {
            get => (TimeSpan)GetValue(RiskRangeProperty);
            set => SetValue(RiskRangeProperty, value);
        }

        private static readonly DependencyProperty RiskRangeProperty =
          DependencyProperty.Register(
              nameof(RiskRange),
              typeof(TimeSpan),
              typeof(MainWindow),
            new PropertyMetadata(s_bl.Admin.GetRiskTimeRange()));

        private void btnAddDay(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.UpdateClock(BO.Enums.TimeUnitEnum.DAY);
        }

        private void btnAddHour(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.UpdateClock(BO.Enums.TimeUnitEnum.HOUR);
            RefreshCallAmounts();
        }

        private void btnAddMin(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.UpdateClock(BO.Enums.TimeUnitEnum.MINUTE);
            RefreshCallAmounts();
        }

        private void btnAddMonth(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.UpdateClock(BO.Enums.TimeUnitEnum.MONTH);
            RefreshCallAmounts();
        }

        private void btnAddYear(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.UpdateClock(BO.Enums.TimeUnitEnum.YEAR);
            RefreshCallAmounts();
        }

        private void update_click(object sender, RoutedEventArgs e)
        {
            try
            {
                // המרה עם TryParse במקום Parse, כדי למנוע חריגות
                TimeSpan RiskRangetxt;
                if (TryParseTimeSpan(textrr.Text, out RiskRangetxt))
                {
                    s_bl.Admin.SetRiskTimeRange(RiskRangetxt);
                    RefreshCallAmounts();
                    MessageBox.Show("העדכון בוצע בהצלחה!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("הזמן שהוזן לא בפורמט תקין", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"אירעה שגיאה בעת עדכון בסיס הנתונים: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool TryParseTimeSpan(string input, out TimeSpan result)
        {
            // מנסים להמיר את הערך מ- string ל-TimeSpan
            var parts = input.Split(':');
            if (parts.Length == 3)
            {
                try
                {
                    int hours = int.Parse(parts[0]);
                    int minutes = int.Parse(parts[1]);
                    int seconds = int.Parse(parts[2]);
                    result = new TimeSpan(0, hours, minutes, seconds);
                    return true;
                }
                catch
                {
                    result = TimeSpan.Zero;
                    return false;
                }
            }
            result = TimeSpan.Zero;
            return false;
        }


        private void btnVolunteerList(object sender, RoutedEventArgs e)
        {
            new VolunteerListWindow().Show();
        }

        private void btnReset(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("האם אתה בטוח שברצונך לאפס את בסיס הנתונים?", "אישור איפוס", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                CloseAllOtherWindows();
                try
                {
                    s_bl.Admin.ResetDatabase();
                    RefreshCallAmounts();
                    MessageBox.Show("Database has been successfully reset.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // עדכון RiskRange לאחר איפוס
                    RiskRange = s_bl.Admin.GetRiskTimeRange(); // עדכון השדה

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while resetting the database:  {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        // Using a DependencyProperty as the backing store for CallAmounts
        public static readonly DependencyProperty CallAmountsProperty =
            DependencyProperty.Register("CallAmounts", typeof(int[]), typeof(MainWindow), new PropertyMetadata(null));

        /// <summary>
        /// Refreshes the call amounts by reading the latest data from the backend.
        /// </summary>
        private void RefreshCallAmounts()
        {
            CallAmounts = helpReadCallAmounts(); // Update the amounts from the data source
        }
        // DependencyProperty for CallAmounts, enables animation, styling, binding, etc.
        public int[] CallAmounts
        {
            get { return (int[])GetValue(CallAmountsProperty); }
            set { SetValue(CallAmountsProperty, value); }
        }
        private static int[] helpReadCallAmounts()
        {
            return s_bl.Call.CallsAmount().ToArray();
        }

        private void btnInit(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to Initialize DataBase?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    s_bl.Admin.InitializeDatabase();
                    RefreshCallAmounts();
                    MessageBox.Show("Database has been successfully initialized.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // עדכון RiskRange לאחר אתחול
                    RiskRange = s_bl.Admin.GetRiskTimeRange(); // עדכון השדה

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while initializing the database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void OnStatusClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string status)
            {
                // מיפוי הסטטוס שנלחץ ל-BO.Status enum
                BO.Enums.CalltStatusEnum? filterStatus = status switch
                {
                    "Open Calls:" => BO.Enums.CalltStatusEnum.OPEN,
                    "In-Progress Calls:" => BO.Enums.CalltStatusEnum.CallIsBeingTreated,
                    "Closed Calls:" => BO.Enums.CalltStatusEnum.CLOSED,
                    "Expired Calls:" => BO.Enums.CalltStatusEnum.EXPIRED,
                    "Risk Treatment:" => BO.Enums.CalltStatusEnum.CallTreatmentAlmostOver,
                    "Risk Open Calls:" => BO.Enums.CalltStatusEnum.CallAlmostOver,
                    _ => null
                };

                if (filterStatus.HasValue)
                {
                    try
                    {
                        new Admin.CallsListWindow(filterStatus).Show();
                        // פתח חלון רשימת קריאות מסונן
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"שגיאה בפתיחת החלון: {ex.Message}");
                    }
                }
            }
        }

        private void btnHandleCalls(object sender, RoutedEventArgs e)
        {
            new Admin.CallsListWindow(null).Show();
        }

        private void StartStopSimulator(object sender, RoutedEventArgs e)
        {
            if (IsSimulatorRunning)
            {
                // עצור את הסימולטור
                s_bl.Admin.StopSimulator();
            }
            else
            {
                // הפעל את הסימולטור
                s_bl.Admin.StartSimulator(Interval);
            }

            // עדכון המאפיין של IsSimulatorRunning
            IsSimulatorRunning = !IsSimulatorRunning;

            // עדכון כפתור (לא חייבים לעשות את זה כאן, ה-binding יעשה את זה אוטומטית)
        }

        private void txtClockSpeed_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void textrr_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
