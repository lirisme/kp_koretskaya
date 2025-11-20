using DrivingSchool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace DrivingSchool.Services
{
    public class DocumentGenerator
    {
        private readonly XmlDataService _dataService;

        public DocumentGenerator(XmlDataService dataService)
        {
            _dataService = dataService;
        }

        public bool GenerateDocument(List<Student> students, DocumentTemplate template, string outputPath)
        {
            try
            {
                if (!File.Exists(template.FilePath))
                {
                    MessageBox.Show($"Файл шаблона не найден: {template.FilePath}", "Ошибка");
                    return false;
                }

                var replacementData = GetUniversalReplacementData(students, template);

                string extension = Path.GetExtension(outputPath).ToLower();

                if (extension == ".docx" || extension == ".doc")
                {
                    File.Copy(template.FilePath, outputPath, true);
                    return ReplaceInWordDocument(outputPath, replacementData);
                }
                else if (extension == ".xlsx" || extension == ".xls")
                {
                    File.Copy(template.FilePath, outputPath, true);
                    return ReplaceInExcelDocument(outputPath, replacementData, students);
                }

                MessageBox.Show($"Неподдерживаемый формат файла: {extension}", "Ошибка");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации документа: {ex.Message}", "Ошибка");
                return false;
            }
        }

        private Dictionary<string, string> GetUniversalReplacementData(List<Student> students, DocumentTemplate template)
        {
            var data = new Dictionary<string, string>();

            data["{CurrentDay}"] = DateTime.Now.Day.ToString();
            data["{CurrentDate}"] = DateTime.Now.ToString("dd.MM.yyyy");
            data["{CurrentYear}"] = DateTime.Now.Year.ToString();
            data["{CurrentMonth}"] = DateTime.Now.Month.ToString();
            data["{StudentCount}"] = students.Count.ToString();

            if (students.Count == 1)
            {
                var student = students[0];
                FillStudentData(data, student);
            }
            else
            {
                FillMultipleStudentsData(data, students);
            }

            return data;
        }

        private void FillStudentData(Dictionary<string, string> data, Student student)
        {
            // Основные данные студента
            data["{GroupNumber}"] = GetGroupNumber(student.GroupId);
            data["{LastName}"] = student.LastName;
            data["{FirstName}"] = student.FirstName;
            data["{MiddleName}"] = student.MiddleName;
            data["{DateBirthday}"] = student.BirthDate.ToString("dd.MM.yyyy");
            data["{BirthPlace}"] = student.BirthPlace;
            data["{PlaceResidence}"] = GetRegistrationAddress(student.Id) ?? student.BirthPlace;
            data["{StudentPhone}"] = student.Phone;
            data["{StudentEmail}"] = student.Email;
            data["{Citizenship}"] = student.Citizenship;
            data["{Gender}"] = student.Gender;
            data["{Age}"] = student.Age.ToString();

            // Паспортные данные
            var passport = _dataService.LoadPassportData().Passports
                .FirstOrDefault(p => p.StudentId == student.Id);
            if (passport != null)
            {
                data["{PassportSeries}"] = passport.Series;
                data["{PassportNumber}"] = passport.Number;
                data["{PassportIssuedBy}"] = passport.IssuedBy;
                data["{PassportDivisionCode}"] = passport.DivisionCode;
                data["{PassportIssueDate}"] = passport.IssueDate.ToString("dd.MM.yyyy");
                data["{DocumentType}"] = passport.DocumentType;
                data["{DivisionCode}"] = passport.DivisionCode;
            }

            // Медицинская справка
            var medical = _dataService.LoadMedicalData().Certificates
                .FirstOrDefault(m => m.StudentId == student.Id);
            if (medical != null)
            {
                data["{MedicalSeries}"] = medical.Series;
                data["{MedicalNumber}"] = medical.Number;
                data["{MedicalIssueDate}"] = medical.IssueDate.ToString("dd.MM.yyyy");
                data["{MedicalInstitution}"] = medical.MedicalInstitution;
            }

            // Данные СНИЛС
            var snils = _dataService.LoadSNILSData().SNILSList
                .FirstOrDefault(s => s.StudentId == student.Id);
            if (snils != null)
            {
                data["{SnilsNumber}"] = snils.Number;
                data["{SnilsIssueDate}"] = snils.IssueDate?.ToString("dd.MM.yyyy") ?? "";
                data["{SnilsIssuedBy}"] = snils.IssuedBy;
            }

            // Данные группы
            var group = _dataService.LoadStudyGroups().Groups
                .FirstOrDefault(g => g.Id == student.GroupId);
            if (group != null)
            {
                data["{GroupName}"] = group.Name;
                data["{GroupStartDate}"] = group.StartDate.ToString("dd.MM.yyyy");
                data["{GroupEndDate}"] = group.EndDate.ToString("dd.MM.yyyy");
                data["{GroupDuration}"] = group.Duration;
            }

            // Адрес регистрации
            var address = _dataService.LoadAddresses().Addresses
                .FirstOrDefault(a => a.StudentId == student.Id);
            if (address != null)
            {
                data["{RegistrationAddress}"] = address.FullAddress;
                data["{Region}"] = address.Region;
                data["{City}"] = address.City;
                data["{Street}"] = address.Street;
                data["{House}"] = address.House;
                data["{Building}"] = address.Building;
                data["{Apartment}"] = address.Apartment;
                data["{PostalCode}"] = address.PostalCode;
            }

            // Водительское удостоверение
            var license = _dataService.LoadDrivingLicenses().Licenses
                .FirstOrDefault(l => l.StudentId == student.Id);
            if (license != null)
            {
                data["{LicenseSeries}"] = license.Series;
                data["{LicenseNumber}"] = license.Number;
                data["{LicenseIssueDate}"] = license.IssueDate.ToString("dd.MM.yyyy");
                data["{LicenseExpiryDate}"] = license.ExpiryDate.ToString("dd.MM.yyyy");
                data["{LicenseCategories}"] = license.Categories;
                data["{LicenseIssuedBy}"] = license.IssuedBy;
                data["{LicenseDivisionCode}"] = license.DivisionCode;
                data["{LicenseExperience}"] = license.ExperienceYears.ToString();
                data["{LicenseStatus}"] = license.Status;
            }

            // Свидетельство об окончании
            var certificate = _dataService.LoadCertificates().Certificates
                .FirstOrDefault(c => c.StudentId == student.Id);
            if (certificate != null)
            {
                data["{CertificateSeries}"] = certificate.CertificateSeries;
                data["{CertificateNumber}"] = certificate.CertificateNumber;
                data["{CertificateIssueDate}"] = certificate.IssueDate.ToString("dd.MM.yyyy");
                data["{CertificateCategory}"] = certificate.CategoryCode;
            }

            // Стоимость обучения
            var tuition = _dataService.LoadStudentTuitions().Tuitions
                .FirstOrDefault(t => t.StudentId == student.Id);
            if (tuition != null)
            {
                data["{TuitionFullAmount}"] = tuition.FullAmount.ToString("N2");
                data["{TuitionDiscount}"] = tuition.Discount.ToString("N2");
                data["{TuitionFinalAmount}"] = tuition.FinalAmount.ToString("N2");
                data["{TuitionCreatedDate}"] = tuition.CreatedDate.ToString("dd.MM.yyyy");
            }

            // Платежи
            var payments = _dataService.LoadPayments().Payments
                .Where(p => p.StudentId == student.Id)
                .OrderBy(p => p.PaymentDate)
                .ToList();

            if (payments.Any())
            {
                data["{TotalPayments}"] = payments.Sum(p => p.Amount).ToString("N2");
                data["{LastPaymentDate}"] = payments.Last().PaymentDate.ToString("dd.MM.yyyy");
                data["{LastPaymentAmount}"] = payments.Last().Amount.ToString("N2");
                data["{FirstPaymentDate}"] = payments.First().PaymentDate.ToString("dd.MM.yyyy");
                data["{FirstPaymentAmount}"] = payments.First().Amount.ToString("N2");
            }

            // Тариф
            var tariff = _dataService.LoadTariffs().Tariffs.FirstOrDefault();
            if (tariff != null)
            {
                data["{TariffName}"] = tariff.Name;
                data["{TariffDescription}"] = tariff.Description;
                data["{TariffBaseCost}"] = tariff.BaseCost.ToString("N2");
                data["{TariffCategory}"] = tariff.Category;
                data["{TariffDuration}"] = $"{tariff.DurationMonths} мес.";
                data["{TariffPracticeHours}"] = tariff.PracticeHours.ToString();
            }
        }

        private void FillMultipleStudentsData(Dictionary<string, string> data, List<Student> students)
        {
            for (int i = 0; i < students.Count && i < 50; i++)
            {
                var student = students[i];
                int rowNumber = i + 1;

                data[$"{{LastName{rowNumber}}}"] = student.LastName;
                data[$"{{FirstName{rowNumber}}}"] = student.FirstName;
                data[$"{{MiddleName{rowNumber}}}"] = student.MiddleName;
                data[$"{{StudentPhone{rowNumber}}}"] = student.Phone;
                data[$"{{BirthDate{rowNumber}}}"] = student.BirthDate.ToString("dd.MM.yyyy");
                data[$"{{Gender{rowNumber}}}"] = student.Gender;
                data[$"{{Citizenship{rowNumber}}}"] = student.Citizenship;
                data[$"{{CertificateNumber{rowNumber}}}"] = $"ЭА-{DateTime.Now:MM}-{rowNumber:000}";

                // Данные паспорта
                var passport = _dataService.LoadPassportData().Passports
                    .FirstOrDefault(p => p.StudentId == student.Id);
                if (passport != null)
                {
                    data[$"{{PassportSeries{rowNumber}}}"] = passport.Series;
                    data[$"{{PassportNumber{rowNumber}}}"] = passport.Number;
                    data[$"{{PassportIssuedBy{rowNumber}}}"] = passport.IssuedBy;
                }

                // Данные мед. справки
                var medical = _dataService.LoadMedicalData().Certificates
                    .FirstOrDefault(m => m.StudentId == student.Id);
                if (medical != null)
                {
                    data[$"{{MedicalSeries{rowNumber}}}"] = medical.Series;
                    data[$"{{MedicalNumber{rowNumber}}}"] = medical.Number;
                }
            }

            for (int i = students.Count + 1; i <= 50; i++)
            {
                data[$"{{LastName{i}}}"] = "";
                data[$"{{FirstName{i}}}"] = "";
                data[$"{{MiddleName{i}}}"] = "";
                data[$"{{StudentPhone{i}}}"] = "";
                data[$"{{BirthDate{i}}}"] = "";
                data[$"{{Gender{i}}}"] = "";
                data[$"{{Citizenship{i}}}"] = "";
                data[$"{{CertificateNumber{i}}}"] = "";
                data[$"{{PassportSeries{i}}}"] = "";
                data[$"{{PassportNumber{i}}}"] = "";
                data[$"{{PassportIssuedBy{i}}}"] = "";
                data[$"{{MedicalSeries{i}}}"] = "";
                data[$"{{MedicalNumber{i}}}"] = "";
            }

            if (students.Count > 0)
            {
                var firstStudent = students[0];
                data["{GroupNumber}"] = GetGroupNumber(firstStudent.GroupId);

                var group = _dataService.LoadStudyGroups().Groups
                    .FirstOrDefault(g => g.Id == firstStudent.GroupId);
                if (group != null)
                {
                    data["{GroupName}"] = group.Name;
                    data["{GroupStartDate}"] = group.StartDate.ToString("dd.MM.yyyy");
                    data["{GroupEndDate}"] = group.EndDate.ToString("dd.MM.yyyy");
                    data["{GroupDuration}"] = group.Duration;
                }

                data["{ProtocolNumber}"] = $"{DateTime.Now:MM}-{DateTime.Now:Year}";
            }
        }

        private bool ReplaceInWordDocument(string filePath, Dictionary<string, string> replacementData)
        {
            dynamic wordApp = null;
            dynamic document = null;

            try
            {
                Type wordType = Type.GetTypeFromProgID("Word.Application");
                if (wordType == null)
                {
                    MessageBox.Show("Microsoft Word не установлен!", "Ошибка");
                    return false;
                }

                wordApp = Activator.CreateInstance(wordType);
                wordApp.Visible = false;
                wordApp.ScreenUpdating = false;

                document = wordApp.Documents.Open(filePath);

                int replacementCount = 0;

                foreach (var replacement in replacementData)
                {
                    string searchText = replacement.Key.Trim('{', '}');
                    string replaceText = replacement.Value ?? "";

                    try
                    {
                        bool found = document.Content.Find.Execute(
                            FindText: searchText,
                            ReplaceWith: replaceText,
                            Replace: 2
                        );

                        if (found)
                        {
                            replacementCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при замене {searchText}: {ex.Message}");
                    }
                }

                document.Save();
                return replacementCount > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при замене в Word: {ex.Message}", "Ошибка");
                return false;
            }
            finally
            {
                try
                {
                    if (document != null)
                    {
                        document.Close(SaveChanges: false);
                        Marshal.ReleaseComObject(document);
                    }
                    if (wordApp != null)
                    {
                        wordApp.Quit(SaveChanges: false);
                        Marshal.ReleaseComObject(wordApp);
                    }
                }
                catch { }
            }
        }

        private bool ReplaceInExcelDocument(string filePath, Dictionary<string, string> replacementData, List<Student> students)
        {
            dynamic excelApp = null;
            dynamic workbook = null;

            try
            {
                Type excelType = Type.GetTypeFromProgID("Excel.Application");
                if (excelType == null)
                {
                    MessageBox.Show("Microsoft Excel не установлен!", "Ошибка");
                    return false;
                }

                excelApp = Activator.CreateInstance(excelType);
                excelApp.Visible = false;
                excelApp.ScreenUpdating = false;
                excelApp.DisplayAlerts = false;

                workbook = excelApp.Workbooks.Open(filePath);

                int replacementCount = 0;

                foreach (dynamic worksheet in workbook.Worksheets)
                {
                    try
                    {
                        replacementCount += ReplaceAllPlaceholdersInWorksheet(worksheet, replacementData);
                        replacementCount += FillStudentTablesAutomatically(worksheet, students);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при обработке листа {worksheet.Name}: {ex.Message}");
                    }
                }

                workbook.Save();
                MessageBox.Show($"Заполнено строк с данными студентов: {replacementCount}", "Информация");
                return replacementCount > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при замене в Excel: {ex.Message}", "Ошибка");
                return false;
            }
            finally
            {
                try
                {
                    if (workbook != null)
                    {
                        workbook.Close(SaveChanges: true);
                        Marshal.ReleaseComObject(workbook);
                    }
                    if (excelApp != null)
                    {
                        excelApp.Quit();
                        Marshal.ReleaseComObject(excelApp);
                    }
                }
                catch { }
            }
        }

        private int ReplaceAllPlaceholdersInWorksheet(dynamic worksheet, Dictionary<string, string> replacementData)
        {
            int replacementCount = 0;

            try
            {
                dynamic usedRange = worksheet.UsedRange;
                if (usedRange == null) return 0;

                int rowCount = usedRange.Rows.Count;
                int colCount = usedRange.Columns.Count;

                for (int row = 1; row <= rowCount; row++)
                {
                    for (int col = 1; col <= colCount; col++)
                    {
                        try
                        {
                            dynamic cell = worksheet.Cells[row, col];
                            if (cell == null) continue;

                            object cellValue = cell.Value;
                            if (cellValue == null) continue;

                            string textValue = cellValue.ToString();
                            string originalText = textValue;

                            foreach (var replacement in replacementData)
                            {
                                if (textValue.Contains(replacement.Key))
                                {
                                    textValue = textValue.Replace(replacement.Key, replacement.Value ?? "");
                                }
                            }

                            if (textValue != originalText)
                            {
                                cell.Value = textValue;
                                replacementCount++;
                            }

                            Marshal.ReleaseComObject(cell);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка в ячейке [{row},{col}]: {ex.Message}");
                        }
                    }
                }

                Marshal.ReleaseComObject(usedRange);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке листа: {ex.Message}");
            }

            return replacementCount;
        }

        private int FillStudentTablesAutomatically(dynamic worksheet, List<Student> students)
        {
            int filledRows = 0;

            try
            {
                TablePosition tablePosition = FindStudentTablePosition(worksheet);

                if (tablePosition != null && students.Count > 0)
                {
                    filledRows = FillDynamicStudentTable(worksheet, tablePosition, students);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при заполнении таблицы: {ex.Message}");
            }

            return filledRows;
        }

        private TablePosition FindStudentTablePosition(dynamic worksheet)
        {
            try
            {
                dynamic usedRange = worksheet.UsedRange;
                if (usedRange == null) return null;

                int rowCount = usedRange.Rows.Count;
                int colCount = usedRange.Columns.Count;

                for (int row = 30; row <= Math.Min(40, rowCount); row++)
                {
                    int headerCount = 0;
                    List<string> foundHeaders = new List<string>();

                    for (int col = 1; col <= colCount; col++)
                    {
                        dynamic cell = worksheet.Cells[row, col];
                        if (cell?.Value == null)
                        {
                            Marshal.ReleaseComObject(cell);
                            continue;
                        }

                        string cellValue = cell.Value.ToString();
                        Marshal.ReleaseComObject(cell);

                        if (IsTableHeader(cellValue))
                        {
                            headerCount++;
                            foundHeaders.Add(cellValue);
                        }
                    }

                    if (headerCount >= 5)
                    {
                        Marshal.ReleaseComObject(usedRange);
                        return new TablePosition
                        {
                            StartRow = row + 2,
                            StartColumn = 1,
                            HeaderRow = row
                        };
                    }
                }

                Marshal.ReleaseComObject(usedRange);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при поиске таблицы: {ex.Message}");
            }

            return null;
        }

        private bool IsTableHeader(string cellValue)
        {
            if (string.IsNullOrEmpty(cellValue))
                return false;

            string[] tableHeaders = {
                "№", "Фамилия", "Имя", "Отчество", "Дата рождения", "Паспорт",
                "Серия", "Номер", "Выдан", "Кем выдан", "СНИЛС", "Медсправка",
                "Телефон", "Пол", "Гражданство", "Адрес", "Регистрация"
            };

            string lowerCellValue = cellValue.Trim().ToLower();

            return tableHeaders.Any(header =>
                lowerCellValue.Contains(header.ToLower()));
        }

        private int FillDynamicStudentTable(dynamic worksheet, TablePosition position, List<Student> students)
        {
            int filledRows = 0;

            try
            {
                var columnMapping = MapTableColumns(worksheet, position.HeaderRow);

                for (int i = 0; i < students.Count; i++)
                {
                    var student = students[i];
                    int currentRow = position.StartRow + i;

                    if (FillStudentRowDynamically(worksheet, currentRow, student, i + 1, columnMapping))
                    {
                        filledRows++;
                    }

                    if (currentRow >= position.StartRow + 100)
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при заполнении динамической таблицы: {ex.Message}");
            }

            return filledRows;
        }

        private bool FillStudentRowDynamically(dynamic worksheet, int row, Student student, int index, Dictionary<string, int> columnMapping)
        {
            try
            {
                foreach (var mapping in columnMapping)
                {
                    string value = GetStudentDataByColumn(student, mapping.Key, index);
                    if (value != null)
                    {
                        worksheet.Cells[row, mapping.Value].Value = value;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при заполнении строки {row}: {ex.Message}");
                return false;
            }
        }

        private string GetStudentDataByColumn(Student student, string columnName, int index)
        {
            switch (columnName.ToLower())
            {
                case "№":
                case "n":
                case "п/п":
                case "порядковый номер":
                    return index.ToString();
                case "фамилия":
                    return student.LastName;
                case "имя":
                    return student.FirstName;
                case "отчество":
                    return student.MiddleName;
                case "дата рождения":
                case "рождение":
                case "дата рожд.":
                    return student.BirthDate.ToString("dd.MM.yyyy");
                case "пол":
                    return ConvertGenderToShortFormat(student.Gender);
                case "гражданство":
                    return student.Citizenship;
                case "телефон":
                case "телефонный":
                case "телефон номер":
                case "контактный телефон":
                    return student.Phone;
                case "серия паспорта":
                case "серия":
                case "пас. серия":
                    return _dataService.LoadPassportData().Passports
                        .FirstOrDefault(p => p.StudentId == student.Id)?.Series ?? "";
                case "номер паспорта":
                case "пас. номер":
                case "номер пасп.":
                    return _dataService.LoadPassportData().Passports
                        .FirstOrDefault(p => p.StudentId == student.Id)?.Number ?? "";
                case "кем выдан":
                case "выдан":
                case "орган выдавший":
                case "паспорт выдан":
                case "кем выдан документ":
                    return _dataService.LoadPassportData().Passports
                        .FirstOrDefault(p => p.StudentId == student.Id)?.IssuedBy ?? "";
                case "адрес":
                case "регистрация":
                case "место жительства":
                case "адрес проживания":
                case "адрес регистрации":
                    return GetRegistrationAddress(student.Id) ?? student.BirthPlace;
                case "снилс":
                case "номер снилс":
                case "снилс номер":
                    return _dataService.LoadSNILSData().SNILSList
                        .FirstOrDefault(s => s.StudentId == student.Id)?.Number ?? "";
                case "медсправка":
                case "медицинская":
                case "мед. справка":
                case "медицинская справка":
                case "серия и номер медсправки":
                    var medical = _dataService.LoadMedicalData().Certificates
                        .FirstOrDefault(m => m.StudentId == student.Id);
                    return medical != null ? $"{medical.Series} {medical.Number}" : "";
                case "дата выдачи паспорта":
                case "дата выдачи":
                    return _dataService.LoadPassportData().Passports
                        .FirstOrDefault(p => p.StudentId == student.Id)?.IssueDate.ToString("dd.MM.yyyy") ?? "";
                case "код подразделения":
                case "подразделение":
                    return _dataService.LoadPassportData().Passports
                        .FirstOrDefault(p => p.StudentId == student.Id)?.DivisionCode ?? "";
                case "место рождения":
                    return student.BirthPlace;
                case "email":
                case "e-mail":
                case "электронная почта":
                    return student.Email;
                case "наименование документа":
                case "документ":
                case "тип документа":
                    return _dataService.LoadPassportData().Passports
                        .FirstOrDefault(p => p.StudentId == student.Id)?.DocumentType ?? "";
                case "серия и номер паспорта":
                    var passport = _dataService.LoadPassportData().Passports
                        .FirstOrDefault(p => p.StudentId == student.Id);
                    return passport != null ? $"{passport.Series} {passport.Number}" : "";
                case "страна":
                    return "Россия";
                case "регион":
                case "регион регистрации":
                    return _dataService.LoadAddresses().Addresses
                        .FirstOrDefault(a => a.StudentId == student.Id)?.Region ?? "";
                case "населенный пункт":
                case "город":
                case "поселок":
                case "село":
                    return GetAddressComponent(student.Id, "city") ?? "";
                case "улица":
                case "проспект":
                case "переулок":
                    return GetAddressComponent(student.Id, "street") ?? "";
                case "дом":
                case "номер дома":
                    return GetAddressComponent(student.Id, "house") ?? "";
                case "корпус":
                case "строение":
                    return GetAddressComponent(student.Id, "building") ?? "";
                case "квартира":
                case "офис":
                case "помещение":
                    return GetAddressComponent(student.Id, "apartment") ?? "";
                case "дата выдачи медсправки":
                case "дата выдачи медицинской справки":
                    medical = _dataService.LoadMedicalData().Certificates
                        .FirstOrDefault(m => m.StudentId == student.Id);
                    return medical?.IssueDate.ToString("dd.MM.yyyy") ?? "";
                case "регион выдачи":
                case "регион выдачи медсправки":
                    return "Оренбургская область";
                case "наименование мед.учреждения":
                case "медицинское учреждение":
                case "мед. учреждение":
                    medical = _dataService.LoadMedicalData().Certificates
                        .FirstOrDefault(m => m.StudentId == student.Id);
                    return medical?.MedicalInstitution ?? "";
                case "серия и номер водительского удостоверения":
                case "водительское удостоверение":
                case "удостоверение":
                    return "";
                case "категории":
                case "категории (подкатегории)":
                    return "B";
                case "стаж":
                    return "";
                default:
                    Console.WriteLine($"Неизвестный заголовок столбца: {columnName}");
                    return null;
            }
        }

        private string GetAddressComponent(int studentId, string componentType)
        {
            var address = _dataService.LoadAddresses().Addresses
                .FirstOrDefault(a => a.StudentId == studentId);

            if (address == null) return "";

            switch (componentType.ToLower())
            {
                case "region":
                    return address.Region ?? "Оренбургская область";
                case "city":
                case "locality":
                    return address.City ?? "";
                case "street":
                    return address.Street ?? "";
                case "house":
                    return address.House ?? "";
                case "building":
                    return address.Building ?? "";
                case "apartment":
                    return address.Apartment ?? "";
                default:
                    return address.FullAddress ?? "";
            }
        }

        private string ConvertGenderToShortFormat(string gender)
        {
            if (string.IsNullOrEmpty(gender))
                return "";

            string lowerGender = gender.ToLower().Trim();

            if (lowerGender.Contains("муж") || lowerGender == "м" || lowerGender.Contains("male"))
                return "М";
            else if (lowerGender.Contains("жен") || lowerGender == "ж" || lowerGender.Contains("female"))
                return "Ж";
            else
                return gender;
        }

        private Dictionary<string, int> MapTableColumns(dynamic worksheet, int headerRow)
        {
            var mapping = new Dictionary<string, int>();

            try
            {
                dynamic usedRange = worksheet.UsedRange;
                int colCount = usedRange.Columns.Count;

                for (int col = 1; col <= colCount; col++)
                {
                    dynamic cell = worksheet.Cells[headerRow, col];
                    if (cell?.Value != null)
                    {
                        string header = cell.Value.ToString().Trim();
                        if (!string.IsNullOrEmpty(header))
                        {
                            mapping[header] = col;
                        }
                    }
                    Marshal.ReleaseComObject(cell);
                }

                Marshal.ReleaseComObject(usedRange);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при маппинге столбцов: {ex.Message}");
            }

            return mapping;
        }

        private string GetGroupNumber(int groupId)
        {
            if (groupId > 0)
            {
                var group = _dataService.LoadStudyGroups().Groups
                    .FirstOrDefault(g => g.Id == groupId);
                return group?.Name ?? "";
            }
            return "";
        }

        private string GetRegistrationAddress(int studentId)
        {
            var address = _dataService.LoadAddresses().Addresses
                .FirstOrDefault(a => a.StudentId == studentId);
            return address?.FullAddress ?? "";
        }
    }

    public class TablePosition
    {
        public int StartRow { get; set; }
        public int StartColumn { get; set; }
        public int HeaderRow { get; set; }
    }
}