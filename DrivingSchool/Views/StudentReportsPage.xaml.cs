using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;
using System.Collections.Generic;

namespace DrivingSchool.Views
{
    public partial class StudentReportsPage : Page
    {
        private readonly XmlDataService _dataService;
        private StudentCollection _students;
        private PaymentCollection _payments;
        private StudentTuitionCollection _tuitions;
        private StudyGroupCollection _groups;
        private StudentPassportDataCollection _passports;
        private StudentSNILSCollection _snils;
        private StudentMedicalCertificateCollection _medical;
        private StudentRegistrationAddressCollection _addresses;
        private VehicleCategoryCollection _categories;

        public StudentReportsPage(XmlDataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            LoadData();
            InitializeFilters();
            GenerateMainReport();
        }

        private void LoadData()
        {
            try
            {
                _students = _dataService.LoadStudents();
                _payments = _dataService.LoadPayments();
                _tuitions = _dataService.LoadStudentTuitions();
                _groups = _dataService.LoadStudyGroups();
                _passports = _dataService.LoadPassportData();
                _snils = _dataService.LoadSNILSData();
                _medical = _dataService.LoadMedicalData();
                _addresses = _dataService.LoadAddresses();
                _categories = _dataService.LoadVehicleCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                _students = new StudentCollection();
                _payments = new PaymentCollection();
                _tuitions = new StudentTuitionCollection();
                _groups = new StudyGroupCollection();
                _passports = new StudentPassportDataCollection();
                _snils = new StudentSNILSCollection();
                _medical = new StudentMedicalCertificateCollection();
                _addresses = new StudentRegistrationAddressCollection();
                _categories = new VehicleCategoryCollection();
            }
        }

        private void InitializeFilters()
        {
            GroupFilterComboBox.Items.Add("Все группы");
            foreach (var group in _groups.Groups)
            {
                GroupFilterComboBox.Items.Add(group.Name);
            }
            GroupFilterComboBox.SelectedIndex = 0;

            GenderFilterComboBox.Items.Add("Все");
            GenderFilterComboBox.Items.Add("Мужской");
            GenderFilterComboBox.Items.Add("Женский");
            GenderFilterComboBox.SelectedIndex = 0;

            AgeFilterComboBox.Items.Add("Все возраста");
            AgeFilterComboBox.Items.Add("16-17 лет");
            AgeFilterComboBox.Items.Add("18-25 лет");
            AgeFilterComboBox.Items.Add("26-35 лет");
            AgeFilterComboBox.Items.Add("36-45 лет");
            AgeFilterComboBox.Items.Add("Старше 45 лет");
            AgeFilterComboBox.SelectedIndex = 0;

            PaymentStatusComboBox.Items.Add("Все статусы");
            PaymentStatusComboBox.Items.Add("С задолженностью");
            PaymentStatusComboBox.Items.Add("Полностью оплачено");
            PaymentStatusComboBox.Items.Add("Не оплачено");
            PaymentStatusComboBox.Items.Add("Частично оплачено");
            PaymentStatusComboBox.SelectedIndex = 0;
        }

        private void GenerateMainReport()
        {
            var studentReports = _students.Students
                .Select(student => CreateStudentReport(student))
                .ToList();

            var filteredReports = ApplyFilters(studentReports);

            StudentsDataGrid.ItemsSource = filteredReports;
            UpdateStatistics(filteredReports);

            GenerateDemographicsReport(studentReports);
            GenerateGroupsReport(studentReports);
            GenerateDocumentsReport(studentReports);
            GeneratePaymentsReport(studentReports);
            GenerateDurationReport(studentReports);
        }

        private StudentReport CreateStudentReport(Student student)
        {
            var tuition = _tuitions.Tuitions.FirstOrDefault(t => t.StudentId == student.Id);
            var studentPayments = _payments.Payments
                .Where(p => p.StudentId == student.Id)
                .ToList();

            var totalPaid = studentPayments.Sum(p => p.Amount);
            var totalToPay = tuition?.FinalAmount ?? 0;
            var debt = totalToPay - totalPaid;

            var category = _categories.Categories.FirstOrDefault(c => c.Id == student.VehicleCategoryId);
            var group = _groups.Groups.FirstOrDefault(g => g.Id == student.GroupId);

            return new StudentReport
            {
                Id = student.Id,
                FullName = student.FullName,
                Phone = student.Phone,
                GroupName = GetGroupName(student.GroupId),
                Age = student.Age,
                Gender = student.Gender ?? "Не указан",
                Citizenship = student.Citizenship,
                CategoryCode = category?.Code ?? "Не указана",
                TotalToPay = totalToPay,
                TotalPaid = totalPaid,
                Debt = debt,
                LastPaymentDate = studentPayments.Any() ?
                    studentPayments.Max(p => p.PaymentDate) : (DateTime?)null,
                PaymentCount = studentPayments.Count,
                PaymentStatus = GetPaymentStatus(debt, totalPaid, totalToPay),
                HasPassport = _passports.Passports.Any(p => p.StudentId == student.Id),
                HasSNILS = _snils.SNILSList.Any(s => s.StudentId == student.Id),
                HasMedical = _medical.Certificates.Any(m => m.StudentId == student.Id),
                HasAddress = _addresses.Addresses.Any(a => a.StudentId == student.Id),
                StudyDuration = CalculateStudyDuration(group),
                GroupStartDate = group?.StartDate ?? DateTime.MinValue,
                GroupEndDate = group?.EndDate ?? DateTime.MinValue
            };
        }

        private string CalculateStudyDuration(StudyGroup group)
        {
            if (group == null || group.StartDate == DateTime.MinValue)
                return "Не назначена";

            var today = DateTime.Today;

            if (today < group.StartDate)
                return $"Начнется через {(group.StartDate - today).Days} дн.";

            var actualDays = (today - group.StartDate).Days;

            if (today > group.EndDate)
            {
                var courseDuration = (group.EndDate - group.StartDate).Days;
                return $"Завершено ({actualDays} дн. из {courseDuration})";
            }

            var totalCourseDays = (group.EndDate - group.StartDate).Days;
            var daysLeft = (group.EndDate - today).Days;

            return $"{actualDays} дн. ({daysLeft} дн. осталось)";
        }

        private List<StudentReport> ApplyFilters(List<StudentReport> reports)
        {
            var filtered = reports;

            if (GroupFilterComboBox.SelectedIndex > 0)
            {
                var selectedGroup = GroupFilterComboBox.SelectedItem.ToString();
                filtered = filtered.Where(r => r.GroupName == selectedGroup).ToList();
            }

            if (GenderFilterComboBox.SelectedIndex > 0)
            {
                var selectedGender = GenderFilterComboBox.SelectedItem.ToString();
                filtered = filtered.Where(r => r.Gender == selectedGender).ToList();
            }

            if (AgeFilterComboBox.SelectedIndex > 0)
            {
                int selectedIndex = AgeFilterComboBox.SelectedIndex;

                if (selectedIndex == 1)
                    filtered = filtered.Where(r => r.Age >= 16 && r.Age <= 17).ToList();
                else if (selectedIndex == 2)
                    filtered = filtered.Where(r => r.Age >= 18 && r.Age <= 25).ToList();
                else if (selectedIndex == 3)
                    filtered = filtered.Where(r => r.Age >= 26 && r.Age <= 35).ToList();
                else if (selectedIndex == 4)
                    filtered = filtered.Where(r => r.Age >= 36 && r.Age <= 45).ToList();
                else if (selectedIndex == 5)
                    filtered = filtered.Where(r => r.Age > 45).ToList();
            }

            return filtered;
        }

        private void UpdateStatistics(List<StudentReport> reports)
        {
            TotalStudentsText.Text = reports.Count.ToString();
            MaleCountText.Text = reports.Count(r => r.Gender == "Мужской").ToString();
            FemaleCountText.Text = reports.Count(r => r.Gender == "Женский").ToString();

            if (reports.Any())
            {
                AverageAgeText.Text = reports.Average(r => r.Age).ToString("N1");
            }
            else
            {
                AverageAgeText.Text = "0";
            }

            var withDocuments = reports.Count(r =>
                r.HasPassport && r.HasSNILS && r.HasMedical && r.HasAddress);
            WithDocumentsText.Text = withDocuments.ToString();
        }

        private void GenerateDemographicsReport(List<StudentReport> reports)
        {
            var ageGroups = new[]
            {
                new { Range = "16-17 лет", Min = 16, Max = 17 },
                new { Range = "18-25 лет", Min = 18, Max = 25 },
                new { Range = "26-35 лет", Min = 26, Max = 35 },
                new { Range = "36-45 лет", Min = 36, Max = 45 },
                new { Range = "Старше 45", Min = 46, Max = 100 }
            };

            var demographics = new List<object>();

            foreach (var ageGroup in ageGroups)
            {
                var groupStudents = reports.Where(r => r.Age >= ageGroup.Min && r.Age <= ageGroup.Max).ToList();
                if (groupStudents.Any())
                {
                    var maleCount = groupStudents.Count(s => s.Gender == "Мужской");
                    var femaleCount = groupStudents.Count(s => s.Gender == "Женский");
                    var unknownCount = groupStudents.Count(s => s.Gender != "Мужской" && s.Gender != "Женский");

                    demographics.Add(new
                    {
                        ВозрастнаяГруппа = ageGroup.Range,
                        Количество = groupStudents.Count,
                        Мужчины = maleCount,
                        Женщины = femaleCount,
                        НеУказан = unknownCount,
                        Процент = $"{((double)groupStudents.Count / reports.Count * 100):N1}%"
                    });
                }
            }

            DemographicsDataGrid.ItemsSource = demographics;
        }

        private void GenerateGroupsReport(List<StudentReport> reports)
        {
            var groupsReport = _groups.Groups
                .Select(group =>
                {
                    var groupStudents = reports.Where(r => r.GroupName == group.Name).ToList();
                    return new
                    {
                        Группа = group.Name,
                        КоличествоСтудентов = groupStudents.Count,
                        СтатусГруппы = group.Status,
                        Мужчины = groupStudents.Count(s => s.Gender == "Мужской"),
                        Женщины = groupStudents.Count(s => s.Gender == "Женский"),
                        СреднийВозраст = groupStudents.Any() ? groupStudents.Average(s => s.Age).ToString("N1") : "0",
                        НачалоОбучения = group.StartDate.ToString("dd.MM.yyyy"),
                        КонецОбучения = group.EndDate.ToString("dd.MM.yyyy")
                    };
                })
                .Where(g => g.КоличествоСтудентов > 0)
                .OrderBy(g => g.Группа)
                .ToList();

            GroupsDataGrid.ItemsSource = groupsReport;
        }

        private void GenerateDocumentsReport(List<StudentReport> reports)
        {
            var documentsReport = reports
                .Select(r => new
                {
                    r.Id,
                    r.FullName,
                    r.GroupName,
                    Паспорт = r.HasPassport ? "✅" : "❌",
                    СНИЛС = r.HasSNILS ? "✅" : "❌",
                    МедСправка = r.HasMedical ? "✅" : "❌",
                    Адрес = r.HasAddress ? "✅" : "❌",
                    ВсегоДокументов = GetDocumentsCount(r),
                    Статус = GetDocumentsStatus(r)
                })
                .OrderByDescending(r => r.ВсегоДокументов)
                .ThenBy(r => r.FullName)
                .ToList();

            DocumentsDataGrid.ItemsSource = documentsReport;
        }

        private void GeneratePaymentsReport(List<StudentReport> reports)
        {
            var filteredReports = reports.Where(r => r.TotalToPay > 0);

            if (PaymentStatusComboBox.SelectedIndex > 0)
            {
                var selectedStatus = PaymentStatusComboBox.SelectedItem.ToString();
                filteredReports = filteredReports.Where(r => r.PaymentStatus == selectedStatus);
            }

            var paymentsReport = filteredReports
                .Select(r => new
                {
                    r.Id,
                    r.FullName,
                    r.GroupName,
                    ВсегоКОплате = r.TotalToPay,
                    Оплачено = r.TotalPaid,
                    Задолженность = r.Debt,
                    СтатусОплаты = r.PaymentStatus,
                    ПоследняяОплата = r.LastPaymentDate?.ToString("dd.MM.yyyy") ?? "Не было",
                    КоличествоОплат = r.PaymentCount
                })
                .ToList();

            PaymentsDataGrid.ItemsSource = paymentsReport;
        }

        private void GenerateDurationReport(List<StudentReport> reports)
        {
            var durationReport = reports
                .Select(r => new
                {
                    r.Id,
                    r.FullName,
                    r.GroupName,
                    НачалоОбучения = r.GroupStartDate.ToString("dd.MM.yyyy"),
                    КонецОбучения = r.GroupEndDate.ToString("dd.MM.yyyy"),
                    Продолжительность = r.StudyDuration,
                    ФактическоДней = CalculateActualDays(r.GroupStartDate, r.GroupEndDate),
                    Статус = GetStudyStatus(r.GroupStartDate, r.GroupEndDate)
                })
                .OrderBy(r => r.НачалоОбучения)
                .ToList();

            DurationDataGrid.ItemsSource = durationReport;
        }
        private int CalculateActualDays(DateTime startDate, DateTime endDate)
        {
            if (startDate == DateTime.MinValue)
                return 0;

            var today = DateTime.Today;

            if (today < startDate)
                return 0;

            if (today > endDate)
                return (endDate - startDate).Days;

            return (today - startDate).Days;
        }

        private string GetStudyStatus(DateTime startDate, DateTime endDate)
        {
            var today = DateTime.Today;

            if (startDate == DateTime.MinValue)
                return "Не назначено";
            if (today < startDate)
                return "Не началось";
            if (today > endDate)
                return "Завершено";
            return "В процессе";
        }

        private void PaymentStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var studentReports = _students.Students
                .Select(student => CreateStudentReport(student))
                .ToList();

            GeneratePaymentsReport(studentReports);
        }

        private int GetDocumentsCount(StudentReport report)
        {
            int count = 0;
            if (report.HasPassport) count++;
            if (report.HasSNILS) count++;
            if (report.HasMedical) count++;
            if (report.HasAddress) count++;
            return count;
        }

        private string GetDocumentsStatus(StudentReport report)
        {
            int count = GetDocumentsCount(report);

            if (count == 4)
                return "Все документы";
            else if (count == 3)
                return "Не хватает 1 документа";
            else if (count == 2)
                return "Не хватает 2 документов";
            else if (count == 1)
                return "Не хватает 3 документов";
            else if (count == 0)
                return "Нет документов";
            else
                return "Неизвестно";
        }

        private string GetGroupName(int groupId)
        {
            var group = _groups.Groups.FirstOrDefault(g => g.Id == groupId);
            return group?.Name ?? "Не назначена";
        }

        private string GetPaymentStatus(decimal debt, decimal totalPaid, decimal totalToPay)
        {
            if (totalToPay == 0) return "Нет стоимости";
            if (debt <= 0) return "Полностью оплачено";
            if (totalPaid == 0) return "Не оплачено";
            return "Частично оплачено";
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GenerateMainReport();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
            GenerateMainReport();
            MessageBox.Show("Данные обновлены", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class StudentReport
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string GroupName { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Citizenship { get; set; }
        public string CategoryCode { get; set; }
        public decimal TotalToPay { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Debt { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public int PaymentCount { get; set; }
        public string PaymentStatus { get; set; }
        public bool HasPassport { get; set; }
        public bool HasSNILS { get; set; }
        public bool HasMedical { get; set; }
        public bool HasAddress { get; set; }
        public string StudyDuration { get; set; }
        public DateTime GroupStartDate { get; set; }
        public DateTime GroupEndDate { get; set; }
    }
}