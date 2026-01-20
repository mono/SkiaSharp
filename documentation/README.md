# SkiaSharp Documentation

SkiaSharp is a cross-platform 2D graphics API for .NET that wraps Google's Skia Graphics Library.

## Architecture

```
C# Wrapper (binding/SkiaSharp/)  →  P/Invoke  →  C API (externals/skia/src/c/)  →  C++ Skia
```

**Key principles:**
- C# is the safety boundary (validates parameters, throws exceptions)
- C API is minimal pass-through (trusts C#, returns bool/null on failure)
- Three pointer types: raw (borrowed), owned (delete), ref-counted (unref)

## Documentation Index

### Architecture & Concepts
| Document | Description |
|----------|-------------|
| [architecture.md](architecture.md) | Three layers, type mappings, call flow, threading |
| [memory-management.md](memory-management.md) | Pointer types, ownership, lifecycle |
| [error-handling.md](error-handling.md) | Error patterns across layers |

### Contributing
| Document | Description |
|----------|-------------|
| [adding-apis.md](adding-apis.md) | Step-by-step guide to add bindings |
| [adding-libraries.md](adding-libraries.md) | Adding new projects and NuGet packages |
| [writing-docs.md](writing-docs.md) | API documentation process |
| [maintaining.md](maintaining.md) | Maintainer responsibilities |

### Building
| Document | Description |
|----------|-------------|
| [building.md](building.md) | Build on Windows & macOS |
| [building-linux.md](building-linux.md) | Build native libraries for Linux |

### Releasing
| Document | Description |
|----------|-------------|
| [releasing.md](releasing.md) | Release verification steps |
| [branching.md](branching.md) | Git branching strategy |
| [versioning.md](versioning.md) | Version numbering scheme |

### Reference
| Document | Description |
|----------|-------------|
| [linux-assets.md](linux-assets.md) | Linux native package information |

## Quick Build

```bash
dotnet cake --target=externals-download  # Get native libs
dotnet cake --target=libs                # Build managed
dotnet cake --target=tests               # Run tests
```

## External Resources

- [Skia Documentation](https://skia.org/)
- [Skia API Reference](https://api.skia.org/)
- [SkiaSharp on Microsoft Learn](https://learn.microsoft.com/dotnet/api/skiasharp)
- [SkiaSharp Samples](https://github.com/mono/SkiaSharp/tree/main/samples)
