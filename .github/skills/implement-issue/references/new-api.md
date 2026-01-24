# New API Implementation

Workflow for adding new APIs to SkiaSharp.

## ABI Safety

**Critical:** Verify ABI safety before implementing. See SKILL.md for the rules.

## Workflow

### 1. Understand the Request

- What API is being requested?
- Is there an existing similar API to follow?
- Does this need C API changes or C#-only?

### 2. Find Similar Patterns

Search for similar existing code:

```bash
# Find the target class
grep -r "class SKTargetType" binding/SkiaSharp/

# Find similar methods
grep -r "public.*MethodName" binding/SkiaSharp/

# Check if C API exists
grep -r "sk_targettype_methodname" externals/skia/include/c/
```

### 3. Check C API Availability

| Scenario | Action |
|----------|--------|
| C API exists | Implement C# wrapper only |
| C API missing | Need to add C API first (see documentation/adding-apis.md) |

### 4. Implementation Checklist

- [ ] Find target class in `binding/SkiaSharp/`
- [ ] Identify overload chain pattern (simpler → more complete)
- [ ] Add new overload(s) - delegate to existing where possible
- [ ] Validate parameters at C# layer
- [ ] Handle null/error returns appropriately
- [ ] Add tests in `tests/Tests/SkiaSharp/`
- [ ] Build: `dotnet build binding/SkiaSharp/SkiaSharp.csproj`
- [ ] Test: `dotnet test tests/SkiaSharp.Tests.Console/...`

## Common Patterns

### Overload Chain

```csharp
// Simpler overload delegates to complete version
public static SKData CreateCopy(byte[] bytes) =>
    CreateCopy(bytes, (ulong)bytes.Length);

public static SKData CreateCopy(byte[] bytes, ulong length)
{
    // Core implementation with validation
    if (bytes == null)
        throw new ArgumentNullException(nameof(bytes));
    // ...
}
```

### Factory Method (returns null on failure)

```csharp
public static SKImage FromEncodedData(SKData data)
{
    if (data == null)
        throw new ArgumentNullException(nameof(data));
    return GetObject(SkiaApi.sk_image_new_from_encoded(data.Handle));
}
```

### Property with Native Call

```csharp
public SKShader Shader
{
    get => SKShader.GetObject(SkiaApi.sk_paint_get_shader(Handle));
    set => SkiaApi.sk_paint_set_shader(Handle, value?.Handle ?? IntPtr.Zero);
}
```

## Key Files

See [context-checklist.md](context-checklist.md) for file locations.
