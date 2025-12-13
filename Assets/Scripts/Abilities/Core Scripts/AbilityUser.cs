// Assets/Scripts/Abilities/Core Scripts/AbilityUser.cs
using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Abilities
{
    /// <summary>
    /// Component that owns and casts abilities.
    /// Handles cooldowns and resource consumption (via IResource).
    /// </summary>
    [DisallowMultipleComponent]
    public class AbilityUser : MonoBehaviour
    {
        [Header("Ability Slots")]
        [SerializeField] private List<AbilityBase> abilities = new();   // 0..N ability slots

        [Header("Resource")]
        [Tooltip("Optional. If null, will try GetComponent<IResource>() on this GameObject.")]
        [SerializeField] private MonoBehaviour resourceComponent;       // must implement IResource
        private IResource resource;

        [Header("Cooldowns")]
        [SerializeField] private bool useGlobalCooldown = false;
        [SerializeField] private float globalCooldownDuration = 0.1f;

        private float[] nextReadyTimes;
        private float globalCooldownEndTime;

        private void Awake()
        {
            // Get resource component (e.g. ResourceSystem)
            if (resourceComponent != null)
                resource = resourceComponent as IResource;

            if (resource == null)
                resource = GetComponent<IResource>();

            if (abilities == null)
                abilities = new List<AbilityBase>();

            RebuildCooldownArray();
        }

        private void OnValidate()
        {
            if (abilities == null)
                abilities = new List<AbilityBase>();

            if (!Application.isPlaying)
                RebuildCooldownArray();
        }

        private void RebuildCooldownArray()
        {
            nextReadyTimes = (abilities != null && abilities.Count > 0)
                ? new float[abilities.Count]
                : new float[0];
        }

        // ------------------------------
        // Assigning & querying abilities
        // ------------------------------

        public void AssignAbilities(List<AbilityBase> list)
        {
            abilities = list ?? new List<AbilityBase>();
            RebuildCooldownArray();
        }

        public void AssignAbilities(AbilityBase[] array)
        {
            abilities = array != null ? new List<AbilityBase>(array) : new List<AbilityBase>();
            RebuildCooldownArray();
        }

        public AbilityBase GetAbility(int index)
        {
            if (abilities == null) return null;
            if (index < 0 || index >= abilities.Count) return null;
            return abilities[index];
        }
        public void AddAbility(AbilityBase ability)
        {
            if (ability == null) return;

            if (abilities == null)
                abilities = new List<AbilityBase>();

            abilities.Add(ability);
            RebuildCooldownArray(); // ensure cooldown array matches new list size
        }

        // ------------------------------
        // Casting
        // ------------------------------

        /// <summary>
        /// Attempts to use an ability in a given slot on a target.
        /// Returns true if cast succeeded.
        /// </summary>
        public bool UseAbility(int index, GameObject target)
        {
            var ability = GetAbility(index);
            if (ability == null) return false;

            float now = Time.time;

            // Global cooldown
            if (useGlobalCooldown && now < globalCooldownEndTime)
                return false;

            // Per-ability cooldown
            if (nextReadyTimes != null &&
                index >= 0 &&
                index < nextReadyTimes.Length &&
                now < nextReadyTimes[index])
            {
                return false;
            }

            // Resource check
            if (resource != null && ability.ResourceCost > 0)
            {
                if (!resource.TryConsume(ability.ResourceCost))
                {
                    Debug.Log($"{name} tried to cast {ability.AbilityName} but didn't have enough resource.");
                    return false;
                }
            }

            // Any extra CanCast logic defined in the ability
            if (!ability.CanCast(gameObject, target))
                return false;

            // Actually cast
            ability.Cast(gameObject, target);

            // Apply cooldown
            if (nextReadyTimes != null &&
                index >= 0 &&
                index < nextReadyTimes.Length)
            {
                nextReadyTimes[index] = now + ability.Cooldown;
            }

            // Global cooldown
            if (useGlobalCooldown)
                globalCooldownEndTime = now + globalCooldownDuration;

            return true;
        }
    }
}
