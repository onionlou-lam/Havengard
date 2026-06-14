using Havengard.Core.HealthSystem;
using Havengard.Units;
using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Orbital Explosion Ability")]
    public class OrbitalExplosionAbility : ZoneAbility
    {
        [Header("Orbital Explosion Properties")]
        [SerializeField] private float vortexSpeed = 5f;
        [SerializeField] private float vortexDuration = 2f;
        [SerializeField] private float explosionRadius = 8f;
        [SerializeField] private float explosionDamageMultiplier = 2f;

        [Header("Knockback")]
        [SerializeField] private bool enableKnockback = true;
        [SerializeField] private float knockbackForce = 10f;
        [SerializeField] private float knockbackDuration = 0.3f;

        [Header("VFX")]
        [SerializeField] private GameObject explosionVFXPrefab;

        public new void Cast(GameObject caster, GameObject target)
        {
            if (!CanCast(caster, target)) return;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            GameObject zoneInstance = Instantiate(GetZonePrefab(), mouseWorld, Quaternion.identity);

            var orbitalEffect = zoneInstance.GetComponent<OrbitalExplosionEffect>();
            if (orbitalEffect != null)
            {
                var casterHealth = caster.GetComponent<IHealth>();
                Faction casterFaction = casterHealth != null ? casterHealth.GetFaction() : Faction.Neutral;

                orbitalEffect.Initialize(
                    caster,
                    casterFaction,
                    vortexSpeed,
                    vortexDuration,
                    explosionRadius,
                    CalculateDamage(caster) * explosionDamageMultiplier,
                    enableKnockback,
                    knockbackForce,
                    knockbackDuration,
                    explosionVFXPrefab
                );
            }
            else
            {
                Debug.LogError($"[OrbitalExplosionAbility] Zone prefab is missing OrbitalExplosionEffect component!");
                Destroy(zoneInstance);
            }
        }

        public override void Activate(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            Cast(user.gameObject, targetEnemy);
        }

        // Helper method to access the protected zonePrefab from base class
        private GameObject GetZonePrefab()
        {
            // Access through reflection since zonePrefab is private in base class
            var field = typeof(ZoneAbility).GetField("zonePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(this) as GameObject;
        }
    }
}