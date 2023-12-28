using Footballers.Data.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Footballers.DataProcessor.ImportDto
{
    [XmlType("Footballer")]
    public class ExportCoachesFootballersDTO
    {
        [Required]
        [MaxLength(40)]
        [MinLength(2)]
        [XmlElement("Name")]
        public string Name { get; set; }
        [Required]
        [XmlElement("ContractStartDate")]
        public string ContractStartDate { get; set; }
        [Required]
        [XmlElement("ContractEndDate")]
        public string ContractEndDate { get; set; }
        [Required]
        public int PositionType { get; set; }
        [Required]
        public int BestSkillType { get; set; }
    }
}