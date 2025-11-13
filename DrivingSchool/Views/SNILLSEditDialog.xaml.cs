using System;
using System.Linq;
using System.Windows;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class SNILSEditDialog : Window
    {
        private readonly XmlDataService _dataService;
        private readonly int _studentId;
        public StudentSNILS SNILSData { get; private set; }
        private bool _isEditMode;

        public SNILSEditDialog(XmlDataService dataService, int studentId, StudentSNILS snilsData = null)
        {
            InitializeComponent();
            _dataService = dataService;
            _studentId = studentId;

            if (snilsData != null)
            {
                SNILSData = snilsData;
                _isEditMode = true;
                Title = "Редактирование данных СНИЛС";
            }
            else
            {
                SNILSData = new StudentSNILS
                {
                    Id = GetNextSNILSId(),
                    StudentId = studentId,
                    IssueDate = DateTime.Now
                };
                _isEditMode = false;
                Title = "Добавление данных СНИЛС";
            }

            DataContext = SNILSData;
        }

        private int GetNextSNILSId()
        {
            var snilsList = _dataService.LoadSNILSData();
            return snilsList.SNILSList.Count > 0 ? snilsList.SNILSList.Max(s => s.Id) + 1 : 1;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SNILSData.Number))
            {
                MessageBox.Show("Введите номер СНИЛС", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(SNILSData.Number, @"^\d{3}-\d{3}-\d{3} \d{2}$"))
            {
                MessageBox.Show("Номер СНИЛС должен быть в формате: XXX-XXX-XXX XX", "Ошибка",
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