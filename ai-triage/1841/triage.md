# Issue Triage Report — #1841

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-22T06:05:39Z |
| Type | type/question (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** Reporter asks whether SkiaSharp supports custom dithering algorithms (Bayer 4x4/8x8, error diffusion) rather than only Skia's built-in simple dither flag.

**Analysis:** SkiaSharp exposes SKPaint.IsDither which enables Skia's built-in ordered dithering, but there is no API for custom dithering algorithms. Custom dithering requires direct pixel manipulation via SKBitmap.GetPixel/SetPixel or GetPixelSpan.

**Recommendations:** **close-as-not-a-bug** — Answerable usage question. SKPaint.IsDither enables built-in dithering. Custom algorithms require manual pixel manipulation — no SkiaSharp bug or missing binding is involved.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

## Analysis

### Technical Summary

SkiaSharp exposes SKPaint.IsDither which enables Skia's built-in ordered dithering, but there is no API for custom dithering algorithms. Custom dithering requires direct pixel manipulation via SKBitmap.GetPixel/SetPixel or GetPixelSpan.

### Rationale

The reporter's title begins with [QUESTION] and the body asks whether the feature exists — this is a usage question, not a bug report. Code investigation confirms that only a binary dither flag is exposed at the paint level; no configurable dithering algorithms (Bayer, error diffusion) exist in Skia's C API or SkiaSharp bindings.

### Key Signals

- "Bayer dithering, bayer 8x8 bayer 4x4, error diffusion dithering... I'd like to enable some of these, but am unsure if SkiaSharp supports custom dithering." — **issue body** (Reporter is asking whether named dithering algorithms are directly supported in SkiaSharp. The framing is exploratory — they are not reporting a broken feature.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPaint.cs` | 128-138 | direct | SKPaint.IsDither is a boolean toggle that enables Skia's built-in ordered dithering. No API for custom dithering algorithms (Bayer 8x8, error diffusion, etc.) exists at the paint level. |
| `binding/SkiaSharp/SKBitmap.cs` | 138-145 | related | SKBitmap.GetPixel(x, y) and SetPixel(x, y, color) are public APIs enabling per-pixel access to implement custom dithering algorithms manually as a post-process step. |
| `binding/SkiaSharp/SKBitmap.cs` | 336-337 | related | SKBitmap.GetPixelSpan() returns a Span<byte> for efficient direct memory access, enabling high-performance custom dithering without the per-pixel overhead of GetPixel/SetPixel. |

### Workarounds

- Enable Skia's built-in simple dithering via SKPaint.IsDither = true — sufficient for gradient banding reduction on low-bit-depth displays.
- For custom dithering algorithms (Bayer, error diffusion): iterate pixels via SKBitmap.GetPixel/SetPixel or use SKBitmap.GetPixelSpan() for high-performance raw byte access.

### Resolution Proposals

**Hypothesis:** SkiaSharp only exposes a boolean dither toggle (SKPaint.IsDither), not pluggable dithering algorithms. Custom Bayer or error-diffusion dithering must be implemented as a post-process step via pixel manipulation.

1. **Enable built-in dithering** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Set SKPaint.IsDither = true to enable Skia's built-in ordered dithering. Sufficient for most gradient banding reduction on low bit-depth targets and requires no custom code.

```csharp
var paint = new SKPaint();
paint.IsDither = true;
// Use paint for drawing gradients, etc.
```
2. **Implement custom dithering via pixel manipulation** — alternative, confidence 0.90 (90%), cost/m, validated=yes
   - Render to an SKBitmap, then apply a custom dithering algorithm (Bayer, Floyd-Steinberg, etc.) by iterating pixels. Use GetPixelSpan() for performance on large images.

```csharp
// After drawing to bitmap via SKCanvas:
for (int y = 0; y < bitmap.Height; y++)
    for (int x = 0; x < bitmap.Width; x++)
    {
        SKColor c = bitmap.GetPixel(x, y);
        bitmap.SetPixel(x, y, ApplyBayerDither(c, x, y));
    }
```

**Recommended proposal:** Enable built-in dithering

**Why:** Zero-effort change; SKPaint.IsDither is already supported. Use custom pixel manipulation only if a specific algorithm (Bayer 8x8, error diffusion) is strictly required.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | Answerable usage question. SKPaint.IsDither enables built-in dithering. Custom algorithms require manual pixel manipulation — no SkiaSharp bug or missing binding is involved. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question and SkiaSharp labels | labels=type/question, area/SkiaSharp |
| add-comment | medium | 0.90 (90%) | Post answer explaining IsDither and custom pixel manipulation options | — |
| close-issue | medium | 0.80 (80%) | Close as answered — usage question fully answered by existing API | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
SkiaSharp exposes Skia's built-in dither flag via `SKPaint.IsDither`:

```csharp
var paint = new SKPaint();
paint.IsDither = true;
// Use paint for drawing gradients, etc.
```

This applies a simple ordered dither internally, which handles most gradient banding cases.

Skia does not expose pluggable dithering algorithms (Bayer 8x8, error diffusion, etc.) at the paint level, so custom dithering needs to be implemented as a post-process step. After rendering to an `SKBitmap`, iterate pixels and apply your algorithm:

```csharp
// After drawing to bitmap via SKCanvas:
for (int y = 0; y < bitmap.Height; y++)
    for (int x = 0; x < bitmap.Width; x++)
    {
        SKColor c = bitmap.GetPixel(x, y);
        bitmap.SetPixel(x, y, ApplyBayerDither(c, x, y));
    }
```

For high-performance scenarios (large images), use `bitmap.GetPixelSpan()` to operate directly on the raw byte buffer, which avoids per-pixel method call overhead.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1841,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-22T06:05:39Z"
  },
  "summary": "Reporter asks whether SkiaSharp supports custom dithering algorithms (Bayer 4x4/8x8, error diffusion) rather than only Skia's built-in simple dither flag.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {},
  "analysis": {
    "summary": "SkiaSharp exposes SKPaint.IsDither which enables Skia's built-in ordered dithering, but there is no API for custom dithering algorithms. Custom dithering requires direct pixel manipulation via SKBitmap.GetPixel/SetPixel or GetPixelSpan.",
    "rationale": "The reporter's title begins with [QUESTION] and the body asks whether the feature exists — this is a usage question, not a bug report. Code investigation confirms that only a binary dither flag is exposed at the paint level; no configurable dithering algorithms (Bayer, error diffusion) exist in Skia's C API or SkiaSharp bindings.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "128-138",
        "finding": "SKPaint.IsDither is a boolean toggle that enables Skia's built-in ordered dithering. No API for custom dithering algorithms (Bayer 8x8, error diffusion, etc.) exists at the paint level.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "138-145",
        "finding": "SKBitmap.GetPixel(x, y) and SetPixel(x, y, color) are public APIs enabling per-pixel access to implement custom dithering algorithms manually as a post-process step.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "336-337",
        "finding": "SKBitmap.GetPixelSpan() returns a Span<byte> for efficient direct memory access, enabling high-performance custom dithering without the per-pixel overhead of GetPixel/SetPixel.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Bayer dithering, bayer 8x8 bayer 4x4, error diffusion dithering... I'd like to enable some of these, but am unsure if SkiaSharp supports custom dithering.",
        "source": "issue body",
        "interpretation": "Reporter is asking whether named dithering algorithms are directly supported in SkiaSharp. The framing is exploratory — they are not reporting a broken feature."
      }
    ],
    "workarounds": [
      "Enable Skia's built-in simple dithering via SKPaint.IsDither = true — sufficient for gradient banding reduction on low-bit-depth displays.",
      "For custom dithering algorithms (Bayer, error diffusion): iterate pixels via SKBitmap.GetPixel/SetPixel or use SKBitmap.GetPixelSpan() for high-performance raw byte access."
    ],
    "resolution": {
      "hypothesis": "SkiaSharp only exposes a boolean dither toggle (SKPaint.IsDither), not pluggable dithering algorithms. Custom Bayer or error-diffusion dithering must be implemented as a post-process step via pixel manipulation.",
      "proposals": [
        {
          "title": "Enable built-in dithering",
          "description": "Set SKPaint.IsDither = true to enable Skia's built-in ordered dithering. Sufficient for most gradient banding reduction on low bit-depth targets and requires no custom code.",
          "category": "workaround",
          "codeSnippet": "var paint = new SKPaint();\npaint.IsDither = true;\n// Use paint for drawing gradients, etc.",
          "validated": "yes",
          "confidence": 0.95,
          "effort": "cost/xs"
        },
        {
          "title": "Implement custom dithering via pixel manipulation",
          "description": "Render to an SKBitmap, then apply a custom dithering algorithm (Bayer, Floyd-Steinberg, etc.) by iterating pixels. Use GetPixelSpan() for performance on large images.",
          "category": "alternative",
          "codeSnippet": "// After drawing to bitmap via SKCanvas:\nfor (int y = 0; y < bitmap.Height; y++)\n    for (int x = 0; x < bitmap.Width; x++)\n    {\n        SKColor c = bitmap.GetPixel(x, y);\n        bitmap.SetPixel(x, y, ApplyBayerDither(c, x, y));\n    }",
          "validated": "yes",
          "confidence": 0.9,
          "effort": "cost/m"
        }
      ],
      "recommendedProposal": "Enable built-in dithering",
      "recommendedReason": "Zero-effort change; SKPaint.IsDither is already supported. Use custom pixel manipulation only if a specific algorithm (Bayer 8x8, error diffusion) is strictly required."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "Answerable usage question. SKPaint.IsDither enables built-in dithering. Custom algorithms require manual pixel manipulation — no SkiaSharp bug or missing binding is involved.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and SkiaSharp labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer explaining IsDither and custom pixel manipulation options",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "SkiaSharp exposes Skia's built-in dither flag via `SKPaint.IsDither`:\n\n```csharp\nvar paint = new SKPaint();\npaint.IsDither = true;\n// Use paint for drawing gradients, etc.\n```\n\nThis applies a simple ordered dither internally, which handles most gradient banding cases.\n\nSkia does not expose pluggable dithering algorithms (Bayer 8x8, error diffusion, etc.) at the paint level, so custom dithering needs to be implemented as a post-process step. After rendering to an `SKBitmap`, iterate pixels and apply your algorithm:\n\n```csharp\n// After drawing to bitmap via SKCanvas:\nfor (int y = 0; y < bitmap.Height; y++)\n    for (int x = 0; x < bitmap.Width; x++)\n    {\n        SKColor c = bitmap.GetPixel(x, y);\n        bitmap.SetPixel(x, y, ApplyBayerDither(c, x, y));\n    }\n```\n\nFor high-performance scenarios (large images), use `bitmap.GetPixelSpan()` to operate directly on the raw byte buffer, which avoids per-pixel method call overhead."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — usage question fully answered by existing API",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
