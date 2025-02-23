using System;

namespace Lotec.Utils.Interfaces
{
    [AttributeUsage(AttributeTargets.Interface)]
    /// <summary>
    /// Mark an interface class to be serialized, when used as a field with SerializeReference attribute.
    /// </summary>
    public class SerializeInterfaceAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class NoSerializeInterfaceAttribute : Attribute { }
}
