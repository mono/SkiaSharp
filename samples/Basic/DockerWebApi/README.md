# SkiaSharp Docker Web API Sample

Demonstrates server-side image rendering with SkiaSharp inside a Docker container using ASP.NET Core minimal APIs. Returns PNG images on the fly via HTTP endpoints.

Includes Dockerfiles for both Linux and Windows (Nano Server) containers.

**Features:**

- **Minimal API** — Single-file `Program.cs` with `MapGet` endpoints, no controllers needed.
- **`SKSurface`** — Creates a CPU-rendered surface for each request.
- **`SKShader`** — Radial gradient background with semi-transparent circles.
- **`SKCanvas.DrawText`** — Dynamic text from the URL path.
- **Bundled Noto Sans** — Copies the font to the output directory and loads it with `SKTypeface.FromFile`, so text renders without installed system fonts.

- **Linux container** — Uses `SkiaSharp.NativeAssets.Linux.NoDependencies` (no system packages required).
- **Windows container** — Uses `SkiaSharp.NativeAssets.NanoServer` on Nano Server LTSC 2025.

## Font license

Noto Sans is Copyright 2022 The Noto Project Authors and licensed under the
[SIL Open Font License 1.1](https://github.com/notofonts/latin-greek-cyrillic/blob/main/OFL.txt).
It may be used, embedded, and redistributed with applications under that license.

## Requirements

- [Docker](https://docs.docker.com/get-docker/)

## API Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /` | Usage info |
| `GET /api/images` | Renders default "SkiaSharp" image |
| `GET /api/images/{text}` | Renders image with custom text |

## Running the Sample

The repository sample tests build this generated sample with exact artifact
packages and exercise the health and image endpoints from an explicit test
method, including PNG decoding and pixel comparison.
See [Building and Validating Samples](../../../documentation/dev/building-samples.md)
for package acquisition and the direct `dotnet test` entry point.

### Linux

```bash
docker build --tag skiasharpsample/webapi --file linux.Dockerfile .
docker run --rm -p 8080:8080 skiasharpsample/webapi
```

Then open `http://localhost:8080/api/images` in a browser.

### Windows (Nano Server)

```powershell
docker build --tag skiasharpsample/webapi --file windows.Dockerfile .
docker run --rm -p 8080:8080 skiasharpsample/webapi
```

### Using the script

```powershell
# Auto-detects platform, builds, runs, fetches image, and stops
./run.ps1

# Or specify explicitly
./run.ps1 -Platform linux
```
