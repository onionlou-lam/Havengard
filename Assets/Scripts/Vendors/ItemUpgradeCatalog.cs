using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Vendors
{
    /// <summary>Blacksmith/enchanter style vendor. Lists the player's upgradable items. Cost = baseCost × current level.</summary>
    [CreateAssetMenu(menuName = "Havengard/Vendors/Item Upgrade Catalog")]
    public class ItemUpgradeCatalog : VendorCatalog
    {
        [SerializeField] private CurrencyType currency = CurrencyType.Celestium;
        [SerializeField] private int baseCost = 50;

        public override IList<IVendorOffer> BuildOffers(VendorContext context)
        {
            var offers = new List<IVendorOffer>();
            if (context.PlayerInventory == null) return offers;

            foreach (var slot in context.PlayerInventory.Items)
            {
                if (slot.itemData == null || slot.currentLevel >= slot.itemData.maxLevel) continue;

                var price = new Price(currency, baseCost * slot.currentLevel);
                offers.Add(new ItemUpgradeOffer(slot.itemData, slot.currentLevel, price, context.PlayerInventory));
            }
            return offers;
        }
    }
}