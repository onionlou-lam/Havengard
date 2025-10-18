using UnityEngine;

namespace Havengard.Statuses
{
    [CreateAssetMenu(menuName = "Havengard/Status Effects/New Status Effect")]
    public class StatusEffectData : ScriptableObject
    {
        [Header("Basic Info")]
        public string effectName = "Unnamed Effect";
        public Sprite icon;
        public Color uiColor = Color.white;

        [Header("Core Behaviour")]
        public StatusCategory category = StatusCategory.Debuff;
        public DamageType damageType = DamageType.Physical;
        public float duration = 3f;
        public bool stackable = false;
        public bool refreshDurationOnReapply = true;

        [Header("Damage-Over-Time (optional)")]
        public bool causesDamage = false;
        public int tickDamage = 0;
        public float tickInterval = 1f;

        [Header("Crowd-Control (optional)")]
        public bool causesStun = false;
        public bool causesSilence = false;
        public bool causesRoot = false;

        [Header("Stat Modifiers (optional)")]
        [Range(0f, 2f)] public float moveSpeedMultiplier = 1f;
        [Range(0f, 2f)] public float attackSpeedMultiplier = 1f;
        [Range(0f, 2f)] public float damageMultiplier = 1f;
        [Range(0f, 2f)] public float defenseMultiplier = 1f;

        [Header("Visuals")]
        public GameObject attachVFX;
        public AudioClip applySFX;

        [Header("Gameplay Tags")]
        public bool canBeResisted = true;
    }

    public enum StatusCategory { Buff, Debuff, Neutral }
    public enum DamageType { Physical, Fire, Frost, Poison, Arcane }
}
