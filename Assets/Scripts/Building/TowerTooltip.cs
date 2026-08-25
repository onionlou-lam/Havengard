using UnityEngine;
using TMPro;

namespace Havengard.Building
{
    /// <summary>
    /// Tooltip displaying tower information on hover
    /// </summary>
    public class TowerTooltip : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI footprintText;
        [SerializeField] private RectTransform tooltipRect;

        [Header("Positioning")]
        [SerializeField] private Vector2 offset = new Vector2(10f, -10f);
        [SerializeField] private bool followMouse = true;

        private bool isVisible = false;
        private Canvas canvas;

        private void Awake()
        {
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);

            if (tooltipRect == null)
                tooltipRect = GetComponent<RectTransform>();

            canvas = GetComponentInParent<Canvas>();
        }

        private void Update()
        {
            if (isVisible && followMouse)
            {
                UpdatePosition();
            }
        }

        public void ShowTooltip(TowerBuildData towerData)
        {
            if (towerData == null)
                return;

            isVisible = true;

            // Update content
            if (titleText != null)
            {
                titleText.text = towerData.displayName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = towerData.description;
            }

            // Get level 1 stats
            var levelData = towerData.GetLevelData(0);
            if (levelData != null)
            {
                if (statsText != null)
                {
                    statsText.text = BuildStatsText(levelData);
                }
            }

            if (footprintText != null)
            {
                footprintText.text = $"Footprint: {towerData.gridWidth} x {towerData.gridHeight}";
            }

            // Show panel
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(true);
            }

            UpdatePosition();
        }

        public void HideTooltip()
        {
            isVisible = false;

            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }

        private string BuildStatsText(TowerLevelData levelData)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine($"<b>Cost:</b> {levelData.buildCost} Gold");
            sb.AppendLine();
            sb.AppendLine($"<b>Damage:</b> {levelData.damage}");
            sb.AppendLine($"<b>Range:</b> {levelData.attackRange}");
            sb.AppendLine($"<b>Attack Speed:</b> {levelData.attackSpeed}/s");

            return sb.ToString();
        }

        private void UpdatePosition()
        {
            if (tooltipRect == null || canvas == null)
                return;

            Vector2 mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out mousePosition
            );

            tooltipRect.localPosition = mousePosition + offset;

            // Keep tooltip on screen
            ClampToScreen();
        }

        private void ClampToScreen()
        {
            if (tooltipRect == null || canvas == null)
                return;

            Vector3[] corners = new Vector3[4];
            tooltipRect.GetWorldCorners(corners);

            RectTransform canvasRect = canvas.transform as RectTransform;
            Vector3[] canvasCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);

            // Clamp horizontally
            float rightOverflow = corners[2].x - canvasCorners[2].x;
            float leftOverflow = canvasCorners[0].x - corners[0].x;

            if (rightOverflow > 0)
            {
                Vector3 pos = tooltipRect.localPosition;
                pos.x -= rightOverflow;
                tooltipRect.localPosition = pos;
            }
            else if (leftOverflow > 0)
            {
                Vector3 pos = tooltipRect.localPosition;
                pos.x += leftOverflow;
                tooltipRect.localPosition = pos;
            }

            // Clamp vertically
            float topOverflow = corners[1].y - canvasCorners[1].y;
            float bottomOverflow = canvasCorners[0].y - corners[0].y;

            if (topOverflow > 0)
            {
                Vector3 pos = tooltipRect.localPosition;
                pos.y -= topOverflow;
                tooltipRect.localPosition = pos;
            }
            else if (bottomOverflow > 0)
            {
                Vector3 pos = tooltipRect.localPosition;
                pos.y += bottomOverflow;
                tooltipRect.localPosition = pos;
            }
        }
    }
}