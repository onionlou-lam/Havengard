using System;

namespace Havengard.Save
{
    /// <summary>
    /// Serializable data for a single inventory item
    /// </summary>
    [Serializable]
    public class ItemSaveData
    {
        public string itemDataName;    // Name of ItemData asset
        public int currentLevel;       // Item level

        public ItemSaveData() { }
        
        public ItemSaveData(string itemDataName, int currentLevel)
        {
            this.itemDataName = itemDataName;
            this.currentLevel = currentLevel;
        }
    }
}