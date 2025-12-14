namespace Lotec.Utils {
    public class DialogueExampleRun : MonoBehaviour {
        [SerializeField] DialogueActor _player;
        [SerializeField] DialogueActor _npc;

        void Start() {
            DialogueManager dialogueManager = gameObject.GetComponent<DialogueManager>();
            _player.IsPlayer = true;
            dialogueManager.StartDialogue(_npc, _player);
        }
    }
}
