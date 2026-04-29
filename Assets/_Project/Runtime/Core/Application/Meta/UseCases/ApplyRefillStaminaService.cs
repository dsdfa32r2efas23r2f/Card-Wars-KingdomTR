using Project.Core.Application.Meta.Commands;
using Project.Core.Domain.Meta.Entities;
using Project.Core.Domain.Meta.ValueObjects;

namespace Project.Core.Application.Meta.UseCases
{
    public class ApplyRefillStaminaService
    {
        private readonly ApplySpendCurrencyService _spend = new ApplySpendCurrencyService();

        public SpendResult Apply(MetaWalletState current, RefillStaminaCommand command)
        {
            if (command == null) return SpendResult.Ok(current ?? MetaWalletState.Empty);
            var spendCommand = new SpendCurrencyCommand(
                command.Source,
                new SpendAmounts(command.HardCurrencyCost, 0, 0, 0));
            return _spend.Apply(current, spendCommand);
        }
    }
}
