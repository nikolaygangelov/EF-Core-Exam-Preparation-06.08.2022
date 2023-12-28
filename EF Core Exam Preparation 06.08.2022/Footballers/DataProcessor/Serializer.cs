namespace Footballers.DataProcessor
{
    using Data;
    using Footballers.Data.Models.Enums;
    using Footballers.DataProcessor.ExportDto;
    using Newtonsoft.Json;
    using System.Globalization;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportCoachesWithTheirFootballers(FootballersContext context)
        {
            //using Data Transfer Object Class to map it with coaches
            XmlSerializer serializer = new XmlSerializer(typeof(ExportCoachesDTO2[]), new XmlRootAttribute("Coaches"));

            //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();

            //"using" automatically closes opened connections
            using var writer = new StringWriter(sb);

            var xns = new XmlSerializerNamespaces();

            //one way to display empty namespace in resulted file
            xns.Add(string.Empty, string.Empty);

            var coachesAndFootballers = context.Coaches
                .Where(c => c.Footballers.Any())
                .Select(c => new ExportCoachesDTO2
                {
                    //using identical properties in order to map successfully
                    FootballersCount = c.Footballers.Count,
                    CoachName = c.Name,
                    Footballers = c.Footballers
                    .Select(f => new ExportCoachesFootballersDTO2
                    {
                        Name = f.Name,
                        Position = f.PositionType
                    })
                    .OrderBy(f => f.Name)
                    .ToArray()
                })
                .OrderByDescending(c => c.FootballersCount)
                .ThenBy(c => c.CoachName)
                .ToArray();

            //Serialize method needs file, TextReader object and namespace to convert/map
            serializer.Serialize(writer, coachesAndFootballers, xns);

            //explicitly closing connection in terms of reaching edge cases
            writer.Close();

            //using TrimEnd() to get rid of white spaces
            return sb.ToString().TrimEnd();
        }

        public static string ExportTeamsWithMostFootballers(FootballersContext context, DateTime date)
        {
            //turning needed info about teams into a collection using anonymous object
            //using less data
            var teamsAndFootballers = context.Teams
                .Where(t => t.TeamsFootballers.Any(f => f.Footballer.ContractStartDate >= date))
                .Select(t => new
                {
                      Name = t.Name,
                      Footballers = t.TeamsFootballers
                          .Where(f => f.Footballer.ContractStartDate >= date)
                          .OrderByDescending(f => f.Footballer.ContractEndDate)
                          .ThenBy(f => f.Footballer.Name)
                          .Select(tf => new
                           {
                              FootballerName = tf.Footballer.Name,
                              ContractStartDate = tf.Footballer.ContractStartDate.ToString("d", CultureInfo.InvariantCulture), //culture-independent format to reach needed format
                              ContractEndDate = tf.Footballer.ContractEndDate.ToString("d", CultureInfo.InvariantCulture),
                              BestSkillType = tf.Footballer.BestSkillType.ToString(),
                              PositionType = tf.Footballer.PositionType.ToString()
                          })
                          //.OrderByDescending(tf => tf.ContractEndDate)
                          //.ThenBy(tf => tf.FootballerName)
                          .ToArray()
                })
                .OrderByDescending(t => t.Footballers.Length)
                .ThenBy(t => t.Name)
                .Take(5)
                .ToArray();

            //Serialize method needs object to convert/map
	        //adding Formatting for better reading 
            return JsonConvert.SerializeObject(teamsAndFootballers, Formatting.Indented);
        }
    }
}
