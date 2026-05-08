# Issue Triage Report — #2164

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T19:58:33Z |
| Type | type/bug (0.65 (65%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.80 (80%)) |

**Issue Summary:** Reporter claims SKPixmap/SKBitmap Encode quality parameter is inverted for WebP, observing that quality=1 produces more bytes (~640) than quality=100 (~50), misinterpreting lossless vs lossy mode switch at quality=100.

**Analysis:** The quality=100 case in SKPixmap.Encode triggers a lossless WebP encoding path instead of a lossy one. For the reporter's test case (solid filled rectangle), lossless compression produces extremely compact output (~50 bytes) while lossy at quality=1 still incurs format overhead (~640 bytes). This is by-design behavior: quality=100 means lossless, quality 1-99 means lossy. The confusion arises because (a) the reporter tests with a degenerate flat-color image where lossless is unusually efficient, and (b) the API documentation does not make the lossless/lossy mode switch obvious.

**Recommendations:** **close-as-not-a-bug** — The behavior is by-design: quality=100 triggers lossless WebP encoding which is correct and intentional. The reporter's test uses a degenerate flat-color image giving misleading byte-size results. A workaround using SKWebpEncoderOptions directly exists.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/Raster |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create WPF app with SKElement
2. In PaintSurface handler, call pixmap.Encode(SKEncodedImageFormat.Webp, 1) and check byte count
3. Repeat with quality=100
4. Observe quality=1 produces more bytes than quality=100

**Environment:** Version 2.88.1-preview.79, VS 2022 Preview 3, WPF on Windows Pro x64

**Code snippets:**

```csharp
pixmap.Encode(SKEncodedImageFormat.Webp, 1)  // produces ~640 bytes
pixmap.Encode(SKEncodedImageFormat.Webp, 100) // produces ~50 bytes
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Encoding with quality=100 returns ~50 bytes; quality=1 returns ~640 bytes |
| Repro quality | complete |
| Target frameworks | net6.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.1-preview.79 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The quality=100 → lossless mode switch logic remains in the current codebase at SKPixmap.cs lines 241-244. |

## Analysis

### Technical Summary

The quality=100 case in SKPixmap.Encode triggers a lossless WebP encoding path instead of a lossy one. For the reporter's test case (solid filled rectangle), lossless compression produces extremely compact output (~50 bytes) while lossy at quality=1 still incurs format overhead (~640 bytes). This is by-design behavior: quality=100 means lossless, quality 1-99 means lossy. The confusion arises because (a) the reporter tests with a degenerate flat-color image where lossless is unusually efficient, and (b) the API documentation does not make the lossless/lossy mode switch obvious.

### Rationale

The code explicitly handles `quality == 100` as a special case producing lossless WebP (SKPixmap.cs line 241), while all other values produce lossy. This is an intentional design that follows WebP conventions. The test image is a solid-black rectangle, a degenerate case where lossless produces far fewer bytes than lossy. For real photographic images the relationship is reversed. No inversion bug exists; this is a documentation/API clarity issue.

### Key Signals

- "Encoding with quality set to 100 returns around 50 bytes. Encoding with quality set to 1 returns around 640 bytes." — **issue body** (For a solid-color rectangle, lossless (quality=100) is extremely compact; lossy at quality=1 still has format overhead. This is expected, not inverted.)
- "1 is a best quality and 100 is the worst" — **issue body** (Reporter is judging quality by file size, not by visual fidelity. Lossless at quality=100 produces perfect pixel-accurate output — that is better quality, not worse.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPixmap.cs` | 235-246 | direct | Encode(SKWStream, SKEncodedImageFormat, int quality) contains a switch: when format=Webp and quality==100, it creates SKWebpEncoderOptions with Lossless compression and effort=75; otherwise creates Lossy with the provided quality. This confirms the lossless/lossy branch at quality=100. |
| `binding/SkiaSharp/Definitions.cs` | 584-601 | related | SKWebpEncoderOptions default is Lossy at quality=100, confirming that the high-level Encode overload's lossless shortcut at quality=100 is a deliberate deviation from the default options struct. |

### Workarounds

- To explicitly use lossless WebP use `pixmap.Encode(new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossless, 75))`
- To explicitly use lossy WebP at specific quality, use `pixmap.Encode(new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, quality))` directly
- To compare quality by visual output rather than file size, view the decoded image rather than measuring bytes

### Resolution Proposals

**Hypothesis:** The quality=100 → lossless shortcut in SKPixmap.Encode is by-design but poorly documented. The API surface should clarify that quality=100 triggers lossless mode for WebP. No code fix needed; documentation update would address the confusion.

1. **Add XML doc comment clarifying quality=100 lossless behavior** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - Add a <remarks> element to SKPixmap.Encode and SKBitmap.Encode documenting that quality=100 for WebP produces lossless output, while quality 1-99 produces lossy output.
2. **Workaround: Use SKWebpEncoderOptions directly** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Use the typed overload `pixmap.Encode(new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, quality))` to explicitly control lossless vs lossy without ambiguity.

**Recommended proposal:** Add XML doc comment clarifying quality=100 lossless behavior

**Why:** The behavior is by design but needs documentation to prevent recurring confusion.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.80 (80%) |
| Reason | The behavior is by-design: quality=100 triggers lossless WebP encoding which is correct and intentional. The reporter's test uses a degenerate flat-color image giving misleading byte-size results. A workaround using SKWebpEncoderOptions directly exists. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug type, core area, Windows platform labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic |
| add-comment | high | 0.82 (82%) | Explain lossless vs lossy WebP behavior and provide workaround | — |
| close-issue | medium | 0.80 (80%) | Close as not a bug — by-design lossless/lossy mode switch | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report!

The behavior you're observing is actually by design. When you pass `quality=100` to `Encode` with `SKEncodedImageFormat.Webp`, SkiaSharp switches to **lossless** WebP encoding. For all other quality values (1–99), it uses **lossy** encoding at that quality level.

For your test case (a solid black rectangle), lossless compression is extremely efficient because the image has no complexity — hence the tiny ~50 byte output. Lossy encoding at `quality=1` still incurs WebP format overhead, which is why it produces more bytes on a flat image.

For real photographic images the expectation holds: `quality=100` (lossless) → much larger file; `quality=1` (very lossy) → tiny file.

If you want explicit control over lossless vs lossy, use the typed overload:
```csharp
// Explicitly lossy at quality 80
pixmap.Encode(new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, 80));

// Explicitly lossless
pixmap.Encode(new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossless, 75));
```

We agree the documentation should be clearer about this behavior.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2164,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T19:58:33Z"
  },
  "summary": "Reporter claims SKPixmap/SKBitmap Encode quality parameter is inverted for WebP, observing that quality=1 produces more bytes (~640) than quality=100 (~50), misinterpreting lossless vs lossy mode switch at quality=100.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.65
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Raster"
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
      "errorMessage": "Encoding with quality=100 returns ~50 bytes; quality=1 returns ~640 bytes",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net6.0-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create WPF app with SKElement",
        "In PaintSurface handler, call pixmap.Encode(SKEncodedImageFormat.Webp, 1) and check byte count",
        "Repeat with quality=100",
        "Observe quality=1 produces more bytes than quality=100"
      ],
      "codeSnippets": [
        "pixmap.Encode(SKEncodedImageFormat.Webp, 1)  // produces ~640 bytes\npixmap.Encode(SKEncodedImageFormat.Webp, 100) // produces ~50 bytes"
      ],
      "environmentDetails": "Version 2.88.1-preview.79, VS 2022 Preview 3, WPF on Windows Pro x64"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.1-preview.79"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The quality=100 → lossless mode switch logic remains in the current codebase at SKPixmap.cs lines 241-244."
    }
  },
  "analysis": {
    "summary": "The quality=100 case in SKPixmap.Encode triggers a lossless WebP encoding path instead of a lossy one. For the reporter's test case (solid filled rectangle), lossless compression produces extremely compact output (~50 bytes) while lossy at quality=1 still incurs format overhead (~640 bytes). This is by-design behavior: quality=100 means lossless, quality 1-99 means lossy. The confusion arises because (a) the reporter tests with a degenerate flat-color image where lossless is unusually efficient, and (b) the API documentation does not make the lossless/lossy mode switch obvious.",
    "rationale": "The code explicitly handles `quality == 100` as a special case producing lossless WebP (SKPixmap.cs line 241), while all other values produce lossy. This is an intentional design that follows WebP conventions. The test image is a solid-black rectangle, a degenerate case where lossless produces far fewer bytes than lossy. For real photographic images the relationship is reversed. No inversion bug exists; this is a documentation/API clarity issue.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "235-246",
        "finding": "Encode(SKWStream, SKEncodedImageFormat, int quality) contains a switch: when format=Webp and quality==100, it creates SKWebpEncoderOptions with Lossless compression and effort=75; otherwise creates Lossy with the provided quality. This confirms the lossless/lossy branch at quality=100.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Definitions.cs",
        "lines": "584-601",
        "finding": "SKWebpEncoderOptions default is Lossy at quality=100, confirming that the high-level Encode overload's lossless shortcut at quality=100 is a deliberate deviation from the default options struct.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Encoding with quality set to 100 returns around 50 bytes. Encoding with quality set to 1 returns around 640 bytes.",
        "source": "issue body",
        "interpretation": "For a solid-color rectangle, lossless (quality=100) is extremely compact; lossy at quality=1 still has format overhead. This is expected, not inverted."
      },
      {
        "text": "1 is a best quality and 100 is the worst",
        "source": "issue body",
        "interpretation": "Reporter is judging quality by file size, not by visual fidelity. Lossless at quality=100 produces perfect pixel-accurate output — that is better quality, not worse."
      }
    ],
    "workarounds": [
      "To explicitly use lossless WebP use `pixmap.Encode(new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossless, 75))`",
      "To explicitly use lossy WebP at specific quality, use `pixmap.Encode(new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, quality))` directly",
      "To compare quality by visual output rather than file size, view the decoded image rather than measuring bytes"
    ],
    "resolution": {
      "hypothesis": "The quality=100 → lossless shortcut in SKPixmap.Encode is by-design but poorly documented. The API surface should clarify that quality=100 triggers lossless mode for WebP. No code fix needed; documentation update would address the confusion.",
      "proposals": [
        {
          "title": "Add XML doc comment clarifying quality=100 lossless behavior",
          "description": "Add a <remarks> element to SKPixmap.Encode and SKBitmap.Encode documenting that quality=100 for WebP produces lossless output, while quality 1-99 produces lossy output.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Workaround: Use SKWebpEncoderOptions directly",
          "description": "Use the typed overload `pixmap.Encode(new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, quality))` to explicitly control lossless vs lossy without ambiguity.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Add XML doc comment clarifying quality=100 lossless behavior",
      "recommendedReason": "The behavior is by design but needs documentation to prevent recurring confusion."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.8,
      "reason": "The behavior is by-design: quality=100 triggers lossless WebP encoding which is correct and intentional. The reporter's test uses a degenerate flat-color image giving misleading byte-size results. A workaround using SKWebpEncoderOptions directly exists.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug type, core area, Windows platform labels",
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
        "description": "Explain lossless vs lossy WebP behavior and provide workaround",
        "risk": "high",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report!\n\nThe behavior you're observing is actually by design. When you pass `quality=100` to `Encode` with `SKEncodedImageFormat.Webp`, SkiaSharp switches to **lossless** WebP encoding. For all other quality values (1–99), it uses **lossy** encoding at that quality level.\n\nFor your test case (a solid black rectangle), lossless compression is extremely efficient because the image has no complexity — hence the tiny ~50 byte output. Lossy encoding at `quality=1` still incurs WebP format overhead, which is why it produces more bytes on a flat image.\n\nFor real photographic images the expectation holds: `quality=100` (lossless) → much larger file; `quality=1` (very lossy) → tiny file.\n\nIf you want explicit control over lossless vs lossy, use the typed overload:\n```csharp\n// Explicitly lossy at quality 80\npixmap.Encode(new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, 80));\n\n// Explicitly lossless\npixmap.Encode(new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossless, 75));\n```\n\nWe agree the documentation should be clearer about this behavior."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — by-design lossless/lossy mode switch",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
