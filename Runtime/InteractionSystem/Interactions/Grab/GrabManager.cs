using Lotec.Utils;
using UnityEngine;

public class GrabManager : MonoBehaviour2 {
    public static GrabManager Instance { get; private set; }

    [Tooltip("The player's hand position where items will be held.")]
    [SerializeField] Transform _handTransform;
    Rigidbody _heldItem;

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
        _heldItem = null;
    }

    internal void UseItemSecondaryAction() {
        if (_heldItem == null) return;

        // Implement secondary action logic here
        Debug.Log($"Using {_heldItem.name} with secondary action.");
    }
    internal void UseItemPrimaryAction() {
        if (_heldItem == null) return;

        // Implement primary action logic here
        Debug.Log($"Using {_heldItem.name} with primary action.");
    }
}
