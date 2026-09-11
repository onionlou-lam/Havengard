using UnityEngine;
using System.Collections;
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

        private bool subscribedToGold;
        private bool subscribedToCelestium;
        private Coroutine waitForSystemsRoutine;

        private void OnEnable()
        {
            TrySubscribe();

            // If either system wasn't ready yet, keep trying until it is.
            if (!subscribedToGold || !subscribedToCelestium)
            {
                if (waitForSystemsRoutine != null)
                    StopCoroutine(waitForSystemsRoutine);

                waitForSystemsRoutine = StartCoroutine(WaitForSystemsAndSubscribe());
            }

            // Immediate refresh (so it shows correct values on scene load)
            RefreshAll();
        }

        private void OnDisable()
        {
            if (waitForSystemsRoutine != null)
            {
                StopCoroutine(waitForSystemsRoutine);
                waitForSystemsRoutine = null;
            }

            Unsubscribe();
        }

        private void TrySubscribe()
        {
            if (!subscribedToGold && GoldSystem.Instance != null)
            {
                GoldSystem.Instance.OnGoldChanged += HandleGoldChanged;
                subscribedToGold = true;
            }

            if (!subscribedToCelestium && CelestiumSystem.Instance != null)
            {
                CelestiumSystem.Instance.OnCelestiumChanged += HandleCelestiumChanged;
                subscribedToCelestium = true;
            }
        }

        private void Unsubscribe()
        {
            if (subscribedToGold && GoldSystem.Instance != null)
                GoldSystem.Instance.OnGoldChanged -= HandleGoldChanged;

            if (subscribedToCelestium && CelestiumSystem.Instance != null)
                CelestiumSystem.Instance.OnCelestiumChanged -= HandleCelestiumChanged;

            subscribedToGold = false;
            subscribedToCelestium = false;
        }

        /// <summary>
        /// Keeps checking each frame until GoldSystem/CelestiumSystem singletons exist,
        /// then subscribes and refreshes immediately. Handles cases where this HUD's
        /// OnEnable ran before those systems finished initializing (e.g. GameManager.Awake
        /// creating them, or scene load order).
        /// </summary>
        private IEnumerator WaitForSystemsAndSubscribe()
        {
            while (!subscribedToGold || !subscribedToCelestium)
            {
                TrySubscribe();

                if (subscribedToGold && subscribedToCelestium)
                {
                    RefreshAll();
                    break;
                }

                yield return null;
            }

            waitForSystemsRoutine = null;
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
