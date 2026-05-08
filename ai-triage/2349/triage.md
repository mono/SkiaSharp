# Issue Triage Report — #2349

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T13:28:23Z |
| Type | type/question (0.85 (85%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** Reporter calls SKTypeface.FromFamilyName('Times New Roman') on Linux (AWS Elastic Beanstalk) using SkiaSharp.NativeAssets.Linux.NoDependencies v2.88.2 and gets back a typeface with an empty FamilyName, while the same code works on Windows.

**Analysis:** This is expected behavior when using SkiaSharp.NativeAssets.Linux.NoDependencies. That package intentionally excludes fontconfig, so SKFontManager cannot enumerate or match system fonts. SKTypeface.FromFamilyName falls back to the Default typeface, which has an empty FamilyName in a fontconfig-less environment. The fix is to load fonts explicitly from a file or embedded resource.

**Recommendations:** **close-as-not-a-bug** — Behavior is by design — NoDependencies package explicitly excludes fontconfig and requires fonts to be loaded explicitly. A contributor already provided the correct answer. Documentation confirms this is expected.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Linux |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Install SkiaSharp.NativeAssets.Linux.NoDependencies 2.88.2 on Linux (e.g., AWS Elastic Beanstalk)
2. Call SKTypeface.FromFamilyName("Times New Roman", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
3. Read the FamilyName property of the returned typeface
4. Observe that FamilyName is empty string

**Environment:** AWS Elastic Beanstalk, .NET 5.0 ASP.NET Core, SkiaSharp 2.88.2, SkiaSharp.NativeAssets.Linux.NoDependencies 2.88.2

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1635 — Related: DllNotFoundException with fontconfig dependency on AWS Lambda — same package selection dilemma

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKTypeface.FromFamilyName delegates to SKFontManager.MatchFamily which relies on fontconfig for system font enumeration. NoDependencies intentionally omits fontconfig; this behavior is unchanged across versions. |

## Analysis

### Technical Summary

This is expected behavior when using SkiaSharp.NativeAssets.Linux.NoDependencies. That package intentionally excludes fontconfig, so SKFontManager cannot enumerate or match system fonts. SKTypeface.FromFamilyName falls back to the Default typeface, which has an empty FamilyName in a fontconfig-less environment. The fix is to load fonts explicitly from a file or embedded resource.

### Rationale

A contributor (CONTRIBUTOR association) already confirmed this is by-design: 'You are using SkiaSharp in a limited environment that does not have any font-matching capabilities. Just load your typeface from a file stream or via some embedded resource.' The packages.md documentation explicitly states NoDependencies is 'Designed for minimal containers. Fonts must be loaded explicitly.' No code change is needed in SkiaSharp; this is a usage question.

### Key Signals

- "If we used SkiaSharp.NativeAssets.Linux (2.88.2) NuGet instead of SkiaSharp.NativeAssets.Linux.NoDependencies (2.88.2) we faced DllNotFoundException: Unable to load shared library 'libSkiaSharp' or one of its dependencies." — **issue body** (User switched to NoDependencies to avoid the fontconfig DllNotFoundException, but is unaware that the NoDependencies build lacks font-matching capabilities.)
- "You are using SkiaSharp in a limited environment that does not have any font-matching capabilities. Just load your typeface from a file stream or via some embedded resource." — **comment by @Gillibald (CONTRIBUTOR)** (Confirms the behavior is by-design. The correct API is SKTypeface.FromFile() or SKTypeface.FromData().)
- "This code works properly in windows." — **issue body** (Windows has system font enumeration built in; Linux NoDependencies package explicitly removes this capability.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 79-80 | direct | FromFamilyName delegates to SKFontManager.Default.MatchFamily(familyName, style) ?? Default. When MatchFamily returns null (no fontconfig), Default typeface is returned. Default has empty FamilyName when no system fonts are available. |
| `binding/SkiaSharp/SKFontManager.cs` | 77-87 | direct | MatchFamily calls native sk_fontmgr_match_family_style. Without fontconfig (NoDependencies build), the native font manager cannot enumerate system fonts and returns null for any family name lookup. |
| `documentation/dev/packages.md` | — | context | NoDependencies documentation explicitly states: 'No fontconfig, no third-party deps — only requires libc/libm/libpthread/libdl. Designed for minimal containers. Fonts must be loaded explicitly.' This confirms the behavior is by design. |

### Workarounds

- Load the font from a file path using SKTypeface.FromFile("/path/to/font.ttf")
- Embed the font file as an assembly resource and load it via SKTypeface.FromData(skData) or SKTypeface.FromStream(stream)
- For AWS Elastic Beanstalk: bundle TTF font files with the application deployment and reference them by relative path

### Resolution Proposals

**Hypothesis:** User is unaware that SkiaSharp.NativeAssets.Linux.NoDependencies intentionally omits fontconfig, making system font resolution impossible. Fonts must be loaded explicitly from files or streams.

1. **Load font from embedded resource** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - Embed the desired font (e.g., a free Times New Roman equivalent like FreeSerif.ttf) as an embedded resource in the project and load it via SKTypeface.FromData().
2. **Load font from file system path** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - Deploy the TTF font file alongside the application and use SKTypeface.FromFile(path) to load it at runtime.

**Recommended proposal:** Load font from embedded resource

**Why:** Embedding font as a resource is the most portable approach for AWS deployments, requiring no file system assumptions.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | Behavior is by design — NoDependencies package explicitly excludes fontconfig and requires fonts to be loaded explicitly. A contributor already provided the correct answer. Documentation confirms this is expected. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question and Linux labels | labels=type/question, area/SkiaSharp, os/Linux |
| add-comment | high | 0.88 (88%) | Confirm by-design behavior and provide workaround with code example | — |
| close-issue | medium | 0.85 (85%) | Close as not a bug — by-design behavior of the NoDependencies package | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! This behavior is expected when using `SkiaSharp.NativeAssets.Linux.NoDependencies`. That package intentionally excludes fontconfig (the Linux font-matching library) to allow deployment on minimal containers like AWS Elastic Beanstalk without needing system font libraries installed.

Because there is no font-matching backend available, `SKTypeface.FromFamilyName()` cannot locate system fonts by name and falls back to an empty default typeface.

**Workaround:** Load your font explicitly from a file or embedded resource instead:

```csharp
// Option 1: Load from a .ttf file bundled with your deployment
var typeface = SKTypeface.FromFile("/path/to/your-font.ttf");

// Option 2: Load from a stream (e.g., embedded resource)
using var stream = Assembly.GetExecutingAssembly()
    .GetManifestResourceStream("YourApp.Fonts.YourFont.ttf");
var typeface = SKTypeface.FromStream(stream);
```

You can download open-licensed fonts (e.g., from Google Fonts) and bundle them with your application. This approach also works reliably on Windows, making your code cross-platform without relying on system font availability.

If you still see a `DllNotFoundException` with the regular `SkiaSharp.NativeAssets.Linux` package, that's a separate issue where fontconfig (`libfontconfig.so.1`) is not installed in the Elastic Beanstalk environment — the NoDependencies package is the correct choice for that environment.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2349,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T13:28:23Z"
  },
  "summary": "Reporter calls SKTypeface.FromFamilyName('Times New Roman') on Linux (AWS Elastic Beanstalk) using SkiaSharp.NativeAssets.Linux.NoDependencies v2.88.2 and gets back a typeface with an empty FamilyName, while the same code works on Windows.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "platforms": [
      "os/Linux"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Install SkiaSharp.NativeAssets.Linux.NoDependencies 2.88.2 on Linux (e.g., AWS Elastic Beanstalk)",
        "Call SKTypeface.FromFamilyName(\"Times New Roman\", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)",
        "Read the FamilyName property of the returned typeface",
        "Observe that FamilyName is empty string"
      ],
      "environmentDetails": "AWS Elastic Beanstalk, .NET 5.0 ASP.NET Core, SkiaSharp 2.88.2, SkiaSharp.NativeAssets.Linux.NoDependencies 2.88.2",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1635",
          "description": "Related: DllNotFoundException with fontconfig dependency on AWS Lambda — same package selection dilemma"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.2"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKTypeface.FromFamilyName delegates to SKFontManager.MatchFamily which relies on fontconfig for system font enumeration. NoDependencies intentionally omits fontconfig; this behavior is unchanged across versions."
    }
  },
  "analysis": {
    "summary": "This is expected behavior when using SkiaSharp.NativeAssets.Linux.NoDependencies. That package intentionally excludes fontconfig, so SKFontManager cannot enumerate or match system fonts. SKTypeface.FromFamilyName falls back to the Default typeface, which has an empty FamilyName in a fontconfig-less environment. The fix is to load fonts explicitly from a file or embedded resource.",
    "rationale": "A contributor (CONTRIBUTOR association) already confirmed this is by-design: 'You are using SkiaSharp in a limited environment that does not have any font-matching capabilities. Just load your typeface from a file stream or via some embedded resource.' The packages.md documentation explicitly states NoDependencies is 'Designed for minimal containers. Fonts must be loaded explicitly.' No code change is needed in SkiaSharp; this is a usage question.",
    "keySignals": [
      {
        "text": "If we used SkiaSharp.NativeAssets.Linux (2.88.2) NuGet instead of SkiaSharp.NativeAssets.Linux.NoDependencies (2.88.2) we faced DllNotFoundException: Unable to load shared library 'libSkiaSharp' or one of its dependencies.",
        "source": "issue body",
        "interpretation": "User switched to NoDependencies to avoid the fontconfig DllNotFoundException, but is unaware that the NoDependencies build lacks font-matching capabilities."
      },
      {
        "text": "You are using SkiaSharp in a limited environment that does not have any font-matching capabilities. Just load your typeface from a file stream or via some embedded resource.",
        "source": "comment by @Gillibald (CONTRIBUTOR)",
        "interpretation": "Confirms the behavior is by-design. The correct API is SKTypeface.FromFile() or SKTypeface.FromData()."
      },
      {
        "text": "This code works properly in windows.",
        "source": "issue body",
        "interpretation": "Windows has system font enumeration built in; Linux NoDependencies package explicitly removes this capability."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "79-80",
        "finding": "FromFamilyName delegates to SKFontManager.Default.MatchFamily(familyName, style) ?? Default. When MatchFamily returns null (no fontconfig), Default typeface is returned. Default has empty FamilyName when no system fonts are available.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "77-87",
        "finding": "MatchFamily calls native sk_fontmgr_match_family_style. Without fontconfig (NoDependencies build), the native font manager cannot enumerate system fonts and returns null for any family name lookup.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "finding": "NoDependencies documentation explicitly states: 'No fontconfig, no third-party deps — only requires libc/libm/libpthread/libdl. Designed for minimal containers. Fonts must be loaded explicitly.' This confirms the behavior is by design.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Load the font from a file path using SKTypeface.FromFile(\"/path/to/font.ttf\")",
      "Embed the font file as an assembly resource and load it via SKTypeface.FromData(skData) or SKTypeface.FromStream(stream)",
      "For AWS Elastic Beanstalk: bundle TTF font files with the application deployment and reference them by relative path"
    ],
    "resolution": {
      "hypothesis": "User is unaware that SkiaSharp.NativeAssets.Linux.NoDependencies intentionally omits fontconfig, making system font resolution impossible. Fonts must be loaded explicitly from files or streams.",
      "proposals": [
        {
          "title": "Load font from embedded resource",
          "description": "Embed the desired font (e.g., a free Times New Roman equivalent like FreeSerif.ttf) as an embedded resource in the project and load it via SKTypeface.FromData().",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Load font from file system path",
          "description": "Deploy the TTF font file alongside the application and use SKTypeface.FromFile(path) to load it at runtime.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Load font from embedded resource",
      "recommendedReason": "Embedding font as a resource is the most portable approach for AWS deployments, requiring no file system assumptions."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "Behavior is by design — NoDependencies package explicitly excludes fontconfig and requires fonts to be loaded explicitly. A contributor already provided the correct answer. Documentation confirms this is expected.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and Linux labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Linux"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm by-design behavior and provide workaround with code example",
        "risk": "high",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report! This behavior is expected when using `SkiaSharp.NativeAssets.Linux.NoDependencies`. That package intentionally excludes fontconfig (the Linux font-matching library) to allow deployment on minimal containers like AWS Elastic Beanstalk without needing system font libraries installed.\n\nBecause there is no font-matching backend available, `SKTypeface.FromFamilyName()` cannot locate system fonts by name and falls back to an empty default typeface.\n\n**Workaround:** Load your font explicitly from a file or embedded resource instead:\n\n```csharp\n// Option 1: Load from a .ttf file bundled with your deployment\nvar typeface = SKTypeface.FromFile(\"/path/to/your-font.ttf\");\n\n// Option 2: Load from a stream (e.g., embedded resource)\nusing var stream = Assembly.GetExecutingAssembly()\n    .GetManifestResourceStream(\"YourApp.Fonts.YourFont.ttf\");\nvar typeface = SKTypeface.FromStream(stream);\n```\n\nYou can download open-licensed fonts (e.g., from Google Fonts) and bundle them with your application. This approach also works reliably on Windows, making your code cross-platform without relying on system font availability.\n\nIf you still see a `DllNotFoundException` with the regular `SkiaSharp.NativeAssets.Linux` package, that's a separate issue where fontconfig (`libfontconfig.so.1`) is not installed in the Elastic Beanstalk environment — the NoDependencies package is the correct choice for that environment."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — by-design behavior of the NoDependencies package",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
