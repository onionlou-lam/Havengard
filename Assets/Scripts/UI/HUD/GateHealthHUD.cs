using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Core.HealthSystem;

namespace Havengard.UI
{
    /// <summary>
    /// Displays gate health in the top-right corner of the HUD
    /// </summary>
    public class GateHealthHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        [Tooltip("Reference to the gate's Health component")]
        private Health gateHealth;

        [SerializeField]
        [Tooltip("Gate icon image")]
        private Image gateIcon;

        [SerializeField]
        [Tooltip("Health bar fill image")]
        private Image healthBarFill;

        [SerializeField]
        [Tooltip("Health text (e.g., '750/1000')")]
        private TextMeshProUGUI healthText;

        [Header("Gate Icon Sprites")]
        [SerializeField]
        [Tooltip("Gate sprite at 100%-76% health")]
        private Sprite gateSprite100;

        [SerializeField]
        [Tooltip("Gate sprite at 75%-51% health")]
        private Sprite gateSprite75;

        [SerializeField]
        [Tooltip("Gate sprite at 50%-26% health")]
        private Sprite gateSprite50;

        [SerializeField]
        [Tooltip("Gate sprite at 25%-1% health")]
        private Sprite gateSprite25;

        [SerializeField]
        [Tooltip("Gate sprite at 0% health (destroyed)")]
        private Sprite gateSprite0;

        [Header("Health Bar Colors")]
        [SerializeField]
        private Color healthyColor = Color.green;

        [SerializeField]
        private Color damagedColor = Color.yellow;

        [SerializeField]
        private Color criticalColor = Color.red;

        [SerializeField]
        private Color destroyedColor = Color.gray;

        private HealthSystem healthSystem;

        private void Start()
        {
            FindGateIfNeeded();
            UpdateDisplay();
        }

        private void OnEnable()
        {
            HookGateHealth();
        }

        private void OnDisable()
        {
            UnhookGateHealth();
        }

        private void FindGateIfNeeded()
        {
            if (gateHealth == null)
            {
                // Try to find gate by tag or name
                GameObject gateObj = GameObject.FindGameObjectWithTag("Gate");
                if (gateObj == null)
                {
                    gateObj = GameObject.Find("Gate");
                }

                if (gateObj != null)
                {
                    gateHealth = gateObj.GetComponent<Health>();
                }

                if (gateHealth == null)
                {
                    Debug.LogWarning("[GateHealthHUD] No gate Health component found in scene!");
                }
            }
        }

        private void HookGateHealth()
        {
            UnhookGateHealth();

            if (gateHealth == null)
            {
                FindGateIfNeeded();
                if (gateHealth == null) return;
            }

            healthSystem = gateHealth.GetHealthSystem();
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged += OnGateHealthChanged;
                healthSystem.OnDeath += OnGateDeath;
            }

            UpdateDisplay();
        }

        private void UnhookGateHealth()
        {
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged -= OnGateHealthChanged;
                healthSystem.OnDeath -= OnGateDeath;
            }
        }

        private void OnGateHealthChanged(int current, int max)
        {
            UpdateDisplay();
        }

        private void OnGateDeath()
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (healthSystem == null || gateHealth == null)
            {
                HideDisplay();
                return;
            }

            int currentHealth = healthSystem.CurrentHealth;
            int maxHealth = healthSystem.MaxHealth;
            float healthPercent = healthSystem.GetHealthNormalized();

            // Update health bar fill
            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = healthPercent;
                healthBarFill.color = GetHealthColor(healthPercent);
            }

            // Update health text
            if (healthText != null)
            {
                healthText.text = $"{currentHealth}/{maxHealth}";
            }

            // Update gate icon sprite based on damage state
            if (gateIcon != null)
            {
                gateIcon.sprite = GetGateSpriteForHealth(healthPercent);
            }
        }

        private Color GetHealthColor(float healthPercent)
        {
            if (healthPercent <= 0f)
                return destroyedColor;
            else if (healthPercent <= 0.25f)
                return criticalColor;
            else if (healthPercent <= 0.5f)
                return damagedColor;
            else
                return healthyColor;
        }

        private Sprite GetGateSpriteForHealth(float healthPercent)
        {
            if (healthPercent <= 0f)
                return gateSprite0 ?? gateSprite100;
            else if (healthPercent <= 0.25f)
                return gateSprite25 ?? gateSprite100;
            else if (healthPercent <= 0.5f)
                return gateSprite50 ?? gateSprite100;
            else if (healthPercent <= 0.75f)
                return gateSprite75 ?? gateSprite100;
            else
                return gateSprite100;
        }

        private void HideDisplay()
        {
            if (healthBarFill != null)
                healthBarFill.fillAmount = 0;

            if (healthText != null)
                healthText.text = "---";

            if (gateIcon != null)
                gateIcon.color = new Color(1, 1, 1, 0.3f); // Semi-transparent
        }

        // Public method to set gate reference (useful for dynamic spawning)
        public void SetGate(Health gate)
        {
            UnhookGateHealth();
            gateHealth = gate;
            HookGateHealth();
        }
    }
}