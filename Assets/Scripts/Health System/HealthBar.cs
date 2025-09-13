using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private IHealth trackedHealth;

    public void Init(IHealth health)
    {
        trackedHealth = health;
        slider.maxValue = health.MaxHealth;
        slider.value = health.CurrentHealth;

        health.OnDamaged += UpdateBar;
        health.OnHealed += UpdateBar;
        health.OnDeath += HandleDeath;
    }

    private void UpdateBar()
    {
        slider.value = trackedHealth.CurrentHealth;
    }

    private void HandleDeath()
    {
        slider.value = 0f;
        Destroy(gameObject);
    }
}
