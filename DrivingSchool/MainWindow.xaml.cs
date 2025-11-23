using DrivingSchool.Services;
using DrivingSchool.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace DrivingSchool
{
    public partial class MainWindow : Window
    {
        private readonly XmlDataService _dataService;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                _dataService = new XmlDataService();
                InitializeApplication();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске: {ex.Message}", "Ошибка");
                throw;
            }
        }

        private void InitializeApplication()
        {
            UpdateDateTime();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => UpdateDateTime();
            timer.Start();

            ShowStudents();
        }

        private void UpdateDateTime()
        {
            DateTimeText.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        }

        private void ShowStudents()
        {
            try
            {
                var studentsPage = new Views.StudentsPage(_dataService);
                MainContentFrame.Navigate(studentsPage);
                StatusText.Text = "Управление учащимися";
            }            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки страницы учащихся: {ex.Message}", "Ошибка");
            }
        }

        private void ShowContracts()
        {
            StatusText.Text = "Договоры на обучение";
            MessageBox.Show("Раздел 'Договоры' в разработке");
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Панель управления";
            MessageBox.Show("Панель управления в разработке");
        }

        private void ShowGroups()
        {
            try
            {
                var groupsPage = new Views.GroupsPage(_dataService);
                MainContentFrame.Navigate(groupsPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки групп: {ex.Message}", "Ошибка");
            }
        }

        private void Templates_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var templatesPage = new Views.TemplatesPage(_dataService);
                MainContentFrame.Navigate(templatesPage);
                StatusText.Text = "Настройка шаблонов документов";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки шаблонов: {ex.Message}", "Ошибка");
            }
        }

        private void Students_Click(object sender, RoutedEventArgs e) => ShowStudents();
        private void Groups_Click(object sender, RoutedEventArgs e) => ShowGroups();
        private void Contracts_Click(object sender, RoutedEventArgs e) => ShowContracts();
        private void GIBDD_Applications_Click(object sender, RoutedEventArgs e) => ShowContracts();

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void Payments_Click(object sender, RoutedEventArgs e)
        {
            ShowPayments();
        }

        private void ShowPayments()
        {
            try
            {
                var paymentsPage = new Views.PaymentsPage(_dataService);
                MainContentFrame.Navigate(paymentsPage);
                StatusText.Text = "Управление оплатами";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки оплат: {ex.Message}", "Ошибка");
            }
        }

        private void DebtsReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Раздел 'Ведомость по задолженностям' в разработке");
        }

        private void StudentReports_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Раздел 'Отчеты по учащимся' в разработке");
        }

        private void Backup_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция 'Резервное копирование' в разработке");
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Раздел 'Настройки' в разработке");
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Элита Авто: Автоматизированная система управления\nВерсия 1.0");
        }

        private void NewContract_Click(object sender, RoutedEventArgs e)
        {
            ShowContracts();
        }

        private void QuickReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция 'Быстрый отчет' в разработке");
        }

        private void PassportData_Click(object sender, RoutedEventArgs e)
        {
            ShowPassportData();
        }

        private void ShowPassportData()
        {
            try
            {
                var passportPage = new Views.PassportDataPage(_dataService);
                MainContentFrame.Navigate(passportPage);
                StatusText.Text = "Управление паспортными данными";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки паспортных данных: {ex.Message}", "Ошибка");
            }
        }

        private void SNILSData_Click(object sender, RoutedEventArgs e)
        {
            ShowSNILSData();
        }

        private void MedicalData_Click(object sender, RoutedEventArgs e)
        {
            ShowMedicalData();
        }

        private void ShowSNILSData()
        {
            try
            {
                var snilsPage = new Views.SNILSPage(_dataService);
                MainContentFrame.Navigate(snilsPage);
                StatusText.Text = "Управление данными СНИЛС";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных СНИЛС: {ex.Message}", "Ошибка");
            }
        }

        private void ShowMedicalData()
        {
            try
            {
                var medicalPage = new Views.MedicalPage(_dataService);
                MainContentFrame.Navigate(medicalPage);
                StatusText.Text = "Управление медицинскими справками";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки медицинских справок: {ex.Message}", "Ошибка");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите выйти из программы?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                MaximizeButton.ToolTip = "Развернуть";
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                MaximizeButton.ToolTip = "Восстановить";
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeButton_Click(sender, e);
            }
            else
            {
                this.DragMove();
            }
        }

        private void Rates_Click(object sender, RoutedEventArgs e)
        {
            ShowTariffs();
        }

        private void ShowTariffs()
        {
            try
            {
                var tariffsPage = new Views.TariffsPage(_dataService);
                MainContentFrame.Navigate(tariffsPage);
                StatusText.Text = "Управление тарифами обучения";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки тарифов: {ex.Message}", "Ошибка");
            }
        }

        private void Employees_Click(object sender, RoutedEventArgs e)
        {
            ShowEmployees();
        }

        private void ShowEmployees()
        {
            try
            {
                var employeesPage = new Views.EmployeesPage(_dataService);
                MainContentFrame.Navigate(employeesPage);
                StatusText.Text = "Управление сотрудниками";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}", "Ошибка");
            }
        }

        private void AddressData_Click(object sender, RoutedEventArgs e)
        {
            ShowAddressData();
        }

        private void ShowAddressData()
        {
            try
            {
                var addressPage = new Views.AddressDataPage(_dataService);
                MainContentFrame.Navigate(addressPage);
                StatusText.Text = "Управление адресами регистрации";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки адресов регистрации: {ex.Message}", "Ошибка");
            }
        }

        private void CertificateData_Click(object sender, RoutedEventArgs e)
        {
            ShowCertificateData();
        }

        private void ShowCertificateData()
        {
            try
            {
                var certificatePage = new Views.CertificateDataPage(_dataService);
                MainContentFrame.Navigate(certificatePage);
                StatusText.Text = "Управление свидетельствами об окончании";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки свидетельств: {ex.Message}", "Ошибка");
            }
        }

        private void Categories_Click(object sender, RoutedEventArgs e)
        {
            ShowCategories();
        }

        private void ShowCategories()
        {
            try
            {
                var categoriesPage = new Views.CategoriesPage(_dataService);
                MainContentFrame.Navigate(categoriesPage);
                StatusText.Text = "Управление категориями ТС";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка");
            }
        }

        private void DrivingLicenseData_Click(object sender, RoutedEventArgs e)
        {
            ShowDrivingLicenseData();
        }

        private void ShowDrivingLicenseData()
        {
            try
            {
                var licensePage = new Views.DrivingLicenseDataPage(_dataService);
                MainContentFrame.Navigate(licensePage);
                StatusText.Text = "Управление водительскими удостоверениями";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки водительских удостоверений: {ex.Message}", "Ошибка");
            }
        }

        private void FinancialReports_Click(object sender, RoutedEventArgs e)
        {
            ShowFinancialReports();
        }

        private void ShowFinancialReports()
        {
            try
            {
                var reportsPage = new Views.FinancialReportsPage(_dataService);
                MainContentFrame.Navigate(reportsPage);
                StatusText.Text = "Финансовые отчеты";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки финансовых отчетов: {ex.Message}", "Ошибка");
            }
        }
    }
}