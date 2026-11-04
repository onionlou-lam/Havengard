using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Havengard.Vendors.UI
{
    public class VendorOfferRow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button buyButton;

        [Header("Colors")]
        [SerializeField] private Color affordableColor = Color.white;
        [SerializeField] private Color unaffordableColor = new Color(0.9f, 0.3f, 0.3f);

        private IVendorOffer offer;
        private bool available;
        private Action<IVendorOffer> onBuy;
        private Action onHover;
        private Action onHoverExit;
        private bool hovering;

        public void Setup(IVendorOffer offer, bool available, bool affordable, string unavailableReason,
            Action<IVendorOffer> onBuy, Action onHoverCallback, Action onHoverExitCallback)
        {
            this.offer = offer;
            this.available = available;
            this.onBuy = onBuy;
            onHover = onHoverCallback;
            onHoverExit = onHoverExitCallback;

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
                buyButton.onClick.AddListener(() => this.onBuy(this.offer));
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovering = true;
            if (buyButton != null && buyButton.IsInteractable()) buyButton.Select();
            if (onHover != null) onHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovering = false;
            if (onHoverExit != null) onHoverExit();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right) return;
            if (!available || offer == null || onBuy == null) return;

            onBuy(offer);
        }

        private void OnDisable()
        {
            // A purchase rebuilds the rows, which destroys the hovered row before it gets an exit event.
            if (hovering && onHoverExit != null) onHoverExit();
            hovering = false;
        }
    }
}