using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BO;
using static BO.Enums;

namespace PL.Admin
{
    public partial class SingleVolunteerWindow : Window, INotifyPropertyChanged
    {
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private VolunteerInListField _selectedVolunteerField = VolunteerInListField.None;

        public ObservableCollection<CallInProgress> VolunteerTakenCare { get; set; } = new();

        public string ButtonText { get; set; }

        public VolunteerInListField SelectedFilter
        {
            get => _selectedVolunteerField;
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

        private bool _isIdEnabled;
        public bool IsIdEnabled
        {
            get => _isIdEnabled;
            set
            {
                if (_isIdEnabled != value)
                {
                    _isIdEnabled = value;
                    OnPropertyChanged(nameof(IsIdEnabled));
                }
            }
        }

        // Flag for handling updates asynchronously
        private volatile bool _isUpdateInProgress = false;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IEnumerable<BO.VolunteerInList> VolunteerInList
        {
            get => (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerInListFieldListProperty);
            set => SetValue(VolunteerInListFieldListProperty, value);
        }

        public static readonly DependencyProperty VolunteerInListFieldListProperty =
           DependencyProperty.Register("VolunteerInList", typeof(IEnumerable<BO.VolunteerInList>), typeof(SingleVolunteerWindow), new PropertyMetadata(null));

        public SingleVolunteerWindow(int id = 0)
        {
            ButtonText = id == 0 ? "Add" : "Update";
            InitializeComponent();
            this.DataContext = this;

            try
            {
                if (id == 0)
                {
                    CurrentVolunteer = new BO.Volunteer();
                    IsIdEnabled = true;
                }
                else
                {
                    CurrentVolunteer = s_bl.Volunteer.RequestVolunteerDetails(id);
                    IsIdEnabled = false;
                    LoadVolunteerTakenCare();
                }

                UpdateVolunteerList();

                _timer = new DispatcherTimer(DispatcherPriority.Background);
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DispatcherTimer _timer;
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Logic to execute every time the timer ticks
            LoadVolunteerTakenCare(); // Refresh the call list every second (or implement specific behavior)
        }

    private void LoadVolunteerTakenCare()
{
    if (CurrentVolunteer == null) return;

    var volunteerDetails = s_bl.Volunteer.RequestVolunteerDetails(CurrentVolunteer.Id);
    var calls = volunteerDetails?.VolunteerTakenCare;

    //Application.Current.Dispatcher.Invoke(() =>
    //{
    //    VolunteerTakenCare.Clear();
    //    if (calls != null)
    //    {
    //        if(calls!=null)
    //        {
    //            VolunteerTakenCare.Add(calls);
    //        }
    //    }
    //    OnPropertyChanged(nameof(VolunteerTakenCare));
    //});
}


        private void UpdateVolunteerList()
        {
            try
            {
                // Update asynchronously with flag check
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (_isUpdateInProgress) return;

                    _isUpdateInProgress = true;
                    VolunteerInList = queryVolunteerList();
                    _isUpdateInProgress = false;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading the volunteer list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BlApi.Factory.Get().Volunteer.AddObserver(volunteerListObserver);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            BlApi.Factory.Get().Volunteer.RemoveObserver(volunteerListObserver);
        }

        private volatile bool isUpdating = false;
        private void volunteerListObserver()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                // אם יש עדכון שממתין, לא נבצע עדכון נוסף
                if (isUpdating)
                    return;

                isUpdating = true;
                try
                {
                    // הוספתי קריאה ל-Dispatcher.Invoke לעדכון סטטוס בצורה בטוחה
                    UpdateVolunteerList();
                }
                finally
                {
                    isUpdating = false;
                }
            }));
        }

        private IEnumerable<BO.VolunteerInList> queryVolunteerList()
        {
            return SelectedFilter switch
            {
                VolunteerInListField.Id => s_bl.Volunteer.RequestVolunteerList(null, VolunteerInListField.Id).OrderBy(v => v.Id),
                VolunteerInListField.FullName => s_bl.Volunteer.RequestVolunteerList(null, VolunteerInListField.FullName).OrderBy(v => v.FullName),
                VolunteerInListField.Active => s_bl.Volunteer.RequestVolunteerList(true, VolunteerInListField.Active).Where(v => v.Active),
                _ => s_bl.Volunteer.RequestVolunteerList(null, null)
            };
        }

        public BO.Volunteer? CurrentVolunteer
        {
            get => (BO.Volunteer?)GetValue(CurrentVolunteerProperty);
            set
            {
                SetValue(CurrentVolunteerProperty, value);
                LoadVolunteerTakenCare();
            }
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
                    Close();
                }
                else
                {
                    s_bl.Volunteer.UpdateVolunteerDetails(CurrentVolunteer!.Id, CurrentVolunteer!);
                    MessageBox.Show("Volunteer updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }

                UpdateVolunteerList();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle selection change if necessary
        }
    }
}
