
using UnityEngine;

namespace Lotec.Interactions {
    /// <summary>
    /// Grab item. (Move item to hand).
    /// </summary>
    public class Grab : Interaction {
        // Player can only grab the item if they have no item in hand.
        public override bool CanInteractWith(Interactable item) => GrabManager.Instance.HeldItem == null;
        public override void OnStartInteraction(IInteractionSystem system, Interactable withInteractable) {
            ShowMessage($"Grabbing {_interactable.Name}...");
            GrabManager.Instance.Grab(_interactable.GetComponent<Rigidbody>());
        }
    }
}
