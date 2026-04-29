namespace Project.Core.Application.Meta.Commands
{
    public class RefillStaminaCommand
    {
        public string Source { get; }
        public int HardCurrencyCost { get; }

        public RefillStaminaCommand(string source, int hardCurrencyCost)
        {
            Source = source;
            HardCurrencyCost = hardCurrencyCost;
        }
    }
}
