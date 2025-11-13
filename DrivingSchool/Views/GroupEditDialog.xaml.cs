using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class GroupEditDialog : Window
    {
        private readonly XmlDataService _dataService;
        public StudyGroup StudyGroup { get; private set; }
        private bool _isEditMode;



        public GroupEditDialog(XmlDataService dataService, StudyGroup studyGroup = null)
        {
            InitializeComponent();
            _dataService = dataService;

            if (studyGroup != null)
            {
                StudyGroup = studyGroup;
                _isEditMode = true;
                Title = "Редактирование группы";
            }
            else
            {
                StudyGroup = new StudyGroup
                {
                    Id = GetNextGroupId(),
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddMonths(3),
                    Status = "Активна"
                };
                _isEditMode = false;
                Title = "Создание новой группы";
            }

            DataContext = this;
            LoadStatusComboBox();
            UpdateDurationInfo();

            StartDatePicker.SelectedDateChanged += (s, e) => UpdateDurationInfo();
            EndDatePicker.SelectedDateChanged += (s, e) => UpdateDurationInfo();
        }

        private int GetNextGroupId()
        {
            var groups = _dataService.LoadStudyGroups();
            return groups.Groups.Count > 0 ? groups.Groups.Max(g => g.Id) + 1 : 1;
        }

        private void LoadStatusComboBox()
        {
            if (!string.IsNullOrEmpty(StudyGroup.Status))
            {
                foreach (ComboBoxItem item in StatusComboBox.Items)
                {
                    if (item.Content.ToString() == StudyGroup.Status)
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

        private void UpdateDurationInfo()
        {
            if (StudyGroup.StartDate != default && StudyGroup.EndDate != default)
            {
                var duration = StudyGroup.EndDate - StudyGroup.StartDate;
                var months = duration.Days / 30;
                var days = duration.Days % 30;

                if (days > 0)
                    DurationInfoText.Text = $"Длительность: {months} мес. {days} дн. ({duration.Days} дней)";
                else
                    DurationInfoText.Text = $"Длительность: {months} мес. ({duration.Days} дней)";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StudyGroup.Name))
            {
                MessageBox.Show("Введите номер группы", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (StudyGroup.StartDate >= StudyGroup.EndDate)
            {
                MessageBox.Show("Дата окончания должна быть позже даты начала", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                EndDatePicker.Focus();
                return;
            }

            if (StudyGroup.StartDate < DateTime.Today && !_isEditMode)
            {
                MessageBox.Show("Дата начала не может быть в прошлом для новой группы", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                StartDatePicker.Focus();
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