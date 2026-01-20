using UnityEngine;
using System.Collections.Generic;

namespace Havengard.Items
{
    [CreateAssetMenu(menuName = "Havengard/Items/Drop Table")]
    public class ItemDropTable : ScriptableObject
    {
        [System.Serializable]
        public class DropEntry
        {
            public ItemData item;
            [Tooltip("Higher weight = more likely to drop")]
            public float weight = 1f;
            [Tooltip("Minimum wave/level required")]
            public int minLevel = 1;
        }

        [Header("Drop Pool")]
        public List<DropEntry> dropEntries = new List<DropEntry>();
        
        [Header("Rarity Weights")]
        [Tooltip("Adjust drop rates by rarity")]
        public float commonWeight = 1f;
        public float uncommonWeight = 0.7f;
        public float rareWeight = 0.4f;
        public float epicWeight = 0.2f;
        public float legendaryWeight = 0.05f;

        [Header("Drop Chance")]
        [Range(0f, 1f)]
        [Tooltip("Chance that any item drops at all")]
        public float dropChance = 0.5f;

        /// <summary>
        /// Roll for an item drop (may return null if no drop)
        /// </summary>
        public ItemData RollForItem(int currentLevel = 1)
        {
            // Check if anything drops at all
            if (Random.value > dropChance)
            {
                return null;
            }

            return GetRandomItem(currentLevel);
        }

        /// <summary>
        /// Get a random item based on weights
        /// </summary>
        public ItemData GetRandomItem(int currentLevel = 1)
        {
            if (dropEntries == null || dropEntries.Count == 0)
            {
                Debug.LogWarning("[ItemDropTable] No drop entries defined!");
                return null;
            }

            // Filter by level requirement
            List<DropEntry> validEntries = dropEntries.FindAll(entry => 
                entry.item != null && entry.minLevel <= currentLevel);

            if (validEntries.Count == 0)
            {
                Debug.LogWarning($"[ItemDropTable] No valid entries for level {currentLevel}");
                return null;
            }

            // Calculate total weight
            float totalWeight = 0f;
            foreach (var entry in validEntries)
            {
                float rarityMultiplier = GetRarityMultiplier(entry.item.rarity);
                totalWeight += entry.weight * rarityMultiplier;
            }

            // Random selection
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var entry in validEntries)
            {
                float rarityMultiplier = GetRarityMultiplier(entry.item.rarity);
                currentWeight += entry.weight * rarityMultiplier;

                if (randomValue <= currentWeight)
                {
                    return entry.item;
                }
            }

            // Fallback (should never reach here)
            return validEntries[0].item;
        }

        /// <summary>
        /// Get rarity weight multiplier
        /// </summary>
        private float GetRarityMultiplier(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => commonWeight,
                ItemRarity.Uncommon => uncommonWeight,
                ItemRarity.Rare => rareWeight,
                ItemRarity.Epic => epicWeight,
                ItemRarity.Legendary => legendaryWeight,
                _ => 1f
            };
        }

        /// <summary>
        /// Get all possible items at a level
        /// </summary>
        public List<ItemData> GetAvailableItems(int currentLevel)
        {
            List<ItemData> available = new List<ItemData>();
            foreach (var entry in dropEntries)
            {
                if (entry.item != null && entry.minLevel <= currentLevel)
                {
                    available.Add(entry.item);
                }
            }
            return available;
        }
    }
}