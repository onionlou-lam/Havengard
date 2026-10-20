using Havengard.Items;
using UnityEngine;

namespace Havengard.Vendors
{
    /// <summary>Upgrade an item the player already owns by one level.</summary>
    public class ItemUpgradeOffer : IVendorOffer
    {
        private readonly ItemData item;
        private readonly int currentLevel;
        private readonly ItemInventory inventory;

        public string DisplayName { get { return item.itemName; } }
        public string Description { get { return "Lv." + currentLevel + " → " + (currentLevel + 1); } }
        public Sprite Icon { get { return item.icon; } }
        public Price Price { get; private set; }

        public ItemUpgradeOffer(ItemData item, int currentLevel, Price price, ItemInventory inventory)
        {
            this.item = item;
            this.currentLevel = currentLevel;
            this.inventory = inventory;
            Price = price;
        }

        public bool CanPurchase(out string failReason)
        {
            failReason = null;
            if (inventory == null) { failReason = "No inventory available."; return false; }

            ItemInventory.ItemSlot slot = inventory.Items.Find(s => s.itemData == item);
            if (slot == null) { failReason = "Item not owned."; return false; }
            if (slot.currentLevel >= item.maxLevel) { failReason = "Max level."; return false; }
            return true;
        }

        public bool Grant()
        {
            return inventory != null && inventory.TryUpgradeItem(item);
        }
    }
}