# Label Taxonomy

Values are **full GitHub labels** (e.g., `type/bug`, `os/Linux`) — use exactly as listed below.

## Cardinality

| Prefix | Card. | Rule |
|--------|-------|------|
| `type/` | **1** (required) | One type per issue — pick the best fit |
| `area/` | **1** (required) | Primary component — "where in the codebase?" |
| `os/` | **0–N** | All platforms affected (omit if cross-platform) |
| `backend/` | **0–N** | All rendering backends affected (omit if not backend-specific) |
| `tenet/` | **0–N** | Quality tenets that apply |
| `perf/` | **0–N** | Performance sub-type — **each implies `tenet/performance`** (add both) |
| `partner/` | **0–1** | Third-party partner flag |

## Valid Labels

### type/ (required, single)
`type/bug` · `type/feature-request` · `type/enhancement` · `type/question` · `type/documentation`

### area/ (required, single)
`area/SkiaSharp` · `area/SkiaSharp.Views` · `area/SkiaSharp.Views.Maui` · `area/SkiaSharp.Views.Blazor` · `area/SkiaSharp.Views.Forms` · `area/SkiaSharp.Views.Uno` · `area/SkiaSharp.HarfBuzz` · `area/HarfBuzzSharp` · `area/SkiaSharp.Workbooks` · `area/libSkiaSharp.native` · `area/libHarfBuzzSharp.native` · `area/Build` · `area/Docs`

Pick the most specific match. SKCanvasView in MAUI → `area/SkiaSharp.Views.Maui`. DllNotFoundException → `area/libSkiaSharp.native`.

### os/ (optional, multiple)
`os/Android` · `os/iOS` · `os/macOS` · `os/Linux` · `os/Windows-Classic` · `os/Windows-WinUI` · `os/Windows-Universal-UWP` · `os/Windows-Nano-Server` · `os/WASM` · `os/Tizen` · `os/tvOS` · `os/watchOS`

### backend/ (optional, multiple)
`backend/Direct3D` · `backend/Metal` · `backend/OpenGL` · `backend/Raster` · `backend/Vulkan` · `backend/PDF` · `backend/SVG` · `backend/XPS`

### tenet/ (optional, multiple)
`tenet/compatibility` · `tenet/performance` · `tenet/reliability`

### perf/ (optional, multiple)
Performance **sub-type** — a finer axis under the `tenet/performance` umbrella. Apply one or
more when an issue is about speed, memory, or size, **and always add `tenet/performance` too**.

`perf/memory-leak` · `perf/allocations` · `perf/interop` · `perf/rendering` · `perf/throughput` · `perf/startup` · `perf/size`

| Label | Use when… | Example |
|-------|-----------|---------|
| `perf/memory-leak` | memory grows unbounded — leaked native handles, undisposed objects, GPU memory not released | "SKGLView leaks memory over time" |
| `perf/allocations` | excessive managed allocations / per-frame GC or heap churn | "per-frame heap allocations in the swapchain" |
| `perf/interop` | P/Invoke marshalling or native interop overhead (incl. lock contention) | "remove native interop in SKMatrix" |
| `perf/rendering` | slow drawing, rendering, or GPU frame performance | "DrawText is slow", "matrix changes drop draw perf" |
| `perf/throughput` | an operation is slower than expected — encode/decode, file loading, pixel/format conversion, readback | "JPEG decode 3× slower than libjpeg-turbo" |
| `perf/startup` | slow initialization or first-use latency | "SKTypeface.CreateDefault very slow on first call" |
| `perf/size` | binary, package, or output-file size | "reduce SkiaSharp dll size", "PDF output too large" |

### partner/ (optional, single)
`partner/maui` · `partner/tizen` · `partner/unoplatform`

## Tips

- Trust content over title prefixes: `[BUG]` title but actually a question → `type/question`
- `enhancement` improves what exists; `feature-request` adds something new
  - **`type/enhancement`**: Improving existing functionality — e.g., "add wheel support to GTK views" (touch infra exists, wheel doesn't), "improve SVG output quality"
  - **`type/feature-request`**: Adding completely new functionality — e.g., "add PDF export", "support new image format"
  - **Platform parity gap**: API exists cross-platform but a specific platform handler is missing → `type/bug` (the API contract is broken). Only use `type/enhancement` if no platform implements it yet.
- `question` asks "how?"; `documentation` says "we need docs for X"
- **Performance issues:** whenever you apply any `perf/*` label, also apply `tenet/performance` — `perf/*` is the sub-type, `tenet/performance` is the umbrella tenet. A **memory leak** (growing native memory, undisposed handles) → `tenet/performance` + `perf/memory-leak` (this is what the `memory-leak-fixer` workflow files). Pick multiple `perf/*` only if genuinely distinct concerns apply.
- When behavior is correct but easy to misuse (disposal ordering, threading, etc.), keep `type/bug` and suggest `close-as-not-a-bug` with a workaround. The `status/by-design` label communicates that the behavior is intentional.
