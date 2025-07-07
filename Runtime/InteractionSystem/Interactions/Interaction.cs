using System.Text.RegularExpressions;
using Lotec.Utils;
using Lotec.Utils.Attributes;
using UnityEngine;

namespace Lotec.Interactions {
    /// <summary>
    /// Base interaction.
    /// </summary>
    public abstract class Interaction : MonoBehaviour2, IInteraction {
        [SerializeField, NotNull] protected Interactable _interactable;
        public virtual string InteractionName => _fromClassName;
        [SerializeField, HideInInspector] string _fromClassName;

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();
            _fromClassName = SplitCamelCase(GetType().Name);
        }
#endif

        public virtual bool IsValid => true;
        public abstract void OnStartInteraction();
        public virtual void OnCancelInteraction() { }

        // TODO: IInteractionSystem.ShowMessage()
        protected void ShowMessage(string message) => Debug.Log(message);

        // Converts "PuckUpItem" to "Pick Up Item"
        static string SplitCamelCase(string str) => Regex.Replace(str, "([a-z])([A-Z])", "$1 $2");
    }
}
