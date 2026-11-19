# Compilation Symbols: WinUI, UWP and Uno Views

Symbols defined for each WinUI, UWP and Uno view build.

## Builds

| Column | Project | Target framework |
|--------|---------|------------------|
| WinUI | `SkiaSharp.Views.WinUI` | `net*-windows10.0.19041.0` |
| UWP | `SkiaSharp.Views.UWP` | `net10.0-windows10.0.26100.0` |
| Uno ref | `SkiaSharp.Views.Uno.WinUI` | `net*` |
| Uno iOS | `SkiaSharp.Views.Uno.WinUI` | `net*-ios` |
| Uno Catalyst | `SkiaSharp.Views.Uno.WinUI` | `net*-maccatalyst` |
| Uno macOS | `SkiaSharp.Views.Uno.WinUI` | `net*-macos` |
| Uno Android | `SkiaSharp.Views.Uno.WinUI` | `net*-android` |
| Uno Skia | `SkiaSharp.Views.Uno.WinUI.Skia` | `net*` |
| Uno Wasm | `SkiaSharp.Views.Uno.WinUI.Wasm` | `net*` |

`SkiaSharp.Views.Uno.WinUI` for `net*-windows` compiles no sources; Uno apps on Windows use `SkiaSharp.Views.WinUI`.

## Symbols

| Symbol | WinUI | UWP | Uno ref | Uno iOS | Uno Catalyst | Uno macOS | Uno Android | Uno Skia | Uno Wasm | Defined by |
|--------|:-----:|:---:|:-------:|:-------:|:------------:|:---------:|:-----------:|:--------:|:--------:|------------|
| `WINDOWS` | ✓ | ✓ | | | | | | | | .NET SDK |
| `WINUI` | ✓ | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | Project |
| `WINDOWS_UWP` | | ✓ | | | | | | | | Project |
| `HAS_UNO` | | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | Uno.WinUI package |
| `HAS_UNO_WINUI` | | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | Uno.WinUI package |
| `UNO_REFERENCE_API` | | | ✓ | | | | | ✓ | ✓ | Uno.WinUI package |
| `__WASM__` | | | | | | | | | ✓ | Project |
| `MACCATALYST` | | | | | ✓ | | | | | .NET SDK |
| `__ANDROID__` | | | | | | | ✓ | | | Android workload |
| `__IOS__` | | | | ✓ | ✓ | | | | | iOS workload |
| `__MACCATALYST__` | | | | | ✓ | | | | | iOS workload |
| `__MACOS__` | | | | | | ✓ | | | | macOS workload |

## Pitfalls

- `WINUI` does not identify `SkiaSharp.Views.WinUI`; every Uno build defines it. Use `WINUI && WINDOWS`.
- `WINDOWS` includes UWP.
- `__IOS__` is also defined on Mac Catalyst.
