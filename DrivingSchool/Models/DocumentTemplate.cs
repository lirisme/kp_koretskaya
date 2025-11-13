using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DrivingSchool.Models
{
    [Serializable]
    public class DocumentTemplate
    {
        public int Id { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string PlaceholdersJson { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [XmlIgnore]
        public string FileExtension => System.IO.Path.GetExtension(FilePath)?.ToLower() ?? "";

        [XmlIgnore]
        public bool IsWordDocument => FileExtension == ".docx" || FileExtension == ".doc";

        [XmlIgnore]
        public bool IsExcelDocument => FileExtension == ".xlsx" || FileExtension == ".xls";

        [XmlIgnore]
        public Dictionary<string, string> Placeholders
        {
            get
            {
                if (string.IsNullOrEmpty(PlaceholdersJson))
                    return new Dictionary<string, string>();

                try
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(PlaceholdersJson);
                }
                catch
                {
                    return new Dictionary<string, string>();
                }
            }
            set
            {
                PlaceholdersJson = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            }
        }
    }

    [Serializable]
    [XmlRoot("DocumentTemplates")]
    public class DocumentTemplateCollection
    {
        [XmlElement("Template")]
        public List<DocumentTemplate> Templates { get; set; } = new List<DocumentTemplate>();
    }
}