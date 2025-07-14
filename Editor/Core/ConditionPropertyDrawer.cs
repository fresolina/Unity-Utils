#if UNITY_EDITOR
using Lotec.Utils.Interfaces.Editor;
using UnityEditor;

namespace Lotec.Utils.Triggers.Editor {
    [CustomPropertyDrawer(typeof(ICondition), true)]
    public class IConditionDrawer : SerializeReferenceDrawer<ICondition> { }

    [CustomPropertyDrawer(typeof(IAction), true)]
    public class IActionDrawer : SerializeReferenceDrawer<IAction> { }
}
#endif
