using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BO;
using static BO.Enums;

namespace PL.Admin;

public partial class VolunteerListWindow : Window, INotifyPropertyChanged
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    private VolunteerInListField _selectedVolunteerField = VolunteerInListField.None;

        public VolunteerListWindow()
        {
            InitializeComponent();
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


    // תכונת תלות לרשימת המתנדבים



    public IEnumerable<BO.VolunteerInList> VolunteerList
    {
        get { return (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerListProperty); }
        set { SetValue(VolunteerListProperty, value); }
    }

    public static readonly DependencyProperty VolunteerListProperty =
        DependencyProperty.Register("VolunteerList", typeof(IEnumerable<BO.VolunteerInList>), typeof(VolunteerListWindow), new PropertyMetadata(null));


   

    // קריאה לשכבת ה-BL כדי להוריד את הרשימה
    private void LoadVolunteerList()
    {
        // ביצוע קריאה לשכבת ה-BL להורדת הרשימה
        UpdateVolunteerList(null);  // טוען את הרשימה הראשונית ללא סינון
    }

    // אירוע שינוי בחירת שדה מתוך ComboBox
    public VolunteerInListField SelectedVolunteerField
    {
        get { return _selectedVolunteerField; }
        set
        {
            if (_selectedVolunteerField != value)
            {
                _selectedVolunteerField = value;
                OnFieldChanged();  // עדכון הרשימה בהתבסס על השדה החדש
            }
        }
    }

  

    // מתודה לעדכון הרשימה לפי השדה הנבחר
    public void OnFieldChanged()
    {
        // עדכון הרשימה בהתבסס על השדה שנבחר ב-ComboBox
        UpdateVolunteerList(vol);
    }

    // מתודה פרטית לעדכון הרשימה עם השדה שנבחר
    private void UpdateVolunteerList(VolunteerInListField? field)
    {
        VolunteerList = GetVolunteerListByFilter(field);
    }

    private IEnumerable<VolunteerInList> GetVolunteerListByFilter(VolunteerInListField? field)
    {
        if (field == VolunteerInListField.None)
        {
            return s_bl?.Volunteer.RequestVolunteerList();
        }
        return s_bl?.Volunteer.RequestVolunteerList(null, field);
    }



    // מתודה שתשקיף על שינויים ברשימת המתנדבים (תעדכן את הרשימה בהתאם לשינויים ב-BL)
    private void ObserveVolunteerListChanges()
    {
        UpdateVolunteerList(vol);  // נעדכן את הרשימה על פי השדה הנבחר
    }

    // רשום לאירוע טעינת המסך (Loaded) כדי להפעיל את מתודת ההשקפה
   
    // מתודה לעדכון תצוגה של פרופרטי, הכרחית עבור INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    // הרשמה לאירוע טעינת המסך (Loaded)
  


    // הגדרת תכונת תלות עבור רשימת המתנדבים
    
    public BO.Enums.VolunteerInListField vol { get; set; } = BO.Enums.VolunteerInListField.None;




    private void cbCallType(object sender, SelectionChangedEventArgs e)
    {
        ObserveVolunteerListChanges();  // עדכון הרשימה על פי הערך החדש ב- ComboBox
    }

}
