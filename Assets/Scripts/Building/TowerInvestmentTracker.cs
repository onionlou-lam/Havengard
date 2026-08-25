using UnityEngine;

namespace Havengard.Building
{
    /// <summary>
    /// Tracks investment and statistics for a placed tower
    /// </summary>
    public class TowerInvestmentTracker : MonoBehaviour
    {
        [Header("Identity")]
        public string towerID;
        public int currentLevel = 0;

        [Header("Grid Position")]
        public Vector2Int gridPosition;
        public int gridWidth;
        public int gridHeight;

        [Header("Investment")]
        public int initialBuildCost;
        public int totalInvestment;

        [Header("Wave Tracking")]
        public int waveNumberPlaced;

        [Header("Damage Statistics")]
        public float totalDamageDealt;
        public float currentWaveDamageDealt;
        public float previousWaveDamageDealt;

        public void Initialize(TowerBuildData buildData, Vector2Int gridPos, int waveNumber, int buildCost)
        {
            towerID = buildData.towerID;
            currentLevel = 0;
            gridPosition = gridPos;
            gridWidth = buildData.gridWidth;
            gridHeight = buildData.gridHeight;
            initialBuildCost = buildCost;
            totalInvestment = buildCost;
            waveNumberPlaced = waveNumber;
            totalDamageDealt = 0f;
            currentWaveDamageDealt = 0f;
            previousWaveDamageDealt = 0f;
        }

        public void AddUpgradeCost(int cost)
        {
            totalInvestment += cost;
            currentLevel++;
        }

        public int GetSellValue()
        {
            return Mathf.FloorToInt(totalInvestment * 0.5f);
        }

        public void RecordDamage(float damage)
        {
            totalDamageDealt += damage;
            currentWaveDamageDealt += damage;
        }

        public void OnWaveStarted()
        {
            previousWaveDamageDealt = currentWaveDamageDealt;
            currentWaveDamageDealt = 0f;
        }

        public void OnWaveEnded()
        {
            previousWaveDamageDealt = currentWaveDamageDealt;
        }
    }
}