# Issue Triage Report — #1902

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:25:48Z |
| Type | type/bug (0.82 (82%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** SKTypeface.FromFamilyName always returns the default font (Noto Mono) in Blazor WASM because Skia's font manager cannot access system fonts in the WASM sandbox; the workaround is to load fonts explicitly via SKTypeface.FromStream or SKTypeface.FromData.

**Analysis:** In WASM (Blazor WebAssembly), Skia's font manager cannot enumerate or load system fonts because the WASM sandbox has no access to browser/OS font directories. SKTypeface.FromFamilyName silently falls back to the default embedded font (Noto Mono) when the requested font is not found. This is expected WASM behavior but confusing for developers used to desktop/server platforms where system fonts are accessible.

**Recommendations:** **close-as-not-a-bug** — This is a WASM platform limitation: Skia's font manager cannot access system fonts in the browser sandbox. SKTypeface.FromFamilyName behavior is correct (falls back to available font). The workaround using FromStream/FromData is well-documented in the thread and self-resolved by the issue author. Related issue #3391 was also closed as completed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/WASM |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a Blazor WASM app with SkiaSharp.Views.Blazor
2. In OnPaintSurface, call SKTypeface.FromFamilyName("Tahoma", SKFontStyle.BoldItalic)
3. Print typeFace.FamilyName — observe 'Noto Mono' instead of 'Tahoma'
4. Draw text with the typeface — text renders in Noto Mono regardless of requested family

**Environment:** Blazor WASM, SkiaSharp.Views.Blazor, .NET 6

**Related issues:** #3391

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | typeFace.FamilyName always returns 'Noto Mono' instead of the requested family name in WASM |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | WASM font manager limitations are inherent to the WASM platform; they have not been fundamentally addressed in SkiaSharp. |

## Analysis

### Technical Summary

In WASM (Blazor WebAssembly), Skia's font manager cannot enumerate or load system fonts because the WASM sandbox has no access to browser/OS font directories. SKTypeface.FromFamilyName silently falls back to the default embedded font (Noto Mono) when the requested font is not found. This is expected WASM behavior but confusing for developers used to desktop/server platforms where system fonts are accessible.

### Rationale

Classified as type/bug because the API silently returns a fallback font without any error or warning — a reliability issue that is surprising and inconsistent with behavior on other platforms. The root cause is a WASM sandbox limitation, not a SkiaSharp code defect, making close-as-not-a-bug appropriate. The issue author self-resolved it in the comments using the stream workaround. Related issue #3391 had the same behavior and was closed as completed.

### Key Signals

- "typeFace: Noto Mono" — **issue body (console output after FromFamilyName call)** (FromFamilyName silently returns the default WASM font when system fonts are unavailable — no error, no null return.)
- "Skia's font manager is highly limited under wasm so you can't expect a functional font manager. It is better to load fonts explicitly via stream in a sandbox environment." — **comment by Gillibald (CONTRIBUTOR)** (Confirms this is a known WASM platform limitation; loading via stream is the established solution.)
- "The TTF file is added as an embedded resource - The font is loaded with SKTypeface.FromStream, reading the resource with Reflection. Issue resolved" — **comment by harveytriana (issue author)** (Author self-resolved by using FromStream with embedded fonts — the standard WASM workaround.)
- "This is a deal killer for using SkiaSharp on WASM if every font needs to be an embedded resource." — **comment by RChrisCoble** (Other users confirm impact; follow-up comment confirms HTTP fetch via JSInterop is also a viable solution.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 60-72 | direct | FromFamilyName calls sk_typeface_create_from_name via P/Invoke. In WASM, Skia's font manager has no system fonts to search; it silently returns the default typeface (Noto Mono) via GetObject. No null return, no exception — the fallback is completely silent. |
| `binding/SkiaSharp/SKTypeface.cs` | 92-113 | related | FromStream(Stream stream) wraps the stream in SKManagedStream, then converts it to a native memory stream via ToMemoryStream(). This path bypasses the font manager entirely and works correctly in WASM. Similarly, FromData(SKData data) at lines 115-121 uses native SKData directly, also bypassing font manager. |

### Workarounds

- Embed the .ttf font file as an EmbeddedResource in the project and load with SKTypeface.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("MyApp.Fonts.FontName.ttf"))
- Fetch font bytes via HttpClient.GetByteArrayAsync() and load with SKTypeface.FromData(SKData.CreateCopy(bytes))
- Use a separate assembly containing only font resources loaded on demand via Blazor's lazy assembly loading feature

### Next Questions

- Should SKTypeface.FromFamilyName return null (or throw) when the font manager can find nothing in WASM, rather than silently falling back?
- Is there a feasible way to register browser CSS fonts with Skia's font manager via JSInterop in WASM?

### Resolution Proposals

**Hypothesis:** WASM sandbox prevents access to system fonts; Skia silently returns its bundled default (Noto Mono) when the requested font family is unavailable via the font manager.

1. **Embed font as resource and load via stream** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Add the .ttf font file as an EmbeddedResource and use SKTypeface.FromStream to load it at runtime. Bypasses the font manager entirely.

```csharp
// Add MyApp.Fonts.Tahoma.ttf as EmbeddedResource in .csproj
var assembly = Assembly.GetExecutingAssembly();
using var stream = assembly.GetManifestResourceStream("MyApp.Fonts.Tahoma.ttf");
var typeface = SKTypeface.FromStream(stream) ?? SKTypeface.Default;
// Store 'typeface' as a field; dispose it when the component is disposed
```
2. **Fetch font from web service and load via SKData** — workaround, confidence 0.85 (85%), cost/s, validated=yes
   - Use HttpClient to download a font file from a web service or CDN and load it with SKTypeface.FromData. Fonts are cached by the browser when served with proper headers.

```csharp
// Inject HttpClient via DI in Blazor
var bytes = await httpClient.GetByteArrayAsync("https://fonts.example.com/tahoma.ttf");
using var data = SKData.CreateCopy(bytes);
var typeface = SKTypeface.FromData(data) ?? SKTypeface.Default;
// Store 'typeface' as a field; dispose it when the component is disposed
```

**Recommended proposal:** Embed font as resource and load via stream

**Why:** Simplest approach, no network requests, works offline, no external CDN dependency. Well-established pattern for WASM apps.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | This is a WASM platform limitation: Skia's font manager cannot access system fonts in the browser sandbox. SKTypeface.FromFamilyName behavior is correct (falls back to available font). The workaround using FromStream/FromData is well-documented in the thread and self-resolved by the issue author. Related issue #3391 was also closed as completed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp, os/WASM, tenet/reliability labels | labels=type/bug, area/SkiaSharp, os/WASM, tenet/reliability |
| add-comment | high | 0.85 (85%) | Post explanation of WASM font limitation with corrected workaround code examples | — |
| close-issue | medium | 0.80 (80%) | Close as not-a-bug — WASM sandbox limitation, workaround is well-documented | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! This is a known limitation of the WebAssembly platform. In WASM/Blazor, Skia's font manager has no access to system or browser fonts because the WASM sandbox has no filesystem path to font directories. When `SKTypeface.FromFamilyName` cannot find the requested font, it silently falls back to the default embedded font (Noto Mono).

**Workaround 1: Embed the font as an EmbeddedResource (recommended)**

Add the `.ttf` file to your project with `<EmbeddedResource>` in your `.csproj`, then:

```csharp
var assembly = Assembly.GetExecutingAssembly();
using var stream = assembly.GetManifestResourceStream("MyApp.Fonts.Tahoma.ttf");
var typeface = SKTypeface.FromStream(stream) ?? SKTypeface.Default;
// Store 'typeface' as a field; call Dispose() when the component is disposed
```

**Workaround 2: Fetch from a web service at runtime**

```csharp
// Inject HttpClient via DI
var bytes = await httpClient.GetByteArrayAsync("https://fonts.example.com/tahoma.ttf");
using var data = SKData.CreateCopy(bytes);
var typeface = SKTypeface.FromData(data) ?? SKTypeface.Default;
// Store 'typeface' as a field; call Dispose() when the component is disposed
```

Both approaches bypass the WASM font manager entirely by loading font binary data directly. This is the standard pattern for custom fonts in WASM apps.

Closing as not-a-bug since this is a fundamental WASM sandbox constraint — the behavior of `FromFamilyName` returning the default font is correct given the runtime environment.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1902,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:25:48Z"
  },
  "summary": "SKTypeface.FromFamilyName always returns the default font (Noto Mono) in Blazor WASM because Skia's font manager cannot access system fonts in the WASM sandbox; the workaround is to load fonts explicitly via SKTypeface.FromStream or SKTypeface.FromData.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
    },
    "platforms": [
      "os/WASM"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "typeFace.FamilyName always returns 'Noto Mono' instead of the requested family name in WASM",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Blazor WASM app with SkiaSharp.Views.Blazor",
        "In OnPaintSurface, call SKTypeface.FromFamilyName(\"Tahoma\", SKFontStyle.BoldItalic)",
        "Print typeFace.FamilyName — observe 'Noto Mono' instead of 'Tahoma'",
        "Draw text with the typeface — text renders in Noto Mono regardless of requested family"
      ],
      "environmentDetails": "Blazor WASM, SkiaSharp.Views.Blazor, .NET 6",
      "relatedIssues": [
        3391
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "WASM font manager limitations are inherent to the WASM platform; they have not been fundamentally addressed in SkiaSharp."
    }
  },
  "analysis": {
    "summary": "In WASM (Blazor WebAssembly), Skia's font manager cannot enumerate or load system fonts because the WASM sandbox has no access to browser/OS font directories. SKTypeface.FromFamilyName silently falls back to the default embedded font (Noto Mono) when the requested font is not found. This is expected WASM behavior but confusing for developers used to desktop/server platforms where system fonts are accessible.",
    "rationale": "Classified as type/bug because the API silently returns a fallback font without any error or warning — a reliability issue that is surprising and inconsistent with behavior on other platforms. The root cause is a WASM sandbox limitation, not a SkiaSharp code defect, making close-as-not-a-bug appropriate. The issue author self-resolved it in the comments using the stream workaround. Related issue #3391 had the same behavior and was closed as completed.",
    "keySignals": [
      {
        "text": "typeFace: Noto Mono",
        "source": "issue body (console output after FromFamilyName call)",
        "interpretation": "FromFamilyName silently returns the default WASM font when system fonts are unavailable — no error, no null return."
      },
      {
        "text": "Skia's font manager is highly limited under wasm so you can't expect a functional font manager. It is better to load fonts explicitly via stream in a sandbox environment.",
        "source": "comment by Gillibald (CONTRIBUTOR)",
        "interpretation": "Confirms this is a known WASM platform limitation; loading via stream is the established solution."
      },
      {
        "text": "The TTF file is added as an embedded resource - The font is loaded with SKTypeface.FromStream, reading the resource with Reflection. Issue resolved",
        "source": "comment by harveytriana (issue author)",
        "interpretation": "Author self-resolved by using FromStream with embedded fonts — the standard WASM workaround."
      },
      {
        "text": "This is a deal killer for using SkiaSharp on WASM if every font needs to be an embedded resource.",
        "source": "comment by RChrisCoble",
        "interpretation": "Other users confirm impact; follow-up comment confirms HTTP fetch via JSInterop is also a viable solution."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "60-72",
        "finding": "FromFamilyName calls sk_typeface_create_from_name via P/Invoke. In WASM, Skia's font manager has no system fonts to search; it silently returns the default typeface (Noto Mono) via GetObject. No null return, no exception — the fallback is completely silent.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "92-113",
        "finding": "FromStream(Stream stream) wraps the stream in SKManagedStream, then converts it to a native memory stream via ToMemoryStream(). This path bypasses the font manager entirely and works correctly in WASM. Similarly, FromData(SKData data) at lines 115-121 uses native SKData directly, also bypassing font manager.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Embed the .ttf font file as an EmbeddedResource in the project and load with SKTypeface.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(\"MyApp.Fonts.FontName.ttf\"))",
      "Fetch font bytes via HttpClient.GetByteArrayAsync() and load with SKTypeface.FromData(SKData.CreateCopy(bytes))",
      "Use a separate assembly containing only font resources loaded on demand via Blazor's lazy assembly loading feature"
    ],
    "nextQuestions": [
      "Should SKTypeface.FromFamilyName return null (or throw) when the font manager can find nothing in WASM, rather than silently falling back?",
      "Is there a feasible way to register browser CSS fonts with Skia's font manager via JSInterop in WASM?"
    ],
    "resolution": {
      "hypothesis": "WASM sandbox prevents access to system fonts; Skia silently returns its bundled default (Noto Mono) when the requested font family is unavailable via the font manager.",
      "proposals": [
        {
          "title": "Embed font as resource and load via stream",
          "description": "Add the .ttf font file as an EmbeddedResource and use SKTypeface.FromStream to load it at runtime. Bypasses the font manager entirely.",
          "category": "workaround",
          "codeSnippet": "// Add MyApp.Fonts.Tahoma.ttf as EmbeddedResource in .csproj\nvar assembly = Assembly.GetExecutingAssembly();\nusing var stream = assembly.GetManifestResourceStream(\"MyApp.Fonts.Tahoma.ttf\");\nvar typeface = SKTypeface.FromStream(stream) ?? SKTypeface.Default;\n// Store 'typeface' as a field; dispose it when the component is disposed",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Fetch font from web service and load via SKData",
          "description": "Use HttpClient to download a font file from a web service or CDN and load it with SKTypeface.FromData. Fonts are cached by the browser when served with proper headers.",
          "category": "workaround",
          "codeSnippet": "// Inject HttpClient via DI in Blazor\nvar bytes = await httpClient.GetByteArrayAsync(\"https://fonts.example.com/tahoma.ttf\");\nusing var data = SKData.CreateCopy(bytes);\nvar typeface = SKTypeface.FromData(data) ?? SKTypeface.Default;\n// Store 'typeface' as a field; dispose it when the component is disposed",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Embed font as resource and load via stream",
      "recommendedReason": "Simplest approach, no network requests, works offline, no external CDN dependency. Well-established pattern for WASM apps."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "This is a WASM platform limitation: Skia's font manager cannot access system fonts in the browser sandbox. SKTypeface.FromFamilyName behavior is correct (falls back to available font). The workaround using FromStream/FromData is well-documented in the thread and self-resolved by the issue author. Related issue #3391 was also closed as completed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/WASM, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/WASM",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post explanation of WASM font limitation with corrected workaround code examples",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report! This is a known limitation of the WebAssembly platform. In WASM/Blazor, Skia's font manager has no access to system or browser fonts because the WASM sandbox has no filesystem path to font directories. When `SKTypeface.FromFamilyName` cannot find the requested font, it silently falls back to the default embedded font (Noto Mono).\n\n**Workaround 1: Embed the font as an EmbeddedResource (recommended)**\n\nAdd the `.ttf` file to your project with `<EmbeddedResource>` in your `.csproj`, then:\n\n```csharp\nvar assembly = Assembly.GetExecutingAssembly();\nusing var stream = assembly.GetManifestResourceStream(\"MyApp.Fonts.Tahoma.ttf\");\nvar typeface = SKTypeface.FromStream(stream) ?? SKTypeface.Default;\n// Store 'typeface' as a field; call Dispose() when the component is disposed\n```\n\n**Workaround 2: Fetch from a web service at runtime**\n\n```csharp\n// Inject HttpClient via DI\nvar bytes = await httpClient.GetByteArrayAsync(\"https://fonts.example.com/tahoma.ttf\");\nusing var data = SKData.CreateCopy(bytes);\nvar typeface = SKTypeface.FromData(data) ?? SKTypeface.Default;\n// Store 'typeface' as a field; call Dispose() when the component is disposed\n```\n\nBoth approaches bypass the WASM font manager entirely by loading font binary data directly. This is the standard pattern for custom fonts in WASM apps.\n\nClosing as not-a-bug since this is a fundamental WASM sandbox constraint — the behavior of `FromFamilyName` returning the default font is correct given the runtime environment."
      },
      {
        "type": "close-issue",
        "description": "Close as not-a-bug — WASM sandbox limitation, workaround is well-documented",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
