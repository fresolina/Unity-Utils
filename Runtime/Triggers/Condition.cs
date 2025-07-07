using System;
using Lotec.Utils.Interfaces;
using UnityEngine;

namespace Lotec.Utils.Triggers
{
    public interface IConditionHandler
    {
        public bool CheckCondition(ICondition condition, Component target = null, Component source = null);
        // TODO: Ta bort CheckConditions. Skapa en Condition som heter AllConditionsTrue som gör detta.
        //       Sen en AnyConditionTrue som returnerar true om någon av villkoren är true.
        public bool CheckConditions(ICondition[] conditions, Component target = null, Component source = null)
        {
            for (int i = 0; i < conditions.Length; i++)
            {
                if (!CheckCondition(conditions[i], target, source)) return false;
            }
            return true;
        }
    }
    public interface IActionHandler
    {
        /// <summary>
        /// Execute an action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns>Was the action executed?</returns>
        public bool TryExecuteAction(IAction action, Component target = null, Component source = null);
        public void ExecuteActions(IAction[] actions, Component target = null, Component source = null)
        {
            if (actions == null) return;

            foreach (IAction action in actions)
            {
                TryExecuteAction(action, target, source);
            }
        }
    }

    [SerializeInterface]
    public interface ICondition { }
    [SerializeInterface]
    public interface IAction { }

    [Serializable, NoSerializeInterface] // TODO: Lägg in att aldrig serialisera abstracts
    public abstract class BoolCondition : ICondition
    {
        [field: SerializeField] public bool True { get; set; } = true;
    }
}
