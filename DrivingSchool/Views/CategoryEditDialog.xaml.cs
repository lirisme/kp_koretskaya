using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DrivingSchool.Models;
using DrivingSchool.Services;

namespace DrivingSchool.Views
{
    public partial class CategoryEditDialog : Window
    {
        private readonly XmlDataService _dataService;
        public VehicleCategory CategoryData { get; private set; }
        private bool _isEditMode;

        public CategoryEditDialog(XmlDataService dataService, VehicleCategory categoryData = null)
        {
            InitializeComponent();
            _dataService = dataService;

            if (categoryData != null)
            {
                CategoryData = categoryData;
                _isEditMode = true;
                Title = "Редактирование категории";
            }
            else
            {
                CategoryData = new VehicleCategory
                {
                    Id = GetNextCategoryId()
                };
                _isEditMode = false;
                Title = "Добавление категории";
            }

            DataContext = CategoryData;
        }

        private int GetNextCategoryId()
        {
            var categories = _dataService.LoadVehicleCategories();
            return categories.Categories.Count > 0 ? categories.Categories.Max(p => p.Id) + 1 : 1;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CategoryData.Code) ||
                string.IsNullOrWhiteSpace(CategoryData.FullName))
            {
                MessageBox.Show("Заполните обязательные поля (Код и Название)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var categories = _dataService.LoadVehicleCategories();
            var existingCategory = categories.Categories
                .FirstOrDefault(c => c.Code.ToLower() == CategoryData.Code.ToLower() && c.Id != CategoryData.Id);

            if (existingCategory != null)
            {
                MessageBox.Show($"Категория с кодом '{CategoryData.Code}' уже существует", "Ошибка",
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