using System.Collections;
using System.Text;
using UnityEngine;

namespace Lotec.Utils.DialogueSystem {
    public interface ITypeWriter {
        IEnumerator WriteRoutine(string text);
        event System.Action<string> TextUpdated;
    }

    [System.Serializable]
    public class TypeWriter : ITypeWriter {
        [Tooltip("How fast to write text, characters per second.\n0 = instant")]
        [SerializeField] float _writeSpeed = 40f;

        float _ouputCharacterDelay = 0;
        string _textWritten;

        public event System.Action<string> TextUpdated;

        void SetTextWritten(string value) {
            _textWritten = value;
            TextUpdated?.Invoke(_textWritten);
        }

        public IEnumerator WriteRoutine(string text) {
            if (_writeSpeed == 0) {
                SetTextWritten(text);
                yield break;
            }

            _ouputCharacterDelay = 1f / _writeSpeed;

            StringBuilder sb = new();
            for (int i = 0; i < text.Length; i++) {
                sb.Append(text[i]);
                SetTextWritten(sb.ToString());
                yield return new WaitForSeconds(_ouputCharacterDelay);
            }
            sb.Append("\n");
            SetTextWritten(sb.ToString());
        }
    }
}
