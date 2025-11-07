# GitHub Copilot Instructions for SkiaSharp

This file provides context for AI coding assistants working on SkiaSharp.

## Quick Start

**For quick reference:** See **[AGENTS.md](../AGENTS.md)** - 2 minute overview

**For practical guide:** See **[design/QUICKSTART.md](../design/QUICKSTART.md)** - 10 minute tutorial

**For comprehensive docs:** See **[design/](../design/)** folder

## Path-Specific Instructions

AI assistants automatically load context based on file paths from `.github/instructions/`:

- **C API Layer** (`externals/skia/src/c/`) → [c-api-layer.instructions.md](instructions/c-api-layer.instructions.md)
- **C# Bindings** (`binding/SkiaSharp/`) → [csharp-bindings.instructions.md](instructions/csharp-bindings.instructions.md)
- **Generated Code** (`*.generated.cs`) → [generated-code.instructions.md](instructions/generated-code.instructions.md)
- **Native Skia** (`externals/skia/`) → [native-skia.instructions.md](instructions/native-skia.instructions.md)
- **Tests** (`tests/`) → [tests.instructions.md](instructions/tests.instructions.md)
- **Samples** (`samples/`) → [samples.instructions.md](instructions/samples.instructions.md)
- **Documentation** (`*.md`) → [documentation.instructions.md](instructions/documentation.instructions.md)

See [instructions/README.md](instructions/README.md) for details.

## Documentation Index

### Essential Reading
- **[AGENTS.md](../AGENTS.md)** - Quick reference (AI agents, quick lookup)
- **[design/QUICKSTART.md](../design/QUICKSTART.md)** - Practical tutorial (new contributors)
- **[design/README.md](../design/README.md)** - Documentation index

### Architecture & Concepts
- **[design/architecture-overview.md](../design/architecture-overview.md)** - Three-layer architecture, design principles
- **[design/memory-management.md](../design/memory-management.md)** - **Critical:** Pointer types, ownership, lifecycle
- **[design/error-handling.md](../design/error-handling.md)** - Error propagation through layers

### Contributor Guides
- **[design/adding-new-apis.md](../design/adding-new-apis.md)** - Complete step-by-step guide with examples
- **[design/layer-mapping.md](../design/layer-mapping.md)** - Type mappings and naming conventions

## Core Principles

### Memory Management
Three pointer types (see [memory-management.md](../design/memory-management.md)):
1. **Raw (Non-Owning)** - Parameters, borrowed refs → No cleanup
2. **Owned** - Canvas, Paint → Call delete on dispose
3. **Ref-Counted** - Image, Shader, Data → Call unref on dispose

### Error Handling
- **C API:** Never throw exceptions, return bool/null
- **C#:** Validate parameters, throw exceptions

### Layer Boundaries
- **C++ → C API:** Exception firewall, type conversion
- **C API → C#:** P/Invoke, parameter validation

## Build & Test

```bash
# Build managed code only
dotnet cake --target=libs

# Run tests
dotnet cake --target=tests

# Download pre-built native libraries
dotnet cake --target=externals-download
```

## When In Doubt

1. Check [QUICKSTART.md](../design/QUICKSTART.md) for common patterns
2. Find similar existing API and follow its pattern
3. See [design/](../design/) for comprehensive details
4. Verify pointer type carefully (most important!)
5. Test memory management thoroughly

---

**Remember:** Three layers, three pointer types, exception firewall at C API.
