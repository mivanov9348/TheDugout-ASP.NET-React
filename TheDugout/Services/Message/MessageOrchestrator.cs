using System.Text.RegularExpressions;
using TheDugout.Models;
using TheDugout.Models.Messages;

namespace TheDugout.Services.Message
{
    public class MessageOrchestrator : IMessageOrchestrator
    {
        private readonly IMessageService _messageService;

        public MessageOrchestrator(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public async Task<Models.Messages.Message> SendMessageAsync(
            MessageCategory category,
            int gameSaveId,
            object contextModel)
        {
            var placeholders = BuildPlaceholders(category, contextModel);

            return await _messageService.CreateAndSaveMessageAsync(
                category,
                placeholders,
                gameSaveId
            );
        }

        private Dictionary<string, string> BuildPlaceholders(
    MessageCategory category,
    object contextModel)
        {
            var dict = new Dictionary<string, string>();

            switch (category)
            {
                case MessageCategory.Welcome:
                    // contextModel очаква (User, Team)
                    var tuple = ((Models.Game.User user, Models.Teams.Team team))contextModel;
                    dict["ManagerName"] = tuple.user.Username;
                    dict["ClubName"] = tuple.team.Name;
                    break;

                case MessageCategory.Transfer:
                    var transfer = (Models.Transfers.Transfer)contextModel;
                    dict["PlayerName"] = transfer.Player.FirstName + " " + transfer.Player.LastName;
                    dict["ClubName"] = transfer.ToTeam.Name; // твоят отбор, който купува
                    dict["Amount"] = transfer.Fee.ToString("N0");
                    break;


                //case MessageCategory.MatchResult:
                //    // contextModel очаква Match
                //    var match = (Models.Matches.Match)contextModel;
                //    dict["HomeTeam"] = match.HomeTeam.Name;
                //    dict["AwayTeam"] = match.AwayTeam.Name;
                //    dict["Score"] = $"{match.HomeGoals}-{match.AwayGoals}";
                //    dict["Competition"] = match.Competition.Name;
                //    break;

                //case MessageCategory.Board:
                //    // contextModel очаква BoardMessageContext (custom DTO)
                //    var boardCtx = (BoardMessageContext)contextModel;
                //    dict["BoardMember"] = boardCtx.MemberName;
                //    dict["Topic"] = boardCtx.Topic;
                //    break;

                //case MessageCategory.Fans:
                //    // contextModel очаква FansMessageContext
                //    var fansCtx = (FansMessageContext)contextModel;
                //    dict["FanGroup"] = fansCtx.GroupName;
                //    dict["Mood"] = fansCtx.Mood;
                //    break;

                //case MessageCategory.Media:
                //    // contextModel очаква MediaMessageContext
                //    var mediaCtx = (MediaMessageContext)contextModel;
                //    dict["Journalist"] = mediaCtx.JournalistName;
                //    dict["Headline"] = mediaCtx.Headline;
                //    break;

                //case MessageCategory.Injury:
                //    // contextModel очаква Injury
                //    var injury = (Models.Players.Injury)contextModel;
                //    dict["PlayerName"] = injury.Player.FullName;
                //    dict["InjuryType"] = injury.Type;
                //    dict["Duration"] = injury.EstimatedDuration.ToString();
                //    break;

                //case MessageCategory.Training:
                //    // contextModel очаква TrainingReport
                //    var training = (TrainingReport)contextModel;
                //    dict["PlayerName"] = training.Player.FullName;
                //    dict["Performance"] = training.PerformanceRating.ToString();
                //    break;

                //case MessageCategory.YouthAcademy:
                //    // contextModel очаква YouthPlayerPromotion
                //    var youth = (YouthPlayerPromotion)contextModel;
                //    dict["PlayerName"] = youth.Player.FullName;
                //    dict["Age"] = youth.Player.Age.ToString();
                //    dict["Potential"] = youth.Player.Potential.ToString();
                //    break;

                //case MessageCategory.Finance:
                //    // contextModel очаква FinanceReport
                //    var finance = (FinanceReport)contextModel;
                //    dict["Budget"] = finance.Budget.ToString("N0");
                //    dict["Income"] = finance.Income.ToString("N0");
                //    dict["Expenses"] = finance.Expenses.ToString("N0");
                //    break;

                //case MessageCategory.Milestone:
                //    // contextModel очаква Milestone
                //    var milestone = (Milestone)contextModel;
                //    dict["Description"] = milestone.Description;
                //    dict["Date"] = milestone.Date.ToShortDateString();
                //    break;

                //case MessageCategory.Scouting:
                //    // contextModel очаква ScoutingReport
                //    var scouting = (ScoutingReport)contextModel;
                //    dict["PlayerName"] = scouting.Player.FullName;
                //    dict["ScoutName"] = scouting.Scout.Name;
                //    dict["Summary"] = scouting.Summary;
                //    break;

                //case MessageCategory.Competition:
                //    // contextModel очаква CompetitionUpdate
                //    var comp = (CompetitionUpdate)contextModel;
                //    dict["CompetitionName"] = comp.Competition.Name;
                //    dict["Round"] = comp.RoundName;
                //    break;

                //case MessageCategory.General:
                //    // contextModel очаква GeneralMessageContext
                //    var general = (GeneralMessageContext)contextModel;
                //    dict["Title"] = general.Title;
                //    dict["Content"] = general.Content;
                //    break;

                default:
                    break;
            }

            return dict;
        }

    }
}
