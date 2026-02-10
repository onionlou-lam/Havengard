using Havengard.Statuses;
using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Abilities
{
    /// <summary>
    /// Base ScriptableObject class for all abilities. 
    /// Implements IAbility and adds shared ability behavior.
    /// </summary>
    public abstract class AbilityBase : ScriptableObject, IAbility
    {
        [Header("General Settings")]
        [SerializeField] private string abilityName = "New Ability";
        [SerializeField] private float cooldown = 1f;
        [SerializeField] private int resourceCost = 0;
        [SerializeField] private Sprite icon;

        [Header("Resource Generation")]
        [Tooltip("Amount of resource generated on cast (before hit)")]
        [SerializeField] private int resourceGeneratedOnCast = 0;
        [Tooltip("Amount of resource generated per enemy hit")]
        [SerializeField] private int resourceGeneratedPerHit = 0;
        [Tooltip("Amount of resource generated on kill")]
        [SerializeField] private int resourceGeneratedOnKill = 0;

        [Header("Buff/Debuff Config (Optional)")]
        [SerializeField] protected StatusEffectData statusEffect;
        [SerializeField] protected int maxStatusStacks = 1;

        [Header("Lifesteal")]
        [Tooltip("Percentage of damage dealt converted to healing (0.0 to 1.0). Example: 0.15 = 15% lifesteal")]
        [Range(0f, 1f)]
        [SerializeField] private float lifestealPercent = 0f;

        public string AbilityName => abilityName;
        public float Cooldown => cooldown;
        public int ResourceCost => resourceCost;
        public Sprite Icon => icon;
        
        // Resource generation properties
        public int ResourceGeneratedOnCast => resourceGeneratedOnCast;
        public int ResourceGeneratedPerHit => resourceGeneratedPerHit;
        public int ResourceGeneratedOnKill => resourceGeneratedOnKill;
        public float LifestealPercent => lifestealPercent;

        // Provide default handling of buff/debuff logic
        protected void ApplyBuffDebuff(GameObject target)
        {
            if (statusEffect != null)
            {
                StatusEffectApplier.ApplyEffect(target, statusEffect, maxStatusStacks);
            }
        }

        /// <summary>
        /// Generates resource on cast. Call this at the start of Cast().
        /// </summary>
        protected void GenerateResourceOnCast(GameObject caster)
        {
            if (resourceGeneratedOnCast > 0)
            {
                var resource = caster.GetComponent<IResource>();
                if (resource != null)
                {
                    resource.AddResource(resourceGeneratedOnCast);
                }
            }
        }

        /// <summary>
        /// Generates resource per hit. Call this after hitting a target.
        /// </summary>
        protected void GenerateResourceOnHit(GameObject caster)
        {
            if (resourceGeneratedPerHit > 0)
            {
                var resource = caster.GetComponent<IResource>();
                if (resource != null)
                {
                    resource.AddResource(resourceGeneratedPerHit);
                }
            }
        }

        /// <summary>
        /// Generates resource on kill. Call this when a target dies from this ability.
        /// </summary>
        protected void GenerateResourceOnKill(GameObject caster)
        {
            if (resourceGeneratedOnKill > 0)
            {
                var resource = caster.GetComponent<IResource>();
                if (resource != null)
                {
                    resource.AddResource(resourceGeneratedOnKill);
                }
            }
        }

        /// <summary>
        /// Applies lifesteal healing to the caster based on damage dealt.
        /// Call this after dealing damage to a target.
        /// </summary>
        protected void ApplyLifesteal(GameObject caster, int damageDealt)
        {
            if (lifestealPercent <= 0f || damageDealt <= 0) return;

            int healAmount = Mathf.RoundToInt(damageDealt * lifestealPercent);
            if (healAmount <= 0) return;

            var health = caster.GetComponent<IHealth>();
            if (health != null)
            {
                health.GetHealthSystem().Heal(healAmount);
                Debug.Log($"[Lifesteal] {caster.name} healed {healAmount} HP from dealing {damageDealt} damage");
            }
        }

        // Implementors must override these methods for custom ability behavior
        public abstract bool CanCast(GameObject caster, GameObject target);
        public abstract void Cast(GameObject caster, GameObject target);
    }
}