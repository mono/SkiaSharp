# GPU test policy

Every GPU backend the test suite can drive is **required** on the platforms we
build it for. Failing to bring one up — no device, no driver, no ICD, no
display, a null context, a broken binding — is a **test failure**, never a skip.

A GPU test may only be skipped when that is *declared* somewhere a human can
read it. There is no path where an exception becomes a skip.

## Why

The suite used to be written as `try { bring up the backend } catch { Assert.Skip }`.
That made a genuine regression indistinguishable from "this agent has no GPU",
so CI stayed green while coverage quietly evaporated. The committed goldens are
the receipt: `ganesh-gl` had a golden for macOS only, and `graphite-dawn` had
none at all, because those cells had been silently skipping for a long time.

## Required, or skipped for a stated reason

A backend is **required** on this host unless one of two things is true:

| Not required because | Example | Configured? |
|---|---|---|
| the platform isn't in its `RequiredOn` set | Metal on Windows, Vulkan on macOS, Dawn on desktop | no |
| it was **disabled** for this agent | `ganesh-gl` on a headless CI agent | **yes** |

Everything else must work. No device, no driver, no ICD, a null context or a
broken binding is a **failure**.

> **The rule:** the table describes **platforms**; the environment variable
> describes **agents**. Nothing about a platform's inherent capabilities is ever
> expressed as configuration — you never need to set anything to make Metal skip
> on Windows.

The table doesn't distinguish "the API doesn't exist here" (Metal off Apple)
from "we don't build it here yet" (Vulkan on macOS, which would need MoltenVK).
Both simply aren't in `RequiredOn`, and both skip without configuration; the
column below says which is which.

## Opting out (`SKIASHARP_TEST_SKIP_GPU`)

Use this only when a backend *should* work on the platform but this particular
machine can't run it — a headless CI session, a container with no Mesa, an agent
whose driver is broken.

```bash
# skip specific backends
SKIASHARP_TEST_SKIP_GPU=ganesh-gl,graphite-dawn dotnet cake --target=tests-netcore

# skip every GPU backend
SKIASHARP_TEST_SKIP_GPU=all dotnet cake --target=tests-netcore
```

- Separators: comma, semicolon or whitespace. Case-insensitive.
- Unset or empty means nothing is opted out.
- **An unrecognised id is a hard error.** A typo like `vulcan`, or a bare `metal`
  instead of `ganesh-metal`, fails loudly rather than quietly leaving the backend
  required. `GpuPolicyTests.OptOutListNamesOnlyKnownBackends` is the guard.

### Device and browser hosts

The Android, iOS, Mac Catalyst and WASM test apps run on a device, emulator or
browser and never see the build agent's environment. For those, the same value
is baked into `runtimeconfig.json` as `SkiaSharp.Tests.SkipGpu` and read back
through `AppContext`:

```bash
dotnet cake --target=tests-android --skipGpu=ganesh-vulkan
dotnet build … -p:SkiaSharpTestSkipGpu=graphite-dawn
```

The cake targets also pick up `SKIASHARP_TEST_SKIP_GPU` from the environment and
forward it as the property, so a pipeline leg can set the variable once and have
it apply on every host. Resolution order is `AppContext` → environment variable
→ nothing.

## The table

Ids match the visual-matrix renderer names one-for-one, so a golden folder, an
opt-out directive and a failure message all use the same word.

| Id | RequiredOn | Absent elsewhere because |
|---|---|---|
| `raster` | all | — (CPU, always required) |
| `ganesh-gl` | Windows, macOS, Linux, Nano Server | no `GlContext` for the device/browser hosts |
| `ganesh-vulkan` | Windows, Linux, Android | not built for Apple, Nano Server or the browser |
| `graphite-vulkan` | Windows, Linux, Android | not built for Apple, Nano Server or the browser |
| `ganesh-vulkan-sharpvk` | Windows | SharpVk cannot create a context off Windows |
| `ganesh-metal` | Apple | Metal is an Apple-only API |
| `graphite-metal` | Apple | Metal is an Apple-only API |
| `ganesh-direct3d` | Windows | Direct3D is Windows-only; not built for Nano Server |
| `graphite-dawn` | Browser | Dawn is only built for WebAssembly |

`RequiredOn` mirrors the `gn` args in `native/*/build.cake` — `skia_use_metal`,
`skia_use_vulkan`, `skia_use_dawn`, `skia_use_direct3d`. **Keep the two in sync.**
Enabling a backend on a new platform is a one-token change to its `RequiredOn`
set, after which that platform's cells become required and must pass.

Windows Nano Server is tracked as its own platform because it runs a *different
native build*: `native/nanoserver/build.cake` passes `supportVulkan=false` and
`supportDirect3D=false`, so those backends are absent from `libSkiaSharp.dll`
there even though the OS is Windows.

## Using it in a test

`GpuPolicy.RequireOrSkip` is the only place in the suite allowed to skip a GPU
test. Call it, then bring the backend up **without a catch**:

```csharp
protected GlContext CreateGlContext()
{
    GpuPolicy.RequireOrSkip(GpuBackends.GaneshGl);

    return TestConfig.Current.CreateGlContext();
}
```

When a required backend genuinely can't come up, just throw — the exception says
what broke, and xUnit reports it as a failure:

```csharp
if (device == IntPtr.Zero)
    throw new InvalidOperationException(
        "MTLCreateSystemDefaultDevice returned null; no Metal device on this host.");
```

Visual-matrix renderers don't call the policy themselves — their `Name` *is* the
backend id, and `VisualMatrixTestsBase.RunCellAsync` gates the cell. A renderer
must never gate itself on the platform, or the "which OS has which API"
knowledge stops living in one table.

Skips are visible in the normal test output: xUnit records every skipped test
with its reason (`'ganesh-metal' is not required on windows.`) in the TRX that
each CI leg publishes.

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

1. Add a const to `GpuBackends` and a row to the `requiredOn` table naming the
   platforms it must work on.
2. Point the tests at it — an `IRenderer` whose `Name` is that id for a visual
   cell, or `GpuPolicy.RequireOrSkip` at the top of a bring-up helper.
3. Run it. On the platforms in `RequiredOn` it is now required, so an unseeded
   golden or a failed bring-up will be red until you seed or fix it. That is the
   intended signal; see [golden-image-tests.md](golden-image-tests.md).

## See also

- [golden-image-tests.md](golden-image-tests.md) — the visual-regression matrix
  that consumes this policy.
- `tests/Tests/SkiaSharp/Gpu/GpuPolicy.cs` — the table, the opt-out parsing and
  the guard tests. `TestPlatforms` lives beside `TestConfig`.
