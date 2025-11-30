using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class AddressEditDialog : Window
    {
        private readonly XmlDataService _dataService;
        private readonly int _studentId;
        public StudentRegistrationAddress Address { get; private set; }
        private bool _isEditMode;

        public AddressEditDialog(XmlDataService dataService, int studentId, StudentRegistrationAddress address = null)
        {
            InitializeComponent();
            _dataService = dataService;
            _studentId = studentId;

            if (address != null)
            {
                Address = address;
                _isEditMode = true;
                Title = "Редактирование адреса регистрации";
            }
            else
            {
                Address = new StudentRegistrationAddress
                {
                    Id = GetNextAddressId(),
                    StudentId = studentId,
                    Region = "Оренбургская область"
                };
                _isEditMode = false;
                Title = "Добавление адреса регистрации";
            }

            DataContext = Address;

            var students = _dataService.LoadStudents();
            var student = students.Students.FirstOrDefault(s => s.Id == studentId);
            if (student != null)
            {
                Title += $" - {student.FullName}";
            }

            UpdateStudentInfo();
        }

        private void UpdateStudentInfo()
        {
            var students = _dataService.LoadStudents();
            var student = students.Students.FirstOrDefault(s => s.Id == _studentId);
            if (student != null)
            {
                TitleTextBlock.Text = $"Адрес регистрации учащегося: {student.FullName}";
            }
        }

        private int GetNextAddressId()
        {
            var addresses = _dataService.LoadAddresses();
            return addresses.Addresses.Count > 0 ? addresses.Addresses.Max(a => a.Id) + 1 : 1;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Address.Region) ||
                string.IsNullOrWhiteSpace(Address.City) ||
                string.IsNullOrWhiteSpace(Address.Street) ||
                string.IsNullOrWhiteSpace(Address.House))
            {
                MessageBox.Show("Заполните обязательные поля (Регион, Населенный пункт, Улица, Дом)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var addresses = _dataService.LoadAddresses();

                if (_isEditMode)
                {
                    var existingAddress = addresses.Addresses.FirstOrDefault(a => a.Id == Address.Id);
                    if (existingAddress != null)
                    {
                        existingAddress.Region = Address.Region;
                        existingAddress.City = Address.City;
                        existingAddress.Street = Address.Street;
                        existingAddress.House = Address.House;
                        existingAddress.Building = Address.Building;
                        existingAddress.Apartment = Address.Apartment;
                    }
                }
                else
                {
                    addresses.Addresses.Add(Address);
                }

                _dataService.SaveAddresses(addresses);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}