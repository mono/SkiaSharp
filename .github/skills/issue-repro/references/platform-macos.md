# Platform: macOS GUI Application

For bugs involving macOS rendering, views, GPU backends, or any macOS-specific UI behavior.

## Contents
1. [Signals](#signals) — [Prerequisites](#prerequisites) — [Create Project](#create-project)
2. [macOS App Tips](#macos-app-tips) — [Backend-Specific Templates](#backend-specific-templates)
3. [Build](#build) — [Run & Verify](#run--verify) — [Iterate](#iterate)
4. [Conclusion Mapping](#conclusion-mapping) — [Main Source Testing (Phase 3C)](#main-source-testing-phase-3c)

## Signals

macOS, Mac Catalyst, `SKGLView`, `SKMetalView`, `NSOpenGLView`, `MTKView`, macOS crash, macOS visual defect, macOS rendering, macOS-specific, `net*-macos`, Mac GPU, OpenGL on Mac, Metal on Mac, macOS window, AppKit.

Also use when triage JSON has `classification.platforms` containing `os/macOS` or `classification.backends` containing `backend/Metal` or `backend/OpenGL`.

## Prerequisites

- .NET SDK (10.0+ recommended)
- macOS workload: `dotnet workload install macos`
- Xcode Command Line Tools: `xcode-select --install`

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

## macOS App Tips

**Run the binary directly** for stdout capture — `open -n AppName.app` launches detached from the terminal:
```bash
# ✅ See stdout
./bin/Release/net10.0-macos/osx-arm64/Repro.app/Contents/MacOS/Repro

# ❌ No stdout capture
open -n ./bin/Release/net10.0-macos/osx-arm64/Repro.app
```

**Use `dotnet build`, not `dotnet run`** — `dotnet run` doesn't reliably produce the `.app` bundle structure that macOS requires.

**Code signing** — For local testing, add `<EnableCodeSigning>false</EnableCodeSigning>` to skip signing errors.

**Add SkiaSharp views** to the window in your `AppDelegate.DidFinishLaunching`:
```csharp
// In DidFinishLaunching, add the appropriate view to your window.
// ⚠️ MATCH THE REPORTER'S BACKEND — do not substitute one for another.
_window.ContentView = new SKGLView(frame);     // OpenGL — use if reporter uses SKGLView
// or: new SKMetalView(frame);  // Metal — use if reporter uses SKMetalView
// or: new SKCanvasView(frame); // Software — use if reporter uses SKCanvasView
```

## Backend-Specific Templates

Use these when the bug involves rendering output or GPU behavior — test multiple backends to isolate whether the issue is backend-specific.

### Metal Backend (SKMetalView)
```csharp
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
- `error: no signing identity found` → Add `<EnableCodeSigning>false</EnableCodeSigning>`

## Run & Verify

```bash
./bin/Release/net10.0-macos/osx-arm64/Repro.app/Contents/MacOS/Repro
```

**Visual verification:** If the bug is visual, take a screenshot to verify rendering. Log key metrics to stdout.

**For GPU bugs:** Test both Metal and GL backends. If one works and the other doesn't, that's a critical signal — note the backend in the conclusion.

## Iterate

- If the bug doesn't reproduce with SKCanvasView (software), try SKGLView (GL) or SKMetalView (Metal)
- For performance issues, see [category-performance.md](category-performance.md) for measurement methodology

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
