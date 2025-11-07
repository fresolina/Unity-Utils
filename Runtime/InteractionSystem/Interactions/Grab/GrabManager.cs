using Lotec.Interactions;
using Lotec.Utils;
using Lotec.Utils.Attributes;
using UnityEngine;

public class GrabManager : MonoBehaviour2 {
    public static GrabManager Instance { get; private set; }

    [Tooltip("The player's hand position where items will be held.")]
    [SerializeField, NotNull] Transform _handTransform;
    [SerializeField] IInteractionSystem _interactionSystem;
    Rigidbody _heldItem;
    int _heldItemLayer;

    public GameObject HeldItem => _heldItem == null ? null : _heldItem.gameObject;

    void Awake() {
        if (Instance != null) {
            Debug.LogError("Duplicate Singleton: GrabManager already exists.", Instance.gameObject);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool IsHolding<T>() => _heldItem != null && _heldItem.TryGetComponent<T>(out _);
    public bool TryGetHolding<T>(out T item) {
        if (_heldItem != null && _heldItem.TryGetComponent(out item)) {
            return true;
        }
        item = default;
        return false;
    }

    public void Grab(Rigidbody body) {
        if (_heldItem != null) return;

        _heldItem = body;
        // Set layer to "Ignore Raycast" in object held, so it doesn't interfere with InteractionSystem raycasts
        _heldItemLayer = _heldItem.gameObject.layer;
        var colliders = _heldItem.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].gameObject.layer = 2; // Layer 2 is "Ignore Raycast"
        }

        // Set Rigidbody properties for holding
        _heldItem.isKinematic = true;
        _heldItem.useGravity = false;
        _heldItem.transform.SetParent(_handTransform);
        _heldItem.transform.localPosition = Vector3.zero;
        _heldItem.transform.localRotation = Quaternion.identity;
    }

    public void Drop() {
        if (_heldItem == null) return;

        _heldItem.isKinematic = false;
        _heldItem.useGravity = true;
        _heldItem.transform.SetParent(null);
        var colliders = _heldItem.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].gameObject.layer = _heldItemLayer;
        }
        _heldItem = null;
        _interactionSystem?.UpdateInteractions();
    }

    internal void UseItemSecondaryAction() {
        if (_heldItem == null) return;

        Interactable interactable = _heldItem.GetComponent<Interactable>();
        if (interactable == null) return;

        if (interactable.Interactions.Length > 1) {
            interactable.Interactions[1].OnStartInteraction();
            return;
        }

        Debug.Log($"Using {_heldItem.name} with secondary action.");
    }
    internal void UseItemPrimaryAction() {
        if (_heldItem == null) return;

        Interactable interactable = _heldItem.GetComponent<Interactable>();
        if (interactable == null) return;

        if (interactable.Interactions.Length > 0) {
            interactable.Interactions[0].OnStartInteraction();
            return;
        }

        Debug.Log($"Using {_heldItem.name} with primary action.");
    }
}
