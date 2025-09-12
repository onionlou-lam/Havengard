using UnityEngine;
using UnityEngine.UI;

namespace Havengard.HealthSystem
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private Image fillImage;

        private void Start()
        {
            if (health == null) health = GetComponentInParent<Health>();

            if (health != null)
            {
                health.OnDamaged += UpdateBar;
                health.OnHealed += UpdateBar;
                health.OnDeath += HandleDeath;

                UpdateBar(0); // initialize UI
            }
        }

        private void UpdateBar(float _)
        {
            if (health == null) return;

            float ratio = health.GetCurrentHealth() / health.GetMaxHealth();
            fillImage.fillAmount = ratio;
        }

        private void HandleDeath()
        {
            // Optionally hide UI
            gameObject.SetActive(false);
        }
    }
}
