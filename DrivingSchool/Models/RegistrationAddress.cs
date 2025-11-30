using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DrivingSchool.Models
{
    [Serializable]
    public class StudentRegistrationAddress
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string Region { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string House { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string Apartment { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;

        [XmlIgnore]
        public string FullAddress
        {
            get
            {
                var parts = new List<string>();

                if (!string.IsNullOrWhiteSpace(PostalCode))
                    parts.Add(PostalCode);

                if (!string.IsNullOrWhiteSpace(Region))
                    parts.Add(Region);

                if (!string.IsNullOrWhiteSpace(City))
                    parts.Add($"г. {City}");

                if (!string.IsNullOrWhiteSpace(Street))
                    parts.Add($"ул. {Street}");

                if (!string.IsNullOrWhiteSpace(House))
                    parts.Add($"д. {House}");

                if (!string.IsNullOrWhiteSpace(Building))
                    parts.Add($"корп. {Building}");

                if (!string.IsNullOrWhiteSpace(Apartment))
                    parts.Add($"кв. {Apartment}");

                return string.Join(", ", parts);
            }
        }

        [XmlIgnore]
        public string StudentName { get; set; } = string.Empty;
    }

    [Serializable]
    [XmlRoot("StudentRegistrationAddresses")]
    public class StudentRegistrationAddressCollection
    {
        [XmlElement("Address")]
        public List<StudentRegistrationAddress> Addresses { get; set; } = new List<StudentRegistrationAddress>();
    }
}