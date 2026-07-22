using UnityEngine;
using Havengard.Abilities;

namespace Havengard.UI
{
    /// <summary>
    /// Manages the ability bar UI with 6 slots (1, 2, 3, 4, LMB, and an extra slot).
    /// Automatically syncs with the player's AbilityUser component.
    /// </summary>
    public class AbilityBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AbilityUser abilityUser;
        [SerializeField] private PlayerController2D playerController;

        [Header("Ability Slots")]
        [SerializeField] private AbilitySlotUI slot1;      // Index 0
        [SerializeField] private AbilitySlotUI slot2;      // Index 1
        [SerializeField] private AbilitySlotUI slot3;      // Index 2
        [SerializeField] private AbilitySlotUI slot4;      // Index 3
        [SerializeField] private AbilitySlotUI slotLMB;    // Left Mouse Button
        [SerializeField] private AbilitySlotUI slotExtra;  // Extra slot

        [Header("Ability Mapping")]
        [Tooltip("Index in AbilityUser for Left Mouse Button ability")]
        [SerializeField] private int leftMouseButtonIndex = 0; // MB1 uses slot 0
        [Tooltip("Index in AbilityUser for Extra slot")]
        [SerializeField] private int extraSlotIndex = 4;

        private AbilitySlotUI[] slots;

        private void Awake()
        {
            // Auto-find AbilityUser if not assigned
            if (abilityUser == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    abilityUser = player.GetComponent<AbilityUser>();
                    playerController = player.GetComponent<PlayerController2D>();
                }
            }

            // Initialize slot array
            slots = new AbilitySlotUI[6];
            slots[0] = slot1;
            slots[1] = slot2;
            slots[2] = slot3;
            slots[3] = slot4;
            slots[4] = slotLMB;
            slots[5] = slotExtra;

            // Set keybind labels dynamically from PlayerController2D
            UpdateKeybindLabels();
        }

        private void UpdateKeybindLabels()
        {
            if (playerController != null)
            {
                KeyCode[] abilityKeys = playerController.GetAbilityKeys();
                
                // Set ability keys 1, 2, 3, 4
                if (slot1 != null && abilityKeys.Length > 0) 
                    slot1.SetKeybind(GetKeyDisplayName(abilityKeys[0]));
                if (slot2 != null && abilityKeys.Length > 1) 
                    slot2.SetKeybind(GetKeyDisplayName(abilityKeys[1]));
                if (slot3 != null && abilityKeys.Length > 2) 
                    slot3.SetKeybind(GetKeyDisplayName(abilityKeys[2]));
                if (slot4 != null && abilityKeys.Length > 3) 
                    slot4.SetKeybind(GetKeyDisplayName(abilityKeys[3]));
            }
            else
            {
                // Fallback to default labels
                if (slot1 != null) slot1.SetKeybind("1");
                if (slot2 != null) slot2.SetKeybind("2");
                if (slot3 != null) slot3.SetKeybind("3");
                if (slot4 != null) slot4.SetKeybind("4");
            }
            
            // Mouse button is always LMB
            if (slotLMB != null) slotLMB.SetKeybind("LMB");
            
            // Extra slot (if you want to add a 5th ability key later)
            if (slotExtra != null) slotExtra.SetKeybind("5");
        }

        /// <summary>
        /// Converts KeyCode to a user-friendly display name
        /// </summary>
        private string GetKeyDisplayName(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.Alpha1: return "1";
                case KeyCode.Alpha2: return "2";
                case KeyCode.Alpha3: return "3";
                case KeyCode.Alpha4: return "4";
                case KeyCode.Alpha5: return "5";
                case KeyCode.Alpha6: return "6";
                case KeyCode.Alpha7: return "7";
                case KeyCode.Alpha8: return "8";
                case KeyCode.Alpha9: return "9";
                case KeyCode.Alpha0: return "0";
                case KeyCode.Mouse0: return "LMB";
                case KeyCode.Mouse1: return "RMB";
                case KeyCode.Mouse2: return "MMB";
                default: return key.ToString();
            }
        }

        private void OnEnable()
        {
            if (abilityUser != null)
            {
                abilityUser.OnAbilityUsed += HandleAbilityUsed;
                abilityUser.OnAbilityCooldownStarted += HandleCooldownStarted;
                abilityUser.OnAbilitiesChanged += RefreshAbilities;
            }
        }

        private void OnDisable()
        {
            if (abilityUser != null)
            {
                abilityUser.OnAbilityUsed -= HandleAbilityUsed;
                abilityUser.OnAbilityCooldownStarted -= HandleCooldownStarted;
                abilityUser.OnAbilitiesChanged -= RefreshAbilities;
            }
        }

        private void Start()
        {
            RefreshAbilities();
        }

        /// <summary>
        /// Refreshes all ability slots with current abilities from AbilityUser.
        /// </summary>
        public void RefreshAbilities()
        {
            if (abilityUser == null) return;

            // Slots 1, 2, 3, 4 (indices 0-3)
            for (int i = 0; i < 4; i++)
            {
                if (slots[i] != null)
                {
                    AbilityBase ability = abilityUser.GetAbility(i);
                    slots[i].SetAbility(ability);
                }
            }

            // LMB slot (uses slot 0 - same as key "1")
            if (slotLMB != null)
            {
                AbilityBase lmbAbility = abilityUser.GetAbility(leftMouseButtonIndex);
                slotLMB.SetAbility(lmbAbility);
            }

            // Extra slot
            if (slotExtra != null)
            {
                AbilityBase extraAbility = abilityUser.GetAbility(extraSlotIndex);
                slotExtra.SetAbility(extraAbility);
            }
        }

        /// <summary>
        /// Called when an ability is used. Handles visual feedback.
        /// </summary>
        private void HandleAbilityUsed(int abilityIndex, AbilityBase ability)
        {
            AbilitySlotUI slot = GetSlotForAbilityIndex(abilityIndex);
            if (slot != null)
                slot.Flash();
        }

        /// <summary>
        /// Called when an ability cooldown starts.
        /// </summary>
        private void HandleCooldownStarted(int abilityIndex, float duration)
        {
            AbilitySlotUI slot = GetSlotForAbilityIndex(abilityIndex);
            if (slot != null)
                slot.StartCooldown(duration);
        }

        /// <summary>
        /// Gets the UI slot for a given ability index.
        /// </summary>
        private AbilitySlotUI GetSlotForAbilityIndex(int abilityIndex)
        {
            // Keys 1, 2, 3, 4
            if (abilityIndex >= 0 && abilityIndex < 4)
                return slots[abilityIndex];

            // Extra slot
            if (abilityIndex == extraSlotIndex)
                return slotExtra;

            return null;
        }

        /// <summary>
        /// Sets the ability for a specific slot.
        /// </summary>
        public void SetAbility(int slotIndex, AbilityBase ability)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length) return;
            if (slots[slotIndex] != null)
                slots[slotIndex].SetAbility(ability);
        }

        /// <summary>
        /// Manually triggers a cooldown on a specific slot (useful for external systems).
        /// </summary>
        public void TriggerCooldown(int slotIndex, float duration)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length) return;
            if (slots[slotIndex] != null)
                slots[slotIndex].StartCooldown(duration);
        }
    }
}