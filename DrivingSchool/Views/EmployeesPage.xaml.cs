using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class EmployeesPage : Page
    {
        private readonly XmlDataService _dataService;
        private EmployeeCollection _employees;
        private EmployeeCollection _filteredEmployees;

        public EmployeesPage(XmlDataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            try
            {
                _employees = _dataService.LoadEmployees();

                if (_employees == null)
                {
                    _employees = new EmployeeCollection();
                }
                if (_employees.Employees == null)
                {
                    _employees.Employees = new System.Collections.Generic.List<Employee>();
                }

                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                _employees = new EmployeeCollection { Employees = new System.Collections.Generic.List<Employee>() };
                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            if (_employees?.Employees == null)
            {
                _filteredEmployees = new EmployeeCollection { Employees = new System.Collections.Generic.List<Employee>() };
            }
            else
            {
                var searchText = SearchTextBox?.Text?.ToLower() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    _filteredEmployees = new EmployeeCollection { Employees = _employees.Employees.ToList() };
                }
                else
                {
                    _filteredEmployees = new EmployeeCollection
                    {
                        Employees = _employees.Employees
                            .Where(e => (e.FullName ?? "").ToLower().Contains(searchText) ||
                                       (e.Position ?? "").ToLower().Contains(searchText) ||
                                       (e.Phone ?? "").Contains(searchText) ||
                                       (e.Email ?? "").ToLower().Contains(searchText) ||
                                       (e.Status ?? "").ToLower().Contains(searchText))
                            .ToList()
                    };
                }
            }

            EmployeesGrid.ItemsSource = _filteredEmployees.Employees;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.StatusText.Text = $"Найдено сотрудников: {_filteredEmployees?.Employees?.Count ?? 0}";
            }
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new EmployeeEditDialog(_dataService);
                if (dialog.ShowDialog() == true)
                {
                    _employees.Employees.Add(dialog.Employee);
                    _dataService.SaveEmployees(_employees);
                    LoadEmployees();

                    MessageBox.Show($"Сотрудник {dialog.Employee.FullName} успешно добавлен!", "Успех");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка");
            }
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EmployeesGrid.SelectedItem is Employee selectedEmployee)
                {
                    var dialog = new EmployeeEditDialog(_dataService, selectedEmployee);
                    if (dialog.ShowDialog() == true)
                    {
                        var originalEmployee = _employees.Employees.FirstOrDefault(emp => emp.Id == selectedEmployee.Id);
                        if (originalEmployee != null)
                        {
                            originalEmployee.FullName = dialog.Employee.FullName;
                            originalEmployee.Position = dialog.Employee.Position;
                            originalEmployee.Status = dialog.Employee.Status;
                            originalEmployee.Phone = dialog.Employee.Phone;
                            originalEmployee.Email = dialog.Employee.Email;
                            originalEmployee.HireDate = dialog.Employee.HireDate;

                            _dataService.SaveEmployees(_employees);
                            LoadEmployees();
                            MessageBox.Show($"Данные сотрудника {dialog.Employee.FullName} обновлены!", "Успех");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите сотрудника для редактирования", "Предупреждение");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании: {ex.Message}", "Ошибка");
            }
        }

        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EmployeesGrid.SelectedItem is Employee selectedEmployee)
                {
                    var result = MessageBox.Show($"Удалить сотрудника {selectedEmployee.FullName}?",
                        "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        _employees.Employees.Remove(selectedEmployee);
                        _dataService.SaveEmployees(_employees);
                        LoadEmployees();
                        MessageBox.Show($"Сотрудник {selectedEmployee.FullName} удален.", "Успех");
                    }
                }
                else
                {
                    MessageBox.Show("Выберите сотрудника для удаления", "Предупреждение");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка");
            }
        }

        private void ViewEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesGrid.SelectedItem is Employee selectedEmployee)
            {
                var experience = CalculateExperience(selectedEmployee.HireDate);

                MessageBox.Show($"Просмотр данных сотрудника:\n\n" +
                               $"ФИО: {selectedEmployee.FullName}\n" +
                               $"Должность: {selectedEmployee.Position}\n" +
                               $"Статус: {selectedEmployee.Status}\n" +
                               $"Телефон: {selectedEmployee.Phone}\n" +
                               $"Email: {selectedEmployee.Email ?? "не указан"}\n" +
                               $"Дата приема: {selectedEmployee.HireDate:dd.MM.yyyy}\n" +
                               $"Стаж: {experience}",
                               $"Данные сотрудника: {selectedEmployee.FullName}");
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для просмотра", "Предупреждение");
            }
        }

        private void ExportEmployees_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Экспорт данных сотрудников\n\nВсего записей: {_filteredEmployees.Employees.Count}",
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

        private void EmployeesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Логика при изменении выбора
        }

        private string CalculateExperience(DateTime hireDate)
        {
            var span = DateTime.Now - hireDate;
            var years = span.Days / 365;
            var months = (span.Days % 365) / 30;

            if (years > 0)
                return $"{years} г. {months} мес.";
            else
                return $"{months} мес.";
        }
    }
}