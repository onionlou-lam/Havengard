using UnityEngine;
using UnityEngine.UI;
using Havengard.HealthSystem;

namespace Havengard.HealthSystem
{
    /// <summary>
    /// World-space health bar that follows a Health component.
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Vector3 offset = new Vector3(0, 1.0f, 0);

        private Health target;
        private HealthSystem healthSystem;
        private bool subscribed;

        public void Init(Health health, Vector3 worldOffset)
        {
            Unhook();

            target = health;
            offset = worldOffset;

            if (!target)
                return;

            healthSystem = target.GetHealthSystem();
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged += UpdateBar;
                healthSystem.OnDeath += HandleDeath;
                subscribed = true;
            }

            UpdateBar();
        }

        private void LateUpdate()
        {
            if (!target) return;
            transform.position = target.transform.position + offset;
            transform.rotation = Quaternion.identity;
        }

        private void UpdateBar()
        {
            if (healthSystem == null || fillImage == null) return;
            float normalized = healthSystem.GetHealthNormalized();
            fillImage.fillAmount = normalized;
        }

        private void HandleDeath()
        {
            Destroy(gameObject);
            Unhook();
        }

        private void OnDisable() => Unhook();

        private void Unhook()
        {
            if (!subscribed || healthSystem == null) return;
            healthSystem.OnHealthChanged -= UpdateBar;
            healthSystem.OnDeath -= HandleDeath;
            subscribed = false;
        }
    }
}
