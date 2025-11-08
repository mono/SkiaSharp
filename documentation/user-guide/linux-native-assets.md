# SkiaSharp Native Assets for Linux

As of SkiaSharp 1.68.0, we provide separate NuGet packages specifically for Linux native assets to better support users running on various Linux distributions.

## The Package

The `SkiaSharp.NativeAssets.Linux` package contains various Linux binaries that have been tested and confirmed to work in our scenarios. This package includes the runtime binaries placed in the `linux-x64` runtime folder.

These binaries have been tested and appear to work on several distributions including:
- Ubuntu (14.04, 16.04, 18.04, 20.04, 22.04)
- OpenSUSE
- CentOS
- Debian
- Many other Debian-based distributions

## Additional Packages

### No Dependencies Build

If you're in an environment where you cannot install required dependencies like FontConfig, you can use:

> `SkiaSharp.NativeAssets.Linux.NoDependencies`

This build only depends on core system libraries.

### Platform Support

At the current time, these packages support several platforms/architectures/distributions:

**Base platforms:**
- Debian x64, ARM and ARM64
- Alpine x64

**Known to work on:**
- Red Hat Enterprise Linux
- CentOS
- Raspberry Pi
- Many Musl distributions
- Most Debian-based distros

## Usage

The overall result is that you continue to use SkiaSharp as usual, but in the "application" part of your solution, you add the native assets package.

> **Important:** Do not release a NuGet package to nuget.org that depends on this package directly, as you will then force all your users to use a specific binary version.

### Example

In your application project file (`.csproj`):

```xml
<ItemGroup>
  <PackageReference Include="SkiaSharp" Version="3.0.0" />
  <PackageReference Include="SkiaSharp.NativeAssets.Linux" Version="3.0.0" />
</ItemGroup>
```

## Community Builds

We encourage the community to create and share builds for specific distributions if they're not yet officially supported.

### Example Workflow

1. **Application developer** builds an app for a specific Linux distro
2. If official binaries don't work, they build custom `libSkiaSharp.so` for their distro
3. Community member creates a NuGet package (e.g., `CommunityName.SkiaSharp.NativeAssets.DistroName`)
4. Other developers use the community package
5. If the package gets popular, it may be incorporated into official packages

### Requesting More Distributions

If you need support for additional Linux distributions, please:

1. Open an issue on the [SkiaSharp GitHub repository](https://github.com/mono/SkiaSharp/issues)
2. Provide details about your distribution (name, version, architecture)
3. Include information about your use case
4. If possible, provide a Dockerfile for building

## Building Custom Binaries

If you need to build SkiaSharp for a specific Linux distribution that isn't officially supported, see:

- [Building on Linux](../building/building-on-linux.md) - Complete guide for building native binaries

## Related Documentation

- [Building on Linux](../building/building-on-linux.md)
- [Views Support Matrix](views-support-matrix.md)
