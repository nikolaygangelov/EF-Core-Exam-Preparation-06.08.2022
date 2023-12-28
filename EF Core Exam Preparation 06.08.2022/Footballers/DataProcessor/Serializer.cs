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
            XmlSerializer serializer = new XmlSerializer(typeof(ExportCoachesDTO2[]), new XmlRootAttribute("Coaches"));

            StringBuilder sb = new StringBuilder();

            using var writer = new StringWriter(sb);

            var xns = new XmlSerializerNamespaces();
            xns.Add(string.Empty, string.Empty);

            var coachesAndFootballers = context.Coaches
                .Where(c => c.Footballers.Any())
                .Select(c => new ExportCoachesDTO2
                {
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

            serializer.Serialize(writer, coachesAndFootballers, xns);
            writer.Close();

            return sb.ToString();
        }

        public static string ExportTeamsWithMostFootballers(FootballersContext context, DateTime date)
        {
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
                              ContractStartDate = tf.Footballer.ContractStartDate.ToString("d", CultureInfo.InvariantCulture), // !!!!!!!!!!
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

            return JsonConvert.SerializeObject(teamsAndFootballers, Formatting.Indented);
        }
    }
}
