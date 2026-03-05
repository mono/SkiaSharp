# Platform: macOS GUI Application

For bugs involving macOS rendering, views, GPU backends, or any macOS-specific UI behavior.

## Contents
1. [Signals](#signals) — [Prerequisites](#prerequisites) — [Create Project](#create-project)
2. [macOS App Setup Gotchas](#macos-app-setup-gotchas) — [Backend-Specific Templates](#backend-specific-templates)
3. [Build](#build) — [Run & Verify](#run--verify) — [Iterate](#iterate)
4. [Conclusion Mapping](#conclusion-mapping) — [Main Source Testing (Phase 3C)](#main-source-testing-phase-3c)
5. [Pitfalls](#pitfalls)

## Signals

macOS, Mac Catalyst, `SKGLView`, `SKMetalView`, `NSOpenGLView`, `MTKView`, macOS crash, macOS visual defect, macOS rendering, macOS-specific, `net*-macos`, Mac GPU, OpenGL on Mac, Metal on Mac, macOS window, AppKit.

Also use when triage JSON has `classification.platforms` containing `os/macOS` or `classification.backends` containing `backend/Metal` or `backend/OpenGL`.

## Prerequisites

- .NET SDK (10.0+ recommended)
- macOS workload: `dotnet workload install macos`
- Xcode Command Line Tools: `xcode-select --install`
- For native baseline comparison (performance bugs): `brew install cmake ninja`

## Create Project

```bash
mkdir -p /tmp/skiasharp/repro/{number} && cd /tmp/skiasharp/repro/{number}
dotnet new macos -n Repro --framework net10.0-macos
cd Repro
dotnet add package SkiaSharp --version {reporter_version}
dotnet add package SkiaSharp.Views.Maui.Controls --version {reporter_version}  # If using MAUI views
# OR for direct macOS views:
dotnet add package SkiaSharp.Views --version {reporter_version}
```

**Required `.csproj` settings** (many macOS apps fail without these):

```xml
<PropertyGroup>
  <TargetFramework>net10.0-macos</TargetFramework>
  <ApplicationId>com.skiasharp.repro</ApplicationId>
  <SupportedOSPlatformVersion>12.0</SupportedOSPlatformVersion>
  <RuntimeIdentifier>osx-arm64</RuntimeIdentifier>
</PropertyGroup>
```

## macOS App Setup Gotchas

These are critical — a macOS .NET app won't show a window without them:

1. **Application activation** — Required for the window to appear:
```csharp
NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Regular;
NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
```

2. **Run the binary directly** — Do NOT use `open -n AppName.app`. The `open` command doesn't pipe stdout, so you won't see console output:
```bash
# ✅ CORRECT — see stdout
./bin/Release/net10.0-macos/osx-arm64/Repro.app/Contents/MacOS/Repro

# ❌ WRONG — no stdout capture
open -n ./bin/Release/net10.0-macos/osx-arm64/Repro.app
```

3. **Window creation** — Minimal AppDelegate pattern:
```csharp
[Register("AppDelegate")]
public class AppDelegate : NSApplicationDelegate
{
    private NSWindow? _window;

    public override void DidFinishLaunching(NSNotification notification)
    {
        _window = new NSWindow(
            new CGRect(100, 100, 800, 600),
            NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Resizable,
            NSBackingStore.Buffered, false);
        _window.Title = "SkiaSharp Repro";

        // Add your SKCanvasView / SKGLView / SKMetalView here
        // _window.ContentView = new SKCanvasView(frame);

        _window.MakeKeyAndOrderFront(null);
    }
}
```

## Backend-Specific Templates

### Metal Backend (SKMetalView)
```csharp
using SkiaSharp.Views.Mac;

var metalView = new SKMetalView(frame);
metalView.PaintSurface += (s, e) =>
{
    var canvas = e.Surface.Canvas;
    canvas.Clear(SKColors.White);
    // Reporter's drawing code here
};
_window.ContentView = metalView;
```

### OpenGL Backend (SKGLView)
```csharp
using SkiaSharp.Views.Mac;

var glView = new SKGLView(frame);
glView.PaintSurface += (s, e) =>
{
    var canvas = e.Surface.Canvas;
    canvas.Clear(SKColors.White);
    // Reporter's drawing code here
};
_window.ContentView = glView;
```

### Software Backend (SKCanvasView)
```csharp
using SkiaSharp.Views.Mac;

var canvasView = new SKCanvasView(frame);
canvasView.PaintSurface += (s, e) =>
{
    var canvas = e.Surface.Canvas;
    canvas.Clear(SKColors.White);
    // Reporter's drawing code here
};
_window.ContentView = canvasView;
```

## Build

```bash
dotnet build -c Release
```

Common build errors:
- `NETSDK1174: missing workload` → `dotnet workload install macos`
- `error MT0000: ApplicationId not set` → Add `<ApplicationId>` to `.csproj`
- `error: no signing identity found` → Add `<EnableCodeSigning>false</EnableCodeSigning>` for local testing

## Run & Verify

```bash
# Run directly for stdout capture
./bin/Release/net10.0-macos/osx-arm64/Repro.app/Contents/MacOS/Repro
```

**Visual verification:** If the bug is visual, take a screenshot to verify rendering. Log key metrics to stdout.

**For GPU bugs:** Test both Metal and GL backends. If one works and the other doesn't, that's a critical signal.

## Iterate

- If the bug doesn't reproduce with SKCanvasView (software), try SKGLView (GL) or SKMetalView (Metal)
- For GL-specific bugs, ensure the NSOpenGLPixelFormat includes the required attributes (stencil, MSAA, depth)
- For performance issues, see **Category 10: Performance** in bug-categories.md for measurement methodology

## Conclusion Mapping

| Observation | Conclusion |
|-------------|-----------|
| Bug reproduced on macOS | `reproduced`, scope `platform-specific/macos` |
| Bug reproduced on macOS AND console | `reproduced`, scope `universal` |
| Bug only on one backend (GL not Metal) | `reproduced`, scope `platform-specific/macos`, note backend |
| Works on macOS, reporter on different OS | `not-reproduced`, suggest platform test |
| Can't build macOS app | `needs-platform` if no macOS host |

## Main Source Testing (Phase 3C)

```bash
cd /path/to/skiasharp-repo

# Bootstrap if needed
[ -d "output/native" ] && ls output/native/ | head -5 || dotnet cake --target=externals-download

# Use the macOS basic sample
cd samples/Basic/macOS/SkiaSharpSample
# Add reporter's repro code to the draw handler
dotnet build
dotnet run
```

After testing, revert: `git checkout -- samples/`

## Pitfalls

- macOS OpenGL is **deprecated by Apple** (last updated at OpenGL 4.1). GL bugs may be macOS driver quirks, not SkiaSharp bugs. Document which GL version is active.
- `glGetIntegerv` can return incorrect values on macOS default framebuffers (e.g., `GL_STENCIL_BITS = 0` when stencil is actually allocated). Cross-check with `NSOpenGLPixelFormat.GetValue()`.
- Metal requires a GPU — won't work in headless CI without GPU passthrough.
- macOS `.app` bundles need specific directory structure. `dotnet build` handles this, but `dotnet run` may not.
- The `NSOpenGLView` used by `SKGLView` is deprecated. Some visual bugs may be due to Apple's abandoned GL implementation.
