using UnityEngine;
using Havengard.Abilities;

namespace Havengard.Items
{
    public enum AbilityModType
    {
        AdditionalProjectiles,
        IncreasedAOE,
        IncreasedDuration,
        IncreasedDamage,
        ReducedCooldown,
        IncreasedRange,
        AdditionalTargets,
        ChainEffect
    }

    [CreateAssetMenu(menuName = "Havengard/Items/Effects/Ability Modifier")]
    public class AbilityModifierEffect : ItemEffect
    {
        [Header("Ability Modifier")]
        public AbilityModType modType;
        [Tooltip("Which ability to modify (leave empty for all)")]
        public AbilityBase targetAbility;

        private const string MODIFIER_KEY_PREFIX = "ItemMod_";

        public override void Apply(GameObject character, int level)
        {
            var abilityUser = character.GetComponent<AbilityUser>();
            if (abilityUser == null) return;

            string modifierKey = GetModifierKey();
            float value = GetValue(level);

            // Store modifier in AbilityUser (you'll need to add this functionality)
            // For now, we'll use PlayerPrefs as a temporary solution
            float currentValue = PlayerPrefs.GetFloat(modifierKey, 0f);
            PlayerPrefs.SetFloat(modifierKey, currentValue + value);
        }

        public override void Remove(GameObject character, int level)
        {
            var abilityUser = character.GetComponent<AbilityUser>();
            if (abilityUser == null) return;

            string modifierKey = GetModifierKey();
            float value = GetValue(level);

            float currentValue = PlayerPrefs.GetFloat(modifierKey, 0f);
            PlayerPrefs.SetFloat(modifierKey, Mathf.Max(0, currentValue - value));
        }

        private string GetModifierKey()
        {
            string abilityName = targetAbility != null ? targetAbility.name : "All";
            return $"{MODIFIER_KEY_PREFIX}{abilityName}_{modType}";
        }

        public override string FormatDescription(string desc, int level)
        {
            float value = GetValue(level);
            return desc.Replace($"{{{modType}}}", $"{value:F0}");
        }
    }
}