using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class DrivingLicenseDataPage : Page
    {
        private readonly XmlDataService _dataService;
        private StudentCollection _students;
        private StudentDrivingLicenseCollection _licenses;
        private StudentDrivingLicenseCollection _filteredLicenses;
        private Student _selectedStudent;

        public DrivingLicenseDataPage(XmlDataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _students = _dataService.LoadStudents();
                _licenses = _dataService.LoadDrivingLicenses();

                if (_students?.Students == null) _students = new StudentCollection();
                if (_licenses?.Licenses == null) _licenses = new StudentDrivingLicenseCollection();

                UpdateButtonsAvailability();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                _students = new StudentCollection();
                _licenses = new StudentDrivingLicenseCollection();
                UpdateButtonsAvailability();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text?.ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                SearchResultsListBox.Visibility = Visibility.Collapsed;
                return;
            }

            var results = _students.Students
                .Where(s => (s.FullName ?? "").ToLower().Contains(searchText) ||
                           (s.Phone ?? "").Contains(searchText) ||
                           s.Id.ToString().Contains(searchText))
                .Take(10)
                .ToList();

            if (results.Any())
            {
                SearchResultsListBox.ItemsSource = results;
                SearchResultsListBox.Visibility = Visibility.Visible;
            }
            else
            {
                SearchResultsListBox.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsListBox.SelectedItem is Student selectedStudent)
            {
                _selectedStudent = selectedStudent;
                UpdateSelectedStudentPanel();
                SearchResultsListBox.Visibility = Visibility.Collapsed;
                SearchTextBox.Text = string.Empty;
                ApplyFilter();
            }
        }

        private void UpdateSelectedStudentPanel()
        {
            if (_selectedStudent != null)
            {
                SelectedStudentPanel.Visibility = Visibility.Visible;
                SelectedStudentText.Text = _selectedStudent.FullName;
                SelectedStudentDetails.Text = $"Телефон: {_selectedStudent.Phone} | " +
                                            $"Дата рождения: {_selectedStudent.BirthDate:dd.MM.yyyy} | " +
                                            $"ID: {_selectedStudent.Id}";
            }
            else
            {
                SelectedStudentPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ClearSelectedStudent_Click(object sender, RoutedEventArgs e)
        {
            _selectedStudent = null;
            UpdateSelectedStudentPanel();
            ApplyFilter();
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            SearchResultsListBox.Visibility = Visibility.Collapsed;
        }

        private void ApplyFilter()
        {
            if (_selectedStudent != null)
            {
                _filteredLicenses = new StudentDrivingLicenseCollection
                {
                    Licenses = _licenses.Licenses
                        .Where(p => p.StudentId == _selectedStudent.Id)
                        .ToList()
                };
            }
            else
            {
                _filteredLicenses = new StudentDrivingLicenseCollection();
            }

            LicenseGrid.ItemsSource = _filteredLicenses.Licenses;
            UpdateButtonsAvailability();
            UpdateStatus();
        }

        private void UpdateButtonsAvailability()
        {
            if (_selectedStudent == null)
            {
                AddLicenseButton.IsEnabled = false;
                EditLicenseButton.IsEnabled = false;
                DeleteLicenseButton.IsEnabled = false;
                return;
            }

            var hasLicenseData = _licenses.Licenses.Any(p => p.StudentId == _selectedStudent.Id);
            var isLicenseSelected = LicenseGrid.SelectedItem != null;

            AddLicenseButton.IsEnabled = !hasLicenseData;
            EditLicenseButton.IsEnabled = hasLicenseData && isLicenseSelected;
            DeleteLicenseButton.IsEnabled = hasLicenseData && isLicenseSelected;

            UpdateStatusPanel();
        }

        private void UpdateStatusPanel()
        {
            if (_selectedStudent == null)
            {
                StatusPanel.Visibility = Visibility.Collapsed;
                return;
            }

            var hasLicenseData = _licenses.Licenses.Any(p => p.StudentId == _selectedStudent.Id);

            if (hasLicenseData)
            {
                StatusPanel.Visibility = Visibility.Visible;
                StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 245, 233));
                StatusPanel.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 230, 201));
                StatusTextBlock.Text = $"✅ Для учащегося {_selectedStudent.FullName} внесены данные водительского удостоверения. " +
                                      $"Выберите запись для редактирования или удаления.";
            }
            else
            {
                StatusPanel.Visibility = Visibility.Visible;
                StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 243, 205));
                StatusPanel.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 238, 186));
                StatusTextBlock.Text = $"ℹ️ Для учащегося {_selectedStudent.FullName} данные водительского удостоверения отсутствуют. " +
                                      $"Нажмите 'Добавить удостоверение' для внесения данных.";
            }
        }

        private void LicenseGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonsAvailability();
        }

        private void UpdateStatus()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                if (_selectedStudent != null)
                {
                    var existingLicense = _licenses.Licenses.FirstOrDefault(p => p.StudentId == _selectedStudent.Id);
                    var status = existingLicense != null ? "✅ данные внесены" : "❌ данные отсутствуют";
                    mainWindow.StatusText.Text = $"Водительские удостоверения: {_selectedStudent.FullName} ({status})";
                }
                else
                {
                    mainWindow.StatusText.Text = "Водительские удостоверения: выберите учащегося";
                }
            }
        }

        private void AddLicense_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent != null)
            {
                var existingLicense = _licenses.Licenses.FirstOrDefault(p => p.StudentId == _selectedStudent.Id);
                if (existingLicense != null)
                {
                    MessageBox.Show($"Для учащегося {_selectedStudent.FullName} уже существуют данные водительского удостоверения.\n\n" +
                                   $"Используйте функцию редактирования для изменения данных.",
                                   "Данные водительского удостоверения уже существуют",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dialog = new DrivingLicenseEditDialog(_dataService, _selectedStudent.Id);
                if (dialog.ShowDialog() == true)
                {
                    _licenses.Licenses.Add(dialog.LicenseData);
                    _dataService.SaveDrivingLicenses(_licenses);
                    ApplyFilter();
                    MessageBox.Show($"Данные водительского удостоверения успешно добавлены!", "Успех");
                }
            }
            else
            {
                MessageBox.Show("Выберите учащегося", "Предупреждение");
            }
        }

        private void EditLicense_Click(object sender, RoutedEventArgs e)
        {
            if (LicenseGrid.SelectedItem is StudentDrivingLicense selectedLicense)
            {
                var dialog = new DrivingLicenseEditDialog(_dataService, selectedLicense.StudentId, selectedLicense);
                if (dialog.ShowDialog() == true)
                {
                    var index = _licenses.Licenses.IndexOf(selectedLicense);
                    if (index >= 0)
                    {
                        _licenses.Licenses[index] = dialog.LicenseData;
                        _dataService.SaveDrivingLicenses(_licenses);
                        ApplyFilter();
                        MessageBox.Show($"Данные водительского удостоверения обновлены!", "Успех");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите запись водительского удостоверения для редактирования", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteLicense_Click(object sender, RoutedEventArgs e)
        {
            if (LicenseGrid.SelectedItem is StudentDrivingLicense selectedLicense)
            {
                var student = _students.Students.FirstOrDefault(s => s.Id == selectedLicense.StudentId);
                var studentName = student?.FullName ?? "неизвестный студент";

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить данные водительского удостоверения?\n\n" +
                    $"Студент: {studentName}\n" +
                    $"Удостоверение: {selectedLicense.Series} {selectedLicense.Number}\n\n" +
                    $"Это действие нельзя отменить!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _licenses.Licenses.Remove(selectedLicense);
                    _dataService.SaveDrivingLicenses(_licenses);
                    ApplyFilter();
                    MessageBox.Show($"Данные водительского удостоверения удалены.", "Успех");
                }
            }
            else
            {
                MessageBox.Show("Выберите запись водительского удостоверения для удаления", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ViewLicense_Click(object sender, RoutedEventArgs e)
        {
            if (LicenseGrid.SelectedItem is StudentDrivingLicense selectedLicense)
            {
                var student = _students.Students.FirstOrDefault(s => s.Id == selectedLicense.StudentId);
                var studentName = student?.FullName ?? "Неизвестный студент";

                MessageBox.Show($"Данные водительского удостоверения:\n\n" +
                               $"Студент: {studentName}\n" +
                               $"Серия: {selectedLicense.Series}\n" +
                               $"Номер: {selectedLicense.Number}\n" +
                               $"Категории: {selectedLicense.Categories}\n" +
                               $"Кем выдан: {selectedLicense.IssuedBy}\n" +
                               $"Код подразделения: {selectedLicense.DivisionCode}\n" +
                               $"Дата выдачи: {selectedLicense.IssueDate:dd.MM.yyyy}\n" +
                               $"Срок действия: {selectedLicense.ExpiryDate:dd.MM.yyyy}\n" +
                               $"Стаж: {selectedLicense.ExperienceYears} лет\n" +
                               $"Статус: {selectedLicense.Status}",
                               "Просмотр данных водительского удостоверения");
            }
            else
            {
                MessageBox.Show("Выберите запись для просмотра", "Предупреждение");
            }
        }

        private void PrintLicense_Click(object sender, RoutedEventArgs e)
        {
            if (LicenseGrid.SelectedItem is StudentDrivingLicense selectedLicense)
            {
                MessageBox.Show($"Печать данных водительского удостоверения:\n{selectedLicense.Series} {selectedLicense.Number}",
                    "Печать");
            }
            else
            {
                MessageBox.Show("Выберите запись для печати", "Предупреждение");
            }
        }
    }
}