# Issue Triage Report — #3832

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-08T20:28:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.97 (97%)) |
| Suggested action | close-as-duplicate (0.95 (95%)) |

**Issue Summary:** NullReferenceException in SKTypeface static constructor because SKFontStyle.Normal is null during static initialization of 4.x preview; duplicate of #3817.

**Analysis:** SKTypeface's static constructor calls SKFontStyle.Normal.Handle (line 30 of SKTypeface.cs). The SKFontStyle.Normal property returns the private static field `normal`, which is set in SKFontStyle's static constructor. However, creating the first SKFontStyleStatic instance (in SKFontStyle's static ctor) calls SKObject's constructor, which triggers SKObject's static ctor. SKObject's static ctor calls SKTypeface.EnsureStaticInstanceAreInitialized(), which triggers SKTypeface's static ctor on the same thread. The .NET CLR allows the re-entrant call to proceed without blocking, but SKFontStyle's static fields haven't been assigned yet, so Normal returns null, causing a NullReferenceException.

**Recommendations:** **close-as-duplicate** — Community commenter and code investigation both confirm this is the same circular static-init bug reported in #3817.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3817 — linked repro

**Code snippets:**

```csharp
return SKFontStyle.Normal; // returns null during SKTypeface..cctor()
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | exception |
| Error message | NullReferenceException: SKFontStyle.Normal is null in SKTypeface static ctor |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 4.x preview, 3.116.0 |
| Worked in | 3.116.0 |
| Broke in | 4.x preview |
| Current relevance | likely |
| Relevance reason | The static initialization circular dependency still exists in the current codebase |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.95 (95%) |
| Reason | Reporter confirms it worked in 3.116.0; broke in 4.x preview. Stack trace in #3817 confirms this is a newly introduced circular static-init dependency. |
| Worked in version | 3.116.0 |
| Broke in version | 4.x preview |

## Analysis

### Technical Summary

SKTypeface's static constructor calls SKFontStyle.Normal.Handle (line 30 of SKTypeface.cs). The SKFontStyle.Normal property returns the private static field `normal`, which is set in SKFontStyle's static constructor. However, creating the first SKFontStyleStatic instance (in SKFontStyle's static ctor) calls SKObject's constructor, which triggers SKObject's static ctor. SKObject's static ctor calls SKTypeface.EnsureStaticInstanceAreInitialized(), which triggers SKTypeface's static ctor on the same thread. The .NET CLR allows the re-entrant call to proceed without blocking, but SKFontStyle's static fields haven't been assigned yet, so Normal returns null, causing a NullReferenceException.

### Rationale

This is a duplicate of #3817. Both issues describe the same NullReferenceException caused by circular static initialization between SKTypeface, SKFontStyle, and SKObject introduced in the 4.x preview. The root cause is the re-entrant static constructor chain where SKObject..cctor() triggers SKTypeface..cctor(), which accesses SKFontStyle.Normal before SKFontStyle's own static ctor has completed its field assignments.

### Key Signals

- "SKFontStyle.Normal is null" — **issue body** (Static field not yet assigned when accessed via re-entrant static ctor call)
- "It's also a duplicate of https://github.com/mono/SkiaSharp/issues/3817" — **comment by EmergentOrder** (Community has already identified the duplicate)
- "Last Known Good Version: 3.116.0" — **issue body** (Regression introduced in 4.x preview series)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 17-34 | direct | SKTypeface static ctor calls SKFontStyle.Normal.Handle at line 30. This is the crash site. |
| `binding/SkiaSharp/SKFontStyle.cs` | 9-20 | direct | SKFontStyle static ctor assigns `normal` field by creating SKFontStyleStatic instances. Creating the first instance triggers SKObject ctor → SKObject static ctor → SKTypeface.EnsureStaticInstanceAreInitialized() → SKTypeface static ctor (reentrant), which accesses SKFontStyle.Normal before the assignment completes. |
| `binding/SkiaSharp/SKObject.cs` | 39-49 | direct | SKObject static ctor calls SKFontManager.EnsureStaticInstanceAreInitialized() followed by SKTypeface.EnsureStaticInstanceAreInitialized(). This chain initiates the circular dependency when SKFontManager (or any SKObject subclass) is constructed during SKFontStyle static initialization. |

### Next Questions

- What is the fix in #3817 — has a PR been opened to resolve the circular static init?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-duplicate |
| Confidence | 0.95 (95%) |
| Reason | Community commenter and code investigation both confirm this is the same circular static-init bug reported in #3817. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply classification labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability |
| link-duplicate | medium | 0.95 (95%) | Mark as duplicate of #3817 (same circular static initialization NullReferenceException) | linkedIssue=#3817 |
| add-comment | medium | 0.95 (95%) | Inform reporter this is a known duplicate of #3817 | — |
| close-issue | medium | 0.95 (95%) | Close as duplicate of #3817 | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! This is a known issue tracked in #3817 — both describe the same `NullReferenceException` caused by a circular static-initialization chain between `SKTypeface`, `SKFontStyle`, and `SKObject` introduced in the 4.x preview.

The root cause: `SKObject..cctor()` triggers `SKTypeface..cctor()`, which accesses `SKFontStyle.Normal` before `SKFontStyle`'s own static constructor has finished assigning the `normal` field. .NET's re-entrant static initialization allows the second call to proceed, but the field is still null.

Please follow #3817 for updates on the fix. Closing this as a duplicate.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3832,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-08T20:28:00Z"
  },
  "summary": "NullReferenceException in SKTypeface static constructor because SKFontStyle.Normal is null during static initialization of 4.x preview; duplicate of #3817.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.97
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "NullReferenceException: SKFontStyle.Normal is null in SKTypeface static ctor",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "codeSnippets": [
        "return SKFontStyle.Normal; // returns null during SKTypeface..cctor()"
      ],
      "repoLinks": [
        {
          "number": 3817,
          "url": "https://github.com/mono/SkiaSharp/issues/3817",
          "relation": "duplicate"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.x preview",
        "3.116.0"
      ],
      "workedIn": "3.116.0",
      "brokeIn": "4.x preview",
      "currentRelevance": "likely",
      "relevanceReason": "The static initialization circular dependency still exists in the current codebase"
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.95,
      "reason": "Reporter confirms it worked in 3.116.0; broke in 4.x preview. Stack trace in #3817 confirms this is a newly introduced circular static-init dependency.",
      "workedInVersion": "3.116.0",
      "brokeInVersion": "4.x preview"
    }
  },
  "analysis": {
    "summary": "SKTypeface's static constructor calls SKFontStyle.Normal.Handle (line 30 of SKTypeface.cs). The SKFontStyle.Normal property returns the private static field `normal`, which is set in SKFontStyle's static constructor. However, creating the first SKFontStyleStatic instance (in SKFontStyle's static ctor) calls SKObject's constructor, which triggers SKObject's static ctor. SKObject's static ctor calls SKTypeface.EnsureStaticInstanceAreInitialized(), which triggers SKTypeface's static ctor on the same thread. The .NET CLR allows the re-entrant call to proceed without blocking, but SKFontStyle's static fields haven't been assigned yet, so Normal returns null, causing a NullReferenceException.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "17-34",
        "finding": "SKTypeface static ctor calls SKFontStyle.Normal.Handle at line 30. This is the crash site.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontStyle.cs",
        "lines": "9-20",
        "finding": "SKFontStyle static ctor assigns `normal` field by creating SKFontStyleStatic instances. Creating the first instance triggers SKObject ctor → SKObject static ctor → SKTypeface.EnsureStaticInstanceAreInitialized() → SKTypeface static ctor (reentrant), which accesses SKFontStyle.Normal before the assignment completes.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "39-49",
        "finding": "SKObject static ctor calls SKFontManager.EnsureStaticInstanceAreInitialized() followed by SKTypeface.EnsureStaticInstanceAreInitialized(). This chain initiates the circular dependency when SKFontManager (or any SKObject subclass) is constructed during SKFontStyle static initialization.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "SKFontStyle.Normal is null",
        "source": "issue body",
        "interpretation": "Static field not yet assigned when accessed via re-entrant static ctor call"
      },
      {
        "text": "It's also a duplicate of https://github.com/mono/SkiaSharp/issues/3817",
        "source": "comment by EmergentOrder",
        "interpretation": "Community has already identified the duplicate"
      },
      {
        "text": "Last Known Good Version: 3.116.0",
        "source": "issue body",
        "interpretation": "Regression introduced in 4.x preview series"
      }
    ],
    "rationale": "This is a duplicate of #3817. Both issues describe the same NullReferenceException caused by circular static initialization between SKTypeface, SKFontStyle, and SKObject introduced in the 4.x preview. The root cause is the re-entrant static constructor chain where SKObject..cctor() triggers SKTypeface..cctor(), which accesses SKFontStyle.Normal before SKFontStyle's own static ctor has completed its field assignments.",
    "nextQuestions": [
      "What is the fix in #3817 — has a PR been opened to resolve the circular static init?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-duplicate",
      "confidence": 0.95,
      "reason": "Community commenter and code investigation both confirm this is the same circular static-init bug reported in #3817.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "id": "labels-1",
        "type": "update-labels",
        "description": "Apply classification labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "id": "link-1",
        "type": "link-duplicate",
        "description": "Mark as duplicate of #3817 (same circular static initialization NullReferenceException)",
        "risk": "medium",
        "confidence": 0.95,
        "linkedIssue": 3817
      },
      {
        "id": "comment-1",
        "type": "add-comment",
        "description": "Inform reporter this is a known duplicate of #3817",
        "risk": "medium",
        "confidence": 0.95,
        "comment": "Thanks for the detailed report! This is a known issue tracked in #3817 — both describe the same `NullReferenceException` caused by a circular static-initialization chain between `SKTypeface`, `SKFontStyle`, and `SKObject` introduced in the 4.x preview.\n\nThe root cause: `SKObject..cctor()` triggers `SKTypeface..cctor()`, which accesses `SKFontStyle.Normal` before `SKFontStyle`'s own static constructor has finished assigning the `normal` field. .NET's re-entrant static initialization allows the second call to proceed, but the field is still null.\n\nPlease follow #3817 for updates on the fix. Closing this as a duplicate."
      },
      {
        "id": "close-1",
        "type": "close-issue",
        "description": "Close as duplicate of #3817",
        "risk": "medium",
        "confidence": 0.95,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
