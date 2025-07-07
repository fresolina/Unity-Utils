using Lotec.Utils;
using Lotec.Utils.Attributes;
using UnityEngine;

namespace Lotec.Interactions {
    /// <summary>
    /// Base interaction.
    /// </summary>
    public abstract class Interaction : MonoBehaviour2, IInteraction {
        [SerializeField, NotNull] protected Interactable _interactable;
        public virtual string InteractionName => _className;
        [SerializeField, HideInInspector] string _className;

#if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();
            _className = GetType().Name;
        }
#endif

        public virtual bool IsValid => true;
        public abstract void OnStartInteraction();
        public virtual void OnCancelInteraction() { }

        // TODO: IInteractionSystem.ShowMessage()
        protected void ShowMessage(string message) => Debug.Log(message);
    }
}
