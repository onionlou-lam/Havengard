using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Character
{
    [RequireComponent(typeof(Health))]
    public class PlayerController : MonoBehaviour
    {
        private IHealth health;

        private void Awake()
        {
            health = GetComponent<IHealth>();
        }

        private void Start()
        {
            if (health is Health h)
            {
                h.OnDamaged += HandleDamaged;
                h.OnHealed += HandleHealed;
                h.OnDeath += HandleDeath;
            }
        }

        private void HandleDamaged(float amount)
        {
            Debug.Log($"Player took {amount} damage! Current HP: {health.GetCurrentHealth()}");
        }

        private void HandleHealed(float amount)
        {
            Debug.Log($"Player healed {amount} HP. Current HP: {health.GetCurrentHealth()}");
        }

        private void HandleDeath()
        {
            Debug.Log("Player has died!");
            // Handle respawn or game over logic here
        }
    }
}
