using Project.Core.Application.Meta.Commands;
using Project.Core.Domain.Meta.Entities;

namespace Project.Core.Application.Meta.UseCases
{
    public class ApplyGrantCurrencyService
    {
        public MetaWalletState Apply(MetaWalletState current, GrantCurrencyCommand command)
        {
            if (current == null) current = MetaWalletState.Empty;
            if (command == null) return current;
            return current.Grant(command.Amounts);
        }
    }
}
