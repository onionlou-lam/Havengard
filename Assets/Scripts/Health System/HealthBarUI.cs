using UnityEngine;
using UnityEngine.UI;
using Havengard.Health;

namespace Havengard.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Slider slider = null;
        private IHealth trackedHealth;

        public void Setup(IHealth health)
        {
            if (health == null) return;
            trackedHealth = health;
            slider.maxValue = health.MaxHealth;
            slider.value = health.CurrentHealth;

            health.OnDamaged += UpdateBar;
            health.OnHealed += UpdateBar;
            health.OnDeath += HandleDeath;
        }

        private void UpdateBar()
        {
            if (trackedHealth == null) return;
            slider.value = trackedHealth.CurrentHealth;
        }

        private void HandleDeath()
        {
            if (slider != null) slider.value = 0f;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (trackedHealth != null)
            {
                trackedHealth.OnDamaged -= UpdateBar;
                trackedHealth.OnHealed -= UpdateBar;
                trackedHealth.OnDeath -= HandleDeath;
            }
        }
    }
}
