using DrivingSchool.Models;
using DrivingSchool.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace DrivingSchool.Views
{
    public partial class TemplateEditDialog : Window, INotifyPropertyChanged
    {
        private readonly XmlDataService _dataService;
        public DocumentTemplate Template { get; private set; }
        private bool _isEditMode;

        private ObservableCollection<KeyValuePair<string, string>> _placeholdersView;
        public ObservableCollection<KeyValuePair<string, string>> PlaceholdersView
        {
            get => _placeholdersView;
            set
            {
                _placeholdersView = value;
                OnPropertyChanged(nameof(PlaceholdersView));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TemplateEditDialog(XmlDataService dataService, DocumentTemplate template = null)
        {
            InitializeComponent();
            _dataService = dataService;

            if (template != null)
            {
                Template = template;
                _isEditMode = true;
                Title = "Редактирование шаблона";
            }
            else
            {
                Template = new DocumentTemplate
                {
                    Id = GetNextTemplateId(),
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };
                _isEditMode = false;
                Title = "Создание нового шаблона";
            }

            InitializePlaceholders();
            DataContext = this;
        }

        private int GetNextTemplateId()
        {
            var templates = _dataService.LoadTemplates();
            return templates.Templates.Count > 0 ? templates.Templates.Max(t => t.Id) + 1 : 1;
        }

        private void InitializePlaceholders()
        {
            if (Template.Placeholders == null || !Template.Placeholders.Any())
            {
                Template.Placeholders = GetDefaultPlaceholders(Template.DocumentType);
            }

            PlaceholdersView = new ObservableCollection<KeyValuePair<string, string>>(Template.Placeholders);
            PlaceholdersView.CollectionChanged += (s, e) => UpdateTemplatePlaceholders();
        }

        private Dictionary<string, string> GetDefaultPlaceholders(string documentType)
        {
            var placeholders = new Dictionary<string, string>();

            switch (documentType)
            {
                case "Договор":
                    placeholders.Add("{GroupNumber}", "Номер группы");
                    placeholders.Add("{LastName}", "Фамилия");
                    placeholders.Add("{FirstName}", "Имя");
                    placeholders.Add("{MiddleName}", "Отчество");
                    placeholders.Add("{DateBirthday}", "Дата рождения");
                    placeholders.Add("{BirthPlace}", "Место рождения");
                    placeholders.Add("{PlaceResidence}", "Адрес регистрации");
                    placeholders.Add("{StudentPhone}", "Телефон");
                    placeholders.Add("{StudentEmail}", "Email");
                    placeholders.Add("{Citizenship}", "Гражданство");
                    placeholders.Add("{Gender}", "Пол");
                    placeholders.Add("{Age}", "Возраст");
                    placeholders.Add("{PassportSeries}", "Серия паспорта");
                    placeholders.Add("{PassportNumber}", "Номер паспорта");
                    placeholders.Add("{PassportIssuedBy}", "Кем выдан паспорт");
                    placeholders.Add("{PassportDivisionCode}", "Код подразделения");
                    placeholders.Add("{PassportIssueDate}", "Дата выдачи паспорта");
                    placeholders.Add("{DocumentType}", "Тип документа");
                    placeholders.Add("{DivisionCode}", "Код подразделения");
                    placeholders.Add("{MedicalSeries}", "Серия медсправки");
                    placeholders.Add("{MedicalNumber}", "Номер медсправки");
                    placeholders.Add("{MedicalIssueDate}", "Дата выдачи медсправки");
                    placeholders.Add("{MedicalInstitution}", "Мед учреждение");
                    placeholders.Add("{SnilsNumber}", "Номер СНИЛС");
                    placeholders.Add("{SnilsIssueDate}", "Дата выдачи СНИЛС");
                    placeholders.Add("{SnilsIssuedBy}", "Кем выдан СНИЛС");
                    placeholders.Add("{GroupName}", "Название группы");
                    placeholders.Add("{GroupStartDate}", "Дата начала обучения");
                    placeholders.Add("{GroupEndDate}", "Дата окончания обучения");
                    placeholders.Add("{GroupDuration}", "Продолжительность обучения");
                    placeholders.Add("{CURRENT_DATE}", "Текущая дата");
                    placeholders.Add("{CURRENT_YEAR}", "Текущий год");
                    placeholders.Add("{ORGANIZATION_NAME}", "Название организации");
                    placeholders.Add("{STUDENT_COUNT}", "Количество студентов");
                    break;

                case "Заявление":
                    placeholders.Add("{GroupNumber}", "Номер группы");
                    placeholders.Add("{LastName}", "Фамилия");
                    placeholders.Add("{FirstName}", "Имя");
                    placeholders.Add("{MiddleName}", "Отчество");
                    placeholders.Add("{DateBirthday}", "Дата рождения");
                    placeholders.Add("{BirthPlace}", "Место рождения");
                    placeholders.Add("{PlaceResidence}", "Адрес регистрации");
                    placeholders.Add("{StudentPhone}", "Телефон");
                    placeholders.Add("{StudentEmail}", "Email");
                    placeholders.Add("{Citizenship}", "Гражданство");
                    placeholders.Add("{Gender}", "Пол");
                    placeholders.Add("{PassportSeries}", "Серия паспорта");
                    placeholders.Add("{PassportNumber}", "Номер паспорта");
                    placeholders.Add("{PassportIssuedBy}", "Кем выдан паспорт");
                    placeholders.Add("{MedicalSeries}", "Серия медсправки");
                    placeholders.Add("{MedicalNumber}", "Номер медсправки");
                    placeholders.Add("{CURRENT_DATE}", "Текущая дата");
                    placeholders.Add("{ORGANIZATION_NAME}", "Название организации");
                    break;

                case "Справка":
                    placeholders.Add("{GroupNumber}", "Номер группы");
                    placeholders.Add("{LastName}", "Фамилия");
                    placeholders.Add("{FirstName}", "Имя");
                    placeholders.Add("{MiddleName}", "Отчество");
                    placeholders.Add("{DateBirthday}", "Дата рождения");
                    placeholders.Add("{StudentPhone}", "Телефон");
                    placeholders.Add("{GroupName}", "Название группы");
                    placeholders.Add("{GroupStartDate}", "Дата начала обучения");
                    placeholders.Add("{GroupEndDate}", "Дата окончания обучения");
                    placeholders.Add("{CertificateNumber}", "Номер свидетельства");
                    placeholders.Add("{CURRENT_DATE}", "Текущая дата");
                    placeholders.Add("{ProtocolNumber}", "Номер протокола");
                    break;

                case "Свидетельство":
                    placeholders.Add("{LastName}", "Фамилия");
                    placeholders.Add("{FirstName}", "Имя");
                    placeholders.Add("{MiddleName}", "Отчество");
                    placeholders.Add("{DateBirthday}", "Дата рождения");
                    placeholders.Add("{CertificateNumber}", "Номер свидетельства");
                    placeholders.Add("{GroupName}", "Название группы");
                    placeholders.Add("{GroupStartDate}", "Дата начала обучения");
                    placeholders.Add("{GroupEndDate}", "Дата окончания обучения");
                    placeholders.Add("{CURRENT_DATE}", "Текущая дата");
                    placeholders.Add("{ORGANIZATION_NAME}", "Название организации");
                    break;

                default:
                    placeholders.Add("{GroupNumber}", "Номер группы");
                    placeholders.Add("{LastName}", "Фамилия");
                    placeholders.Add("{FirstName}", "Имя");
                    placeholders.Add("{MiddleName}", "Отчество");
                    placeholders.Add("{DateBirthday}", "Дата рождения");
                    placeholders.Add("{BirthPlace}", "Место рождения");
                    placeholders.Add("{PlaceResidence}", "Адрес регистрации");
                    placeholders.Add("{StudentPhone}", "Телефон");
                    placeholders.Add("{StudentEmail}", "Email");
                    placeholders.Add("{Citizenship}", "Гражданство");
                    placeholders.Add("{Gender}", "Пол");
                    placeholders.Add("{Age}", "Возраст");
                    placeholders.Add("{CURRENT_DATE}", "Текущая дата");
                    placeholders.Add("{CURRENT_YEAR}", "Текущий год");
                    placeholders.Add("{ORGANIZATION_NAME}", "Название организации");
                    placeholders.Add("{STUDENT_COUNT}", "Количество студентов");
                    break;
            }

            for (int i = 1; i <= 20; i++)
            {
                placeholders.Add($"{{LastName{i}}}", $"Фамилия студента {i}");
                placeholders.Add($"{{FirstName{i}}}", $"Имя студента {i}");
                placeholders.Add($"{{MiddleName{i}}}", $"Отчество студента {i}");
                placeholders.Add($"{{StudentPhone{i}}}", $"Телефон студента {i}");
                placeholders.Add($"{{BirthDate{i}}}", $"Дата рождения студента {i}");
                placeholders.Add($"{{CertificateNumber{i}}}", $"Номер свидетельства студента {i}");
                placeholders.Add($"{{PassportSeries{i}}}", $"Серия паспорта студента {i}");
                placeholders.Add($"{{PassportNumber{i}}}", $"Номер паспорта студента {i}");
            }

            return placeholders;
        }

        private void UpdateTemplatePlaceholders()
        {
            var newPlaceholders = new Dictionary<string, string>();
            foreach (var item in PlaceholdersView)
            {
                if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                {
                    newPlaceholders[item.Key] = item.Value;
                }
            }
            Template.Placeholders = newPlaceholders;
        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Документы Word (*.docx;*.doc)|*.docx;*.doc|Документы Excel (*.xlsx;*.xls)|*.xlsx;*.xls|Все файлы (*.*)|*.*",
                Title = "Выберите файл шаблона"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Template.FilePath = openFileDialog.FileName;

                if (string.IsNullOrWhiteSpace(Template.TemplateName))
                {
                    Template.TemplateName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                }

                if (string.IsNullOrWhiteSpace(Template.DocumentType))
                {
                    var fileName = Template.TemplateName.ToLower();
                    if (fileName.Contains("договор")) Template.DocumentType = "Договор";
                    else if (fileName.Contains("заявление")) Template.DocumentType = "Заявление";
                    else if (fileName.Contains("справка")) Template.DocumentType = "Справка";
                    else if (fileName.Contains("свидетельство")) Template.DocumentType = "Свидетельство";
                    else Template.DocumentType = "Другой";

                    var defaultPlaceholders = GetDefaultPlaceholders(Template.DocumentType);
                    Template.Placeholders = defaultPlaceholders;
                    PlaceholdersView = new ObservableCollection<KeyValuePair<string, string>>(defaultPlaceholders);
                }

                OnPropertyChanged(nameof(Template));
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Template.TemplateName))
            {
                MessageBox.Show("Введите название шаблона", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TemplateNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Template.FilePath))
            {
                MessageBox.Show("Выберите файл шаблона", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!System.IO.File.Exists(Template.FilePath))
            {
                MessageBox.Show("Выбранный файл не существует", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Template.ModifiedDate = DateTime.Now;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DocumentTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DocumentTypeComboBox.SelectedItem != null && !_isEditMode)
            {
                var selectedType = (DocumentTypeComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();
                if (!string.IsNullOrEmpty(selectedType))
                {
                    var defaultPlaceholders = GetDefaultPlaceholders(selectedType);
                    Template.Placeholders = defaultPlaceholders;
                    PlaceholdersView = new ObservableCollection<KeyValuePair<string, string>>(defaultPlaceholders);
                }
            }
        }

        private string GetDocumentTypeFromName(string fileName)
        {
            string lowerName = fileName.ToLower();

            if (lowerName.Contains("договор"))
            {
                if (lowerName.Contains("категория а") || lowerName.Contains("кат. а") || lowerName.Contains("договор а"))
                    return "Договор категория A";
                else if (lowerName.Contains("категория b") || lowerName.Contains("кат. b") || lowerName.Contains("договор b"))
                    return "Договор категория B";
                else if (lowerName.Contains("категория c") || lowerName.Contains("кат. c") || lowerName.Contains("договор c"))
                    return "Договор категория C";
                else if (lowerName.Contains("категория d") || lowerName.Contains("кат. d") || lowerName.Contains("договор d"))
                    return "Договор категория D";
                else if (lowerName.Contains("категория m") || lowerName.Contains("кат. m") || lowerName.Contains("договор m"))
                    return "Договор категория M";
                else
                    return "Договор";
            }
            if (lowerName.Contains("заявление")) return "Заявление";
            if (lowerName.Contains("справка")) return "Справка";
            if (lowerName.Contains("свидетельство")) return "Свидетельство";
            return "Другой";
        }
    }
}