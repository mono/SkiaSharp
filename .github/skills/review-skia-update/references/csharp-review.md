# C# Companion PR Review

When a SkiaSharp companion PR is provided, review the non-generated C# changes.
This is **required** — the companion PR contains the hand-written wrappers that expose
the Skia update to .NET consumers.

## What to Ignore

- **All `*Api.generated.cs` files** — already validated by the orchestrator
- **Whitespace-only changes** — not worth reviewing
- **Comment-only changes** — unless they document a behavioral change

## What to Review

### 1. New or Changed C# Wrappers

Files in `binding/SkiaSharp/` (excluding generated). Check:
- **Null handling** — factory methods return `null` on failure, constructors throw
- **Parameter validation** — `ArgumentNullException` before P/Invoke calls
- **Disposal patterns** — `SKObject.GetObject()` for owned objects, `owns: false` for borrowed
- **Same-instance returns** — methods like `Subset()`, `ToRasterImage()` may return `this`
  ```csharp
  // ✅ Required pattern for same-instance returns
  var result = source.Subset(bounds);
  if (result != source)
      source.Dispose();
  ```

### 2. ABI Compatibility

SkiaSharp maintains stable ABI. Flag any:
- Modified existing method signatures (parameters, return types)
- Removed public APIs (use `[Obsolete]` instead)
- Default parameters in public APIs (use overloads instead)

### 3. New API Overloads

New overloads are safe and expected. Verify:
- They delegate to existing methods (not duplicate logic)
- Parameter names follow existing conventions (`camelCase`)
- `SK` prefix on new types/enums

### 4. Platform-Specific Changes

Skia updates often change platform behavior. Check:
- Font manager changes (platform-specific singletons)
- GPU context factory changes (namespace moves, new parameters)
- Build configuration changes (`*.targets`, `*.props`)

### 5. Test Coverage

Check `tests/Tests/` for:
- New tests covering new APIs
- Updated tests reflecting changed behavior
- No skipped tests (except hardware-dependent: GPU, display)

## Output Format

Add a `companionPr` section to the JSON report:

```json
{
  "companionPr": {
    "prNumber": 3560,
    "summary": "Brief overview of C# changes...",
    "recommendations": ["Action item 1", "..."],
    "filesReviewed": 12,
    "filesSkipped": 4,
    "findings": [
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "type": "new_api",
        "summary": "New DrawSurface overload with SKSamplingOptions parameter"
      }
    ]
  }
}
```

Finding types: `new_api`, `changed_api`, `disposal_pattern`, `abi_concern`,
`missing_test`, `platform_specific`, `other`.
