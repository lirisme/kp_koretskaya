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