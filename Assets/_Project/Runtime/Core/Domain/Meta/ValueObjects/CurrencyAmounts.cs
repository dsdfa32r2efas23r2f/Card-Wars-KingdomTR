namespace Project.Core.Domain.Meta.ValueObjects
{
    public class CurrencyAmounts
    {
        public static CurrencyAmounts Zero => new CurrencyAmounts(0, 0, 0, 0, 0);

        public int PaidHardCurrency { get; }
        public int FreeHardCurrency { get; }
        public int PvpCurrency { get; }
        public int SoftCurrency { get; }
        public int CustomizationCurrency { get; }
        public int HardCurrency => PaidHardCurrency + FreeHardCurrency;

        public CurrencyAmounts(int paidHardCurrency, int freeHardCurrency, int pvpCurrency, int softCurrency, int customizationCurrency)
        {
            PaidHardCurrency = paidHardCurrency;
            FreeHardCurrency = freeHardCurrency;
            PvpCurrency = pvpCurrency;
            SoftCurrency = softCurrency;
            CustomizationCurrency = customizationCurrency;
        }

        public CurrencyAmounts Add(CurrencyAmounts other)
        {
            return new CurrencyAmounts(
                PaidHardCurrency + other.PaidHardCurrency,
                FreeHardCurrency + other.FreeHardCurrency,
                PvpCurrency + other.PvpCurrency,
                SoftCurrency + other.SoftCurrency,
                CustomizationCurrency + other.CustomizationCurrency);
        }
    }
}
