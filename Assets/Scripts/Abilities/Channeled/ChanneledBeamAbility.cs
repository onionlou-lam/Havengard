using Havengard.Combat;
using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Channeled/Beam Ability")]
    public class ChanneledBeamAbility : ChanneledAbilityBase
    {
        [Header("Beam Configuration")]
        [SerializeField] private float beamWidth = 1f;
        [SerializeField] private float beamMaxDistance = 15f;
        [SerializeField] private LayerMask hitLayers;

        [Header("Beam Pulsing (uses MagicBeamScript settings)")]
        [Tooltip("MagicBeamScript on beam prefab handles pulsing automatically")]
        [SerializeField] private bool useBeamPulsing = true;
        [Tooltip("Info: Pulse frequency and width are controlled by the beam prefab's MagicBeamScript component")]
        [SerializeField] private string pulseInfo = "Configure pulse in beam prefab";

        [Header("Damage Scaling")]
        [Tooltip("Damage is multiplied by this per tick (prevents beam from being too strong)")]
        [SerializeField] private float damagePerTickMultiplier = 0.3f;
        [Tooltip("If true, damage ramps up the longer you channel (0% to 100%)")]
        [SerializeField] private bool rampDamageWithCharge = true;
        [Tooltip("Minimum damage percent at start of channel (if ramping enabled)")]
        [Range(0f, 1f)]
        [SerializeField] private float minDamagePercent = 0.4f;

        [Header("Tick Control")]
        [Tooltip("Minimum time between damage ticks (overrides base tickRate if higher)")]
        [SerializeField] private float minTimeBetweenTicks = 0.5f;

        [Header("Damage Type Specific Effects")]
        [SerializeField] private float frostSlowPercent = 0.3f;
        [SerializeField] private float fireBurnDamagePerTick = 5f;
        [SerializeField] private int lightningChainCount = 2;
        [SerializeField] private float holyHealingBonus = 1.2f;
        [SerializeField] private float arcaneManaBurnPercent = 0.2f; // NEW
        [SerializeField] private float arcaneVulnerabilityPercent = 0.15f; // NEW
        [SerializeField] private float arcaneVulnerabilityDuration = 3f; // NEW

        // Public property for range sync
        public float BeamMaxRange => beamMaxDistance;

        private float lastChargePercent = 0f;
        private float lastDamageTick = 0f;

        // Called by AbilityUser Update loop
        public override void OnChannelTick(GameObject caster, float chargePercent)
        {
            if (caster == null) return;

            lastChargePercent = chargePercent;

            // Enforce minimum tick interval for damage
            if (Time.time - lastDamageTick < minTimeBetweenTicks)
            {
                return; // Skip this tick (visuals still update)
            }

            lastDamageTick = Time.time;

            // Get mouse position in world space
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            
            Vector3 direction = (mousePos - caster.transform.position).normalized;
            
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                caster.transform.position,
                direction,
                beamMaxDistance,
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

        // For compatibility with other systems
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
                beamMaxDistance,
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
            // Check faction filtering
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
                // Calculate base damage from ability stats
                float baseDmg = CalculateDamage(caster);

                // Apply per-tick multiplier (makes beam damage controllable)
                float tickDamage = baseDmg * damagePerTickMultiplier;

                // Optionally ramp damage based on charge percent
                if (rampDamageWithCharge)
                {
                    float damageScale = Mathf.Lerp(minDamagePercent, 1f, chargePercent);
                    tickDamage *= damageScale;
                }

                int finalDamage = Mathf.RoundToInt(tickDamage);

                health.TakeDamage(finalDamage, caster);

                // Apply damage-type specific effects
                ApplyDamageTypeEffects(target, caster, chargePercent);
            }
        }

        private void ApplyDamageTypeEffects(GameObject target, GameObject caster, float chargePercent)
        {
            switch (damageType)
            {
                case DamageType.Arcane:
                    // Mana burn (if enemy has resource system)
                    var resourceSys = target.GetComponent<ResourceSystem>();
                    if (resourceSys != null)
                    {
                        float burnAmount = resourceSys.CurrentResource * arcaneManaBurnPercent;
                        resourceSys.SpendResource((int)burnAmount);
                    }

                    // Apply vulnerability debuff
                    var statusApplier = target.GetComponent<StatusEffectApplier>();
                    if (statusApplier != null)
                    {
                        // Apply "Arcane Vulnerability" status (you'd need to create this ScriptableObject)
                        // statusApplier.ApplyStatusEffect(arcaneVulnerabilityStatus, caster);
                    }
                    
                    Debug.Log($"[Arcane] Burned mana and applied vulnerability to {target.name}");
                    break;

                // ... other cases
            }
        }

        public override void Deactivate(AbilityUser user)
        {
            base.Deactivate(user);
            lastChargePercent = 0f;
            lastDamageTick = 0f;
        }
    }
}