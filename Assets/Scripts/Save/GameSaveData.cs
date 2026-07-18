using System;
using System.Collections.Generic;
using UnityEngine;
using Havengard.Core.Heroes;
using Havengard.Items;
using Havengard.Core.Progression;

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

        // Character info (for main menu display)
        public string mainCharacterName;
        public string mainCharacterClass;
        public int mainCharacterLevel;

        // Currency
        public int gold;
        public int celestium;

        // Player Position
        public float playerPositionX;
        public float playerPositionY;
        public float playerPositionZ;

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

        public GameSaveData()
        {
            saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // Helper methods
        public void SetPlayerPosition(Vector3 position)
        {
            playerPositionX = position.x;
            playerPositionY = position.y;
            playerPositionZ = position.z;
        }

        public Vector3 GetPlayerPosition()
        {
            return new Vector3(playerPositionX, playerPositionY, playerPositionZ);
        }
    }
}