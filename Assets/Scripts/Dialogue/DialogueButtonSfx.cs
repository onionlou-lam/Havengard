using UnityEngine;
using UnityEngine.EventSystems;

namespace Havengard.Dialogue
{
    /// <summary>Put on the answer button prefab used by the package's AnswerPanel.</summary>
    public class DialogueButtonSfx : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (DialogueAudio.Instance != null) DialogueAudio.Instance.PlayChoiceHover();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (DialogueAudio.Instance != null) DialogueAudio.Instance.PlayChoiceSelect();
        }
    }
}