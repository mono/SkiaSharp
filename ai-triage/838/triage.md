# Issue Triage Report — #838

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T12:31:25Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** On Linux, SKPaint.MeasureText and SKFont.MeasureText return integer-pixel-rounded widths instead of fractional subpixel widths, producing different measurements than Windows for identical text, font, and size combinations.

**Analysis:** Linux Skia defaults to integer-pixel glyph positioning rather than subpixel positioning, causing MeasureText to return rounded integer widths. The fix is to set SKFont.Subpixel = true (legacy: paint.SubpixelText = true), which enables fractional glyph advances matching Windows behavior. A secondary bug exists where FromStream loading on Linux may also cause measurement differences due to HarfBuzz BlobExtensions GetMemoryBase returning IntPtr.Zero.

**Recommendations:** **needs-investigation** — Confirmed platform-specific wrong-output bug with a known workaround. Root cause (Skia Linux default glyph positioning) is understood but the fix path (change SkiaSharp default vs document workaround) needs maintainer decision.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug, os/Linux, area/SkiaSharp, tenet/compatibility, triage/triaged |

## Evidence

### Reproduction

1. Create an SKPaint with Arial Bold, TextSize=9 on Linux
2. Call paint.MeasureText("Locatio", ref rect)
3. Observe result is 34 on Linux vs 32.00098 on Windows

**Environment:** SkiaSharp 1.59.3, Ubuntu 18.04, .NET Core 2.1, MonoDevelop

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/838#issuecomment-490852626 — Contributor (Gillibald) identified SubpixelText as root cause and suggested paint.SubpixelText = true workaround
- https://github.com/mono/SkiaSharp/issues/838#issuecomment-506943075 — Gillibald identified secondary bug: GetMemoryBase returns IntPtr.Zero on Linux in BlobExtensions.cs

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | MeasureText returns integer-rounded width on Linux (e.g. 34 instead of 32.00098) |
| Repro quality | complete |
| Target frameworks | netcoreapp2.1 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.59.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SKFont.Subpixel property (formerly SubpixelText) is still false by default and not documented as required for cross-platform consistency. Multiple users reported the issue persists in newer versions (2020, 2022). |

## Analysis

### Technical Summary

Linux Skia defaults to integer-pixel glyph positioning rather than subpixel positioning, causing MeasureText to return rounded integer widths. The fix is to set SKFont.Subpixel = true (legacy: paint.SubpixelText = true), which enables fractional glyph advances matching Windows behavior. A secondary bug exists where FromStream loading on Linux may also cause measurement differences due to HarfBuzz BlobExtensions GetMemoryBase returning IntPtr.Zero.

### Rationale

The discrepancy is real: Linux Skia uses pixel-grid-aligned glyph advances by default, while Windows Skia uses subpixel advances (influenced by DirectWrite). Setting Subpixel=true resolves the main case. The issue is still open and multiple users continue to hit it, suggesting the workaround is not well-known or that there are additional cases.

### Key Signals

- "width of the text was calculated in Linux is invalid - Windows: 10.69922, Linux: 6" — **issue comment** (Integer rounding of subpixel advances causes significant measurement error, especially for special characters in symbol fonts)
- "if paint.SubpixelText was set as true, then width of the text size was calculated as expected in Linux OS" — **issue comment #490878984** (Confirms root cause: Linux defaults SubpixelText to false; enabling it aligns measurements)
- "GetMemoryBase returns IntPtr.Zero on Linux" — **issue comment #506943075** (Secondary bug in HarfBuzz blob loading on Linux, though the current BlobExtensions.cs code handles this via fallback allocation)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPaint.cs` | 98-102 | direct | SubpixelText is marked [Obsolete] and delegates to SKFont.Subpixel — property exists, defaults to false, no platform-conditional default |
| `binding/SkiaSharp/SKFont.cs` | 47-50 | direct | SKFont.Subpixel property wraps sk_font_is_subpixel/sk_font_set_subpixel — no platform-specific default initialization; Skia native defaults to false on Linux |
| `binding/SkiaSharp/SKFont.cs` | 315-323 | direct | MeasureText delegates to sk_font_measure_text_no_return — the measurement is directly driven by native Skia, so the integer rounding is in Skia's native glyph advance computation when subpixel is false |
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/BlobExtensions.cs` | 21-32 | related | ToHarfBuzzBlob handles GetMemoryBase==IntPtr.Zero via fallback Marshal.AllocCoTaskMem path — this was the secondary Linux issue; current code handles it safely |

### Workarounds

- Set SKFont.Subpixel = true (modern API) or paint.SubpixelText = true (legacy, obsolete) before measuring text on Linux to enable fractional subpixel glyph advances
- Use SKTypeface.FromData(SKData.Create(stream)) instead of SKTypeface.FromStream() when loading font files from streams on Linux

### Next Questions

- Should SkiaSharp set Subpixel=true by default on all platforms for cross-platform consistency?
- Does the issue persist on current SkiaSharp versions (3.x) when Subpixel is true?
- Is there a platform-independent way to get consistent text metrics (e.g., using LinearMetrics instead)?

### Resolution Proposals

**Hypothesis:** Skia native defaults to integer-pixel glyph positioning on Linux. SKFont.Subpixel=true enables fractional advances and restores cross-platform consistency. Whether SkiaSharp should default this to true on all platforms is a design decision.

1. **Set SKFont.Subpixel = true in application code** — workaround, confidence 0.88 (88%), cost/xs, validated=yes
   - Caller sets Subpixel=true on the font or paint before measurement. Immediate workaround with no library change needed.

```csharp
var font = new SKFont(typeface, size);
font.Subpixel = true;
float width = font.MeasureText("Locatio");
```
2. **Default SKFont.Subpixel to true in SkiaSharp** — fix, confidence 0.65 (65%), cost/s, validated=untested
   - Change the SKFont default so Subpixel=true across all platforms, matching Windows behavior. This would be a behavior change but improves cross-platform parity.

**Recommended proposal:** Set SKFont.Subpixel = true in application code

**Why:** Immediately actionable workaround. The design decision to change the default requires broader discussion.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Confirmed platform-specific wrong-output bug with a known workaround. Root cause (Skia Linux default glyph positioning) is understood but the fix path (change SkiaSharp default vs document workaround) needs maintainer decision. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/SkiaSharp, os/Linux, tenet/compatibility labels | labels=type/bug, area/SkiaSharp, os/Linux, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Post workaround and analysis for the subpixel measurement difference | — |

**Comment draft for `add-comment`:**

```markdown
**Root cause:** On Linux, Skia defaults to integer-pixel glyph positioning rather than subpixel positioning, causing `MeasureText` to return rounded widths instead of fractional values.

**Workaround:** Set `SKFont.Subpixel = true` before measuring text:

```csharp
var font = new SKFont(typeface, size);
font.Subpixel = true;
float width = font.MeasureText("Locatio");
```

Or with the legacy `SKPaint` API (obsolete but still works):
```csharp
paint.SubpixelText = true;
```

This enables fractional glyph advances on Linux, matching the behavior seen on Windows.

Note: If you are also loading fonts from a stream, prefer `SKTypeface.FromData(SKData.Create(stream))` over `SKTypeface.FromStream()` for more reliable behavior on Linux.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 838,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T12:31:25Z",
    "currentLabels": [
      "type/bug",
      "os/Linux",
      "area/SkiaSharp",
      "tenet/compatibility",
      "triage/triaged"
    ]
  },
  "summary": "On Linux, SKPaint.MeasureText and SKFont.MeasureText return integer-pixel-rounded widths instead of fractional subpixel widths, producing different measurements than Windows for identical text, font, and size combinations.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "MeasureText returns integer-rounded width on Linux (e.g. 34 instead of 32.00098)",
      "reproQuality": "complete",
      "targetFrameworks": [
        "netcoreapp2.1"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKPaint with Arial Bold, TextSize=9 on Linux",
        "Call paint.MeasureText(\"Locatio\", ref rect)",
        "Observe result is 34 on Linux vs 32.00098 on Windows"
      ],
      "environmentDetails": "SkiaSharp 1.59.3, Ubuntu 18.04, .NET Core 2.1, MonoDevelop",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/838#issuecomment-490852626",
          "description": "Contributor (Gillibald) identified SubpixelText as root cause and suggested paint.SubpixelText = true workaround"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/838#issuecomment-506943075",
          "description": "Gillibald identified secondary bug: GetMemoryBase returns IntPtr.Zero on Linux in BlobExtensions.cs"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.59.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The SKFont.Subpixel property (formerly SubpixelText) is still false by default and not documented as required for cross-platform consistency. Multiple users reported the issue persists in newer versions (2020, 2022)."
    }
  },
  "analysis": {
    "summary": "Linux Skia defaults to integer-pixel glyph positioning rather than subpixel positioning, causing MeasureText to return rounded integer widths. The fix is to set SKFont.Subpixel = true (legacy: paint.SubpixelText = true), which enables fractional glyph advances matching Windows behavior. A secondary bug exists where FromStream loading on Linux may also cause measurement differences due to HarfBuzz BlobExtensions GetMemoryBase returning IntPtr.Zero.",
    "rationale": "The discrepancy is real: Linux Skia uses pixel-grid-aligned glyph advances by default, while Windows Skia uses subpixel advances (influenced by DirectWrite). Setting Subpixel=true resolves the main case. The issue is still open and multiple users continue to hit it, suggesting the workaround is not well-known or that there are additional cases.",
    "keySignals": [
      {
        "text": "width of the text was calculated in Linux is invalid - Windows: 10.69922, Linux: 6",
        "source": "issue comment",
        "interpretation": "Integer rounding of subpixel advances causes significant measurement error, especially for special characters in symbol fonts"
      },
      {
        "text": "if paint.SubpixelText was set as true, then width of the text size was calculated as expected in Linux OS",
        "source": "issue comment #490878984",
        "interpretation": "Confirms root cause: Linux defaults SubpixelText to false; enabling it aligns measurements"
      },
      {
        "text": "GetMemoryBase returns IntPtr.Zero on Linux",
        "source": "issue comment #506943075",
        "interpretation": "Secondary bug in HarfBuzz blob loading on Linux, though the current BlobExtensions.cs code handles this via fallback allocation"
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "98-102",
        "finding": "SubpixelText is marked [Obsolete] and delegates to SKFont.Subpixel — property exists, defaults to false, no platform-conditional default",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "lines": "47-50",
        "finding": "SKFont.Subpixel property wraps sk_font_is_subpixel/sk_font_set_subpixel — no platform-specific default initialization; Skia native defaults to false on Linux",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "lines": "315-323",
        "finding": "MeasureText delegates to sk_font_measure_text_no_return — the measurement is directly driven by native Skia, so the integer rounding is in Skia's native glyph advance computation when subpixel is false",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/BlobExtensions.cs",
        "lines": "21-32",
        "finding": "ToHarfBuzzBlob handles GetMemoryBase==IntPtr.Zero via fallback Marshal.AllocCoTaskMem path — this was the secondary Linux issue; current code handles it safely",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Set SKFont.Subpixel = true (modern API) or paint.SubpixelText = true (legacy, obsolete) before measuring text on Linux to enable fractional subpixel glyph advances",
      "Use SKTypeface.FromData(SKData.Create(stream)) instead of SKTypeface.FromStream() when loading font files from streams on Linux"
    ],
    "nextQuestions": [
      "Should SkiaSharp set Subpixel=true by default on all platforms for cross-platform consistency?",
      "Does the issue persist on current SkiaSharp versions (3.x) when Subpixel is true?",
      "Is there a platform-independent way to get consistent text metrics (e.g., using LinearMetrics instead)?"
    ],
    "resolution": {
      "hypothesis": "Skia native defaults to integer-pixel glyph positioning on Linux. SKFont.Subpixel=true enables fractional advances and restores cross-platform consistency. Whether SkiaSharp should default this to true on all platforms is a design decision.",
      "proposals": [
        {
          "title": "Set SKFont.Subpixel = true in application code",
          "description": "Caller sets Subpixel=true on the font or paint before measurement. Immediate workaround with no library change needed.",
          "category": "workaround",
          "codeSnippet": "var font = new SKFont(typeface, size);\nfont.Subpixel = true;\nfloat width = font.MeasureText(\"Locatio\");",
          "confidence": 0.88,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Default SKFont.Subpixel to true in SkiaSharp",
          "description": "Change the SKFont default so Subpixel=true across all platforms, matching Windows behavior. This would be a behavior change but improves cross-platform parity.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Set SKFont.Subpixel = true in application code",
      "recommendedReason": "Immediately actionable workaround. The design decision to change the default requires broader discussion."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Confirmed platform-specific wrong-output bug with a known workaround. Root cause (Skia Linux default glyph positioning) is understood but the fix path (change SkiaSharp default vs document workaround) needs maintainer decision.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Linux, tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Linux",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post workaround and analysis for the subpixel measurement difference",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "**Root cause:** On Linux, Skia defaults to integer-pixel glyph positioning rather than subpixel positioning, causing `MeasureText` to return rounded widths instead of fractional values.\n\n**Workaround:** Set `SKFont.Subpixel = true` before measuring text:\n\n```csharp\nvar font = new SKFont(typeface, size);\nfont.Subpixel = true;\nfloat width = font.MeasureText(\"Locatio\");\n```\n\nOr with the legacy `SKPaint` API (obsolete but still works):\n```csharp\npaint.SubpixelText = true;\n```\n\nThis enables fractional glyph advances on Linux, matching the behavior seen on Windows.\n\nNote: If you are also loading fonts from a stream, prefer `SKTypeface.FromData(SKData.Create(stream))` over `SKTypeface.FromStream()` for more reliable behavior on Linux."
      }
    ]
  }
}
```

</details>
