using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy.World_Of_Red_Moon;
using UnityEngine.InputSystem;

public class DialogueUI : MonoBehaviour, IArticyFlowPlayerCallbacks {
    [Header("Articy")]
    [SerializeField] private ArticyFlowPlayer flowPlayer;
    [SerializeField] private Entity playerEntity; // перетащи сюда Entity главного героя из Articy

    [Header("UI")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private TMP_Text dialogueSpeaker;
    [SerializeField] private ResponseHandler responseHandler;

    [Header("Input")]
    [Tooltip("Имя действия в Input System, которое подтверждает реплику/\"Далее\"")]
    [SerializeField] private string advanceActionName = "Interact";
    private InputAction advanceAction;
    private float suppressAdvanceUntil = 0f; // гашим подтверждение сразу после запуска диалога

    private bool dialogueFinished = false;
    private string lastDisplayedText = null;

    private void Awake() {
        if (dialogueBox != null)
            dialogueBox.SetActive(false); // диалог скрыт до начала взаимодействия
    }

    private void Start() {
        if (flowPlayer == null) {
            Debug.LogError("[DialogueUI] flowPlayer не назначен.");
            return;
        }
        // Находим действие подтверждения
        if (!string.IsNullOrEmpty(advanceActionName))
            advanceAction = InputSystem.actions.FindAction(advanceActionName);

        // ВАЖНО: ничего не запускаем автоматически!
        // Раньше могло быть: flowPlayer.StartOn = ...; flowPlayer.Play();
    }

    private void Update() {
        // подтверждение "Далее" по клавише после запуска и только когда диалог открыт
        if (dialogueBox != null
            && dialogueBox.activeSelf
            && !dialogueFinished
            && advanceAction != null
            && Time.time >= suppressAdvanceUntil
            && advanceAction.triggered) {
            if (responseHandler != null && responseHandler.ResponsesCount > 0) {
                responseHandler.ClickFirstResponse();
            }
        }
    }

    // ======== ПУБЛИЧНЫЕ API ДЛЯ ЗАПУСКА ДИАЛОГА ========
    public void StartDialogue(ArticyRef startRef) {
        if (startRef == null || flowPlayer == null) {
            Debug.LogWarning("[DialogueUI] StartDialogue(ArticyRef) — пустой startRef или flowPlayer.");
            return;
        }
        var obj = startRef.GetObject() as IFlowObject;
        StartDialogue(obj);
    }

    public void StartDialogue(IFlowObject startObject) {
        if (startObject == null || flowPlayer == null) {
            Debug.LogWarning("[DialogueUI] StartDialogue(IFlowObject) — пустой startObject или flowPlayer.");
            return;
        }

        // сброс состояния UI
        responseHandler?.ClearResponses();
        lastDisplayedText = null;
        dialogueFinished = false;
        if (dialogueBox != null) dialogueBox.SetActive(true);

        // IFlowObject -> IArticyObject
        var startAsArticy = startObject as IArticyObject;
        if (startAsArticy == null) {
            Debug.LogError("[DialogueUI] startObject не приводится к IArticyObject — не могу запустить диалог.");
            return;
        }

        // краткий дебаунс, чтобы стартовое нажатие Interact не листало первую реплику
        suppressAdvanceUntil = Time.time + 0.2f;

        flowPlayer.StartOn = startAsArticy;
        flowPlayer.Play();
    }

    /// <summary>Принудительно закрыть текущий диалог (например, кнопкой "Esc").</summary>
    public void CloseDialogueByUser() {
        dialogueBox?.SetActive(false);
        dialogueFinished = false;
        responseHandler?.ClearResponses();
        Debug.Log("[DialogueUI] Dialogue closed by user.");
    }

    // ======== IArticyFlowPlayerCallbacks ========
    public void OnFlowPlayerPaused(IFlowObject aObject) {
        // Очистим старые ответы (если были)
        responseHandler?.ClearResponses();

        dialogueBox?.SetActive(true);
        if (dialogueSpeaker != null) dialogueSpeaker.text = GetSpeakerDisplayName(aObject);

        // Попытаемся получить текст прямо с текущего объекта
        string currentText = GetTextFromFlowObject(aObject);

        if (!string.IsNullOrEmpty(currentText)) {
            // У нас есть текст — сразу показываем и запоминаем
            lastDisplayedText = currentText;
            if (textLabel != null) textLabel.text = currentText;
            dialogueFinished = false;
            return;
        }

        // Текущий объект не содержит текста — оставляем последнюю реплику видимой
        dialogueFinished = false;
        if (string.IsNullOrEmpty(lastDisplayedText)) {
            if (textLabel != null) textLabel.text = "<<< NO TEXT >>>";
        } else {
            if (textLabel != null) textLabel.text = lastDisplayedText;
        }
    }

    public void OnBranchesUpdated(IList<Branch> branches) => HandleBranchesUpdate(branches);
    public void OnBranchesUpdated(IList<Branch> branches, IFlowObject flowObject) => HandleBranchesUpdate(branches);

    private void HandleBranchesUpdate(IList<Branch> branches) {
        if (branches == null || branches.Count == 0) {
            // Терминальная нода — оставляем последнюю фразу видимой, не закрываем автоматически
            dialogueFinished = true;
            CloseDialogueByUser();
            return;
        }

        // Разбиваем на ветки игрока / другие
        var playerBranches = new List<Branch>();
        var otherBranches = new List<Branch>();

        foreach (var b in branches) {
            if (b == null || b.Target == null) continue;
            var targSpeaker = GetSpeakerEntity(b.Target);
            if (playerEntity != null && targSpeaker != null && ReferenceEquals(targSpeaker, playerEntity))
                playerBranches.Add(b);
            else
                otherBranches.Add(b);
        }

        if (playerBranches.Count > 0) {
            // Показываем варианты игрока — ждём подтверждения
            responseHandler?.ShowResponses(playerBranches, flowPlayer, playerEntity);
            dialogueFinished = false;
        } else {
            if (otherBranches.Count == 1) {
                // ЕДИНСТВЕННАЯ NPC-ветка: НЕ проигрываем автоматически.
                // Показываем кнопку "Далее" и ждём подтверждения.
                responseHandler?.CreateSingleResponse("Далее", otherBranches[0], flowPlayer);
                dialogueFinished = false;
            } else if (otherBranches.Count > 1) {
                // Несколько не-игровых веток — можно показать как варианты
                responseHandler?.ShowResponses(otherBranches, flowPlayer, null);
                dialogueFinished = false;
            }
        }
    }

    // -------------------- Вспомогательные безопасные методы --------------------
    private string GetTextFromFlowObject(IFlowObject obj) {
        if (obj == null) return null;

        if (obj is IObjectWithText tw && !string.IsNullOrEmpty(tw.Text)) return tw.Text;

        try {
            var type = obj.GetType();
            var textProp = type.GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);
            if (textProp != null) {
                var val = textProp.GetValue(obj);
                if (val != null) return val.ToString();
            }

            var propsProp = type.GetProperty("Properties", BindingFlags.Public | BindingFlags.Instance);
            if (propsProp != null) {
                var props = propsProp.GetValue(obj);
                if (props != null) {
                    var txtProp = props.GetType().GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);
                    if (txtProp != null) {
                        var val = txtProp.GetValue(props);
                        if (val != null) return val.ToString();
                    }
                }
            }
        } catch { }

        return null;
    }

    private Entity GetSpeakerEntity(IFlowObject obj) {
        if (obj == null) return null;

        if (obj is IObjectWithSpeaker withSpeaker && withSpeaker.Speaker is Entity ent1)
            return ent1;

        try {
            var prop = obj.GetType().GetProperty("Speaker", BindingFlags.Public | BindingFlags.Instance);
            if (prop != null) {
                var sp = prop.GetValue(obj);
                if (sp is Entity ent2) return ent2;
            }
        } catch { }

        return null;
    }

    private string GetSpeakerDisplayName(IFlowObject obj) {
        var ent = GetSpeakerEntity(obj);
        if (ent == null) return "";

        // Попробуем ряд "дружественных" свойств
        string[] candidateProps = new[] { "DisplayName", "Name", "Label", "Title", "FullName" };
        foreach (var p in candidateProps) {
            var v = TryGetStringProperty(ent, p);
            if (!string.IsNullOrEmpty(v)) return v;
        }

        // Попробуем в Properties
        try {
            var propsProp = ent.GetType().GetProperty("Properties", BindingFlags.Public | BindingFlags.Instance);
            if (propsProp != null) {
                var props = propsProp.GetValue(ent);
                if (props != null) {
                    foreach (var p in candidateProps) {
                        var v = TryGetStringProperty(props, p);
                        if (!string.IsNullOrEmpty(v)) return v;
                    }

                    var alt = TryGetStringProperty(props, "DisplayNameText") ?? TryGetStringProperty(props, "NameText");
                    if (!string.IsNullOrEmpty(alt)) return alt;
                }
            }
        } catch { }

        // Fallback — TechnicalName prettified
        var tech = TryGetStringProperty(ent, "TechnicalName") ?? ent.ToString();
        return PrettyfyTechnicalName(tech);
    }

    private string TryGetStringProperty(object target, string propertyName) {
        if (target == null || string.IsNullOrEmpty(propertyName)) return null;
        try {
            var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null) {
                var val = prop.GetValue(target);
                if (val != null) return val.ToString();
            }
        } catch { }
        return null;
    }

    private string PrettyfyTechnicalName(string technical) {
        if (string.IsNullOrEmpty(technical)) return technical ?? "";
        string s = technical.Trim();
        s = s.Replace("_", " ").Replace("-", " ");
        s = Regex.Replace(s, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])", " $1");
        s = Regex.Replace(s, @"\s+", " ").Trim();
        s = Regex.Replace(s.ToLower(), @"\b[a-z]", m => m.Value.ToUpper());
        return s;
    }

    private string TruncateForLog(string s, int max = 60) {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Length <= max) return s.Replace("\n", "\\n");
        return s.Substring(0, max).Replace("\n", "\\n") + "...";
    }
    
    // Пустые реализации интерфейса
    public void OnExecutionEngineStarted() { }
    public void OnExecutionEngineStopped() { }
}
