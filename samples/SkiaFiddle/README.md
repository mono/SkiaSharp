# SkiaFiddle — Live SkiaSharp Playground (Uno Platform · WebAssembly)

A WebAssembly-only Uno Platform sample that compiles **C# SkiaSharp snippets**
in the browser with **Roslyn** and renders them through a `SKCanvasElement`.
Two code panes — *Setup* (runs once) and *Draw* (runs every frame) — let you
write animations and SkSL runtime shaders. Both panes use the **Monaco**
editor (the same one VS Code uses) via `Uno.Monaco.Editor`.

The project is WASM-only: its purpose is running C# in the browser.

## How it works

- **Roslyn in WASM** — `Microsoft.CodeAnalysis.CSharp` runs under the Mono
  interpreter. Snippets are wrapped in a class with a
  `Draw(SKCanvas, int, int, double)` method, compiled to an in-memory
  assembly, and loaded into a fresh collectible `AssemblyLoadContext`.
- **Reference assemblies** — `Basic.Reference.Assemblies.Net100` ships the
  .NET 10 ref-pack as embedded resources, so no DLLs need to travel in
  `wwwroot/` and `MetadataReference` calls hit memory.
- **SkiaSharp metadata** — added to the compilation via
  `AssemblyExtensions.TryGetRawMetadata`, so user snippets can reference any
  SkiaSharp API the host has loaded.
- **Animation** — a `DispatcherTimer` invalidates the canvas every frame; an
  elapsed-time `t` parameter is passed to the user's `Draw`.
- **Shaders** — Setup-block fields persist across frames, so an
  `SKRuntimeEffect.CreateShader(...)` declared there compiles its SkSL once
  and is reused per-frame.

## Bundled samples

| Name | What it shows |
|---|---|
| Static · Hello world | Radial gradient circle + centered text. |
| Animated · Orbits | Multi-planet orbit animation using `t`. |
| Shader · Plasma | SkSL plasma fragment shader. |
| Shader · Ripple | SkSL concentric ripples + radial gradient. |
| Animated · Sine wave | Phase-shifted sine. |
| Animated · Color grid | Hue-cycling color grid. |

## Prerequisites

Install the .NET 10 SDK. From this sample's folder:

```bash
dotnet workload install wasm-tools
```

SkiaSharp and its native assets are restored from NuGet packages.

## Build and run

```bash
dotnet run \
  --project SkiaFiddle.csproj \
  -c Debug \
  -f net10.0-browserwasm
```

Then open <http://localhost:5000/>.

For a release build / static deployment:

```bash
dotnet publish SkiaFiddle.csproj \
  -c Release \
  -f net10.0-browserwasm \
  -p:WasmEnableSIMD=false \
  -o output/skiafiddle-publish

python3 -m http.server 5050 --directory output/skiafiddle-publish/wwwroot
```

## Project layout

```
SkiaFiddle/
  SkiaFiddle.csproj                 # Uno.Sdk/6.6.0-dev.208, SkiaRenderer, WASM-only
  App.xaml + App.xaml.cs
  MainPage.xaml + MainPage.xaml.cs  # Two Monaco editors + SKCanvasElement
  GlobalUsings.cs
  Fiddle/
    FiddleCanvas.cs                 # SKCanvasElement subclass, per-frame loop
    IFiddleCompiler.cs
    RoslynFiddleCompiler.cs         # Parse → compile → emit → load → bind
    BuiltinFiddleCompiler.cs        # Fallback for diagnostics
    SampleSnippets.cs               # Bundled samples (Setup + Draw bodies)
  Platforms/WebAssembly/
    Program.cs                      # New Uno 6.6 hosting API
    LinkerConfig.xml                # Preserve Roslyn + ref-pack assemblies
    manifest.webmanifest
nuget.config                         # Adds nuget.org for the dev SDK + extras
```

The project pins `SkiaSharpVersion` so Uno uses the sample's selected
SkiaSharp package version rather than the version bundled with Uno.
