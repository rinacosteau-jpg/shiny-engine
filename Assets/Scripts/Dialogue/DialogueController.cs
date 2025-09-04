using System;
using UnityEngine;

namespace Dialogue
{
    public enum DialogueState { Idle, Opening, Active, Transition, Closing }

    [DisallowMultipleComponent]
    public class DialogueController : MonoBehaviour
    {
        [SerializeField] private DialogueState state = DialogueState.Idle;

        public event Action OnOpened;
        public event Action OnClosed;

        public bool IsActive => state == DialogueState.Active;

        private DialogueConfig Config => DialogueConfig.Active;

        // No input interception when UseController == false.
        private void Update()
        {
            if (!Config.UseController)
                return; // passive unless explicitly enabled via config

            // Intentionally left passive for now: do not change existing behavior.
            // Placeholder for future input handling when controller is enabled.
        }

        public void OpenDialogue(object context = null)
        {
            if (state != DialogueState.Idle && state != DialogueState.Closing)
            {
                Debug.Log("[DialogueController] OpenDialogue ignored: not idle");
                return;
            }

            Debug.Log("[DialogueController] Opening dialogue" + (context != null ? $" with context: {context}" : string.Empty));
            state = DialogueState.Opening;

            // Immediately transition to Active for now (no side effects)
            state = DialogueState.Active;
            Debug.Log("[DialogueController] Dialogue Active");
            try { OnOpened?.Invoke(); } catch (Exception e) { Debug.LogException(e); }
        }

        public void CloseDialogue()
        {
            if (state == DialogueState.Idle)
            {
                Debug.Log("[DialogueController] CloseDialogue ignored: already idle");
                return;
            }

            Debug.Log("[DialogueController] Closing dialogue");
            state = DialogueState.Closing;

            // Immediately transition back to Idle for now (no side effects)
            state = DialogueState.Idle;
            Debug.Log("[DialogueController] Dialogue Closed -> Idle");
            try { OnClosed?.Invoke(); } catch (Exception e) { Debug.LogException(e); }
        }

        public void ForceRefresh()
        {
            Debug.Log("[DialogueController] ForceRefresh requested");
            // Placeholder: No-op to avoid impacting current systems.
        }
    }
}