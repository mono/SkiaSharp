# Issue Triage Report — #2423

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T23:13:27Z |
| Type | type/question (0.92 (92%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** Text measured with SKPaint/SKFont renders at different sizes on Windows vs macOS because Skia uses different native font APIs (DirectWrite vs CoreText) per OS.

**Analysis:** The reporter observes that the same text drawn and measured via SKPaint produces different pixel dimensions on Windows vs macOS. This is by-design: Skia's underlying native font engine uses DirectWrite on Windows and CoreText on macOS, which resolves different glyphs and metrics even for the same font family name. The standard fix is to embed a platform-independent font file and load it with SKTypeface.FromFile / SKTypeface.FromStream so that identical glyph data is used on all platforms.

**Recommendations:** **close-as-not-a-bug** — This is by-design behavior in Skia (platform-native font stacks). A clear workaround exists. The issue can be closed with a helpful explanation and workaround.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic, os/macOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Attachments:**
- text-size-comparison.png — https://user-images.githubusercontent.com/45782815/226529620-75e9eb52-1baa-4b00-9856-d2409e6bee2a.png — Side-by-side comparison of text rendered on Windows (left) and macOS (right)

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Skia continues to use platform-native font stacks (DirectWrite/CoreText), so the behavior persists in current versions. |

## Analysis

### Technical Summary

The reporter observes that the same text drawn and measured via SKPaint produces different pixel dimensions on Windows vs macOS. This is by-design: Skia's underlying native font engine uses DirectWrite on Windows and CoreText on macOS, which resolves different glyphs and metrics even for the same font family name. The standard fix is to embed a platform-independent font file and load it with SKTypeface.FromFile / SKTypeface.FromStream so that identical glyph data is used on all platforms.

### Rationale

Classified as type/question because the reporter is asking 'why does this happen and how do I fix it?' — not filing a defect. The behavior is by design in Skia (platform-native font stacks). The workaround (embed a custom font) is well-known and can be answered directly.

### Key Signals

- "Skia (native) uses different apis for fonts in different environments. Windows build uses DirectWrite, MacOS build uses CoreText." — **Community comment #1503467232** (Confirms the root cause is in the Skia native layer, not in SkiaSharp bindings.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | — | direct | SKTypeface.Default resolves via SKFontManager.Default which delegates to the OS font manager. On Windows this is DirectWrite; on macOS it is CoreText. SKTypeface.FromFile / FromStream bypass the OS font manager and load a specific font binary. |
| `binding/SkiaSharp/SKPaint.cs` | — | direct | SKPaint.MeasureText delegates to SKFont.MeasureText which uses the Typeface set on the font. When no typeface is set explicitly, it falls through to SKTypeface.Default — the OS default font — explaining the platform discrepancy. |

### Workarounds

- Load a platform-independent font file using SKTypeface.FromFile(path) or SKTypeface.FromStream(stream) and assign it to SKPaint.Typeface / SKFont.Typeface. This ensures identical glyph data and metrics on all platforms.

### Resolution Proposals

**Hypothesis:** The difference in text dimensions is caused by Skia using DirectWrite on Windows and CoreText on macOS for the default system typeface. Embedding a custom font file makes metrics deterministic across platforms.

1. **Embed and load a custom font file** — workaround, cost/xs, validated=yes
   - Bundle a .ttf/.otf file with the application and load it explicitly with SKTypeface.FromFile or SKTypeface.FromStream, assigning it to SKFont.Typeface. This bypasses the OS font resolver and guarantees identical metrics on all platforms.

**Recommended proposal:** workaround-embed-font

**Why:** Minimal change, works on all platforms, no SkiaSharp code changes required.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | This is by-design behavior in Skia (platform-native font stacks). A clear workaround exists. The issue can be closed with a helpful explanation and workaround. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/question, area/SkiaSharp, os/Windows-Classic, os/macOS, tenet/compatibility | labels=type/question, area/SkiaSharp, os/Windows-Classic, os/macOS, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Explain the platform font backend difference and provide the embed-font workaround. | — |
| close-issue | medium | 0.85 (85%) | Close as not-a-bug — behavior is by design in Skia; workaround provided. | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the screenshot comparison — that makes the difference easy to see.

This is expected behavior in Skia itself (not specific to SkiaSharp): on Windows, Skia uses DirectWrite to resolve and render fonts, while on macOS it uses CoreText. Because these are different font engines, they may choose different default typefaces or apply different metrics for the same family name, producing different layout widths and heights.

The standard fix is to **embed a font file** in your project and load it explicitly so all platforms use identical glyph data:

```csharp
// Load an embedded .ttf/.otf instead of relying on the system default
using var typeface = SKTypeface.FromFile("path/to/your/font.ttf");
using var font = new SKFont(typeface, textSize);
using var paint = new SKPaint(font);

// MeasureText will now return the same values on Windows and macOS
float width = font.MeasureText("Hello");
```

With a bundled font, `MeasureText` returns identical values on every platform. Free options include [Roboto](https://fonts.google.com/specimen/Roboto) or [Noto Sans](https://fonts.google.com/noto/specimen/Noto+Sans).
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2423,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T23:13:27Z"
  },
  "summary": "Text measured with SKPaint/SKFont renders at different sizes on Windows vs macOS because Skia uses different native font APIs (DirectWrite vs CoreText) per OS.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
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
    "reproEvidence": {
      "codeSnippets": [],
      "attachments": [
        {
          "url": "https://user-images.githubusercontent.com/45782815/226529620-75e9eb52-1baa-4b00-9856-d2409e6bee2a.png",
          "filename": "text-size-comparison.png",
          "description": "Side-by-side comparison of text rendered on Windows (left) and macOS (right)"
        }
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "Skia continues to use platform-native font stacks (DirectWrite/CoreText), so the behavior persists in current versions."
    }
  },
  "analysis": {
    "summary": "The reporter observes that the same text drawn and measured via SKPaint produces different pixel dimensions on Windows vs macOS. This is by-design: Skia's underlying native font engine uses DirectWrite on Windows and CoreText on macOS, which resolves different glyphs and metrics even for the same font family name. The standard fix is to embed a platform-independent font file and load it with SKTypeface.FromFile / SKTypeface.FromStream so that identical glyph data is used on all platforms.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "finding": "SKTypeface.Default resolves via SKFontManager.Default which delegates to the OS font manager. On Windows this is DirectWrite; on macOS it is CoreText. SKTypeface.FromFile / FromStream bypass the OS font manager and load a specific font binary.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "finding": "SKPaint.MeasureText delegates to SKFont.MeasureText which uses the Typeface set on the font. When no typeface is set explicitly, it falls through to SKTypeface.Default — the OS default font — explaining the platform discrepancy.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Skia (native) uses different apis for fonts in different environments. Windows build uses DirectWrite, MacOS build uses CoreText.",
        "source": "Community comment #1503467232",
        "interpretation": "Confirms the root cause is in the Skia native layer, not in SkiaSharp bindings."
      }
    ],
    "rationale": "Classified as type/question because the reporter is asking 'why does this happen and how do I fix it?' — not filing a defect. The behavior is by design in Skia (platform-native font stacks). The workaround (embed a custom font) is well-known and can be answered directly.",
    "workarounds": [
      "Load a platform-independent font file using SKTypeface.FromFile(path) or SKTypeface.FromStream(stream) and assign it to SKPaint.Typeface / SKFont.Typeface. This ensures identical glyph data and metrics on all platforms."
    ],
    "resolution": {
      "hypothesis": "The difference in text dimensions is caused by Skia using DirectWrite on Windows and CoreText on macOS for the default system typeface. Embedding a custom font file makes metrics deterministic across platforms.",
      "proposals": [
        {
          "title": "Embed and load a custom font file",
          "category": "workaround",
          "description": "Bundle a .ttf/.otf file with the application and load it explicitly with SKTypeface.FromFile or SKTypeface.FromStream, assigning it to SKFont.Typeface. This bypasses the OS font resolver and guarantees identical metrics on all platforms.",
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "workaround-embed-font",
      "recommendedReason": "Minimal change, works on all platforms, no SkiaSharp code changes required."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "This is by-design behavior in Skia (platform-native font stacks). A clear workaround exists. The issue can be closed with a helpful explanation and workaround.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/question, area/SkiaSharp, os/Windows-Classic, os/macOS, tenet/compatibility",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "os/macOS",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain the platform font backend difference and provide the embed-font workaround.",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the screenshot comparison — that makes the difference easy to see.\n\nThis is expected behavior in Skia itself (not specific to SkiaSharp): on Windows, Skia uses DirectWrite to resolve and render fonts, while on macOS it uses CoreText. Because these are different font engines, they may choose different default typefaces or apply different metrics for the same family name, producing different layout widths and heights.\n\nThe standard fix is to **embed a font file** in your project and load it explicitly so all platforms use identical glyph data:\n\n```csharp\n// Load an embedded .ttf/.otf instead of relying on the system default\nusing var typeface = SKTypeface.FromFile(\"path/to/your/font.ttf\");\nusing var font = new SKFont(typeface, textSize);\nusing var paint = new SKPaint(font);\n\n// MeasureText will now return the same values on Windows and macOS\nfloat width = font.MeasureText(\"Hello\");\n```\n\nWith a bundled font, `MeasureText` returns identical values on every platform. Free options include [Roboto](https://fonts.google.com/specimen/Roboto) or [Noto Sans](https://fonts.google.com/noto/specimen/Noto+Sans)."
      },
      {
        "type": "close-issue",
        "description": "Close as not-a-bug — behavior is by design in Skia; workaround provided.",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
