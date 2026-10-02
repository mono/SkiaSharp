<!-- RELEASE-NOTES DATA (generated, do not edit) format:5 version:4.147.0 -->
# Version 4.147.0
> **First v4 previews** · [NuGet (prerelease)](https://www.nuget.org/packages/SkiaSharp/4.147.0-preview.3.1)

> **Superseded by [4.148.0](4.148.0.md)** · Never released as stable — these changes rolled up into 4.148.0.

> **API changes** · [SkiaSharp API diff](4.147.0/index.md) · [HarfBuzzSharp API diff](harfbuzzsharp/8.3.1.6/index.md)

## Highlights

SkiaSharp 4.147.0 previews the v4 line with Skia m147, variable fonts, and animated WebP encoding. The preview series also adds sampling control for surface draws and zero-copy stream-to-data conversion, broadens native platform support, and fixes a finalizer crash. It includes a breaking Vulkan API rename; review the migration note before testing an upgrade.

## Breaking Changes

- **Vulkan YCbCr conversion info has a new type name** — `GRVkImageInfo.YcbcrConversionInfo` now uses `GRVkYcbcrConversionInfo` instead of `GrVkYcbcrConversionInfo`. Update references and equality comparisons when rebuilding Vulkan code, because the former type's value-equality interface and operators are removed.

## Engine

- **Skia advances to milestone m147** — The v4 preview line moves the bundled renderer through Skia milestones m132, m133, and m147. — ❤️ [@ramezgerges](https://github.com/ramezgerges) ([#3560](https://github.com/mono/SkiaSharp/pull/3560), [#3660](https://github.com/mono/SkiaSharp/pull/3660), [#3702](https://github.com/mono/SkiaSharp/pull/3702))
- **Variable and color font support** — Variable fonts flow through SkiaSharp and HarfBuzzSharp, while color font palette selection and variable-font handling are improved. — ❤️ [@ramezgerges](https://github.com/ramezgerges) ([#3703](https://github.com/mono/SkiaSharp/pull/3703), [#3742](https://github.com/mono/SkiaSharp/pull/3742))

## API Surface

- **Sampling control for surface draws** — New surface-draw overloads accept `SKSamplingOptions`, allowing callers to choose filter and mipmap behavior. ([#3491](https://github.com/mono/SkiaSharp/pull/3491))
- **Animated WebP and stream data conversion** — `SKWebpEncoder` can encode frame sequences as animated WebP, and `SKStream.GetData()` converts seekable streams to `SKData` without an intermediate byte array. ([#3771](https://github.com/mono/SkiaSharp/pull/3771), [#3772](https://github.com/mono/SkiaSharp/pull/3772))
- **HarfBuzz value and wrapper additions** — HarfBuzzSharp adds the `HBColor` representation and fills in missing wrappers with consistent naming. ([#4000](https://github.com/mono/SkiaSharp/pull/4000), [#4001](https://github.com/mono/SkiaSharp/pull/4001))

## Bug Fixes

- **SKPath finalizer crash fixed** — A collection-order bug no longer crashes finalization when an `SKPathBuilder` is collected before an `SKPath` it created. — ❤️ [@ramezgerges](https://github.com/ramezgerges) ([#3796](https://github.com/mono/SkiaSharp/pull/3796))
- **MAUI Android tab switching restores SKGLView** — SKGLView is reattached after returning to a MAUI TabBar tab instead of remaining blank. — ❤️ [@SimonvBez](https://github.com/SimonvBez) ([#3076](https://github.com/mono/SkiaSharp/pull/3076))

## Platform

- **Additional Linux and Tizen native assets** — Android-hosted Linux Bionic and Tizen x64 and arm64 are available as native targets. — ❤️ [@4Darmygeometry](https://github.com/4Darmygeometry) ([#3217](https://github.com/mono/SkiaSharp/pull/3217), [#3620](https://github.com/mono/SkiaSharp/pull/3620))
- **Updated Apple and WASM targeting** — Apple libraries align with current versioned TFMs, and WASM native assets now require .NET 8 or later. ([#3798](https://github.com/mono/SkiaSharp/pull/3798), [#4022](https://github.com/mono/SkiaSharp/pull/4022))

## Security

- **Bundled native libraries refreshed** — The bundled libexpat, libpng, zlib, FreeType, and libjpeg-turbo dependencies have been updated. ([#3717](https://github.com/mono/SkiaSharp/pull/3717), [#3718](https://github.com/mono/SkiaSharp/pull/3718), [#3720](https://github.com/mono/SkiaSharp/pull/3720), [#3726](https://github.com/mono/SkiaSharp/pull/3726), [#4012](https://github.com/mono/SkiaSharp/pull/4012))

## HarfBuzzSharp 8.3.1.6

Adds variable-font and color-palette support, the HBColor value type, and missing HarfBuzz wrappers.

## Community Contributors ❤️

Thank you to everyone who contributed to this release!

| Contributor | Contributions |
|-------------|---------------|
| [@ramezgerges](https://github.com/ramezgerges) | Skia milestone and variable-font work, managed default-typeface resolution, the SKPath finalizer fix, and sample improvements ([#3560](https://github.com/mono/SkiaSharp/pull/3560), [#3666](https://github.com/mono/SkiaSharp/pull/3666), [#3692](https://github.com/mono/SkiaSharp/pull/3692), [#3702](https://github.com/mono/SkiaSharp/pull/3702), [#3703](https://github.com/mono/SkiaSharp/pull/3703), [#3714](https://github.com/mono/SkiaSharp/pull/3714), [#3730](https://github.com/mono/SkiaSharp/pull/3730), [#3758](https://github.com/mono/SkiaSharp/pull/3758), [#3762](https://github.com/mono/SkiaSharp/pull/3762), [#3785](https://github.com/mono/SkiaSharp/pull/3785), [#3790](https://github.com/mono/SkiaSharp/pull/3790), [#3796](https://github.com/mono/SkiaSharp/pull/3796), [#3821](https://github.com/mono/SkiaSharp/pull/3821), [#3867](https://github.com/mono/SkiaSharp/pull/3867)) |
| [@4Darmygeometry](https://github.com/4Darmygeometry) | Linux Bionic native assets and C# 13 support for legacy target frameworks ([#3217](https://github.com/mono/SkiaSharp/pull/3217), [#3642](https://github.com/mono/SkiaSharp/pull/3642)) |
| [@ebariche](https://github.com/ebariche) | Uno sample and integration improvements ([#3849](https://github.com/mono/SkiaSharp/pull/3849)) |
| [@sasakrsmanovic](https://github.com/sasakrsmanovic) | Sample and infrastructure improvements ([#3966](https://github.com/mono/SkiaSharp/pull/3966)) |
| [@SimonvBez](https://github.com/SimonvBez) | The Android MAUI TabBar fix for SKGLView rendering ([#3076](https://github.com/mono/SkiaSharp/pull/3076)) |

## Preview 3 (May 24, 2026)

Preview 3 added HBColor and HarfBuzz wrapper improvements, refreshed bundled dependencies, and updated WASM targeting.

[Full changelog](https://github.com/mono/SkiaSharp/compare/v4.147.0-preview.2.1...v4.147.0-preview.3.1)

## Preview 2 (May 6, 2026)

Preview 2 added animated WebP encoding, zero-copy `SKStream.GetData()`, and the SKPath finalizer fix.

[Full changelog](https://github.com/mono/SkiaSharp/compare/v4.147.0-preview.1.1...v4.147.0-preview.2.1)

## Preview 1 (April 28, 2026)

Preview 1 opened the v4 line with the Skia m147 engine update, variable fonts, surface sampling options, and expanded Linux and Tizen support.

[Full changelog](https://github.com/mono/SkiaSharp/compare/v3.119.4...v4.147.0-preview.1.1)
