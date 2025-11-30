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
                    ExperienceYears = CalculateExperience(DateTime.Now)
                };
                _isEditMode = false;
                Title = "Добавление водительского удостоверения";
            }

            DataContext = LicenseData;
            InitializeCategoryField();

            IssueDatePicker.SelectedDateChanged += IssueDatePicker_SelectedDateChanged;

            if (_student != null)
            {
                Title += $" - {_student.FullName}";
            }

            UpdateStudentInfo();
        }

        private int CalculateExperience(DateTime issueDate)
        {
            if (issueDate == default) return 0;

            var today = DateTime.Today;
            var experience = today.Year - issueDate.Year;

            if (issueDate.Date > today.AddYears(-experience))
            {
                experience--;
            }

            return Math.Max(0, experience);
        }

        private void IssueDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IssueDatePicker.SelectedDate.HasValue)
            {
                LicenseData.ExperienceYears = CalculateExperience(IssueDatePicker.SelectedDate.Value);

                var binding = ExperienceTextBox.GetBindingExpression(TextBox.TextProperty);
                binding?.UpdateTarget();
            }
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

            try
            {
                var licenses = _dataService.LoadDrivingLicenses();

                if (_isEditMode)
                {
                    var existingLicense = licenses.Licenses.FirstOrDefault(c => c.Id == LicenseData.Id);
                    if (existingLicense != null)
                    {
                        existingLicense.Series = LicenseData.Series;
                        existingLicense.Number = LicenseData.Number;
                        existingLicense.Categories = LicenseData.Categories;
                        existingLicense.IssuedBy = LicenseData.IssuedBy;
                        existingLicense.DivisionCode = LicenseData.DivisionCode;
                        existingLicense.IssueDate = LicenseData.IssueDate;
                        existingLicense.ExpiryDate = LicenseData.ExpiryDate;
                        existingLicense.ExperienceYears = LicenseData.ExperienceYears;
                    }
                }
                else
                {
                    licenses.Licenses.Add(LicenseData);
                }

                _dataService.SaveDrivingLicenses(licenses);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStudentInfo()
        {
            if (_student != null)
            {
                TitleTextBlock.Text = $"Водительское удостоверение учащегося: {_student.FullName}";

                var categories = _dataService.LoadVehicleCategories();
                var studentCategory = categories.Categories.FirstOrDefault(c => c.Id == _student.VehicleCategoryId);
                if (studentCategory != null)
                {
                    CategoriesTextBox.ToolTip = $"Категория автоматически заполняется из данных студента: {_student.FullName}";
                }
            }
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}