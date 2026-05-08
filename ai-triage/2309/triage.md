# Issue Triage Report — #2309

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T17:09:12Z |
| Type | type/bug (0.93 (93%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** After upgrading from SkiaSharp v2.80.4 to v2.88.x, SKImage.Subset(SKRectI) returns null for GPU texture-backed images because the overload now calls sk_image_make_subset_raster (raster-only), while a separate Subset(GRRecordingContext, SKRectI) overload is required for texture images.

**Analysis:** The parameterless SKImage.Subset(SKRectI) no longer works for GPU texture-backed images after v2.88.x because it was changed to call sk_image_make_subset_raster. A new overload Subset(GRRecordingContext, SKRectI) is available that properly handles texture images, but the old single-parameter API silently returns null instead of failing gracefully or routing to the correct implementation.

**Recommendations:** **needs-investigation** — The immediate workaround (use the context overload) is clear, but it should be determined whether the parameterless Subset should throw an informative exception instead of returning null for GPU images, and whether a migration note is needed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create an OpenGL context using OpenTK
2. Create a GRContext with GRContext.CreateGl()
3. Create a GPU texture and wrap it with SKImage.FromAdoptedTexture()
4. Call textureImage.Subset(new SKRectI(0, 0, 16, 16))
5. Observe that the result is null

**Environment:** Windows 11, .NET 6, JetBrains Rider, SkiaSharp v2.88.x, OpenTK

**Code snippets:**

```csharp
var subsetTextureImage = textureImage.Subset(new SKRectI(0, 0, 16, 16)); // returns null
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Subset method returns null for texture-backed SKImage |
| Repro quality | complete |
| Target frameworks | net6.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.4, 2.88.x |
| Worked in | 2.80.4 |
| Broke in | 2.88.x |
| Current relevance | likely |
| Relevance reason | The Subset(SKRectI) overload still calls sk_image_make_subset_raster which cannot produce a subset of a texture-backed image. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | In v2.80.4 the single Subset(SKRectI) overload called sk_image_make_subset which worked for both raster and texture images. In v2.88.x the Skia C++ API split into makeSubset (requires GrDirectContext for GPU images) and makeSubsetRaster, and SkiaSharp's parameterless Subset now calls only the raster variant. |
| Worked in version | 2.80.4 |
| Broke in version | 2.88.x |

## Analysis

### Technical Summary

The parameterless SKImage.Subset(SKRectI) no longer works for GPU texture-backed images after v2.88.x because it was changed to call sk_image_make_subset_raster. A new overload Subset(GRRecordingContext, SKRectI) is available that properly handles texture images, but the old single-parameter API silently returns null instead of failing gracefully or routing to the correct implementation.

### Rationale

Clear regression: the reporter's code worked in v2.80.4 but returns null in v2.88.x with identical logic. Code investigation confirms that the parameterless Subset calls sk_image_make_subset_raster which cannot produce GPU texture subsets. The new Subset(GRRecordingContext, SKRectI) overload is the correct API but was not communicated, causing a silent null return instead of a helpful error.

### Key Signals

- "Subset method returns null" — **issue body — Actual Behavior section** (Calling Subset(SKRectI) on a GPU texture image returns null because sk_image_make_subset_raster cannot subset texture images.)
- "Last known good version: v2.80.4 — Version with issue: v2.88.x" — **issue body — Basic Information** (Confirmed regression between these two versions corresponding to the Skia API split.)
- "// TODO need to pass skiaContext!" — **issue body — code comment** (Reporter is aware that the context is needed but cannot find the API to pass it.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImage.cs` | 544-552 | direct | Subset(SKRectI) calls sk_image_make_subset_raster (raster-only). The overload Subset(GRRecordingContext, SKRectI) calls sk_image_make_subset with context and is the correct API for texture images. The parameterless overload does NOT fall back to the context-aware version. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 7095-7130 | related | Both sk_image_make_subset(cimage, context, subset) and sk_image_make_subset_raster(cimage, subset) are defined. The split reflects the upstream Skia API change where makeSubset now requires GrDirectContext for GPU images. |

### Workarounds

- Use the Subset(GRRecordingContext context, SKRectI subset) overload, passing the same GRContext used to create the texture image: textureImage.Subset(skiaContext, new SKRectI(0, 0, 16, 16))
- Alternatively: convert to raster first with ToRasterImage(), then call Subset(SKRectI), then convert back to texture — expensive but works without context.

### Next Questions

- Should Subset(SKRectI) throw an InvalidOperationException when called on a texture-backed image without a context, rather than silently returning null?
- Was this API split communicated in the v2.88.x release notes or migration guide?

### Resolution Proposals

**Hypothesis:** The reporter needs to use Subset(GRRecordingContext, SKRectI) instead of Subset(SKRectI). Optionally, the parameterless Subset could be improved to throw a descriptive exception when called on a GPU image.

1. **Use the context overload** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Call textureImage.Subset(skiaContext, new SKRectI(0, 0, 16, 16)) — the GRRecordingContext overload correctly routes to sk_image_make_subset and works for GPU texture images.

```csharp
var subsetTextureImage = textureImage.Subset(skiaContext, new SKRectI(0, 0, 16, 16));
```
2. **Improve error message for parameterless Subset on GPU images** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Add a guard in Subset(SKRectI) that checks IsTextureBacked and throws InvalidOperationException with a helpful message directing the user to the context overload.

**Recommended proposal:** Use the context overload

**Why:** The workaround is a one-line change to existing code and is immediately actionable. The API exists and works correctly.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | The immediate workaround (use the context overload) is clear, but it should be determined whether the parameterless Subset should throw an informative exception instead of returning null for GPU images, and whether a migration note is needed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.93 (93%) | Apply bug, SkiaSharp core, Windows, OpenGL, and compatibility tenet labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/OpenGL, tenet/compatibility |
| add-comment | medium | 0.90 (90%) | Explain the API change and workaround using the context overload | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro!

This is a known breaking change in SkiaSharp v2.88.x that mirrors an upstream Skia API split. In v2.80.x `Subset(SKRectI)` internally called `sk_image_make_subset` which worked for both raster and GPU texture images. In v2.88.x, Skia split this into two separate functions:

- `sk_image_make_subset_raster` — raster images only (called by `Subset(SKRectI)`)
- `sk_image_make_subset` with GrDirectContext — GPU texture images (called by `Subset(GRRecordingContext, SKRectI)`)

The fix is to use the new overload and pass the same context you used to create the image:

```csharp
// Before (v2.80.4 — no longer works for texture images)
var subsetImage = textureImage.Subset(new SKRectI(0, 0, 16, 16));

// After (v2.88.x — use the context overload for texture-backed images)
var subsetImage = textureImage.Subset(skiaContext, new SKRectI(0, 0, 16, 16));
```

This matches the [Skia documentation](https://api.skia.org/classSkImage.html#a02b229cde4f7f2b80e810157e2d98b9b) you linked: the context parameter is required for texture-backed images.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2309,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T17:09:12Z"
  },
  "summary": "After upgrading from SkiaSharp v2.80.4 to v2.88.x, SKImage.Subset(SKRectI) returns null for GPU texture-backed images because the overload now calls sk_image_make_subset_raster (raster-only), while a separate Subset(GRRecordingContext, SKRectI) overload is required for texture images.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.93
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "Subset method returns null for texture-backed SKImage",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an OpenGL context using OpenTK",
        "Create a GRContext with GRContext.CreateGl()",
        "Create a GPU texture and wrap it with SKImage.FromAdoptedTexture()",
        "Call textureImage.Subset(new SKRectI(0, 0, 16, 16))",
        "Observe that the result is null"
      ],
      "codeSnippets": [
        "var subsetTextureImage = textureImage.Subset(new SKRectI(0, 0, 16, 16)); // returns null"
      ],
      "environmentDetails": "Windows 11, .NET 6, JetBrains Rider, SkiaSharp v2.88.x, OpenTK",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.4",
        "2.88.x"
      ],
      "workedIn": "2.80.4",
      "brokeIn": "2.88.x",
      "currentRelevance": "likely",
      "relevanceReason": "The Subset(SKRectI) overload still calls sk_image_make_subset_raster which cannot produce a subset of a texture-backed image."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "In v2.80.4 the single Subset(SKRectI) overload called sk_image_make_subset which worked for both raster and texture images. In v2.88.x the Skia C++ API split into makeSubset (requires GrDirectContext for GPU images) and makeSubsetRaster, and SkiaSharp's parameterless Subset now calls only the raster variant.",
      "workedInVersion": "2.80.4",
      "brokeInVersion": "2.88.x"
    }
  },
  "analysis": {
    "summary": "The parameterless SKImage.Subset(SKRectI) no longer works for GPU texture-backed images after v2.88.x because it was changed to call sk_image_make_subset_raster. A new overload Subset(GRRecordingContext, SKRectI) is available that properly handles texture images, but the old single-parameter API silently returns null instead of failing gracefully or routing to the correct implementation.",
    "rationale": "Clear regression: the reporter's code worked in v2.80.4 but returns null in v2.88.x with identical logic. Code investigation confirms that the parameterless Subset calls sk_image_make_subset_raster which cannot produce GPU texture subsets. The new Subset(GRRecordingContext, SKRectI) overload is the correct API but was not communicated, causing a silent null return instead of a helpful error.",
    "keySignals": [
      {
        "text": "Subset method returns null",
        "source": "issue body — Actual Behavior section",
        "interpretation": "Calling Subset(SKRectI) on a GPU texture image returns null because sk_image_make_subset_raster cannot subset texture images."
      },
      {
        "text": "Last known good version: v2.80.4 — Version with issue: v2.88.x",
        "source": "issue body — Basic Information",
        "interpretation": "Confirmed regression between these two versions corresponding to the Skia API split."
      },
      {
        "text": "// TODO need to pass skiaContext!",
        "source": "issue body — code comment",
        "interpretation": "Reporter is aware that the context is needed but cannot find the API to pass it."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "544-552",
        "finding": "Subset(SKRectI) calls sk_image_make_subset_raster (raster-only). The overload Subset(GRRecordingContext, SKRectI) calls sk_image_make_subset with context and is the correct API for texture images. The parameterless overload does NOT fall back to the context-aware version.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "7095-7130",
        "finding": "Both sk_image_make_subset(cimage, context, subset) and sk_image_make_subset_raster(cimage, subset) are defined. The split reflects the upstream Skia API change where makeSubset now requires GrDirectContext for GPU images.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use the Subset(GRRecordingContext context, SKRectI subset) overload, passing the same GRContext used to create the texture image: textureImage.Subset(skiaContext, new SKRectI(0, 0, 16, 16))",
      "Alternatively: convert to raster first with ToRasterImage(), then call Subset(SKRectI), then convert back to texture — expensive but works without context."
    ],
    "nextQuestions": [
      "Should Subset(SKRectI) throw an InvalidOperationException when called on a texture-backed image without a context, rather than silently returning null?",
      "Was this API split communicated in the v2.88.x release notes or migration guide?"
    ],
    "resolution": {
      "hypothesis": "The reporter needs to use Subset(GRRecordingContext, SKRectI) instead of Subset(SKRectI). Optionally, the parameterless Subset could be improved to throw a descriptive exception when called on a GPU image.",
      "proposals": [
        {
          "title": "Use the context overload",
          "description": "Call textureImage.Subset(skiaContext, new SKRectI(0, 0, 16, 16)) — the GRRecordingContext overload correctly routes to sk_image_make_subset and works for GPU texture images.",
          "category": "workaround",
          "codeSnippet": "var subsetTextureImage = textureImage.Subset(skiaContext, new SKRectI(0, 0, 16, 16));",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Improve error message for parameterless Subset on GPU images",
          "description": "Add a guard in Subset(SKRectI) that checks IsTextureBacked and throws InvalidOperationException with a helpful message directing the user to the context overload.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use the context overload",
      "recommendedReason": "The workaround is a one-line change to existing code and is immediately actionable. The API exists and works correctly."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "The immediate workaround (use the context overload) is clear, but it should be determined whether the parameterless Subset should throw an informative exception instead of returning null for GPU images, and whether a migration note is needed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp core, Windows, OpenGL, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.93,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/OpenGL",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain the API change and workaround using the context overload",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed repro!\n\nThis is a known breaking change in SkiaSharp v2.88.x that mirrors an upstream Skia API split. In v2.80.x `Subset(SKRectI)` internally called `sk_image_make_subset` which worked for both raster and GPU texture images. In v2.88.x, Skia split this into two separate functions:\n\n- `sk_image_make_subset_raster` — raster images only (called by `Subset(SKRectI)`)\n- `sk_image_make_subset` with GrDirectContext — GPU texture images (called by `Subset(GRRecordingContext, SKRectI)`)\n\nThe fix is to use the new overload and pass the same context you used to create the image:\n\n```csharp\n// Before (v2.80.4 — no longer works for texture images)\nvar subsetImage = textureImage.Subset(new SKRectI(0, 0, 16, 16));\n\n// After (v2.88.x — use the context overload for texture-backed images)\nvar subsetImage = textureImage.Subset(skiaContext, new SKRectI(0, 0, 16, 16));\n```\n\nThis matches the [Skia documentation](https://api.skia.org/classSkImage.html#a02b229cde4f7f2b80e810157e2d98b9b) you linked: the context parameter is required for texture-backed images."
      }
    ]
  }
}
```

</details>
