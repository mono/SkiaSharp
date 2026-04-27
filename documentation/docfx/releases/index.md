# Release Notes

Release notes for all SkiaSharp versions. Each page includes the stable release and all associated preview releases.

## What's Coming Next

<!-- UNRELEASED_BEGIN -->

Typography gets a major upgrade with full variable font support and color font palettes, while the core API gains finer sampling control for surface drawing. Community contributor [@ramezgerges](https://github.com/ramezgerges) has been instrumental in driving these font and platform improvements.

### 🎨 Core API

- **SKSamplingOptions for surface drawing** — `SKSurface.Draw` and `SKCanvas.DrawSurface` now accept `SKSamplingOptions` for finer control over image sampling quality. ❤️ ([#3491](https://github.com/mono/SkiaSharp/pull/3491))

### 📦 Text & Fonts

- **Variable font support** — Full variable font axis support including weight, width, slant, and custom axes. ❤️ @ramezgerges ([#3703](https://github.com/mono/SkiaSharp/pull/3703))
- **Color font palette support** — Added color font (COLR/CPAL) palette selection and improved variable font robustness. ([#3742](https://github.com/mono/SkiaSharp/pull/3742))
- **Default typeface resolution fix** — Moved default-typeface resolution to the managed layer, fixing incorrect fallback behavior. ❤️ @ramezgerges ([#3730](https://github.com/mono/SkiaSharp/pull/3730))
- **Legacy typeface creation** — Bumped Skia to include `sk_fontmgr_legacy_create_typeface` for improved font manager support. ([#3744](https://github.com/mono/SkiaSharp/pull/3744))

### 🐧 Linux

- **Debian 13 Docker build fix** — Fixed Docker build for non-loong64/riscv64 architectures on Debian 13. ([#3747](https://github.com/mono/SkiaSharp/pull/3747))

### 🌐 WebAssembly

- **Uno Platform WebAssembly gallery** — New sample project demonstrating SkiaSharp with Uno Platform on WebAssembly. ❤️ @ramezgerges ([#3758](https://github.com/mono/SkiaSharp/pull/3758))

### 📦 General

- **Uno Platform sample update** — Updated existing Uno Platform sample project. ❤️ @ramezgerges ([#3666](https://github.com/mono/SkiaSharp/pull/3666))
- **Threading test fix** — Fixed x86 .NET Framework threading test OOM failures. ([#3674](https://github.com/mono/SkiaSharp/pull/3674))

Plus several CI, documentation, and tooling improvements.

<!-- UNRELEASED_END -->

## All Versions

### SkiaSharp 3.x

- [Version 3.119.4](3.119.4.md)
- [Version 3.119.3](3.119.3.md)
- [Version 3.119.2](3.119.2.md)
- [Version 3.119.1](3.119.1.md)
- [Version 3.119.0](3.119.0.md)
- [Version 3.118.0](3.118.0.md)
- [Version 3.116.1](3.116.1.md)
- [Version 3.116.0](3.116.0.md)
- [Version 3.0.0](3.0.0.md)

### SkiaSharp 2.x

- [Version 2.88.9](2.88.9.md)
- [Version 2.88.8](2.88.8.md)
- [Version 2.88.7](2.88.7.md)
- [Version 2.88.6](2.88.6.md)
- [Version 2.88.5](2.88.5.md)
- [Version 2.88.4](2.88.4.md)
- [Version 2.88.3](2.88.3.md)
- [Version 2.88.2](2.88.2.md)
- [Version 2.88.1](2.88.1.md)
- [Version 2.88.0](2.88.0.md)
- [Version 2.80.4](2.80.4.md)
- [Version 2.80.3](2.80.3.md)
- [Version 2.80.2](2.80.2.md)
- [Version 2.80.1](2.80.1.md)
- [Version 2.80.0](2.80.0.md)

### SkiaSharp 1.x

- [Version 1.68.3](1.68.3.md)
- [Version 1.68.2.1](1.68.2.1.md)
- [Version 1.68.2](1.68.2.md)
- [Version 1.68.1.1](1.68.1.1.md)
- [Version 1.68.1](1.68.1.md)
- [Version 1.68.0](1.68.0.md)
- [Version 1.60.3](1.60.3.md)
- [Version 1.60.2](1.60.2.md)
- [Version 1.60.1](1.60.1.md)
- [Version 1.60.0](1.60.0.md)
- [Version 1.59.3](1.59.3.md)
- [Version 1.59.2](1.59.2.md)
- [Version 1.59.1.1](1.59.1.1.md)
- [Version 1.59.1](1.59.1.md)
- [Version 1.59.0](1.59.0.md)
- [Version 1.58.1.1](1.58.1.1.md)
- [Version 1.58.1](1.58.1.md)
- [Version 1.58.0](1.58.0.md)
- [Version 1.57.1](1.57.1.md)
- [Version 1.57.0](1.57.0.md)
- [Version 1.56.2](1.56.2.md)
- [Version 1.56.1](1.56.1.md)
- [Version 1.56.0](1.56.0.md)
- [Version 1.55.1](1.55.1.md)
- [Version 1.55.0](1.55.0.md)
- [Version 1.54.1](1.54.1.md)
- [Version 1.54.0](1.54.0.md)
- [Version 1.53.2](1.53.2.md)
- [Version 1.53.1.2](1.53.1.2.md)
- [Version 1.53.1.1](1.53.1.1.md)
- [Version 1.53.1](1.53.1.md)
- [Version 1.53.0](1.53.0.md)
- [Version 1.49.4](1.49.4.md)
- [Version 1.49.3](1.49.3.md)
- [Version 1.49.2.1](1.49.2.1.md)
- [Version 1.49.2](1.49.2.md)
- [Version 1.49.1](1.49.1.md)
- [Version 1.49.0](1.49.0.md)
