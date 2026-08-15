using UnityEngine;

namespace Havengard.Abilities
{
    public abstract class ChanneledAbilityBase : AbilityBase
    {
        [Header("Channeling Settings")]
        [SerializeField] protected float channelDuration = 3f;
        [SerializeField] protected float tickRate = 0.2f;
        [SerializeField] protected bool canMoveWhileChanneling = false;
        [SerializeField] protected bool preventMovement = true;
        [SerializeField] protected float minReleasePercent = 0.3f;
        [SerializeField] protected bool allowPartialRelease = true;

        [Header("VFX")]
        [SerializeField] protected GameObject chargingVFXPrefab;
        [SerializeField] protected GameObject beamPrefab;

        [Header("Channeled Resource Generation")]
        [Tooltip("Generate resource per tick while channeling")]
        public int resourcePerTick = 0;
        [Tooltip("Generate resource based on damage dealt per tick")]
        [Range(0f, 1f)]
        public float resourcePercentPerTick = 0f;

        protected bool isChanneling;
        protected float channelStartTime;
        protected float lastTickTime;

        // Public properties for ChannelController
        public float MaxChargeTime => channelDuration;
        public float TickRate => tickRate;
        public bool PreventMovement => preventMovement;
        public float MinReleasePercent => minReleasePercent;
        public bool AllowPartialRelease => allowPartialRelease;
        public GameObject ChargingVFXPrefab => chargingVFXPrefab;
        public GameObject BeamPrefab => beamPrefab;

        // For ChannelController compatibility
        public virtual bool CanCast(GameObject caster, GameObject target)
        {
            if (caster == null) return false;

            // Check resource cost
            var resourceSystem = caster.GetComponent<ResourceSystem>();
            if (resourceSystem != null && resourceCost > 0)
            {
                if (!resourceSystem.HasResource(resourceCost))
                    return false;
            }

            return true;
        }

        public override void Activate(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            if (user == null) return;

            isChanneling = true;
            channelStartTime = Time.time;
            lastTickTime = Time.time;

            Debug.Log($"[ChanneledAbilityBase] Started channeling {abilityName}");
        }

        // For ChannelController - different signature!
        public virtual void OnChannelTick(GameObject caster, float chargePercent)
        {
            // Override in derived classes
            // This is called from ChannelController.Update()
        }

        // For AbilityUser/other systems
        public void UpdateChannel(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            if (!isChanneling) return;

            float elapsed = Time.time - channelStartTime;

            if (elapsed >= channelDuration)
            {
                StopChannel(user, targetPosition, targetEnemy);
                return;
            }

            if (Time.time >= lastTickTime + tickRate)
            {
                OnChannelTick(user, targetPosition, targetEnemy);
                lastTickTime = Time.time;
            }
        }

        // For AbilityUser - original signature
        protected virtual void OnChannelTick(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            // Override in derived classes
        }

        public void StopChannel(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            if (!isChanneling) return;

            float channelTime = Time.time - channelStartTime;
            isChanneling = false;

            OnRelease(user.gameObject, targetEnemy, channelTime);
            Deactivate(user);
        }

        public virtual void OnChannelCancel(GameObject caster)
        {
            isChanneling = false;
        }

        public override void Deactivate(AbilityUser user)
        {
            isChanneling = false;
        }

        protected virtual void OnRelease(GameObject caster, GameObject target, float channelTime)
        {
            // Override in derived classes
        }

        public bool IsChanneling => isChanneling;
        public float ChannelProgress => isChanneling ? (Time.time - channelStartTime) / channelDuration : 0f;
    }
}