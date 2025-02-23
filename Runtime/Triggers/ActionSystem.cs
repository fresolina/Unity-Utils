using UnityEngine;
using System;
using System.Collections.Generic;
using Lotec.Utils.Extensions;

namespace Lotec.Utils.Triggers {
    public interface IActionHandlerProvider {
        public Dictionary<Type, IActionHandler> ActionHandlers { get; }
    }

    // TODO: Rename to IConditionHandlerProvider
    public interface IConditionSystem {
        public Dictionary<Type, IConditionHandler> ConditionHandlers { get; }
    }

    // Start() must run after the providers Awake() but before their Start().
    [DefaultExecutionOrder(-1)]
    public class ActionSystem : MonoBehaviour, IActionHandler {
        Dictionary<Type, IActionHandler> _actionHandlers;

        void Start() {
            _actionHandlers = new Dictionary<Type, IActionHandler> { };
            IActionHandlerProvider[] providers = GetComponentsInChildren<IActionHandlerProvider>();
            foreach (IActionHandlerProvider provider in providers) {
                if (provider.ActionHandlers == null) continue;
                _actionHandlers.Merge(provider.ActionHandlers);
            }
        }

        public bool TryExecuteAction(IAction action, Component target = null, Component source = null) {
            if (action == null) return false;

            if (_actionHandlers.TryGetValue(action.GetType(), out IActionHandler handler)) {
                return handler.TryExecuteAction(action, target, source);
            } else {
                Debug.LogWarning($"No handler found for action {action.GetType()}", this);
            }

            return false;
        }
    }
}
