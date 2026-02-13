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

### partner/ (optional, single)
`partner/maui` · `partner/tizen` · `partner/unoplatform`

## Tips

- Trust content over title prefixes: `[BUG]` title but actually a question → `type/question`
- `enhancement` improves what exists; `feature-request` adds something new
- `question` asks "how?"; `documentation` says "we need docs for X"
