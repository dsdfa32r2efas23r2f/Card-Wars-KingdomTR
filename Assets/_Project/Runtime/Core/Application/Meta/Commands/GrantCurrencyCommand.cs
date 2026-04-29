using Project.Core.Domain.Meta.ValueObjects;

namespace Project.Core.Application.Meta.Commands
{
    public class GrantCurrencyCommand
    {
        public string Source { get; }
        public CurrencyAmounts Amounts { get; }

        public GrantCurrencyCommand(string source, CurrencyAmounts amounts)
        {
            Source = source;
            Amounts = amounts;
        }
    }
}
