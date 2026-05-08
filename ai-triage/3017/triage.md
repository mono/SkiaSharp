# Issue Triage Report — #3017

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T02:02:41Z |
| Type | type/bug (0.80 (80%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** Reporter claims SKBlendMode.Multiply produces wrong alpha when composing a transparent color over an opaque color, expecting all RGBA channels to be independently multiplied to zero, but this misunderstands the W3C compositing spec for Multiply blend mode.

**Analysis:** SKBlendMode.Multiply follows the W3C compositing specification which defines Multiply as Cr = Cs*Cd + Cs*(1-αd) + Cd*(1-αs), with alpha compositing αr = αs + αd - αs*αd. With src=(0,0,0,1) opaque black and dst=(0,0,0,0) transparent black: result alpha = 1+0-1*0 = 1 (fully opaque) and RGB = 0. The observed output RGBA(0,0,0,255) is mathematically correct. The reporter expects simple per-channel multiplication but that is not how Porter-Duff compositing modes work.

**Recommendations:** **close-as-not-a-bug** — SKBlendMode.Multiply correctly implements the W3C compositing specification. The result RGBA(0,0,0,255) is mathematically correct. The issue is a misunderstanding of how Porter-Duff blend modes handle alpha compositing.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a shader using SKShader.CreateCompose(SKShader.CreateColor(0), SKShader.CreateColor(new SKColor(0,0,0,255)), SKBlendMode.Multiply)
2. Create an SKPaint with Shader = that shader
3. Draw using the paint
4. Observe that the drawn color is RGBA(0,0,0,255) instead of fully transparent

**Environment:** SkiaSharp 3.x Alpha, Visual Studio on Windows 11 Home 23H2

**Code snippets:**

```csharp
var shader = SKShader.CreateCompose(SKShader.CreateColor(0), SKShader.CreateColor(new(0,0,0,255)), SKBlendMode.Multiply);
var paint =  new SKPaint() { Shader = shader };
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | It draws zero RGB with 255 alpha. |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.x |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The behavior is correct per W3C compositing spec; the issue is a misunderstanding of how Multiply blend mode composites alpha. |

## Analysis

### Technical Summary

SKBlendMode.Multiply follows the W3C compositing specification which defines Multiply as Cr = Cs*Cd + Cs*(1-αd) + Cd*(1-αs), with alpha compositing αr = αs + αd - αs*αd. With src=(0,0,0,1) opaque black and dst=(0,0,0,0) transparent black: result alpha = 1+0-1*0 = 1 (fully opaque) and RGB = 0. The observed output RGBA(0,0,0,255) is mathematically correct. The reporter expects simple per-channel multiplication but that is not how Porter-Duff compositing modes work.

### Rationale

The SKShader.CreateCompose delegation to sk_shader_new_blend passes through to Skia's native Multiply mode, which correctly implements the W3C Porter-Duff Multiply compositing formula. The result alpha is not simply src_alpha * dst_alpha — it includes coverage terms. The behavior is by-design and consistent with every major graphics system (CSS compositing, SVG, Photoshop). This is not a bug in SkiaSharp or Skia; it is a misunderstanding of the blend mode semantics.

### Key Signals

- "The paint should draw nothing, as the composed color is 0 after multiplication." — **issue body** (Reporter assumes all 4 RGBA channels are multiplied independently. The W3C Multiply formula preserves coverage from the opaque source layer.)
- "It draws zero RGB with 255 alpha." — **issue body** (This is the correct result: src is fully opaque (alpha=1), so the composited result is fully opaque regardless of both color values being 0.)
- "the uint representation of color takes first byte as alpha, this is inconsistent with the convention" — **issue body** (Misunderstanding: SkiaSharp uses standard ARGB format (alpha in high byte), which is the widely-used convention for Windows and Skia.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKShader.cs` | 409-417 | direct | SKShader.CreateCompose delegates to SkiaApi.sk_shader_new_blend(mode, shaderA.Handle, shaderB.Handle). No transformation is applied — this directly calls Skia's native blend shader, which implements standard Porter-Duff compositing. |
| `binding/SkiaSharp/SKColor.cs` | 19-21 | context | SKColor constructor: new SKColor(red, green, blue, alpha) stores as (alpha<<24)|(red<<16)|(green<<8)|blue. So new(0,0,0,255) creates RGBA(0,0,0,255) = opaque black. SKShader.CreateColor(0) creates RGBA(0,0,0,0) = transparent black. Both confirm the reporter's colors are correctly interpreted. |

### Workarounds

- If pure per-channel alpha multiplication is desired, use SKColorFilter with a custom blend or SKColorFilter.CreateBlendMode with pre-multiplied colors instead of SKBlendMode.Multiply on shaders.
- To achieve transparent output, compose two transparent colors or use SKBlendMode.Clear which discards all color.

### Resolution Proposals

**Hypothesis:** The reporter misunderstands the W3C Porter-Duff Multiply compositing mode. The behavior is correct. No fix needed.

1. **Explain Multiply compositing semantics** — investigation, confidence 0.92 (92%), cost/xs, validated=untested
   - Clarify that SKBlendMode.Multiply follows the W3C compositing spec: Cr = Cs*Cd + Cs*(1-αd) + Cd*(1-αs). With opaque src (α=1), the result alpha is always 1 regardless of the color values. Point to alternatives like SKBlendMode.DstIn or SKBlendMode.SrcIn for intersection-based alpha.

**Recommended proposal:** Explain Multiply compositing semantics

**Why:** The behavior is correct — no code change needed. A comment explaining the spec behavior and suggesting alternatives closes the issue.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | SKBlendMode.Multiply correctly implements the W3C compositing specification. The result RGBA(0,0,0,255) is mathematically correct. The issue is a misunderstanding of how Porter-Duff blend modes handle alpha compositing. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug and area/SkiaSharp labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic |
| add-comment | high | 0.88 (88%) | Explain Multiply blend mode semantics and close | — |
| close-issue | medium | 0.88 (88%) | Close as not a bug — behavior matches W3C compositing spec | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thank you for the report! The behavior you're seeing is actually correct according to the [W3C Compositing and Blending specification](https://www.w3.org/TR/compositing-1/#blendingmultiply).

`SKBlendMode.Multiply` is a Porter-Duff compositing operation, not a simple per-channel multiplication. The formula is:

```
Cr = Cs*Cd + Cs*(1-αd) + Cd*(1-αs)
αr = αs + αd - αs*αd
```

With your inputs:
- **shaderA** (dst): transparent black — RGBA(0, 0, 0, 0), so αd=0
- **shaderB** (src): opaque black — RGBA(0, 0, 0, 255), so αs=1

Result alpha: αr = 1 + 0 - 1×0 = **1** (fully opaque)
Result RGB: Cr = 0×0 + 0×(1−0) + 0×(1−1) = **0**

So RGBA(0, 0, 0, 255) is the mathematically correct answer. When the source is fully opaque, the composited result is fully opaque — the source "covers" the destination completely, with its colors multiplied.

If you want the alpha to also be affected (i.e., only paint where both layers overlap), consider `SKBlendMode.DstIn` or `SKBlendMode.SrcIn`, which mask by the intersection of alphas.

Regarding the color format: SkiaSharp uses standard ARGB byte order (`(alpha << 24) | (red << 16) | (green << 8) | blue`), so `0xFF000000` is opaque black — this is consistent with the Windows COLORREF / GDI+ convention and Skia's native format.

Closing as not a bug. Feel free to reopen if you have further questions!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3017,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T02:02:41Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter claims SKBlendMode.Multiply produces wrong alpha when composing a transparent color over an opaque color, expecting all RGBA channels to be independently multiplied to zero, but this misunderstands the W3C compositing spec for Multiply blend mode.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "It draws zero RGB with 255 alpha.",
      "reproQuality": "complete",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a shader using SKShader.CreateCompose(SKShader.CreateColor(0), SKShader.CreateColor(new SKColor(0,0,0,255)), SKBlendMode.Multiply)",
        "Create an SKPaint with Shader = that shader",
        "Draw using the paint",
        "Observe that the drawn color is RGBA(0,0,0,255) instead of fully transparent"
      ],
      "codeSnippets": [
        "var shader = SKShader.CreateCompose(SKShader.CreateColor(0), SKShader.CreateColor(new(0,0,0,255)), SKBlendMode.Multiply);\nvar paint =  new SKPaint() { Shader = shader };"
      ],
      "environmentDetails": "SkiaSharp 3.x Alpha, Visual Studio on Windows 11 Home 23H2"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.x"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "The behavior is correct per W3C compositing spec; the issue is a misunderstanding of how Multiply blend mode composites alpha."
    }
  },
  "analysis": {
    "summary": "SKBlendMode.Multiply follows the W3C compositing specification which defines Multiply as Cr = Cs*Cd + Cs*(1-αd) + Cd*(1-αs), with alpha compositing αr = αs + αd - αs*αd. With src=(0,0,0,1) opaque black and dst=(0,0,0,0) transparent black: result alpha = 1+0-1*0 = 1 (fully opaque) and RGB = 0. The observed output RGBA(0,0,0,255) is mathematically correct. The reporter expects simple per-channel multiplication but that is not how Porter-Duff compositing modes work.",
    "rationale": "The SKShader.CreateCompose delegation to sk_shader_new_blend passes through to Skia's native Multiply mode, which correctly implements the W3C Porter-Duff Multiply compositing formula. The result alpha is not simply src_alpha * dst_alpha — it includes coverage terms. The behavior is by-design and consistent with every major graphics system (CSS compositing, SVG, Photoshop). This is not a bug in SkiaSharp or Skia; it is a misunderstanding of the blend mode semantics.",
    "keySignals": [
      {
        "text": "The paint should draw nothing, as the composed color is 0 after multiplication.",
        "source": "issue body",
        "interpretation": "Reporter assumes all 4 RGBA channels are multiplied independently. The W3C Multiply formula preserves coverage from the opaque source layer."
      },
      {
        "text": "It draws zero RGB with 255 alpha.",
        "source": "issue body",
        "interpretation": "This is the correct result: src is fully opaque (alpha=1), so the composited result is fully opaque regardless of both color values being 0."
      },
      {
        "text": "the uint representation of color takes first byte as alpha, this is inconsistent with the convention",
        "source": "issue body",
        "interpretation": "Misunderstanding: SkiaSharp uses standard ARGB format (alpha in high byte), which is the widely-used convention for Windows and Skia."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKShader.cs",
        "lines": "409-417",
        "finding": "SKShader.CreateCompose delegates to SkiaApi.sk_shader_new_blend(mode, shaderA.Handle, shaderB.Handle). No transformation is applied — this directly calls Skia's native blend shader, which implements standard Porter-Duff compositing.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKColor.cs",
        "lines": "19-21",
        "finding": "SKColor constructor: new SKColor(red, green, blue, alpha) stores as (alpha<<24)|(red<<16)|(green<<8)|blue. So new(0,0,0,255) creates RGBA(0,0,0,255) = opaque black. SKShader.CreateColor(0) creates RGBA(0,0,0,0) = transparent black. Both confirm the reporter's colors are correctly interpreted.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "If pure per-channel alpha multiplication is desired, use SKColorFilter with a custom blend or SKColorFilter.CreateBlendMode with pre-multiplied colors instead of SKBlendMode.Multiply on shaders.",
      "To achieve transparent output, compose two transparent colors or use SKBlendMode.Clear which discards all color."
    ],
    "resolution": {
      "hypothesis": "The reporter misunderstands the W3C Porter-Duff Multiply compositing mode. The behavior is correct. No fix needed.",
      "proposals": [
        {
          "title": "Explain Multiply compositing semantics",
          "description": "Clarify that SKBlendMode.Multiply follows the W3C compositing spec: Cr = Cs*Cd + Cs*(1-αd) + Cd*(1-αs). With opaque src (α=1), the result alpha is always 1 regardless of the color values. Point to alternatives like SKBlendMode.DstIn or SKBlendMode.SrcIn for intersection-based alpha.",
          "category": "investigation",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Explain Multiply compositing semantics",
      "recommendedReason": "The behavior is correct — no code change needed. A comment explaining the spec behavior and suggesting alternatives closes the issue."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "SKBlendMode.Multiply correctly implements the W3C compositing specification. The result RGBA(0,0,0,255) is mathematically correct. The issue is a misunderstanding of how Porter-Duff blend modes handle alpha compositing.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug and area/SkiaSharp labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain Multiply blend mode semantics and close",
        "risk": "high",
        "confidence": 0.88,
        "comment": "Thank you for the report! The behavior you're seeing is actually correct according to the [W3C Compositing and Blending specification](https://www.w3.org/TR/compositing-1/#blendingmultiply).\n\n`SKBlendMode.Multiply` is a Porter-Duff compositing operation, not a simple per-channel multiplication. The formula is:\n\n```\nCr = Cs*Cd + Cs*(1-αd) + Cd*(1-αs)\nαr = αs + αd - αs*αd\n```\n\nWith your inputs:\n- **shaderA** (dst): transparent black — RGBA(0, 0, 0, 0), so αd=0\n- **shaderB** (src): opaque black — RGBA(0, 0, 0, 255), so αs=1\n\nResult alpha: αr = 1 + 0 - 1×0 = **1** (fully opaque)\nResult RGB: Cr = 0×0 + 0×(1−0) + 0×(1−1) = **0**\n\nSo RGBA(0, 0, 0, 255) is the mathematically correct answer. When the source is fully opaque, the composited result is fully opaque — the source \"covers\" the destination completely, with its colors multiplied.\n\nIf you want the alpha to also be affected (i.e., only paint where both layers overlap), consider `SKBlendMode.DstIn` or `SKBlendMode.SrcIn`, which mask by the intersection of alphas.\n\nRegarding the color format: SkiaSharp uses standard ARGB byte order (`(alpha << 24) | (red << 16) | (green << 8) | blue`), so `0xFF000000` is opaque black — this is consistent with the Windows COLORREF / GDI+ convention and Skia's native format.\n\nClosing as not a bug. Feel free to reopen if you have further questions!"
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — behavior matches W3C compositing spec",
        "risk": "medium",
        "confidence": 0.88,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
