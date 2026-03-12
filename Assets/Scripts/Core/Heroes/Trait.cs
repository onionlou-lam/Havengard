using UnityEngine;

namespace Havengard.Heroes
{
    [CreateAssetMenu(menuName = "Havengard/Hero Trait")]
    public class Trait : ScriptableObject
    {
        public string traitName;
        [TextArea] public string description;

        // Example modifiers
        public float goldMultiplier = 1f;
        public float expMultiplier = 1f;
        public float durationMultiplier = 1f;
        public float successChanceBonus = 0f;
    }
}
