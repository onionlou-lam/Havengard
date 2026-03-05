using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Character;
using Havengard.Combat;

namespace Havengard.Abilities
{
    /// <summary>
    /// Flexible channeled beam ability that can be configured for different beam types
    /// (Frost Beam, Arcane Beam, Fire Beam, etc.) through inspector settings
    /// </summary>
    [CreateAssetMenu(menuName = "Havengard/Abilities/Channeled Beam")]
    public class ChanneledBeamAbility : ChanneledAbilityBase
    {
        [Header("Beam Damage")]
        [SerializeField] private int baseDamagePerSecond = 40;
        [Tooltip("Damage multiplier at full charge (1 = no scaling, 2 = double damage at full charge)")]
        [SerializeField] private float damageScaling = 2f;
        [Tooltip("How often damage ticks are applied (lower = more frequent damage)")]
        [SerializeField] private float damageTickRate = 0.1f;
        [SerializeField] private bool friendlyFire = false;

        [Header("Beam Properties")]
        [SerializeField] private float beamMaxRange = 25f;
        [SerializeField] private LayerMask hitLayers = -1;

        // PUBLIC PROPERTY for external access
        public float BeamMaxRange => beamMaxRange;

        [Header("Full Charge Release Effect")]
        [Tooltip("Enable special effect when released at full charge")]
        [SerializeField] private bool enableFullChargeEffect = false;
        [SerializeField] private FullChargeEffectType fullChargeEffectType = FullChargeEffectType.AreaEffect;
        [Tooltip("Radius for area effects at release")]
        [SerializeField] private float fullChargeRadius = 3f;
        [Tooltip("VFX spawned at release point when fully charged")]
        [SerializeField] private GameObject fullChargeVFXPrefab;

        [Header("Continuous Effects")]
        [Tooltip("Apply status effects continuously while beam is active")]
        [SerializeField] private bool applyContinuousStatusEffects = true;

        [Header("Audio")]
        [SerializeField] private AudioClip beamLoopSFX;
        [SerializeField] private AudioClip fullChargeReleaseSFX;
        [Tooltip("Volume for beam loop sound (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float beamLoopVolume = 0.5f;

        [Header("2D Support")]
        [Tooltip("Enable for 2D games - uses screen to world point instead of raycasting")]
        public bool use2DMode = true;
        [Tooltip("Maximum beam distance in world units")]
        public float maxBeamDistance = 100f;
        [Tooltip("Layers that block the beam visually (leave as Nothing for infinite beam)")]
        public LayerMask beamBlockingLayers = 0; // ADD THIS - default to "Nothing"

        private float lastDamageTick;
        private AudioSource beamAudioSource;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return caster != null && BeamPrefab != null;
        }

        public override void OnChannelTick(GameObject caster, float chargePercent)
        {
            base.OnChannelTick(caster, chargePercent);

            // Play looping beam sound if configured
            if (beamLoopSFX != null && beamAudioSource == null)
            {
                beamAudioSource = caster.GetComponent<AudioSource>();
                if (beamAudioSource == null)
                    beamAudioSource = caster.AddComponent<AudioSource>();
                
                beamAudioSource.clip = beamLoopSFX;
                beamAudioSource.loop = true;
                beamAudioSource.volume = beamLoopVolume;
                beamAudioSource.Play();
            }

            // Apply continuous damage while channeling
            if (Time.time - lastDamageTick >= damageTickRate)
            {
                //Debug.Log($"Applying beam damage at {Time.time}, last tick was at {lastDamageTick}"); // ADD THIS
                ApplyBeamDamage(caster, chargePercent);
                lastDamageTick = Time.time;
            }
            else
            {
                //Debug.Log($"Skipping damage tick - Time.time: {Time.time}, lastDamageTick: {lastDamageTick}, difference: {Time.time - lastDamageTick}, required: {damageTickRate}"); // ADD THIS
            }
        }

        public override void OnRelease(GameObject caster, GameObject target, float chargePercent)
        {
            // Stop beam sound
            StopBeamAudio();

            // IMPORTANT: Reset lastDamageTick for next cast
            lastDamageTick = 0f;

            // Apply full charge effect if enabled and fully charged
            if (enableFullChargeEffect && chargePercent >= 0.99f)
            {
                ApplyFullChargeEffect(caster);
                
                // Play release sound
                if (fullChargeReleaseSFX != null)
                {
                    AudioSource.PlayClipAtPoint(fullChargeReleaseSFX, caster.transform.position);
                }
            }
            //Debug.Log($"Beam released at {chargePercent * 100}% charge");
        }

        public override void OnChannelCancel(GameObject caster)
        {
            base.OnChannelCancel(caster);
            lastDamageTick = 0f; // Reset for next cast
            StopBeamAudio();
            Debug.Log("Beam channeling cancelled");
        }

        private void ApplyBeamDamage(GameObject caster, float chargePercent)
        {
            // Get mouse world position
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            Vector2 beamDirection = (mouseWorld - caster.transform.position).normalized;
            Vector2 startPos = caster.transform.position;

            // VISUAL DEBUG - Draw the raycast in Scene view
            Debug.DrawRay(startPos, beamDirection * beamMaxRange, Color.red, 0.1f);

            // Raycast along beam path
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                startPos,
                beamDirection,
                beamMaxRange,
                hitLayers
            );

            //Debug.Log($"Beam damage raycast: start={startPos}, dir={beamDirection}, range={beamMaxRange}, hits={hits.Length}, hitLayers={hitLayers.value}");

            var casterHealth = caster.GetComponent<IHealth>();
            Faction casterFaction = casterHealth != null ? casterHealth.GetFaction() : Faction.Neutral;

            foreach (var hit in hits)
            {
                if (hit.collider == null)
                    continue;

                GameObject hitObject = hit.collider.gameObject;

                // Skip the caster
                if (hitObject == caster)
                {
                    Debug.Log($"Skipping caster: {hitObject.name}");
                    continue;
                }

                // Skip if this is a child of the caster (includes the beam VFX)
                Transform checkParent = hitObject.transform;
                bool isChildOfCaster = false;
                while (checkParent != null)
                {
                    if (checkParent.gameObject == caster)
                    {
                        isChildOfCaster = true;
                        break;
                    }
                    checkParent = checkParent.parent;
                }

                if (isChildOfCaster)
                {
                    Debug.Log($"Skipping caster child (beam VFX): {hitObject.name}");
                    continue;
                }

                //Debug.Log($"Beam hit: {hitObject.name} on layer {LayerMask.LayerToName(hitObject.layer)}");

                var health = hitObject.GetComponent<IHealth>();
                if (health != null && FactionUtility.CanDamage(casterFaction, health, friendlyFire))
                {
                    // Calculate damage based on charge
                    float damageMultiplier = Mathf.Lerp(1f, damageScaling, chargePercent);
                    int tickDamage = CalculateTickDamage(caster, damageMultiplier);

                    var healthSystem = health.GetHealthSystem();
                    bool targetWasAlive = healthSystem.IsAlive;

                    healthSystem.Damage(tickDamage);
                    
                    //Debug.Log($"Beam damaged {hitObject.name} for {tickDamage} damage");

                    // Generate resource per hit
                    GenerateResourceOnHit(caster);

                    // Apply lifesteal
                    ApplyLifesteal(caster, tickDamage);

                    // Check for kill
                    if (targetWasAlive && !healthSystem.IsAlive)
                    {
                        GenerateResourceOnKill(caster);
                    }

                    // Apply status effects continuously if enabled
                    if (applyContinuousStatusEffects && statusEffect != null)
                    {
                        ApplyBuffDebuff(hitObject);
                    }
                }
                else
                {
                    //Debug.Log($"Cannot damage {hitObject.name} - health: {health != null}, Can damage: {(health != null ? FactionUtility.CanDamage(casterFaction, health, friendlyFire).ToString() : "N/A")}");
                }
            }
        }

        private int CalculateTickDamage(GameObject caster, float damageMultiplier)
        {
            int tickDamage = Mathf.RoundToInt((baseDamagePerSecond * damageTickRate) * damageMultiplier);

            // Use caster's attack stat if available
            var stats = caster.GetComponent<StatsComponent>();
            if (stats != null && stats.CurrentStats != null)
            {
                float statDamage = stats.CurrentStats.Attack * damageTickRate;
                tickDamage = Mathf.RoundToInt(statDamage * damageMultiplier);
            }

            return Mathf.Max(1, tickDamage);
        }

        private void ApplyFullChargeEffect(GameObject caster)
        {
            Vector3 effectPosition = GetEffectPosition(caster);

            // Spawn VFX if configured
            if (fullChargeVFXPrefab != null)
            {
                GameObject vfx = Instantiate(fullChargeVFXPrefab, effectPosition, Quaternion.identity);
                
                // Auto-destroy after particle lifetime
                var ps = vfx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
                }
                else
                {
                    Destroy(vfx, 3f);
                }
            }

            // Apply effect based on type
            switch (fullChargeEffectType)
            {
                case FullChargeEffectType.AreaEffect:
                    ApplyAreaEffect(caster, effectPosition);
                    break;
                    
                case FullChargeEffectType.ExtraDamage:
                    ApplyExtraDamageEffect(caster, effectPosition);
                    break;
                    
                case FullChargeEffectType.StatusBurst:
                    ApplyStatusBurst(caster, effectPosition);
                    break;
            }

            Debug.Log($"{caster.name} released beam at full charge - {fullChargeEffectType} activated!");
        }

        private Vector3 GetEffectPosition(GameObject caster)
        {
            // Use mouse position as effect center
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            return mouseWorld;
        }

        private void ApplyAreaEffect(GameObject caster, Vector3 position)
        {
            // Apply status effects in an area
            Collider2D[] nearbyTargets = Physics2D.OverlapCircleAll(position, fullChargeRadius);
            
            var casterHealth = caster.GetComponent<IHealth>();
            Faction casterFaction = casterHealth != null ? casterHealth.GetFaction() : Faction.Neutral;

            foreach (var col in nearbyTargets)
            {
                if (col.gameObject == caster) continue;

                var health = col.GetComponent<IHealth>();
                if (health != null && FactionUtility.CanDamage(casterFaction, health, friendlyFire))
                {
                    ApplyBuffDebuff(col.gameObject);
                }
            }
        }

        private void ApplyExtraDamageEffect(GameObject caster, Vector3 position)
        {
            // Apply extra burst damage in an area
            Collider2D[] nearbyTargets = Physics2D.OverlapCircleAll(position, fullChargeRadius);
            
            var casterHealth = caster.GetComponent<IHealth>();
            Faction casterFaction = casterHealth != null ? casterHealth.GetFaction() : Faction.Neutral;

            int burstDamage = Mathf.RoundToInt(baseDamagePerSecond * 0.5f); // 50% of DPS as burst

            var stats = caster.GetComponent<StatsComponent>();
            if (stats != null && stats.CurrentStats != null)
            {
                burstDamage = Mathf.RoundToInt(stats.CurrentStats.Attack * 0.5f);
            }

            foreach (var col in nearbyTargets)
            {
                if (col.gameObject == caster) continue;

                var health = col.GetComponent<IHealth>();
                if (health != null && FactionUtility.CanDamage(casterFaction, health, friendlyFire))
                {
                    var healthSystem = health.GetHealthSystem();
                    bool targetWasAlive = healthSystem.IsAlive;

                    healthSystem.Damage(burstDamage);
                    ApplyLifesteal(caster, burstDamage);

                    if (targetWasAlive && !healthSystem.IsAlive)
                    {
                        GenerateResourceOnKill(caster);
                    }
                }
            }
        }

        private void ApplyStatusBurst(GameObject caster, Vector3 position)
        {
            // Apply stacked status effects in an area
            if (statusEffect == null) return;

            Collider2D[] nearbyTargets = Physics2D.OverlapCircleAll(position, fullChargeRadius);
            
            var casterHealth = caster.GetComponent<IHealth>();
            Faction casterFaction = casterHealth != null ? casterHealth.GetFaction() : Faction.Neutral;

            foreach (var col in nearbyTargets)
            {
                if (col.gameObject == caster) continue;

                var health = col.GetComponent<IHealth>();
                if (health != null && FactionUtility.CanDamage(casterFaction, health, friendlyFire))
                {
                    // Apply max stacks at once
                    for (int i = 0; i < maxStatusStacks; i++)
                    {
                        ApplyBuffDebuff(col.gameObject);
                    }
                }
            }
        }

        private void StopBeamAudio()
        {
            if (beamAudioSource != null && beamAudioSource.isPlaying)
            {
                beamAudioSource.Stop();
                beamAudioSource = null;
            }
        }

        // Optional: Draw gizmo in editor to visualize full charge radius
        private void OnDrawGizmosSelected()
        {
            if (enableFullChargeEffect)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                Gizmos.DrawWireSphere(Vector3.zero, fullChargeRadius);
            }
        }
    }

    /// <summary>
    /// Types of effects that can occur when beam is released at full charge
    /// </summary>
    public enum FullChargeEffectType
    {
        None,           // No special effect
        AreaEffect,     // Apply status effects in area
        ExtraDamage,    // Deal extra burst damage in area
        StatusBurst     // Apply maximum stacks of status effect instantly
    }
}