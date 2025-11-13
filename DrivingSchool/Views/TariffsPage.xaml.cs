using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class TariffsPage : Page
    {
        private readonly XmlDataService _dataService;
        private TariffCollection _tariffs;

        public int TariffsCount => _tariffs?.Tariffs?.Count ?? 0;

        public TariffsPage(XmlDataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            LoadData();
            UpdateStatistics();
        }

        private void LoadData()
        {
            try
            {
                _tariffs = _dataService.LoadTariffs();
                TariffsGrid.ItemsSource = _tariffs.Tariffs;
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки тарифов: {ex.Message}", "Ошибка");
                _tariffs = new TariffCollection();
            }
        }

        private void UpdateStatistics()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.StatusText.Text = $"Тарифы: всего {TariffsCount}";
            }
        }

        private void UpdateButtonsAvailability()
        {
            bool hasSelection = TariffsGrid.SelectedItem != null;

            var editButton = FindName("EditTariffButton") as Button;
            var deleteButton = FindName("DeleteTariffButton") as Button;

            if (editButton != null) editButton.Opacity = hasSelection ? 1.0 : 0.5;
            if (deleteButton != null) deleteButton.Opacity = hasSelection ? 1.0 : 0.5;
        }

        private void AddTariff_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TariffEditDialog(_dataService);
            if (dialog.ShowDialog() == true)
            {
                _tariffs.Tariffs.Add(dialog.TariffData);
                _dataService.SaveTariffs(_tariffs);
                LoadData();
                MessageBox.Show("Тариф добавлен!", "Успех");
            }
        }

        private void EditTariff_Click(object sender, RoutedEventArgs e)
        {
            if (TariffsGrid.SelectedItem is Tariff selectedTariff)
            {
                var dialog = new TariffEditDialog(_dataService, selectedTariff);
                if (dialog.ShowDialog() == true)
                {
                    var index = _tariffs.Tariffs.IndexOf(selectedTariff);
                    if (index >= 0)
                    {
                        _tariffs.Tariffs[index] = dialog.TariffData;
                        _dataService.SaveTariffs(_tariffs);
                        LoadData();
                        MessageBox.Show("Тариф обновлен!", "Успех");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите тариф для редактирования", "Предупреждение");
            }
        }

        private void DeleteTariff_Click(object sender, RoutedEventArgs e)
        {
            if (TariffsGrid.SelectedItem is Tariff selectedTariff)
            {
                var result = MessageBox.Show(
                    $"Удалить тариф '{selectedTariff.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _tariffs.Tariffs.Remove(selectedTariff);
                    _dataService.SaveTariffs(_tariffs);
                    LoadData();
                    MessageBox.Show("Тариф удален!", "Успех");
                }
            }
            else
            {
                MessageBox.Show("Выберите тариф для удаления", "Предупреждение");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void TariffsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonsAvailability();
        }
    }
}