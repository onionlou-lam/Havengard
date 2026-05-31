using UnityEngine;
using Havengard.Items;
using System.Collections.Generic;

namespace Havengard.UI
{
    /// <summary>
    /// Spawns floating text when items are picked up
    /// </summary>
    public class ItemPickupNumberSpawner : MonoBehaviour
    {
        public static ItemPickupNumberSpawner Instance { get; private set; }

        [Header("Prefab")]
        [SerializeField] private GameObject pickupNumberPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private Vector3 worldSpaceOffset = new Vector3(0, 1f, 0);
        [SerializeField] private Vector2 randomOffset = new Vector2(0.5f, 0.2f);

        [Header("Canvas")]
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private RectTransform canvasRectTransform;

        private Camera mainCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            mainCamera = Camera.main;

            if (targetCanvas == null)
            {
                targetCanvas = FindObjectOfType<Canvas>();
            }

            if (canvasRectTransform == null && targetCanvas != null)
            {
                canvasRectTransform = targetCanvas.GetComponent<RectTransform>();
            }

            //Debug.Log($"[ItemPickupNumberSpawner] Initialized");
        }

        /// <summary>
        /// Spawn a pickup number with stat bonuses
        /// </summary>
        public void SpawnPickupNumber(ItemData itemData, Vector3 worldPosition, int itemLevel = 1)
        {
            if (pickupNumberPrefab == null || itemData == null || targetCanvas == null || canvasRectTransform == null)
            {
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            // Calculate position
            Vector3 spawnWorldPos = worldPosition + worldSpaceOffset;
            spawnWorldPos.x += Random.Range(-randomOffset.x, randomOffset.x);
            spawnWorldPos.y += Random.Range(-randomOffset.y, randomOffset.y);

            Vector3 screenPoint = mainCamera.WorldToScreenPoint(spawnWorldPos);
            if (screenPoint.z < 0) return;

            Vector2 canvasPos;
            bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                screenPoint,
                targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out canvasPos
            );

            if (!success) return;

            // Get stat bonuses from effects
            List<string> statBonuses = new List<string>();
            if (itemData.effects != null)
            {
                foreach (var effect in itemData.effects)
                {
                    if (effect == null) continue;

                    if (effect is StatModifierEffect statMod)
                    {
                        statBonuses.Add(statMod.GetStatBonusText(itemLevel));
                    }
                    else if (effect is AbilityModifierEffect abilityMod)
                    {
                        statBonuses.Add(abilityMod.GetAbilityBonusText(itemLevel));
                    }
                }
            }

            // Instantiate
            GameObject numberObj = Instantiate(pickupNumberPrefab, targetCanvas.transform);
            
            RectTransform numberRect = numberObj.GetComponent<RectTransform>();
            if (numberRect != null)
            {
                numberRect.anchoredPosition = canvasPos;
            }

            // Initialize with stats
            ItemPickupNumber pickupNumber = numberObj.GetComponent<ItemPickupNumber>();
            if (pickupNumber != null)
            {
                pickupNumber.Initialize(itemData.itemName, itemData.rarityColor, canvasPos, statBonuses);
            }
            else
            {
                Destroy(numberObj);
            }
        }

        public void SpawnPickupNumber(ItemInstance item, Vector3 worldPosition)
        {
            if (item != null && item.itemData != null)
            {
                SpawnPickupNumber(item.itemData, worldPosition, item.level);
            }
        }
    }
}