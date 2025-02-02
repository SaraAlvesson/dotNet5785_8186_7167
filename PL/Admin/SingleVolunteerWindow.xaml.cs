using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BO;
using static BO.Enums;

namespace PL.Admin
{
    public partial class SingleVolunteerWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private VolunteerInListField _selectedVolunteerField = VolunteerInListField.None;

        // תכונה (Property) שמייצגת את הטקסט שעל הכפתור
        public string ButtonText { get; set; }

        public VolunteerInListField SelectedFilter
        {
            get { return _selectedVolunteerField; }
            set
            {
                if (_selectedVolunteerField != value)
                {
                    _selectedVolunteerField = value;
                    OnPropertyChanged(nameof(SelectedFilter));
                    UpdateVolunteerList();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // VolunteerInList property (DependencyProperty)
        public IEnumerable<BO.VolunteerInList> VolunteerInList
        {
            get { return (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerInListFieldListProperty); }
            set { SetValue(VolunteerInListFieldListProperty, value); }
        }

        public static readonly DependencyProperty VolunteerInListFieldListProperty =
           DependencyProperty.Register(
               "VolunteerInList",
               typeof(IEnumerable<BO.VolunteerInList>),
               typeof(SingleVolunteerWindow),
               new PropertyMetadata(null));

        // קונסטרוקטור של החלון
        public SingleVolunteerWindow(int id = 0)
        {
            ButtonText = id == 0 ? "Add" : "Update";
            InitializeComponent();

            // חיבור התכונה למנגנון Binding, כדי שה-XAML יקבל את הערך שלה
            this.DataContext = this;

            try
            {
                if (id == 0)
                {
                    CurrentVolunteer = new BO.Volunteer();
                }
                else
                {
                    CurrentVolunteer = s_bl.Volunteer.RequestVolunteerDetails(id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateVolunteerList()
        {
            try
            {
                IEnumerable<BO.VolunteerInList> volunteers = queryVolunteerList();
                VolunteerInList = volunteers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading the volunteer list: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl?.Call.AddObserver(volunteerListObserver);
            UpdateVolunteerList();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            s_bl?.Call.RemoveObserver(volunteerListObserver);
        }

        private void volunteerListObserver()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateVolunteerList();
            });
        }

        private IEnumerable<BO.VolunteerInList> queryVolunteerList()
        {
            IEnumerable<BO.VolunteerInList> volunteers;

            switch (SelectedFilter)
            {
                case VolunteerInListField.Id:
                    volunteers = BlApi.Factory.Get().Volunteer.RequestVolunteerList(null, VolunteerInListField.Id).OrderBy(v => v.Id);
                    break;
                case VolunteerInListField.FullName:
                    volunteers = BlApi.Factory.Get().Volunteer.RequestVolunteerList(null, VolunteerInListField.FullName).OrderBy(v => v.FullName);
                    break;
                case VolunteerInListField.Active:
                    volunteers = BlApi.Factory.Get().Volunteer.RequestVolunteerList(true, VolunteerInListField.Active).Where(v => v.Active);
                    break;
                case VolunteerInListField.None:
                    volunteers = BlApi.Factory.Get().Volunteer.RequestVolunteerList(null, null);
                    break;
                default:
                    volunteers = BlApi.Factory.Get().Volunteer.RequestVolunteerList(null, null);
                    break;
            }

            return volunteers;
        }

        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set { SetValue(CurrentVolunteerProperty, value); }
        }

        public static readonly DependencyProperty CurrentVolunteerProperty =
            DependencyProperty.Register("CurrentVolunteer", typeof(BO.Volunteer), typeof(SingleVolunteerWindow), new PropertyMetadata(null));

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is VolunteerInListField selectedFilter)
            {
                SelectedFilter = selectedFilter;
            }
        }

        private void btnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentVolunteer == null)
                {
                    MessageBox.Show("No volunteer data available", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (ButtonText == "Add")
                {
                    s_bl.Volunteer.AddVolunteer(CurrentVolunteer!);
                    MessageBox.Show("Volunteer added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                else
                {
                    s_bl.Volunteer.UpdateVolunteerDetails(CurrentVolunteer!.Id, CurrentVolunteer!);
                    MessageBox.Show("Volunteer updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                UpdateVolunteerList();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred: {ex.ToString()}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
