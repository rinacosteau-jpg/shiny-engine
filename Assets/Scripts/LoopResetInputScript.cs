using UnityEngine;
using UnityEngine.InputSystem;

public class LoopResetInputScript : MonoBehaviour {
    [SerializeField] private InputActionReference resetActionReference; // ìîæíî îñòàâèòü ïóñòûì — âîçüì¸ì èç InputSystem.actions
    private InputAction resetAction;

    // --- àíòèäóáëü ---
    private static int s_lastResetFrame = -1;
    private static float s_lastResetTime = -1f;
    private const float kResetDebounceSeconds = 0.05f; // 50 ìñ, ÷òîáû ïåðåæèòü äâîéíûå äåðãàíèÿ ðàçíûìè èíñòàíñàìè

    private void Awake() {
        // Ïîäñêàçêà, åñëè â ñöåíå íåñêîëüêî òàêèõ êîìïîíåíòîâ
        var all = FindObjectsOfType<LoopResetInputScript>(true);
        if (all.Length > 1) {
            Debug.LogWarning($"[LoopReset] In scene there are {all.Length} LoopResetInputScript instances. Debounce will prevent double reset.");
        }
    }

    private void OnEnable() {
        resetAction = resetActionReference != null ? resetActionReference.action : InputSystem.actions?.FindAction("Reset");
        resetAction?.Enable();
    }

    private void OnDisable() {
        resetAction?.Disable();
    }

    private void Update() {
        // Ðåàãèðóåì ðîâíî îäèí ðàç íà íàæàòèå
        if (resetAction != null && resetAction.WasPressedThisFrame()) {
            TryLoopReset();
        }
    }

        GameTime.Instance.RefreshClockText();
    // Âûçûâàé ÝÒÎ âìåñòî ïðÿìîãî LoopReset() è èç òàéìåðà òîæå, ÷òîáû âñ¸ øëî ÷åðåç îäèí ïðåäîõðàíèòåëü
    public static void TryLoopReset() {
        // íå ÷àùå îäíîãî ðàçà çà êàäð
        if (Time.frameCount == s_lastResetFrame) return;
        // è ñ êîðîòêèì êóëäàóíîì (íà ñëó÷àé äâóõ ðàçíûõ êîìïîíåíòîâ/ñèñòåì)
        if (s_lastResetTime >= 0f && (Time.unscaledTime - s_lastResetTime) < kResetDebounceSeconds) return;

        s_lastResetFrame = Time.frameCount;
        s_lastResetTime = Time.unscaledTime;

        DoLoopReset();
    }

    // Âûíåñ òåëî ðåñåòà â ñòàòè÷åñêèé ìåòîä — òàê ëþáîé èñòî÷íèê (êíîïêà, âðåìÿ, òðèããåðû) èä¸ò ïî îäíîé äîðîãå
    private static void DoLoopReset() {
        Debug.Log("[LoopReset] start");

        QuestManager.ResetTemporary();

        GameTime.Instance.Hours = 12;
        GameTime.Instance.Minutes = 12;

        ArticyReset.ResetRQUE();
        ArticyReset.ResetEVT();

        bool hasArtefactNow =
            (GlobalVariables.Instance != null && GlobalVariables.Instance.player.hasArtifact)
            || InventoryStorage.Contains("InventoryArtefact");

        Debug.Log($"[LoopReset] GV.Instance={(GlobalVariables.Instance != null)} hasArtifact={GlobalVariables.Instance?.player.hasArtifact} fallbackContains={InventoryStorage.Contains("InventoryArtefact")} -> hasArtefactNow={hasArtefactNow}");

        if (!hasArtefactNow) {
            Debug.Log("[LoopReset] clearing inventory (no artefact)");
            InventoryStorage.Clear();
        } else {
            Debug.Log("[LoopReset] preserving inventory (artefact present)");
        }

        var monoBehaviours = Object.FindObjectsOfType<MonoBehaviour>(true);
        foreach (var mb in monoBehaviours) {
            if (mb is ILoopResettable resettable) {
                try {
                    resettable.OnLoopReset();
                } catch (System.Exception e) {
                    Debug.LogWarning($"[LoopReset] OnLoopReset error on {mb.name}: {e}");
                }
            }
        }

        Debug.Log("[LoopReset] end");
    }

    public void LoopReset() => TryLoopReset(); // äåðãàåò òîò æå äåáàóíñ

}
