using System.Collections;
using UnityEngine;

namespace Lotec.Utils.DialogueSystem {
    /// <summary>
    /// Talker that outputs to UI elements.
    /// </summary>
    [System.Serializable]
    public class TypeWriterOutput : MonoBehaviour2, IText {
        [Tooltip("Minimum time to show text, even for short text")]
        [SerializeField] float _minimumDuration = 2f;
        [Tooltip("How long should text be visible, characters per second.\n20 = 3 seconds for 60 characters")]
        [SerializeField] float _readSpeed = 20f;
        [SerializeField] TypeWriter _typeWriter;
        [SerializeField] IText _output;
        float _startedTalkingAt;

        public void OnTextUpdated(string text) => _output.SetText(text);

        void Awake() {
            _typeWriter.TextUpdated += OnTextUpdated;
        }

        void OnDestroy() {
            _typeWriter.TextUpdated -= OnTextUpdated;
        }

        public IEnumerator SetTextRoutine(string text) {
            _startedTalkingAt = Time.time;
            yield return StartCoroutine(_typeWriter.WriteRoutine(text));

            // Ensure minimum duration has passed.
            float minTimeLeft = _startedTalkingAt + _minimumDuration - Time.time;
            float timeLeft = Mathf.Max(minTimeLeft, text.Length / _readSpeed);
            if (timeLeft > 0) {
                yield return new WaitForSeconds(timeLeft);
            }
        }

        public void SetText(string text) => _output.SetText(text);
    }
}
