using UnityEngine;

namespace Havengard.UI
{
    /// <summary>Put on a row root so the arrow points at the whole row instead of the button inside it.</summary>
    public class UISelectionAnchor : MonoBehaviour
    {
        [Tooltip("Leave empty to use this object's own RectTransform.")]
        [SerializeField] private RectTransform target;

        public RectTransform Target
        {
            get { return target != null ? target : (RectTransform)transform; }
        }
    }
}