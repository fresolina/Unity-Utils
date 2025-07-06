using Lotec.Utils;
using Lotec.Utils.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lotec.Interactions {
    public class InteractionSystemInput : MonoBehaviour2 {
        [SerializeField, NotNull] InputActionReference[] _interactActions;
        [SerializeField, NotNull] InputActionReference _cancelInteractionAction;
        [SerializeField, NotNull] SimpleInteractionSystem _interactionSystem;

        public InputActionReference[] InteractActions => _interactActions;
        public InputControl ActiveControl { get; private set; }
        public string[] ActionKeys { get; } = new string[] { "E", "R", "T", "Y" };

        void Start() {
            for (int i = 0; i < _interactActions.Length; i++) {
                int index = i;
                _interactActions[i].action.actionMap.Enable();
                _interactActions[i].action.Enable();
                _interactActions[i].action.performed += (context) => {
                    UpdateControl(context.control);
                    _interactionSystem.Interact(index);
                };
            }

            // Player pressed "Cancel Interaction" button.
            _cancelInteractionAction.action.actionMap.Enable();
            _cancelInteractionAction.action.Enable();
            _cancelInteractionAction.action.performed += (context) => _interactionSystem.CancelInteraction();
        }

        void UpdateControl(InputControl control) {
            if (control == ActiveControl) return;

            for (int i = 0; i < _interactActions.Length; i++) {
                var action = _interactActions[i].action;
                var binding = action.GetBindingForControl(control);
                if (binding != null) {
                    ActionKeys[i] = binding.Value.ToDisplayString();
                }
            }
            ActiveControl = control;
        }
    }
}
