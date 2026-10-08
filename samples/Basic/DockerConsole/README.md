# SkiaSharp Docker Console Sample

Demonstrates SkiaSharp rendering inside a Docker container. Renders a radial gradient background with semi-transparent circles and centered text, then saves it as a PNG — the same visual as the other SkiaSharp samples.

Includes Dockerfiles for both Linux and Windows (Nano Server) containers.

**Features:**

- **`SKSurface`** — Creates a CPU-rendered surface and saves it as a PNG.
- **`SKShader`** — Radial gradient background created with `SKShader.CreateRadialGradient`.
- **`SKCanvas.DrawCircle`** — Semi-transparent colored circles composited over the gradient.
- **`SKCanvas.DrawText`** — Centered text rendered with measured alignment. Text is customizable via command-line argument.
- **Linux container** — Uses `SkiaSharp.NativeAssets.Linux.NoDependencies` (no system packages required).
- **Windows container** — Uses `SkiaSharp.NativeAssets.NanoServer` on Nano Server LTSC 2025.

## Requirements

- [Docker](https://docs.docker.com/get-docker/)

## Running the Sample

The repository sample tests build this generated sample with exact artifact
packages, check the container exit code, and decode its output PNG.
See [Building and Validating Samples](../../../documentation/dev/building-samples.md)
for package acquisition and the direct `dotnet test` entry point.

### Linux

```bash
docker build --tag skiasharpsample/console --file linux.Dockerfile .
docker run --rm skiasharpsample/console
```

With custom text:

```bash
docker run --rm skiasharpsample/console "Hello World!"
```

### Windows (Nano Server)

```powershell
docker build --tag skiasharpsample/console --file windows.Dockerfile .
docker run --rm skiasharpsample/console
```

### Using the script

```powershell
# Auto-detects platform
./run.ps1

# Or specify explicitly
./run.ps1 -Platform linux
./run.ps1 -Platform windows
```
