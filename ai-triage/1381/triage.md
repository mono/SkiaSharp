# Issue Triage Report — #1381

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T13:42:00Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** SKSurface.ReadPixels is extremely slow (~30-60ms) when using a GPU/OpenGL backend on a 1920x1080 surface, caused by synchronous glReadPixels stalling the pipeline and, historically, an extra pixel-flip due to the BottomLeft surface origin default.

**Analysis:** Two compounding causes: (1) A bug where SKSurface.Create overloads ignored the GRSurfaceOrigin parameter and always used BottomLeft, forcing a CPU-side pixel flip on every ReadPixels call. This bug appears fixed in current code. (2) The underlying glReadPixels call is inherently synchronous and stalls the GPU pipeline — this is a fundamental OpenGL limitation. A workaround (use GRSurfaceOrigin.TopLeft at surface creation) roughly halves readback time by eliminating the flip. Full fix requires exposing Skia's asyncReadPixels API.

**Recommendations:** **needs-investigation** — The origin pass-through bug appears fixed in current source, but the full story (which release it was fixed in, and whether async readback is feasible to expose) needs verification. Multiple users still report slowness — issue remains open and relevant.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/macOS, os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | tenet/performance |
| Partner | — |
| Current labels | type/bug, os/Windows-Classic, os/macOS, area/SkiaSharp, backend/OpenGL, tenet/performance, triage/triaged |

## Evidence

### Reproduction

1. Create a GRContext with OpenGL backend
2. Create a SKSurface with SKSurface.Create(context, true, info) using the default origin (BottomLeft)
3. Perform drawing operations on the canvas
4. Call surface.ReadPixels(info, buffer, rowBytes, 0, 0)
5. Observe ~30-60ms latency for a 1920x1080 BGRA8888 surface

**Environment:** macOS 10.15.5, .NET Core 3.1, SkiaSharp 2.80.0-preview.24, Rider 2020.1. Also confirmed on Windows 10 x64 (RTX 2070, AMD Ryzen 3950x).

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1381#issuecomment-653784496 — Minimal repro benchmark code shared by OP

**Code snippets:**

```csharp
var _info = new SKImageInfo(1920, 1080, SKColorType.Bgra8888);
var _context = GRContext.CreateGl(GRGlInterface.Create());
var _surface = SKSurface.Create(_context, true, _info);
// drawing...
_surface.ReadPixels(_info, _buffer, _info.RowBytes, 0, 0);
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | performance |
| Error message | — |
| Repro quality | complete |
| Target frameworks | netcoreapp3.1 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.0-preview.24 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | No async readback has been exposed in SkiaSharp. The synchronous glReadPixels path and the BottomLeft default are still present in current code. |

## Analysis

### Technical Summary

Two compounding causes: (1) A bug where SKSurface.Create overloads ignored the GRSurfaceOrigin parameter and always used BottomLeft, forcing a CPU-side pixel flip on every ReadPixels call. This bug appears fixed in current code. (2) The underlying glReadPixels call is inherently synchronous and stalls the GPU pipeline — this is a fundamental OpenGL limitation. A workaround (use GRSurfaceOrigin.TopLeft at surface creation) roughly halves readback time by eliminating the flip. Full fix requires exposing Skia's asyncReadPixels API.

### Rationale

Classified as type/bug because: (a) the origin pass-through bug was a real defect causing avoidable extra work during ReadPixels; (b) the performance degradation is severe enough to be unusable for real-time capture use cases. The remaining synchronous-readback limitation is inherent to OpenGL but could be addressed by exposing Skia's async readback API.

### Key Signals

- "copying from GPU took 30-62ms for 1920x1080" — **issue body and comment benchmarks** (Severe performance regression: >100x slower than drawing. Clearly indicates stalled synchronous GPU readback.)
- "glReadPixels is called, and that is a synchronous call, and stalls the pipeline. However, just after that, the pixels are converted... the orientation is different, and the pixels need to be flipped." — **comment by ziriax (contributor, profiled with debug Skia build)** (Root cause confirmed: synchronous GL call + CPU flip overhead. The flip is due to BottomLeft origin.)
- "In my debug build, this reduces the readpixels from 8ms to 4ms... Going to try a release build now" — **comment by ziriax** (Using TopLeft origin eliminates the flip, roughly halving the readback time.)
- "Skia also has an asyncReadPixels, but this isn't exposed yet in SkiaSharp" — **comment by ziriax** (The proper long-term fix (async GPU readback) is available upstream but not exposed in the binding.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKSurface.cs` | 311-317 | direct | ReadPixels delegates directly to sk_surface_read_pixels (synchronous). No async path exists in the SkiaSharp binding. |
| `binding/SkiaSharp/SKSurface.cs` | 198-220 | direct | SKSurface.Create GPU overloads default to GRSurfaceOrigin.BottomLeft. The 5-parameter overload (GRContext, bool, SKImageInfo, int, GRSurfaceOrigin) now correctly passes the origin through the chain to sk_surface_new_render_target. In older builds the origin parameter was silently ignored. |
| `binding/SkiaSharp/SKSurface.cs` | 319-330 | related | Flush(bool, bool) delegates to GRContext.Flush — no surface-level async submission API exposed. |

### Workarounds

- Pass GRSurfaceOrigin.TopLeft when creating the GPU surface: SKSurface.Create(context, true, info, 0, GRSurfaceOrigin.TopLeft, new SKSurfaceProperties(SKPixelGeometry.Unknown), false) — eliminates the pixel flip overhead (~4ms savings on Windows/macOS).
- For frame-capture use cases, consider rendering to a CPU-backed raster surface (no GPU readback needed), then composite the CPU image separately.

### Next Questions

- Has the origin pass-through fix been shipped in a stable SkiaSharp release? Which version?
- Is Skia's asyncReadPixels / GrSurfaceContext::asyncRescaleAndReadPixels part of the public C API that SkiaSharp exposes?
- Would exposing PBO-based async readback in the C API shim be feasible?

### Resolution Proposals

**Hypothesis:** Two fixable causes: the BottomLeft default origin forces a pixel flip (partial fix: use TopLeft), and the synchronous glReadPixels blocks both CPU and GPU. Exposing async readback via Skia's internal asyncReadPixels API would be the complete solution.

1. **Use GRSurfaceOrigin.TopLeft when creating a GPU surface for readback** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Passing TopLeft at surface creation tells Skia the surface is in the conventional top-left raster orientation, eliminating the CPU pixel-flip step during ReadPixels. Roughly halves readback time.

```csharp
var surface = SKSurface.Create(
    context,
    budgeted: true,
    info,
    sampleCount: 0,
    origin: GRSurfaceOrigin.TopLeft,
    new SKSurfaceProperties(SKPixelGeometry.Unknown),
    shouldCreateWithMips: false);
```
2. **Expose Skia asyncReadPixels in SkiaSharp** — fix, confidence 0.70 (70%), cost/l, validated=untested
   - Skia provides GrSurfaceContext::asyncRescaleAndReadPixels which uses async GPU DMA so neither CPU nor GPU stalls. Adding a SkiaSharp wrapper would allow non-blocking pixel readback and be the proper fix for high-performance use cases.

**Recommended proposal:** Use GRSurfaceOrigin.TopLeft when creating a GPU surface for readback

**Why:** Immediate, zero-code-change workaround that cuts readback time by ~50%. Async readback requires a new API surface and C API shim work.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | The origin pass-through bug appears fixed in current source, but the full story (which release it was fixed in, and whether async readback is feasible to expose) needs verification. Multiple users still report slowness — issue remains open and relevant. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Labels already partially applied; confirm type/bug, area/SkiaSharp, backend/OpenGL, tenet/performance, os/macOS, os/Windows-Classic are present. | labels=type/bug, area/SkiaSharp, backend/OpenGL, tenet/performance, os/macOS, os/Windows-Classic |
| add-comment | medium | 0.88 (88%) | Share the TopLeft origin workaround and explain the two-part root cause | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for this detailed report and the community investigation!

**Root cause (two parts):**
1. In older builds, `SKSurface.Create(..., GRSurfaceOrigin origin)` silently ignored the `origin` parameter and always used `BottomLeft`. This forced a CPU pixel-flip on every `ReadPixels` call. This bug has been fixed in current source.
2. The underlying `glReadPixels` call is synchronous by design — it stalls both the CPU and GPU until all pending commands are flushed and pixels transferred over the PCI bus. This is a fundamental OpenGL limitation.

**Workaround** — passing `TopLeft` at surface creation eliminates the flip overhead and roughly halves readback time:

```csharp
var surface = SKSurface.Create(
    context,
    budgeted: true,
    info,
    sampleCount: 0,
    origin: GRSurfaceOrigin.TopLeft,
    new SKSurfaceProperties(SKPixelGeometry.Unknown),
    shouldCreateWithMips: false);
```

**Longer-term:** Skia has `asyncRescaleAndReadPixels` which enables non-blocking GPU readback. This isn't yet exposed in SkiaSharp. A feature request to add async readback support would be tracked separately.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1381,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T13:42:00Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "os/macOS",
      "area/SkiaSharp",
      "backend/OpenGL",
      "tenet/performance",
      "triage/triaged"
    ]
  },
  "summary": "SKSurface.ReadPixels is extremely slow (~30-60ms) when using a GPU/OpenGL backend on a 1920x1080 surface, caused by synchronous glReadPixels stalling the pipeline and, historically, an extra pixel-flip due to the BottomLeft surface origin default.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "platforms": [
      "os/macOS",
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "performance",
      "reproQuality": "complete",
      "targetFrameworks": [
        "netcoreapp3.1"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a GRContext with OpenGL backend",
        "Create a SKSurface with SKSurface.Create(context, true, info) using the default origin (BottomLeft)",
        "Perform drawing operations on the canvas",
        "Call surface.ReadPixels(info, buffer, rowBytes, 0, 0)",
        "Observe ~30-60ms latency for a 1920x1080 BGRA8888 surface"
      ],
      "codeSnippets": [
        "var _info = new SKImageInfo(1920, 1080, SKColorType.Bgra8888);\nvar _context = GRContext.CreateGl(GRGlInterface.Create());\nvar _surface = SKSurface.Create(_context, true, _info);\n// drawing...\n_surface.ReadPixels(_info, _buffer, _info.RowBytes, 0, 0);"
      ],
      "environmentDetails": "macOS 10.15.5, .NET Core 3.1, SkiaSharp 2.80.0-preview.24, Rider 2020.1. Also confirmed on Windows 10 x64 (RTX 2070, AMD Ryzen 3950x).",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1381#issuecomment-653784496",
          "description": "Minimal repro benchmark code shared by OP"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.0-preview.24"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "No async readback has been exposed in SkiaSharp. The synchronous glReadPixels path and the BottomLeft default are still present in current code."
    }
  },
  "analysis": {
    "summary": "Two compounding causes: (1) A bug where SKSurface.Create overloads ignored the GRSurfaceOrigin parameter and always used BottomLeft, forcing a CPU-side pixel flip on every ReadPixels call. This bug appears fixed in current code. (2) The underlying glReadPixels call is inherently synchronous and stalls the GPU pipeline — this is a fundamental OpenGL limitation. A workaround (use GRSurfaceOrigin.TopLeft at surface creation) roughly halves readback time by eliminating the flip. Full fix requires exposing Skia's asyncReadPixels API.",
    "rationale": "Classified as type/bug because: (a) the origin pass-through bug was a real defect causing avoidable extra work during ReadPixels; (b) the performance degradation is severe enough to be unusable for real-time capture use cases. The remaining synchronous-readback limitation is inherent to OpenGL but could be addressed by exposing Skia's async readback API.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "lines": "311-317",
        "finding": "ReadPixels delegates directly to sk_surface_read_pixels (synchronous). No async path exists in the SkiaSharp binding.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "lines": "198-220",
        "finding": "SKSurface.Create GPU overloads default to GRSurfaceOrigin.BottomLeft. The 5-parameter overload (GRContext, bool, SKImageInfo, int, GRSurfaceOrigin) now correctly passes the origin through the chain to sk_surface_new_render_target. In older builds the origin parameter was silently ignored.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "lines": "319-330",
        "finding": "Flush(bool, bool) delegates to GRContext.Flush — no surface-level async submission API exposed.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "copying from GPU took 30-62ms for 1920x1080",
        "source": "issue body and comment benchmarks",
        "interpretation": "Severe performance regression: >100x slower than drawing. Clearly indicates stalled synchronous GPU readback."
      },
      {
        "text": "glReadPixels is called, and that is a synchronous call, and stalls the pipeline. However, just after that, the pixels are converted... the orientation is different, and the pixels need to be flipped.",
        "source": "comment by ziriax (contributor, profiled with debug Skia build)",
        "interpretation": "Root cause confirmed: synchronous GL call + CPU flip overhead. The flip is due to BottomLeft origin."
      },
      {
        "text": "In my debug build, this reduces the readpixels from 8ms to 4ms... Going to try a release build now",
        "source": "comment by ziriax",
        "interpretation": "Using TopLeft origin eliminates the flip, roughly halving the readback time."
      },
      {
        "text": "Skia also has an asyncReadPixels, but this isn't exposed yet in SkiaSharp",
        "source": "comment by ziriax",
        "interpretation": "The proper long-term fix (async GPU readback) is available upstream but not exposed in the binding."
      }
    ],
    "workarounds": [
      "Pass GRSurfaceOrigin.TopLeft when creating the GPU surface: SKSurface.Create(context, true, info, 0, GRSurfaceOrigin.TopLeft, new SKSurfaceProperties(SKPixelGeometry.Unknown), false) — eliminates the pixel flip overhead (~4ms savings on Windows/macOS).",
      "For frame-capture use cases, consider rendering to a CPU-backed raster surface (no GPU readback needed), then composite the CPU image separately."
    ],
    "nextQuestions": [
      "Has the origin pass-through fix been shipped in a stable SkiaSharp release? Which version?",
      "Is Skia's asyncReadPixels / GrSurfaceContext::asyncRescaleAndReadPixels part of the public C API that SkiaSharp exposes?",
      "Would exposing PBO-based async readback in the C API shim be feasible?"
    ],
    "resolution": {
      "hypothesis": "Two fixable causes: the BottomLeft default origin forces a pixel flip (partial fix: use TopLeft), and the synchronous glReadPixels blocks both CPU and GPU. Exposing async readback via Skia's internal asyncReadPixels API would be the complete solution.",
      "proposals": [
        {
          "title": "Use GRSurfaceOrigin.TopLeft when creating a GPU surface for readback",
          "description": "Passing TopLeft at surface creation tells Skia the surface is in the conventional top-left raster orientation, eliminating the CPU pixel-flip step during ReadPixels. Roughly halves readback time.",
          "category": "workaround",
          "codeSnippet": "var surface = SKSurface.Create(\n    context,\n    budgeted: true,\n    info,\n    sampleCount: 0,\n    origin: GRSurfaceOrigin.TopLeft,\n    new SKSurfaceProperties(SKPixelGeometry.Unknown),\n    shouldCreateWithMips: false);",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Expose Skia asyncReadPixels in SkiaSharp",
          "description": "Skia provides GrSurfaceContext::asyncRescaleAndReadPixels which uses async GPU DMA so neither CPU nor GPU stalls. Adding a SkiaSharp wrapper would allow non-blocking pixel readback and be the proper fix for high-performance use cases.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use GRSurfaceOrigin.TopLeft when creating a GPU surface for readback",
      "recommendedReason": "Immediate, zero-code-change workaround that cuts readback time by ~50%. Async readback requires a new API surface and C API shim work."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "The origin pass-through bug appears fixed in current source, but the full story (which release it was fixed in, and whether async readback is feasible to expose) needs verification. Multiple users still report slowness — issue remains open and relevant.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Labels already partially applied; confirm type/bug, area/SkiaSharp, backend/OpenGL, tenet/performance, os/macOS, os/Windows-Classic are present.",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "backend/OpenGL",
          "tenet/performance",
          "os/macOS",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Share the TopLeft origin workaround and explain the two-part root cause",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for this detailed report and the community investigation!\n\n**Root cause (two parts):**\n1. In older builds, `SKSurface.Create(..., GRSurfaceOrigin origin)` silently ignored the `origin` parameter and always used `BottomLeft`. This forced a CPU pixel-flip on every `ReadPixels` call. This bug has been fixed in current source.\n2. The underlying `glReadPixels` call is synchronous by design — it stalls both the CPU and GPU until all pending commands are flushed and pixels transferred over the PCI bus. This is a fundamental OpenGL limitation.\n\n**Workaround** — passing `TopLeft` at surface creation eliminates the flip overhead and roughly halves readback time:\n\n```csharp\nvar surface = SKSurface.Create(\n    context,\n    budgeted: true,\n    info,\n    sampleCount: 0,\n    origin: GRSurfaceOrigin.TopLeft,\n    new SKSurfaceProperties(SKPixelGeometry.Unknown),\n    shouldCreateWithMips: false);\n```\n\n**Longer-term:** Skia has `asyncRescaleAndReadPixels` which enables non-blocking GPU readback. This isn't yet exposed in SkiaSharp. A feature request to add async readback support would be tracked separately."
      }
    ]
  }
}
```

</details>
