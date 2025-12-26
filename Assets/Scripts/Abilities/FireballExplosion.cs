using Havengard.Combat;
using Havengard.HealthSystem;
using Havengard.Statuses;
using Havengard.Units;
using UnityEngine;

namespace Havengard.Abilities
{
    public class FireballExplosion : MonoBehaviour
    {
        private float radius;
        private Faction casterFaction;
        private bool friendlyFire;
        private int aoeDamage;
        private bool hasExploded;
        private StatusEffectData burnEffect;

        public void Setup(float radius, Faction faction, bool allowFriendly, int aoeDamage, StatusEffectData burnEffect)
        {
            this.radius = radius;
            casterFaction = faction;
            friendlyFire = allowFriendly;
            this.aoeDamage = aoeDamage;
            this.burnEffect = burnEffect;
        }

        private void OnDestroy()
        {
            // only trigger explosion once
            if (!hasExploded)
            {
                hasExploded = true;
                Explode();
            }
        }

        private void Explode()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

            foreach (var hit in hits)
            {
                // Health may be on parent (your setup)
                var health = hit.GetComponentInParent<IHealth>();
                if (health == null) continue;
                if (!FactionUtility.CanDamage(casterFaction, health, friendlyFire)) continue;

                // AoE damage
                if (aoeDamage > 0)
                    health.GetHealthSystem().Damage(aoeDamage);

                // Burn (DoT + VFX) via StatusEffect system
                if (burnEffect != null)
                {
                    var targetMono = health as MonoBehaviour;
                    if (targetMono != null)
                        StatusEffectApplier.ApplyEffect(targetMono.gameObject, burnEffect);
                }
            }

            // Debug.Log($"Fireball exploded at {transform.position}, radius={radius}, aoeDamage={aoeDamage}, burn={(burnEffect != null ? burnEffect.effectName : "none")}");
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}
