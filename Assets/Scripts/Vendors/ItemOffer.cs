using Havengard.Items;
using UnityEngine;

namespace Havengard.Vendors
{
    /// <summary>Buy an item (or stack onto one the player already owns).</summary>
    public class ItemOffer : IVendorOffer
    {
        private readonly ItemData item;
        private readonly ItemInventory inventory;

        public string DisplayName { get { return item.itemName; } }
        public string Description { get { return item.description; } }
        public Sprite Icon { get { return item.icon; } }
        public Price Price { get; private set; }

        public ItemOffer(ItemData item, Price price, ItemInventory inventory)
        {
            this.item = item;
            this.inventory = inventory;
            Price = price;
        }

        public bool CanPurchase(out string failReason)
        {
            failReason = null;
            if (inventory == null) { failReason = "No inventory available."; return false; }

            ItemInventory.ItemSlot slot = inventory.Items.Find(s => s.itemData == item);
            if (slot != null)
            {
                if (!item.autoStackOnPickup) { failReason = "Already owned."; return false; }
                if (slot.currentLevel >= item.maxLevel) { failReason = "Max level."; return false; }
                return true;
            }

            if (inventory.IsFull) { failReason = "Inventory full."; return false; }
            return true;
        }

        public bool Grant()
        {
            return inventory != null && inventory.AddItem(item);
        }
    }
}