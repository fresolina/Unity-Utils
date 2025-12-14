using Lotec.Utils.Triggers;
using UnityEngine;

namespace Lotec.Utils.DialogueSystem {
    [System.Serializable]
    public class DialogueOption {
        [Tooltip("Text displayed in UI. Defaults to node text if empty.")]
        [SerializeField] string _text;
        [SerializeField] DialogueNode _nextNode;
        [Tooltip("Choose option automatically if the condition is met, without player input.")]
        [SerializeField] bool _isAutomatic;

        [field: SerializeReference] public ICondition Condition { get; set; }

        public string OptionText => string.IsNullOrEmpty(_text) && _nextNode != null ? _nextNode.Text : _text;
        public DialogueNode NextNode => _nextNode;
        public bool IsAutomatic => _isAutomatic;

        public override string ToString() => OptionText;
    }
}
