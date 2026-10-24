using cherrydev;
using UnityEngine;

namespace Havengard.Dialogue
{
    /// <summary>Shows the whole dialogue UI (bubble + portrait) while a conversation runs and hides it afterwards.</summary>
    public class DialoguePanelVisibility : MonoBehaviour
    {
        [SerializeField] private DialogBehaviour dialogBehaviour;
        [Tooltip("Root of the bubble and portrait (DialoguePanel).")]
        [SerializeField] private GameObject panelRoot;

        private void Awake()
        {
            panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            dialogBehaviour.OnDialogStarted.AddListener(Show);
            dialogBehaviour.OnDialogFinished.AddListener(Hide);
        }

        private void OnDisable()
        {
            dialogBehaviour.OnDialogStarted.RemoveListener(Show);
            dialogBehaviour.OnDialogFinished.RemoveListener(Hide);
        }

        private void Show() { panelRoot.SetActive(true); }
        private void Hide() { panelRoot.SetActive(false); }
    }
}