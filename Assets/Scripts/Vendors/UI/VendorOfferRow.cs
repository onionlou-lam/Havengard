using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Havengard.Vendors.UI
{
    public class VendorOfferRow : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button buyButton;

        [Header("Colors")]
        [SerializeField] private Color affordableColor = Color.white;
        [SerializeField] private Color unaffordableColor = new Color(0.9f, 0.3f, 0.3f);

        private Action onHover;

        public void Setup(IVendorOffer offer, bool available, bool affordable, string unavailableReason,
            Action<IVendorOffer> onBuy, Action onHoverCallback)
        {
            onHover = onHoverCallback;

            if (iconImage != null)
            {
                iconImage.sprite = offer.Icon;
                iconImage.enabled = offer.Icon != null;
            }
            if (nameText != null) nameText.text = offer.DisplayName;
            if (descriptionText != null)
            {
                descriptionText.text = available || string.IsNullOrEmpty(unavailableReason)
                    ? offer.Description
                    : offer.Description + "\n" + unavailableReason;
            }
            if (priceText != null)
            {
                priceText.text = offer.Price.ToString();
                priceText.color = affordable ? affordableColor : unaffordableColor;
            }
            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                // Left clickable when unaffordable so the player gets "not enough funds" feedback.
                buyButton.interactable = available;
                buyButton.onClick.AddListener(() => onBuy(offer));
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (onHover != null) onHover();
        }
    }
}