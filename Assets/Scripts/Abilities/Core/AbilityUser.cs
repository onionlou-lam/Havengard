using System.Collections.Generic;
using UnityEngine;
using System;

namespace Havengard.Abilities
{
    [DisallowMultipleComponent]
    public class AbilityUser : MonoBehaviour
    {
        [Header("Ability Slots")]
        [SerializeField] private List<AbilityBase> abilities = new List<AbilityBase>();

        [Header("Resource")]
        [Tooltip("Optional. If null, will try GetComponent<ResourceSystem>() on this GameObject.")]
        [SerializeField] private ResourceSystem resourceSystem;

        [Header("Cooldowns")]
        [SerializeField] private bool useGlobalCooldown = false;
        [SerializeField] private float globalCooldownDuration = 0.1f;

        private float[] nextReadyTimes;
        private float globalCooldownEndTime;

        public event Action<int, AbilityBase> OnAbilityUsed;
        public event Action<int, float> OnAbilityCooldownStarted;

        private void Awake()
        {
            if (resourceSystem == null)
                resourceSystem = GetComponent<ResourceSystem>();

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
            RebuildCooldownArray();
        }

        public float GetRemainingCooldown(int index)
        {
            if (nextReadyTimes == null || index < 0 || index >= nextReadyTimes.Length)
                return 0f;

            float remaining = nextReadyTimes[index] - Time.time;
            return Mathf.Max(0f, remaining);
        }

        public bool IsOnCooldown(int index)
        {
            return GetRemainingCooldown(index) > 0f;
        }

        public bool UseAbility(int index, GameObject target)
        {
            var ability = GetAbility(index);
            if (ability == null) return false;

            float now = Time.time;

            if (useGlobalCooldown && now < globalCooldownEndTime)
                return false;

            if (nextReadyTimes != null &&
                index >= 0 &&
                index < nextReadyTimes.Length &&
                now < nextReadyTimes[index])
            {
                return false;
            }

            // Resource check
            if (resourceSystem != null && ability.resourceCost > 0)
            {
                if (!resourceSystem.HasResource(ability.resourceCost))
                    return false;

                if (!resourceSystem.SpendResource(ability.resourceCost))
                    return false;
            }

            // Cast the ability
            Vector3 targetPos = target != null ? target.transform.position : transform.position;
            ability.Activate(this, targetPos, target);

            // Start cooldowns
            if (nextReadyTimes != null && index >= 0 && index < nextReadyTimes.Length)
            {
                nextReadyTimes[index] = now + ability.baseCooldown;
                OnAbilityCooldownStarted?.Invoke(index, ability.baseCooldown);
            }

            if (useGlobalCooldown)
            {
                globalCooldownEndTime = now + globalCooldownDuration;
            }

            OnAbilityUsed?.Invoke(index, ability);
            return true;
        }

        public bool UseAbility(int index, Vector3 targetPosition)
        {
            return UseAbility(index, null);
        }

        public List<AbilityBase> GetAllAbilities()
        {
            return new List<AbilityBase>(abilities);
        }

        public int GetAbilityCount()
        {
            return abilities != null ? abilities.Count : 0;
        }
    }
}