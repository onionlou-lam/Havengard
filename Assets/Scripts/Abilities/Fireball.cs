using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Fireball")]
    public class Fireball : AbilityBase
    {
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private float damage = 25f;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return fireballPrefab != null && target != null;
        }

        public override void Execute(GameObject caster, GameObject target)
        {
            if (fireballPrefab == null || target == null) return;

            var projGO = Instantiate(fireballPrefab, caster.transform.position, Quaternion.identity);

            // Optional: initialize your projectile if it exposes an Init-like API
            var projectile = projGO.GetComponent<MonoBehaviour>();
            var casterHealth = caster.GetComponent<Health>();
            Faction faction = casterHealth != null ? casterHealth.GetFaction() : Faction.Neutral;

            // If your projectile script has something like:
            // void Init(Transform target, float dmg, Faction faction)
            var method = projectile != null
                ? projectile.GetType().GetMethod("Init")
                : null;

            if (method != null)
                method.Invoke(projectile, new object[] { target.transform, damage, faction });

            Debug.Log($"{caster.name} cast {AbilityName} toward {target.name}");
        }
    }
}
