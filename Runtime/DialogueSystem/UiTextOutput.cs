using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Lotec.Utils.DialogueSystem {
    /// <summary>
    /// Wrapper to output text to a UnityEngine.UI.Text component.
    /// </summary>
    public class UiTextOutput : MonoBehaviour, IText {
        [SerializeField] Text _output;

        public void SetText(string text) => _output.text = text;

        public IEnumerator SetTextRoutine(string text) {
            _output.text = text;
            yield return null;
        }
    }
}
