# Issue Triage Report — #2077

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T10:08:27Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | needs-info (0.82 (82%)) |

**Issue Summary:** After upgrading SkiaSharp.Views from 2.80.4 to 2.88.0 on iOS, a TypeLoadException is thrown at startup because the Xamarin.iOS assembly referenced during compilation of 2.88.0 contains ObjCRuntime.DisposableObject which is missing from the older Xamarin.iOS SDK the user has installed.

**Analysis:** SkiaSharp 2.88.0 was compiled against a Xamarin.iOS SDK version (15.x) that introduced ObjCRuntime.DisposableObject as a base class in the NSObject hierarchy. When a user with an older Xamarin.iOS SDK (pre-15.2) installs SkiaSharp 2.88.0, the runtime cannot resolve the ObjCRuntime.DisposableObject typeref token embedded in the SkiaSharp.Views.iOS assembly, causing a TypeLoadException on app startup.

**Recommendations:** **needs-info** — The exception and regression are clear, but the reporter has not provided their Xamarin.iOS SDK version. Confirming the SDK version will validate the DisposableObject hypothesis and allow a definitive fix recommendation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Open the SkiaSharpNativeDemo iOS project at https://github.com/mattleibow/SkiaSharpDemo/tree/master/SkiaSharpNativeDemo/SkiaSharpNativeDemo.iOS
2. Upgrade SkiaSharp.Views NuGet package to 2.88.0
3. Run the app on iOS 14.1 (iPhone 12 Pro Max)

**Environment:** Visual Studio, iOS 14.1, iPhone 12 Pro Max, SkiaSharp.Views 2.88.0 (broke), 2.80.4 (worked)

**Repository links:**
- https://github.com/mattleibow/SkiaSharpDemo/tree/master/SkiaSharpNativeDemo/SkiaSharpNativeDemo.iOS — Reproduction project provided by reporter

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | exception |
| Error message | System.TypeLoadException: Could not resolve type with token 01000074 from typeref (expected class 'ObjCRuntime.DisposableObject' in assembly 'Xamarin.iOS, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065') |
| Repro quality | partial |
| Target frameworks | xamarin.ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.0, 2.88.1, 2.80.4, 2.80.3, 2.80.2 |
| Worked in | 2.80.4 |
| Broke in | 2.88.0 |
| Current relevance | likely |
| Relevance reason | Three independent users confirm the same failure across 2.88.0 and 2.88.1 starting from different 2.80.x versions. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.92 (92%) |
| Reason | Reporter explicitly states 2.80.4 worked and 2.88.0 does not. Two additional commenters confirm the same regression pattern from their own 2.80.x versions. |
| Worked in version | 2.80.4 |
| Broke in version | 2.88.0 |

## Analysis

### Technical Summary

SkiaSharp 2.88.0 was compiled against a Xamarin.iOS SDK version (15.x) that introduced ObjCRuntime.DisposableObject as a base class in the NSObject hierarchy. When a user with an older Xamarin.iOS SDK (pre-15.2) installs SkiaSharp 2.88.0, the runtime cannot resolve the ObjCRuntime.DisposableObject typeref token embedded in the SkiaSharp.Views.iOS assembly, causing a TypeLoadException on app startup.

### Rationale

The TypeLoadException references a missing type in the Xamarin.iOS assembly. The SkiaSharp.Views.iOS library inherits UIView/CALayer/MTKView which chain up to NSObject, and in Xamarin.iOS 15.2+ NSObject inherits from ObjCRuntime.DisposableObject. SkiaSharp 2.88.0 was built against this newer SDK so its typeref table includes DisposableObject. Users on older SDK versions (before ~15.2) lack this class, causing a binary incompatibility at runtime. The breaking changelog for 2.88.0 shows an assembly version bump but does not document new minimum SDK requirements.

### Key Signals

- "System.TypeLoadException: Could not resolve type with token 01000074 from typeref (expected class 'ObjCRuntime.DisposableObject' in assembly 'Xamarin.iOS')" — **issue body** (SkiaSharp.Views.iOS.dll contains a typeref to ObjCRuntime.DisposableObject which is absent in the user's installed Xamarin.iOS SDK — a binary ABI incompatibility.)
- "After upgrade SkiaSharp.View from 2.80.4 to 2.88.0, when running program, throws exception" — **issue body** (Clear regression — same code and Xamarin.iOS SDK version, only the SkiaSharp NuGet was upgraded.)
- "Same problem on my side! / I am getting exactly the same error, updating from V2.80.3 to 2.88.0 / Version 2.88.1 but the error still exists." — **comments #1, #2, #3** (At least three independent users hit the same failure across 2.88.0 and 2.88.1, confirming it is a systematic incompatibility introduced in 2.88.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKCanvasView.cs` | 15 | direct | SKCanvasView inherits UIView which ultimately inherits NSObject. In Xamarin.iOS 15.2+, NSObject inherits from ObjCRuntime.DisposableObject. The compiled SkiaSharp 2.88.0 DLL was linked against this newer SDK, embedding a typeref to DisposableObject. Users on older SDK versions do not have this type. |
| `changelogs/SkiaSharp.Views/2.88.0/SkiaSharp.Views.iOS.breaking.md` | — | related | The 2.88.0 iOS breaking changes show assembly version bump (2.88.0.0 vs 2.80.0.0) and removed obsolete methods, but do not document any new minimum Xamarin.iOS SDK version requirement. The bump in build target SDK silently raised the binary compatibility floor. |

### Workarounds

- Update Xamarin.iOS SDK to version 15.2 or later (which introduces ObjCRuntime.DisposableObject). This is a free update via Visual Studio for Mac updater.
- Pin SkiaSharp.Views (and related packages) to 2.80.x until the Xamarin.iOS SDK can be updated.

### Next Questions

- What exact Xamarin.iOS SDK version is the reporter using? (Help confirm the 15.2 boundary hypothesis.)
- Is the same failure reproducible with SkiaSharp.Views.Forms on iOS, or only with SkiaSharp.Views?
- Has SkiaSharp 2.88.x ever been retargeted to restore compatibility with older Xamarin.iOS SDK versions in a patch release?

### Resolution Proposals

**Hypothesis:** SkiaSharp 2.88.0 was compiled against Xamarin.iOS 15.2+ which introduced ObjCRuntime.DisposableObject in the type hierarchy. Users on older Xamarin.iOS SDK versions encounter a runtime TypeLoadException because the embedded typeref cannot be resolved.

1. **Update Xamarin.iOS SDK to 15.2+** — workaround, confidence 0.85 (85%), cost/xs, validated=untested
   - Instruct the reporter to update their Xamarin.iOS SDK via the Visual Studio for Mac updater to version 15.2 or later, which introduces ObjCRuntime.DisposableObject.
2. **Pin to SkiaSharp.Views 2.80.x** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - As a temporary workaround, pin SkiaSharp.Views and related packages to the latest 2.80.x release until the SDK can be updated.
3. **Document minimum Xamarin.iOS SDK requirement for 2.88.x** — fix, confidence 0.80 (80%), cost/xs, validated=untested
   - Add a note to the 2.88.0 release notes and README stating that SkiaSharp 2.88.x requires Xamarin.iOS SDK 15.2 or later, to prevent future confusion.

**Recommended proposal:** Update Xamarin.iOS SDK to 15.2+

**Why:** The simplest fix with the least disruption: updating the SDK resolves the binary incompatibility immediately without requiring any code changes.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.82 (82%) |
| Reason | The exception and regression are clear, but the reporter has not provided their Xamarin.iOS SDK version. Confirming the SDK version will validate the DisposableObject hypothesis and allow a definitive fix recommendation. |
| Suggested repro platform | macos |

### Missing Info

- Xamarin.iOS SDK version installed (visible in Visual Studio for Mac → About → Show Details or in .csproj TargetFramework)
- Whether upgrading the Xamarin.iOS SDK to 15.2+ resolves the issue

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/bug, area/SkiaSharp.Views, os/iOS, tenet/compatibility labels | labels=type/bug, area/SkiaSharp.Views, os/iOS, tenet/compatibility |
| add-comment | medium | 0.82 (82%) | Ask for Xamarin.iOS SDK version and confirm workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and repro link!

This looks like a binary compatibility issue between SkiaSharp 2.88.0 and your installed Xamarin.iOS SDK version. SkiaSharp 2.88.0 was compiled against Xamarin.iOS 15.2+, which introduced `ObjCRuntime.DisposableObject` as part of the `NSObject` hierarchy. If your SDK is older than 15.2, the runtime cannot resolve this type and throws the `TypeLoadException` you're seeing.

**To confirm:** Could you check your installed Xamarin.iOS SDK version? (In Visual Studio for Mac: Help → About Visual Studio → Show Details, look for Xamarin.iOS.)

**Likely workaround:** Update your Xamarin.iOS SDK to version 15.2 or later via the Visual Studio for Mac updater. If you cannot update immediately, pinning `SkiaSharp.Views` (and related packages) back to `2.80.x` should restore functionality.

If updating the SDK resolves it, please let us know — it will help us document the minimum SDK requirement for 2.88.x.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2077,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T10:08:27Z"
  },
  "summary": "After upgrading SkiaSharp.Views from 2.80.4 to 2.88.0 on iOS, a TypeLoadException is thrown at startup because the Xamarin.iOS assembly referenced during compilation of 2.88.0 contains ObjCRuntime.DisposableObject which is missing from the older Xamarin.iOS SDK the user has installed.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/iOS"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "System.TypeLoadException: Could not resolve type with token 01000074 from typeref (expected class 'ObjCRuntime.DisposableObject' in assembly 'Xamarin.iOS, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065')",
      "reproQuality": "partial",
      "targetFrameworks": [
        "xamarin.ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Open the SkiaSharpNativeDemo iOS project at https://github.com/mattleibow/SkiaSharpDemo/tree/master/SkiaSharpNativeDemo/SkiaSharpNativeDemo.iOS",
        "Upgrade SkiaSharp.Views NuGet package to 2.88.0",
        "Run the app on iOS 14.1 (iPhone 12 Pro Max)"
      ],
      "environmentDetails": "Visual Studio, iOS 14.1, iPhone 12 Pro Max, SkiaSharp.Views 2.88.0 (broke), 2.80.4 (worked)",
      "repoLinks": [
        {
          "url": "https://github.com/mattleibow/SkiaSharpDemo/tree/master/SkiaSharpNativeDemo/SkiaSharpNativeDemo.iOS",
          "description": "Reproduction project provided by reporter"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.0",
        "2.88.1",
        "2.80.4",
        "2.80.3",
        "2.80.2"
      ],
      "workedIn": "2.80.4",
      "brokeIn": "2.88.0",
      "currentRelevance": "likely",
      "relevanceReason": "Three independent users confirm the same failure across 2.88.0 and 2.88.1 starting from different 2.80.x versions."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.92,
      "reason": "Reporter explicitly states 2.80.4 worked and 2.88.0 does not. Two additional commenters confirm the same regression pattern from their own 2.80.x versions.",
      "workedInVersion": "2.80.4",
      "brokeInVersion": "2.88.0"
    }
  },
  "analysis": {
    "summary": "SkiaSharp 2.88.0 was compiled against a Xamarin.iOS SDK version (15.x) that introduced ObjCRuntime.DisposableObject as a base class in the NSObject hierarchy. When a user with an older Xamarin.iOS SDK (pre-15.2) installs SkiaSharp 2.88.0, the runtime cannot resolve the ObjCRuntime.DisposableObject typeref token embedded in the SkiaSharp.Views.iOS assembly, causing a TypeLoadException on app startup.",
    "rationale": "The TypeLoadException references a missing type in the Xamarin.iOS assembly. The SkiaSharp.Views.iOS library inherits UIView/CALayer/MTKView which chain up to NSObject, and in Xamarin.iOS 15.2+ NSObject inherits from ObjCRuntime.DisposableObject. SkiaSharp 2.88.0 was built against this newer SDK so its typeref table includes DisposableObject. Users on older SDK versions (before ~15.2) lack this class, causing a binary incompatibility at runtime. The breaking changelog for 2.88.0 shows an assembly version bump but does not document new minimum SDK requirements.",
    "keySignals": [
      {
        "text": "System.TypeLoadException: Could not resolve type with token 01000074 from typeref (expected class 'ObjCRuntime.DisposableObject' in assembly 'Xamarin.iOS')",
        "source": "issue body",
        "interpretation": "SkiaSharp.Views.iOS.dll contains a typeref to ObjCRuntime.DisposableObject which is absent in the user's installed Xamarin.iOS SDK — a binary ABI incompatibility."
      },
      {
        "text": "After upgrade SkiaSharp.View from 2.80.4 to 2.88.0, when running program, throws exception",
        "source": "issue body",
        "interpretation": "Clear regression — same code and Xamarin.iOS SDK version, only the SkiaSharp NuGet was upgraded."
      },
      {
        "text": "Same problem on my side! / I am getting exactly the same error, updating from V2.80.3 to 2.88.0 / Version 2.88.1 but the error still exists.",
        "source": "comments #1, #2, #3",
        "interpretation": "At least three independent users hit the same failure across 2.88.0 and 2.88.1, confirming it is a systematic incompatibility introduced in 2.88."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/iOS/SKCanvasView.cs",
        "lines": "15",
        "finding": "SKCanvasView inherits UIView which ultimately inherits NSObject. In Xamarin.iOS 15.2+, NSObject inherits from ObjCRuntime.DisposableObject. The compiled SkiaSharp 2.88.0 DLL was linked against this newer SDK, embedding a typeref to DisposableObject. Users on older SDK versions do not have this type.",
        "relevance": "direct"
      },
      {
        "file": "changelogs/SkiaSharp.Views/2.88.0/SkiaSharp.Views.iOS.breaking.md",
        "finding": "The 2.88.0 iOS breaking changes show assembly version bump (2.88.0.0 vs 2.80.0.0) and removed obsolete methods, but do not document any new minimum Xamarin.iOS SDK version requirement. The bump in build target SDK silently raised the binary compatibility floor.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Update Xamarin.iOS SDK to version 15.2 or later (which introduces ObjCRuntime.DisposableObject). This is a free update via Visual Studio for Mac updater.",
      "Pin SkiaSharp.Views (and related packages) to 2.80.x until the Xamarin.iOS SDK can be updated."
    ],
    "nextQuestions": [
      "What exact Xamarin.iOS SDK version is the reporter using? (Help confirm the 15.2 boundary hypothesis.)",
      "Is the same failure reproducible with SkiaSharp.Views.Forms on iOS, or only with SkiaSharp.Views?",
      "Has SkiaSharp 2.88.x ever been retargeted to restore compatibility with older Xamarin.iOS SDK versions in a patch release?"
    ],
    "resolution": {
      "hypothesis": "SkiaSharp 2.88.0 was compiled against Xamarin.iOS 15.2+ which introduced ObjCRuntime.DisposableObject in the type hierarchy. Users on older Xamarin.iOS SDK versions encounter a runtime TypeLoadException because the embedded typeref cannot be resolved.",
      "proposals": [
        {
          "title": "Update Xamarin.iOS SDK to 15.2+",
          "description": "Instruct the reporter to update their Xamarin.iOS SDK via the Visual Studio for Mac updater to version 15.2 or later, which introduces ObjCRuntime.DisposableObject.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Pin to SkiaSharp.Views 2.80.x",
          "description": "As a temporary workaround, pin SkiaSharp.Views and related packages to the latest 2.80.x release until the SDK can be updated.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Document minimum Xamarin.iOS SDK requirement for 2.88.x",
          "description": "Add a note to the 2.88.0 release notes and README stating that SkiaSharp 2.88.x requires Xamarin.iOS SDK 15.2 or later, to prevent future confusion.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Update Xamarin.iOS SDK to 15.2+",
      "recommendedReason": "The simplest fix with the least disruption: updating the SDK resolves the binary incompatibility immediately without requiring any code changes."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.82,
      "reason": "The exception and regression are clear, but the reporter has not provided their Xamarin.iOS SDK version. Confirming the SDK version will validate the DisposableObject hypothesis and allow a definitive fix recommendation.",
      "suggestedReproPlatform": "macos"
    },
    "missingInfo": [
      "Xamarin.iOS SDK version installed (visible in Visual Studio for Mac → About → Show Details or in .csproj TargetFramework)",
      "Whether upgrading the Xamarin.iOS SDK to 15.2+ resolves the issue"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views, os/iOS, tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/iOS",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for Xamarin.iOS SDK version and confirm workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report and repro link!\n\nThis looks like a binary compatibility issue between SkiaSharp 2.88.0 and your installed Xamarin.iOS SDK version. SkiaSharp 2.88.0 was compiled against Xamarin.iOS 15.2+, which introduced `ObjCRuntime.DisposableObject` as part of the `NSObject` hierarchy. If your SDK is older than 15.2, the runtime cannot resolve this type and throws the `TypeLoadException` you're seeing.\n\n**To confirm:** Could you check your installed Xamarin.iOS SDK version? (In Visual Studio for Mac: Help → About Visual Studio → Show Details, look for Xamarin.iOS.)\n\n**Likely workaround:** Update your Xamarin.iOS SDK to version 15.2 or later via the Visual Studio for Mac updater. If you cannot update immediately, pinning `SkiaSharp.Views` (and related packages) back to `2.80.x` should restore functionality.\n\nIf updating the SDK resolves it, please let us know — it will help us document the minimum SDK requirement for 2.88.x."
      }
    ]
  }
}
```

</details>
