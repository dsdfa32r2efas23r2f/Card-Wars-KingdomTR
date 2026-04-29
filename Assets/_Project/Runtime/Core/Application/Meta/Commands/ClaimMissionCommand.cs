using Project.Core.Domain.Meta.ValueObjects;

namespace Project.Core.Application.Meta.Commands
{
    public class ClaimMissionCommand
    {
        public string MissionId { get; }
        public CurrencyAmounts Rewards { get; }

        public ClaimMissionCommand(string missionId, CurrencyAmounts rewards)
        {
            MissionId = missionId;
            Rewards = rewards;
        }
    }
}
