using UnityEngine;
using UnityEngine.UI;
using Havengard.Resources;

namespace Havengard.UI.Shop
{
    /// <summary>
    /// Base class for shop UIs (Enchanter, Blacksmith, etc.)
    /// Handles currency display and common shop functionality.
    /// </summary>
    public abstract class BaseShopUI : MonoBehaviour
    {
        [Header("Common UI Elements")]
        [SerializeField]
        [Tooltip("Button to close the shop")]
        protected Button closeButton;

        [SerializeField]
        [Tooltip("Text displaying player's current gold")]
        protected TMPro.TextMeshProUGUI goldText;

        [Header("Audio")]
        [SerializeField]
        protected AudioClip purchaseSound;

        [SerializeField]
        protected AudioClip insufficientFundsSound;

        protected GoldSystem goldSystem;

        protected virtual void Awake()
        {
            goldSystem = GoldSystem.Instance;

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseShop);
            }
        }

        protected virtual void OnEnable()
        {
            UpdateGoldDisplay();
        }

        protected void UpdateGoldDisplay()
        {
            if (goldText != null && goldSystem != null)
            {
                goldText.text = $"Gold: {goldSystem.CurrentGold}";
            }
        }

        protected bool TryPurchase(int cost)
        {
            if (goldSystem == null)
            {
                Debug.LogError("[BaseShopUI] GoldSystem not found!");
                return false;
            }

            if (goldSystem.SpendGold(cost))
            {
                PlaySound(purchaseSound);
                UpdateGoldDisplay();
                OnPurchaseSuccess(cost);
                return true;
            }
            else
            {
                PlaySound(insufficientFundsSound);
                OnPurchaseFailed(cost);
                return false;
            }
        }

        protected virtual void OnPurchaseSuccess(int cost)
        {
            // Override in derived classes for specific behavior
        }

        protected virtual void OnPurchaseFailed(int cost)
        {
            // Override in derived classes (e.g., show "Not enough gold" message)
        }

        protected void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            }
        }

        protected virtual void CloseShop()
        {
            gameObject.SetActive(false);

            // Return to town or previous scene
            var shopController = FindObjectOfType<Havengard.Town.ShopInteriorController>();
            if (shopController != null)
            {
                shopController.OnShopExit();
            }
        }
    }
}