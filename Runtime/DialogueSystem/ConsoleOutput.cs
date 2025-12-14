using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Lotec.Utils.DialogueSystem {
    /// <summary>
    /// Wrapper to output text to the console.
    /// </summary>
    public class ConsoleOutput : MonoBehaviour, IText {
        public void SetText(string text) => Debug.Log(text);
        public IEnumerator SetTextRoutine(string text) {
            Debug.Log(text);
            yield break;
        }
    }
}
