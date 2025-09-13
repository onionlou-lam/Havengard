using UnityEngine;
using Havengard;
using Havengard.HealthSystem;

namespace Havengard.Abilities
{
    public class FireballProjectile : MonoBehaviour
    {
        private float damage;
        private float radius;
        private Faction sourceFaction;

        public void Init(float damage, float radius, Faction sourceFaction)
        {
            this.damage = damage;
            this.radius = radius;
            this.sourceFaction = sourceFaction;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Damage main target
            if (collision.gameObject.TryGetComponent<IHealth>(out var mainTarget))
            {
                mainTarget.TakeDamage(damage);
            }

            // AoE splash damage
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IHealth>(out var health))
                {
                    if (health.GetFaction() != sourceFaction)
                    {
                        float splashDamage = damage * 0.5f;
                        health.TakeDamage(splashDamage);
                    }
                }
            }

            Destroy(gameObject);
        }
    }
}
