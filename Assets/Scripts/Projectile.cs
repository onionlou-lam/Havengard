using UnityEngine;
using Havengard.Health;

namespace Havengard.Projectiles
{
    [RequireComponent(typeof(Collider2D))]
    public class Projectile2D : MonoBehaviour
    {
        private Transform targetTransform;
        private Vector2 targetPoint;
        private float speed = 10f;
        private float damage = 10f;
        private Faction sourceFaction;
        private bool isPoint = false;
        public bool allowFriendlyFire = false;

        public void Init(Transform target, float damage, Faction sourceFaction, float speed = 10f, bool allowFriendly = false)
        {
            this.targetTransform = target;
            this.damage = damage;
            this.sourceFaction = sourceFaction;
            this.speed = speed;
            this.isPoint = false;
            this.allowFriendlyFire = allowFriendly;
        }

        public void Init(Vector2 point, float damage, Faction sourceFaction, float speed = 10f, bool allowFriendly = false)
        {
            this.targetPoint = point;
            this.damage = damage;
            this.sourceFaction = sourceFaction;
            this.speed = speed;
            this.isPoint = true;
            this.allowFriendlyFire = allowFriendly;
        }

        private void Update()
        {
            Vector2 dest = isPoint ? targetPoint : (targetTransform != null ? (Vector2)targetTransform.position : (Vector2)transform.position);
            transform.position = Vector2.MoveTowards(transform.position, dest, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, dest) < 0.1f)
            {
                // If it reaches the destination (point) then destroy
                if (isPoint) Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.transform == transform) return;

            if (other.TryGetComponent<IHealth>(out var health))
            {
                // prevent friendly fire unless allowed
                if (!allowFriendlyFire && health.GetFaction() == sourceFaction) return;

                health.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}
