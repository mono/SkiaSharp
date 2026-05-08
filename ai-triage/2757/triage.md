# Issue Triage Report — #2757

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T21:00:00Z |
| Type | type/bug (0.82 (82%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.82 (82%)) |

**Issue Summary:** Reporter claims SKImage.Encode(PNG) is 3-4x slower than System.Drawing/GDI on Windows; root cause is that SKPngEncoderOptions.Default uses AllFilters + ZLibLevel=6, which is by design in Skia and a known workaround exists.

**Analysis:** The perceived slowness is caused by SKPngEncoderOptions.Default using AllFilters and ZLibLevel=6, which tells the encoder to try all filter types for best compression — this is computationally expensive but produces smaller files. System.Drawing uses different (lower) compression defaults, making the comparison not apples-to-apples. This is Skia/SkiaSharp by-design behavior, not a defect.

**Recommendations:** **close-as-not-a-bug** — The performance difference is caused by Skia's default PNG encoder settings (AllFilters+ZLibLevel=6 vs GDI's lower defaults). This is intentional behavior. A confirmed workaround exists that reduces overhead to ~10-20%. Community analysis concurs. No defect in SkiaSharp itself.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Load a JPEG image with SKBitmap.Decode()
2. Create SKImage from the bitmap
3. Encode to PNG using image.Encode(SKEncodedImageFormat.Png, 100)
4. Compare timing with System.Drawing equivalent

**Environment:** SkiaSharp 2.88.3, Visual Studio, Windows 11

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | performance |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net6.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The default PNG encoder settings (AllFilters, ZLibLevel=6) have not changed; the performance characteristic persists in current releases. |

## Analysis

### Technical Summary

The perceived slowness is caused by SKPngEncoderOptions.Default using AllFilters and ZLibLevel=6, which tells the encoder to try all filter types for best compression — this is computationally expensive but produces smaller files. System.Drawing uses different (lower) compression defaults, making the comparison not apples-to-apples. This is Skia/SkiaSharp by-design behavior, not a defect.

### Rationale

Multiple commenters confirm the 4x slowdown is specific to SKImage.Encode with PNG. Code investigation reveals SKPixmap.Encode(SKWStream, SKEncodedImageFormat.Png) hard-codes SKPngEncoderOptions.Default which sets AllFilters+ZLibLevel=6 — maximum compression effort. A confirmed workaround (SKPngEncoderOptions with FilterFlags=None, ZLibLevel=1) brings performance within 10-20% of GDI. Community comment from @molesmoke correctly notes this is Skia default behavior, not a SkiaSharp defect. No crash or data corruption; workaround exists.

### Key Signals

- "no matter what I encode and how, it always comes out at least 3-4 times slower than Gdi" — **issue body** (Performance gap is consistent; suggests a systemic default setting difference, not an occasional regression.)
- "Found a workaround for now that seems to get much closer in performance. ~10-20% slower so close enough" — **comment #2424337566** (Setting FilterFlags=None, ZLibLevel=1 resolves the performance issue, confirming the default settings are the root cause.)
- "this appears to be Skia behavior rather than a SkiaSharp issue" — **comment #3212168472** (Community analysis confirms the defaults come from Skia; SkiaSharp just exposes them.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPixmap.cs` | 235-246 | direct | SKPixmap.Encode(SKWStream, SKEncodedImageFormat.Png) hard-codes SKPngEncoderOptions.Default, which is AllFilters+ZLibLevel=6 — the most computationally expensive PNG encoding mode. |
| `binding/SkiaSharp/Definitions.cs` | 521-543 | direct | SKPngEncoderOptions.Default is statically initialized to new SKPngEncoderOptions(SKPngEncoderFilterFlags.AllFilters, 6). AllFilters causes the encoder to evaluate every filter combination, which is slow but produces optimally compressed output. |

### Workarounds

- Use SKPixmap.Encode with explicit SKPngEncoderOptions(FilterFlags = SKPngEncoderFilterFlags.None, ZLibLevel = 1) to reduce encoding overhead to within ~10-20% of GDI performance
- Use SKEncodedImageFormat.Webp as an alternative format — WebP often provides faster encoding with better compression ratios than PNG

### Next Questions

- Should SkiaSharp consider exposing a 'fast' PNG encoding preset to make speed-vs-compression trade-off more discoverable?
- Is the quality parameter (100) being silently ignored for PNG since SKPngEncoderOptions.Default is always used regardless of the quality parameter value?

### Resolution Proposals

**Hypothesis:** The performance gap is entirely explained by the default PNG encoder settings. AllFilters+ZLibLevel=6 is optimal for compression ratio but slow; System.Drawing defaults to lower compression and fewer filters.

1. **Use custom SKPngEncoderOptions for speed** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Call pixmap.Encode(stream, new SKPngEncoderOptions(SKPngEncoderFilterFlags.None, 1)) instead of image.Encode(SKEncodedImageFormat.Png, 100) to get ~10-20% overhead vs GDI instead of 3-4x.

```csharp
using var pixmap = image.PeekPixels();
using var ms = new MemoryStream();
pixmap.Encode(ms, new SKPngEncoderOptions(SKPngEncoderFilterFlags.None, 1));
byte[] pngBytes = ms.ToArray();
```
2. **Switch to WebP encoding** — alternative, confidence 0.80 (80%), cost/xs, validated=untested
   - WebP lossless encoding via image.Encode(SKEncodedImageFormat.Webp, 100) typically encodes faster than PNG with AllFilters and produces smaller files.

**Recommended proposal:** Use custom SKPngEncoderOptions for speed

**Why:** Confirmed by multiple community members; keeps PNG format and drops overhead to acceptable levels.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.82 (82%) |
| Reason | The performance difference is caused by Skia's default PNG encoder settings (AllFilters+ZLibLevel=6 vs GDI's lower defaults). This is intentional behavior. A confirmed workaround exists that reduces overhead to ~10-20%. Community analysis concurs. No defect in SkiaSharp itself. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply performance bug, SkiaSharp area, Windows platform, and tenet/performance labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/performance |
| add-comment | medium | 0.85 (85%) | Explain the root cause (PNG default settings) and provide the confirmed workaround | — |
| close-issue | medium | 0.82 (82%) | Close as not a bug — behavior is by design (Skia PNG encoder defaults) | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The performance difference is caused by how SkiaSharp's `SKPngEncoderOptions.Default` is configured: it uses `AllFilters` + `ZLibLevel = 6`, which instructs the PNG encoder to evaluate every filter combination to find the optimal one. This produces highly compressed output but is significantly slower than System.Drawing, which uses lower compression defaults by default.

This is the Skia default behavior, not a defect — the two frameworks are simply not configured the same way out of the box.

**Workaround (confirmed by the community):** Use explicit encoder options to trade compression ratio for speed:

```csharp
using var pixmap = image.PeekPixels();
using var ms = new MemoryStream();
pixmap.Encode(ms, new SKPngEncoderOptions(SKPngEncoderFilterFlags.None, 1));
byte[] pngBytes = ms.ToArray();
```

This brings performance to within ~10-20% of GDI. If file size isn't critical, setting `ZLibLevel = 0` (no compression) is even faster.

Alternatively, WebP lossless encoding (`SKEncodedImageFormat.Webp` with quality 100) often encodes faster than PNG while producing smaller files.

Closing as not a bug since the behavior is by design. Feel free to reopen if you believe the defaults should be changed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2757,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T21:00:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter claims SKImage.Encode(PNG) is 3-4x slower than System.Drawing/GDI on Windows; root cause is that SKPngEncoderOptions.Default uses AllFilters + ZLibLevel=6, which is by design in Skia and a known workaround exists.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
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
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Load a JPEG image with SKBitmap.Decode()",
        "Create SKImage from the bitmap",
        "Encode to PNG using image.Encode(SKEncodedImageFormat.Png, 100)",
        "Compare timing with System.Drawing equivalent"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Visual Studio, Windows 11",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The default PNG encoder settings (AllFilters, ZLibLevel=6) have not changed; the performance characteristic persists in current releases."
    }
  },
  "analysis": {
    "summary": "The perceived slowness is caused by SKPngEncoderOptions.Default using AllFilters and ZLibLevel=6, which tells the encoder to try all filter types for best compression — this is computationally expensive but produces smaller files. System.Drawing uses different (lower) compression defaults, making the comparison not apples-to-apples. This is Skia/SkiaSharp by-design behavior, not a defect.",
    "rationale": "Multiple commenters confirm the 4x slowdown is specific to SKImage.Encode with PNG. Code investigation reveals SKPixmap.Encode(SKWStream, SKEncodedImageFormat.Png) hard-codes SKPngEncoderOptions.Default which sets AllFilters+ZLibLevel=6 — maximum compression effort. A confirmed workaround (SKPngEncoderOptions with FilterFlags=None, ZLibLevel=1) brings performance within 10-20% of GDI. Community comment from @molesmoke correctly notes this is Skia default behavior, not a SkiaSharp defect. No crash or data corruption; workaround exists.",
    "keySignals": [
      {
        "text": "no matter what I encode and how, it always comes out at least 3-4 times slower than Gdi",
        "source": "issue body",
        "interpretation": "Performance gap is consistent; suggests a systemic default setting difference, not an occasional regression."
      },
      {
        "text": "Found a workaround for now that seems to get much closer in performance. ~10-20% slower so close enough",
        "source": "comment #2424337566",
        "interpretation": "Setting FilterFlags=None, ZLibLevel=1 resolves the performance issue, confirming the default settings are the root cause."
      },
      {
        "text": "this appears to be Skia behavior rather than a SkiaSharp issue",
        "source": "comment #3212168472",
        "interpretation": "Community analysis confirms the defaults come from Skia; SkiaSharp just exposes them."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "235-246",
        "finding": "SKPixmap.Encode(SKWStream, SKEncodedImageFormat.Png) hard-codes SKPngEncoderOptions.Default, which is AllFilters+ZLibLevel=6 — the most computationally expensive PNG encoding mode.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "521-543",
        "finding": "SKPngEncoderOptions.Default is statically initialized to new SKPngEncoderOptions(SKPngEncoderFilterFlags.AllFilters, 6). AllFilters causes the encoder to evaluate every filter combination, which is slow but produces optimally compressed output.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use SKPixmap.Encode with explicit SKPngEncoderOptions(FilterFlags = SKPngEncoderFilterFlags.None, ZLibLevel = 1) to reduce encoding overhead to within ~10-20% of GDI performance",
      "Use SKEncodedImageFormat.Webp as an alternative format — WebP often provides faster encoding with better compression ratios than PNG"
    ],
    "nextQuestions": [
      "Should SkiaSharp consider exposing a 'fast' PNG encoding preset to make speed-vs-compression trade-off more discoverable?",
      "Is the quality parameter (100) being silently ignored for PNG since SKPngEncoderOptions.Default is always used regardless of the quality parameter value?"
    ],
    "resolution": {
      "hypothesis": "The performance gap is entirely explained by the default PNG encoder settings. AllFilters+ZLibLevel=6 is optimal for compression ratio but slow; System.Drawing defaults to lower compression and fewer filters.",
      "proposals": [
        {
          "title": "Use custom SKPngEncoderOptions for speed",
          "description": "Call pixmap.Encode(stream, new SKPngEncoderOptions(SKPngEncoderFilterFlags.None, 1)) instead of image.Encode(SKEncodedImageFormat.Png, 100) to get ~10-20% overhead vs GDI instead of 3-4x.",
          "category": "workaround",
          "codeSnippet": "using var pixmap = image.PeekPixels();\nusing var ms = new MemoryStream();\npixmap.Encode(ms, new SKPngEncoderOptions(SKPngEncoderFilterFlags.None, 1));\nbyte[] pngBytes = ms.ToArray();",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Switch to WebP encoding",
          "description": "WebP lossless encoding via image.Encode(SKEncodedImageFormat.Webp, 100) typically encodes faster than PNG with AllFilters and produces smaller files.",
          "category": "alternative",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use custom SKPngEncoderOptions for speed",
      "recommendedReason": "Confirmed by multiple community members; keeps PNG format and drops overhead to acceptable levels."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.82,
      "reason": "The performance difference is caused by Skia's default PNG encoder settings (AllFilters+ZLibLevel=6 vs GDI's lower defaults). This is intentional behavior. A confirmed workaround exists that reduces overhead to ~10-20%. Community analysis concurs. No defect in SkiaSharp itself.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply performance bug, SkiaSharp area, Windows platform, and tenet/performance labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain the root cause (PNG default settings) and provide the confirmed workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the report! The performance difference is caused by how SkiaSharp's `SKPngEncoderOptions.Default` is configured: it uses `AllFilters` + `ZLibLevel = 6`, which instructs the PNG encoder to evaluate every filter combination to find the optimal one. This produces highly compressed output but is significantly slower than System.Drawing, which uses lower compression defaults by default.\n\nThis is the Skia default behavior, not a defect — the two frameworks are simply not configured the same way out of the box.\n\n**Workaround (confirmed by the community):** Use explicit encoder options to trade compression ratio for speed:\n\n```csharp\nusing var pixmap = image.PeekPixels();\nusing var ms = new MemoryStream();\npixmap.Encode(ms, new SKPngEncoderOptions(SKPngEncoderFilterFlags.None, 1));\nbyte[] pngBytes = ms.ToArray();\n```\n\nThis brings performance to within ~10-20% of GDI. If file size isn't critical, setting `ZLibLevel = 0` (no compression) is even faster.\n\nAlternatively, WebP lossless encoding (`SKEncodedImageFormat.Webp` with quality 100) often encodes faster than PNG while producing smaller files.\n\nClosing as not a bug since the behavior is by design. Feel free to reopen if you believe the defaults should be changed."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — behavior is by design (Skia PNG encoder defaults)",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
