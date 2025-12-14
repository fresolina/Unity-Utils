using System.Collections;
using UnityEngine;

namespace Lotec.Utils.DialogueSystem {
    public interface IDialogueTalker {
        void StartSpeaking(DialogueActor actor, DialogueNode node);
        void Stop();
        event System.Action SpeakingEnded;
    }

    /// <summary>
    /// Simplest talker outputs to debug log and returns immediately.
    /// </summary>
    public class DialogueTalker : MonoBehaviour2, IDialogueTalker {
        [SerializeField] bool _prefixTextWithActorName = true;
        [SerializeField] IText _output;

        public event System.Action SpeakingEnded;

        public void StartSpeaking(DialogueActor actor, DialogueNode node) {
            StartCoroutine(SpeakRoutine(actor, node));
        }

        IEnumerator SpeakRoutine(DialogueActor actor, DialogueNode node) {
            string textToSpeak = _prefixTextWithActorName ? $"{actor.name}: {node.Text}" : node.Text;
            yield return StartCoroutine(_output.SetTextRoutine(textToSpeak));
            SpeakingEnded?.Invoke();
        }

        public void Stop() {
            Debug.Log("Stopped talking");
            StopAllCoroutines();
        }
    }
}
