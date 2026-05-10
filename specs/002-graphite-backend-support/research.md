# Phase 0 Research: Graphite Backend Support

**Date**: 2026-04-30 · **Branch**: `002-graphite-backend-support` · **Spec**: [spec.md](spec.md)

This document resolves every "NEEDS CLARIFICATION" implied by the Technical Context in [plan.md](plan.md). All decisions below are inputs to Phase 1 design.

---

## R1 — How is Graphite enabled in Skia's build, and is it currently wired through the SkiaSharp build?

**Decision**: Add three new SkiaSharp build arguments — `SUPPORT_GRAPHITE`, `SUPPORT_DAWN`, and (already-present) `SUPPORT_VULKAN` / `SUPPORT_METAL` — and inject the corresponding GN flags into the per-platform `build.cake` files. Graphite is OFF by default initially; once smoke tests are green it flips to ON for platforms that support it.

**Rationale**:

- Skia exposes Graphite via GN args declared in `externals/skia/gn/skia.gni`:
  - `skia_enable_graphite` (line 100, default `false`) — master switch.
  - `skia_use_dawn` (line 47, default `false`) — pulls in Dawn; line 185 asserts `!skia_use_dawn || skia_enable_graphite`.
  - `skia_use_vulkan` (line 139), `skia_use_metal` (line 76), `skia_use_webgpu` (line 90, default `is_wasm`) — backend toggles.
- SkiaSharp's existing native build (e.g. `native/linux/build.cake:99-115`) already injects flags for `skia_enable_ganesh` and `skia_use_vulkan`. The pattern is mature and well-suited to adding more.
- Direct3D is excluded because Skia has no Graphite/D3D backend (`gn/skia.gni:192`).

**Alternatives considered**:

- *One global `SUPPORT_GRAPHITE` flag that auto-enables every available backend.* Rejected: each backend has its own Skia source dependency cost (Dawn pulls Tint, Tint pulls SPIRV-Tools…). Per-backend flags let CI build only what each runner can test, mirroring the existing Ganesh pattern.
- *A separate `libSkiaSharp.Graphite` native library.* Rejected: doubles the SkiaSharp NuGet matrix and forces apps to choose at link time. Compiling Graphite *into* `libSkiaSharp` (gated by per-backend flags at build time) keeps a single binary per platform and matches FR-018 (conditional compilation, no dangling symbols).

---

## R2 — How do we exercise Graphite end-to-end on a Linux WSL2 dev machine that has no GPU?

**Decision**: Use **Vulkan via Mesa Lavapipe** as the canonical headless-validation path. Lavapipe is a CPU-only Vulkan ICD shipped by Mesa; SkiaSharp already enables Vulkan on Linux, so adding Lavapipe is purely a runtime-environment concern (install `mesa-vulkan-drivers` / equivalent, set `VK_ICD_FILENAMES` if needed). The Graphite C++ smoke, the C smoke, and the C# smoke all default to creating a Graphite Context over Vulkan.

**Rationale**:

- A Graphite `Context` *requires* a real backend — Dawn, Metal, or Vulkan. The CPU recorder (`Context::makeCPURecorder()`, see `externals/skia/include/gpu/graphite/Context.h`) draws *through* a Context; it does not let you skip context creation. Skia's own test `externals/skia/tests/graphite/GraphiteContextRecorderTest.cpp:25-46` confirms this: it still uses `DEF_GRAPHITE_TEST_FOR_ALL_CONTEXTS`, which iterates whatever GPU backends are buildable on the host.
- Lavapipe is the minimum external dependency that satisfies "real Vulkan device" and runs in headless containers / WSL2.
- SwiftShader is a viable alternative but is heavier (Skia-side build glue and ABI considerations).
- Dawn does not expose a stable user-facing "null adapter" today; its build-time `null` backend is for Dawn's own tests.

**Alternatives considered**:

- *Skia's `skcpu::Recorder` + a Context that we never actually ask to render anything.* Rejected: that does not exercise insertRecording / submit, so it does not validate the parts of the C API most likely to misbehave.
- *Defer headless validation; require GPU CI for any Graphite work.* Rejected: directly contradicts the user's instruction in this command ("Do whatever is necessary to get Graphite working in this environment even without a GPU").
- *Dawn null backend.* Rejected for v1: not exposed as a stable runtime BackendType; would couple our smoke story to internal Dawn internals.

**Operational note**: After v1, we can additionally bring up SwiftShader for Vulkan and a Dawn-WebGPU smoke when Dawn's adapter story stabilizes for headless, but neither blocks v1.

---

## R3 — What is the minimum useful v1 C++ surface to wrap?

**Decision**: Wrap exactly the surface that the spec's FR-001 through FR-012 require — Context, Recorder, Recording, Surface, Image, BackendTexture, TextureInfo, ContextOptions, plus the per-backend BackendContext value types (Dawn, Metal, Vulkan). Defer everything else.

**Concrete v1 surface (each item maps to one or more C entry points in Phase 1):**

| Skia C++ symbol | Header | C API method intent |
|---|---|---|
| `skgpu::graphite::ContextFactory::MakeDawn / MakeMetal / MakeVulkan` | `gpu/graphite/{dawn,mtl,vk}/...` | One factory per backend; takes a backend context value and a `ContextOptions` value; returns a `sk_graphite_context_t*` or null on failure |
| `Context::makeRecorder(RecorderOptions)` | `gpu/graphite/Context.h` | `sk_graphite_context_make_recorder` |
| `Context::insertRecording(InsertRecordingInfo)` | `gpu/graphite/Context.h` | `sk_graphite_context_insert_recording` |
| `Context::submit(SubmitInfo)` | `gpu/graphite/Context.h` | `sk_graphite_context_submit` |
| `Context::isDeviceLost`, `maxTextureSize`, `supportsProtectedContent`, `freeGpuResources`, `deleteBackendTexture`, `currentBudgetedBytes`, `setMaxBudgetedBytes` | `gpu/graphite/Context.h` | Direct wrappers |
| `Recorder::snap()` | `gpu/graphite/Recorder.h` | `sk_graphite_recorder_snap` |
| `Recorder::createBackendTexture / updateBackendTexture / deleteBackendTexture` | `gpu/graphite/Recorder.h` | Direct wrappers |
| `Recorder::makeDeferredCanvas` | `gpu/graphite/Recorder.h` | `sk_graphite_recorder_make_deferred_canvas` |
| `Recording` lifetime | `gpu/graphite/Recording.h` | `sk_graphite_recording_delete` |
| `SkSurfaces::RenderTarget(Recorder*, SkImageInfo, …)` | `gpu/graphite/Surface.h` | `sk_graphite_surface_make_render_target` |
| `SkSurfaces::WrapBackendTexture(Recorder*, BackendTexture, …)` | `gpu/graphite/Surface.h` | `sk_graphite_surface_wrap_backend_texture` |
| `SkSurfaces::AsImage(SkSurface)` / `AsImageCopy` | `gpu/graphite/Surface.h` | `sk_graphite_surface_as_image{,_copy}` |
| `SkImages::WrapTexture(Recorder*, BackendTexture, …)` | `gpu/graphite/Image.h` | `sk_graphite_image_wrap_texture` |
| `BackendTexture` ctor/dtor/queries | `gpu/graphite/BackendTexture.h` | Value-type wrappers (`sk_graphite_backend_texture_*`) |
| `TextureInfo` ctor/queries (per backend, via `TextureInfos::MakeVulkan`, `BackendTextures::MakeVulkan`, etc.) | `gpu/graphite/{vk,dawn,mtl}/...Types.h` | Per-backend constructor functions returning a `sk_graphite_texture_info_t*` |
| `DawnBackendContext`, `MtlBackendContext`, `skgpu::VulkanBackendContext` | per-backend headers | Per-backend constructor functions taking primitive handles (`VkInstance`, `VkPhysicalDevice`, `VkDevice`, `VkQueue`, etc.) and returning a `sk_graphite_dawn_backend_context_t*` etc. |

**Rationale**: This is exactly the "context → recorder → snap → insert → submit; wrap textures; create surfaces and images" cycle the spec's user stories all rely on. Anything beyond it is a pure addition that doesn't block FR-001…FR-012.

**Explicitly deferred**: `PrecompileContext`, `PersistentPipelineStorage`, `ImageProvider`, `YUVABackendTextures`, `ShaderErrorHandler`, `SkCapture`, `MutableTextureState`, `BackendSemaphore` (no semaphore-based external sync in v1), `asyncRescaleAndReadPixels*` (sync readback only in v1), `PromiseTextureFrom`, `addFinishInfo` callback overload. All can be added incrementally in follow-up features.

---

## R4 — How does the C API integrate with the existing SkiaSharp shim, and how are bindings generated?

**Decision**:

- New C headers live in `externals/skia/include/c/sk_graphite.h` (core), and per-backend headers `sk_graphite_dawn.h`, `sk_graphite_metal.h`, `sk_graphite_vulkan.h`. New implementations in `externals/skia/src/c/sk_graphite.cpp` (core) and matching backend-suffixed `.cpp` files. Each `.cpp` is bracketed by `#if SK_GRAPHITE` / `#if SK_DAWN` / `#if SK_VULKAN` / `#if SK_METAL` so platforms that don't enable a given backend link cleanly (FR-018).
- C# P/Invoke signatures are produced through the existing `pwsh ./utils/generate.ps1` flow. `utils/SkiaSharpGenerator` already understands the `sk_<type>_<action>` convention; no generator changes are expected for v1.
- Hand-written wrappers go under a new `binding/SkiaSharp/Gpu/Graphite/` subdirectory: `SKGraphiteContext.cs`, `SKGraphiteRecorder.cs`, `SKGraphiteRecording.cs`, plus per-backend `SKGraphiteDawnBackendContext.cs`, `SKGraphiteMtlBackendContext.cs`, `SKGraphiteVkBackendContext.cs`. Class prefix is `SKGraphite*` rather than `GR*` to avoid colliding with Ganesh names.
- Surface and Image factory methods are added as **new overloads** on `SKSurface` and `SKImage` taking a `SKGraphiteRecorder` (FR-013: ABI stability — no signature changes to existing methods).

**Rationale**:

- Mirrors the file layout already used for the C shim (`sk_canvas.cpp`, `gr_context.cpp`, etc.) and for the C# binding.
- Choosing `SKGraphite` as a prefix keeps every new public type recognizable, avoids `GR` (Ganesh) collisions, and matches Skia's own `skgpu::graphite::` namespace.
- Putting Surface/Image factories on the existing `SKSurface`/`SKImage` types keeps caller code symmetrical with Ganesh (`SKSurface.Create(GRContext, …)` ↔ `SKSurface.Create(SKGraphiteRecorder, …)`).

**Alternatives considered**:

- *Standalone `SKGraphiteSurface` / `SKGraphiteImage` types.* Rejected: forces callers to convert to `SKImage`/`SKSurface` everywhere; doesn't match SkiaSharp's existing layered design.
- *Separate `SkiaSharp.Graphite` NuGet package.* Rejected for v1: adds packaging churn before we know the shape works. Can split later if needed.

---

## R5 — What three-layer validation strategy satisfies FR-019 *and* the user's "C++ → C → C#" instruction?

**Decision**: Three nested smoke harnesses, each replayable in isolation:

1. **C++ smoke** (`tests/native/Graphite/cpp_smoke/`): A standalone executable that creates a Vulkan `skgpu::VulkanBackendContext`, calls `ContextFactory::MakeVulkan`, makes a recorder, draws an `SkRRect` into a `SkSurfaces::RenderTarget`, snaps & inserts the recording, submits with `SyncToCpu::kYes`, reads back via `SkSurface::readPixels`, and asserts a known color at known pixel coordinates. Built directly via the same GN/ninja invocation as Skia (or a tiny CMake project linking to the freshly built `libSkiaSharp.so` headers). **Purpose**: prove Lavapipe + the build flags work before we touch any C-shim code.
2. **C smoke** (`tests/native/Graphite/c_smoke/`): Same scenario, expressed entirely through the new `sk_graphite_*` C API, linked against `libSkiaSharp.so` (the actual binary the C# layer will load). **Purpose**: prove the C API's ownership semantics, error returns, and conditional-compilation gates work as designed.
3. **C# smoke** (`tests/SkiaSharp.Tests/Graphite/`): Same scenario, expressed through the new `SKGraphite*` C# types, run inside the existing `SkiaSharp.Tests.Console` xUnit harness with a `[SkippableFact]` that auto-skips if Vulkan isn't available. **Purpose**: prove the full P/Invoke surface plus disposal semantics, exception paths, and `[Fact]`-level integration.

Each layer has a "draw and read back" minimum, plus targeted disposal-ordering and ownership-transfer cases per FR-019.

**Rationale**: Bisects failure causes cleanly. If the C smoke breaks but the C++ smoke passes, the bug is in the C shim. If the C# smoke breaks but the C smoke passes, the bug is in P/Invoke or the managed wrappers. This is exactly the layering the user asked for.

**Alternatives considered**:

- *Skip the C smoke; go straight from C++ to C#.* Rejected: would conflate generator/P/Invoke bugs with C-shim bugs and double the time-to-find-bug for the most error-prone layer.

---

## R6 — How is the existing Ganesh test surface kept regression-free (FR-020 / SC-003)?

**Decision**: No code changes to any existing `GR*` source file or to any existing test. Graphite types live in new files under new namespaces in C# (`SkiaSharp` namespace, `SKGraphite*` prefix), and behind new GN args in C++. The existing test runs (`dotnet test tests/SkiaSharp.Tests.Console/...`) and the existing Vulkan test console (`tests/SkiaSharp.Vulkan.Tests.Console/`) continue to be the regression gate; CI runs them unchanged.

**Rationale**: The cheapest possible compliance with FR-013 is "do not touch the existing files." The only seams that *could* break are the few new overloads on `SKSurface` / `SKImage`. Those are pure additions and cannot affect existing call sites by ABI rules.

---

## Summary of decisions feeding Phase 1

- Build: add `SUPPORT_GRAPHITE`, per-backend flags; per-platform cake files; defaults OFF until smoke-green.
- Headless validation: Lavapipe Vulkan ICD, in CI and dev.
- Surface area: scoped exactly to spec FR-001…FR-012; advanced features deferred.
- File layout: `externals/skia/{include,src}/c/sk_graphite{,_dawn,_metal,_vulkan}.{h,cpp}`; managed code under `binding/SkiaSharp/Gpu/Graphite/`.
- Naming: `SKGraphite*` prefix in C#; `sk_graphite_*` in C; matches existing conventions without colliding with `GR*`.
- Validation strategy: three-layer smoke (C++, C, C#) before declaring done.
- Regression strategy: zero edits to existing `GR*` files; new methods only as overloads.
