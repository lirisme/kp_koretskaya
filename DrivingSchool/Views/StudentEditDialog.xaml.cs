using DrivingSchool.Models;
using DrivingSchool.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DrivingSchool.Views
{
    public partial class StudentEditDialog : Window
    {
        private readonly XmlDataService _dataService;
        public Student Student { get; private set; }
        private bool _isEditMode;

        public StudentEditDialog(XmlDataService dataService, Student student = null)
        {
            InitializeComponent();
            _dataService = dataService;

            if (student != null)
            {
                Student = student;
                _isEditMode = true;
                Title = "Редактирование учащегося";
            }
            else
            {
                Student = new Student
                {
                    Id = GetNextStudentId(),
                    BirthDate = DateTime.Now.AddYears(-18),
                    Citizenship = "Российская Федерация",
                    VehicleCategoryId = 2,
                    Gender = "Мужской"
                };
                _isEditMode = false;
                Title = "Добавление учащегося";
            }

            DataContext = Student;
            LoadGroups();
            LoadCategories();
            LoadGenderComboBox();
        }

        private int GetNextStudentId()
        {
            var students = _dataService.LoadStudents();
            return students.Students.Count > 0 ? students.Students.Max(s => s.Id) + 1 : 1;
        }

        private void LoadGroups()
        {
            try
            {
                var groups = _dataService.LoadStudyGroups();
                GroupComboBox.Items.Clear();

                foreach (var group in groups.Groups)
                {
                    GroupComboBox.Items.Add(new
                    {
                        Id = group.Id,
                        Name = $"{group.Name} ({group.Status}) - {group.StartDate:dd.MM.yy} - {group.EndDate:dd.MM.yy}"
                    });
                }

                if (GroupComboBox.Items.Count > 0)
                {
                    if (Student.GroupId > 0)
                    {
                        foreach (var item in GroupComboBox.Items)
                        {
                            dynamic groupItem = item;
                            if (groupItem.Id == Student.GroupId)
                            {
                                GroupComboBox.SelectedItem = item;
                                break;
                            }
                        }
                    }

                    if (GroupComboBox.SelectedItem == null)
                    {
                        GroupComboBox.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки групп: {ex.Message}", "Ошибка");
                GroupComboBox.Items.Add(new { Id = 1, Name = "01-24 (Активна)" });
                GroupComboBox.Items.Add(new { Id = 2, Name = "02-24 (Завершена)" });
                GroupComboBox.SelectedIndex = 0;
            }
        }

        private void LoadCategories()
        {
            try
            {
                var categories = _dataService.LoadVehicleCategories();
                CategoryComboBox.ItemsSource = categories.Categories;

                if (categories.Categories.Count > 0)
                {
                    if (Student.VehicleCategoryId > 0)
                    {
                        CategoryComboBox.SelectedValue = Student.VehicleCategoryId;
                    }
                    else
                    {
                        var defaultCategory = categories.Categories.FirstOrDefault(c => c.Code == "B");
                        CategoryComboBox.SelectedValue = defaultCategory?.Id ?? 2;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка");
            }
        }

        private void LoadGenderComboBox()
        {
            if (!string.IsNullOrEmpty(Student.Gender))
            {
                if (Student.Gender == "Мужской")
                    GenderComboBox.SelectedIndex = 0;
                else if (Student.Gender == "Женский")
                    GenderComboBox.SelectedIndex = 1;
                else
                    GenderComboBox.SelectedIndex = 0;
            }
            else
            {
                GenderComboBox.SelectedIndex = 0;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (GenderComboBox.SelectedItem is ComboBoxItem genderItem)
            {
                Student.Gender = genderItem.Content.ToString();
            }
            else
            {
                Student.Gender = "Мужской";
            }

            if (string.IsNullOrWhiteSpace(Student.LastName) ||
                string.IsNullOrWhiteSpace(Student.FirstName) ||
                string.IsNullOrWhiteSpace(Student.Phone))
            {
                MessageBox.Show("Заполните обязательные поля (Фамилия, Имя, Телефон)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Student.BirthDate > DateTime.Now.AddYears(-16))
            {
                MessageBox.Show("Учащийся должен быть старше 16 лет", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CategoryComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите категорию транспортного средства", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (GroupComboBox.SelectedItem != null)
            {
                dynamic selectedGroup = GroupComboBox.SelectedItem;
                Student.GroupId = selectedGroup.Id;
            }

            Student.VehicleCategoryId = (int)CategoryComboBox.SelectedValue;

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