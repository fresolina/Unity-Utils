using UnityEditor;
using UnityEngine;

namespace Lotec.Utils {
    public class MoveComponentTool {
        const string k_menuMoveToTop = "CONTEXT/Component/Move To Top";
        const string k_menuMoveToBottom = "CONTEXT/Component/Move To Bottom";

        [MenuItem(k_menuMoveToTop, priority = 501)]
        public static void MoveComponentToTopMenuItem(MenuCommand command) {
            while (UnityEditorInternal.ComponentUtility.MoveComponentUp((Component)command.context)) ;
        }

        [MenuItem(k_menuMoveToTop, validate = true)]
        public static bool MoveComponentToTopMenuItemValidate(MenuCommand command) {
            if (command.context == null) return false;
            Component component = ((Component)command.context);
            Component[] components = component.GetComponents<Component>();
            return components.Length > 1 && components[1] != component;
        }

        [MenuItem(k_menuMoveToBottom, priority = 501)]
        public static void MoveComponentToBottomMenuItem(MenuCommand command) {
            while (UnityEditorInternal.ComponentUtility.MoveComponentDown((Component)command.context)) ;
        }

        [MenuItem(k_menuMoveToBottom, validate = true)]
        public static bool MoveComponentToBottomMenuItemValidate(MenuCommand command) {
            if (command.context == null) return false;
            Component component = ((Component)command.context);
            Component[] components = component.GetComponents<Component>();
            return components.Length > 1 && components[components.Length - 1] != component;
        }
    }
}
