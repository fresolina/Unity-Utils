using System.Collections.Generic;
using Lotec.Utils.Triggers;
using UnityEngine;

namespace Lotec.Utils.DialogueSystem {
    public enum DialogueSpeakerSlot {
        Initiator = 0,
        Target = 1,
        Custom = 2
    }

    [CreateAssetMenu(fileName = "New Dialogue Node", menuName = "Lotec/Dialogue/Node")]
    public class DialogueNode : ScriptableObject {
        [SerializeField] DialogueSpeakerSlot _speakerSlot = DialogueSpeakerSlot.Initiator;
        [SerializeField] DialogueActor _customActor;
        [SerializeField, TextArea] string _text;
        [SerializeField] List<DialogueOption> _options = new List<DialogueOption>();
        [field: SerializeReference] public IAction ActionOnSpoke { get; set; }
        // TODO: Emotion { get; } [laughing, irritated, angry]

        public DialogueSpeakerSlot SpeakerSlot => _speakerSlot;
        public DialogueActor CustomActor => _customActor;
        public string Text => _text;
        public IReadOnlyList<DialogueOption> Options => _options;
    }
}
