using DrivingSchool.Models;
using DrivingSchool.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DrivingSchool.Views
{
    public partial class TemplateSelectionDialog : Window
    {
        private readonly XmlDataService _dataService;
        private readonly System.Collections.Generic.List<Student> _students;
        private readonly DocumentGenerator _documentGenerator;
        private DocumentTemplateCollection _templates;

        public TemplateSelectionDialog(XmlDataService dataService, Student student)
            : this(dataService, new System.Collections.Generic.List<Student> { student })
        {
        }

        public TemplateSelectionDialog(XmlDataService dataService, List<Student> students)
        {
            InitializeComponent();
            _dataService = dataService;
            _students = students;
            _documentGenerator = new DocumentGenerator(dataService);

            LoadTemplates();
            UpdateStudentsInfo();
            SetDefaultOutputPath();
        }

        private void LoadTemplates()
        {
            try
            {
                _templates = _dataService.LoadTemplates();
                TemplatesGrid.ItemsSource = _templates.Templates;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки шаблонов: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateStudentsInfo()
        {
            if (_students.Count == 1)
            {
                StudentInfoText.Text = $"Студент: {_students[0].FullName}\n" +
                                     $"Телефон: {_students[0].Phone}\n" +
                                     $"Группа: {GetGroupName(_students[0].GroupId)}";
            }
            else
            {
                StudentInfoText.Text = $"Выбрано студентов: {_students.Count}\n" +
                                     $"Группа: {GetGroupName(_students[0].GroupId)}\n" +
                                     $"Диапазон: {_students.Min(s => s.LastName)} - {_students.Max(s => s.LastName)}";
            }
        }

        private string GetGroupName(int groupId)
        {
            if (groupId == 0) return "Не назначена";

            var group = _dataService.LoadStudyGroups().Groups
                .FirstOrDefault(g => g.Id == groupId);
            return group?.Name ?? "Неизвестная группа";
        }

        private void SetDefaultOutputPath()
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (_students.Count == 1)
            {
                var fileName = $"{_students[0].LastName}_{_students[0].FirstName}_документ.docx";
                OutputPathTextBox.Text = System.IO.Path.Combine(desktopPath, fileName);
            }
            else
            {
                var fileName = $"Документы_группа_{GetGroupName(_students[0].GroupId)}_{DateTime.Now:ddMMyyyy}.docx";
                OutputPathTextBox.Text = System.IO.Path.Combine(desktopPath, fileName);
            }

            if (TemplatesGrid.SelectedItem is DocumentTemplate selectedTemplate)
            {
                string templateExtension = System.IO.Path.GetExtension(selectedTemplate.FilePath).ToLower();
                if (templateExtension == ".xlsx" || templateExtension == ".xls")
                {
                    string currentPath = OutputPathTextBox.Text;
                    string newPath = System.IO.Path.ChangeExtension(currentPath, templateExtension);
                    OutputPathTextBox.Text = newPath;
                }
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (TemplatesGrid.SelectedItem is DocumentTemplate selectedTemplate)
            {
                if (string.IsNullOrWhiteSpace(OutputPathTextBox.Text))
                {
                    MessageBox.Show("Выберите путь для сохранения документа", "Ошибка");
                    return;
                }

                try
                {
                    bool result = _documentGenerator.GenerateDocument(_students, selectedTemplate, OutputPathTextBox.Text);

                    if (result)
                    {
                        string message = _students.Count == 1
                            ? $"Документ успешно сгенерирован!\n\nПуть: {OutputPathTextBox.Text}"
                            : $"Документы успешно сгенерированы для {_students.Count} студентов!\n\nПуть: {OutputPathTextBox.Text}";

                        MessageBox.Show(message, "Успех");

                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = OutputPathTextBox.Text,
                                UseShellExecute = true
                            });
                        }
                        catch
                        {
                        }

                        DialogResult = true;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при генерации документа: {ex.Message}", "Ошибка");
                }
            }
            else
            {
                MessageBox.Show("Выберите шаблон документа", "Ошибка");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BrowseOutputPath_Click(object sender, RoutedEventArgs e)
        {
            if (TemplatesGrid.SelectedItem is DocumentTemplate selectedTemplate)
            {
                string templateExtension = System.IO.Path.GetExtension(selectedTemplate.FilePath).ToLower();
                string filter;
                string defaultExtension;

                if (templateExtension == ".xlsx" || templateExtension == ".xls")
                {
                    filter = "Документы Excel (*.xlsx)|*.xlsx|Документы Excel (*.xls)|*.xls";
                    defaultExtension = ".xlsx";
                }
                else
                {
                    filter = "Документы Word (*.docx)|*.docx|Документы Word (*.doc)|*.doc";
                    defaultExtension = ".docx";
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = filter,
                    DefaultExt = defaultExtension,
                    FileName = System.IO.Path.GetFileName(OutputPathTextBox.Text),
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    OutputPathTextBox.Text = saveFileDialog.FileName;
                }
            }
            else
            {
                MessageBox.Show("Сначала выберите шаблон", "Ошибка");
            }
        }

        private void TemplatesGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TemplatesGrid.SelectedItem is DocumentTemplate selectedTemplate && !string.IsNullOrEmpty(OutputPathTextBox.Text))
            {
                string templateExtension = System.IO.Path.GetExtension(selectedTemplate.FilePath).ToLower();
                string currentPath = OutputPathTextBox.Text;
                string newPath;

                if (templateExtension == ".xlsx" || templateExtension == ".xls")
                {
                    newPath = System.IO.Path.ChangeExtension(currentPath, ".xlsx");
                }
                else
                {
                    newPath = System.IO.Path.ChangeExtension(currentPath, ".docx");
                }

                OutputPathTextBox.Text = newPath;
            }
        }
    }
}