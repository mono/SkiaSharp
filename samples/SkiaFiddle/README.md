# SkiaFiddle — Live SkiaSharp Playground

Write and run C# drawing code in your browser. SkiaFiddle combines Monaco
editors, Roslyn compilation, and a SkiaSharp canvas to explore drawings,
animations, and SkSL shaders.

## Build and run

Install the .NET 10 SDK and WASM workload. From this folder:

```sh
dotnet workload install wasm-tools
dotnet run --project SkiaFiddle.csproj -c Debug -f net10.0-browserwasm
```

Open the URL printed by the application.

## Try a snippet

Choose a bundled example, edit the code, and press **Run**:

- **Setup** declares fields, methods, and initialization that persist between frames.
- **Draw** runs every frame with `canvas`, `width`, `height`, and elapsed time `t`.
- The font and image pickers provide the `typeface` and `image` variables.

Start with the gradient drawing, animate the orbit example, or modify a plasma
or ripple shader. Use **Stop** to pause drawing; compilation errors appear below
the editors. Bundled media and its licenses are described in [Media](Media/README.md).

## Publish a static site

```sh
dotnet publish SkiaFiddle.csproj \
  -c Release -f net10.0-browserwasm \
  -p:WasmEnableSIMD=false -o output/skiafiddle
python3 -m http.server 5050 --directory output/skiafiddle/wwwroot
```

Open <http://localhost:5050/>. The local server command requires Python 3.
