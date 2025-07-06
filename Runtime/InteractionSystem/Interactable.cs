using Lotec.Utils;
using UnityEngine;

namespace Lotec.Interactions {
    public interface IInteractable {
        string Name { get; }
        string Description { get; }
        IInteraction[] Interactions { get; }
    }

    public class Interactable : MonoBehaviour2, IInteractable {
        [SerializeField] protected string _name;
        [SerializeField] protected string _description;
        IInteraction[] _interactions;

        public virtual string Name => _name;
        public virtual string Description => _description;
        public virtual IInteraction[] Interactions => _interactions;

        void Awake() {
            _interactions = GetComponents<IInteraction>();
        }

#if UNITY_EDITOR
        protected override void Reset() {
            base.Reset();
            if (string.IsNullOrEmpty(_name)) {
                _name = gameObject.name;
            }
        }
#endif
    }
}
