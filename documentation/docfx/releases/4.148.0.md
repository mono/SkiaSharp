<!-- RELEASE-NOTES DATA (generated, do not edit) format:5 version:4.148.0 -->
# Version 4.148.0
> **First stable v4 release** · Released June 22, 2026 · [NuGet](https://www.nuget.org/packages/SkiaSharp/4.148.0) · [GitHub Release](https://github.com/mono/SkiaSharp/releases/tag/v4.148.0)

> **Supersedes [4.147.0](4.147.0.md)** · Rolls up preview-only work that was never released as stable — those changes are included cumulatively below.

> **API changes** · [SkiaSharp API diff](4.148.0/index.md) · [HarfBuzzSharp API diff](harfbuzzsharp/14.2.0/index.md)

## Highlights

SkiaSharp 4.148.0 is the first stable v4 release, built on Skia m148. It rolls up the v4 preview features, including variable fonts and animated WebP encoding, and adds lifecycle and pixel-access fixes. This release contains source and behavioral breaking changes; review the migration guidance before upgrading from v3.

## Breaking Changes

- **Legacy SKPaint text and font members are compile errors** — The legacy paint text and font state members deprecated in v3 are now errors, and the remaining pre-v4 obsolete members are trimmed from the reference assembly. Move typeface, text size, hinting, and text encoding to `SKFont` and use the `SKCanvas.DrawText` overloads that accept it. ([#4068](https://github.com/mono/SkiaSharp/pull/4068), [#4114](https://github.com/mono/SkiaSharp/pull/4114), [#4205](https://github.com/mono/SkiaSharp/pull/4205))
- **Vulkan YCbCr conversion info has a new type name** — `GRVkImageInfo.YcbcrConversionInfo` now uses `GRVkYcbcrConversionInfo` instead of `GrVkYcbcrConversionInfo`. Update references and equality comparisons, because the former type's value-equality interface and operators are removed.
- **SKNativeObject disposal configuration is read-only** — `SKNativeObject.IgnorePublicDispose` no longer has a protected setter. Subclasses that changed it at runtime must set their disposal behavior through their construction path instead. ([#4080](https://github.com/mono/SkiaSharp/pull/4080))
- **Default SKFont and SKPaint typeface semantics changed** — `new SKFont()` now uses `SKTypeface.Empty`, so text measurement is zero until you supply a typeface; construct it with an explicit face such as `SKTypeface.Default`. `SKPaint.Typeface` now returns a non-null default face, so use `IsEmpty` rather than a null check when detecting an unset face.
- **Typeface lookup no longer reports a missing family as null** — `SKTypeface.FromFamilyName` now falls back to `SKTypeface.Default`, including on Android. Use `SKFontManager.Default.MatchFamily` when your application must distinguish an unavailable family and apply its own fallback.
- **SKStream.Move(long) rejects out-of-range offsets** — `Move(long)` is obsolete and now throws `OverflowException` instead of truncating offsets outside the 32-bit native range. Validate or clamp the value and call `Move(int)`.
- **Ganesh Vulkan now requires Vulkan 1.1** — `GRContext.CreateVulkan` can terminate the process rather than return null when the instance is below Vulkan 1.1. Create the instance at Vulkan 1.1 or later and set `GRVkBackendContext.MaxAPIVersion` to that version; use another backend for genuinely Vulkan 1.0-only hardware.
- **GRVkBackendContextNative layout changed** — Remove assignments to `fMinAPIVersion`, `fInstanceVersion`, `fExtensions`, `fFeatures`, and `fOwnsInstanceAndDevice`. Use `fVkExtensions` and `fDeviceFeatures` or `fDeviceFeatures2` for the replacements, and account for the new `Components` field in YCbCr conversion structs.

## Engine

- **Skia updated to milestone m148** — The stable v4 release includes the Skia m148 engine update and upstream m147 bug-fix syncs. ([#4044](https://github.com/mono/SkiaSharp/pull/4044), [#4081](https://github.com/mono/SkiaSharp/pull/4081), [#4125](https://github.com/mono/SkiaSharp/pull/4125))
- **Variable and color font support** — Variable fonts flow through SkiaSharp and HarfBuzzSharp, while color font palette selection and variable-font handling are improved. — ❤️ [@ramezgerges](https://github.com/ramezgerges) ([#3703](https://github.com/mono/SkiaSharp/pull/3703), [#3742](https://github.com/mono/SkiaSharp/pull/3742))

## API Surface

- **Sampling control for surface draws** — New surface-draw overloads accept `SKSamplingOptions`, allowing callers to choose filter and mipmap behavior. ([#3491](https://github.com/mono/SkiaSharp/pull/3491))
- **Animated WebP and stream data conversion** — `SKWebpEncoder` can encode frame sequences as animated WebP, and `SKStream.GetData()` converts seekable streams to `SKData` without an intermediate byte array. ([#3771](https://github.com/mono/SkiaSharp/pull/3771), [#3772](https://github.com/mono/SkiaSharp/pull/3772))
- **HarfBuzz value and wrapper additions** — HarfBuzzSharp adds the `HBColor` representation and fills in missing wrappers with consistent naming. ([#4000](https://github.com/mono/SkiaSharp/pull/4000), [#4001](https://github.com/mono/SkiaSharp/pull/4001))

## Bug Fixes

- **Pixel span access honors bitmap layout** — `GetPixelSpan` honors `RowBytes` for stride, and its coordinate overload calculates the row offset from width, returning correct pixels from non-tight bitmaps. ([#4128](https://github.com/mono/SkiaSharp/pull/4128), [#4148](https://github.com/mono/SkiaSharp/pull/4148))
- **SKPath finalizer crash fixed** — A collection-order bug no longer crashes finalization when an `SKPathBuilder` is collected before an `SKPath` it created. — ❤️ [@ramezgerges](https://github.com/ramezgerges) ([#3796](https://github.com/mono/SkiaSharp/pull/3796))
- **MAUI Android and WinUI startup fixes** — SKGLView is restored after MAUI TabBar switching, and .NET 9 WinUI consumers receive the Projection assembly needed at startup. — ❤️ [@SimonvBez](https://github.com/SimonvBez) ([#3076](https://github.com/mono/SkiaSharp/pull/3076), [#4084](https://github.com/mono/SkiaSharp/pull/4084))

## Lifecycle & Internals

- **Singleton lifecycle reworked** — Process-wide singleton initialization, disposal, and finalization are coordinated by the managed layer. — ❤️ [@ramezgerges](https://github.com/ramezgerges) ([#4080](https://github.com/mono/SkiaSharp/pull/4080))
- **Native compatibility checks run on first API use** — The compatibility gate now runs from the SkiaApi type initializer rather than module initialization, improving startup behavior for AOT and trimmed applications. ([#4133](https://github.com/mono/SkiaSharp/pull/4133))

## Platform

- **Additional Linux and Tizen native assets** — Android-hosted Linux Bionic and Tizen x64 and arm64 are available as native targets. — ❤️ [@4Darmygeometry](https://github.com/4Darmygeometry) ([#3217](https://github.com/mono/SkiaSharp/pull/3217), [#3620](https://github.com/mono/SkiaSharp/pull/3620))
- **Updated Apple and WASM targeting** — Apple libraries align with current versioned TFMs, and WASM native assets now require .NET 8 or later. ([#3798](https://github.com/mono/SkiaSharp/pull/3798), [#4022](https://github.com/mono/SkiaSharp/pull/4022))

## Security

- **Bundled native libraries refreshed** — The bundled libexpat, libpng, zlib, FreeType, libjpeg-turbo, and HarfBuzz dependencies have been updated. ([#3717](https://github.com/mono/SkiaSharp/pull/3717), [#3718](https://github.com/mono/SkiaSharp/pull/3718), [#3720](https://github.com/mono/SkiaSharp/pull/3720), [#3726](https://github.com/mono/SkiaSharp/pull/3726), [#4012](https://github.com/mono/SkiaSharp/pull/4012), [#4035](https://github.com/mono/SkiaSharp/pull/4035), [#4079](https://github.com/mono/SkiaSharp/pull/4079))

## HarfBuzzSharp 14.2.0

Bundles HarfBuzz 14.2.0 and carries the variable-font, color-palette, HBColor, and wrapper improvements from the v4 line.

## Community Contributors ❤️

Thank you to everyone who contributed to this release!

| Contributor | Contributions |
|-------------|---------------|
| [@ramezgerges](https://github.com/ramezgerges) | Skia milestone and variable-font work, managed default-typeface resolution, the SKPath finalizer fix, singleton lifecycle work, and legacy paint API migration ([#3560](https://github.com/mono/SkiaSharp/pull/3560), [#3666](https://github.com/mono/SkiaSharp/pull/3666), [#3692](https://github.com/mono/SkiaSharp/pull/3692), [#3702](https://github.com/mono/SkiaSharp/pull/3702), [#3703](https://github.com/mono/SkiaSharp/pull/3703), [#3714](https://github.com/mono/SkiaSharp/pull/3714), [#3730](https://github.com/mono/SkiaSharp/pull/3730), [#3758](https://github.com/mono/SkiaSharp/pull/3758), [#3762](https://github.com/mono/SkiaSharp/pull/3762), [#3785](https://github.com/mono/SkiaSharp/pull/3785), [#3790](https://github.com/mono/SkiaSharp/pull/3790), [#3796](https://github.com/mono/SkiaSharp/pull/3796), [#3821](https://github.com/mono/SkiaSharp/pull/3821), [#3867](https://github.com/mono/SkiaSharp/pull/3867), [#4068](https://github.com/mono/SkiaSharp/pull/4068), [#4080](https://github.com/mono/SkiaSharp/pull/4080)) |
| [@4Darmygeometry](https://github.com/4Darmygeometry) | Linux Bionic native assets and C# 13 support for legacy target frameworks ([#3217](https://github.com/mono/SkiaSharp/pull/3217), [#3642](https://github.com/mono/SkiaSharp/pull/3642)) |
| [@ebariche](https://github.com/ebariche) | Uno sample and integration improvements ([#3849](https://github.com/mono/SkiaSharp/pull/3849)) |
| [@sasakrsmanovic](https://github.com/sasakrsmanovic) | Sample and infrastructure improvements ([#3966](https://github.com/mono/SkiaSharp/pull/3966)) |
| [@SimonvBez](https://github.com/SimonvBez) | The Android MAUI TabBar fix for SKGLView rendering ([#3076](https://github.com/mono/SkiaSharp/pull/3076)) |

## Release Candidate 1 (June 12, 2026)

The release candidate added Skia m148, singleton lifecycle work, pixel-span fixes, and the WinUI .NET 9 startup fix.

[Full changelog](https://github.com/mono/SkiaSharp/compare/v4.147.0-preview.3.1...v4.148.0-rc.1.2)

## Preview 3 (May 24, 2026)

Preview 3 added HBColor and HarfBuzz wrapper improvements, refreshed bundled dependencies, and updated WASM targeting.

[Full changelog](https://github.com/mono/SkiaSharp/compare/v4.147.0-preview.2.1...v4.147.0-preview.3.1)

## Preview 2 (May 6, 2026)

Preview 2 added animated WebP encoding, zero-copy `SKStream.GetData()`, and the SKPath finalizer fix.

[Full changelog](https://github.com/mono/SkiaSharp/compare/v4.147.0-preview.1.1...v4.147.0-preview.2.1)

## Preview 1 (April 28, 2026)

Preview 1 opened the v4 line with the Skia m147 engine update, variable fonts, surface sampling options, and expanded Linux and Tizen support.

[Full changelog](https://github.com/mono/SkiaSharp/compare/v3.119.4...v4.147.0-preview.1.1)
