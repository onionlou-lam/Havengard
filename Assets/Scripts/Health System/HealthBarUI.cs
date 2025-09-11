using UnityEngine;
using UnityEngine.UI;

namespace Havengard.HealthSystem
{
    /// <summary>
    /// Simple world-space or screen-space health bar UI.
    /// Updates an Image's fill amount based on a linked HealthSystem.
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Tooltip("GameObject that provides a HealthSystem (via IGetHealthSystem). Optional.")]
        [SerializeField] private GameObject healthSource;

        [Tooltip("Image representing the health bar fill.")]
        [SerializeField] private Image healthFillImage;

        private HealthSystem healthSystem;

        private void Start()
        {
            if (healthSource != null && HealthSystem.TryGet(healthSource, out HealthSystem found))
            {
                SetHealthSystem(found);
            }
        }

        public void SetHealthSystem(HealthSystem newHealthSystem)
        {
            if (healthSystem != null)
                healthSystem.OnHealthChanged -= OnHealthChanged;

            healthSystem = newHealthSystem;
            UpdateHealthBar();

            if (healthSystem != null)
                healthSystem.OnHealthChanged += OnHealthChanged;
        }

        private void OnHealthChanged(object sender, System.EventArgs e) => UpdateHealthBar();

        private void UpdateHealthBar()
        {
            if (healthFillImage != null && healthSystem != null)
                healthFillImage.fillAmount = healthSystem.GetHealthNormalized();
        }

        private void OnDestroy()
        {
            if (healthSystem != null)
                healthSystem.OnHealthChanged -= OnHealthChanged;
        }
    }
}
