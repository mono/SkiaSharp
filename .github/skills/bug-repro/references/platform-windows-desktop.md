# Platform: Windows Desktop

Reproduce bugs in WPF, WinForms, WinUI, or UWP applications using SkiaSharp.

**⚠️ Requires Windows host.** If the current host is macOS or Linux, try the console
fallback first — many "WPF bugs" are actually core SkiaSharp bugs reproducible anywhere.

## Signals

WPF, WinForms, WinUI 3, UWP, ClickOnce, XPS, GDI+, `SkiaSharp.Views.WPF`,
`SkiaSharp.Views.WindowsForms`, `SKElement`, `SKXamlCanvas`, Windows-specific HWND,
`System.Windows`, `System.Drawing` interop.

## Prerequisites

- Windows host with .NET SDK
- Windows desktop workload: `dotnet workload install windowsdesktop` (if needed)
- For WinUI: Windows App SDK

## Strategy When Host is NOT Windows

1. **Try console fallback first.** Read `platform-console.md` and attempt a console
   reproduction. If the bug is in core SkiaSharp (e.g., wrong pixel values, codec errors,
   path calculations), it will reproduce in a console app regardless of the UI framework.

2. **Try Docker Linux.** Read `platform-docker-linux.md` — some bugs manifest on both
   Windows and Linux (e.g., native library loading, font resolution).

3. **If neither works:** Record `needs-platform` with blocker:
   "Requires Windows host — bug is in WPF/WinForms view layer, not reproducible via console or Docker."

## Create Project (Windows host only)

```bash
# WPF
dotnet new wpf -n Repro --framework {reporter_tfm}
cd Repro
dotnet add package SkiaSharp.Views.WPF --version {reporter_version}

# WinForms
dotnet new winforms -n Repro --framework {reporter_tfm}
cd Repro
dotnet add package SkiaSharp.Views.WindowsForms --version {reporter_version}
```

## Add Repro Code

- For WPF: Add `<skia:SKElement PaintSurface="OnPaintSurface" />` to XAML
- For WinForms: Add `SKControl` to form, wire up `PaintSurface`
- Paste reporter's drawing code in the event handler

## Build & Run

```bash
dotnet build
dotnet run
```

WPF/WinForms apps open a window — visual inspection or programmatic assertions required.

## Conclusion Mapping

| Observation | Conclusion |
|------------|------------|
| Bug reproduced on console (no Windows needed) | `reproduced` (platform-console) |
| Bug requires Windows UI layer | `needs-platform` (if host is not Windows) |
| Bug reproduced on Windows host | `reproduced` |

## Main Source Testing (Phase 3C)

For Windows desktop bugs, Phase 3C uses the repo's WPF or WinUI sample:

```bash
# Back in the SkiaSharp repo
cd /Users/matthew/Documents/GitHub/SkiaSharp-2-worktrees/main
[ -d "output/native" ] && ls output/native/ | head -5 || dotnet cake --target=externals-download

# WPF sample (project references local SkiaSharp + Views.WPF source)
dotnet build samples/Basic/WPF/SkiaSharpSample/SkiaSharpSample.csproj
dotnet run --project samples/Basic/WPF/SkiaSharpSample/SkiaSharpSample.csproj

# WinUI sample
dotnet build samples/Basic/WinUI/SkiaSharpSample/SkiaSharpSample.csproj
dotnet run --project samples/Basic/WinUI/SkiaSharpSample/SkiaSharpSample.csproj
```

**If host is not Windows:** You can still BUILD these samples (to verify compilation). Run
requires Windows — record `result: "not-tested"` for runtime if no Windows host available,
but record `result: "success"` / `"failure"` for the build step.

Temporarily modify the sample's drawing code to match the reporter's repro, then revert
with `git checkout` after recording the result.
