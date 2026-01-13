using UnityEngine;
using Havengard.Abilities;

namespace Havengard.UI
{
    /// <summary>
    /// Manages the ability bar UI with 6 slots (Q, W, E, R, LMB, RMB).
    /// Automatically syncs with the player's AbilityUser component.
    /// </summary>
    public class AbilityBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AbilityUser abilityUser;

        [Header("Ability Slots")]
        [SerializeField] private AbilitySlotUI slotQ;      // Index 0
        [SerializeField] private AbilitySlotUI slotW;      // Index 1
        [SerializeField] private AbilitySlotUI slotE;      // Index 2
        [SerializeField] private AbilitySlotUI slotR;      // Index 3
        [SerializeField] private AbilitySlotUI slotLMB;    // Left Mouse Button
        [SerializeField] private AbilitySlotUI slotRMB;    // Right Mouse Button

        [Header("Ability Mapping")]
        [Tooltip("Index in AbilityUser for Left Mouse Button ability")]
        [SerializeField] private int leftMouseButtonIndex = 4;
        [Tooltip("Index in AbilityUser for Right Mouse Button ability")]
        [SerializeField] private int rightMouseButtonIndex = 5;

        private AbilitySlotUI[] slots;

        private void Awake()
        {
            // Auto-find AbilityUser if not assigned
            if (abilityUser == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    abilityUser = player.GetComponent<AbilityUser>();
            }

            // Initialize slot array
            slots = new AbilitySlotUI[6];
            slots[0] = slotQ;
            slots[1] = slotW;
            slots[2] = slotE;
            slots[3] = slotR;
            slots[4] = slotLMB;
            slots[5] = slotRMB;

            // Set keybind labels
            if (slotQ != null) slotQ.SetKeybind("Q");
            if (slotW != null) slotW.SetKeybind("W");
            if (slotE != null) slotE.SetKeybind("E");
            if (slotR != null) slotR.SetKeybind("R");
            if (slotLMB != null) slotLMB.SetKeybind("LMB");
            if (slotRMB != null) slotRMB.SetKeybind("RMB");
        }

        private void OnEnable()
        {
            if (abilityUser != null)
            {
                abilityUser.OnAbilityUsed += HandleAbilityUsed;
                abilityUser.OnAbilityCooldownStarted += HandleCooldownStarted;
            }
        }

        private void OnDisable()
        {
            if (abilityUser != null)
            {
                abilityUser.OnAbilityUsed -= HandleAbilityUsed;
                abilityUser.OnAbilityCooldownStarted -= HandleCooldownStarted;
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

            // Q, W, E, R slots (indices 0-3)
            for (int i = 0; i < 4; i++)
            {
                if (slots[i] != null)
                {
                    AbilityBase ability = abilityUser.GetAbility(i);
                    slots[i].SetAbility(ability);
                }
            }

            // LMB slot
            if (slotLMB != null)
            {
                AbilityBase lmbAbility = abilityUser.GetAbility(leftMouseButtonIndex);
                slotLMB.SetAbility(lmbAbility);
            }

            // RMB slot
            if (slotRMB != null)
            {
                AbilityBase rmbAbility = abilityUser.GetAbility(rightMouseButtonIndex);
                slotRMB.SetAbility(rmbAbility);
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
            // Q, W, E, R
            if (abilityIndex >= 0 && abilityIndex < 4)
                return slots[abilityIndex];

            // LMB
            if (abilityIndex == leftMouseButtonIndex)
                return slotLMB;

            // RMB
            if (abilityIndex == rightMouseButtonIndex)
                return slotRMB;

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