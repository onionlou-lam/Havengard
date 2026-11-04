using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Havengard.UI
{
    /// <summary>
    /// Put on a modal panel. Keeps a selectable highlighted while the panel is open
    /// and (optionally) makes up/down navigation wrap around.
    /// </summary>
    [DisallowMultipleComponent]
    public class UINavigationScope : MonoBehaviour
    {
        [Tooltip("Optional. Defaults to the top-most interactable selectable in this panel.")]
        [SerializeField] private Selectable firstSelected;
        [Tooltip("Up on the first item goes to the last, and vice versa.")]
        [SerializeField] private bool wrapVertical = true;

        private static readonly List<UINavigationScope> Stack = new List<UINavigationScope>();

        private readonly List<Selectable> items = new List<Selectable>();
        private readonly List<Selectable> scratch = new List<Selectable>();
        private int lastIndex = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            Stack.Clear();
        }

        private void OnEnable()
        {
            lastIndex = -1;
            Stack.Add(this);
        }

        private void OnDisable()
        {
            Stack.Remove(this);
            items.Clear();
        }

        private void Update()
        {
            if (Stack.Count == 0 || Stack[Stack.Count - 1] != this) return;

            EventSystem es = EventSystem.current;
            if (es == null) return;

            Refresh();
            if (items.Count == 0) return;

            int index = IndexOf(es.currentSelectedGameObject);
            if (index >= 0)
            {
                lastIndex = index;
                return;
            }

            // Nothing (or something outside this panel) is selected. Pick first, or keep the old position.
            Selectable target = lastIndex < 0 && firstSelected != null && firstSelected.IsInteractable()
                ? firstSelected
                : items[Mathf.Clamp(lastIndex, 0, items.Count - 1)];

            es.SetSelectedGameObject(target.gameObject);
        }

        private int IndexOf(GameObject go)
        {
            if (go == null) return -1;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].gameObject == go) return i;
            }
            return -1;
        }

        private void Refresh()
        {
            scratch.Clear();
            GetComponentsInChildren(false, scratch);
            scratch.RemoveAll(s => s is Scrollbar || !s.IsInteractable());

            scratch.Sort((a, b) =>
            {
                int byY = b.transform.position.y.CompareTo(a.transform.position.y);
                return byY != 0 ? byY : a.transform.position.x.CompareTo(b.transform.position.x);
            });

            if (SameAsItems()) return;

            items.Clear();
            items.AddRange(scratch);
            if (wrapVertical) ApplyWrapNavigation();
        }

        private bool SameAsItems()
        {
            if (scratch.Count != items.Count) return false;
            for (int i = 0; i < items.Count; i++)
            {
                if (scratch[i] != items[i]) return false;
            }
            return true;
        }

        private void ApplyWrapNavigation()
        {
            int n = items.Count;
            for (int i = 0; i < n; i++)
            {
                Navigation nav = new Navigation();
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = n > 1 ? items[(i - 1 + n) % n] : null;
                nav.selectOnDown = n > 1 ? items[(i + 1) % n] : null;
                items[i].navigation = nav;
            }
        }
    }
}