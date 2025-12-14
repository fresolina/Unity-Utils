using UnityEngine;

namespace Lotec.Utils.DialogueSystem {
    [CreateAssetMenu(fileName = "New Dialogue Actor", menuName = "Lotec/Dialogue/Actor")]
    public class DialogueActor : ScriptableObject {
        [SerializeField] string _actorName;
        [SerializeField] DialogueNode _startNode;

        public string ActorName => _actorName;
        public DialogueNode StartNode => _startNode;

        public bool IsPlayer { get; set; }
    }
}
