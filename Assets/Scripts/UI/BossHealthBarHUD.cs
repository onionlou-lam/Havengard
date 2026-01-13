using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.HealthSystem;

public class BossHealthBarHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health bossHealth;               // Boss Health
    [SerializeField] private Image bossHealthFill;            // Fill image
    [SerializeField] private TextMeshProUGUI bossHealthText;  // e.g. "1234 / 2000"
    [SerializeField] private TextMeshProUGUI bossNameText;    // e.g. "Emissary of the Void"

    [Header("Animation")]
    [SerializeField] private float fillLerpSpeed = 6f;

    private HealthSystem healthSystem;
    private float displayedFill = 1f;

    private void OnEnable()
    {
        TryHook();
        ForceSnap();
    }

    private void OnDisable()
    {
        Unhook();
    }

    private void Update()
    {
        if (bossHealth == null || healthSystem == null)
            TryHook();

        if (healthSystem == null || bossHealthFill == null)
            return;

        float targetFill = healthSystem.GetHealthNormalized();
        displayedFill = Mathf.Lerp(displayedFill, targetFill, Time.deltaTime * fillLerpSpeed);
        bossHealthFill.fillAmount = displayedFill;
    }

    public void SetBoss(Health newBossHealth, string bossName = null)
    {
        Unhook();
        bossHealth = newBossHealth;
        if (bossNameText != null && !string.IsNullOrEmpty(bossName))
            bossNameText.text = bossName;

        TryHook();
        ForceSnap();
    }

    private void TryHook()
    {
        if (bossHealth == null) return;

        if (healthSystem != null) return;

        healthSystem = bossHealth.GetHealthSystem();
        if (healthSystem == null) return;

        healthSystem.OnHealthChanged += OnBossHealthChanged;
        healthSystem.OnDeath += OnBossDeathHandler;

        ForceSnap();
    }

    private void Unhook()
    {
        if (healthSystem == null) return;

        healthSystem.OnHealthChanged -= OnBossHealthChanged;
        healthSystem.OnDeath -= OnBossDeathHandler;
        healthSystem = null;
    }

    private void OnBossHealthChanged(int current, int max)
    {
        UpdateText();
    }

    private void OnBossDeathHandler()
    {
        UpdateText();
    }

    private void ForceSnap()
    {
        if (healthSystem == null || bossHealthFill == null) return;
        displayedFill = healthSystem.GetHealthNormalized();
        bossHealthFill.fillAmount = displayedFill;
        UpdateText();
    }

    private void UpdateText()
    {
        if (bossHealthText == null || healthSystem == null) return;
        bossHealthText.text = $"{healthSystem.GetHealth()} / {healthSystem.GetMaxHealth()}";
    }
}
