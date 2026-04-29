using System;
using Project.Core.Domain.Meta.ValueObjects;

namespace Project.Core.Domain.Meta.Entities
{
    public class MetaWalletState
    {
        public static MetaWalletState Empty => new MetaWalletState(CurrencyAmounts.Zero);

        private readonly CurrencyAmounts _amounts;

        public int PaidHardCurrency => _amounts.PaidHardCurrency;
        public int FreeHardCurrency => _amounts.FreeHardCurrency;
        public int PvpCurrency => _amounts.PvpCurrency;
        public int SoftCurrency => _amounts.SoftCurrency;
        public int CustomizationCurrency => _amounts.CustomizationCurrency;
        public int HardCurrency => _amounts.HardCurrency;

        public MetaWalletState(CurrencyAmounts amounts)
        {
            _amounts = amounts ?? CurrencyAmounts.Zero;
        }

        public MetaWalletState Grant(CurrencyAmounts delta)
        {
            return new MetaWalletState(_amounts.Add(delta));
        }

        public bool CanAfford(SpendAmounts cost)
        {
            return _amounts.HardCurrency >= cost.HardCurrency
                && _amounts.PvpCurrency >= cost.PvpCurrency
                && _amounts.SoftCurrency >= cost.SoftCurrency
                && _amounts.CustomizationCurrency >= cost.CustomizationCurrency;
        }

        // Deducts free hard currency first, then paid.
        public MetaWalletState Spend(SpendAmounts cost)
        {
            int freeAfter = _amounts.FreeHardCurrency;
            int paidAfter = _amounts.PaidHardCurrency;
            int toDeduct = cost.HardCurrency;
            if (toDeduct > 0)
            {
                int freeDeducted = Math.Min(freeAfter, toDeduct);
                freeAfter -= freeDeducted;
                paidAfter -= toDeduct - freeDeducted;
            }
            return new MetaWalletState(new CurrencyAmounts(
                paidAfter,
                freeAfter,
                _amounts.PvpCurrency - cost.PvpCurrency,
                _amounts.SoftCurrency - cost.SoftCurrency,
                _amounts.CustomizationCurrency - cost.CustomizationCurrency));
        }
    }
}
