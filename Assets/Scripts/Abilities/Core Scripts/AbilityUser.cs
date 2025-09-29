using System;
using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Abilities
{
    public class AbilityUser : MonoBehaviour
    {
        [SerializeField] private List<AbilityBase> abilities = new List<AbilityBase>();
        private float[] cooldownTimers;

        private void Awake()
        {
            cooldownTimers = new float[abilities.Count];
        }

        private void Update()
        {
            for (int i = 0; i < cooldownTimers.Length; i++)
            {
                if (cooldownTimers[i] > 0f)
                    cooldownTimers[i] -= Time.deltaTime;
            }
        }

        public AbilityBase GetAbility(int index)
        {
            if (index >= 0 && index < abilities.Count)
                return abilities[index];
            return null;
        }
        public void AddAbility(AbilityBase newAbility)
        {
            if (newAbility == null) return;
            if (!abilities.Contains(newAbility))
            {
                abilities.Add(newAbility);
                Array.Resize(ref cooldownTimers, abilities.Count);
            }
        }

        public void UseAbility(int index, GameObject target)
        {
            if (index < 0 || index >= abilities.Count) return;

            AbilityBase ability = abilities[index];
            if (ability == null) return;

            if (cooldownTimers[index] > 0f) return;
            if (!ability.CanCast(gameObject, target)) return;

            ability.Cast(gameObject, target);
            cooldownTimers[index] = ability.Cooldown;
        }
        public void AssignAbilities(List<AbilityBase> newAbilities)
        {
            abilities = newAbilities;
            cooldownTimers = new float[abilities.Count];
        }

        public void AssignAbilities(AbilityBase[] newAbilities)
        {
            abilities = new List<AbilityBase>(newAbilities);
            cooldownTimers = new float[abilities.Count];
        }
    }
}
