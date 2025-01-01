using System;
using System.Windows;
using BlApi;
using DalApi;

namespace PL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        // הגדרת הצופים
        private Action clockObserver;
        private Action configObserver;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed; // רישום לאירוע סגירת החלון
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // השמת הערך הנוכחי של שעון המערכת
            CurrentTime = s_bl.Admin.GetCurrentTime();

            // השמת ערך משתני התצורה
            RiskRange = s_bl.Admin.GetRiskTimeRange();

            // יצירת הצופים
            clockObserver = () => CurrentTime = s_bl.Admin.GetCurrentTime();
            configObserver = () => RiskRange = s_bl.Admin.GetRiskTimeRange();

            // הוספת המשקיפים
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
        }

        // מתודת סגירת החלון
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // הסרת המשקיפים בסגירת החלון
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);
        }

        // רכיבי ממשק המשתמש
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
            new PropertyMetadata(s_bl.Admin.GetRiskTimeRange())); // או מתודה אחרת שמחזירה את הערך


        // פעולות הקידום
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

        // עדכון משתני התצורה
        private void update_click(object sender, RoutedEventArgs e)
        {
            TimeSpan RiskRangetxt = TimeSpan.Parse(textrr.Text);
            s_bl.Admin.SetRiskTimeRange(RiskRangetxt);
        }
    }
}