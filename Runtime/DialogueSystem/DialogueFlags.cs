using System;
using System.Collections;
using System.Collections.Generic;
using Lotec.Utils.Triggers;
using UnityEngine;

namespace Lotec.Utils.DialogueSystem {
    /// <summary>
    /// A simple static memory for dialogue states.
    /// In a real game, you would save/load this dictionary.
    /// </summary>
    public static class DialogueFlags {
        static readonly HashSet<UnityEngine.Object> s_flags = new HashSet<UnityEngine.Object>();

        public static void Set(UnityEngine.Object flag) => s_flags.Add(flag);
        public static void Unset(UnityEngine.Object flag) => s_flags.Remove(flag);
        public static bool Has(UnityEngine.Object flag) => s_flags.Contains(flag);
        public static void Clear() => s_flags.Clear();
    }

    [Serializable]
    public class CheckFlagCondition : ICondition {
        [SerializeField] UnityEngine.Object _flag;
        [SerializeField] bool _shouldExist = true;

        public bool IsMet() {
            return DialogueFlags.Has(_flag) == _shouldExist;
        }
    }

    [Serializable]
    public class SetFlagAction : IAction {
        [SerializeField] UnityEngine.Object _flag;
        [SerializeField] bool _set = true;

        public IEnumerator Execute() {
            if (_set)
                DialogueFlags.Set(_flag);
            else
                DialogueFlags.Unset(_flag);
            yield return null;
        }
    }
}
