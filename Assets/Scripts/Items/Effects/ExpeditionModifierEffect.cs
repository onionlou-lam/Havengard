using UnityEngine;
using Havengard.Expeditions;

namespace Havengard.Items
{
    /// <summary>
    /// Item effect that grants expedition-related bonuses to a hero/follower, such as
    /// reduced mission duration, increased reward currency, bonus item chance, or
    /// increased success chance for a specific (or all) mission types.
    /// </summary>
    [CreateAssetMenu(menuName = "Havengard/Items/Effects/Expedition Modifier")]
    public class ExpeditionModifierEffect : ItemEffect
    {
        public enum ExpeditionStatType
        {
            /// <summary>Reduces mission duration in days.</summary>
            DurationReduction,
            /// <summary>Increases Gold reward (percentage, 1f = +100%).</summary>
            GoldRewardMultiplier,
            /// <summary>Increases Celestium reward (percentage, 1f = +100%).</summary>
            CelestiumRewardMultiplier,
            /// <summary>Increases chance of receiving a bonus item reward.</summary>
            BonusItemChance,
            /// <summary>Increases success chance for the configured mission type (or all, if unset).</summary>
            SuccessChance
        }

        [Header("Expedition Modification")]
        public ExpeditionStatType statType;

        [Tooltip("Only used when statType is SuccessChance. Leave unchecked to apply to ALL mission types.")]
        public bool restrictToSpecificMissionType;

        [Tooltip("The specific mission type this bonus applies to, when restrictToSpecificMissionType is enabled.")]
        public MissionType specificMissionType;

        public override void Apply(GameObject target, int level)
        {
            var modifiers = GetOrWarn(target);
            if (modifiers == null) return;

            var modifier = BuildModifier(target, level);
            modifiers.AddModifier(modifier);

            Debug.Log($"[ExpeditionModifierEffect] Applied {statType} to {target.name} (Level {level})");
        }

        public override void Remove(GameObject target, int level)
        {
            var modifiers = GetOrWarn(target);
            if (modifiers == null) return;

            modifiers.RemoveModifier(GetSourceKey(target));

            Debug.Log($"[ExpeditionModifierEffect] Removed {statType} from {target.name}");
        }

        public override string FormatDescription(string description, int level)
        {
            float value = GetValue(level);
            return description.Replace("{value}", value.ToString("F1"));
        }

        private HeroExpeditionModifiers GetOrWarn(GameObject target)
        {
            if (target == null)
            {
                Debug.LogWarning("[ExpeditionModifierEffect] Target is null");
                return null;
            }

            var modifiers = target.GetComponent<HeroExpeditionModifiers>();
            if (modifiers == null)
            {
                Debug.LogWarning($"[ExpeditionModifierEffect] {target.name} has no HeroExpeditionModifiers component");
                return null;
            }

            return modifiers;
        }

        /// <summary>
        /// Unique key identifying this effect instance as a modifier source, so it can be removed later
        /// even if the hero has multiple expedition-modifying items equipped.
        /// </summary>
        private string GetSourceKey(GameObject target) => $"{name}:{GetInstanceID()}";

        private ExpeditionModifier BuildModifier(GameObject target, int level)
        {
            float totalValue = GetValue(level);
            var modifier = new ExpeditionModifier(GetSourceKey(target));

            switch (statType)
            {
                case ExpeditionStatType.DurationReduction:
                    modifier.durationReductionDays = Mathf.RoundToInt(totalValue);
                    break;

                case ExpeditionStatType.GoldRewardMultiplier:
                    modifier.goldRewardMultiplier = totalValue;
                    break;

                case ExpeditionStatType.CelestiumRewardMultiplier:
                    modifier.celestiumRewardMultiplier = totalValue;
                    break;

                case ExpeditionStatType.BonusItemChance:
                    modifier.bonusItemChance = totalValue;
                    break;

                case ExpeditionStatType.SuccessChance:
                    modifier.successChanceBonus = totalValue;
                    modifier.missionType = restrictToSpecificMissionType ? specificMissionType : (MissionType?)null;
                    break;
            }

            return modifier;
        }
    }
}