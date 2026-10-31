using UnityEngine;
using UnityEngine.EventSystems;

namespace Havengard.Dialogue
{
    /// <summary>
    /// Put on the answer button prefab. Plays the hover sound only.
    /// Click sounds (accept, decline, generic) are chosen by HavengardDialoguePresenter.
    /// </summary>
    public class DialogueButtonSfx : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (DialogueAudio.Instance != null) DialogueAudio.Instance.PlayChoiceHover();
        }
    }
}