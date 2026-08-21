using Havengard.Combat;
using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Channeled/Beam Ability")]
    public class ChanneledBeamAbility : ChanneledAbilityBase
    {
        [Header("Beam Visual Configuration")]
        [Tooltip("Reference to BeamConfig asset for visual settings")]
        [SerializeField] private BeamConfig beamConfig;

        [Header("Gameplay Configuration")]
        [SerializeField] private LayerMask hitLayers;

        [Header("Damage Scaling")]
        [Tooltip("Damage is multiplied by this per tick")]
        [SerializeField] private float damagePerTickMultiplier = 0.3f;
        [Tooltip("If true, damage ramps up the longer you channel")]
        [SerializeField] private bool rampDamageWithCharge = true;
        [Range(0f, 1f)]
        [SerializeField] private float minDamagePercent = 0.4f;

        [Header("Tick Control")]
        [Tooltip("Minimum time between damage ticks")]
        [SerializeField] private float minTimeBetweenTicks = 0.5f;

        [Header("Damage Type Specific Effects")]
        [SerializeField] private float frostSlowPercent = 0.3f;
        [SerializeField] private float fireBurnDamagePerTick = 5f;
        [SerializeField] private int lightningChainCount = 2;
        [SerializeField] private float holyHealingBonus = 1.2f;
        [SerializeField] private float arcaneManaBurnPercent = 0.2f;
        [SerializeField] private float arcaneVulnerabilityPercent = 0.15f;
        [SerializeField] private float arcaneVulnerabilityDuration = 3f;

        // Expose beam range from BeamConfig
        public float BeamMaxRange => beamConfig != null ? beamConfig.maxBeamDistance : 15f;

        private float lastChargePercent = 0f;
        private float lastDamageTick = 0f;

        public override void OnChannelTick(GameObject caster, float chargePercent)
        {
            if (caster == null) return;

            lastChargePercent = chargePercent;

            if (Time.time - lastDamageTick < minTimeBetweenTicks)
            {
                return;
            }

            lastDamageTick = Time.time;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            
            Vector3 direction = (mousePos - caster.transform.position).normalized;
            
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                caster.transform.position,
                direction,
                BeamMaxRange,
                hitLayers
            );

            foreach (var hit in hits)
            {
                if (hit.collider == null || hit.collider.gameObject == caster)
                {
                    continue;
                }

                GameObject target = hit.collider.gameObject;
                ApplyDamage(target, caster, chargePercent);
            }
        }

        protected override void OnChannelTick(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            if (user == null) return;

            if (Time.time - lastDamageTick < minTimeBetweenTicks)
            {
                return;
            }

            lastDamageTick = Time.time;

            Vector3 direction = (targetPosition - user.transform.position).normalized;
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                user.transform.position,
                direction,
                BeamMaxRange,
                hitLayers
            );

            foreach (var hit in hits)
            {
                if (hit.collider == null || hit.collider.gameObject == user.gameObject)
                    continue;

                GameObject target = hit.collider.gameObject;
                ApplyDamage(target, user.gameObject, lastChargePercent);
            }
        }

        private void ApplyDamage(GameObject target, GameObject caster, float chargePercent)
        {
            var casterHealth = caster.GetComponent<Core.HealthSystem.IHealth>();
            var targetHealth = target.GetComponent<Core.HealthSystem.IHealth>();

            if (casterHealth != null && targetHealth != null)
            {
                bool canDamage = FactionUtility.CanDamage(
                    casterHealth.GetFaction(), 
                    targetHealth, 
                    false
                );
                
                if (!canDamage)
                {
                    return;
                }
            }

            var health = target.GetComponent<Core.HealthSystem.Health>();
            if (health != null)
            {
                float baseDmg = CalculateDamage(caster);
                float tickDamage = baseDmg * damagePerTickMultiplier;

                if (rampDamageWithCharge)
                {
                    float damageScale = Mathf.Lerp(minDamagePercent, 1f, chargePercent);
                    tickDamage *= damageScale;
                }

                int finalDamage = Mathf.RoundToInt(tickDamage);
                health.TakeDamage(finalDamage, caster);

                ApplyDamageTypeEffects(target, caster, chargePercent);
            }
        }

        private void ApplyDamageTypeEffects(GameObject target, GameObject caster, float chargePercent)
        {
            switch (damageType)
            {
                case DamageType.Arcane:
                    var resourceSys = target.GetComponent<ResourceSystem>();
                    if (resourceSys != null)
                    {
                        float burnAmount = resourceSys.CurrentResource * arcaneManaBurnPercent;
                        resourceSys.SpendResource((int)burnAmount);
                    }

                    var statusApplier = target.GetComponent<StatusEffectApplier>();
                    if (statusApplier != null)
                    {
                        // Apply status effect here
                    }
                    
                    Debug.Log($"[Arcane] Burned mana and applied vulnerability to {target.name}");
                    break;
            }
        }

        public override void Deactivate(AbilityUser user)
        {
            base.Deactivate(user);
            lastChargePercent = 0f;
            lastDamageTick = 0f;
        }

        /// <summary>
        /// Get the BeamConfig for this ability
        /// </summary>
        public BeamConfig GetBeamConfig()
        {
            return beamConfig;
        }
    }
}