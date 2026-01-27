# Context Gathering Checklist

What to gather before implementing any issue.

## For All Issues

### From GitHub Issue

- [ ] Title and description
- [ ] Labels (bug, enhancement, API, etc.)
- [ ] Linked issues or PRs
- [ ] Comments (often contain clarifications)
- [ ] Reporter's environment if relevant

### From Codebase

- [ ] Affected class/method location
- [ ] Related tests that exist
- [ ] Similar patterns in nearby code
- [ ] Recent git history for the area

## Type-Specific Context

After gathering basics above, load the appropriate reference:
- **New API:** See [new-api.md](new-api.md) for what else to gather
- **Bug Fix:** See [bug-fix.md](bug-fix.md) for what else to gather
- **Enhancement:** Follow new-api (if adding) or bug-fix (if improving) workflow

## Useful Commands

```bash
# Find class file
grep -rn "class SKTargetType" binding/SkiaSharp/

# Find method
grep -rn "public.*MethodName" binding/SkiaSharp/

# Find test file
find tests -name "*.cs" | xargs grep -l "SKTargetType"

# Check git history
git log --oneline -10 -- binding/SkiaSharp/SKTargetType.cs

# Find C API
grep -r "sk_targettype" externals/skia/include/c/
```

## Test Infrastructure

Tests inherit from `BaseTest` which provides:

| Helper | Usage |
|--------|-------|
| `PathToImages` | `Path.Combine(PathToImages, "baboon.jpg")` |
| `PathToFonts` | `Path.Combine(PathToFonts, "segoeui.ttf")` |
| `IsWindows/Mac/Linux` | Platform-specific tests |
| `DefaultFontFamily` | Platform-appropriate font |

Test images available in `tests/Content/images/`:
- `baboon.jpg`, `baboon.png` - Standard test image
- `color-wheel.png` - Color testing
- Various format samples

## Documentation References

| Topic | File |
|-------|------|
| API design | `documentation/api-design.md` |
| Adding APIs | `documentation/adding-apis.md` |
| Memory management | `documentation/memory-management.md` |
| Error handling | `documentation/error-handling.md` |
| Architecture | `documentation/architecture.md` |

## Key Files

| Purpose | Location |
|---------|----------|
| C# bindings | `binding/SkiaSharp/*.cs` |
| Tests | `tests/Tests/SkiaSharp/*.cs` |
| Test images | `tests/Content/images/` |
| Test fonts | `tests/Content/fonts/` |
| C API headers | `externals/skia/include/c/*.h` |
| C API implementation | `externals/skia/src/c/*.cpp` |
