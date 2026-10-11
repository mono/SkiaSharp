# Ganesh and Graphite views

`SKGaneshView` and `SKGraphiteView` name Skia's rendering engines, not a GPU
technology. Their native UIKit, Android and WinUI containers own private
presenters. A future backend replacement does not change the control's base
class or paint contract. GPU resources remain specific to a context generation:
they are **not** interchangeable between engines or backend instances.

| Host | Ganesh | Graphite |
| --- | --- | --- |
| iOS / Mac Catalyst | Metal | Native Metal |
| Android | GLES | Native Vulkan 1.1; Android API 24+, compatible device required |
| Windows / WinUI | GLES through ANGLE | Explicitly unsupported |

Native Graphite Metal is an intentional limited implementation, not a claim
that it is upstream's primary production backend. Dawn, Windows Graphite,
Tizen, and new macOS/tvOS/framework ports are deferred.

## MAUI

Install `SkiaSharp.Views.Maui.Controls` and call `UseSkiaSharp()` as usual.
Both new controls are registered alongside the unchanged legacy `SKGLView`.
Defaults are on-demand rendering, physical pixels and touch forwarding disabled.

```xml
<skia:SKGaneshView PaintSurface="Paint"
                   IgnorePixelScaling="True"
                   EnableTouchEvents="True"
                   Touch="Touch" />
```

```csharp
void Paint(object sender, SKPaintSurfaceEventArgs e)
{
    e.Surface.Canvas.Clear(SKColors.White);
    using var paint = new SKPaint { Color = SKColors.Blue };
    e.Surface.Canvas.DrawCircle(e.Info.Width / 2f, e.Info.Height / 2f, 40, paint);
}
```

The typed engine paint arguments derive from the existing
`SKPaintSurfaceEventArgs`: the same draw handler can consume CPU, Ganesh or
Graphite metadata. `Info` describes drawing coordinates; `RawInfo` describes
physical pixels. With `IgnorePixelScaling = true`, canvas and touch coordinates
both use logical pixels. Each callback's canvas state is restored automatically.

`HasRenderLoop` renders continuously only while attached to a window.
`InvalidateSurface()` requests one frame. Zero-sized frames and temporarily
unavailable Apple drawables are not rendering failures.

## Context, recorder and callback lifetime

`GRContext` and `GraphiteContext` are typed **borrowed** properties. They are
null before creation and after detach. Do not dispose them, submit the
Graphite context independently, or create untracked recorders directly from
the Graphite context. `ContextCreated`, `ContextDestroying` and
`ContextDestroyed` describe each generation. Destroy dependent surfaces,
images and shaders before their owning context.

Paint and lifecycle callbacks run on the **GPU owner thread**: UI thread on
Apple and Windows, dedicated GL/Vulkan thread on Android. Dispatch only UI
updates to MAUI's dispatcher, not GPU operations. Capture diagnostic backend
names on the owner thread before dispatching; never capture a borrowed frame
surface for later use.

Presentation-dependent texture wrappers and GPU snapshots must not be retained
past the frame or a resize. Use caller-owned offscreen surfaces for persistent
GPU images. Vulkan resize drains the GPU and releases frame surfaces and the
private recorder's caches before destroying old swapchain images; it does not
replace the public context or caller-owned offscreen recorders.

`SKGraphiteView.CreateRecorder()` creates a caller-owned recorder on the context
owner thread. It has a bounded per-recorder image provider and GPU budget.
At most eight caller recorders may be active per view. A producer may hand
that recorder exclusively to one worker, record and call
`view.SnapRecording(recorder)` there, then hand
recordings back to the owner thread for `view.InsertRecording(recording)`.
Insert on the owner thread (for example, in `PaintSurface`); the view owns frame
submission and does not expose its mutable primary recorder. View insertion
accepts only recordings registered by this helper in the current generation,
so stale work is rejected before reaching native code. At most 32 caller
recordings may be outstanding; dispose each recording after insertion/use.
Registered recordings also keep a retiring context/device alive until disposed.

During `ContextDestroying`, synchronously stop/join producers, dispose their
surfaces/recordings, then dispose their recorders. Do not synchronously dispatch
back to the UI thread from Android's shutdown callback: the UI thread may be
waiting for window resources to be released. If callers retain registered
recorders, native window resources are still released, but the device/context
is retired on its owner thread only after those recorders are disposed.
Recreation is refused while a previous generation is retiring. There is no
process-lifetime renderer pool or faulted-renderer list.

The bounded image cache is disposed while its recorder is alive. Graphite
completion work is polled even while an on-demand view is idle. Asynchronous
readback callbacks must copy result data during the callback; results are
borrowed. Stop issuing new work when destruction starts.

Use `e.RequestReadPixels(info, sourceRect, callback)` during Graphite paint to
read the current frame. The view schedules the read after inserting its drawing;
requesting a context read before the frame is snapped can instead read old
contents. Source rectangles use physical surface pixels regardless of logical
canvas scaling.

The Vulkan presenter keeps at most three submitted frames in flight and polls
their completion without waiting for the device after each frame. Acquisition
semaphores and fences are retired after Graphite execution completes;
presentation semaphores belong to their swapchain images and are reused after
image reacquisition. Resize, teardown and exceptional cleanup may drain the GPU.
It uses complete 64-bit Vulkan handles, acquisition waits, render-completion
signals, exclusive queue ownership and the presentation image layout.

## Failure and migration

Subscribe to `RenderFailed` and display errors/retry UI. Without a subscriber,
failures are logged to standard error. No Graphite view silently becomes
Ganesh or software-rendered. Apple Graphite recreates a failed context on explicit retry; Android retries
recreate the private presenter only after shutdown and recorder retirement finish.
Do not advertise presentation based only on `IsBackendAvailable`: compiled
backend support does not establish window presentation or device compatibility.

MAUI's Windows Graphite handler throws `PlatformNotSupportedException`.
The Basic sample hides its Graphite page on Windows and the Gallery disables
Graphite selection with an accessibility explanation. GPU sample paths now use
Ganesh; Graphite is a separate explicit selection on supported hosts. Switching
engines destroys and recreates dependent sample resources. CPU remains available.
Legacy `SKGLView`, its public signatures, and unmodernized framework examples
remain supported and unchanged.

## Validation

GPU device and package-consumer tests must exercise actual pixels, idle
readback, input scaling and shutdown/collection. Missing drivers or devices
are not success or a new skip policy. Build from the exact prerequisite native
revision; never use `externals-download` for these changes.

Package-consumer native Basic tests use the producing build's `samples.zip`;
stage it at `output/samples.zip` or pass `-p:NativeSamplesArchive=<path>` alongside
the exact BAR package versions/feed. They launch native Ganesh/Graphite pages
and verify the visible colored shader scene, while MAUI tests compare reference
pixels. Keep test working directories under the checkout when running locally
(for example, a process-local `TMPDIR` pointing at an existing output directory).

The Windows .NET Core CI leg also installs the MAUI Windows workload and runs
the device host's GPU view tests. It checks real Ganesh pixels, input/scaling,
on-demand and continuous rendering, context recreation and handler collection,
plus the explicitly unsupported Graphite contract. Its framework is resolved
from `WindowsTargetFrameworksCurrent`; a missing UI host or presentation failure
fails the leg rather than becoming a GPU skip.
