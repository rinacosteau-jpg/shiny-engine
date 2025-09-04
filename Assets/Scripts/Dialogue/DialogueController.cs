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
    
#if ARTICY_RUNTIME || true
using System.Collections.Generic;
using Articy.Unity;
using Articy.Unity.Interfaces;
#endif

namespace Dialogue
{
    public partial class DialogueController
    {
#if ARTICY_RUNTIME || true
        private Component articyListener;
        private object gvEventSource;
        private System.Delegate gvEventHandler;
        private System.Delegate flowFinishedHandler;

        private void OnEnable()
        {
            if (DialogueConfig.Active.UseController)
                TryHookArticy();
        }

        private void OnDisable()
        {
            TryUnhookArticy();
        }

        // Listener methods (log-only)
        public void OnNodeEnter(object node)
        {
            Debug.Log($"[DialogueController] OnNodeEnter: {node}");
        }

        public void OnChoicesReady(IList<Branch> branches)
        {
            Debug.Log($"[DialogueController] OnChoicesReady: {branches?.Count ?? 0} choices");
        }

        public void OnChoiceSelected(object choice)
        {
            Debug.Log($"[DialogueController] OnChoiceSelected: {choice}");
        }

        public void OnVariablesChanged(string name, object value)
        {
            Debug.Log($"[DialogueController] OnVariablesChanged: {name}={value}");
        }

        public void OnDialogueEnded()
        {
            Debug.Log("[DialogueController] OnDialogueEnded");
        }

        private void TryHookArticy()
        {
            try
            {
                var flow = FindObjectOfType<ArticyFlowPlayer>();
                if (flow != null)
                {
                    var go = flow.gameObject;
                    var existing = go.GetComponent<DialogueArticyListener>();
                    if (existing == null)
                    {
                        existing = go.AddComponent<DialogueArticyListener>();
                    }
                    existing.Target = this;
                    articyListener = existing;

                    // Try subscribe to a possible "Finished" event via reflection (if present)
                    var evt = flow.GetType().GetEvent("Finished") ?? flow.GetType().GetEvent("OnFinished") ?? flow.GetType().GetEvent("FlowEnded");
                    if (evt != null)
                    {
                        var mi = GetType().GetMethod(nameof(HandleFlowFinished), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        flowFinishedHandler = System.Delegate.CreateDelegate(evt.EventHandlerType, this, mi);
                        evt.AddEventHandler(flow, flowFinishedHandler);
                    }
                }

                // Try subscribe to variables changed via reflection on global variables
                var gvDefaultProp = System.Type.GetType("Articy.World_Of_Red_Moon.GlobalVariables.ArticyGlobalVariables, ArticyRuntime")?.GetProperty("Default", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var gvDefault = gvDefaultProp?.GetValue(null);
                if (gvDefault != null)
                {
                    // Common event names
                    var ev = gvDefault.GetType().GetEvent("VariableChanged") ?? gvDefault.GetType().GetEvent("OnVariableChanged") ?? gvDefault.GetType().GetEvent("VariablesChanged");
                    if (ev != null)
                    {
                        var mi = GetType().GetMethod(nameof(HandleGVChanged), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        gvEventHandler = System.Delegate.CreateDelegate(ev.EventHandlerType, this, mi, false);
                        if (gvEventHandler != null)
                        {
                            ev.AddEventHandler(gvDefault, gvEventHandler);
                            gvEventSource = gvDefault;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("[DialogueController] HookArticy skipped: " + e.Message);
            }
        }

        private void TryUnhookArticy()
        {
            try
            {
                if (articyListener != null)
                {
                    var go = articyListener.gameObject;
                    Destroy(articyListener);
                    articyListener = null;
                }

                if (gvEventSource != null && gvEventHandler != null)
                {
                    var ev = gvEventSource.GetType().GetEvent("VariableChanged") ?? gvEventSource.GetType().GetEvent("OnVariableChanged") ?? gvEventSource.GetType().GetEvent("VariablesChanged");
                    if (ev != null)
                    {
                        ev.RemoveEventHandler(gvEventSource, gvEventHandler);
                    }
                }
                gvEventSource = null;
                gvEventHandler = null;
                flowFinishedHandler = null;
            }
            catch (System.Exception e)
            {
                Debug.Log("[DialogueController] UnhookArticy skipped: " + e.Message);
            }
        }

        // Reflection event handlers (signatures are unknown; accept object, EventArgs, etc.)
        private void HandleFlowFinished(object sender, System.EventArgs e)
        {
            OnDialogueEnded();
        }

        private void HandleGVChanged(object arg1)
        {
            OnVariablesChanged(arg1?.ToString(), null);
        }

        private void HandleGVChanged(object sender, System.EventArgs e)
        {
            OnVariablesChanged("<unknown>", null);
        }
#endif
    }
}

#if ARTICY_RUNTIME || true
public class DialogueArticyListener : MonoBehaviour, IArticyFlowPlayerCallbacks
{
    public Dialogue.DialogueController Target { get; set; }

    public void OnFlowPlayerPaused(IFlowObject aObject)
    {
        Target?.OnNodeEnter(aObject);
    }

    public void OnBranchesUpdated(IList<Branch> aBranches)
    {
        Target?.OnChoicesReady(aBranches);
    }
}
#endif