using UnityEngine;

namespace Dialogue
{
    [CreateAssetMenu(fileName = "DialogueConfig", menuName = "Dialogue/Config", order = 0)]
    public class DialogueConfig : ScriptableObject
    {
        private static DialogueConfig active;
        private static bool triedLoad;

        public static DialogueConfig Active
        {
            get
            {
                if (!triedLoad)
                {
                    // Try load from Resources (preferred paths)
                    active = Resources.Load<DialogueConfig>("Dialogue/DialogueConfig");
                    if (active == null)
                        active = Resources.Load<DialogueConfig>("DialogueConfig");

                    triedLoad = true;

                    // If not found, create a transient default in memory
                    if (active == null)
                    {
                        active = CreateInstance<DialogueConfig>();
                    }
                }
                return active;
            }
        }

        [SerializeField] private bool useController = false;
        [SerializeField] private bool useInputGate = false;
        [SerializeField] private bool useBatchEffects = false;
        [SerializeField] private bool useCentralTime = false;
        [SerializeField] private bool useRefresh = false;
        [SerializeField] private bool useCentralGVSync = false;

        public bool UseController => useController;
        public bool UseInputGate => useInputGate;
        public bool UseBatchEffects => useBatchEffects;
        public bool UseCentralTime => useCentralTime;
        public bool UseRefresh => useRefresh;
        public bool UseCentralGVSync => useCentralGVSync;
    }
}