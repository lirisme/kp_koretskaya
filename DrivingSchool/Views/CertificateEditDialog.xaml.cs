using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class CertificateEditDialog : Window
    {
        private readonly XmlDataService _dataService;
        private readonly int _studentId;
        public StudentCertificate CertificateData { get; private set; }
        private bool _isEditMode;
        private Student _student;
        
        public CertificateEditDialog(XmlDataService dataService, int studentId, StudentCertificate certificateData = null)
        {
            InitializeComponent();
            _dataService = dataService;
            _studentId = studentId;

            _student = _dataService.LoadStudents().Students.FirstOrDefault(s => s.Id == studentId);

            if (certificateData != null)
            {
                CertificateData = certificateData;
                _isEditMode = true;
                Title = "Редактирование свидетельства";
            }
            else
            {
                CertificateData = new StudentCertificate
                {
                    Id = GetNextCertificateId(),
                    StudentId = studentId,
                    IssueDate = DateTime.Now,
                    VehicleCategoryId = _student?.VehicleCategoryId ?? 2
                };
                _isEditMode = false;
                Title = "Добавление свидетельства";
            }

            DataContext = CertificateData;
            InitializeCategoryField();

            if (_student != null)
            {
                Title += $" - {_student.FullName}";
            }

            UpdateStudentInfo();
        }

        private void UpdateStudentInfo()
        {
            if (_student != null)
            {
                TitleTextBlock.Text = $"Свидетельство об окончании учащегося: {_student.FullName}";

                var categories = _dataService.LoadVehicleCategories();
                var studentCategory = categories.Categories.FirstOrDefault(c => c.Id == _student.VehicleCategoryId);
                if (studentCategory != null)
                {
                    CategoryTextBox.ToolTip = $"Категория автоматически заполняется из данных студента: {_student.FullName}";
                }
            }
        }

        private int GetNextCertificateId()
        {
            var certificates = _dataService.LoadCertificates();
            return certificates.Certificates.Count > 0 ? certificates.Certificates.Max(p => p.Id) + 1 : 1;
        }

        private void InitializeCategoryField()
        {
            if (_student != null)
            {
                var categories = _dataService.LoadVehicleCategories();
                var studentCategory = categories.Categories.FirstOrDefault(c => c.Id == _student.VehicleCategoryId);

                if (studentCategory != null)
                {
                    CategoryTextBox.Text = studentCategory.Code;
                    CategoryTextBox.IsEnabled = false;
                    CertificateData.VehicleCategoryId = _student.VehicleCategoryId;

                    CertificateData.CategoryCode = studentCategory.Code;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CertificateData.CertificateSeries) ||
                string.IsNullOrWhiteSpace(CertificateData.CertificateNumber))
            {
                MessageBox.Show("Заполните обязательные поля (Серия и Номер)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CertificateData.IssueDate > DateTime.Now)
            {
                MessageBox.Show("Дата выдачи не может быть в будущем", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var certificates = _dataService.LoadCertificates();

                if (_isEditMode)
                {
                    var existingCertificate = certificates.Certificates.FirstOrDefault(c => c.Id == CertificateData.Id);
                    if (existingCertificate != null)
                    {
                        existingCertificate.CertificateSeries = CertificateData.CertificateSeries;
                        existingCertificate.CertificateNumber = CertificateData.CertificateNumber;
                        existingCertificate.IssueDate = CertificateData.IssueDate;
                        existingCertificate.VehicleCategoryId = CertificateData.VehicleCategoryId;
                        existingCertificate.CategoryCode = CertificateData.CategoryCode;
                    }
                }
                else
                {
                    certificates.Certificates.Add(CertificateData);
                }

                _dataService.SaveCertificates(certificates);

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