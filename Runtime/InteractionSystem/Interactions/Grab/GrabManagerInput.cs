using Lotec.Utils;
using Lotec.Utils.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lotec.Interactions {
    public class GrabManagerInput : MonoBehaviour2 {
        [SerializeField, NotNull] InputActionReference _usePrimaryAction;
        [SerializeField, NotNull] InputActionReference _useSecondaryAction;
        [SerializeField, NotNull] InputActionReference _dropAction;
        [SerializeField, NotNull] GrabManager _grabManager;

        void EnableAction(InputAction action) {
            if (!action.actionMap.enabled)
                action.actionMap.Enable();
            action.Enable();
        }

        void OnEnable() {
            EnableAction(_usePrimaryAction.action);
            _usePrimaryAction.action.performed += OnUsePrimary;

            EnableAction(_useSecondaryAction.action);
            _useSecondaryAction.action.performed += OnUseSecondary;

            EnableAction(_dropAction.action);
            _dropAction.action.performed += OnDrop;
        }

        void OnDisable() {
            _usePrimaryAction.action.performed -= OnUsePrimary;
            _useSecondaryAction.action.performed -= OnUseSecondary;
            _dropAction.action.performed -= OnDrop;
        }

        void OnUsePrimary(InputAction.CallbackContext ctx) =>
            _grabManager.UseItemPrimaryAction();

        void OnUseSecondary(InputAction.CallbackContext ctx) =>
            _grabManager.UseItemSecondaryAction();

        void OnDrop(InputAction.CallbackContext ctx) =>
            _grabManager.Drop();
    }
}
