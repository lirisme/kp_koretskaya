using DrivingSchool.Models;
using DrivingSchool.Services;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DrivingSchool.Views
{
    public partial class PaymentEditDialog : Window
    {
        private readonly XmlDataService _dataService;
        private readonly int _studentId;
        public Payment PaymentData { get; private set; }
        private bool _isEditMode;

        public PaymentEditDialog(XmlDataService dataService, int studentId, Payment paymentData = null)
        {
            InitializeComponent();
            _dataService = dataService;
            _studentId = studentId;

            if (paymentData != null)
            {
                PaymentData = new Payment
                {
                    Id = paymentData.Id,
                    StudentId = paymentData.StudentId,
                    PaymentDate = paymentData.PaymentDate,
                    Amount = paymentData.Amount,
                    PaymentType = paymentData.PaymentType,
                };
                _isEditMode = true;
                Title = "Редактирование оплаты";
            }
            else
            {
                PaymentData = new Payment
                {
                    Id = GetNextPaymentId(),
                    StudentId = studentId,
                    PaymentDate = DateTime.Now,
                    PaymentType = "Наличные",
                    Amount = 0
                };
                _isEditMode = false;
                Title = "Внесение оплаты";
            }

            DataContext = PaymentData;
            InitializeComboBoxes();
        }

        private int GetNextPaymentId()
        {
            var payments = _dataService.LoadPayments();
            return payments.Payments.Count > 0 ? payments.Payments.Max(p => p.Id) + 1 : 1;
        }

        private void InitializeComboBoxes()
        {
            if (!string.IsNullOrEmpty(PaymentData.PaymentType))
            {
                PaymentTypeComboBox.Text = PaymentData.PaymentType;
            }
            else
            {
                PaymentTypeComboBox.SelectedIndex = 0;
            }
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != ',' && c != '.')
                {
                    e.Handled = true;
                    return;
                }
            }

            e.Handled = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            PaymentData.PaymentType = PaymentTypeComboBox.Text;

            if (PaymentData.PaymentDate == DateTime.MinValue)
            {
                MessageBox.Show("Выберите дату оплаты", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PaymentDatePicker.Focus();
                return;
            }

            if (PaymentData.Amount <= 0)
            {
                MessageBox.Show("Сумма оплаты должна быть больше 0", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                AmountTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(PaymentData.PaymentType))
            {
                MessageBox.Show("Выберите тип оплаты", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PaymentTypeComboBox.Focus();
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