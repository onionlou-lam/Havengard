using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Character;

namespace Havengard.Abilities
{
    [CreateAssetMenu(fileName = "Fireball", menuName = "Abilities/Fireball")]
    public class Fireball : AbilityBase
    {
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private float damage = 25f;

        protected override void Execute(GameObject caster, GameObject target)
        {
            if (target == null || fireballPrefab == null) return;

            // Resource check
            if (caster.TryGetComponent(out ResourceSystem resource))
            {
                if (resource.Current < resourceCost) return;
                resource.Consume(resourceCost);
            }

            // Spawn fireball
            Vector3 spawnPos = caster.transform.position;
            GameObject projectileGO = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);

            if (projectileGO.TryGetComponent(out Projectile projectile))
            {
                Faction casterFaction = caster.GetComponent<Faction>();
                projectile.Init(target.transform, damage, casterFaction);
            }

            Debug.Log($"{caster.name} cast {abilityName} toward {target.name}");
        }
    }
}