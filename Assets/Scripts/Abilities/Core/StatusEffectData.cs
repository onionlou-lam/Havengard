using UnityEngine;

namespace Havengard.Statuses
{
    [CreateAssetMenu(menuName = "Havengard/Statuses/Status Effect")]
    public class StatusEffectData : ScriptableObject
    {
        [Header("Identity")]
        public string effectName = "New Status";

        [Header("Duration")]
        public float duration = 3f;

        [Header("Stacking")]
        public bool stackable = false;
        [Min(1)] public int maxStacks = 1;
        public bool refreshDurationOnReapply = true;

        [Header("Damage Over Time")]
        public bool causesDamage = false;
        public int tickDamage = 5;
        public float tickInterval = 1f;

        [Header("Crowd Control")]
        public bool causesStun = false;
        public bool causesRoot = false;
        public bool causesSilence = false;

        [Header("Stat Modifiers (multipliers)")]
        public float moveSpeedMultiplier = 1f;
        public float attackSpeedMultiplier = 1f;
        public float damageMultiplier = 1f;
        public float defenseMultiplier = 1f;

        [Header("Lifesteal")]
        [Tooltip("Percentage of damage dealt converted to healing (0.0 to 1.0). Example: 0.2 = 20% lifesteal")]
        [Range(0f, 1f)] public float lifestealPercent = 0f;

        [Header("VFX / SFX")]
        public GameObject attachVFX;
        public AudioClip applySFX;
    }
}
