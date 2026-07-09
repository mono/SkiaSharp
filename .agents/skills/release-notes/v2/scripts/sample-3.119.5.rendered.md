<!-- RELEASE-NOTES DATA (generated, do not edit) format:3 version:3.119.5 -->
# Version 3.119.5
> **Skia sync and WASM fixes** · Released March 18, 2026 · [NuGet](https://www.nuget.org/packages/SkiaSharp/3.119.5) · [GitHub Release](https://github.com/mono/SkiaSharp/releases/tag/v3.119.5)
> **API changes** · [SkiaSharp API diff](3.119.5/index.md)

## Highlights

SkiaSharp 3.119.5 is a servicing release that syncs upstream Skia bug fixes and restores WASM startup on .NET 10 RC.

## Breaking Changes

*None in this release.*

## Engine

- **Upstream Skia bug-fix sync** — Picks up the latest bug and stability fixes from Skia's chrome/m133 branch, keeping rendering behaviour aligned with upstream without moving the engine milestone. ([#4210](https://github.com/mono/SkiaSharp/pull/4210))

## Bug Fixes

- **WASM startup restored on .NET 10 RC** — Resolves a regression that prevented the SkiaSharp WASM module from initializing under the .NET 10 release candidate runtime. ([#4212](https://github.com/mono/SkiaSharp/pull/4212))
- **SKColorSpace equality corrected for named spaces** — Two SKColorSpace instances that refer to the same named colour space (such as sRGB) now compare as equal, matching what callers expect. ([#4215](https://github.com/mono/SkiaSharp/pull/4215))

## Links

- [SkiaSharp on NuGet](https://www.nuget.org/packages/SkiaSharp/3.119.5)
- [GitHub release](https://github.com/mono/SkiaSharp/releases/tag/v3.119.5)

