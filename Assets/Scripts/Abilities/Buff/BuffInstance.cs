    using UnityEngine;
using Havengard.Core.Character;
using Havengard.Core.HealthSystem;
using System.Collections;

namespace Havengard.Abilities
{
    /// <summary>
    /// Runtime component attached to a caster when a BuffAbility is active.
    /// Manages stat modifications, VFX, duration, and cleanup.
    /// </summary>
    [DisallowMultipleComponent]
    public class BuffInstance : MonoBehaviour
    {
        private BuffAbility sourceAbility;
        private GameObject caster;
        private float duration;
        private float remainingTime;
        private BuffModifier[] modifiers;
        private bool isActive;

        // VFX references
        private GameObject persistentVFXInstance;
        private GameObject deactivationVFX;
        private AudioClip deactivationSFX;

        // Stat snapshot for restoration
        private Stats originalStats;
        private StatsComponent statsComponent;

        public BuffAbility SourceAbility => sourceAbility;
        public float RemainingTime => remainingTime;
        public bool IsActive => isActive;

        public void Initialize(BuffAbility ability, GameObject target, float buffDuration,
            BuffModifier[] buffModifiers, GameObject activationVFX, GameObject persistentVFX,
            GameObject deactivationVFX, AudioClip activationSFX, AudioClip deactivationSFX)
        {
            sourceAbility = ability;
            caster = target;
            duration = buffDuration;
            remainingTime = buffDuration;
            modifiers = buffModifiers;
            this.deactivationVFX = deactivationVFX;
            this.deactivationSFX = deactivationSFX;

            // Get stats component
            statsComponent = caster.GetComponent<StatsComponent>();
            if (statsComponent == null || statsComponent.CurrentStats == null)
            {
                Debug.LogWarning($"[BuffInstance] {caster.name} has no StatsComponent, buff will not apply");
                Destroy(this);
                return;
            }

            // Snapshot original stats for restoration
            originalStats = statsComponent.CurrentStats.Clone();

            // Apply stat modifiers
            ApplyStatModifiers();

            // Spawn activation VFX
            if (activationVFX != null)
            {
                GameObject vfx = Instantiate(activationVFX, caster.transform.position, Quaternion.identity);
                vfx.transform.SetParent(caster.transform);
                Destroy(vfx, 3f); // Cleanup after 3 seconds
            }

            // Spawn persistent VFX
            if (persistentVFX != null)
            {
                persistentVFXInstance = Instantiate(persistentVFX, caster.transform.position, Quaternion.identity);
                persistentVFXInstance.transform.SetParent(caster.transform);
            }

            // Play activation sound
            if (activationSFX != null)
            {
                AudioSource.PlayClipAtPoint(activationSFX, caster.transform.position);
            }

            isActive = true;

            // Start duration countdown for duration-based buffs
            if (sourceAbility.GetBuffType() == BuffType.Duration)
            {
                StartCoroutine(DurationCountdown());
            }

            Debug.Log($"[BuffInstance] Initialized {sourceAbility.abilityName} on {caster.name}");
        }

        /// <summary>
        /// Refreshes the buff duration (for duration-based buffs)
        /// </summary>
        public void RefreshDuration(float newDuration)
        {
            if (sourceAbility.GetBuffType() == BuffType.Duration)
            {
                remainingTime = newDuration;
                Debug.Log($"[BuffInstance] Refreshed duration to {newDuration}s");
            }
        }

        private void ApplyStatModifiers()
        {
            if (modifiers == null || modifiers.Length == 0) return;

            Stats currentStats = statsComponent.CurrentStats;

            foreach (BuffModifier mod in modifiers)
            {
                ApplySingleModifier(currentStats, mod);
            }

            // Update health system if MaxHP was modified
            if (HasModifier(BuffModifier.StatType.MaxHP))
            {
                var health = caster.GetComponent<Health>();
                if (health != null)
                {
                    health.SetMaxHealthFromStats(refill: false);
                }
            }

            Debug.Log($"[BuffInstance] Applied {modifiers.Length} stat modifiers to {caster.name}");
        }

        private void RemoveStatModifiers()
        {
            if (statsComponent == null || originalStats == null) return;

            // Restore original stats
            statsComponent.SetCurrentStats(originalStats);

            // Update health system if MaxHP was modified
            if (HasModifier(BuffModifier.StatType.MaxHP))
            {
                var health = caster.GetComponent<Health>();
                if (health != null)
                {
                    health.SetMaxHealthFromStats(refill: false);
                }
            }

            Debug.Log($"[BuffInstance] Removed stat modifiers from {caster.name}");
        }

        private void ApplySingleModifier(Stats stats, BuffModifier mod)
        {
            switch (mod.statType)
            {
                case BuffModifier.StatType.MaxHP:
                    if (mod.modifierType == BuffModifier.ModifierType.Additive)
                        stats.MaxHP += Mathf.RoundToInt(mod.value);
                    else
                        stats.MaxHP = Mathf.RoundToInt(stats.MaxHP * mod.value);
                    stats.MaxHP = Mathf.Max(1, stats.MaxHP);
                    break;

                case BuffModifier.StatType.Attack:
                    if (mod.modifierType == BuffModifier.ModifierType.Additive)
                        stats.Attack += Mathf.RoundToInt(mod.value);
                    else
                        stats.Attack = Mathf.RoundToInt(stats.Attack * mod.value);
                    stats.Attack = Mathf.Max(0, stats.Attack);
                    break;

                case BuffModifier.StatType.Defense:
                    if (mod.modifierType == BuffModifier.ModifierType.Additive)
                        stats.Defense += Mathf.RoundToInt(mod.value);
                    else
                        stats.Defense = Mathf.RoundToInt(stats.Defense * mod.value);
                    stats.Defense = Mathf.Max(0, stats.Defense);
                    break;

                case BuffModifier.StatType.MaxResource:
                    if (mod.modifierType == BuffModifier.ModifierType.Additive)
                        stats.MaxResource += Mathf.RoundToInt(mod.value);
                    else
                        stats.MaxResource = Mathf.RoundToInt(stats.MaxResource * mod.value);
                    stats.MaxResource = Mathf.Max(1, stats.MaxResource);
                    break;

                case BuffModifier.StatType.AttackSpeed:
                    if (mod.modifierType == BuffModifier.ModifierType.Additive)
                        stats.AttackSpeed += mod.value;
                    else
                        stats.AttackSpeed *= mod.value;
                    break;

                case BuffModifier.StatType.MoveSpeed:
                    if (mod.modifierType == BuffModifier.ModifierType.Additive)
                        stats.MoveSpeed += mod.value;
                    else
                        stats.MoveSpeed *= mod.value;
                    break;

                case BuffModifier.StatType.CritChance:
                    if (mod.modifierType == BuffModifier.ModifierType.Additive)
                        stats.CritChance += mod.value;
                    else
                        stats.CritChance *= mod.value;
                    break;

                case BuffModifier.StatType.CritMultiplier:
                    if (mod.modifierType == BuffModifier.ModifierType.Additive)
                        stats.CritMultiplier += mod.value;
                    else
                        stats.CritMultiplier *= mod.value;
                    break;
            }
        }

        private bool HasModifier(BuffModifier.StatType type)
        {
            if (modifiers == null) return false;
            foreach (var mod in modifiers)
            {
                if (mod.statType == type) return true;
            }
            return false;
        }

        private IEnumerator DurationCountdown()
        {
            while (remainingTime > 0f)
            {
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            // Duration expired, remove buff
            Cleanup();
            Destroy(this);
        }

        private void Cleanup()
        {
            if (!isActive) return;
            isActive = false;

            // Remove stat modifiers
            RemoveStatModifiers();

            // Destroy persistent VFX
            if (persistentVFXInstance != null)
            {
                Destroy(persistentVFXInstance);
            }

            // Spawn deactivation VFX
            if (deactivationVFX != null && caster != null)
            {
                GameObject vfx = Instantiate(deactivationVFX, caster.transform.position, Quaternion.identity);
                Destroy(vfx, 3f);
            }

            // Play deactivation sound
            if (deactivationSFX != null && caster != null)
            {
                AudioSource.PlayClipAtPoint(deactivationSFX, caster.transform.position);
            }

            Debug.Log($"[BuffInstance] Cleaned up {sourceAbility.abilityName} from {caster.name}");
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void Update()
        {
            // Keep persistent VFX following the caster
            if (persistentVFXInstance != null && caster != null)
            {
                persistentVFXInstance.transform.position = caster.transform.position;
            }
        }
    }
}