using DrivingSchool.Models;
using DrivingSchool.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DrivingSchool.Views
{
    public partial class StudentSelectionDialog : Window
    {
        private readonly XmlDataService _dataService;
        public List<Student> SelectedStudents { get; private set; }
        private List<Student> _allStudents;
        private Dictionary<int, bool> _selectionStates;

        public StudentSelectionDialog(XmlDataService dataService, List<Student> initiallySelectedStudents = null)
        {
            InitializeComponent();
            _dataService = dataService;
            SelectedStudents = initiallySelectedStudents ?? new List<Student>();
            _allStudents = new List<Student>();
            _selectionStates = new Dictionary<int, bool>();

            LoadAllStudents();
            UpdateSelectedStudentsList();
            UpdateButtons();
        }

        private void LoadAllStudents()
        {
            try
            {
                var studentsData = _dataService.LoadStudents();
                _allStudents = studentsData.Students;

                foreach (var student in _allStudents)
                {
                    _selectionStates[student.Id] = SelectedStudents.Any(s => s.Id == student.Id);
                }

                UpdateStudentsGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки студентов: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateStudentsGrid()
        {
            var studentsForDisplay = _allStudents.Select(student => new
            {
                Student = student,
                IsSelected = _selectionStates[student.Id],
                LastName = student.LastName,
                FirstName = student.FirstName,
                MiddleName = student.MiddleName,
                Phone = student.Phone,
                GroupId = student.GroupId
            }).ToList();

            AllStudentsGrid.ItemsSource = studentsForDisplay;
        }

        private void UpdateSelectedStudentsList()
        {
            SelectedStudents.Clear();

            foreach (var student in _allStudents)
            {
                if (_selectionStates.ContainsKey(student.Id) && _selectionStates[student.Id])
                {
                    SelectedStudents.Add(student);
                }
            }

            SelectedStudentsGrid.ItemsSource = null;
            SelectedStudentsGrid.ItemsSource = SelectedStudents;
            SelectedCountText.Text = $"Выбрано: {SelectedStudents.Count}";
        }

        private void UpdateButtons()
        {
            RemoveButton.IsEnabled = SelectedStudentsGrid.SelectedItems.Count > 0;
            ClearButton.IsEnabled = SelectedStudents.Count > 0;
            OKButton.IsEnabled = SelectedStudents.Count > 0;
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox?.DataContext != null)
            {
                dynamic item = checkBox.DataContext;
                var student = item.Student as Student;

                if (student != null)
                {
                    _selectionStates[student.Id] = checkBox.IsChecked == true;
                    UpdateSelectedStudentsList();
                    UpdateButtons();
                }
            }
        }

        private void RemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = SelectedStudentsGrid.SelectedItems.Cast<Student>().ToList();

            foreach (var student in selected)
            {
                _selectionStates[student.Id] = false;
            }

            UpdateSelectedStudentsList();
            UpdateButtons();
            UpdateStudentsGrid();
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var student in _allStudents)
            {
                _selectionStates[student.Id] = false;
            }

            UpdateSelectedStudentsList();
            UpdateButtons();
            UpdateStudentsGrid();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
            {
                UpdateStudentsGrid();
            }
            else
            {
                var filtered = _allStudents
                    .Where(s => s.FullName.ToLower().Contains(searchText) ||
                               s.LastName.ToLower().Contains(searchText) ||
                               s.FirstName.ToLower().Contains(searchText) ||
                               s.Phone.Contains(searchText))
                    .Select(student => new
                    {
                        Student = student,
                        IsSelected = _selectionStates[student.Id],
                        LastName = student.LastName,
                        FirstName = student.FirstName,
                        MiddleName = student.MiddleName,
                        Phone = student.Phone,
                        GroupId = student.GroupId
                    })
                    .ToList();

                AllStudentsGrid.ItemsSource = filtered;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SelectedStudentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtons();
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedStudentsGrid.SelectedItem is Student selectedStudent)
            {
                int index = SelectedStudents.IndexOf(selectedStudent);
                if (index > 0)
                {
                    SelectedStudents.RemoveAt(index);
                    SelectedStudents.Insert(index - 1, selectedStudent);
                    UpdateSelectedStudentsList();
                    SelectedStudentsGrid.SelectedItem = selectedStudent;
                }
            }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedStudentsGrid.SelectedItem is Student selectedStudent)
            {
                int index = SelectedStudents.IndexOf(selectedStudent);
                if (index < SelectedStudents.Count - 1)
                {
                    SelectedStudents.RemoveAt(index);
                    SelectedStudents.Insert(index + 1, selectedStudent);
                    UpdateSelectedStudentsList();
                    SelectedStudentsGrid.SelectedItem = selectedStudent;
                }
            }
        }

        private void SelectedStudentsGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var student in _allStudents)
            {
                _selectionStates[student.Id] = true;
            }

            UpdateSelectedStudentsList();
            UpdateButtons();
            UpdateStudentsGrid();
        }
    }
}