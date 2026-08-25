using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Havengard.Waves.UI
{
    /// <summary>
    /// Individual enemy item in the wave preview list
    /// </summary>
    public class WavePreviewEnemyItem : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Image backgroundImage;

        [Header("Colors")]
        [SerializeField] private Color defaultColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color eliteColor = new Color(0.8f, 0.4f, 0f, 0.8f);
        [SerializeField] private Color bossColor = new Color(0.8f, 0f, 0f, 0.8f);

        public void Setup(WavePreviewData.EnemyPreview enemy)
        {
            if (enemy == null) return;

            // Set icon
            if (iconImage != null && enemy.icon != null)
            {
                iconImage.sprite = enemy.icon;
                iconImage.enabled = true;
            }
            else if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            // Set name
            if (nameText != null)
            {
                nameText.text = CleanEnemyName(enemy.enemyName);
            }

            // Set count
            if (countText != null)
            {
                countText.text = $"x{enemy.count}";
            }

            // Set background color based on enemy type (simple heuristic)
            if (backgroundImage != null)
            {
                string nameLower = enemy.enemyName.ToLower();
                
                if (nameLower.Contains("boss"))
                {
                    backgroundImage.color = bossColor;
                }
                else if (nameLower.Contains("elite") || nameLower.Contains("champion"))
                {
                    backgroundImage.color = eliteColor;
                }
                else
                {
                    backgroundImage.color = defaultColor;
                }
            }
        }

        private string CleanEnemyName(string name)
        {
            // Remove "(Clone)" and other unity suffixes
            name = name.Replace("(Clone)", "").Trim();
            
            // Remove common prefixes
            if (name.StartsWith("Enemy_"))
                name = name.Substring(6);
            
            // Add spaces before capitals (PascalCase -> Pascal Case)
            name = System.Text.RegularExpressions.Regex.Replace(name, "(\\B[A-Z])", " $1");
            
            return name;
        }
    }
}