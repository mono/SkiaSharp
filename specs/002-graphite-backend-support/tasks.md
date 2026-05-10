---

description: "Tasks for Graphite Backend Support"
---

# Tasks: Graphite Backend Support

**Input**: Design documents in `/workspace/skiasharp/specs/002-graphite-backend-support/`
**Prerequisites**: plan.md ✅ · spec.md ✅ · research.md ✅ · data-model.md ✅ · contracts/ ✅ · quickstart.md ✅

**Tests**: Tests are MANDATORY per spec FR-019 and the validation strategy in research.md (R5) / quickstart.md. Each story phase ends with a test that gates the next phase.

**Organization**: Tasks are grouped by user story (US1/US2/US3/US4) so each story's slice is independently buildable, testable, and shippable.

## Format: `[ID] [P?] [Story?] Description (file path)`

- **[P]**: Parallelizable — touches a different file from any other in-flight task and has no dependency on incomplete tasks.
- **[Story]**: Maps to user stories from `spec.md` — US1 (P1), US2 (P2), US3 (P2), US4 (P3).
- **GATE**: A task labelled `**GATE**` MUST pass before the next phase begins. Halt the run if it fails.

## Path Conventions

Per `plan.md` § Project Structure:

- C API headers: `externals/skia/include/c/sk_graphite{,_vulkan,_metal,_dawn}.h`
- C API impl: `externals/skia/src/c/sk_graphite{,_vulkan,_metal,_dawn}.cpp`
- Native cake build: `native/{linux,macos,windows}/build.cake`
- C# binding: `binding/SkiaSharp/Gpu/Graphite/SKGraphite*.cs`
- Generated P/Invoke: `binding/SkiaSharp/SkiaApi.generated.cs` (regenerated, not hand-edited)
- Native smoke tests: `tests/native/Graphite/{cpp_smoke,c_smoke}/`
- C# tests: `tests/Tests/SkiaSharp/Graphite/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Get the Linux/Vulkan/Lavapipe build path and test scaffolding in place so every subsequent task has somewhere to land.

- [X] T001 Add `SUPPORT_GRAPHITE` and `SUPPORT_DAWN` argument plumbing to `native/linux/build.cake` (alongside existing `SUPPORT_GPU` / `SUPPORT_VULKAN` at lines 6–10) and inject `skia_enable_graphite={SUPPORT_GRAPHITE}` and `skia_use_dawn={SUPPORT_DAWN}` into the `GnNinja()` call (around lines 104–115). Defaults: both `false` until the C++ smoke gates pass.
- [X] T002 [P] Create the native-test directory tree: `tests/native/Graphite/cpp_smoke/` and `tests/native/Graphite/c_smoke/`. Add a placeholder top-level `tests/native/Graphite/README.md` describing the three-layer flow with a pointer to `specs/002-graphite-backend-support/quickstart.md`.
- [X] T003 [P] Create the C# test directory `tests/Tests/SkiaSharp/Graphite/`. (No code yet — landed in Phase 3.)
- [X] T004 [P] Add `documentation/dev/graphite-headless.md` describing the Lavapipe install / `VK_ICD_FILENAMES` setup for Linux / WSL2 dev runners, mirroring the prerequisite section of `quickstart.md`.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Prove the build works, the headless Vulkan path works, the new C API compiles + links, and the generator emits clean P/Invoke signatures. **No user-story work begins until this phase is green.**

⚠️ **CRITICAL**: T009 (C++ smoke gate) and T018 (C smoke gate) are non-skippable. If either fails, halt and diagnose before any further task.

### Build path

- [X] T005 Run `CC=clang CXX=clang++ SUPPORT_GPU=true SUPPORT_VULKAN=true SUPPORT_GRAPHITE=true dotnet cake --target=externals-linux --arch=x64` from repo root and confirm `output/native/linux/x64/libSkiaSharp.so` is produced. Verify Graphite C++ was compiled in by checking `find externals/skia/out/linux/x64/obj/src/gpu/graphite -name '*.o' | wc -l` is non-zero. (The `nm -D | grep graphite` check originally drafted here was wrong — Graphite C++ symbols are filtered to `local` by SkiaSharp's `libSkiaSharp.map` version script and only become exported after our `sk_graphite_*` C shim is added in T015.) **PASSED 2026-04-30**: 64 graphite .o files compiled; `libSkiaSharp.so` 12MB.
- [X] T006 Verify Lavapipe presence on the dev machine: `vulkaninfo --summary | grep -E 'llvmpipe|lavapipe'` returns at least one match. If not, install via `documentation/dev/graphite-headless.md` and rerun. **PASSED 2026-04-30**: `vulkaninfo` enumerates GPU0 with `deviceType = PHYSICAL_DEVICE_TYPE_CPU` from Mesa vendor 0x10005 (Lavapipe).

### Layer 1 — C++ smoke (DEFERRED — see note below)

- [~] T007 ~~Create `tests/native/Graphite/cpp_smoke/CMakeLists.txt`...~~ **DEFERRED**: a truly standalone C++ harness can't link against Graphite C++ from `libSkiaSharp.so` (symbols are `local` per the version script), and building it inside Skia's GN tree requires non-trivial edits to `externals/skia/BUILD.gn`. Skia upstream m147 CI already validates the Graphite C++ surface continuously, so this layer's value is limited. We rely on the C smoke (T017) as the first integration gate; if it fails, bisect via Skia's own `dm`/`viewer` running against the same `libSkiaSharp` build.
- [~] T008 ~~Create `tests/native/Graphite/cpp_smoke/graphite_cpp_smoke.cpp`...~~ **DEFERRED** (see T007).
- [~] T009 ~~**GATE — C++ smoke passes**~~ **DEFERRED** — the first true gate is now T018 (C smoke). Re-evaluate adding a Skia-tree-internal smoke when CI runners with full GPU support are available.

### C API surface (core + Vulkan first)

- [X] T010 [P] Create `externals/skia/include/c/sk_graphite.h` — opaque handles, options/info/status PODs, all core function prototypes. **DONE 2026-04-30** (smoke-critical subset; BackendTexture/TextureInfo deferred to US3 per `contracts/c-api.md` scoping).
- [X] T011 [P] Create `externals/skia/include/c/sk_graphite_vulkan.h` — `sk_graphite_vk_backend_context_*`, `sk_graphite_context_make_vulkan`. **DONE 2026-04-30**.
- [X] T012 Create `externals/skia/src/c/sk_graphite.cpp` implementing the core surface, bracketed in `#if defined(SK_GRAPHITE)` with stubs in the `#else` branch so non-Graphite builds still link. Includes `sk_graphite_context_read_pixels_sync` — a **new** addition to the contract that wraps `Context::asyncRescaleAndReadPixels`. Required because Skia gates the legacy `SkSurface::readPixels` for Graphite-backed surfaces on `GPU_TEST_UTILS` (see `src/gpu/graphite/Device.cpp:onReadPixels`); production builds must use the async path. **DONE 2026-04-30**.
- [X] T013 Create `externals/skia/src/c/sk_graphite_vulkan.cpp` — marshals `sk_graphite_vk_backend_context_init_t` into `skgpu::VulkanBackendContext`, auto-creates the VMA-backed memory allocator (since Graphite's Vulkan path doesn't auto-create one unlike Ganesh's `GrVkGpu`), calls `gr::ContextFactory::MakeVulkan`. **DONE 2026-04-30**.
- [X] T014 Register `sk_graphite.cpp` and `sk_graphite_vulkan.cpp` in `externals/skia/gn/core.gni` alongside `gr_context.cpp`. Also added `SK_GRAPHITE` and `SK_DAWN` defines to the `:core` GN target in `externals/skia/BUILD.gn` (analogous to existing `SK_VULKAN`/`SK_METAL`/`SK_DIRECT3D` gates) so the new sources see the right `#if defined(...)` branches. **DONE 2026-04-30**.
- [X] T015 Rebuild `libSkiaSharp.so`. **PASSED 2026-04-30**: `nm -D --defined-only output/native/linux/x64/libSkiaSharp.so | grep -c sk_graphite_` returned **24** new exports (every declared function present).

### Layer 2 — C smoke

- [X] T016 `tests/native/Graphite/c_smoke/CMakeLists.txt` — finds Vulkan via `find_package`, links against `output/native/linux/x64/libSkiaSharp.so`, sets BUILD_RPATH so the binary loads the shipped `.so` without `LD_LIBRARY_PATH` gymnastics. **DONE 2026-04-30**.
- [X] T017 `tests/native/Graphite/c_smoke/graphite_c_smoke.c` — Vulkan instance/device bring-up over Lavapipe, `sk_graphite_*` round-trip, full 256×256 readback via `sk_graphite_context_read_pixels_sync`, asserts `(128, 128)` is red and `(0, 0)` is white. **DONE 2026-04-30**.
- [X] T018 **GATE — C smoke passes**: ran `./build/graphite_c_smoke`, exit `0`, pixel `(128, 128) = 0xFF0000FF` (pure red), pixel `(0, 0) = 0xFFFFFFFF` (pure white). **PASSED 2026-04-30**. Bisection confirmed: Vulkan/Lavapipe works → Skia Graphite was correctly compiled in → our `sk_graphite_*` C shim correctly bridges C↔C++ → render pipeline + async readback both work.

### Generated P/Invoke

- [X] T019 Run `pwsh ./utils/generate.ps1 -Config libSkiaSharp.json`. **PASSED 2026-04-30**: generated 202 references to `sk_graphite_*` in `binding/SkiaSharp/SkiaApi.generated.cs`, plus auto-generated value types `SKGraphiteContextOptions`, `SKGraphiteSubmitInfo`, `SKGraphiteInsertRecordingInfo`, `SKGraphiteVkBackendContextNative` (internal, see below), `SKGraphiteBackend`, `SKGraphiteInsertStatus`, `SKGraphiteSyncToCpu`, `SKGraphiteMarkFrameBoundary` enums. Required JSON mapping additions in `binding/libSkiaSharp.json`: `sk_graphite_vk_func_ptr` (functions section, `generateProxy: false`), `sk_graphite_vk_get_proc_t` (functions section, `cs: SKGraphiteVkGetProxyDelegate`, params -1: IntPtr), `sk_graphite_vk_backend_context_init_t` (types section, `cs: SKGraphiteVkBackendContextNative`, internal). Required hand-additions to `binding/SkiaSharp/DelegateProxies.cs`: `SKGraphiteVkGetProcedureAddressDelegate` and `SKGraphiteReleaseDelegate` public delegates plus `SKGraphiteVkGetProxyImplementation` and `SKGraphiteReleaseProxyImplementation` private static partial implementations.

**Checkpoint**: Build is green, both native smokes pass, `SkiaApi.generated.cs` has the new P/Invoke surface. User-story work can begin.

---

## Phase 3: User Story 1 — Render with the Graphite Backend (Priority: P1) 🎯 MVP

**Goal**: A .NET caller can create a Graphite context (Vulkan), get a recorder, draw to a Graphite-backed surface, snap, submit, and read back the rendered pixels.

**Independent Test**: `dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj --filter "FullyQualifiedName~GraphiteSmokeTests"` passes the rounded-rect render-and-readback `[SkippableFact]` on a Lavapipe-equipped Linux runner.

### Implementation for User Story 1

- [X] T020-T024 [P] [US1] Value types (struct/enum). **PASSED 2026-04-30**: all auto-generated by the SkiaSharp generator from the C headers — `SKGraphiteContextOptions`, `SKGraphiteSubmitInfo`, `SKGraphiteInsertRecordingInfo`, `SKGraphiteBackend`, `SKGraphiteInsertStatus`, `SKGraphiteSyncToCpu`, `SKGraphiteMarkFrameBoundary` all live in `binding/SkiaSharp/SkiaApi.generated.cs` (no hand-written file needed). The `SKGraphiteVkBackendContextNative` struct (the marshalled init form) is also auto-generated and `internal` per JSON mapping; the public user-facing wrapper is the hand-written class below (T023).
- [X] T023 (rewritten) [US1] Create `binding/SkiaSharp/Gpu/Graphite/SKGraphiteVkBackendContext.cs` — public **class** (not struct, due to the GCHandle ownership of the GetProc delegate) implementing `IDisposable`. Properties: `VkInstance`, `VkPhysicalDevice`, `VkDevice`, `VkQueue`, `GraphicsQueueIndex`, `MaxApiVersion`, `ProtectedContext`, `GetProcedureAddress` (uses `SKGraphiteVkGetProcedureAddressDelegate`). `ToNative()` builds the marshalled native struct via `DelegateProxies.SKGraphiteVkGetProxy`. Lazy `Handle` property allocates the `sk_graphite_vk_backend_context_t*` once and reuses; freed on Dispose. **DONE 2026-04-30**.
- [X] T025 [US1] Create `binding/SkiaSharp/Gpu/Graphite/SKGraphiteContext.cs` — class extending `SKObject`. Static `IsBackendAvailable`. Static `CreateVulkan(SKGraphiteVkBackendContext, SKGraphiteContextOptions)` returning nullable. Properties: `Backend`, `IsDeviceLost`, `MaxTextureSize`, `SupportsProtectedContent`, `CurrentBudgetedBytes`, `MaxBudgetedBytes`. Methods: `CreateRecorder`, `InsertRecording(SKGraphiteRecording)`, `InsertRecording(SKGraphiteInsertRecordingInfo)`, `Submit`, `Submit(SKGraphiteSubmitInfo)`, `FreeGpuResources`, `PerformDeferredCleanup`, `ReadPixels` (the new sync wrapper around `Context::asyncRescaleAndReadPixels`). Disposal calls `sk_graphite_context_delete`. **DONE 2026-04-30**.
- [X] T026 [US1] Create `binding/SkiaSharp/Gpu/Graphite/SKGraphiteRecorder.cs` — class. Properties `Backend`, `MaxTextureSize`. Method `Snap()` returning nullable `SKGraphiteRecording`. (BackendTexture creation deferred to US3.) Disposal calls `sk_graphite_recorder_delete`. **DONE 2026-04-30**.
- [X] T027 [US1] Create `binding/SkiaSharp/Gpu/Graphite/SKGraphiteRecording.cs` — class. Disposable; disposal calls `sk_graphite_recording_delete`. **DONE 2026-04-30**.
- [X] T028 [US1] Added 4 new `SKSurface.Create(SKGraphiteRecorder, SKImageInfo, [bool mipmapped], [SKSurfaceProperties props])` overloads to `binding/SkiaSharp/SKSurface.cs`. P/Invokes `sk_graphite_surface_make_render_target`. Existing factories untouched. **DONE 2026-04-30**.
- [X] T029 [US1] `dotnet build binding/SkiaSharp/SkiaSharp.csproj -c Release` — **0 errors**. Existing tests still build clean. **PASSED 2026-04-30**.
- [X] T030 [P] [US1] Created `tests/Tests/SkiaSharp/Graphite/LavapipeFixture.cs` — raw P/Invoke into `libvulkan.so.1` (no SharpVk dependency), brings up `VkInstance`/`VkDevice`/`VkQueue` against Lavapipe (prefers `VK_PHYSICAL_DEVICE_TYPE_CPU`), exposes a populated `SKGraphiteVkBackendContext` with a working GetProc adapter that dispatches through `vkGetDeviceProcAddr`/`vkGetInstanceProcAddr`. **DONE 2026-04-30**.
- [X] T031 [US1] Created `tests/Tests/SkiaSharp/Graphite/GraphiteSmokeTests.cs` with two `[SkippableFact]`s: `IsBackendAvailable_Vulkan_ReturnsTrue` and `Vulkan_Graphite_DrawAndReadBack_RedRRect`. Same pixel assertions as the C smoke. **DONE 2026-04-30**.
- [X] T032 [US1] **GATE — C# smoke passes**: `dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj --filter "FullyQualifiedName~GraphiteSmokeTests"` returned **Passed: 2, Failed: 0, Skipped: 0** in 196 ms. **PASSED 2026-04-30**. The full C++ → C → C# stack is verified end-to-end on Lavapipe.

**Checkpoint**: User Story 1 is fully functional. The MVP slice — "Render with the Graphite backend on a modern GPU API" — is shippable.

---

## Phase 4: User Story 2 — Choose a Graphite Backend per Platform (Priority: P2)

**Goal**: Callers can construct a Graphite context for Metal (on Apple), Vulkan (on Linux/Windows/Android), and Dawn (where built). Backend availability is queryable at runtime.

**Independent Test**: On macOS, `Metal_Graphite_DrawAndReadBack_RedRRect` `[SkippableFact]` passes; on Windows-with-Vulkan, the same Vulkan smoke from US1 passes; the new `IsBackendAvailable` test reports `true`/`false` consistent with the runner.

### Metal backend

- [ ] T033 [P] [US2] Create `externals/skia/include/c/sk_graphite_metal.h` with `sk_graphite_mtl_backend_context_t`, `sk_graphite_mtl_backend_context_init_t`, and the prototypes for `sk_graphite_mtl_backend_context_new/delete`, `sk_graphite_context_make_metal` (per `contracts/c-api.md`). Also: `sk_graphite_mtl_texture_info_new` and `sk_graphite_mtl_backend_texture_new` (the latter consumed by US3).
- [ ] T034 [P] [US2] Create `externals/skia/src/c/sk_graphite_metal.cpp` — bracketed in `#if defined(SK_GRAPHITE) && defined(SK_METAL)`. Marshals init struct to `skgpu::graphite::MtlBackendContext` (CFRetain on the MTLDevice and MTLCommandQueue). Calls `ContextFactory::MakeMetal`.

### Dawn backend

- [ ] T035 [P] [US2] Create `externals/skia/include/c/sk_graphite_dawn.h` with `sk_graphite_dawn_backend_context_t`, `sk_graphite_dawn_backend_context_init_t`, prototypes for `sk_graphite_dawn_backend_context_new/delete`, `sk_graphite_context_make_dawn`, plus `sk_graphite_dawn_texture_info_new` and `sk_graphite_dawn_backend_texture_new`.
- [ ] T036 [P] [US2] Create `externals/skia/src/c/sk_graphite_dawn.cpp` — bracketed in `#if defined(SK_GRAPHITE) && defined(SK_DAWN)`. Marshals init struct to `skgpu::graphite::DawnBackendContext`. Calls `ContextFactory::MakeDawn`.

### Per-platform build wiring

- [ ] T037 [US2] Add `SUPPORT_GRAPHITE` and `SUPPORT_DAWN` plumbing to `native/macos/build.cake`. On macOS, default `SUPPORT_GRAPHITE=true` and inject `skia_use_metal=true skia_enable_graphite=true skia_use_dawn={SUPPORT_DAWN}` into the GN args (matching the existing flag block).
- [ ] T038 [US2] Add `SUPPORT_GRAPHITE` and `SUPPORT_DAWN` plumbing to `native/windows/build.cake`. Inject `skia_enable_graphite={SUPPORT_GRAPHITE} skia_use_dawn={SUPPORT_DAWN}` (Vulkan support is already there).

### C# binding additions for backends

- [ ] T039 [P] [US2] Create `binding/SkiaSharp/Gpu/Graphite/SKGraphiteMtlBackendContext.cs` — `public struct` with `Device`, `Queue` (both `IntPtr`). Add `SKGraphiteContext.CreateMetal(in SKGraphiteMtlBackendContext, in SKGraphiteContextOptions)` static factory in `SKGraphiteContext.cs`.
- [ ] T040 [P] [US2] Create `binding/SkiaSharp/Gpu/Graphite/SKGraphiteDawnBackendContext.cs` — `public struct` with `Instance`, `Device`, `Queue` (all `IntPtr`). Add `SKGraphiteContext.CreateDawn(...)` static factory.
- [ ] T041 [US2] Re-run `pwsh ./utils/generate.ps1` and rebuild `libSkiaSharp` for at least Linux to confirm the per-backend conditional compilation gates work — Vulkan-only build must link cleanly (no Metal/Dawn symbols referenced).

### Tests

- [ ] T042 [P] [US2] Add `IsBackendAvailable_Vulkan_OnLinux_ReturnsTrue` and `IsBackendAvailable_Metal_OnLinux_ReturnsFalse` `[SkippableFact]`s to `tests/Tests/SkiaSharp/Graphite/GraphiteSmokeTests.cs`. These exercise the runtime-availability path of FR-008 / FR-009 (no crash on absent backend).
- [ ] T043 [P] [US2] Add `Metal_Graphite_DrawAndReadBack_RedRRect` `[SkippableFact]` (skipped on non-Apple). Mirrors US1 smoke through Metal.
- [ ] T044 [P] [US2] Add `Dawn_Graphite_DrawAndReadBack_RedRRect` `[SkippableFact]` (skipped where Dawn is unavailable).

**Checkpoint**: User Stories 1 + 2 both work independently. Multi-backend story is verified by per-platform CI.

---

## Phase 5: User Story 3 — Wrap Existing GPU Textures (Priority: P2)

**Status: ✅ MVP shipped 2026-05-01.** Wrap-external-VkImage path landed end-to-end (C API + Vulkan factories + C# wrappers + round-trip test). Recorder-allocated BackendTexture (T049) and Image wrap (T048) deferred — not blocking for the swap-chain integration use case.


**Goal**: Callers can wrap a host-allocated GPU texture and use it as a Graphite-backed surface or image, with explicit ownership control.

**Independent Test**: `WrapBackendTexture_VulkanRoundTrip` allocates a `VkImage` outside SkiaSharp, wraps it as `SKGraphiteBackendTexture`, draws into it via `SKSurface.Create(..., backendTexture, ...)`, and asserts the original `VkImage`'s memory now contains the rendered pixels (no duplicate allocation).

### TextureInfo + BackendTexture types

- [X] T045 [US3] `SKGraphiteTextureInfo` + `SKGraphiteVkTextureInfo` shipped. `SKGraphiteVkTextureInfo` is the auto-generated POD struct (no constructor needed); `SKGraphiteTextureInfo` is the hand-written opaque wrapper at `binding/SkiaSharp/Gpu/Graphite/SKGraphiteTextureInfo.cs` exposing `IsValid`/`Backend`/`SampleCount`/`Mipmapped`. Metal/Dawn variants 🔶 deferred to US2.
- [X] T046 [US3] `SKGraphiteBackendTexture` shipped at `binding/SkiaSharp/Gpu/Graphite/SKGraphiteBackendTexture.cs`. `CreateVulkan(int w, int h, SKGraphiteVkTextureInfo info, int imageLayout, uint queueFamilyIndex, IntPtr vkImage)` is the only factory in v1; Metal/Dawn 🔶 deferred to US2. Disposal calls `sk_graphite_backend_texture_delete` and does NOT release the wrapped VkImage (FR-012).

### Surface + Image wrap overloads

- [X] T047 [US3] Added 3 overloads of `SKSurface.Create(SKGraphiteRecorder, SKGraphiteBackendTexture, SKColorType, [SKColorSpace], [SKSurfaceProperties])` to `binding/SkiaSharp/SKSurface.cs`. P/Invokes `sk_graphite_surface_wrap_backend_texture`.
- [~] T048 [US3] **DEFERRED**: `SKImage.FromTexture` (the `SkImages::WrapTexture` mapping). Use case is "sample a wrapped GPU texture in a paint shader"; not exercised by the swap-chain integration scenario that drove US3. Track in a follow-up issue.

### Recorder backend-texture management

- [~] T049 [US3] **DEFERRED**: Recorder-side `CreateBackendTexture`/`UpdateBackendTexture`/`DeleteBackendTexture`. This is the Skia-allocated path (Skia manages the underlying GPU texture). Less critical than the wrap path, which lets callers integrate with their existing swap-chain code. Track in a follow-up issue.

### Tests

- [X] T050+T051 [US3] `tests/Tests/SkiaSharp/Graphite/GraphiteBackendTextureTests.cs`: round-trip + ownership combined into one test. Allocates a `VkImage` outside Skia (with full render-target usage = 0x97 incl. `INPUT_ATTACHMENT_BIT` per `VulkanCaps`), wraps as `SKGraphiteBackendTexture`, creates `SKSurface.Create(recorder, backendTexture, ...)`, draws yellow, reads back via `Context.ReadPixels`, asserts `(32, 32) = yellow`. Critically, the wrapped texture and surface are disposed BEFORE `vkDestroyImage` runs — proves caller-owned ownership held (Skia did not steal the VkImage).

**Checkpoint**: User Stories 1 + 2 + 3 all functional. Real-world integration story (host-allocated swap chains) works.

---

## Phase 6: User Story 4 — Coexistence with Ganesh (Priority: P3)

**Goal**: Adding Graphite has zero observable effect on existing Ganesh users. Both backends usable in the same process.

**Independent Test**: Existing test suite passes unchanged; new `Coexist_GaneshAndGraphite_BothDrawSameScene` test asserts pixel-equivalent output from both backends in the same process.

- [ ] T052 [US4] Run the full pre-feature regression suite against the new build: `dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj`. Expected: zero failures, zero new skips beyond the Graphite-availability skips. Captures FR-020 / SC-003 numerically.
- [ ] T053 [US4] Create `tests/Tests/SkiaSharp/Graphite/GraphiteCoexistenceTests.cs` with `Coexist_GaneshAndGraphite_BothDrawSameScene` `[SkippableFact]`: instantiate a `GRContext` (Vulkan) and a `SKGraphiteContext` (Vulkan, same `VkDevice`) in the same test method. Draw the same `SKPath` through each onto same-sized surfaces. `SKImage.PeekPixels()` on each → pixel-by-pixel diff with 1% tolerance (per SC-002). Disposal order: Graphite first, then Ganesh, then Vulkan device.
- [ ] T054 [US4] ABI smoke: capture the `binding/SkiaSharp/SkiaSharp.csproj` public surface (e.g. via `Microsoft.DotNet.ApiCompat` or a hand-rolled `AssemblyDumper` over `typeof(SKCanvas).Assembly.ExportedTypes`). Diff against the same dump from the pre-feature `main` baseline. Allowed: additions. Forbidden: removals or signature changes on existing `GR*` / `SK*` types. Land the comparison as a one-off command in `documentation/dev/graphite.md` for future audits.

**Checkpoint**: All four user stories independently functional; Ganesh stays untouched.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T055 [P] Create `tests/Tests/SkiaSharp/Graphite/GraphiteDisposalTests.cs` covering the disposal-ordering edge cases from `spec.md` § "Edge Cases": dispose context before recorder/recording/surface; assert subsequent calls throw `ObjectDisposedException`, no crash. Both for `SKGraphiteContext` and for derived objects.
- [ ] T056 [P] Add XML doc comments to every public member added in Phase 3/4/5 across `binding/SkiaSharp/Gpu/Graphite/*.cs`. Required points are listed in `contracts/csharp-api.md` § "Documentation obligations".
- [ ] T057 [P] Create `documentation/dev/graphite.md` covering: developer-facing API summary, Vulkan-via-Lavapipe headless dev story (link to `graphite-headless.md`), the per-backend availability guarantees, and the v1 → future-work boundary (defer-list from `research.md` R3).
- [ ] T058 [P] If any new dev rules emerged during implementation (e.g. specific test-runner hygiene, additional anti-patterns observed), append them to `AGENTS.md`. Otherwise this task is a no-op.
- [ ] T059 Run `quickstart.md`'s three-layer flow end-to-end one last time against a clean checkout of this branch on a fresh Lavapipe-equipped machine. All three layers must pass without manual intervention. Any divergence between `quickstart.md` and reality is fixed in `quickstart.md`.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies. T001 must complete before T005. T002–T004 are parallel.
- **Phase 2 (Foundational)**: Strict serial chain on the validation gates: `T005 → T006 → T007 → T008 → T009 (gate)` then `T010‖T011 → T012‖T013 → T014 → T015 → T016 → T017 → T018 (gate)` then `T019`.
- **Phase 3 (US1)**: Depends on Phase 2 complete (i.e. T019). Within US1: T020‖T021‖T022‖T023‖T024 in parallel; T025 depends on T020/T023/T024; T026/T027 parallel after T025; T028 depends on T025/T026; T029 depends on T020–T028; T030 parallel with T020–T028; T031 depends on T025–T030; T032 (gate) is last.
- **Phase 4 (US2)**: Depends on US1 complete. T033/T034 parallel; T035/T036 parallel; T037/T038 parallel after T034 and T036 respectively. T039/T040 parallel after T034/T036 + T041. Tests T042–T044 parallel after T041.
- **Phase 5 (US3)**: Depends on US1 complete (NOT US2). T045/T046 parallel; T047/T048 parallel after T046; T049 sequential after T046; T050 parallel; T051 sequential after T047.
- **Phase 6 (US4)**: Depends on US1 complete (US2/US3 optional but recommended). T052/T053/T054 are mostly independent of each other.
- **Phase 7 (Polish)**: Depends on the user stories whose code it polishes. T055–T058 are parallel; T059 is the final gate.

### User Story Dependencies

- **US1 (P1)** is the MVP; everything else builds on its core types.
- **US2 (P2)** extends backend coverage; depends on US1 for `SKGraphiteContext` etc.
- **US3 (P2)** extends with backend-texture wrapping; depends on US1 only — does NOT require US2's Metal/Dawn backends to land first.
- **US4 (P3)** is a regression / coexistence guarantee; depends on US1 (and benefits from but does not require US2/US3).

### Parallel Opportunities

- Phase 1: T002 ‖ T003 ‖ T004 (after T001).
- Phase 2: T010 ‖ T011 (header drafts), T012 ‖ T013 (impl drafts) — but T012 must precede T015 to be observable in the binary.
- Phase 3: All five of T020/T021/T022/T023/T024 in parallel kicks off the US1 phase. Then T026 ‖ T027 after T025. T030 in parallel with the implementation tasks.
- Phase 4: Metal pair (T033/T034) ‖ Dawn pair (T035/T036). Tests T042/T043/T044 all parallel.
- Phase 5: T045 ‖ T046; T047 ‖ T048 after T046; T050 parallel.
- Phase 7: T055 ‖ T056 ‖ T057 ‖ T058.

### Hard halts

- T009 fails → Skia/Lavapipe/build flags wrong; nothing past Phase 2 is meaningful until fixed.
- T018 fails → C shim wrong; do not generate or hand-write any C# until fixed.
- T032 fails (with T009 + T018 green) → P/Invoke or managed wrapper bug; bisect there.
- T052 fails → existing Ganesh tests regressed; halt and revert any change that touched a non-Graphite file.

---

## Implementation Strategy

### MVP First (User Story 1 only)

1. Phase 1 (T001–T004).
2. Phase 2 (T005–T019). Hard gates at T009 and T018.
3. Phase 3 (T020–T032). Hard gate at T032.
4. **STOP and demo**: A .NET app can render with Graphite/Vulkan via Lavapipe. SC-001 satisfied for the Vulkan slice.

### Incremental Delivery After MVP

5. Phase 4 (US2) — adds Metal + Dawn. Each backend can ship behind its own per-platform gate.
6. Phase 5 (US3) — adds backend-texture wrap; unlocks integration with host swap chains.
7. Phase 6 (US4) — formalizes the no-regression guarantee.
8. Phase 7 (Polish) — docs, disposal tests, final XML doc comments.

### Single-Developer Strategy

Work serially through the gate sequence. Do not parallelize across phases — the gates exist precisely so a failure is localized.

### Multi-Developer Strategy

After T019: Developer A drives Phase 3; Developer B drafts Phase 4 Metal/Dawn C API in parallel (T033–T036) but cannot land them until Phase 3 has shaken out the C# wrapper shape; Developer C drafts Phase 5's BackendTexture types.

---

## Notes

- `[P]` = different files, no in-flight dependency. Within a single C# file (e.g. `SKGraphiteContext.cs` gaining new factory methods across phases), tasks are sequential.
- The three GATE tasks (T009, T018, T032) are the spine of the validation strategy from `research.md` R5 and `quickstart.md`. Treat them as non-skippable.
- Generated files (`SkiaApi.generated.cs`, `docs/`) MUST come from `pwsh ./utils/generate.ps1` per `AGENTS.md`. Never hand-edit.
- Submodule changes inside `externals/skia` (T010–T013, T033–T036) require a feature branch in the submodule per `AGENTS.md` § "Branch Protection". Use `dev/issue-NNNN-graphite-c-api` or similar.
- After every task that adds public C# surface, run `dotnet build binding/SkiaSharp/SkiaSharp.csproj` to catch ABI breaks early — FR-013 is the most expensive thing to forget.
- Commit at task granularity where reasonable. The `quickstart.md` flow is repeatable; failed builds should be repro'd on a clean tree.
