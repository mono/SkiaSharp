# Issue Triage Report — #3159

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:16:24Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.80 (80%)) |

**Issue Summary:** SKBitmap.ScalePixels quality regression when downscaling in v3.116.x: using SKSamplingOptions(SKCubicResampler.Mitchell) as the v3 equivalent of SKFilterQuality.High produces larger file sizes and inferior quality compared to v2.88.9, because the old API had implicit smart behavior that switched to bilinear+mipmaps for downscaling.

**Analysis:** Old SKFilterQuality.High had implicit scale-direction-aware behavior in Skia: for downscaling it used Medium quality (bilinear+linear mipmap) internally, switching to cubic only for upscaling. The v3 migration guidance maps High→Mitchell cubic unconditionally. Users who migrate to new SKSamplingOptions following this guidance get cubic resampling for downscaling, which produces worse visual quality (and larger encoded size) than the old bilinear+mipmap approach used internally for downscaling.

**Recommendations:** **close-as-not-a-bug** — The new Skia API is explicitly direction-agnostic and the behavior is by-design. The old SKFilterQuality.High had implicit smart behavior that is not preserved in the compat mapping. A confirmed workaround exists (use Linear+Linear for downscaling). The issue is better resolved by documentation and a comment with the workaround.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic, os/macOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Load a large image (1440x720) as SKBitmap
2. Create a smaller SKBitmap (480x240)
3. Call bitmapSrc.ScalePixels(bitmapDst, new SKSamplingOptions(SKCubicResampler.Mitchell))
4. Encode to WebP with quality 75
5. Compare output with v2.88.9 equivalent using SKFilterQuality.High

**Environment:** SkiaSharp 3.116.0/3.116.1, Windows 11 24H2 and macOS 15.1.1, Visual Studio

**Repository links:**
- https://ptdt.azureedge.net/data/test/large.png — Original image (1440x720)
- https://ptdt.azureedge.net/data/test/small_v2.webp — v2.88.9 output (480x240)
- https://ptdt.azureedge.net/data/test/small_v3.webp — v3.116.1 output (480x240, lower quality)

**Code snippets:**

```csharp
// v2.88.9 (old API)
bitmapSrc.ScalePixels(bitmapDst, SKFilterQuality.High);

// v3.116.1 (new API, migrated from obsolete)
SKSamplingOptions samplingOptions = new SKSamplingOptions(resampler: SKCubicResampler.Mitchell);
bitmapSrc.ScalePixels(bitmapDst, samplingOptions);
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Downsized image is larger and of inferior quality compared to v2.88.9 output |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.9, 3.116.0, 3.116.1 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | The SKSamplingOptions API and ToSamplingOptions() mapping have not changed since 3.116.0. The behavioral difference is inherent to Skia's new API design. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.85 (85%) |
| Reason | Reporter confirmed working in 2.88.9 and broken in 3.116.x. Root cause is that SKFilterQuality.High in the old Skia had smart behavior: for downscaling it internally used Medium quality (bilinear+mipmaps), not cubic. Migrating to SKSamplingOptions(Mitchell cubic) does not preserve this smart behavior. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

Old SKFilterQuality.High had implicit scale-direction-aware behavior in Skia: for downscaling it used Medium quality (bilinear+linear mipmap) internally, switching to cubic only for upscaling. The v3 migration guidance maps High→Mitchell cubic unconditionally. Users who migrate to new SKSamplingOptions following this guidance get cubic resampling for downscaling, which produces worse visual quality (and larger encoded size) than the old bilinear+mipmap approach used internally for downscaling.

### Rationale

This is a by-design behavior change in the Skia C++ library — the new API is more explicit. The bug manifests as a confusing and undocumented migration footgun: ToSamplingOptions() naively maps SKFilterQuality.High→Mitchell cubic without documenting that the old High had smart downscaling behavior. A community comment (gnattu) already identified the root cause with high confidence.

### Key Signals

- "Although SKFilterQuality.High used to imply SKCubicResampler.Mitchell, but this is only the case for up-sampling. If you specified SKFilterQuality.High and you are actually down-sampling, skia would switch back to SKFilterQuality.Medium internally." — **comment #2888746726 by gnattu** (Root cause: old Skia had smart scale-direction-aware behavior that new explicit API doesn't replicate. The compat mapping is lossy for downscaling.)
- "Resized using v2.88.9 (480X240) vs Resized using v3.116.1 (480X240) - image links show visible quality difference" — **issue body** (Regression is visually confirmed with reference images. Reported on two platforms (Windows, Mac Catalyst).)
- "After changing our code to use new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear) when downscaling, we are getting the results same as the old API." — **comment #2888746726 by gnattu** (Workaround confirmed working — use Linear+Linear mipmap for downscaling.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPaint.cs` | 28-35 | direct | ToSamplingOptions() maps SKFilterQuality.High unconditionally to new SKSamplingOptions(SKCubicResampler.Mitchell). Does not preserve old Skia's smart behavior of using Medium (Linear+LinearMipmap) for downscaling. This mapping is the root cause of the quality regression. |
| `binding/SkiaSharp/SKBitmap.cs` | 683-710 | related | Deprecated ScalePixels(bitmap, SKFilterQuality) delegates to ScalePixels(bitmap, quality.ToSamplingOptions()). The same lossy compat mapping applies here, affecting users who haven't yet migrated away from the obsolete API. |
| `binding/SkiaSharp/Definitions.cs` | 610-657 | context | SKCubicResampler.Mitchell, SKSamplingOptions(SKFilterMode, SKMipmapMode), and SKSamplingOptions(SKCubicResampler) constructors all exist and are valid. The workaround using SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear) for downscaling is API-correct. |

### Workarounds

- For downscaling: use new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear) instead of Mitchell cubic
- For upscaling: use new SKSamplingOptions(SKCubicResampler.Mitchell) as recommended
- Detect scale direction and select sampling options accordingly: check if target dimensions are smaller than source to choose downscale path

### Next Questions

- Should ToSamplingOptions() document that High does NOT preserve the smart downscale behavior?
- Should SkiaSharp add a helper/note in documentation explaining the High→new API migration nuance?

### Resolution Proposals

**Hypothesis:** Users migrating from SKFilterQuality.High to SKSamplingOptions(Mitchell cubic) get inferior downscale quality because the old API had implicit bilinear+mipmap downscale behavior. The fix is to document the migration path and use Linear+LinearMipmap for downscaling.

1. **Use direction-aware sampling options for downscaling** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Check whether the operation is downscaling (target < source) and select the appropriate sampling options. Use Mitchell cubic only for upscaling; use Linear+LinearMipmap for downscaling to match v2.88.9 behavior.

```csharp
// Determine scale direction
bool isDownscaling = bitmapDst.Width < bitmapSrc.Width || bitmapDst.Height < bitmapSrc.Height;

// Select sampling options matching v2 behavior
SKSamplingOptions samplingOptions = isDownscaling
    ? new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear)   // matches old SKFilterQuality.High for downscale
    : new SKSamplingOptions(SKCubicResampler.Mitchell);                  // matches old SKFilterQuality.High for upscale

bitmapSrc.ScalePixels(bitmapDst, samplingOptions);
```
2. **Document the migration nuance in ToSamplingOptions() and obsolete comments** — fix, confidence 0.90 (90%), cost/xs, validated=untested
   - Update the XML doc comments on ToSamplingOptions() and ScalePixels obsolete overloads to note that High quality had smart direction-aware behavior and recommend the user inspect scale direction to pick the right options.

**Recommended proposal:** Use direction-aware sampling options for downscaling

**Why:** Provides an immediate, API-correct workaround requiring minimal code change. Community has verified it produces equivalent v2 results.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.80 (80%) |
| Reason | The new Skia API is explicitly direction-agnostic and the behavior is by-design. The old SKFilterQuality.High had implicit smart behavior that is not preserved in the compat mapping. A confirmed workaround exists (use Linear+Linear for downscaling). The issue is better resolved by documentation and a comment with the workaround. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Add area/SkiaSharp, os/Windows-Classic, os/macOS, tenet/compatibility labels (type/bug already applied) | labels=type/bug, area/SkiaSharp, os/Windows-Classic, os/macOS, tenet/compatibility |
| add-comment | medium | 0.90 (90%) | Post explanation of root cause and confirmed workaround for direction-aware sampling | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report, and thanks to @gnattu for identifying the root cause!

This is a subtle behavioral difference between the old `SKFilterQuality` API and the new explicit `SKSamplingOptions` API in Skia.

**Root cause:** In the old Skia (≤2.88.x), `SKFilterQuality.High` had smart scale-direction awareness:
- **Downscaling**: internally used `SKFilterQuality.Medium` (bilinear + linear mipmaps)
- **Upscaling**: used cubic resampling (Mitchell)

The new API is explicit, and the compat mapping `High → SKCubicResampler.Mitchell` is unconditional — it does not preserve this smart downscale behavior.

**Workaround** (matches v2.88.x quality for both up and down scaling):

```csharp
bool isDownscaling = bitmapDst.Width < bitmapSrc.Width || bitmapDst.Height < bitmapSrc.Height;

SKSamplingOptions samplingOptions = isDownscaling
    ? new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear)  // matches old High for downscale
    : new SKSamplingOptions(SKCubicResampler.Mitchell);                 // matches old High for upscale

bitmapSrc.ScalePixels(bitmapDst, samplingOptions);
```

We will look at improving the XML documentation on `ToSamplingOptions()` to make this migration nuance clearer.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3159,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:16:24Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKBitmap.ScalePixels quality regression when downscaling in v3.116.x: using SKSamplingOptions(SKCubicResampler.Mitchell) as the v3 equivalent of SKFilterQuality.High produces larger file sizes and inferior quality compared to v2.88.9, because the old API had implicit smart behavior that switched to bilinear+mipmaps for downscaling.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic",
      "os/macOS"
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
      "errorMessage": "Downsized image is larger and of inferior quality compared to v2.88.9 output",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Load a large image (1440x720) as SKBitmap",
        "Create a smaller SKBitmap (480x240)",
        "Call bitmapSrc.ScalePixels(bitmapDst, new SKSamplingOptions(SKCubicResampler.Mitchell))",
        "Encode to WebP with quality 75",
        "Compare output with v2.88.9 equivalent using SKFilterQuality.High"
      ],
      "environmentDetails": "SkiaSharp 3.116.0/3.116.1, Windows 11 24H2 and macOS 15.1.1, Visual Studio",
      "repoLinks": [
        {
          "url": "https://ptdt.azureedge.net/data/test/large.png",
          "description": "Original image (1440x720)"
        },
        {
          "url": "https://ptdt.azureedge.net/data/test/small_v2.webp",
          "description": "v2.88.9 output (480x240)"
        },
        {
          "url": "https://ptdt.azureedge.net/data/test/small_v3.webp",
          "description": "v3.116.1 output (480x240, lower quality)"
        }
      ],
      "codeSnippets": [
        "// v2.88.9 (old API)\nbitmapSrc.ScalePixels(bitmapDst, SKFilterQuality.High);\n\n// v3.116.1 (new API, migrated from obsolete)\nSKSamplingOptions samplingOptions = new SKSamplingOptions(resampler: SKCubicResampler.Mitchell);\nbitmapSrc.ScalePixels(bitmapDst, samplingOptions);"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.9",
        "3.116.0",
        "3.116.1"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "The SKSamplingOptions API and ToSamplingOptions() mapping have not changed since 3.116.0. The behavioral difference is inherent to Skia's new API design."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.85,
      "reason": "Reporter confirmed working in 2.88.9 and broken in 3.116.x. Root cause is that SKFilterQuality.High in the old Skia had smart behavior: for downscaling it internally used Medium quality (bilinear+mipmaps), not cubic. Migrating to SKSamplingOptions(Mitchell cubic) does not preserve this smart behavior.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "Old SKFilterQuality.High had implicit scale-direction-aware behavior in Skia: for downscaling it used Medium quality (bilinear+linear mipmap) internally, switching to cubic only for upscaling. The v3 migration guidance maps High→Mitchell cubic unconditionally. Users who migrate to new SKSamplingOptions following this guidance get cubic resampling for downscaling, which produces worse visual quality (and larger encoded size) than the old bilinear+mipmap approach used internally for downscaling.",
    "rationale": "This is a by-design behavior change in the Skia C++ library — the new API is more explicit. The bug manifests as a confusing and undocumented migration footgun: ToSamplingOptions() naively maps SKFilterQuality.High→Mitchell cubic without documenting that the old High had smart downscaling behavior. A community comment (gnattu) already identified the root cause with high confidence.",
    "keySignals": [
      {
        "text": "Although SKFilterQuality.High used to imply SKCubicResampler.Mitchell, but this is only the case for up-sampling. If you specified SKFilterQuality.High and you are actually down-sampling, skia would switch back to SKFilterQuality.Medium internally.",
        "source": "comment #2888746726 by gnattu",
        "interpretation": "Root cause: old Skia had smart scale-direction-aware behavior that new explicit API doesn't replicate. The compat mapping is lossy for downscaling."
      },
      {
        "text": "Resized using v2.88.9 (480X240) vs Resized using v3.116.1 (480X240) - image links show visible quality difference",
        "source": "issue body",
        "interpretation": "Regression is visually confirmed with reference images. Reported on two platforms (Windows, Mac Catalyst)."
      },
      {
        "text": "After changing our code to use new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear) when downscaling, we are getting the results same as the old API.",
        "source": "comment #2888746726 by gnattu",
        "interpretation": "Workaround confirmed working — use Linear+Linear mipmap for downscaling."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "28-35",
        "finding": "ToSamplingOptions() maps SKFilterQuality.High unconditionally to new SKSamplingOptions(SKCubicResampler.Mitchell). Does not preserve old Skia's smart behavior of using Medium (Linear+LinearMipmap) for downscaling. This mapping is the root cause of the quality regression.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "683-710",
        "finding": "Deprecated ScalePixels(bitmap, SKFilterQuality) delegates to ScalePixels(bitmap, quality.ToSamplingOptions()). The same lossy compat mapping applies here, affecting users who haven't yet migrated away from the obsolete API.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "610-657",
        "finding": "SKCubicResampler.Mitchell, SKSamplingOptions(SKFilterMode, SKMipmapMode), and SKSamplingOptions(SKCubicResampler) constructors all exist and are valid. The workaround using SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear) for downscaling is API-correct.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "For downscaling: use new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear) instead of Mitchell cubic",
      "For upscaling: use new SKSamplingOptions(SKCubicResampler.Mitchell) as recommended",
      "Detect scale direction and select sampling options accordingly: check if target dimensions are smaller than source to choose downscale path"
    ],
    "nextQuestions": [
      "Should ToSamplingOptions() document that High does NOT preserve the smart downscale behavior?",
      "Should SkiaSharp add a helper/note in documentation explaining the High→new API migration nuance?"
    ],
    "resolution": {
      "hypothesis": "Users migrating from SKFilterQuality.High to SKSamplingOptions(Mitchell cubic) get inferior downscale quality because the old API had implicit bilinear+mipmap downscale behavior. The fix is to document the migration path and use Linear+LinearMipmap for downscaling.",
      "proposals": [
        {
          "title": "Use direction-aware sampling options for downscaling",
          "description": "Check whether the operation is downscaling (target < source) and select the appropriate sampling options. Use Mitchell cubic only for upscaling; use Linear+LinearMipmap for downscaling to match v2.88.9 behavior.",
          "category": "workaround",
          "codeSnippet": "// Determine scale direction\nbool isDownscaling = bitmapDst.Width < bitmapSrc.Width || bitmapDst.Height < bitmapSrc.Height;\n\n// Select sampling options matching v2 behavior\nSKSamplingOptions samplingOptions = isDownscaling\n    ? new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear)   // matches old SKFilterQuality.High for downscale\n    : new SKSamplingOptions(SKCubicResampler.Mitchell);                  // matches old SKFilterQuality.High for upscale\n\nbitmapSrc.ScalePixels(bitmapDst, samplingOptions);",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Document the migration nuance in ToSamplingOptions() and obsolete comments",
          "description": "Update the XML doc comments on ToSamplingOptions() and ScalePixels obsolete overloads to note that High quality had smart direction-aware behavior and recommend the user inspect scale direction to pick the right options.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use direction-aware sampling options for downscaling",
      "recommendedReason": "Provides an immediate, API-correct workaround requiring minimal code change. Community has verified it produces equivalent v2 results."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.8,
      "reason": "The new Skia API is explicitly direction-agnostic and the behavior is by-design. The old SKFilterQuality.High had implicit smart behavior that is not preserved in the compat mapping. A confirmed workaround exists (use Linear+Linear for downscaling). The issue is better resolved by documentation and a comment with the workaround.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Add area/SkiaSharp, os/Windows-Classic, os/macOS, tenet/compatibility labels (type/bug already applied)",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "os/macOS",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post explanation of root cause and confirmed workaround for direction-aware sampling",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed report, and thanks to @gnattu for identifying the root cause!\n\nThis is a subtle behavioral difference between the old `SKFilterQuality` API and the new explicit `SKSamplingOptions` API in Skia.\n\n**Root cause:** In the old Skia (≤2.88.x), `SKFilterQuality.High` had smart scale-direction awareness:\n- **Downscaling**: internally used `SKFilterQuality.Medium` (bilinear + linear mipmaps)\n- **Upscaling**: used cubic resampling (Mitchell)\n\nThe new API is explicit, and the compat mapping `High → SKCubicResampler.Mitchell` is unconditional — it does not preserve this smart downscale behavior.\n\n**Workaround** (matches v2.88.x quality for both up and down scaling):\n\n```csharp\nbool isDownscaling = bitmapDst.Width < bitmapSrc.Width || bitmapDst.Height < bitmapSrc.Height;\n\nSKSamplingOptions samplingOptions = isDownscaling\n    ? new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear)  // matches old High for downscale\n    : new SKSamplingOptions(SKCubicResampler.Mitchell);                 // matches old High for upscale\n\nbitmapSrc.ScalePixels(bitmapDst, samplingOptions);\n```\n\nWe will look at improving the XML documentation on `ToSamplingOptions()` to make this migration nuance clearer."
      }
    ]
  }
}
```

</details>
