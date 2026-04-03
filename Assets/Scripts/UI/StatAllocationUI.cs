using Havengard.Combat;
using Havengard.Stats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Havengard.UI
{
    /// <summary>
    /// UI for allocating stat and power points
    /// </summary>
    public class StatAllocationUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button closeButton;

        [Header("Stat Points")]
        [SerializeField] private TextMeshProUGUI statPointsText;
        [SerializeField] private Button healthButton;
        [SerializeField] private Button defenseButton;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button resourceButton;
        [SerializeField] private Button movementButton;

        [Header("Power Points")]
        [SerializeField] private TextMeshProUGUI powerPointsText;
        [SerializeField] private Button fireButton;
        [SerializeField] private Button frostButton;
        [SerializeField] private Button lightningButton;
        [SerializeField] private Button holyButton;
        [SerializeField] private Button physicalButton;

        private PlayerStatAllocator allocator;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            // Setup stat buttons
            if (healthButton != null) healthButton.onClick.AddListener(() => AllocateStat("Health"));
            if (defenseButton != null) defenseButton.onClick.AddListener(() => AllocateStat("Defense"));
            if (attackButton != null) attackButton.onClick.AddListener(() => AllocateStat("Attack"));
            if (resourceButton != null) resourceButton.onClick.AddListener(() => AllocateStat("Resource"));
            if (movementButton != null) movementButton.onClick.AddListener(() => AllocateStat("Movement"));

            // Setup power buttons
            if (fireButton != null) fireButton.onClick.AddListener(() => AllocateDamageType(DamageType.Fire));
            if (frostButton != null) frostButton.onClick.AddListener(() => AllocateDamageType(DamageType.Frost));
            if (lightningButton != null) lightningButton.onClick.AddListener(() => AllocateDamageType(DamageType.Lightning));
            if (holyButton != null) holyButton.onClick.AddListener(() => AllocateDamageType(DamageType.Holy));
            if (physicalButton != null) physicalButton.onClick.AddListener(() => AllocateDamageType(DamageType.Physical));

            Hide();
        }

        public void Show(PlayerStatAllocator statAllocator)
        {
            if (statAllocator == null)
            {
                Debug.LogWarning("[StatAllocationUI] Cannot show: allocator is null");
                return;
            }

            allocator = statAllocator;
            panel.SetActive(true);
            RefreshDisplay();

            // Subscribe to changes
            allocator.OnStatPointsChanged += (_) => RefreshDisplay();
            allocator.OnPowerPointsChanged += (_) => RefreshDisplay();
        }

        public void Hide()
        {
            panel.SetActive(false);

            if (allocator != null)
            {
                allocator.OnStatPointsChanged -= (_) => RefreshDisplay();
                allocator.OnPowerPointsChanged -= (_) => RefreshDisplay();
                allocator = null;
            }
        }

        private void RefreshDisplay()
        {
            if (allocator == null) return;

            if (statPointsText != null)
                statPointsText.text = $"Stat Points: {allocator.UnspentStatPoints}";

            if (powerPointsText != null)
                powerPointsText.text = $"Power Points: {allocator.UnspentPowerPoints}";

            // Enable/disable buttons based on available points
            bool hasStatPoints = allocator.UnspentStatPoints > 0;
            if (healthButton != null) healthButton.interactable = hasStatPoints;
            if (defenseButton != null) defenseButton.interactable = hasStatPoints;
            if (attackButton != null) attackButton.interactable = hasStatPoints;
            if (resourceButton != null) resourceButton.interactable = hasStatPoints;
            if (movementButton != null) movementButton.interactable = hasStatPoints;

            bool hasPowerPoints = allocator.UnspentPowerPoints > 0;
            if (fireButton != null) fireButton.interactable = hasPowerPoints;
            if (frostButton != null) frostButton.interactable = hasPowerPoints;
            if (lightningButton != null) lightningButton.interactable = hasPowerPoints;
            if (holyButton != null) holyButton.interactable = hasPowerPoints;
            if (physicalButton != null) physicalButton.interactable = hasPowerPoints;
        }

        private void AllocateStat(string statType)
        {
            if (allocator == null) return;

            bool success = false;
            switch (statType)
            {
                case "Health": success = allocator.AllocateHealth(); break;
                case "Defense": success = allocator.AllocateDefense(); break;
                case "Attack": success = allocator.AllocateAttack(); break;
                case "Resource": success = allocator.AllocateResource(); break;
                case "Movement": success = allocator.AllocateMovementSpeed(); break;
            }

            if (success)
            {
                RefreshDisplay();
            }
        }

        private void AllocateDamageType(DamageType damageType)
        {
            if (allocator == null) return;

            if (allocator.IncreaseDamageType(damageType))
            {
                RefreshDisplay();
            }
        }
    }
}