# Issue Triage Report — #3865

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T13:35:00Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-duplicate (0.92 (92%)) |

**Issue Summary:** SkiaSharp 4.147.0-preview.1.1 crashes on startup with a TypeInitializationException caused by a circular static initializer between SKTypeface and SKFontManager, which is a duplicate of issue #3817.

**Analysis:** In SkiaSharp 4.x, SKTypeface's static constructor calls SKFontManager.Default.Handle (binding/SkiaSharp/SKTypeface.cs line 29-30). When SKFontManager.Default is first accessed, SKFontManager's cctor instantiates SKFontManagerStatic, which calls SKObject..ctor, triggering SKObject..cctor. SKObject's cctor calls SKTypeface.EnsureStaticInstanceAreInitialized(), which triggers SKTypeface..cctor again — but SKFontManager.Default is not yet assigned, returning null, causing NullReferenceException. This is a circular static initialization bug.

**Recommendations:** **close-as-duplicate** — Issue #3817 reports the identical TypeInitializationException/NullReferenceException in SKTypeface..cctor when accessing SKFontManager.Default, in the same SkiaSharp 4.147.0-preview.1.1 version.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create an Avalonia app targeting SkiaSharp 4.147.0-preview.1.1
2. Run the application on Windows 11
3. App crashes immediately with TypeInitializationException for SKFontManager

**Environment:** Visual Studio on Windows 11 25H2, Avalonia 11.3.14, SkiaSharp 4.147.0-preview.1.1

**Related issues:** #3817

**Screenshots:**
- https://github.com/user-attachments/assets/6a8ca53f-2aac-4d76-bda7-74eace018829 — Screenshot of the crash output in Visual Studio on Windows 11

**Code snippets:**

```csharp
AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace()
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | exception |
| Error message | System.TypeInitializationException: The type initializer for 'SkiaSharp.SKFontManager' threw an exception. ---> NullReferenceException at SKTypeface..cctor() |
| Repro quality | complete |
| Target frameworks | net8.0, net9.0 |

**Stack trace:**

```text
at SkiaSharp.SKTypeface..cctor()
at SkiaSharp.SKTypeface.EnsureStaticInstanceAreInitialized()
at SkiaSharp.SKObject..cctor()
at SkiaSharp.SKObject..ctor(IntPtr handle, Boolean owns)
at SkiaSharp.SKFontManager..ctor(IntPtr handle, Boolean owns)
at SkiaSharp.SKFontManager.SKFontManagerStatic..ctor(IntPtr x)
at SkiaSharp.SKFontManager..cctor()
at SkiaSharp.SKFontManager.get_Default()
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 4.147.0-preview.1.1, 3.119.4-preview.1.1 |
| Worked in | 3.119.4-preview.1.1 |
| Broke in | 4.147.0-preview.1.1 |
| Current relevance | likely |
| Relevance reason | Regression introduced in 4.x preview series; 3.x still works. Issue is reproduced on both Windows and macOS per related issue #3817. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.97 (97%) |
| Reason | Works in 3.119.4-preview.1.1, fails in 4.147.0-preview.1.1 with the same codebase. Issue #3817 corroborates on both Windows and macOS. |
| Worked in version | 3.119.4-preview.1.1 |
| Broke in version | 4.147.0-preview.1.1 |

## Analysis

### Technical Summary

In SkiaSharp 4.x, SKTypeface's static constructor calls SKFontManager.Default.Handle (binding/SkiaSharp/SKTypeface.cs line 29-30). When SKFontManager.Default is first accessed, SKFontManager's cctor instantiates SKFontManagerStatic, which calls SKObject..ctor, triggering SKObject..cctor. SKObject's cctor calls SKTypeface.EnsureStaticInstanceAreInitialized(), which triggers SKTypeface..cctor again — but SKFontManager.Default is not yet assigned, returning null, causing NullReferenceException. This is a circular static initialization bug.

### Rationale

Classified as type/bug with high severity because the app crashes immediately on startup with no workaround in 4.x. Duplicate of #3817 which describes the identical stack trace. The root cause is a circular static initialization between SKTypeface and SKFontManager introduced in 4.x.

### Key Signals

- "System.NullReferenceException: Object reference not set to an instance of an object. at SkiaSharp.SKTypeface..cctor()" — **issue body / stack trace** (SKFontManager.Default is null when accessed from SKTypeface's static constructor due to circular static initialization.)
- "but when I use SkiaSharp 3.119.4-preview.1.1, it is normal." — **issue body** (Confirmed regression introduced in 4.x branch.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 16-33 | direct | Static constructor calls SKFontManager.Default.Handle at line 30 to initialize the defaultTypeface field. If SKFontManager's own static initializer has not completed (circular dependency), SKFontManager.Default returns null. |
| `binding/SkiaSharp/SKObject.cs` | 39-48 | direct | SKObject's static constructor calls both SKFontManager.EnsureStaticInstanceAreInitialized() (line 45) and SKTypeface.EnsureStaticInstanceAreInitialized() (line 46), creating the circular chain: SKFontManager..cctor → SKObject..ctor → SKObject..cctor → SKTypeface..cctor → SKFontManager.Default (not yet set). |

**Error fingerprint:** `TypeInitializationException:SKFontManager<-NullReferenceException:SKTypeface..cctor`

### Workarounds

- Downgrade to SkiaSharp 3.119.x until the circular initialization is resolved in 4.x.

### Next Questions

- Was SKFontManager.Default moved/refactored in 4.x such that it now depends on SKObject before it was previously initialized?
- Can SKTypeface's static constructor be refactored to defer the SKFontManager.Default call until after static initialization completes?

### Resolution Proposals

1. **Close as duplicate of #3817** — investigation, cost/xs, validated=untested
   - Issue #3817 reports the exact same TypeInitializationException with the same stack trace in the same version (4.147.0-preview.1.1). This issue should be closed as a duplicate with a cross-reference.

**Recommended proposal:** close-duplicate

**Why:** Issue #3817 is an exact duplicate filed 6 days earlier, confirmed on multiple platforms. Consolidating tracking reduces noise.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-duplicate |
| Confidence | 0.92 (92%) |
| Reason | Issue #3817 reports the identical TypeInitializationException/NullReferenceException in SKTypeface..cctor when accessing SKFontManager.Default, in the same SkiaSharp 4.147.0-preview.1.1 version. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.98 (98%) | Apply classification labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability, tenet/compatibility |
| link-duplicate | medium | 0.92 (92%) | Mark as duplicate of #3817 (same SKFontManager.Default TypeInitializationException in 4.147.0-preview.1.1) | linkedIssue=#3817 |
| close-issue | medium | 0.92 (92%) | Close as duplicate — exact same crash reported in #3817 | stateReason=not_planned |
| add-comment | medium | 0.92 (92%) | Inform reporter this is a known issue tracked in #3817 | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the report! This crash is a known regression introduced in SkiaSharp 4.147.0-preview.1.1 and is tracked in #3817.

The root cause is a circular static initializer between `SKTypeface` and `SKFontManager`: `SKTypeface`'s static constructor calls `SKFontManager.Default.Handle` before `SKFontManager`'s own static initialization completes, resulting in a `NullReferenceException`.

**Workaround:** Until this is fixed in 4.x, downgrade to `SkiaSharp 3.119.4-preview.1.1` (or the latest stable 3.x release).

Tracking this in #3817 — please follow that issue for updates.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3865,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T13:35:00Z"
  },
  "summary": "SkiaSharp 4.147.0-preview.1.1 crashes on startup with a TypeInitializationException caused by a circular static initializer between SKTypeface and SKFontManager, which is a duplicate of issue #3817.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability",
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "System.TypeInitializationException: The type initializer for 'SkiaSharp.SKFontManager' threw an exception. ---> NullReferenceException at SKTypeface..cctor()",
      "stackTrace": "at SkiaSharp.SKTypeface..cctor()\nat SkiaSharp.SKTypeface.EnsureStaticInstanceAreInitialized()\nat SkiaSharp.SKObject..cctor()\nat SkiaSharp.SKObject..ctor(IntPtr handle, Boolean owns)\nat SkiaSharp.SKFontManager..ctor(IntPtr handle, Boolean owns)\nat SkiaSharp.SKFontManager.SKFontManagerStatic..ctor(IntPtr x)\nat SkiaSharp.SKFontManager..cctor()\nat SkiaSharp.SKFontManager.get_Default()",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net8.0",
        "net9.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an Avalonia app targeting SkiaSharp 4.147.0-preview.1.1",
        "Run the application on Windows 11",
        "App crashes immediately with TypeInitializationException for SKFontManager"
      ],
      "codeSnippets": [
        "AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace()"
      ],
      "screenshots": [
        {
          "url": "https://github.com/user-attachments/assets/6a8ca53f-2aac-4d76-bda7-74eace018829",
          "description": "Screenshot of the crash output in Visual Studio on Windows 11"
        }
      ],
      "environmentDetails": "Visual Studio on Windows 11 25H2, Avalonia 11.3.14, SkiaSharp 4.147.0-preview.1.1",
      "relatedIssues": [
        3817
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.147.0-preview.1.1",
        "3.119.4-preview.1.1"
      ],
      "workedIn": "3.119.4-preview.1.1",
      "brokeIn": "4.147.0-preview.1.1",
      "currentRelevance": "likely",
      "relevanceReason": "Regression introduced in 4.x preview series; 3.x still works. Issue is reproduced on both Windows and macOS per related issue #3817."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.97,
      "reason": "Works in 3.119.4-preview.1.1, fails in 4.147.0-preview.1.1 with the same codebase. Issue #3817 corroborates on both Windows and macOS.",
      "workedInVersion": "3.119.4-preview.1.1",
      "brokeInVersion": "4.147.0-preview.1.1"
    }
  },
  "analysis": {
    "summary": "In SkiaSharp 4.x, SKTypeface's static constructor calls SKFontManager.Default.Handle (binding/SkiaSharp/SKTypeface.cs line 29-30). When SKFontManager.Default is first accessed, SKFontManager's cctor instantiates SKFontManagerStatic, which calls SKObject..ctor, triggering SKObject..cctor. SKObject's cctor calls SKTypeface.EnsureStaticInstanceAreInitialized(), which triggers SKTypeface..cctor again — but SKFontManager.Default is not yet assigned, returning null, causing NullReferenceException. This is a circular static initialization bug.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "16-33",
        "finding": "Static constructor calls SKFontManager.Default.Handle at line 30 to initialize the defaultTypeface field. If SKFontManager's own static initializer has not completed (circular dependency), SKFontManager.Default returns null.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "39-48",
        "finding": "SKObject's static constructor calls both SKFontManager.EnsureStaticInstanceAreInitialized() (line 45) and SKTypeface.EnsureStaticInstanceAreInitialized() (line 46), creating the circular chain: SKFontManager..cctor → SKObject..ctor → SKObject..cctor → SKTypeface..cctor → SKFontManager.Default (not yet set).",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "System.NullReferenceException: Object reference not set to an instance of an object. at SkiaSharp.SKTypeface..cctor()",
        "source": "issue body / stack trace",
        "interpretation": "SKFontManager.Default is null when accessed from SKTypeface's static constructor due to circular static initialization."
      },
      {
        "text": "but when I use SkiaSharp 3.119.4-preview.1.1, it is normal.",
        "source": "issue body",
        "interpretation": "Confirmed regression introduced in 4.x branch."
      }
    ],
    "rationale": "Classified as type/bug with high severity because the app crashes immediately on startup with no workaround in 4.x. Duplicate of #3817 which describes the identical stack trace. The root cause is a circular static initialization between SKTypeface and SKFontManager introduced in 4.x.",
    "workarounds": [
      "Downgrade to SkiaSharp 3.119.x until the circular initialization is resolved in 4.x."
    ],
    "nextQuestions": [
      "Was SKFontManager.Default moved/refactored in 4.x such that it now depends on SKObject before it was previously initialized?",
      "Can SKTypeface's static constructor be refactored to defer the SKFontManager.Default call until after static initialization completes?"
    ],
    "errorFingerprint": "TypeInitializationException:SKFontManager<-NullReferenceException:SKTypeface..cctor",
    "resolution": {
      "proposals": [
        {
          "title": "Close as duplicate of #3817",
          "category": "investigation",
          "description": "Issue #3817 reports the exact same TypeInitializationException with the same stack trace in the same version (4.147.0-preview.1.1). This issue should be closed as a duplicate with a cross-reference.",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "close-duplicate",
      "recommendedReason": "Issue #3817 is an exact duplicate filed 6 days earlier, confirmed on multiple platforms. Consolidating tracking reduces noise."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-duplicate",
      "confidence": 0.92,
      "reason": "Issue #3817 reports the identical TypeInitializationException/NullReferenceException in SKTypeface..cctor when accessing SKFontManager.Default, in the same SkiaSharp 4.147.0-preview.1.1 version.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels",
        "risk": "low",
        "confidence": 0.98,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/reliability",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-duplicate",
        "description": "Mark as duplicate of #3817 (same SKFontManager.Default TypeInitializationException in 4.147.0-preview.1.1)",
        "risk": "medium",
        "confidence": 0.92,
        "linkedIssue": 3817
      },
      {
        "type": "close-issue",
        "description": "Close as duplicate — exact same crash reported in #3817",
        "risk": "medium",
        "confidence": 0.92,
        "stateReason": "not_planned"
      },
      {
        "type": "add-comment",
        "description": "Inform reporter this is a known issue tracked in #3817",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "Thank you for the report! This crash is a known regression introduced in SkiaSharp 4.147.0-preview.1.1 and is tracked in #3817.\n\nThe root cause is a circular static initializer between `SKTypeface` and `SKFontManager`: `SKTypeface`'s static constructor calls `SKFontManager.Default.Handle` before `SKFontManager`'s own static initialization completes, resulting in a `NullReferenceException`.\n\n**Workaround:** Until this is fixed in 4.x, downgrade to `SkiaSharp 3.119.4-preview.1.1` (or the latest stable 3.x release).\n\nTracking this in #3817 — please follow that issue for updates."
      }
    ]
  }
}
```

</details>
