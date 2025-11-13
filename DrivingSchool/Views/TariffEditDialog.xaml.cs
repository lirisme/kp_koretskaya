// TariffEditDialog.xaml.cs
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class TariffEditDialog : Window
    {
        private readonly XmlDataService _dataService;
        public Tariff TariffData { get; private set; }
        private bool _isEditMode;

        public TariffEditDialog(XmlDataService dataService, Tariff tariffData = null)
        {
            InitializeComponent();
            _dataService = dataService;

            if (tariffData != null)
            {
                TariffData = tariffData;
                _isEditMode = true;
                Title = "Редактирование тарифа";
            }
            else
            {
                TariffData = new Tariff
                {
                    Id = GetNextTariffId(),
                    CreatedDate = DateTime.Now
                };
                _isEditMode = false;
                Title = "Добавление тарифа";
            }

            DataContext = TariffData;
        }

        private int GetNextTariffId()
        {
            var tariffs = _dataService.LoadTariffs();
            return tariffs.Tariffs.Count > 0 ? tariffs.Tariffs.Max(t => t.Id) + 1 : 1;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
            e.Handled = !regex.IsMatch((sender as System.Windows.Controls.TextBox).Text + e.Text);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TariffData.Name))
            {
                MessageBox.Show("Введите название тарифа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TariffData.BaseCost <= 0)
            {
                MessageBox.Show("Стоимость должна быть больше 0", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TariffData.DurationMonths <= 0)
            {
                MessageBox.Show("Длительность обучения должна быть больше 0 месяцев", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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