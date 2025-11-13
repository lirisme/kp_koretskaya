using DrivingSchool.Models;
using DrivingSchool.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DrivingSchool.Views
{
    public partial class TuitionEditDialog : Window
    {
        private readonly XmlDataService _dataService;
        private readonly int _studentId;
        public StudentTuition TuitionData { get; private set; }
        private bool _isEditMode;
        private TariffCollection _tariffs;

        public TuitionEditDialog(XmlDataService dataService, int studentId, StudentTuition tuitionData = null)
        {
            InitializeComponent();
            _dataService = dataService;
            _studentId = studentId;
            _tariffs = _dataService.LoadTariffs();



            if (tuitionData != null)
            {
                TuitionData = new StudentTuition
                {
                    Id = tuitionData.Id,
                    StudentId = tuitionData.StudentId,
                    FullAmount = tuitionData.FullAmount,
                    Discount = tuitionData.Discount,
                    CreatedDate = tuitionData.CreatedDate
                };
                _isEditMode = true;
                Title = "Редактирование стоимости обучения";
            }
            else
            {
                TuitionData = new StudentTuition
                {
                    Id = GetNextTuitionId(),
                    StudentId = studentId,
                    FullAmount = 0,
                    Discount = 0,
                    CreatedDate = DateTime.Now
                };
                _isEditMode = false;
                Title = "Установка стоимости обучения";
            }

            DataContext = TuitionData;
            LoadTariffs();
            UpdateFinalAmount();
        }

        private int GetNextTuitionId()
        {
            var tuitions = _dataService.LoadStudentTuitions();
            return tuitions.Tuitions.Count > 0 ? tuitions.Tuitions.Max(t => t.Id) + 1 : 1;
        }

        private void LoadTariffs()
        {
            var allTariffs = _tariffs.Tariffs.ToList();

            TariffComboBox.ItemsSource = allTariffs;

            TariffComboBox.DisplayMemberPath = "Name";

            if (allTariffs.Any())
            {
                TariffComboBox.SelectedIndex = 0;
            }

            foreach (var tariff in allTariffs)
            {
                System.Diagnostics.Debug.WriteLine($"Tariff in combobox: {tariff.Name}");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (TuitionData.FullAmount <= 0)
            {
                MessageBox.Show("Выберите тариф для установки стоимости", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TariffComboBox.Focus();
                return;
            }

            if (TuitionData.Discount < 0)
            {
                MessageBox.Show("Скидка не может быть отрицательной", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DiscountTextBox.Focus();
                return;
            }

            if (TuitionData.Discount > TuitionData.FullAmount)
            {
                MessageBox.Show("Скидка не может превышать полную стоимость", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DiscountTextBox.Focus();
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

        private void TariffComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TariffComboBox.SelectedItem is Tariff selectedTariff)
            {
                TuitionData.FullAmount = selectedTariff.BaseCost;
                UpdateFinalAmount();
                UpdateTariffDetails(selectedTariff);
            }
        }

        private void DiscountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string inputText = DiscountTextBox.Text;

            inputText = inputText.Replace(" ", "").Replace("₽", "").Trim();

            if (decimal.TryParse(inputText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("ru-RU"), out decimal discount))
            {
                TuitionData.Discount = discount;
                System.Diagnostics.Debug.WriteLine($"СКИДКА УСТАНОВЛЕНА: {discount}");
            }
            else if (string.IsNullOrWhiteSpace(inputText))
            {
                TuitionData.Discount = 0;
            }

            UpdateFinalAmount();
        }

        private void UpdateFinalAmount()
        {
            if (TuitionData != null)
            {
                FinalAmountText.Text = $"Итоговая сумма: {TuitionData.FinalAmount:N2} руб.";
            }
        }

        private void UpdateTariffDetails(Tariff tariff)
        {
            if (tariff != null)
            {
                TariffDetailsText.Text = $"{tariff.Description}\n" +
                                       $"Категория: {tariff.Category}\n" +
                                       $"Длительность: {tariff.DurationMonths} мес.\n" +
                                       $"Практика: {tariff.PracticeHours} ч.\n" +
                                       $"Полная стоимость: {tariff.BaseCost:N2} руб.";
            }
            else
            {
                TariffDetailsText.Text = string.Empty;
            }
        }

        private void DecimalValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != ',' && c != '.')
                {
                    e.Handled = true;
                    return;
                }
            }
        }
    }
}