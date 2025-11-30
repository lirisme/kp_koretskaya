using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class CertificateDataPage : Page
    {
        private readonly XmlDataService _dataService;
        private StudentCollection _students;
        private StudentCertificateCollection _certificates;
        private StudentCertificateCollection _filteredCertificates;
        private Student _selectedStudent;
        private VehicleCategoryCollection _categories;

        public CertificateDataPage(XmlDataService dataService)
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
                _certificates = _dataService.LoadCertificates();
                _categories = _dataService.LoadVehicleCategories();

                if (_students?.Students == null) _students = new StudentCollection();
                if (_certificates?.Certificates == null) _certificates = new StudentCertificateCollection();
                if (_categories?.Categories == null) _categories = new VehicleCategoryCollection();

                ApplyFilter();
                UpdateButtonsAvailability();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                _students = new StudentCollection();
                _certificates = new StudentCertificateCollection();
                _categories = new VehicleCategoryCollection();
                ApplyFilter();
                UpdateButtonsAvailability();
            }
        }

        private string GetStudentName(int studentId)
        {
            var student = _students.Students.FirstOrDefault(s => s.Id == studentId);
            return student?.FullName ?? "Неизвестный студент";
        }

        private string GetCategoryCode(int categoryId)
        {
            var category = _categories.Categories.FirstOrDefault(c => c.Id == categoryId);
            return category?.Code ?? "Неизвестно";
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
                _filteredCertificates = new StudentCertificateCollection
                {
                    Certificates = _certificates.Certificates
                        .Where(p => p.StudentId == _selectedStudent.Id)
                        .ToList()
                };
            }
            else
            {
                _filteredCertificates = new StudentCertificateCollection
                {
                    Certificates = _certificates.Certificates.ToList()
                };
            }

            UpdateCertificateData();

            CertificateGrid.ItemsSource = _filteredCertificates.Certificates;
            UpdateButtonsAvailability();
            UpdateStatus();
        }

        private void UpdateCertificateData()
        {
            foreach (var certificate in _filteredCertificates.Certificates)
            {
                certificate.StudentName = GetStudentName(certificate.StudentId);

                if (string.IsNullOrEmpty(certificate.CategoryCode))
                {
                    certificate.CategoryCode = GetCategoryCode(certificate.VehicleCategoryId);
                }
            }
        }

        private void UpdateButtonsAvailability()
        {
            if (_selectedStudent == null)
            {
                AddCertificateButton.IsEnabled = false;
                EditCertificateButton.IsEnabled = false;
                DeleteCertificateButton.IsEnabled = false;

                StatusPanel.Visibility = Visibility.Visible;
                StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 245, 233));
                StatusTextBlock.Text = $"Всего свидетельств в базе: {_certificates.Certificates.Count}. Выберите студента для работы с конкретным свидетельством.";
                return;
            }

            var hasCertificateData = _certificates.Certificates.Any(p => p.StudentId == _selectedStudent.Id);
            var isCertificateSelected = CertificateGrid.SelectedItem != null;

            AddCertificateButton.IsEnabled = !hasCertificateData;
            EditCertificateButton.IsEnabled = hasCertificateData && isCertificateSelected;
            DeleteCertificateButton.IsEnabled = hasCertificateData && isCertificateSelected;

            UpdateStatusPanel();
        }

        private void UpdateStatusPanel()
        {
            if (_selectedStudent == null)
            {
                StatusPanel.Visibility = Visibility.Collapsed;
                return;
            }

            var hasCertificateData = _certificates.Certificates.Any(p => p.StudentId == _selectedStudent.Id);

            if (hasCertificateData)
            {
                StatusPanel.Visibility = Visibility.Visible;
                StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 245, 233));
                StatusPanel.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 230, 201));
                StatusTextBlock.Text = $"✅ Для учащегося {_selectedStudent.FullName} внесены данные свидетельства. " +
                                      $"Выберите запись для редактирования или удаления.";
            }
            else
            {
                StatusPanel.Visibility = Visibility.Visible;
                StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 243, 205));
                StatusPanel.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 238, 186));
                StatusTextBlock.Text = $"ℹ️ Для учащегося {_selectedStudent.FullName} данные свидетельства отсутствуют. " +
                                      $"Нажмите 'Добавить свидетельство' для внесения данных.";
            }
        }

        private void CertificateGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                    var existingCertificate = _certificates.Certificates.FirstOrDefault(p => p.StudentId == _selectedStudent.Id);
                    var status = existingCertificate != null ? "✅ данные внесены" : "❌ данные отсутствуют";
                    mainWindow.StatusText.Text = $"Свидетельства: {_selectedStudent.FullName} ({status})";
                }
                else
                {
                    mainWindow.StatusText.Text = $"Свидетельства: всего {_certificates.Certificates.Count} записей";
                }
            }
        }

        private void AddCertificate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent != null)
            {
                var existingCertificate = _certificates.Certificates.FirstOrDefault(p => p.StudentId == _selectedStudent.Id);
                if (existingCertificate != null)
                {
                    MessageBox.Show($"Для учащегося {_selectedStudent.FullName} уже существуют данные свидетельства.\n\n" +
                                   $"Используйте функцию редактирования для изменения данных.",
                                   "Данные свидетельства уже существуют",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dialog = new CertificateEditDialog(_dataService, _selectedStudent.Id);
                if (dialog.ShowDialog() == true)
                {
                    LoadData();
                    MessageBox.Show($"Данные свидетельства успешно добавлены!", "Успех");
                }
            }
            else
            {
                MessageBox.Show("Выберите учащегося", "Предупреждение");
            }
        }

        private void EditCertificate_Click(object sender, RoutedEventArgs e)
        {
            if (CertificateGrid.SelectedItem is StudentCertificate selectedCertificate)
            {
                var dialog = new CertificateEditDialog(_dataService, selectedCertificate.StudentId, selectedCertificate);
                if (dialog.ShowDialog() == true)
                {
                    LoadData();
                    MessageBox.Show($"Данные свидетельства обновлены!", "Успех");
                }
            }
            else
            {
                MessageBox.Show("Выберите запись свидетельства для редактирования", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteCertificate_Click(object sender, RoutedEventArgs e)
        {
            if (CertificateGrid.SelectedItem is StudentCertificate selectedCertificate)
            {
                var student = _students.Students.FirstOrDefault(s => s.Id == selectedCertificate.StudentId);
                var studentName = student?.FullName ?? "неизвестный студент";

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить данные свидетельства?\n\n" +
                    $"Студент: {studentName}\n" +
                    $"Свидетельство: {selectedCertificate.CertificateSeries} {selectedCertificate.CertificateNumber}\n\n" +
                    $"Это действие нельзя отменить!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _certificates.Certificates.Remove(selectedCertificate);
                    _dataService.SaveCertificates(_certificates);
                    ApplyFilter();
                    MessageBox.Show($"Данные свидетельства удалены.", "Успех");
                }
            }
            else
            {
                MessageBox.Show("Выберите запись свидетельства для удаления", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ViewCertificate_Click(object sender, RoutedEventArgs e)
        {
            if (CertificateGrid.SelectedItem is StudentCertificate selectedCertificate)
            {
                var student = _students.Students.FirstOrDefault(s => s.Id == selectedCertificate.StudentId);
                var studentName = student?.FullName ?? "Неизвестный студент";

                MessageBox.Show($"Данные свидетельства:\n\n" +
                               $"Студент: {studentName}\n" +
                               $"Серия: {selectedCertificate.CertificateSeries}\n" +
                               $"Номер: {selectedCertificate.CertificateNumber}\n" +
                               $"Дата выдачи: {selectedCertificate.IssueDate:dd.MM.yyyy}\n" +
                               $"Категория: {selectedCertificate.CategoryCode}",
                               "Просмотр данных свидетельства");
            }
            else
            {
                MessageBox.Show("Выберите запись для просмотра", "Предупреждение");
            }
        }

        private void PrintCertificate_Click(object sender, RoutedEventArgs e)
        {
            if (CertificateGrid.SelectedItem is StudentCertificate selectedCertificate)
            {
                MessageBox.Show($"Печать данных свидетельства:\n{selectedCertificate.CertificateSeries} {selectedCertificate.CertificateNumber}",
                    "Печать");
            }
            else
            {
                MessageBox.Show("Выберите запись для печати", "Предупреждение");
            }
        }

        private void CertificateGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

            if (row != null && row.DataContext is StudentCertificate certificate)
            {
                EditCertificate_Click(sender, e);
            }
        }
    }
}