using Havengard.Core;
using Havengard.Statuses;
using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Zone Ability")]
    public class ZoneAbility : AbilityBase
    {
        [Header("Zone Properties")]
        [SerializeField] protected GameObject zonePrefab;
        [SerializeField] private float maximumRange = 15f;
        [SerializeField] private bool followsCaster = false;

        [Header("Zone Status Effect")]
        [SerializeField] private StatusEffectData statusEffect;
        [SerializeField] private int maxStatusStacks = 1;

        [Header("Zone Resource Generation")]
        [Tooltip("Generate resource per tick while zone is active")]
        public int resourcePerTick = 0;
        [Tooltip("Generate resource based on damage dealt per tick")]
        [Range(0f, 1f)]
        public float resourcePercentPerTick = 0f;

        protected GameObject ZonePrefab => zonePrefab;
        protected bool FollowsCaster => followsCaster;

        public bool CanCast(GameObject caster, GameObject target)
        {
            if (zonePrefab == null) return false;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            return Vector3.Distance(caster.transform.position, mouseWorld) <= maximumRange;
        }

        public virtual void Cast(GameObject caster, GameObject target)
        {
            if (zonePrefab == null || !CanCast(caster, target))
                return;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            Vector3 targetPosition =
                followsCaster
                ? caster.transform.position
                : mouseWorld;

            GameObject zoneInstance =
                Instantiate(zonePrefab, targetPosition, Quaternion.identity);

            ZoneEffect zoneEffect =
                zoneInstance.GetComponent<ZoneEffect>();

            if (zoneEffect != null)
            {
                zoneEffect.Initialize(
                    caster,
                    followsCaster,
                    statusEffect,
                    maxStatusStacks);
            }
        }

        public override void Activate(
            AbilityUser user,
            Vector3 targetPosition,
            GameObject targetEnemy)
        {
            Cast(user.gameObject, targetEnemy);
        }

        public override void Deactivate(AbilityUser user)
        {
        }
    }
}