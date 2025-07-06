namespace Lotec.Interactions {
    /// <summary>
    /// Interactions are actions that can be performed on interactables.
    /// </summary>
    public interface IInteraction {
        string InteractionName { get; }
        void OnStartInteraction();
        void OnStartInteraction(IInteractionSystem system, Interactable withInteractable);
        void OnCancelInteraction() { }

        bool CanInteractWith(Interactable itemInHand);
        bool IsInteractable { get; }
    }
}
