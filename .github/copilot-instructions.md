# GitHub Copilot Instructions for SkiaSharp

This file provides context for AI coding assistants working on SkiaSharp.

## Quick Start

**For quick reference:** See **[AGENTS.md](../AGENTS.md)** - 2 minute overview

**For comprehensive docs:** See **[documentation/](../documentation/)** folder

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
- **[documentation/README.md](../documentation/README.md)** - Documentation index

### Architecture & Concepts
- **[documentation/architecture.md](../documentation/architecture.md)** - Three-layer architecture, design principles
- **[documentation/memory-management.md](../documentation/memory-management.md)** - **Critical:** Pointer types, ownership, lifecycle
- **[documentation/error-handling.md](../documentation/error-handling.md)** - Error propagation through layers

### Contributor Guides
- **[documentation/adding-apis.md](../documentation/adding-apis.md)** - Complete step-by-step guide with examples

## Core Principles

### Memory Management
Three pointer types (see [memory-management.md](../documentation/memory-management.md)):
1. **Raw (Non-Owning)** - Parameters, borrowed refs → No cleanup
2. **Owned** - Canvas, Paint → Call delete on dispose
3. **Ref-Counted** - Image, Shader, Data → Call unref on dispose

### Error Handling
- **C API:** Minimal wrapper, trusts C# validation
- **C#:** Validates ALL parameters, checks returns, throws exceptions

### Layer Boundaries
- **C++ → C API:** Direct calls, type conversion
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

1. Find similar existing API and follow its pattern
2. See [documentation/](../documentation/) for comprehensive details
3. Verify pointer type carefully (most important!)
4. Test memory management thoroughly

---

**Remember:** Three layers, three pointer types, C# is the safety boundary.
