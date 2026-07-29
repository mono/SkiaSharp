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

## The four states

`GpuPolicy.Resolve(backend)` returns exactly one of these. Only the first can
produce a failure, and only the second is configured.

| State | Example | Can fail? | Configured? |
|---|---|---|---|
| `Required` | Vulkan on Linux, Metal on macOS | **yes** | no |
| `Disabled` | `ganesh-gl` on a headless CI agent | no | **yes** |
| `Unsupported` | Metal on Windows, Direct3D on Linux, Vulkan in the browser | no | no |
| `NotBuilt` | Vulkan on macOS, Dawn on desktop, OpenGL on Android | no | no |

The distinction that matters:

- **`Unsupported`** is a permanent fact about the platform. The API does not
  exist there and never will.
- **`NotBuilt`** is a fact about *our* build today. The API exists, we just
  don't ship it on that platform yet — Vulkan on macOS would need MoltenVK,
  Dawn on desktop would need `skia_use_dawn` outside the WASM build. These may
  flip later; the reason string names what would have to change.
- **`Disabled`** is a fact about *one machine*. It is the only state driven by
  configuration.

> **The rule:** the matrix describes **platforms**; the environment variable
> describes **agents**. Nothing about a platform's inherent capabilities is ever
> expressed as configuration — you never need to set anything to make Metal skip
> on Windows.

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

## The matrix

Ids match the visual-matrix renderer names one-for-one, so a golden folder, an
opt-out directive and a failure message all use the same word.

Each backend declares two platform sets. Outside `ExistsOn` it is `Unsupported`;
inside `ExistsOn` but outside `BuiltOn` it is `NotBuilt`; inside `BuiltOn` it is
`Required`.

| Id | ExistsOn | BuiltOn (⇒ required) |
|---|---|---|
| `raster` | all | all (CPU — always required) |
| `ganesh-gl` | all | Windows, macOS, Linux |
| `ganesh-vulkan` | all but Browser | Windows, Linux, Android |
| `graphite-vulkan` | all but Browser | Windows, Linux, Android |
| `ganesh-vulkan-sharpvk` | all but Browser | Windows |
| `ganesh-metal` | Apple | Apple |
| `graphite-metal` | Apple | Apple |
| `ganesh-direct3d` | Windows | Windows |
| `graphite-dawn` | all | Browser |

`BuiltOn` mirrors the `gn` args in `native/*/build.cake` — `skia_use_metal`,
`skia_use_vulkan`, `skia_use_dawn`. **Keep the two in sync.** Enabling a backend
on a new platform is a one-token change to its `BuiltOn` set, after which that
platform's cells become required and must pass.

## Using it in a test

`GpuPolicy.RequireOrSkip` is the only place in the suite allowed to skip a GPU
test. Call it, then bring the backend up **without a catch**:

```csharp
protected GlContext CreateGlContext()
{
    GpuPolicy.RequireOrSkip(GpuBackend.GaneshGl);

    return TestConfig.Current.CreateGlContext();
}
```

When a required backend genuinely can't come up, throw with
`GpuPolicy.OptOutHint(backend)` appended so the reader gets the exact directive
that would legitimise a skip:

```csharp
if (device == IntPtr.Zero)
    throw new InvalidOperationException(
        "MTLCreateSystemDefaultDevice returned null; no Metal device on this host. " +
        GpuPolicy.OptOutHint(GpuBackend.GaneshMetal));
```

Visual-matrix renderers don't call the policy themselves — they declare
`IRenderer.Backend` and `VisualMatrixTestsBase.RunCellAsync` gates the cell. A
renderer must never gate itself on the platform, or the "which OS has which API"
knowledge stops living in one table.

## The per-run report

`GpuPolicyTests.ReportsResolvedPolicy` writes one line per backend into the test
log:

```
##SKIA-GPU-POLICY## platform=linux
##SKIA-GPU-POLICY## backend=ganesh-gl state=required
##SKIA-GPU-POLICY## backend=ganesh-metal state=unsupported reason=Metal is an Apple-only API …
##SKIA-GPU-POLICY## backend=graphite-dawn state=not-built reason=Dawn/WebGPU is only built for …
```

The TRX is the one output channel that exists on every host — desktop, device
and browser — so this is how a CI leg reports which backends it actually
required. It is what makes a skip auditable: every skipped backend has to name
the reason it was skipped, and a backend that silently never ran shows up as
`state=required` with no corresponding test results.

The test is deliberately **not** tagged `Category=GPU`, so a leg that filters the
GPU suite out still publishes its report.

## Adding a backend

1. Add a value to `GpuBackend` and a row to the `GpuPolicy` matrix with its
   `ExistsOn` / `BuiltOn` sets and both reason strings.
2. Point the tests at it — an `IRenderer.Backend` for a visual cell, or
   `GpuPolicy.RequireOrSkip` at the top of a bring-up helper.
3. Run it. On the platforms in `BuiltOn` it is now required, so an unseeded
   golden or a failed bring-up will be red until you seed or fix it. That is the
   intended signal; see [golden-image-tests.md](golden-image-tests.md).

## See also

- [golden-image-tests.md](golden-image-tests.md) — the visual-regression matrix
  that consumes this policy.
- `tests/Tests/SkiaSharp/Gpu/` — `GpuPolicy`, `GpuBackend`, `TestPlatforms` and
  the policy guard tests.
