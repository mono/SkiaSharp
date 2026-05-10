# Feature Specification: Graphite Backend Support

**Feature Branch**: `002-graphite-backend-support`
**Created**: 2026-04-30
**Status**: Draft
**Input**: User description: "The goal is add support in SkiaSharp for the new Graphite backend in Skia. A new C API for the Graphite APIs should be added, and then a C# binding layer for the C API."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Render with the Graphite Backend on a Modern GPU API (Priority: P1)

A .NET application developer building a graphics-heavy app (game, design tool, charting library) wants to render their existing SkiaSharp drawing code through Skia's next-generation Graphite backend instead of the legacy Ganesh backend so the app benefits from Graphite's modern GPU pipeline (Vulkan / Metal / Dawn) and Skia's ongoing investment in that backend.

The developer creates a Graphite context bound to their chosen GPU API, obtains a recorder from it, draws to a GPU-backed surface, snaps the resulting recording, and submits it through the context. They reuse the same `SKCanvas` / `SKPaint` / `SKPath` drawing code they already use today.

**Why this priority**: This is the core value proposition. Without it, the feature delivers nothing. It is also the smallest piece that proves the C API and C# binding round-trip works end-to-end and produces a correct rendered image.

**Independent Test**: A unit/integration test creates a Graphite context for one supported backend, draws a known shape to a GPU surface, snaps the recording, submits it, reads back the pixels, and asserts they match a reference image. This proves the entire pipeline works without any other Graphite features in place.

**Acceptance Scenarios**:

1. **Given** a developer with a working GPU device for one of the supported backends, **When** they create a Graphite context, obtain a recorder, draw a shape to a Graphite-backed surface, snap the recording, and submit it, **Then** the rendered output matches the equivalent CPU-rendered output within an acceptable pixel tolerance.
2. **Given** a developer drawing across multiple frames, **When** they reuse the same Graphite context and obtain a fresh recorder per frame, **Then** each frame produces correct output without leaking GPU resources between frames.
3. **Given** a developer at the end of their app's lifetime, **When** they dispose the context, recorders, recordings, and surfaces in any disposal order, **Then** no crashes, leaks, or use-after-free occur.

---

### User Story 2 - Choose a Graphite Backend per Platform (Priority: P2)

A developer shipping a cross-platform .NET app (e.g., macOS + Windows + Linux + Android) needs to pick the Graphite backend that fits each platform: Metal on Apple platforms, Vulkan on Android / Windows / Linux, and Dawn (WebGPU) where their host stack already uses Dawn.

**Why this priority**: Without per-backend selection, the feature is only useful on one platform and cannot ship in a real cross-platform product. It is P2 rather than P1 because story 1 can be demonstrated on a single backend first; this story turns the demo into something usable.

**Independent Test**: For each supported backend, a test (gated on platform availability) creates a backend-specific Graphite context using a backend-specific configuration object, runs the round-trip from story 1, and verifies success. Each backend can be developed and tested in isolation.

**Acceptance Scenarios**:

1. **Given** a Metal-capable Apple device, **When** the developer creates a Graphite context configured with a Metal device and command queue, **Then** drawing succeeds and reads back the expected pixels.
2. **Given** a Vulkan-capable device, **When** the developer creates a Graphite context configured with a Vulkan instance / physical device / device / queue, **Then** drawing succeeds and reads back the expected pixels.
3. **Given** a Dawn-capable host, **When** the developer creates a Graphite context configured with a Dawn (WebGPU) device, **Then** drawing succeeds and reads back the expected pixels.
4. **Given** a developer on a platform where a backend is not built into the native library, **When** they attempt to create a context for that backend, **Then** they receive a clear error indicating the backend is not available, not a crash.

---

### User Story 3 - Wrap Existing GPU Textures and Surfaces (Priority: P2)

A developer integrating SkiaSharp into an existing render loop (e.g., a Unity / MAUI / Avalonia / custom engine) already owns swap-chain textures and other GPU resources allocated outside SkiaSharp. They need to wrap those textures so Graphite can draw into them or sample from them, instead of allocating duplicate GPU memory.

**Why this priority**: Real apps almost always render into a host-owned swap chain or compose with engine-allocated resources. Without backend-texture / backend-render-target interop, the feature is a sandbox demo, not a production option. P2 because story 1 establishes the pipeline; this story makes it composable with host code.

**Independent Test**: A test allocates a GPU texture via the platform API, wraps it as a backend texture, builds a Graphite-backed surface or image around it, draws into it, and verifies the pixels are written to the original GPU texture. Can be tested per backend without depending on advanced features.

**Acceptance Scenarios**:

1. **Given** a developer who allocated a backend texture themselves, **When** they wrap it and create a Graphite surface around it, **Then** drawing into that surface writes pixels into their original texture and does not allocate a duplicate.
2. **Given** a developer who has a sampleable backend texture, **When** they create a Graphite-backed image from it, **Then** they can use it as a paint source or input to image filters with the same drawing code as a CPU-backed image.
3. **Given** ownership of a backend texture is transferred to SkiaSharp, **When** the wrapping object is disposed, **Then** SkiaSharp releases the underlying texture exactly once.
4. **Given** ownership remains with the host, **When** the wrapping object is disposed, **Then** the underlying texture is left intact.

---

### User Story 4 - Migrate from Ganesh to Graphite Without Breaking the Existing API (Priority: P3)

A developer with an existing SkiaSharp app using `GRContext` (Ganesh) wants to evaluate Graphite without rewriting their app or being forced to migrate immediately. Both backends must continue to coexist in the same app, in the same process, against the same drawing code.

**Why this priority**: Migration friction is the single biggest barrier to adopting a new backend. By keeping Ganesh fully working and adding Graphite as a parallel option, existing users are not disrupted, and Graphite can be adopted incrementally. P3 because it is a property of the rollout (no regressions) rather than a new capability.

**Independent Test**: Run the existing Ganesh test suite unchanged against the build that adds Graphite, and verify zero regressions. Separately, run a sample app that uses both backends in different windows of the same process and verify they do not interfere.

**Acceptance Scenarios**:

1. **Given** an app that today uses `GRContext`, **When** it is rebuilt against the SkiaSharp version that adds Graphite, **Then** all existing public APIs are unchanged in name, signature, and behavior, and the app builds and runs without code changes.
2. **Given** an app that creates both a Ganesh `GRContext` and a Graphite context, **When** it draws using each in turn, **Then** both produce correct output and neither destabilizes the other.

---

### Edge Cases

- **Backend not available at runtime**: User builds against a SkiaSharp package that does not include a particular Graphite backend (e.g., Metal symbols on Linux). Creation of that backend's context must fail with a clear, catchable error rather than a missing-symbol crash.
- **GPU device lost / TDR**: While a recording is in flight, the underlying GPU device is lost (driver reset, removed external GPU, headless container loses access). The system must surface a recoverable failure on the next submit and not corrupt subsequent draws on a freshly created context.
- **Submitting a recording on the wrong context**: A recording snapped from a recorder belonging to context A is submitted to context B. The system must reject this with a clear error rather than crash or silently produce garbage.
- **Disposal ordering**: User disposes the Graphite context while recorders, recordings, surfaces, or backend textures bound to it are still alive. Behavior must be defined — either each dependent object's later use becomes a no-op / clear error, or disposal of the context is deferred until dependents are released — but never an undefined-behavior crash.
- **Threading**: User attempts to share a single recorder across threads simultaneously. Recorders are not thread-safe in Skia; the binding must document this and not silently corrupt state. Multiple recorders from one context, each used on its own thread, must work.
- **Empty / no-op recording**: Snapping a recording from a recorder with no draws and submitting it is legal and must produce no GPU work and no error.
- **Out-of-memory on GPU allocation**: Creating a surface or backend texture larger than the device can allocate must return null / fail cleanly, not crash.
- **WASM / browser host**: On the WebAssembly target, only the Dawn (WebGPU) backend is meaningful. Attempts to create Vulkan or Metal contexts in that environment must fail predictably.

## Requirements *(mandatory)*

### Functional Requirements

#### Core Graphite Pipeline

- **FR-001**: SkiaSharp MUST expose a Graphite context type that .NET callers can construct, hold, use to obtain recorders, submit recordings, flush, and dispose.
- **FR-002**: SkiaSharp MUST expose a recorder type obtained from a Graphite context that can be passed to drawing operations and used to produce a recording.
- **FR-003**: SkiaSharp MUST expose a recording type produced by a recorder that can be inserted into a context for execution and is independently disposable.
- **FR-004**: SkiaSharp MUST allow creation of a GPU-backed surface and image bound to a Graphite recorder, usable with the existing `SKCanvas` drawing API without changes to caller drawing code.
- **FR-005**: SkiaSharp MUST allow snapshotting a Graphite-backed surface to a Graphite-backed image, and reading back pixels from a Graphite-backed surface or image into CPU memory.
- **FR-006**: SkiaSharp MUST expose a configuration object (analogous to context options) that lets the caller set common Graphite context options (e.g., resource budget, caching hints) at context creation time.

#### Backend Coverage

- **FR-007**: SkiaSharp MUST support creating a Graphite context for the following backends: Vulkan, Metal, and Dawn (WebGPU). Each backend's context creation accepts the backend-specific device / queue / instance handles required by Skia's Graphite API for that backend.
- **FR-008**: SkiaSharp MUST allow callers to query, at runtime, whether a given Graphite backend is available in the loaded native library, without attempting to create a context.
- **FR-009**: When a Graphite backend is not compiled into the loaded native library, requesting a context for that backend MUST return a null / failure result with a clearly documented error path, not a process crash or missing-symbol exception.

#### Backend Resource Interop

- **FR-010**: SkiaSharp MUST expose backend-texture and backend-render-target wrapper types for Graphite, mirroring the role of the Ganesh equivalents, so that callers can wrap externally-allocated GPU textures and use them with Graphite.
- **FR-011**: SkiaSharp MUST allow constructing a Graphite-backed image from a backend texture and a Graphite-backed surface from a backend texture or backend render target.
- **FR-012**: SkiaSharp MUST give callers explicit control over whether ownership of a wrapped backend texture transfers to SkiaSharp or remains with the caller, and MUST honor that choice on disposal.

#### Coexistence and Stability

- **FR-013**: Adding Graphite MUST NOT modify, remove, or change the behavior of any existing public Ganesh-related API (`GRContext`, `GRBackendTexture`, `GRBackendRenderTarget`, etc.).
- **FR-014**: A single .NET process MUST be able to instantiate and use both a Ganesh `GRContext` and a Graphite context in the same run without one corrupting the other.
- **FR-015**: All Graphite-related public types MUST follow SkiaSharp's existing naming, disposal (`SKObject`-style), and exception-on-misuse conventions so that they feel native to existing SkiaSharp users.

#### C API Layer

- **FR-016**: A new C API MUST be added in `externals/skia/include/c/` and `externals/skia/src/c/` covering exactly the Graphite surface area required by FR-001 through FR-012, following the existing `sk_<type>_<action>` naming convention used by the rest of the SkiaSharp C shim.
- **FR-017**: The C API MUST validate ownership and return-on-failure semantics consistently with the rest of the SkiaSharp C API: nullable factories return `nullptr` on failure, ref-counted objects use `sk_sp` lifetimes, and non-ref-counted objects are paired with explicit destroy functions.
- **FR-018**: The C API MUST be conditionally compilable so that builds that exclude a given Graphite backend (e.g., a Metal-less Linux build) link cleanly without dangling symbols.

#### Quality Gates

- **FR-019**: The feature MUST ship with automated tests that, at minimum, cover end-to-end render-and-readback for at least one backend on at least one CI platform, plus disposal-ordering and ownership-transfer cases.
- **FR-020**: Existing SkiaSharp tests MUST continue to pass without modification after this feature is added.

### Key Entities *(include if feature involves data)*

- **Graphite Context**: The root object representing an initialized Graphite renderer bound to a particular GPU backend. Owns GPU resources, vends recorders, accepts and submits recordings, and is the only object that must be explicitly created per GPU device.
- **Recorder**: A short-lived recording-side object obtained from a context. Drawing operations target a recorder (indirectly, via a Graphite-backed canvas), and a recorder produces a recording. Not thread-safe; intended to be used by one thread at a time.
- **Recording**: An immutable, replayable command list produced by a recorder. Independent of the recorder that produced it; must be inserted into a context to actually execute on the GPU. Disposable.
- **Graphite-backed Surface**: A drawable target whose pixels live in GPU memory managed by Graphite. Provides an `SKCanvas` for drawing. Bound to a recorder, not directly to the context.
- **Graphite-backed Image**: A read-only GPU image, either created from a backend texture or snapshotted from a Graphite surface. Usable as a paint source or filter input.
- **Graphite Backend Texture / Render Target**: Caller-allocated GPU texture (or render target) wrapped so Graphite can draw into it or sample from it. Ownership of the underlying GPU object is configurable.
- **Backend-specific Configuration**: Per-backend value object (Vulkan: instance/physical-device/device/queue/feature flags; Metal: device + command queue; Dawn: device) supplied at context creation time. Each backend has its own configuration type — they are not interchangeable.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A .NET developer can take SkiaSharp drawing code that runs today against `GRContext` (Ganesh) and re-target it to a Graphite context with changes confined to context / surface creation; no `SKCanvas` / `SKPaint` / `SKPath` drawing code needs to change.
- **SC-002**: For at least one supported backend, a Graphite-rendered image and a CPU-rendered image of the same scene match within a 1% per-pixel tolerance for a defined regression-test scene set.
- **SC-003**: 100% of existing SkiaSharp Ganesh-related tests pass unchanged after the Graphite feature is merged.
- **SC-004**: The Graphite C API is fully usable from C# binding code without any direct C++ interop on the C# side — i.e., every operation called out in FR-001 through FR-012 has a `sk_*` C entry point.
- **SC-005**: Each supported Graphite backend (Vulkan, Metal, Dawn) has at least one automated end-to-end test that creates a context, draws, submits, reads back, and asserts on pixel content; tests are gated on platform availability and skipped (not failed) when the backend is genuinely unavailable.
- **SC-006**: A developer evaluating Graphite can determine at runtime, in under one API call, whether a desired backend is available in the loaded native library, without triggering a crash on unavailability.

## Assumptions

- **Backend selection for v1**: Vulkan, Metal, and Dawn are the three Graphite backends to expose, mirroring Skia's upstream Graphite support. Direct3D is intentionally out of scope because Skia's Graphite backend does not target it; D3D users continue to use Ganesh.
- **Coexistence, not replacement**: Ganesh (`GRContext`) remains fully supported and unchanged in this feature. Graphite is additive. Deprecation of Ganesh is out of scope and will be considered as a separate, later decision.
- **API parity is goal-state, not v1 scope**: The v1 Graphite surface area covers context, recorder, recording, surface, image, backend-texture/render-target, and the per-backend configuration objects needed to construct contexts. Advanced upstream Graphite features such as `PrecompileContext`, `PersistentPipelineStorage`, `ImageProvider`, `YUVABackendTextures`, and `ShaderErrorHandler` are deferred to follow-up work and not required for this feature to be useful.
- **Build system**: The existing native build pipeline (`dotnet cake --target=externals-{platform}`) is extended to compile the Graphite C API into the existing `libSkiaSharp` per-platform binaries, rather than producing a separate Graphite-only library. Per-platform builds enable only the backends meaningful for that platform.
- **Generated bindings**: P/Invoke signatures for the new C API are produced through the existing `pwsh ./utils/generate.ps1` flow; manually-written C# wrappers sit on top of those generated `SkiaApi` entries, consistent with the rest of SkiaSharp.
- **Threading model matches Skia upstream**: Recorders and contexts inherit Skia's existing thread-affinity model. The binding documents this rather than enforcing it in C#.
- **Dawn dependency**: Dawn is assumed to be buildable and linkable in the SkiaSharp native build for at least desktop targets; if Dawn cannot be built on a given platform, that platform's package simply ships without the Dawn backend, and FR-008 / FR-009 cover the runtime-availability case.
- **Test infrastructure**: Existing CI runners on macOS (Metal), Windows (Vulkan), and Linux (Vulkan) are used to satisfy SC-005. Dawn coverage initially comes from whichever of those runners has Dawn available; broader Dawn / WASM coverage is a follow-up.
