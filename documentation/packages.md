# NuGet Packages

Reference for all NuGet packages produced by SkiaSharp — purpose, contents, and usage guidance.

> **Related:** [Adding Libraries](adding-libraries.md) | [Architecture](architecture.md) | [Linux Assets](linux-assets.md)

## Contents

- [Core Managed Packages](#core-managed-packages) — SkiaSharp, HarfBuzzSharp, and extensions
- [Native Assets](#native-assets) — Platform-specific native binaries for SkiaSharp and HarfBuzzSharp
  - [Platform Packages](#platform-packages) — Per-platform package details
  - [Auto-Included NativeAssets](#auto-included-nativeassets) — What's pulled in automatically by TFM
  - [Linux Package Selection Guide](#linux-package-selection-guide) — Which Linux package to use
- [Views Packages](#views-packages) — UI controls for each platform
- [GPU Backend Packages](#gpu-backend-packages) — Vulkan and Direct3D support
- [Deployment & Containers](#deployment--containers) — Container and publishing guidance
  - [Container Deployment](#container-deployment) — Common issues and fixes
  - [Publishing Modes](#publishing-modes) — Framework-dependent, self-contained, single-file
- [Obsolete Packages](#obsolete-packages) — Deprecated, do not use

---

## Core Managed Packages

These contain the managed C# assemblies. The `SkiaSharp` and `HarfBuzzSharp` core packages automatically include the appropriate NativeAssets for most platforms (see [Auto-Included NativeAssets](#auto-included-nativeassets)). For Linux, WebAssembly, NanoServer, and WinUI you must add the NativeAssets package manually.

| Package | Description |
|---------|-------------|
| **SkiaSharp** | Cross-platform 2D graphics API. Managed C# bindings to Google's Skia library. Core types: `SKCanvas`, `SKPaint`, `SKBitmap`, `SKImage`, `SKPath`, `SKSurface`. Includes .NET Interactive support (embedded DLL for Polyglot Notebooks). |
| **HarfBuzzSharp** | Managed C# bindings to the HarfBuzz text shaping engine. Provides `Buffer`, `Font`, `Face` for complex text layout. |
| **SkiaSharp.HarfBuzz** | Integration bridge — adds HarfBuzz text shaping to SkiaSharp via `SKShaper`. Depends on both SkiaSharp and HarfBuzzSharp. |
| **SkiaSharp.Skottie** | Lottie/Bodymovin animation playback. Renders After Effects animations. Depends on SkiaSharp, SkiaSharp.SceneGraph, and SkiaSharp.Resources. |
| **SkiaSharp.SceneGraph** | Scene graph API for complex rendering pipelines. Depends on SkiaSharp. Used by Skottie internally. |
| **SkiaSharp.Resources** | Resource provider implementations. Depends on SkiaSharp. Used by Skottie for asset loading. |

---

## Native Assets

Each platform has a pair of NativeAssets packages containing pre-built native binaries:

- `SkiaSharp.NativeAssets.{Platform}` — contains `libSkiaSharp`
- `HarfBuzzSharp.NativeAssets.{Platform}` — contains `libHarfBuzzSharp`

Both follow the same platform matrix and architectures. HarfBuzzSharp does **not** have Linux.NoDependencies, NanoServer, or WinUI variants.

> **Important:** Library projects should depend on `SkiaSharp` only. NativeAssets packages belong in the **application** (executable) project so the correct binary is deployed. See [Deployment & Containers](#deployment--containers).

### Platform Packages

| Package | Description |
|---------|-------------|
| **SkiaSharp.NativeAssets.Win32**<br/>**HarfBuzzSharp.NativeAssets.Win32** | Windows (x64, x86, arm64). Auto-included. |
| **SkiaSharp.NativeAssets.macOS**<br/>**HarfBuzzSharp.NativeAssets.macOS** | macOS universal binary (Intel + Apple Silicon). Auto-included. |
| **SkiaSharp.NativeAssets.Linux**<br/>**HarfBuzzSharp.NativeAssets.Linux** | Linux (x64, x86, arm, arm64, riscv64, loongarch64). Both glibc and musl (Alpine) variants. **Must add manually.** Requires fontconfig (`libfontconfig.so.1`) for system font enumeration. |
| **SkiaSharp.NativeAssets.Linux.NoDependencies** | Linux (same architectures as above, glibc + musl). **SkiaSharp only. Must add manually.** No fontconfig, no third-party deps — only requires libc/libm/libpthread/libdl. **Designed for minimal containers.** Fonts must be loaded explicitly. |
| **SkiaSharp.NativeAssets.NanoServer** | Windows Nano Server containers (x64 only). **SkiaSharp only. Must add manually.** |
| **SkiaSharp.NativeAssets.WinUI** | ANGLE rendering surface for WinUI 3 apps (x64, x86, arm64). **SkiaSharp only.** Contains `SkiaSharp.Views.WinUI.Native.dll`, `libEGL.dll`, and `libGLESv2.dll` for hardware-accelerated OpenGL ES — this is **not** a Skia binary. Auto-included by SkiaSharp.Views.WinUI. |
| **SkiaSharp.NativeAssets.Android**<br/>**HarfBuzzSharp.NativeAssets.Android** | Android (x86, x64, arm, arm64). Auto-included. |
| **SkiaSharp.NativeAssets.iOS**<br/>**HarfBuzzSharp.NativeAssets.iOS** | iOS framework bundle (arm64 device + simulator). Auto-included. |
| **SkiaSharp.NativeAssets.MacCatalyst**<br/>**HarfBuzzSharp.NativeAssets.MacCatalyst** | Mac Catalyst universal framework bundle. Auto-included. |
| **SkiaSharp.NativeAssets.tvOS**<br/>**HarfBuzzSharp.NativeAssets.tvOS** | Apple tvOS framework bundle (arm64, device only). Auto-included. |
| **SkiaSharp.NativeAssets.Tizen**<br/>**HarfBuzzSharp.NativeAssets.Tizen** | Samsung Tizen (armel, x86). Auto-included. |
| **SkiaSharp.NativeAssets.WebAssembly**<br/>**HarfBuzzSharp.NativeAssets.WebAssembly** | Emscripten static library (`.a`). **Static linking, not P/Invoke** — linked into `dotnet.wasm` at build time via MSBuild `NativeFileReference`. Includes multiple Emscripten versions with threading (st/mt) and SIMD variants. **Must add manually** (or use SkiaSharp.Views.Blazor / SkiaSharp.Views.Uno.WinUI). |

> **⚠️ WASM is NOT dynamic loading.** Unlike all other platforms, WebAssembly uses static linking at compile time. The `.a` files are passed to the Emscripten linker which embeds them into the final `dotnet.wasm` binary. `DllImport("libSkiaSharp")` still works — the .NET WASM runtime resolves it to statically linked symbols.

> **Unity WebGL:** Unity WebGL also uses Emscripten. While not officially supported, the same static linking principle applies. Unity-specific integration may require manual configuration beyond the standard NativeFileReference approach.

### Auto-Included NativeAssets

The core `SkiaSharp` and `HarfBuzzSharp` packages automatically include NativeAssets for most platforms via TFM-conditional dependencies:

| Packages | Auto-included when targeting |
|----------|------------------------------|
| SkiaSharp.NativeAssets.Win32<br/>HarfBuzzSharp.NativeAssets.Win32 | Windows TFM (`net8.0-windows`) or non-platform TFM (`net6.0`, `net8.0`, `netstandard2.0`, `netstandard2.1`, `net462`) |
| SkiaSharp.NativeAssets.macOS<br/>HarfBuzzSharp.NativeAssets.macOS | macOS TFM (`net8.0-macos`) or non-platform TFM (`net6.0`, `net8.0`, `netstandard2.0`, `netstandard2.1`, `net462`) |
| SkiaSharp.NativeAssets.Android<br/>HarfBuzzSharp.NativeAssets.Android | Android TFM (`net8.0-android`) |
| SkiaSharp.NativeAssets.iOS<br/>HarfBuzzSharp.NativeAssets.iOS | iOS TFM (`net8.0-ios`) |
| SkiaSharp.NativeAssets.MacCatalyst<br/>HarfBuzzSharp.NativeAssets.MacCatalyst | Mac Catalyst TFM (`net8.0-maccatalyst`) |
| SkiaSharp.NativeAssets.tvOS<br/>HarfBuzzSharp.NativeAssets.tvOS | tvOS TFM (`net8.0-tvos`) |
| SkiaSharp.NativeAssets.Tizen<br/>HarfBuzzSharp.NativeAssets.Tizen | Tizen TFM (`net8.0-tizen`) |

**Must be added manually** (not auto-included):

- `SkiaSharp.NativeAssets.Linux` / `HarfBuzzSharp.NativeAssets.Linux` — for Linux server/desktop
- `SkiaSharp.NativeAssets.Linux.NoDependencies` — for minimal Linux containers (SkiaSharp only)
- `SkiaSharp.NativeAssets.WebAssembly` / `HarfBuzzSharp.NativeAssets.WebAssembly` — for Blazor/Uno WASM. `SkiaSharp.NativeAssets.WebAssembly` is auto-included by `SkiaSharp.Views.Blazor` and `SkiaSharp.Views.Uno.WinUI`; `HarfBuzzSharp.NativeAssets.WebAssembly` must always be added manually.
- `SkiaSharp.NativeAssets.NanoServer` — for Windows Nano Server (SkiaSharp only)
- `SkiaSharp.NativeAssets.WinUI` — for WinUI 3 apps (SkiaSharp only; auto-included by `SkiaSharp.Views.WinUI`)

### Linux Package Selection Guide

| Scenario | Package | Why |
|----------|---------|-----|
| Standard Linux server/desktop | `SkiaSharp.NativeAssets.Linux` | Full fontconfig integration for system font access |
| Docker containers (Debian/Ubuntu) | `SkiaSharp.NativeAssets.Linux.NoDependencies` | No system library dependencies beyond glibc |
| Alpine Docker containers | `SkiaSharp.NativeAssets.Linux.NoDependencies` | Includes `linux-musl-*` variants, no deps |
| Minimal/distroless containers | `SkiaSharp.NativeAssets.Linux.NoDependencies` | Zero third-party deps |
| App needs system font enumeration | `SkiaSharp.NativeAssets.Linux` | Fontconfig required for `SKFontManager` system fonts |

---

## Views Packages

Platform-specific UI controls for rendering SkiaSharp content. These provide ready-to-use views/controls that handle the platform-specific rendering surface setup.

| Package | Description |
|---------|-------------|
| **SkiaSharp.Views** | Platform views for iOS, tvOS, macOS, Mac Catalyst, Android, and Tizen. Provides `SKCanvasView` and `SKGLView`. |
| **SkiaSharp.Views.Desktop.Common** | Common base classes for desktop views (netstandard2.0+, net462+). |
| **SkiaSharp.Views.WindowsForms** | Windows Forms controls: `SKControl`, `SKGLControl`. net462+ and net6.0-windows+. Depends on OpenTK. |
| **SkiaSharp.Views.WPF** | WPF control: `SKElement`. net462+ and net6.0-windows+. Depends on OpenTK. |
| **SkiaSharp.Views.WinUI** | WinUI 3 controls: `SKXamlCanvas`, `SKSwapChainPanel`. net6.0-windows+. Depends on Microsoft.WindowsAppSDK. Auto-includes SkiaSharp.NativeAssets.WinUI. |
| **SkiaSharp.Views.Gtk3** | GTK# 3 control: `SKDrawingArea`. netstandard2.0+. For Linux desktop apps. Depends on GtkSharp. |
| **SkiaSharp.Views.Blazor** | Blazor WebAssembly controls: `SKCanvasView`, `SKGLView`. net6.0+. Auto-includes SkiaSharp.NativeAssets.WebAssembly. |
| **SkiaSharp.Views.Maui.Core** | .NET MAUI shared view infrastructure. net8.0+. Depends on Microsoft.Maui.Core. |
| **SkiaSharp.Views.Maui.Controls** | .NET MAUI controls: `SKCanvasView`, `SKGLView`. net8.0+. Depends on SkiaSharp.Views.Maui.Core and Microsoft.Maui.Controls. |
| **SkiaSharp.Views.Uno.WinUI** | Uno Platform controls: `SKXamlCanvas`, `SKSwapChainPanel`. net8.0+. Depends on Uno.WinUI. Auto-includes SkiaSharp.NativeAssets.WebAssembly for WASM targets and SkiaSharp.Views.WinUI for Windows targets. |

---

## GPU Backend Packages

Optional packages for hardware-accelerated rendering via specific GPU APIs.

| Package | Description |
|---------|-------------|
| **SkiaSharp.Vulkan.SharpVk** | Vulkan GPU backend. netstandard2.0+, net462+, net6.0+. Depends on [SharpVk](https://github.com/FacticiusVir/SharpVk) 0.4.2. |
| **SkiaSharp.Direct3D.Vortice** | Direct3D 12 GPU backend. net8.0 only. Windows-only due to [Vortice.Direct3D12](https://github.com/amerkoleci/Vortice.Windows) 3.5.0 dependency. |

---

## Deployment & Containers

### Application vs Library References

NativeAssets packages must be referenced in the **application project** (the one that produces the executable), not in library projects. The .NET runtime resolves native binaries from the application's output directory using runtime identifiers (RIDs).

If a NativeAssets package is only referenced in a transitive library, the native binary may not be copied to the final output — causing `DllNotFoundException` at runtime.

### Container Deployment

For containers, use `SkiaSharp.NativeAssets.Linux.NoDependencies` unless you specifically need fontconfig for system font enumeration. This package has zero third-party dependencies and works in minimal base images (`mcr.microsoft.com/dotnet/aspnet`, Alpine, distroless).

Common container deployment issues:

| Problem | Cause | Fix |
|---------|-------|-----|
| `DllNotFoundException: libSkiaSharp` | Native binary not in output | Ensure `SkiaSharp.NativeAssets.Linux.NoDependencies` (or `.Linux`) is a direct `PackageReference` in the application project |
| `libfontconfig.so.1: cannot open` | Using `SkiaSharp.NativeAssets.Linux` in minimal container | Switch to `SkiaSharp.NativeAssets.Linux.NoDependencies` or install fontconfig in Dockerfile |
| Wrong binary for container arch | RID mismatch (glibc vs musl) | Alpine needs `linux-musl-*` RIDs — `SkiaSharp.NativeAssets.Linux.NoDependencies` includes both variants |
| Trimming removes transitive deps | .NET trimmer strips unused assemblies | Add missing assembly as a direct `PackageReference` |

### Publishing Modes

| Mode | Native Binary Handling |
|------|----------------------|
| Framework-dependent | Binaries in `runtimes/{rid}/native/` resolved at runtime |
| Self-contained | Binaries copied to publish output for the specified RID |
| Single-file | Binaries extracted alongside the executable at runtime |

---

## Obsolete Packages

These packages are no longer actively maintained. Use the listed replacement.

| Package | Replacement |
|---------|-------------|
| SkiaSharp.NativeAssets.UWP | SkiaSharp.NativeAssets.WinUI |
| SkiaSharp.NativeAssets.watchOS | *(none — watchOS not supported)* |
| HarfBuzzSharp.NativeAssets.UWP | HarfBuzzSharp.NativeAssets.Win32 |
| HarfBuzzSharp.NativeAssets.watchOS | *(none)* |
| SkiaSharp.Views.NativeAssets.UWP | SkiaSharp.Views.WinUI |
| SkiaSharp.Views.Forms | SkiaSharp.Views.Maui.Controls |
| SkiaSharp.Views.Forms.WPF | SkiaSharp.Views.WPF |
| SkiaSharp.Views.Forms.GTK | SkiaSharp.Views.Gtk3 |
| SkiaSharp.Views.Gtk2 | SkiaSharp.Views.Gtk3 |
| SkiaSharp.Views.Uno | SkiaSharp.Views.Uno.WinUI |
| SkiaSharp.Views.Maui.Controls.Compatibility | SkiaSharp.Views.Maui.Controls |

## Related Documentation

- [Adding Libraries](adding-libraries.md) — Creating new packages
- [Architecture](architecture.md) — Three-layer design
- [Linux Assets](linux-assets.md) — Linux package design philosophy
- [Dependencies](dependencies.md) — Native dependency tracking
