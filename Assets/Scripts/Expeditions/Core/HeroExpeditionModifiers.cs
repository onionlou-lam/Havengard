using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Expeditions
{
    /// <summary>
    /// A single stackable expedition bonus, sourced from an item, trait, ability, etc.
    /// Multiple modifiers from different sources can be applied/removed independently.
    /// </summary>
    [System.Serializable]
    public class ExpeditionModifier
    {
        /// <summary>
        /// What granted this modifier (item name, trait name, etc.) - used to remove it later.
        /// </summary>
        public string source;

        /// <summary>
        /// Reduces expedition duration by this many days (flat, clamped so duration never drops below 1).
        /// </summary>
        public int durationReductionDays;

        /// <summary>
        /// Bonus gold reward multiplier (0.5 = +50% gold).
        /// </summary>
        public float goldRewardMultiplier;

        /// <summary>
        /// Bonus celestium reward multiplier (1f = +100% celestium).
        /// </summary>
        public float celestiumRewardMultiplier;

        /// <summary>
        /// Additional chance (0-1) of receiving a bonus item reward on completion.
        /// </summary>
        public float bonusItemChance;

        /// <summary>
        /// Additional success chance (0-1) for a specific mission type.
        /// If <see cref="missionType"/> is null, this bonus applies to ALL mission types.
        /// </summary>
        public float successChanceBonus;

        /// <summary>
        /// The mission type this modifier applies to. Leave null to apply to all mission types.
        /// </summary>
        public MissionType? missionType;

        public ExpeditionModifier(string source)
        {
            this.source = source;
        }
    }

    /// <summary>
    /// Component holding a hero's personal expedition-related bonuses.
    /// Attach alongside <see cref="Havengard.Core.Heroes.HeroInstance"/> and
    /// <see cref="Havengard.Core.Character.StatsComponent"/> so items/traits/abilities can
    /// grant expedition-specific bonuses (duration reduction, reward multipliers,
    /// bonus item chance, mission-type success chance) independently of combat stats.
    /// </summary>
    [DisallowMultipleComponent]
    public class HeroExpeditionModifiers : MonoBehaviour
    {
        private readonly List<ExpeditionModifier> modifiers = new List<ExpeditionModifier>();

        public IReadOnlyList<ExpeditionModifier> Modifiers => modifiers;

        /// <summary>
        /// Adds a modifier from a given source. If a modifier from the same source already
        /// exists, it is replaced (prevents double-stacking when re-equipping the same item).
        /// </summary>
        public void AddModifier(ExpeditionModifier modifier)
        {
            if (modifier == null || string.IsNullOrEmpty(modifier.source))
            {
                Debug.LogWarning($"[HeroExpeditionModifiers] {name} tried to add a modifier with no source.");
                return;
            }

            RemoveModifier(modifier.source);
            modifiers.Add(modifier);
        }

        /// <summary>
        /// Removes any modifier(s) previously added under the given source.
        /// </summary>
        public void RemoveModifier(string source)
        {
            if (string.IsNullOrEmpty(source)) return;
            modifiers.RemoveAll(m => m.source == source);
        }

        /// <summary>
        /// Total duration reduction (in days) from all sources.
        /// </summary>
        public int GetDurationReductionDays()
        {
            int total = 0;
            foreach (var mod in modifiers)
                total += mod.durationReductionDays;
            return total;
        }

        /// <summary>
        /// Total bonus gold reward multiplier from all sources (e.g. 1f = +100% gold).
        /// </summary>
        public float GetGoldRewardMultiplier()
        {
            float total = 0f;
            foreach (var mod in modifiers)
                total += mod.goldRewardMultiplier;
            return total;
        }

        /// <summary>
        /// Total bonus celestium reward multiplier from all sources (e.g. 1f = +100% celestium).
        /// </summary>
        public float GetCelestiumRewardMultiplier()
        {
            float total = 0f;
            foreach (var mod in modifiers)
                total += mod.celestiumRewardMultiplier;
            return total;
        }

        /// <summary>
        /// Total bonus chance (0-1, clamped) of receiving a bonus item reward.
        /// </summary>
        public float GetBonusItemChance()
        {
            float total = 0f;
            foreach (var mod in modifiers)
                total += mod.bonusItemChance;
            return Mathf.Clamp01(total);
        }

        /// <summary>
        /// Total success chance bonus (0-1) for a specific mission type. Includes
        /// modifiers targeted at that specific type as well as "all mission types" modifiers.
        /// </summary>
        public float GetSuccessChanceBonus(MissionType missionType)
        {
            float total = 0f;
            foreach (var mod in modifiers)
            {
                if (mod.missionType == null || mod.missionType.Value == missionType)
                    total += mod.successChanceBonus;
            }
            return total;
        }
    }
}