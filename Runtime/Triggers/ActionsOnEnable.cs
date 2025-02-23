using UnityEngine;

namespace Lotec.Utils.Triggers
{
    public class ActionsOnEnable : MonoBehaviour
    {
        [field: SerializeReference] public IAction[] ActionsOnEnabled { get; private set; }
        IActionHandler _actionSystem;
        bool _started = false;

        void Awake()
        {
            _actionSystem = FindAnyObjectByType<ActionSystem>();
        }

        void Start()
        {
            _started = true;
            OnEnableAfterStart();
        }

        void OnEnable()
        {
            if (!_started) return;
            OnEnableAfterStart();
        }

        void OnEnableAfterStart()
        {
            if (ActionsOnEnabled == null) return;
            _actionSystem.ExecuteActions(ActionsOnEnabled);
        }
    }
}
