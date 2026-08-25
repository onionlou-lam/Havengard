using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Havengard.Building
{
    /// <summary>
    /// UI panel showing available towers for building
    /// Dynamically creates buttons from TowerBuildDatabase
    /// </summary>
    public class TowerSelectionPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TowerBuildDatabase towerDatabase;
        [SerializeField] private Transform towerButtonContainer;
        [SerializeField] private GameObject towerButtonPrefab;

        [Header("Tooltip")]
        [SerializeField] private TowerTooltip tooltip;

        private List<TowerButton> towerButtons = new List<TowerButton>();

        private void Start()
        {
            // Auto-find references
            if (towerDatabase == null)
            {
                Debug.LogWarning("[TowerSelectionPanel] No TowerBuildDatabase assigned");
            }

            if (tooltip == null)
                tooltip = FindFirstObjectByType<TowerTooltip>();

            // Generate tower buttons
            GenerateTowerButtons();
        }

        private void GenerateTowerButtons()
        {
            if (towerDatabase == null || towerDatabase.towers == null)
            {
                Debug.LogWarning("[TowerSelectionPanel] No towers in database");
                return;
            }

            // Clear existing buttons
            ClearButtons();

            // Create button for each tower
            foreach (var towerData in towerDatabase.towers)
            {
                if (towerData == null)
                    continue;

                CreateTowerButton(towerData);
            }

            Debug.Log($"[TowerSelectionPanel] Created {towerButtons.Count} tower buttons");
        }

        private void CreateTowerButton(TowerBuildData towerData)
        {
            GameObject buttonObj;

            if (towerButtonPrefab != null)
            {
                buttonObj = Instantiate(towerButtonPrefab, towerButtonContainer);
            }
            else
            {
                // Create default button if no prefab
                buttonObj = CreateDefaultButton();
            }

            buttonObj.name = $"TowerButton_{towerData.towerID}";

            // Setup TowerButton component
            TowerButton towerButton = buttonObj.GetComponent<TowerButton>();
            if (towerButton == null)
                towerButton = buttonObj.AddComponent<TowerButton>();

            towerButton.Initialize(towerData, this, tooltip);
            towerButtons.Add(towerButton);
        }

        private GameObject CreateDefaultButton()
        {
            GameObject buttonObj = new GameObject("TowerButton");
            buttonObj.transform.SetParent(towerButtonContainer);

            // Add Image component
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.3f);

            // Add Button component
            Button button = buttonObj.AddComponent<Button>();

            // Add RectTransform
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80f, 80f);

            return buttonObj;
        }

        private void ClearButtons()
        {
            foreach (var button in towerButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            towerButtons.Clear();
        }

        public void OnTowerSelected(TowerBuildData towerData)
        {
            if (BuildingModeController.Instance != null)
            {
                BuildingModeController.Instance.SelectTower(towerData);
            }
        }

        private void OnDestroy()
        {
            ClearButtons();
        }
    }
}