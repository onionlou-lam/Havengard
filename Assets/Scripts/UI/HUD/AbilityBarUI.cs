using UnityEngine;
using Havengard.Abilities;
using System.Collections.Generic;

namespace Havengard.UI
{
    /// <summary>
    /// Manages the ability bar UI with 6 slots.
    /// Supports drag-and-drop assignment from skill tree.
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
        [SerializeField] private AbilitySlotUI slotRMB;    // Right Mouse Button

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
            slots[5] = slotRMB;

            // Initialize each slot with its index
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].Initialize(i, this);
                }
            }

            // Set keybind labels
            UpdateKeybindLabels();
        }

        private void Start()
        {
            // Sync with AbilityUser on start
            RefreshAll();
            
            // Subscribe to ability changes and cooldowns
            if (abilityUser != null)
            {
                abilityUser.OnAbilitiesChanged += RefreshAll;
                abilityUser.OnAbilityCooldownStarted += OnCooldownStarted;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe
            if (abilityUser != null)
            {
                abilityUser.OnAbilitiesChanged -= RefreshAll;
                abilityUser.OnAbilityCooldownStarted -= OnCooldownStarted;
            }
        }

        /// <summary>
        /// Called when an ability goes on cooldown
        /// </summary>
        private void OnCooldownStarted(int abilityIndex, float duration)
        {
            TriggerCooldown(abilityIndex, duration);
        }

        private void UpdateKeybindLabels()
        {
            if (playerController != null)
            {
                KeyCode[] abilityKeys = playerController.GetAbilityKeys();
                
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
                if (slot1 != null) slot1.SetKeybind("1");
                if (slot2 != null) slot2.SetKeybind("2");
                if (slot3 != null) slot3.SetKeybind("3");
                if (slot4 != null) slot4.SetKeybind("4");
            }
            
            if (slotLMB != null) slotLMB.SetKeybind("LMB");
            if (slotRMB != null) slotRMB.SetKeybind("RMB");
        }

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
                case KeyCode.Q: return "Q";
                case KeyCode.E: return "E";
                case KeyCode.R: return "R";
                case KeyCode.F: return "F";
                default: return key.ToString();
            }
        }

        /// <summary>
        /// Assign an ability to a specific slot.
        /// Note: This adds the ability to AbilityUser's list if not already present.
        /// </summary>
        public void AssignAbilityToSlot(int slotIndex, AbilityBase ability)
        {
            if (abilityUser == null || ability == null)
                return;

            if (slotIndex < 0 || slotIndex >= slots.Length)
                return;

            // Get current abilities list
            List<AbilityBase> currentAbilities = abilityUser.GetAllAbilities();

            // Make sure the ability is in the list
            if (!currentAbilities.Contains(ability))
            {
                currentAbilities.Add(ability);
            }

            // Find the index of this ability
            int abilityIndex = currentAbilities.IndexOf(ability);

            // Map slot to the ability index we want to use
            int targetIndex = MapSlotToAbilityUserIndex(slotIndex);

            // Ensure the list is big enough
            while (currentAbilities.Count <= targetIndex)
            {
                currentAbilities.Add(null);
            }

            // Set the ability at the target index
            currentAbilities[targetIndex] = ability;

            // Reassign the full list to AbilityUser
            abilityUser.AssignAbilities(currentAbilities);

            // Update the UI slot
            if (slots[slotIndex] != null)
            {
                slots[slotIndex].SetAbility(ability);
            }

            Debug.Log($"[AbilityBarUI] Assigned {ability.abilityName} to slot {slotIndex}");
        }

        /// <summary>
        /// Swap abilities between two slots.
        /// </summary>
        public void SwapAbilities(int slotIndexA, int slotIndexB)
        {
            if (abilityUser == null)
                return;

            if (slotIndexA < 0 || slotIndexA >= slots.Length ||
                slotIndexB < 0 || slotIndexB >= slots.Length)
                return;

            // Get abilities from slots
            AbilityBase abilityA = slots[slotIndexA]?.GetAbility();
            AbilityBase abilityB = slots[slotIndexB]?.GetAbility();

            // Get the full ability list
            List<AbilityBase> currentAbilities = abilityUser.GetAllAbilities();

            // Map to AbilityUser indices
            int indexA = MapSlotToAbilityUserIndex(slotIndexA);
            int indexB = MapSlotToAbilityUserIndex(slotIndexB);

            if (indexA < 0 || indexB < 0)
                return;

            // Ensure list is big enough
            while (currentAbilities.Count <= Mathf.Max(indexA, indexB))
            {
                currentAbilities.Add(null);
            }

            // Swap in the list
            currentAbilities[indexA] = abilityB;
            currentAbilities[indexB] = abilityA;

            // Reassign to AbilityUser
            abilityUser.AssignAbilities(currentAbilities);

            // Update UI
            if (slots[slotIndexA] != null)
                slots[slotIndexA].SetAbility(abilityB);
            if (slots[slotIndexB] != null)
                slots[slotIndexB].SetAbility(abilityA);

            Debug.Log($"[AbilityBarUI] Swapped slot {slotIndexA} with slot {slotIndexB}");
        }

        /// <summary>
        /// Refresh all slots from AbilityUser.
        /// </summary>
        public void RefreshAll()
        {
            if (abilityUser == null) return;

            List<AbilityBase> currentAbilities = abilityUser.GetAllAbilities();

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                {
                    int abilityUserIndex = MapSlotToAbilityUserIndex(i);
                    if (abilityUserIndex >= 0 && abilityUserIndex < currentAbilities.Count)
                    {
                        AbilityBase ability = currentAbilities[abilityUserIndex];
                        slots[i].SetAbility(ability);
                    }
                    else
                    {
                        slots[i].SetAbility(null);
                    }
                }
            }
        }

        /// <summary>
        /// Map UI slot index to AbilityUser ability index.
        /// </summary>
        private int MapSlotToAbilityUserIndex(int slotIndex)
        {
            // Default mapping:
            // Slot 0-3 (1-4 keys) → indices 1-4
            // Slot 4 (LMB) → index 0
            // Slot 5 (RMB) → index 5

            if (slotIndex == 4) return 0;  // LMB
            if (slotIndex == 5) return 5;  // RMB
            if (slotIndex >= 0 && slotIndex < 4) return slotIndex + 1; // 1-4 keys

            return -1;
        }

        /// <summary>
        /// Trigger cooldown visual on a specific slot.
        /// </summary>
        public void TriggerCooldown(int abilityUserIndex, float duration)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (MapSlotToAbilityUserIndex(i) == abilityUserIndex && slots[i] != null)
                {
                    slots[i].StartCooldown(duration);
                    break;
                }
            }
        }

        /// <summary>
        /// Trigger flash effect on a specific slot.
        /// </summary>
        public void TriggerFlash(int abilityUserIndex)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (MapSlotToAbilityUserIndex(i) == abilityUserIndex && slots[i] != null)
                {
                    slots[i].Flash();
                    break;
                }
            }
        }
    }
}