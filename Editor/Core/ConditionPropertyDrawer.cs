using UnityEditor;
using Lotec.Utils.Interfaces.Editor;

namespace Lotec.Utils.Triggers.Editor {
    [CustomPropertyDrawer(typeof(ICondition), true)]
    public class IConditionDrawer : SerializeReferenceDrawer<ICondition> { }

    [CustomPropertyDrawer(typeof(IAction), true)]
    public class IActionDrawer : SerializeReferenceDrawer<IAction> { }
}
