using System.Collections;

namespace Lotec.Utils.DialogueSystem {
    public interface IText {
        IEnumerator SetTextRoutine(string text);
        void SetText(string text);
    }
}
