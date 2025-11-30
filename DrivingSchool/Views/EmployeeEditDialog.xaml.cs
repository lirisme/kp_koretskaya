using DrivingSchool.Models;
using DrivingSchool.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DrivingSchool.Views
{
    public partial class EmployeeEditDialog : Window
    {
        private readonly XmlDataService _dataService;
        public Employee Employee { get; private set; }
        private bool _isEditMode;

        public EmployeeEditDialog(XmlDataService dataService, Employee employee = null)
        {
            InitializeComponent();
            _dataService = dataService;

            if (employee != null)
            {
                Employee = new Employee
                {
                    Id = employee.Id,
                    FullName = employee.FullName,
                    Position = employee.Position,
                    Status = employee.Status,
                    Phone = employee.Phone,
                    Email = employee.Email,
                    HireDate = employee.HireDate
                };
                _isEditMode = true;
                Title = "Редактирование сотрудника";
            }
            else
            {
                Employee = new Employee
                {
                    Id = GetNextEmployeeId(),
                    HireDate = DateTime.Now,
                    Status = "Активен"
                };
                _isEditMode = false;
                Title = "Добавление сотрудника";
            }

            DataContext = Employee;
            InitializeComboBoxes();
        }

        private int GetNextEmployeeId()
        {
            var employees = _dataService.LoadEmployees();
            return employees.Employees.Count > 0 ? employees.Employees.Max(e => e.Id) + 1 : 1;
        }

        private void InitializeComboBoxes()
        {
            if (!string.IsNullOrEmpty(Employee.Position))
            {
                foreach (ComboBoxItem item in PositionComboBox.Items)
                {
                    if (item.Content.ToString() == Employee.Position)
                    {
                        PositionComboBox.SelectedItem = item;
                        break;
                    }
                }

                if (PositionComboBox.SelectedItem == null)
                {
                    PositionComboBox.Text = Employee.Position;
                }
            }
            else
            {
                PositionComboBox.SelectedIndex = 0;
            }

            if (!string.IsNullOrEmpty(Employee.Status))
            {
                foreach (ComboBoxItem item in StatusComboBox.Items)
                {
                    if (item.Content.ToString() == Employee.Status)
                    {
                        StatusComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                StatusComboBox.SelectedIndex = 0;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (PositionComboBox.SelectedItem is ComboBoxItem positionItem)
            {
                Employee.Position = positionItem.Content.ToString();
            }
            else
            {
                Employee.Position = PositionComboBox.Text;
            }

            if (StatusComboBox.SelectedItem is ComboBoxItem statusItem)
            {
                Employee.Status = statusItem.Content.ToString();
            }
            else
            {
                Employee.Status = StatusComboBox.Text;
            }

            if (string.IsNullOrWhiteSpace(Employee.FullName))
            {
                MessageBox.Show("Введите ФИО сотрудника", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                FullNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Employee.Position))
            {
                MessageBox.Show("Выберите должность сотрудника", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PositionComboBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Employee.Status))
            {
                MessageBox.Show("Выберите статус сотрудника", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusComboBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Employee.Phone))
            {
                MessageBox.Show("Введите телефон сотрудника", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PhoneTextBox.Focus();
                return;
            }

            if (Employee.HireDate > DateTime.Now)
            {
                MessageBox.Show("Дата приема не может быть в будущем", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                HireDatePicker.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}