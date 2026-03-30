# C# Companion PR Review

The orchestrator mechanically generates the `companionPr` section in `raw-results.json`
with file lists, diffs, and categories (added/changed). Your job is to:
1. Add a `summary` to each file item (same as upstream/interop summaries)
2. Review the diffs for correctness (see checklist below)
3. Add `relatedFiles` cross-links to interop files where applicable

## What to Ignore

- **All `*Api.generated.cs` files** — already filtered out by `check_companion.py`
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

Add a `companionPr` section to the JSON report using the same `sourceFile` structure
as upstream/interop integrity sections:

```json
{
  "companionPr": {
    "prNumber": 3560,
    "status": "REVIEW_REQUIRED",
    "summary": "Brief overview of C# changes...",
    "recommendations": ["Action item 1", "..."],
    "added": [
      {
        "path": "binding/SkiaSharp/SKFoo.cs",
        "summary": "New wrapper for sk_foo_bar API — creates SKFoo with factory method",
        "diff": "--- a/dev/null\n+++ b/binding/SkiaSharp/SKFoo.cs\n...",
        "relatedFiles": [
          { "path": "src/c/sk_foo.cpp", "relationship": "C API implementation" }
        ]
      }
    ],
    "changed": [
      {
        "path": "binding/SkiaSharp/SKSurface.cs",
        "summary": "New DrawSurface overload with SKSamplingOptions parameter",
        "diff": "...",
        "oldDiff": "...",
        "newDiff": "...",
        "patchDiff": "..."
      }
    ],
    "unchanged": 4
  }
}
```

Categories:
- `added` — new C# files (new wrappers, new test files)
- `changed` — modified C# files (updated wrappers, new overloads, signature changes)
- `unchanged` — count of reviewed files with no changes requiring attention (skipped generated files)

Use `relatedFiles` to cross-link companion PR files to their interop counterparts
(e.g., a C# wrapper → its C API implementation in the interop section).
