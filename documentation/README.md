# SkiaSharp Documentation

Welcome to the SkiaSharp documentation! This directory contains comprehensive guides for using, building, and contributing to SkiaSharp.

> **Note:** This documentation was migrated from the [GitHub wiki](https://github.com/mono/SkiaSharp/wiki) to the source repository for better version control and community contributions.

## 🚀 Quick Start

**New to SkiaSharp?**
- [User Guide](#user-guide) - Learn how to use SkiaSharp in your projects
- [Samples](../samples/) - Working code examples
- [API Documentation](https://docs.microsoft.com/dotnet/api/skiasharp) - Online API reference

**Want to build from source?**
- [Building SkiaSharp](building/building-skiasharp.md) - Complete build guide
- [Building on Linux](building/building-on-linux.md) - Linux-specific instructions

**Want to contribute?**
- [Contributing Guide](contributing/README.md) - How to contribute
- [Adding New APIs](../design/adding-new-apis.md) - Step-by-step binding guide
- [Architecture Overview](../design/architecture-overview.md) - Understanding SkiaSharp's design

## 📚 Documentation Sections

### User Guide

Documentation for SkiaSharp users:

- **[Views Support Matrix](user-guide/views-support-matrix.md)** - UI view support across platforms
- **[Linux Native Assets](user-guide/linux-native-assets.md)** - Using SkiaSharp on Linux
- **[Python Integration](user-guide/python-integration.md)** - Using SkiaSharp with Python

### Building

Guides for building SkiaSharp from source:

- **[Building SkiaSharp](building/building-skiasharp.md)** - General build instructions
- **[Building on Linux](building/building-on-linux.md)** - Linux-specific build guide
- **[Creating New Libraries](building/creating-new-libraries.md)** - Adding new platform libraries

### Contributing

Information for contributors:

- **[Contributing Guide](contributing/README.md)** - How to contribute to SkiaSharp
- **[Submitting Issues](contributing/submitting-issues.md)** - Reporting bugs effectively
- **[Writing Documentation](contributing/writing-docs.md)** - Contributing to docs

### Maintainer

Documentation for project maintainers:

- **[Being a Maintainer](maintainer/being-a-maintainer.md)** - Maintainer responsibilities
- **[Release Checklist](maintainer/release-checklist.md)** - Pre-release validation
- **[Versioning](maintainer/versioning.md)** - Version numbering scheme
- **[Branching Strategy](maintainer/branching.md)** - Git branching workflow
- **[Native Changelogs](maintainer/native-changelogs.md)** - Links to Skia release notes

### Design Documentation

Architecture and design documents (located in [`../design/`](../design/)):

- **[Quick Start](../design/QUICKSTART.md)** ⭐ - 10-minute practical tutorial
- **[Architecture Overview](../design/architecture-overview.md)** - Three-layer architecture
- **[Memory Management](../design/memory-management.md)** - Pointer types and ownership
- **[Error Handling](../design/error-handling.md)** - Error propagation patterns
- **[Adding New APIs](../design/adding-new-apis.md)** - Complete binding guide
- **[Layer Mapping](../design/layer-mapping.md)** - Type and naming conventions

## 🔍 Find What You Need

### I want to...

**Use SkiaSharp in my app:**
- ✅ Install from NuGet: `dotnet add package SkiaSharp`
- ✅ Check [Views Support Matrix](user-guide/views-support-matrix.md) for your platform
- ✅ Browse [Samples](../samples/) for code examples
- ✅ Read the [API Documentation](https://docs.microsoft.com/dotnet/api/skiasharp)

**Build SkiaSharp from source:**
- ✅ Follow [Building SkiaSharp](building/building-skiasharp.md)
- ✅ For Linux: [Building on Linux](building/building-on-linux.md)
- ✅ Need help? Check [Contributing Guide](contributing/README.md)

**Add a new Skia API to SkiaSharp:**
- ✅ Start with [Adding New APIs](../design/adding-new-apis.md)
- ✅ Understand [Architecture](../design/architecture-overview.md)
- ✅ Learn [Memory Management](../design/memory-management.md)
- ✅ Follow [Contributing Guide](contributing/README.md)

**Fix a bug or contribute:**
- ✅ Read [Contributing Guide](contributing/README.md)
- ✅ Check [Submitting Issues](contributing/submitting-issues.md)
- ✅ Review [Architecture Overview](../design/architecture-overview.md)

**Understand how SkiaSharp works internally:**
- ✅ Read [Architecture Overview](../design/architecture-overview.md)
- ✅ Study [Memory Management](../design/memory-management.md)
- ✅ Check [Error Handling](../design/error-handling.md)
- ✅ Review [Layer Mapping](../design/layer-mapping.md)

**Use SkiaSharp on Linux:**
- ✅ See [Linux Native Assets](user-guide/linux-native-assets.md)
- ✅ Build custom: [Building on Linux](building/building-on-linux.md)

**Use SkiaSharp with Python:**
- ✅ Follow [Python Integration](user-guide/python-integration.md)

## 📖 API Documentation

The XML-based API documentation is generated from code comments and published online:

- [SkiaSharp API](https://docs.microsoft.com/dotnet/api/skiasharp)
- [HarfBuzzSharp API](https://docs.microsoft.com/dotnet/api/harfbuzzsharp)
- [Skottie API](https://docs.microsoft.com/dotnet/api/skiasharp.skottie)

The API documentation source is maintained in a separate repository: [mono/SkiaSharp-API-docs](https://github.com/mono/SkiaSharp-API-docs)

See [Writing Documentation](contributing/writing-docs.md) for how to contribute to API docs.

## 🔗 External Resources

- **SkiaSharp Repository:** https://github.com/mono/SkiaSharp
- **Skia Website:** https://skia.org/
- **Skia C++ API Reference:** https://api.skia.org/
- **NuGet Packages:** https://www.nuget.org/packages/SkiaSharp

## 📝 Documentation Conventions

- **User-facing docs** → `documentation/` (this directory)
- **Architecture/design docs** → `design/` (for contributors)
- **API docs** → Separate repository: [mono/SkiaSharp-API-docs](https://github.com/mono/SkiaSharp-API-docs)
- **Examples** → `samples/` directory

## 🤝 Contributing to Documentation

We welcome documentation improvements! See:
- [Writing Documentation](contributing/writing-docs.md)
- [Contributing Guide](contributing/README.md)

To contribute:
1. Fork the repository
2. Make your changes to files in `documentation/`
3. Submit a pull request
4. Documentation changes don't require building the project

## ℹ️ About This Documentation

This documentation was migrated from the GitHub wiki to the source repository in November 2024. This provides several benefits:

- **Version Control** - Documentation changes are tracked alongside code changes
- **Synchronization** - Docs stay in sync with the code they document
- **Community Contributions** - Anyone can submit documentation improvements via pull requests
- **Better Organization** - Clear structure with logical groupings
- **Discoverability** - Documentation is part of the repository and appears in code navigation

The original wiki remains available at: https://github.com/mono/SkiaSharp/wiki

## 📋 Archived Documentation

Older documentation that may still be useful is preserved in [archived/](archived/):

- [Building on Linux (Legacy)](archived/building-on-linux-legacy.md) - Pre-v1.60.1 Linux build instructions

## License

This documentation is licensed under the [Creative Commons Attribution 4.0 International License](https://creativecommons.org/licenses/by/4.0/).

SkiaSharp itself is licensed under the MIT License. See [LICENSE.txt](../LICENSE.txt) for details.
