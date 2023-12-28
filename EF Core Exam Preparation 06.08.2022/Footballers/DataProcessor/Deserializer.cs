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
            //using Data Transfer Object Class to map it with coaches
            var serializer = new XmlSerializer(typeof(ExportCoachesDTO[]), new XmlRootAttribute("Coaches"));

            //Deserialize method needs TextReader object to convert/map
            using StringReader inputReader = new StringReader(xmlString);
            var coachesArrayDTOs = (ExportCoachesDTO[])serializer.Deserialize(inputReader);

            //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();

            //creating List where all valid coaches can be kept
            List<Coach> coachesXML = new List<Coach>();

            foreach (ExportCoachesDTO coachDTO in coachesArrayDTOs)
            {
                //validating info for coach from data
                if (!IsValid(coachDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //creating a valid coach
                Coach coachToAdd = new Coach
                {
                    //using identical properties in order to map successfully
                    Name = coachDTO.Name,
                    Nationality = coachDTO.Nationality
                };

                foreach (var footballer in coachDTO.Footballers)
                {
                    //validating info for footballer from data
                    if (!IsValid(footballer))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //using InvariantCulture property for culture-independent format
                    if (DateTime.ParseExact(footballer.ContractStartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture) > 
                        DateTime.ParseExact(footballer.ContractEndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //adding valid footballer
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

            //actual importing info from data
            context.SaveChanges();

            //using TrimEnd() to get rid of white spaces
            return sb.ToString().TrimEnd();
        }

        public static string ImportTeams(FootballersContext context, string jsonString)
        {
            //using Data Transfer Object Class to map it with teams
            var teamsArray = JsonConvert.DeserializeObject<ImportTeamsDTO[]>(jsonString);

            //using StringBuilder to gather all info in one string
            StringBuilder sb = new StringBuilder();

            //creating List where all valid teams can be kept
            List<Team> teamList = new List<Team>();

            //taking only unique footballers
            var existingFootballerIds = context.Footballers
                .Select(f => f.Id)
                .ToArray();

            foreach (ImportTeamsDTO teamDTO in teamsArray)
            {
                //validating info for team from data
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

                //creating a valid team
                Team teamToAdd = new Team()
                {
                    // using identical properties in order to map successfully
                    Name = teamDTO.Name,
                    Nationality = teamDTO.Nationality,
                    Trophies = int.Parse(teamDTO.Trophies)
                };



                foreach (int footballerId in teamDTO.Footballers.Distinct())
                {
                    //validating only unique footballers
                    if (!existingFootballerIds.Contains(footballerId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //adding valid TeamFootballer
                    teamToAdd.TeamsFootballers.Add(new TeamFootballer()
                    {
                        //using identical properties in order to map successfully
                        Team = teamToAdd,
                        FootballerId = footballerId
                    });

                }

                teamList.Add(teamToAdd);
                sb.AppendLine(string.Format(SuccessfullyImportedTeam, teamToAdd.Name, teamToAdd.TeamsFootballers.Count));
            }

            context.AddRange(teamList);

            //actual importing info from data
            context.SaveChanges();

            //using TrimEnd() to get rid of white spaces
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
