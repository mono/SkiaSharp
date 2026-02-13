# Platform: Blazor WebAssembly

Reproduce bugs in SkiaSharp's Blazor WASM integration using Playwright browser tools.

**⚠️ Build success ≠ runtime success.** WASM bugs often manifest ONLY at runtime in the
browser. You MUST serve the app and check browser console — never stop at `dotnet build`.

## Signals

Blazor, WASM, WebAssembly, browser, `SKCanvasView` (Blazor context), `SKHtmlCanvas`,
`SkiaSharp.Views.Blazor`, `DllNotFoundException` in browser console,
`TypeInitializationException` in browser, `_framework/`, `dotnet.wasm`, Uno Platform (WebAssembly).

## Prerequisites

- .NET SDK matching `{reporter_tfm}` (e.g., SDK 10 for `net10.0`)
- **⚠️ Run `dotnet --info` in `/tmp/` (NOT the SkiaSharp repo)** to see which SDK and
  wasm-tools version will actually be used. The SkiaSharp repo has `global.json` pinning
  to SDK 8.0, which won't be used for test projects in `/tmp/`.
- `wasm-tools` workload:
  ```bash
  dotnet workload install wasm-tools
  ```
  If this requires `sudo`, ask the user. Do NOT skip — it's required for native WASM compilation.
- Playwright MCP browser tools: `navigate`, `snapshot`, `console_messages`, `take_screenshot`

## Create Project

```bash
mkdir -p /tmp/repro-{number} && cd /tmp/repro-{number}
dotnet new blazorwasm -n Repro --framework {reporter_tfm}
cd Repro
dotnet add package SkiaSharp.Views.Blazor --version {reporter_version}
```

## Configure

Add to `.csproj` `<PropertyGroup>`:
```xml
<WasmBuildNative>true</WasmBuildNative>
```

This enables native WASM compilation of libSkiaSharp — required for SkiaSharp to work.

## Add Repro Code

Create or modify a Razor page (e.g., `Pages/Index.razor`):

```razor
@page "/"
@using SkiaSharp
@using SkiaSharp.Views.Blazor

<SKCanvasView OnPaintSurface="OnPaintSurface" style="width:800px;height:600px;" />

@code {
    private void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);

        // Paste reporter's drawing code here
        {reporter_code}

        Console.WriteLine($"SUCCESS: rendered {e.Info.Width}x{e.Info.Height}");
    }
}
```

Adapt based on the reporter's code. The `Console.WriteLine` outputs to browser console.

## Build

```bash
dotnet build
```

Record exit code, warnings, errors. Build failure may itself be the reproduction
(e.g., reporter says "build fails with WASM error").

**Do NOT stop here if build succeeds.** Proceed to Run & Verify.

## Run & Verify (CRITICAL)

### 1. Start dev server

```bash
dotnet run --urls http://localhost:5111 --no-build
```

Run this in background (async shell). Wait ~5 seconds for startup.

### 2. Navigate with Playwright

Use `browser_navigate` to `http://localhost:5111` (or the page containing SKCanvasView).

### 3. Check browser console

Use `browser_console_messages` with level `"error"`. Look for:
- `TypeInitializationException` — native WASM binary crash
- `DllNotFoundException` — missing native library
- `System.Exception` — runtime errors
- `SUCCESS: rendered` — confirms rendering worked

### 4. Check page content

Use `browser_snapshot` to verify the canvas rendered (not blank/error page).

### 5. Take screenshot (optional)

Use `browser_take_screenshot` if the bug is visual (wrong rendering).

### 6. Stop server

Kill the background process.

## Conclusion Mapping

| Observation | Conclusion |
|------------|------------|
| Browser console errors matching report | `reproduced` |
| Canvas renders, SUCCESS in console, no errors | `not-reproduced` |
| Build fails matching report | `reproduced` |
| `wasm-tools` not installable (no sudo) | blocker (not `needs-platform`) |
| Page loads but canvas is blank (no errors) | `wrong-output` |

## Common Issues

- **`wasm-tools` version mismatch:** SDK 10 may need a different workload version than SDK 8.
  Check `dotnet workload search wasm-tools` for available versions.
- **Long build times:** First WASM native build can take 2-5 minutes. This is normal.
- **Port conflicts:** If 5111 is taken, try 5112, 5113, etc.
- **`global.json` interference:** If the repo has a `global.json` pinning SDK version,
  create the repro project OUTSIDE the repo tree (in `/tmp/`).
