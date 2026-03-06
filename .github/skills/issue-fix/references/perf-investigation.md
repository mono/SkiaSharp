# Performance Investigation Methodology

For issue-fix Phase 5 when the bug involves performance degradation, slow rendering, or FPS issues.

## Overview

Performance bugs differ from correctness bugs: the code "works" but is slow. This requires quantitative measurement, not just pass/fail verification. The goal is to identify WHERE time is spent and WHY it's slower than expected.

## Step 1: Establish Baselines

### Native C++ Baseline (if reporter provides benchmark)

If the issue includes a native C++ benchmark repo:

1. Clone and build it:
   ```bash
   git clone <reporter-repo> /tmp/skiasharp-perf/native
   cd /tmp/skiasharp-perf/native
   # Follow repo build instructions (typically cmake + ninja)
   ```
2. Run with VSync disabled and record FPS at multiple complexity levels
3. This is your **ceiling** — C# should approach but may not exceed it

If no native baseline exists, use SkiaSharp's own samples as the baseline.

### C# Baseline (using current SkiaSharp views)

Build the same scene using the reporter's SkiaSharp version:

```bash
mkdir -p /tmp/skiasharp-perf/csharp-before && cd /tmp/skiasharp-perf/csharp-before
# Create project matching the reporter's setup (platform, backend, version)
```

Record per-phase timing (see Step 2).

## Step 2: Per-Phase Profiling

**This is the most critical technique.** Instrument each rendering phase with `System.Diagnostics.Stopwatch`:

```csharp
var sw = Stopwatch.StartNew();

// Phase 1: Render (CPU-side draw calls)
canvas.Clear(SKColors.White);
foreach (var elem in elements)
    canvas.DrawPath(elem.Path, elem.Paint);
var renderMs = sw.Elapsed.TotalMilliseconds;

// Phase 2: Canvas Flush (Skia submits to GPU)
sw.Restart();
canvas.Flush();
var flushMs = sw.Elapsed.TotalMilliseconds;

// Phase 3: GPU Drain (wait for GPU completion)
sw.Restart();
// GL: glFinish()  |  Metal: commandBuffer.WaitUntilCompleted()
var finishMs = sw.Elapsed.TotalMilliseconds;

// Phase 4: Buffer Swap (present to screen)
sw.Restart();
// GL: context.FlushBuffer()  |  Metal: drawable.Present()
var swapMs = sw.Elapsed.TotalMilliseconds;

// Log
Console.WriteLine($"render={renderMs:F1}ms flush={flushMs:F1}ms finish={finishMs:F1}ms swap={swapMs:F1}ms total={renderMs+flushMs+finishMs+swapMs:F1}ms");
```

### What Each Phase Tells You

| Phase | If slow... | Likely cause |
|-------|-----------|--------------|
| render | CPU-bound | Complex path tessellation, too many draw calls, slow software fallback |
| flush | GPU submission overhead | Per-draw-call GL state validation, wrong rendering path |
| finish | GPU-bound | Fragment shader complexity, MSAA resolve, fillrate limit |
| swap | VSync | VSync enabled (expected ~16ms at 60Hz, ~8ms at 120Hz) |

## Step 3: Multi-Complexity Testing

Test at **multiple load levels** to distinguish constant overhead from O(n) scaling:

| Level | Elements | Purpose |
|-------|----------|---------|
| C0 | 1,000 | Baseline / constant overhead |
| C4 | 5,000 | Light load |
| C8 | 9,000 | Medium load |
| C12 | 40,000 | Heavy load / stress test |

If performance degrades linearly with element count → per-draw-call overhead.
If performance is constant → fixed overhead (context setup, buffer management).

## Step 4: Multi-Backend Comparison

Test the same scene across available GPU backends:

| Backend | What it tests |
|---------|--------------|
| Metal | Modern GPU API (macOS/iOS default) |
| OpenGL | Legacy GPU API (macOS deprecated, Linux/Windows common) |
| Software (CPU) | No GPU — pure CPU rendering |

If one backend is fast and another slow → backend-specific issue.
If all backends are slow → likely CPU-side or API usage issue.

## Step 5: Isolation Experiments

Change **one variable at a time** and record the result:

| Experiment | What it proves |
|-----------|---------------|
| Enable/disable MSAA | Whether MSAA changes rendering path |
| Batch draw calls | Whether overhead is per-draw-call |
| Raster + blit | Whether GPU path is the bottleneck |
| Managed vs wrapped surface | Whether surface setup affects renderer selection |
| Reduce scene complexity | Whether issue is load-dependent |

### Debugging Table

Maintain a running table for every experiment:

| # | What Changed | Result | What It Proves |
|---|-------------|--------|----------------|
| 1 | Enabled 4x MSAA | Worse (5.3fps→5.3fps) | MSAA alone doesn't help |
| 2 | Used managed surface | render dropped 55ms→8.9ms | Different renderer active |
| 3 | Checked stencil bits | GL reports 0, pixfmt has 8 | ROOT CAUSE |

**Rules:**
- Never change two things at once
- Record the EXACT numbers, not "faster" or "slower"
- If an experiment makes things worse, that's still valuable data
- Revert each experiment before the next one

## Step 6: AI Model Consultation

For complex performance bugs, consult multiple AI models for corroboration:

### Protocol
1. Prepare a data packet: profiling numbers, code snippets, architecture diagram
2. Give the SAME data to 3 different models
3. Ask each for independent analysis of the bottleneck
4. Require 2/3 consensus for corroboration

### How to Invoke
```
Use the `task` tool with agent_type="general-purpose" and model override:
- model: "gpt-5.3-codex" — strong at code reasoning
- model: "gemini-3-pro-preview" — good at architecture analysis  
- model: "claude-opus-4.6" — thorough, detailed analysis
```

### What to Ask Each Model
> "Given these profiling numbers from a SkiaSharp rendering benchmark:
> [paste phase timing table]
> 
> And this code that sets up the rendering context:
> [paste relevant setup code]
> 
> What is the most likely cause of the performance bottleneck?
> What experiments would you suggest to isolate it?"

### Interpreting Results
- All 3 agree → high confidence, proceed with that hypothesis
- 2/3 agree → medium confidence, test the hypothesis before committing
- No agreement → need more data, run another experiment

## Step 7: VSync Control

**Always disable VSync before measuring.** VSync caps frame rate to display refresh rate, making everything look the same.

Each platform has its own VSync mechanism. On macOS, disable via `NSOpenGLContext.setValues(0, forParameter: NSOpenGLCPSwapInterval)`.

**Key indicator of VSync masking:** If all configurations show exactly 60fps or 120fps regardless of scene complexity → VSync is enabled.

## Step 8: Verify the Fix

After implementing a fix, re-run the FULL benchmark matrix:

| What to verify | How |
|---------------|-----|
| Fix actually improves perf | Before/after timing at all complexity levels |
| No regression on other backends | Test Metal AND GL (or other backends) |
| Native baseline comparison | C# should approach native performance |
| Correctness preserved | Visual output should be identical |

## Statistical Requirements

- **Stability detection:** Record FPS only after 5+ consecutive frames within 10% variance
- **Warm-up period:** Discard first 10 frames (JIT, cache warming)
- **Multiple runs:** Run each config 3+ times if results are noisy
- **Report ranges:** "77-93 fps" is more honest than "85 fps"

## Tools Needed

| Tool | Purpose | Install |
|------|---------|---------|
| cmake + ninja | Build native C++ benchmarks | `brew install cmake ninja` (macOS) |
| Stopwatch (System.Diagnostics) | Phase timing | Built into .NET |
| GL diagnostic queries | GPU state inspection | Via P/Invoke |
| dotnet-trace | Managed profiling (if needed) | `dotnet tool install -g dotnet-trace` |
