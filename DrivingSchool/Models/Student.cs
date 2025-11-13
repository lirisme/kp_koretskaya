using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DrivingSchool.Models
{
    [Serializable]
    public class Student
    {
        public int Id { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string BirthPlace { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Citizenship { get; set; } = string.Empty;
        public int GroupId { get; set; }
        public int ContractId { get; set; }
        public int VehicleCategoryId { get; set; }

        [XmlIgnore]
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        [XmlIgnore]
        public string FullNameWithPhone => $"{FullName} | 📞 {Phone} | ID: {Id}";

        [XmlIgnore]
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Year;
                if (BirthDate.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        [XmlIgnore]
        public string CategoryCode { get; set; } = string.Empty; 
    }

    [Serializable]
    [XmlRoot("Students")]
    public class StudentCollection
    {
        [XmlElement("Student")]
        public List<Student> Students { get; set; } = new List<Student>();
    }
}