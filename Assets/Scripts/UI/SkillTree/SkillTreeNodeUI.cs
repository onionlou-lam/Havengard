using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Havengard.Abilities;

namespace Havengard.UI
{
    /// <summary>
    /// Individual skill tree node button
    /// Shows ability icon, locked/unlocked state, handles click
    /// No tooltip - uses fixed info panel instead
    /// </summary>
    public class SkillTreeNodeUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private GameObject selectedIndicator; // NEW: Shows when node is selected

        [Header("State Colors")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = Color.gray;
        [SerializeField] private Color availableColor = new Color(1f, 1f, 0.5f);
        [SerializeField] private Color cannotAffordColor = new Color(1f, 0.5f, 0.5f);

        // Runtime data
        private int abilityIndex;
        private ClassAbility classAbility;
        private SkillTreeUI parentUI;
        private bool isUnlocked;
        private bool canUnlock;
        private bool isSelected;

        public int AbilityIndex => abilityIndex;
        public RectTransform RectTransform => GetComponent<RectTransform>();

        private void Awake()
        {
            if (selectedIndicator != null)
                selectedIndicator.SetActive(false);
        }

        public void Initialize(int index, ClassAbility ability, SkillTreeUI parent)
        {
            abilityIndex = index;
            classAbility = ability;
            parentUI = parent;

            if (iconImage != null && ability.ability != null && ability.ability.icon != null)
            {
                iconImage.sprite = ability.ability.icon;
            }
        }

        public void RefreshState(bool[] unlockedAbilities, int availableSkillPoints, int playerLevel)
        {
            if (classAbility == null) return;

            isUnlocked = unlockedAbilities[abilityIndex];

            if (isUnlocked)
            {
                SetVisualState(unlockedColor, false);
                canUnlock = false;
                return;
            }

            bool meetsLevelRequirement = playerLevel >= classAbility.requiredLevel;
            bool hasEnoughSkillPoints = availableSkillPoints >= classAbility.skillPointCost;
            bool prerequisitesMet = classAbility.ArePrerequisitesMet(unlockedAbilities);

            canUnlock = meetsLevelRequirement && hasEnoughSkillPoints && prerequisitesMet;

            if (canUnlock)
            {
                SetVisualState(availableColor, true);
            }
            else
            {
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

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (selectedIndicator != null)
                selectedIndicator.SetActive(selected);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (parentUI != null)
            {
                parentUI.OnNodeClicked(abilityIndex, this);
            }
        }
    }
}