# Issue Triage Report — #1761

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T07:50:00Z |
| Type | type/bug (0.82 (82%)) |
| Area | area/libSkiaSharp.native (0.80 (80%)) |
| Suggested action | needs-info (0.78 (78%)) |

**Issue Summary:** iOS app crashes with SIGSEGV in dlopen/LoadFrameworks at startup during Xamarin Hot Restart debug deployment after adding SkiaSharp and SkiaSharp.Views.Forms NuGet packages to a Xamarin.Forms project.

**Analysis:** The crash occurs in Xamarin Hot Restart's native framework loading (dlopen) at app startup. Xamarin Hot Restart uses a pre-built iOS container ('Xamarin.PreBuilt.iOS') that dynamically loads NuGet-contributed native frameworks at debug time. The SkiaSharp native framework (libSkiaSharp) cannot be successfully loaded via dlopen in this Hot Restart pre-built container, resulting in a SIGSEGV. This is a known interaction limitation between Xamarin Hot Restart and SkiaSharp's native iOS framework. The same user filed a nearly identical issue (#1771) four days later with a more explicit stack trace showing 'Xamarin.iOS.HotRestart.Application:LoadFrameworks'.

**Recommendations:** **needs-info** — Crash pattern strongly suggests Xamarin Hot Restart incompatibility with SkiaSharp native framework, but SkiaSharp version is missing and we need to confirm whether disabling Hot Restart resolves the issue. A near-duplicate #1771 was filed by the same user.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Forms project
2. Add SkiaSharp and SkiaSharp.Views.Forms NuGet packages
3. Run the project in Debug mode on a physical iPhone SE (iOS 14.7.1)
4. Observe SIGSEGV crash in dlopen during app startup

**Environment:** iPhone SE iOS 14.7.1, Windows 10 Pro, VS 2019 CE, Xamarin 16.10.000.234

**Related issues:** #1771

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | crash |
| Error message | Got a segv while executing native code — ObjCRuntime.Dlfcn:_dlopen crash in Xamarin.PreBuilt.iOS:LoadFrameworks |
| Repro quality | complete |
| Target frameworks | xamarinios10 |

**Stack trace:**

```text
at ObjCRuntime.Dlfcn:_dlopen -> ObjCRuntime.Dlfcn:dlopen -> Xamarin.PreBuilt.iOS.Applications:LoadFrameworks -> Xamarin.PreBuilt.iOS.Applications:Main
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | SkiaSharp version not mentioned in the issue. Xamarin.Forms with Xamarin 16.10 suggests an older SkiaSharp (1.x or 2.x era). |

## Analysis

### Technical Summary

The crash occurs in Xamarin Hot Restart's native framework loading (dlopen) at app startup. Xamarin Hot Restart uses a pre-built iOS container ('Xamarin.PreBuilt.iOS') that dynamically loads NuGet-contributed native frameworks at debug time. The SkiaSharp native framework (libSkiaSharp) cannot be successfully loaded via dlopen in this Hot Restart pre-built container, resulting in a SIGSEGV. This is a known interaction limitation between Xamarin Hot Restart and SkiaSharp's native iOS framework. The same user filed a nearly identical issue (#1771) four days later with a more explicit stack trace showing 'Xamarin.iOS.HotRestart.Application:LoadFrameworks'.

### Rationale

Classified as type/bug in area/libSkiaSharp.native because the crash is a SIGSEGV during native framework loading (dlopen) — a real crash with full stack trace. The root cause is an incompatibility between Xamarin Hot Restart and SkiaSharp's native iOS framework packaging. The area is libSkiaSharp.native because the failure is in loading the native binary. Severity is medium because Release builds (non-Hot Restart) presumably work, limiting impact to Debug workflow only. The same user filed #1771 as a near-duplicate.

### Key Signals

- "Xamarin.PreBuilt.iOS.Applications:LoadFrameworks → ObjCRuntime.Dlfcn:dlopen" — **issue body stack trace** (The crash is inside Xamarin Hot Restart's framework loading routine, not inside SkiaSharp managed code. The pre-built container is trying to dlopen the SkiaSharp native framework and crashing.)
- "In Debug mode - app Crashes" — **issue body** (Debug mode implies Xamarin Hot Restart is active. Release mode would use a different deployment path and likely does not crash.)
- "Identical crash in issue #1771 filed by the same user 4 days later, with stack trace showing 'Xamarin.iOS.HotRestart.Application:LoadFrameworks'" — **GitHub issue search** (The duplicate issue #1771 makes the Hot Restart involvement explicit. The reporter may not have realized these are the same root cause.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKCanvasView.cs` | 1-153 | context | iOS SKCanvasView implementation is clean and does not crash at initialization. The crash is earlier — at native framework load time during Xamarin Hot Restart's dlopen call, before any SkiaSharp managed code runs. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKCGSurfaceFactory.cs` | 1-50 | context | Surface factory is not involved — crash is at dlopen level before any SkiaSharp class is instantiated. |

### Workarounds

- Disable Xamarin Hot Restart: In Visual Studio, go to Tools > Options > Xamarin > iOS Settings and uncheck 'Enable Hot Restart'. Then redeploy in Debug mode.
- Build and deploy in Release mode instead of Debug mode to bypass the Hot Restart pre-built container.
- Alternatively, use a Mac build host for iOS debugging (non-Hot Restart path).

### Next Questions

- Which version of SkiaSharp NuGet packages is the reporter using?
- Does the crash also occur when Hot Restart is disabled in VS settings?
- Does the crash occur with Release configuration?
- Is this reproducible with only SkiaSharp (not SkiaSharp.Views.Forms) referenced?

### Resolution Proposals

**Hypothesis:** Xamarin Hot Restart's pre-built container uses dlopen to load native frameworks at debug time. SkiaSharp's native iOS framework may have link flags or dependencies that prevent successful dynamic loading in this restricted Hot Restart environment, causing a SIGSEGV.

1. **Disable Hot Restart as workaround** — workaround, confidence 0.85 (85%), cost/xs, validated=untested
   - Disable Xamarin Hot Restart in Visual Studio IDE settings. This forces a full deployment without the pre-built container, and the SkiaSharp native framework loads via the normal iOS app bundle mechanism.
2. **Investigate SkiaSharp framework packaging for Hot Restart compatibility** — investigation, confidence 0.60 (60%), cost/l, validated=untested
   - Investigate whether SkiaSharp's iOS native framework bundle (XCFramework packaging, bitcode, or symbol visibility settings) can be adjusted to support Xamarin Hot Restart's dlopen loading path.

**Recommended proposal:** Disable Hot Restart as workaround

**Why:** Immediate actionable workaround for the reporter. The root cause (Hot Restart + native framework dlopen incompatibility) requires deeper investigation to fix properly.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.78 (78%) |
| Reason | Crash pattern strongly suggests Xamarin Hot Restart incompatibility with SkiaSharp native framework, but SkiaSharp version is missing and we need to confirm whether disabling Hot Restart resolves the issue. A near-duplicate #1771 was filed by the same user. |
| Suggested repro platform | macos |

### Missing Info

- SkiaSharp NuGet package version being used
- Whether the crash occurs when Xamarin Hot Restart is disabled in VS settings
- Whether the crash occurs in Release mode

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, native library, and iOS labels | labels=type/bug, area/libSkiaSharp.native, os/iOS, tenet/reliability |
| link-related | low | 0.95 (95%) | Link to near-duplicate #1771 filed by same user 4 days later with same crash | linkedIssue=#1771 |
| add-comment | medium | 0.82 (82%) | Request SkiaSharp version and ask reporter to try disabling Hot Restart | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report. The stack trace (`Xamarin.PreBuilt.iOS.Applications:LoadFrameworks → dlopen`) points to **Xamarin Hot Restart** as the likely culprit. Hot Restart uses a pre-built app container to dynamically load native frameworks at debug time, and SkiaSharp's native iOS framework may be incompatible with this loading mechanism.

Could you try the following?

1. **Disable Hot Restart**: In Visual Studio, go to **Tools > Options > Xamarin > iOS Settings** and uncheck *Enable Hot Restart*. Then redeploy in Debug mode.
2. **Try Release mode** to confirm the app works without Hot Restart.

Also, which version of the SkiaSharp NuGet packages are you using?

Note: You also filed a similar report in #1771 — these appear to be the same issue.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1761,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T07:50:00Z"
  },
  "summary": "iOS app crashes with SIGSEGV in dlopen/LoadFrameworks at startup during Xamarin Hot Restart debug deployment after adding SkiaSharp and SkiaSharp.Views.Forms NuGet packages to a Xamarin.Forms project.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.82
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.8
    },
    "platforms": [
      "os/iOS"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "Got a segv while executing native code — ObjCRuntime.Dlfcn:_dlopen crash in Xamarin.PreBuilt.iOS:LoadFrameworks",
      "stackTrace": "at ObjCRuntime.Dlfcn:_dlopen -> ObjCRuntime.Dlfcn:dlopen -> Xamarin.PreBuilt.iOS.Applications:LoadFrameworks -> Xamarin.PreBuilt.iOS.Applications:Main",
      "reproQuality": "complete",
      "targetFrameworks": [
        "xamarinios10"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms project",
        "Add SkiaSharp and SkiaSharp.Views.Forms NuGet packages",
        "Run the project in Debug mode on a physical iPhone SE (iOS 14.7.1)",
        "Observe SIGSEGV crash in dlopen during app startup"
      ],
      "environmentDetails": "iPhone SE iOS 14.7.1, Windows 10 Pro, VS 2019 CE, Xamarin 16.10.000.234",
      "relatedIssues": [
        1771
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unknown",
      "relevanceReason": "SkiaSharp version not mentioned in the issue. Xamarin.Forms with Xamarin 16.10 suggests an older SkiaSharp (1.x or 2.x era)."
    }
  },
  "analysis": {
    "summary": "The crash occurs in Xamarin Hot Restart's native framework loading (dlopen) at app startup. Xamarin Hot Restart uses a pre-built iOS container ('Xamarin.PreBuilt.iOS') that dynamically loads NuGet-contributed native frameworks at debug time. The SkiaSharp native framework (libSkiaSharp) cannot be successfully loaded via dlopen in this Hot Restart pre-built container, resulting in a SIGSEGV. This is a known interaction limitation between Xamarin Hot Restart and SkiaSharp's native iOS framework. The same user filed a nearly identical issue (#1771) four days later with a more explicit stack trace showing 'Xamarin.iOS.HotRestart.Application:LoadFrameworks'.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKCanvasView.cs",
        "lines": "1-153",
        "finding": "iOS SKCanvasView implementation is clean and does not crash at initialization. The crash is earlier — at native framework load time during Xamarin Hot Restart's dlopen call, before any SkiaSharp managed code runs.",
        "relevance": "context"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKCGSurfaceFactory.cs",
        "lines": "1-50",
        "finding": "Surface factory is not involved — crash is at dlopen level before any SkiaSharp class is instantiated.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Xamarin.PreBuilt.iOS.Applications:LoadFrameworks → ObjCRuntime.Dlfcn:dlopen",
        "source": "issue body stack trace",
        "interpretation": "The crash is inside Xamarin Hot Restart's framework loading routine, not inside SkiaSharp managed code. The pre-built container is trying to dlopen the SkiaSharp native framework and crashing."
      },
      {
        "text": "In Debug mode - app Crashes",
        "source": "issue body",
        "interpretation": "Debug mode implies Xamarin Hot Restart is active. Release mode would use a different deployment path and likely does not crash."
      },
      {
        "text": "Identical crash in issue #1771 filed by the same user 4 days later, with stack trace showing 'Xamarin.iOS.HotRestart.Application:LoadFrameworks'",
        "source": "GitHub issue search",
        "interpretation": "The duplicate issue #1771 makes the Hot Restart involvement explicit. The reporter may not have realized these are the same root cause."
      }
    ],
    "rationale": "Classified as type/bug in area/libSkiaSharp.native because the crash is a SIGSEGV during native framework loading (dlopen) — a real crash with full stack trace. The root cause is an incompatibility between Xamarin Hot Restart and SkiaSharp's native iOS framework packaging. The area is libSkiaSharp.native because the failure is in loading the native binary. Severity is medium because Release builds (non-Hot Restart) presumably work, limiting impact to Debug workflow only. The same user filed #1771 as a near-duplicate.",
    "workarounds": [
      "Disable Xamarin Hot Restart: In Visual Studio, go to Tools > Options > Xamarin > iOS Settings and uncheck 'Enable Hot Restart'. Then redeploy in Debug mode.",
      "Build and deploy in Release mode instead of Debug mode to bypass the Hot Restart pre-built container.",
      "Alternatively, use a Mac build host for iOS debugging (non-Hot Restart path)."
    ],
    "nextQuestions": [
      "Which version of SkiaSharp NuGet packages is the reporter using?",
      "Does the crash also occur when Hot Restart is disabled in VS settings?",
      "Does the crash occur with Release configuration?",
      "Is this reproducible with only SkiaSharp (not SkiaSharp.Views.Forms) referenced?"
    ],
    "resolution": {
      "hypothesis": "Xamarin Hot Restart's pre-built container uses dlopen to load native frameworks at debug time. SkiaSharp's native iOS framework may have link flags or dependencies that prevent successful dynamic loading in this restricted Hot Restart environment, causing a SIGSEGV.",
      "proposals": [
        {
          "title": "Disable Hot Restart as workaround",
          "description": "Disable Xamarin Hot Restart in Visual Studio IDE settings. This forces a full deployment without the pre-built container, and the SkiaSharp native framework loads via the normal iOS app bundle mechanism.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate SkiaSharp framework packaging for Hot Restart compatibility",
          "description": "Investigate whether SkiaSharp's iOS native framework bundle (XCFramework packaging, bitcode, or symbol visibility settings) can be adjusted to support Xamarin Hot Restart's dlopen loading path.",
          "category": "investigation",
          "confidence": 0.6,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Disable Hot Restart as workaround",
      "recommendedReason": "Immediate actionable workaround for the reporter. The root cause (Hot Restart + native framework dlopen incompatibility) requires deeper investigation to fix properly."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.78,
      "reason": "Crash pattern strongly suggests Xamarin Hot Restart incompatibility with SkiaSharp native framework, but SkiaSharp version is missing and we need to confirm whether disabling Hot Restart resolves the issue. A near-duplicate #1771 was filed by the same user.",
      "suggestedReproPlatform": "macos"
    },
    "missingInfo": [
      "SkiaSharp NuGet package version being used",
      "Whether the crash occurs when Xamarin Hot Restart is disabled in VS settings",
      "Whether the crash occurs in Release mode"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, native library, and iOS labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/iOS",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to near-duplicate #1771 filed by same user 4 days later with same crash",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 1771
      },
      {
        "type": "add-comment",
        "description": "Request SkiaSharp version and ask reporter to try disabling Hot Restart",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report. The stack trace (`Xamarin.PreBuilt.iOS.Applications:LoadFrameworks → dlopen`) points to **Xamarin Hot Restart** as the likely culprit. Hot Restart uses a pre-built app container to dynamically load native frameworks at debug time, and SkiaSharp's native iOS framework may be incompatible with this loading mechanism.\n\nCould you try the following?\n\n1. **Disable Hot Restart**: In Visual Studio, go to **Tools > Options > Xamarin > iOS Settings** and uncheck *Enable Hot Restart*. Then redeploy in Debug mode.\n2. **Try Release mode** to confirm the app works without Hot Restart.\n\nAlso, which version of the SkiaSharp NuGet packages are you using?\n\nNote: You also filed a similar report in #1771 — these appear to be the same issue."
      }
    ]
  }
}
```

</details>
