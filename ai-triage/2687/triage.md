# Issue Triage Report — #2687

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T11:53:21Z |
| Type | type/bug (0.75 (75%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** SKFontManager.MatchFamily returns incorrect typeface for 'Noto Mono' on macOS 12.

**Analysis:** The reporter claims that SKFontManager (via Avalonia's font matching code on macOS 12) does not correctly match the 'Noto Mono' font family using SkiaSharp 2.88.6. The actual behavior was not described and no minimal SkiaSharp repro was provided. The issue likely stems from how the macOS CoreText font manager resolves family names for third-party fonts like Noto Mono, possibly returning a fallback font instead of null or the correct match.

**Recommendations:** **needs-info** — The actual behavior field is empty. All repro code references Avalonia's font management layer, not SkiaSharp directly. Cannot determine if the bug is in SkiaSharp or Avalonia without a minimal SkiaSharp-level repro.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/macOS |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/Coloryr/Avalonia/blob/114cd1c0d04e763be56fe7f8facec8943778d0f3/src/Avalonia.Base/Media/Fonts/SystemFontCollection.cs — Avalonia SystemFontCollection code referenced by reporter
- https://github.com/Coloryr/Avalonia/blob/114cd1c0d04e763be56fe7f8facec8943778d0f3/src/Skia/Avalonia.Skia/FontManagerImpl.cs — Avalonia FontManagerImpl code (line 84) referenced by reporter
- https://github.com/AvaloniaUI/Avalonia/issues/13585 — Avalonia upstream issue
- https://github.com/AvaloniaUI/Avalonia/pull/13593 — Avalonia fix PR

**Screenshots:**
- https://github.com/mono/SkiaSharp/assets/26276037/cf6646ef-9361-4564-a4f4-2e92fb042dd3 — Screenshot from reporter showing font issue

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.6 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | No last-known-good version provided; issue may still exist in current SkiaSharp releases. |

## Analysis

### Technical Summary

The reporter claims that SKFontManager (via Avalonia's font matching code on macOS 12) does not correctly match the 'Noto Mono' font family using SkiaSharp 2.88.6. The actual behavior was not described and no minimal SkiaSharp repro was provided. The issue likely stems from how the macOS CoreText font manager resolves family names for third-party fonts like Noto Mono, possibly returning a fallback font instead of null or the correct match.

### Rationale

Classified as type/bug at medium confidence because the reported behavior (incorrect font matching on macOS) is consistent with known Skia CoreText backend limitations, but the actual behavior section is empty and no minimal SkiaSharp reproduction exists. The fix was attempted in Avalonia rather than SkiaSharp, making it unclear if the root cause is in SkiaSharp itself. Needs-info because actual behavior was not described and no SkiaSharp-level repro was provided.

### Key Signals

- "this issus find in https://github.com/AvaloniaUI/Avalonia/issues/13585 and try fix in https://github.com/AvaloniaUI/Avalonia/pull/13593" — **issue body** (The fix was attempted in Avalonia's codebase rather than SkiaSharp, suggesting the root cause may be in how SkiaSharp exposes font matching or it may be entirely in Avalonia.)
- "Font match to Noto Mono" — **expected behavior** (Third-party fonts like Noto Mono may not be resolved correctly on macOS by Skia's CoreText backend.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKFontManager.cs` | 74-87 | direct | SKFontManager.MatchFamily(string, SKFontStyle) delegates directly to sk_fontmgr_match_family_style, passing the family name as UTF-8 bytes. No macOS-specific handling or normalization of the family name is performed at the C# layer. |
| `binding/SkiaSharp/SKFontManager.cs` | 176-191 | related | MatchCharacter has a workaround comment for a Skia bug (https://bugs.chromium.org/p/skia/issues/detail?id=6196) converting null family to empty string. MatchFamily does not have an equivalent workaround. |

### Next Questions

- What typeface is actually returned when 'Noto Mono' is requested — null or a different font?
- Does the issue reproduce with a direct SkiaSharp call to SKFontManager.Default.MatchFamily('Noto Mono')?
- Is Noto Mono installed via a package manager or bundled within the app?

### Resolution Proposals

1. **Request minimal SkiaSharp repro** — investigation, cost/xs, validated=untested
   - Ask reporter to provide a direct SkiaSharp test (no Avalonia) calling SKFontManager.Default.MatchFamily('Noto Mono') and logging the returned typeface's FamilyName on macOS 12.

**Recommended proposal:** Request minimal SkiaSharp repro

**Why:** The actual behavior field is empty and the repro goes through Avalonia's font manager layer, making it impossible to determine if the root cause is in SkiaSharp without a direct repro.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | The actual behavior field is empty. All repro code references Avalonia's font management layer, not SkiaSharp directly. Cannot determine if the bug is in SkiaSharp or Avalonia without a minimal SkiaSharp-level repro. |
| Suggested repro platform | macos |

### Missing Info

- Actual behavior: what font (or null) is returned when searching for 'Noto Mono'?
- Minimal SkiaSharp repro without Avalonia dependency
- Confirmation that Noto Mono is installed as a system font on the test machine

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply type/bug, area/SkiaSharp, os/macOS labels | labels=type/bug, area/SkiaSharp, os/macOS |
| add-comment | medium | 0.80 (80%) | Request minimal repro and actual behavior details | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! To investigate this further, we need a bit more information:

1. **Actual behavior**: What is returned when searching for 'Noto Mono' — is `MatchFamily` returning `null`, a fallback font, or a wrong typeface? You can check `typeface.FamilyName` on the result.

2. **Minimal repro**: Can you reproduce this with a direct SkiaSharp call (without Avalonia)?
```csharp
var typeface = SKFontManager.Default.MatchFamily("Noto Mono");
Console.WriteLine(typeface?.FamilyName ?? "null");
```

3. **Font installation**: Is Noto Mono installed as a system font, or is it being loaded from a file/bundle?

This will help us determine if the issue is in SkiaSharp's macOS font manager or in Avalonia's font matching layer.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2687,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T11:53:21Z"
  },
  "summary": "SKFontManager.MatchFamily returns incorrect typeface for 'Noto Mono' on macOS 12.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.75
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
    },
    "platforms": [
      "os/macOS"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "partial"
    },
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/Coloryr/Avalonia/blob/114cd1c0d04e763be56fe7f8facec8943778d0f3/src/Avalonia.Base/Media/Fonts/SystemFontCollection.cs",
          "description": "Avalonia SystemFontCollection code referenced by reporter"
        },
        {
          "url": "https://github.com/Coloryr/Avalonia/blob/114cd1c0d04e763be56fe7f8facec8943778d0f3/src/Skia/Avalonia.Skia/FontManagerImpl.cs",
          "description": "Avalonia FontManagerImpl code (line 84) referenced by reporter"
        },
        {
          "url": "https://github.com/AvaloniaUI/Avalonia/issues/13585",
          "description": "Avalonia upstream issue"
        },
        {
          "url": "https://github.com/AvaloniaUI/Avalonia/pull/13593",
          "description": "Avalonia fix PR"
        }
      ],
      "screenshots": [
        {
          "url": "https://github.com/mono/SkiaSharp/assets/26276037/cf6646ef-9361-4564-a4f4-2e92fb042dd3",
          "description": "Screenshot from reporter showing font issue"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.6"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "No last-known-good version provided; issue may still exist in current SkiaSharp releases."
    }
  },
  "analysis": {
    "summary": "The reporter claims that SKFontManager (via Avalonia's font matching code on macOS 12) does not correctly match the 'Noto Mono' font family using SkiaSharp 2.88.6. The actual behavior was not described and no minimal SkiaSharp repro was provided. The issue likely stems from how the macOS CoreText font manager resolves family names for third-party fonts like Noto Mono, possibly returning a fallback font instead of null or the correct match.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "74-87",
        "finding": "SKFontManager.MatchFamily(string, SKFontStyle) delegates directly to sk_fontmgr_match_family_style, passing the family name as UTF-8 bytes. No macOS-specific handling or normalization of the family name is performed at the C# layer.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "176-191",
        "finding": "MatchCharacter has a workaround comment for a Skia bug (https://bugs.chromium.org/p/skia/issues/detail?id=6196) converting null family to empty string. MatchFamily does not have an equivalent workaround.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "this issus find in https://github.com/AvaloniaUI/Avalonia/issues/13585 and try fix in https://github.com/AvaloniaUI/Avalonia/pull/13593",
        "source": "issue body",
        "interpretation": "The fix was attempted in Avalonia's codebase rather than SkiaSharp, suggesting the root cause may be in how SkiaSharp exposes font matching or it may be entirely in Avalonia."
      },
      {
        "text": "Font match to Noto Mono",
        "source": "expected behavior",
        "interpretation": "Third-party fonts like Noto Mono may not be resolved correctly on macOS by Skia's CoreText backend."
      }
    ],
    "rationale": "Classified as type/bug at medium confidence because the reported behavior (incorrect font matching on macOS) is consistent with known Skia CoreText backend limitations, but the actual behavior section is empty and no minimal SkiaSharp reproduction exists. The fix was attempted in Avalonia rather than SkiaSharp, making it unclear if the root cause is in SkiaSharp itself. Needs-info because actual behavior was not described and no SkiaSharp-level repro was provided.",
    "nextQuestions": [
      "What typeface is actually returned when 'Noto Mono' is requested — null or a different font?",
      "Does the issue reproduce with a direct SkiaSharp call to SKFontManager.Default.MatchFamily('Noto Mono')?",
      "Is Noto Mono installed via a package manager or bundled within the app?"
    ],
    "resolution": {
      "proposals": [
        {
          "category": "investigation",
          "title": "Request minimal SkiaSharp repro",
          "description": "Ask reporter to provide a direct SkiaSharp test (no Avalonia) calling SKFontManager.Default.MatchFamily('Noto Mono') and logging the returned typeface's FamilyName on macOS 12.",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Request minimal SkiaSharp repro",
      "recommendedReason": "The actual behavior field is empty and the repro goes through Avalonia's font manager layer, making it impossible to determine if the root cause is in SkiaSharp without a direct repro."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "The actual behavior field is empty. All repro code references Avalonia's font management layer, not SkiaSharp directly. Cannot determine if the bug is in SkiaSharp or Avalonia without a minimal SkiaSharp-level repro.",
      "suggestedReproPlatform": "macos"
    },
    "missingInfo": [
      "Actual behavior: what font (or null) is returned when searching for 'Noto Mono'?",
      "Minimal SkiaSharp repro without Avalonia dependency",
      "Confirmation that Noto Mono is installed as a system font on the test machine"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/macOS labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/macOS"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request minimal repro and actual behavior details",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the report! To investigate this further, we need a bit more information:\n\n1. **Actual behavior**: What is returned when searching for 'Noto Mono' — is `MatchFamily` returning `null`, a fallback font, or a wrong typeface? You can check `typeface.FamilyName` on the result.\n\n2. **Minimal repro**: Can you reproduce this with a direct SkiaSharp call (without Avalonia)?\n```csharp\nvar typeface = SKFontManager.Default.MatchFamily(\"Noto Mono\");\nConsole.WriteLine(typeface?.FamilyName ?? \"null\");\n```\n\n3. **Font installation**: Is Noto Mono installed as a system font, or is it being loaded from a file/bundle?\n\nThis will help us determine if the issue is in SkiaSharp's macOS font manager or in Avalonia's font matching layer."
      }
    ]
  }
}
```

</details>
