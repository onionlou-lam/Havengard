using UnityEngine;
using Yarn.Unity;

namespace Havengard.Dialogue
{
    /// <summary>Advances Yarn dialogue using the legacy Input Manager (Input.GetKeyDown).</summary>
    public class DialogueLegacyInput : MonoBehaviour
    {
        [SerializeField] private DialogueRunner runner;
        [SerializeField] private HavengardDialoguePresenter presenter;

        [Header("Input")]
        [SerializeField] private KeyCode[] advanceKeys = { KeyCode.Space, KeyCode.Return };
        [SerializeField] private bool allowMouseClick = true;

        private void Update()
        {
            if (runner == null || presenter == null) return;
            if (!runner.IsDialogueRunning || presenter.IsShowingOptions) return;
            if (!AdvancePressed()) return;

            if (presenter.IsTyping)
                runner.RequestHurryUpLine();
            else
                runner.RequestNextLine();
        }

        private bool AdvancePressed()
        {
            foreach (KeyCode key in advanceKeys)
            {
                if (Input.GetKeyDown(key)) return true;
            }

            return allowMouseClick && Input.GetMouseButtonDown(0);
        }
    }
}