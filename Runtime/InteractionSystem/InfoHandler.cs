using Lotec.Utils;
using Lotec.Utils.Attributes;
using TMPro;
using UnityEngine;

namespace Lotec.Interactions {
    public class InfoHandler : MonoBehaviour2 {
        [SerializeField, NotNull] TextMeshProUGUI _text;

        public void SetText(string text) {
            _text.text = text;
        }
        public string Text => _text.text;
    }
}
