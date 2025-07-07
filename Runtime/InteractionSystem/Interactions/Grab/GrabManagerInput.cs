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

        void OnEnable() {
            _usePrimaryAction.action.Enable();
            _usePrimaryAction.action.performed += OnUsePrimary;

            _useSecondaryAction.action.Enable();
            _useSecondaryAction.action.performed += OnUseSecondary;

            _dropAction.action.Enable();
            _dropAction.action.performed += OnDrop;
        }

        void OnDisable() {
            _usePrimaryAction.action.performed -= OnUsePrimary;
            _usePrimaryAction.action.Disable();

            _useSecondaryAction.action.performed -= OnUseSecondary;
            _useSecondaryAction.action.Disable();

            _dropAction.action.performed -= OnDrop;
            _dropAction.action.Disable();
        }

        void OnUsePrimary(InputAction.CallbackContext _) => _grabManager.UseItemPrimaryAction();

        void OnUseSecondary(InputAction.CallbackContext _) => _grabManager.UseItemSecondaryAction();

        void OnDrop(InputAction.CallbackContext _) => _grabManager.Drop();
    }
}
