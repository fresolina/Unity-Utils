#if UNITY_EDITOR
using Lotec.Utils.Interfaces.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace Lotec.Utils.Triggers.Editor {
    [CustomPropertyDrawer(typeof(ICondition), true)]
    public class SerializeReferenceUIDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            return new SerializeReferenceElement<ICondition>(property);
        }
    }


    [CustomPropertyDrawer(typeof(IAction), true)]
    public class IActionDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            return new SerializeReferenceElement<ICondition>(property);
        }
    }

}
#endif
