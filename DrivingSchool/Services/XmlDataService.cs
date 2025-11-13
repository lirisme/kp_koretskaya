using DrivingSchool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace DrivingSchool.Services
{
    public class XmlDataService
    {
        private readonly string _dataDirectory;

        public XmlDataService()
        {
            _dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            if (!Directory.Exists(_dataDirectory))
                Directory.CreateDirectory(_dataDirectory);

            InitializeDataFiles();
        }

        private void InitializeDataFiles()
        {
            // students.xml
            string studentsFile = Path.Combine(_dataDirectory, "students.xml");
            if (!File.Exists(studentsFile))
            {
                var initialStudents = new StudentCollection
                {
                    Students =
                    {
                        new Student
                        {
                            Id = 1,
                            LastName = "Иванов",
                            FirstName = "Петр",
                            MiddleName = "Сергеевич",
                            Gender = "Мужской",
                            BirthDate = new DateTime(1990, 5, 15),
                            BirthPlace = "г. Оренбург",
                            Phone = "+79001234567",
                            Email = "ivanov@mail.ru",
                            Citizenship = "Российская Федерация",
                            GroupId = 1,
                            ContractId = 1
                        },
                        new Student
                        {
                            Id = 2,
                            LastName = "Петрова",
                            FirstName = "Мария",
                            MiddleName = "Ивановна",
                            Gender = "Женский",
                            BirthDate = new DateTime(1995, 8, 20),
                            BirthPlace = "г. Оренбург",
                            Phone = "+79007654321",
                            Email = "petrova@mail.ru",
                            Citizenship = "Российская Федерация",
                            GroupId = 1,
                            ContractId = 2
                        }
                    }
                };
                SaveData("students.xml", initialStudents);
            }

            // student_passport_data.xml
            string passportFile = Path.Combine(_dataDirectory, "student_passport_data.xml");
            if (!File.Exists(passportFile))
            {
                var initialPassports = new StudentPassportDataCollection
                {
                    Passports =
                    {
                        new StudentPassportData
                        {
                            Id = 1,
                            StudentId = 1,
                            DocumentType = "Паспорт РФ",
                            Series = "5314",
                            Number = "501001",
                            IssuedBy = "ОУФМС России по Оренбургской области",
                            DivisionCode = "560-001",
                            IssueDate = new DateTime(2015, 1, 29)
                        },
                        new StudentPassportData
                        {
                            Id = 2,
                            StudentId = 2,
                            DocumentType = "Паспорт РФ",
                            Series = "5315",
                            Number = "501002",
                            IssuedBy = "ОУФМС России по Оренбургской области",
                            DivisionCode = "560-001",
                            IssueDate = new DateTime(2016, 3, 15)
                        }
                    }
                };
                SaveData("student_passport_data.xml", initialPassports);
            }

            // student_snils.xml
            string snilsFile = Path.Combine(_dataDirectory, "student_snils.xml");
            if (!File.Exists(snilsFile))
            {
                var initialSNILS = new StudentSNILSCollection();
                SaveData("student_snils.xml", initialSNILS);
            }

            // student_medical.xml
            string medicalFile = Path.Combine(_dataDirectory, "student_medical.xml");
            if (!File.Exists(medicalFile))
            {
                var initialMedical = new StudentMedicalCertificateCollection();
                SaveData("student_medical.xml", initialMedical);
            }

            // tariffs.xml
            string tariffsFile = Path.Combine(_dataDirectory, "tariffs.xml");
            if (!File.Exists(tariffsFile))
            {
                var initialTariffs = new TariffCollection
                {
                    Tariffs =
                    {
                        new Tariff
                        {
                            Id = 1,
                            Name = "Стандарт (категория B)",
                            Description = "Полный курс обучения на категорию B. Включает теорию и практику.",
                            BaseCost = 35000.00m,
                            Category = "B",
                            DurationMonths = 3,
                            PracticeHours = 56,
                        },
                        new Tariff
                        {
                            Id = 2,
                            Name = "Премиум (категория B)",
                            Description = "Расширенный курс с дополнительными занятиями и индивидуальным подходом",
                            BaseCost = 45000.00m,
                            Category = "B",
                            DurationMonths = 3,
                            PracticeHours = 70,
                        },
                        new Tariff
                        {
                            Id = 3,
                            Name = "Экспресс (категория B)",
                            Description = "Ускоренный курс обучения",
                            BaseCost = 40000.00m,
                            Category = "B",
                            DurationMonths = 2,
                            PracticeHours = 56,
                        }
                    }
                };
                SaveData("tariffs.xml", initialTariffs);
            }

            // employees.xml
            string employeesFile = Path.Combine(_dataDirectory, "employees.xml");
            if (!File.Exists(employeesFile))
            {
                var initialEmployees = new EmployeeCollection
                {
                    Employees =
                    {
                        new Employee
                        {
                            Id = 1,
                            FullName = "Смирнов Алексей Владимирович",
                            Position = "Преподаватель теории",
                            Status = "Активен",
                            Phone = "+79001112233",
                            Email = "smirnov@elitavto.ru",
                            HireDate = new DateTime(2020, 1, 15)
                        },
                        new Employee
                        {
                            Id = 2,
                            FullName = "Козлова Анна Петровна",
                            Position = "Инструктор по вождению",
                            Status = "Активен",
                            Phone = "+79004445566",
                            Email = "kozlova@elitavto.ru",
                            HireDate = new DateTime(2021, 3, 20)
                        }
                    }
                };
                SaveData("employees.xml", initialEmployees);
            }

            

            // vehicle_categories.xml
            string categoriesFile = Path.Combine(_dataDirectory, "vehicle_categories.xml");
            if (!File.Exists(categoriesFile))
            {
                var initialCategories = new VehicleCategoryCollection
                {
                    Categories =
                    {
                        new VehicleCategory { Id = 1, Code = "A", FullName = "Мотоциклы" },
                        new VehicleCategory { Id = 2, Code = "B", FullName = "Легковые автомобили" },
                        new VehicleCategory { Id = 3, Code = "C", FullName = "Грузовые автомобили" },
                        new VehicleCategory { Id = 4, Code = "D", FullName = "Автобусы" },
                        new VehicleCategory { Id = 5, Code = "M", FullName = "Мопеды" }
                    }
                };
                SaveData("vehicle_categories.xml", initialCategories);
            }

           

            // payments.xml
            string paymentsFile = Path.Combine(_dataDirectory, "payments.xml");
            if (!File.Exists(paymentsFile))
            {
                var initialPayments = new PaymentCollection
                {
                    Payments =
                    {
                        new Payment
                        {
                            Id = 1,
                            StudentId = 1,
                            PaymentDate = new DateTime(2024, 1, 10),
                            Amount = 17500.00m,
                            PaymentType = "Аванс",
                        },
                        new Payment
                        {
                            Id = 2,
                            StudentId = 2,
                            PaymentDate = new DateTime(2024, 1, 12),
                            Amount = 22500.00m,
                            PaymentType = "Аванс",
                        }
                    }
                };
                SaveData("payments.xml", initialPayments);
            }

            // student_tuitions.xml
            string tuitionsFile = Path.Combine(_dataDirectory, "student_tuitions.xml");
            if (!File.Exists(tuitionsFile))
            {
                var initialTuitions = new StudentTuitionCollection();
                SaveData("student_tuitions.xml", initialTuitions);
            }

            // study_groups.xml
            string groupsFile = Path.Combine(_dataDirectory, "study_groups.xml");
            if (!File.Exists(groupsFile))
            {
                var initialGroups = new StudyGroupCollection
                {
                    Groups =
        {
            new StudyGroup
            {
                Id = 1,
                Name = "01-24",
                StartDate = new DateTime(2024, 4, 22),
                EndDate = new DateTime(2024, 6, 22),
                Status = "Завершена"
            },
            new StudyGroup
            {
                Id = 2,
                Name = "02-24",
                StartDate = new DateTime(2024, 5, 2),
                EndDate = new DateTime(2024, 7, 2),
                Status = "Завершена"
            },
            new StudyGroup
            {
                Id = 3,
                Name = "03-24",
                StartDate = new DateTime(2024, 5, 13),
                EndDate = new DateTime(2024, 7, 13),
                Status = "Завершена"
            }
        }
                };
                SaveData("study_groups.xml", initialGroups);
            }

            // document_templates.xml
            string templatesFile = Path.Combine(_dataDirectory, "document_templates.xml");
            if (!File.Exists(templatesFile))
            {
                var initialTemplates = new DocumentTemplateCollection
                {
                    Templates =
            {
                new DocumentTemplate
                {
                    Id = 1,
                    TemplateName = "Договор на обучение (стандартный)",
                    DocumentType = "Договор",
                    FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Договор_шаблон.docx"),
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    Placeholders = new Dictionary<string, string>
                    {//ИСПРАВИТЬ
                        { "{STUDENT_FULL_NAME}", "ФИО учащегося" },
                        { "{STUDENT_BIRTH_DATE}", "Дата рождения" },
                        { "{STUDENT_PASSPORT}", "Паспортные данные" },
                        { "{STUDENT_ADDRESS}", "Адрес регистрации" },
                        { "{STUDENT_PHONE}", "Телефон" },
                        { "{CONTRACT_NUMBER}", "Номер договора" },
                        { "{CONTRACT_DATE}", "Дата договора" },
                        { "{TARIFF_NAME}", "Название тарифа" },
                        { "{TARIFF_COST}", "Стоимость обучения" },
                        { "{GROUP_NAME}", "Номер группы" },
                        { "{START_DATE}", "Дата начала обучения" },
                        { "{END_DATE}", "Дата окончания обучения" },
                        { "{ORGANIZATION_NAME}", "Название организации" },
                        { "{CURRENT_DATE}", "Текущая дата" }
                    }
                },
                new DocumentTemplate
                {
                    Id = 2,
                    TemplateName = "Заявление в ГИБДД",
                    DocumentType = "Заявление",
                    FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Заявление_шаблон.docx"),
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    Placeholders = new Dictionary<string, string>
                    {
                        { "{STUDENT_FULL_NAME}", "ФИО учащегося" },
                        { "{STUDENT_BIRTH_DATE}", "Дата рождения" },
                        { "{STUDENT_ADDRESS}", "Адрес регистрации" },
                        { "{STUDENT_PHONE}", "Телефон" },
                        { "{STUDENT_PASSPORT}", "Паспортные данные" },
                        { "{MEDICAL_CERTIFICATE}", "Медицинская справка" },
                        { "{APPLICATION_DATE}", "Дата заявления" },
                        { "{CATEGORY}", "Категория ТС" },
                        { "{ORGANIZATION_NAME}", "Название организации" }
                    }
                }
            }
                };
                SaveData("document_templates.xml", initialTemplates);
            }
            InitializeEmptyFile<StudentDrivingLicenseCollection>("student_driving_licenses.xml");
            InitializeEmptyFile<StudentRegistrationAddressCollection>("student_registration_addresses.xml");
            InitializeEmptyFile<StudentCertificateCollection>("student_certificates.xml");
            InitializeEmptyFile<GeneratedDocumentCollection>("generated_documents.xml");
        }

        private void InitializeEmptyFile<T>(string fileName) where T : new()
        {
            string filePath = Path.Combine(_dataDirectory, fileName);
            if (!File.Exists(filePath))
            {
                SaveData(fileName, new T());
            }
        }

        public T LoadData<T>(string fileName) where T : new()
        {
            try
            {
                string filePath = Path.Combine(_dataDirectory, fileName);
                if (!File.Exists(filePath))
                    return new T();

                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    return (T)serializer.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading {fileName}: {ex.Message}");
                return new T();
            }
        }

        public void SaveData<T>(string fileName, T data)
        {
            try
            {
                string filePath = Path.Combine(_dataDirectory, fileName);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(stream, data);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving {fileName}: {ex.Message}");
                throw;
            }
        }

        // Методы для стоимости обучения студентов
        public StudentTuitionCollection LoadStudentTuitions() =>
            LoadData<StudentTuitionCollection>("student_tuitions.xml");

        public void SaveStudentTuitions(StudentTuitionCollection data) =>
            SaveData("student_tuitions.xml", data);

        // Методы для студентов
        public StudentCollection LoadStudents() => LoadData<StudentCollection>("students.xml");
        public void SaveStudents(StudentCollection students) => SaveData("students.xml", students);

        // Методы для паспортных данных
        public StudentPassportDataCollection LoadPassportData() =>
            LoadData<StudentPassportDataCollection>("student_passport_data.xml");
        public void SavePassportData(StudentPassportDataCollection data) =>
            SaveData("student_passport_data.xml", data);

        // Методы для СНИЛС
        public StudentSNILSCollection LoadSNILSData() =>
            LoadData<StudentSNILSCollection>("student_snils.xml");
        public void SaveSNILSData(StudentSNILSCollection data) =>
            SaveData("student_snils.xml", data);

        // Методы для медицинских справок
        public StudentMedicalCertificateCollection LoadMedicalData() =>
            LoadData<StudentMedicalCertificateCollection>("student_medical.xml");
        public void SaveMedicalData(StudentMedicalCertificateCollection data) =>
            SaveData("student_medical.xml", data);

        // Методы для тарифов
        public TariffCollection LoadTariffs() => LoadData<TariffCollection>("tariffs.xml");
        public void SaveTariffs(TariffCollection tariffs) => SaveData("tariffs.xml", tariffs);

        // Методы для сотрудников
        public EmployeeCollection LoadEmployees() => LoadData<EmployeeCollection>("employees.xml");
        public void SaveEmployees(EmployeeCollection data) => SaveData("employees.xml", data);

        // Методы для учебных групп
        public StudyGroupCollection LoadStudyGroups() => LoadData<StudyGroupCollection>("study_groups.xml");
        public void SaveStudyGroups(StudyGroupCollection data) => SaveData("study_groups.xml", data);

        // Методы для категорий ТС
        public VehicleCategoryCollection LoadVehicleCategories() => LoadData<VehicleCategoryCollection>("vehicle_categories.xml");
        public void SaveVehicleCategories(VehicleCategoryCollection data) => SaveData("vehicle_categories.xml", data);

        // Методы для платежей
        public PaymentCollection LoadPayments() => LoadData<PaymentCollection>("payments.xml");
        public void SavePayments(PaymentCollection data) => SaveData("payments.xml", data);


        // Методы для водительских удостоверений
        public StudentDrivingLicenseCollection LoadDrivingLicenses() => LoadData<StudentDrivingLicenseCollection>("student_driving_licenses.xml");
        public void SaveDrivingLicenses(StudentDrivingLicenseCollection data) => SaveData("student_driving_licenses.xml", data);

        // Методы для адресов регистрации
        public StudentRegistrationAddressCollection LoadAddresses() => LoadData<StudentRegistrationAddressCollection>("student_registration_addresses.xml");
        public void SaveAddresses(StudentRegistrationAddressCollection data) => SaveData("student_registration_addresses.xml", data);

        // Методы для свидетельств об окончании
        public StudentCertificateCollection LoadCertificates() => LoadData<StudentCertificateCollection>("student_certificates.xml");
        public void SaveCertificates(StudentCertificateCollection data) => SaveData("student_certificates.xml", data);

        // Методы для шаблонов документов
        public DocumentTemplateCollection LoadTemplates() =>
            LoadData<DocumentTemplateCollection>("document_templates.xml");

        public void SaveTemplates(DocumentTemplateCollection data) =>
            SaveData("document_templates.xml", data);

        // Методы для сгенерированных документов
        public GeneratedDocumentCollection LoadGeneratedDocuments() => LoadData<GeneratedDocumentCollection>("generated_documents.xml");
        public void SaveGeneratedDocuments(GeneratedDocumentCollection data) => SaveData("generated_documents.xml", data);
    }
}