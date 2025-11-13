using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class PaymentsPage : Page
    {
        private readonly XmlDataService _dataService;
        private StudentCollection _students;
        private PaymentCollection _payments;
        private StudentTuitionCollection _tuitions;
        private Student _selectedStudent;

        private void TuitionDisplayPanel_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditTuitionButton_Click(sender, e);
        }

       private void UpdateColorProgressBar(decimal totalToPay, decimal totalPaid)
{
    if (PaidAmountColorText == null || RemainingAmountColorText == null)
        return;

    var remaining = totalToPay - totalPaid;

    PaidAmountColorText.Text = $"Оплачено: {totalPaid:N2} руб.";
    RemainingAmountColorText.Text = $"Остаток: {remaining:N2} руб.";
}

        private void UpdatePaymentProgress(decimal totalToPay, decimal totalPaid)
        {
            double progressPercentage = 0;
            if (totalToPay > 0)
            {
                progressPercentage = Math.Round((double)(totalPaid * 100 / totalToPay), 1);
            }

            TotalAmountText.Text = $"Общая сумма к оплате: {totalToPay:N2} руб.";

            if (ProgressBarGrid.ActualWidth > 0)
            {
                double maxWidth = ProgressBarGrid.ActualWidth - 2;
                double paidWidth = (progressPercentage / 100.0) * maxWidth;
                PaidAmountBar.Width = paidWidth;
            }

            ProgressText.Text = $"{progressPercentage}%";
            PaymentDetailsText.Text = $"Оплачено: {totalPaid:N2} руб. из {totalToPay:N2} руб.";

            var remaining = totalToPay - totalPaid;
            if (remaining <= 0)
            {
                RemainingAmountText.Text = "✅ ОПЛАЧЕНО ПОЛНОСТЬЮ";
                RemainingAmountText.Foreground = Brushes.Green;
            }
            else
            {
                RemainingAmountText.Text = $"Осталось оплатить: {remaining:N2} руб.";
                RemainingAmountText.Foreground = Brushes.Red;
            }

            UpdateColorProgressBar(totalToPay, totalPaid);
        }

        private void ResetPaymentProgress()
        {
            TotalAmountText.Text = "Общая сумма к оплате: 0 руб.";
            PaidAmountBar.Width = 0;
            ProgressText.Text = "0%";
            PaymentDetailsText.Text = "Оплачено: 0 руб. из 0 руб.";
            RemainingAmountText.Text = "Осталось оплатить: 0 руб.";
            RemainingAmountText.Foreground = Brushes.Red;

            PaidAmountColorText.Text = "Оплачено: 0 руб.";
            RemainingAmountColorText.Text = "Остаток: 0 руб.";
        }

        public PaymentsPage(XmlDataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            LoadData();
            UpdateButtonsAvailability();

            Loaded += (s, e) =>
            {
                if (_selectedStudent != null)
                    LoadPaymentsForStudent();
            };
        }

        private void LoadData()
        {
            try
            {
                _students = _dataService.LoadStudents();
                _payments = _dataService.LoadPayments();
                _tuitions = _dataService.LoadStudentTuitions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                _students = new StudentCollection();
                _payments = new PaymentCollection();
                _tuitions = new StudentTuitionCollection();
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
                LoadPaymentsForStudent();
                UpdateTuitionInfo();
            }
        }

        private void UpdateSelectedStudentPanel()
        {
            if (_selectedStudent != null)
            {
                SelectedStudentPanel.Visibility = Visibility.Visible;
                SelectedStudentText.Text = _selectedStudent.FullName;
                SelectedStudentDetails.Text = $"Телефон: {_selectedStudent.Phone} | ID: {_selectedStudent.Id}";
            }
            else
            {
                SelectedStudentPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateTuitionInfo()
        {
            if (_selectedStudent != null)
            {
                var tuition = _tuitions.Tuitions.FirstOrDefault(t => t.StudentId == _selectedStudent.Id);

                if (tuition != null)
                {
                    SetTuitionButton.Visibility = Visibility.Collapsed;
                    TuitionDisplayPanel.Visibility = Visibility.Visible;

                    TuitionDisplayText.Text = $"{tuition.FinalAmount:N0} руб.";

                    TuitionInfoPanel.Visibility = Visibility.Visible;
                    NoTuitionText.Visibility = Visibility.Collapsed;

                    FullAmountText.Text = $"Полная стоимость: {tuition.FullAmount:N2} руб.";
                    DiscountText.Text = $"Скидка: {tuition.Discount:N2} руб.";
                    FinalAmountText.Text = $"Итоговая сумма: {tuition.FinalAmount:N2} руб.";
                }
                else
                {
                    SetTuitionButton.Visibility = Visibility.Visible;
                    TuitionDisplayPanel.Visibility = Visibility.Collapsed;

                    TuitionInfoPanel.Visibility = Visibility.Collapsed;
                    NoTuitionText.Visibility = Visibility.Visible;
                }
            }
            else
            {
                SetTuitionButton.Visibility = Visibility.Visible;
                TuitionDisplayPanel.Visibility = Visibility.Collapsed;
                TuitionInfoPanel.Visibility = Visibility.Collapsed;
                NoTuitionText.Visibility = Visibility.Visible;
            }
        }

        private void LoadPaymentsForStudent()
        {
            if (_selectedStudent != null)
            {
                var tuition = _tuitions.Tuitions.FirstOrDefault(t => t.StudentId == _selectedStudent.Id);
                var totalToPay = tuition?.FinalAmount ?? 0;

                var studentPayments = _payments.Payments
                    .Where(p => p.StudentId == _selectedStudent.Id)
                    .OrderBy(p => p.PaymentDate)
                    .ToList();

                decimal cumulativeAmount = 0;
                var paymentsWithStatus = studentPayments.Select(p =>
                {
                    cumulativeAmount += p.Amount;
                    var status = cumulativeAmount >= totalToPay ? PaymentStatus.FullyPaid :
                                cumulativeAmount > 0 ? PaymentStatus.PartiallyPaid : PaymentStatus.NotPaid;

                    return new PaymentWithStatus(p, status, cumulativeAmount, totalToPay);
                }).ToList();

                PaymentsGrid.ItemsSource = paymentsWithStatus;

                UpdatePaymentProgress(totalToPay, cumulativeAmount);
            }
            else
            {
                PaymentsGrid.ItemsSource = null;
                ResetPaymentProgress();
            }

            UpdateButtonsAvailability();
        }

        private void UpdateButtonsAvailability()
        {
            bool hasStudent = _selectedStudent != null;
            bool hasTuition = hasStudent && _tuitions.Tuitions.Any(t => t.StudentId == _selectedStudent.Id);
            bool hasPayments = hasStudent && _payments.Payments.Any(p => p.StudentId == _selectedStudent.Id);
            bool hasSelection = PaymentsGrid.SelectedItem != null;

            SetTuitionButton.IsEnabled = hasStudent;
            EditTuitionButton.IsEnabled = hasStudent && hasTuition;
            AddPaymentButton.IsEnabled = hasStudent && hasTuition;
            EditPaymentButton.IsEnabled = hasStudent && hasPayments && hasSelection;
            DeletePaymentButton.IsEnabled = hasStudent && hasPayments && hasSelection;

            SetTuitionButton.Opacity = SetTuitionButton.IsEnabled ? 1.0 : 0.5;
            EditTuitionButton.Opacity = EditTuitionButton.IsEnabled ? 1.0 : 0.5;
            AddPaymentButton.Opacity = AddPaymentButton.IsEnabled ? 1.0 : 0.5;
            EditPaymentButton.Opacity = EditPaymentButton.IsEnabled ? 1.0 : 0.5;
            DeletePaymentButton.Opacity = DeletePaymentButton.IsEnabled ? 1.0 : 0.5;
        }

        private void SetTuitionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent != null)
            {
                var dialog = new TuitionEditDialog(_dataService, _selectedStudent.Id);
                if (dialog.ShowDialog() == true)
                {
                    _tuitions.Tuitions.Add(dialog.TuitionData);
                    _dataService.SaveStudentTuitions(_tuitions);
                    UpdateTuitionInfo();
                    LoadPaymentsForStudent();
                    MessageBox.Show("Стоимость обучения установлена!", "Успех");
                }
            }
        }

        private void EditTuitionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent != null)
            {
                var tuition = _tuitions.Tuitions.FirstOrDefault(t => t.StudentId == _selectedStudent.Id);
                if (tuition != null)
                {
                    var dialog = new TuitionEditDialog(_dataService, _selectedStudent.Id, tuition);
                    if (dialog.ShowDialog() == true)
                    {
                        var index = _tuitions.Tuitions.IndexOf(tuition);
                        if (index >= 0)
                        {
                            _tuitions.Tuitions[index] = dialog.TuitionData;
                            _dataService.SaveStudentTuitions(_tuitions);
                            UpdateTuitionInfo();
                            LoadPaymentsForStudent();
                            MessageBox.Show("Стоимость обучения обновлена!", "Успех");
                        }
                    }
                }
            }
        }

        private void ClearSelectedStudent_Click(object sender, RoutedEventArgs e)
        {
            _selectedStudent = null;
            UpdateSelectedStudentPanel();
            PaymentsGrid.ItemsSource = null;
            UpdateTuitionInfo();
            ResetPaymentProgress();
            UpdateButtonsAvailability();

            SetTuitionButton.Visibility = Visibility.Visible;
            TuitionDisplayPanel.Visibility = Visibility.Collapsed;
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            SearchResultsListBox.Visibility = Visibility.Collapsed;
        }

        private void PaymentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonsAvailability();
        }

        private void AddPayment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent != null)
            {
                var dialog = new PaymentEditDialog(_dataService, _selectedStudent.Id);
                if (dialog.ShowDialog() == true)
                {
                    _payments.Payments.Add(dialog.PaymentData);
                    _dataService.SavePayments(_payments);
                    LoadPaymentsForStudent();
                    MessageBox.Show("Оплата добавлена!", "Успех");
                }
            }
        }

        private void EditPayment_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentsGrid.SelectedItem is PaymentWithStatus selectedPaymentWithStatus)
            {
                var selectedPayment = selectedPaymentWithStatus.Payment;
                var dialog = new PaymentEditDialog(_dataService, selectedPayment.StudentId, selectedPayment);
                if (dialog.ShowDialog() == true)
                {
                    var index = _payments.Payments.IndexOf(selectedPayment);
                    if (index >= 0)
                    {
                        _payments.Payments[index] = dialog.PaymentData;
                        _dataService.SavePayments(_payments);
                        LoadPaymentsForStudent();
                        MessageBox.Show("Оплата обновлена!", "Успех");
                    }
                }
            }
        }

        private void DeletePayment_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentsGrid.SelectedItem is PaymentWithStatus selectedPaymentWithStatus)
            {
                var selectedPayment = selectedPaymentWithStatus.Payment;
                var result = MessageBox.Show("Удалить запись об оплате?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _payments.Payments.Remove(selectedPayment);
                    _dataService.SavePayments(_payments);
                    LoadPaymentsForStudent();
                    MessageBox.Show("Оплата удалена!", "Успех");
                }
            }
        }

        private void ViewPayment_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentsGrid.SelectedItem is PaymentWithStatus selectedPaymentWithStatus)
            {
                var selectedPayment = selectedPaymentWithStatus.Payment;
                var student = _students.Students.FirstOrDefault(s => s.Id == selectedPayment.StudentId);
                var studentName = student?.FullName ?? "Неизвестный студент";

                MessageBox.Show($"Информация об оплате:\n\n" +
                               $"Студент: {studentName}\n" +
                               $"Дата оплаты: {selectedPayment.PaymentDate:dd.MM.yyyy}\n" +
                               $"Сумма: {selectedPayment.Amount:N2} руб.\n" +
                               $"Тип оплаты: {selectedPayment.PaymentType}\n" +
                               "Просмотр оплаты");
            }
        }

        private void ExportPayments_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция экспорта в разработке", "Информация");
        }

        private void ProgressBarGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_selectedStudent != null)
            {
                LoadPaymentsForStudent();
            }
        }
    }

    public class PaymentWithStatus
    {
        public Payment Payment { get; set; }
        public PaymentStatus Status { get; set; }
        public decimal CumulativeAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public PaymentWithStatus(Payment payment, PaymentStatus status, decimal cumulativeAmount, decimal totalAmount)
        {
            Payment = payment;
            Status = status;
            CumulativeAmount = cumulativeAmount;
            TotalAmount = totalAmount;
        }

        public int Id => Payment.Id;
        public DateTime PaymentDate => Payment.PaymentDate;
        public decimal Amount => Payment.Amount;
        public string PaymentType => Payment.PaymentType;

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case PaymentStatus.FullyPaid:
                        return "Оплачено";
                    case PaymentStatus.PartiallyPaid:
                        return "Частично";
                    case PaymentStatus.NotPaid:
                        return "Не оплачено";
                    default:
                        return "Неизвестно";
                }
            }
        }

        public string StatusColor
        {
            get
            {
                switch (Status)
                {
                    case PaymentStatus.FullyPaid:
                        return "#4CAF50";
                    case PaymentStatus.PartiallyPaid:
                        return "#FF9800"; 
                    case PaymentStatus.NotPaid:
                        return "#F44336"; 
                    default:
                        return "#9E9E9E";
                }
            }
        }
    }

    public enum PaymentStatus
    {
        FullyPaid,
        PartiallyPaid,
        NotPaid,
        Unknown
    }
}