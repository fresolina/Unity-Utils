using System.Text;
using Lotec.Utils;
using Lotec.Utils.Attributes;
using UnityEngine;

namespace Lotec.Interactions.GUI {
    /// <summary>
    /// Show text about what the player can interact with.
    /// </summary>
    public class InteractionSystemUI : MonoBehaviour2 {
        [SerializeField, NotNull] SimpleInteractionSystem _interactionSystem;
        [SerializeField, NotNull] InteractionSystemInput _input;
        [SerializeField, NotNull] InfoHandler _interactionInfoHandler;
        [SerializeField, NotNull] InfoHandler _interactableNameInfoHandler;
        [SerializeField, NotNull] InfoHandler _interactableDescriptionInfoHandler;
        [SerializeField] bool _showButtonPrefix = true;
        readonly StringBuilder _stringBuilder = new StringBuilder();
        IInteractionSystem _system;

        void Start() {
            _interactionSystem.InteractionsUpdated += OnInteractionChanged;
            ClearInteractableInfos();
            _interactionInfoHandler.SetText(string.Empty);
        }

        void OnInteractionChanged(IInteractionSystem system) {
            _system = system;
            UpdateInfo();
        }


        void UpdateInfo() {
            SetInteractableInfos();
            SetInteractionInfos();
        }

        void SetInteractionInfos() {
            _stringBuilder.Clear();
            for (int i = 0; i < 4 && i < _system.ValidInteractions.Count; i++) {
                AddInteraction(_system.ValidInteractions[i], _input.ActionKeys[i]);
            }
            _interactionInfoHandler.SetText(_stringBuilder.ToString());
        }

        void AddInteraction(InteractionMap interactionMap, string prefix) {
            if (_stringBuilder.Length > 0) {
                _stringBuilder.Append("\n");
            }
            if (_showButtonPrefix) {
                _stringBuilder.Append($"{prefix}: ");
            }
            _stringBuilder.Append($"{interactionMap.Interaction.InteractionName}");
        }

        void SetInteractableInfos() {
            if (_system.ItemInWorld == null) {
                ClearInteractableInfos();
                return;
            }

            _interactableNameInfoHandler.SetText(_system.ItemInWorld.Name);
            _interactableDescriptionInfoHandler.SetText(_system.ItemInWorld.Description);
        }

        void ClearInteractableInfos() {
            _interactableNameInfoHandler.SetText(string.Empty);
            _interactableDescriptionInfoHandler.SetText(string.Empty);
        }

#if UNITY_EDITOR
        protected override void Reset() {
            base.Reset();
            if (gameObject.scene.IsValid()) // Don't do this for prefab assets
                _interactionSystem = FindAnyObjectByType<SimpleInteractionSystem>();
        }
#endif
    }
}
