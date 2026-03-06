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

## Follow SKILL.md Phase 3 with these additions

Performance bugs follow the same Phase 3A–3E flow as all other bugs. The additions below apply
on top of the universal flow.

### In Phase 3A (baseline)

The reporter's baseline is especially important for performance — it's the "fast" side of the
comparison. Without it, you have no way to quantify the gap.

Baseline builds (native C++, GN/Ninja) can be slow. Skia dependency syncs may take 10-20
minutes and compilation may take longer. Be patient, retry on network timeouts. Only record a
genuine blocker when the toolchain is truly unavailable, not when something is slow.

### In Phase 3C (your minimal repro)

Your observable is **timing or FPS** instead of crash or bad image. Add per-phase instrumentation
to your standalone repro with `System.Diagnostics.Stopwatch`:

- `render`: CPU-side draw calls (canvas.DrawX)
- `flush`: Skia command submission to GPU (canvas.Flush / context.Flush)
- `finish`: GPU drain (glFinish / commandBuffer.WaitUntilCompleted)
- `swap`: Buffer swap / present

**Disable VSync** before measuring — it caps frame rate to 60/120fps, masking real differences.

**Statistical stability:** Record FPS only after 5+ consecutive frames within 10% variance.
Discard first 10 frames (JIT warm-up).

### Example: Per-phase timing

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
