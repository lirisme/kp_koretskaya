using DrivingSchool.Models;
using DrivingSchool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DrivingSchool.Views
{
    public partial class GroupEditDialog : Window
    {
        private readonly XmlDataService _dataService;
        public StudyGroup StudyGroup { get; private set; }
        private bool _isEditMode;

        public ObservableCollection<StudentWithSelection> StudentsInGroup { get; set; }
        private StudentCollection _allStudents;
        private StudyGroupCollection _allGroups;

        public GroupEditDialog(XmlDataService dataService, StudyGroup studyGroup = null)
        {
            InitializeComponent();
            _dataService = dataService;

            StudentsInGroup = new ObservableCollection<StudentWithSelection>();
            DataContext = this;

            if (studyGroup != null)
            {
                StudyGroup = studyGroup;
                _isEditMode = true;
                Title = $"Редактирование группы: {studyGroup.Name}";
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

            LoadAllData();
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

        private void LoadAllData()
        {
            _allStudents = _dataService.LoadStudents();
            _allGroups = _dataService.LoadStudyGroups();

            var studentsInThisGroup = _allStudents.Students
                .Where(s => s.GroupId == StudyGroup.Id)
                .ToList();

            StudentsInGroup.Clear();
            foreach (var student in studentsInThisGroup)
            {
                StudentsInGroup.Add(new StudentWithSelection(student));
            }
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

        private void AddStudent_Click(object sender, RoutedEventArgs e)
        {
            var newStudentDialog = new StudentEditDialog(_dataService);
            if (newStudentDialog.ShowDialog() == true)
            {
                newStudentDialog.Student.GroupId = StudyGroup.Id;
                _allStudents.Students.Add(newStudentDialog.Student);
                _dataService.SaveStudents(_allStudents);

                StudentsInGroup.Add(new StudentWithSelection(newStudentDialog.Student));
                MessageBox.Show($"Студент {newStudentDialog.Student.FullName} добавлен в группу!", "Успех");
            }
        }

        private void SelectStudents_Click(object sender, RoutedEventArgs e)
        {
            var allStudents = _allStudents.Students.ToList();

            if (!allStudents.Any())
            {
                MessageBox.Show("В системе нет студентов", "Информация");
                return;
            }

            var selectionWindow = new Window
            {
                Title = "Выбор студентов для добавления в группу",
                Width = 900,
                Height = 550,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            var selectedStudents = ShowStudentSelectionDialogWithFilter(allStudents, selectionWindow);

            if (selectedStudents != null && selectedStudents.Any())
            {
                ProcessSelectedStudents(selectedStudents);
            }
        }

        private List<Student> ShowStudentSelectionDialogWithFilter(List<Student> allStudents, Window window)
        {
            var grid = new Grid();
            window.Content = grid;

            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            var filterPanel = CreateFilterPanel();
            Grid.SetRow(filterPanel, 0);

            var searchPanel = CreateSearchPanel();
            Grid.SetRow(searchPanel, 1);

            var dataGrid = CreateStudentsDataGrid();
            Grid.SetRow(dataGrid, 2);

            var buttonPanel = CreateButtonPanel();
            Grid.SetRow(buttonPanel, 3);

            var allStudentSelections = allStudents.Select(s => new StudentSelectionItem(s, GetGroupName(s.GroupId))).ToList();
            dataGrid.ItemsSource = allStudentSelections.Where(s => s.Student.GroupId == 0).ToList();

            var filterComboBox = FindChild<ComboBox>(filterPanel, "FilterComboBox");
            var studentsCountText = FindChild<TextBlock>(filterPanel, "StudentsCountText");
            var searchTextBox = FindChild<TextBox>(searchPanel, "SearchTextBox");
            var searchButton = FindChild<Button>(searchPanel, "SearchButton");
            var clearSearchButton = FindChild<Button>(searchPanel, "ClearSearchButton");
            var selectButton = FindChild<Button>(buttonPanel, "SelectButton");
            var cancelButton = FindChild<Button>(buttonPanel, "CancelButton");

            List<Student> result = null;

            void ApplyFilters()
            {
                var searchText = searchTextBox?.Text?.ToLower() ?? "";
                var filterType = (filterComboBox?.SelectedItem as ComboBoxItem)?.Tag as string;

                IEnumerable<StudentSelectionItem> filteredStudents = allStudentSelections;

                switch (filterType)
                {
                    case "no_group":
                        filteredStudents = filteredStudents.Where(s => s.Student.GroupId == 0);
                        break;
                    case "other_groups":
                        filteredStudents = filteredStudents.Where(s => s.Student.GroupId != 0 && s.Student.GroupId != StudyGroup.Id);
                        break;
                    case "all":
                    default:
                        break;
                }

                if (!string.IsNullOrWhiteSpace(searchText) && searchText != "поиск по фио, телефону или группе...")
                {
                    filteredStudents = filteredStudents.Where(item =>
                        item.Student.FullName.ToLower().Contains(searchText) ||
                        item.Student.Phone.Contains(searchText) ||
                        item.Student.Email?.ToLower().Contains(searchText) == true ||
                        item.CurrentGroupName.ToLower().Contains(searchText)
                    );
                }

                dataGrid.ItemsSource = filteredStudents.ToList();
                UpdateStudentsCountText(studentsCountText, dataGrid.ItemsSource as ICollection<StudentSelectionItem>);
            }

            void UpdateStudentsCountText(TextBlock textBlock, ICollection<StudentSelectionItem> students)
            {
                if (textBlock == null) return;

                var selectedCount = students?.Count(s => s.IsSelected) ?? 0;
                var totalCount = students?.Count ?? 0;
                textBlock.Text = $"Показано: {totalCount} | Выбрано: {selectedCount}";
            }

            if (filterComboBox != null)
                filterComboBox.SelectionChanged += (s, e) => ApplyFilters();

            if (searchButton != null)
                searchButton.Click += (s, e) => ApplyFilters();

            if (clearSearchButton != null)
                clearSearchButton.Click += (s, e) =>
                {
                    if (searchTextBox != null) searchTextBox.Text = "";
                    ApplyFilters();
                };

            if (searchTextBox != null)
            {
                searchTextBox.KeyDown += (s, e) =>
                {
                    if (e.Key == System.Windows.Input.Key.Enter) ApplyFilters();
                };

                searchTextBox.GotFocus += (s, e) =>
                {
                    if (searchTextBox.Text == "Поиск по ФИО, телефону или группе...")
                        searchTextBox.Text = "";
                };

                searchTextBox.LostFocus += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(searchTextBox.Text))
                        searchTextBox.Text = "Поиск по ФИО, телефону или группе...";
                };
            }

            dataGrid.SelectionChanged += (s, e) => UpdateStudentsCountText(studentsCountText, dataGrid.ItemsSource as ICollection<StudentSelectionItem>);

            if (selectButton != null)
            {
                selectButton.Click += (s, e) =>
                {
                    var currentItems = dataGrid.ItemsSource as IEnumerable<StudentSelectionItem>;
                    if (currentItems != null)
                    {
                        result = currentItems
                            .Where(item => item.IsSelected)
                            .Select(item => item.Student)
                            .ToList();

                        window.DialogResult = true;
                        window.Close();
                    }
                };
            }

            if (cancelButton != null)
            {
                cancelButton.Click += (s, e) =>
                {
                    window.DialogResult = false;
                    window.Close();
                };
            }

            UpdateStudentsCountText(studentsCountText, dataGrid.ItemsSource as ICollection<StudentSelectionItem>);

            grid.Children.Add(filterPanel);
            grid.Children.Add(searchPanel);
            grid.Children.Add(dataGrid);
            grid.Children.Add(buttonPanel);

            if (window.ShowDialog() == true)
            {
                return result;
            }

            return null;
        }

        private StackPanel CreateFilterPanel()
        {
            var filterPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10, 10, 10, 5)
            };

            var filterLabel = new TextBlock
            {
                Text = "Фильтр:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontWeight = FontWeights.SemiBold
            };

            var filterComboBox = new ComboBox
            {
                Width = 200,
                Margin = new Thickness(0, 0, 20, 0),
                Name = "FilterComboBox"
            };

            filterComboBox.Items.Add(new ComboBoxItem { Content = "Только студенты без группы", Tag = "no_group" });
            filterComboBox.Items.Add(new ComboBoxItem { Content = "Все студенты", Tag = "all" });
            filterComboBox.Items.Add(new ComboBoxItem { Content = "Только из других групп", Tag = "other_groups" });
            filterComboBox.SelectedIndex = 0;

            var studentsCountText = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0),
                FontWeight = FontWeights.SemiBold,
                Name = "StudentsCountText"
            };

            filterPanel.Children.Add(filterLabel);
            filterPanel.Children.Add(filterComboBox);
            filterPanel.Children.Add(studentsCountText);

            return filterPanel;
        }

        private StackPanel CreateSearchPanel()
        {
            var searchPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10, 5, 10, 10)
            };

            var searchTextBox = new TextBox
            {
                Width = 250,
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(5),
                Text = "Поиск по ФИО, телефону или группе...",
                Name = "SearchTextBox"
            };

            var searchButton = new Button
            {
                Content = "Поиск",
                Width = 80,
                Padding = new Thickness(5),
                Name = "SearchButton"
            };

            var clearSearchButton = new Button
            {
                Content = "Очистить",
                Width = 80,
                Padding = new Thickness(5),
                Margin = new Thickness(5, 0, 0, 0),
                Name = "ClearSearchButton"
            };

            searchPanel.Children.Add(searchTextBox);
            searchPanel.Children.Add(searchButton);
            searchPanel.Children.Add(clearSearchButton);

            return searchPanel;
        }

        private DataGrid CreateStudentsDataGrid()
        {
            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                SelectionMode = DataGridSelectionMode.Extended,
                Margin = new Thickness(10, 0, 10, 10),
                CanUserAddRows = false
            };

            dataGrid.Columns.Add(new DataGridCheckBoxColumn
            {
                Header = "Выбрать",
                Binding = new System.Windows.Data.Binding("IsSelected"),
                Width = new DataGridLength(60)
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "ФИО",
                Binding = new System.Windows.Data.Binding("Student.FullName"),
                Width = new DataGridLength(2, DataGridLengthUnitType.Star)
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Телефон",
                Binding = new System.Windows.Data.Binding("Student.Phone"),
                Width = 120
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Email",
                Binding = new System.Windows.Data.Binding("Student.Email"),
                Width = 150
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Возраст",
                Binding = new System.Windows.Data.Binding("Student.Age"),
                Width = 60
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Текущая группа",
                Binding = new System.Windows.Data.Binding("CurrentGroupName"),
                Width = 150
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Статус",
                Binding = new System.Windows.Data.Binding("GroupStatus"),
                Width = 100
            });

            return dataGrid;
        }

        private StackPanel CreateButtonPanel()
        {
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            var selectButton = new Button
            {
                Content = "✅ Добавить выбранных",
                Margin = new Thickness(5),
                Padding = new Thickness(15, 8, 15, 8),
                Background = System.Windows.Media.Brushes.Green,
                Foreground = System.Windows.Media.Brushes.White,
                Name = "SelectButton"
            };

            var cancelButton = new Button
            {
                Content = "❌ Отмена",
                Margin = new Thickness(5),
                Padding = new Thickness(15, 8, 15, 8),
                Background = System.Windows.Media.Brushes.Gray,
                Foreground = System.Windows.Media.Brushes.White,
                Name = "CancelButton"
            };

            buttonPanel.Children.Add(selectButton);
            buttonPanel.Children.Add(cancelButton);

            return buttonPanel;
        }

        private void ProcessSelectedStudents(List<Student> selectedStudents)
        {
            if (selectedStudents == null || !selectedStudents.Any())
                return;

            var studentsFromOtherGroups = selectedStudents
                .Where(s => s.GroupId != StudyGroup.Id && s.GroupId != 0)
                .ToList();

            var studentsAlreadyInThisGroup = selectedStudents
                .Where(s => s.GroupId == StudyGroup.Id)
                .ToList();

            var studentsWithoutGroup = selectedStudents
                .Where(s => s.GroupId == 0)
                .ToList();

            var warnings = new List<string>();

            if (studentsFromOtherGroups.Any())
            {
                var otherGroupsNames = studentsFromOtherGroups
                    .Select(s => {
                        var group = _allGroups.Groups.FirstOrDefault(g => g.Id == s.GroupId);
                        return $"{s.FullName} - {group?.Name ?? $"Группа {s.GroupId}"}";
                    })
                    .ToList();

                warnings.Add($"Следующие студенты будут ПЕРЕМЕЩЕНЫ из других групп:\n\n{string.Join("\n", otherGroupsNames)}");
            }

            if (studentsAlreadyInThisGroup.Any())
            {
                var alreadyInGroupNames = studentsAlreadyInThisGroup
                    .Select(s => s.FullName)
                    .ToList();

                warnings.Add($"Следующие студенты УЖЕ находятся в этой группе и не будут добавлены повторно:\n\n{string.Join("\n", alreadyInGroupNames)}");
            }

            if (warnings.Any())
            {
                var warningMessage = string.Join("\n\n", warnings) +
                                   $"\n\nПродолжить добавление {selectedStudents.Count} студентов?";

                var result = MessageBox.Show(warningMessage, "Предупреждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var studentsToAdd = selectedStudents.Except(studentsAlreadyInThisGroup).ToList();

            if (!studentsToAdd.Any())
            {
                MessageBox.Show("Нет новых студентов для добавления в группу", "Информация");
                return;
            }

            foreach (var student in studentsToAdd)
            {
                student.GroupId = StudyGroup.Id;

                if (!StudentsInGroup.Any(s => s.Student.Id == student.Id))
                {
                    StudentsInGroup.Add(new StudentWithSelection(student));
                }
            }

            _dataService.SaveStudents(_allStudents);

            var messageText = $"Добавлено студентов: {studentsToAdd.Count}";
            if (studentsFromOtherGroups.Any())
            {
                messageText += $"\nПеремещено из других групп: {studentsFromOtherGroups.Count}";
            }
            if (studentsWithoutGroup.Any())
            {
                messageText += $"\nНовых студентов: {studentsWithoutGroup.Count}";
            }
            if (studentsAlreadyInThisGroup.Any())
            {
                messageText += $"\nУже в группе (пропущено): {studentsAlreadyInThisGroup.Count}";
            }

            MessageBox.Show(messageText, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string GetGroupName(int groupId)
        {
            if (groupId == 0) return "Без группы";
            var group = _allGroups.Groups.FirstOrDefault(g => g.Id == groupId);
            return group?.Name ?? $"Группа {groupId}";
        }

        private void MoveToGroup_Click(object sender, RoutedEventArgs e)
        {
            var studentsWithoutGroup = _allStudents.Students
                .Where(s => s.GroupId == 0)
                .ToList();

            if (!studentsWithoutGroup.Any())
            {
                MessageBox.Show("Нет студентов без группы", "Информация");
                return;
            }

            var result = MessageBox.Show(
                $"Переместить всех студентов без группы ({studentsWithoutGroup.Count} чел.) в эту группу?",
                "Перемещение студентов",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var student in studentsWithoutGroup)
                {
                    student.GroupId = StudyGroup.Id;
                    if (!StudentsInGroup.Any(s => s.Student.Id == student.Id))
                    {
                        StudentsInGroup.Add(new StudentWithSelection(student));
                    }
                }

                _dataService.SaveStudents(_allStudents);
                MessageBox.Show($"Перемещено студентов: {studentsWithoutGroup.Count}", "Успех");
            }
        }

        private void RemoveStudent_Click(object sender, RoutedEventArgs e)
        {
            var studentsToRemove = StudentsInGroup.Where(s => s.IsSelected).ToList();

            if (!studentsToRemove.Any())
            {
                MessageBox.Show("Выберите студентов для удаления из группы", "Предупреждение");
                return;
            }

            var result = MessageBox.Show(
                $"Удалить {studentsToRemove.Count} студентов из группы?\n\n" +
                "Студенты не будут удалены из системы, только исключены из группы.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var studentWithSelection in studentsToRemove)
                {
                    var student = _allStudents.Students.FirstOrDefault(s => s.Id == studentWithSelection.Student.Id);
                    if (student != null)
                    {
                        student.GroupId = 0;
                    }
                    StudentsInGroup.Remove(studentWithSelection);
                }

                _dataService.SaveStudents(_allStudents);
                MessageBox.Show($"Удалено студентов: {studentsToRemove.Count}", "Успех");
            }
        }

        private void StudentName_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            var studentWithSelection = textBlock?.DataContext as StudentWithSelection;

            if (studentWithSelection != null)
            {
                ViewStudentDetails(studentWithSelection.Student);
            }
        }

        private void ViewStudentDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var studentWithSelection = button?.Tag as StudentWithSelection;

            if (studentWithSelection != null)
            {
                ViewStudentDetails(studentWithSelection.Student);
            }
        }

        private void ViewStudentDetails(Student student)
        {
            var passportData = _dataService.LoadPassportData();
            var medicalData = _dataService.LoadMedicalData();
            var snilsData = _dataService.LoadSNILSData();
            var addressData = _dataService.LoadAddresses();

            var passport = passportData.Passports.FirstOrDefault(p => p.StudentId == student.Id);
            var medical = medicalData.Certificates.FirstOrDefault(m => m.StudentId == student.Id);
            var snils = snilsData.SNILSList.FirstOrDefault(s => s.StudentId == student.Id);
            var address = addressData.Addresses.FirstOrDefault(a => a.StudentId == student.Id);

            var message = $"📋 Полные данные студента:\n\n" +
                         $"👤 ФИО: {student.FullName}\n" +
                         $"📞 Телефон: {student.Phone}\n" +
                         $"📧 Email: {student.Email ?? "не указан"}\n" +
                         $"🎂 Дата рождения: {student.BirthDate:dd.MM.yyyy} (Возраст: {student.Age} лет)\n" +
                         $"🏠 Место рождения: {student.BirthPlace}\n" +
                         $"🌍 Гражданство: {student.Citizenship}\n" +
                         $"🚗 Категория: {student.CategoryCode}\n\n";

            if (passport != null)
                message += $"📔 Паспорт: {passport.Series} {passport.Number}\n";
            else
                message += $"📔 Паспорт: не заполнен\n";

            if (medical != null)
                message += $"🏥 Мед. справка: {medical.Series} {medical.Number} (до {medical.ValidUntil:dd.MM.yyyy})\n";
            else
                message += $"🏥 Мед. справка: не заполнена\n";

            if (snils != null)
                message += $"📄 СНИЛС: {snils.Number}\n";
            else
                message += $"📄 СНИЛС: не заполнен\n";

            if (address != null)
                message += $"🏠 Адрес: {address.FullAddress}\n";
            else
                message += $"🏠 Адрес: не заполнен\n";

            MessageBox.Show(message, $"Данные студента: {student.FullName}");
        }

        private void EditStudent_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var studentWithSelection = button?.Tag as StudentWithSelection;

            if (studentWithSelection != null)
            {
                var editDialog = new StudentEditDialog(_dataService, studentWithSelection.Student);
                if (editDialog.ShowDialog() == true)
                {
                    var index = _allStudents.Students.FindIndex(s => s.Id == studentWithSelection.Student.Id);
                    if (index >= 0)
                    {
                        _allStudents.Students[index] = editDialog.Student;
                        _dataService.SaveStudents(_allStudents);

                        LoadAllData();
                        MessageBox.Show("Данные студента обновлены", "Успех");
                    }
                }
            }
        }

        private void ViewStudentDocuments_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var studentWithSelection = button?.Tag as StudentWithSelection;

            if (studentWithSelection != null)
            {
                var student = studentWithSelection.Student;

                var message = $"📑 Документы студента: {student.FullName}\n\n" +
                             "Доступные действия:\n" +
                             "• 📝 Редактировать паспортные данные\n" +
                             "• 🏥 Добавить мед. справку\n" +
                             "• 📄 Заполнить СНИЛС\n" +
                             "• 🏠 Указать адрес регистрации\n" +
                             "• 🎓 Внести свидетельство об окончании\n\n" +
                             "Перейдите в соответствующие разделы для работы с документами.";

                MessageBox.Show(message, "Документы студента");
            }
        }

        private void FindStudentByGroup_Click(object sender, RoutedEventArgs e)
        {
            var searchWindow = new Window
            {
                Title = "Поиск студента по группам",
                Width = 600,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            var students = _dataService.LoadStudents();
            var groups = _dataService.LoadStudyGroups();

            var stackPanel = new StackPanel();
            searchWindow.Content = stackPanel;

            var searchBox = new TextBox { Margin = new Thickness(10), Padding = new Thickness(5) };
            var searchButton = new Button { Content = "Найти", Margin = new Thickness(10), Padding = new Thickness(10, 5, 10, 5) };
            var resultsList = new ListBox { Margin = new Thickness(10), Height = 200 };

            stackPanel.Children.Add(searchBox);
            stackPanel.Children.Add(searchButton);
            stackPanel.Children.Add(resultsList);

            searchButton.Click += (s, args) =>
            {
                var searchText = searchBox.Text.ToLower();
                var filteredStudents = students.Students
                    .Where(st => st.FullName.ToLower().Contains(searchText) || st.Phone.Contains(searchText))
                    .ToList();

                resultsList.Items.Clear();
                foreach (var student in filteredStudents)
                {
                    var group = groups.Groups.FirstOrDefault(g => g.Id == student.GroupId);
                    var groupInfo = group != null ? $"Группа: {group.Name}" : "Без группы";
                    resultsList.Items.Add($"{student.FullName} | {student.Phone} | {groupInfo}");
                }
            };

            searchWindow.ShowDialog();
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

            var groups = _dataService.LoadStudyGroups();

            if (_isEditMode)
            {
                var index = groups.Groups.FindIndex(g => g.Id == StudyGroup.Id);
                if (index >= 0)
                {
                    groups.Groups[index] = StudyGroup;
                }
            }
            else
            {
                groups.Groups.Add(StudyGroup);
            }

            _dataService.SaveStudyGroups(groups);

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public class StudentSelectionItem
        {
            public Student Student { get; set; }
            public bool IsSelected { get; set; }
            public string CurrentGroupName { get; set; }
            public string GroupStatus
            {
                get
                {
                    if (Student.GroupId == 0)
                        return "Без группы";
                    else
                        return "В группе";
                }
            }

            public StudentSelectionItem(Student student, string groupName)
            {
                Student = student;
                CurrentGroupName = groupName;
                IsSelected = false;
            }
        }

        private T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                T childType = child as T;

                if (childType == null)
                {
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
    }

    public class StudentWithSelection
    {
        public Student Student { get; set; }
        public bool IsSelected { get; set; }

        public StudentWithSelection(Student student)
        {
            Student = student;
            IsSelected = false;
        }
    }
}