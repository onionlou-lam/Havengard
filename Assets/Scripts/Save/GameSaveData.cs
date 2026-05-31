using System;
using System.Collections.Generic;

namespace Havengard.Save
{
    /// <summary>
    /// Root save data structure - contains all game state
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        // Save metadata
        public string saveVersion = "1.0";
        public string saveDate;
        public float playTime;

        // Currency
        public int gold;
        public int celestium;

        // Heroes
        public List<HeroSaveData> heroes = new List<HeroSaveData>();

        // Inventory (player's item inventory)
        public List<ItemSaveData> inventoryItems = new List<ItemSaveData>();

        // Buildings
        public List<BuildingSaveData> placedBuildings = new List<BuildingSaveData>();

        // Progression
        public int playerLevel;
        public int playerExp;
        public int availableSkillPoints;
        public int spentSkillPoints;

        // Wave progress
        public int currentNight;
        public int highestWaveCompleted;

        // Quest/Mission state (future phase)
        // Will add MissionSaveData[] here later

        public GameSaveData()
        {
            saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}