

using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Footballers.DataProcessor.ExportDto
{
    [XmlType("Coach")]
    public class ExportCoachesDTO2
    {
        [XmlAttribute("FootballersCount")]
        public int FootballersCount { get; set; }

        [Required]
        [MaxLength(40)]
        [MinLength(2)]
        [XmlElement("CoachName")]
        public string CoachName { get; set; }

        [XmlArray("Footballers")]
        public ExportCoachesFootballersDTO2[] Footballers { get; set; }
    }
}
