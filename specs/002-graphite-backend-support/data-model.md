# Phase 1 Data Model: Graphite Backend Support

**Date**: 2026-04-30 · **Branch**: `002-graphite-backend-support`

This document maps the conceptual entities from [spec.md](spec.md) onto concrete types at each layer (C++ → C → C#). It is the input contract for Phase 1's API contracts and the test scenarios.

For each entity:

- **C++ source-of-truth**: the upstream Skia type/free function we wrap.
- **C handle**: the opaque pointer name in our shim.
- **C# type**: the public managed type we expose.
- **Lifetime**: who owns the underlying memory and when it is freed.
- **Mutability / threading**: derived from upstream Skia behavior; we do not relax it.
- **Validation rules**: enforced in C# before reaching the C shim.

---

## 1. Graphite Context

| Field | Value |
|---|---|
| **C++** | `skgpu::graphite::Context` (owned `unique_ptr`; created by `ContextFactory::Make{Dawn,Metal,Vulkan}`) |
| **C handle** | `sk_graphite_context_t*` |
| **C#** | `SKGraphiteContext : SKObject, ISKReferenceCounted? → No: ISKSkipObjectRegistration / IDisposable` (non-ref-counted, owned-pointer pattern; matches `GRContext`) |
| **Lifetime** | Created by static factory methods `SKGraphiteContext.CreateDawn(...)`, `CreateMetal(...)`, `CreateVulkan(...)`. Disposed explicitly. Disposing tears down all GPU resources owned by this context. |
| **Threading** | Single-thread-affine (matches Skia upstream). C# does not enforce; documented in XML doc. |
| **Validation** | Backend-context argument must be non-null and `IsValid`. Options struct is value-typed; default is allowed. |

**Lifecycle invariants**:

- A `SKGraphiteContext` MUST outlive every `SKGraphiteRecorder`, `SKGraphiteRecording`, `SKSurface` (Graphite-backed), `SKImage` (Graphite-backed), and `SKGraphiteBackendTexture` it produced.
- Disposing it while dependents are alive throws `InvalidOperationException` from those dependents on next use; no crash.

**State transitions**:

```
created -> active -> disposed
              \-> device-lost -> (terminal, only Dispose() is valid)
```

---

## 2. Graphite Recorder

| Field | Value |
|---|---|
| **C++** | `skgpu::graphite::Recorder` (owned `unique_ptr` from `Context::makeRecorder`) |
| **C handle** | `sk_graphite_recorder_t*` |
| **C#** | `SKGraphiteRecorder : SKObject, IDisposable` |
| **Lifetime** | Vended from `SKGraphiteContext.CreateRecorder([options])`. Disposed by caller. Multiple recorders per context are allowed and intended. |
| **Threading** | Single-thread-affine. Different recorders from the same context may be used concurrently on different threads. |
| **Validation** | C# rejects use after dispose. Drawing methods validate the recorder is still alive before native call. |

**Lifecycle invariants**:

- Snapping consumes any pending draws into a `SKGraphiteRecording`. The recorder remains usable for further drawing afterwards (mirrors upstream).
- Disposing the recorder before snapping discards in-flight work without error.

---

## 3. Graphite Recording

| Field | Value |
|---|---|
| **C++** | `skgpu::graphite::Recording` (owned `unique_ptr` from `Recorder::snap`) |
| **C handle** | `sk_graphite_recording_t*` |
| **C#** | `SKGraphiteRecording : SKObject, IDisposable` |
| **Lifetime** | Created by `SKGraphiteRecorder.Snap()`. Submitted (consumed) via `SKGraphiteContext.InsertRecording(recording, [info])` or disposed without submission (legal, no-op). |
| **Threading** | Recording is thread-safe to *hold* but not to insert from multiple threads simultaneously into the same context. |
| **Validation** | Inserting into a context that did not vend the parent recorder MUST return `InsertStatus.InvalidRecording` from native; C# surfaces this as a non-throwing returned enum so callers can branch. |

---

## 4. Graphite-backed SKSurface (extension on existing type)

| Field | Value |
|---|---|
| **C++** | `SkSurfaces::RenderTarget(Recorder*, SkImageInfo, …)`, `SkSurfaces::WrapBackendTexture(Recorder*, BackendTexture, …)` |
| **C handle** | `sk_surface_t*` (existing handle reused) |
| **C#** | `SKSurface` (existing type; new factory overloads only) |
| **Lifetime** | Reference-counted via existing `SKSurface` lifetime. Holds a soft dependency on its parent recorder. Disposing the parent context invalidates draws but not the SKSurface allocation. |
| **Threading** | Inherits SKSurface rules. |
| **Validation** | `recorder` argument non-null and not disposed; `imageInfo` must have `Width > 0 && Height > 0 && ColorType != Unknown`. Existing SKSurface validation reused. |

**New public methods (additive — FR-013)**:

- `static SKSurface Create(SKGraphiteRecorder recorder, SKImageInfo info, …)`
- `static SKSurface Create(SKGraphiteRecorder recorder, SKGraphiteBackendTexture backendTexture, …)`

Existing factories on `SKSurface` are untouched.

---

## 5. Graphite-backed SKImage (extension on existing type)

| Field | Value |
|---|---|
| **C++** | `SkImages::WrapTexture(Recorder*, BackendTexture, …)`, `SkSurfaces::AsImage(SkSurface)`, `SkSurfaces::AsImageCopy(SkSurface, ...)` |
| **C handle** | `sk_image_t*` (existing handle reused) |
| **C#** | `SKImage` (existing type; new factory overloads only) |
| **Lifetime** | Reference-counted via existing `SKImage` lifetime. |

**New public methods**:

- `static SKImage FromTexture(SKGraphiteRecorder recorder, SKGraphiteBackendTexture backendTexture, …)`
- `SKImage SnapshotGraphite()` — convenience over `SkSurfaces::AsImage` available only on Graphite-backed surfaces.

---

## 6. Graphite Backend Texture

| Field | Value |
|---|---|
| **C++** | `skgpu::graphite::BackendTexture` (value type, copyable) |
| **C handle** | `sk_graphite_backend_texture_t*` (heap-allocated wrapper around the value type) |
| **C#** | `SKGraphiteBackendTexture : SKObject, IDisposable` |
| **Lifetime** | Constructed via per-backend factory: `SKGraphiteBackendTexture.CreateVulkan(...)`, `CreateMetal(...)`, `CreateDawn(...)`. The wrapping object's disposal does not by itself free the underlying GPU texture; ownership is governed by the *flag passed at construction*: `OwnsTexture: true` calls the backend's release on dispose; `OwnsTexture: false` leaves the GPU texture alone. |
| **Validation** | `dimensions` must have positive width/height. `IsValid` reflects upstream `BackendTexture::isValid`. |

This entity directly satisfies FR-012's "explicit ownership control" requirement.

---

## 7. Graphite Texture Info

| Field | Value |
|---|---|
| **C++** | `skgpu::graphite::TextureInfo` (value type) + per-backend `VulkanTextureInfo`, etc. |
| **C handle** | `sk_graphite_texture_info_t*` |
| **C#** | `SKGraphiteTextureInfo` (struct or sealed class — see contracts/csharp-api.md) |
| **Lifetime** | Pure value type at the C# level; backed by a heap pointer at the C level only because cross-backend layout is not stable across Skia revisions. |
| **Validation** | Must specify a backend; backend-specific fields (e.g., Vulkan format) must be set when constructing per-backend variants. |

---

## 8. Backend-Specific Configuration (per-backend BackendContext)

These three are **strictly distinct** types; they are not interchangeable.

### 8a. `SKGraphiteVkBackendContext` (Vulkan)

| Source | `skgpu::VulkanBackendContext` (`include/gpu/vk/VulkanBackendContext.h`) |
|---|---|
| **Required fields** | `VkInstance fInstance`, `VkPhysicalDevice fPhysicalDevice`, `VkDevice fDevice`, `VkQueue fQueue`, `uint32_t fGraphicsQueueIndex`, function-pointer getters `fGetProc`, version + extension flags |
| **Optional fields** | `fProtectedContext`, `fDeviceFeatures`, `fEnabledExtensions` |
| **C handle** | `sk_graphite_vk_backend_context_t*` |
| **Lifetime** | Owned by the caller in C# (struct). Marshalled to native at context-creation time; native does not retain the struct after `MakeVulkan` returns. |
| **Validation** | All four primary handles non-zero; `fGetProc` non-null. |

### 8b. `SKGraphiteMtlBackendContext` (Metal)

| Source | `skgpu::graphite::MtlBackendContext` |
|---|---|
| **Required fields** | `IntPtr Device` (CFRetain'd MTLDevice), `IntPtr Queue` (CFRetain'd MTLCommandQueue) |
| **C handle** | `sk_graphite_mtl_backend_context_t*` |
| **Validation** | Both handles non-zero. C# does not validate they are actually MTL types — caller's responsibility. |

### 8c. `SKGraphiteDawnBackendContext` (Dawn / WebGPU)

| Source | `skgpu::graphite::DawnBackendContext` |
|---|---|
| **Required fields** | `WGPUInstance Instance`, `WGPUDevice Device`, `WGPUQueue Queue`, optional `DawnTickFunction Tick` |
| **C handle** | `sk_graphite_dawn_backend_context_t*` |
| **Validation** | All three handles non-zero. |

---

## 9. ContextOptions

| Field | Value |
|---|---|
| **C++** | `skgpu::graphite::ContextOptions` |
| **C#** | `SKGraphiteContextOptions` (struct) |
| **v1 fields exposed** | `bool DisableDriverCorrectnessWorkarounds`, `int InternalMultisampleCount`, `long GpuBudgetInBytes`, `bool RequireOrderedRecordings`, `bool SetBackendLabels` |
| **v1 fields NOT exposed** | Pipeline callbacks, runtime effects span, executor, persistent pipeline storage, capture flag (all deferred — see research.md R3) |

---

## 10. SubmitInfo / InsertRecordingInfo / InsertStatus

| Field | Value |
|---|---|
| **C++** | `skgpu::graphite::SubmitInfo`, `InsertRecordingInfo`, `InsertStatus` |
| **C#** | `SKGraphiteSubmitInfo` (struct), `SKGraphiteInsertRecordingInfo` (struct), `SKGraphiteInsertStatus` (enum) |
| **v1 InsertRecordingInfo fields** | `Recording*`, `TargetSurface?`, `TargetTranslation`, `TargetClip`. `WaitSemaphores` / `SignalSemaphores` / async callbacks deferred. |
| **v1 SubmitInfo fields** | `SyncToCpu Sync`, `MarkFrameBoundary MarkBoundary`, `ulong FrameID` |

`SKGraphiteInsertStatus` mirrors upstream enum members: `Success`, `InvalidRecording`, `PromiseImageInstantiationFailed`, `AddCommandsFailed`, `AsyncShaderCompilesFailed`, `OutOfOrderRecording`. Returned by `InsertRecording` rather than thrown — callers branch on it.

---

## Cross-Entity Relationships

```
SKGraphiteContext (1) ──owns──▶ (n) SKGraphiteRecorder
       │                              │
       │                              └─snap()─▶ (n) SKGraphiteRecording
       │
       └─submits──▶ SKGraphiteRecording

SKGraphiteRecorder ──creates──▶ SKSurface (Graphite-backed)
SKGraphiteRecorder ──creates──▶ SKImage   (Graphite-backed)
SKGraphiteRecorder ──manages──▶ SKGraphiteBackendTexture (allocate/update/delete)

SKGraphiteBackendTexture ──wrapped by──▶ SKSurface or SKImage
```

## Edge Cases Covered (from spec)

| Spec edge case | Modeled here as |
|---|---|
| Backend not available at runtime | Factory methods `CreateVulkan/Metal/Dawn` return null when symbols missing; static `IsBackendAvailable(Backend)` query method |
| Submit on wrong context | Native `InsertStatus.InvalidRecording` returned by `InsertRecording`; no exception |
| Disposal ordering | Each dependent type's native handle nulls out parent reference on parent dispose; subsequent use throws `ObjectDisposedException` |
| Empty recording | Allowed; `Submit` succeeds with `Success` status |
| OOM | Native returns null; C# factories return null (factory pattern) or throw `OutOfMemoryException` (constructor pattern) — see contracts/csharp-api.md per call |
| Threading | Documented in XML docs; not enforced |
