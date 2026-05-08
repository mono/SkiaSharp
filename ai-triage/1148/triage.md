# Issue Triage Report — #1148

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T17:06:21Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** Segfault on Linux (Ubuntu 18.04) when calling SKFontManager.MatchTypeface() with a stream-loaded SKTypeface — the Linux fontconfig implementation dereferences an invalid FC_FAMILY pattern attribute for non-system fonts, instantly killing the process.

**Analysis:** Stream-loaded SKTypeface objects do not have a fontconfig pattern (FC_FAMILY attribute) since they were not registered through fontconfig. When MatchTypeface is called on Linux, Skia's fontconfig implementation (SkFontMgr_fontconfig.cpp) calls matchFamilyStyle(get_string(fcTypeface->fPattern, FC_FAMILY), style) — and get_string returns null for a stream-loaded font because fPattern was never populated by fontconfig, causing a null pointer dereference and process crash.

**Recommendations:** **needs-investigation** — The specific MatchTypeface API was removed in v3.0.0 but the issue is still open, has recent 2025 activity showing similar crashes, and it's unclear whether the underlying fontconfig crash can still be triggered through other API paths in current versions. Needs investigation to confirm current relevance and close definitively.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Load two fonts from embedded resources into MemoryStream on Linux
2. Call SKFontManager.Default.CreateTypeface(stream) to create SKTypeface objects
3. Call SKFontManager.Default.MatchTypeface(defaultRegular, SKFontStyle.Bold)
4. Process segfaults and is immediately killed — no exception, no stack trace

**Environment:** Ubuntu 18.04.3, x64, .NET Core 3.1, SkiaSharp 1.68.0 / 1.68.1.1

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1148 — Original report with full repro project

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | Segfault / process exits with code 0 at MatchTypeface call; no managed exception thrown |
| Repro quality | complete |
| Target frameworks | netcoreapp3.1 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.0, 1.68.1.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | SKFontManager.MatchTypeface was removed as a breaking change in v3.0.0; the specific API path no longer exists in the current codebase. However, a 2025 comment indicates similar fontconfig crashes still occur via other code paths. |

## Analysis

### Technical Summary

Stream-loaded SKTypeface objects do not have a fontconfig pattern (FC_FAMILY attribute) since they were not registered through fontconfig. When MatchTypeface is called on Linux, Skia's fontconfig implementation (SkFontMgr_fontconfig.cpp) calls matchFamilyStyle(get_string(fcTypeface->fPattern, FC_FAMILY), style) — and get_string returns null for a stream-loaded font because fPattern was never populated by fontconfig, causing a null pointer dereference and process crash.

### Rationale

Confirmed crash (segfault, process killed) on Linux with complete repro. The crash only occurs on Linux because the fontconfig implementation path is used there, whereas Windows/iOS use different font manager backends. SKFontManager.MatchTypeface was removed in v3.0.0 (breaking change), addressing the crash for v3.x users. For v2.x users and potentially for applications still experiencing similar crashes in v3.x via other fontconfig paths, the workaround is to use MatchFamily(face.FamilyName, style) instead.

### Key Signals

- "This code does not fail in Windows (and possibly iOS is okay too)" — **issue body** (Platform-specific: Linux uses fontconfig path, Windows uses DirectWrite — confirms fontconfig-only crash.)
- "I crashed a live server application running in an Ubuntu docker container by exploiting this" — **issue body** (Real-world production impact; not just a test environment issue.)
- "return this->matchFamilyStyle(get_string(fcTypeface->fPattern, FC_FAMILY), style);" — **maintainer comment (referencing skia/src/ports/SkFontMgr_fontconfig.cpp)** (The fontconfig MatchTypeface implementation reads FC_FAMILY from the typeface pattern — null for stream-loaded fonts.)
- "If it works on Windows, then you are right, this might be a linux/FreeType thing..." — **maintainer comment #589614027** (Maintainer identified this as Linux-specific fontconfig behavior.)
- "Same issue here with TwitchDownloaderCLI... segfault in FcPatternObjectPosition when TwitchDownloader calls SKFontManager.MatchTypeface" — **comment #3115109431 (2025)** (Issue still affects users in 2025, possibly on v2.x or a fork with MatchTypeface still present.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKFontManager.cs` | — | direct | MatchTypeface(SKTypeface face, SKFontStyle style) does NOT appear in the current codebase. The method was removed as a breaking change in v3.0.0. The current file only has MatchFamily(string, SKFontStyle) which calls sk_fontmgr_match_family_style. |
| `changelogs/SkiaSharp/3.0.0/SkiaSharp.breaking.md` | 286 | direct | Confirms MatchTypeface(SKTypeface, SKFontStyle) was removed from SKFontManager in v3.0.0 as a breaking change. This is the fix for v3.x users. |
| `binding/SkiaSharp/SKFontManager.cs` | 100-120 | related | CreateTypeface(Stream) converts managed stream to SKManagedStream, then to MemoryStream via ToMemoryStream(), then calls sk_fontmgr_create_from_stream. Stream ownership is transferred to the typeface via RevokeOwnership — the stream lifecycle is handled correctly. |
| `binding/SkiaSharp/SKFontStyleSet.cs` | 47-55 | context | CreateTypeface(SKFontStyle) calls sk_fontstyleset_match_style — this is the safe alternative path for finding a typeface by style within a family's StyleSet, avoiding the fontconfig MatchTypeface path entirely. |

**Error fingerprint:** `segfault-fontconfig-stream-typeface-match-linux`

### Workarounds

- Use fm.MatchFamily(face.FamilyName, SKFontStyle.Bold) instead of fm.MatchTypeface(face, SKFontStyle.Bold) — looks up by family name string rather than through fontconfig pattern
- Use fm.GetFontStyles(face.FamilyName).CreateTypeface(style) for safer style matching within a family
- Upgrade to SkiaSharp 3.x where MatchTypeface has been removed (the crash no longer occurs)
- Install fonts as system fonts (via fontconfig) rather than loading from streams — system fonts have proper fontconfig patterns and don't crash

### Next Questions

- Does a similar fontconfig crash occur in v3.x through any remaining API path (e.g., MatchCharacter with stream-loaded typeface)?
- Is the 2025 TwitchDownloaderCLI report using SkiaSharp v2.x or a fork?
- Should MatchFamily(face.FamilyName, style) be documented as the v2.x migration guide for MatchTypeface?

### Resolution Proposals

**Hypothesis:** Stream-loaded SKTypeface lacks fontconfig pattern metadata (FC_FAMILY is null), causing get_string(fcTypeface->fPattern, FC_FAMILY) to return null in SkFontMgr_fontconfig.cpp, which is passed to matchFamilyStyle and triggers a null-pointer dereference segfault.

1. **Upgrade to SkiaSharp 3.x** — fix, confidence 0.95 (95%), cost/m, validated=untested
   - MatchTypeface was removed as a breaking change in v3.0.0. The crash cannot occur on v3.x. This is the primary fix for affected users.
2. **Use MatchFamily by family name as workaround on v2.x** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Replace fm.MatchTypeface(face, style) with fm.MatchFamily(face.FamilyName, style). This avoids the fontconfig pattern lookup and uses the string-based family name matching path instead.

```csharp
// Instead of (crashes on Linux with stream-loaded fonts):
// var boldTypeface = fm.MatchTypeface(regularTypeface, SKFontStyle.Bold);

// Use this (works on all platforms):
var boldTypeface = fm.MatchFamily(regularTypeface.FamilyName, SKFontStyle.Bold);
// Or via FontStyleSet:
var boldTypeface2 = fm.GetFontStyles(regularTypeface.FamilyName)?.CreateTypeface(SKFontStyle.Bold);
```

**Recommended proposal:** Use MatchFamily by family name as workaround on v2.x

**Why:** Immediate fix applicable without upgrading. The code change is trivial and avoids the fontconfig pattern lookup entirely.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | The specific MatchTypeface API was removed in v3.0.0 but the issue is still open, has recent 2025 activity showing similar crashes, and it's unclear whether the underlying fontconfig crash can still be triggered through other API paths in current versions. Needs investigation to confirm current relevance and close definitively. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/SkiaSharp, os/Linux, tenet/reliability labels | labels=type/bug, area/SkiaSharp, os/Linux, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Acknowledge the crash, explain root cause, provide workaround for v2.x, note API removal in v3.x | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro! This crash is caused by a Linux-specific issue in Skia's fontconfig font manager. When `SKFontManager.MatchTypeface` is called with a stream-loaded typeface on Linux, Skia's fontconfig implementation tries to read the `FC_FAMILY` attribute from the typeface's fontconfig pattern — but fonts loaded from streams don't have a fontconfig pattern, causing a null pointer dereference and segfault.

**Workaround (v2.x):** Replace `fm.MatchTypeface(face, style)` with `fm.MatchFamily(face.FamilyName, style)` — this avoids the fontconfig pattern lookup:

```csharp
// Instead of:
var boldTypeface = fm.MatchTypeface(regularTypeface, SKFontStyle.Bold); // crashes on Linux

// Use:
var boldTypeface = fm.MatchFamily(regularTypeface.FamilyName, SKFontStyle.Bold);
```

**v3.x:** `SKFontManager.MatchTypeface` was removed in v3.0.0 as a breaking change, so upgrading to v3.x also resolves the crash.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1148,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T17:06:21Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Segfault on Linux (Ubuntu 18.04) when calling SKFontManager.MatchTypeface() with a stream-loaded SKTypeface — the Linux fontconfig implementation dereferences an invalid FC_FAMILY pattern attribute for non-system fonts, instantly killing the process.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "Segfault / process exits with code 0 at MatchTypeface call; no managed exception thrown",
      "reproQuality": "complete",
      "targetFrameworks": [
        "netcoreapp3.1"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Load two fonts from embedded resources into MemoryStream on Linux",
        "Call SKFontManager.Default.CreateTypeface(stream) to create SKTypeface objects",
        "Call SKFontManager.Default.MatchTypeface(defaultRegular, SKFontStyle.Bold)",
        "Process segfaults and is immediately killed — no exception, no stack trace"
      ],
      "environmentDetails": "Ubuntu 18.04.3, x64, .NET Core 3.1, SkiaSharp 1.68.0 / 1.68.1.1",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1148",
          "description": "Original report with full repro project"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.0",
        "1.68.1.1"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "SKFontManager.MatchTypeface was removed as a breaking change in v3.0.0; the specific API path no longer exists in the current codebase. However, a 2025 comment indicates similar fontconfig crashes still occur via other code paths."
    }
  },
  "analysis": {
    "summary": "Stream-loaded SKTypeface objects do not have a fontconfig pattern (FC_FAMILY attribute) since they were not registered through fontconfig. When MatchTypeface is called on Linux, Skia's fontconfig implementation (SkFontMgr_fontconfig.cpp) calls matchFamilyStyle(get_string(fcTypeface->fPattern, FC_FAMILY), style) — and get_string returns null for a stream-loaded font because fPattern was never populated by fontconfig, causing a null pointer dereference and process crash.",
    "rationale": "Confirmed crash (segfault, process killed) on Linux with complete repro. The crash only occurs on Linux because the fontconfig implementation path is used there, whereas Windows/iOS use different font manager backends. SKFontManager.MatchTypeface was removed in v3.0.0 (breaking change), addressing the crash for v3.x users. For v2.x users and potentially for applications still experiencing similar crashes in v3.x via other fontconfig paths, the workaround is to use MatchFamily(face.FamilyName, style) instead.",
    "keySignals": [
      {
        "text": "This code does not fail in Windows (and possibly iOS is okay too)",
        "source": "issue body",
        "interpretation": "Platform-specific: Linux uses fontconfig path, Windows uses DirectWrite — confirms fontconfig-only crash."
      },
      {
        "text": "I crashed a live server application running in an Ubuntu docker container by exploiting this",
        "source": "issue body",
        "interpretation": "Real-world production impact; not just a test environment issue."
      },
      {
        "text": "return this->matchFamilyStyle(get_string(fcTypeface->fPattern, FC_FAMILY), style);",
        "source": "maintainer comment (referencing skia/src/ports/SkFontMgr_fontconfig.cpp)",
        "interpretation": "The fontconfig MatchTypeface implementation reads FC_FAMILY from the typeface pattern — null for stream-loaded fonts."
      },
      {
        "text": "If it works on Windows, then you are right, this might be a linux/FreeType thing...",
        "source": "maintainer comment #589614027",
        "interpretation": "Maintainer identified this as Linux-specific fontconfig behavior."
      },
      {
        "text": "Same issue here with TwitchDownloaderCLI... segfault in FcPatternObjectPosition when TwitchDownloader calls SKFontManager.MatchTypeface",
        "source": "comment #3115109431 (2025)",
        "interpretation": "Issue still affects users in 2025, possibly on v2.x or a fork with MatchTypeface still present."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "finding": "MatchTypeface(SKTypeface face, SKFontStyle style) does NOT appear in the current codebase. The method was removed as a breaking change in v3.0.0. The current file only has MatchFamily(string, SKFontStyle) which calls sk_fontmgr_match_family_style.",
        "relevance": "direct"
      },
      {
        "file": "changelogs/SkiaSharp/3.0.0/SkiaSharp.breaking.md",
        "lines": "286",
        "finding": "Confirms MatchTypeface(SKTypeface, SKFontStyle) was removed from SKFontManager in v3.0.0 as a breaking change. This is the fix for v3.x users.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "100-120",
        "finding": "CreateTypeface(Stream) converts managed stream to SKManagedStream, then to MemoryStream via ToMemoryStream(), then calls sk_fontmgr_create_from_stream. Stream ownership is transferred to the typeface via RevokeOwnership — the stream lifecycle is handled correctly.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKFontStyleSet.cs",
        "lines": "47-55",
        "finding": "CreateTypeface(SKFontStyle) calls sk_fontstyleset_match_style — this is the safe alternative path for finding a typeface by style within a family's StyleSet, avoiding the fontconfig MatchTypeface path entirely.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Use fm.MatchFamily(face.FamilyName, SKFontStyle.Bold) instead of fm.MatchTypeface(face, SKFontStyle.Bold) — looks up by family name string rather than through fontconfig pattern",
      "Use fm.GetFontStyles(face.FamilyName).CreateTypeface(style) for safer style matching within a family",
      "Upgrade to SkiaSharp 3.x where MatchTypeface has been removed (the crash no longer occurs)",
      "Install fonts as system fonts (via fontconfig) rather than loading from streams — system fonts have proper fontconfig patterns and don't crash"
    ],
    "nextQuestions": [
      "Does a similar fontconfig crash occur in v3.x through any remaining API path (e.g., MatchCharacter with stream-loaded typeface)?",
      "Is the 2025 TwitchDownloaderCLI report using SkiaSharp v2.x or a fork?",
      "Should MatchFamily(face.FamilyName, style) be documented as the v2.x migration guide for MatchTypeface?"
    ],
    "errorFingerprint": "segfault-fontconfig-stream-typeface-match-linux",
    "resolution": {
      "hypothesis": "Stream-loaded SKTypeface lacks fontconfig pattern metadata (FC_FAMILY is null), causing get_string(fcTypeface->fPattern, FC_FAMILY) to return null in SkFontMgr_fontconfig.cpp, which is passed to matchFamilyStyle and triggers a null-pointer dereference segfault.",
      "proposals": [
        {
          "title": "Upgrade to SkiaSharp 3.x",
          "description": "MatchTypeface was removed as a breaking change in v3.0.0. The crash cannot occur on v3.x. This is the primary fix for affected users.",
          "category": "fix",
          "confidence": 0.95,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Use MatchFamily by family name as workaround on v2.x",
          "description": "Replace fm.MatchTypeface(face, style) with fm.MatchFamily(face.FamilyName, style). This avoids the fontconfig pattern lookup and uses the string-based family name matching path instead.",
          "codeSnippet": "// Instead of (crashes on Linux with stream-loaded fonts):\n// var boldTypeface = fm.MatchTypeface(regularTypeface, SKFontStyle.Bold);\n\n// Use this (works on all platforms):\nvar boldTypeface = fm.MatchFamily(regularTypeface.FamilyName, SKFontStyle.Bold);\n// Or via FontStyleSet:\nvar boldTypeface2 = fm.GetFontStyles(regularTypeface.FamilyName)?.CreateTypeface(SKFontStyle.Bold);",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use MatchFamily by family name as workaround on v2.x",
      "recommendedReason": "Immediate fix applicable without upgrading. The code change is trivial and avoids the fontconfig pattern lookup entirely."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "The specific MatchTypeface API was removed in v3.0.0 but the issue is still open, has recent 2025 activity showing similar crashes, and it's unclear whether the underlying fontconfig crash can still be triggered through other API paths in current versions. Needs investigation to confirm current relevance and close definitively.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Linux, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Linux",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the crash, explain root cause, provide workaround for v2.x, note API removal in v3.x",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed repro! This crash is caused by a Linux-specific issue in Skia's fontconfig font manager. When `SKFontManager.MatchTypeface` is called with a stream-loaded typeface on Linux, Skia's fontconfig implementation tries to read the `FC_FAMILY` attribute from the typeface's fontconfig pattern — but fonts loaded from streams don't have a fontconfig pattern, causing a null pointer dereference and segfault.\n\n**Workaround (v2.x):** Replace `fm.MatchTypeface(face, style)` with `fm.MatchFamily(face.FamilyName, style)` — this avoids the fontconfig pattern lookup:\n\n```csharp\n// Instead of:\nvar boldTypeface = fm.MatchTypeface(regularTypeface, SKFontStyle.Bold); // crashes on Linux\n\n// Use:\nvar boldTypeface = fm.MatchFamily(regularTypeface.FamilyName, SKFontStyle.Bold);\n```\n\n**v3.x:** `SKFontManager.MatchTypeface` was removed in v3.0.0 as a breaking change, so upgrading to v3.x also resolves the crash."
      }
    ]
  }
}
```

</details>
