using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BO;

namespace PL.Admin
{
    public partial class VolunteerListWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public BO.VolunteerInList? SelectedVolunteer { get; set; }

        public VolunteerListWindow()
        {
            InitializeComponent();
            this.DataContext = this;  // DataContext יהיה חלון זה עצמו

            LoadVolunteerList(); // טוען את הרשימה הראשונית
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
        public ObservableCollection<VolunteerInList> Volunteers { get; set; } = new();

        // טעינת הרשימה
        private void LoadVolunteerList()
        {
            try
            {
                Volunteers.Clear();  // ניקוי הרשימה לפני הוספה חדשה

                var volunteerList = s_bl?.Volunteer.RequestVolunteerList(null) ?? new List<VolunteerInList>();

                foreach (var volunteer in volunteerList)
                {
                    Volunteers.Add(volunteer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading volunteer list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ObserveVolunteerListChanges()
        {
            LoadVolunteerList();  // טוען את הרשימה מחדש
        }

        // מחיקת מתנדב
        private void DeleteVolunteer(int volunteerId)
        {
            try
            {
                var bl = BlApi.Factory.Get().Volunteer;
                bl.DeleteVolunteer(volunteerId); // מחיקת המתנדב
                LoadVolunteerList();  // עדכון הרשימה
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting the volunteer: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // לחצן מחיקה
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int volunteerId)
            {
                var result = MessageBox.Show("Are you sure you want to delete this volunteer?",
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    DeleteVolunteer(volunteerId); // קריאה למחיקה
                }
            }
        }

        // לחצן הוספה
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            new SingleVolunteerWindow().Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Double-click on volunteer list to view details
        private void lsvVolunteerList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedVolunteer != null)
                new SingleVolunteerWindow(SelectedVolunteer.Id).Show();
        }
    }
}
