using UnityEngine;
using UnityEngine.UI;

namespace Havengard.HealthSystem
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private Image healthBar;

        private void Awake()
        {
            if (health == null) health = GetComponentInParent<Health>();

            if (health != null)
            {
                health.GetHealthSystem().OnHealthChanged += UpdateHealthBar;
                health.GetHealthSystem().OnDeath += HandleDeath;
            }
        }

        private void UpdateHealthBar()
        {
            if (health == null) return;
            float percent = health.GetHealthSystem().GetHealthNormalized();
            healthBar.fillAmount = percent;
        }

        private void HandleDeath()
        {
            // Optional: hide bar or set to 0
            healthBar.fillAmount = 0;
        }
    }
}
