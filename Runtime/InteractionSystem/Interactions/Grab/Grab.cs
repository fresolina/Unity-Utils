
using UnityEngine;

namespace Lotec.Interactions {
    /// <summary>
    /// Grab item interaction. (InteractionSystem integration)
    /// </summary>
    public class Grab : Interaction {
        // Player can only grab the item if they have no item in hand.
        public override bool IsValid => GrabManager.Instance.HeldItem == null;
        public override void OnStartInteraction() {
            GrabManager.Instance.Grab(_interactable.GetComponent<Rigidbody>());
        }
    }
}
