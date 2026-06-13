using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Havengard.Abilities;

namespace Havengard.UI.SkillTree
{
    /// <summary>
    /// Individual skill tree node button
    /// Shows ability icon, locked/unlocked state, handles click and hover
    /// </summary>
    public class SkillTreeNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private GameObject lockedOverlay; // Dark overlay when locked

        [Header("State Colors")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = Color.gray;
        [SerializeField] private Color availableColor = new Color(1f, 1f, 0.5f); // Yellow tint = can unlock
        [SerializeField] private Color cannotAffordColor = new Color(1f, 0.5f, 0.5f); // Red tint = missing requirements

        // Runtime data
        private int abilityIndex; // FIXED: Missing field declaration
        private ClassAbility classAbility;
        private SkillTreeUI parentUI;
        private bool isUnlocked;
        private bool canUnlock;

        public int AbilityIndex => abilityIndex;
        public RectTransform RectTransform => GetComponent<RectTransform>();

        public void Initialize(int index, ClassAbility ability, SkillTreeUI parent)
        {
            abilityIndex = index;
            classAbility = ability;
            parentUI = parent;

            // Set icon (uses 'icon' not 'abilityIcon')
            if (iconImage != null && ability.ability != null && ability.ability.icon != null)
            {
                iconImage.sprite = ability.ability.icon;
            }
        }

        /// <summary>
        /// Update visual state based on unlock status and requirements
        /// </summary>
        public void RefreshState(bool[] unlockedAbilities, int availableSkillPoints, int playerLevel)
        {
            if (classAbility == null)
                return;

            // Check if unlocked
            isUnlocked = unlockedAbilities[abilityIndex];

            if (isUnlocked)
            {
                // UNLOCKED STATE
                SetVisualState(unlockedColor, false);
                canUnlock = false;
                return;
            }

            // LOCKED STATE - check if can be unlocked
            bool meetsLevelRequirement = playerLevel >= classAbility.requiredLevel;
            bool hasEnoughSkillPoints = availableSkillPoints >= classAbility.skillPointCost;
            bool prerequisitesMet = classAbility.ArePrerequisitesMet(unlockedAbilities);

            canUnlock = meetsLevelRequirement && hasEnoughSkillPoints && prerequisitesMet;

            if (canUnlock)
            {
                // AVAILABLE TO UNLOCK
                SetVisualState(availableColor, true);
            }
            else
            {
                // LOCKED - cannot unlock yet
                SetVisualState(lockedColor, true);
            }
        }

        private void SetVisualState(Color tintColor, bool showLockedOverlay)
        {
            if (iconImage != null)
                iconImage.color = tintColor;

            if (backgroundImage != null)
                backgroundImage.color = tintColor;

            if (lockedOverlay != null)
                lockedOverlay.SetActive(showLockedOverlay);
        }

        #region Mouse Events
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (parentUI != null && classAbility != null)
            {
                parentUI.ShowTooltip(classAbility, RectTransform.position, isUnlocked, canUnlock);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (parentUI != null)
            {
                parentUI.HideTooltip();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (parentUI != null && !isUnlocked)
            {
                parentUI.OnNodeClicked(abilityIndex);
            }
        }
        #endregion
    }
}