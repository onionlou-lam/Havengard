using UnityEngine;
using UnityEngine.UI;

namespace Havengard.Core.HealthSystem
{
    /// <summary>
    /// World-space health bar that follows a Health component.
    /// Subscribes to Health wrapper events (not the HealthSystem instance) so it stays valid
    /// even if stats/health are initialised after the bar spawns.
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Vector3 offset = new Vector3(0, 1.0f, 0);

        private Health target;
        private bool subscribed;

        public void Init(Health health, Vector3 worldOffset)
        {
            Unhook();

            target = health;
            offset = worldOffset;

            if (!target)
                return;

            // Subscribe to the wrapper events (stable even if HealthSystem instance changes)
            target.OnDamaged += OnDamagedHandler;
            target.OnHealed += OnHealedHandler;
            target.OnDeath += HandleDeath;
            subscribed = true;

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
            if (!target || fillImage == null) return;

            var hs = target.GetHealthSystem();
            if (hs == null) return;

            fillImage.fillAmount = hs.GetHealthNormalized();
        }

        private void HandleDeath()
        {
            Unhook();
            Destroy(gameObject);
        }

        private void OnDisable() => Unhook();

        private void Unhook()
        {
            if (!subscribed || !target) return;

            target.OnDamaged -= OnDamagedHandler;
            target.OnHealed -= OnHealedHandler;
            target.OnDeath -= HandleDeath;

            subscribed = false;
        }

        private void OnDamagedHandler(int amount)
        {
            UpdateBar();
        }

        private void OnHealedHandler(int amount)
        {
            UpdateBar();
        }
    }
}
