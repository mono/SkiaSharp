# Issue Triage Report — #1714

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T04:00:00Z |
| Type | type/bug (0.85 (85%)) |
| Area | area/libSkiaSharp.native (0.72 (72%)) |
| Suggested action | needs-info (0.88 (88%)) |

**Issue Summary:** SkiaSharp in a Xamarin.Forms iOS project crashes at launch past the splash screen when deployed from Visual Studio 16.10 on Windows with Hot Reload; the same project works when built from Visual Studio for Mac or VS 16.9.

**Analysis:** The crash is triggered by Visual Studio 16.10's new Xamarin Hot Reload (interpreter) mode on iOS, which uses Xamarin.PreBuilt.iOS to dynamically load native frameworks via dlopen at startup. Related issue #1761 has the actual native stack trace showing the crash occurs inside Xamarin.PreBuilt.iOS.Applications:LoadFrameworks → ObjCRuntime.Dlfcn:_dlopen — this is Xamarin iOS tooling trying to dynamically load the libSkiaSharp native framework in the new interpreter mode. The SkiaSharp version is identical between 'working' and 'broken' states (2.80.2), confirming the regression is in VS 16.10 tooling, not SkiaSharp itself. The reporter did not include error output from the VS output window.

**Recommendations:** **needs-info** — No error message or stack trace provided. The VS output window error is required to confirm if this is the same native dlopen crash seen in #1761 or a different issue.

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

1. Create new Xamarin Forms blank project
2. Add certificate configuration
3. Run on iOS device – works
4. Add SkiaSharp and SkiaSharp.Views.Forms NuGet packages
5. Run on iOS device from Visual Studio 16.10 on Windows with Hot Reload enabled
6. Observe: app builds and deploys, splash screen appears, then error in output window and crash

**Environment:** VS 2019 16.10.0 on Windows, Xamarin.iOS 14.20.0.1, SkiaSharp 2.80.2, iPhoneX/iPhone12/iPad v8

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1761 — Related issue #1761 with full native crash stack trace showing same crash pattern in Xamarin.PreBuilt.iOS.Applications:LoadFrameworks on iOS with VS 16.10

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | crash |
| Error message | — |
| Repro quality | partial |
| Target frameworks | xamarin.ios14.20.0.1 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | SkiaSharp 2.80.2 is ancient and Xamarin.Forms/Xamarin.iOS is deprecated in favor of .NET MAUI. VS 16.10 and its Xamarin Hot Reload behavior are no longer the current toolchain. |

## Analysis

### Technical Summary

The crash is triggered by Visual Studio 16.10's new Xamarin Hot Reload (interpreter) mode on iOS, which uses Xamarin.PreBuilt.iOS to dynamically load native frameworks via dlopen at startup. Related issue #1761 has the actual native stack trace showing the crash occurs inside Xamarin.PreBuilt.iOS.Applications:LoadFrameworks → ObjCRuntime.Dlfcn:_dlopen — this is Xamarin iOS tooling trying to dynamically load the libSkiaSharp native framework in the new interpreter mode. The SkiaSharp version is identical between 'working' and 'broken' states (2.80.2), confirming the regression is in VS 16.10 tooling, not SkiaSharp itself. The reporter did not include error output from the VS output window.

### Rationale

Classified as type/bug because the app crashes on a supported configuration. Area is libSkiaSharp.native because the related issue #1761 shows the crash is in native framework loading (dlopen). The issue is likely external (VS 16.10 Xamarin Hot Reload tooling bug) but no error log was provided to confirm. suggestedAction is needs-info because the reporter must share the error output from VS output window.

### Key Signals

- "Version with issue: 2.80.2 / Last known good version: 2.80.2" — **issue body** (The same SkiaSharp version works and doesn't work — the regression is in VS 16.10 tooling, not SkiaSharp.)
- "Visual Studio on Mac works, Visual Studio 16.9 worked, Visual Studio 16.10 does not work" — **issue body** (The failure is tied to VS 16.10 on Windows with its new Xamarin Hot Reload interpreter mode, not SkiaSharp.)
- "error in the output window and eventually the app crashes" — **issue body** (Reporter did not include the actual error message — critical missing information for diagnosis.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKCanvasView.cs` | — | context | iOS SKCanvasView has standard UIView lifecycle code; no unusual P/Invoke or dlopen calls that would specifically conflict with Hot Reload. The crash occurs before views are even created, during framework loading. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKCGSurfaceFactory.cs` | — | context | SKCGSurfaceFactory uses standard CoreGraphics APIs and SKSurface.Create — no native loading code that would be involved in startup crash. |

### Workarounds

- Build and deploy from Visual Studio for Mac instead of Windows VS 16.10
- Disable Xamarin Hot Reload in VS 16.10: Tools → Options → Xamarin → Hot Restart → uncheck Enable Hot Restart
- Try a newer SkiaSharp version (2.88.x) which may have updated iOS framework packaging

### Next Questions

- What is the full error message shown in the VS output window at the time of crash?
- Does the crash occur without Hot Reload enabled (disable in VS Tools → Options → Xamarin → iOS Settings)?
- Does disabling the SkiaSharp linker skip attributes help (add --linkskip SkiaSharp to the linker args)?
- Is this reproducible on the latest SkiaSharp and current .NET MAUI tooling?

### Resolution Proposals

**Hypothesis:** VS 16.10 introduced Xamarin Hot Reload for iOS which uses an interpreter/prebuilt runtime (Xamarin.PreBuilt.iOS) that attempts to dynamically load all native frameworks via dlopen at startup. The libSkiaSharp.framework or its packaging may be incompatible with this loading mechanism.

1. **Request error logs** — investigation, confidence 0.90 (90%), cost/xs, validated=untested
   - Ask reporter to share the full VS output window error at time of crash. Related issue #1761 has a full stack trace showing the crash is in Xamarin.PreBuilt.iOS.Applications:LoadFrameworks — confirm if #1714 is the same crash.
2. **Workaround: disable Hot Reload** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Disable Xamarin Hot Reload in VS options to revert to classic deploy mode, which works on VS 16.9.

**Recommended proposal:** Request error logs

**Why:** Without the actual error message the root cause cannot be confirmed. The reporter likely has the same native crash as #1761.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.88 (88%) |
| Reason | No error message or stack trace provided. The VS output window error is required to confirm if this is the same native dlopen crash seen in #1761 or a different issue. |
| Suggested repro platform | macos |

### Missing Info

- Full error message from VS output window at time of crash
- Whether disabling Xamarin Hot Reload resolves the issue
- Whether the crash also occurs without adding the SKCanvasView to any page (just adding the NuGet)

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply type/bug, area/libSkiaSharp.native, os/iOS, tenet/reliability labels | labels=type/bug, area/libSkiaSharp.native, os/iOS, tenet/reliability |
| link-related | low | 0.82 (82%) | Link related issue #1761 which has the same crash pattern with full stack trace | linkedIssue=#1761 |
| add-comment | medium | 0.88 (88%) | Ask for VS output window error and whether disabling Hot Reload helps | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed environment info!

To help diagnose this, could you please:

1. **Share the full error message** from the Visual Studio output window at the time of the crash. The error text is critical for identifying the root cause.

2. **Try disabling Xamarin Hot Reload**: Go to Tools → Options → Xamarin → iOS Settings, and disable Hot Reload/Hot Restart. Does the crash go away?

3. **Check #1761** which describes a very similar crash pattern on the same VS 16.10 + Xamarin.iOS setup — does your crash output look similar to the native stacktrace there?

Workaround in the meantime: deploy from Visual Studio for Mac, which you noted works correctly.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1714,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T04:00:00Z"
  },
  "summary": "SkiaSharp in a Xamarin.Forms iOS project crashes at launch past the splash screen when deployed from Visual Studio 16.10 on Windows with Hot Reload; the same project works when built from Visual Studio for Mac or VS 16.9.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.85
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.72
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
      "reproQuality": "partial",
      "targetFrameworks": [
        "xamarin.ios14.20.0.1"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create new Xamarin Forms blank project",
        "Add certificate configuration",
        "Run on iOS device – works",
        "Add SkiaSharp and SkiaSharp.Views.Forms NuGet packages",
        "Run on iOS device from Visual Studio 16.10 on Windows with Hot Reload enabled",
        "Observe: app builds and deploys, splash screen appears, then error in output window and crash"
      ],
      "environmentDetails": "VS 2019 16.10.0 on Windows, Xamarin.iOS 14.20.0.1, SkiaSharp 2.80.2, iPhoneX/iPhone12/iPad v8",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1761",
          "description": "Related issue #1761 with full native crash stack trace showing same crash pattern in Xamarin.PreBuilt.iOS.Applications:LoadFrameworks on iOS with VS 16.10"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "SkiaSharp 2.80.2 is ancient and Xamarin.Forms/Xamarin.iOS is deprecated in favor of .NET MAUI. VS 16.10 and its Xamarin Hot Reload behavior are no longer the current toolchain."
    }
  },
  "analysis": {
    "summary": "The crash is triggered by Visual Studio 16.10's new Xamarin Hot Reload (interpreter) mode on iOS, which uses Xamarin.PreBuilt.iOS to dynamically load native frameworks via dlopen at startup. Related issue #1761 has the actual native stack trace showing the crash occurs inside Xamarin.PreBuilt.iOS.Applications:LoadFrameworks → ObjCRuntime.Dlfcn:_dlopen — this is Xamarin iOS tooling trying to dynamically load the libSkiaSharp native framework in the new interpreter mode. The SkiaSharp version is identical between 'working' and 'broken' states (2.80.2), confirming the regression is in VS 16.10 tooling, not SkiaSharp itself. The reporter did not include error output from the VS output window.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKCanvasView.cs",
        "finding": "iOS SKCanvasView has standard UIView lifecycle code; no unusual P/Invoke or dlopen calls that would specifically conflict with Hot Reload. The crash occurs before views are even created, during framework loading.",
        "relevance": "context"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKCGSurfaceFactory.cs",
        "finding": "SKCGSurfaceFactory uses standard CoreGraphics APIs and SKSurface.Create — no native loading code that would be involved in startup crash.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Version with issue: 2.80.2 / Last known good version: 2.80.2",
        "source": "issue body",
        "interpretation": "The same SkiaSharp version works and doesn't work — the regression is in VS 16.10 tooling, not SkiaSharp."
      },
      {
        "text": "Visual Studio on Mac works, Visual Studio 16.9 worked, Visual Studio 16.10 does not work",
        "source": "issue body",
        "interpretation": "The failure is tied to VS 16.10 on Windows with its new Xamarin Hot Reload interpreter mode, not SkiaSharp."
      },
      {
        "text": "error in the output window and eventually the app crashes",
        "source": "issue body",
        "interpretation": "Reporter did not include the actual error message — critical missing information for diagnosis."
      }
    ],
    "rationale": "Classified as type/bug because the app crashes on a supported configuration. Area is libSkiaSharp.native because the related issue #1761 shows the crash is in native framework loading (dlopen). The issue is likely external (VS 16.10 Xamarin Hot Reload tooling bug) but no error log was provided to confirm. suggestedAction is needs-info because the reporter must share the error output from VS output window.",
    "nextQuestions": [
      "What is the full error message shown in the VS output window at the time of crash?",
      "Does the crash occur without Hot Reload enabled (disable in VS Tools → Options → Xamarin → iOS Settings)?",
      "Does disabling the SkiaSharp linker skip attributes help (add --linkskip SkiaSharp to the linker args)?",
      "Is this reproducible on the latest SkiaSharp and current .NET MAUI tooling?"
    ],
    "workarounds": [
      "Build and deploy from Visual Studio for Mac instead of Windows VS 16.10",
      "Disable Xamarin Hot Reload in VS 16.10: Tools → Options → Xamarin → Hot Restart → uncheck Enable Hot Restart",
      "Try a newer SkiaSharp version (2.88.x) which may have updated iOS framework packaging"
    ],
    "resolution": {
      "hypothesis": "VS 16.10 introduced Xamarin Hot Reload for iOS which uses an interpreter/prebuilt runtime (Xamarin.PreBuilt.iOS) that attempts to dynamically load all native frameworks via dlopen at startup. The libSkiaSharp.framework or its packaging may be incompatible with this loading mechanism.",
      "proposals": [
        {
          "title": "Request error logs",
          "description": "Ask reporter to share the full VS output window error at time of crash. Related issue #1761 has a full stack trace showing the crash is in Xamarin.PreBuilt.iOS.Applications:LoadFrameworks — confirm if #1714 is the same crash.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Workaround: disable Hot Reload",
          "description": "Disable Xamarin Hot Reload in VS options to revert to classic deploy mode, which works on VS 16.9.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Request error logs",
      "recommendedReason": "Without the actual error message the root cause cannot be confirmed. The reporter likely has the same native crash as #1761."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.88,
      "reason": "No error message or stack trace provided. The VS output window error is required to confirm if this is the same native dlopen crash seen in #1761 or a different issue.",
      "suggestedReproPlatform": "macos"
    },
    "missingInfo": [
      "Full error message from VS output window at time of crash",
      "Whether disabling Xamarin Hot Reload resolves the issue",
      "Whether the crash also occurs without adding the SKCanvasView to any page (just adding the NuGet)"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/libSkiaSharp.native, os/iOS, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/iOS",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Link related issue #1761 which has the same crash pattern with full stack trace",
        "risk": "low",
        "confidence": 0.82,
        "linkedIssue": 1761
      },
      {
        "type": "add-comment",
        "description": "Ask for VS output window error and whether disabling Hot Reload helps",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed environment info!\n\nTo help diagnose this, could you please:\n\n1. **Share the full error message** from the Visual Studio output window at the time of the crash. The error text is critical for identifying the root cause.\n\n2. **Try disabling Xamarin Hot Reload**: Go to Tools → Options → Xamarin → iOS Settings, and disable Hot Reload/Hot Restart. Does the crash go away?\n\n3. **Check #1761** which describes a very similar crash pattern on the same VS 16.10 + Xamarin.iOS setup — does your crash output look similar to the native stacktrace there?\n\nWorkaround in the meantime: deploy from Visual Studio for Mac, which you noted works correctly."
      }
    ]
  }
}
```

</details>
