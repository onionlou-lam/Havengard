using System;
using System.Collections.Generic;
using Havengard.Core;
using Havengard.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Havengard.Vendors.UI
{
    /// <summary>Generic shop window. Keep this component on an always-active object and the visuals under 'panel'.</summary>
    [RequireComponent(typeof(AudioSource))]
    public class VendorShopUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform rowContainer;
        [SerializeField] private VendorOfferRow rowPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI celestiumText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private GameObject emptyLabel;

        [Header("Input")]
        [SerializeField] private KeyCode closeKey = KeyCode.Escape;

        [Header("Audio")]
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip closeClip;
        [SerializeField] private AudioClip rowHoverClip;
        [SerializeField] private AudioClip purchaseClip;
        [SerializeField] private AudioClip insufficientFundsClip;
        [SerializeField] private AudioClip errorClip;
        [Range(0f, 1f)] [SerializeField] private float volume = 0.8f;

        private readonly List<GameObject> rows = new List<GameObject>();
        private readonly VendorTransactionService transactions = new VendorTransactionService(new CurrencyService());
        private ICurrencyService currency = new CurrencyService();

        private AudioSource audioSource;
        private VendorCatalog catalog;
        private VendorContext context;
        private Action onClosed;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            if (panel != null) panel.SetActive(false);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        private void Update()
        {
            if (IsOpen && Input.GetKeyDown(closeKey)) Close();
        }

        private void OnDestroy()
        {
            if (IsOpen) Close();
        }

        public void Open(string vendorName, VendorCatalog vendorCatalog, VendorContext vendorContext, Action closedCallback)
        {
            if (IsOpen || vendorCatalog == null) return;

            catalog = vendorCatalog;
            context = vendorContext;
            onClosed = closedCallback;
            IsOpen = true;

            if (titleText != null) titleText.text = vendorName;
            SetMessage(string.Empty);
            panel.SetActive(true);
            PlayerInputLock.Acquire();

            if (GoldSystem.Instance != null) GoldSystem.Instance.OnGoldChanged += OnCurrencyChanged;
            if (CelestiumSystem.Instance != null) CelestiumSystem.Instance.OnCelestiumChanged += OnCurrencyChanged;

            RefreshCurrency();
            Rebuild();
            Play(openClip);
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;

            if (GoldSystem.Instance != null) GoldSystem.Instance.OnGoldChanged -= OnCurrencyChanged;
            if (CelestiumSystem.Instance != null) CelestiumSystem.Instance.OnCelestiumChanged -= OnCurrencyChanged;

            ClearRows();
            panel.SetActive(false);
            PlayerInputLock.Release();
            Play(closeClip);

            Action callback = onClosed;
            onClosed = null;
            if (callback != null) callback();
        }

        private void OnBuyClicked(IVendorOffer offer)
        {
            string reason;
            PurchaseResult result = transactions.TryPurchase(offer, out reason);

            switch (result)
            {
                case PurchaseResult.Success:
                    Play(purchaseClip);
                    SetMessage("Purchased " + offer.DisplayName + ".");
                    break;
                case PurchaseResult.InsufficientFunds:
                    Play(insufficientFundsClip);
                    SetMessage(reason);
                    break;
                default:
                    Play(errorClip);
                    SetMessage(reason);
                    break;
            }

            Rebuild();
        }

        private void Rebuild()
        {
            ClearRows();
            IList<IVendorOffer> offers = catalog.BuildOffers(context);

            if (emptyLabel != null) emptyLabel.SetActive(offers.Count == 0);

            foreach (IVendorOffer offer in offers)
            {
                string reason;
                bool available = offer.CanPurchase(out reason);
                bool affordable = currency.CanAfford(offer.Price);

                VendorOfferRow row = Instantiate(rowPrefab, rowContainer);
                row.Setup(offer, available, affordable, reason, OnBuyClicked, () => Play(rowHoverClip));
                rows.Add(row.gameObject);
            }
        }

        private void ClearRows()
        {
            foreach (GameObject row in rows)
            {
                if (row != null) Destroy(row);
            }
            rows.Clear();
        }

        private void OnCurrencyChanged(int _)
        {
            RefreshCurrency();
        }

        private void RefreshCurrency()
        {
            if (goldText != null && GoldSystem.Instance != null)
                goldText.text = "Gold: " + GoldSystem.Instance.CurrentGold;
            if (celestiumText != null && CelestiumSystem.Instance != null)
                celestiumText.text = "Celestium: " + CelestiumSystem.Instance.CurrentCelestium;
        }

        private void SetMessage(string message)
        {
            if (messageText != null) messageText.text = message;
        }

        private void Play(AudioClip clip)
        {
            if (clip != null) audioSource.PlayOneShot(clip, volume);
        }
    }
}