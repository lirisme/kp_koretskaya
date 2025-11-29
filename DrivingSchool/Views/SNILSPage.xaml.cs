using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class SNILSPage : Page
    {
        private readonly XmlDataService _dataService;
        private StudentCollection _students;
        private StudentSNILSCollection _snilsList;
        private Student _selectedStudent;

        public SNILSPage(XmlDataService dataService)
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
                _snilsList = _dataService.LoadSNILSData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                _students = new StudentCollection();
                _snilsList = new StudentSNILSCollection();
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
                LoadSNILSDataForStudent();
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

        private void LoadSNILSDataForStudent()
        {
            if (_selectedStudent != null)
            {
                var studentSNILS = _snilsList.SNILSList
                    .Where(s => s.StudentId == _selectedStudent.Id)
                    .ToList();

                SNILSGrid.ItemsSource = studentSNILS;

                if (studentSNILS.Any())
                {
                    StatusTextBlock.Text = $"✅ Для учащегося {_selectedStudent.FullName} внесены данные СНИЛС";
                    StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 245, 233));
                }
                else
                {
                    StatusTextBlock.Text = $"ℹ️ Для учащегося {_selectedStudent.FullName} данные СНИЛС отсутствуют";
                    StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 243, 205));
                }
            }

            UpdateButtonsAvailability();
        }

        private void UpdateButtonsAvailability()
        {
            bool hasStudent = _selectedStudent != null;
            bool hasSNILS = hasStudent && _snilsList.SNILSList.Any(s => s.StudentId == _selectedStudent.Id);
            bool hasSelection = SNILSGrid.SelectedItem != null;

            AddSNILSButton.IsEnabled = hasStudent && !hasSNILS;
            EditSNILSButton.IsEnabled = hasStudent && hasSNILS && hasSelection;
            DeleteSNILSButton.IsEnabled = hasStudent && hasSNILS && hasSelection;

            AddSNILSButton.Opacity = AddSNILSButton.IsEnabled ? 1.0 : 0.5;
            EditSNILSButton.Opacity = EditSNILSButton.IsEnabled ? 1.0 : 0.5;
            DeleteSNILSButton.Opacity = DeleteSNILSButton.IsEnabled ? 1.0 : 0.5;
        }

        private void ClearSelectedStudent_Click(object sender, RoutedEventArgs e)
        {
            _selectedStudent = null;
            UpdateSelectedStudentPanel();
            SNILSGrid.ItemsSource = null;
            StatusTextBlock.Text = "Выберите учащегося для работы с данными СНИЛС";
            StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 243, 205));
            UpdateButtonsAvailability();
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            SearchResultsListBox.Visibility = Visibility.Collapsed;
        }

        private void SNILSGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonsAvailability();
        }

        private void AddSNILS_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent != null)
            {
                var dialog = new SNILSEditDialog(_dataService, _selectedStudent.Id);
                if (dialog.ShowDialog() == true)
                {
                    _snilsList.SNILSList.Add(dialog.SNILSData);
                    _dataService.SaveSNILSData(_snilsList);
                    LoadSNILSDataForStudent();
                    MessageBox.Show("Данные СНИЛС добавлены!", "Успех");
                }
            }
        }

        private void EditSNILS_Click(object sender, RoutedEventArgs e)
        {
            if (SNILSGrid.SelectedItem is StudentSNILS selectedSNILS)
            {
                var dialog = new SNILSEditDialog(_dataService, selectedSNILS.StudentId, selectedSNILS);
                if (dialog.ShowDialog() == true)
                {
                    var index = _snilsList.SNILSList.IndexOf(selectedSNILS);
                    if (index >= 0)
                    {
                        _snilsList.SNILSList[index] = dialog.SNILSData;
                        _dataService.SaveSNILSData(_snilsList);
                        LoadSNILSDataForStudent();
                        MessageBox.Show("Данные СНИЛС обновлены!", "Успех");
                    }
                }
            }
        }

        private void DeleteSNILS_Click(object sender, RoutedEventArgs e)
        {
            if (SNILSGrid.SelectedItem is StudentSNILS selectedSNILS)
            {
                var result = MessageBox.Show("Удалить данные СНИЛС?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _snilsList.SNILSList.Remove(selectedSNILS);
                    _dataService.SaveSNILSData(_snilsList);
                    LoadSNILSDataForStudent();
                    MessageBox.Show("Данные СНИЛС удалены!", "Успех");
                }
            }
        }

        private void ViewSNILS_Click(object sender, RoutedEventArgs e)
        {
            if (SNILSGrid.SelectedItem is StudentSNILS selectedSNILS)
            {
                var student = _students.Students.FirstOrDefault(s => s.Id == selectedSNILS.StudentId);
                var studentName = student?.FullName ?? "Неизвестный студент";

                MessageBox.Show($"Данные СНИЛС:\n\n" +
                               $"Студент: {studentName}\n" +
                               $"Номер СНИЛС: {selectedSNILS.Number}\n" +
                               $"Дата выдачи: {selectedSNILS.IssueDate:dd.MM.yyyy}\n" +
                               $"Кем выдан: {selectedSNILS.IssuedBy}",
                               "Просмотр данных СНИЛС");
            }
        }

        private void PrintSNILS_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция печати в разработке", "Информация");
        }

        private void SNILSGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

            if (row != null && row.DataContext is StudentSNILS snils)
            {
                EditSNILS_Click(sender, e);
            }
        }
    }
}