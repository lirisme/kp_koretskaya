using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class StudentsPage : Page
    {
        private readonly XmlDataService _dataService;
        private StudentCollection _students;
        private StudentCollection _filteredStudents;

        public StudentsPage(XmlDataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            LoadStudents();
        }

        private void LoadStudents()
        {
            try
            {
                _students = _dataService.LoadStudents();

                if (_students == null)
                {
                    _students = new StudentCollection();
                }
                if (_students.Students == null)
                {
                    _students.Students = new System.Collections.Generic.List<Student>();
                }

                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                _students = new StudentCollection { Students = new System.Collections.Generic.List<Student>() };
                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            if (_students?.Students == null)
            {
                _filteredStudents = new StudentCollection { Students = new System.Collections.Generic.List<Student>() };
            }
            else
            {
                var searchText = SearchTextBox?.Text?.ToLower() ?? string.Empty;

                var categories = _dataService.LoadVehicleCategories();

                var filtered = _students.Students;
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    filtered = filtered
                        .Where(s => (s.FullName ?? "").ToLower().Contains(searchText) ||
                                   (s.Phone ?? "").ToLower().Contains(searchText) ||
                                   (s.Email ?? "").ToLower().Contains(searchText))
                        .ToList();
                }

                foreach (var student in filtered)
                {
                    var category = categories.Categories.FirstOrDefault(c => c.Id == student.VehicleCategoryId);
                    student.CategoryCode = category?.Code ?? "B";
                }

                _filteredStudents = new StudentCollection { Students = filtered.ToList() };
            }

            StudentsGrid.ItemsSource = _filteredStudents.Students;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.StatusText.Text = $"Найдено учащихся: {_filteredStudents?.Students?.Count ?? 0}";
            }
        }

        private void AddStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new StudentEditDialog(_dataService);
                if (dialog.ShowDialog() == true)
                {
                    _students.Students.Add(dialog.Student);
                    _dataService.SaveStudents(_students);
                    LoadStudents();

                    MessageBox.Show($"Учащийся {dialog.Student.FullName} успешно добавлен!", "Успех");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка");
            }
        }

        private void EditStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (StudentsGrid.SelectedItem is Student selectedStudent)
                {
                    var dialog = new StudentEditDialog(_dataService, selectedStudent);
                    if (dialog.ShowDialog() == true)
                    {
                        var index = _students.Students.IndexOf(selectedStudent);
                        if (index >= 0)
                        {
                            _students.Students[index] = dialog.Student;
                            _dataService.SaveStudents(_students);
                            LoadStudents();
                            MessageBox.Show($"Данные учащегося {dialog.Student.FullName} обновлены!", "Успех");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите учащегося для редактирования", "Предупреждение");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании: {ex.Message}", "Ошибка");
            }
        }

        private void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (StudentsGrid.SelectedItem is Student selectedStudent)
                {
                    var result = MessageBox.Show($"Удалить учащегося {selectedStudent.FullName}?",
                        "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        _students.Students.Remove(selectedStudent);
                        _dataService.SaveStudents(_students);
                        LoadStudents();
                        MessageBox.Show($"Учащийся {selectedStudent.FullName} удален.", "Успех");
                    }
                }
                else
                {
                    MessageBox.Show("Выберите учащегося для удаления", "Предупреждение");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка");
            }
        }

        private void ViewStudent_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsGrid.SelectedItem is Student selectedStudent)
            {
                MessageBox.Show($"Просмотр данных учащегося:\n\n" +
                               $"ФИО: {selectedStudent.FullName}\n" +
                               $"Телефон: {selectedStudent.Phone}\n" +
                               $"Email: {selectedStudent.Email ?? "не указан"}\n" +
                               $"Дата рождения: {selectedStudent.BirthDate:dd.MM.yyyy}\n" +
                               $"Место рождения: {selectedStudent.BirthPlace}\n" +
                               $"Гражданство: {selectedStudent.Citizenship}",
                               $"Данные учащегося: {selectedStudent.FullName}");
            }
            else
            {
                MessageBox.Show("Выберите учащегося для просмотра", "Предупреждение");
            }
        }

        private void DocumentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsGrid.SelectedItem is Student selectedStudent)
            {
                MessageBox.Show($"Генерация документов для: {selectedStudent.FullName}\n\n" +
                               "Доступные документы:\n" +
                               "• Договор на обучение\n" +
                               "• Заявление в ГИБДД\n" +
                               "• Водительская карточка",
                               "Документы");
            }
            else
            {
                MessageBox.Show("Выберите учащегося для генерации документов", "Предупреждение");
            }
        }

        private void ExportStudents_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Экспорт данных учащихся\n\nВсего записей: {_filteredStudents.Students.Count}",
                "Экспорт данных");
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            ApplyFilter();
        }

        private void StudentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Логика при изменении выбора
        }

        private void AddPassport_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsGrid.SelectedItem is Student selectedStudent)
            {
                var passports = _dataService.LoadPassportData();
                var existingPassport = passports.Passports.FirstOrDefault(p => p.StudentId == selectedStudent.Id);

                if (existingPassport != null)
                {
                    var dialog = new PassportEditDialog(_dataService, selectedStudent.Id, existingPassport);
                    if (dialog.ShowDialog() == true)
                    {
                        var index = passports.Passports.IndexOf(existingPassport);
                        if (index >= 0)
                        {
                            passports.Passports[index] = dialog.PassportData;
                            _dataService.SavePassportData(passports);
                            MessageBox.Show("Паспортные данные обновлены!", "Успех");
                        }
                    }
                }
                else
                {
                    var dialog = new PassportEditDialog(_dataService, selectedStudent.Id);
                    if (dialog.ShowDialog() == true)
                    {
                        passports.Passports.Add(dialog.PassportData);
                        _dataService.SavePassportData(passports);
                        MessageBox.Show("Паспортные данные добавлены!", "Успех");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите учащегося для работы с паспортными данными", "Предупреждение");
            }
        }

        private void AddSNILS_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsGrid.SelectedItem is Student selectedStudent)
            {
                var snilsList = _dataService.LoadSNILSData();
                var existingSNILS = snilsList.SNILSList.FirstOrDefault(s => s.StudentId == selectedStudent.Id);

                if (existingSNILS != null)
                {
                    var dialog = new SNILSEditDialog(_dataService, selectedStudent.Id, existingSNILS);
                    if (dialog.ShowDialog() == true)
                    {
                        var index = snilsList.SNILSList.IndexOf(existingSNILS);
                        if (index >= 0)
                        {
                            snilsList.SNILSList[index] = dialog.SNILSData;
                            _dataService.SaveSNILSData(snilsList);
                            MessageBox.Show("Данные СНИЛС обновлены!", "Успех");
                        }
                    }
                }
                else
                {
                    var dialog = new SNILSEditDialog(_dataService, selectedStudent.Id);
                    if (dialog.ShowDialog() == true)
                    {
                        snilsList.SNILSList.Add(dialog.SNILSData);
                        _dataService.SaveSNILSData(snilsList);
                        MessageBox.Show("Данные СНИЛС добавлены!", "Успех");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите учащегося для работы с данными СНИЛС", "Предупреждение");
            }
        }

        private void AddMedical_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsGrid.SelectedItem is Student selectedStudent)
            {
                var medicals = _dataService.LoadMedicalData();
                var existingMedical = medicals.Certificates.FirstOrDefault(m => m.StudentId == selectedStudent.Id);

                if (existingMedical != null)
                {
                    var dialog = new MedicalEditDialog(_dataService, selectedStudent.Id, existingMedical);
                    if (dialog.ShowDialog() == true)
                    {
                        var index = medicals.Certificates.IndexOf(existingMedical);
                        if (index >= 0)
                        {
                            medicals.Certificates[index] = dialog.MedicalData;
                            _dataService.SaveMedicalData(medicals);
                            MessageBox.Show("Медицинская справка обновлена!", "Успех");
                        }
                    }
                }
                else
                {
                    var dialog = new MedicalEditDialog(_dataService, selectedStudent.Id);
                    if (dialog.ShowDialog() == true)
                    {
                        medicals.Certificates.Add(dialog.MedicalData);
                        _dataService.SaveMedicalData(medicals);
                        MessageBox.Show("Медицинская справка добавлена!", "Успех");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите учащегося для работы с медицинской справкой", "Предупреждение");
            }
        }

        private void GenerateDocument_Click(object sender, RoutedEventArgs e)
        {
            var initiallySelected = StudentsGrid.SelectedItems.Cast<Student>().ToList();

            try
            {
                var selectionDialog = new StudentSelectionDialog(_dataService, initiallySelected);

                if (selectionDialog.ShowDialog() == true && selectionDialog.SelectedStudents.Count > 0)
                {
                    var templateDialog = new TemplateSelectionDialog(_dataService, selectionDialog.SelectedStudents);

                    if (templateDialog.ShowDialog() == true)
                    {
                        MessageBox.Show($"Успешно сгенерировано документов для {selectionDialog.SelectedStudents.Count} студентов", "Успех");
                    }
                }
                else if (selectionDialog.SelectedStudents.Count == 0)
                {
                    MessageBox.Show("Не выбрано ни одного студента", "Предупреждение");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации документов: {ex.Message}", "Ошибка");
            }
        }
    }
}