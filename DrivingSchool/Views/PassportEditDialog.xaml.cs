using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class PassportEditDialog : Window
    {
        private readonly XmlDataService _dataService;
        private readonly int _studentId;
        public StudentPassportData PassportData { get; private set; }
        private bool _isEditMode;

        public PassportEditDialog(XmlDataService dataService, int studentId, StudentPassportData passportData = null)
        {
            InitializeComponent();
            _dataService = dataService;
            _studentId = studentId;

            if (passportData != null)
            {
                PassportData = passportData;
                _isEditMode = true;
                Title = "Редактирование паспортных данных";
            }
            else
            {
                PassportData = new StudentPassportData
                {
                    Id = GetNextPassportId(),
                    StudentId = studentId,
                    DocumentType = "Паспорт РФ",
                    IssueDate = DateTime.Now
                };
                _isEditMode = false;
                Title = "Добавление паспортных данных";
            }

            DataContext = PassportData;
            InitializeDocumentTypeComboBox();
        }

        private int GetNextPassportId()
        {
            var passports = _dataService.LoadPassportData();
            return passports.Passports.Count > 0 ? passports.Passports.Max(p => p.Id) + 1 : 1;
        }

        private void InitializeDocumentTypeComboBox()
        {
            if (!string.IsNullOrEmpty(PassportData.DocumentType))
            {
                foreach (ComboBoxItem item in DocumentTypeComboBox.Items)
                {
                    if (item.Content.ToString() == PassportData.DocumentType)
                    {
                        DocumentTypeComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                DocumentTypeComboBox.SelectedIndex = 0;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                PassportData.DocumentType = selectedItem.Content.ToString();
            }

            if (string.IsNullOrWhiteSpace(PassportData.Series) ||
                string.IsNullOrWhiteSpace(PassportData.Number) ||
                string.IsNullOrWhiteSpace(PassportData.IssuedBy))
            {
                MessageBox.Show("Заполните обязательные поля (Серия, Номер, Кем выдан)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PassportData.IssueDate > DateTime.Now)
            {
                MessageBox.Show("Дата выдачи не может быть в будущем", "Ошибка",
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