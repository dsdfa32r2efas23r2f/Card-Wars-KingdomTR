using Project.Core.Domain.Meta.ValueObjects;

namespace Project.Core.Application.Meta.Commands
{
    public class SpendCurrencyCommand
    {
        public string Source { get; }
        public SpendAmounts Amounts { get; }

        public SpendCurrencyCommand(string source, SpendAmounts amounts)
        {
            Source = source;
            Amounts = amounts;
        }
    }
}
