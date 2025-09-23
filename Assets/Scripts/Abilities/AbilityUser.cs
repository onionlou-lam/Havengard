using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Abilities
{
    [DisallowMultipleComponent]
    public class AbilityUser : MonoBehaviour
    {
        [SerializeField] private AbilityBase[] abilities;
        private readonly Dictionary<AbilityBase, float> _lastCast = new();

        public AbilityBase[] Abilities => abilities;

        public void UseAbility(int index, GameObject target)
        {
            if (abilities == null || index < 0 || index >= abilities.Length) return;
            var ability = abilities[index];
            if (ability == null) return;

            // Cooldown
            _lastCast.TryGetValue(ability, out var last);
            if (Time.time < last + ability.Cooldown) return;

            // Resource check
            if (ability.ResourceCost > 0f && TryGetComponent<IResource>(out var resource))
            {
                if (!resource.TryConsume(ability.ResourceCost)) return;
            }

            // Ability-specific check
            if (!ability.CanCast(gameObject, target)) return;

            // Cast
            ability.Execute(gameObject, target);
            _lastCast[ability] = Time.time;
        }

        /// <summary>
        /// Assign a new set of abilities at runtime (e.g. when hero is recruited).
        /// </summary>
        public void AssignAbilities(AbilityBase[] newAbilities)
        {
            abilities = newAbilities;
            _lastCast.Clear(); // reset cooldowns for new set
        }
    }
}
