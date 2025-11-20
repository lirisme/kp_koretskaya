using DrivingSchool.Models;
using DrivingSchool.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var button = contextMenu?.PlacementTarget as Button;
            var student = button?.Tag as Student;

            if (student == null)
            {
                student = StudentsGrid.SelectedItem as Student;
            }

            if (student != null)
            {
                MessageBox.Show($"Просмотр данных учащегося:\n\n" +
                               $"ФИО: {student.FullName}\n" +
                               $"Телефон: {student.Phone}\n" +
                               $"Email: {student.Email ?? "не указан"}\n" +
                               $"Дата рождения: {student.BirthDate:dd.MM.yyyy}\n" +
                               $"Место рождения: {student.BirthPlace}\n" +
                               $"Гражданство: {student.Citizenship}",
                               $"Данные учащегося: {student.FullName}");
            }
            else
            {
                MessageBox.Show("Выберите учащегося для просмотра", "Предупреждение");
            }
        }


        private void DocumentsButton_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var button = contextMenu?.PlacementTarget as Button;
            var student = button?.Tag as Student;

            if (student == null)
            {
                student = StudentsGrid.SelectedItem as Student;
            }

            if (student != null)
            {
                MessageBox.Show($"Генерация документов для: {student.FullName}\n\n" +
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

        private void MoreOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Student student)
            {
                StudentsGrid.SelectedItem = student;

                button.ContextMenu.IsOpen = true;
            }
        }

        private void AddPassport_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var button = contextMenu?.PlacementTarget as Button;
            var student = button?.Tag as Student;

            if (student == null)
            {
                student = StudentsGrid.SelectedItem as Student;
            }

            if (student != null)
            {
                var passports = _dataService.LoadPassportData();
                var existingPassport = passports.Passports.FirstOrDefault(p => p.StudentId == student.Id);

                if (existingPassport != null)
                {
                    var dialog = new PassportEditDialog(_dataService, student.Id, existingPassport);
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
                    var dialog = new PassportEditDialog(_dataService, student.Id);
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
            StudentsGrid.Items.Refresh();
        }

        private void AddSNILS_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var button = contextMenu?.PlacementTarget as Button;
            var student = button?.Tag as Student;

            if (student == null)
            {
                student = StudentsGrid.SelectedItem as Student;
            }

            if (student != null)
            {
                var snilsList = _dataService.LoadSNILSData();
                var existingSNILS = snilsList.SNILSList.FirstOrDefault(s => s.StudentId == student.Id);

                if (existingSNILS != null)
                {
                    var dialog = new SNILSEditDialog(_dataService, student.Id, existingSNILS);
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
                    var dialog = new SNILSEditDialog(_dataService, student.Id);
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
            StudentsGrid.Items.Refresh();
        }

        private void AddMedical_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var button = contextMenu?.PlacementTarget as Button;
            var student = button?.Tag as Student;

            if (student == null)
            {
                student = StudentsGrid.SelectedItem as Student;
            }

            if (student != null)
            {
                var medicals = _dataService.LoadMedicalData();
                var existingMedical = medicals.Certificates.FirstOrDefault(m => m.StudentId == student.Id);

                if (existingMedical != null)
                {
                    var dialog = new MedicalEditDialog(_dataService, student.Id, existingMedical);
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
                    var dialog = new MedicalEditDialog(_dataService, student.Id);
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
            StudentsGrid.Items.Refresh();
        }

        private void AddAddress_Click(object sender, RoutedEventArgs e)
        {
            var student = GetStudentFromContext(sender);
            if (student != null)
            {
                var addresses = _dataService.LoadAddresses();
                var existingAddress = addresses.Addresses.FirstOrDefault(a => a.StudentId == student.Id);

                var dialog = new AddressEditDialog(_dataService, student.Id, existingAddress);
                if (dialog.ShowDialog() == true)
                {
                    if (existingAddress != null)
                    {
                        var index = addresses.Addresses.IndexOf(existingAddress);
                        addresses.Addresses[index] = dialog.Address;
                    }
                    else
                    {
                        addresses.Addresses.Add(dialog.Address);
                    }
                    _dataService.SaveAddresses(addresses);
                    MessageBox.Show("Адрес регистрации сохранен!", "Успех");
                }
            }
            StudentsGrid.Items.Refresh();
        }

        private void AddCertificate_Click(object sender, RoutedEventArgs e)
        {
            var student = GetStudentFromContext(sender);
            if (student != null)
            {
                var certificates = _dataService.LoadCertificates();
                var existingCert = certificates.Certificates.FirstOrDefault(c => c.StudentId == student.Id);

                var dialog = new CertificateEditDialog(_dataService, student.Id, existingCert);
                if (dialog.ShowDialog() == true)
                {
                    if (existingCert != null)
                    {
                        var index = certificates.Certificates.IndexOf(existingCert);
                        certificates.Certificates[index] = dialog.CertificateData;
                    }
                    else
                    {
                        certificates.Certificates.Add(dialog.CertificateData);
                    }
                    _dataService.SaveCertificates(certificates);
                    MessageBox.Show("Свидетельство об окончании сохранено!", "Успех");
                }
            }
            StudentsGrid.Items.Refresh();
        }
        
        private void Contract_Click(object sender, RoutedEventArgs e)
        {
            var student = GetStudentFromContext(sender);
            if (student != null)
            {
                var result = MessageBox.Show($"Сгенерировать договор для {student.FullName}?",
                                           "Генерация договора",
                                           MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Договор сгенерирован успешно!", "Успех");
                }
            }
        }

        private Student GetStudentFromContext(object sender)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var button = contextMenu?.PlacementTarget as Button;
            var student = button?.Tag as Student;

            return student ?? StudentsGrid.SelectedItem as Student;
        }

        private void ExportStudent_Click(object sender, RoutedEventArgs e)
        {
            var student = GetStudentFromContext(sender);
            if (student != null)
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf|Word documents (*.docx)|*.docx",
                    FileName = $"{student.LastName}_{student.FirstName}_profile"
                };

                if (dialog.ShowDialog() == true)
                {
                    MessageBox.Show($"Данные экспортированы в: {dialog.FileName}", "Экспорт");
                }
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

        private void WarningButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Student student)
            {
                var missingData = new List<string>();

                var passports = _dataService.LoadPassportData();
                var snilsList = _dataService.LoadSNILSData();
                var medicals = _dataService.LoadMedicalData();
                var addresses = _dataService.LoadAddresses();

                if (!passports.Passports.Any(p => p.StudentId == student.Id))
                    missingData.Add("• Паспортные данные");

                if (!snilsList.SNILSList.Any(s => s.StudentId == student.Id))
                    missingData.Add("• СНИЛС");

                if (!medicals.Certificates.Any(m => m.StudentId == student.Id))
                    missingData.Add("• Медицинская справка");

                if (!addresses.Addresses.Any(a => a.StudentId == student.Id))
                    missingData.Add("• Адрес регистрации");

                if (missingData.Any())
                {
                    var message = $"У студента {student.FullName} не заполнены:\n\n" +
                                 string.Join("\n", missingData) +
                                 "\n\nНажмите на кнопку '⋯' для заполнения недостающих данных.";

                    MessageBox.Show(message, "Недостающие данные", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"У студента {student.FullName} все основные данные заполнены!",
                                   "Данные заполнены", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private bool CheckMissingData(Student student)
        {
            var passports = _dataService.LoadPassportData();
            var snilsList = _dataService.LoadSNILSData();
            var medicals = _dataService.LoadMedicalData();
            var addresses = _dataService.LoadAddresses();

            bool hasPassport = passports.Passports.Any(p => p.StudentId == student.Id);
            bool hasSNILS = snilsList.SNILSList.Any(s => s.StudentId == student.Id);
            bool hasMedical = medicals.Certificates.Any(m => m.StudentId == student.Id);
            bool hasAddress = addresses.Addresses.Any(a => a.StudentId == student.Id);

            return !hasPassport || !hasSNILS || !hasMedical || !hasAddress;
        }

        private void StudentsGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.DataContext is Student student)
            {
                e.Row.Loaded += (s, args) =>
                {
                    var button = FindWarningButton(e.Row);
                    if (button != null)
                    {
                        bool hasMissingData = CheckMissingData(student);
                        button.Visibility = hasMissingData ? Visibility.Visible : Visibility.Collapsed;
                    }
                };
            }
        }

        private Button FindWarningButton(DataGridRow row)
        {
            return FindVisualChild<Button>(row, btn => btn.Content?.ToString() == "⚠️");
        }

        private T FindVisualChild<T>(DependencyObject parent, Func<T, bool> predicate = null) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result && (predicate == null || predicate(result)))
                    return result;

                var found = FindVisualChild(child, predicate);
                if (found != null) return found;
            }
            return null;
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            return FindVisualChild<T>(parent, null);
        }
    }
}