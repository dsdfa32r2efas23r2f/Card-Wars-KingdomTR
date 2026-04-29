using Project.Core.Application.Meta.Commands;
using Project.Core.Domain.Meta.Entities;

namespace Project.Core.Application.Meta.UseCases
{
    public class ApplyClaimMissionService
    {
        private readonly ApplyGrantCurrencyService _grant = new ApplyGrantCurrencyService();

        public MetaWalletState Apply(MetaWalletState current, ClaimMissionCommand command)
        {
            if (command == null) return current ?? MetaWalletState.Empty;
            var grantCommand = new GrantCurrencyCommand(command.MissionId, command.Rewards);
            return _grant.Apply(current, grantCommand);
        }
    }
}
