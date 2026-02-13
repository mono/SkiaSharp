# Platform: Mobile (iOS / Android / MAUI)

Reproduce bugs in iOS, Android, or .NET MAUI applications using SkiaSharp.

**⚠️ Requires device or emulator.** If unavailable, try the console fallback first —
many mobile-reported bugs are actually core SkiaSharp bugs reproducible anywhere.

## Signals

iOS, Android, .NET MAUI, Xamarin, Xamarin.Forms, `SkiaSharp.Views.Maui.Controls`,
`SkiaSharp.Views.iOS`, `SkiaSharp.Views.Android`, `SKCanvasView` (mobile context),
`SKGLView`, mobile-specific crashes, touch events, device-specific rendering.

## Prerequisites

- For iOS: macOS host + Xcode + iOS simulator
- For Android: Android SDK + emulator (any host)
- For MAUI: `dotnet workload install maui`

## Strategy When Devices Are Unavailable

1. **Try console fallback first.** Read `platform-console.md`. If the bug is in core
   SkiaSharp (e.g., SKBitmap operations, SKPath calculations, codec bugs), it will
   reproduce in a console app.

2. **Try Docker Linux.** Read `platform-docker-linux.md` — useful for bugs in the
   shared native layer.

3. **Build-only test.** Even without a device, you can verify the project BUILDS:
   ```bash
   dotnet build -f net8.0-android   # Android
   dotnet build -f net8.0-ios       # iOS (macOS only)
   ```
   Build failures are valid reproductions if the reporter says "build fails."

4. **If none work:** Record `needs-platform` with blocker:
   "Requires iOS simulator / Android emulator — bug is in mobile view layer."

## Create Project (when device/emulator available)

```bash
dotnet new maui -n Repro
cd Repro
dotnet add package SkiaSharp.Views.Maui.Controls --version {reporter_version}
```

Register SkiaSharp in `MauiProgram.cs`:
```csharp
.UseSkiaSharp()
```

## Add Repro Code

Add `SKCanvasView` to a XAML page and wire up `PaintSurface`.
Paste reporter's code in the event handler.

## Build & Run

```bash
# Android emulator
dotnet build -f net8.0-android -t:Run

# iOS simulator (macOS only)
dotnet build -f net8.0-ios -t:Run
```

## Conclusion Mapping

| Observation | Conclusion |
|------------|------------|
| Bug reproduced on console (no device needed) | `reproduced` (platform-console) |
| Bug requires device/emulator | `needs-platform` (if unavailable) |
| Build failure matching report | `reproduced` |
| Bug reproduced on device | `reproduced` |

## Main Source Testing (Phase 3C)

For mobile bugs, Phase 3C uses the repo's platform-specific sample:

```bash
# Return to the SkiaSharp repo root
cd "$(git rev-parse --show-toplevel)"
[ -d "output/native" ] && ls output/native/ | head -5 || dotnet cake --target=externals-download

# iOS sample (macOS host required)
dotnet build samples/Basic/iOS/SkiaSharpSample/SkiaSharpSample.csproj

# Android sample (any host with Android SDK)
dotnet build samples/Basic/Android/SkiaSharpSample/SkiaSharpSample.csproj

# Mac Catalyst sample
dotnet build samples/Basic/MacCatalyst/SkiaSharpSample/SkiaSharpSample.csproj

# MAUI multi-platform sample
dotnet build samples/Basic/Maui/SkiaSharpSample/SkiaSharpSample.csproj -f net8.0-ios
dotnet build samples/Basic/Maui/SkiaSharpSample/SkiaSharpSample.csproj -f net8.0-android
```

Build success alone is a useful data point. If a simulator/emulator is available, also run:
```bash
dotnet build -f net8.0-ios -t:Run samples/Basic/iOS/SkiaSharpSample/SkiaSharpSample.csproj
dotnet build -f net8.0-android -t:Run samples/Basic/Android/SkiaSharpSample/SkiaSharpSample.csproj
```

Temporarily modify the sample's drawing code to match the reporter's repro, then revert
with `git checkout` after recording the result.
