using Lotec.Utils.Triggers;
using UnityEngine;

namespace Lotec.Utils {
    public class CheckRigidbodyEnabled : BoolCondition {
        [SerializeField] Rigidbody _rigidbody;
        public override bool IsMet() => _rigidbody.useGravity == True;
    }
}
