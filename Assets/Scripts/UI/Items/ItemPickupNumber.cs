using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Havengard.UI
{
    /// <summary>
    /// Floating text that appears when an item is picked up
    /// </summary>
    public class ItemPickupNumber : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;

        [Header("Animation Settings")]
        [SerializeField] private float floatHeight = 100f;
        [SerializeField] private float lifetime = 2f; // Increased to show stats
        [SerializeField] private float fadeStartTime = 1f;
        [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Vector2 startPosition;
        private float spawnTime;

        private void Awake()
        {
            if (itemNameText == null)
            {
                itemNameText = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
        }

        public void Initialize(string itemName, Color rarityColor, Vector2 canvasPosition, List<string> statBonuses = null)
        {
            Debug.Log($"[ItemPickupNumber] Initialize - Item: {itemName}, Stats: {statBonuses?.Count ?? 0}");

            if (itemNameText == null)
            {
                itemNameText = GetComponentInChildren<TextMeshProUGUI>();
                if (itemNameText == null)
                {
                    Debug.LogError("[ItemPickupNumber] No TextMeshProUGUI found!");
                    Destroy(gameObject);
                    return;
                }
            }

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            // Set item name
            itemNameText.text = $"+{itemName}";
            itemNameText.color = rarityColor;
            itemNameText.enabled = true;

            // Set stat bonuses
            if (statsText != null && statBonuses != null && statBonuses.Count > 0)
            {
                statsText.text = string.Join("\n", statBonuses);
                statsText.enabled = true;
                
                // Make stats text slightly smaller and white
                statsText.fontSize = itemNameText.fontSize * 0.75f;
                statsText.color = Color.white;
            }
            else if (statsText != null)
            {
                statsText.enabled = false;
            }

            // Ensure visible
            canvasGroup.alpha = 1f;

            // Set position
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = canvasPosition;
                startPosition = canvasPosition;
            }

            spawnTime = Time.time;

            // Start animation
            StartCoroutine(AnimateAndDestroy());
        }

        private IEnumerator AnimateAndDestroy()
        {
            float elapsed = 0f;

            while (elapsed < lifetime)
            {
                elapsed = Time.time - spawnTime;
                float progress = elapsed / lifetime;

                // Move upward with curve
                float curveValue = movementCurve.Evaluate(progress);
                Vector2 newPosition = startPosition + Vector2.up * (curveValue * floatHeight);

                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = newPosition;
                }

                // Fade out after fadeStartTime
                if (elapsed > fadeStartTime && canvasGroup != null)
                {
                    float fadeProgress = (elapsed - fadeStartTime) / (lifetime - fadeStartTime);
                    canvasGroup.alpha = 1f - fadeProgress;
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}