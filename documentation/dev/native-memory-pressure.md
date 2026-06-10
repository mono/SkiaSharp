# Native Memory Pressure Reporting

The .NET garbage collector has no visibility into Skia's native allocations. A
process can hold gigabytes of native pixel buffers, paths, glyph caches, and
similar through SkiaSharp wrappers without the GC ever being prompted to
reclaim eligible managed wrappers. This causes memory bloat that looks like a
leak but is actually just the GC waiting for managed pressure that never
arrives.

`SKNativeMemoryPressureMonitor` is an opt-in mechanism that reports Skia's
outstanding native byte count to the GC via
`GC.AddMemoryPressure` / `GC.RemoveMemoryPressure`.

## The flag

The feature is **off by default**. Enable it programmatically, typically at
application startup:

```csharp
SKNativeMemoryPressureMonitor.Start();
// or with a custom threshold:
SKNativeMemoryPressureMonitor.Start(thresholdBytes: 4 * 1024 * 1024);
```

Disable it at shutdown (or any time):

```csharp
SKNativeMemoryPressureMonitor.Stop();
```

`Start()` is idempotent; `Stop()` is a no-op when the monitor isn't running.
See *Tuning* below for the threshold.

## How it works

```
Skia native allocator               managed monitor                GC
────────────────────                 ───────────────────            ───
sk_malloc / sk_free
 (src/ports/SkMemory_malloc.cpp)
  ├─ hook: account(delta) ──► atomic counter ──┐
  │     (src/c/sk_memory.cpp)                  │
  └─ if |counter - last_notified| ≥ N:         │
         CAS to claim crossing                 │
         callback() ───────────────────────────┴─► OnNativeThresholdCrossed
                                                    ├─ Interlocked.Exchange flag
                                                    └─ ThreadPool.UnsafeQueueUserWorkItem
                                                        └─ Reconcile()
                                                             ├─ read counter
                                                             ├─ compute delta
                                                             └─ GC.AddMemoryPressure(delta)
                                                                       ──► nudges GC heuristic
```

1. The upstream allocator (`src/ports/SkMemory_malloc.cpp`) is instrumented
   with two `extern "C"` hook calls per allocation: `sk_memory_internal_size_of`
   measures the block via `malloc_usable_size`/`_msize`/`malloc_size`, and
   `sk_memory_internal_account_delta` updates a process-wide
   `std::atomic<int64_t>`. Both helpers live in `src/c/sk_memory.cpp` so the
   upstream allocator's diff is tiny — easier to maintain across Skia bumps.

2. When the counter drifts past **±threshold bytes** since the last
   notification, the allocator fires a registered callback. A
   `compare_exchange` on a *last-notified* watermark ensures only one
   thread fires per crossing.

3. The managed callback (running on whichever thread did the allocation)
   does minimal work: an `Interlocked.Exchange` flag plus a single
   `ThreadPool.UnsafeQueueUserWorkItem`. It never takes managed locks
   that could be held by the caller, never calls back into Skia.

4. The thread-pool work item reads the counter, computes the delta vs.
   what it last told the GC, and calls `GC.AddMemoryPressure` /
   `RemoveMemoryPressure`.

No background timer. No polling. Idle apps pay nothing.

## Public API

| Member | Purpose |
|---|---|
| `SKNativeMemoryPressureMonitor.Start()` | Start with the default 1 MB threshold. |
| `SKNativeMemoryPressureMonitor.Start(long thresholdBytes)` | Start with a custom threshold. |
| `SKNativeMemoryPressureMonitor.Stop()` | Detach the callback and release outstanding pressure. |
| `SKNativeMemoryPressureMonitor.IsRunning` | Is the callback currently installed? |
| `SKNativeMemoryPressureMonitor.ReportedPressure` | Bytes currently reported to the GC (diagnostic). |
| `SKNativeMemoryPressureMonitor.DefaultThresholdBytes` | The 1 MB default. |
| `SKGraphics.GetNativeMemoryAllocated()` | Live native byte counter (works whether the monitor is running or not). |

## Tuning the threshold

The threshold controls the trade-off between **accuracy** and **callback
frequency**:

- A smaller threshold (e.g. 64 KB) reports pressure closer to real time but
  fires the callback more often, increasing thread-pool work.
- A larger threshold (e.g. 16 MB) keeps the callback quiet but lets the GC's
  view drift further from reality between notifications.

The default 1 MB is a reasonable balance for typical workloads.

## Limitations

- **Third-party allocations are invisible.** Only Skia's own
  `sk_malloc`/`sk_free` are accounted for. Libraries linked into
  `libSkiaSharp` that call `malloc` directly — libpng, libjpeg-turbo,
  freetype, harfbuzz, expat, zlib, libwebp, dng_sdk, wuffs — bypass the
  counter. Apps doing heavy image decoding or text shaping will see the
  GC's view understate native memory.

- **Threshold-band staleness.** Between notifications, the GC's view can
  drift up to `threshold` bytes from reality. With the default 1 MB
  threshold and an idle process holding 999 KB, the GC sees no pressure
  until the next allocation pushes the counter over.

- **`GC.AddMemoryPressure` only nudges the heuristic.** It does not force a
  collection. Even with perfect reporting, the actual reclamation lag is
  still up to the GC — particularly under Server GC, where Gen 2 collection
  is rarer.

- **Allocator overhead.** Every `sk_malloc`/`sk_free` does a
  `malloc_usable_size` lookup and an atomic add. Measured overhead on
  mid-size allocations is ≈3% (≈15 ns per op). Large allocations dispatched
  via `mmap` see the cost buried under syscall noise.

- **Process-singleton.** State is static. The native callback pointer is
  process-wide. Multi-AppDomain hosts share one monitor.

## Stopping cleanly

`Stop()` detaches the native callback and releases any pressure previously
reported. In-flight thread-pool reconciles that run after `Stop()` observe
the stopped state and refuse to add new pressure (preventing a leaked
`AddMemoryPressure` with nobody to call `RemoveMemoryPressure`).

For most apps this only matters at shutdown; long-running apps that toggle
the monitor will see clean state transitions across `Start`/`Stop` cycles.

## See also

- [memory-management.md](memory-management.md) — wrapper-level ownership
  and disposal in SkiaSharp.
- [architecture.md](architecture.md) — overall C# / P-invoke / C API / C++
  Skia layering.
