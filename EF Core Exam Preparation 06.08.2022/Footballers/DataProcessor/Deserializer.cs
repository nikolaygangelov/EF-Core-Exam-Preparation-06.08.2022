namespace Footballers.DataProcessor
{
    using Footballers.Data;
    using Footballers.Data.Models;
    using Footballers.Data.Models.Enums;
    using Footballers.DataProcessor.ImportDto;
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedCoach
            = "Successfully imported coach - {0} with {1} footballers.";

        private const string SuccessfullyImportedTeam
            = "Successfully imported team - {0} with {1} footballers.";

        public static string ImportCoaches(FootballersContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ExportCoachesDTO[]), new XmlRootAttribute("Coaches"));
            using StringReader inputReader = new StringReader(xmlString);
            var coachesArrayDTOs = (ExportCoachesDTO[])serializer.Deserialize(inputReader);

            StringBuilder sb = new StringBuilder();
            List<Coach> coachesXML = new List<Coach>();

            foreach (ExportCoachesDTO coachDTO in coachesArrayDTOs)
            {
                Coach coachToAdd = new Coach
                {
                    Name = coachDTO.Name,
                    Nationality = coachDTO.Nationality
                };

                if (!IsValid(coachDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                foreach (var footballer in coachDTO.Footballers)
                {
                    if (!IsValid(footballer))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (DateTime.ParseExact(footballer.ContractStartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture) > 
                        DateTime.ParseExact(footballer.ContractEndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    coachToAdd.Footballers.Add(new Footballer()
                    {
                        Name = footballer.Name,
                        ContractStartDate = DateTime.ParseExact(footballer.ContractStartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        ContractEndDate = DateTime.ParseExact(footballer.ContractEndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        BestSkillType = (BestSkillType)footballer.BestSkillType,
                        PositionType = (PositionType)footballer.PositionType
                    });
                }

                coachesXML.Add(coachToAdd);
                sb.AppendLine(string.Format(SuccessfullyImportedCoach, coachToAdd.Name,
                    coachToAdd.Footballers.Count));
            }

            context.Coaches.AddRange(coachesXML);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportTeams(FootballersContext context, string jsonString)
        {
            var teamsArray = JsonConvert.DeserializeObject<ImportTeamsDTO[]>(jsonString);

            StringBuilder sb = new StringBuilder();
            List<Team> teamList = new List<Team>();

            var existingFootballerIds = context.Footballers
                .Select(f => f.Id)
                .ToArray();

            foreach (ImportTeamsDTO teamDTO in teamsArray)
            {

                if (!IsValid(teamDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (teamDTO.Trophies == "0")
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Team teamToAdd = new Team()
                {
                    Name = teamDTO.Name,
                    Nationality = teamDTO.Nationality,
                    Trophies = int.Parse(teamDTO.Trophies)
                };



                foreach (int footballerId in teamDTO.Footballers.Distinct())
                {
                    if (!existingFootballerIds.Contains(footballerId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    teamToAdd.TeamsFootballers.Add(new TeamFootballer()
                    {
                        Team = teamToAdd,
                        FootballerId = footballerId
                    });

                }

                teamList.Add(teamToAdd);
                sb.AppendLine(string.Format(SuccessfullyImportedTeam, teamToAdd.Name, teamToAdd.TeamsFootballers.Count));
            }

            context.AddRange(teamList);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}
