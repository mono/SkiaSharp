# SkiaSharp Console Sample

Renders a SkiaSharp scene to a PNG file from the command line. Draws a radial gradient background with semi-transparent circles and centered text — the same visual as the other SkiaSharp samples.

![Console Sample Output](screenshots/screenshot.png)

**Features:**

- **`SKSurface`** — Creates a CPU-rendered surface and saves it as a PNG.
- **`SKShader`** — Radial gradient background created with `SKShader.CreateRadialGradient`.
- **`SKCanvas.DrawCircle`** — Semi-transparent colored circles composited over the gradient.
- **`SKCanvas.DrawText`** — Centered text rendered with measured alignment. Text is customizable via command-line argument.
- **CLI arguments** — Optional text and `--output` path.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later

## Running the Sample

```bash
# Default: renders "SkiaSharp" to output.png
dotnet run --project SkiaSharpSample/SkiaSharpSample.csproj

# Custom text and output path
dotnet run --project SkiaSharpSample/SkiaSharpSample.csproj -- "Hello World!" --output /path/to/image.png
```
