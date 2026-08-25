using UnityEngine;
using System.Collections.Generic;

namespace Havengard.Waves.UI
{
    /// <summary>
    /// Parsed data for displaying wave preview information
    /// </summary>
    public class WavePreviewData
    {
        public int waveNumber;
        public string waveName;
        public List<EnemyPreview> enemies = new List<EnemyPreview>();
        public int totalEnemyCount;
        public int goldReward;
        public int expReward;
        public int celestiumReward;

        public class EnemyPreview
        {
            public GameObject enemyPrefab;
            public string enemyName;
            public Sprite icon;
            public int count;
            public string description;

            public EnemyPreview(GameObject prefab, int count)
            {
                this.enemyPrefab = prefab;
                this.count = count;
                this.enemyName = prefab != null ? prefab.name : "Unknown";
                
                // Try to get icon from sprite renderer
                if (prefab != null)
                {
                    var spriteRenderer = prefab.GetComponentInChildren<SpriteRenderer>();
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                    {
                        icon = spriteRenderer.sprite;
                    }
                }
            }
        }

        public static WavePreviewData FromWaveDefinition(WaveDefinition wave, int waveNumber)
        {
            if (wave == null) return null;

            var data = new WavePreviewData
            {
                waveNumber = waveNumber,
                waveName = wave.waveName,
                goldReward = wave.rewardGold,
                expReward = wave.rewardExp,
                celestiumReward = wave.rewardCelestium
            };

            // Count enemies by type
            Dictionary<GameObject, int> enemyCounts = new Dictionary<GameObject, int>();

            if (wave.groups != null)
            {
                foreach (var group in wave.groups)
                {
                    if (group == null || group.entries == null) continue;

                    foreach (var entry in group.entries)
                    {
                        if (entry.enemyPrefab == null) continue;

                        if (enemyCounts.ContainsKey(entry.enemyPrefab))
                        {
                            enemyCounts[entry.enemyPrefab] += entry.count;
                        }
                        else
                        {
                            enemyCounts[entry.enemyPrefab] = entry.count;
                        }

                        data.totalEnemyCount += entry.count;
                    }
                }
            }

            // Convert to preview list
            foreach (var kvp in enemyCounts)
            {
                data.enemies.Add(new EnemyPreview(kvp.Key, kvp.Value));
            }

            return data;
        }
    }
}