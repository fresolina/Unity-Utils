using Lotec.Utils.Triggers;
using UnityEngine;

namespace Lotec.Utils {
    public class LevelConditionManager : MonoBehaviour2 {
        [SerializeField] Component _target;
        [SerializeReference] ICondition _condition;
        [SerializeReference] AllCondition _allCondition;
        [SerializeReference] AnyCondition _anyCondition;
        [SerializeReference] IAction _action;
        public string Foo;

        public void CheckCondition() {
            if (_condition == null) return;
            if (_condition.IsMet()) {
                Debug.Log("Condition met: " + _condition);
                if (_action != null) {
                    StartCoroutine(_action.Execute());
                }
            } else {
                Debug.Log("Condition not met: " + _condition);
            }
        }
    }
}
