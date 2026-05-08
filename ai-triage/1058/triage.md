# Issue Triage Report — #1058

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T13:00:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKTypeface.FromFamilyName returns null on Android and Linux (fontconfig) when a requested font family is not found, while on macOS and other platforms it returns a fallback typeface. The upstream Skia C++ header documents this method as 'Will never return null', creating a documented contradiction between the C++ promise and the actual platform implementations.

**Analysis:** The root cause is that Skia's Android font manager (SkFontMgr_android.cpp) and fontconfig port explicitly return nullptr for unrecognized family names, while other platforms (macOS CoreText, Windows DirectWrite) fall back to the system default typeface. The C# wrapper SKTypeface.FromFamilyName passes this through unchanged — it calls GetObject(sk_typeface_create_from_name(...)) and uses tf?.PreventPublicDisposal(), which is null-safe but silently exposes platform-inconsistent behavior to callers. The upstream Skia C++ header says 'Will never return null' but that promise is violated on Android and Linux. The fix requires either: (a) documenting the null behavior in the SkiaSharp API, or (b) normalizing behavior in the C# wrapper to always fall back to Default when null is returned.

**Recommendations:** **needs-investigation** — Real, reproducible platform-inconsistency bug confirmed by maintainer. Root cause understood (Skia platform ports). Requires maintainer decision on normalization strategy before a fix can land.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Android, os/Linux |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Linux, os/Android, area/SkiaSharp, tenet/compatibility, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Call SKTypeface.FromFamilyName("System") on Android
2. Observe null return value on Android
3. Call same on macOS — observe non-null fallback typeface
4. Call SKFontManager.Default.MatchFamily("System", SKFontStyle.Bold) on Android — also null

**Environment:** Android (any version), Linux (fontconfig). Confirmed by issue maintainer in comments.

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/643 — Prior related issue noting no null guarantee
- https://github.com/toptensoftware/RichTextKit/pull/6 — RichTextKit workaround PR handling null typeface

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | platform-specific |
| Error message | SKTypeface.FromFamilyName returns null on Android/Linux but non-null on macOS for same input |
| Repro quality | complete |
| Target frameworks | net9.0-android, net9.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The C# wrapper code still uses tf?.PreventPublicDisposal() (null-safe), confirming null passthrough is still present. No fix has been landed. |

## Analysis

### Technical Summary

The root cause is that Skia's Android font manager (SkFontMgr_android.cpp) and fontconfig port explicitly return nullptr for unrecognized family names, while other platforms (macOS CoreText, Windows DirectWrite) fall back to the system default typeface. The C# wrapper SKTypeface.FromFamilyName passes this through unchanged — it calls GetObject(sk_typeface_create_from_name(...)) and uses tf?.PreventPublicDisposal(), which is null-safe but silently exposes platform-inconsistent behavior to callers. The upstream Skia C++ header says 'Will never return null' but that promise is violated on Android and Linux. The fix requires either: (a) documenting the null behavior in the SkiaSharp API, or (b) normalizing behavior in the C# wrapper to always fall back to Default when null is returned.

### Rationale

This is type/bug because the API surface is contractually inconsistent: calling the same method with the same argument produces null on Android/Linux but a valid typeface on macOS. The area is area/SkiaSharp (core typeface binding), not Views or native. Platforms os/Android and os/Linux are affected. Tenets tenet/compatibility and tenet/reliability apply because callers cannot write cross-platform code without null-checking everywhere.

### Key Signals

- "On Android, SKTypeface.FromFamilyName("System") returns null; on macOS returns default" — **issue comment #9** (Direct confirmation of platform-inconsistent null behavior from maintainer testing)
- "On Android we must return nullptr when we can't find the requested named typeface so that the system/app can provide their own recovery mechanism" — **issue body (SkFontMgr_android.cpp comment)** (Android Skia port intentionally returns nullptr for unrecognized families — upstream design decision)
- "fontconfig variants which basically just return nullptr" — **issue body** (Linux fontconfig port has the same null-return behavior)
- "fm (GetFontStyles) returns an empty set for non-existent family on both platforms; MatchFamily also returns null on Android" — **issue comment #10** (GetFontStyles returning empty is a reliable cross-platform check for font existence)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 60-72 | direct | FromFamilyName(string, SKFontStyle) calls GetObject(sk_typeface_create_from_name(...)) then tf?.PreventPublicDisposal() — the ?. operator confirms null is expected and silently passed through to callers |
| `binding/SkiaSharp/SKFontManager.cs` | 77-87 | related | SKFontManager.MatchFamily(string, SKFontStyle) has identical null-passthrough pattern with tf?.PreventPublicDisposal() — same null behavior on Android/Linux |
| `binding/SkiaSharp/SKFontStyleSet.cs` | 9,24 | context | SKFontStyleSet implements IReadOnlyCollection<SKFontStyle> with Count backed by sk_fontstyleset_get_count — GetFontStyles().Count is a reliable font-existence check |

### Workarounds

- var typeface = SKTypeface.FromFamilyName("MyFont") ?? SKTypeface.Default;
- Use SKFontManager.Default.GetFontStyles(familyName).Count > 0 to test font existence before calling MatchFamily
- Use SKFontManager.Default.MatchFamily(familyName, style) and null-check the result

### Next Questions

- Should SkiaSharp normalize behavior in FromFamilyName to always return Default when null would be returned?
- Should a TryFromFamilyName(name, out SKTypeface) overload be added for explicit null-detection?
- Does latest Skia milestone still exhibit this behavior in Android/fontconfig ports?

### Resolution Proposals

**Hypothesis:** Platform Skia font managers (Android, fontconfig) intentionally return nullptr for unrecognized families. The C# wrapper passes null through. The fix requires a policy decision: normalize to non-null or document and keep null.

1. **Null-coalescing at call site** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Callers use ?? SKTypeface.Default to safely fall back when font is not found

```csharp
var typeface = SKTypeface.FromFamilyName("MyFont") ?? SKTypeface.Default;
```
2. **Check font existence with GetFontStyles** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Use SKFontManager.Default.GetFontStyles to check if a family is registered before requesting a typeface. GetFontStyles returns an empty set on both Android and macOS for unregistered families.

```csharp
var styles = SKFontManager.Default.GetFontStyles("MyFont");
if (styles.Count > 0)
{
    var typeface = SKFontManager.Default.MatchFamily("MyFont", SKFontStyle.Normal);
    // use typeface
}
else
{
    var typeface = SKTypeface.Default; // font not installed
}
```
3. **Normalize null in C# wrapper** — fix, confidence 0.75 (75%), cost/xs, validated=untested
   - Update SKTypeface.FromFamilyName to return SKTypeface.Default instead of null when the native call returns a null pointer, making behavior consistent across all platforms.
4. **Clarify null behavior in XML documentation** — fix, confidence 0.90 (90%), cost/xs, validated=untested
   - Add XML doc comments to SKTypeface.FromFamilyName documenting that on Android and Linux (fontconfig), the method may return null if the requested family is not found.

**Recommended proposal:** Null-coalescing at call site

**Why:** Simple one-liner that immediately protects call sites from NullReferenceException on Android/Linux, with no risk of changing observable behavior for code that already handles null.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real, reproducible platform-inconsistency bug confirmed by maintainer. Root cause understood (Skia platform ports). Requires maintainer decision on normalization strategy before a fix can land. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/SkiaSharp, os/Android, os/Linux, tenet/compatibility, tenet/reliability labels | labels=type/bug, area/SkiaSharp, os/Android, os/Linux, tenet/compatibility, tenet/reliability |
| link-related | low | 0.90 (90%) | Cross-reference prior related issue #643 | linkedIssue=#643 |
| add-comment | medium | 0.85 (85%) | Post analysis with workarounds for null return behavior | — |

**Comment draft for `add-comment`:**

```markdown
## Analysis

This is a genuine platform inconsistency in the underlying Skia library:

- **Android** and **Linux (fontconfig)**: `SKTypeface.FromFamilyName` returns `null` when the family isn't found (Android explicitly chose this behavior to let apps provide their own recovery)
- **macOS / Windows**: Returns a fallback typeface from the default family instead

The upstream Skia C++ header says "Will never return null" but the Android and fontconfig ports explicitly return `nullptr` for unrecognized families — that promise doesn't hold on all platforms.

The current SkiaSharp C# wrapper is aware of this (it uses `?.PreventPublicDisposal()` to null-safely unwrap the result) but passes the null through to callers without documentation.

## Workarounds

**Quick fix — null-coalescing at call site:**
```csharp
var typeface = SKTypeface.FromFamilyName("MyFont") ?? SKTypeface.Default;
```

**Explicit font-existence check (when you need to know if the font is installed):**
```csharp
var styles = SKFontManager.Default.GetFontStyles("MyFont");
if (styles.Count > 0)
{
    var typeface = SKFontManager.Default.MatchFamily("MyFont", SKFontStyle.Normal);
    // use typeface
}
else
{
    // Font not found on this device — use fallback
    var typeface = SKTypeface.Default;
}
```

`GetFontStyles(familyName).Count == 0` reliably indicates a font family is not registered on both Android and Linux.

## Next steps

A maintainer decision is needed on whether to:
1. Normalize behavior in `SKTypeface.FromFamilyName` to always return `SKTypeface.Default` instead of null (non-breaking for most callers)
2. Add `TryFromFamilyName` overload for explicit null-detection
3. At minimum, update XML documentation to document the platform-specific null behavior
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1058,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T13:00:00Z",
    "currentLabels": [
      "type/bug",
      "os/Linux",
      "os/Android",
      "area/SkiaSharp",
      "tenet/compatibility",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "SKTypeface.FromFamilyName returns null on Android and Linux (fontconfig) when a requested font family is not found, while on macOS and other platforms it returns a fallback typeface. The upstream Skia C++ header documents this method as 'Will never return null', creating a documented contradiction between the C++ promise and the actual platform implementations.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Android",
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "platform-specific",
      "errorMessage": "SKTypeface.FromFamilyName returns null on Android/Linux but non-null on macOS for same input",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0-android",
        "net9.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Call SKTypeface.FromFamilyName(\"System\") on Android",
        "Observe null return value on Android",
        "Call same on macOS — observe non-null fallback typeface",
        "Call SKFontManager.Default.MatchFamily(\"System\", SKFontStyle.Bold) on Android — also null"
      ],
      "environmentDetails": "Android (any version), Linux (fontconfig). Confirmed by issue maintainer in comments.",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/643",
          "description": "Prior related issue noting no null guarantee"
        },
        {
          "url": "https://github.com/toptensoftware/RichTextKit/pull/6",
          "description": "RichTextKit workaround PR handling null typeface"
        }
      ]
    },
    "versionAnalysis": {
      "currentRelevance": "likely",
      "relevanceReason": "The C# wrapper code still uses tf?.PreventPublicDisposal() (null-safe), confirming null passthrough is still present. No fix has been landed."
    }
  },
  "analysis": {
    "summary": "The root cause is that Skia's Android font manager (SkFontMgr_android.cpp) and fontconfig port explicitly return nullptr for unrecognized family names, while other platforms (macOS CoreText, Windows DirectWrite) fall back to the system default typeface. The C# wrapper SKTypeface.FromFamilyName passes this through unchanged — it calls GetObject(sk_typeface_create_from_name(...)) and uses tf?.PreventPublicDisposal(), which is null-safe but silently exposes platform-inconsistent behavior to callers. The upstream Skia C++ header says 'Will never return null' but that promise is violated on Android and Linux. The fix requires either: (a) documenting the null behavior in the SkiaSharp API, or (b) normalizing behavior in the C# wrapper to always fall back to Default when null is returned.",
    "rationale": "This is type/bug because the API surface is contractually inconsistent: calling the same method with the same argument produces null on Android/Linux but a valid typeface on macOS. The area is area/SkiaSharp (core typeface binding), not Views or native. Platforms os/Android and os/Linux are affected. Tenets tenet/compatibility and tenet/reliability apply because callers cannot write cross-platform code without null-checking everywhere.",
    "keySignals": [
      {
        "text": "On Android, SKTypeface.FromFamilyName(\"System\") returns null; on macOS returns default",
        "source": "issue comment #9",
        "interpretation": "Direct confirmation of platform-inconsistent null behavior from maintainer testing"
      },
      {
        "text": "On Android we must return nullptr when we can't find the requested named typeface so that the system/app can provide their own recovery mechanism",
        "source": "issue body (SkFontMgr_android.cpp comment)",
        "interpretation": "Android Skia port intentionally returns nullptr for unrecognized families — upstream design decision"
      },
      {
        "text": "fontconfig variants which basically just return nullptr",
        "source": "issue body",
        "interpretation": "Linux fontconfig port has the same null-return behavior"
      },
      {
        "text": "fm (GetFontStyles) returns an empty set for non-existent family on both platforms; MatchFamily also returns null on Android",
        "source": "issue comment #10",
        "interpretation": "GetFontStyles returning empty is a reliable cross-platform check for font existence"
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "60-72",
        "finding": "FromFamilyName(string, SKFontStyle) calls GetObject(sk_typeface_create_from_name(...)) then tf?.PreventPublicDisposal() — the ?. operator confirms null is expected and silently passed through to callers",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "77-87",
        "finding": "SKFontManager.MatchFamily(string, SKFontStyle) has identical null-passthrough pattern with tf?.PreventPublicDisposal() — same null behavior on Android/Linux",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKFontStyleSet.cs",
        "lines": "9,24",
        "finding": "SKFontStyleSet implements IReadOnlyCollection<SKFontStyle> with Count backed by sk_fontstyleset_get_count — GetFontStyles().Count is a reliable font-existence check",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "var typeface = SKTypeface.FromFamilyName(\"MyFont\") ?? SKTypeface.Default;",
      "Use SKFontManager.Default.GetFontStyles(familyName).Count > 0 to test font existence before calling MatchFamily",
      "Use SKFontManager.Default.MatchFamily(familyName, style) and null-check the result"
    ],
    "nextQuestions": [
      "Should SkiaSharp normalize behavior in FromFamilyName to always return Default when null would be returned?",
      "Should a TryFromFamilyName(name, out SKTypeface) overload be added for explicit null-detection?",
      "Does latest Skia milestone still exhibit this behavior in Android/fontconfig ports?"
    ],
    "resolution": {
      "hypothesis": "Platform Skia font managers (Android, fontconfig) intentionally return nullptr for unrecognized families. The C# wrapper passes null through. The fix requires a policy decision: normalize to non-null or document and keep null.",
      "proposals": [
        {
          "title": "Null-coalescing at call site",
          "description": "Callers use ?? SKTypeface.Default to safely fall back when font is not found",
          "category": "workaround",
          "codeSnippet": "var typeface = SKTypeface.FromFamilyName(\"MyFont\") ?? SKTypeface.Default;",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Check font existence with GetFontStyles",
          "description": "Use SKFontManager.Default.GetFontStyles to check if a family is registered before requesting a typeface. GetFontStyles returns an empty set on both Android and macOS for unregistered families.",
          "category": "workaround",
          "codeSnippet": "var styles = SKFontManager.Default.GetFontStyles(\"MyFont\");\nif (styles.Count > 0)\n{\n    var typeface = SKFontManager.Default.MatchFamily(\"MyFont\", SKFontStyle.Normal);\n    // use typeface\n}\nelse\n{\n    var typeface = SKTypeface.Default; // font not installed\n}",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Normalize null in C# wrapper",
          "description": "Update SKTypeface.FromFamilyName to return SKTypeface.Default instead of null when the native call returns a null pointer, making behavior consistent across all platforms.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Clarify null behavior in XML documentation",
          "description": "Add XML doc comments to SKTypeface.FromFamilyName documenting that on Android and Linux (fontconfig), the method may return null if the requested family is not found.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Null-coalescing at call site",
      "recommendedReason": "Simple one-liner that immediately protects call sites from NullReferenceException on Android/Linux, with no risk of changing observable behavior for code that already handles null."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real, reproducible platform-inconsistency bug confirmed by maintainer. Root cause understood (Skia platform ports). Requires maintainer decision on normalization strategy before a fix can land.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Android, os/Linux, tenet/compatibility, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Android",
          "os/Linux",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference prior related issue #643",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 643
      },
      {
        "type": "add-comment",
        "description": "Post analysis with workarounds for null return behavior",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "## Analysis\n\nThis is a genuine platform inconsistency in the underlying Skia library:\n\n- **Android** and **Linux (fontconfig)**: `SKTypeface.FromFamilyName` returns `null` when the family isn't found (Android explicitly chose this behavior to let apps provide their own recovery)\n- **macOS / Windows**: Returns a fallback typeface from the default family instead\n\nThe upstream Skia C++ header says \"Will never return null\" but the Android and fontconfig ports explicitly return `nullptr` for unrecognized families — that promise doesn't hold on all platforms.\n\nThe current SkiaSharp C# wrapper is aware of this (it uses `?.PreventPublicDisposal()` to null-safely unwrap the result) but passes the null through to callers without documentation.\n\n## Workarounds\n\n**Quick fix — null-coalescing at call site:**\n```csharp\nvar typeface = SKTypeface.FromFamilyName(\"MyFont\") ?? SKTypeface.Default;\n```\n\n**Explicit font-existence check (when you need to know if the font is installed):**\n```csharp\nvar styles = SKFontManager.Default.GetFontStyles(\"MyFont\");\nif (styles.Count > 0)\n{\n    var typeface = SKFontManager.Default.MatchFamily(\"MyFont\", SKFontStyle.Normal);\n    // use typeface\n}\nelse\n{\n    // Font not found on this device — use fallback\n    var typeface = SKTypeface.Default;\n}\n```\n\n`GetFontStyles(familyName).Count == 0` reliably indicates a font family is not registered on both Android and Linux.\n\n## Next steps\n\nA maintainer decision is needed on whether to:\n1. Normalize behavior in `SKTypeface.FromFamilyName` to always return `SKTypeface.Default` instead of null (non-breaking for most callers)\n2. Add `TryFromFamilyName` overload for explicit null-detection\n3. At minimum, update XML documentation to document the platform-specific null behavior"
      }
    ]
  }
}
```

</details>
