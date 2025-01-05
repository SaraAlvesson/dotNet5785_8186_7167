using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BO;
using static BO.Enums;

namespace PL.Admin
{
    public partial class VolunteerListWindow : Window, INotifyPropertyChanged
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private CallTypeEnum _selectedCallType = CallTypeEnum.None;

        public VolunteerListWindow()
        {
            InitializeComponent();
            this.DataContext = new VolunteerListViewModel();  // הגדרת DataContext
            LoadVolunteerList();  // טוען את רשימת המתנדבים עם הערכים הראשונים
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl?.Volunteer.AddObserver(ObserveVolunteerListChanges);  // נרשמים למשקיף
            ObserveVolunteerListChanges();  // מבצע את הקריאה כדי להוריד את הרשימה המעודכנת
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Volunteer.RemoveObserver(ObserveVolunteerListChanges);  // מסירים את המשקיף
        }

        public IEnumerable<VolunteerInList> VolunteerList
        {
            get { return (IEnumerable<VolunteerInList>)GetValue(VolunteerListProperty); }
            set { SetValue(VolunteerListProperty, value); OnPropertyChanged(nameof(VolunteerList)); }
        }

        public static readonly DependencyProperty VolunteerListProperty =
            DependencyProperty.Register("VolunteerList", typeof(IEnumerable<VolunteerInList>), typeof(VolunteerListWindow), new PropertyMetadata(null));

        // קריאה לשכבת ה-BL כדי להוריד את הרשימה
        private void LoadVolunteerList()
        {
            UpdateVolunteerListBasedOnSelectedType();  // טוען את הרשימה הראשונית עם סינון לפי CallType
        }

        // אירוע שינוי בחירת שדה מתוך ComboBox
        public CallTypeEnum SelectedCallTypeEnumField
        {
            get { return _selectedCallType; }
            set
            {
                if (_selectedCallType != value)
                {
                    _selectedCallType = value;
                    UpdateVolunteerListBasedOnSelectedType(); // עדכון הרשימה בהתבסס על השדה החדש
                }
            }
        }

        // מתודה לעדכון הרשימה לפי השדה הנבחר
        private void UpdateVolunteerListBasedOnSelectedType()
        {
            // עדכון רשימת המתנדבים בהתבסס על סוג הקריאה הנבחר
            VolunteerList = GetCallTypeEnumByFilter(_selectedCallType);
        }

        private IEnumerable<VolunteerInList> GetCallTypeEnumByFilter(CallTypeEnum selectedCallType)
        {
            if (selectedCallType == CallTypeEnum.None)
            {
                // במקרה של "None", לא עושים סינון, משדרים null לסינון
                return s_bl?.Volunteer.RequestVolunteerList(null, null);
            }
            else
            {
                // במקרה של סוג קריאה שנבחר, מעבירים את סוג הקריאה לסינון המתנדבים
                return s_bl?.Volunteer.RequestVolunteerList(null, (VolunteerInListField)selectedCallType);
            }
        }

        // מתודה שתשקיף על שינויים ברשימת המתנדבים
        private void ObserveVolunteerListChanges()
        {
            UpdateVolunteerListBasedOnSelectedType();  // עדכון הרשימה על פי השדה הנבחר
        }

        // הגדרת תכונת תלות עבור רשימת המתנדבים
        public CallTypeEnum vol { get; set; } = CallTypeEnum.None;

        // אירוע שינוי ערך ב-ComboBox
        private void cbCallType(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selectedItem = (CallTypeEnum)e.AddedItems[0];
                if (_selectedCallType != selectedItem)
                {
                    _selectedCallType = selectedItem;
                    UpdateVolunteerListBasedOnSelectedType();  // עדכון הרשימה על פי הערך החדש ב-ComboBox
                }
            }
        }

        // מימוש של PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // אם יש פריט שנבחר, נקבל אותו
            var selectedVolunteer = (VolunteerInList)((ListView)sender).SelectedItem;

            // אם נבחר מתנדב (לא null), ניתן לבצע פעולות נוספות
            if (selectedVolunteer != null)
            {
                // לדוגמה, עדכון פרטי המתנדב
                DisplayVolunteerDetails(selectedVolunteer);
            }
            else
            {
                MessageBox.Show("No volunteer selected.");
            }
        }

        private void DisplayVolunteerDetails(VolunteerInList volunteer)
        {
            // כאן תוכל להוסיף את הלוגיקה להציג את פרטי המתנדב, כמו הצגת מידע נוסף על המסך
            // לדוגמה:
            MessageBox.Show($"Selected Volunteer: {volunteer.FullName}, Calls Treated: {volunteer.SumTreatedCalls}");
        }

        // ViewModel לעדכון הרשימה
        public class VolunteerListViewModel : INotifyPropertyChanged
        {
            private IEnumerable<CallTypeEnum> _callTypes;

            public IEnumerable<CallTypeEnum> CallTypes
            {
                get { return _callTypes; }
                set
                {
                    _callTypes = value;
                    OnPropertyChanged(nameof(CallTypes));
                }
            }

            public VolunteerListViewModel()
            {
                // כאן אתה יכול להגדיר את אוסף ה-callTypes מהמנהל שלך
                CallTypes = Enum.GetValues(typeof(CallTypeEnum)) as IEnumerable<CallTypeEnum>;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
