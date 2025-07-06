using System.Collections.Generic;
using UnityEngine;

namespace Lotec.Interactions {
    public interface IInventory<T> {
        void Add(T interactable);
        bool Remove(T interactable);
        IReadOnlyList<T> Items { get; }
        int Capacity { get; }
        event System.Action InventoryUpdated;
    }

    /// <summary>
    /// Advanced interactable with logic. Also automatically adds required components.
    /// </summary>
    /// 
    public class Inventory : Interactable, IInventory<Interactable> {
        [SerializeField] protected int _capacity = 4;
        // Dynamic list of items. UI handles moving items around
        protected List<Interactable> _items = new List<Interactable>();

        public virtual IReadOnlyList<Interactable> Items => _items;
        public int Capacity => _capacity;
        public event System.Action InventoryUpdated;

        public virtual void Add(Interactable interactable) {
            _items.Add(interactable);
            interactable.gameObject.SetActive(false);
            InventoryUpdated?.Invoke();
        }

        public virtual bool Remove(Interactable interactable) {
            if (!_items.Remove(interactable)) return false;

            interactable.gameObject.SetActive(true);
            InventoryUpdated?.Invoke();
            return true;
        }
    }
}
