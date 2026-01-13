using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.HealthSystem;

public class HealthBarHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;           // Player Health
    [SerializeField] private Image healthBar;         // Fill image
    [SerializeField] private TextMeshProUGUI healthText; // "HP / Max" text

    private HealthSystem healthSystem;

    private void Awake()
    {
        if (health == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                health = player.GetComponent<Health>();
        }
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
        healthSystem.OnDeath += UpdateBar;
    }

    private void Unhook()
    {
        if (healthSystem == null) return;

        healthSystem.OnHealthChanged -= OnHealthChangedHandler;
        healthSystem.OnDeath -= UpdateBar;
        healthSystem = null;
    }

    private void OnHealthChangedHandler(int current, int max)
    {
        UpdateBar();
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
