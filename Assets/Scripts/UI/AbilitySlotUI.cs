using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Havengard.Abilities;

namespace Havengard.UI
{
    /// <summary>
    /// Represents a single ability slot in the ability bar.
    /// Displays icon, keybind, cooldown overlay, and countdown timer.
    /// </summary>
    public class AbilitySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] private Image abilityIcon;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private TextMeshProUGUI keybindText;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private Image flashOverlay;

        [Header("Visual Settings")]
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color onCooldownColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private float flashDuration = 0.2f;

        private AbilityBase ability;
        private float cooldownEndTime;
        private bool isOnCooldown;
        private float flashTimer;

        private void Awake()
        {
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 0f;
                cooldownOverlay.enabled = true;
            }

            if (cooldownText != null)
                cooldownText.enabled = false;

            if (flashOverlay != null)
                flashOverlay.enabled = false;
        }

        private void Update()
        {
            UpdateCooldown();
            UpdateFlash();
        }

        /// <summary>
        /// Sets the keybind text (e.g., "Q", "LMB", "RMB").
        /// </summary>
        public void SetKeybind(string keybind)
        {
            if (keybindText != null)
                keybindText.text = keybind;
        }

        /// <summary>
        /// Sets the ability for this slot.
        /// </summary>
        public void SetAbility(AbilityBase newAbility)
        {
            ability = newAbility;

            if (abilityIcon != null)
            {
                if (ability != null && ability.icon != null)
                {
                    abilityIcon.sprite = ability.icon;
                    abilityIcon.enabled = true;
                }
                else
                {
                    abilityIcon.sprite = null;
                    abilityIcon.enabled = false;
                }
            }

            ResetCooldown();
        }

        /// <summary>
        /// Starts the cooldown visual effect.
        /// </summary>
        public void StartCooldown(float duration)
        {
            if (duration <= 0f) return;

            cooldownEndTime = Time.time + duration;
            isOnCooldown = true;

            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 1f;
            }

            if (cooldownText != null)
                cooldownText.enabled = true;

            if (abilityIcon != null)
                abilityIcon.color = onCooldownColor;
        }

        /// <summary>
        /// Triggers a flash effect on the ability icon.
        /// </summary>
        public void Flash()
        {
            flashTimer = flashDuration;

            if (flashOverlay != null)
            {
                flashOverlay.color = flashColor;
                flashOverlay.enabled = true;
            }
        }

        /// <summary>
        /// Resets the cooldown state.
        /// </summary>
        public void ResetCooldown()
        {
            isOnCooldown = false;
            cooldownEndTime = 0f;

            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = 0f;

            if (cooldownText != null)
                cooldownText.enabled = false;

            if (abilityIcon != null)
                abilityIcon.color = availableColor;
        }

        private void UpdateCooldown()
        {
            if (!isOnCooldown) return;

            float now = Time.time;
            float remaining = cooldownEndTime - now;

            if (remaining <= 0f)
            {
                ResetCooldown();
                return;
            }

            // Update cooldown overlay (radial fill)
            if (cooldownOverlay != null && ability != null)
            {
                float cooldownDuration = ability.baseCooldown;
                if (cooldownDuration > 0f)
                {
                    float progress = remaining / cooldownDuration;
                    cooldownOverlay.fillAmount = progress;
                }
            }

            // Update cooldown text
            if (cooldownText != null)
            {
                if (remaining >= 1f)
                    cooldownText.text = Mathf.Ceil(remaining).ToString("F0");
                else
                    cooldownText.text = remaining.ToString("F1");
            }
        }

        private void UpdateFlash()
        {
            if (flashTimer <= 0f) return;

            flashTimer -= Time.deltaTime;

            if (flashOverlay != null)
            {
                if (flashTimer <= 0f)
                {
                    flashOverlay.enabled = false;
                }
                else
                {
                    // Fade out the flash
                    float alpha = Mathf.Lerp(0f, flashColor.a, flashTimer / flashDuration);
                    Color c = flashColor;
                    c.a = alpha;
                    flashOverlay.color = c;
                }
            }
        }

        /// <summary>
        /// Returns the ability assigned to this slot.
        /// </summary>
        public AbilityBase GetAbility()
        {
            return ability;
        }

        // NEW: Tooltip Integration
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (ability != null && TooltipManager.Instance != null)
            {
                TooltipManager.Instance.ShowAbilityTooltip(ability);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.HideAbilityTooltip();
            }
        }
    }
}