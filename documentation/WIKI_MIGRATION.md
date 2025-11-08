# Wiki Migration Reference

This document maps the original GitHub wiki pages to their new locations in the source repository.

## Migration Date

Documentation was migrated from the GitHub wiki to the source repository on **November 8, 2024**.

## Page Mapping

| Original Wiki Page | New Location | Notes |
|-------------------|--------------|-------|
| [Home](https://github.com/mono/SkiaSharp/wiki/Home) | [documentation/README.md](README.md) | Recreated as comprehensive navigation hub |
| [Views Support Matrix](https://github.com/mono/SkiaSharp/wiki/Views-Support-Matrix) | [user-guide/views-support-matrix.md](user-guide/views-support-matrix.md) | Updated with .NET MAUI and WinUI 3 |
| [SkiaSharp with Python](https://github.com/mono/SkiaSharp/wiki/SkiaSharp-with-Python) | [user-guide/python-integration.md](user-guide/python-integration.md) | Minor formatting updates |
| [SkiaSharp Native Assets for Linux](https://github.com/mono/SkiaSharp/wiki/SkiaSharp-Native-Assets-for-Linux) | [user-guide/linux-native-assets.md](user-guide/linux-native-assets.md) | Updated package information |
| [Building SkiaSharp](https://github.com/mono/SkiaSharp/wiki/Building-SkiaSharp) | [building/building-skiasharp.md](building/building-skiasharp.md) | Updated to .NET 8, VS 2022, modern tooling |
| [Building on Linux](https://github.com/mono/SkiaSharp/wiki/Building-on-Linux) | [building/building-on-linux.md](building/building-on-linux.md) | Enhanced with Docker examples, updated deps |
| [Building on Linux (LEGACY)](https://github.com/mono/SkiaSharp/wiki/Building-on-Linux-(LEGACY)) | [archived/building-on-linux-legacy.md](archived/building-on-linux-legacy.md) | Archived, unchanged |
| [Creating Bindings](https://github.com/mono/SkiaSharp/wiki/Creating-Bindings) | [../design/adding-new-apis.md](../design/adding-new-apis.md) | Content merged into comprehensive design doc |
| [Creating New Libraries](https://github.com/mono/SkiaSharp/wiki/Creating-New-Libraries) | [building/creating-new-libraries.md](building/creating-new-libraries.md) | Unchanged |
| [Writing Docs](https://github.com/mono/SkiaSharp/wiki/Writing-Docs) | [contributing/writing-docs.md](contributing/writing-docs.md) | Unchanged |
| [Branching](https://github.com/mono/SkiaSharp/wiki/Branching) | [maintainer/branching.md](maintainer/branching.md) | Unchanged |
| [Being a Maintainer](https://github.com/mono/SkiaSharp/wiki/Being-a-Maintainer) | [maintainer/being-a-maintainer.md](maintainer/being-a-maintainer.md) | Unchanged |
| [Release Checklist](https://github.com/mono/SkiaSharp/wiki/Release-Checklist) | [maintainer/release-checklist.md](maintainer/release-checklist.md) | Unchanged |
| [Versioning](https://github.com/mono/SkiaSharp/wiki/Versioning) | [maintainer/versioning.md](maintainer/versioning.md) | Unchanged |
| [Submitting Issues](https://github.com/mono/SkiaSharp/wiki/Submitting-Issues) | [contributing/submitting-issues.md](contributing/submitting-issues.md) | Unchanged |
| [Native Changelogs](https://github.com/mono/SkiaSharp/wiki/Native-Changelogs) | [maintainer/native-changelogs.md](maintainer/native-changelogs.md) | Unchanged |
| [WIP Changelog](https://github.com/mono/SkiaSharp/wiki/WIP-Changelog) | *Not migrated* | Content was outdated; active changelog is in releases |
| _Sidebar | [documentation/README.md](README.md) | Navigation integrated into main README |

## Structural Changes

### New Directory Structure

The documentation is now organized into clear categories:

- **`user-guide/`** - Documentation for SkiaSharp users
- **`building/`** - Guides for building from source
- **`contributing/`** - Information for contributors
- **`maintainer/`** - Documentation for project maintainers
- **`archived/`** - Legacy documentation for reference

### Relationship to Existing Documentation

- **`documentation/`** - User-facing guides and how-to's (this migration)
- **`design/`** - Architecture and technical design docs (pre-existing)
- **`docs/`** - API documentation (separate submodule: mono/SkiaSharp-API-docs)
- **`samples/`** - Code examples (pre-existing)

## Content Updates

### Modernization

Several documents were updated to reflect current tools and practices:

1. **Building SkiaSharp** - Updated from VS 2019/.NET Core to VS 2022/.NET 8
2. **Building on Linux** - Added Docker examples, updated dependencies
3. **Views Support Matrix** - Added .NET MAUI and WinUI 3 support

### Improvements

- Added comprehensive navigation in README files
- Fixed and verified all internal cross-references
- Standardized formatting and structure
- Added "I want to..." navigation pattern

## Accessing Old Wiki

The original GitHub wiki remains available at:
- https://github.com/mono/SkiaSharp/wiki

However, it should be considered deprecated in favor of this documentation.

## Why Migrate?

The migration provides several benefits:

1. **Version Control** - Documentation changes are tracked alongside code
2. **Synchronization** - Docs stay current with the code they document
3. **Community Contributions** - Anyone can submit doc improvements via PR
4. **Better Organization** - Clear structure with logical groupings
5. **Discoverability** - Documentation is part of repository navigation
6. **Search** - Documentation is indexed with code search

## Contributing

To update documentation:

1. Edit files in the `documentation/` directory
2. Submit a pull request
3. No build required for documentation-only changes

See [contributing/README.md](contributing/README.md) for details.

## Questions?

If you have questions about the documentation:
- Check the [README](README.md) for navigation
- Open an issue on GitHub
- Review the [Contributing Guide](contributing/README.md)
