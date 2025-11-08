# Contributing to SkiaSharp

Thank you for your interest in contributing to SkiaSharp! This guide will help you get started.

## Quick Links

- [Submitting Issues](submitting-issues.md) - Report bugs or request features
- [Writing Documentation](writing-docs.md) - Contribute to documentation
- [Adding New APIs](../../design/adding-new-apis.md) - Add bindings for new Skia APIs
- [Building SkiaSharp](../building/building-skiasharp.md) - Build from source

## Ways to Contribute

### Reporting Issues

If you've found a bug or have a feature request:
1. Check [existing issues](https://github.com/mono/SkiaSharp/issues) first
2. Read our [issue submission guide](submitting-issues.md)
3. Create a detailed issue with reproduction steps

### Contributing Code

1. **Fork the repository** on GitHub
2. **Create a branch** for your changes
3. **Make your changes** following our coding standards
4. **Test your changes** thoroughly
5. **Submit a pull request** with a clear description

### Writing Documentation

We always need help improving our documentation:
- API documentation (XML comments in code)
- Conceptual documentation (this folder and `design/`)
- Tutorials and examples
- See [Writing Documentation](writing-docs.md) for details

### Helping Others

- Answer questions on [GitHub Discussions](https://github.com/mono/SkiaSharp/discussions)
- Help triage issues
- Review pull requests
- Share your SkiaSharp projects

## Development Workflow

### 1. Set Up Your Environment

Follow the [Building SkiaSharp](../building/building-skiasharp.md) guide to:
- Install required dependencies
- Clone the repository
- Build the project

### 2. Find Something to Work On

Good places to start:
- Issues labeled [`good first issue`](https://github.com/mono/SkiaSharp/labels/good%20first%20issue)
- Issues labeled [`help wanted`](https://github.com/mono/SkiaSharp/labels/help%20wanted)
- Documentation improvements
- Adding missing bindings for Skia APIs

### 3. Make Your Changes

**For managed code changes:**
1. Download pre-built native libraries:
   ```bash
   dotnet cake --target=externals-download
   ```
2. Make your changes in `binding/SkiaSharp/` or `source/`
3. Build and test:
   ```bash
   dotnet cake --target=tests --skipExternals=all
   ```

**For native code changes:**
1. Install native build dependencies
2. Make changes in `externals/skia/src/c/` (C API layer)
3. Build native libraries:
   ```bash
   dotnet cake --target=externals-<platform>
   ```
4. Build managed code and test

**For adding new APIs:**
Follow the comprehensive [Adding New APIs](../../design/adding-new-apis.md) guide.

### 4. Test Your Changes

Always test your changes:
- Run unit tests: `dotnet cake --target=tests`
- Test on multiple platforms if possible
- Ensure no memory leaks (especially for new bindings)
- Verify error handling works correctly

### 5. Submit a Pull Request

1. **Commit your changes** with clear, descriptive messages
2. **Push to your fork**
3. **Open a pull request** against the `main` branch
4. **Fill out the PR template** completely
5. **Respond to feedback** from reviewers

## Coding Standards

### C# Code

- Follow standard C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Validate all parameters before calling native code
- Handle errors appropriately (see [Error Handling](../../design/error-handling.md))

### C API Code

- Use C linkage: `SK_C_API` or `extern "C"`
- Keep functions simple - minimal logic
- Trust C# layer to validate parameters
- Follow naming: `sk_<type>_<action>`
- Document ownership and memory management

### Memory Management

This is critical! See [Memory Management](../../design/memory-management.md) for details:
- Identify pointer type correctly (raw, owned, reference-counted)
- Implement disposal correctly in C#
- Never expose `IntPtr` in public APIs
- Use `using` statements in all examples

## Architecture Understanding

Before contributing code, understand SkiaSharp's architecture:

1. **Read** [Architecture Overview](../../design/architecture-overview.md)
2. **Understand** [Memory Management](../../design/memory-management.md)
3. **Review** [Error Handling](../../design/error-handling.md)
4. **Study** existing similar APIs in the codebase

## Code Review Process

All contributions go through code review:

1. **Automated checks** must pass (CI builds, tests)
2. **Maintainer review** - usually within a few days
3. **Address feedback** - make requested changes
4. **Approval and merge** - once approved, a maintainer will merge

## Community Guidelines

- Be respectful and constructive
- Follow the [Code of Conduct](../../CODE-OF-CONDUCT.md)
- Ask questions if you're unsure
- Help others when you can

## Getting Help

If you need assistance:
- **Questions:** [GitHub Discussions](https://github.com/mono/SkiaSharp/discussions)
- **Issues:** [GitHub Issues](https://github.com/mono/SkiaSharp/issues)
- **Documentation:** Check `design/` and `docs/` folders first

## Maintainer Resources

If you're a maintainer or want to understand the maintainer workflow:
- [Being a Maintainer](../maintainer/being-a-maintainer.md)
- [Release Checklist](../maintainer/release-checklist.md)
- [Branching Strategy](../maintainer/branching.md)
- [Versioning](../maintainer/versioning.md)

## Related Documentation

- [Building SkiaSharp](../building/building-skiasharp.md)
- [Adding New APIs](../../design/adding-new-apis.md)
- [Architecture Overview](../../design/architecture-overview.md)
- [Submitting Issues](submitting-issues.md)
- [Writing Documentation](writing-docs.md)

## License

By contributing to SkiaSharp, you agree that your contributions will be licensed under the MIT License.

---

**Thank you for contributing to SkiaSharp!** 🎨
