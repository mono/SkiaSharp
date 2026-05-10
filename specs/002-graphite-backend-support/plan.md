# Implementation Plan: Graphite Backend Support

**Branch**: `002-graphite-backend-support` · **Date**: 2026-04-30 · **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-graphite-backend-support/spec.md`

## Summary

Expose Skia's new Graphite GPU backend to .NET callers through SkiaSharp by (a) adding a new C API shim around `skgpu::graphite::*` and the per-backend `Make{Vulkan,Metal,Dawn}` factories, and (b) adding C# wrappers (`SKGraphite*` types) that P/Invoke that shim. Roll out is layered: a C++ smoke proves Skia's Graphite builds and runs in this headless environment first; a C smoke proves the new shim's ownership and error contracts; a C# smoke proves the managed binding end-to-end. Validation in this WSL2 dev environment (no GPU) is achieved with **Vulkan via Mesa Lavapipe** — the smallest external dependency that satisfies "real Vulkan device" and runs headless. Direct3D is intentionally excluded because Skia upstream does not ship a Graphite/D3D backend; advanced Graphite features (`PrecompileContext`, `PersistentPipelineStorage`, `ImageProvider`, `YUVABackendTextures`, semaphores, async readback) are scoped out of v1 and tracked for follow-up.

## Technical Context

**Language/Version**: C++20 (Skia + new C shim, matching `externals/skia/gn/skia/BUILD.gn` which enforces `/std:c++20` on MSVC and `-std=c++20` on POSIX) · C 11 (C-shim public headers) · C# 11 / .NET 8.0 (existing SkiaSharp target)
**Primary Dependencies**: Skia at **milestone m147** (Chrome 147; `externals/skia/include/core/SkMilestone.h` → `SK_MILESTONE 147`; submodule HEAD `6f8139adf7 Merge upstream chrome/m147 (#184)`) · Vulkan loader + Lavapipe ICD (CI / dev) · Dawn-WebGPU and Apple Metal SDK (per-platform, gated by build flags) · existing SkiaSharp build pipeline (`dotnet cake`)
**Storage**: N/A
**Testing**:
- C++ smoke: standalone executable in `tests/native/Graphite/cpp_smoke/` linked against `libSkiaSharp.so`
- C smoke: standalone executable in `tests/native/Graphite/c_smoke/` linked against `libSkiaSharp.so`
- C# smoke + regression: xUnit in `tests/SkiaSharp.Tests/Graphite/`, run via `dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj`
**Target Platform**: Linux/x64 (CI primary, Lavapipe headless), macOS/arm64 (Metal), Windows/x64 (Vulkan + Dawn), Android (Vulkan), iOS (Metal). WASM Graphite is out of v1 (re-evaluate when SkiaSharp's WebGPU story matures).
**Project Type**: Cross-platform graphics library (C/C++ native + .NET binding)
**Performance Goals**: Match or improve on the equivalent Ganesh path for the same scene at the same resolution. No regressions in existing Ganesh tests (SC-003).
**Constraints**: ABI-stable (FR-013) — every existing public symbol on `GR*` and existing `SK*` types is preserved. Conditional compilation must keep platforms link-clean even when their backend is disabled (FR-018).
**Scale/Scope**: ~30–40 new public C entry points · ~10 new public C# types + a handful of new overloads on `SKSurface`/`SKImage` · estimated 1.5–2 k LOC new managed code, ~1.5 k LOC new native code (incl. boilerplate).

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

The project's `.specify/memory/constitution.md` is currently the unfilled template (no project-specific principles ratified). In its absence, this plan defers to the durable, codified guidance in `AGENTS.md` (loaded via `CLAUDE.md`) which represents the de facto SkiaSharp constitution:

| AGENTS.md rule | How this plan complies |
|---|---|
| **Bootstrap first; rebuild natives after C-API change** | Plan explicitly rebuilds via `dotnet cake --target=externals-{platform}` after every C-API change. C# smoke is gated on the rebuilt `libSkiaSharp.so`. |
| **Never edit generated files** | New bindings come through `pwsh ./utils/generate.ps1`. Hand-written wrappers go in NEW files only. |
| **ABI stability** | Plan adds new types and overloads only. Section "Coexistence and Stability" of the spec (FR-013…FR-015) is the gating criterion at design review. |
| **Tests are mandatory** | Three-layer smoke is non-skippable; xUnit smoke runs in CI (FR-019, FR-020, SC-005). |
| **Branch protection** | All work on `002-graphite-backend-support`. Submodule changes (if any) must be on a `dev/issue-*` branch in `externals/skia`, not on `main` or `skiasharp`. |
| **Memory management discipline** | C shim follows existing `sk_<type>_<action>` + ownership conventions (see contracts/c-api.md). Same-instance returns flagged where they appear (e.g., `Surface::asImage` may share storage). |
| **No skipping failing tests** | C# smoke uses `[SkippableFact]` only for hardware (Lavapipe absent), not for fixable failures. |

**Gate verdict**: PASS. No deviations needed; no entries in Complexity Tracking.

**Re-check after Phase 1 design**: PASS. Contracts (C / C#) follow the prescribed prefixes (`sk_graphite_*` / `SKGraphite*`), ABI is preserved, conditional-compilation gates are explicit.

## Project Structure

### Documentation (this feature)

```text
specs/002-graphite-backend-support/
├── plan.md                      # this file
├── research.md                  # Phase 0 — build flags, headless story, scope
├── data-model.md                # Phase 1 — entities and lifetimes
├── quickstart.md                # Phase 1 — three-layer smoke runbook
├── contracts/
│   ├── c-api.md                 # Phase 1 — sk_graphite_* signatures and ABI rules
│   └── csharp-api.md            # Phase 1 — SKGraphite* public surface
├── checklists/
│   └── requirements.md          # /speckit-specify quality gate
└── tasks.md                     # Phase 2 — produced by /speckit-tasks (not this command)
```

### Source Code (repository root)

```text
externals/skia/
├── include/c/
│   ├── sk_graphite.h                       # NEW: core C API (context/recorder/recording/surface/image/backend texture)
│   ├── sk_graphite_vulkan.h                # NEW: Vulkan-only entry points
│   ├── sk_graphite_metal.h                 # NEW: Metal-only entry points
│   └── sk_graphite_dawn.h                  # NEW: Dawn-only entry points
├── src/c/
│   ├── sk_graphite.cpp                     # NEW: built when SK_GRAPHITE
│   ├── sk_graphite_vulkan.cpp              # NEW: built when SK_GRAPHITE && SK_VULKAN
│   ├── sk_graphite_metal.cpp               # NEW: built when SK_GRAPHITE && SK_METAL
│   └── sk_graphite_dawn.cpp                # NEW: built when SK_GRAPHITE && SK_DAWN
├── include/gpu/graphite/                   # UPSTREAM: read-only, do not modify
└── src/gpu/graphite/                       # UPSTREAM: read-only, do not modify

native/
├── linux/build.cake                        # MODIFIED: add SUPPORT_GRAPHITE / pass skia_enable_graphite
├── macos/build.cake                        # MODIFIED: same + skia_use_metal/dawn as available
├── windows/build.cake                      # MODIFIED: same + Vulkan/Dawn as available
├── ios/build.cake, tvos/build.cake, …      # MODIFIED if Metal Graphite is in scope per platform
└── wasm/build.cake                         # UNTOUCHED in v1 (Graphite-on-WASM deferred)

binding/SkiaSharp/
├── Gpu/Graphite/
│   ├── SKGraphiteContext.cs                # NEW
│   ├── SKGraphiteRecorder.cs               # NEW
│   ├── SKGraphiteRecording.cs              # NEW
│   ├── SKGraphiteBackendTexture.cs         # NEW
│   ├── SKGraphiteTextureInfo.cs            # NEW (+ per-backend subclasses)
│   ├── SKGraphiteContextOptions.cs         # NEW (struct)
│   ├── SKGraphiteSubmitInfo.cs             # NEW (struct)
│   ├── SKGraphiteInsertRecordingInfo.cs    # NEW (struct)
│   ├── SKGraphiteVkBackendContext.cs       # NEW (struct)
│   ├── SKGraphiteMtlBackendContext.cs      # NEW (struct)
│   └── SKGraphiteDawnBackendContext.cs     # NEW (struct)
├── SkiaApi.generated.cs                    # REGENERATED via pwsh ./utils/generate.ps1
├── SKSurface.cs                            # MODIFIED: add new factory overloads only
└── SKImage.cs                              # MODIFIED: add new factory overloads only

tests/
├── native/Graphite/                        # NEW
│   ├── cpp_smoke/                          # standalone C++ smoke (Layer 1)
│   │   ├── CMakeLists.txt
│   │   └── graphite_cpp_smoke.cpp
│   └── c_smoke/                            # standalone C smoke (Layer 2)
│       ├── CMakeLists.txt
│       └── graphite_c_smoke.c
├── Tests/SkiaSharp/Graphite/               # NEW: xUnit smoke + regression (Layer 3)
│   ├── GraphiteSmokeTests.cs
│   ├── GraphiteDisposalTests.cs
│   ├── GraphiteOwnershipTests.cs
│   ├── GraphiteCoexistenceTests.cs
│   └── LavapipeFixture.cs                  # builds a SKGraphiteVkBackendContext on demand

utils/
└── SkiaSharpGenerator/                     # UNCHANGED in v1 (existing generator handles new sk_graphite_* prototypes)
```

**Structure Decision**: Single project (existing SkiaSharp repository layout). New code is additive in well-defined sub-folders; existing folders are touched only to add overloads, regenerate bindings, and extend per-platform cake build flags. The native test project — `tests/native/Graphite/` — is new but follows the pattern already established by `utils/NativeLibraryMiniTest`.

## Phase 0: Outline & Research

Complete. See [research.md](research.md). Six research questions resolved (R1 build flags · R2 headless validation strategy · R3 v1 surface scoping · R4 generator/file-layout decisions · R5 three-layer validation · R6 regression-prevention strategy). All decisions feed Phase 1.

## Phase 1: Design & Contracts

Complete. Artifacts:

- [data-model.md](data-model.md) — 10 entities, lifetimes, threading rules, cross-entity relationships, edge-case mapping.
- [contracts/c-api.md](contracts/c-api.md) — `sk_graphite_*` C ABI: 4 headers, ~35 public functions, ABI stability rules, test obligations.
- [contracts/csharp-api.md](contracts/csharp-api.md) — `SKGraphite*` public surface: 11 new types + structs + 1 enum, plus 3 new overloads on existing `SKSurface`/`SKImage`. Documentation obligations and ABI freeze rules included.
- [quickstart.md](quickstart.md) — three-layer smoke runbook with bisection table.

Agent context updated: `CLAUDE.md` SPECKIT block now points at this plan.

## Phase 2 Approach (will be executed by `/speckit-tasks`)

The next command will produce `tasks.md`. The expected shape — informing but not prescribing the task generator — is:

1. **Build-flags + Lavapipe environment** (foundational; blocks layers below):
   - Add `SUPPORT_GRAPHITE` and `SUPPORT_DAWN` to `native/linux/build.cake` (start), `native/macos/build.cake`, `native/windows/build.cake`. Inject `skia_enable_graphite=…`, `skia_use_dawn=…` into the `GnNinja()` call. Default OFF until smoke is green.
   - Document Lavapipe install in `documentation/dev/graphite-headless.md` (or similar).
   - Smoke: `dotnet cake --target=externals-linux SUPPORT_GRAPHITE=true SUPPORT_VULKAN=true` produces a `libSkiaSharp.so` whose `nm` output contains `*graphite*` symbols.

2. **C++ smoke (Layer 1)** — pure validation; no shim code yet:
   - Create `tests/native/Graphite/cpp_smoke/{CMakeLists.txt,graphite_cpp_smoke.cpp}`.
   - Bring up a Vulkan instance/device on Lavapipe; populate a `skgpu::VulkanBackendContext`; call `ContextFactory::MakeVulkan`; run the rounded-rect render-and-readback from quickstart.md.
   - Pass criterion: pixel at (128,128) is red. **Do not proceed past this until green.**

3. **C API shim (core)** — wrap the upstream Skia surface:
   - Add `externals/skia/include/c/sk_graphite.h` and `src/c/sk_graphite.cpp`. Cover Context lifecycle (without backend creation), Recorder, Recording, Surface (RenderTarget + WrapBackendTexture), Image (WrapTexture + AsImage/AsImageCopy), BackendTexture/TextureInfo lifecycle. Wrap conditional on `SK_GRAPHITE`.
   - Add a corresponding update to the SkiaSharp source list / GN build glue so these new files compile into `libSkiaSharp` when `skia_enable_graphite=true`.

4. **C API shim (Vulkan first)** — backend-specific:
   - Add `externals/skia/include/c/sk_graphite_vulkan.h` and `src/c/sk_graphite_vulkan.cpp`. Wrap `MakeVulkan` factory + per-backend `TextureInfo`/`BackendTexture` factories. Conditional on `SK_GRAPHITE && SK_VULKAN`.

5. **C smoke (Layer 2)**:
   - Create `tests/native/Graphite/c_smoke/{CMakeLists.txt,graphite_c_smoke.c}`. Reproduce the rounded-rect render-and-readback purely through `sk_graphite_*` and existing `sk_canvas_*` / `sk_surface_*`. Pass criterion identical to Layer 1.

6. **C# bindings (generation + hand-written wrappers)**:
   - Run `pwsh ./utils/generate.ps1` to regenerate `binding/SkiaSharp/SkiaApi.generated.cs` against the new C headers.
   - Add hand-written wrappers per `contracts/csharp-api.md` (one task per type).
   - Add overloads on `SKSurface` / `SKImage`.

7. **C# smoke (Layer 3)**:
   - Add `tests/Tests/SkiaSharp/Graphite/{LavapipeFixture.cs,GraphiteSmokeTests.cs}`. `[SkippableFact]` gated on `SKGraphiteContext.IsBackendAvailable(Vulkan)`. Same pixel assertion.

8. **Test obligations from contracts**:
   - Add disposal-ordering tests, ownership-transfer test, Ganesh+Graphite coexistence test.
   - Verify (via existing test suite run) that all GR* / SK* / SkiaSharp tests pass unchanged (FR-020, SC-003).

9. **Backend rollout (parallelizable, independent of #5–8 once C# layer is shaped)**:
   - Metal: add `sk_graphite_metal.{h,cpp}` and the corresponding C# bits. macOS-only smoke gated on `IsBackendAvailable(Metal)`.
   - Dawn: same for Dawn. CI runners that have Dawn vendored run the smoke; others skip.

10. **Documentation + release-notes hooks**:
    - Add `documentation/dev/graphite.md` describing the new public surface and the headless dev story.
    - Existing release-notes pipeline (`/release-notes`) picks up the new commits automatically.

The `/speckit-tasks` output will turn each numbered item into one or more concrete, dependency-ordered tasks with explicit acceptance criteria.

## Complexity Tracking

> No constitution violations to justify. Section intentionally empty.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| — | — | — |
