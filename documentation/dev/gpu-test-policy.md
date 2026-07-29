# GPU test policy

A GPU backend is **required** on every host `GpuPolicy.RequiredOn` names for it.
Failing to bring one up — no device, no driver, no ICD, a null context, a broken
binding — is a **test failure**. A GPU test may only be skipped when `GpuPolicy`
says so.

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

`GpuPolicy.RequiredOn` should track the `gn` args in `native/*/build.cake`
(`skia_use_metal`, `skia_use_vulkan`, `skia_use_dawn`, `skia_use_direct3d`) for
every platform that has a test host — keep the two in sync. It lists test hosts,
not every native build: Tizen builds Ganesh but has no test leg, and the browser
build enables Ganesh without `ganesh-gl` being required there.

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
`scripts/azure-templates-stages-test.yml`, declared per leg with the reason and a
tracking issue recorded inline. That file is the live list — read it there.

An opt-out is a statement about **one agent**, not about the backend. It belongs
in the YAML precisely so it can be removed when the agent gains the capability,
without touching the policy or this document.

Opt-outs are per leg, so an architecture-specific limitation needs its own job:
`GpuPolicy` models the OS, not the architecture. That is why the .NET Framework
tests run as separate x64 and x86 jobs.

## Declared opt-outs in CI

Every opt-out lives in `scripts/azure-templates-stages-test.yml` as a bootstrapper
`env:` value, so what a leg can and cannot do is visible in the pipeline
definition rather than inferred at runtime. The full current set:

| Leg(s) | Opted out | Why |
|---|---|---|
| iOS | `ganesh-metal`, `graphite-metal` | Runs the *simulator*, whose virtualized Metal leaves dispatch-queue state that hangs the test host on shutdown. macOS and Mac Catalyst drive real Metal on the same agent and stay **required**. |
| Windows (.NET Framework + .NET Core) | `ganesh-gl` | No GPU driver, so Windows falls back to GDI generic OpenGL 1.1 with no `WGL_ARB_*` extensions. Fixable — see below. Vulkan and Direct3D stay required. |
| Azure Linux, Alpine (+ NoDeps ×2), Nano Server | `ganesh-gl` | No X server, no Mesa, no ICD in those images. |

Everything else is required, including Vulkan on Windows/Linux/Android, Metal on
macOS/Mac Catalyst, Direct3D on Windows, and Dawn in the browser.

### Known gap: software OpenGL on Windows

`ganesh-gl` on the Windows agents is a provisioning gap, not a platform limit.
The fix is Mesa's llvmpipe: drop its `opengl32.dll` + `libgallium_wgl.dll` into
`System32`/`SysWOW64` (the same shape as `install-vulkan-icd.ps1`) and set
`GALLIUM_DRIVER=llvmpipe`. It needs **no test-code change** — Mesa's WGL
extension string statically advertises both `WGL_ARB_pixel_format` and
`WGL_ARB_pbuffer`, and its `wglChoosePixelFormatARB` reports
`WGL_FULL_ACCELERATION_ARB`, which is exactly what `WglContext` requires.

It is not wired up yet only because there is no trustworthy package feed for
Mesa's Windows binaries: `Silk.NET` ships no desktop-GL package (and its
`Silk.NET.OpenGLES.ANGLE.Native` is unusable — every published version has
32-bit binaries in `runtimes/win-x64/`), and the community NuGet alternatives
are neither complete nor credible. Mirroring the upstream `mesa-dist-win` MSVC
release into a trusted feed would unblock it.

## Adding a backend

Add a const to `GpuBackends` and a row to `GpuPolicy.RequiredOn`. On those
platforms it is now required, so a failed bring-up is red until you fix it or
declare an opt-out.

See also: [golden-image-tests.md](golden-image-tests.md).
