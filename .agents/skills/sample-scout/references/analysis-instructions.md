# Sample Scout — Analysis Instructions

Read this file before analyzing any GM samples. It contains the classification criteria,
interest level guidelines, and API availability checking methodology.

## Classification Criteria

### Interest Level

| Level | Criteria | Examples |
|-------|----------|---------|
| **high** | Visually impressive, demonstrates a useful technique, showcases a powerful API that users would want to learn. Would make a great Gallery demo. | Runtime shaders, shadow utils, path effects showcase, gradient interpolation, mesh drawing, image filter chains, text effects |
| **medium** | Educational or useful reference but not visually exciting on its own. Could be combined with other samples. | Bitmap copy, color matrix, simple clip operations, tile modes, basic shapes |
| **low** | Internal test, bug regression, GPU-backend-specific test, stress test, or not relevant to SkiaSharp users. | crbug_* files, skbug_* files, Graphite-only tests, GrFragmentProcessor tests, Vulkan/Dawn internals, benchmark GMs |

### Quick Classification Rules

- Files starting with `crbug_` or `skbug` → **low** (bug regression tests)
- Files mentioning `Graphite`, `Dawn`, `GrFragmentProcessor`, `GrGeometryProcessor` → **low** (GPU internals)
- Files testing a single specific visual feature with nice output → **high**
- Files testing edge cases of an already-covered feature → **medium**
- Files showing off compositing, effects chains, or creative techniques → **high**

### API Availability Check

For each key Skia API used in a GM sample, check if SkiaSharp has a C# equivalent:

| Skia C++ | SkiaSharp C# | Where to check |
|----------|-------------|----------------|
| `SkCanvas::drawRect` | `SKCanvas.DrawRect` | `binding/SkiaSharp/SKCanvas.cs` |
| `SkPaint` properties | `SKPaint` properties | `binding/SkiaSharp/SKPaint.cs` |
| `SkShader` factories | `SKShader.Create*` | `binding/SkiaSharp/SKShader.cs` |
| `SkImageFilter` factories | `SKImageFilter.Create*` | `binding/SkiaSharp/SKImageFilter.cs` |
| `SkPathEffect` factories | `SKPathEffect.Create*` | `binding/SkiaSharp/SKPathEffect.cs` |
| `SkColorFilter` factories | `SKColorFilter.Create*` | `binding/SkiaSharp/SKColorFilter.cs` |
| `SkRuntimeEffect` | `SKRuntimeEffect` | `binding/SkiaSharp/SKRuntimeEffect.cs` |
| `SkMesh` | ❌ Not available | — |
| `SkShadowUtils` | ❌ Not available | — |
| `SkImageFilters::RuntimeShader` | ❌ Not available | — |
| `SkColorFilters::HSLAMatrix` | ❌ Not available | — |
| GPU-specific (GrDirectContext, GrBackendTexture) | ❌ Not relevant | Internal GPU plumbing |
| `experimental_DrawEdgeAAQuad` | ❌ Not available | Experimental API |

To verify, grep the binding directory:
```bash
grep -ril "MethodName\|method_name" binding/SkiaSharp/
```

### What Makes a Good Gallery Sample

A good SkiaSharp Gallery sample:
1. **Looks impressive** — visual output that makes people say "wow, SkiaSharp can do that?"
2. **Teaches something** — demonstrates an API or technique users would want to learn
3. **Is self-contained** — doesn't need external resources (or uses bundled assets)
4. **Has controls** — interactive parameters users can tweak to understand the API
5. **Works on all platforms** — no GPU-specific or platform-specific requirements

### Notes Field

Use the notes field to capture important context:
- "GPU-only" — requires GPU backend, won't work on CPU raster
- "Graphite-specific" — only relevant to Graphite backend (not SkiaSharp)
- "Bug regression test" — tests a specific fixed bug, not a feature demo
- "Stress test" — performance/edge case test, not visual
- "Requires test fonts" — needs specific font files
- "Animated" — has frame-based animation
- "Combines well with X" — could be merged with another GM for a richer sample

## Output Schema

Each finding must have these fields:

```json
{
  "file": "mesh.cpp",
  "name": "Custom Vertex Mesh Drawing",
  "description": "Draws custom vertex meshes with SkSL vertex/fragment programs and per-vertex attributes.",
  "interesting": "high",
  "apis_available": false,
  "missing_apis": ["SkMesh", "SkMeshSpecification", "SkCanvas::drawMesh"],
  "key_apis": ["SkMesh::Make", "SkMeshSpecification::Make", "SkCanvas::drawMesh"],
  "notes": "Requires SkMesh API which is not yet exposed in SkiaSharp",
  "visualGoal": "A colorful warped grid of triangles with per-vertex colors, demonstrating programmable vertex displacement and custom fragment shading.",
  "suggestedControls": ["Grid density slider (4–32)", "Warp amplitude slider (0–50)", "Color mode toggle (rainbow/grayscale)"],
  "category": "Shaders",
  "skiaSharpApis": ["SKCanvas.DrawVertices", "SKRuntimeEffect", "SKVertices"]
}
```

### Required fields
`file`, `name`, `description`, `interesting`, `apis_available`, `missing_apis`, `key_apis`, `visualGoal`, `suggestedControls`, `category`, `skiaSharpApis`

All fields are required on every finding. `missing_apis` and `suggestedControls` should be empty arrays `[]` when not applicable.
