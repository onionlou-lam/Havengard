using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Havengard.UI
{
    /// <summary>
    /// Universal selection indicator. Follows whatever the EventSystem has selected
    /// (keyboard / controller / hover) and pulses beside it.
    /// </summary>
    public class UISelectionArrow : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Arrow graphic under a high-sorting Overlay canvas. Pivot (1, 0.5), Raycast Target off, points right.")]
        [SerializeField] private RectTransform arrow;

        [Header("Placement")]
        [SerializeField] private Vector2 offset = new Vector2(-8f, 0f);
        [SerializeField] private float followSpeed = 30f;

        [Header("Pulse")]
        [SerializeField] private float pulseDistance = 6f;
        [SerializeField] private float pulseSpeed = 6f;
        [SerializeField] private float pulseScale = 0.12f;

        private readonly Vector3[] corners = new Vector3[4];
        private RectTransform arrowParent;
        private Camera arrowCamera;
        private bool visible;

        private void Awake()
        {
            if (arrow == null)
            {
                Debug.LogError("[UISelectionArrow] Arrow is not assigned.", this);
                enabled = false;
                return;
            }

            arrowParent = arrow.parent as RectTransform;
            arrowCamera = GetCanvasCamera(arrow);
            arrow.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            RectTransform target = GetTarget();

            if (target == null)
            {
                SetVisible(false);
                return;
            }

            target.GetWorldCorners(corners);
            Vector3 leftMiddle = (corners[0] + corners[1]) * 0.5f;

            Vector2 screen = RectTransformUtility.WorldToScreenPoint(GetCanvasCamera(target), leftMiddle);
            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(arrowParent, screen, arrowCamera, out local))
            {
                SetVisible(false);
                return;
            }

            float wave = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
            Vector3 desired = new Vector3(local.x + offset.x + wave * pulseDistance, local.y + offset.y, 0f);

            bool wasVisible = visible;
            SetVisible(true);

            // Snap when first shown, slide between rows afterwards.
            arrow.localPosition = wasVisible
                ? Vector3.Lerp(arrow.localPosition, desired, 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime))
                : desired;

            arrow.localScale = Vector3.one * (1f + wave * pulseScale);
        }

        private static RectTransform GetTarget()
        {
            EventSystem es = EventSystem.current;
            if (es == null) return null;

            GameObject selected = es.currentSelectedGameObject;
            if (selected == null || !selected.activeInHierarchy) return null;

            Selectable selectable = selected.GetComponent<Selectable>();
            if (selectable != null && !selectable.IsInteractable()) return null;

            UISelectionAnchor anchor = selected.GetComponentInParent<UISelectionAnchor>();
            if (anchor != null) return anchor.Target;

            return selected.transform as RectTransform;
        }

        private static Camera GetCanvasCamera(Component c)
        {
            Canvas canvas = c.GetComponentInParent<Canvas>();
            if (canvas == null) return null;
            canvas = canvas.rootCanvas;
            return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }

        private void SetVisible(bool value)
        {
            if (visible == value) return;
            visible = value;
            arrow.gameObject.SetActive(value);
        }
    }
}