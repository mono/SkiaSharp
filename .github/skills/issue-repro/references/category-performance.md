# Performance Category

Identification signals and reproduction strategies for performance and FPS issues.
These may use either `reproduced` / `not-reproduced` (for measurable regressions) or
`confirmed` / `not-confirmed` (for "always been slow" enhancement-style reports).

For platform-specific setup (how to create/build/run), see the `platform-*.md` files.

---

**Signals:** Slow rendering, low FPS, frame rate drop, performance degradation, jank, stutter,
latency, "much slower than native," GPU bottleneck, frame time, rendering performance, "takes
too long."

## Goal

A performance reproduction requires **your own measurement of a gap**. "The reporter said it was
slow and I also saw it was slow" is not a reproduction — you need a baseline to compare against.
Observing 12fps means nothing without knowing what "fast" looks like on the same machine. The
reporter's claimed numbers are not your baseline; your own measurements are.

The end goal is the same as any repro: a **minimal reproduction** with everything unnecessary
removed, but still demonstrating the performance gap. The minimal repro must use the **same
rendering mode** as the reported issue:

- **GPU issue** (SKGLView, SKMetalView, "slow rendering in my app") → standalone GPU app, no
  framework (no Avalonia, no MAUI, no WPF). Use SKGLView or SKMetalView directly.
- **CPU issue** (SKSurface.Create raster, "image processing is slow", "encoding takes too long")
  → console app with CPU raster surface is correct.

The key rule: **stay in the reporter's rendering mode.** GPU bugs need GPU repros. CPU bugs need
CPU repros. Crossing modes proves nothing — GPU and CPU use entirely different Skia code paths.

## Strategy

SKILL.md Rules 7–9 apply with particular force here. Performance reproduction **replaces**
SKILL.md's standard Phase 3A/3B/3C flow with its own ordering. Do not follow the standard
version-based phases — follow these steps instead.

### Phase 3A: Measure both sides of the reporter's comparison

**Step 1 — Build and run the reporter's baseline (the "fast" side).**
If the reporter provides a native C++ benchmark or alternative implementation, BUILD AND RUN IT
on your machine. Follow their build instructions (GN, CMake, Ninja, etc.).

These builds can be slow — Skia dependency syncs (`git-sync-deps`) may take 10-20 minutes and
native compilation may take longer. Be patient, retry on network timeouts, and don't give up
because a step takes a while. Record the FPS/timing you observe.

**Without this number, you cannot quantify the gap.** If you only measure the "slow" side, you
haven't reproduced a performance issue — you've just confirmed the reporter's code runs.

**Step 2 — Build and run the reporter's test (the "slow" side).**
Build and run the reporter's SkiaSharp benchmark with their exact version, platform, and
configuration. The rendering mode, backend, and scene complexity MUST match the baseline
(Rule 9). Record your FPS/timing.

Now you have both sides of the comparison measured by you.

### Phase 3B: Test reporter's code on latest SkiaSharp

Same as standard 3B — run the reporter's SkiaSharp benchmark with the latest stable release in
a clean project directory. Record whether the gap changes.

### Phase 3C: Create YOUR standalone repro (MANDATORY — do not skip)

This is the most important step. The reporter's code is evidence, but it's not YOUR reproduction.
Their project may have framework overhead, debug settings, or other variables. You need your own
minimal app that demonstrates the gap independently.

**For GPU performance bugs:**

1. Read [platform-macos.md](platform-macos.md) (or the relevant platform file)
2. Create a new macOS/Windows/Linux GPU app using the platform file's "Create Project" steps
3. Add an SKGLView or SKMetalView using the "Backend-Specific Templates" from the platform file
4. Put the reporter's drawing scene in the PaintSurface handler
5. Add FPS measurement and per-phase timing (see below)
6. Build and run it — this standalone GPU app IS your minimal repro

Example for macOS GPU:
```bash
mkdir -p /tmp/skiasharp/repro/{number}-standalone && cd /tmp/skiasharp/repro/{number}-standalone
dotnet new macos -n GpuRepro --framework net10.0-macos
cd GpuRepro
dotnet add package SkiaSharp.Views --version {reporter_version}
```
Then in AppDelegate.cs, create an SKGLView with the drawing scene (see platform-macos.md for
the full template).

**For CPU performance bugs:** Create a console app with `SKSurface.Create` raster and the same
workload. Console IS correct for CPU issues.

**What this tells you:** If your standalone GPU app is also slow → the gap is in SkiaSharp's GPU
rendering. If it's fast → the gap is in the reporter's framework integration (Avalonia, MAUI,
etc.), not SkiaSharp.

### Phase 3D: Instrument your standalone repro

Add `System.Diagnostics.Stopwatch` to your standalone repro to identify WHERE the time goes:

- `render`: CPU-side draw calls (canvas.DrawX)
- `flush`: Skia command submission to GPU (canvas.Flush / context.Flush)
- `finish`: GPU drain (glFinish / commandBuffer.WaitUntilCompleted)
- `swap`: Buffer swap / present

Disable VSync before measuring — it caps frame rate to 60/120fps, masking real differences.

Record FPS only after 5+ consecutive frames within 10% variance. Discard first 10 frames
(JIT warm-up).

## Timeouts and retries

Network operations (git clone, git-sync-deps, package restore) may be slow. Retry 2-3 times
before considering a step failed. Native builds (Skia via GN/Ninja) can take 15+ minutes on
first build — that's normal. Only record a genuine blocker when the toolchain or platform is
truly unavailable, not when something is slow.

## Example: Per-phase timing

```csharp
// In an SKGLView.PaintSurface handler (GPU) or with SKSurface.Create (CPU)
var sw = Stopwatch.StartNew();
canvas.Clear(SKColors.White);
foreach (var elem in elements)
    canvas.DrawPath(elem.Path, elem.Paint);
var renderMs = sw.Elapsed.TotalMilliseconds;
sw.Restart();
canvas.Flush();
var flushMs = sw.Elapsed.TotalMilliseconds;
Console.WriteLine($"render={renderMs:F1}ms flush={flushMs:F1}ms");
```

## Pitfalls

- VSync masks real performance — always disable before measuring
- "120fps native" may be VSync-masked — verify native numbers yourself
- Without a baseline, "slow" is meaningless — you need both sides of the comparison
- **Mode crossing invalidates data** — GPU-to-CPU or CPU-to-GPU comparisons prove nothing

## Conclusion

- `reproduced`: You measured a performance gap yourself (include YOUR fps/timing data for
  both sides — baseline AND test)
- `not-reproduced`: Your measurements show no gap (include comparative data)
- Use `assessment: "likely-bug"` if the gap is clearly abnormal

## Only bail when

- No access to GPU or required platform (conclude `needs-platform`)
- Reporter's benchmark requires proprietary tools/hardware unavailable
- After 3+ substantially different measurement approaches show no gap
