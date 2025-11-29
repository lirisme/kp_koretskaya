using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class CategoriesPage : Page
    {
        private readonly XmlDataService _dataService;
        private VehicleCategoryCollection _categories;

        public CategoriesPage(XmlDataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _categories = _dataService.LoadVehicleCategories();
                if (_categories?.Categories == null)
                    _categories = new VehicleCategoryCollection();

                CategoriesGrid.ItemsSource = _categories.Categories;
                UpdateButtonsAvailability();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                _categories = new VehicleCategoryCollection();
                UpdateButtonsAvailability();
            }
        }

        private void UpdateButtonsAvailability()
        {
            var isCategorySelected = CategoriesGrid.SelectedItem != null;

            EditCategoryButton.IsEnabled = isCategorySelected;
            DeleteCategoryButton.IsEnabled = isCategorySelected;
        }

        private void CategoriesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonsAvailability();
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CategoryEditDialog(_dataService);
            if (dialog.ShowDialog() == true)
            {
                _categories.Categories.Add(dialog.CategoryData);
                _dataService.SaveVehicleCategories(_categories);
                LoadData();
                MessageBox.Show($"Категория успешно добавлена!", "Успех");
            }
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesGrid.SelectedItem is VehicleCategory selectedCategory)
            {
                var dialog = new CategoryEditDialog(_dataService, selectedCategory);
                if (dialog.ShowDialog() == true)
                {
                    var index = _categories.Categories.IndexOf(selectedCategory);
                    if (index >= 0)
                    {
                        _categories.Categories[index] = dialog.CategoryData;
                        _dataService.SaveVehicleCategories(_categories);
                        LoadData();
                        MessageBox.Show($"Категория обновлена!", "Успех");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите категорию для редактирования", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesGrid.SelectedItem is VehicleCategory selectedCategory)
            {
                var certificates = _dataService.LoadCertificates();
                var isUsed = certificates.Certificates.Any(c => c.VehicleCategoryId == selectedCategory.Id);

                if (isUsed)
                {
                    MessageBox.Show(
                        $"Невозможно удалить категорию '{selectedCategory.DisplayText}'!\n\n" +
                        "Эта категория используется в свидетельствах об окончании.",
                        "Ошибка удаления",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить категорию?\n\n" +
                    $"Код: {selectedCategory.Code}\n" +
                    $"Название: {selectedCategory.FullName}\n\n" +
                    $"Это действие нельзя отменить!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _categories.Categories.Remove(selectedCategory);
                    _dataService.SaveVehicleCategories(_categories);
                    LoadData();
                    MessageBox.Show($"Категория удалена.", "Успех");
                }
            }
            else
            {
                MessageBox.Show("Выберите категорию для удаления", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ViewCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesGrid.SelectedItem is VehicleCategory selectedCategory)
            {
                MessageBox.Show($"Информация о категории:\n\n" +
                               $"Код: {selectedCategory.Code}\n" +
                               $"Название: {selectedCategory.FullName}\n" +
                               $"ID: {selectedCategory.Id}",
                               "Просмотр категории");
            }
            else
            {
                MessageBox.Show("Выберите категорию для просмотра", "Предупреждение");
            }
        }

        private void CategoriesGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;

            if (row != null && row.DataContext is VehicleCategory category)
            {
                EditCategory_Click(sender, e);
            }
        }
    }
}