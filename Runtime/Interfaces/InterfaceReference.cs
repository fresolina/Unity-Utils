using System;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class InterfaceReference<TInterface, TObject> where TObject : Object where TInterface : class {
    [SerializeField, HideInInspector] TObject _underlyingValue;

    public TInterface Value {
        get => _underlyingValue switch {
            null => null,
            TInterface @interface => @interface,
            _ => throw new InvalidOperationException($"{_underlyingValue} needs to implement interface {nameof(TInterface)}.")
        };
        set => _underlyingValue = value switch {
            null => null,
            TObject newValue => newValue,
            _ => throw new ArgumentException($"{value} needs to be of type {typeof(TObject)}.", string.Empty)
        };
    }

    public TObject UnderlyingValue {
        get => _underlyingValue;
        set => _underlyingValue = value;
    }

    public InterfaceReference() { }

    public InterfaceReference(TObject target) => _underlyingValue = target;

    public InterfaceReference(TInterface @interface) => _underlyingValue = @interface as TObject;

    public static implicit operator TInterface(InterfaceReference<TInterface, TObject> obj) => obj.Value;
}

[Serializable]
public class InterfaceReference<TInterface> : InterfaceReference<TInterface, Object> where TInterface : class { }
