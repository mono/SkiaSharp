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

👉 **Full details:** [design/memory-management.md](design/memory-management.md)

### Error Handling

- **C# validates all parameters** before calling C API
- **C API is minimal wrapper** - no validation, trusts C#
- **Factory methods return null** on failure (do NOT throw)
- **Constructors throw** on failure

👉 **Full details:** [design/error-handling.md](design/error-handling.md)

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
- `design/` - Architecture documentation (comprehensive guides)

## Adding New APIs - Quick Steps

1. Find C++ API in Skia
2. Identify pointer type (raw/owned/ref-counted)
3. Add C API wrapper (minimal, no validation)
4. Add C API header
5. Add P/Invoke declaration
6. Add C# wrapper with validation

👉 **Full step-by-step guide:** [design/adding-new-apis.md](design/adding-new-apis.md)

## Common Pitfalls

❌ Wrong pointer type → memory leaks/crashes  
❌ Missing ref count increment when C++ expects `sk_sp<T>`  
❌ Disposing borrowed objects  
❌ Not checking factory method null returns  
❌ Missing parameter validation in C#  

👉 **Full list with solutions:** [design/memory-management.md#common-pitfalls](design/memory-management.md#common-pitfalls) and [design/error-handling.md#common-mistakes](design/error-handling.md#common-mistakes)

## Code Generation

- **Hand-written:** C API layer (all `.cpp` in `externals/skia/src/c/`)
- **Generated:** Some P/Invoke declarations (`SkiaApi.generated.cs`)
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

👉 **Full threading guide:** [design/architecture-overview.md#threading-model-and-concurrency](design/architecture-overview.md#threading-model-and-concurrency)

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

**Practical tutorial:** [design/QUICKSTART.md](design/QUICKSTART.md) - 10-minute walkthrough

**Contributing:** [CONTRIBUTING.md](CONTRIBUTING.md) - How to submit issues and PRs

**Detailed guides** in `design/` folder:
- `QUICKSTART.md` - **Start here!** Practical end-to-end tutorial
- `architecture-overview.md` - Three-layer architecture, design principles
- `memory-management.md` - **Critical**: Pointer types, ownership, lifecycle
- `error-handling.md` - Error propagation patterns through layers
- `adding-new-apis.md` - Complete step-by-step guide with examples
- `layer-mapping.md` - Type mappings and naming conventions

**Build & maintainer guides** in `wiki/` folder:
- `Building-SkiaSharp.md` - Build instructions for Windows & macOS
- `Building-on-Linux.md` - Build instructions for Linux
- `Being-a-Maintainer.md` - Maintainer responsibilities
- `Release-Checklist.md` - Release verification process
- See [wiki/README.md](wiki/README.md) for complete index

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

👉 **See also:** [design/adding-new-apis.md#decision-flowcharts](design/adding-new-apis.md#decision-flowcharts)

## Examples

See [design/adding-new-apis.md](design/adding-new-apis.md) for complete examples with all three layers.

## When In Doubt

1. Find similar existing API and follow its pattern
2. Check `design/` documentation for detailed guidance
3. Verify pointer type carefully (most important!)
4. Test memory management thoroughly
5. Ensure error handling at all layers

---

**Remember:** Three layers, three pointer types, C# is the safety boundary.
