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

        private VolunteerInListField _selectedVolunteerField = VolunteerInListField.None;

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

        // תכונת תלות לרשימת המתנדבים
        public IEnumerable<VolunteerInList> VolunteerList
        {
            get => (IEnumerable<VolunteerInList>)GetValue(VolunteerListProperty);
            set
            {
                SetValue(VolunteerListProperty, value ?? new List<VolunteerInList>());  // טיפול במקרה של null
                OnPropertyChanged(nameof(VolunteerList));
            }
        }

        private void LoadVolunteerList()
        {
            UpdateVolunteerList(null);  // טוען את הרשימה הראשונית ללא סינון
        }

        public VolunteerInListField SelectedVolunteerField
        {
            get => _selectedVolunteerField;
            set
            {
                if (_selectedVolunteerField != value)
                {
                    _selectedVolunteerField = value;
                    OnFieldChanged();  // עדכון הרשימה בהתבסס על השדה החדש
                }
            }
        }

        public void OnFieldChanged()
        {
            UpdateVolunteerList(_selectedVolunteerField);
        }

        private void UpdateVolunteerList(VolunteerInListField? field)
        {
            try
            {
                VolunteerList = field == VolunteerInListField.None
                    ? s_bl?.Volunteer.RequestVolunteerList(null) ?? new List<VolunteerInList>()  // טיפול במקרה של null
                    : s_bl?.Volunteer.RequestVolunteerList(null, field) ?? new List<VolunteerInList>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating volunteer list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                VolunteerList = new List<VolunteerInList>();  // רשימה ריקה במקרה של כשל
            }
        }

        private void ObserveVolunteerListChanges()
        {
            UpdateVolunteerList(_selectedVolunteerField);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static readonly DependencyProperty VolunteerListProperty =
            DependencyProperty.Register(
                "VolunteerList",
                typeof(IEnumerable<VolunteerInList>),
                typeof(VolunteerListWindow),
                new PropertyMetadata(null));
    }

}
