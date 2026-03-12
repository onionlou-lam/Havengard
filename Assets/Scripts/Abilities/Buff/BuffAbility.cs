using UnityEngine;
using Havengard.Core.Character;
using Havengard.Core.HealthSystem;

namespace Havengard.Abilities
{
    /// <summary>
    /// Buff ability that can modify stats either temporarily or persistently when toggled.
    /// Supports both duration-based buffs and toggle-on/off buffs.
    /// </summary>
    [CreateAssetMenu(menuName = "Havengard/Abilities/Buff Ability")]
    public class BuffAbility : AbilityBase
    {
        [Header("Buff Type")]
        [Tooltip("Duration-based: Applies buff for a set time. Toggle: Persists until toggled off.")]
        public BuffType buffType = BuffType.Duration;

        [Tooltip("Duration in seconds (only for Duration type)")]
        [SerializeField] private float duration = 5f;

        [Tooltip("Can cast while buff is active to refresh/cancel (Duration) or toggle off (Toggle)")]
        [SerializeField] private bool canRefreshOrToggle = true;

        [Header("Stat Modifiers")]
        [Tooltip("Stat modifications applied by this buff")]
        [SerializeField] private BuffModifier[] statModifiers;

        [Header("Visual Effects")]
        [Tooltip("Particle effect spawned on the caster when buff activates")]
        [SerializeField] private GameObject activationVFX;

        [Tooltip("Particle effect that follows the caster while buff is active")]
        [SerializeField] private GameObject persistentVFX;

        [Tooltip("Particle effect spawned when buff ends")]
        [SerializeField] private GameObject deactivationVFX;

        [Header("Audio")]
        [SerializeField] private AudioClip activationSFX;
        [SerializeField] private AudioClip deactivationSFX;

        // REMOVED 'override' - this method doesn't exist in AbilityBase
        public bool CanCast(GameObject caster, GameObject target)
        {
            if (caster == null) return false;

            // Check if buff instance already exists on caster
            BuffInstance existingBuff = caster.GetComponent<BuffInstance>();

            // If buff exists and we can't refresh/toggle, prevent casting
            if (existingBuff != null && existingBuff.SourceAbility == this)
            {
                return canRefreshOrToggle;
            }

            return true;
        }

        // REMOVED 'override' - this method doesn't exist in AbilityBase
        public void Cast(GameObject caster, GameObject target)
        {
            if (caster == null) return;

            // REMOVED: GenerateResourceOnCast(caster); - method doesn't exist

            // Check for existing buff instance
            BuffInstance existingBuff = caster.GetComponent<BuffInstance>();

            if (existingBuff != null && existingBuff.SourceAbility == this)
            {
                // Handle refresh/toggle logic
                if (buffType == BuffType.Duration && canRefreshOrToggle)
                {
                    // Refresh duration
                    existingBuff.RefreshDuration(duration);
                    Debug.Log($"[BuffAbility] Refreshed {abilityName} on {caster.name}");
                }
                else if (buffType == BuffType.Toggle && canRefreshOrToggle)
                {
                    // Toggle off
                    Destroy(existingBuff);
                    Debug.Log($"[BuffAbility] Toggled off {abilityName} on {caster.name}");
                }
                return;
            }

            // Create new buff instance
            BuffInstance buffInstance = caster.AddComponent<BuffInstance>();
            buffInstance.Initialize(this, caster, duration, statModifiers,
                activationVFX, persistentVFX, deactivationVFX,
                activationSFX, deactivationSFX);

            Debug.Log($"[BuffAbility] Applied {abilityName} to {caster.name}");
        }

        // ADD: Implement abstract methods from AbilityBase
        public override void Activate(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            Cast(user.gameObject, targetEnemy);
        }

        public override void Deactivate(AbilityUser user)
        {
            // Buff cleanup is handled by BuffInstance component
        }

        /// <summary>
        /// Gets the buff type (Duration or Toggle)
        /// </summary>
        public BuffType GetBuffType() => buffType;

        /// <summary>
        /// Gets the buff duration (only relevant for Duration type)
        /// </summary>
        public float GetDuration() => duration;

        /// <summary>
        /// Gets a copy of the stat modifiers array
        /// </summary>
        public BuffModifier[] GetStatModifiers()
        {
            if (statModifiers == null) return new BuffModifier[0];
            BuffModifier[] copy = new BuffModifier[statModifiers.Length];
            System.Array.Copy(statModifiers, copy, statModifiers.Length);
            return copy;
        }
    }

    public enum BuffType
    {
        Duration,   // Temporary buff with duration
        Toggle      // Persistent buff until toggled off
    }

    [System.Serializable]
    public struct BuffModifier
    {
        [Tooltip("The stat to modify")]
        public StatType statType;

        [Tooltip("Whether this is an additive (+10) or multiplicative (x1.5) modifier")]
        public ModifierType modifierType;

        [Tooltip("Amount to add (additive) or multiply by (multiplicative). Example: 1.5 = +50% for multiplicative")]
        public float value;

        public enum StatType
        {
            MaxHP,
            Attack,
            Defense,
            MaxResource,
            AttackSpeed,
            MoveSpeed,
            CritChance,
            CritMultiplier
        }

        public enum ModifierType
        {
            Additive,       // Adds flat value
            Multiplicative  // Multiplies by percentage (1.5 = +50%)
        }
    }
}