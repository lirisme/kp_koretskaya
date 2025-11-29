using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class PassportDataPage : Page
    {
        private readonly XmlDataService _dataService;
        private StudentCollection _students;
        private StudentPassportDataCollection _passports;
        private StudentPassportDataCollection _filteredPassports;
        private Student _selectedStudent;

        public PassportDataPage(XmlDataService dataService)
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
                _passports = _dataService.LoadPassportData();

                if (_students?.Students == null) _students = new StudentCollection();
                if (_passports?.Passports == null) _passports = new StudentPassportDataCollection();

                ApplyFilter();
                UpdateUIForSelectedStudent(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                _students = new StudentCollection();
                _passports = new StudentPassportDataCollection();
                ApplyFilter();
                UpdateUIForSelectedStudent(null);
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
                _filteredPassports = new StudentPassportDataCollection
                {
                    Passports = _passports.Passports
                        .Where(p => p.StudentId == _selectedStudent.Id)
                        .ToList()
                };
            }
            else
            {
                _filteredPassports = new StudentPassportDataCollection
                {
                    Passports = _passports.Passports.ToList()
                };
            }

            PassportGrid.ItemsSource = _filteredPassports.Passports;
            UpdateButtonsAvailability();
            UpdateStatus();
        }

        private void UpdateUIForSelectedStudent(Student student)
        {
            if (student == null)
            {
                StatusPanel.Visibility = Visibility.Collapsed;
                AddPassportButton.IsEnabled = false;
                EditPassportButton.IsEnabled = false;
                DeletePassportButton.IsEnabled = false;
                return;
            }

            var existingPassport = _passports.Passports.FirstOrDefault(p => p.StudentId == student.Id);

            if (existingPassport != null)
            {
                StatusPanel.Visibility = Visibility.Visible;
                StatusTextBlock.Text = $"Для учащегося {student.FullName} уже внесены паспортные данные. " +
                                      $"Используйте функции редактирования или удаления.";

                AddPassportButton.IsEnabled = false;
                EditPassportButton.IsEnabled = true;  
                DeletePassportButton.IsEnabled = true;
            }
            else
            {
                StatusPanel.Visibility = Visibility.Collapsed;

                AddPassportButton.IsEnabled = true;   
                EditPassportButton.IsEnabled = false; 
                DeletePassportButton.IsEnabled = false; 
            }
        }

        private void UpdateButtonsAvailability()
        {
            if (_selectedStudent == null)
            {
                AddPassportButton.IsEnabled = false;
                EditPassportButton.IsEnabled = false;
                DeletePassportButton.IsEnabled = false;

                StatusPanel.Visibility = Visibility.Visible;
                StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 245, 233));
                StatusTextBlock.Text = $"Всего паспортных данных в базе: {_passports.Passports.Count}. Выберите студента для работы с конкретными данными.";
                return;
            }

            var hasPassportData = _passports.Passports.Any(p => p.StudentId == _selectedStudent.Id);
            var isPassportSelected = PassportGrid.SelectedItem != null;

            AddPassportButton.IsEnabled = !hasPassportData;
            EditPassportButton.IsEnabled = hasPassportData && isPassportSelected;
            DeletePassportButton.IsEnabled = hasPassportData && isPassportSelected;

            UpdateStatusPanel();
        }

        private void UpdateStatusPanel()
        {
            if (_selectedStudent == null)
            {
                StatusPanel.Visibility = Visibility.Collapsed;
                return;
            }

            var hasPassportData = _passports.Passports.Any(p => p.StudentId == _selectedStudent.Id);

            if (hasPassportData)
            {
                StatusPanel.Visibility = Visibility.Visible;
                StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 245, 233)); 
                StatusPanel.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 230, 201));
                StatusTextBlock.Text = $"✅ Для учащегося {_selectedStudent.FullName} внесены паспортные данные. " +
                                      $"Выберите запись для редактирования или удаления.";
            }
            else
            {
                StatusPanel.Visibility = Visibility.Visible;
                StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 243, 205)); 
                StatusPanel.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 238, 186));
                StatusTextBlock.Text = $"ℹ️ Для учащегося {_selectedStudent.FullName} паспортные данные отсутствуют. " +
                                      $"Нажмите 'Добавить паспорт' для внесения данных.";
            }
        }

        private void PassportGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                    var existingPassport = _passports.Passports.FirstOrDefault(p => p.StudentId == _selectedStudent.Id);
                    var status = existingPassport != null ? "✅ данные внесены" : "❌ данные отсутствуют";
                    mainWindow.StatusText.Text = $"Паспортные данные: {_selectedStudent.FullName} ({status})";
                }
                else
                {
                    mainWindow.StatusText.Text = $"Паспортные данные: всего {_passports.Passports.Count} записей";
                }
            }
        }

        private void AddPassport_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent != null)
            {
                var existingPassport = _passports.Passports.FirstOrDefault(p => p.StudentId == _selectedStudent.Id);
                if (existingPassport != null)
                {
                    MessageBox.Show($"Для учащегося {_selectedStudent.FullName} уже существуют паспортные данные.\n\n" +
                                   $"Используйте функцию редактирования для изменения данных.",
                                   "Паспортные данные уже существуют",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dialog = new PassportEditDialog(_dataService, _selectedStudent.Id);
                if (dialog.ShowDialog() == true)
                {
                    _passports.Passports.Add(dialog.PassportData);
                    _dataService.SavePassportData(_passports);
                    ApplyFilter();

                    MessageBox.Show($"Паспортные данные успешно добавлены!", "Успех");
                }
            }
            else
            {
                MessageBox.Show("Выберите учащегося", "Предупреждение");
            }
        }

        private void EditPassport_Click(object sender, RoutedEventArgs e)
        {
            if (PassportGrid.SelectedItem is StudentPassportData selectedPassport)
            {
                var dialog = new PassportEditDialog(_dataService, selectedPassport.StudentId, selectedPassport);
                if (dialog.ShowDialog() == true)
                {
                    var index = _passports.Passports.IndexOf(selectedPassport);
                    if (index >= 0)
                    {
                        _passports.Passports[index] = dialog.PassportData;
                        _dataService.SavePassportData(_passports);
                        ApplyFilter();
                        MessageBox.Show($"Паспортные данные обновлены!", "Успех");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите запись паспортных данных для редактирования", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeletePassport_Click(object sender, RoutedEventArgs e)
        {
            if (PassportGrid.SelectedItem is StudentPassportData selectedPassport)
            {
                var student = _students.Students.FirstOrDefault(s => s.Id == selectedPassport.StudentId);
                var studentName = student?.FullName ?? "неизвестный студент";

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить паспортные данные?\n\n" +
                    $"Студент: {studentName}\n" +
                    $"Документ: {selectedPassport.DocumentType} {selectedPassport.Series} {selectedPassport.Number}\n\n" +
                    $"Это действие нельзя отменить!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _passports.Passports.Remove(selectedPassport);
                    _dataService.SavePassportData(_passports);
                    ApplyFilter();
                    MessageBox.Show($"Паспортные данные удалены.", "Успех");
                }
            }
            else
            {
                MessageBox.Show("Выберите запись паспортных данных для удаления", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ViewPassport_Click(object sender, RoutedEventArgs e)
        {
            if (PassportGrid.SelectedItem is StudentPassportData selectedPassport)
            {
                var student = _students.Students.FirstOrDefault(s => s.Id == selectedPassport.StudentId);
                var studentName = student?.FullName ?? "Неизвестный студент";

                MessageBox.Show($"Паспортные данные:\n\n" +
                               $"Студент: {studentName}\n" +
                               $"Тип документа: {selectedPassport.DocumentType}\n" +
                               $"Серия: {selectedPassport.Series}\n" +
                               $"Номер: {selectedPassport.Number}\n" +
                               $"Кем выдан: {selectedPassport.IssuedBy}\n" +
                               $"Код подразделения: {selectedPassport.DivisionCode}\n" +
                               $"Дата выдачи: {selectedPassport.IssueDate:dd.MM.yyyy}",
                               "Просмотр паспортных данных");
            }
            else
            {
                MessageBox.Show("Выберите запись для просмотра", "Предупреждение");
            }
        }

        private void PrintPassport_Click(object sender, RoutedEventArgs e)
        {
            if (PassportGrid.SelectedItem is StudentPassportData selectedPassport)
            {
                MessageBox.Show($"Печать паспортных данных:\n{selectedPassport.DocumentType} {selectedPassport.Series} {selectedPassport.Number}",
                    "Печать");
            }
            else
            {
                MessageBox.Show("Выберите запись для печати", "Предупреждение");
            }
        }

        private void PassportGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

            if (row != null && row.DataContext is StudentPassportData passport)
            {
                EditPassport_Click(sender, e);
            }
        }
    }
}