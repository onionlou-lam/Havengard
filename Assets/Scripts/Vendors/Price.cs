using System;

namespace Havengard.Vendors
{
    public enum CurrencyType
    {
        Gold,
        Celestium
    }

    [Serializable]
    public struct Price
    {
        public CurrencyType currency;
        public int amount;

        public Price(CurrencyType currency, int amount)
        {
            this.currency = currency;
            this.amount = amount;
        }

        public override string ToString()
        {
            return amount + " " + currency;
        }
    }
}