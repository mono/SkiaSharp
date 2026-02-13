# Platform: Docker Linux

Reproduce bugs on Linux using Docker. Use when the host is macOS/Windows, or to test
older SkiaSharp versions that lack host-platform native assets.

See also: [docker-testing.md](../../bug-fix/references/docker-testing.md) for full
dependency matrix and troubleshooting.

## Signals

Linux, Docker, container, `NativeAssets.Linux`, fontconfig, "works on Windows/Mac but
not Linux", `DllNotFoundException` on Linux, case-sensitivity issues, server deployment,
Azure App Service, AWS Lambda.

Also use for:
- **Old versions (1.68.x) on Apple Silicon** — no arm64 native exists, must use `--platform linux/amd64`
- **Cross-platform verification** — Docker Linux is the default alternative platform
- **Inconclusive host results** — second environment may clarify

## Prerequisites

- Docker Desktop: `docker --version`
- For old SkiaSharp (pre-2.x): use `--platform linux/amd64` (Rosetta emulation on Apple Silicon)

## Quick Repro Pattern

```bash
docker run --rm --platform linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 bash -c '
apt-get update -qq && apt-get install -y -qq libfontconfig1 2>&1 | tail -1
mkdir -p /tmp/repro && cd /tmp/repro
dotnet new console -n Repro --framework {reporter_tfm} --no-restore 2>&1 | tail -1
cd Repro
dotnet add package SkiaSharp --version {reporter_version} --no-restore 2>&1 | tail -1
dotnet add package SkiaSharp.NativeAssets.Linux --version {reporter_version} --no-restore 2>&1 | tail -1
cat > Program.cs << "ENDOFFILE"
{reporter_code}
ENDOFFILE
dotnet run --runtime linux-x64 --no-self-contained 2>&1
'
```

**Adapt the SDK image tag** to match `{reporter_tfm}`:
- `net6.0` → `sdk:6.0`
- `net8.0` → `sdk:8.0`
- `net9.0` → `sdk:9.0`
- `net10.0` → `sdk:10.0` (or `sdk:10.0-preview` if not yet GA)

## Required Dependencies

**⚠️ `libfontconfig1` is MANDATORY** — without it you get `DllNotFoundException` for libSkiaSharp.

```bash
# Debian/Ubuntu (default SDK images)
apt-get update -qq && apt-get install -y -qq libfontconfig1

# Alpine (sdk:8.0-alpine) — use sh not bash
apk add --no-cache fontconfig
```

For rendering/font bugs, also install fonts:
```bash
apt-get install -y -qq fonts-dejavu-core   # Debian
apk add --no-cache ttf-dejavu              # Alpine
```

## Platform Variants

| Variant | Docker flag | When to use |
|---------|------------|-------------|
| Linux x64 (Debian) | `--platform linux/amd64` | Default — most common deployment target |
| Linux arm64 (Debian) | `--platform linux/arm64` | Native on Apple Silicon, tests arm64 natives |
| Alpine musl | `sdk:8.0-alpine` | Tests musl libc compatibility |

## Run & Verify

All output comes through Docker stdout/stderr. Capture the full output of the `docker run` command.

- Exit code 0 + expected output → `success`
- Non-zero exit code or crash → `failure`
- Exit code 0 but wrong values → `wrong-output`

## Recording in JSON

```json
{
  "environment": {
    "os": "Linux (Docker)",
    "arch": "x64",
    "dotnetVersion": "8.0.xxx",
    "skiaSharpVersion": "{reporter_version}",
    "dockerUsed": true
  }
}
```

Tag `versionResults` entries with `"platform": "docker-linux-x64"` (or `docker-linux-arm64`).

## Common Issues

| Problem | Cause | Fix |
|---------|-------|-----|
| `DllNotFoundException: libSkiaSharp` | Missing fontconfig | `apt-get install libfontconfig1` |
| `liblibSkiaSharp.so` (double prefix) | RID resolution issue | Add `--runtime linux-x64 --no-self-contained` |
| Very slow on Apple Silicon | x64 emulation via Rosetta | Normal — allow extra time |
| No arm64 natives for 1.68.x | Version too old | Must use `--platform linux/amd64` |
| `dotnet restore` timeout | Network in Docker | Retry or use `--no-restore` + explicit restore |

## Conclusion Mapping

| Observation | Conclusion |
|------------|------------|
| Crash/error matching report | `reproduced` |
| Code runs correctly | `not-reproduced` |
| Docker not available | blocker (not `needs-platform`) |
| Need specific kernel feature Docker can't provide | `needs-platform` (rare) |

## Main Source Testing (Phase 3C)

For Docker/Linux bugs, Phase 3C tests the main branch console sample inside Docker:

```bash
# Back in the SkiaSharp repo — ensure native binaries exist
cd /Users/matthew/Documents/GitHub/SkiaSharp-2-worktrees/main
[ -d "output/native" ] && ls output/native/ | head -5 || dotnet cake --target=externals-download

# Build the console sample from source
dotnet build samples/Basic/Console/SkiaSharpSample/SkiaSharpSample.csproj

# Run in Docker with the source-built binary
dotnet publish samples/Basic/Console/SkiaSharpSample/SkiaSharpSample.csproj -r linux-x64 --no-self-contained -o /tmp/repro-publish/
docker run --rm --platform linux/amd64 -v /tmp/repro-publish:/app mcr.microsoft.com/dotnet/runtime:8.0 bash -c '
apt-get update -qq && apt-get install -y -qq libfontconfig1 2>&1 | tail -1
cd /app && dotnet SkiaSharpSample.dll 2>&1
'
```

Alternatively, if the sample doesn't cover the specific bug, temporarily modify
`samples/Basic/Console/SkiaSharpSample/Program.cs` with the repro code, rebuild, and
run in Docker. Revert with `git checkout` after recording the result.
