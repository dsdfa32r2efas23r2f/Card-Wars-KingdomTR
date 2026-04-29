using Project.Core.Application.Meta.Commands;
using Project.Core.Domain.Meta.Entities;

namespace Project.Core.Application.Meta.UseCases
{
    public class ApplySpendCurrencyService
    {
        public SpendResult Apply(MetaWalletState current, SpendCurrencyCommand command)
        {
            if (current == null) current = MetaWalletState.Empty;
            if (command == null) return SpendResult.Ok(current);
            if (!current.CanAfford(command.Amounts))
                return SpendResult.Fail(current, "Insufficient funds");
            return SpendResult.Ok(current.Spend(command.Amounts));
        }
    }

    public class SpendResult
    {
        public bool Succeeded { get; }
        public MetaWalletState NewState { get; }
        public string FailureReason { get; }

        private SpendResult(bool succeeded, MetaWalletState state, string failureReason = null)
        {
            Succeeded = succeeded;
            NewState = state;
            FailureReason = failureReason;
        }

        public static SpendResult Ok(MetaWalletState state) => new SpendResult(true, state);
        public static SpendResult Fail(MetaWalletState state, string reason) => new SpendResult(false, state, reason);
    }
}
