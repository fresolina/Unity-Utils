using System.Collections.Generic;

namespace Lotec.Interactions {
    public interface IInteractionSystem {
        Interactable ItemInWorld { get; }
        List<IInteraction> ValidInteractions { get; }
        event System.Action<IInteractionSystem> InteractionsUpdated;
        void ShowMessage(IInteraction interaction, string v);
        void UpdateInteractions();
    }
}
