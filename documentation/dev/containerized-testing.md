# Containerized testing (Docker) — running the SkiaSharp test suite in a container

This describes how the SkiaSharp **console test suite** is run **inside a container** in CI, using
the repository's existing **bootstrapper `docker:` feature** — the same mechanism the native Linux
builds already use. It exists primarily to add **Windows Nano Server** to the test matrix (a minimal
Windows container with, notably, **no system fonts**), and also covers **Linux glibc** and **Linux
Alpine (musl)**. The Linux variants are the local proving ground.

Pieces:

- **Cake target** [`tests-container`](../../scripts/infra/tests/tests-container.cake) — runs *only*
  the console suite (`SkiaSharp.Tests.Console`) against a prebuilt native library. Builds **no**
  externals.
- **Env images** [`scripts/infra/tests/docker/`](../../scripts/infra/tests/docker) —
  `glibc/Dockerfile` (Linux SDK + fonts), `alpine/Dockerfile` (Alpine/musl SDK + fonts),
  `nanoserver/Dockerfile` (Nano Server .NET SDK, no fonts). These are *environment* images.
- **CI legs** `tests_container_linux`, `tests_container_alpine_linux`,
  `tests_container_nanoserver_windows` in
  [`scripts/azure-templates-stages-test.yml`](../../scripts/azure-templates-stages-test.yml).

## How it works (the bootstrapper `docker:` feature)

The `docker:` parameter of `azure-templates-jobs-bootstrapper.yml` does exactly two things:

1. builds the **env image** from `<docker>/Dockerfile` with the approved container-image task
   (`Docker@2`; the tests stage does not run under 1ES), then
2. runs the target **inside** that image against the **mounted repo**:
   `docker run --volume <repo>:/work skiasharp … dotnet cake --target=<target> …`.

So a containerized test leg is just: pick an env image, and run the `tests-container` cake target in
it. No bespoke Dockerfiles, no wrapper script, no raw `docker build` in a step — it reuses the same
compliant path as the native docker builds. (This change also taught the feature to run on **Windows**
containers — see *Windows support* below.)

### What `tests-container` does

`tests-container` (→ `scripts/infra/tests/tests-container.cake`):

1. builds `tests/SkiaSharp.Tests.Console` with `-p:TargetFrameworks=net10.0` (plural), which
   collapses the multi-targeted binding projects to a single TFM so the **Android/iOS/etc. workloads
   are never needed** — keeping the env image minimal;
2. selects the native build under test with `--nativePlatform=<linux|alpine|nanoserver|…>`, then
3. runs the suite via `dotnet test` (Microsoft.Testing.Platform), writing
   `output/logs/testlogs/**/TestResults.trx`.

It builds **no externals** — the prebuilt `libSkiaSharp` is provided in the mounted repo's
`output/native/<platform>/<arch>/`.

### The native swap (a real swap, not an overlay)

`--nativePlatform` is passed straight through to the native-asset targets as the
`SkiaSharpNativePlatform` / `HarfBuzzSharpNativePlatform` MSBuild properties. In
`binding/IncludeNativeAssets.SkiaSharp.targets` (and the HarfBuzz twin), when that property is set
the desktop `Content` include copies **only** from `output/native/<platform>/<arch>/` — instead of
the default OS-derived globs (which, given the `native` artifact contains *every* platform, would
otherwise copy several platforms' libraries at once). So the build copies exactly one library: the
one under test.

This is what lets the **Windows** container run against the **nanoserver** build
(`output/native/nanoserver/x64/libSkiaSharp.dll`), and the **Alpine** container against the **musl**
build (`output/native/alpine/x64/libSkiaSharp.so`) — neither of which the OS-derived defaults map.
The `<arch>` is derived automatically from the host (`OSArchitecture`). When the property is empty,
behavior is unchanged.

### Where the native library comes from

The container never builds or downloads the native library:

| Where | How `output/native/<platform>/<arch>/libSkiaSharp.*` is populated |
|---|---|
| **CI** | The merged **`native` pipeline artifact** (`requiredArtifacts: - name: native`), exactly like every existing test leg. **Never `externals-download`.** |
| **Local** | `dotnet cake --target=externals-download` (Linux / current milestone) or `dotnet cake --target=externals-nanoserver` on Windows (the nano lib must be built on Windows). Locally, `externals-download` is the normal bootstrap — the *no-download* rule is a CI constraint. |

## Running it locally

The local flow runs the *same* commands the bootstrapper feature runs in CI: build the env image,
then run `dotnet cake --target=tests-container` inside it against the mounted repo.

```bash
# Linux glibc. (--nativePlatform picks the build; <arch> is auto from the host.)
dotnet cake --target=externals-download            # provision output/native/linux/<arch>

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
# Nano Server — ONLY on a Windows container host.
dotnet cake --target=externals-nanoserver          # provision output/native/nanoserver/x64 (Windows)

docker build -t skiasharp-tests-env scripts/infra/tests/docker/nanoserver
docker run --rm --volume "${pwd}:C:\work" -w C:\work skiasharp-tests-env `
    cmd /c "dotnet tool restore && dotnet cake --target=tests-container --nativePlatform=nanoserver"
```

Results (`output/logs/testlogs/**/TestResults.trx`) are the standard TRX consumed by
`PublishTestResults@2` in CI (Azure DevOps' Tests tab), exactly like the other test legs.

### Worked example — real Linux proof run

Proven end-to-end on Linux (arm64, glibc) — running the `tests-container` target inside the
`glibc` env image against a **genuine CI-built** `output/native/linux/arm64/libSkiaSharp.so`
(SkiaSharp `4.151.0-preview.0.78`, AzDO build `1518863`):

- **Total 5902 · Passed 5720 · Failed 0 · Skipped 182** (5902 = test methods expanded by Theory
  data rows; ~1162 method definitions).
- The native swap was verified: the test output directory contained **only** the linux
  `libSkiaSharp.so` / `libHarfBuzzSharp.so` (no `.dll` / `.dylib`).
- The 182 skips are all environmental (no `libX11`/GL in the container, Metal is Apple-only, a few
  XR pixel formats, Windows-specific font paths, etc.).

The Linux env images install fonts (fontconfig + DejaVu, matching the CI Linux agents), so they are a
**baseline that validates the harness**, not a prediction of Nano Server behavior.

The **Alpine** leg additionally surfaced a real per-platform finding: the musl `libSkiaSharp.so`
loads and runs the whole suite (5715 passed), but ~4 unicode-font tests fail because Alpine does not
package the **"Symbola"** font that the Linux test config expects (glibc gets it via
`ttf-ancient-fonts`; there is no equivalent apk package). Those are environment gaps, not harness or
native bugs — and precisely the kind of result these non-gating legs exist to catalogue.

## Nano Server: the font limitation (context)

Nano Server has **zero system fonts**. Since PR #4280, the nano font manager is
`SkFontMgr_New_Custom_Empty()` (a FreeType scanner) — it still enumerates **no** system fonts, but
**can** create typefaces from external streams/data/files (`SKTypeface.FromFile/FromStream/FromData`).
Therefore, on Nano Server, expect:

- Tests that rely on the **default typeface** or a **family lookup** (e.g. `SKTypeface.FromFamilyName("Arial")`,
  `SKFontManager.Default` enumeration, default `new SKFont()` text draw/measure) to **fail or
  render/measure empty**.
- Tests that **load a font explicitly from the test assets** (`PathToFonts`) to **pass**.

The harness does **not** pre-skip or filter these tests — the full suite runs unmodified and the run
reports the real outcomes. The Nano Server env image uses the Nano Server **.NET SDK** image so the
suite is built *and* run in the same no-fonts environment.

## Windows support in the `docker:` feature

The bootstrapper `docker:` feature was Linux-only (it used `bash`, `/work`, and
`--platform linux/amd64`). It now branches on the agent OS:

- **Image build:** the `--platform linux/amd64` flag is only passed for non-Windows agents (Windows
  container images are built for the Windows host and can't cross-build from linux/amd64).
- **Run:** Windows agents mount the repo at `C:\work` and invoke `cmd /c … dotnet cake …`; Linux
  agents keep the existing `/work` + `/bin/bash` path.

The native docker build legs (all Linux) are unaffected.

## Wiring into CI

The three legs live in `scripts/azure-templates-stages-test.yml` and **run by default** (non-gating
for now). Each leg is an ordinary bootstrapper job that:

- is in the `tests` stage (`dependsOn: native`) and declares `requiredArtifacts: - name: native`, so
  `output/native/…` is populated from the merged **native artifact** — **never a download**;
- sets `docker:` to the env image (`…/glibc`, `…/alpine`, or `…/nanoserver`) and
  `target: tests-container` with `additionalArgs: --nativePlatform=<…>` — the image is built with the
  approved container-image task and the target runs inside it;
- publishes the TRX (`PublishTestResults`, `output/logs/testlogs/**/*.trx`) and the
  `testlogs_container_<platform>` artifact.

The legs intentionally do **not** flow `use1ESPipelineTemplates`: tests never run on the 1ES
pipeline, and should fail loudly if they ever do.

Both are **non-gating** (`continueOnError: true`) so expected failures — notably the Nano Server
font-related ones — do not turn the build red while the known-failure baseline is established.
Promote to gating once that baseline is agreed (drop `continueOnError`).

The **Nano Server** leg requires a **Windows agent with container support** (matching the `ltsc2022`
image base) and pulls the Nano Server .NET SDK image (non-trivial pull cost). If the pool cannot run
Windows containers, this leg fails — hence non-gating until the pool/host story is confirmed.
