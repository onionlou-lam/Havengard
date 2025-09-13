using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Character
{
    [RequireComponent(typeof(Health))]
    public class PlayerController : MonoBehaviour
    {
        private IHealth health;

        private void Start()
        {
            health = GetComponent<IHealth>();
            if (health != null)
            {
                health.OnDamaged += HandleDamaged;
                health.OnHealed += HandleHealed;
                health.OnDeath += HandleDeath;
            }
        }

        private void HandleDamaged()
        {
            Debug.Log("Player damaged: " + health.CurrentHealth);
        }

        private void HandleHealed()
        {
            Debug.Log("Player healed: " + health.CurrentHealth);
        }

        private void HandleDeath()
        {
            Debug.Log("Player died!");
        }

    }
}
