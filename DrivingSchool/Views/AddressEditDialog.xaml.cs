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