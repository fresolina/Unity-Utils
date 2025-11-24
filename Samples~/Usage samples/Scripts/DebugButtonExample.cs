using Lotec.Utils.Attributes;
using Lotec.Utils.Triggers;
using UnityEngine;


namespace Lotec.Utils.Examples {
    /// <summary>
    /// Example MonoBehaviour2 class to test debug buttons functionality.
    /// </summary>
    public class DebugButtonExample : MonoBehaviour2 {
        [SerializeField] private string message = "Hello from debug button!";
        [SerializeField] private int counter = 0;
        [SerializeField, NotNull] private GameObject requiredObject;
        [SerializeReference, NotNull] IAction requiredAction;
        [SerializeReference, NotNull] IAction[] requiredActions;

        /// <summary>
        /// A simple public method that logs a message.
        /// </summary>
        public void LogMessage() {
            Debug.Log($"[{name}] {message}");
        }

        /// <summary>
        /// A public method that increments and logs a counter.
        /// </summary>
        public void IncrementCounter() {
            counter++;
            Debug.Log($"[{name}] Counter is now: {counter}");
        }

        /// <summary>
        /// A public method that resets the counter.
        /// </summary>
        public void ResetCounter() {
            counter = 0;
            Debug.Log($"[{name}] Counter reset to 0");
        }

        /// <summary>
        /// A public method with parameters - this should show a warning when clicked.
        /// </summary>
        public void MethodWithParams(string param) {
            Debug.Log($"This method has parameters: {param}");
        }

        /// <summary>
        /// A private method - this should not appear in debug buttons.
        /// </summary>
        private void PrivateMethod() {
            Debug.Log("This is a private method");
        }
    }
}
