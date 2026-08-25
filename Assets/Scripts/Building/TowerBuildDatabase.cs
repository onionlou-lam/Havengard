using UnityEngine;

namespace Havengard.Building
{
    /// <summary>
    /// Database containing all buildable tower types
    /// </summary>
    [CreateAssetMenu(menuName = "Havengard/Building/Tower Build Database")]
    public class TowerBuildDatabase : ScriptableObject
    {
        public TowerBuildData[] towers;

        public TowerBuildData GetTowerByID(string id)
        {
            if (towers == null) return null;

            foreach (var tower in towers)
            {
                if (tower != null && tower.towerID == id)
                    return tower;
            }

            return null;
        }
    }
}