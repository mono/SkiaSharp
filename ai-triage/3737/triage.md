# Issue Triage Report — #3737

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-02T05:47:53Z |
| Type | type/documentation (0.90 (90%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Breaking change tracking issue: SKTypeface.FromFamilyName now returns SKTypeface.Default instead of null on Android when the requested font family is missing, aligning Android behavior with all other platforms for the 4.x release.

**Analysis:** This is an intentional behavioral breaking change in SkiaSharp 4.x (Skia m132 upgrade): SKTypeface.FromFamilyName for a missing font now always returns SKTypeface.Default instead of null on Android. The code change is confirmed at binding/SkiaSharp/SKTypeface.cs:79-80. The issue serves as a sub-tracker of #3734 documenting this change with before/after behavior and migration guidance. The mentioned test cases (BC3_FromFamilyNameMissingReturnsNonNull etc.) are not yet present in the test files, so test coverage still needs to be added.

**Recommendations:** **keep-open** — This is a sub-tracker of #3734 documenting an intentional breaking change. It should remain open until the mentioned test cases are added and the change is included in the 4.x upgrade documentation. The implementation is already done.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/documentation |
| Area | area/SkiaSharp |
| Platforms | os/Android |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | breaking-change, upgrading/4.x, cost/xs, priority/2 |

## Evidence

### Reproduction

1. On Android, call SKTypeface.FromFamilyName("MissingFont")
2. In 3.x (m119): returns null
3. In 4.x (m132): returns SKTypeface.Default (non-null)

**Environment:** Android only; other platforms already returned a fallback typeface in 3.x

**Related issues:** #3734, #1058, #2471

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3734 — Parent tracker: behavioral breaking changes from Skia m147 upgrade
- https://github.com/mono/SkiaSharp/issues/1058 — Related: FromFamilyName null return semantics
- https://github.com/mono/SkiaSharp/issues/2471 — Related: getting typefaces for CJK fonts on Android

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.x, 4.x, m119, m132 |
| Worked in | 3.x (m119) |
| Broke in | 4.x (m132) |
| Current relevance | likely |
| Relevance reason | The new implementation in binding/SkiaSharp/SKTypeface.cs line 79-80 uses SKFontManager.Default.MatchFamily ?? Default, which consistently returns Default for missing fonts on all platforms including Android. |

## Analysis

### Technical Summary

This is an intentional behavioral breaking change in SkiaSharp 4.x (Skia m132 upgrade): SKTypeface.FromFamilyName for a missing font now always returns SKTypeface.Default instead of null on Android. The code change is confirmed at binding/SkiaSharp/SKTypeface.cs:79-80. The issue serves as a sub-tracker of #3734 documenting this change with before/after behavior and migration guidance. The mentioned test cases (BC3_FromFamilyNameMissingReturnsNonNull etc.) are not yet present in the test files, so test coverage still needs to be added.

### Rationale

This issue is a sub-tracker of #3734 documenting a behavioral breaking change introduced in SkiaSharp 4.x. The change is intentional: SKTypeface.FromFamilyName now consistently returns SKTypeface.Default across all platforms when a font is missing, removing the Android-specific null return. The migration path is clearly documented in the issue. The classification is type/documentation because this is a change-tracking and documentation issue, not a bug report. The issue should remain open until test coverage (BC3_* tests) is added and the change is fully documented in upgrade guides.

### Key Signals

- "SKTypeface.FromFamilyName("missing") now returns SKTypeface.Default on all platforms. Previously, it returned null on Android" — **issue body** (Intentional behavioral breaking change for API consistency across platforms)
- "The new code uses SKFontManager.Default.MatchFamily(familyName, style) ?? Default, making all platforms consistent — a missing font always returns Default." — **issue body — Reason section** (Confirms this is a deliberate design decision, not a bug)
- "Tests: BC3_FromFamilyNameMissingReturnsNonNull — confirms non-null for missing font" — **issue body — Tests section** (Test coverage is planned but not yet found in source; issue is tracking completion)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 79-80 | direct | FromFamilyName(string, SKFontStyle) now returns `SKFontManager.Default.MatchFamily(familyName, style) ?? Default;` — always non-null. The old code used native SkTypeface::MakeFromName which on Android returned null for missing fonts. |
| `tests/Tests/SkiaSharp/SKTypefaceTest.cs` | 649-653 | related | Existing test FromFamilyNameNullReturnsNonNull tests null family name returning non-null, but tests specifically for missing-font-returns-Default (BC3_FromFamilyNameMissingReturnsNonNull, BC3_MatchFamilyMissingReturnsNull) are not present in the file. |

### Resolution Proposals

**Hypothesis:** The breaking change is intentional and already implemented. The issue needs test coverage added (BC3_FromFamilyNameMissingReturnsNonNull, BC3_MatchFamilyMissingReturnsNull, BC3_FromFamilyNameKnownFontWorks) and documentation finalized before closing.

1. **Add missing test coverage for the breaking change** — investigation, cost/xs, validated=untested
   - Add the three test cases mentioned in the issue body (BC3_FromFamilyNameMissingReturnsNonNull, BC3_MatchFamilyMissingReturnsNull, BC3_FromFamilyNameKnownFontWorks) to the SKTypeface test file to confirm the new behavior is verified.
2. **Use SKFontManager.Default.MatchFamily for strict null semantics** — workaround, cost/xs, validated=untested
   - Developers who relied on null return from FromFamilyName on Android should migrate to SKFontManager.Default.MatchFamily which still returns null when no match is found.

```csharp
// Migration for code that checks null on Android
var tf = SKFontManager.Default.MatchFamily("MissingFont");
if (tf == null) { /* font not found */ }
```

**Recommended proposal:** p1

**Why:** The primary open item for this tracking issue is adding the test coverage. The migration workaround is already documented in the issue body.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | This is a sub-tracker of #3734 documenting an intentional breaking change. It should remain open until the mentioned test cases are added and the change is included in the 4.x upgrade documentation. The implementation is already done. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply triage classification labels | labels=type/documentation, area/SkiaSharp, os/Android, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Acknowledge triage and note remaining work | — |

**Comment draft for `add-comment`:**

```markdown
**Triage note:** This is classified as a documentation/tracking issue for an intentional breaking change in SkiaSharp 4.x (Skia m132 upgrade).

The code change is confirmed in `binding/SkiaSharp/SKTypeface.cs` — `FromFamilyName` now returns `SKTypeface.Default` consistently across all platforms via `SKFontManager.Default.MatchFamily(familyName, style) ?? Default`.

**Remaining work before closing:**
- Add test cases: `BC3_FromFamilyNameMissingReturnsNonNull`, `BC3_MatchFamilyMissingReturnsNull`, `BC3_FromFamilyNameKnownFontWorks`
- Ensure migration guidance is referenced from the 4.x upgrade docs

**Migration:** Use `SKFontManager.Default.MatchFamily("FontName")` if strict null semantics are required.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3737,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-02T05:47:53Z",
    "currentLabels": [
      "breaking-change",
      "upgrading/4.x",
      "cost/xs",
      "priority/2"
    ]
  },
  "summary": "Breaking change tracking issue: SKTypeface.FromFamilyName now returns SKTypeface.Default instead of null on Android when the requested font family is missing, aligning Android behavior with all other platforms for the 4.x release.",
  "classification": {
    "type": {
      "value": "type/documentation",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Android"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "On Android, call SKTypeface.FromFamilyName(\"MissingFont\")",
        "In 3.x (m119): returns null",
        "In 4.x (m132): returns SKTypeface.Default (non-null)"
      ],
      "environmentDetails": "Android only; other platforms already returned a fallback typeface in 3.x",
      "relatedIssues": [
        3734,
        1058,
        2471
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3734",
          "description": "Parent tracker: behavioral breaking changes from Skia m147 upgrade"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1058",
          "description": "Related: FromFamilyName null return semantics"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2471",
          "description": "Related: getting typefaces for CJK fonts on Android"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.x",
        "4.x",
        "m119",
        "m132"
      ],
      "workedIn": "3.x (m119)",
      "brokeIn": "4.x (m132)",
      "currentRelevance": "likely",
      "relevanceReason": "The new implementation in binding/SkiaSharp/SKTypeface.cs line 79-80 uses SKFontManager.Default.MatchFamily ?? Default, which consistently returns Default for missing fonts on all platforms including Android."
    }
  },
  "analysis": {
    "summary": "This is an intentional behavioral breaking change in SkiaSharp 4.x (Skia m132 upgrade): SKTypeface.FromFamilyName for a missing font now always returns SKTypeface.Default instead of null on Android. The code change is confirmed at binding/SkiaSharp/SKTypeface.cs:79-80. The issue serves as a sub-tracker of #3734 documenting this change with before/after behavior and migration guidance. The mentioned test cases (BC3_FromFamilyNameMissingReturnsNonNull etc.) are not yet present in the test files, so test coverage still needs to be added.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "79-80",
        "finding": "FromFamilyName(string, SKFontStyle) now returns `SKFontManager.Default.MatchFamily(familyName, style) ?? Default;` — always non-null. The old code used native SkTypeface::MakeFromName which on Android returned null for missing fonts.",
        "relevance": "direct"
      },
      {
        "file": "tests/Tests/SkiaSharp/SKTypefaceTest.cs",
        "lines": "649-653",
        "finding": "Existing test FromFamilyNameNullReturnsNonNull tests null family name returning non-null, but tests specifically for missing-font-returns-Default (BC3_FromFamilyNameMissingReturnsNonNull, BC3_MatchFamilyMissingReturnsNull) are not present in the file.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "SKTypeface.FromFamilyName(\"missing\") now returns SKTypeface.Default on all platforms. Previously, it returned null on Android",
        "source": "issue body",
        "interpretation": "Intentional behavioral breaking change for API consistency across platforms"
      },
      {
        "text": "The new code uses SKFontManager.Default.MatchFamily(familyName, style) ?? Default, making all platforms consistent — a missing font always returns Default.",
        "source": "issue body — Reason section",
        "interpretation": "Confirms this is a deliberate design decision, not a bug"
      },
      {
        "text": "Tests: BC3_FromFamilyNameMissingReturnsNonNull — confirms non-null for missing font",
        "source": "issue body — Tests section",
        "interpretation": "Test coverage is planned but not yet found in source; issue is tracking completion"
      }
    ],
    "rationale": "This issue is a sub-tracker of #3734 documenting a behavioral breaking change introduced in SkiaSharp 4.x. The change is intentional: SKTypeface.FromFamilyName now consistently returns SKTypeface.Default across all platforms when a font is missing, removing the Android-specific null return. The migration path is clearly documented in the issue. The classification is type/documentation because this is a change-tracking and documentation issue, not a bug report. The issue should remain open until test coverage (BC3_* tests) is added and the change is fully documented in upgrade guides.",
    "resolution": {
      "hypothesis": "The breaking change is intentional and already implemented. The issue needs test coverage added (BC3_FromFamilyNameMissingReturnsNonNull, BC3_MatchFamilyMissingReturnsNull, BC3_FromFamilyNameKnownFontWorks) and documentation finalized before closing.",
      "proposals": [
        {
          "title": "Add missing test coverage for the breaking change",
          "category": "investigation",
          "description": "Add the three test cases mentioned in the issue body (BC3_FromFamilyNameMissingReturnsNonNull, BC3_MatchFamilyMissingReturnsNull, BC3_FromFamilyNameKnownFontWorks) to the SKTypeface test file to confirm the new behavior is verified.",
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Use SKFontManager.Default.MatchFamily for strict null semantics",
          "category": "workaround",
          "description": "Developers who relied on null return from FromFamilyName on Android should migrate to SKFontManager.Default.MatchFamily which still returns null when no match is found.",
          "codeSnippet": "// Migration for code that checks null on Android\nvar tf = SKFontManager.Default.MatchFamily(\"MissingFont\");\nif (tf == null) { /* font not found */ }",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "p1",
      "recommendedReason": "The primary open item for this tracking issue is adding the test coverage. The migration workaround is already documented in the issue body."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "This is a sub-tracker of #3734 documenting an intentional breaking change. It should remain open until the mentioned test cases are added and the change is included in the 4.x upgrade documentation. The implementation is already done.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply triage classification labels",
        "labels": [
          "type/documentation",
          "area/SkiaSharp",
          "os/Android",
          "tenet/compatibility"
        ],
        "risk": "low",
        "confidence": 0.9
      },
      {
        "type": "add-comment",
        "description": "Acknowledge triage and note remaining work",
        "comment": "**Triage note:** This is classified as a documentation/tracking issue for an intentional breaking change in SkiaSharp 4.x (Skia m132 upgrade).\n\nThe code change is confirmed in `binding/SkiaSharp/SKTypeface.cs` — `FromFamilyName` now returns `SKTypeface.Default` consistently across all platforms via `SKFontManager.Default.MatchFamily(familyName, style) ?? Default`.\n\n**Remaining work before closing:**\n- Add test cases: `BC3_FromFamilyNameMissingReturnsNonNull`, `BC3_MatchFamilyMissingReturnsNull`, `BC3_FromFamilyNameKnownFontWorks`\n- Ensure migration guidance is referenced from the 4.x upgrade docs\n\n**Migration:** Use `SKFontManager.Default.MatchFamily(\"FontName\")` if strict null semantics are required.",
        "risk": "medium",
        "confidence": 0.85
      }
    ]
  }
}
```

</details>
