using UnityEngine;
using UnityEngine.UI;

namespace Havengard.UI.Shop
{
    /// <summary>
    /// Enchanter shop UI - handles enchantment purchases and upgrades.
    /// </summary>
    public class EnchanterShopUI : BaseShopUI
    {
        [Header("Enchanter Specific")]
        [SerializeField]
        private Transform enchantmentListContainer;

        [SerializeField]
        private GameObject enchantmentItemPrefab;

        // Add your enchantment system integration here
        // Example: List of available enchantments, upgrade slots, etc.

        protected override void OnEnable()
        {
            base.OnEnable();
            PopulateEnchantments();
        }

        private void PopulateEnchantments()
        {
            // TODO: Populate available enchantments
            // This will integrate with your item/enchantment system
        }

        protected override void OnPurchaseSuccess(int cost)
        {
            base.OnPurchaseSuccess(cost);
            // Add enchantment to player's inventory or apply to item
        }

        protected override void OnPurchaseFailed(int cost)
        {
            base.OnPurchaseFailed(cost);
            // Show "Not enough gold" feedback
            Debug.Log($"Need {cost} gold for this enchantment!");
        }
    }
}