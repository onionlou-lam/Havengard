using UnityEngine;
using System;
using System.Collections;

namespace Havengard.Town
{
    /// <summary>
    /// Represents a shopkeeper NPC with dialogue and shop UI integration.
    /// </summary>
    public class ShopkeeperNPC : MonoBehaviour
    {
        [Header("Dialogue")]
        [SerializeField]
        [Tooltip("Possible greeting lines (one chosen randomly)")]
        private string[] greetingLines = new string[]
        {
            "Welcome! How can I help you today?",
            "Ah, a customer! What brings you here?",
            "Good to see you! Looking for something special?"
        };

        [SerializeField]
        [Tooltip("Tips/mechanic explanations the player can ask about")]
        private ShopTip[] tips;

        [Header("UI References")]
        [SerializeField]
        [Tooltip("The dialogue UI panel")]
        private ShopDialogueUI dialogueUI;

        [SerializeField]
        [Tooltip("The shop UI panel (Enchanter/Blacksmith specific)")]
        private GameObject shopUI;

        [Header("Animation")]
        [SerializeField]
        [Tooltip("Optional animator for the shopkeeper")]
        private Animator animator;

        private ShopInteriorController shopController;

        private void Awake()
        {
            shopController = FindObjectOfType<ShopInteriorController>();

            // Ensure UIs start disabled
            if (dialogueUI != null)
            {
                dialogueUI.gameObject.SetActive(false);
            }
            if (shopUI != null)
            {
                shopUI.SetActive(false);
            }
        }

        public void StartGreeting()
        {
            if (dialogueUI == null)
            {
                Debug.LogWarning("[ShopkeeperNPC] No dialogue UI assigned!");
                return;
            }

            // Pick random greeting
            string greeting = greetingLines[UnityEngine.Random.Range(0, greetingLines.Length)];

            // Show dialogue UI with greeting and options
            dialogueUI.gameObject.SetActive(true);
            dialogueUI.ShowDialogue(greeting, GetDialogueOptions());

            // Play greeting animation
            if (animator != null)
            {
                animator.SetTrigger("Greet");
            }
        }

        private DialogueOption[] GetDialogueOptions()
        {
            // Build dialogue options dynamically
            var options = new System.Collections.Generic.List<DialogueOption>();

            // "Browse shop" option
            options.Add(new DialogueOption
            {
                text = "Let me see what you have.",
                onSelected = OpenShopUI
            });

            // Add tip options
            foreach (var tip in tips)
            {
                var tipCopy = tip; // Capture for closure
                options.Add(new DialogueOption
                {
                    text = tip.question,
                    onSelected = () => ShowTip(tipCopy)
                });
            }

            // "Leave" option
            options.Add(new DialogueOption
            {
                text = "I'll come back later.",
                onSelected = CloseDialogue
            });

            return options.ToArray();
        }

        private void OpenShopUI()
        {
            // Close dialogue
            if (dialogueUI != null)
            {
                dialogueUI.gameObject.SetActive(false);
            }

            // Open shop UI
            if (shopUI != null)
            {
                shopUI.SetActive(true);

                // Notify controller
                if (shopController != null)
                {
                    shopController.OnShopUIOpened();
                }
            }
        }

        private void ShowTip(ShopTip tip)
        {
            if (dialogueUI != null)
            {
                // Show tip explanation, then return to options
                dialogueUI.ShowDialogue(tip.answer, new DialogueOption[]
                {
                    new DialogueOption
                    {
                        text = "Got it, thanks!",
                        onSelected = () => StartGreeting()
                    },
                    new DialogueOption
                    {
                        text = "Let me see what you have.",
                        onSelected = OpenShopUI
                    }
                });
            }
        }

        private void CloseDialogue()
        {
            if (dialogueUI != null)
            {
                dialogueUI.gameObject.SetActive(false);
            }

            // Could trigger exit scene or re-enable player control
            if (shopController != null)
            {
                shopController.OnShopExit();
            }
        }

        [Serializable]
        public struct ShopTip
        {
            [TextArea(2, 4)]
            public string question;
            [TextArea(3, 6)]
            public string answer;
        }
    }

    [Serializable]
    public struct DialogueOption
    {
        public string text;
        public Action onSelected;
    }
}