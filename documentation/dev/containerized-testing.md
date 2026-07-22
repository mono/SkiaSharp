# Containerized testing (Docker)

The console test suite (`SkiaSharp.Tests.Console`) can be run **inside a Docker container** against a
chosen native build. This covers container runtimes that the agent-based test legs don't — **Linux
glibc**, **Linux Alpine (musl)**, and **Windows Nano Server**.

Pieces:

- **Cake target** [`tests-container`](../../scripts/infra/tests/tests-container.cake) — runs the
  console suite against a prebuilt native library. Builds no externals.
- **Env images** [`scripts/infra/tests/docker/`](../../scripts/infra/tests/docker) —
  `glibc/`, `alpine/`, `nanoserver/`.
- **CI legs** `tests_container_linux`, `tests_container_alpine_linux`,
  `tests_container_nanoserver_windows` in
  [`scripts/azure-templates-stages-test.yml`](../../scripts/azure-templates-stages-test.yml).

## How a leg runs

Each leg uses the bootstrapper `docker:` feature (`azure-templates-jobs-bootstrapper.yml`), which:

1. builds the **env image** from `<docker>/Dockerfile` with the `Docker@2` task, then
2. runs `dotnet cake --target=tests-container` **inside** that image against the mounted repo:
   `docker run --volume <repo>:<work> skiasharp … dotnet cake --target=tests-container …`.

The feature runs on both Linux and Windows agents: Linux mounts `/work` and runs via `/bin/bash`;
Windows mounts `C:\work` and runs via `cmd`.

The **env images** provide only the runtime and fonts — the SDK, and for the Linux images the
`fontconfig` + DejaVu fonts. The Nano Server image is the Nano Server **.NET SDK** image, which has
no system fonts (see [Fonts on Nano Server](#fonts-on-nano-server)). There is no `COPY` in these
Dockerfiles; the repo is mounted at run time.

## What `tests-container` does

1. builds `tests/SkiaSharp.Tests.Console` with `-p:TargetFrameworks=net10.0`, collapsing the
   multi-targeted binding projects to a single TFM so no Android/iOS workloads are needed;
2. runs the suite via `dotnet test`, writing `output/logs/testlogs/**/TestResults.trx`.

It builds no externals — the prebuilt `libSkiaSharp` comes from `output/native/<platform>/<arch>/`
in the mounted repo (in CI, the merged `native` artifact; locally, `externals-download`).

## Selecting the native build

`--nativePlatform=<platform>` sets the `SkiaSharpNativePlatform` / `HarfBuzzSharpNativePlatform`
MSBuild properties. In `binding/IncludeNativeAssets.SkiaSharp.targets` (and the HarfBuzz twin), those
properties make the desktop `Content` include copy the native library from
`output/native/<platform>/<arch>/` only. The `<arch>` is derived from the host (`OSArchitecture`).

This is how a container runs against a platform-specific build the OS-derived defaults don't map —
`nanoserver` (`output/native/nanoserver/x64/libSkiaSharp.dll`) on Windows, and `alpine`
(`output/native/alpine/x64/libSkiaSharp.so`) on musl.

## Running it locally

Build the env image, then run `tests-container` inside it against the mounted repo:

```bash
# Linux glibc  (--nativePlatform picks the build; <arch> is auto from the host).
dotnet cake --target=externals-download
docker build -t skiasharp-tests-env scripts/infra/tests/docker/glibc
docker run --rm --volume "$(pwd):/work" -w /work skiasharp-tests-env \
    /bin/bash -c "dotnet tool restore && dotnet cake --target=tests-container --nativePlatform=linux"
```

```bash
# Linux Alpine (musl).
docker build -t skiasharp-tests-env-alpine scripts/infra/tests/docker/alpine
docker run --rm --volume "$(pwd):/work" -w /work skiasharp-tests-env-alpine \
    /bin/bash -c "dotnet tool restore && dotnet cake --target=tests-container --nativePlatform=alpine"
```

```powershell
# Nano Server  (Windows container host only).
dotnet cake --target=externals-nanoserver
docker build -t skiasharp-tests-env scripts/infra/tests/docker/nanoserver
docker run --rm --volume "${pwd}:C:\work" -w C:\work skiasharp-tests-env `
    cmd /c "dotnet tool restore && dotnet cake --target=tests-container --nativePlatform=nanoserver"
```

## CI wiring

Each leg is a bootstrapper job in the `tests` stage that:

- declares `requiredArtifacts: - name: native`, so `output/native/…` is the merged native artifact;
- sets `docker:` to the env image and `target: tests-container` with
  `additionalArgs: --nativePlatform=<…>`;
- publishes the TRX via `PublishTestResults@2` (Azure DevOps Tests tab) and the
  `testlogs_container_<platform>` artifact.

The legs are gating: a genuine test failure fails the build. Environment differences that are not
SkiaSharp regressions — most notably the fontless `nodeps` and Nano Server builds — are handled by
the runtime self-skip helpers (see [Fonts](#fonts) below), so those tests skip rather than fail.

The Nano Server leg requires a Windows agent with container support (matching the `ltsc2022` image
base) and pulls the Nano Server .NET SDK image.

## Fonts

Whether the suite can resolve **system fonts** depends on how the native library was compiled, not
just on the image:

- **fontconfig builds** (`linux`, `alpine`) enumerate whatever fonts are installed in the image.
  Base .NET SDK images ship no fonts, so the env images install them: `fontconfig` + DejaVu on both,
  plus `font-noto-emoji` on Alpine for emoji coverage. These provide the families the test config
  expects (`DefaultFontFamily`, `UnicodeFontFamilies`).
- **non-fontconfig builds** — the NoDependencies variants (`linuxnodeps`, `alpinenodeps`, built with
  `skia_use_fontconfig=false`) and **Nano Server** — enumerate **no** system fonts regardless of
  what the image contains. Their font manager (`SkFontMgr_New_Custom_Empty`, a FreeType scanner) can
  only use fonts loaded explicitly (`SKTypeface.FromFile` / `FromStream` / `FromData`). APIs that
  resolve a system family or the default typeface (`SKTypeface.FromFamilyName`, `SKFontManager.Default`,
  a default `SKFont`) have nothing to bind to.

The Linux test config chooses `UnicodeFontFamilies` per libc — `Symbola` on glibc (from
`ttf-ancient-fonts`) and `Noto Color Emoji` on musl (from `font-noto-emoji`), keyed off
`PlatformConfiguration.IsGlibc`.

Tests that need system fonts detect the environment at runtime and self-skip, so the same suite
runs unmodified everywhere:

- `SkipWhenNoSystemFontManager()` — for tests that enumerate or match families
  (`SKFontManager` / `SKFontStyleSet`, or `MatchCharacter`). Skips where there is no usable font
  manager: WASM, the NoDependencies builds, and Nano Server.
- `SkipWhenNoDefaultFont()` — for tests that measure or draw with the **default** typeface. Skips
  where the default typeface is empty: the NoDependencies builds and Nano Server. WASM keeps a
  single embedded default font, so these still run there.
