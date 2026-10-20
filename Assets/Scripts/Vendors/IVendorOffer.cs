using UnityEngine;

namespace Havengard.Vendors
{
    public interface IVendorOffer
    {
        string DisplayName { get; }
        string Description { get; }
        Sprite Icon { get; }
        Price Price { get; }

        /// <summary>Whether the offer can be fulfilled right now (ignores funds).</summary>
        bool CanPurchase(out string failReason);

        /// <summary>Gives the player what they bought. Payment is handled by the transaction service.</summary>
        bool Grant();
    }
}