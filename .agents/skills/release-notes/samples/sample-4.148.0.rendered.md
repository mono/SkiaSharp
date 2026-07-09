<!-- RELEASE-NOTES DATA (generated, do not edit) format:3 version:4.148.0 -->
# Version 4.148.0
> **First stable v4 release** · Released June 22, 2026 · [NuGet](https://www.nuget.org/packages/SkiaSharp/4.148.0) · [GitHub Release](https://github.com/mono/SkiaSharp/releases/tag/v4.148.0)
> **Supersedes [4.147.0](4.147.0.md)** · Rolls up preview-only work that was never released as stable — those changes are included cumulatively below.
> **API changes** · [SkiaSharp API diff](4.148.0/index.md) · [HarfBuzzSharp 14.2.0](harfbuzzsharp/14.2.0.md)

## Highlights

SkiaSharp 4.148.0 is the first stable release of the v4 line, built on Skia milestone m148. It introduces animated WebP encoding and a zero-copy stream-to-data helper, and reworks how singleton instances are torn down and re-created. This is a breaking release — review the changes below before upgrading from v3.

## Breaking Changes

- **SKPaint no longer exposes legacy text and font state** — The text and font state-reading members on SKPaint that were obsoleted throughout the v3 line are now compile errors. Move typeface, text size, hinting and related state onto an SKFont, and draw through the SKCanvas.DrawText overloads that take an SKFont before upgrading. ([#4068](https://github.com/mono/SkiaSharp/pull/4068), [#4114](https://github.com/mono/SkiaSharp/pull/4114))
- **Singleton instance lifecycle changed** — Shared singleton instances now follow a deterministic teardown-and-recreate model instead of relying on the GC to keep them alive across resets. If your code cached a reference to a singleton across an engine reset or teardown, re-fetch it after the reset — the old instance is no longer valid. ([#4080](https://github.com/mono/SkiaSharp/pull/4080))

## Engine

- **Skia updated to milestone m148** — The rendering engine moves to the latest Skia release, picking up upstream text, path and codec fixes. ([#4125](https://github.com/mono/SkiaSharp/pull/4125))

## API Surface

- **Animated WebP encoding** — SKWebpEncoder can now write animated WebP output, matching the animated decoding support that already shipped. ([#3771](https://github.com/mono/SkiaSharp/pull/3771))
- **Zero-copy stream-to-data conversion** — The new SKStream.GetData() returns an SKData view over a stream's contents without allocating or copying the bytes. ([#3772](https://github.com/mono/SkiaSharp/pull/3772))

## Bug Fixes

- **Pixel span access corrected** — GetPixelSpan on SKPixmap and SKBitmap now uses RowBytes for the stride and applies x/y offsets against the correct axis, so multi-row and offset reads return the intended pixels. ([#4148](https://github.com/mono/SkiaSharp/pull/4148), [#4128](https://github.com/mono/SkiaSharp/pull/4128))
- **SKPath finalizer crash fixed** — An intermittent crash that happened when an SKPathBuilder was collected before the SKPath it produced no longer occurs during finalization. — ❤️ [@ramezgerges](https://github.com/ramezgerges) ([#3796](https://github.com/mono/SkiaSharp/pull/3796))

## Lifecycle & Internals

- **Singleton lifecycle rework** — Shared singleton instances now follow a deterministic teardown-and-recreate model, closing a long-standing class of resurrection bugs. See the breaking-change note above for the migration. — ❤️ [@ramezgerges](https://github.com/ramezgerges) ([#4080](https://github.com/mono/SkiaSharp/pull/4080))

## Platform

- **WinUI projection loads under .NET 9** — Fixed a missing native DLL that prevented the WinUI projection from loading for .NET 9 consumers. ([#4084](https://github.com/mono/SkiaSharp/pull/4084))

## Security

- **Expat refreshed to 2.8.1** — The bundled Expat XML parser is updated to 2.8.1, picking up the latest upstream security fixes. ([#4079](https://github.com/mono/SkiaSharp/pull/4079))

## Community Contributors ❤️

Thank you to everyone who contributed to this release!

| Contributor | Contributions |
|-------------|---------------|
| [@ramezgerges](https://github.com/ramezgerges) | Singleton lifecycle rework, promoting the legacy SKPaint text and font APIs to errors, the SKPath finalizer crash fix, and Uno Platform sample updates. ([#4080](https://github.com/mono/SkiaSharp/pull/4080), [#4068](https://github.com/mono/SkiaSharp/pull/4068), [#3796](https://github.com/mono/SkiaSharp/pull/3796), [#3867](https://github.com/mono/SkiaSharp/pull/3867), [#3821](https://github.com/mono/SkiaSharp/pull/3821)) |
| [@ebariche](https://github.com/ebariche) | Cut the SkiaFiddle sample's WASM download size by roughly 60%. ([#3849](https://github.com/mono/SkiaSharp/pull/3849)) |
| [@sasakrsmanovic](https://github.com/sasakrsmanovic) | Refreshed the Uno Platform link and description in the README. ([#3966](https://github.com/mono/SkiaSharp/pull/3966)) |

## Links

- [SkiaSharp on NuGet](https://www.nuget.org/packages/SkiaSharp/4.148.0)
- [GitHub release](https://github.com/mono/SkiaSharp/releases/tag/v4.148.0)
- [Full changelog](https://github.com/mono/SkiaSharp/compare/v3.119.4...v4.148.0)

## Release Candidate 1 (June 12, 2026)

Release Candidate 1 finalised the Skia m148 update and the Expat 2.8.1 refresh, and shipped the singleton lifecycle rework alongside the SKPaint obsolete-to-error promotion and the SKPixmap stride fix.

[Full changelog](https://github.com/mono/SkiaSharp/compare/v4.147.0-preview.3.1...v4.148.0-rc.1.2)

## Preview 2 (May 6, 2026)

Preview 2 added animated WebP encoding and the zero-copy SKStream.GetData() helper, and fixed the SKPath finalizer crash reported against SKPathBuilder.

[Full changelog](https://github.com/mono/SkiaSharp/compare/v4.147.0-preview.1.1...v4.147.0-preview.2.1)

## Preview 1 (April 28, 2026)

Preview 1 opened the v4.148 line and pointed the Uno Platform Gallery sample at the new binding references.

[Full changelog](https://github.com/mono/SkiaSharp/compare/v3.119.4...v4.147.0-preview.1.1)
