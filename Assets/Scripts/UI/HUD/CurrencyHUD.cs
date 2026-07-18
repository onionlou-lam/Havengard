using UnityEngine;
using TMPro;
using Havengard.Resources;

namespace Havengard.UI
{
    public class CurrencyHUD : MonoBehaviour
    {
        [Header("Text References (TMP)")]
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text celestiumText;

        [Header("Formatting")]
        [SerializeField] private string goldPrefix = "Gold: ";
        [SerializeField] private string celestiumPrefix = "Celestium: ";

        private void OnEnable()
        {
            // Subscribe
            if (GoldSystem.Instance != null)
                GoldSystem.Instance.OnGoldChanged += HandleGoldChanged;

            if (CelestiumSystem.Instance != null)
                CelestiumSystem.Instance.OnCelestiumChanged += HandleCelestiumChanged;

            // Immediate refresh (so it shows correct values on scene load)
            RefreshAll();
        }

        private void OnDisable()
        {
            // Unsubscribe
            if (GoldSystem.Instance != null)
                GoldSystem.Instance.OnGoldChanged -= HandleGoldChanged;

            if (CelestiumSystem.Instance != null)
                CelestiumSystem.Instance.OnCelestiumChanged -= HandleCelestiumChanged;
        }

        private void RefreshAll()
        {
            if (GoldSystem.Instance != null)
                HandleGoldChanged(GoldSystem.Instance.Current);

            if (CelestiumSystem.Instance != null)
                HandleCelestiumChanged(CelestiumSystem.Instance.Current);
        }

        private void HandleGoldChanged(int current)
        {
            if (goldText != null)
                goldText.text = $"{goldPrefix}{current}";
        }

        private void HandleCelestiumChanged(int current)
        {
            if (celestiumText != null)
                celestiumText.text = $"{celestiumPrefix}{current}";
        }
    }
}
