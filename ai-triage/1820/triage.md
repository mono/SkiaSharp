# Issue Triage Report — #1820

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T21:59:56Z |
| Type | type/question (0.97 (97%)) |
| Area | area/SkiaSharp (0.97 (97%)) |
| Suggested action | close-as-not-a-bug (0.90 (90%)) |

**Issue Summary:** User asks how to set a rectangular subset of pixels in an SKBitmap, unable to find an API for it.

**Analysis:** The reporter wants to write pixel data into a rectangular region of an SKBitmap. This is a 'how-to' question, not a bug. The functionality is achievable via SKCanvas drawn on the bitmap or by writing raw pixel memory via GetPixels() with row-stride arithmetic.

**Recommendations:** **close-as-not-a-bug** — This is a usage question. The functionality exists via SKCanvas + DrawBitmap with source/dest rects. Answering with a code snippet and closing is appropriate.

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

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/416 — Related issue: how to set all bitmap pixels from a byte array

## Analysis

### Technical Summary

The reporter wants to write pixel data into a rectangular region of an SKBitmap. This is a 'how-to' question, not a bug. The functionality is achievable via SKCanvas drawn on the bitmap or by writing raw pixel memory via GetPixels() with row-stride arithmetic.

### Rationale

This is a how-to question. The answer exists in the API: an SKCanvas can be constructed from an SKBitmap and DrawBitmap/DrawImage with source+destination rects will copy pixel regions. The issue should be answered and closed as not-a-bug.

### Key Signals

- "what I need to do is set a subset (rectangle) of the pixels in an SKBitmap" — **issue body** (User wants to overwrite a rectangular region of an existing bitmap with new pixel data.)
- "I couldn't find an API to do this" — **issue body** (Discovery problem — the API exists but is not obvious.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 131-141 | direct | SKBitmap.SetPixel(int x, int y, SKColor color) sets one pixel at a time by creating an SKCanvas internally and calling DrawPoint. Usable for small subsets. |
| `binding/SkiaSharp/SKCanvas.cs` | 21 | direct | new SKCanvas(SKBitmap bitmap) constructor exists. Users can create a canvas over the bitmap and call DrawBitmap/DrawImage with source and destination rects to paint a pixel region. |
| `binding/SkiaSharp/SKBitmap.cs` | 297-313 | related | GetPixels() returns IntPtr to the raw pixel buffer; SetPixels(IntPtr) replaces the whole buffer. Raw memory manipulation with row-stride arithmetic can target any sub-region. |
| `binding/SkiaSharp/SKCanvas.cs` | 493-502 | direct | SKCanvas.DrawImage(SKImage image, SKRect source, SKRect dest) and the equivalent DrawBitmap overloads allow copying a source rectangle from one image/bitmap to a destination rectangle, which is exactly what the reporter needs. |

### Resolution Proposals

1. **Use SKCanvas over SKBitmap with DrawBitmap** — workaround, cost/xs, validated=yes
   - Create an SKCanvas wrapping the target bitmap, then call DrawBitmap with a source rect and destination rect to write pixels into the desired subset.

```csharp
// target is the SKBitmap you want to write into
// source is the SKBitmap containing the pixels to write
// destRect is the rectangle in target where pixels should be placed
using var canvas = new SKCanvas(target);
canvas.DrawBitmap(source, sourceRect, destRect);
```

**Recommended proposal:** Use SKCanvas over SKBitmap with DrawBitmap

**Why:** SKCanvas wrapping an SKBitmap is a well-established SkiaSharp pattern. DrawBitmap with source+dest rects directly solves the stated problem.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.90 (90%) |
| Reason | This is a usage question. The functionality exists via SKCanvas + DrawBitmap with source/dest rects. Answering with a code snippet and closing is appropriate. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/question and area/SkiaSharp labels | labels=type/question, area/SkiaSharp |
| add-comment | medium | 0.90 (90%) | Explain how to write pixels to a subset rectangle using SKCanvas | — |
| close-issue | medium | 0.90 (90%) | Close as not-a-bug — question answered | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the question!

You can write pixels into a rectangular region of an `SKBitmap` by creating an `SKCanvas` over it and using `DrawBitmap` (or `DrawImage`) with source and destination rectangles:

```csharp
// 'target' is the SKBitmap you want to partially overwrite
// 'source' is the SKBitmap (or SKImage) containing the new pixels
// 'destRect' is the region in 'target' where you want the pixels placed
using var canvas = new SKCanvas(target);
canvas.DrawBitmap(source, sourceRect, destRect);
```

For a single pixel you can also use `SKBitmap.SetPixel(x, y, color)`, but the canvas approach is more efficient for rectangular regions.

If you need to write raw bytes directly, `GetPixels()` returns an `IntPtr` to the pixel buffer and you can use row-stride arithmetic to target any sub-region with `Marshal.Copy` or `unsafe` spans.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1820,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T21:59:56Z"
  },
  "summary": "User asks how to set a rectangular subset of pixels in an SKBitmap, unable to find an API for it.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.97
    }
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/416",
          "description": "Related issue: how to set all bitmap pixels from a byte array"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The reporter wants to write pixel data into a rectangular region of an SKBitmap. This is a 'how-to' question, not a bug. The functionality is achievable via SKCanvas drawn on the bitmap or by writing raw pixel memory via GetPixels() with row-stride arithmetic.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "131-141",
        "finding": "SKBitmap.SetPixel(int x, int y, SKColor color) sets one pixel at a time by creating an SKCanvas internally and calling DrawPoint. Usable for small subsets.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "21",
        "finding": "new SKCanvas(SKBitmap bitmap) constructor exists. Users can create a canvas over the bitmap and call DrawBitmap/DrawImage with source and destination rects to paint a pixel region.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "297-313",
        "finding": "GetPixels() returns IntPtr to the raw pixel buffer; SetPixels(IntPtr) replaces the whole buffer. Raw memory manipulation with row-stride arithmetic can target any sub-region.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "493-502",
        "finding": "SKCanvas.DrawImage(SKImage image, SKRect source, SKRect dest) and the equivalent DrawBitmap overloads allow copying a source rectangle from one image/bitmap to a destination rectangle, which is exactly what the reporter needs.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "what I need to do is set a subset (rectangle) of the pixels in an SKBitmap",
        "source": "issue body",
        "interpretation": "User wants to overwrite a rectangular region of an existing bitmap with new pixel data."
      },
      {
        "text": "I couldn't find an API to do this",
        "source": "issue body",
        "interpretation": "Discovery problem — the API exists but is not obvious."
      }
    ],
    "rationale": "This is a how-to question. The answer exists in the API: an SKCanvas can be constructed from an SKBitmap and DrawBitmap/DrawImage with source+destination rects will copy pixel regions. The issue should be answered and closed as not-a-bug.",
    "resolution": {
      "proposals": [
        {
          "title": "Use SKCanvas over SKBitmap with DrawBitmap",
          "description": "Create an SKCanvas wrapping the target bitmap, then call DrawBitmap with a source rect and destination rect to write pixels into the desired subset.",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "yes",
          "codeSnippet": "// target is the SKBitmap you want to write into\n// source is the SKBitmap containing the pixels to write\n// destRect is the rectangle in target where pixels should be placed\nusing var canvas = new SKCanvas(target);\ncanvas.DrawBitmap(source, sourceRect, destRect);"
        }
      ],
      "recommendedProposal": "Use SKCanvas over SKBitmap with DrawBitmap",
      "recommendedReason": "SKCanvas wrapping an SKBitmap is a well-established SkiaSharp pattern. DrawBitmap with source+dest rects directly solves the stated problem."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.9,
      "reason": "This is a usage question. The functionality exists via SKCanvas + DrawBitmap with source/dest rects. Answering with a code snippet and closing is appropriate.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/question and area/SkiaSharp labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain how to write pixels to a subset rectangle using SKCanvas",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the question!\n\nYou can write pixels into a rectangular region of an `SKBitmap` by creating an `SKCanvas` over it and using `DrawBitmap` (or `DrawImage`) with source and destination rectangles:\n\n```csharp\n// 'target' is the SKBitmap you want to partially overwrite\n// 'source' is the SKBitmap (or SKImage) containing the new pixels\n// 'destRect' is the region in 'target' where you want the pixels placed\nusing var canvas = new SKCanvas(target);\ncanvas.DrawBitmap(source, sourceRect, destRect);\n```\n\nFor a single pixel you can also use `SKBitmap.SetPixel(x, y, color)`, but the canvas approach is more efficient for rectangular regions.\n\nIf you need to write raw bytes directly, `GetPixels()` returns an `IntPtr` to the pixel buffer and you can use row-stride arithmetic to target any sub-region with `Marshal.Copy` or `unsafe` spans."
      },
      {
        "type": "close-issue",
        "description": "Close as not-a-bug — question answered",
        "risk": "medium",
        "confidence": 0.9,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
