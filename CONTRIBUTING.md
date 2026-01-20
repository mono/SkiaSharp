# Contributing to SkiaSharp

Thank you for your interest in contributing to SkiaSharp! This document provides guidelines for submitting issues and pull requests.

## 📖 Documentation

Before contributing, please review:
- **[AGENTS.md](AGENTS.md)** - Quick reference for AI agents
- **[design/QUICKSTART.md](design/QUICKSTART.md)** - 10-minute practical tutorial
- **[design/adding-new-apis.md](design/adding-new-apis.md)** - Complete guide for adding APIs
- **[wiki/](wiki/)** - Build instructions, maintainer guides, and additional documentation

## 🐛 Reporting Issues

If you think you've found a bug, please check the [Issues](https://github.com/mono/SkiaSharp/issues) page first to see if an issue has already been filed. This helps reduce duplicate reports.

### Before Submitting

1. **Validate** that the issue is not resolved with the latest stable, pre-release, or nightly builds
2. **Create a minimal reproduction** project using *only* the code necessary to reproduce the issue
3. **Clean your solution** before compressing:
   - Delete all `bin/` and `obj/` folders
   - Remove the `packages/` folder
   - Example structure to clean:
     ```
     [ProjectRoot]/ProjectName/bin
     [ProjectRoot]/ProjectName/obj
     [ProjectRoot]/ProjectName.Android/bin
     [ProjectRoot]/ProjectName.Android/obj
     [ProjectRoot]/ProjectName.iOS/bin
     [ProjectRoot]/ProjectName.iOS/obj
     [ProjectRoot]/ProjectName.UWP/bin
     [ProjectRoot]/ProjectName.UWP/obj
     ```

### When Submitting Issues

- **Platform information:** Note if the issue occurs only on specific platforms
- **Regressions:** If applicable, identify the last working build version
- **Device specifics:** Some issues occur only on specific:
  - Device types (phones, tablets, desktops)
  - OS versions
  - Target SDK versions
  - Physical devices vs simulators/emulators
- **Upload reproductions:** Attach reproduction projects directly to the issue (don't use external file sharing)

### Platform-Specific Information

#### Android
- Note issues that may stem from different versions of support library packages (e.g., `Xamarin.Android.Support.v7.AppCompat`)
- Sometimes bugs occur only with specific package versions

#### UWP
- Note if an issue appears hardware-specific (slower machines, multiple monitors, etc.)

## 🔧 Contributing Code

We greatly welcome PRs with fixes and improvements from the community!

### Pull Request Guidelines

1. **Follow existing patterns** - Study similar APIs before adding new ones
2. **Add tests** - All new features and bug fixes should include tests
3. **Update documentation** - If you're changing public APIs
4. **Memory management** - Ensure proper disposal and pointer type handling (see [design/memory-management.md](design/memory-management.md))
5. **Error handling** - Validate parameters and handle errors appropriately (see [design/error-handling.md](design/error-handling.md))

### Adding New APIs

Follow the comprehensive guide in [design/adding-new-apis.md](design/adding-new-apis.md), which covers:
- Analyzing C++ APIs and identifying pointer types
- Creating C API wrappers
- Writing P/Invoke declarations
- Implementing C# wrapper classes
- Testing your changes

### Code Standards

- **Dispose pattern:** Always use `using` statements in samples and tests
- **Parameter validation:** Validate all parameters in C# before P/Invoke
- **Error checking:** Check return values from C API calls
- **Memory safety:** Understand pointer types (see [design/memory-management.md](design/memory-management.md))
- **Naming conventions:** Follow existing naming patterns (see [design/layer-mapping.md](design/layer-mapping.md))

## 🏗️ Building SkiaSharp

### Quick Build (Managed Only)

If you're only making changes to managed code:

```bash
# Download pre-built native libraries
dotnet cake --target=externals-download

# Build managed libraries
dotnet cake --target=libs

# Run tests
dotnet cake --target=tests
```

See [wiki/Building-SkiaSharp.md](wiki/Building-SkiaSharp.md) for complete build instructions.

### Full Build (Including Native)

For native code changes, see:
- [wiki/Building-SkiaSharp.md](wiki/Building-SkiaSharp.md) - Windows & macOS
- [wiki/Building-on-Linux.md](wiki/Building-on-Linux.md) - Linux

## 📚 Additional Resources

- **[wiki/](wiki/)** - Complete collection of contributor documentation
- **[design/](design/)** - Architecture and design documentation
- **Maintainer guides** - See [wiki/Being-a-Maintainer.md](wiki/Being-a-Maintainer.md)
- **Release process** - See [wiki/Release-Checklist.md](wiki/Release-Checklist.md)

## 💡 Questions?

- **Architecture questions:** Review [design/](design/) documentation
- **Build issues:** Check [wiki/](wiki/) folder
- **API usage:** Check the [API documentation](https://docs.microsoft.com/dotnet/api/skiasharp)
- **Still stuck?** Open an issue with your question

## 🙏 Thank You!

Your contributions make SkiaSharp better for everyone. We appreciate your time and effort!
