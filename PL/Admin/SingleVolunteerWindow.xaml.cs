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
        private volatile bool _isUpdating = false;

        private DispatcherTimer _timer;

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

        private BO.Volunteer _currentVolunteer;
        public BO.Volunteer CurrentVolunteer
        {
            get => _currentVolunteer;
            set
            {
                if (_currentVolunteer != value)
                {
                    _currentVolunteer = value;
                    OnPropertyChanged(nameof(CurrentVolunteer));
                    OnPropertyChanged(nameof(CurrentVolunteer.Id));
                    OnPropertyChanged(nameof(CurrentVolunteer.FullName));
                    OnPropertyChanged(nameof(CurrentVolunteer.PhoneNumber));
                    OnPropertyChanged(nameof(CurrentVolunteer.Email));
                    OnPropertyChanged(nameof(CurrentVolunteer.Active));
                    OnPropertyChanged(nameof(CurrentVolunteer.Position));
                    OnPropertyChanged(nameof(CurrentVolunteer.DistanceType));
                    OnPropertyChanged(nameof(CurrentVolunteer.MaxDistance));
                    OnPropertyChanged(nameof(CurrentVolunteer.VolunteerTakenCare));
                }
            }
        }

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
                    StartAutoRefresh(); // מתחיל את הרענון האוטומטי
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartAutoRefresh()
        {
            if (_timer != null) return;

            _timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                var updatedVolunteer = s_bl.Volunteer.RequestVolunteerDetails(CurrentVolunteer.Id);
                if (updatedVolunteer != null)
                {
                    CurrentVolunteer = updatedVolunteer;
                    if (updatedVolunteer.VolunteerTakenCare != null)
                    {
                        VolunteerTakenCare.Clear();
                        VolunteerTakenCare.Add(updatedVolunteer.VolunteerTakenCare);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating volunteer: {ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadVolunteerTakenCare()
        {
            if (CurrentVolunteer?.Id == null || CurrentVolunteer.Id == 0) return;

            try
            {
                var volunteerDetails = s_bl.Volunteer.RequestVolunteerDetails(CurrentVolunteer.Id);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (volunteerDetails != null && !_isUpdating)
                    {
                        _isUpdating = true;
                        try
                        {
                            // עדכון רק אם יש שינוי בנתונים
                            if (!AreVolunteersEqual(CurrentVolunteer, volunteerDetails))
                            {
                                CurrentVolunteer = volunteerDetails;
                                VolunteerTakenCare.Clear();
                                if (volunteerDetails.VolunteerTakenCare != null)
                                {
                                    VolunteerTakenCare.Add(volunteerDetails.VolunteerTakenCare);
                                }
                                OnPropertyChanged(nameof(VolunteerTakenCare));
                            }
                        }
                        finally
                        {
                            _isUpdating = false;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error updating volunteer data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer?.Id > 0)
            {
                // נרשם לעדכונים ספציפיים למתנדב הזה
                s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, volunteerListObserver);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
            if (CurrentVolunteer?.Id > 0)
            {
                // מסיר את הרישום לעדכונים
                s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, volunteerListObserver);
            }
        }

        private void volunteerListObserver()
        {
            if (_isUpdating) return;
            
            _isUpdating = true;
            
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    // מבקש את הפרטים העדכניים של המתנדב
                    var updatedVolunteer = s_bl.Volunteer.RequestVolunteerDetails(CurrentVolunteer.Id);
                    if (updatedVolunteer != null)
                    {
                        // מעדכן את המתנדב בממשק
                        CurrentVolunteer = updatedVolunteer;

                        // מעדכן את רשימת הקריאות
                        VolunteerTakenCare.Clear();
                        if (updatedVolunteer.VolunteerTakenCare != null)
                        {
                            VolunteerTakenCare.Add(updatedVolunteer.VolunteerTakenCare);
                        }

                        // מעדכן את הממשק
                        OnPropertyChanged(nameof(CurrentVolunteer));
                        OnPropertyChanged(nameof(VolunteerTakenCare));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating volunteer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    _isUpdating = false;
                }
            }));
        }

        private bool AreVolunteersEqual(BO.Volunteer v1, BO.Volunteer v2)
        {
            if (v1 == null || v2 == null) return false;
            return v1.Id == v2.Id &&
                   v1.FullName == v2.FullName &&
                   v1.PhoneNumber == v2.PhoneNumber &&
                   v1.Email == v2.Email &&
                   v1.Active == v2.Active &&
                   v1.Position == v2.Position &&
                   v1.DistanceType == v2.DistanceType &&
                   v1.MaxDistance == v2.MaxDistance &&
                   (v1.VolunteerTakenCare?.CallId ?? 0) == (v2.VolunteerTakenCare?.CallId ?? 0);
        }

        private void UpdateVolunteerList()
        {
            if (_isUpdating) return;

            _isUpdating = true;

            try
            {
                Task.Run(() =>
                {
                    try
                    {
                        var volunteers = queryVolunteerList();

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                // Update the volunteer list
                                var selectedVolunteer = volunteers.FirstOrDefault();
                                if (selectedVolunteer != null)
                                {
                                    CurrentVolunteer = s_bl.Volunteer.RequestVolunteerDetails(selectedVolunteer.Id);
                                    LoadVolunteerTakenCare();
                                }
                            }
                            finally
                            {
                                _isUpdating = false;
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Error updating volunteer data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            _isUpdating = false;
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error updating volunteer data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _isUpdating = false;
                });
            }
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
                Close();
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
