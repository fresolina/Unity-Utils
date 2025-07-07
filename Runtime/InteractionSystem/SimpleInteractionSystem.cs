using System.Collections.Generic;
using Lotec.Utils;
using Lotec.Utils.Attributes;
using UnityEngine;

namespace Lotec.Interactions {
    /// <summary>
    /// Tells player what they can interact with. Supports up to 4 interactions.
    /// Finds interactable in sight, and uses an IInteraction on that Interactable.
    /// * Finds all valid interactions -> event InteractionsUpdated.
    /// * Execute interaction on E press.
    /// * The interaction can require an item in hand to be usable, for example a key to unlock a door.
    /// * Finds interactable in sight. Event on change.
    /// </summary>
    [ScriptTooltip("InteractionManager?\nTells player what they can interact with.\nTells player what they can do with item in hand.\nTells player what they can do with item in world.\nOrder: _itemInHand.InteractWith(_aimer.ItemInWorld), _aimer.ItemInWorld.Interact(), _itemInHand.Interact()")]
    public class SimpleInteractionSystem : MonoBehaviour2, IInteractionSystem {
        [SerializeField, NotNull] IObjectSensor<Interactable> _sensor;
        IInteraction _interaction;

        // InteractableInSight, used by UI.
        public Interactable ItemInWorld => _sensor.SensorObject;
        public List<IInteraction> ValidInteractions { get; } = new List<IInteraction>();
        /// <summary>
        /// Event triggers when interactable in sight changes. Useful for UI to show/hide info.
        /// </summary>
        public event System.Action<IInteractionSystem> InteractionsUpdated;

        void Start() {
            // _hands.InventoryUpdated += UpdateInteractions;
            _sensor.SensorUpdated += (obj) => UpdateInteractions();
        }

        public void ShowMessage(IInteraction interaction, string v) => Debug.Log(v);


        public void Interact(int index) {
            if (index >= ValidInteractions.Count) return;

            _interaction = ValidInteractions[index];
            _interaction.OnStartInteraction();
            UpdateInteractions();
        }

        public void CancelInteraction() {
            if (_interaction == null) return;

            _interaction.OnCancelInteraction();
            _interaction = null;
            UpdateInteractions();
        }
        /// <summary>
        /// Run if:
        /// * ItemInWorld.changed: _aimer.ItemInWorld
        /// * _hands.Items.changed: _hands.InventoryUpdated
        /// </summary>
        public void UpdateInteractions() {
            UpdateValidInteractions();

            InteractionsUpdated?.Invoke(this);
        }

        void UpdateValidInteractions() {
            ValidInteractions.Clear();
            if (_sensor.SensorObject == null) return;

            AddValidInteractions(ValidInteractions);
        }

        void AddValidInteractions(List<IInteraction> validInteractions) {
            IInteraction[] interactions = _sensor.SensorObject.Interactions;
            for (int i = 0; i < interactions.Length; i++) {
                IInteraction interaction = interactions[i];
                if (!((MonoBehaviour)interaction).enabled) continue; // Skip disabled interactions.

                if (interaction.IsValid) {
                    validInteractions.Add(interaction);
                }
            }
        }
    }

    [System.Serializable]
    public struct InteractionMap {
        public IInteraction Interaction;
        public Interactable Item;
    }
}
