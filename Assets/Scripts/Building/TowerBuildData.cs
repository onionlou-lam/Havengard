using UnityEngine;

namespace Havengard.Building
{
    /// <summary>
    /// ScriptableObject defining a buildable tower type with stats per level
    /// </summary>
    [CreateAssetMenu(menuName = "Havengard/Building/Tower Build Data", fileName = "TowerData_")]
    public class TowerBuildData : ScriptableObject
    {
        [Header("Identity")]
        public string towerID;
        public string displayName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;

        [Header("Footprint")]
        [Tooltip("Grid width in cells")]
        public int gridWidth = 4;
        [Tooltip("Grid height in cells")]
        public int gridHeight = 4;

        [Header("Prefab")]
        [Tooltip("Tower prefab with TowerUnit component")]
        public GameObject towerPrefab;

        [Header("Level Data")]
        public TowerLevelData[] levels;

        /// <summary>
        /// Get level data for a specific level (0-indexed)
        /// </summary>
        public TowerLevelData GetLevelData(int level)
        {
            if (levels == null || level < 0 || level >= levels.Length)
                return null;
            return levels[level];
        }

        public int MaxLevel => levels != null ? levels.Length : 0;
    }

    [System.Serializable]
    public class TowerLevelData
    {
        [Header("Cost")]
        public int buildCost;
        public int upgradeCost;

        [Header("Stats")]
        public int damage;
        public float attackRange;
        public float attackSpeed;
        public float projectileSpeed;

        [Header("Visual")]
        [Tooltip("Optional: override prefab for this level")]
        public GameObject levelPrefab;
    }
}