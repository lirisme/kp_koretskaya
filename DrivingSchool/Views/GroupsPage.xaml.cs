using DrivingSchool.Models;
using DrivingSchool.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DrivingSchool.Views
{
    public partial class GroupsPage : Page
    {
        private readonly XmlDataService _dataService;
        private StudyGroupCollection _groups;
        private StudentCollection _students;

        public GroupsPage(XmlDataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _groups = _dataService.LoadStudyGroups();
                _students = _dataService.LoadStudents();

                if (_groups?.Groups == null) _groups = new StudyGroupCollection();
                if (_students?.Students == null) _students = new StudentCollection();

                var groupsList = _groups.Groups.ToList();
                var studentsList = _students.Students.ToList();

                foreach (var group in groupsList)
                {
                    var count = studentsList.Count(s => s.GroupId == group.Id);
                    group.StudentCount = count;

                    Debug.WriteLine($"Группа '{group.Name}' (ID:{group.Id}): {count} студентов");
                }

                GroupsGrid.ItemsSource = groupsList;
                UpdateStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                Debug.WriteLine($"Ошибка: {ex}");
            }
        }

        private void UpdateStatus()
        {
            if (_groups?.Groups == null)
            {
                return;
            }

            var activeGroups = _groups.Groups.Count(g => g.Status == "Активна");
            var totalStudents = _groups.Groups.Sum(g => g.StudentCount);

            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.StatusText.Text = $"Групп: {_groups.Groups.Count} | Активных: {activeGroups} | Студентов: {totalStudents}";
            }
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new GroupEditDialog(_dataService);
                if (dialog.ShowDialog() == true)
                {
                    _groups.Groups.Add(dialog.StudyGroup);
                    _dataService.SaveStudyGroups(_groups);
                    LoadData();
                    MessageBox.Show($"Группа {dialog.StudyGroup.Name} создана!", "Успех");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании группы: {ex.Message}", "Ошибка");
            }
        }

        private void EditGroup_Click(object sender, RoutedEventArgs e)
        {
            if (GroupsGrid.SelectedItem is StudyGroup selectedGroup)
            {
                var dialog = new GroupEditDialog(_dataService, selectedGroup);
                if (dialog.ShowDialog() == true)
                {
                    var index = _groups.Groups.IndexOf(selectedGroup);
                    if (index >= 0)
                    {
                        _groups.Groups[index] = dialog.StudyGroup;
                        _dataService.SaveStudyGroups(_groups);
                        LoadData();
                        MessageBox.Show($"Группа {dialog.StudyGroup.Name} обновлена!", "Успех");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите группу для редактирования", "Предупреждение");
            }
        }

        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            if (GroupsGrid.SelectedItem is StudyGroup selectedGroup)
            {
                var studentsInGroup = _students.Students.Count(s => s.GroupId == selectedGroup.Id);
                if (studentsInGroup > 0)
                {
                    MessageBox.Show($"Невозможно удалить группу. В группе {studentsInGroup} студентов.", "Ошибка");
                    return;
                }

                var result = MessageBox.Show($"Удалить группу {selectedGroup.Name}?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _groups.Groups.Remove(selectedGroup);
                    _dataService.SaveStudyGroups(_groups);
                    LoadData();
                    MessageBox.Show($"Группа {selectedGroup.Name} удалена.", "Успех");
                }
            }
            else
            {
                MessageBox.Show("Выберите группу для удаления", "Предупреждение");
            }
        }

        private void ViewStudents_Click(object sender, RoutedEventArgs e)
        {
            if (GroupsGrid.SelectedItem is StudyGroup selectedGroup)
            {
                var studentsInGroup = _students.Students
                    .Where(s => s.GroupId == selectedGroup.Id)
                    .ToList();

                if (studentsInGroup.Any())
                {
                    string studentList = string.Join("\n", studentsInGroup.Select(s => $"{s.FullName} (тел: {s.Phone})"));
                    MessageBox.Show($"Студенты в группе {selectedGroup.Name}:\n\n{studentList}",
                        $"Студенты группы ({studentsInGroup.Count} чел.)");
                }
                else
                {
                    MessageBox.Show("В выбранной группе нет студентов", "Информация");
                }
            }
            else
            {
                MessageBox.Show("Выберите группу для просмотра студентов", "Предупреждение");
            }
        }

        private void AddStudentToGroup_Click(object sender, RoutedEventArgs e)
        {
            if (GroupsGrid.SelectedItem is StudyGroup selectedGroup)
            {
                try
                {
                    var choiceWindow = new Window
                    {
                        Title = "Добавление студента в группу",
                        Width = 400,
                        Height = 200,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        Owner = Application.Current.MainWindow
                    };

                    var stackPanel = new StackPanel { Margin = new Thickness(20) };
                    choiceWindow.Content = stackPanel;

                    var titleText = new TextBlock
                    {
                        Text = $"Добавить студента в группу: {selectedGroup.Name}",
                        FontWeight = FontWeights.Bold,
                        FontSize = 14,
                        Margin = new Thickness(0, 0, 0, 20),
                        TextAlignment = TextAlignment.Center
                    };
                    stackPanel.Children.Add(titleText);

                    var newStudentButton = new Button
                    {
                        Content = "➕ Создать нового студента",
                        Height = 40,
                        Margin = new Thickness(0, 0, 0, 10),
                        Background = System.Windows.Media.Brushes.LightGreen
                    };

                    var existingStudentButton = new Button
                    {
                        Content = "👥 Выбрать существующего студента",
                        Height = 40,
                        Margin = new Thickness(0, 0, 0, 10),
                        Background = System.Windows.Media.Brushes.LightBlue
                    };

                    var cancelButton = new Button
                    {
                        Content = "❌ Отмена",
                        Height = 40,
                        Background = System.Windows.Media.Brushes.LightCoral
                    };

                    stackPanel.Children.Add(newStudentButton);
                    stackPanel.Children.Add(existingStudentButton);
                    stackPanel.Children.Add(cancelButton);

                    bool createNewStudent = false;

                    newStudentButton.Click += (s, args) =>
                    {
                        createNewStudent = true;
                        choiceWindow.DialogResult = true;
                    };

                    existingStudentButton.Click += (s, args) =>
                    {
                        createNewStudent = false;
                        choiceWindow.DialogResult = true;
                    };

                    cancelButton.Click += (s, args) =>
                    {
                        choiceWindow.DialogResult = false;
                    };

                    if (choiceWindow.ShowDialog() == true)
                    {
                        if (createNewStudent)
                        {
                            var dialog = new StudentEditDialog(_dataService);
                            if (dialog.ShowDialog() == true)
                            {
                                dialog.Student.GroupId = selectedGroup.Id;
                                var students = _dataService.LoadStudents();
                                students.Students.Add(dialog.Student);
                                _dataService.SaveStudents(students);
                                LoadData();
                                MessageBox.Show($"Студент {dialog.Student.FullName} добавлен в группу {selectedGroup.Name}!", "Успех");
                            }
                        }
                        else
                        {
                            var students = _dataService.LoadStudents();
                            var studentsWithoutGroup = students.Students.Where(s => s.GroupId == 0 || s.GroupId != selectedGroup.Id).ToList();

                            if (!studentsWithoutGroup.Any())
                            {
                                MessageBox.Show("Нет студентов без группы или в других группах", "Информация");
                                return;
                            }

                            var studentSelectionWindow = new Window
                            {
                                Title = "Выбор студента для добавления в группу",
                                Width = 600,
                                Height = 400,
                                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                                Owner = Application.Current.MainWindow
                            };

                            var grid = new Grid();
                            studentSelectionWindow.Content = grid;

                            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                            var searchPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10) };
                            var searchTextBox = new TextBox { Width = 200, Margin = new Thickness(0, 0, 10, 0)};
                            var searchButton = new Button { Content = "Поиск", Width = 80 };

                            searchPanel.Children.Add(searchTextBox);
                            searchPanel.Children.Add(searchButton);
                            Grid.SetRow(searchPanel, 0);

                            var dataGrid = new DataGrid
                            {
                                AutoGenerateColumns = false,
                                ItemsSource = studentsWithoutGroup,
                                SelectionMode = DataGridSelectionMode.Single,
                                Margin = new Thickness(10)
                            };

                            dataGrid.Columns.Add(new DataGridTextColumn { Header = "ФИО", Binding = new System.Windows.Data.Binding("FullName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
                            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new System.Windows.Data.Binding("Phone"), Width = 120 });
                            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Возраст", Binding = new System.Windows.Data.Binding("Age"), Width = 60 });
                            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Текущая группа", Binding = new System.Windows.Data.Binding("GroupId"), Width = 100 });

                            Grid.SetRow(dataGrid, 1);

                            var buttonPanel = new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Margin = new Thickness(10)
                            };
                            var selectButton = new Button { Content = "Добавить в группу", Width = 120, Margin = new Thickness(5), Background = System.Windows.Media.Brushes.LightGreen };
                            var cancelSelectButton = new Button { Content = "Отмена", Width = 80, Margin = new Thickness(5) };

                            Student selectedStudent = null;

                            searchButton.Click += (s, args) =>
                            {
                                var searchText = searchTextBox.Text.ToLower();
                                if (string.IsNullOrWhiteSpace(searchText))
                                {
                                    dataGrid.ItemsSource = studentsWithoutGroup;
                                }
                                else
                                {
                                    var filteredStudents = studentsWithoutGroup
                                        .Where(st => st.FullName.ToLower().Contains(searchText) || st.Phone.Contains(searchText))
                                        .ToList();
                                    dataGrid.ItemsSource = filteredStudents;
                                }
                            };

                            selectButton.Click += (s, args) =>
                            {
                                selectedStudent = dataGrid.SelectedItem as Student;
                                if (selectedStudent == null)
                                {
                                    MessageBox.Show("Выберите студента", "Предупреждение");
                                    return;
                                }
                                studentSelectionWindow.DialogResult = true;
                            };

                            cancelSelectButton.Click += (s, args) =>
                            {
                                studentSelectionWindow.DialogResult = false;
                            };

                            buttonPanel.Children.Add(selectButton);
                            buttonPanel.Children.Add(cancelSelectButton);
                            Grid.SetRow(buttonPanel, 2);

                            grid.Children.Add(searchPanel);
                            grid.Children.Add(dataGrid);
                            grid.Children.Add(buttonPanel);

                            if (studentSelectionWindow.ShowDialog() == true && selectedStudent != null)
                            {
                                selectedStudent.GroupId = selectedGroup.Id;
                                _dataService.SaveStudents(students);
                                LoadData();
                                MessageBox.Show($"Студент {selectedStudent.FullName} добавлен в группу {selectedGroup.Name}!", "Успех");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении студента: {ex.Message}", "Ошибка");
                }
            }
            else
            {
                MessageBox.Show("Выберите группу для добавления студента", "Предупреждение");
            }
        }

        private void FindGroupByStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var students = _dataService.LoadStudents();
                var groups = _dataService.LoadStudyGroups();

                if (!students.Students.Any())
                {
                    MessageBox.Show("В системе нет студентов", "Информация");
                    return;
                }

                var searchWindow = new Window
                {
                    Title = "Поиск группы по студенту",
                    Width = 600,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Application.Current.MainWindow
                };

                var grid = new Grid();
                searchWindow.Content = grid;

                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                var searchPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10) };
                var searchTextBox = new TextBox { Width = 250, Margin = new Thickness(0, 0, 10, 0) };
                var searchButton = new Button { Content = "Найти", Width = 80 };

                searchPanel.Children.Add(searchTextBox);
                searchPanel.Children.Add(searchButton);
                Grid.SetRow(searchPanel, 0);

                var dataGrid = new DataGrid
                {
                    AutoGenerateColumns = false,
                    ItemsSource = students.Students,
                    SelectionMode = DataGridSelectionMode.Single,
                    Margin = new Thickness(10)
                };

                dataGrid.Columns.Add(new DataGridTextColumn { Header = "ФИО", Binding = new System.Windows.Data.Binding("FullName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new System.Windows.Data.Binding("Phone"), Width = 120 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "Возраст", Binding = new System.Windows.Data.Binding("Age"), Width = 60 });
                dataGrid.Columns.Add(new DataGridTextColumn { Header = "ID группы", Binding = new System.Windows.Data.Binding("GroupId"), Width = 80 });

                Grid.SetRow(dataGrid, 1);

                var resultPanel = new StackPanel { Margin = new Thickness(10) };
                var resultText = new TextBlock { Text = "Выберите студента для просмотра группы", FontWeight = FontWeights.Bold };
                resultPanel.Children.Add(resultText);
                Grid.SetRow(resultPanel, 2);

                searchButton.Click += (s, args) =>
                {
                    var searchText = searchTextBox.Text.ToLower();
                    if (string.IsNullOrWhiteSpace(searchText))
                    {
                        dataGrid.ItemsSource = students.Students;
                    }
                    else
                    {
                        var filteredStudents = students.Students
                            .Where(st => st.FullName.ToLower().Contains(searchText) || st.Phone.Contains(searchText))
                            .ToList();
                        dataGrid.ItemsSource = filteredStudents;
                    }
                };

                dataGrid.SelectionChanged += (s, args) =>
                {
                    var selectedStudent = dataGrid.SelectedItem as Student;
                    if (selectedStudent != null)
                    {
                        if (selectedStudent.GroupId > 0)
                        {
                            var studentGroup = groups.Groups.FirstOrDefault(g => g.Id == selectedStudent.GroupId);
                            if (studentGroup != null)
                            {
                                resultText.Text = $"Студент: {selectedStudent.FullName}\n" +
                                                $"Группа: {studentGroup.Name}\n" +
                                                $"Статус: {studentGroup.Status}\n" +
                                                $"Период: {studentGroup.StartDate:dd.MM.yyyy} - {studentGroup.EndDate:dd.MM.yyyy}\n";
                            }
                            else
                            {
                                resultText.Text = $"Студент: {selectedStudent.FullName}\nГруппа не найдена (ID: {selectedStudent.GroupId})";
                            }
                        }
                        else
                        {
                            resultText.Text = $"Студент: {selectedStudent.FullName}\nНе состоит в группе";
                        }
                    }
                };

                grid.Children.Add(searchPanel);
                grid.Children.Add(dataGrid);
                grid.Children.Add(resultPanel);

                searchWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске группы: {ex.Message}", "Ошибка");
            }
        }

        private void MoveStudentToGroup_Click(object sender, RoutedEventArgs e)
        {
            if (GroupsGrid.SelectedItem is StudyGroup selectedGroup)
            {
                try
                {
                    var students = _dataService.LoadStudents();
                    var studentsWithoutGroup = students.Students.Where(s => s.GroupId == 0).ToList();

                    if (!studentsWithoutGroup.Any())
                    {
                        MessageBox.Show("Нет студентов без группы", "Информация");
                        return;
                    }

                    var studentSelectionWindow = new Window
                    {
                        Title = "Выбор студента для перемещения",
                        Width = 500,
                        Height = 300,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        Owner = Application.Current.MainWindow
                    };

                    var grid = new Grid();
                    studentSelectionWindow.Content = grid;

                    grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                    grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                    var dataGrid = new DataGrid
                    {
                        AutoGenerateColumns = false,
                        ItemsSource = studentsWithoutGroup,
                        SelectionMode = DataGridSelectionMode.Single,
                        Margin = new Thickness(10)
                    };

                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "ФИО", Binding = new System.Windows.Data.Binding("FullName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new System.Windows.Data.Binding("Phone"), Width = 120 });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Возраст", Binding = new System.Windows.Data.Binding("Age"), Width = 60 });

                    Grid.SetRow(dataGrid, 0);

                    var buttonPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(10)
                    };
                    var selectButton = new Button { Content = "Выбрать", Width = 80, Margin = new Thickness(5) };
                    var cancelButton = new Button { Content = "Отмена", Width = 80, Margin = new Thickness(5) };

                    Student selectedStudent = null;

                    selectButton.Click += (s, args) =>
                    {
                        selectedStudent = dataGrid.SelectedItem as Student;
                        if (selectedStudent == null)
                        {
                            MessageBox.Show("Выберите студента", "Предупреждение");
                            return;
                        }
                        studentSelectionWindow.DialogResult = true;
                    };

                    cancelButton.Click += (s, args) =>
                    {
                        studentSelectionWindow.DialogResult = false;
                    };

                    buttonPanel.Children.Add(selectButton);
                    buttonPanel.Children.Add(cancelButton);
                    Grid.SetRow(buttonPanel, 1);

                    grid.Children.Add(dataGrid);
                    grid.Children.Add(buttonPanel);

                    if (studentSelectionWindow.ShowDialog() == true && selectedStudent != null)
                    {
                        selectedStudent.GroupId = selectedGroup.Id;
                        _dataService.SaveStudents(students);

                        LoadData();
                        MessageBox.Show($"Студент {selectedStudent.FullName} перемещен в группу {selectedGroup.Name}!", "Успех");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при перемещении студента: {ex.Message}", "Ошибка");
                }
            }
            else
            {
                MessageBox.Show("Выберите группу для перемещения студента", "Предупреждение");
            }
        }

        private void RemoveStudentFromGroup_Click(object sender, RoutedEventArgs e)
        {
            if (GroupsGrid.SelectedItem is StudyGroup selectedGroup)
            {
                try
                {
                    var students = _dataService.LoadStudents();
                    var studentsInGroup = students.Students.Where(s => s.GroupId == selectedGroup.Id).ToList();

                    if (!studentsInGroup.Any())
                    {
                        MessageBox.Show("В выбранной группе нет студентов", "Информация");
                        return;
                    }

                    var studentSelectionWindow = new Window
                    {
                        Title = "Удаление студента из группы",
                        Width = 500,
                        Height = 300,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        Owner = Application.Current.MainWindow
                    };

                    var grid = new Grid();
                    studentSelectionWindow.Content = grid;

                    grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                    grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                    var dataGrid = new DataGrid
                    {
                        AutoGenerateColumns = false,
                        ItemsSource = studentsInGroup,
                        SelectionMode = DataGridSelectionMode.Single,
                        Margin = new Thickness(10)
                    };

                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "ФИО", Binding = new System.Windows.Data.Binding("FullName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new System.Windows.Data.Binding("Phone"), Width = 120 });
                    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Возраст", Binding = new System.Windows.Data.Binding("Age"), Width = 60 });

                    Grid.SetRow(dataGrid, 0);

                    var buttonPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(10)
                    };
                    var removeButton = new Button { Content = "Удалить", Width = 80, Margin = new Thickness(5), Background = System.Windows.Media.Brushes.LightCoral };
                    var cancelButton = new Button { Content = "Отмена", Width = 80, Margin = new Thickness(5) };

                    Student selectedStudent = null;

                    removeButton.Click += (s, args) =>
                    {
                        selectedStudent = dataGrid.SelectedItem as Student;
                        if (selectedStudent == null)
                        {
                            MessageBox.Show("Выберите студента", "Предупреждение");
                            return;
                        }
                        studentSelectionWindow.DialogResult = true;
                    };

                    cancelButton.Click += (s, args) =>
                    {
                        studentSelectionWindow.DialogResult = false;
                    };

                    buttonPanel.Children.Add(removeButton);
                    buttonPanel.Children.Add(cancelButton);
                    Grid.SetRow(buttonPanel, 1);

                    grid.Children.Add(dataGrid);
                    grid.Children.Add(buttonPanel);

                    if (studentSelectionWindow.ShowDialog() == true && selectedStudent != null)
                    {
                        selectedStudent.GroupId = 0;
                        _dataService.SaveStudents(students);

                        LoadData();
                        MessageBox.Show($"Студент {selectedStudent.FullName} удален из группы {selectedGroup.Name}!", "Успех");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении студента из группы: {ex.Message}", "Ошибка");
                }
            }
            else
            {
                MessageBox.Show("Выберите группу", "Предупреждение");
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_groups?.Groups == null) return;

            var searchText = SearchTextBox.Text?.ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                GroupsGrid.ItemsSource = _groups.Groups;
            }
            else
            {
                var filteredGroups = _groups.Groups
                    .Where(g => g.Name.ToLower().Contains(searchText) ||
                               (g.Status != null && g.Status.ToLower().Contains(searchText)))
                    .ToList();
                GroupsGrid.ItemsSource = filteredGroups;
            }
        }

        private void GroupsGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

            if (row != null && row.DataContext is StudyGroup group)
            {
                var dialog = new GroupEditDialog(_dataService, group);
                if (dialog.ShowDialog() == true)
                {
                    LoadData();
                }
            }
        }
    }
}