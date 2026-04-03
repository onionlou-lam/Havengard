using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Status Effect")]
    public class StatusEffectData : ScriptableObject
    {
        public string effectName = "New Effect";
        public Sprite icon;

        [Header("Duration")]
        public float duration = 5f;
        public bool isPermanent = false;

        [Header("Damage Over Time")]
        public float tickInterval = 1f;
        public float damagePerTick = 5f;

        [Header("Stat Modifiers")]
        public bool modifiesMovementSpeed = false;
        public float movementSpeedMultiplier = 1f;

        [Header("Lifesteal")]
        public float lifestealPercent = 0f; // 0.0 to 1.0 (0% to 100%)

        [Header("Visual")]
        public GameObject vfxPrefab;
        public Color tintColor = Color.white;
    }
}