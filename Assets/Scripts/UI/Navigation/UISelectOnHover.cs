using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Havengard.UI
{
    [RequireComponent(typeof(Selectable))]
    public class UISelectOnHover : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            Selectable selectable = GetComponent<Selectable>();
            if (selectable != null && selectable.IsInteractable()) selectable.Select();
        }
    }
}