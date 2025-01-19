using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlApi;
using DalApi;
using PL.Admin;

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

        // סגירת כל החלונות הפתוחים חוץ מהחלון הראשי
        private void CloseAllOtherWindows()
        {
            foreach (Window window in Application.Current.Windows)
            {
                // סגור את כל החלונות חוץ מהחלון הראשי
                if (window != this)
                {
                    window.Close();
                }
            }
        }

        // מתודת סגירת החלון
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // הסרת הצופים
            s_bl.Admin.RemoveClockObserver(clockObserver);
            s_bl.Admin.RemoveConfigObserver(configObserver);

            // סגירת כל החלונות חוץ מהחלון הראשי
            CloseAllOtherWindows();
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

        private void btnVolunteerList(object sender, RoutedEventArgs e)
        {
            new VolunteerListWindow().Show();
        }

        private void btnReset(object sender, RoutedEventArgs e)
        {
            // בקשה לאישור מהמשתמש
            var result = MessageBox.Show("האם אתה בטוח שברצונך לאפס את בסיס הנתונים?", "אישור איפוס", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                //// שינוי אייקון העכבר לשעון חול
                //Mouse.OverrideCursor = Cursors.Wait;

                // סגירת כל החלונות הפתוחים (חוץ מהחלון הראשי)
                CloseAllOtherWindows();

                try
                {
                    // קריאה לפוקנציה של איפוס בסיס הנתונים ב-BL
                    s_bl.Admin.ResetDatabase();
                    MessageBox.Show("האיפוס בוצע בהצלחה!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"אירעה שגיאה בעת איפוס בסיס הנתונים: {ex.Message}", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                //finally
                //{
                //    // החזרת אייקון העכבר לברירת המחדל
                //    Mouse.OverrideCursor = null;
                //}
            }
        }

        private void btnInit(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to Initialize DataBase?", "אישור אתחול", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {


                

                try
                {
                    // קריאה לפוקנציה של אתחול בסיס הנתונים ב-BL
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
        private void textrr_TextChanged(object sender, TextChangedEventArgs e)
        {
            // הוספת קוד פעולה במקרה של שינוי בטקסט
        }


    }
}
