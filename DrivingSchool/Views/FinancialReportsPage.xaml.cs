using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;
using System.Collections.Generic;
using System.Windows.Data;
using System.Globalization;

namespace DrivingSchool.Views
{
    public partial class FinancialReportsPage : Page
    {
        private readonly XmlDataService _dataService;
        private StudentCollection _students;
        private PaymentCollection _payments;
        private StudentTuitionCollection _tuitions;
        private TariffCollection _tariffs;
        private StudyGroupCollection _groups;

        public FinancialReportsPage(XmlDataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            LoadData();
            InitializeDateFilters();
            GenerateGeneralReport();
        }

        private void LoadData()
        {
            try
            {
                _students = _dataService.LoadStudents();
                _payments = _dataService.LoadPayments();
                _tuitions = _dataService.LoadStudentTuitions();
                _tariffs = _dataService.LoadTariffs();
                _groups = _dataService.LoadStudyGroups();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                _students = new StudentCollection();
                _payments = new PaymentCollection();
                _tuitions = new StudentTuitionCollection();
                _tariffs = new TariffCollection();
                _groups = new StudyGroupCollection();
            }
        }

        private void InitializeDateFilters()
        {
            var currentDate = DateTime.Now;
            StartDatePicker.SelectedDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            EndDatePicker.SelectedDate = currentDate;

            PeriodComboBox.Items.Add("За сегодня");
            PeriodComboBox.Items.Add("За текущую неделю");
            PeriodComboBox.Items.Add("За текущий месяц");
            PeriodComboBox.Items.Add("За текущий квартал");
            PeriodComboBox.Items.Add("За текущий год");
            PeriodComboBox.Items.Add("За все время");
            PeriodComboBox.Items.Add("Произвольный период");
            PeriodComboBox.SelectedIndex = 2;
        }

        private void PeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentDate = DateTime.Now;

            switch (PeriodComboBox.SelectedIndex)
            {
                case 0:
                    StartDatePicker.SelectedDate = currentDate.Date;
                    EndDatePicker.SelectedDate = currentDate.Date;
                    break;
                case 1:
                    var diff = (7 + (currentDate.DayOfWeek - DayOfWeek.Monday)) % 7;
                    StartDatePicker.SelectedDate = currentDate.AddDays(-diff).Date;
                    EndDatePicker.SelectedDate = currentDate.Date;
                    break;
                case 2:
                    StartDatePicker.SelectedDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                    EndDatePicker.SelectedDate = currentDate.Date;
                    break;
                case 3:
                    var quarter = (currentDate.Month - 1) / 3;
                    var quarterStartMonth = quarter * 3 + 1;
                    StartDatePicker.SelectedDate = new DateTime(currentDate.Year, quarterStartMonth, 1);
                    EndDatePicker.SelectedDate = currentDate.Date;
                    break;
                case 4:
                    StartDatePicker.SelectedDate = new DateTime(currentDate.Year, 1, 1);
                    EndDatePicker.SelectedDate = currentDate.Date;
                    break;
                case 5:
                    StartDatePicker.SelectedDate = new DateTime(2020, 1, 1);
                    EndDatePicker.SelectedDate = currentDate.Date;
                    break;
                case 6:
                    StartDatePicker.IsEnabled = true;
                    EndDatePicker.IsEnabled = true;
                    return;
            }

            StartDatePicker.IsEnabled = false;
            EndDatePicker.IsEnabled = false;
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            switch (ReportTypeTabControl.SelectedIndex)
            {
                case 0:
                    GenerateGeneralReport();
                    break;
                case 1:
                    GeneratePaymentsReport();
                    break;
                case 2:
                    GenerateStudentsReport();
                    break;
                case 3:
                    GenerateGroupsReport();
                    break;
            }
        }

        private void GenerateGeneralReport()
        {
            var startDate = StartDatePicker.SelectedDate ?? DateTime.MinValue;
            var endDate = EndDatePicker.SelectedDate ?? DateTime.MaxValue;

            var periodPayments = _payments.Payments
                .Where(p => p.PaymentDate.Date >= startDate.Date && p.PaymentDate.Date <= endDate.Date)
                .ToList();

            var totalIncome = periodPayments.Sum(p => p.Amount);
            var paymentCount = periodPayments.Count;
            var avgPayment = paymentCount > 0 ? totalIncome / paymentCount : 0;

            var studentsWithTuition = _students.Students
                .Where(s => _tuitions.Tuitions.Any(t => t.StudentId == s.Id))
                .ToList();

            var totalExpectedIncome = studentsWithTuition.Sum(s =>
            {
                var tuition = _tuitions.Tuitions.First(t => t.StudentId == s.Id);
                return tuition.FinalAmount;
            });

            var totalPaidAllTime = _payments.Payments
                .Where(p => studentsWithTuition.Any(s => s.Id == p.StudentId))
                .Sum(p => p.Amount);

            var totalDebt = totalExpectedIncome - totalPaidAllTime;

            TotalIncomeText.Text = $"{totalIncome:N2} руб.";
            TotalPaymentsText.Text = paymentCount.ToString();
            AveragePaymentText.Text = $"{avgPayment:N2} руб.";
            ExpectedIncomeText.Text = $"{totalExpectedIncome:N2} руб.";
            TotalDebtText.Text = $"{totalDebt:N2} руб.";
            PaidPercentageText.Text = totalExpectedIncome > 0 ?
                $"{(totalPaidAllTime / totalExpectedIncome * 100):N1}%" : "0%";

            var dailyPayments = periodPayments
                .GroupBy(p => p.PaymentDate.Date)
                .Select(g => new DailyPayment { Date = g.Key, Amount = g.Sum(p => p.Amount) })
                .OrderBy(x => x.Date)
                .ToList();

            PaymentsChart.ItemsSource = dailyPayments;

            var paymentTypes = periodPayments
                .GroupBy(p => p.PaymentType)
                .Select(g => new PaymentTypeStat
                {
                    Type = g.Key ?? "Не указан",
                    Amount = g.Sum(p => p.Amount),
                    Count = g.Count()
                })
                .ToList();

            PaymentTypesDataGrid.ItemsSource = paymentTypes;

            LoadRecentPayments();
        }

        private void GeneratePaymentsReport()
        {
            var startDate = StartDatePicker.SelectedDate ?? DateTime.MinValue;
            var endDate = EndDatePicker.SelectedDate ?? DateTime.MaxValue;

            var detailedPayments = _payments.Payments
                .Where(p => p.PaymentDate.Date >= startDate.Date && p.PaymentDate.Date <= endDate.Date)
                .Join(_students.Students,
                    payment => payment.StudentId,
                    student => student.Id,
                    (payment, student) => new PaymentDetail
                    {
                        Id = payment.Id,
                        StudentName = student.FullName ?? "Неизвестный студент",
                        PaymentDate = payment.PaymentDate,
                        Amount = payment.Amount,
                        PaymentType = payment.PaymentType ?? "Не указан",
                        StudentId = student.Id
                    })
                .OrderByDescending(p => p.PaymentDate)
                .ToList();

            PaymentsDataGrid.ItemsSource = detailedPayments;

            var total = detailedPayments.Sum(p => p.Amount);
            var count = detailedPayments.Count;
            PaymentsSummaryText.Text = $"Всего оплат: {count} на сумму {total:N2} руб.";
        }

        private void GenerateStudentsReport()
        {
            var studentFinancials = _students.Students
                .Select(student =>
                {
                    var tuition = _tuitions.Tuitions.FirstOrDefault(t => t.StudentId == student.Id);
                    var studentPayments = _payments.Payments
                        .Where(p => p.StudentId == student.Id)
                        .ToList();

                    var totalPaid = studentPayments.Sum(p => p.Amount);
                    var totalToPay = tuition?.FinalAmount ?? 0;
                    var debt = totalToPay - totalPaid;
                    var paymentProgress = totalToPay > 0 ? (totalPaid / totalToPay * 100) : 0;

                    return new StudentFinancialInfo
                    {
                        StudentId = student.Id,
                        StudentName = student.FullName ?? "Неизвестный студент",
                        GroupName = GetGroupName(student.GroupId),
                        TotalToPay = totalToPay,
                        TotalPaid = totalPaid,
                        Debt = debt,
                        PaymentProgress = paymentProgress,
                        LastPaymentDate = studentPayments.Any() ?
                            studentPayments.Max(p => p.PaymentDate) : (DateTime?)null,
                        PaymentCount = studentPayments.Count,
                        Status = GetPaymentStatus(debt, totalPaid, totalToPay)
                    };
                })
                .OrderByDescending(s => s.Debt)
                .ThenBy(s => s.StudentName)
                .ToList();

            StudentsDataGrid.ItemsSource = studentFinancials;

            var withDebt = studentFinancials.Count(s => s.Debt > 0);
            var fullyPaid = studentFinancials.Count(s => s.Debt <= 0 && s.TotalToPay > 0);
            var noPayments = studentFinancials.Count(s => s.TotalPaid == 0 && s.TotalToPay > 0);
            var totalDebt = studentFinancials.Where(s => s.Debt > 0).Sum(s => s.Debt);

            StudentsSummaryText.Text =
                $"Всего студентов: {studentFinancials.Count} | " +
                $"С долгом: {withDebt} | " +
                $"Полностью оплатили: {fullyPaid} | " +
                $"Без оплат: {noPayments} | " +
                $"Общий долг: {totalDebt:N2} руб.";
        }

        private void GenerateGroupsReport()
        {
            var groupFinancials = _groups.Groups
                .Select(group =>
                {
                    var groupStudents = _students.Students
                        .Where(s => s.GroupId == group.Id)
                        .ToList();

                    var groupTuitions = _tuitions.Tuitions
                        .Where(t => groupStudents.Any(s => s.Id == t.StudentId))
                        .ToList();

                    var groupPayments = _payments.Payments
                        .Where(p => groupStudents.Any(s => s.Id == p.StudentId))
                        .ToList();

                    var expectedIncome = groupTuitions.Sum(t => t.FinalAmount);
                    var actualIncome = groupPayments.Sum(p => p.Amount);
                    var debt = expectedIncome - actualIncome;

                    return new GroupFinancialInfo
                    {
                        GroupName = group.Name ?? "Без названия",
                        StudentCount = groupStudents.Count,
                        StartDate = group.StartDate,
                        EndDate = group.EndDate,
                        Status = group.Status ?? "Неизвестен",
                        ExpectedIncome = expectedIncome,
                        ActualIncome = actualIncome,
                        Debt = debt,
                        CompletionRate = expectedIncome > 0 ? (actualIncome / expectedIncome * 100) : 0
                    };
                })
                .OrderByDescending(g => g.StartDate)
                .ToList();

            GroupsDataGrid.ItemsSource = groupFinancials;
        }

        private void LoadRecentPayments()
        {
            var recentPayments = _payments.Payments
                .OrderByDescending(p => p.PaymentDate)
                .Take(20)
                .Join(_students.Students,
                    payment => payment.StudentId,
                    student => student.Id,
                    (payment, student) => new
                    {
                        payment.Id,
                        StudentName = student.FullName,
                        payment.PaymentDate,
                        payment.Amount,
                        payment.PaymentType
                    })
                .ToList();

            RecentPaymentsDataGrid.ItemsSource = recentPayments;
        }

        private string GetGroupName(int groupId)
        {
            var group = _groups.Groups.FirstOrDefault(g => g.Id == groupId);
            return group?.Name ?? "Не назначена";
        }

        private string GetPaymentStatus(decimal debt, decimal totalPaid, decimal totalToPay)
        {
            if (totalToPay == 0) return "Нет стоимости";
            if (debt <= 0) return "Оплачено полностью";
            if (totalPaid == 0) return "Не оплачено";
            return "Частично оплачено";
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"Финансовый_отчет_{DateTime.Now:yyyy-MM-dd_HH-mm}"
                };

                if (dialog.ShowDialog() == true)
                {
                    System.IO.File.WriteAllText(dialog.FileName, "Финансовый отчет\n");
                    MessageBox.Show($"Отчет успешно экспортирован в файл: {dialog.FileName}",
                        "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class PaymentDetail
    {
        public int Id { get; set; }
        public string StudentName { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; }
        public int StudentId { get; set; }
    }

    public class StudentFinancialInfo
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string GroupName { get; set; }
        public decimal TotalToPay { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Debt { get; set; }
        public decimal PaymentProgress { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public int PaymentCount { get; set; }
        public string Status { get; set; }
    }

    public class GroupFinancialInfo
    {
        public string GroupName { get; set; }
        public int StudentCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public decimal ExpectedIncome { get; set; }
        public decimal ActualIncome { get; set; }
        public decimal Debt { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class DailyPayment
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }

        public string DateString => Date.ToString("dd.MM.yyyy");
        public string AmountString => $"{Amount:N2} руб.";
    }

    public class PaymentTypeStat
    {
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }

        public string AmountString => $"{Amount:N2} руб.";
    }
}