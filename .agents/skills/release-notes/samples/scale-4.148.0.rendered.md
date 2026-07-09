<!-- RELEASE-NOTES DATA (generated, do not edit) format:3 version:4.148.0 -->
# Version 4.148.0
> **First stable v4 release** · Released June 22, 2026 · [NuGet](https://www.nuget.org/packages/SkiaSharp/4.148.0) · [GitHub Release](https://github.com/mono/SkiaSharp/releases/tag/v4.148.0)
> **Supersedes [4.147.0](4.147.0.md)** · Rolls up preview-only work that was never released as stable — those changes are included cumulatively below.
> **API changes** · [SkiaSharp API diff](4.148.0/index.md)

## Highlights

SkiaSharp 4.148.0 is the first stable release on Skia m148, rolling up preview-only work from 4.147. It ships animated WebP encoding, variable and color font support, and a reworked singleton lifecycle. Legacy paint text and font members are now compile-time errors, and pixel access on SKPixmap and SKBitmap has been corrected. Review the breaking changes below before upgrading.

## Breaking Changes

- **Legacy SKPaint text and font state removed** — The SKPaint text and font members that were obsoleted throughout the v3 line are now compile errors, and the remaining state-reading APIs on SKPaint have been newly obsoleted. Move typeface, text size, hinting and measurement onto SKFont, and draw through the SKCanvas overloads that take an SKFont before upgrading. ([#4068](https://github.com/mono/SkiaSharp/pull/4068), [#4114](https://github.com/mono/SkiaSharp/pull/4114))
- **Pre-v4 obsoletes are now errors and trimmed from the reference assembly** — The remaining APIs marked obsolete before the v4 line have been promoted from warnings to compile errors, and the obsolete enum members are no longer surfaced by the reference assembly. Delete or migrate any code that still calls them — a warning-suppression will no longer keep a v3 codebase building. ([#4205](https://github.com/mono/SkiaSharp/pull/4205))
- **Singleton instance lifecycle changed** — Shared singleton instances now follow a deterministic teardown-and-recreate model instead of relying on the GC to keep them alive across resets. If your code cached a reference to a singleton across an engine reset or teardown, re-fetch it after the reset — the old instance is no longer valid. ([#4080](https://github.com/mono/SkiaSharp/pull/4080))

## Engine

- **Skia updated to milestone m148** — The rendering engine moves to the latest Skia release, arriving via progressive bumps through m132, m133 and m147 during the preview cycle. This pulls in the full m148 API surface and the upstream text, path and codec fixes that shipped with it, and includes a small submodule refresh to pick up the legacy fontmgr typeface entry point. — ❤️ [@ramezgerges](https://github.com/ramezgerges) ([#4125](https://github.com/mono/SkiaSharp/pull/4125), [#3702](https://github.com/mono/SkiaSharp/pull/3702), [#3660](https://github.com/mono/SkiaSharp/pull/3660), [#3560](https://github.com/mono/SkiaSharp/pull/3560), [#3744](https://github.com/mono/SkiaSharp/pull/3744))
- **Upstream Chrome bug-fix syncs from chrome/m147** — Two upstream syncs pull in Chrome's post-branch fixes for the m147 milestone, hardening text, image and GPU paths without changing the public surface. ([#4081](https://github.com/mono/SkiaSharp/pull/4081), [#4044](https://github.com/mono/SkiaSharp/pull/4044))

## API Surface

- **Animated WebP encoding** — SKWebpEncoder can now write multi-frame animated WebP output, matching the animated WebP decoding support that already shipped. ([#3771](https://github.com/mono/SkiaSharp/pull/3771))
- **Variable fonts and color palettes** — SkiaSharp and HarfBuzzSharp now expose variable font axes end-to-end, and color font palette selection is surfaced with additional robustness fixes for the variable-font code paths. — ❤️ [@ramezgerges](https://github.com/ramezgerges) ([#3703](https://github.com/mono/SkiaSharp/pull/3703), [#3742](https://github.com/mono/SkiaSharp/pull/3742))
- **Zero-copy SKStream.GetData()** — The new SKStream.GetData() returns an SKData view over a stream's contents without allocating or copying the bytes, making stream-to-data handoffs cheap in hot paths. ([#3772](https://github.com/mono/SkiaSharp/pull/3772))
- **SKSamplingOptions on surface draw** — SKSurface.Draw and SKCanvas.DrawSurface gained SKSamplingOptions overloads so callers can pick the filter and mipmap mode for surface-to-surface composition. ([#3491](https://github.com/mono/SkiaSharp/pull/3491))
- **HarfBuzz surface polish** — A dedicated HBColor struct now represents hb_color_t correctly, and several previously missing HarfBuzz wrappers have been added and renamed to follow the binding's naming conventions. ([#4000](https://github.com/mono/SkiaSharp/pull/4000), [#4001](https://github.com/mono/SkiaSharp/pull/4001))

## Bug Fixes

- **SKPixmap and SKBitmap pixel span access corrected** — GetPixelSpan now uses RowBytes for the stride and applies x/y offsets against the correct axis, so multi-row and offset reads return the intended pixels. Code that was working around the old behaviour by hand-adjusting offsets should be re-checked before upgrading. ([#4148](https://github.com/mono/SkiaSharp/pull/4148), [#4128](https://github.com/mono/SkiaSharp/pull/4128))
- **SKPath finalizer crash fixed** — An intermittent crash that happened when an SKPathBuilder was collected before the SKPath it produced no longer occurs during finalization. — ❤️ [@ramezgerges](https://github.com/ramezgerges) ([#3796](https://github.com/mono/SkiaSharp/pull/3796))
- **Default typeface resolution moved to the managed layer** — Default typeface fallback now happens in managed code instead of the native shim, fixing #3693 and making the lookup behaviour consistent across platforms. — ❤️ [@ramezgerges](https://github.com/ramezgerges) ([#3730](https://github.com/mono/SkiaSharp/pull/3730))
- **WinUI projection loads under .NET 9** — Fixed a missing native DLL that prevented the WinUI projection from being resolved for .NET 9 consumers. ([#4084](https://github.com/mono/SkiaSharp/pull/4084))
- **MAUI Android SKGLView redraws after tab switch** — SKGLView no longer stops rendering when a MAUI TabBar tab is switched away and back on Android. — ❤️ [@SimonvBez](https://github.com/SimonvBez) ([#3076](https://github.com/mono/SkiaSharp/pull/3076))

## Lifecycle & Internals

- **Native-compat check moved off ModuleInitializer** — The native runtime compatibility gate now runs from the SkiaApi static constructor instead of a [ModuleInitializer], so the check fires on first API use rather than at module load. This avoids initializer ordering surprises for hosts that load SkiaSharp indirectly. ([#4133](https://github.com/mono/SkiaSharp/pull/4133))

## Platform

- **Apple platform TFMs aligned** — Libraries now target the 26.0 Apple TFMs while apps stay on the unversioned TFMs, matching the current .NET for iOS/macOS/tvOS/Mac Catalyst conventions. ([#3798](https://github.com/mono/SkiaSharp/pull/3798))
- **Linux Bionic native assets** — SkiaSharp now ships native assets for Linux Bionic, unblocking Android-adjacent Linux hosts that use the Bionic C library. — ❤️ [@4Darmygeometry](https://github.com/4Darmygeometry) ([#3217](https://github.com/mono/SkiaSharp/pull/3217))
- **Tizen gains x64 and arm64** — Native builds for Tizen have been extended to x64 and arm64 alongside the existing architectures, with per-architecture CI splits. ([#3620](https://github.com/mono/SkiaSharp/pull/3620))
- **WASM drops pre-.NET 8 Emscripten builds** — Native WASM builds for Emscripten versions predating .NET 8 have been retired. .NET 8 and later consumers are unaffected; hosts still on older .NET should stay on the previous SkiaSharp line. ([#4022](https://github.com/mono/SkiaSharp/pull/4022))

## Security

- **Bundled native dependencies refreshed** — The vendored native libraries have been updated to their latest upstream releases across the preview cycle: expat (to 2.7.5 and then 2.8.1), libpng 1.6.58, freetype 2.14.3, harfbuzz 14.2.0, libjpeg-turbo 3.1.4.1 and zlib 1.3.2.1-motley. This picks up the accumulated upstream security and correctness fixes without changing the public API. ([#4079](https://github.com/mono/SkiaSharp/pull/4079), [#3717](https://github.com/mono/SkiaSharp/pull/3717), [#3718](https://github.com/mono/SkiaSharp/pull/3718), [#3726](https://github.com/mono/SkiaSharp/pull/3726), [#4035](https://github.com/mono/SkiaSharp/pull/4035), [#4012](https://github.com/mono/SkiaSharp/pull/4012), [#3720](https://github.com/mono/SkiaSharp/pull/3720))

## Community Contributors ❤️

Thank you to everyone who contributed to this release!

| Contributor | Contributions |
|-------------|---------------|
| [@ramezgerges](https://github.com/ramezgerges) | Reworked the singleton instance lifecycle, promoted the legacy SKPaint text and font APIs to compile errors, added variable font support to SkiaSharp and HarfBuzzSharp, fixed the SKPath finalizer crash, drove the initial Skia m132 and m147 bumps, and updated the SkiaSharpGenerator and Uno gallery samples. ([#3560](https://github.com/mono/SkiaSharp/pull/3560), [#3666](https://github.com/mono/SkiaSharp/pull/3666), [#3692](https://github.com/mono/SkiaSharp/pull/3692), [#3702](https://github.com/mono/SkiaSharp/pull/3702), [#3703](https://github.com/mono/SkiaSharp/pull/3703), [#3714](https://github.com/mono/SkiaSharp/pull/3714), [#3730](https://github.com/mono/SkiaSharp/pull/3730), [#3758](https://github.com/mono/SkiaSharp/pull/3758), [#3762](https://github.com/mono/SkiaSharp/pull/3762), [#3785](https://github.com/mono/SkiaSharp/pull/3785), [#3790](https://github.com/mono/SkiaSharp/pull/3790), [#3796](https://github.com/mono/SkiaSharp/pull/3796), [#3821](https://github.com/mono/SkiaSharp/pull/3821), [#3867](https://github.com/mono/SkiaSharp/pull/3867), [#4068](https://github.com/mono/SkiaSharp/pull/4068), [#4080](https://github.com/mono/SkiaSharp/pull/4080)) |
| [@4Darmygeometry](https://github.com/4Darmygeometry) | Added native asset support for Linux Bionic and introduced PolySharp so the bindings can target C# 13 on legacy TFMs. ([#3217](https://github.com/mono/SkiaSharp/pull/3217), [#3642](https://github.com/mono/SkiaSharp/pull/3642)) |
| [@ebariche](https://github.com/ebariche) | Cut the SkiaFiddle WASM sample's download size by roughly 60%. ([#3849](https://github.com/mono/SkiaSharp/pull/3849)) |
| [@sasakrsmanovic](https://github.com/sasakrsmanovic) | Refreshed the Uno Platform link and description in the project README. ([#3966](https://github.com/mono/SkiaSharp/pull/3966)) |
| [@SimonvBez](https://github.com/SimonvBez) | Fixed SKGLView on Android so it keeps rendering after switching MAUI TabBar tabs. ([#3076](https://github.com/mono/SkiaSharp/pull/3076)) |

## Release Candidate 1 (June 12, 2026)

Release Candidate 1 finalised the Skia m148 update, the singleton lifecycle rework and the SKPaint obsolete-to-error promotion, and shipped the SKPixmap/SKBitmap pixel-access fix, the WinUI .NET 9 projection fix, and the expat 2.8.1 and harfbuzz 14.2.0 refreshes.

[Full changelog](https://github.com/mono/SkiaSharp/compare/v4.147.0-preview.3.1...v4.148.0-rc.1.2)

## Preview 3 (May 24, 2026)

Preview 3 added the HBColor struct and the missing HarfBuzz wrappers, dropped native WASM builds for pre-.NET 8 Emscripten versions, and refreshed libjpeg-turbo, freetype and zlib.

[Full changelog](https://github.com/mono/SkiaSharp/compare/v4.147.0-preview.2.1...v4.147.0-preview.3.1)

## Preview 2 (May 6, 2026)

Preview 2 introduced animated WebP encoding and the zero-copy SKStream.GetData() helper, fixed the SKPath finalizer crash reported against SKPathBuilder, and aligned the Apple platform TFMs to 26.0.

[Full changelog](https://github.com/mono/SkiaSharp/compare/v4.147.0-preview.1.1...v4.147.0-preview.2.1)

## Preview 1 (April 28, 2026)

Preview 1 opened the 4.148 line with variable and color font support, SKSamplingOptions on SKSurface.Draw and SKCanvas.DrawSurface, the earliest Skia milestone bumps, Linux Bionic and Tizen architecture expansion, and the MAUI Android SKGLView tab-switch fix.

[Full changelog](https://github.com/mono/SkiaSharp/compare/v3.119.4...v4.147.0-preview.1.1)
