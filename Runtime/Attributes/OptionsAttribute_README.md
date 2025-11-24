# Dynamic Options Attributes for Unity Inspector

This library provides two powerful attributes for creating dynamic dropdown fields in the Unity Inspector that are populated by calling methods at edit time.

## Options Attribute (Recommended)

The generic `Options` attribute is more powerful and works with any serializable type that Unity can display in the Inspector.

### Supported Types

- **string**: Strings
- **int**: Integer dropdowns
- **float**: Float value dropdowns  
- **Enums**: Enum value dropdowns
- **UnityEngine.Object derivatives**: GameObject, Component, Material, Texture, etc.
- **Custom serializable classes**: With optional display formatting

### Basic Usage

```csharp
using Lotec.Utils;
using Lotec.Utils.Attributes;
using UnityEngine;

public class MyComponent : MonoBehaviour2
{
    // String options
    [SerializeField, Options(nameof(GetStringOptions))]
    private string selectedString = "";

    // Integer options
    [SerializeField, Options(nameof(GetIntOptions))]
    private int selectedInt = 0;

    // GameObject options
    [SerializeField, Options(nameof(GetGameObjectOptions))]
    private GameObject selectedObject;

    private string[] GetStringOptions()
    {
        return new string[] { "Morning", "Evening", "Night" };
    }

    private int[] GetIntOptions()
    {
        return new int[] { 1, 2, 5, 10, 20 };
    }

    private GameObject[] GetGameObjectOptions()
    {
        // Return all GameObjects with Light components
        return FindObjectsOfType<Light>()
            .Select(light => light.gameObject)
            .Prepend(null) // Include null option
            .ToArray();
    }
}
```

### Custom Objects with Display Formatting

For complex objects, you can specify how they should be displayed in the dropdown:

```csharp
[System.Serializable]
public class LightingScenario
{
    public string name;
    public Color ambientColor;
    public float intensity;
    
    public override string ToString() => name; // Default display
}

public class LightingManager : MonoBehaviour2
{
    // Display with custom format
    [SerializeField, Options(nameof(GetScenarios), "{0.name} (Intensity: {0.intensity})")]
    private LightingScenario selectedScenario;

    private LightingScenario[] GetScenarios()
    {
        return new LightingScenario[]
        {
            new LightingScenario { name = "Morning", intensity = 1.2f },
            new LightingScenario { name = "Evening", intensity = 0.8f },
            new LightingScenario { name = "Night", intensity = 0.3f }
        };
    }
}
```

### Dynamic Content

Options can change based on other field values or game state:

```csharp
public class DynamicExample : MonoBehaviour2
{
    [SerializeField] private bool includeAdvancedOptions = false;
    
    [SerializeField, Options(nameof(GetDynamicOptions))]
    private string selectedOption = "";

    private string[] GetDynamicOptions()
    {
        var options = new List<string> { "Basic Option 1", "Basic Option 2" };
        
        if (includeAdvancedOptions)
        {
            options.AddRange(new[] { "Advanced Option 1", "Expert Mode" });
        }
        
        return options.ToArray();
    }
}
```

## Method Requirements

Methods used with both attributes must:

1. **Take no parameters**: `GetOptions()` ✓, `GetOptions(int count)` ✗
2. **Return compatible types**:
   - **Arrays**: `T[]` (recommended)
   - **Lists**: `List<T>`, `IList<T>`
   - **Enumerables**: `IEnumerable<T>`
   - **Single values**: `T` (creates single-option dropdown)
3. **Be accessible**: Can be public, private, protected, or internal
4. **Be instance or static**: Both are supported

### Return Type Examples

```csharp
// Arrays (recommended)
private string[] GetStringArray() => new[] { "A", "B", "C" };
private GameObject[] GetObjects() => FindObjectsOfType<GameObject>();

// Lists
private List<string> GetStringList() => new List<string> { "A", "B" };
private List<Material> GetMaterials() => new List<Material>();

// Enumerables
private IEnumerable<int> GetNumbers() => Enumerable.Range(1, 10);

// Single values (creates single-option dropdown)
private string GetSingleOption() => "Only Option";
```

## Display Formatting

The `Options` attribute supports custom display formatting for complex objects:

### Format Syntax

- `{0}` - The object itself (calls ToString())
- `{0.PropertyName}` - Access object properties
- `{0.FieldName}` - Access object fields

### Format Examples

```csharp
// Show object name and type
[Options(nameof(GetComponents), "{0.name} ({0.GetType().Name})")]

// Show custom property
[Options(nameof(GetScenarios), "{0.name} - {0.description}")]

// Complex formatting
[Options(nameof(GetItems), "Item: {0.name} (Value: {0.value}, Rarity: {0.rarity})")]
```

## Error Handling

Both attributes provide helpful error messages:

- **Method not found**: "Method 'MethodName' not found."
- **Wrong field type**: Clear indication of unsupported types
- **No options**: "No options available from method."
- **Method exceptions**: Detailed error logging

## Performance Considerations

1. **Method calls**: Options are generated during Inspector rendering, so keep methods lightweight
2. **Caching**: For expensive operations, consider caching:

   ```csharp
   private string[] _cachedOptions;
   private string[] GetCachedOptions()
   {
       return _cachedOptions ??= LoadExpensiveOptions();
   }
   ```

3. **Scene references**: Finding objects in scene is relatively fast, but cache if called frequently

## Best Practices

1. **Include null options**: For object references, include null as first option
2. **Validation**: Use `OnValidate()` to ensure selected values remain valid
3. **Default values**: Provide sensible defaults that are likely to be in the options
4. **Descriptive names**: Use clear method names like `GetAvailableLightingScenarios()`
5. **Error resilience**: Handle cases where no options are available

## Integration with Existing Systems

Both attributes work seamlessly with:

- **MonoBehaviour2**: Part of the existing attribute system
- **Unity Serialization**: Properly serialized and support prefab overrides  
- **Unity Inspector**: Native editor integration
- **Version Control**: No binary data, plays well with Git

## Examples in This Project

See example scripts for practical usage:

- `GenericOptionsExample.cs`: Comprehensive examples with all supported types
