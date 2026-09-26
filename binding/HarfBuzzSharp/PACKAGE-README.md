# HarfBuzzSharp

[![NuGet](https://img.shields.io/nuget/v/HarfBuzzSharp?style=flat-square&label=NuGet)](https://www.nuget.org/packages/HarfBuzzSharp)
[![NuGet downloads](https://img.shields.io/nuget/dt/HarfBuzzSharp?style=flat-square&label=Downloads)](https://www.nuget.org/packages/HarfBuzzSharp)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)]({{RepositoryUrl}}/blob/main/LICENSE.md)

**HarfBuzzSharp** provides .NET bindings for the [HarfBuzz](https://harfbuzz.github.io/) OpenType text shaping engine. It converts Unicode text into correctly selected and positioned glyphs for complex scripts, ligatures, kerning, variable fonts, bidirectional text, and advanced typographic features.

## Capabilities

- Shape text for Arabic, Indic, Southeast Asian, and other complex writing systems.
- Apply OpenType features, language, script, and direction settings.
- Inspect glyph IDs, clusters, advances, and offsets.
- Read OpenType variation axes, names, metrics, color glyph data, and layout information.
- Run on Windows, macOS, Linux, Android, iOS, Mac Catalyst, tvOS, Tizen, and WebAssembly.
- Integrate with SkiaSharp rendering through [`SkiaSharp.HarfBuzz`](https://www.nuget.org/packages/SkiaSharp.HarfBuzz).

## Get started

Install the package:

```bash
dotnet add package HarfBuzzSharp
```

Shape a string using an OpenType font:

```csharp
using HarfBuzzSharp;

using var blob = Blob.FromFile("NotoSansArabic-Regular.ttf");
using var face = new Face(blob, 0);
using var font = new Font(face);
font.SetScale(face.UnitsPerEm, face.UnitsPerEm);

using var buffer = new HarfBuzzSharp.Buffer();
buffer.AddUtf8("مرحبا بالعالم");
buffer.GuessSegmentProperties();

font.Shape(buffer);

foreach (var glyph in buffer.GlyphInfos)
    Console.WriteLine($"Glyph {glyph.Codepoint}, cluster {glyph.Cluster}");
```

For drawing shaped text with SkiaSharp, install `SkiaSharp.HarfBuzz` and use `SKShaper`:

```bash
dotnet add package SkiaSharp.HarfBuzz
```

> **Linux and WebAssembly:** add the matching `HarfBuzzSharp.NativeAssets.*` package directly to the application project when the target does not receive native assets transitively. See the [package deployment guide]({{RepositoryUrl}}/blob/main/documentation/dev/packages.md).

## Documentation and resources

- [HarfBuzzSharp API reference on Microsoft Learn](https://learn.microsoft.com/dotnet/api/harfbuzzsharp)
- [SkiaSharp.HarfBuzz API reference](https://learn.microsoft.com/dotnet/api/skiasharp.harfbuzz)
- [HarfBuzz documentation](https://harfbuzz.github.io/)
- [SkiaSharp package and deployment guide]({{RepositoryUrl}}/blob/main/documentation/dev/packages.md)
- [SkiaSharp samples]({{RepositoryUrl}}/tree/main/samples)

## Feedback and contributing

HarfBuzzSharp is part of the open-source SkiaSharp project from Microsoft. Use [GitHub Discussions]({{RepositoryUrl}}/discussions) for questions, file bugs and feature requests in the [issue tracker]({{RepositoryUrl}}/issues), and read the [contributing guide]({{RepositoryUrl}}/blob/main/CONTRIBUTING.md) to get involved.

HarfBuzzSharp is released under the [MIT license]({{RepositoryUrl}}/blob/main/LICENSE.md).
