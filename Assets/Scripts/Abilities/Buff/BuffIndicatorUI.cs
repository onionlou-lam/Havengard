using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Havengard.Abilities
{
    /// <summary>
    /// Optional UI component to show active buffs with duration timers.
    /// Can be attached to a UI canvas to display buff icons and remaining time.
    /// </summary>
    public class BuffIndicatorUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject buffIconPrefab;
        [SerializeField] private Transform buffContainer;
        [SerializeField] private GameObject targetObject;

        [Header("Settings")]
        [SerializeField] private bool autoFindPlayer = true;
        [SerializeField] private float updateInterval = 0.1f;

        private float lastUpdateTime;

        private void Start()
        {
            if (autoFindPlayer && targetObject == null)
            {
                targetObject = GameObject.FindGameObjectWithTag("Player");
            }
        }

        private void Update()
        {
            if (Time.time - lastUpdateTime < updateInterval) return;
            lastUpdateTime = Time.time;

            RefreshBuffs();
        }

        private void RefreshBuffs()
        {
            if (targetObject == null || buffContainer == null) return;

            // Clear existing indicators
            foreach (Transform child in buffContainer)
            {
                Destroy(child.gameObject);
            }

            // Get all active buff instances
            BuffInstance[] buffs = targetObject.GetComponents<BuffInstance>();

            foreach (BuffInstance buff in buffs)
            {
                if (!buff.IsActive) continue;

                CreateBuffIcon(buff);
            }
        }

        private void CreateBuffIcon(BuffInstance buff)
        {
            if (buffIconPrefab == null) return;

            GameObject icon = Instantiate(buffIconPrefab, buffContainer);
            
            // Set icon image
            Image iconImage = icon.GetComponent<Image>();
            if (iconImage != null && buff.SourceAbility != null && buff.SourceAbility.Icon != null)
            {
                iconImage.sprite = buff.SourceAbility.Icon;
            }

            // Set duration text (only for duration-based buffs)
            TextMeshProUGUI durationText = icon.GetComponentInChildren<TextMeshProUGUI>();
            if (durationText != null)
            {
                if (buff.SourceAbility.GetBuffType() == BuffType.Duration)
                {
                    float remaining = buff.RemainingTime;
                    durationText.text = remaining >= 1f ? 
                        Mathf.Ceil(remaining).ToString("F0") : 
                        remaining.ToString("F1");
                }
                else
                {
                    durationText.text = "∞"; // Infinite/toggle buff
                }
            }
        }
    }
}