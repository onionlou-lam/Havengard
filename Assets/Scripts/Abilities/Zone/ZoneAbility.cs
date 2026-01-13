using Havengard.Core;
using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Zone Ability")]
    public class ZoneAbility : AbilityBase
    {
        [Header("Zone Properties")]
        [SerializeField] private GameObject zonePrefab; // Prefab contains all VFX
        [SerializeField] private float maximumRange = 15f;
        [SerializeField] private bool followsCaster = false;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            if (zonePrefab == null) return false;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            return Vector3.Distance(caster.transform.position, mouseWorld) <= maximumRange;
        }

        public override void Cast(GameObject caster, GameObject target)
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
    }
}