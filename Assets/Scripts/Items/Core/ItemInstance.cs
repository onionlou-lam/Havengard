using System;
using UnityEngine;

namespace Havengard.Items
{
    [Serializable]
    public class ItemInstance
    {
        public ItemData itemData;
        public int level;  // ADD THIS BACK
        public string uniqueID;
        public float acquiredTime;

        public ItemInstance(ItemData data, int startLevel = 1)
        {
            itemData = data;
            level = Mathf.Clamp(startLevel, 1, data != null ? data.maxLevel : 1);
            uniqueID = System.Guid.NewGuid().ToString();
            acquiredTime = Time.time;
        }

        public bool CanStackWith(ItemInstance other)
        {
            if (other == null || itemData == null || other.itemData == null)
                return false;

            return itemData == other.itemData &&
                   itemData.autoStackOnPickup &&
                   level < itemData.maxLevel;
        }

        public bool TryStack()
        {
            if (itemData == null) return false;

            if (level < itemData.maxLevel)
            {
                level++;
                return true;
            }
            return false;
        }

        public int GetDisenchantValue()
        {
            if (itemData == null) return 0;
            return itemData.disenchantValue * level;
        }

        public override string ToString()
        {
            return $"{(itemData != null ? itemData.itemName : "Unknown")} (Lv.{level})";
        }
    }
}