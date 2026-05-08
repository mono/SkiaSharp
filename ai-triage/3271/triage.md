# Issue Triage Report — #3271

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T23:10:00Z |
| Type | type/bug (0.80 (80%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** Text drawn with SKCanvas.DrawText() produces no visible output on AWS Lambda (.NET 8) when using SkiaSharp.NativeAssets.Linux.NoDependencies because no system fonts are available and SKTypeface.FromFamilyName falls back to a default typeface with no glyphs.

**Analysis:** AWS Lambda's .NET 8 runtime (Amazon Linux 2023) ships no system fonts. SKTypeface.FromFamilyName("Arial") asks Skia to resolve the family name via the system font manager, which returns null or a default no-glyph typeface when no fonts are installed. With NoDependencies (no fontconfig), font enumeration cannot discover system fonts even if they existed. DrawText silently draws nothing because the resolved typeface has no glyphs for the requested text. The fix is to embed a TTF/OTF font as an embedded resource and load it with SKTypeface.FromStream().

**Recommendations:** **needs-investigation** — Root cause is clear (no fonts on Lambda + NoDependencies), but the issue is still open and the reporter has not found a working solution. A detailed workaround comment with code and a pointer to the duplicate #2827 should resolve this. Keeping open pending confirmation from reporter.

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
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a .NET 8 ASP.NET Core app targeting AWS Lambda
2. Add SkiaSharp and SkiaSharp.NativeAssets.Linux.NoDependencies NuGet packages
3. Use SKTypeface.FromFamilyName("Arial") and call canvas.DrawText("Hello, SkiaSharp!", 50, 100, paint)
4. Deploy to AWS Lambda and invoke the function
5. Observe that the output PNG has a white background but no text

**Environment:** AWS Lambda, .NET 8.0, SkiaSharp 2.88.6 / 2.88.9 / 3.116.1, SkiaSharp.NativeAssets.Linux.NoDependencies

**Related issues:** #2827

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2827 — Near-identical issue: Text Missing When Running From AWS Lambda — closed with embedded font workaround
- https://github.com/oxyplot/oxyplot/issues/2145 — OxyPlot reporter linked same root cause: no fonts in .NET 8 Lambda

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Text is not rendered in the output file (no exception thrown) |
| Repro quality | partial |
| Target frameworks | net8.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.6, 2.88.9, 3.116.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The NoDependencies behavior (no fontconfig, no system fonts) has not changed across these versions. The perceived .NET 6 regression is most likely due to the Lambda runtime image changing from Amazon Linux 2 (.NET 6) to Amazon Linux 2023 (.NET 8), which ships fewer system fonts by default. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | False |
| Confidence | 0.75 (75%) |
| Reason | The NoDependencies package is documented as requiring explicit font loading. .NET 6 Lambda ran on Amazon Linux 2 which had some system fonts installed; .NET 8 Lambda runs on Amazon Linux 2023 which has fewer or no system fonts. The SkiaSharp behavior is consistent — the environment changed, not SkiaSharp. |
| Worked in version | — |
| Broke in version | — |

## Analysis

### Technical Summary

AWS Lambda's .NET 8 runtime (Amazon Linux 2023) ships no system fonts. SKTypeface.FromFamilyName("Arial") asks Skia to resolve the family name via the system font manager, which returns null or a default no-glyph typeface when no fonts are installed. With NoDependencies (no fontconfig), font enumeration cannot discover system fonts even if they existed. DrawText silently draws nothing because the resolved typeface has no glyphs for the requested text. The fix is to embed a TTF/OTF font as an embedded resource and load it with SKTypeface.FromStream().

### Rationale

This is classified as type/bug because the silent failure (DrawText succeeds with no visible output, no exception) is poor UX for a common scenario. The root cause is the environment (no fonts + NoDependencies) but the failure mode is a SkiaSharp UX gap. The behavior is by-design per documentation but the documentation is not surfaced at the API call site. A related issue #2827 with identical symptoms was closed as 'completed' after the user found the embedded-font workaround — this issue deserves the same guidance.

### Key Signals

- "NuGet: SkiaSharp.NativeAssets.Linux.NoDependencies" — **issue body** (NoDependencies explicitly has no fontconfig. System font enumeration is disabled. All fonts must be loaded explicitly via SKTypeface.FromFile/FromStream/FromData.)
- "It works correctly in .NET 6" — **issue body** (Amazon Linux 2 (.NET 6 Lambda runtime) ships some fonts (e.g., liberation-fonts); Amazon Linux 2023 (.NET 8) is a minimal OS with no font packages pre-installed.)
- "In .NET 8 on AWS Lambda, no fonts are available by default." — **comment by Rodriguevb** (Confirms the root cause: missing system fonts, not a SkiaSharp version regression.)
- "It still doesn't work. Does anyone have a solution?" — **comment by Rodriguevb** (Maintainer suggestion was too vague; reporter needs specific code showing how to embed and load a font.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 60-72 | direct | FromFamilyName calls sk_typeface_create_from_name. If the family is not found by the Skia font manager, Skia falls back to a default typeface (not null). On a no-fontconfig build with no installed fonts, the fallback typeface has no usable glyphs, so text draws nothing silently. |
| `binding/SkiaSharp/SKTypeface.cs` | 92-114 | direct | SKTypeface.FromStream() and FromData() bypass the font manager entirely and load directly from bytes. These APIs work regardless of fontconfig or system fonts — they are the correct solution for minimal Linux containers. |
| `documentation/dev/packages.md` | 1-1 | context | NoDependencies documentation states: 'No fontconfig, no third-party deps. Designed for minimal containers. Fonts must be loaded explicitly.' This is by-design behavior, but not surfaced as an error at runtime. |

### Workarounds

- Embed a TTF/OTF font file as an EmbeddedResource in the project, then load it with: SKTypeface font = SKTypeface.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("YourProject.Fonts.arial.ttf"));
- Use SKTypeface.FromFile("/path/to/font.ttf") if a font file is available on the Lambda filesystem (e.g., deployed as a Lambda layer)
- Install fonts on the container by switching to SkiaSharp.NativeAssets.Linux (with fontconfig) and adding font packages to the Lambda deployment image

### Next Questions

- Does AWS Lambda support Lambda Layers for font files? This would allow sharing fonts across functions without embedding.
- Would it be useful for SkiaSharp to log a warning when FromFamilyName returns the default typeface due to no fonts available?

### Resolution Proposals

**Hypothesis:** The reporter's Lambda environment (AL2023) has no system fonts. The NoDependencies package cannot enumerate fonts. SKTypeface.FromFamilyName("Arial") silently returns a default no-glyph typeface. The fix is to embed a TTF font as a resource and load it explicitly.

1. **Embed font as embedded resource** — workaround, confidence 0.95 (95%), cost/s, validated=yes
   - Add a TTF font file to the project as EmbeddedResource and load it with SKTypeface.FromStream(). This works in all environments with no external dependencies.

```csharp
// 1. Add font to project (e.g., Fonts/arial.ttf) with Build Action = EmbeddedResource
// 2. Load it at runtime:
using var fontStream = Assembly.GetExecutingAssembly()
    .GetManifestResourceStream("YourProject.Fonts.arial.ttf");
using var typeface = SKTypeface.FromStream(fontStream);

using var paint = new SKPaint
{
    Color = SKColors.Black,
    TextSize = 48,
    IsAntialias = true,
    Typeface = typeface
};
canvas.DrawText("Hello, SkiaSharp!", 50, 100, paint);
```
2. **Deploy font via Lambda Layer** — alternative, confidence 0.85 (85%), cost/m, validated=yes
   - Create a Lambda Layer containing font files (e.g., /opt/fonts/arial.ttf) and load them with SKTypeface.FromFile(). This allows sharing fonts across multiple Lambda functions.

```csharp
using var typeface = SKTypeface.FromFile("/opt/fonts/arial.ttf");
```

**Recommended proposal:** Embed font as embedded resource

**Why:** Self-contained, no external dependencies, works identically on all Lambda runtimes. Closely follows the solution documented in the near-duplicate issue #2827.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | Root cause is clear (no fonts on Lambda + NoDependencies), but the issue is still open and the reporter has not found a working solution. A detailed workaround comment with code and a pointer to the duplicate #2827 should resolve this. Keeping open pending confirmation from reporter. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Add area/SkiaSharp, os/Linux, tenet/compatibility labels | labels=type/bug, area/SkiaSharp, os/Linux, tenet/compatibility |
| link-related | low | 0.92 (92%) | Cross-reference near-duplicate issue #2827 which has same symptom and workaround | linkedIssue=#2827 |
| add-comment | medium | 0.90 (90%) | Post workaround with embedded font code example | — |

**Comment draft for `add-comment`:**

```markdown
## Root Cause

AWS Lambda's .NET 8 runtime (Amazon Linux 2023) ships **no system fonts** by default. When you call `SKTypeface.FromFamilyName("Arial")`, Skia tries to find the font via the system font manager — but with `NoDependencies` (no fontconfig) and no installed fonts, it falls back to a default typeface with no glyphs. `DrawText` then silently draws nothing.

This is the same issue as #2827. It appeared to work with .NET 6 because the older Amazon Linux 2 runtime shipped some fonts; AL2023 does not.

## Workaround: Embed a Font as a Resource

The recommended fix is to embed a `.ttf` or `.otf` font directly in your project:

**1. Add the font file to your project** (e.g., `Fonts/arial.ttf`) and set Build Action to `EmbeddedResource` in your `.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Fonts\arial.ttf" />
</ItemGroup>
```

**2. Load it at runtime instead of using `FromFamilyName`:**

```csharp
using var fontStream = Assembly.GetExecutingAssembly()
    .GetManifestResourceStream("YourProject.Fonts.arial.ttf");
using var typeface = SKTypeface.FromStream(fontStream);

using var paint = new SKPaint
{
    Color = SKColors.Black,
    TextSize = 48,
    IsAntialias = true,
    Typeface = typeface  // use the explicitly loaded typeface
};
canvas.DrawText("Hello, SkiaSharp!", 50, 100, paint);
```

Replace `YourProject` with your actual assembly name (use `Assembly.GetExecutingAssembly().GetName().Name` if unsure). You can use any freely-licensed TTF font (e.g., [Liberation Fonts](https://github.com/liberationfonts/liberation-fonts), [Roboto](https://fonts.google.com/specimen/Roboto), or any font from `C:\Windows\Fonts` on Windows).

## Why NoDependencies?

`SkiaSharp.NativeAssets.Linux.NoDependencies` is designed for minimal containers with no external dependencies. This means no fontconfig, so system font discovery is disabled — all fonts must be loaded explicitly. See the [package documentation](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md#linux-package-selection-guide) for details.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3271,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T23:10:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Text drawn with SKCanvas.DrawText() produces no visible output on AWS Lambda (.NET 8) when using SkiaSharp.NativeAssets.Linux.NoDependencies because no system fonts are available and SKTypeface.FromFamilyName falls back to a default typeface with no glyphs.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
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
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "Text is not rendered in the output file (no exception thrown)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET 8 ASP.NET Core app targeting AWS Lambda",
        "Add SkiaSharp and SkiaSharp.NativeAssets.Linux.NoDependencies NuGet packages",
        "Use SKTypeface.FromFamilyName(\"Arial\") and call canvas.DrawText(\"Hello, SkiaSharp!\", 50, 100, paint)",
        "Deploy to AWS Lambda and invoke the function",
        "Observe that the output PNG has a white background but no text"
      ],
      "environmentDetails": "AWS Lambda, .NET 8.0, SkiaSharp 2.88.6 / 2.88.9 / 3.116.1, SkiaSharp.NativeAssets.Linux.NoDependencies",
      "relatedIssues": [
        2827
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2827",
          "description": "Near-identical issue: Text Missing When Running From AWS Lambda — closed with embedded font workaround"
        },
        {
          "url": "https://github.com/oxyplot/oxyplot/issues/2145",
          "description": "OxyPlot reporter linked same root cause: no fonts in .NET 8 Lambda"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.6",
        "2.88.9",
        "3.116.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The NoDependencies behavior (no fontconfig, no system fonts) has not changed across these versions. The perceived .NET 6 regression is most likely due to the Lambda runtime image changing from Amazon Linux 2 (.NET 6) to Amazon Linux 2023 (.NET 8), which ships fewer system fonts by default."
    },
    "regression": {
      "isRegression": false,
      "confidence": 0.75,
      "reason": "The NoDependencies package is documented as requiring explicit font loading. .NET 6 Lambda ran on Amazon Linux 2 which had some system fonts installed; .NET 8 Lambda runs on Amazon Linux 2023 which has fewer or no system fonts. The SkiaSharp behavior is consistent — the environment changed, not SkiaSharp."
    }
  },
  "analysis": {
    "summary": "AWS Lambda's .NET 8 runtime (Amazon Linux 2023) ships no system fonts. SKTypeface.FromFamilyName(\"Arial\") asks Skia to resolve the family name via the system font manager, which returns null or a default no-glyph typeface when no fonts are installed. With NoDependencies (no fontconfig), font enumeration cannot discover system fonts even if they existed. DrawText silently draws nothing because the resolved typeface has no glyphs for the requested text. The fix is to embed a TTF/OTF font as an embedded resource and load it with SKTypeface.FromStream().",
    "rationale": "This is classified as type/bug because the silent failure (DrawText succeeds with no visible output, no exception) is poor UX for a common scenario. The root cause is the environment (no fonts + NoDependencies) but the failure mode is a SkiaSharp UX gap. The behavior is by-design per documentation but the documentation is not surfaced at the API call site. A related issue #2827 with identical symptoms was closed as 'completed' after the user found the embedded-font workaround — this issue deserves the same guidance.",
    "keySignals": [
      {
        "text": "NuGet: SkiaSharp.NativeAssets.Linux.NoDependencies",
        "source": "issue body",
        "interpretation": "NoDependencies explicitly has no fontconfig. System font enumeration is disabled. All fonts must be loaded explicitly via SKTypeface.FromFile/FromStream/FromData."
      },
      {
        "text": "It works correctly in .NET 6",
        "source": "issue body",
        "interpretation": "Amazon Linux 2 (.NET 6 Lambda runtime) ships some fonts (e.g., liberation-fonts); Amazon Linux 2023 (.NET 8) is a minimal OS with no font packages pre-installed."
      },
      {
        "text": "In .NET 8 on AWS Lambda, no fonts are available by default.",
        "source": "comment by Rodriguevb",
        "interpretation": "Confirms the root cause: missing system fonts, not a SkiaSharp version regression."
      },
      {
        "text": "It still doesn't work. Does anyone have a solution?",
        "source": "comment by Rodriguevb",
        "interpretation": "Maintainer suggestion was too vague; reporter needs specific code showing how to embed and load a font."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "60-72",
        "finding": "FromFamilyName calls sk_typeface_create_from_name. If the family is not found by the Skia font manager, Skia falls back to a default typeface (not null). On a no-fontconfig build with no installed fonts, the fallback typeface has no usable glyphs, so text draws nothing silently.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "92-114",
        "finding": "SKTypeface.FromStream() and FromData() bypass the font manager entirely and load directly from bytes. These APIs work regardless of fontconfig or system fonts — they are the correct solution for minimal Linux containers.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "1-1",
        "finding": "NoDependencies documentation states: 'No fontconfig, no third-party deps. Designed for minimal containers. Fonts must be loaded explicitly.' This is by-design behavior, but not surfaced as an error at runtime.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Embed a TTF/OTF font file as an EmbeddedResource in the project, then load it with: SKTypeface font = SKTypeface.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(\"YourProject.Fonts.arial.ttf\"));",
      "Use SKTypeface.FromFile(\"/path/to/font.ttf\") if a font file is available on the Lambda filesystem (e.g., deployed as a Lambda layer)",
      "Install fonts on the container by switching to SkiaSharp.NativeAssets.Linux (with fontconfig) and adding font packages to the Lambda deployment image"
    ],
    "nextQuestions": [
      "Does AWS Lambda support Lambda Layers for font files? This would allow sharing fonts across functions without embedding.",
      "Would it be useful for SkiaSharp to log a warning when FromFamilyName returns the default typeface due to no fonts available?"
    ],
    "resolution": {
      "hypothesis": "The reporter's Lambda environment (AL2023) has no system fonts. The NoDependencies package cannot enumerate fonts. SKTypeface.FromFamilyName(\"Arial\") silently returns a default no-glyph typeface. The fix is to embed a TTF font as a resource and load it explicitly.",
      "proposals": [
        {
          "title": "Embed font as embedded resource",
          "description": "Add a TTF font file to the project as EmbeddedResource and load it with SKTypeface.FromStream(). This works in all environments with no external dependencies.",
          "category": "workaround",
          "codeSnippet": "// 1. Add font to project (e.g., Fonts/arial.ttf) with Build Action = EmbeddedResource\n// 2. Load it at runtime:\nusing var fontStream = Assembly.GetExecutingAssembly()\n    .GetManifestResourceStream(\"YourProject.Fonts.arial.ttf\");\nusing var typeface = SKTypeface.FromStream(fontStream);\n\nusing var paint = new SKPaint\n{\n    Color = SKColors.Black,\n    TextSize = 48,\n    IsAntialias = true,\n    Typeface = typeface\n};\ncanvas.DrawText(\"Hello, SkiaSharp!\", 50, 100, paint);",
          "confidence": 0.95,
          "effort": "cost/s",
          "validated": "yes"
        },
        {
          "title": "Deploy font via Lambda Layer",
          "description": "Create a Lambda Layer containing font files (e.g., /opt/fonts/arial.ttf) and load them with SKTypeface.FromFile(). This allows sharing fonts across multiple Lambda functions.",
          "category": "alternative",
          "codeSnippet": "using var typeface = SKTypeface.FromFile(\"/opt/fonts/arial.ttf\");",
          "confidence": 0.85,
          "effort": "cost/m",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Embed font as embedded resource",
      "recommendedReason": "Self-contained, no external dependencies, works identically on all Lambda runtimes. Closely follows the solution documented in the near-duplicate issue #2827."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "Root cause is clear (no fonts on Lambda + NoDependencies), but the issue is still open and the reporter has not found a working solution. A detailed workaround comment with code and a pointer to the duplicate #2827 should resolve this. Keeping open pending confirmation from reporter.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Add area/SkiaSharp, os/Linux, tenet/compatibility labels",
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
        "type": "link-related",
        "description": "Cross-reference near-duplicate issue #2827 which has same symptom and workaround",
        "risk": "low",
        "confidence": 0.92,
        "linkedIssue": 2827
      },
      {
        "type": "add-comment",
        "description": "Post workaround with embedded font code example",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "## Root Cause\n\nAWS Lambda's .NET 8 runtime (Amazon Linux 2023) ships **no system fonts** by default. When you call `SKTypeface.FromFamilyName(\"Arial\")`, Skia tries to find the font via the system font manager — but with `NoDependencies` (no fontconfig) and no installed fonts, it falls back to a default typeface with no glyphs. `DrawText` then silently draws nothing.\n\nThis is the same issue as #2827. It appeared to work with .NET 6 because the older Amazon Linux 2 runtime shipped some fonts; AL2023 does not.\n\n## Workaround: Embed a Font as a Resource\n\nThe recommended fix is to embed a `.ttf` or `.otf` font directly in your project:\n\n**1. Add the font file to your project** (e.g., `Fonts/arial.ttf`) and set Build Action to `EmbeddedResource` in your `.csproj`:\n\n```xml\n<ItemGroup>\n  <EmbeddedResource Include=\"Fonts\\arial.ttf\" />\n</ItemGroup>\n```\n\n**2. Load it at runtime instead of using `FromFamilyName`:**\n\n```csharp\nusing var fontStream = Assembly.GetExecutingAssembly()\n    .GetManifestResourceStream(\"YourProject.Fonts.arial.ttf\");\nusing var typeface = SKTypeface.FromStream(fontStream);\n\nusing var paint = new SKPaint\n{\n    Color = SKColors.Black,\n    TextSize = 48,\n    IsAntialias = true,\n    Typeface = typeface  // use the explicitly loaded typeface\n};\ncanvas.DrawText(\"Hello, SkiaSharp!\", 50, 100, paint);\n```\n\nReplace `YourProject` with your actual assembly name (use `Assembly.GetExecutingAssembly().GetName().Name` if unsure). You can use any freely-licensed TTF font (e.g., [Liberation Fonts](https://github.com/liberationfonts/liberation-fonts), [Roboto](https://fonts.google.com/specimen/Roboto), or any font from `C:\\Windows\\Fonts` on Windows).\n\n## Why NoDependencies?\n\n`SkiaSharp.NativeAssets.Linux.NoDependencies` is designed for minimal containers with no external dependencies. This means no fontconfig, so system font discovery is disabled — all fonts must be loaded explicitly. See the [package documentation](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md#linux-package-selection-guide) for details."
      }
    ]
  }
}
```

</details>
