When adding a new project in the `source` directory, there are a few things that need to be done as this repo is a tiny bit non-standard when building/packing.

## Project Structure

The first thing is that the NuGets are not actually created via a `dotnet pack` or a `msbuild /t:pack` because there is overlap with both packages and projects. A typical example is the Xamarin.Forms views: 

Typically, we would have a single project and multiple platforms, but when packing, this results in a single package, which we do not want. Instead we want a core set of non-overlapping TFMs and then additional packages for the rest. For example, we want Android, iOS and UWP in the first package, and then WPF in the second. Another case would be the fact that both WPF and GTK# shared the same TFM. Finally, the dependencies get harder because libraries would define the same types due to bait and switch as well as non-overlapping TFMs.

In the end, each TFM gets its own project to be very consistent. Then, file linking is used in the form of wildcards from a shared folder.

## Versioning

Since it is hard to track all version numbers in all the various projects, all versioning information lives in a `VERSIONS.txt` file at the repo root. There is a pre-build task that will read this file and extract the correct version for this particular assembly.

This version is inserted into the project via the `<PackagingGroup>` property - which is basically the NuGet ID:

```xml
<PackagingGroup>SkiaSharp.Views.MyNewFramework</PackagingGroup>
```

Once you have those elements in your .csproj, you just need to add a line to the `VERSIONS.txt` (somewhere under the `# nuget versions` heading) at the repo root:

```
# nuget versions
...
SkiaSharp.Views.MyNewFramework   nuget   2.80.2
```

## Signing / Strong Naming

By default, all projects are string named to assist users who require this feature. However, not all assemblies _can_ be string named because the actual framework may not be. For example, Xamarin.Forms is not strong named, so we cannot strong name the SkiaSharp views.

As a result, this must be turned off via the `<SignAssembly>` property:

```xml
<SignAssembly>false</SignAssembly>
```

## Starter Project

That should be it, but a simple project would be:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- the set of TFMs this code is for -->
    <TargetFrameworks>netstandard1.3;netstandard2.0</TargetFrameworks>
    <!-- the assembly name -->
    <AssemblyName>SkiaSharp.Views.MyNewFramework</AssemblyName>
    <!-- the root namespace for any generated things -->
    <RootNamespace>SkiaSharp.Views.MyNewFramework</RootNamespace>
    <!-- skip [or not] strong naming -->
    <SignAssembly>false</SignAssembly>
    <!-- the NuGet ID for this assembly -->
    <PackagingGroup>SkiaSharp.Views.MyNewFramework</PackagingGroup>
  </PropertyGroup>

  <ItemGroup>
    <!-- pull in any libraries and frameworks -->
    <PackageReference Include="MyNewFramework" Version="3.5.0" />
  </ItemGroup>

  <ItemGroup>
    <!-- reference the required projects -->
    <ProjectReference Include="..\..\..\binding\SkiaSharp\SkiaSharp.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- include any additional files for the final NuGet -->
    <None Include="nuget\build\SkiaSharp.Views.MyNewFramework.targets" Link="nuget\build\$(TargetFramework)\SkiaSharp.Views.MyNewFramework.targets" />
  </ItemGroup>

  <ItemGroup>
    <!-- include any shared files -->
    <Compile Include="..\SkiaSharp.Views.MyNewFramework.Shared\**\*.cs" Link="%(RecursiveDir)%(Filename)%(Extension)" />
  </ItemGroup>

</Project>
```

## NuGet

All the NuGets are built from a "enhanced" .nuspec file which supports more features. This is a typical .nuspec, but with optional `target` and `platform` attributes on `<file>` entries. The version number is also automatically updated and dependency versions updated:

```xml
<?xml version="1.0" encoding="utf-8"?>
<package>
  <metadata>

    <!-- the NuGet ID -->
    <id>SkiaSharp.Views.MyNewFramework</id>
    <!-- the NuGet version, automatically updated to the VERSIONS.txt value -->
    <version>1.0.0</version>

    <dependencies>
      <group targetFramework="netstandard1.3">
        <!-- frameworks and libraries NOT in this repo need explicit versions -->
        <dependency id="MyNewFramework" version="3.5.0" />
        <!-- dependencies IN this repo are automatically updated -->
        <dependency id="SkiaSharp" version="1.0.0" />
      </group>
    </dependencies>

  </metadata>
  <files>

    <!-- no need for the `target` attribute if the value is the same as the src -->
    <file src="lib/netstandard1.3/SkiaSharp.Views.MyNewFramework.dll" />

    <!-- some assemblies are only built on some HOST platforms, so specify which of "windows,macos,linux" is supported -->
    <file platform="macos,windows" src="lib/monoandroid1.0/SkiaSharp.Views.MyNewFramework.dll" />

  </files>
</package>
```

## Build Output

Once all the properties are set in the .csproj file, a build will place all the files and docs in the correct sub-folders in the `output` directory. And, then the NuGet packaging task will just use that folder as the base.