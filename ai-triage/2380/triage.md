# Issue Triage Report — #2380

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T22:06:00Z |
| Type | type/question (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.92 (92%)) |

**Issue Summary:** User asks how to create a SkiaSharp bitmap from raw pixel data with a stride and IntPtr scan0, equivalent to System.Drawing.Bitmap(width, height, stride, PixelFormat, IntPtr).

**Analysis:** The reporter wants to wrap existing pixel memory (width, height, stride/rowBytes, format, raw pointer) in a SkiaSharp bitmap, equivalent to the System.Drawing constructor that takes an IntPtr scan0. SKBitmap.InstallPixels(SKImageInfo, IntPtr pixels, int rowBytes) is the direct API equivalent — it wraps an external pixel buffer without copying and accepts a stride (rowBytes). The answer is already implemented in the shipping SkiaSharp API.

**Recommendations:** **close-as-not-a-bug** — This is a how-to question with a clear answer: SKBitmap.InstallPixels(SKImageInfo, IntPtr, int rowBytes) is the direct equivalent. No bug or missing feature.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

## Analysis

### Technical Summary

The reporter wants to wrap existing pixel memory (width, height, stride/rowBytes, format, raw pointer) in a SkiaSharp bitmap, equivalent to the System.Drawing constructor that takes an IntPtr scan0. SKBitmap.InstallPixels(SKImageInfo, IntPtr pixels, int rowBytes) is the direct API equivalent — it wraps an external pixel buffer without copying and accepts a stride (rowBytes). The answer is already implemented in the shipping SkiaSharp API.

### Rationale

This is a pure how-to question — no bug, no missing feature. SKBitmap.InstallPixels(SKImageInfo, IntPtr, int rowBytes) is the documented, shipping equivalent. Classification is type/question with close-as-not-a-bug since the API exists and works.

### Key Signals

- "System.Drawing.Bitmap (int width, int height, int stride, PixelFormat format, IntPtr scan0)" — **issue body** (Reporter wants to wrap existing native pixel memory into a SkiaSharp bitmap with explicit stride and pixel format.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 591-614 | direct | SKBitmap.InstallPixels(SKImageInfo info, IntPtr pixels, int rowBytes) wraps an external pixel buffer without copying. The rowBytes parameter is equivalent to 'stride' in System.Drawing. An optional SKBitmapReleaseDelegate overload handles lifetime management of the external buffer. |
| `binding/SkiaSharp/SKImage.cs` | 119-129 | related | SKImage.FromPixels(SKImageInfo info, IntPtr pixels, int rowBytes) and SKImage.FromPixelCopy(SKImageInfo info, IntPtr pixels, int rowBytes) offer alternative paths — non-copying and copying respectively — for constructing from raw pointer + stride. |

### Resolution Proposals

**Hypothesis:** SKBitmap.InstallPixels(SKImageInfo, IntPtr pixels, int rowBytes) is the direct equivalent of the System.Drawing constructor the reporter is asking about.

1. **Use SKBitmap.InstallPixels to wrap existing pixel memory** — workaround, confidence 0.97 (97%), cost/xs, validated=yes
   - Create an SKBitmap that wraps an existing IntPtr pixel buffer without copying, specifying width, height, rowBytes (stride), and color type.

```csharp
// Equivalent of: new System.Drawing.Bitmap(width, height, stride, PixelFormat.Format32bppArgb, scan0)
var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
var bitmap = new SKBitmap();
bool success = bitmap.InstallPixels(info, scan0, stride);
// bitmap now wraps the existing pixel buffer without copying
// The caller is responsible for keeping scan0 alive while bitmap is in use.
```

**Recommended proposal:** Use SKBitmap.InstallPixels to wrap existing pixel memory

**Why:** SKBitmap.InstallPixels is the exact API designed for this use case — it accepts (SKImageInfo, IntPtr pixels, int rowBytes) which maps 1:1 to (PixelFormat, scan0, stride).

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.92 (92%) |
| Reason | This is a how-to question with a clear answer: SKBitmap.InstallPixels(SKImageInfo, IntPtr, int rowBytes) is the direct equivalent. No bug or missing feature. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/question and area/SkiaSharp labels | labels=type/question, area/SkiaSharp, tenet/compatibility |
| add-comment | medium | 0.92 (92%) | Answer the question with SKBitmap.InstallPixels usage and close the issue | — |
| close-issue | medium | 0.88 (88%) | Close as answered (not a bug, question is fully answered) | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
## Answer

The SkiaSharp equivalent of `System.Drawing.Bitmap(width, height, stride, PixelFormat, IntPtr scan0)` is **`SKBitmap.InstallPixels`**:

```csharp
// Equivalent of: new System.Drawing.Bitmap(width, height, stride, PixelFormat.Format32bppArgb, scan0)
var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
var bitmap = new SKBitmap();
bool success = bitmap.InstallPixels(info, scan0, stride);
// bitmap now wraps the existing pixel buffer without copying
// The caller is responsible for keeping scan0 alive while bitmap is in use.
```

**Mapping:**
| `System.Drawing` | SkiaSharp |
|---|---|
| `width`, `height` | `SKImageInfo.Width`, `SKImageInfo.Height` |
| `stride` | `rowBytes` |
| `PixelFormat` | `SKColorType` + `SKAlphaType` in `SKImageInfo` |
| `scan0` (IntPtr) | `pixels` (IntPtr) |

If you want a **copy** instead of wrapping the buffer, use `SKImage.FromPixelCopy(info, scan0, stride)` instead.

If you need to be notified when the bitmap is done with the buffer (to free it), use the overload with `SKBitmapReleaseDelegate`:
```csharp
bool success = bitmap.InstallPixels(info, scan0, stride, (addr, ctx) => { /* free the native buffer */ });
```

Closing as answered. Feel free to reopen if you run into further issues.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2380,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T22:06:00Z"
  },
  "summary": "User asks how to create a SkiaSharp bitmap from raw pixel data with a stride and IntPtr scan0, equivalent to System.Drawing.Bitmap(width, height, stride, PixelFormat, IntPtr).",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {}
  },
  "analysis": {
    "summary": "The reporter wants to wrap existing pixel memory (width, height, stride/rowBytes, format, raw pointer) in a SkiaSharp bitmap, equivalent to the System.Drawing constructor that takes an IntPtr scan0. SKBitmap.InstallPixels(SKImageInfo, IntPtr pixels, int rowBytes) is the direct API equivalent — it wraps an external pixel buffer without copying and accepts a stride (rowBytes). The answer is already implemented in the shipping SkiaSharp API.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "591-614",
        "finding": "SKBitmap.InstallPixels(SKImageInfo info, IntPtr pixels, int rowBytes) wraps an external pixel buffer without copying. The rowBytes parameter is equivalent to 'stride' in System.Drawing. An optional SKBitmapReleaseDelegate overload handles lifetime management of the external buffer.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "119-129",
        "finding": "SKImage.FromPixels(SKImageInfo info, IntPtr pixels, int rowBytes) and SKImage.FromPixelCopy(SKImageInfo info, IntPtr pixels, int rowBytes) offer alternative paths — non-copying and copying respectively — for constructing from raw pointer + stride.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "System.Drawing.Bitmap (int width, int height, int stride, PixelFormat format, IntPtr scan0)",
        "source": "issue body",
        "interpretation": "Reporter wants to wrap existing native pixel memory into a SkiaSharp bitmap with explicit stride and pixel format."
      }
    ],
    "rationale": "This is a pure how-to question — no bug, no missing feature. SKBitmap.InstallPixels(SKImageInfo, IntPtr, int rowBytes) is the documented, shipping equivalent. Classification is type/question with close-as-not-a-bug since the API exists and works.",
    "resolution": {
      "hypothesis": "SKBitmap.InstallPixels(SKImageInfo, IntPtr pixels, int rowBytes) is the direct equivalent of the System.Drawing constructor the reporter is asking about.",
      "proposals": [
        {
          "title": "Use SKBitmap.InstallPixels to wrap existing pixel memory",
          "category": "workaround",
          "effort": "cost/xs",
          "confidence": 0.97,
          "validated": "yes",
          "description": "Create an SKBitmap that wraps an existing IntPtr pixel buffer without copying, specifying width, height, rowBytes (stride), and color type.",
          "codeSnippet": "// Equivalent of: new System.Drawing.Bitmap(width, height, stride, PixelFormat.Format32bppArgb, scan0)\nvar info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);\nvar bitmap = new SKBitmap();\nbool success = bitmap.InstallPixels(info, scan0, stride);\n// bitmap now wraps the existing pixel buffer without copying\n// The caller is responsible for keeping scan0 alive while bitmap is in use."
        }
      ],
      "recommendedProposal": "Use SKBitmap.InstallPixels to wrap existing pixel memory",
      "recommendedReason": "SKBitmap.InstallPixels is the exact API designed for this use case — it accepts (SKImageInfo, IntPtr pixels, int rowBytes) which maps 1:1 to (PixelFormat, scan0, stride)."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.92,
      "reason": "This is a how-to question with a clear answer: SKBitmap.InstallPixels(SKImageInfo, IntPtr, int rowBytes) is the direct equivalent. No bug or missing feature.",
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
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Answer the question with SKBitmap.InstallPixels usage and close the issue",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "## Answer\n\nThe SkiaSharp equivalent of `System.Drawing.Bitmap(width, height, stride, PixelFormat, IntPtr scan0)` is **`SKBitmap.InstallPixels`**:\n\n```csharp\n// Equivalent of: new System.Drawing.Bitmap(width, height, stride, PixelFormat.Format32bppArgb, scan0)\nvar info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);\nvar bitmap = new SKBitmap();\nbool success = bitmap.InstallPixels(info, scan0, stride);\n// bitmap now wraps the existing pixel buffer without copying\n// The caller is responsible for keeping scan0 alive while bitmap is in use.\n```\n\n**Mapping:**\n| `System.Drawing` | SkiaSharp |\n|---|---|\n| `width`, `height` | `SKImageInfo.Width`, `SKImageInfo.Height` |\n| `stride` | `rowBytes` |\n| `PixelFormat` | `SKColorType` + `SKAlphaType` in `SKImageInfo` |\n| `scan0` (IntPtr) | `pixels` (IntPtr) |\n\nIf you want a **copy** instead of wrapping the buffer, use `SKImage.FromPixelCopy(info, scan0, stride)` instead.\n\nIf you need to be notified when the bitmap is done with the buffer (to free it), use the overload with `SKBitmapReleaseDelegate`:\n```csharp\nbool success = bitmap.InstallPixels(info, scan0, stride, (addr, ctx) => { /* free the native buffer */ });\n```\n\nClosing as answered. Feel free to reopen if you run into further issues."
      },
      {
        "type": "close-issue",
        "description": "Close as answered (not a bug, question is fully answered)",
        "risk": "medium",
        "confidence": 0.88,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
