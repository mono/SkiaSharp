# SkiaSharp - AGENTS.md

## Project Overview

SkiaSharp is a cross-platform 2D graphics API for .NET that wraps Google's Skia Graphics Library. It uses a three-layer architecture to bridge native C++ code with managed C#.

**Key principle:** C++ exceptions cannot cross the C API boundary - all error handling uses return values.

## Architecture

### Three-Layer Design
```
C# Wrapper Layer (binding/SkiaSharp/) 
    ↓ P/Invoke
C API Layer (externals/skia/include/c/, externals/skia/src/c/)
    ↓ Type casting
C++ Skia Library (externals/skia/)
```

**Call flow example:**
```
SKCanvas.DrawRect() → sk_canvas_draw_rect() → SkCanvas::drawRect()
```

## Critical Concepts

### Memory Management - Pointer Types

Three pointer types with different ownership rules:
- **Raw (`T*`)**: Non-owning, no cleanup needed
- **Owned**: Single owner, caller deletes (Canvas, Paint, Path)
- **Reference-Counted**: Shared ownership with ref counting (Image, Shader, Data)

**Critical:** Wrong pointer type = memory leaks or crashes.

👉 **Full details:** [documentation/memory-management.md](documentation/memory-management.md)

### Error Handling

- **C# validates all parameters** before calling C API
- **C API is minimal wrapper** - no validation, trusts C#
- **Factory methods return null** on failure (do NOT throw)
- **Constructors throw** on failure

👉 **Full details:** [documentation/error-handling.md](documentation/error-handling.md)

## File Organization

### Naming Convention
```
C++: SkCanvas.h → C API: sk_canvas.h, sk_canvas.cpp → C#: SKCanvas.cs
Pattern: SkType → sk_type_t* → SKType
```

### Key Directories

**Do Not Modify:**
- `docs/` - Auto-generated API documentation

**Core Areas:**
- `externals/skia/include/c/` - C API headers
- `externals/skia/src/c/` - C API implementation
- `binding/SkiaSharp/` - C# wrappers and P/Invoke
- `documentation/` - Architecture documentation (comprehensive guides)

## Adding New APIs - Quick Steps

1. Find C++ API in Skia
2. Identify pointer type (raw/owned/ref-counted)
3. Add C API wrapper (minimal, no validation)
4. Add C API header
5. Regenerate P/Invoke declarations
6. Add C# wrapper with validation

👉 **Full step-by-step guide:** [documentation/adding-apis.md](documentation/adding-apis.md)

## Common Pitfalls

❌ Wrong pointer type → memory leaks/crashes  
❌ Missing ref count increment when C++ expects `sk_sp<T>`  
❌ Disposing borrowed objects  
❌ Not checking factory method null returns  
❌ Missing parameter validation in C#  

👉 **Full list with solutions:** [documentation/memory-management.md#common-mistakes](documentation/memory-management.md#common-mistakes) and [documentation/error-handling.md#common-mistakes](documentation/error-handling.md#common-mistakes)

## Code Generation

- **Hand-written:** C API layer (all `.cpp` in `externals/skia/src/c/`)
- **Generated:** P/Invoke declarations (`SkiaApi.generated.cs`)
- **Tool:** `utils/SkiaSharpGenerator/`

To regenerate:
```bash
dotnet run --project utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate
```

## Testing Checklist

- [ ] Pointer type correctly identified
- [ ] Memory properly managed (no leaks)
- [ ] Object disposes correctly
- [ ] Error cases handled (null params, failed operations)
- [ ] P/Invoke signature matches C API
- [ ] Parameters validated in C#
- [ ] Return values checked

## Threading

**⚠️ Skia is NOT thread-safe** - Most objects must be used from a single thread.

| Type | Thread-Safe? | Notes |
|------|--------------|-------|
| **Canvas/Paint/Path** | ❌ No | Keep thread-local |
| **Image/Shader/Typeface** | ✅ Yes* | Read-only after creation |

*Immutable objects can be shared across threads.

👉 **Full threading guide:** [documentation/architecture.md#threading-model](documentation/architecture.md#threading-model)

## Build Commands

```bash
# Build managed code only (after downloading native bits)
dotnet cake --target=libs

# Run tests
dotnet cake --target=tests

# Download pre-built native libraries
dotnet cake --target=externals-download
```

## Documentation

**Quick reference:** This file + code comments

**Contributing:** [CONTRIBUTING.md](CONTRIBUTING.md) - How to submit issues and PRs

**Detailed guides** in `documentation/` folder:
- `README.md` - Documentation index
- `architecture.md` - Three-layer architecture, design principles
- `memory-management.md` - **Critical**: Pointer types, ownership, lifecycle
- `error-handling.md` - Error propagation patterns through layers
- `adding-apis.md` - Complete step-by-step guide with examples

**Build & maintainer guides** also in `documentation/`:
- `building.md` - Build instructions for Windows & macOS
- `building-linux.md` - Build instructions for Linux
- `maintaining.md` - Maintainer responsibilities
- `releasing.md` - Release verification process

**AI assistant context:** `.github/copilot-instructions.md`

## Quick Decision Trees

**"What pointer type?"**  
Inherits SkRefCnt/SkNVRefCnt? → Reference-counted  
Mutable (Canvas/Paint)? → Owned  
Parameter/getter? → Raw (non-owning)  

**"What wrapper pattern?"**  
Reference-counted → `ISKReferenceCounted` or `ISKNonVirtualReferenceCounted`  
Owned → `SKObject` with `DisposeNative()`  
Raw → `owns: false` in handle  

**"How to handle errors?"**  
C API → Minimal pass-through; no extra exception handling, returns whatever Skia returns (bool/null/void)  
C# → Validate where needed, but some APIs propagate null/bool/default results instead of throwing  

👉 **See also:** [documentation/adding-apis.md](documentation/adding-apis.md)

## Examples

See [documentation/adding-apis.md](documentation/adding-apis.md) for complete examples with all three layers.

## When In Doubt

1. Find similar existing API and follow its pattern
2. Check `documentation/` for detailed guidance
3. Verify pointer type carefully (most important!)
4. Test memory management thoroughly
5. Ensure error handling at all layers

---

**Remember:** Three layers, three pointer types, C# is the safety boundary.
