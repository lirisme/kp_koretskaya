using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class MedicalPage : Page
    {
        private readonly XmlDataService _dataService;
        private StudentCollection _students;
        private StudentMedicalCertificateCollection _medicalList;
        private Student _selectedStudent;

        public MedicalPage(XmlDataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            LoadData();

            UpdateButtonsAvailability();
        }

        private void LoadData()
        {
            try
            {
                _students = _dataService.LoadStudents();
                _medicalList = _dataService.LoadMedicalData();

                LoadMedicalDataForStudent();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                _students = new StudentCollection();
                _medicalList = new StudentMedicalCertificateCollection();
                LoadMedicalDataForStudent();
            }
        }

        private string GetStudentName(int studentId)
        {
            var student = _students.Students.FirstOrDefault(s => s.Id == studentId);
            return student?.FullName ?? "Неизвестный студент";
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
                LoadMedicalDataForStudent();
            }
        }

        private void UpdateSelectedStudentPanel()
        {
            if (_selectedStudent != null)
            {
                SelectedStudentPanel.Visibility = Visibility.Visible;
                SelectedStudentText.Text = _selectedStudent.FullName;
                SelectedStudentDetails.Text = $"Телефон: {_selectedStudent.Phone} | Дата рождения: {_selectedStudent.BirthDate:dd.MM.yyyy}";
            }
            else
            {
                SelectedStudentPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadMedicalDataForStudent()
        {
            if (_selectedStudent != null)
            {
                var studentMedical = _medicalList.Certificates
                    .Where(m => m.StudentId == _selectedStudent.Id)
                    .ToList();

                MedicalGrid.ItemsSource = studentMedical;

                if (studentMedical.Any())
                {
                    StatusTextBlock.Text = $"✅ Для учащегося {_selectedStudent.FullName} внесены медицинские справки";
                    StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 245, 233));
                }
                else
                {
                    StatusTextBlock.Text = $"ℹ️ Для учащегося {_selectedStudent.FullName} медицинские справки отсутствуют";
                    StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 243, 205));
                }
            }
            else
            {
                MedicalGrid.ItemsSource = _medicalList.Certificates.ToList();

                StatusTextBlock.Text = $"Всего медицинских справок в базе: {_medicalList.Certificates.Count}. Выберите студента для работы с конкретной справкой.";
                StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 245, 233));
            }

            UpdateStudentNames();
            UpdateButtonsAvailability();
        }

        private void UpdateStudentNames()
        {
            var items = MedicalGrid.ItemsSource as System.Collections.IEnumerable;
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item is StudentMedicalCertificate medical)
                    {
                        medical.StudentName = GetStudentName(medical.StudentId);
                    }
                }
            }
        }

        private void UpdateButtonsAvailability()
        {
            if (_selectedStudent == null)
            {
                AddMedicalButton.IsEnabled = false;
                EditMedicalButton.IsEnabled = false;
                DeleteMedicalButton.IsEnabled = false;

                AddMedicalButton.Opacity = 0.5;
                EditMedicalButton.Opacity = 0.5;
                DeleteMedicalButton.Opacity = 0.5;
                return;
            }

            bool hasStudent = _selectedStudent != null;
            bool hasMedical = hasStudent && _medicalList.Certificates.Any(m => m.StudentId == _selectedStudent.Id);
            bool hasSelection = MedicalGrid.SelectedItem != null;

            AddMedicalButton.IsEnabled = hasStudent && !hasMedical;
            EditMedicalButton.IsEnabled = hasStudent && hasMedical && hasSelection;
            DeleteMedicalButton.IsEnabled = hasStudent && hasMedical && hasSelection;

            AddMedicalButton.Opacity = AddMedicalButton.IsEnabled ? 1.0 : 0.5;
            EditMedicalButton.Opacity = EditMedicalButton.IsEnabled ? 1.0 : 0.5;
            DeleteMedicalButton.Opacity = DeleteMedicalButton.IsEnabled ? 1.0 : 0.5;
        }

        private void ClearSelectedStudent_Click(object sender, RoutedEventArgs e)
        {
            _selectedStudent = null;
            UpdateSelectedStudentPanel();
            LoadMedicalDataForStudent();
            StatusTextBlock.Text = "Выберите учащегося для работы с медицинскими справками";
            StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 243, 205));
            UpdateButtonsAvailability();
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            SearchResultsListBox.Visibility = Visibility.Collapsed;
        }

        private void MedicalGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonsAvailability();
        }

        private void AddMedical_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent != null)
            {
                var dialog = new MedicalEditDialog(_dataService, _selectedStudent.Id);
                if (dialog.ShowDialog() == true)
                {
                    LoadData();
                    MessageBox.Show("Медицинская справка добавлена!", "Успех");
                }
            }
        }

        private void EditMedical_Click(object sender, RoutedEventArgs e)
        {
            if (MedicalGrid.SelectedItem is StudentMedicalCertificate selectedMedical)
            {
                var dialog = new MedicalEditDialog(_dataService, selectedMedical.StudentId, selectedMedical);
                if (dialog.ShowDialog() == true)
                {
                    LoadData();
                    MessageBox.Show("Медицинская справка обновлена!", "Успех");
                }
            }
        }

        private void DeleteMedical_Click(object sender, RoutedEventArgs e)
        {
            if (MedicalGrid.SelectedItem is StudentMedicalCertificate selectedMedical)
            {
                var result = MessageBox.Show("Удалить медицинскую справку?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _medicalList.Certificates.Remove(selectedMedical);
                    _dataService.SaveMedicalData(_medicalList);
                    LoadMedicalDataForStudent();
                    MessageBox.Show("Медицинская справка удалена!", "Успех");
                }
            }
        }

        private void ViewMedical_Click(object sender, RoutedEventArgs e)
        {
            if (MedicalGrid.SelectedItem is StudentMedicalCertificate selectedMedical)
            {
                var student = _students.Students.FirstOrDefault(s => s.Id == selectedMedical.StudentId);
                var studentName = student?.FullName ?? "Неизвестный студент";

                MessageBox.Show($"Медицинская справка:\n\n" +
                               $"Студент: {studentName}\n" +
                               $"Серия: {selectedMedical.Series}\n" +
                               $"Номер: {selectedMedical.Number}\n" +
                               $"Мед. учреждение: {selectedMedical.MedicalInstitution}\n" +
                               $"Дата выдачи: {selectedMedical.IssueDate:dd.MM.yyyy}\n" +
                               $"Действует до: {selectedMedical.ValidUntil:dd.MM.yyyy}\n" +
                               $"Регион: {selectedMedical.Region}",
                               "Просмотр медицинской справки");
            }
        }

        private void PrintMedical_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция печати в разработке", "Информация");
        }

        private void MedicalGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

            if (row != null && row.DataContext is StudentMedicalCertificate medical)
            {
                EditMedical_Click(sender, e);
            }
        }
    }
}