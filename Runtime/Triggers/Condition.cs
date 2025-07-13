using System;
using System.Collections;
using Lotec.Utils.Interfaces;
using UnityEngine;

namespace Lotec.Utils.Triggers {
    public interface IConditionHandler {
        public bool CheckCondition(ICondition condition, Component target = null, Component source = null);
        // TODO: Ta bort CheckConditions. Skapa en Condition som heter AllConditionsTrue som gör detta.
        //       Sen en AnyConditionTrue som returnerar true om någon av villkoren är true.
        public bool CheckConditions(ICondition[] conditions, Component target = null, Component source = null) {
            for (int i = 0; i < conditions.Length; i++) {
                if (!CheckCondition(conditions[i], target, source)) return false;
            }
            return true;
        }
    }
    public interface IActionHandler {
        /// <summary>
        /// Execute an action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns>Was the action executed?</returns>
        public bool TryExecuteAction(IAction action, Component target = null, Component source = null);
        public void ExecuteActions(IAction[] actions, Component target = null, Component source = null) {
            if (actions == null) return;

            foreach (IAction action in actions) {
                TryExecuteAction(action, target, source);
            }
        }
    }

    [SerializeInterface]
    public interface ICondition {
        /// <summary>
        /// Check if the condition is met.
        /// </summary>
        bool IsMet();
    }
    [SerializeInterface]
    public interface IAction {
        /// <summary>
        /// Execute the action.
        /// </summary>
        IEnumerator Execute();
    }

    [Serializable, NoSerializeInterface] // TODO: Lägg in att aldrig serialisera abstracts
    public abstract class BoolCondition : ICondition {
        [field: SerializeField] public bool True { get; set; } = true;

        public abstract bool IsMet();
    }

    // Or
    public class AnyCondition : ICondition {
        [field: SerializeReference] public ICondition[] Conditions { get; private set; }

        public bool IsMet() {
            for (int i = 0; i < Conditions.Length; i++) {
                ICondition condition = Conditions[i];
                if (condition.IsMet())
                    return true;
            }
            return false;
        }
    }

    // And
    public class AllCondition : ICondition {
        [field: SerializeReference] public ICondition[] Conditions { get; private set; }

        public bool IsMet() {
            for (int i = 0; i < Conditions.Length; i++) {
                ICondition condition = Conditions[i];
                if (!condition.IsMet())
                    return false;
            }
            return true;
        }
    }

    // Wrap multiple actions in one action
    public class MultipleAction : IAction {
        [field: SerializeReference] public IAction[] Actions { get; private set; }

        public IEnumerator Execute() {
            for (int i = 0; i < Actions.Length; i++) {
                IAction action = Actions[i];
                yield return action.Execute();
            }
        }
    }

}
