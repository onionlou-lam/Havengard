using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Enemies
{
    [RequireComponent(typeof(Health))]
    public abstract class EnemyBase : MonoBehaviour, IEnemy
    {
        protected IHealth health;

        protected virtual void Awake()
        {
            health = GetComponent<IHealth>();
            if (health == null)
                Debug.LogError($"{name} is missing an IHealth component!");
        }

        protected virtual void Start()
        {
            if (health is Health h)
            {
                h.OnDeath += HandleDeath;
            }
        }

        public abstract void PerformAttack(GameObject target);

        protected virtual void HandleDeath()
        {
            Debug.Log($"{name} has died.");
            Destroy(gameObject);
        }
    }
}
