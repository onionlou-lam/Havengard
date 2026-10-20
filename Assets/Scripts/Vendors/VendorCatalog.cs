using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Vendors
{
    /// <summary>Base for anything a vendor can sell or offer. Subclass per vendor type.</summary>
    public abstract class VendorCatalog : ScriptableObject
    {
        public abstract IList<IVendorOffer> BuildOffers(VendorContext context);
    }
}