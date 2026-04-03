using UnityEngine;
using System.Collections.Generic;

namespace Havengard.Items
{
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum ItemType
    {
        Passive,        // Stat bonuses
        AbilityMod,     // Modifies abilities
        Consumable      // One-time use
    }

    [CreateAssetMenu(menuName = "Havengard/Items/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        public string itemName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;
        public ItemRarity rarity = ItemRarity.Common;
        public ItemType itemType = ItemType.Passive;

        [Header("Stacking")]
        [Tooltip("Maximum level this item can reach")]
        public int maxLevel = 5;
        [Tooltip("Does collecting the same item auto-upgrade?")]
        public bool autoStackOnPickup = true;

        [Header("Economy")]
        public int disenchantValue = 10;
        [Tooltip("Drop weight (higher = more common)")]
        public float dropWeight = 1f;

        [Header("Effects")]
        [Tooltip("Effects applied per level")]
        public List<ItemEffect> effects;

        [Header("Visual")]
        public Sprite pickupSprite;  // ADD THIS - the in-game sprite without background
        public Color rarityColor = Color.white;
        public GameObject pickupVFX;
        public AudioClip pickupSFX;

        /// <summary>
        /// Get total effect value at a given level
        /// </summary>
        public float GetEffectValue(System.Type effectType, int level)
        {
            float total = 0f;
            foreach (var effect in effects)
            {
                if (effect.GetType() == effectType)
                {
                    total += effect.GetValue(level);
                }
            }
            return total;
        }

        /// <summary>
        /// Get description with level-scaled values
        /// </summary>
        public string GetScaledDescription(int level)
        {
            string desc = description;
            foreach (var effect in effects)
            {
                desc = effect.FormatDescription(desc, level);
            }
            return desc.Replace("{level}", level.ToString());
        }
    }
}