# Issue Triage Report — #3817

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-25T17:47:00Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/SkiaSharp (0.97 (97%)) |
| Suggested action | ready-to-fix (0.90 (90%)) |

**Issue Summary:** Calling SKFontManager.Default in SkiaSharp 4.147.0-preview.1.1 throws a TypeInitializationException due to a circular static constructor dependency between SKFontManager and SKTypeface introduced in the 4.x preview.

**Analysis:** SKTypeface..cctor() calls SKFontManager.Default.Handle, but when SKFontManager..cctor() runs it constructs an SKFontManagerStatic instance (an SKObject subclass), which triggers SKObject..cctor(), which calls SKTypeface.EnsureStaticInstanceAreInitialized(), which triggers SKTypeface..cctor() — at that point SKFontManager.defaultManager is still null, causing a NullReferenceException that propagates as a TypeInitializationException.

**Recommendations:** **ready-to-fix** — Root cause is clearly identified as a circular static initialization between SKTypeface and SKFontManager. The fix path is known. Duplicate is #3865.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic, os/macOS |
| Backends | — |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Install SkiaSharp 4.147.0-preview.1.1
2. Write: using SkiaSharp; var def = SKFontManager.Default;
3. Run on .NET 8 or .NET 10 on Windows or macOS
4. Observe TypeInitializationException

**Environment:** SkiaSharp 4.147.0-preview.1.1, .NET 8 and .NET 10, Windows 11 and macOS

**Related issues:** #3865

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | critical |
| Regression claimed | True |
| Error type | exception |
| Error message | System.TypeInitializationException: The type initializer for 'SkiaSharp.SKFontManager' threw an exception. ---> System.NullReferenceException: Object reference not set to an instance of an object. at SkiaSharp.SKTypeface..cctor() |
| Repro quality | complete |
| Target frameworks | net8.0, net10.0 |

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
| Mentioned versions | 4.147.0-preview.1.1, 3.116.0 |
| Worked in | 3.116.0 |
| Broke in | 4.147.0-preview.1.1 |
| Current relevance | likely |
| Relevance reason | The circular static initialization in SKTypeface..cctor() calling SKFontManager.Default is present in the current codebase. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.97 (97%) |
| Reason | Reporter states it worked in 3.116.0. The static constructor change in SKTypeface that references SKFontManager.Default was introduced in the 4.x preview, creating a circular dependency. |
| Worked in version | 3.116.0 |
| Broke in version | 4.147.0-preview.1.1 |

## Analysis

### Technical Summary

SKTypeface..cctor() calls SKFontManager.Default.Handle, but when SKFontManager..cctor() runs it constructs an SKFontManagerStatic instance (an SKObject subclass), which triggers SKObject..cctor(), which calls SKTypeface.EnsureStaticInstanceAreInitialized(), which triggers SKTypeface..cctor() — at that point SKFontManager.defaultManager is still null, causing a NullReferenceException that propagates as a TypeInitializationException.

### Rationale

The stack trace and code investigation confirm a circular static initialization: SKFontManager.cctor -> SKFontManagerStatic (SKObject) -> SKObject.cctor -> SKTypeface.EnsureStaticInstanceAreInitialized -> SKTypeface.cctor -> SKFontManager.Default (null at this point) -> NullReferenceException. This is a critical regression that blocks any usage of SKFontManager. Issue #3865 is an identical duplicate filed later.

### Key Signals

- "System.NullReferenceException: Object reference not set to an instance of an object. at SkiaSharp.SKTypeface..cctor()" — **issue body** (SKTypeface static ctor accesses a null reference — SKFontManager.defaultManager is null because SKFontManager is still initializing.)
- "Last Known Good Version: 3.116.0" — **issue body** (Regression introduced in the 4.x preview when SKTypeface..cctor() was changed to call SKFontManager.Default.Handle.)
- "Tested with both Win32 native assets package and macos native assets package. Happens on .NET 8 and .NET 10" — **issue body** (Platform-independent; affects all platforms. This is a pure C# circular initialization issue, not a native library problem.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 16-34 | direct | SKTypeface static constructor calls SKFontManager.Default.Handle at line 30, creating a circular dependency: SKFontManager.cctor -> SKObject subclass construction -> SKObject.cctor -> SKTypeface.EnsureStaticInstanceAreInitialized -> SKTypeface.cctor -> SKFontManager.Default (null) |
| `binding/SkiaSharp/SKFontManager.cs` | 15-23 | direct | SKFontManager static constructor creates SKFontManagerStatic (extends SKFontManager : SKObject). This SKObject subclass construction triggers SKObject..cctor() before defaultManager is assigned, creating the circular chain. |
| `binding/SkiaSharp/SKObject.cs` | 39-48 | direct | SKObject static constructor calls SKFontManager.EnsureStaticInstanceAreInitialized() then SKTypeface.EnsureStaticInstanceAreInitialized(). When triggered mid-way through SKFontManager.cctor, this causes SKTypeface.cctor to run and access the not-yet-assigned SKFontManager.Default. |

### Next Questions

- Was the SKTypeface..cctor() change to use SKFontManager.Default.Handle intentional in the 4.x preview?
- The fix should call the native API directly (sk_fontmgr_legacy_create_typeface with a direct fontmgr handle) rather than going through SKFontManager.Default

### Resolution Proposals

**Hypothesis:** SKTypeface..cctor() should not call SKFontManager.Default (which triggers a circular static init chain). Instead, it should call the native sk_fontmgr_create_default() and sk_fontmgr_legacy_create_typeface() APIs directly, bypassing the managed SKFontManager.Default property.

1. **Call native API directly in SKTypeface.cctor** — fix, confidence 0.85 (85%), cost/s, validated=untested
   - Replace SKFontManager.Default.Handle in SKTypeface..cctor() with a direct native call to sk_fontmgr_create_default(), avoiding the circular initialization. After using it, release it or store as an unowned reference.
2. **Reorder EnsureStaticInstanceAreInitialized in SKObject.cctor** — fix, confidence 0.60 (60%), cost/xs, validated=untested
   - Move SKTypeface.EnsureStaticInstanceAreInitialized() before SKFontManager.EnsureStaticInstanceAreInitialized() in SKObject..cctor() so SKTypeface initializes before SKFontManager tries to construct an SKObject.

**Recommended proposal:** Call native API directly in SKTypeface.cctor

**Why:** Avoids the circular dependency structurally. Reordering only masks the symptom and may break again if initialization order changes.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.90 (90%) |
| Reason | Root cause is clearly identified as a circular static initialization between SKTypeface and SKFontManager. The fix path is known. Duplicate is #3865. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply bug, area/SkiaSharp, platform, tenet labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, os/macOS, tenet/reliability, tenet/compatibility |
| link-related | low | 0.97 (97%) | Link to duplicate #3865 which has the same stack trace | linkedIssue=#3865 |
| add-comment | medium | 0.90 (90%) | Post root cause analysis and fix guidance | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report!

This is a confirmed regression in `4.147.0-preview.1.1` caused by a **circular static initialization** between `SKTypeface` and `SKFontManager`:

1. `SKFontManager.get_Default()` triggers `SKFontManager..cctor()`
2. `SKFontManager..cctor()` creates `SKFontManagerStatic` (an `SKObject` subclass)
3. That construction triggers `SKObject..cctor()`
4. `SKObject..cctor()` calls `SKTypeface.EnsureStaticInstanceAreInitialized()` → triggers `SKTypeface..cctor()`
5. `SKTypeface..cctor()` calls `SKFontManager.Default.Handle` — but `defaultManager` is **still null** at this point
6. `NullReferenceException` → `TypeInitializationException`

This is tracked as a duplicate of #3865. The fix requires `SKTypeface..cctor()` to call the native API directly instead of going through `SKFontManager.Default`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3817,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-25T17:47:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Calling SKFontManager.Default in SkiaSharp 4.147.0-preview.1.1 throws a TypeInitializationException due to a circular static constructor dependency between SKFontManager and SKTypeface introduced in the 4.x preview.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.97
    },
    "platforms": [
      "os/Windows-Classic",
      "os/macOS"
    ],
    "tenets": [
      "tenet/reliability",
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "critical",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "System.TypeInitializationException: The type initializer for 'SkiaSharp.SKFontManager' threw an exception. ---> System.NullReferenceException: Object reference not set to an instance of an object. at SkiaSharp.SKTypeface..cctor()",
      "stackTrace": "at SkiaSharp.SKTypeface..cctor()\nat SkiaSharp.SKTypeface.EnsureStaticInstanceAreInitialized()\nat SkiaSharp.SKObject..cctor()\nat SkiaSharp.SKObject..ctor(IntPtr handle, Boolean owns)\nat SkiaSharp.SKFontManager..ctor(IntPtr handle, Boolean owns)\nat SkiaSharp.SKFontManager.SKFontManagerStatic..ctor(IntPtr x)\nat SkiaSharp.SKFontManager..cctor()\nat SkiaSharp.SKFontManager.get_Default()",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net8.0",
        "net10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Install SkiaSharp 4.147.0-preview.1.1",
        "Write: using SkiaSharp; var def = SKFontManager.Default;",
        "Run on .NET 8 or .NET 10 on Windows or macOS",
        "Observe TypeInitializationException"
      ],
      "environmentDetails": "SkiaSharp 4.147.0-preview.1.1, .NET 8 and .NET 10, Windows 11 and macOS",
      "relatedIssues": [
        3865
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.147.0-preview.1.1",
        "3.116.0"
      ],
      "workedIn": "3.116.0",
      "brokeIn": "4.147.0-preview.1.1",
      "currentRelevance": "likely",
      "relevanceReason": "The circular static initialization in SKTypeface..cctor() calling SKFontManager.Default is present in the current codebase."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.97,
      "reason": "Reporter states it worked in 3.116.0. The static constructor change in SKTypeface that references SKFontManager.Default was introduced in the 4.x preview, creating a circular dependency.",
      "workedInVersion": "3.116.0",
      "brokeInVersion": "4.147.0-preview.1.1"
    }
  },
  "analysis": {
    "summary": "SKTypeface..cctor() calls SKFontManager.Default.Handle, but when SKFontManager..cctor() runs it constructs an SKFontManagerStatic instance (an SKObject subclass), which triggers SKObject..cctor(), which calls SKTypeface.EnsureStaticInstanceAreInitialized(), which triggers SKTypeface..cctor() — at that point SKFontManager.defaultManager is still null, causing a NullReferenceException that propagates as a TypeInitializationException.",
    "rationale": "The stack trace and code investigation confirm a circular static initialization: SKFontManager.cctor -> SKFontManagerStatic (SKObject) -> SKObject.cctor -> SKTypeface.EnsureStaticInstanceAreInitialized -> SKTypeface.cctor -> SKFontManager.Default (null at this point) -> NullReferenceException. This is a critical regression that blocks any usage of SKFontManager. Issue #3865 is an identical duplicate filed later.",
    "keySignals": [
      {
        "text": "System.NullReferenceException: Object reference not set to an instance of an object. at SkiaSharp.SKTypeface..cctor()",
        "source": "issue body",
        "interpretation": "SKTypeface static ctor accesses a null reference — SKFontManager.defaultManager is null because SKFontManager is still initializing."
      },
      {
        "text": "Last Known Good Version: 3.116.0",
        "source": "issue body",
        "interpretation": "Regression introduced in the 4.x preview when SKTypeface..cctor() was changed to call SKFontManager.Default.Handle."
      },
      {
        "text": "Tested with both Win32 native assets package and macos native assets package. Happens on .NET 8 and .NET 10",
        "source": "issue body",
        "interpretation": "Platform-independent; affects all platforms. This is a pure C# circular initialization issue, not a native library problem."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "16-34",
        "finding": "SKTypeface static constructor calls SKFontManager.Default.Handle at line 30, creating a circular dependency: SKFontManager.cctor -> SKObject subclass construction -> SKObject.cctor -> SKTypeface.EnsureStaticInstanceAreInitialized -> SKTypeface.cctor -> SKFontManager.Default (null)",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "15-23",
        "finding": "SKFontManager static constructor creates SKFontManagerStatic (extends SKFontManager : SKObject). This SKObject subclass construction triggers SKObject..cctor() before defaultManager is assigned, creating the circular chain.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "39-48",
        "finding": "SKObject static constructor calls SKFontManager.EnsureStaticInstanceAreInitialized() then SKTypeface.EnsureStaticInstanceAreInitialized(). When triggered mid-way through SKFontManager.cctor, this causes SKTypeface.cctor to run and access the not-yet-assigned SKFontManager.Default.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Was the SKTypeface..cctor() change to use SKFontManager.Default.Handle intentional in the 4.x preview?",
      "The fix should call the native API directly (sk_fontmgr_legacy_create_typeface with a direct fontmgr handle) rather than going through SKFontManager.Default"
    ],
    "resolution": {
      "hypothesis": "SKTypeface..cctor() should not call SKFontManager.Default (which triggers a circular static init chain). Instead, it should call the native sk_fontmgr_create_default() and sk_fontmgr_legacy_create_typeface() APIs directly, bypassing the managed SKFontManager.Default property.",
      "proposals": [
        {
          "title": "Call native API directly in SKTypeface.cctor",
          "description": "Replace SKFontManager.Default.Handle in SKTypeface..cctor() with a direct native call to sk_fontmgr_create_default(), avoiding the circular initialization. After using it, release it or store as an unowned reference.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Reorder EnsureStaticInstanceAreInitialized in SKObject.cctor",
          "description": "Move SKTypeface.EnsureStaticInstanceAreInitialized() before SKFontManager.EnsureStaticInstanceAreInitialized() in SKObject..cctor() so SKTypeface initializes before SKFontManager tries to construct an SKObject.",
          "category": "fix",
          "confidence": 0.6,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Call native API directly in SKTypeface.cctor",
      "recommendedReason": "Avoids the circular dependency structurally. Reordering only masks the symptom and may break again if initialization order changes."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.9,
      "reason": "Root cause is clearly identified as a circular static initialization between SKTypeface and SKFontManager. The fix path is known. Duplicate is #3865.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area/SkiaSharp, platform, tenet labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "os/macOS",
          "tenet/reliability",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to duplicate #3865 which has the same stack trace",
        "risk": "low",
        "confidence": 0.97,
        "linkedIssue": 3865
      },
      {
        "type": "add-comment",
        "description": "Post root cause analysis and fix guidance",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed report!\n\nThis is a confirmed regression in `4.147.0-preview.1.1` caused by a **circular static initialization** between `SKTypeface` and `SKFontManager`:\n\n1. `SKFontManager.get_Default()` triggers `SKFontManager..cctor()`\n2. `SKFontManager..cctor()` creates `SKFontManagerStatic` (an `SKObject` subclass)\n3. That construction triggers `SKObject..cctor()`\n4. `SKObject..cctor()` calls `SKTypeface.EnsureStaticInstanceAreInitialized()` → triggers `SKTypeface..cctor()`\n5. `SKTypeface..cctor()` calls `SKFontManager.Default.Handle` — but `defaultManager` is **still null** at this point\n6. `NullReferenceException` → `TypeInitializationException`\n\nThis is tracked as a duplicate of #3865. The fix requires `SKTypeface..cctor()` to call the native API directly instead of going through `SKFontManager.Default`."
      }
    ]
  }
}
```

</details>
