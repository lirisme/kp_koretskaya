using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DrivingSchool.Models
{
    [Serializable]
    public class GeneratedDocument
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public int StudentId { get; set; }
        public DateTime CreationDate { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }

    [Serializable]
    [XmlRoot("GeneratedDocuments")]
    public class GeneratedDocumentCollection
    {
        [XmlElement("GeneratedDocument")]
        public List<GeneratedDocument> Documents { get; set; } = new List<GeneratedDocument>();
    }
}