using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class AddressDataPage : Page
    {
        private readonly XmlDataService _dataService;
        private StudentCollection _students;
        private StudentRegistrationAddressCollection _addresses;
        private StudentRegistrationAddressCollection _filteredAddresses;
        private Student _selectedStudent;

        public AddressDataPage(XmlDataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _students = _dataService.LoadStudents();
                _addresses = _dataService.LoadAddresses();

                if (_students?.Students == null) _students = new StudentCollection();
                if (_addresses?.Addresses == null) _addresses = new StudentRegistrationAddressCollection();

                ApplyFilter();
                UpdateUIForSelectedStudent(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                _students = new StudentCollection();
                _addresses = new StudentRegistrationAddressCollection();
                ApplyFilter();
                UpdateUIForSelectedStudent(null);
            }
        }

        private string GetStudentName(int studentId)
        {
            var student = _students.Students.FirstOrDefault(s => s.Id == studentId);
            return student?.FullName ?? "Неизвестный студент";
        }

        private void UpdateUIForSelectedStudent(Student student)
        {
            if (student == null)
            {
                StatusPanel.Visibility = Visibility.Collapsed;
                AddAddressButton.IsEnabled = false;
                EditAddressButton.IsEnabled = false;
                DeleteAddressButton.IsEnabled = false;
                return;
            }

            var existingAddress = _addresses.Addresses.FirstOrDefault(a => a.StudentId == student.Id);

            if (existingAddress != null)
            {
                StatusPanel.Visibility = Visibility.Visible;
                StatusTextBlock.Text = $"Для учащегося {student.FullName} уже внесен адрес регистрации. " +
                                      $"Используйте функции редактирования или удаления.";

                AddAddressButton.IsEnabled = false;
                EditAddressButton.IsEnabled = true;
                DeleteAddressButton.IsEnabled = true;
            }
            else
            {
                StatusPanel.Visibility = Visibility.Collapsed;

                AddAddressButton.IsEnabled = true;
                EditAddressButton.IsEnabled = false;
                DeleteAddressButton.IsEnabled = false;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text?.ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                SearchResultsListBox.Visibility = Visibility.Collapsed;
                return;
            }

            var results = _students.Students
                .Where(s => (s.FullName ?? "").ToLower().Contains(searchText) ||
                           (s.Phone ?? "").Contains(searchText) ||
                           s.Id.ToString().Contains(searchText))
                .Take(10)
                .ToList();

            if (results.Any())
            {
                SearchResultsListBox.ItemsSource = results;
                SearchResultsListBox.Visibility = Visibility.Visible;
            }
            else
            {
                SearchResultsListBox.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsListBox.SelectedItem is Student selectedStudent)
            {
                _selectedStudent = selectedStudent;
                UpdateSelectedStudentPanel();
                SearchResultsListBox.Visibility = Visibility.Collapsed;
                SearchTextBox.Text = string.Empty;
                ApplyFilter();
            }
        }

        private void UpdateSelectedStudentPanel()
        {
            if (_selectedStudent != null)
            {
                SelectedStudentPanel.Visibility = Visibility.Visible;
                SelectedStudentText.Text = _selectedStudent.FullName;
                SelectedStudentDetails.Text = $"Телефон: {_selectedStudent.Phone} | " +
                                            $"Дата рождения: {_selectedStudent.BirthDate:dd.MM.yyyy} | " +
                                            $"ID: {_selectedStudent.Id}";
            }
            else
            {
                SelectedStudentPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ClearSelectedStudent_Click(object sender, RoutedEventArgs e)
        {
            _selectedStudent = null;
            UpdateSelectedStudentPanel();
            ApplyFilter();
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            SearchResultsListBox.Visibility = Visibility.Collapsed;
        }

        private void ApplyFilter()
        {
            if (_selectedStudent != null)
            {
                _filteredAddresses = new StudentRegistrationAddressCollection
                {
                    Addresses = _addresses.Addresses
                        .Where(a => a.StudentId == _selectedStudent.Id)
                        .ToList()
                };
            }
            else
            {
                _filteredAddresses = new StudentRegistrationAddressCollection
                {
                    Addresses = _addresses.Addresses.ToList()
                };
            }

            AddressGrid.ItemsSource = _filteredAddresses.Addresses;

            UpdateStudentNames();
            UpdateButtonsAvailability();
            UpdateStatus();
        }

        private void UpdateStudentNames()
        {
            foreach (var address in _filteredAddresses.Addresses)
            {
                address.StudentName = GetStudentName(address.StudentId);
            }
        }

        private void UpdateButtonsAvailability()
        {
            if (_selectedStudent == null)
            {
                AddAddressButton.IsEnabled = false;
                EditAddressButton.IsEnabled = false;
                DeleteAddressButton.IsEnabled = false;

                StatusPanel.Visibility = Visibility.Visible;
                StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 245, 233));
                StatusTextBlock.Text = $"Всего адресов в базе: {_addresses.Addresses.Count}. Выберите студента для работы с конкретным адресом.";
                return;
            }

            var hasAddressData = _addresses.Addresses.Any(a => a.StudentId == _selectedStudent.Id);
            var isAddressSelected = AddressGrid.SelectedItem != null;

            AddAddressButton.IsEnabled = !hasAddressData;
            EditAddressButton.IsEnabled = hasAddressData && isAddressSelected;
            DeleteAddressButton.IsEnabled = hasAddressData && isAddressSelected;

            UpdateStatusPanel();
        }

        private void UpdateStatusPanel()
        {
            if (_selectedStudent == null)
            {
                StatusPanel.Visibility = Visibility.Collapsed;
                return;
            }

            var hasAddressData = _addresses.Addresses.Any(a => a.StudentId == _selectedStudent.Id);

            if (hasAddressData)
            {
                StatusPanel.Visibility = Visibility.Visible;
                StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 245, 233));
                StatusPanel.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 230, 201));
                StatusTextBlock.Text = $"✅ Для учащегося {_selectedStudent.FullName} внесен адрес регистрации. " +
                                      $"Выберите запись для редактирования или удаления.";
            }
            else
            {
                StatusPanel.Visibility = Visibility.Visible;
                StatusPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 243, 205));
                StatusPanel.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 238, 186));
                StatusTextBlock.Text = $"ℹ️ Для учащегося {_selectedStudent.FullName} адрес регистрации отсутствует. " +
                                      $"Нажмите 'Добавить адрес' для внесения данных.";
            }
        }

        private void AddressGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonsAvailability();
        }

        private void UpdateStatus()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                if (_selectedStudent != null)
                {
                    var existingAddress = _addresses.Addresses.FirstOrDefault(a => a.StudentId == _selectedStudent.Id);
                    var status = existingAddress != null ? "✅ данные внесены" : "❌ данные отсутствуют";
                    mainWindow.StatusText.Text = $"Адрес регистрации: {_selectedStudent.FullName} ({status})";
                }
                else
                {
                    mainWindow.StatusText.Text = $"Адреса регистрации: всего {_addresses.Addresses.Count} записей";
                }
            }
        }

        private void AddAddress_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent != null)
            {
                var existingAddress = _addresses.Addresses.FirstOrDefault(a => a.StudentId == _selectedStudent.Id);
                if (existingAddress != null)
                {
                    MessageBox.Show($"Для учащегося {_selectedStudent.FullName} уже существует адрес регистрации.\n\n" +
                                   $"Используйте функцию редактирования для изменения данных.",
                                   "Адрес регистрации уже существует",
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dialog = new AddressEditDialog(_dataService, _selectedStudent.Id);
                if (dialog.ShowDialog() == true)
                {
                    LoadData();
                    MessageBox.Show($"Адрес регистрации успешно добавлен!", "Успех");
                }
            }
            else
            {
                MessageBox.Show("Выберите учащегося", "Предупреждение");
            }
        }

        private void EditAddress_Click(object sender, RoutedEventArgs e)
        {
            if (AddressGrid.SelectedItem is StudentRegistrationAddress selectedAddress)
            {
                var dialog = new AddressEditDialog(_dataService, selectedAddress.StudentId, selectedAddress);
                if (dialog.ShowDialog() == true)
                {
                    LoadData();
                    MessageBox.Show($"Адрес регистрации обновлен!", "Успех");
                }
            }
            else
            {
                MessageBox.Show("Выберите запись адреса для редактирования", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteAddress_Click(object sender, RoutedEventArgs e)
        {
            if (AddressGrid.SelectedItem is StudentRegistrationAddress selectedAddress)
            {
                var student = _students.Students.FirstOrDefault(s => s.Id == selectedAddress.StudentId);
                var studentName = student?.FullName ?? "неизвестный студент";

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить адрес регистрации?\n\n" +
                    $"Студент: {studentName}\n" +
                    $"Адрес: {selectedAddress.FullAddress}\n\n" +
                    $"Это действие нельзя отменить!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _addresses.Addresses.Remove(selectedAddress);
                    _dataService.SaveAddresses(_addresses);
                    ApplyFilter();
                    MessageBox.Show($"Адрес регистрации удален.", "Успех");
                }
            }
            else
            {
                MessageBox.Show("Выберите запись адреса для удаления", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ViewAddress_Click(object sender, RoutedEventArgs e)
        {
            if (AddressGrid.SelectedItem is StudentRegistrationAddress selectedAddress)
            {
                var student = _students.Students.FirstOrDefault(s => s.Id == selectedAddress.StudentId);
                var studentName = student?.FullName ?? "Неизвестный студент";

                MessageBox.Show($"Адрес регистрации:\n\n" +
                               $"Студент: {studentName}\n" +
                               $"Полный адрес: {selectedAddress.FullAddress}\n" +
                               $"Регион: {selectedAddress.Region}\n" +
                               $"Населенный пункт: {selectedAddress.City}\n" +
                               $"Улица: {selectedAddress.Street}\n" +
                               $"Дом: {selectedAddress.House}\n" +
                               $"Корпус: {selectedAddress.Building}\n" +
                               $"Квартира: {selectedAddress.Apartment}\n" +
                               "Просмотр адреса регистрации");
            }
            else
            {
                MessageBox.Show("Выберите запись для просмотра", "Предупреждение");
            }
        }

        private void PrintAddress_Click(object sender, RoutedEventArgs e)
        {
            if (AddressGrid.SelectedItem is StudentRegistrationAddress selectedAddress)
            {
                MessageBox.Show($"Печать адреса регистрации:\n{selectedAddress.FullAddress}",
                    "Печать");
            }
            else
            {
                MessageBox.Show("Выберите запись для печати", "Предупреждение");
            }
        }

        private void AddressGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

            if (row != null && row.DataContext is StudentRegistrationAddress address)
            {
                EditAddress_Click(sender, e);
            }
        }
    }
}