# GPU test policy

A GPU backend is **required** on every host we build it for. Failing to bring one
up — no device, no driver, no ICD, a null context, a broken binding — is a **test
failure**. A GPU test may only be skipped when `GpuPolicy` says so.

## The rule

`GpuPolicy.RequireOrSkip(backend)` skips when either:

- the current platform is not in the backend's `RequiredOn` set, or
- the backend was opted out for this agent via `SKIASHARP_TEST_SKIP_GPU`.

Otherwise it returns and the caller brings the backend up **without a catch**.

> The table describes **platforms**; the environment variable describes
> **agents**. You never set anything to make Metal skip on Windows.

## Where it applies

| Test | Gate |
|---|---|
| Visual tests | `VisualMatrixTestsBase.RunTestAsync` |
| OpenGL tests | `SKTest.CreateGlContext` |
| Vulkan tests | `VKTest` |
| Direct3D tests | `Direct3DTest` |
| Graphite release tests | `SKGraphiteReleaseTestsBase` |

## Required platforms

`GpuPolicy.RequiredOn` mirrors the `gn` args in `native/*/build.cake`
(`skia_use_metal`, `skia_use_vulkan`, `skia_use_dawn`, `skia_use_direct3d`) —
keep the two in sync.

| Backend | Required on |
|---|---|
| `raster` | everywhere (CPU) |
| `ganesh-gl` | Windows, macOS, Linux, Nano Server |
| `ganesh-metal`, `graphite-metal` | Apple |
| `ganesh-vulkan`, `graphite-vulkan` | Windows, Linux, Android |
| `ganesh-vulkan-sharpvk` | Windows |
| `ganesh-direct3d` | Windows |
| `graphite-dawn` | Browser |

Nano Server is its own platform: `native/nanoserver/build.cake` builds without
Vulkan or Direct3D.

## Opting out

```bash
SKIASHARP_TEST_SKIP_GPU=ganesh-gl,graphite-dawn   # specific backends
SKIASHARP_TEST_SKIP_GPU=all                       # every GPU backend but raster
```

Comma, semicolon or whitespace separated, case-insensitive. An unrecognised id
is an error, so a typo cannot quietly leave a backend required.

Device and browser hosts never see the agent environment, so the same list is
baked into `runtimeconfig.json` from the `SkiaSharpTestSkipGpu` MSBuild property:

```bash
dotnet cake --target=tests-android --skipGpu=ganesh-vulkan
```

In CI each opt-out is a bootstrapper `env:` value in
`scripts/azure-templates-stages-test.yml` — read it there for the current set
rather than duplicating it here. Today: iOS and Mac Catalyst skip Metal (the
simulator's virtualized Metal hangs the host on shutdown; a Catalyst app never
sees the `Mac2` GPU family Skia needs), the Windows and container legs skip
`ganesh-gl` for want of a software GL stack, and the WASM legs skip
`graphite-dawn` because the headless browser exposes no WebGPU adapter.

## Adding a backend

Add a const to `GpuBackends` and a row to `GpuPolicy.RequiredOn`. On those
platforms it is now required, so a failed bring-up is red until you fix it or
declare an opt-out.

See also: [golden-image-tests.md](golden-image-tests.md).
