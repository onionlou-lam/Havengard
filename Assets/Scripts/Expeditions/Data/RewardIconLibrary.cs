using UnityEngine;

namespace Havengard.Expeditions
{
    /// <summary>
    /// Shared icon set used to display expedition/mission rewards (currency icons, etc.)
    /// Assign one instance and reference it from reward-display UI (ExpeditionResultPanel, ExpeditionSetupPanel).
    /// </summary>
    [CreateAssetMenu(menuName = "Havengard/Expeditions/Reward Icon Library")]
    public class RewardIconLibrary : ScriptableObject
    {
        [Header("Currency Icons")]
        public Sprite goldIcon;
        public Sprite celestiumIcon;
        public Sprite experienceIcon;

        [Header("Result Icons")]
        public Sprite successIcon;
        public Sprite failureIcon;
    }
}