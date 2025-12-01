using UnityEngine;
using UnityEngine.UI;
using Havengard.HealthSystem;

public class HealthBarHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;       // Player Health component
    [SerializeField] private Image healthBar;     // Fill image in the HUD

    private HealthSystem healthSystem;

    private void Awake()
    {
        // Try to auto-find player if not assigned
        if (health == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                health = player.GetComponent<Health>();
                //Debug.Log($"[HealthBarHUD] Found player Health on {player.name}");
            }
            else
            {
                Debug.LogWarning("[HealthBarHUD] No object with tag 'Player' found. Assign Health manually in inspector.");
            }
        }
    }

    private void OnEnable()
    {
        TryHook();
        UpdateBar();   // do an initial draw
    }

    private void OnDisable()
    {
        Unhook();
    }

    private void Update()
    {
        // Safety net: if the hookup failed earlier (e.g. player spawned later),
        // keep trying until we succeed.
        if ((health == null || healthSystem == null))
        {
            TryHook();
        }

        // As a fallback, update every frame so the HUD never lags behind.
        if (healthSystem != null && healthBar != null)
        {
            float normalized = healthSystem.GetHealthNormalized();
            healthBar.fillAmount = normalized;
        }
    }

    private void TryHook()
    {
        if (health == null)
        {
            // Re-try auto-find in case player spawned after Awake
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                health = player.GetComponent<Health>();
                Debug.Log($"[HealthBarHUD] (Retry) Found player Health on {player.name}");
            }
        }

        if (health == null || healthSystem != null) return;

        healthSystem = health.GetHealthSystem();
        if (healthSystem == null)
        {
            Debug.LogWarning("[HealthBarHUD] HealthSystem is null on assigned Health.");
            return;
        }

        healthSystem.OnHealthChanged += UpdateBar;
        healthSystem.OnDeath += UpdateBar;

        Debug.Log($"[HealthBarHUD] Hooked into HealthSystem. Start HP = {healthSystem.GetHealth()}/{healthSystem.GetMaxHealth()}");
    }

    private void Unhook()
    {
        if (healthSystem == null) return;

        healthSystem.OnHealthChanged -= UpdateBar;
        healthSystem.OnDeath -= UpdateBar;
        healthSystem = null;
    }

    private void UpdateBar()
    {
        if (healthSystem == null || healthBar == null) return;

        float normalized = healthSystem.GetHealthNormalized();
        healthBar.fillAmount = normalized;
        // Uncomment if you want spam for debugging:
        // Debug.Log($"[HealthBarHUD] Health bar updated: {normalized}");
    }
}
