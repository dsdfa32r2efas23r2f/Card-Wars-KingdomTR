namespace Project.Core.Domain.Meta.ValueObjects
{
    // HardCurrency is the total to spend (free deducted first, then paid).
    // Paid/free split is intentionally absent — that's the wallet's internal concern.
    public class SpendAmounts
    {
        public static SpendAmounts Zero => new SpendAmounts(0, 0, 0, 0);

        public int HardCurrency { get; }
        public int PvpCurrency { get; }
        public int SoftCurrency { get; }
        public int CustomizationCurrency { get; }

        public SpendAmounts(int hardCurrency, int pvpCurrency, int softCurrency, int customizationCurrency)
        {
            HardCurrency = hardCurrency;
            PvpCurrency = pvpCurrency;
            SoftCurrency = softCurrency;
            CustomizationCurrency = customizationCurrency;
        }
    }
}
