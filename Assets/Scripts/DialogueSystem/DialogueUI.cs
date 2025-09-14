﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy.World_Of_Red_Moon;
using Articy.World_Of_Red_Moon.GlobalVariables;

public class DialogueUI : MonoBehaviour, IArticyFlowPlayerCallbacks, ILoopResettable {
    [Header("Articy")]
    [SerializeField] private ArticyFlowPlayer flowPlayer;
    [SerializeField] private Entity playerEntity; // перетащи сюда Entity главного героя из Articy

    [Header("UI")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private TMP_Text dialogueSpeaker;
    [SerializeField] private TMP_Text fraction;
    [SerializeField] private TMP_Text title;
    [SerializeField] private Image portraitImage;
    [SerializeField] private ResponseHandler responseHandler;

    private bool dialogueFinished = false;
    private string lastDisplayedText = null;
    private string lastSpeakerName = null;
    public bool IsDialogueOpen { get; private set; }

    public IObjectWithFeatureDuration kek;
    private bool suppressOnFlowPause = false;
    private bool? originalRecalcSetting;

    private void SetContinuousRecalculation(bool enable) {
        if (flowPlayer == null) return;
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var type = flowPlayer.GetType();

        var field = type.GetField("continuouslyRecalculateBranches", flags);
        if (field != null) {
            if (originalRecalcSetting == null)
                originalRecalcSetting = (bool)field.GetValue(flowPlayer);
            field.SetValue(flowPlayer, enable);
            return;
        }

        var prop = type.GetProperty("continuouslyRecalculateBranches", flags) ??
                   type.GetProperty("ContinuouslyRecalculateBranches", flags);
        if (prop != null) {
            if (originalRecalcSetting == null && prop.CanRead)
                originalRecalcSetting = (bool)prop.GetValue(flowPlayer);
            if (prop.CanWrite)
                prop.SetValue(flowPlayer, enable);
        }
    }

    private string globalVariablesSnapshot;
    private IFlowObject currentFlowObject;

    private void TryInitReflection()
    {
        if (globalVariablesSnapshot == null)
        {
            globalVariablesSnapshot = JsonUtility.ToJson(ArticyGlobalVariables.Default);
        }
    }

    private bool HaveGlobalVariablesChanged()
    {
        var current = JsonUtility.ToJson(ArticyGlobalVariables.Default);
        if (current != globalVariablesSnapshot)
        {
            globalVariablesSnapshot = current;
            return true;
        }
        return false;
    }

    private void Update()
    {
      /*  if (!IsDialogueOpen) //something sus going on here
            return;*/

        

        TryInitReflection();

        if (HaveGlobalVariablesChanged())
        {
            Debug.Log("Global variables changed");

            if (currentFlowObject is DialogueFragment fragment)
            {
                CloseDialogue();
                StartDialogue(fragment);
                flowPlayer?.Play();
            }
        }
    }

    private void Awake() {
        if (dialogueBox != null)
            dialogueBox.SetActive(false); // диалог скрыт до начала взаимодействия
    }

    private void Start() {
        if (flowPlayer == null) {
            Debug.LogError("[DialogueUI] flowPlayer не назначен.");
            return;
        }

        // ВАЖНО: ничего не запускаем автоматически!
        // Раньше могло быть: flowPlayer.StartOn = ...; flowPlayer.Play();
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
        lastDisplayedText = string.Empty;
        lastSpeakerName = null;
        if (textLabel != null) textLabel.text = string.Empty;
        if (portraitImage != null) portraitImage.sprite = null;
        dialogueFinished = false;
        if (dialogueBox != null) dialogueBox.SetActive(true);
        if (flowPlayer != null)
            flowPlayer.enabled = true;


        // Убедимся, что стартуем именно с нужного DialogueFragment
        var startFragment = startObject as DialogueFragment;
        if (startFragment == null) {
            Debug.LogError("[DialogueUI] startObject не является DialogueFragment — не могу запустить диалог.");
            return;
        }

        // Задаём стартовую точку и не проигрываем первый узел автоматически
        flowPlayer.StartOn = startFragment;
        IsDialogueOpen = true;

    }

    //public void UpdateDialogue(bool skipPostClose = false) {
    //    dialogueBox?.SetActive(false);
    //    dialogueFinished = false;
    //    responseHandler?.ClearResponses();
    //    if (portraitImage != null) portraitImage.sprite = null;
    //    IsDialogueOpen = false;
    //    if (flowPlayer != null) {
    //        SetContinuousRecalculation(originalRecalcSetting ?? false);
    //        suppressOnFlowPause = true;
    //        var stopMethod = flowPlayer.GetType().GetMethod("Stop", BindingFlags.Public | BindingFlags.Instance);
    //        stopMethod?.Invoke(flowPlayer, null);
    //        flowPlayer.enabled = false;
    //    }
    //}

    /// <summary>Принудительно закрыть текущий диалог (например, кнопкой "Esc").</summary>
    public void CloseDialogue(bool skipPostClose = false) {
        dialogueBox?.SetActive(false);
        dialogueFinished = false;
        responseHandler?.ClearResponses();
        if (portraitImage != null) portraitImage.sprite = null;
        IsDialogueOpen = false;
        lastSpeakerName = null;
        if (flowPlayer != null) {
            SetContinuousRecalculation(originalRecalcSetting ?? false);
            suppressOnFlowPause = true;
            var stopMethod = flowPlayer.GetType().GetMethod("Stop", BindingFlags.Public | BindingFlags.Instance);
            stopMethod?.Invoke(flowPlayer, null);
            flowPlayer.enabled = false;
        }



        Debug.Log("[DialogueUI] Dialogue closed by user.");
        if (!skipPostClose) {
            GlobalVariables.Instance?.GetKnowledge();
            GlobalVariables.Instance?.GetTempObjectives();
            GlobalVariables.Instance?.GetItems();

            if (ArticyGlobalVariables.Default.RFLG.neutralizedByGuard) {
                LoopResetInputScript.TryLoopReset();
            }
        }
    }

    public void OnLoopReset() {
        CloseDialogue();
    }


    // ======== IArticyFlowPlayerCallbacks ========
    public void OnFlowPlayerPaused(IFlowObject aObject) {
        currentFlowObject = aObject;
        /*if (suppressOnFlowPause) {
            suppressOnFlowPause = false;
            return;
        }*/
        // Добавим время из свойства Duration
        if (aObject is IObjectWithFeatureDuration)
        {
            kek = (IObjectWithFeatureDuration)aObject;
            int duration = ((int)kek.GetFeatureDuration().Minutes);

            Debug.Log(duration);
            if (duration > 0)
            {
                GameTime.Instance?.AddMinutes(duration);
                Debug.Log("Added minutes");
            }
        }

        // Очистим старые ответы (если были)
        responseHandler?.ClearResponses();

      //dialogueBox?.SetActive(true);
        UpdateSpeakerInfo(aObject);
        UpdatePortrait(aObject);

        // Попытаемся получить текст прямо с текущего объекта
        string currentText = GetTextFromFlowObject(aObject);

        if (!string.IsNullOrEmpty(currentText)) {
            // У нас есть текст — добавляем его к уже отображённому
            if (textLabel != null) {
                string speakerName = GetSpeakerDisplayName(aObject);
                if (string.IsNullOrEmpty(textLabel.text)) {
                    textLabel.text = string.IsNullOrEmpty(speakerName)
                        ? currentText
                        : speakerName + " - " + currentText;
                } else if (speakerName == lastSpeakerName) {
                    textLabel.text += "\n" + currentText;
                } else {
                    string prefix = string.IsNullOrEmpty(speakerName)
                        ? string.Empty
                        : speakerName + " - ";
                    textLabel.text += "\n\n" + prefix + currentText;
                }
                lastSpeakerName = speakerName;
                lastDisplayedText = textLabel.text;
            }

            dialogueFinished = false;
            return;
        }

        // Текущий объект не содержит текста — оставляем весь предыдущий диалог видимым
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
            CloseDialogue();
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
                responseHandler?.ShowContinueButton(otherBranches[0], flowPlayer);
                dialogueFinished = false;
            } else if (otherBranches.Count > 1) {
                // Несколько не-игровых веток — можно показать как варианты
                responseHandler?.ShowResponses(otherBranches, flowPlayer, null);
                dialogueFinished = false;
            }
        }
    }

    // -------------------- Вспомогательные безопасные методы --------------------
    private void UpdateSpeakerInfo(IFlowObject obj) {
        if (dialogueSpeaker == null) return;

        var speaker = GetSpeakerEntity(obj);
        if (speaker is IObjectWithFeatureCharacterCard withCard) {
            var card = withCard.GetFeatureCharacterCard();
            if (card != null) {
                dialogueSpeaker.text = card.Name != null ? card.Name.ToString() : "";
                if (title != null) title.text = card.Title != null ? card.Title.ToString() : "";
                if (fraction != null) fraction.text = card.Fraction != null ? card.Fraction.ToString() : "";
                return;
            }
        }

        dialogueSpeaker.text = GetSpeakerDisplayName(obj);
        if (title != null) title.text = "";
        if (fraction != null) fraction.text = "";
    }

    private void UpdatePortrait(IFlowObject obj) {
        if (portraitImage == null) return;
        portraitImage.sprite = null;
        var speaker = GetSpeakerEntity(obj);
        if (speaker is IObjectWithPreviewImage withPreview) {
            var asset = withPreview.PreviewImage.Asset;
            if (asset != null)
                portraitImage.sprite = asset.LoadAssetAsSprite();
        }
    }

    private string GetTextFromFlowObject(IFlowObject obj) {
        if (obj == null) return null;

        if (obj is IObjectWithText tw /*&& !string.IsNullOrEmpty(tw.Text)*/) return tw.Text;

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

    private int GetDurationFromFlowObject(IFlowObject obj)
    {

        //if (obj == null) return 0;

        try {

            var type = obj.GetType();
            var durProp = type.GetProperty("Duration", BindingFlags.Public | BindingFlags.Instance);
            Debug.Log(durProp);
            if (durProp != null)
            {
                var val = durProp.GetValue(obj);
                Debug.Log(val);
                if (val is int i) return i;
                if (val != null && int.TryParse(val.ToString(), out i)) return i;
            }

            var propsProp = type.GetProperty("Properties", BindingFlags.Public | BindingFlags.Instance);
            if (propsProp != null)
            {
                var props = propsProp.GetValue(obj);
                if (props != null)
                {
                    var inner = props.GetType().GetProperty("Duration", BindingFlags.Public | BindingFlags.Instance);
                    if (inner != null)
                    {
                        var val = inner.GetValue(props);
                        if (val is int j) return j;
                        if (val != null && int.TryParse(val.ToString(), out j)) return j;
                    }
                }
            }
        }

       

        

        catch { }

        return 0;
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
