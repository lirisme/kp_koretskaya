using DrivingSchool.Models;
using DrivingSchool.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DrivingSchool.Views
{
    public partial class TemplatesPage : Page
    {
        private readonly XmlDataService _dataService;
        private DocumentTemplateCollection _templates;

        public TemplatesPage(XmlDataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            try
            {
                _templates = _dataService.LoadTemplates();
                TemplatesGrid.ItemsSource = _templates.Templates;
                UpdateStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки шаблонов: {ex.Message}", "Ошибка");
                _templates = new DocumentTemplateCollection();
            }
        }

        private void UpdateStatus()
        {
            StatusText.Text = $"Шаблонов: {_templates.Templates.Count}";
        }

        private void AddTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Все поддерживаемые форматы (*.docx;*.doc;*.xlsx;*.xls)|*.docx;*.doc;*.xlsx;*.xls|" +
              "Word документы (*.docx;*.doc)|*.docx;*.doc|" +
              "Excel документы (*.xlsx;*.xls)|*.xlsx;*.xls|" +
              "Все файлы (*.*)|*.*",
                    FilterIndex = 1,
                    Title = "Выберите файл шаблона"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var template = new DocumentTemplate
                    {
                        Id = GetNextTemplateId(),
                        TemplateName = Path.GetFileNameWithoutExtension(openFileDialog.FileName),
                        DocumentType = GetDocumentTypeFromName(Path.GetFileNameWithoutExtension(openFileDialog.FileName)),
                        FilePath = openFileDialog.FileName,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };

                    var editDialog = new TemplateEditDialog(_dataService, template);
                    if (editDialog.ShowDialog() == true)
                    {
                        _templates.Templates.Add(editDialog.Template);
                        _dataService.SaveTemplates(_templates);
                        LoadTemplates();
                        MessageBox.Show($"Шаблон '{editDialog.Template.TemplateName}' добавлен!", "Успех");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении шаблона: {ex.Message}", "Ошибка");
            }
        }

        private int GetNextTemplateId()
        {
            return _templates.Templates.Count > 0 ? _templates.Templates.Max(t => t.Id) + 1 : 1;
        }

        private string GetDocumentTypeFromName(string fileName)
        {
            if (fileName.ToLower().Contains("договор")) return "Договор";
            if (fileName.ToLower().Contains("заявление")) return "Заявление";
            if (fileName.ToLower().Contains("справка")) return "Справка";
            if (fileName.ToLower().Contains("свидетельство")) return "Свидетельство";
            return "Другой";
        }

        private void EditTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (TemplatesGrid.SelectedItem is DocumentTemplate selectedTemplate)
            {
                var dialog = new TemplateEditDialog(_dataService, selectedTemplate);
                if (dialog.ShowDialog() == true)
                {
                    var index = _templates.Templates.IndexOf(selectedTemplate);
                    if (index >= 0)
                    {
                        _templates.Templates[index] = dialog.Template;
                        _dataService.SaveTemplates(_templates);
                        LoadTemplates();
                        MessageBox.Show($"Шаблон '{dialog.Template.TemplateName}' обновлен!", "Успех");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите шаблон для редактирования", "Предупреждение");
            }
        }

        private void DeleteTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (TemplatesGrid.SelectedItem is DocumentTemplate selectedTemplate)
            {
                var result = MessageBox.Show($"Удалить шаблон '{selectedTemplate.TemplateName}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _templates.Templates.Remove(selectedTemplate);
                    _dataService.SaveTemplates(_templates);
                    LoadTemplates();
                    MessageBox.Show($"Шаблон '{selectedTemplate.TemplateName}' удален.", "Успех");
                }
            }
            else
            {
                MessageBox.Show("Выберите шаблон для удаления", "Предупреждение");
            }
        }

        private void PreviewTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (TemplatesGrid.SelectedItem is DocumentTemplate selectedTemplate)
            {
                if (File.Exists(selectedTemplate.FilePath))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = selectedTemplate.FilePath,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка");
                    }
                }
                else
                {
                    MessageBox.Show("Файл шаблона не найден", "Ошибка");
                }
            }
            else
            {
                MessageBox.Show("Выберите шаблон для просмотра", "Предупреждение");
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text?.ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                TemplatesGrid.ItemsSource = _templates.Templates;
            }
            else
            {
                var filteredTemplates = _templates.Templates
                    .Where(t => t.TemplateName.ToLower().Contains(searchText) ||
                               t.DocumentType.ToLower().Contains(searchText))
                    .ToList();
                TemplatesGrid.ItemsSource = filteredTemplates;
            }
        }

        private void TemplatesGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditTemplate_Click(sender, e);
        }
    }
}