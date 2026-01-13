using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.HealthSystem;
using System.Collections;

public class HealthBarHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;           // Player Health
    [SerializeField] private Image healthBar;         // Fill image
    [SerializeField] private TextMeshProUGUI healthText; // "HP / Max" text

    [Header("Flash Effects")]
    [SerializeField] private Image flashOverlay;      // Optional separate image for flash effect
    [SerializeField] private Color damageFlashColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private Color healFlashColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private float glowIntensity = 1.5f;

    private HealthSystem healthSystem;
    private Coroutine currentFlashCoroutine;
    private Color originalHealthBarColor;

    private void Awake()
    {
        if (health == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                health = player.GetComponent<Health>();
        }

        if (healthBar != null)
            originalHealthBarColor = healthBar.color;
    }

    private void OnEnable()
    {
        TryHook();
        UpdateBar();
    }

    private void OnDisable()
    {
        Unhook();
    }

    private void Update()
    {
        if (health == null || healthSystem == null)
            TryHook();

        if (healthSystem != null && healthBar != null)
        {
            float normalized = healthSystem.GetHealthNormalized();
            healthBar.fillAmount = normalized;
            UpdateHealthText();
        }
    }

    private void TryHook()
    {
        if (health == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                health = player.GetComponent<Health>();
        }

        if (health == null || healthSystem != null) return;

        healthSystem = health.GetHealthSystem();
        if (healthSystem == null) return;

        healthSystem.OnHealthChanged += OnHealthChangedHandler;
        healthSystem.OnDamaged += OnDamagedHandler;
        healthSystem.OnHealed += OnHealedHandler;
        healthSystem.OnDeath += UpdateBar;
    }

    private void Unhook()
    {
        if (healthSystem == null) return;

        healthSystem.OnHealthChanged -= OnHealthChangedHandler;
        healthSystem.OnDamaged -= OnDamagedHandler;
        healthSystem.OnHealed -= OnHealedHandler;
        healthSystem.OnDeath -= UpdateBar;
        healthSystem = null;
    }

    private void OnHealthChangedHandler(int current, int max)
    {
        UpdateBar();
    }

    private void OnDamagedHandler(int amount)
    {
        PlayFlashEffect(damageFlashColor);
    }

    private void OnHealedHandler(int amount)
    {
        PlayFlashEffect(healFlashColor);
    }

    private void PlayFlashEffect(Color flashColor)
    {
        if (currentFlashCoroutine != null)
            StopCoroutine(currentFlashCoroutine);

        currentFlashCoroutine = StartCoroutine(FlashCoroutine(flashColor));
    }

    private IEnumerator FlashCoroutine(Color flashColor)
    {
        float elapsed = 0f;

        // If using a separate flash overlay
        if (flashOverlay != null)
        {
            flashOverlay.color = flashColor;
            flashOverlay.enabled = true;

            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(flashColor.a, 0f, elapsed / flashDuration);
                Color c = flashColor;
                c.a = alpha;
                flashOverlay.color = c;
                yield return null;
            }

            flashOverlay.enabled = false;
        }
        else
        {
            // Flash the health bar itself with glow
            Color glowColor = new Color(
                flashColor.r * glowIntensity,
                flashColor.g * glowIntensity,
                flashColor.b * glowIntensity,
                1f
            );

            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;
                healthBar.color = Color.Lerp(glowColor, originalHealthBarColor, t);
                yield return null;
            }

            healthBar.color = originalHealthBarColor;
        }

        currentFlashCoroutine = null;
    }

    private void UpdateBar()
    {
        if (healthSystem == null || healthBar == null) return;

        float normalized = healthSystem.GetHealthNormalized();
        healthBar.fillAmount = normalized;
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        if (healthText == null || healthSystem == null) return;

        int current = healthSystem.GetHealth();
        int max = healthSystem.GetMaxHealth();
        healthText.text = $"{current} / {max}";
    }
}
