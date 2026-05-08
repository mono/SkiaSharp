# Issue Triage Report — #1913

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T22:30:00Z |
| Type | type/question (0.90 (90%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** Reporter asks why SKSurface.Create(grContext, renderTarget, surfaceOrigin, colorType) returns null when using an OpenGL backend with what appears to be valid GRContext and GRBackendRenderTarget objects.

**Analysis:** The most likely cause is that the GRGlFramebufferInfo was constructed without specifying the GL sized format (fFormat=0). Skia requires a non-zero GL sized format in the framebuffer info to match the render target with the requested color type. Using GRGlFramebufferInfo(fboId, colorType.ToGlSizedFormat()) fixes this.

**Recommendations:** **close-as-not-a-bug** — This is a usage question with a clear answer: the GRGlFramebufferInfo must include the GL sized format. The API works correctly when used as intended.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create a GRContext from an OpenGL interface
2. Create a GRBackendRenderTarget using GRGlFramebufferInfo
3. Call SKSurface.Create(grContext, renderTarget, surfaceOrigin, colorType)
4. Observe null return value

**Environment:** Windows, OpenGL 4.6.0 NVIDIA 462.96, colorType=Bgra8888, surfaceOrigin=BottomLeft, renderTarget Width=1280 Height=720 SampleCount=1 StencilBits=8 Backend=OpenGL IsValid=true

**Code snippets:**

```csharp
surface = SKSurface.Create(grContext, renderTarget, surfaceOrigin, colorType);
```

## Analysis

### Technical Summary

The most likely cause is that the GRGlFramebufferInfo was constructed without specifying the GL sized format (fFormat=0). Skia requires a non-zero GL sized format in the framebuffer info to match the render target with the requested color type. Using GRGlFramebufferInfo(fboId, colorType.ToGlSizedFormat()) fixes this.

### Rationale

Issue is classified as type/question because the reporter is asking why an API call returns null, not reporting a crash or regression. Code investigation confirms that GRGlFramebufferInfo has a constructor that defaults fFormat to 0, and ToGlSizedFormat() is the correct extension to obtain the format value for a given SKColorType. A null return from sk_surface_new_backend_render_target is the expected Skia behavior when the format is unspecified.

### Key Signals

- "I'm trying to understand why surface creation is failing." — **issue body** (Usage question — reporter does not know they need to specify the GL sized format.)
- "sk_surface_new_backend_render_target is returning null" — **issue comment** (Confirms null comes from the C API call. This is the expected Skia behavior when GRGlFramebufferInfo.fFormat is 0.)
- "colorType: Bgra8888, renderTarget IsValid: true, grContext IsAbandoned: false" — **issue body** (Objects are valid; the problem is the framebuffer info format, not the context or render target object validity.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/GRDefinitions.cs` | 37-53 | direct | GRGlFramebufferInfo(uint fboId) sets fFormat=0. The two-argument constructor GRGlFramebufferInfo(fboId, format) is required to pass a valid GL sized format. A format of 0 causes sk_surface_new_backend_render_target to return null. |
| `binding/SkiaSharp/GRDefinitions.cs` | 139-168 | direct | ToGlSizedFormat(SKColorType) extension method maps each SKColorType to its GL sized format constant. For Bgra8888 it returns GRGlSizedFormat.BGRA8 (0x93A1). This is the correct format to pass when using colorType=Bgra8888. |
| `binding/SkiaSharp/SKSurface.cs` | 111-119 | related | SKSurface.Create(GRRecordingContext, GRBackendRenderTarget, origin, colorType, colorspace, props) passes context.Handle and renderTarget.Handle directly to sk_surface_new_backend_render_target with no null check of return value before GetObject wraps it. Returns null if native call returns null. |

### Workarounds

- Specify GL sized format in GRGlFramebufferInfo: new GRGlFramebufferInfo(fboId, colorType.ToGlSizedFormat()). For Bgra8888 this is GRGlSizedFormat.BGRA8 (0x93A1).
- Alternatively pass the format value directly: new GRGlFramebufferInfo(fboId, 0x93A1) for BGRA8.

### Resolution Proposals

**Hypothesis:** The GRGlFramebufferInfo was constructed using the single-argument constructor which defaults fFormat to 0. Skia requires a valid GL sized format in the framebuffer info; a zero value causes surface creation to fail and return null.

1. **Pass GL sized format in GRGlFramebufferInfo** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Use the two-argument GRGlFramebufferInfo constructor, passing the GL sized format obtained from ToGlSizedFormat(). This matches the framebuffer format to the requested color type so Skia can create the surface.

```csharp
var glInfo = new GRGlFramebufferInfo(fboId, colorType.ToGlSizedFormat());
var renderTarget = new GRBackendRenderTarget(width, height, sampleCount, stencilBits, glInfo);
var surface = SKSurface.Create(grContext, renderTarget, surfaceOrigin, colorType);
```

**Recommended proposal:** Pass GL sized format in GRGlFramebufferInfo

**Why:** Directly addresses the root cause. The ToGlSizedFormat() extension already provides the correct mapping.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | This is a usage question with a clear answer: the GRGlFramebufferInfo must include the GL sized format. The API works correctly when used as intended. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question, area/SkiaSharp, Windows, OpenGL labels | labels=type/question, area/SkiaSharp, os/Windows-Classic, backend/OpenGL |
| add-comment | medium | 0.90 (90%) | Post answer explaining the missing GL sized format in GRGlFramebufferInfo | — |
| close-issue | medium | 0.80 (80%) | Close as answered — usage question with known workaround | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
The most common cause of `SKSurface.Create` returning `null` with a `GRBackendRenderTarget` is a missing GL sized format in `GRGlFramebufferInfo`.

When you construct `GRGlFramebufferInfo` with only the framebuffer object ID (the single-argument constructor), the `fFormat` field is left as `0`. Skia requires a valid GL sized format to match the render target with the requested color type, and returns `null` when it's absent.

The fix is to pass the GL sized format explicitly:

```csharp
// Use the two-argument constructor, with colorType.ToGlSizedFormat()
var glInfo = new GRGlFramebufferInfo(fboId, colorType.ToGlSizedFormat());
var renderTarget = new GRBackendRenderTarget(width, height, sampleCount, stencilBits, glInfo);
var surface = SKSurface.Create(grContext, renderTarget, surfaceOrigin, colorType);
```

For `SKColorType.Bgra8888` (which you're using), `ToGlSizedFormat()` returns `0x93A1` (GL `BGRA8`). Make sure `fboId` is the actual OpenGL framebuffer object name (from `GL.GenFramebuffer()`) bound to your render target.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1913,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T22:30:00Z"
  },
  "summary": "Reporter asks why SKSurface.Create(grContext, renderTarget, surfaceOrigin, colorType) returns null when using an OpenGL backend with what appears to be valid GRContext and GRBackendRenderTarget objects.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/OpenGL"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "surface = SKSurface.Create(grContext, renderTarget, surfaceOrigin, colorType);"
      ],
      "environmentDetails": "Windows, OpenGL 4.6.0 NVIDIA 462.96, colorType=Bgra8888, surfaceOrigin=BottomLeft, renderTarget Width=1280 Height=720 SampleCount=1 StencilBits=8 Backend=OpenGL IsValid=true",
      "stepsToReproduce": [
        "Create a GRContext from an OpenGL interface",
        "Create a GRBackendRenderTarget using GRGlFramebufferInfo",
        "Call SKSurface.Create(grContext, renderTarget, surfaceOrigin, colorType)",
        "Observe null return value"
      ]
    }
  },
  "analysis": {
    "summary": "The most likely cause is that the GRGlFramebufferInfo was constructed without specifying the GL sized format (fFormat=0). Skia requires a non-zero GL sized format in the framebuffer info to match the render target with the requested color type. Using GRGlFramebufferInfo(fboId, colorType.ToGlSizedFormat()) fixes this.",
    "rationale": "Issue is classified as type/question because the reporter is asking why an API call returns null, not reporting a crash or regression. Code investigation confirms that GRGlFramebufferInfo has a constructor that defaults fFormat to 0, and ToGlSizedFormat() is the correct extension to obtain the format value for a given SKColorType. A null return from sk_surface_new_backend_render_target is the expected Skia behavior when the format is unspecified.",
    "keySignals": [
      {
        "text": "I'm trying to understand why surface creation is failing.",
        "source": "issue body",
        "interpretation": "Usage question — reporter does not know they need to specify the GL sized format."
      },
      {
        "text": "sk_surface_new_backend_render_target is returning null",
        "source": "issue comment",
        "interpretation": "Confirms null comes from the C API call. This is the expected Skia behavior when GRGlFramebufferInfo.fFormat is 0."
      },
      {
        "text": "colorType: Bgra8888, renderTarget IsValid: true, grContext IsAbandoned: false",
        "source": "issue body",
        "interpretation": "Objects are valid; the problem is the framebuffer info format, not the context or render target object validity."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/GRDefinitions.cs",
        "lines": "37-53",
        "finding": "GRGlFramebufferInfo(uint fboId) sets fFormat=0. The two-argument constructor GRGlFramebufferInfo(fboId, format) is required to pass a valid GL sized format. A format of 0 causes sk_surface_new_backend_render_target to return null.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRDefinitions.cs",
        "lines": "139-168",
        "finding": "ToGlSizedFormat(SKColorType) extension method maps each SKColorType to its GL sized format constant. For Bgra8888 it returns GRGlSizedFormat.BGRA8 (0x93A1). This is the correct format to pass when using colorType=Bgra8888.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "lines": "111-119",
        "finding": "SKSurface.Create(GRRecordingContext, GRBackendRenderTarget, origin, colorType, colorspace, props) passes context.Handle and renderTarget.Handle directly to sk_surface_new_backend_render_target with no null check of return value before GetObject wraps it. Returns null if native call returns null.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Specify GL sized format in GRGlFramebufferInfo: new GRGlFramebufferInfo(fboId, colorType.ToGlSizedFormat()). For Bgra8888 this is GRGlSizedFormat.BGRA8 (0x93A1).",
      "Alternatively pass the format value directly: new GRGlFramebufferInfo(fboId, 0x93A1) for BGRA8."
    ],
    "resolution": {
      "hypothesis": "The GRGlFramebufferInfo was constructed using the single-argument constructor which defaults fFormat to 0. Skia requires a valid GL sized format in the framebuffer info; a zero value causes surface creation to fail and return null.",
      "proposals": [
        {
          "title": "Pass GL sized format in GRGlFramebufferInfo",
          "description": "Use the two-argument GRGlFramebufferInfo constructor, passing the GL sized format obtained from ToGlSizedFormat(). This matches the framebuffer format to the requested color type so Skia can create the surface.",
          "category": "workaround",
          "codeSnippet": "var glInfo = new GRGlFramebufferInfo(fboId, colorType.ToGlSizedFormat());\nvar renderTarget = new GRBackendRenderTarget(width, height, sampleCount, stencilBits, glInfo);\nvar surface = SKSurface.Create(grContext, renderTarget, surfaceOrigin, colorType);",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Pass GL sized format in GRGlFramebufferInfo",
      "recommendedReason": "Directly addresses the root cause. The ToGlSizedFormat() extension already provides the correct mapping."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "This is a usage question with a clear answer: the GRGlFramebufferInfo must include the GL sized format. The API works correctly when used as intended.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, area/SkiaSharp, Windows, OpenGL labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer explaining the missing GL sized format in GRGlFramebufferInfo",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "The most common cause of `SKSurface.Create` returning `null` with a `GRBackendRenderTarget` is a missing GL sized format in `GRGlFramebufferInfo`.\n\nWhen you construct `GRGlFramebufferInfo` with only the framebuffer object ID (the single-argument constructor), the `fFormat` field is left as `0`. Skia requires a valid GL sized format to match the render target with the requested color type, and returns `null` when it's absent.\n\nThe fix is to pass the GL sized format explicitly:\n\n```csharp\n// Use the two-argument constructor, with colorType.ToGlSizedFormat()\nvar glInfo = new GRGlFramebufferInfo(fboId, colorType.ToGlSizedFormat());\nvar renderTarget = new GRBackendRenderTarget(width, height, sampleCount, stencilBits, glInfo);\nvar surface = SKSurface.Create(grContext, renderTarget, surfaceOrigin, colorType);\n```\n\nFor `SKColorType.Bgra8888` (which you're using), `ToGlSizedFormat()` returns `0x93A1` (GL `BGRA8`). Make sure `fboId` is the actual OpenGL framebuffer object name (from `GL.GenFramebuffer()`) bound to your render target."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — usage question with known workaround",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
