using Havengard.Resources;

namespace Havengard.Vendors
{
    public interface ICurrencyService
    {
        bool CanAfford(Price price);
        bool TrySpend(Price price);
        void Refund(Price price);
    }

    /// <summary>Adapter over the existing GoldSystem and CelestiumSystem singletons.</summary>
    public class CurrencyService : ICurrencyService
    {
        public bool CanAfford(Price price)
        {
            switch (price.currency)
            {
                case CurrencyType.Gold:
                    return GoldSystem.Instance != null && GoldSystem.Instance.CurrentGold >= price.amount;
                case CurrencyType.Celestium:
                    return CelestiumSystem.Instance != null && CelestiumSystem.Instance.CurrentCelestium >= price.amount;
                default:
                    return false;
            }
        }

        public bool TrySpend(Price price)
        {
            switch (price.currency)
            {
                case CurrencyType.Gold:
                    return GoldSystem.Instance != null && GoldSystem.Instance.SpendGold(price.amount);
                case CurrencyType.Celestium:
                    return CelestiumSystem.Instance != null && CelestiumSystem.Instance.SpendCelestium(price.amount);
                default:
                    return false;
            }
        }

        public void Refund(Price price)
        {
            switch (price.currency)
            {
                case CurrencyType.Gold:
                    if (GoldSystem.Instance != null) GoldSystem.Instance.AddGold(price.amount);
                    break;
                case CurrencyType.Celestium:
                    if (CelestiumSystem.Instance != null) CelestiumSystem.Instance.AddCelestium(price.amount);
                    break;
            }
        }
    }
}