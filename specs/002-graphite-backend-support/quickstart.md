# Quickstart: Validating Graphite Backend Support End-to-End

**Date**: 2026-04-30 · **Branch**: `002-graphite-backend-support`

This is the runbook the spec's user-stories rely on, expressed as the three-layer validation flow ([research.md R5](research.md#r5--what-three-layer-validation-strategy-satisfies-fr-019-and-the-users-c--c--c-instruction)). Every layer renders the same scene (a filled red rounded rectangle on a white background, 256×256) and reads back the pixel at (128, 128) to assert it is red. This turns "does Graphite work?" into a single byte comparison.

## Environment prerequisites (one-time, this Linux WSL2 dev machine)

```bash
# Install Mesa Lavapipe (CPU-only Vulkan ICD)
sudo apt-get update
sudo apt-get install -y mesa-vulkan-drivers vulkan-tools

# Confirm a Vulkan device is exposed (LLVMpipe / Lavapipe)
vulkaninfo --summary | head -40
# Expect to see "deviceName = llvmpipe" or "lavapipe"
```

If the runner has no Vulkan ICD, set `VK_ICD_FILENAMES` to point at the Lavapipe `.json` (typical path: `/usr/share/vulkan/icd.d/lvp_icd.x86_64.json`).

## Build the native library with Graphite + Vulkan enabled

```bash
# From repo root
SUPPORT_GPU=true \
SUPPORT_VULKAN=true \
SUPPORT_GRAPHITE=true \
dotnet cake --target=externals-linux --arch=x64
```

This produces `output/native/linux/x64/libSkiaSharp.so` containing both Ganesh and Graphite-Vulkan symbols.

## Layer 1 — C++ smoke

**Location**: `tests/native/Graphite/cpp_smoke/`

```cpp
// graphite_cpp_smoke.cpp  (essence — full file in tasks.md output)
#include "include/gpu/graphite/Context.h"
#include "include/gpu/graphite/Recorder.h"
#include "include/gpu/graphite/vk/VulkanGraphiteContext.h"
#include "include/gpu/graphite/Surface.h"
#include "include/core/SkSurface.h"
#include "include/core/SkCanvas.h"

int main() {
    skgpu::VulkanBackendContext bc = MakeVulkanBackendContextLavapipe();  // helper
    auto ctx = skgpu::graphite::ContextFactory::MakeVulkan(bc, {});
    auto rec = ctx->makeRecorder({});

    SkImageInfo info = SkImageInfo::MakeN32Premul(256, 256);
    auto surface = SkSurfaces::RenderTarget(rec.get(), info);
    auto canvas  = surface->getCanvas();
    canvas->clear(SK_ColorWHITE);
    SkPaint p; p.setColor(SK_ColorRED); p.setAntiAlias(true);
    canvas->drawRRect(SkRRect::MakeRectXY({32, 32, 224, 224}, 24, 24), p);

    auto recording = rec->snap();
    skgpu::graphite::InsertRecordingInfo iri{};
    iri.fRecording = recording.get();
    if (!ctx->insertRecording(iri)) return 1;
    if (!ctx->submit({skgpu::graphite::SyncToCpu::kYes})) return 2;

    SkBitmap bm; bm.allocPixels(info);
    if (!surface->readPixels(bm, 0, 0)) return 3;

    SkColor px = bm.getColor(128, 128);
    return (SkColorGetR(px) > 200 && SkColorGetG(px) < 50 && SkColorGetB(px) < 50) ? 0 : 4;
}
```

Build with the same toolchain ninja used to build Skia, link against `libSkiaSharp.so`. Pass means: Graphite is reachable in this environment.

## Layer 2 — C smoke

**Location**: `tests/native/Graphite/c_smoke/`

Same flow expressed entirely through `sk_graphite_*` and existing `sk_*` C entry points:

```c
// graphite_c_smoke.c  (essence)
#include "sk_canvas.h"
#include "sk_surface.h"
#include "sk_graphite.h"
#include "sk_graphite_vulkan.h"

int main(void) {
    if (!sk_graphite_backend_is_available(SK_GRAPHITE_BACKEND_VULKAN)) return 10;

    sk_graphite_vk_backend_context_init_t init = make_lavapipe_init();
    sk_graphite_vk_backend_context_t* bc = sk_graphite_vk_backend_context_new(&init);
    sk_graphite_context_t* ctx = sk_graphite_context_make_vulkan(bc, NULL);
    sk_graphite_recorder_t* rec = sk_graphite_context_make_recorder(ctx, -1);

    sk_imageinfo_t info = make_n32_premul(256, 256);
    sk_surface_t* surf = sk_graphite_surface_make_render_target(rec, &info, 0, NULL);
    /* draw via sk_canvas_*  ... */

    sk_graphite_recording_t* rcd = sk_graphite_recorder_snap(rec);
    sk_graphite_insert_recording_info_t iri = { .fRecording = rcd };
    sk_graphite_insert_status_t st = sk_graphite_context_insert_recording(ctx, &iri);
    if (st != SK_GRAPHITE_INSERT_STATUS_SUCCESS) return 11;
    sk_graphite_submit_info_t si = { .fSync = SK_GRAPHITE_SYNC_TO_CPU_YES };
    if (!sk_graphite_context_submit(ctx, &si)) return 12;

    /* readPixels via sk_surface_read_pixels, assert red */
    /* delete in reverse: surface, rcd, rec, ctx, bc */
}
```

Built with a tiny CMake project (or hand-rolled clang invocation) under `tests/native/Graphite/c_smoke/CMakeLists.txt`, linked against the same `libSkiaSharp.so`.

## Layer 3 — C# smoke

**Location**: `tests/SkiaSharp.Tests/Graphite/GraphiteSmokeTests.cs`

```csharp
using SkiaSharp;
using Xunit;

public class GraphiteSmokeTests : BaseTest
{
    [SkippableFact]
    public void Vulkan_Graphite_DrawAndReadBack_RedRRect()
    {
        Skip.IfNot(SKGraphiteContext.IsBackendAvailable(SKGraphiteBackend.Vulkan), "Vulkan/Lavapipe not available");

        var bc = LavapipeFixture.CreateVkBackendContext();
        using var ctx = SKGraphiteContext.CreateVulkan(bc);
        Assert.NotNull(ctx);

        using var rec = ctx!.CreateRecorder();
        using var surface = SKSurface.Create(rec, new SKImageInfo(256, 256, SKColorType.Rgba8888, SKAlphaType.Premul));
        Assert.NotNull(surface);

        var canvas = surface!.Canvas;
        canvas.Clear(SKColors.White);
        using var paint = new SKPaint { Color = SKColors.Red, IsAntialias = true };
        canvas.DrawRoundRect(SKRect.Create(32, 32, 192, 192), 24, 24, paint);

        using var recording = rec.Snap();
        Assert.Equal(SKGraphiteInsertStatus.Success, ctx.InsertRecording(recording!));
        Assert.True(ctx.Submit(new SKGraphiteSubmitInfo { Sync = true }));

        using var snap = surface.Snapshot();
        using var pixels = snap.PeekPixels();
        var px = pixels.GetPixelColor(128, 128);
        Assert.True(px.Red > 200 && px.Green < 50 && px.Blue < 50, $"unexpected pixel {px}");
    }
}
```

Run:

```bash
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj --filter "FullyQualifiedName~GraphiteSmokeTests"
```

## Bisecting a failure

| Layer | Passes | Layer | Passes | Verdict |
|---|---|---|---|---|
| C++ | ❌ | — | — | Build flags / Lavapipe / Skia integration is broken |
| C++ | ✅ | C | ❌ | C shim has a bug (struct layout, missing init, misuse of Skia C++ types) |
| C | ✅ | C# | ❌ | P/Invoke signature mismatch, marshalling, or managed wrapper logic |

Run each layer in isolation; do not skip layers when diagnosing.

## Once green

1. Add disposal-ordering tests (dispose context first; assert dependents throw `ObjectDisposedException`).
2. Add ownership-transfer tests for `SKGraphiteBackendTexture`.
3. Add `GRContext` + `SKGraphiteContext` coexistence test in the same fixture.
4. Wire CI (Linux runner) to the same `dotnet test` command above.
5. Bring up Metal smoke on macOS and Dawn smoke wherever a Dawn-capable host exists (follow-up to v1).
