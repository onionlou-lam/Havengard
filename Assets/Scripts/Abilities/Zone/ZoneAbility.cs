using Havengard.Core;
using UnityEngine;
using Havengard.Statuses;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Zone Ability")]
    public class ZoneAbility : AbilityBase
    {
        [Header("Zone Properties")]
        [SerializeField] private GameObject zonePrefab; // Prefab contains all VFX
        [SerializeField] private float maximumRange = 15f;
        [SerializeField] private bool followsCaster = false;

        [Header("Zone Status Effect")]
        [SerializeField] private StatusEffectData statusEffect;
        [SerializeField] private int maxStatusStacks = 1;

        // Changed from override to regular method
        public bool CanCast(GameObject caster, GameObject target)
        {
            if (zonePrefab == null) return false;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            return Vector3.Distance(caster.transform.position, mouseWorld) <= maximumRange;
        }

        public void Cast(GameObject caster, GameObject target)
        {
            if (zonePrefab == null || !CanCast(caster, target)) return;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            Vector3 targetPosition = followsCaster ? caster.transform.position : mouseWorld;

            GameObject zoneInstance = Instantiate(zonePrefab, targetPosition, Quaternion.identity);

            // Initialize the zone effect component
            var zoneEffect = zoneInstance.GetComponent<ZoneEffect>();
            if (zoneEffect != null)
            {
                zoneEffect.Initialize(caster, followsCaster, statusEffect, maxStatusStacks);
            }
            else
            {
                Debug.LogError($"[ZoneAbility] Zone prefab '{zonePrefab.name}' is missing ZoneEffect component!");
                Destroy(zoneInstance);
            }
        }

        public override void Activate(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            // Implementation for Activate, can be empty or call Cast if appropriate
            Cast(user.gameObject, targetEnemy);
        }

        public override void Deactivate(AbilityUser user)
        {
            // Implementation for Deactivate, can be empty or handle zone cleanup if needed
        }
    }
}