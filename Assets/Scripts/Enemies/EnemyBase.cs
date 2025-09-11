using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Enemies
{
    public abstract class EnemyBase : MonoBehaviour, IEnemy
    {
        protected IHealth health;

        protected virtual void Awake()
        {
            health = GetComponent<IHealth>();
            if (health == null)
                Debug.LogError($"{name} is missing an IHealth component!");

            if (health != null)
                (health as Health).OnDeath += HandleDeath;
        }

        public abstract void PerformAttack(GameObject target);

        protected virtual void HandleDeath()
        {
            OnDeath();
            Destroy(gameObject);
        }

        public virtual void OnDeath()
        {
            Debug.Log($"{name} has died.");
        }
    }
}
