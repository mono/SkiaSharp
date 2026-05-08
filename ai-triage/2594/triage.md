# Issue Triage Report — #2594

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T21:42:58Z |
| Type | type/bug (0.82 (82%)) |
| Area | area/SkiaSharp (0.80 (80%)) |
| Suggested action | needs-info (0.78 (78%)) |

**Issue Summary:** Font rendering quality is visibly worse on Linux Docker (SkiaSharp 2.88.3) than on Windows when using Neodynamic thermal label library with NativePrinterFont aliases, producing degraded text glyphs in the output image.

**Analysis:** Rendering quality difference between Windows and Linux Docker for text using 'NativePrinterFontA/B' aliases. These are Neodynamic-internal font references that SkiaSharp resolves differently on each platform: Windows uses DirectWrite with system fonts and font hinting; Linux without fontconfig (NoDependencies package) falls back to Skia's built-in minimal font with no hinting, producing coarser glyph rendering. The root cause is likely either (1) missing fontconfig support in the Linux NativeAssets package used, or (2) inherent cross-platform rendering differences in Skia's font rendering backends (DirectWrite on Windows vs FreeType on Linux).

**Recommendations:** **needs-info** — Issue reports a real rendering difference with a screenshot, but critical information is missing: which Linux NativeAssets package is used, whether fontconfig is installed, and what the Docker base image is. The issue is routed through a third-party library (Neodynamic), making it difficult to confirm SkiaSharp is the actual root cause without more info.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Linux |
| Backends | backend/Raster |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Run microservice using Neodynamic thermal label library on Windows — labels render correctly
2. Run same microservice in Docker Linux container — text appears degraded/lower quality
3. Compare output images: Windows version shows sharp font rendering; Linux version shows degraded rendering

**Environment:** SkiaSharp 2.88.3, Visual Studio (Windows IDE), Linux Docker container

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1131 — Related: Can't draw text using default typeface on Linux (fontconfig/NoDependencies issue)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Font glyphs rendered with lower quality on Linux Docker vs Windows |
| Repro quality | partial |
| Target frameworks | net6.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Font rendering on Linux is determined by which NativeAssets package is used and whether fontconfig is available. This has not fundamentally changed since 2.88.3. |

## Analysis

### Technical Summary

Rendering quality difference between Windows and Linux Docker for text using 'NativePrinterFontA/B' aliases. These are Neodynamic-internal font references that SkiaSharp resolves differently on each platform: Windows uses DirectWrite with system fonts and font hinting; Linux without fontconfig (NoDependencies package) falls back to Skia's built-in minimal font with no hinting, producing coarser glyph rendering. The root cause is likely either (1) missing fontconfig support in the Linux NativeAssets package used, or (2) inherent cross-platform rendering differences in Skia's font rendering backends (DirectWrite on Windows vs FreeType on Linux).

### Rationale

Reporter provides screenshot showing visual rendering difference (not a crash or exception). The issue is clearly wrong/degraded output. The 'NativePrinterFontA/B' are Neodynamic's own font aliases for Zebra printer protocol fonts — they're not real system fonts. On Linux without fontconfig, Skia cannot enumerate system fonts and falls back to built-in fonts, producing poor rendering. This is a known Linux font rendering pattern in SkiaSharp. Classified as type/bug because the API contract (consistent rendering) is not met, and area/SkiaSharp because it's the core text rendering layer.

### Key Signals

- "why under Linux the rendering seems to not be as good as under Windows it's something outside our scope. The rendering part is performed by the SkiaSharp dependency." — **issue body — Neodynamic vendor response** (Vendor confirms SkiaSharp is responsible for the actual rendering difference. Font glyphs are the same; quality difference is in the rendering layer.)
- "Font="NativePrinterFontA,1,Point,,,False,90,,CP850"" — **issue body — ThermalLabel template XML** (NativePrinterFontA/B are Zebra ZPL protocol font aliases, not real system fonts. Neodynamic likely substitutes these with a system font. On Linux without fontconfig, the substitution may fail or produce a low-quality fallback.)
- "running microservice in docker Linux environment" — **issue body** (Docker Linux containers typically use NoDependencies package (no fontconfig), which affects font enumeration and rendering quality.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 29-33 | direct | Default typeface resolution uses sk_fontmgr_legacy_create_typeface with null family name. On Linux without fontconfig (NoDependencies package), SKFontManager cannot enumerate system fonts and falls back to Skia's built-in empty/minimal typeface. This produces visible rendering quality differences compared to Windows (DirectWrite). |
| `documentation/dev/packages.md` | 86-87 | direct | NativeAssets.Linux.NoDependencies has 'no fontconfig, no third-party deps — Fonts must be loaded explicitly.' This is the recommended Docker package but it explicitly lacks fontconfig, which is required for system font enumeration. Without fontconfig, unknown font aliases like NativePrinterFontA fall back to a built-in font with no hinting. |

### Workarounds

- If using SkiaSharp.NativeAssets.Linux.NoDependencies, switch to SkiaSharp.NativeAssets.Linux and install libfontconfig1 in the Docker image (apt-get install -y libfontconfig1). This enables proper font enumeration and improves rendering quality.
- Install fonts that match the substitution target on Linux (e.g., apt-get install -y fonts-liberation fonts-dejavu) to improve font fallback quality.
- Contact Neodynamic to ask which actual SKTypeface/font name is being passed to SkiaSharp for NativePrinterFontA/B — then embed that font explicitly in the Docker image.

### Next Questions

- Which SkiaSharp Linux NativeAssets package is being used? (NativeAssets.Linux vs NativeAssets.Linux.NoDependencies)
- What is the Docker base image (mcr.microsoft.com/dotnet/aspnet, Alpine, etc.)?
- Is libfontconfig1 installed in the Docker container?
- What actual font name/file does Neodynamic use for NativePrinterFontA/B on Windows?

### Resolution Proposals

**Hypothesis:** Linux Docker container likely uses NoDependencies package (no fontconfig), causing font fallback to Skia's built-in minimal typeface for unknown 'NativePrinterFontA/B' font aliases. On Windows, DirectWrite resolves these to a system font with proper hinting.

1. **Switch to NativeAssets.Linux with fontconfig** — workaround, confidence 0.75 (75%), cost/xs, validated=untested
   - Change the Linux NativeAssets package from NoDependencies to SkiaSharp.NativeAssets.Linux, and add libfontconfig1 to the Docker image. This enables proper system font enumeration, which improves fallback quality for unrecognized font names.
2. **Install fonts in Docker image** — workaround, confidence 0.65 (65%), cost/xs, validated=untested
   - Install common font packages in the Docker image (e.g., apt-get install -y fonts-liberation fonts-dejavu-core). Even with NoDependencies, if font files are present in standard locations and fontconfig is available, font substitution quality improves.

**Recommended proposal:** Switch to NativeAssets.Linux with fontconfig

**Why:** Most likely root cause is absence of fontconfig causing poor font fallback. Switching packages is a low-effort change that directly addresses the likely root cause.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.78 (78%) |
| Reason | Issue reports a real rendering difference with a screenshot, but critical information is missing: which Linux NativeAssets package is used, whether fontconfig is installed, and what the Docker base image is. The issue is routed through a third-party library (Neodynamic), making it difficult to confirm SkiaSharp is the actual root cause without more info. |
| Suggested repro platform | linux |

### Missing Info

- Which SkiaSharp Linux NativeAssets package is used: NativeAssets.Linux or NativeAssets.Linux.NoDependencies?
- Docker base image name and version
- Is libfontconfig1 installed in the container?
- Direct minimal SkiaSharp repro code (without Neodynamic) showing the rendering difference

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, SkiaSharp, Linux labels | labels=type/bug, area/SkiaSharp, os/Linux, tenet/compatibility |
| add-comment | medium | 0.78 (78%) | Request missing info and suggest fontconfig workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report and the screenshot — the rendering difference is clearly visible.

Font rendering quality on Linux depends heavily on which SkiaSharp native package is deployed. Could you share:

1. **Which Linux NativeAssets package** are you using? (`SkiaSharp.NativeAssets.Linux` or `SkiaSharp.NativeAssets.Linux.NoDependencies`)
2. **Docker base image** (e.g., `mcr.microsoft.com/dotnet/aspnet:6.0`, Alpine, etc.)
3. **Is `libfontconfig1` installed** in the container?

**Likely workaround to try:**

If you're using `SkiaSharp.NativeAssets.Linux.NoDependencies` (common in minimal containers), it has **no fontconfig support**, which means SkiaSharp cannot enumerate system fonts. Unknown font names like `NativePrinterFontA` may fall back to a built-in minimal typeface with no hinting, causing the degraded rendering you see.

Try switching to `SkiaSharp.NativeAssets.Linux` and adding `libfontconfig1` to your Docker image:

```dockerfile
RUN apt-get update && apt-get install -y libfontconfig1 fonts-liberation
```

This enables proper font enumeration and should improve rendering quality.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2594,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T21:42:58Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Font rendering quality is visibly worse on Linux Docker (SkiaSharp 2.88.3) than on Windows when using Neodynamic thermal label library with NativePrinterFont aliases, producing degraded text glyphs in the output image.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.8
    },
    "platforms": [
      "os/Linux"
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
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Font glyphs rendered with lower quality on Linux Docker vs Windows",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Run microservice using Neodynamic thermal label library on Windows — labels render correctly",
        "Run same microservice in Docker Linux container — text appears degraded/lower quality",
        "Compare output images: Windows version shows sharp font rendering; Linux version shows degraded rendering"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Visual Studio (Windows IDE), Linux Docker container",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1131",
          "description": "Related: Can't draw text using default typeface on Linux (fontconfig/NoDependencies issue)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Font rendering on Linux is determined by which NativeAssets package is used and whether fontconfig is available. This has not fundamentally changed since 2.88.3."
    }
  },
  "analysis": {
    "summary": "Rendering quality difference between Windows and Linux Docker for text using 'NativePrinterFontA/B' aliases. These are Neodynamic-internal font references that SkiaSharp resolves differently on each platform: Windows uses DirectWrite with system fonts and font hinting; Linux without fontconfig (NoDependencies package) falls back to Skia's built-in minimal font with no hinting, producing coarser glyph rendering. The root cause is likely either (1) missing fontconfig support in the Linux NativeAssets package used, or (2) inherent cross-platform rendering differences in Skia's font rendering backends (DirectWrite on Windows vs FreeType on Linux).",
    "rationale": "Reporter provides screenshot showing visual rendering difference (not a crash or exception). The issue is clearly wrong/degraded output. The 'NativePrinterFontA/B' are Neodynamic's own font aliases for Zebra printer protocol fonts — they're not real system fonts. On Linux without fontconfig, Skia cannot enumerate system fonts and falls back to built-in fonts, producing poor rendering. This is a known Linux font rendering pattern in SkiaSharp. Classified as type/bug because the API contract (consistent rendering) is not met, and area/SkiaSharp because it's the core text rendering layer.",
    "keySignals": [
      {
        "text": "why under Linux the rendering seems to not be as good as under Windows it's something outside our scope. The rendering part is performed by the SkiaSharp dependency.",
        "source": "issue body — Neodynamic vendor response",
        "interpretation": "Vendor confirms SkiaSharp is responsible for the actual rendering difference. Font glyphs are the same; quality difference is in the rendering layer."
      },
      {
        "text": "Font=\"NativePrinterFontA,1,Point,,,False,90,,CP850\"",
        "source": "issue body — ThermalLabel template XML",
        "interpretation": "NativePrinterFontA/B are Zebra ZPL protocol font aliases, not real system fonts. Neodynamic likely substitutes these with a system font. On Linux without fontconfig, the substitution may fail or produce a low-quality fallback."
      },
      {
        "text": "running microservice in docker Linux environment",
        "source": "issue body",
        "interpretation": "Docker Linux containers typically use NoDependencies package (no fontconfig), which affects font enumeration and rendering quality."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "29-33",
        "finding": "Default typeface resolution uses sk_fontmgr_legacy_create_typeface with null family name. On Linux without fontconfig (NoDependencies package), SKFontManager cannot enumerate system fonts and falls back to Skia's built-in empty/minimal typeface. This produces visible rendering quality differences compared to Windows (DirectWrite).",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "86-87",
        "finding": "NativeAssets.Linux.NoDependencies has 'no fontconfig, no third-party deps — Fonts must be loaded explicitly.' This is the recommended Docker package but it explicitly lacks fontconfig, which is required for system font enumeration. Without fontconfig, unknown font aliases like NativePrinterFontA fall back to a built-in font with no hinting.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "If using SkiaSharp.NativeAssets.Linux.NoDependencies, switch to SkiaSharp.NativeAssets.Linux and install libfontconfig1 in the Docker image (apt-get install -y libfontconfig1). This enables proper font enumeration and improves rendering quality.",
      "Install fonts that match the substitution target on Linux (e.g., apt-get install -y fonts-liberation fonts-dejavu) to improve font fallback quality.",
      "Contact Neodynamic to ask which actual SKTypeface/font name is being passed to SkiaSharp for NativePrinterFontA/B — then embed that font explicitly in the Docker image."
    ],
    "nextQuestions": [
      "Which SkiaSharp Linux NativeAssets package is being used? (NativeAssets.Linux vs NativeAssets.Linux.NoDependencies)",
      "What is the Docker base image (mcr.microsoft.com/dotnet/aspnet, Alpine, etc.)?",
      "Is libfontconfig1 installed in the Docker container?",
      "What actual font name/file does Neodynamic use for NativePrinterFontA/B on Windows?"
    ],
    "resolution": {
      "hypothesis": "Linux Docker container likely uses NoDependencies package (no fontconfig), causing font fallback to Skia's built-in minimal typeface for unknown 'NativePrinterFontA/B' font aliases. On Windows, DirectWrite resolves these to a system font with proper hinting.",
      "proposals": [
        {
          "title": "Switch to NativeAssets.Linux with fontconfig",
          "description": "Change the Linux NativeAssets package from NoDependencies to SkiaSharp.NativeAssets.Linux, and add libfontconfig1 to the Docker image. This enables proper system font enumeration, which improves fallback quality for unrecognized font names.",
          "category": "workaround",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Install fonts in Docker image",
          "description": "Install common font packages in the Docker image (e.g., apt-get install -y fonts-liberation fonts-dejavu-core). Even with NoDependencies, if font files are present in standard locations and fontconfig is available, font substitution quality improves.",
          "category": "workaround",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Switch to NativeAssets.Linux with fontconfig",
      "recommendedReason": "Most likely root cause is absence of fontconfig causing poor font fallback. Switching packages is a low-effort change that directly addresses the likely root cause."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.78,
      "reason": "Issue reports a real rendering difference with a screenshot, but critical information is missing: which Linux NativeAssets package is used, whether fontconfig is installed, and what the Docker base image is. The issue is routed through a third-party library (Neodynamic), making it difficult to confirm SkiaSharp is the actual root cause without more info.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Which SkiaSharp Linux NativeAssets package is used: NativeAssets.Linux or NativeAssets.Linux.NoDependencies?",
      "Docker base image name and version",
      "Is libfontconfig1 installed in the container?",
      "Direct minimal SkiaSharp repro code (without Neodynamic) showing the rendering difference"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp, Linux labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Linux",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request missing info and suggest fontconfig workaround",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for the report and the screenshot — the rendering difference is clearly visible.\n\nFont rendering quality on Linux depends heavily on which SkiaSharp native package is deployed. Could you share:\n\n1. **Which Linux NativeAssets package** are you using? (`SkiaSharp.NativeAssets.Linux` or `SkiaSharp.NativeAssets.Linux.NoDependencies`)\n2. **Docker base image** (e.g., `mcr.microsoft.com/dotnet/aspnet:6.0`, Alpine, etc.)\n3. **Is `libfontconfig1` installed** in the container?\n\n**Likely workaround to try:**\n\nIf you're using `SkiaSharp.NativeAssets.Linux.NoDependencies` (common in minimal containers), it has **no fontconfig support**, which means SkiaSharp cannot enumerate system fonts. Unknown font names like `NativePrinterFontA` may fall back to a built-in minimal typeface with no hinting, causing the degraded rendering you see.\n\nTry switching to `SkiaSharp.NativeAssets.Linux` and adding `libfontconfig1` to your Docker image:\n\n```dockerfile\nRUN apt-get update && apt-get install -y libfontconfig1 fonts-liberation\n```\n\nThis enables proper font enumeration and should improve rendering quality."
      }
    ]
  }
}
```

</details>
