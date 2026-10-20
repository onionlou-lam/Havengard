namespace Havengard.Vendors
{
    public enum PurchaseResult
    {
        Success,
        Unavailable,
        InsufficientFunds,
        Failed
    }

    public class VendorTransactionService
    {
        private readonly ICurrencyService currency;

        public VendorTransactionService(ICurrencyService currency)
        {
            this.currency = currency;
        }

        public PurchaseResult TryPurchase(IVendorOffer offer, out string failReason)
        {
            if (!offer.CanPurchase(out failReason))
                return PurchaseResult.Unavailable;

            if (!currency.TrySpend(offer.Price))
            {
                failReason = "Not enough " + offer.Price.currency + ".";
                return PurchaseResult.InsufficientFunds;
            }

            if (!offer.Grant())
            {
                currency.Refund(offer.Price);
                failReason = "Purchase failed.";
                return PurchaseResult.Failed;
            }

            failReason = null;
            return PurchaseResult.Success;
        }
    }
}