using System;
using System.Collections.Generic;
using Articy.Unity;
using Articy.Unity.Interfaces;
using UnityEngine;
using System.Reflection;
using Articy.World_Of_Red_Moon.GlobalVariables;
using System.Linq;

public enum DialogueState
{
    Idle,
    Opening,
    Active,
    Transition,
    Closing
}

public interface IDialogueEffect
{
    void Apply(DialogueServices services);
    string Debug();
}

public class DialogueContext { }

public class DialogueServices
{
    public GlobalVariables globals;
    public PlayerState playerState;
    public DialogueServices(GlobalVariables g, PlayerState ps)
    {
        globals = g;
        playerState = ps;
    }
}

public class NodeData
{
    public IFlowObject node;
    public NodeData(IFlowObject n) { node = n; }
}

public class ChoiceData
{
    public string Text;
    public Branch Branch;
    public ChoiceData(string text, Branch branch)
    {
        Text = text;
        Branch = branch;
    }
}

public class DialogueController : MonoBehaviour, IArticyFlowPlayerCallbacks
{
    [SerializeField] private ArticyFlowPlayer flowPlayer;
    [SerializeField] private DialogueUI ui;
    [SerializeField] private GlobalVariables globals;

    private DialogueState state = DialogueState.Idle;
    private bool needsRefresh;
    private List<IDialogueEffect> effectBatch = new List<IDialogueEffect>();
    private DialogueContext context;

    public bool IsActive => state == DialogueState.Active;
    public event Action OnOpened;
    public event Action OnClosed;

    private void Awake()
    {
        if (flowPlayer != null)
        {
            flowPlayer.enabled = false;
        }
        ui?.BindController(this);
    }

    public void OpenDialogue(ArticyRef refId, DialogueContext ctx)
    {
        if (state != DialogueState.Idle || refId == null || flowPlayer == null)
            return;

        context = ctx;
        globals?.SyncGlobalsToArticy();

        state = DialogueState.Opening;
        flowPlayer.StartOn = refId.GetObject() as IFlowObject;
        flowPlayer.enabled = true;
        flowPlayer.Play();
        ui?.gameObject.SetActive(true);
        OnOpened?.Invoke();
    }

    public void CloseDialogue()
    {
        if (state == DialogueState.Closing || flowPlayer == null)
            return;

        state = DialogueState.Closing;
        flowPlayer.Stop();
        flowPlayer.enabled = false;
        ui?.gameObject.SetActive(false);
        OnClosed?.Invoke();
        state = DialogueState.Idle;
    }

    public void ForceRefresh()
    {
        needsRefresh = true;
    }

    // --------- Articy callbacks ---------
    public void OnFlowPlayerPaused(IFlowObject aObject)
    {
        OnNodeEnter(new NodeData(aObject));
    }

    public void OnNodeEnter(NodeData node)
    {
        ui?.DisplayNode(node);
        BuildEffects(node.node);
    }

    public void OnBranchesUpdated(IList<Branch> branches)
    {
        var list = new List<ChoiceData>();
        foreach (var b in branches)
        {
            if (b?.Target == null) continue;
            string text = b.Target is IObjectWithMenuText mt && !string.IsNullOrEmpty(mt.MenuText)
                ? mt.MenuText
                : b.Target.ToString();
            list.Add(new ChoiceData(text, b));
        }
        OnChoicesReady(list);
    }

    public void OnChoicesReady(List<ChoiceData> choices)
    {
        ui?.DisplayChoices(choices, this);
        state = DialogueState.Active;
    }

    public void SelectChoice(ChoiceData choice)
    {
        if (state != DialogueState.Active)
            return;

        state = DialogueState.Transition;
        ui?.DisableChoices();
        if (choice?.Branch != null)
            flowPlayer.Play(choice.Branch);
        ApplyBatch();
        if (needsRefresh)
        {
            needsRefresh = false;
            ui?.RefreshBindings();
        }
    }

    public void OnChoiceSelected(ChoiceData choice) { }

    public void OnVariablesChanged(string key)
    {
        needsRefresh = true;
    }

    public void OnDialogueEnded()
    {
        CloseDialogue();
    }

    private void BuildEffects(IFlowObject node)
    {
        effectBatch.Clear();
        if (node == null) return;

        int minutes = TryGetInt(node, "DurationMinutes");
        if (minutes > 0)
            effectBatch.Add(new DurationEffect(minutes));

        string knowledge = TryGetString(node, "SetKnowledge");
        if (!string.IsNullOrEmpty(knowledge))
        {
            effectBatch.Add(new SetKnowledgeEffect(knowledge));
            needsRefresh = true;
        }

        string startQuest = TryGetString(node, "StartQuest");
        if (!string.IsNullOrEmpty(startQuest))
        {
            effectBatch.Add(new StartQuestEffect(startQuest));
            needsRefresh = true;
        }

        string completeQuest = TryGetString(node, "CompleteQuest");
        if (!string.IsNullOrEmpty(completeQuest))
        {
            effectBatch.Add(new CompleteQuestEffect(completeQuest));
            needsRefresh = true;
        }

        string itemId = TryGetString(node, "GiveItemId");
        if (!string.IsNullOrEmpty(itemId))
        {
            effectBatch.Add(new GiveItemEffect(itemId));
            needsRefresh = true;
        }

        string flag = TryGetString(node, "SetFlag");
        if (!string.IsNullOrEmpty(flag))
        {
            effectBatch.Add(new SetFlagEffect(flag));
            needsRefresh = true;
        }
    }

    private string TryGetString(IFlowObject node, string name)
    {
        var type = node.GetType();
        var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        if (prop != null && prop.PropertyType == typeof(string))
        {
            return prop.GetValue(node) as string;
        }
        var propsProp = type.GetProperty("Properties", BindingFlags.Instance | BindingFlags.Public);
        var props = propsProp?.GetValue(node);
        var inner = props?.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        if (inner != null && inner.PropertyType == typeof(string))
        {
            return inner.GetValue(props) as string;
        }
        return null;
    }

    private int TryGetInt(IFlowObject node, string name)
    {
        var type = node.GetType();
        var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        if (prop != null)
        {
            var val = prop.GetValue(node);
            if (val is int i) return i;
            if (val != null && int.TryParse(val.ToString(), out i)) return i;
        }
        var propsProp = type.GetProperty("Properties", BindingFlags.Instance | BindingFlags.Public);
        var props = propsProp?.GetValue(node);
        var inner = props?.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        if (inner != null)
        {
            var val = inner.GetValue(props);
            if (val is int j) return j;
            if (val != null && int.TryParse(val.ToString(), out j)) return j;
        }
        return 0;
    }

    private void ApplyBatch()
    {
        var services = new DialogueServices(globals, globals?.player);
        foreach (var e in effectBatch)
        {
            e.Apply(services);
        }
        globals?.SyncGlobalsToArticy();
    }

    // Example effect implementation
    private class DurationEffect : IDialogueEffect
    {
        private readonly int minutes;
        public DurationEffect(int m) { minutes = m; }
        public void Apply(DialogueServices s)
        {
            GameTime.Instance?.AddMinutes(minutes);
        }
        public string Debug() => $"Duration +{minutes}";
    }

    private class SetKnowledgeEffect : IDialogueEffect
    {
        private readonly string key;
        public SetKnowledgeEffect(string k) { key = k; }
        public void Apply(DialogueServices s)
        {
            KnowledgeManager.AddKnowledge(key);
        }
        public string Debug() => $"SetKnowledge {key}";
    }

    private class StartQuestEffect : IDialogueEffect
    {
        private readonly string key;
        public StartQuestEffect(string k) { key = k; }
        public void Apply(DialogueServices s)
        {
            QuestManager.StartQuest(key);
        }
        public string Debug() => $"StartQuest {key}";
    }

    private class CompleteQuestEffect : IDialogueEffect
    {
        private readonly string key;
        public CompleteQuestEffect(string k) { key = k; }
        public void Apply(DialogueServices s)
        {
            QuestManager.CompleteQuest(key);
        }
        public string Debug() => $"CompleteQuest {key}";
    }

    private class GiveItemEffect : IDialogueEffect
    {
        private readonly string itemId;
        public GiveItemEffect(string id) { itemId = id; }
        public void Apply(DialogueServices s)
        {
            InventoryStorage.Add(itemId);
            if (s.playerState != null && itemId.Equals("Artifact", StringComparison.OrdinalIgnoreCase))
                s.playerState.hasArtifact = true;
        }
        public string Debug() => $"GiveItem {itemId}";
    }

    private class SetFlagEffect : IDialogueEffect
    {
        private readonly string flag;
        public SetFlagEffect(string f) { flag = f; }
        public void Apply(DialogueServices s)
        {
            var rflg = ArticyGlobalVariables.Default.RFLG;
            var prop = rflg.GetType().GetProperty(flag, BindingFlags.Instance | BindingFlags.Public);
            if (prop != null && prop.PropertyType == typeof(bool))
                prop.SetValue(rflg, true);
        }
        public string Debug() => $"SetFlag {flag}";
    }
}

