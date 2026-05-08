# Issue Triage Report — #1270

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T18:15:37Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Feature request to expose Skia's GrTriangulator (path-to-vertices conversion) in SkiaSharp so GPU-backed surfaces can render paths faster using DrawVertices instead of DrawPath.

**Analysis:** Reporter requests a static path-to-vertices triangulation helper, ideally wrapping Skia's GrTriangulator, to improve GPU rendering performance by converting polygon paths to SKVertices for DrawVertices(). Maintainer confirmed GrTriangulator lives in Skia's private src/ directory (not the public include/ surface), making direct wrapping unsuitable. Any implementation would require porting the private triangulation logic. The feature remains valid but unimplemented as of the last maintainer comment in June 2020.

**Recommendations:** **keep-open** — Valid performance-motivated feature request with no current implementation. The path is technically feasible but requires significant effort (porting private Skia logic or implementing a C# triangulator). A third-party workaround exists for immediate use.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

**Environment:** No specific version mentioned; issue filed May 2020

**Repository links:**
- https://groups.google.com/forum/#!topic/skia-discuss/sRecOulVo8A — Skia Google Group discussion thread referenced by maintainer regarding GrTriangulator API stability

## Analysis

### Technical Summary

Reporter requests a static path-to-vertices triangulation helper, ideally wrapping Skia's GrTriangulator, to improve GPU rendering performance by converting polygon paths to SKVertices for DrawVertices(). Maintainer confirmed GrTriangulator lives in Skia's private src/ directory (not the public include/ surface), making direct wrapping unsuitable. Any implementation would require porting the private triangulation logic. The feature remains valid but unimplemented as of the last maintainer comment in June 2020.

### Rationale

Clearly a new feature request — no existing SkiaSharp API converts an SKPath to triangulated SKVertices. The underlying Skia class (GrTriangulator) is private/internal and not part of the stable C API surface. The maintainer explicitly investigated and confirmed this. Since the issue is from 2020 with no subsequent activity, it is open-but-unscheduled rather than abandoned — the performance rationale is sound and the request is valid.

### Key Signals

- "On GPU backed surfaces, it is faster to use DrawVertices() instead of DrawPath(). So it would be helpful to have the native GrTriangulator to convert a path with a polygon to vertices." — **issue body** (Performance-motivated feature request; reporter wants triangulation for GPU-optimized rendering.)
- "It would be great to use the skia version, but I see it is in the src directory... The rules for that directory is that things can change at any time." — **comment by mattleibow** (Maintainer identified that GrTriangulator is in Skia's private src/ directory — not a stable public API — making direct wrapping risky.)
- "Because this code is also in the private directories, we will have to port it anyway." — **comment by mattleibow** (Any implementation would require a full port of the private triangulator logic, not a simple C API wrapper.)
- "SKVertices right now is a bit limited in the API surface, but that can change. I see they have builders and all that. We could certainly do this." — **comment by mattleibow** (Maintainer is open to expanding SKVertices API; the feature is feasible in principle.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKVertices.cs` | — | direct | SKVertices.CreateCopy() exists and accepts positions, texs, colors, and indices — the destination API is present but there is no factory method that accepts an SKPath or polygon for triangulation. |
| `binding/SkiaSharp/SKCanvas.cs` | — | direct | DrawVertices() has five overloads including DrawVertices(SKVertices, SKBlendMode, SKPaint) — consuming the requested output is already possible; only the path-to-vertices conversion helper is missing. |

### Workarounds

- Use a third-party C# triangulation library (e.g., LibTessDotNet or poly2tri-cs) to triangulate the polygon, then pass the resulting triangles to SKVertices.CreateCopy(SKVertexMode.Triangles, positions, colors) and draw with SKCanvas.DrawVertices().
- For simple convex polygons, implement a fan triangulation manually: fix vertex[0] as pivot and emit (vertex[0], vertex[i], vertex[i+1]) triples into SKVertices.CreateCopy().

### Next Questions

- Has upstream Skia promoted any path-triangulation utility to its public include/ headers since m84?
- Would a pure C# implementation of the ear-clipping algorithm be sufficient, or is GPU-side tessellation (via Vulkan/Metal geometry shaders) needed for the target use case?
- Does the reporter need fill-rule-correct triangulation (even-odd / non-zero) or only convex-polygon support?

### Resolution Proposals

**Hypothesis:** The feature requires either (a) porting GrTriangulator logic from Skia's private src/ to C# or a new C shim, or (b) wrapping a third-party triangulation library. The existing SKVertices + DrawVertices API is the correct consumer surface; only the conversion helper is missing.

1. **Workaround: third-party triangulation → SKVertices** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Use LibTessDotNet to triangulate the polygon path, then feed results into SKVertices.CreateCopy(). This works today without any SkiaSharp changes.

```csharp
// Add NuGet package: LibTessDotNet
// using LibTessDotNet;
var tess = new Tess();
var contour = points.Select(p => new ContourVertex { Position = new Vec3 { X = p.X, Y = p.Y } }).ToArray();
tess.AddContour(contour, ContourOrientation.Counterclockwise);
tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
var positions = tess.Vertices.Select(v => new SKPoint(v.Position.X, v.Position.Y)).ToArray();
var indices = tess.Elements.Select(i => (ushort)i).ToArray();
var verts = SKVertices.CreateCopy(SKVertexMode.Triangles, positions, null, null, indices);
canvas.DrawVertices(verts, SKBlendMode.Modulate, paint);
```
2. **Feature: Add SKPath.Triangulate() static helper in SkiaSharp** — fix, confidence 0.65 (65%), cost/l, validated=untested
   - Implement a C# ear-clipping or monotone-polygon triangulator as a new static SKPath extension / SKVertices factory that accepts an SKPath and returns an SKVertices. Does not require Skia native changes.
3. **Feature: Port GrTriangulator to a new C API shim** — fix, confidence 0.50 (50%), cost/xl, validated=untested
   - Port relevant portions of Skia's private GrTriangulator to the externals/skia/src/c/ shim layer and expose as sk_path_triangulate(). Requires maintaining a private copy of the triangulator logic across Skia milestone upgrades.

**Recommended proposal:** Workaround: third-party triangulation → SKVertices

**Why:** Available today with no SkiaSharp changes needed. The proper feature implementation requires significant native work or maintaining a private fork of Skia internal code.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid performance-motivated feature request with no current implementation. The path is technically feasible but requires significant effort (porting private Skia logic or implementing a C# triangulator). A third-party workaround exists for immediate use. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request, SkiaSharp core, and performance tenet labels | labels=type/feature-request, area/SkiaSharp, tenet/performance |
| add-comment | medium | 0.82 (82%) | Acknowledge the request, explain why GrTriangulator cannot be directly wrapped, and provide third-party workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for filing this! The core request is valid — converting a path to triangulated vertices can significantly improve GPU-backed rendering throughput.

Unfortunately, Skia's `GrTriangulator` lives in the private `src/` directory rather than the stable `include/` surface, so it cannot be reliably wrapped (it can change at any time). A proper SkiaSharp implementation would require either porting that logic to C# or maintaining a private copy of the internal triangulator across Skia milestone upgrades — which is non-trivial.

**Workaround available today:** Use [LibTessDotNet](https://www.nuget.org/packages/LibTessDotNet/) to triangulate your polygon, then feed the result into the existing `SKVertices.CreateCopy()` + `DrawVertices()` APIs:

```csharp
// Add NuGet package: LibTessDotNet
var tess = new Tess();
var contour = points.Select(p => new ContourVertex { Position = new Vec3 { X = p.X, Y = p.Y } }).ToArray();
tess.AddContour(contour, ContourOrientation.Counterclockwise);
tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
var positions = tess.Vertices.Select(v => new SKPoint(v.Position.X, v.Position.Y)).ToArray();
var indices = tess.Elements.Select(i => (ushort)i).ToArray();
var verts = SKVertices.CreateCopy(SKVertexMode.Triangles, positions, null, null, indices);
canvas.DrawVertices(verts, SKBlendMode.Modulate, paint);
```

Keeping this open as a feature request for a native `SKPath` → `SKVertices` triangulation helper.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1270,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T18:15:37Z"
  },
  "summary": "Feature request to expose Skia's GrTriangulator (path-to-vertices conversion) in SkiaSharp so GPU-backed surfaces can render paths faster using DrawVertices instead of DrawPath.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "No specific version mentioned; issue filed May 2020",
      "repoLinks": [
        {
          "url": "https://groups.google.com/forum/#!topic/skia-discuss/sRecOulVo8A",
          "description": "Skia Google Group discussion thread referenced by maintainer regarding GrTriangulator API stability"
        }
      ]
    }
  },
  "analysis": {
    "summary": "Reporter requests a static path-to-vertices triangulation helper, ideally wrapping Skia's GrTriangulator, to improve GPU rendering performance by converting polygon paths to SKVertices for DrawVertices(). Maintainer confirmed GrTriangulator lives in Skia's private src/ directory (not the public include/ surface), making direct wrapping unsuitable. Any implementation would require porting the private triangulation logic. The feature remains valid but unimplemented as of the last maintainer comment in June 2020.",
    "rationale": "Clearly a new feature request — no existing SkiaSharp API converts an SKPath to triangulated SKVertices. The underlying Skia class (GrTriangulator) is private/internal and not part of the stable C API surface. The maintainer explicitly investigated and confirmed this. Since the issue is from 2020 with no subsequent activity, it is open-but-unscheduled rather than abandoned — the performance rationale is sound and the request is valid.",
    "keySignals": [
      {
        "text": "On GPU backed surfaces, it is faster to use DrawVertices() instead of DrawPath(). So it would be helpful to have the native GrTriangulator to convert a path with a polygon to vertices.",
        "source": "issue body",
        "interpretation": "Performance-motivated feature request; reporter wants triangulation for GPU-optimized rendering."
      },
      {
        "text": "It would be great to use the skia version, but I see it is in the src directory... The rules for that directory is that things can change at any time.",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer identified that GrTriangulator is in Skia's private src/ directory — not a stable public API — making direct wrapping risky."
      },
      {
        "text": "Because this code is also in the private directories, we will have to port it anyway.",
        "source": "comment by mattleibow",
        "interpretation": "Any implementation would require a full port of the private triangulator logic, not a simple C API wrapper."
      },
      {
        "text": "SKVertices right now is a bit limited in the API surface, but that can change. I see they have builders and all that. We could certainly do this.",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer is open to expanding SKVertices API; the feature is feasible in principle."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKVertices.cs",
        "finding": "SKVertices.CreateCopy() exists and accepts positions, texs, colors, and indices — the destination API is present but there is no factory method that accepts an SKPath or polygon for triangulation.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "finding": "DrawVertices() has five overloads including DrawVertices(SKVertices, SKBlendMode, SKPaint) — consuming the requested output is already possible; only the path-to-vertices conversion helper is missing.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use a third-party C# triangulation library (e.g., LibTessDotNet or poly2tri-cs) to triangulate the polygon, then pass the resulting triangles to SKVertices.CreateCopy(SKVertexMode.Triangles, positions, colors) and draw with SKCanvas.DrawVertices().",
      "For simple convex polygons, implement a fan triangulation manually: fix vertex[0] as pivot and emit (vertex[0], vertex[i], vertex[i+1]) triples into SKVertices.CreateCopy()."
    ],
    "nextQuestions": [
      "Has upstream Skia promoted any path-triangulation utility to its public include/ headers since m84?",
      "Would a pure C# implementation of the ear-clipping algorithm be sufficient, or is GPU-side tessellation (via Vulkan/Metal geometry shaders) needed for the target use case?",
      "Does the reporter need fill-rule-correct triangulation (even-odd / non-zero) or only convex-polygon support?"
    ],
    "resolution": {
      "hypothesis": "The feature requires either (a) porting GrTriangulator logic from Skia's private src/ to C# or a new C shim, or (b) wrapping a third-party triangulation library. The existing SKVertices + DrawVertices API is the correct consumer surface; only the conversion helper is missing.",
      "proposals": [
        {
          "title": "Workaround: third-party triangulation → SKVertices",
          "description": "Use LibTessDotNet to triangulate the polygon path, then feed results into SKVertices.CreateCopy(). This works today without any SkiaSharp changes.",
          "category": "workaround",
          "codeSnippet": "// Add NuGet package: LibTessDotNet\n// using LibTessDotNet;\nvar tess = new Tess();\nvar contour = points.Select(p => new ContourVertex { Position = new Vec3 { X = p.X, Y = p.Y } }).ToArray();\ntess.AddContour(contour, ContourOrientation.Counterclockwise);\ntess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);\nvar positions = tess.Vertices.Select(v => new SKPoint(v.Position.X, v.Position.Y)).ToArray();\nvar indices = tess.Elements.Select(i => (ushort)i).ToArray();\nvar verts = SKVertices.CreateCopy(SKVertexMode.Triangles, positions, null, null, indices);\ncanvas.DrawVertices(verts, SKBlendMode.Modulate, paint);",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Feature: Add SKPath.Triangulate() static helper in SkiaSharp",
          "description": "Implement a C# ear-clipping or monotone-polygon triangulator as a new static SKPath extension / SKVertices factory that accepts an SKPath and returns an SKVertices. Does not require Skia native changes.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/l",
          "validated": "untested"
        },
        {
          "title": "Feature: Port GrTriangulator to a new C API shim",
          "description": "Port relevant portions of Skia's private GrTriangulator to the externals/skia/src/c/ shim layer and expose as sk_path_triangulate(). Requires maintaining a private copy of the triangulator logic across Skia milestone upgrades.",
          "category": "fix",
          "confidence": 0.5,
          "effort": "cost/xl",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Workaround: third-party triangulation → SKVertices",
      "recommendedReason": "Available today with no SkiaSharp changes needed. The proper feature implementation requires significant native work or maintaining a private fork of Skia internal code."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid performance-motivated feature request with no current implementation. The path is technically feasible but requires significant effort (porting private Skia logic or implementing a C# triangulator). A third-party workaround exists for immediate use.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, SkiaSharp core, and performance tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request, explain why GrTriangulator cannot be directly wrapped, and provide third-party workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for filing this! The core request is valid — converting a path to triangulated vertices can significantly improve GPU-backed rendering throughput.\n\nUnfortunately, Skia's `GrTriangulator` lives in the private `src/` directory rather than the stable `include/` surface, so it cannot be reliably wrapped (it can change at any time). A proper SkiaSharp implementation would require either porting that logic to C# or maintaining a private copy of the internal triangulator across Skia milestone upgrades — which is non-trivial.\n\n**Workaround available today:** Use [LibTessDotNet](https://www.nuget.org/packages/LibTessDotNet/) to triangulate your polygon, then feed the result into the existing `SKVertices.CreateCopy()` + `DrawVertices()` APIs:\n\n```csharp\n// Add NuGet package: LibTessDotNet\nvar tess = new Tess();\nvar contour = points.Select(p => new ContourVertex { Position = new Vec3 { X = p.X, Y = p.Y } }).ToArray();\ntess.AddContour(contour, ContourOrientation.Counterclockwise);\ntess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);\nvar positions = tess.Vertices.Select(v => new SKPoint(v.Position.X, v.Position.Y)).ToArray();\nvar indices = tess.Elements.Select(i => (ushort)i).ToArray();\nvar verts = SKVertices.CreateCopy(SKVertexMode.Triangles, positions, null, null, indices);\ncanvas.DrawVertices(verts, SKBlendMode.Modulate, paint);\n```\n\nKeeping this open as a feature request for a native `SKPath` → `SKVertices` triangulation helper."
      }
    ]
  }
}
```

</details>
