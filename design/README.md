# SkiaSharp Design Documentation

This folder contains comprehensive documentation for understanding and contributing to SkiaSharp.

## 🚀 Start Here

### For Quick Answers
- **[../AGENTS.md](../AGENTS.md)** - 2-minute quick reference (AI agents, quick lookup)

### For Getting Started  
- **[QUICKSTART.md](QUICKSTART.md)** - **⭐ Start here!** 10-minute practical tutorial
  - How to add an API end-to-end
  - Pointer type identification flowchart
  - Common mistakes and how to avoid them

### For Comprehensive Reference
Continue reading below for the complete documentation index.

---

## Documentation Index

### Getting Started

0. **[QUICKSTART.md](QUICKSTART.md)** - **⭐ Practical tutorial (start here!)**
   - Complete API addition walkthrough
   - Pointer type decision flowchart
   - Error handling patterns
   - Top 10 common mistakes
   - Quick examples for immediate productivity

### Core Architecture Documents

1. **[architecture-overview.md](architecture-overview.md)** - Three-layer architecture
   - Three-layer architecture (C++ → C API → C#)
   - How components connect
   - Call flow examples
   - File organization
   - Code generation overview
   - Key design principles

2. **[memory-management.md](memory-management.md)** - Critical for correct bindings
   - Three pointer type categories (raw, owned, reference-counted)
   - How each type maps through layers
   - Ownership semantics and lifecycle patterns
   - How to identify pointer types from C++ signatures
   - Common mistakes and how to avoid them
   - Thread safety considerations

3. **[error-handling.md](error-handling.md)** - Understanding error flow
   - Why C++ exceptions can't cross C API boundary
   - Error handling strategy by layer
   - Validation patterns in C#
   - Exception firewall in C API
   - Complete error flow examples
   - Best practices and debugging tips

4. **[adding-new-apis.md](adding-new-apis.md)** - Step-by-step contributor guide
   - How to analyze C++ APIs
   - Adding C API wrapper functions
   - Creating P/Invoke declarations
   - Writing C# wrapper code
   - Testing your changes
   - Complete examples and patterns
   - Troubleshooting guide

5. **[layer-mapping.md](layer-mapping.md)** - Quick reference
   - Type naming conventions across layers
   - Function naming patterns
   - File organization mapping
   - Type conversion macros
   - Common API patterns
   - Parameter passing patterns

## Quick Start Guide

### For AI Assistants (GitHub Copilot)

See [../.github/copilot-instructions.md](../.github/copilot-instructions.md) for:
- Condensed context optimized for AI
- Quick decision trees
- Common patterns and anti-patterns
- Checklist for changes

### For Human Contributors

**First time working with SkiaSharp?**

1. Read [architecture-overview.md](architecture-overview.md) to understand the three-layer structure
2. Study [memory-management.md](memory-management.md) to understand pointer types (critical!)
3. Review [error-handling.md](error-handling.md) to understand error propagation
4. When ready to add APIs, follow [adding-new-apis.md](adding-new-apis.md)
5. Keep [layer-mapping.md](layer-mapping.md) open as a reference

**Want to understand existing code?**

Use the documentation to trace through layers:
1. Start with C# API in `binding/SkiaSharp/SK*.cs`
2. Find P/Invoke in `SkiaApi.cs` or `SkiaApi.generated.cs`
3. Locate C API in `externals/skia/include/c/sk_*.h`
4. Check implementation in `externals/skia/src/c/sk_*.cpp`
5. Find C++ API in `externals/skia/include/core/Sk*.h`

## Key Concepts Summary

### The Three-Layer Architecture

```
┌─────────────────────────────────────┐
│   C# Wrapper Layer                  │  ← Managed .NET code
│   (binding/SkiaSharp/*.cs)          │     - Type safety
│   - SKCanvas, SKPaint, SKImage      │     - Memory management
└──────────────┬──────────────────────┘     - Validation
               │ P/Invoke
┌──────────────▼──────────────────────┐
│   C API Layer                       │  ← Exception boundary
│   (externals/skia/include/c/*.h)    │     - Never throws
│   - sk_canvas_*, sk_paint_*         │     - Error codes
└──────────────┬──────────────────────┘     - Type conversion
               │ Casting
┌──────────────▼──────────────────────┐
│   C++ Skia Library                  │  ← Native graphics engine
│   (externals/skia/include/core/*.h) │     - Original Skia API
│   - SkCanvas, SkPaint, SkImage      │     - Implementation
└─────────────────────────────────────┘
```

### Three Pointer Type Categories

Understanding pointer types is **critical** for working with SkiaSharp:

| Type | C++ | C API | C# | Cleanup | Examples |
|------|-----|-------|-----|---------|----------|
| **Raw (Non-Owning)** | `SkType*` param | `sk_type_t*` | `OwnsHandle=false` | None | Parameters, getters |
| **Owned** | `new SkType()` | `sk_type_new/delete` | `SKObject` | `delete` | SKCanvas, SKPaint |
| **Reference-Counted** | `sk_sp<SkType>` | `sk_type_ref/unref` | `ISKReferenceCounted` | `unref()` | SKImage, SKShader |

**→ See [memory-management.md](memory-management.md) for detailed explanation**

### Error Handling Across Layers

- **C++ Layer:** Can throw exceptions, use normal C++ error handling
- **C API Layer:** **Never throws** - catches all exceptions, returns error codes (bool/null)
- **C# Layer:** Validates parameters, checks return values, throws C# exceptions

**→ See [error-handling.md](error-handling.md) for patterns and examples**

## Use Cases Supported

### Use Case 1: Understanding Existing Code

> "I'm looking at `SKCanvas.DrawRect()` in C# - trace this call through all layers to understand how it reaches native Skia code and what memory management is happening."

**Solution:** Follow the call flow in [architecture-overview.md](architecture-overview.md), check pointer types in [memory-management.md](memory-management.md)

### Use Case 2: Understanding Pointer Types

> "I see `sk_canvas_t*` in the C API and `SKCanvas` in C#. What pointer type does SKCanvas use in native Skia? How does this affect the C# wrapper's dispose pattern?"

**Solution:** Check [memory-management.md](memory-management.md) section on "Owned Pointers" and "Identifying Pointer Types"

### Use Case 3: Adding a New API

> "Skia added a new `SkCanvas::drawArc()` method. What files do I need to modify in each layer, and how should I handle memory management?"

**Solution:** Follow step-by-step guide in [adding-new-apis.md](adding-new-apis.md)

### Use Case 4: Debugging Memory Issues

> "There's a memory leak involving SKBitmap objects. How do I understand the lifecycle and pointer type to find where disposal or reference counting might be wrong?"

**Solution:** Check [memory-management.md](memory-management.md) for lifecycle patterns and common mistakes

### Use Case 5: Understanding Error Flow

> "A native Skia operation failed but my C# code didn't catch any exception. How do errors flow through the layers, and where might error handling be missing?"

**Solution:** Review [error-handling.md](error-handling.md) for error propagation patterns and debugging

### Use Case 6: Working with Reference Counting

> "SKImage seems to use reference counting. How does this work across the C API boundary? When do I need to call ref/unref functions?"

**Solution:** See [memory-management.md](memory-management.md) section on "Reference-Counted Pointers" with examples

## Documentation Maintenance

### When to Update

Update this documentation when:
- Adding new patterns or architectural changes
- Discovering common mistakes or gotchas
- Significant changes to memory management strategy
- Adding new pointer type categories
- Changing error handling approach

### What NOT to Document Here

Don't duplicate information that's better elsewhere:
- **API documentation** - Use XML comments in code, generated to `docs/`
- **Build instructions** - Keep in Wiki or root README
- **Version history** - Keep in CHANGELOG or release notes
- **Platform specifics** - Keep in platform-specific docs

### Keep It Maintainable

- Focus on architecture and patterns, not specific APIs
- Use examples that are unlikely to change
- Reference stable parts of the codebase
- Update when patterns change, not when APIs are added

## Contributing to Documentation

Improvements welcome! When contributing:

1. **Keep it high-level** - Focus on concepts, not exhaustive API lists
2. **Add examples** - Show complete patterns through all three layers
3. **Optimize for searchability** - Use clear headings and keywords
4. **Test understanding** - Can someone follow your examples?
5. **Update cross-references** - Keep links between documents current

## Additional Resources

### External Documentation

- **Skia Website:** https://skia.org/
- **Skia C++ API Reference:** https://api.skia.org/
- **SkiaSharp Wiki:** https://github.com/mono/SkiaSharp/wiki
- **SkiaSharp Samples:** https://github.com/mono/SkiaSharp/tree/main/samples

### Related Documentation

- **Root README.md** - Project overview and getting started
- **Wiki: Creating Bindings** - Original binding guide (less detailed)
- **Wiki: Building SkiaSharp** - Build instructions
- **Source XML Comments** - API-level documentation

### Questions or Issues?

- **Architecture questions:** Review this documentation first
- **Build issues:** Check Wiki or root README
- **API usage:** Check generated API docs in `docs/`
- **Bugs:** File an issue on GitHub

## Version History

- **2024-11-07:** Initial architecture documentation created
  - Comprehensive coverage of three-layer architecture
  - Detailed memory management with pointer types
  - Complete error handling patterns
  - Step-by-step API addition guide
  - Layer mapping reference
  - GitHub Copilot instructions

---

**Remember:** The three most important concepts are:
1. **Three-layer architecture** (C++ → C API → C#)
2. **Three pointer types** (raw, owned, reference-counted)
3. **Exception firewall** (C API never throws)

Master these, and you'll understand SkiaSharp's design.
