using System;
using System.Linq;
using System.Windows;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class MedicalEditDialog : Window
    {
        private readonly XmlDataService _dataService;
        private readonly int _studentId;
        public StudentMedicalCertificate MedicalData { get; private set; }
        private bool _isEditMode;

        public MedicalEditDialog(XmlDataService dataService, int studentId, StudentMedicalCertificate medicalData = null)
        {
            InitializeComponent();
            _dataService = dataService;
            _studentId = studentId;

            if (medicalData != null)
            {
                MedicalData = medicalData;
                _isEditMode = true;
                Title = "Редактирование медицинской справки";
            }
            else
            {
                MedicalData = new StudentMedicalCertificate
                {
                    Id = GetNextMedicalId(),
                    StudentId = studentId,
                    IssueDate = DateTime.Now,
                    Region = "Оренбургская область"
                };
                _isEditMode = false;
                Title = "Добавление медицинской справки";
            }

            DataContext = MedicalData;

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
                TitleTextBlock.Text = $"Медицинская справка учащегося: {student.FullName}";
            }
        }

        private int GetNextMedicalId()
        {
            var medicals = _dataService.LoadMedicalData();
            return medicals.Certificates.Count > 0 ? medicals.Certificates.Max(m => m.Id) + 1 : 1;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MedicalData.Series) ||
                string.IsNullOrWhiteSpace(MedicalData.Number) ||
                string.IsNullOrWhiteSpace(MedicalData.MedicalInstitution))
            {
                MessageBox.Show("Заполните обязательные поля (Серия, Номер, Медицинское учреждение)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var medicals = _dataService.LoadMedicalData();

                if (_isEditMode)
                {
                    var existingMedical = medicals.Certificates.FirstOrDefault(m => m.Id == MedicalData.Id);
                    if (existingMedical != null)
                    {
                        existingMedical.Series = MedicalData.Series;
                        existingMedical.Number = MedicalData.Number;
                        existingMedical.MedicalInstitution = MedicalData.MedicalInstitution;
                        existingMedical.IssueDate = MedicalData.IssueDate;
                        existingMedical.ValidUntil = MedicalData.ValidUntil;
                        existingMedical.Region = MedicalData.Region;
                    }
                }
                else
                {
                    medicals.Certificates.Add(MedicalData);
                }

                _dataService.SaveMedicalData(medicals);

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