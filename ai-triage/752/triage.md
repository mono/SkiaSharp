# Issue Triage Report — #752

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T16:38:58Z |
| Type | type/bug (0.72 (72%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.78 (78%)) |

**Issue Summary:** SKPaint.MeasureText returns different numeric values on Windows vs Linux when measuring the same string with a Calibri Bold font at size 19, due to platform-specific font manager resolution and shaping backend differences.

**Analysis:** SKPaint.MeasureText (now delegating to SKFont.MeasureText) produces different results on Windows vs Linux because: (1) SKTypeface.FromFamilyName calls sk_typeface_create_from_name which queries the OS font manager — DirectWrite on Windows vs FontConfig on Linux — potentially resolving to a different actual font file even when both have Calibri installed; (2) even with the same font loaded from a stream, different shaping backends (DirectWrite vs FreeType) produce slightly different glyph advance metrics. This is expected cross-platform behavior, not a SkiaSharp defect.

**Recommendations:** **close-as-not-a-bug** — Cross-platform font measurement differences are expected due to platform-specific font manager resolution (DirectWrite vs FontConfig) and shaping backend differences. Maintainer and community comments confirm this is by-design. Workaround is documented. The behavior is not a SkiaSharp defect.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic, os/Linux |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | status/needs-attention |

## Evidence

### Reproduction

1. Create SKPaint with Calibri Bold typeface loaded via SKTypeface.FromFamilyName
2. Set TextSize = 19 and IsAntialias = true
3. Call paint.MeasureText("Progressive Report Template", ref rect)
4. Compare result on Windows vs Linux

**Environment:** SkiaSharp 1.59.3, Visual Studio, Windows vs Linux container

**Related issues:** #707

**Repository links:**
- https://github.com/mono/SkiaSharp/files/2748183/Calibri.zip — Calibri font file used in the repro
- https://github.com/markusrt/SkiaSharpMeasureText — Community test repo (2025) showing font measurement differences persist even when loading font via stream

**Code snippets:**

```csharp
SKPaint paint = new SKPaint();
paint.Typeface = SKTypeface.FromFamilyName("Calibri", SKTypefaceStyle.Bold);
paint.IsAntialias = true;
paint.TextSize = 19;
SKRect rect = new SKRect();
float width = paint.MeasureText("Progressive Report Template", ref rect);
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Windows: 228.213379 vs Linux: 231 width for same text |
| Repro quality | complete |
| Target frameworks | net471 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.59.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Recent comments from 2024-2025 confirm the issue persists in newer versions. The underlying platform font manager and shaping architecture has not changed. |

## Analysis

### Technical Summary

SKPaint.MeasureText (now delegating to SKFont.MeasureText) produces different results on Windows vs Linux because: (1) SKTypeface.FromFamilyName calls sk_typeface_create_from_name which queries the OS font manager — DirectWrite on Windows vs FontConfig on Linux — potentially resolving to a different actual font file even when both have Calibri installed; (2) even with the same font loaded from a stream, different shaping backends (DirectWrite vs FreeType) produce slightly different glyph advance metrics. This is expected cross-platform behavior, not a SkiaSharp defect.

### Rationale

The issue describes a cross-platform measurement discrepancy rooted in platform-specific font resolution and shaping. SKTypeface.FromFamilyName uses sk_typeface_create_from_name which delegates to the OS font manager, and SKPaint.MeasureText (deprecated) delegates to SKFont.MeasureText which calls into the native Skia text measurement path. Community comments and maintainer responses confirm this is by-design behavior. A 2025 community investigation (markusrt) showed differences persist even with font loaded via stream, confirming the shaping backend is involved. The workaround (load from stream + use HarfBuzz shaping) is documented in the comments.

### Key Signals

- "Values returned by measureText in linux container and in windows are not same." — **issue body** (Cross-platform measurement divergence due to different font stacks.)
- "FromFamilyName asks the current font manager for a typeface that is matching the name. This will be an installed font and it can be any font that relates to the specified name. On Linux this can be an alias." — **comment by Gillibald (2024-10-30)** (Confirms root cause: platform font manager resolution differs, and Linux FontConfig may return an alias or different metric.)
- "To be sure to get the same rendering on all platforms you need to load a typeface via stream." — **comment by Gillibald (2024-10-30)** (Establishes the known workaround for consistent font selection across platforms.)
- "Font loaded via Resource - Tests on fonts show that the expected font is used both on Windows and Linux - Difference in calculation is always on width" — **comment by markusrt (2025-08-04)** (Even with same font file loaded via stream, width measurement differs — confirms shaping engine difference is the underlying cause.)
- "My two-cents is that this is the expected outcome. The font rendering backend is platform dependent." — **comment by molesmoke (2025-08-04)** (Community consensus supports close-as-not-a-bug.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 60-72 | direct | FromFamilyName(string, SKFontStyle) calls sk_typeface_create_from_name via SkiaApi, which delegates to the platform font manager. On Windows this is DirectWrite; on Linux it is FontConfig. The same family name can resolve to different font files or metrics tables on each platform. |
| `binding/SkiaSharp/SKPaint.cs` | — | direct | SKPaint.MeasureText is marked [Obsolete] and delegates to SKFont.MeasureText(). The measurement path itself is not platform-specific in the C# wrapper — divergence originates in the native Skia layer's interaction with platform shaping backends. |
| `binding/SkiaSharp/SKFont.cs` | — | related | SKFont.MeasureText delegates to the native sk_font_measure_text C API, which internally uses the platform shaping engine (DirectWrite on Windows, FreeType/HarfBuzz on Linux) for glyph advance metrics. |

### Workarounds

- Load the typeface from a stream or file instead of by family name: var tf = SKTypeface.FromStream(stream); This ensures the same font data is used across platforms, though shaping metrics may still differ slightly between DirectWrite and FreeType.
- Use HarfBuzz (via SkiaSharp.HarfBuzz or RichTextKit) for cross-platform text shaping, which provides consistent glyph advances regardless of the OS font stack.
- Accept platform-specific measurement variance and use relative/flexible layouts instead of hardcoded pixel measurements.

### Next Questions

- Does measurement still differ after loading the exact same font file via SKTypeface.FromStream on both platforms?
- Does HarfBuzz text shaping produce identical glyph advances on Windows and Linux with the same font file?

### Resolution Proposals

**Hypothesis:** The measurement difference is caused by (1) different font manager resolution via FromFamilyName on Windows (DirectWrite) vs Linux (FontConfig), and (2) different low-level glyph advance calculations in the platform shaping backends. This is expected cross-platform behavior.

1. **Load typeface from stream/file for consistent font selection** — workaround, confidence 0.85 (85%), cost/xs, validated=untested
   - Replace SKTypeface.FromFamilyName with SKTypeface.FromStream() or SKTypeface.FromFile() to guarantee the same font data is used on all platforms. This removes the platform font manager variable.
2. **Use HarfBuzz for cross-platform text shaping** — alternative, confidence 0.80 (80%), cost/s, validated=untested
   - Use SkiaSharp.HarfBuzz or RichTextKit for text measurement. HarfBuzz provides consistent shaping results independent of the OS font stack.

**Recommended proposal:** Load typeface from stream/file for consistent font selection

**Why:** Smallest change for the user, addresses the font resolution variable. If metrics still differ slightly after this, the HarfBuzz approach resolves the shaping backend variable too.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.78 (78%) |
| Reason | Cross-platform font measurement differences are expected due to platform-specific font manager resolution (DirectWrite vs FontConfig) and shaping backend differences. Maintainer and community comments confirm this is by-design. Workaround is documented. The behavior is not a SkiaSharp defect. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp, os/Windows-Classic, os/Linux, tenet/compatibility labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, os/Linux, tenet/compatibility |
| add-comment | high | 0.78 (78%) | Post explanation of root cause and workarounds, then close as not-a-bug | — |
| close-issue | medium | 0.78 (78%) | Close as not a bug — platform-specific font shaping behavior | stateReason=not_planned |
| link-related | low | 0.95 (95%) | Cross-reference related issue #707 — same cross-platform MeasureText discrepancy | linkedIssue=#707 |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and the font files. After investigating the code and community follow-ups, this measurement difference is expected cross-platform behavior:

**Root cause:**
1. `SKTypeface.FromFamilyName("Calibri", ...)` calls into the OS font manager — **DirectWrite on Windows** and **FontConfig on Linux**. Even with the same Calibri font installed, each platform may resolve it differently.
2. Even when the same font data is loaded from a stream (as confirmed by follow-up testing in 2025), the low-level glyph advance metrics are computed by **platform shaping backends** (DirectWrite on Windows vs FreeType/HarfBuzz on Linux) which can produce slightly different results.

**Workarounds:**

1. **Load from stream for consistent font selection** — embed the font as a resource and use `SKTypeface.FromStream()` to bypass the OS font manager:
   ```csharp
   using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("YourApp.Calibri.ttf");
   using var tf = SKTypeface.FromStream(stream);
   paint.Typeface = tf;
   ```
   Note: shaping backend differences may still cause minor metric variations.

2. **Use HarfBuzz or RichTextKit for cross-platform text shaping** — [RichTextKit](https://github.com/toptensoftware/RichTextKit) uses HarfBuzz under the hood and provides consistent shaping results across platforms.

3. **Design for metric variance** — if pixel-perfect cross-platform layout consistency is required, use flexible/relative layouts rather than hardcoded measurements.

Since SkiaSharp delegates to platform shaping for text metrics, there is no fix possible at the SkiaSharp layer. Closing as not-a-bug.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 752,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T16:38:58Z",
    "currentLabels": [
      "status/needs-attention"
    ]
  },
  "summary": "SKPaint.MeasureText returns different numeric values on Windows vs Linux when measuring the same string with a Calibri Bold font at size 19, due to platform-specific font manager resolution and shaping backend differences.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.72
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic",
      "os/Linux"
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
      "errorMessage": "Windows: 228.213379 vs Linux: 231 width for same text",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net471"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create SKPaint with Calibri Bold typeface loaded via SKTypeface.FromFamilyName",
        "Set TextSize = 19 and IsAntialias = true",
        "Call paint.MeasureText(\"Progressive Report Template\", ref rect)",
        "Compare result on Windows vs Linux"
      ],
      "codeSnippets": [
        "SKPaint paint = new SKPaint();\npaint.Typeface = SKTypeface.FromFamilyName(\"Calibri\", SKTypefaceStyle.Bold);\npaint.IsAntialias = true;\npaint.TextSize = 19;\nSKRect rect = new SKRect();\nfloat width = paint.MeasureText(\"Progressive Report Template\", ref rect);"
      ],
      "environmentDetails": "SkiaSharp 1.59.3, Visual Studio, Windows vs Linux container",
      "relatedIssues": [
        707
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/2748183/Calibri.zip",
          "description": "Calibri font file used in the repro"
        },
        {
          "url": "https://github.com/markusrt/SkiaSharpMeasureText",
          "description": "Community test repo (2025) showing font measurement differences persist even when loading font via stream"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.59.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Recent comments from 2024-2025 confirm the issue persists in newer versions. The underlying platform font manager and shaping architecture has not changed."
    }
  },
  "analysis": {
    "summary": "SKPaint.MeasureText (now delegating to SKFont.MeasureText) produces different results on Windows vs Linux because: (1) SKTypeface.FromFamilyName calls sk_typeface_create_from_name which queries the OS font manager — DirectWrite on Windows vs FontConfig on Linux — potentially resolving to a different actual font file even when both have Calibri installed; (2) even with the same font loaded from a stream, different shaping backends (DirectWrite vs FreeType) produce slightly different glyph advance metrics. This is expected cross-platform behavior, not a SkiaSharp defect.",
    "rationale": "The issue describes a cross-platform measurement discrepancy rooted in platform-specific font resolution and shaping. SKTypeface.FromFamilyName uses sk_typeface_create_from_name which delegates to the OS font manager, and SKPaint.MeasureText (deprecated) delegates to SKFont.MeasureText which calls into the native Skia text measurement path. Community comments and maintainer responses confirm this is by-design behavior. A 2025 community investigation (markusrt) showed differences persist even with font loaded via stream, confirming the shaping backend is involved. The workaround (load from stream + use HarfBuzz shaping) is documented in the comments.",
    "keySignals": [
      {
        "text": "Values returned by measureText in linux container and in windows are not same.",
        "source": "issue body",
        "interpretation": "Cross-platform measurement divergence due to different font stacks."
      },
      {
        "text": "FromFamilyName asks the current font manager for a typeface that is matching the name. This will be an installed font and it can be any font that relates to the specified name. On Linux this can be an alias.",
        "source": "comment by Gillibald (2024-10-30)",
        "interpretation": "Confirms root cause: platform font manager resolution differs, and Linux FontConfig may return an alias or different metric."
      },
      {
        "text": "To be sure to get the same rendering on all platforms you need to load a typeface via stream.",
        "source": "comment by Gillibald (2024-10-30)",
        "interpretation": "Establishes the known workaround for consistent font selection across platforms."
      },
      {
        "text": "Font loaded via Resource - Tests on fonts show that the expected font is used both on Windows and Linux - Difference in calculation is always on width",
        "source": "comment by markusrt (2025-08-04)",
        "interpretation": "Even with same font file loaded via stream, width measurement differs — confirms shaping engine difference is the underlying cause."
      },
      {
        "text": "My two-cents is that this is the expected outcome. The font rendering backend is platform dependent.",
        "source": "comment by molesmoke (2025-08-04)",
        "interpretation": "Community consensus supports close-as-not-a-bug."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "60-72",
        "finding": "FromFamilyName(string, SKFontStyle) calls sk_typeface_create_from_name via SkiaApi, which delegates to the platform font manager. On Windows this is DirectWrite; on Linux it is FontConfig. The same family name can resolve to different font files or metrics tables on each platform.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "finding": "SKPaint.MeasureText is marked [Obsolete] and delegates to SKFont.MeasureText(). The measurement path itself is not platform-specific in the C# wrapper — divergence originates in the native Skia layer's interaction with platform shaping backends.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFont.cs",
        "finding": "SKFont.MeasureText delegates to the native sk_font_measure_text C API, which internally uses the platform shaping engine (DirectWrite on Windows, FreeType/HarfBuzz on Linux) for glyph advance metrics.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Load the typeface from a stream or file instead of by family name: var tf = SKTypeface.FromStream(stream); This ensures the same font data is used across platforms, though shaping metrics may still differ slightly between DirectWrite and FreeType.",
      "Use HarfBuzz (via SkiaSharp.HarfBuzz or RichTextKit) for cross-platform text shaping, which provides consistent glyph advances regardless of the OS font stack.",
      "Accept platform-specific measurement variance and use relative/flexible layouts instead of hardcoded pixel measurements."
    ],
    "resolution": {
      "hypothesis": "The measurement difference is caused by (1) different font manager resolution via FromFamilyName on Windows (DirectWrite) vs Linux (FontConfig), and (2) different low-level glyph advance calculations in the platform shaping backends. This is expected cross-platform behavior.",
      "proposals": [
        {
          "title": "Load typeface from stream/file for consistent font selection",
          "description": "Replace SKTypeface.FromFamilyName with SKTypeface.FromStream() or SKTypeface.FromFile() to guarantee the same font data is used on all platforms. This removes the platform font manager variable.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Use HarfBuzz for cross-platform text shaping",
          "description": "Use SkiaSharp.HarfBuzz or RichTextKit for text measurement. HarfBuzz provides consistent shaping results independent of the OS font stack.",
          "category": "alternative",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Load typeface from stream/file for consistent font selection",
      "recommendedReason": "Smallest change for the user, addresses the font resolution variable. If metrics still differ slightly after this, the HarfBuzz approach resolves the shaping backend variable too."
    },
    "nextQuestions": [
      "Does measurement still differ after loading the exact same font file via SKTypeface.FromStream on both platforms?",
      "Does HarfBuzz text shaping produce identical glyph advances on Windows and Linux with the same font file?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.78,
      "reason": "Cross-platform font measurement differences are expected due to platform-specific font manager resolution (DirectWrite vs FontConfig) and shaping backend differences. Maintainer and community comments confirm this is by-design. Workaround is documented. The behavior is not a SkiaSharp defect.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Windows-Classic, os/Linux, tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "os/Linux",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post explanation of root cause and workarounds, then close as not-a-bug",
        "risk": "high",
        "confidence": 0.78,
        "comment": "Thank you for the detailed report and the font files. After investigating the code and community follow-ups, this measurement difference is expected cross-platform behavior:\n\n**Root cause:**\n1. `SKTypeface.FromFamilyName(\"Calibri\", ...)` calls into the OS font manager — **DirectWrite on Windows** and **FontConfig on Linux**. Even with the same Calibri font installed, each platform may resolve it differently.\n2. Even when the same font data is loaded from a stream (as confirmed by follow-up testing in 2025), the low-level glyph advance metrics are computed by **platform shaping backends** (DirectWrite on Windows vs FreeType/HarfBuzz on Linux) which can produce slightly different results.\n\n**Workarounds:**\n\n1. **Load from stream for consistent font selection** — embed the font as a resource and use `SKTypeface.FromStream()` to bypass the OS font manager:\n   ```csharp\n   using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(\"YourApp.Calibri.ttf\");\n   using var tf = SKTypeface.FromStream(stream);\n   paint.Typeface = tf;\n   ```\n   Note: shaping backend differences may still cause minor metric variations.\n\n2. **Use HarfBuzz or RichTextKit for cross-platform text shaping** — [RichTextKit](https://github.com/toptensoftware/RichTextKit) uses HarfBuzz under the hood and provides consistent shaping results across platforms.\n\n3. **Design for metric variance** — if pixel-perfect cross-platform layout consistency is required, use flexible/relative layouts rather than hardcoded measurements.\n\nSince SkiaSharp delegates to platform shaping for text metrics, there is no fix possible at the SkiaSharp layer. Closing as not-a-bug."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — platform-specific font shaping behavior",
        "risk": "medium",
        "confidence": 0.78,
        "stateReason": "not_planned"
      },
      {
        "type": "link-related",
        "description": "Cross-reference related issue #707 — same cross-platform MeasureText discrepancy",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 707
      }
    ]
  }
}
```

</details>
