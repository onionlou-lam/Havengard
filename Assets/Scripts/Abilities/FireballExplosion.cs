using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;
using Havengard.Statuses;

namespace Havengard.Abilities
{
    public class FireballExplosion : MonoBehaviour
    {
        private float radius;
        private Faction casterFaction;
        private bool friendlyFire;
        private int damage;
        private bool hasExploded;

        public void Setup(float radius, Faction faction, bool allowFriendly, int dmg)
        {
            this.radius = radius;
            casterFaction = faction;
            friendlyFire = allowFriendly;
            damage = dmg;
        }

        private void OnDestroy()
        {
            // only trigger explosion on valid destruction (not on scene unload)
            if (!hasExploded)
            {
                Explode();
                hasExploded = true;
            }
        }

        private void Explode()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits)
            {
                var health = hit.GetComponent<IHealth>();
                if (health == null) continue;
                if (!FactionUtility.CanDamage(casterFaction, health, friendlyFire)) continue;

                health.GetHealthSystem().Damage(damage);
            }

            Debug.Log($"Fireball exploded at {transform.position}, radius={radius}, damage={damage}");
        }

#if UNITY_EDITOR
        // visualize the explosion radius in Scene view
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}
