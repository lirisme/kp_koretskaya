using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DrivingSchool.Models
{
        [Serializable]
        public class Employee
        {
            public int Id { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Position { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public DateTime HireDate { get; set; }

            [XmlIgnore]
            public string Experience
            {
                get
                {
                    return CalculateExperience(HireDate);
                }
            }

            private string CalculateExperience(DateTime hireDate)
            {
                if (hireDate == DateTime.MinValue)
                    return "0 мес.";

                var today = DateTime.Today;
                var years = today.Year - hireDate.Year;
                var months = today.Month - hireDate.Month;

                if (months < 0)
                {
                    years--;
                    months += 12;
                }

                if (today.Day < hireDate.Day)
                {
                    months--;
                    if (months < 0)
                    {
                        years--;
                        months += 12;
                    }
                }

                if (years > 0)
                    return $"{years} г. {months} мес.";
                else if (months > 0)
                    return $"{months} мес.";
                else
                    return "< 1 мес.";
            }
        }
    [Serializable]
    [XmlRoot("Employees")]
    public class EmployeeCollection
    {
        [XmlElement("Employee")]
        public List<Employee> Employees { get; set; } = new List<Employee>();
    }
}
    
