using Havengard.Core.Heroes;
using Havengard.Expeditions;
using Havengard.Items;
using Havengard.Resources;
using UnityEngine;

namespace Havengard.Quests
{
    public enum QuestRewardMode { Fixed, Choice }

    public enum QuestRewardType { Gold, Celestium, Experience, Item }

    [System.Serializable]
    public class QuestReward
    {
        public QuestRewardType type;

        [Tooltip("Currency or EXP amount. For items, the level granted (an item the player already owns is levelled up).")]
        public int amount = 1;

        public ItemData item;

        public string GetLabel()
        {
            switch (type)
            {
                case QuestRewardType.Gold: return "+" + amount + " Gold";
                case QuestRewardType.Celestium: return "+" + amount + " Celestium";
                case QuestRewardType.Experience: return "+" + amount + " EXP";
                case QuestRewardType.Item:
                    if (item == null) return "Item";
                    return amount > 1 ? item.itemName + " (Lv." + amount + ")" : item.itemName;
                default: return string.Empty;
            }
        }

        public Sprite GetIcon(RewardIconLibrary icons)
        {
            switch (type)
            {
                case QuestRewardType.Gold: return icons != null ? icons.goldIcon : null;
                case QuestRewardType.Celestium: return icons != null ? icons.celestiumIcon : null;
                case QuestRewardType.Experience: return icons != null ? icons.experienceIcon : null;
                case QuestRewardType.Item: return item != null ? item.icon : null;
                default: return null;
            }
        }
    }

    /// <summary>Checks and applies quest rewards to the player's systems.</summary>
    public static class QuestRewardGrant
    {
        public static bool CanGrant(QuestReward reward, out string reason)
        {
            reason = null;

            switch (reward.type)
            {
                case QuestRewardType.Gold:
                    if (GoldSystem.Instance != null) return true;
                    reason = "Gold system unavailable.";
                    return false;

                case QuestRewardType.Celestium:
                    if (CelestiumSystem.Instance != null) return true;
                    reason = "Celestium system unavailable.";
                    return false;

                case QuestRewardType.Experience:
                    if (FindPlayerHero() != null) return true;
                    reason = "No hero found to receive EXP.";
                    return false;

                case QuestRewardType.Item:
                {
                    if (reward.item == null) { reason = "Reward item is missing."; return false; }

                    ItemInventory inventory = FindInventory();
                    if (inventory == null) { reason = "No inventory found."; return false; }

                    ItemInventory.ItemSlot slot = inventory.Items.Find(s => s.itemData == reward.item);
                    bool hasRoom = slot != null
                        ? reward.item.autoStackOnPickup && slot.currentLevel < reward.item.maxLevel
                        : !inventory.IsFull;

                    if (hasRoom) return true;
                    reason = "Your inventory is full.";
                    return false;
                }

                default:
                    return false;
            }
        }

        public static bool Grant(QuestReward reward)
        {
            switch (reward.type)
            {
                case QuestRewardType.Gold:
                    if (GoldSystem.Instance == null) return false;
                    GoldSystem.Instance.AddGold(reward.amount);
                    return true;

                case QuestRewardType.Celestium:
                    if (CelestiumSystem.Instance == null) return false;
                    CelestiumSystem.Instance.AddCelestium(reward.amount);
                    return true;

                case QuestRewardType.Experience:
                {
                    HeroInstance hero = FindPlayerHero();
                    if (hero == null) return false;
                    hero.GrantEXP(reward.amount);
                    return true;
                }

                case QuestRewardType.Item:
                {
                    ItemInventory inventory = FindInventory();
                    return inventory != null && inventory.TryAddItem(new ItemInstance(reward.item, reward.amount));
                }

                default:
                    return false;
            }
        }

        private static HeroInstance FindPlayerHero()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return null;

            HeroInstance hero = player.GetComponentInChildren<HeroInstance>();
            return hero != null ? hero : player.GetComponentInParent<HeroInstance>();
        }

        private static ItemInventory FindInventory()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            ItemInventory inventory = player != null ? player.GetComponentInChildren<ItemInventory>() : null;
            return inventory != null ? inventory : UnityEngine.Object.FindFirstObjectByType<ItemInventory>();
        }
    }
}