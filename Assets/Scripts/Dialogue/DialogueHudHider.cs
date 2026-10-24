using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Dialogue
{
    /// <summary>Hides the listed HUD objects while a dialogue is running, then restores them.</summary>
    public class DialogueHudHider : MonoBehaviour
    {
        [Tooltip("HUD canvases or panels to hide during dialogue. Do NOT include the dialogue bubble.")]
        [SerializeField] private List<GameObject> hudRoots = new List<GameObject>();

        [Range(0f, 1f)]
        [SerializeField] private float hiddenAlpha = 0f;

        private struct SavedState
        {
            public CanvasGroup group;
            public float alpha;
            public bool interactable;
            public bool blocksRaycasts;
            public bool addedByUs;
        }

        private readonly List<SavedState> saved = new List<SavedState>();
        private bool hidden;

        private void OnEnable()
        {
            DialogueManager.ActiveChanged += HandleActiveChanged;
        }

        private void OnDisable()
        {
            DialogueManager.ActiveChanged -= HandleActiveChanged;
            Restore();
        }

        private void HandleActiveChanged(bool active)
        {
            if (active) Hide();
            else Restore();
        }

        private void Hide()
        {
            if (hidden) return;
            hidden = true;

            foreach (GameObject root in hudRoots)
            {
                if (root == null) continue;

                CanvasGroup group = root.GetComponent<CanvasGroup>();
                bool added = false;
                if (group == null)
                {
                    group = root.AddComponent<CanvasGroup>();
                    added = true;
                }

                saved.Add(new SavedState
                {
                    group = group,
                    alpha = group.alpha,
                    interactable = group.interactable,
                    blocksRaycasts = group.blocksRaycasts,
                    addedByUs = added
                });

                group.alpha = hiddenAlpha;
                group.interactable = false;
                group.blocksRaycasts = false;
            }
        }

        private void Restore()
        {
            if (!hidden) return;
            hidden = false;

            foreach (SavedState state in saved)
            {
                if (state.group == null) continue;

                if (state.addedByUs)
                {
                    Destroy(state.group);
                    continue;
                }

                state.group.alpha = state.alpha;
                state.group.interactable = state.interactable;
                state.group.blocksRaycasts = state.blocksRaycasts;
            }

            saved.Clear();
        }
    }
}