# Adding Libraries

This guide covers adding new projects and NuGet packages to SkiaSharp.

The repo has a non-standard structure due to multi-platform packaging requirements.

> **Related:** [Adding APIs](adding-apis.md) | [Architecture](architecture.md)

## TL;DR

**Key points for creating a new library:**

1. **Project Structure** - Each TFM gets its own project; shared code uses file linking
2. **Versioning** - Set `<PackagingGroup>` in .csproj, add entry to `scripts/VERSIONS.txt`
3. **Strong Naming** - Enabled by default; disable with `<SignAssembly>false</SignAssembly>` if framework isn't signed
4. **NuGet** - Built via SDK-style `dotnet pack` from .csproj (no nuspec needed)

**Files to create/modify:**
- `source/SkiaSharp.MyLibrary/` - New project folder
- `scripts/VERSIONS.txt` - Add version entry

---

## Project Structure

SkiaSharp uses a one-project-per-TFM structure to handle complex multi-platform packaging requirements. This avoids issues with:

- Overlapping TFMs (e.g., WPF and GTK# sharing the same TFM)
- Bait-and-switch patterns across platforms
- Dependency conflicts from shared types

Each TFM gets its own project for consistency. Shared code uses file linking with wildcards from a shared folder.

## Versioning

All versioning information lives in `scripts/VERSIONS.txt`. The build system reads this file and extracts the correct version based on the `<PackagingGroup>` property.

Set the `<PackagingGroup>` property in your .csproj - this is the NuGet package ID:

```xml
<PackagingGroup>SkiaSharp.MyLibrary</PackagingGroup>
```

Then add a corresponding entry in `scripts/VERSIONS.txt`:

```
# In scripts/VERSIONS.txt (under # nuget versions section)
SkiaSharp.MyLibrary                             nuget       3.119.3
```

The format is: `PackageId  nuget  version` (whitespace separated).

## Strong Naming

By default, all projects are strong named to assist users who require this feature. However, not all assemblies _can_ be strong named because the actual framework may not be. For example, Xamarin.Forms is not strong named, so we cannot strong name the SkiaSharp views.

As a result, this must be turned off via the `<SignAssembly>` property:

```xml
<SignAssembly>false</SignAssembly>
```

## Starter Project

Here's a minimal project:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- Target frameworks - use variables from SkiaSharp.Build.props -->
    <TargetFrameworks>$(TFMCurrent)</TargetFrameworks>
    <!-- Namespace for the library -->
    <RootNamespace>SkiaSharp</RootNamespace>
    <!-- NuGet package ID - must match entry in VERSIONS.txt -->
    <PackagingGroup>SkiaSharp.MyLibrary</PackagingGroup>
    <!-- Package metadata -->
    <Title>SkiaSharp MyLibrary Support</Title>
    <PackageDescription>This package adds MyLibrary support to SkiaSharp.</PackageDescription>
    <!-- Disable strong naming if framework isn't signed -->
    <SignAssembly>false</SignAssembly>
  </PropertyGroup>

  <ItemGroup>
    <!-- External framework dependency -->
    <PackageReference Include="MyLibrary" Version="1.0.0" />
  </ItemGroup>

  <ItemGroup>
    <!-- Reference core SkiaSharp -->
    <ProjectReference Include="..\..\..\binding\SkiaSharp\SkiaSharp.csproj" />
  </ItemGroup>

</Project>
```

### Optional: Shared Source Files

If multiple projects share common code, use file linking:

```xml
<ItemGroup>
  <!-- Include shared source files from a sibling folder -->
  <Compile Include="..\SkiaSharp.MyLibrary.Shared\**\*.cs" Link="%(RecursiveDir)%(Filename)%(Extension)" />
</ItemGroup>
```

### Optional: MSBuild Targets for Consumers

To include `.targets` or `.props` files that run in consuming projects:

```xml
<ItemGroup>
  <!-- Include build targets in the NuGet package -->
  <None Include="build\SkiaSharp.MyLibrary.targets" PackagePath="build\$(TargetFramework)\" Pack="true" />
</ItemGroup>
```

## NuGet Packaging

NuGet packages are built using **SDK-style `dotnet pack`** directly from the .csproj files. The build system automatically:

- Reads version from `VERSIONS.txt` based on `<PackagingGroup>`
- Resolves dependencies between projects in the repo
- Generates package metadata from project properties

To pack all NuGets:

```bash
dotnet cake --target=nuget
```

The output is placed in `output/nugets/`.

> **Note:** The `scripts/nuget/` folder contains nuspec files only for special meta-packages (like `_NuGets`, `_NativeAssets`) that bundle multiple packages together. Regular libraries don't need nuspec files.

## Build Output

Once all the properties are set in the .csproj file, a build will place all the files and docs in the correct sub-folders in the `output` directory. And, then the NuGet packaging task will just use that folder as the base.

## Summary

To create a new library:

1. Create project folder in `source/`
2. Set required properties: `TargetFrameworks`, `RootNamespace`, `PackagingGroup`
3. Add version entry to `scripts/VERSIONS.txt`
4. Build and verify output in `output/` directory

## Related Documentation

- [Adding New APIs](adding-apis.md) - Adding APIs to existing libraries
- [Architecture Overview](architecture.md) - Understanding the three-layer architecture