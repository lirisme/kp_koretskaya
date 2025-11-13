using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DrivingSchool.Models
{
    [Serializable]
    public class VehicleCategory
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        [XmlIgnore]
        public string DisplayText => $"{Code} - {FullName}";
    }

    [Serializable]
    [XmlRoot("VehicleCategories")]
    public class VehicleCategoryCollection
    {
        [XmlElement("VehicleCategory")]
        public List<VehicleCategory> Categories { get; set; } = new List<VehicleCategory>();
    }
}