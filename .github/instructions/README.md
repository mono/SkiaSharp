# Path-Specific Instructions for SkiaSharp

This directory contains path-specific instruction files that provide targeted guidance for AI coding agents working on different parts of the SkiaSharp codebase.

## Overview

Path-specific instructions automatically apply based on the files being edited, ensuring AI assistants use appropriate patterns, rules, and best practices for each layer or component.

## Instruction Files

| File | Applies To | Key Focus |
|------|-----------|-----------|
| **[c-api-layer.instructions.md](c-api-layer.instructions.md)** | `externals/skia/include/c/`, `externals/skia/src/c/` | C API bridging layer - no exceptions, C types, error codes |
| **[csharp-bindings.instructions.md](csharp-bindings.instructions.md)** | `binding/SkiaSharp/` | C# wrappers - IDisposable, P/Invoke, validation, exceptions |
| **[generated-code.instructions.md](generated-code.instructions.md)** | `*.generated.cs` files | Generated code - don't edit manually, modify templates |
| **[native-skia.instructions.md](native-skia.instructions.md)** | `externals/skia/` (excluding C API) | Upstream Skia C++ - understanding only, pointer types |
| **[tests.instructions.md](tests.instructions.md)** | `tests/`, `*Tests.cs` | Test code - memory management, error cases, lifecycle |
| **[documentation.instructions.md](documentation.instructions.md)** | `design/`, `*.md` | Documentation - clear examples, architecture focus |
| **[samples.instructions.md](samples.instructions.md)** | `samples/` | Sample code - best practices, complete examples |

## How It Works

AI coding agents that support path-specific instructions (like GitHub Copilot, Cursor, etc.) will automatically load and apply the relevant instruction file based on the file paths you're working with.

For example:
- Editing `externals/skia/src/c/sk_canvas.cpp` → Loads **c-api-layer.instructions.md**
- Editing `binding/SkiaSharp/SKCanvas.cs` → Loads **csharp-bindings.instructions.md**
- Editing `tests/SKCanvasTests.cs` → Loads **tests.instructions.md**

## Key Benefits

### 1. Layer-Specific Guidance
Each layer has unique requirements:
- **C API:** Never throw exceptions, use C types, handle errors with return codes
- **C# Bindings:** Always dispose, validate parameters, convert to C# exceptions
- **Tests:** Focus on memory management, error cases, lifecycle

### 2. Automatic Context
AI assistants automatically understand:
- Which patterns to follow
- What mistakes to avoid
- How to handle special cases

### 3. Consistency
Ensures all AI-generated code follows the same patterns across the codebase.

## Critical Concepts Covered

### Memory Management (All Layers)
- **Raw pointers** (non-owning) - No cleanup needed
- **Owned pointers** - One owner, explicit delete/dispose
- **Reference-counted** - Shared ownership, ref/unref

### Error Handling (Per Layer)
- **C API:** Minimal wrapper, no exception handling, no validation, trusts C#
- **C#:** Validate parameters, check returns, throw typed exceptions
- **Tests:** Verify proper exception handling

### Best Practices
- Proper disposal in C# (`using` statements)
- Complete, self-contained examples in samples
- Memory leak testing in test code
- Clear documentation with examples

## Usage Examples

### For AI Assistants

When working on different files:

```
# Editing C API layer
externals/skia/src/c/sk_canvas.cpp
→ Applies: Never throw exceptions, use SK_C_API, handle errors

# Editing C# wrapper
binding/SkiaSharp/SKCanvas.cs
→ Applies: Validate parameters, use IDisposable, throw exceptions

# Writing tests
tests/SKCanvasTests.cs
→ Applies: Use using statements, test disposal, verify no leaks
```

### For Contributors

These files serve as quick reference guides for:
- Understanding layer-specific requirements
- Following established patterns
- Avoiding common mistakes

## Maintaining Instructions

### When to Update

Update instruction files when:
- Patterns or best practices change
- New common mistakes are discovered
- Layer responsibilities change
- New tooling or generators are added

### What to Include

Each instruction file should cover:
- ✅ Critical rules and requirements
- ✅ Common patterns with code examples
- ✅ What NOT to do (anti-patterns)
- ✅ Error handling specifics
- ✅ Memory management patterns

### What to Avoid

Don't include in instruction files:
- ❌ Exhaustive API documentation
- ❌ Build/setup instructions (use main docs)
- ❌ Temporary workarounds
- ❌ Implementation details

## Related Documentation

For comprehensive guidance, see:
- **[AGENTS.md](../../AGENTS.md)** - High-level project overview for AI agents
- **[design/](../../design/)** - Detailed architecture documentation
- **[.github/copilot-instructions.md](../copilot-instructions.md)** - General AI assistant context

## Integration with AI Tools

These instructions integrate with:
- **GitHub Copilot** - Workspace instructions
- **Cursor** - .cursorrules and workspace context
- **Other AI assistants** - Supporting path-specific patterns

## Summary

Path-specific instructions ensure AI coding agents apply the right patterns in the right places, maintaining code quality and consistency across SkiaSharp's three-layer architecture.

**Key Principle:** Different layers require different approaches - these instructions ensure AI assistants understand and apply the correct patterns for each context.
