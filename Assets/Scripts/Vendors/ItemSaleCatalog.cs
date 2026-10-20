using System;
using System.Collections.Generic;
using Havengard.Items;
using UnityEngine;

namespace Havengard.Vendors
{
    [CreateAssetMenu(menuName = "Havengard/Vendors/Item Sale Catalog")]
    public class ItemSaleCatalog : VendorCatalog
    {
        [Serializable]
        public class Entry
        {
            public ItemData item;
            public CurrencyType currency = CurrencyType.Gold;
            public int price = 100;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public override IList<IVendorOffer> BuildOffers(VendorContext context)
        {
            var offers = new List<IVendorOffer>();
            foreach (Entry entry in entries)
            {
                if (entry == null || entry.item == null) continue;
                offers.Add(new ItemOffer(entry.item, new Price(entry.currency, entry.price), context.PlayerInventory));
            }
            return offers;
        }
    }
}