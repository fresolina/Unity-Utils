namespace Lotec.Interactions {
    /// <summary>
    /// Interactions are actions that can be performed on interactables.
    /// </summary>
    public interface IInteraction {
        string InteractionName { get; }
        void OnStartInteraction();
        void OnCancelInteraction() { } // optional
        bool IsValid { get; }
    }
}
