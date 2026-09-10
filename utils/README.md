# Utils

This directory contains a few tools to help with the binding and development of SkiaSharp and related libraries.

## SkiaSharpGenerator

This is a small set of tools that help with generating the p/invoke layer from the C header files.

### Generate

Regenerate all P/Invoke bindings:

```pwsh
pwsh ./utils/generate.ps1
```

Regenerate a specific binding:

```pwsh
pwsh ./utils/generate.ps1 -Config libSkiaSharp.json
```

Available configs:
- `libSkiaSharp.json` - Core SkiaSharp
- `libSkiaSharp.Skottie.json` - Skottie animation
- `libSkiaSharp.SceneGraph.json` - Scene graph
- `libSkiaSharp.Resources.json` - Resources
- `libHarfBuzzSharp.json` - HarfBuzz text shaping

### Verify

This can be run with:

```pwsh
dotnet run --project=utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- verify --config binding/libSkiaSharp.json --skia externals/skia
```

* `--config binding/libSkiaSharp.json`  
  The path to the JSON file that help generate a useful set of p/invoke definions and structures.
* `--skia externals/skia`  
  The path to the root of the skia source.
* `--output binding/Binding/Generated`
  The output directory for the generated source tree.

### Cookie Detector

This can be run with:

```pwsh
dotnet run --project=utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- cookie --assembly binding\SkiaSharp\bin\Debug\netstandard2.0\SkiaSharp.dll --type "SkiaSharp.SkiaApi"
```

Or:

```pwsh
dotnet run --project=utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- cookie --assembly binding\HarfBuzzSharp\bin\Debug\netstandard2.0\HarfBuzzSharp.dll --type "HarfBuzzSharp.HarfBuzzApi"
```


* `--assembly <assembly>`  
  Read the assembly and log any missing interops.
* `--type <full type name>`  
  The type containing the interops.

## ApiDocsMigrator

Migrates compiler XML documentation from an extracted NuGet package sidecar
into C# `///` documentation comments. The initial focused scope is
`binding/SkiaSharp`, including its split generated source files.

```pwsh
dotnet run --project utils/ApiDocsMigrator -- `
  --package-xml artifacts/ci-3070267/core/SkiaSharp.xml `
  --source binding/SkiaSharp `
  --apply-resolved
```

Use `--dry-run` to report proposed changes without writing files. The default
mode is strict and makes no changes when package DocIds are missing or
ambiguous. `--apply-resolved` writes exact matches and exits nonzero with the
remaining DocIds listed for later manual resolution.

The migration preserves XML content, including Markdown CDATA, examples, and
media references. It emits documentation elements in this deterministic order:

1. `summary`, `inheritdoc`, and `include`
2. `typeparam`
3. `param`
4. `returns` or `value`
5. `exception`
6. `remarks`
7. `example`
8. `seealso`
9. `permission`

Compare the compiler-emitted XML against the package baseline with:

```pwsh
dotnet run --project utils/ApiDocsMigrator -- `
  --package-xml artifacts/ci-3070267/core/SkiaSharp.xml `
  --compare-xml binding/SkiaSharp/bin/Debug/net10.0/SkiaSharp.xml
```

The comparison normalizes package constructor IDs (`C:` versus compiler
`M:#ctor`), nested type separators, insignificant XML indentation, Markdown
CDATA indentation, and historical non-breaking-space suffixes in
`paramref` names. It still reports missing, unexpected, and changed members.
