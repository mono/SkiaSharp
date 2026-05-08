# Issue Triage Report — #1279

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T05:55:00Z |
| Type | type/bug (0.78 (78%)) |
| Area | area/SkiaSharp (0.88 (88%)) |
| Suggested action | needs-investigation (0.72 (72%)) |

**Issue Summary:** SKTypeface.FromFamilyName returns 'Euclid Flex Semibolditalic' instead of 'Euclid Flex Semibold' when running on Linux (Docker), likely due to a bug in the user's font-name-stripping helper and fontconfig returning an unexpected match.

**Analysis:** The reporter's GetTypeface helper function has a logic bug: it checks fontName.Contains("Bold") before fontName.Contains("Semibold"), so 'Semibold' matches the Bold branch first, stripping 'Bold' from 'Semibold' to produce 'Euclid Flex Semi' as the trimmed family name. Independently, on Linux via fontconfig, SKTypeface.FromFamilyName with 'Euclid Flex Semibold' may fall back to 'Euclid Flex Semibolditalic' if the font's internal metadata registers 'Semibolditalic' as a separate family — which is unusual but valid for OTF fonts. The primary fix is to load fonts directly from file using SKTypeface.FromFile() to bypass fontconfig matching entirely.

**Recommendations:** **needs-investigation** — Reporter has provided Dockerfile and sample link. Two issues are intertwined: a user code bug in GetTypeface and a potential fontconfig matching discrepancy. A minimal repro calling FromFamilyName directly (without the GetTypeface wrapper) is needed to isolate whether the underlying fontconfig matching is also wrong.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Linux |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Install Euclid Flex fonts in a Docker Linux container using fontconfig (fc-cache -f -v)
2. Call GetTypeface("Euclid Flex Semibold", SKTypefaceStyle.Normal) which internally calls SKTypeface.FromFamilyName
3. Observe that the returned typeface has FamilyName = 'Euclid Flex Semibolditalic'

**Environment:** SkiaSharp 1.68.1.1, .NET Core 3.0/3.1, Docker on Windows 10, base image mcr.microsoft.com/dotnet/core/runtime:3.1, libfontconfig installed via apt

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2170 — Related: SKTypeface FontWeight wrong when using OpenStream on Linux vs Windows — same class of Linux/fontconfig font matching discrepancy
- https://github.com/mono/SkiaSharp/issues/1131 — Related closed: Can't draw text using default typeface on Linux/Docker — resolved by installing fontconfig

**Code snippets:**

```csharp
SKTypeface typeface = GetTypeface("Euclid Flex Semibold", SKTypefaceStyle.Normal);
// Expected: typeface.FamilyName == "Euclid Flex Semibold"
// Actual: typeface.FamilyName == "Euclid Flex Semibolditalic"
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Created font name is 'Euclid Flex Semibolditalic' instead of expected 'Euclid Flex Semibold' |
| Repro quality | partial |
| Target frameworks | netcoreapp3.0, netcoreapp3.1 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.1.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKTypeface.FromFamilyName still delegates to sk_typeface_create_from_name which uses fontconfig on Linux. The font-name matching behavior has not fundamentally changed. |

## Analysis

### Technical Summary

The reporter's GetTypeface helper function has a logic bug: it checks fontName.Contains("Bold") before fontName.Contains("Semibold"), so 'Semibold' matches the Bold branch first, stripping 'Bold' from 'Semibold' to produce 'Euclid Flex Semi' as the trimmed family name. Independently, on Linux via fontconfig, SKTypeface.FromFamilyName with 'Euclid Flex Semibold' may fall back to 'Euclid Flex Semibolditalic' if the font's internal metadata registers 'Semibolditalic' as a separate family — which is unusual but valid for OTF fonts. The primary fix is to load fonts directly from file using SKTypeface.FromFile() to bypass fontconfig matching entirely.

### Rationale

Classified as type/bug because wrong output (wrong font name) is returned — the reporter expects a specific typeface and gets a different one. Classified as area/SkiaSharp because the issue involves SKTypeface behavior. os/Linux because the problem is specific to the Linux/Docker environment (Docker base image uses Linux, Windows host is irrelevant). Severity is medium because the font file is present and can be loaded directly as a workaround.

### Key Signals

- "The create font name is 'Euclid Flex Semibolditalic'" — **issue body — Actual Behavior** (Wrong typeface returned — fontconfig may be matching a different font variant, or the font's metadata has an unusual family name)
- "RUN apt-get update -y && apt-get install -y libfontconfig fontconfig" — **comment #4 — Dockerfile** (Fontconfig is properly installed; fonts are copied to /usr/share/fonts/ and fc-cache is run — font loading infrastructure is set up correctly)
- "COPY ./fonts/Euclid/ /usr/share/fonts/" — **comment #4 — Dockerfile** (Font files are present in the container — issue is in name matching/resolution, not missing fonts)
- "it found the font but it is not exactly what we need" — **comment #7** (Confirms the bug is wrong-output: a font is found but the wrong variant is returned)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 60-72 | direct | SKTypeface.FromFamilyName(string, SKFontStyle) calls sk_typeface_create_from_name via P/Invoke. On Linux this delegates to the Skia fontconfig-based font manager. There is no SkiaSharp-level name normalization — the family name string is passed through directly to the native layer. |
| `binding/SkiaSharp/SKTypeface.cs` | 74-77 | direct | The overload SKTypeface.FromFamilyName(string, SKFontStyleWeight, SKFontStyleWidth, SKFontStyleSlant) simply casts the enum values to int and delegates — no validation or name normalization is performed in SkiaSharp's managed layer. |
| `binding/SkiaSharp/SKFontManager.cs` | 1-10 | related | SKFontManager.MatchFamily(string, SKFontStyle) provides an alternative font lookup path via the font manager, which can enumerate available families. This could be used as an alternative approach to resolve fonts by family name. |

### Workarounds

- Use SKTypeface.FromFile('/path/to/semibold.otf') to load the font directly from the known file path — bypasses fontconfig name matching entirely
- Fix the GetTypeface helper to check for 'Semibold' before 'Bold' so the Bold branch doesn't consume 'Semibold'
- Use SKFontManager.Default.MatchFamily(familyName, style) to enumerate available families and find the closest match

### Next Questions

- What does fc-list show for the installed Euclid Flex fonts? Is the family name registered as 'Euclid Flex Semibolditalic' in fontconfig?
- Does the issue reproduce with a minimal repro that only calls SKTypeface.FromFamilyName('Euclid Flex Semibold', ...) directly without the GetTypeface wrapper?
- Does the font's PostScript name differ from its Family name in the OTF metadata?

### Resolution Proposals

**Hypothesis:** The reporter's GetTypeface helper has a logic bug (Bold check precedes Semibold check), and independently fontconfig may map 'Euclid Flex Semibold' to 'Euclid Flex Semibolditalic' if the font file's internal family name metadata is non-standard. Using SKTypeface.FromFile() avoids both issues.

1. **Load font directly from file path** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Use SKTypeface.FromFile() with the known font path instead of name-based lookup via fontconfig. This is the most reliable approach in a Docker environment where font file paths are controlled.

```csharp
// Instead of: SKTypeface.FromFamilyName("Euclid Flex Semibold", ...)
// Use the direct file path:
SKTypeface typeface = SKTypeface.FromFile(Path.Combine(fontDirectory, "semibold.otf"));
Console.WriteLine(typeface.FamilyName); // Will show the correct family name
```
2. **Fix GetTypeface helper ordering bug** — fix, confidence 0.85 (85%), cost/xs, validated=yes
   - Move the 'Semibold' check before the 'Bold' check in the GetTypeface helper to prevent 'Semibold' from being consumed by the Bold branch. Note: this fixes the user code bug but does not address the underlying fontconfig matching issue.

```csharp
// Fix: check Semibold BEFORE Bold
if (fontName.Contains("Semibold"))
{
    sKFontStyleWeight = SKFontStyleWeight.SemiBold;
    fontName = fontName.Replace("Semibold", string.Empty).Trim();
}
else if (style == SKTypefaceStyle.Bold || fontName.Contains("Bold"))
{
    sKFontStyleWeight = SKFontStyleWeight.Bold;
    if (fontName.Contains("Bold"))
        fontName = fontName.Replace("Bold", string.Empty).Trim();
}
```

**Recommended proposal:** Load font directly from file path

**Why:** The FromFile approach completely bypasses fontconfig name resolution and is robust to font metadata variations. The GetTypeface fix addresses the user code bug but still relies on fontconfig matching which may behave differently between platforms.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.72 (72%) |
| Reason | Reporter has provided Dockerfile and sample link. Two issues are intertwined: a user code bug in GetTypeface and a potential fontconfig matching discrepancy. A minimal repro calling FromFamilyName directly (without the GetTypeface wrapper) is needed to isolate whether the underlying fontconfig matching is also wrong. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply type/bug, area/SkiaSharp, os/Linux labels | labels=type/bug, area/SkiaSharp, os/Linux |
| add-comment | medium | 0.82 (82%) | Post analysis with identified user code bug and FromFile workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the Dockerfile and sample.

After reviewing the code, I found two issues:

**1. Bug in the `GetTypeface` helper (user code)**

The `GetTypeface` function checks `fontName.Contains("Bold")` before checking `fontName.Contains("Semibold")`. Since `"Semibold"` contains `"Bold"`, the Bold branch fires first and strips `"Bold"` from `"Semibold"`, leaving `"Euclid Flex Semi"` as the family name. The `SemiBold` branch is never reached.

Fix: move the `Semibold` check *before* the `Bold` check:

```csharp
if (fontName.Contains("Semibold"))
{
    sKFontStyleWeight = SKFontStyleWeight.SemiBold;
    fontName = fontName.Replace("Semibold", string.Empty).Trim();
}
else if (style == SKTypefaceStyle.Bold || fontName.Contains("Bold"))
{
    sKFontStyleWeight = SKFontStyleWeight.Bold;
    if (fontName.Contains("Bold"))
        fontName = fontName.Replace("Bold", string.Empty).Trim();
}
```

**2. Workaround: load fonts directly from file**

In a Docker environment where you control font file locations, the most reliable approach is to bypass fontconfig name matching entirely:

```csharp
// Replace name-based lookup with direct file loading
SKTypeface typeface = SKTypeface.FromFile(Path.Combine(fontDirectory, "semibold.otf"));
```

This eliminates any platform difference between Windows (GDI+) and Linux (fontconfig) font name resolution.

Could you also run `fc-list | grep -i euclid` in your container to share the registered family names? That will help determine whether fontconfig is registering the font as `"Euclid Flex Semibolditalic"` in its metadata.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1279,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T05:55:00Z"
  },
  "summary": "SKTypeface.FromFamilyName returns 'Euclid Flex Semibolditalic' instead of 'Euclid Flex Semibold' when running on Linux (Docker), likely due to a bug in the user's font-name-stripping helper and fontconfig returning an unexpected match.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.78
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.88
    },
    "platforms": [
      "os/Linux"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Created font name is 'Euclid Flex Semibolditalic' instead of expected 'Euclid Flex Semibold'",
      "reproQuality": "partial",
      "targetFrameworks": [
        "netcoreapp3.0",
        "netcoreapp3.1"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Install Euclid Flex fonts in a Docker Linux container using fontconfig (fc-cache -f -v)",
        "Call GetTypeface(\"Euclid Flex Semibold\", SKTypefaceStyle.Normal) which internally calls SKTypeface.FromFamilyName",
        "Observe that the returned typeface has FamilyName = 'Euclid Flex Semibolditalic'"
      ],
      "codeSnippets": [
        "SKTypeface typeface = GetTypeface(\"Euclid Flex Semibold\", SKTypefaceStyle.Normal);\n// Expected: typeface.FamilyName == \"Euclid Flex Semibold\"\n// Actual: typeface.FamilyName == \"Euclid Flex Semibolditalic\""
      ],
      "environmentDetails": "SkiaSharp 1.68.1.1, .NET Core 3.0/3.1, Docker on Windows 10, base image mcr.microsoft.com/dotnet/core/runtime:3.1, libfontconfig installed via apt",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2170",
          "description": "Related: SKTypeface FontWeight wrong when using OpenStream on Linux vs Windows — same class of Linux/fontconfig font matching discrepancy"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1131",
          "description": "Related closed: Can't draw text using default typeface on Linux/Docker — resolved by installing fontconfig"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.1.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKTypeface.FromFamilyName still delegates to sk_typeface_create_from_name which uses fontconfig on Linux. The font-name matching behavior has not fundamentally changed."
    }
  },
  "analysis": {
    "summary": "The reporter's GetTypeface helper function has a logic bug: it checks fontName.Contains(\"Bold\") before fontName.Contains(\"Semibold\"), so 'Semibold' matches the Bold branch first, stripping 'Bold' from 'Semibold' to produce 'Euclid Flex Semi' as the trimmed family name. Independently, on Linux via fontconfig, SKTypeface.FromFamilyName with 'Euclid Flex Semibold' may fall back to 'Euclid Flex Semibolditalic' if the font's internal metadata registers 'Semibolditalic' as a separate family — which is unusual but valid for OTF fonts. The primary fix is to load fonts directly from file using SKTypeface.FromFile() to bypass fontconfig matching entirely.",
    "rationale": "Classified as type/bug because wrong output (wrong font name) is returned — the reporter expects a specific typeface and gets a different one. Classified as area/SkiaSharp because the issue involves SKTypeface behavior. os/Linux because the problem is specific to the Linux/Docker environment (Docker base image uses Linux, Windows host is irrelevant). Severity is medium because the font file is present and can be loaded directly as a workaround.",
    "keySignals": [
      {
        "text": "The create font name is 'Euclid Flex Semibolditalic'",
        "source": "issue body — Actual Behavior",
        "interpretation": "Wrong typeface returned — fontconfig may be matching a different font variant, or the font's metadata has an unusual family name"
      },
      {
        "text": "RUN apt-get update -y && apt-get install -y libfontconfig fontconfig",
        "source": "comment #4 — Dockerfile",
        "interpretation": "Fontconfig is properly installed; fonts are copied to /usr/share/fonts/ and fc-cache is run — font loading infrastructure is set up correctly"
      },
      {
        "text": "COPY ./fonts/Euclid/ /usr/share/fonts/",
        "source": "comment #4 — Dockerfile",
        "interpretation": "Font files are present in the container — issue is in name matching/resolution, not missing fonts"
      },
      {
        "text": "it found the font but it is not exactly what we need",
        "source": "comment #7",
        "interpretation": "Confirms the bug is wrong-output: a font is found but the wrong variant is returned"
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "60-72",
        "finding": "SKTypeface.FromFamilyName(string, SKFontStyle) calls sk_typeface_create_from_name via P/Invoke. On Linux this delegates to the Skia fontconfig-based font manager. There is no SkiaSharp-level name normalization — the family name string is passed through directly to the native layer.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "74-77",
        "finding": "The overload SKTypeface.FromFamilyName(string, SKFontStyleWeight, SKFontStyleWidth, SKFontStyleSlant) simply casts the enum values to int and delegates — no validation or name normalization is performed in SkiaSharp's managed layer.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "1-10",
        "finding": "SKFontManager.MatchFamily(string, SKFontStyle) provides an alternative font lookup path via the font manager, which can enumerate available families. This could be used as an alternative approach to resolve fonts by family name.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use SKTypeface.FromFile('/path/to/semibold.otf') to load the font directly from the known file path — bypasses fontconfig name matching entirely",
      "Fix the GetTypeface helper to check for 'Semibold' before 'Bold' so the Bold branch doesn't consume 'Semibold'",
      "Use SKFontManager.Default.MatchFamily(familyName, style) to enumerate available families and find the closest match"
    ],
    "nextQuestions": [
      "What does fc-list show for the installed Euclid Flex fonts? Is the family name registered as 'Euclid Flex Semibolditalic' in fontconfig?",
      "Does the issue reproduce with a minimal repro that only calls SKTypeface.FromFamilyName('Euclid Flex Semibold', ...) directly without the GetTypeface wrapper?",
      "Does the font's PostScript name differ from its Family name in the OTF metadata?"
    ],
    "resolution": {
      "hypothesis": "The reporter's GetTypeface helper has a logic bug (Bold check precedes Semibold check), and independently fontconfig may map 'Euclid Flex Semibold' to 'Euclid Flex Semibolditalic' if the font file's internal family name metadata is non-standard. Using SKTypeface.FromFile() avoids both issues.",
      "proposals": [
        {
          "title": "Load font directly from file path",
          "description": "Use SKTypeface.FromFile() with the known font path instead of name-based lookup via fontconfig. This is the most reliable approach in a Docker environment where font file paths are controlled.",
          "category": "workaround",
          "codeSnippet": "// Instead of: SKTypeface.FromFamilyName(\"Euclid Flex Semibold\", ...)\n// Use the direct file path:\nSKTypeface typeface = SKTypeface.FromFile(Path.Combine(fontDirectory, \"semibold.otf\"));\nConsole.WriteLine(typeface.FamilyName); // Will show the correct family name",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Fix GetTypeface helper ordering bug",
          "description": "Move the 'Semibold' check before the 'Bold' check in the GetTypeface helper to prevent 'Semibold' from being consumed by the Bold branch. Note: this fixes the user code bug but does not address the underlying fontconfig matching issue.",
          "category": "fix",
          "codeSnippet": "// Fix: check Semibold BEFORE Bold\nif (fontName.Contains(\"Semibold\"))\n{\n    sKFontStyleWeight = SKFontStyleWeight.SemiBold;\n    fontName = fontName.Replace(\"Semibold\", string.Empty).Trim();\n}\nelse if (style == SKTypefaceStyle.Bold || fontName.Contains(\"Bold\"))\n{\n    sKFontStyleWeight = SKFontStyleWeight.Bold;\n    if (fontName.Contains(\"Bold\"))\n        fontName = fontName.Replace(\"Bold\", string.Empty).Trim();\n}",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Load font directly from file path",
      "recommendedReason": "The FromFile approach completely bypasses fontconfig name resolution and is robust to font metadata variations. The GetTypeface fix addresses the user code bug but still relies on fontconfig matching which may behave differently between platforms."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.72,
      "reason": "Reporter has provided Dockerfile and sample link. Two issues are intertwined: a user code bug in GetTypeface and a potential fontconfig matching discrepancy. A minimal repro calling FromFamilyName directly (without the GetTypeface wrapper) is needed to isolate whether the underlying fontconfig matching is also wrong.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Linux labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Linux"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis with identified user code bug and FromFile workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the Dockerfile and sample.\n\nAfter reviewing the code, I found two issues:\n\n**1. Bug in the `GetTypeface` helper (user code)**\n\nThe `GetTypeface` function checks `fontName.Contains(\"Bold\")` before checking `fontName.Contains(\"Semibold\")`. Since `\"Semibold\"` contains `\"Bold\"`, the Bold branch fires first and strips `\"Bold\"` from `\"Semibold\"`, leaving `\"Euclid Flex Semi\"` as the family name. The `SemiBold` branch is never reached.\n\nFix: move the `Semibold` check *before* the `Bold` check:\n\n```csharp\nif (fontName.Contains(\"Semibold\"))\n{\n    sKFontStyleWeight = SKFontStyleWeight.SemiBold;\n    fontName = fontName.Replace(\"Semibold\", string.Empty).Trim();\n}\nelse if (style == SKTypefaceStyle.Bold || fontName.Contains(\"Bold\"))\n{\n    sKFontStyleWeight = SKFontStyleWeight.Bold;\n    if (fontName.Contains(\"Bold\"))\n        fontName = fontName.Replace(\"Bold\", string.Empty).Trim();\n}\n```\n\n**2. Workaround: load fonts directly from file**\n\nIn a Docker environment where you control font file locations, the most reliable approach is to bypass fontconfig name matching entirely:\n\n```csharp\n// Replace name-based lookup with direct file loading\nSKTypeface typeface = SKTypeface.FromFile(Path.Combine(fontDirectory, \"semibold.otf\"));\n```\n\nThis eliminates any platform difference between Windows (GDI+) and Linux (fontconfig) font name resolution.\n\nCould you also run `fc-list | grep -i euclid` in your container to share the registered family names? That will help determine whether fontconfig is registering the font as `\"Euclid Flex Semibolditalic\"` in its metadata."
      }
    ]
  }
}
```

</details>
