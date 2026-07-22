using UnityEngine;
using UnityEngine.UI;

namespace Havengard.UI.Shop
{
    /// <summary>
    /// Blacksmith shop UI - handles weapon/armor purchases and upgrades.
    /// </summary>
    public class BlacksmithShopUI : BaseShopUI
    {
        [Header("Blacksmith Specific")]
        [SerializeField]
        private Transform itemListContainer;

        [SerializeField]
        private GameObject itemPrefab;

        // Add your equipment system integration here
        // Example: List of weapons, armor, upgrade options

        protected override void OnEnable()
        {
            base.OnEnable();
            PopulateItems();
        }

        private void PopulateItems()
        {
            // TODO: Populate available weapons/armor
            // This will integrate with your item system
        }

        protected override void OnPurchaseSuccess(int cost)
        {
            base.OnPurchaseSuccess(cost);
            // Add item to player's inventory
        }

        protected override void OnPurchaseFailed(int cost)
        {
            base.OnPurchaseFailed(cost);
            // Show "Not enough gold" feedback
            Debug.Log($"Need {cost} gold for this item!");
        }
    }
}