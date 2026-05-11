# Headless Graphite testing on Linux / WSL2

Skia's Graphite backend always requires a real GPU API context (Vulkan, Metal, or Dawn). The CPU recorder vended by `Context::makeCPURecorder()` is a fallback for *drawing through* a Context — it does not let you skip context creation. So to validate Graphite end-to-end on a CI runner or developer machine that has no GPU, we install a software Vulkan ICD: **Mesa Lavapipe** (the LLVM-pipe Vulkan driver).

This document covers the install + verification steps. The full three-layer test flow is in `specs/002-graphite-backend-support/quickstart.md`.

## Why Lavapipe

| Option | Verdict |
|---|---|
| **Lavapipe** (Mesa LLVM-pipe Vulkan ICD) | ✅ Chosen. Pure CPU, no GPU required, packaged in mainstream distros, runs in WSL2 / containers. |
| SwiftShader (Vulkan) | Possible alternative. Heavier integration; we'll evaluate if Lavapipe coverage is insufficient. |
| Dawn null backend | Not exposed to user code in stable Dawn. |
| Skia `skcpu::Recorder` only | Doesn't exercise insertRecording / submit, so it's not a substitute for a real backend smoke. |

## Install (Debian / Ubuntu / WSL2-Ubuntu)

```bash
sudo apt-get update
sudo apt-get install -y mesa-vulkan-drivers vulkan-tools
```

Other distros: install the equivalent `mesa-vulkan-*` / `vulkan-loader` / `vulkan-tools` packages.

## Verify

```bash
vulkaninfo --summary | head -40
```

Expect at least one device named `llvmpipe` (or sometimes `lavapipe`) under `Devices`. If `vulkaninfo` exits non-zero or reports no devices, jump to "Troubleshooting" below.

## Forcing the Lavapipe ICD

If multiple Vulkan ICDs are installed (e.g. an NVIDIA ICD on a host with no usable GPU passthrough into WSL2), force Lavapipe:

```bash
export VK_ICD_FILENAMES=/usr/share/vulkan/icd.d/lvp_icd.x86_64.json
```

(The exact path may differ — search with `find /usr -name 'lvp_icd*.json'`.)

## Building SkiaSharp with Graphite + Vulkan enabled

From the repo root:

```bash
dotnet cake --target=externals-linux --arch=x64
```

`SUPPORT_GPU`, `SUPPORT_VULKAN`, and `SUPPORT_GRAPHITE` all default to
`true` since the matrix work landed; pass `SUPPORT_X=false` only if you
want to opt out.

The first run is a from-scratch Skia build — expect it to take 30–90 minutes wall-clock and to download a few GB of Skia/Dawn DEPS. Subsequent rebuilds are much faster (incremental ninja).

After the build, the new `sk_graphite_*` symbols should be exported:

```bash
nm -D --defined-only output/native/linux/x64/libSkiaSharp.so | grep -ci graphite
# Expect a non-zero count once the C API in externals/skia/src/c/sk_graphite*.cpp lands.
```

## Troubleshooting

- **`vulkaninfo` reports no Vulkan devices** — the loader can't find an ICD. `ls /usr/share/vulkan/icd.d/` should list at least `lvp_icd.*.json`. If empty, the `mesa-vulkan-drivers` package isn't installed correctly.
- **`Failed to load WSL libGL` or similar in WSL2** — Lavapipe is CPU-only; it does not need or use the WSL2 GPU passthrough. `LIBGL_ALWAYS_SOFTWARE=1` may help.
- **The Skia build fails with `gn: command not found`** — `dotnet cake --target=externals-download` (or one full `--target=externals-linux` run) populates `externals/skia/bin/gn`; make sure that step has completed.
- **Skia DEPS sync fails** — the build pipeline runs `git-sync-deps` which fetches Skia's third-party deps. Network access to googlesource.com is required.

## What this enables

With Lavapipe in place, the following are runnable on a GPU-less Linux dev machine:

1. The C++ smoke at `tests/native/Graphite/cpp_smoke/` — proves Skia Graphite + Lavapipe work.
2. The C smoke at `tests/native/Graphite/c_smoke/` — proves the new `sk_graphite_*` C shim is correct.
3. The C# smoke at `tests/Tests/SkiaSharp/Graphite/` — proves the managed binding and P/Invoke layer.

Each layer has its own pass criterion (pixel readback at `(128, 128)`); see `specs/002-graphite-backend-support/quickstart.md` for the bisection table.
