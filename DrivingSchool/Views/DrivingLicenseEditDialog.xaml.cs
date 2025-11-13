using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class DrivingLicenseEditDialog : Window
    {
        private readonly XmlDataService _dataService;
        private readonly int _studentId;
        public StudentDrivingLicense LicenseData { get; private set; }
        private bool _isEditMode;
        private Student _student;

        public DrivingLicenseEditDialog(XmlDataService dataService, int studentId, StudentDrivingLicense licenseData = null)
        {
            InitializeComponent();
            _dataService = dataService;
            _studentId = studentId;

            _student = _dataService.LoadStudents().Students.FirstOrDefault(s => s.Id == studentId);

            if (licenseData != null)
            {
                LicenseData = licenseData;
                _isEditMode = true;
                Title = "Редактирование водительского удостоверения";
            }
            else
            {
                LicenseData = new StudentDrivingLicense
                {
                    Id = GetNextLicenseId(),
                    StudentId = studentId,
                    IssueDate = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddYears(10),
                    Categories = GetStudentCategoryCode(),
                    ExperienceYears = 0
                };
                _isEditMode = false;
                Title = "Добавление водительского удостоверения";
            }

            DataContext = LicenseData;
            InitializeCategoryField();
        }

        private int GetNextLicenseId()
        {
            var licenses = _dataService.LoadDrivingLicenses();
            return licenses.Licenses.Count > 0 ? licenses.Licenses.Max(p => p.Id) + 1 : 1;
        }

        private string GetStudentCategoryCode()
        {
            if (_student != null)
            {
                var categories = _dataService.LoadVehicleCategories();
                var studentCategory = categories.Categories.FirstOrDefault(c => c.Id == _student.VehicleCategoryId);
                return studentCategory?.Code ?? "B";
            }
            return "B";
        }

        private void InitializeCategoryField()
        {
            string categoryCode = GetStudentCategoryCode();

            CategoriesTextBox.Text = categoryCode;
            CategoriesTextBox.IsEnabled = false;

            CategoriesTextBox.ToolTip = "Категория автоматически заполняется из данных студента";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LicenseData.Series) ||
                string.IsNullOrWhiteSpace(LicenseData.Number) ||
                string.IsNullOrWhiteSpace(LicenseData.IssuedBy))
            {
                MessageBox.Show("Заполните обязательные поля (Серия, Номер, Кем выдан)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (LicenseData.IssueDate > DateTime.Now)
            {
                MessageBox.Show("Дата выдачи не может быть в будущем", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (LicenseData.ExpiryDate <= LicenseData.IssueDate)
            {
                MessageBox.Show("Срок действия должен быть позже даты выдачи", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(ExperienceTextBox.Text, out int experience) || experience < 0)
            {
                MessageBox.Show("Стаж должен быть положительным числом", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LicenseData.ExperienceYears = experience;

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