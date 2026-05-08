# Issue Triage Report — #3402

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T23:51:23Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Feature request to add native support for 1bpp, 2bpp, and 4bpp indexed/palette color bitmap drawing in SkiaSharp for memory-efficient retro game graphics storage.

**Analysis:** Reporter wants native indexed/palette color bitmap support (1bpp, 2bpp, 4bpp) in SkiaSharp for building a NES/SNES emulator-style tile graphics system in Avalonia UI. Upstream Skia removed indexed color type support (kIndex_8_SkColorType was removed in 2016). SkiaSharp's SKColorType enum has no indexed variants; minimum bpp supported is Alpha8/Gray8 at 8bpp. This feature would require upstream Skia to re-add palette/indexed support, which it does not expose.

**Recommendations:** **keep-open** — Valid feature request for indexed/palette color bitmap support. Not implementable today because upstream Skia removed indexed color type support. Worth tracking for future Skia milestone changes.

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
| Current labels | type/feature-request |

## Evidence

### Reproduction

**Environment:** Avalonia UI using SkiaSharp; retro NES/SNES-style tile graphics storage (1bpp, 2bpp, 4bpp indexed color formats)

## Analysis

### Technical Summary

Reporter wants native indexed/palette color bitmap support (1bpp, 2bpp, 4bpp) in SkiaSharp for building a NES/SNES emulator-style tile graphics system in Avalonia UI. Upstream Skia removed indexed color type support (kIndex_8_SkColorType was removed in 2016). SkiaSharp's SKColorType enum has no indexed variants; minimum bpp supported is Alpha8/Gray8 at 8bpp. This feature would require upstream Skia to re-add palette/indexed support, which it does not expose.

### Rationale

This is clearly a feature request — the reporter explicitly frames it as one and requests new functionality (indexed color bitmaps) that does not exist in SkiaSharp or the underlying Skia library. The feature was removed from Skia upstream and cannot be re-added in SkiaSharp alone without upstream support. The suggested action is keep-open as a valid low-priority feature request pending upstream Skia changes.

### Key Signals

- "Please support drawing to and from compressed 1bpp 2bpp and 4bpp indexed color graphics" — **issue body** (Explicit feature request for sub-byte pixel formats not present in Skia's color type system.)
- "Pixel operations should work on multiple pixels at a time with these formats without expanding them in memory" — **issue body** (Reporter requires zero-copy native pixel format support, not a software conversion layer.)
- "I am using avalonia ui which uses this" — **issue body** (User is consuming SkiaSharp indirectly via Avalonia; the feature gap is in SkiaSharp core, not the view layer.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/Definitions.cs` | 36-63 | direct | SKColorType enum contains no indexed/palette color types. Lowest bpp entries are Alpha8 (1 byte), Gray8 (1 byte), and Rgb565 (2 bytes). No 1bpp, 2bpp, or 4bpp variants exist. |
| `binding/SkiaSharp/Definitions.cs` | 83-115 | related | GetBytesPerPixel() extension has no case for sub-byte formats, confirming indexed color is not supported anywhere in the current codebase. |

### Workarounds

- Maintain a separate indexed palette lookup table and convert tiles to Rgba8888 before passing to SKBitmap for rendering
- Use SKBitmap with Gray8 or Alpha8 to represent single-channel 8bpp tiles and apply a palette via a custom SKColorFilter or SKShader
- Implement a custom tile blitter in C# that expands indexed pixels to a 32bpp SKBitmap on-demand for rendering

### Next Questions

- Has upstream Skia considered re-adding indexed color type support in recent milestones?
- Would a pure C# wrapper that handles pack/unpack of indexed pixel data be an acceptable interim solution?

### Resolution Proposals

**Hypothesis:** Skia removed kIndex_8_SkColorType support years ago. SkiaSharp cannot implement this without upstream Skia changes. The workaround is a software expansion layer in C#.

1. **Workaround: Expand indexed pixels to RGBA at render time** — workaround, confidence 0.85 (85%), cost/m, validated=untested
   - Maintain a palette array and an indexed byte buffer separately. When drawing, expand the indexed tile to an SKBitmap with Rgba8888 using a palette lookup loop before drawing to canvas.
2. **Track upstream Skia for indexed color re-introduction** — investigation, confidence 0.70 (70%), cost/s, validated=untested
   - File or track an upstream Skia issue requesting re-addition of indexed bitmap support. If Skia adds it, SkiaSharp can expose it with minimal effort.

**Recommended proposal:** Workaround: Expand indexed pixels to RGBA at render time

**Why:** Skia does not support indexed color formats; a software workaround is the only viable path today.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid feature request for indexed/palette color bitmap support. Not implementable today because upstream Skia removed indexed color type support. Worth tracking for future Skia milestone changes. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request and area labels | labels=type/feature-request, area/SkiaSharp, tenet/performance |
| add-comment | medium | 0.85 (85%) | Explain Skia limitation and provide workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed request! Unfortunately, Skia (the underlying C++ graphics library that SkiaSharp wraps) removed indexed/palette color type support (`kIndex_8_SkColorType`) several years ago, so SkiaSharp cannot expose 1bpp, 2bpp, or 4bpp indexed pixel formats natively without upstream Skia changes.

**Workaround for now:** You can maintain your tile data as a packed indexed byte buffer alongside a palette array, then expand tiles to `Rgba8888` on demand before passing them to an `SKBitmap` for rendering:

```csharp
// palette: SKColor[16] for 4bpp
// indexedTile: byte[] packed 4bpp data (nibble per pixel)
var bmp = new SKBitmap(8, 8, SKColorType.Rgba8888, SKAlphaType.Opaque);
for (int i = 0; i < 64; i++) {
    int nibble = (i % 2 == 0) ? (indexedTile[i/2] >> 4) : (indexedTile[i/2] & 0xF);
    bmp.SetPixel(i % 8, i / 8, palette[nibble]);
}
canvas.DrawBitmap(bmp, x, y);
```

This isn't zero-copy, but it works today. We'll keep this open to track if Skia re-adds indexed color support in a future milestone.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3402,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T23:51:23Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Feature request to add native support for 1bpp, 2bpp, and 4bpp indexed/palette color bitmap drawing in SkiaSharp for memory-efficient retro game graphics storage.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
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
      "environmentDetails": "Avalonia UI using SkiaSharp; retro NES/SNES-style tile graphics storage (1bpp, 2bpp, 4bpp indexed color formats)"
    }
  },
  "analysis": {
    "summary": "Reporter wants native indexed/palette color bitmap support (1bpp, 2bpp, 4bpp) in SkiaSharp for building a NES/SNES emulator-style tile graphics system in Avalonia UI. Upstream Skia removed indexed color type support (kIndex_8_SkColorType was removed in 2016). SkiaSharp's SKColorType enum has no indexed variants; minimum bpp supported is Alpha8/Gray8 at 8bpp. This feature would require upstream Skia to re-add palette/indexed support, which it does not expose.",
    "rationale": "This is clearly a feature request — the reporter explicitly frames it as one and requests new functionality (indexed color bitmaps) that does not exist in SkiaSharp or the underlying Skia library. The feature was removed from Skia upstream and cannot be re-added in SkiaSharp alone without upstream support. The suggested action is keep-open as a valid low-priority feature request pending upstream Skia changes.",
    "keySignals": [
      {
        "text": "Please support drawing to and from compressed 1bpp 2bpp and 4bpp indexed color graphics",
        "source": "issue body",
        "interpretation": "Explicit feature request for sub-byte pixel formats not present in Skia's color type system."
      },
      {
        "text": "Pixel operations should work on multiple pixels at a time with these formats without expanding them in memory",
        "source": "issue body",
        "interpretation": "Reporter requires zero-copy native pixel format support, not a software conversion layer."
      },
      {
        "text": "I am using avalonia ui which uses this",
        "source": "issue body",
        "interpretation": "User is consuming SkiaSharp indirectly via Avalonia; the feature gap is in SkiaSharp core, not the view layer."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "36-63",
        "finding": "SKColorType enum contains no indexed/palette color types. Lowest bpp entries are Alpha8 (1 byte), Gray8 (1 byte), and Rgb565 (2 bytes). No 1bpp, 2bpp, or 4bpp variants exist.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "83-115",
        "finding": "GetBytesPerPixel() extension has no case for sub-byte formats, confirming indexed color is not supported anywhere in the current codebase.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Maintain a separate indexed palette lookup table and convert tiles to Rgba8888 before passing to SKBitmap for rendering",
      "Use SKBitmap with Gray8 or Alpha8 to represent single-channel 8bpp tiles and apply a palette via a custom SKColorFilter or SKShader",
      "Implement a custom tile blitter in C# that expands indexed pixels to a 32bpp SKBitmap on-demand for rendering"
    ],
    "nextQuestions": [
      "Has upstream Skia considered re-adding indexed color type support in recent milestones?",
      "Would a pure C# wrapper that handles pack/unpack of indexed pixel data be an acceptable interim solution?"
    ],
    "resolution": {
      "hypothesis": "Skia removed kIndex_8_SkColorType support years ago. SkiaSharp cannot implement this without upstream Skia changes. The workaround is a software expansion layer in C#.",
      "proposals": [
        {
          "title": "Workaround: Expand indexed pixels to RGBA at render time",
          "description": "Maintain a palette array and an indexed byte buffer separately. When drawing, expand the indexed tile to an SKBitmap with Rgba8888 using a palette lookup loop before drawing to canvas.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Track upstream Skia for indexed color re-introduction",
          "description": "File or track an upstream Skia issue requesting re-addition of indexed bitmap support. If Skia adds it, SkiaSharp can expose it with minimal effort.",
          "category": "investigation",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Workaround: Expand indexed pixels to RGBA at render time",
      "recommendedReason": "Skia does not support indexed color formats; a software workaround is the only viable path today."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid feature request for indexed/palette color bitmap support. Not implementable today because upstream Skia removed indexed color type support. Worth tracking for future Skia milestone changes.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request and area labels",
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
        "description": "Explain Skia limitation and provide workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed request! Unfortunately, Skia (the underlying C++ graphics library that SkiaSharp wraps) removed indexed/palette color type support (`kIndex_8_SkColorType`) several years ago, so SkiaSharp cannot expose 1bpp, 2bpp, or 4bpp indexed pixel formats natively without upstream Skia changes.\n\n**Workaround for now:** You can maintain your tile data as a packed indexed byte buffer alongside a palette array, then expand tiles to `Rgba8888` on demand before passing them to an `SKBitmap` for rendering:\n\n```csharp\n// palette: SKColor[16] for 4bpp\n// indexedTile: byte[] packed 4bpp data (nibble per pixel)\nvar bmp = new SKBitmap(8, 8, SKColorType.Rgba8888, SKAlphaType.Opaque);\nfor (int i = 0; i < 64; i++) {\n    int nibble = (i % 2 == 0) ? (indexedTile[i/2] >> 4) : (indexedTile[i/2] & 0xF);\n    bmp.SetPixel(i % 8, i / 8, palette[nibble]);\n}\ncanvas.DrawBitmap(bmp, x, y);\n```\n\nThis isn't zero-copy, but it works today. We'll keep this open to track if Skia re-adds indexed color support in a future milestone."
      }
    ]
  }
}
```

</details>
