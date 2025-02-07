using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("clockObserver activated - Time is updating");

            CurrentTime = s_bl.Admin.GetCurrentTime();
            RiskRange = s_bl.Admin.GetRiskTimeRange();

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
            CloseAllOtherWindows();
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
        }

        private void btnAddMin(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.UpdateClock(BO.Enums.TimeUnitEnum.MINUTE);
        }

        private void btnAddMonth(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.UpdateClock(BO.Enums.TimeUnitEnum.MONTH);
        }

        private void btnAddYear(object sender, RoutedEventArgs e)
        {
            s_bl.Admin.UpdateClock(BO.Enums.TimeUnitEnum.YEAR);
        }

        private void update_click(object sender, RoutedEventArgs e)
        {
            try
            {
                TimeSpan RiskRangetxt = TimeSpan.Parse(textrr.Text);
                s_bl.Admin.SetRiskTimeRange(RiskRangetxt);
                MessageBox.Show("העדכון בוצע בהצלחה!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"אירעה שגיאה בעת עדכון בסיס הנתונים: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           
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
                    MessageBox.Show("האיפוס בוצע בהצלחה!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"אירעה שגיאה בעת איפוס בסיס הנתונים: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnInit(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to Initialize DataBase?", "אישור אתחול", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    s_bl.Admin.InitializeDatabase();
                    MessageBox.Show("האתחול בוצע בהצלחה!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"אירעה שגיאה בעת אתחול בסיס הנתונים: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }




        private void btnHandleCalls(object sender, RoutedEventArgs e)
        {
            new Admin.CallsListWindow().Show();
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
    }
    }
