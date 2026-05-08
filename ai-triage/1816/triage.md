# Issue Triage Report — #1816

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T22:18:29Z |
| Type | type/question (0.80 (80%)) |
| Area | area/SkiaSharp.Views (0.85 (85%)) |
| Suggested action | close-as-not-a-bug (0.80 (80%)) |

**Issue Summary:** After upgrading to SkiaSharp 2.80.3, iOS Simulator builds fail with undefined MTKView symbol (MetalKit not linked) when using --registrar:static mtouch option.

**Analysis:** SkiaSharp 2.80.3 introduced SKMetalView, which inherits from MTKView (MetalKit framework). When building with --registrar:static, the Xamarin.iOS linker does not automatically include the MetalKit framework, causing a native link error. The fix is to explicitly reference MetalKit via mtouch arguments.

**Recommendations:** **close-as-not-a-bug** — This is a build configuration issue, not a SkiaSharp defect. The MetalKit framework must be explicitly linked when using --registrar:static. A confirmed workaround exists in the comments.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp.Views |
| Platforms | os/iOS |
| Backends | backend/Metal |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Upgrade SkiaSharp from 2.80.2 to 2.80.3
2. Use --registrar:static in iOS mtouch options (required for binding libraries)
3. Attempt to build for iOS Simulator

**Environment:** Visual Studio for Mac, iOS Simulator, SkiaSharp 2.80.3, --registrar:static mtouch option

**Repository links:**
- https://github.com/xamarin/xamarin-macios/issues/4422 — Older iOS Simulator didn't support Metal — upstream Xamarin.iOS issue
- https://github.com/xamarin/xamarin-macios/pull/4510 — Xamarin.Macios linker fix for Metal on Simulator
- https://github.com/mono/SkiaSharp/commit/2a87a0d947e69800cdfc8dc31ab38a6a916bf7c1 — Commit that introduced Metal (SKMetalView/MTKView) support in 2.80.3

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2, 2.80.3 |
| Worked in | 2.80.2 |
| Broke in | 2.80.3 |
| Current relevance | unlikely |
| Relevance reason | SkiaSharp 2.80.x is old; modern versions use net8.0-ios TFMs. However, the MetalKit linkage requirement still applies to any app using SKMetalView with --registrar:static. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.85 (85%) |
| Reason | Builds worked with 2.80.2 and broke after upgrade to 2.80.3 due to Metal API (SKMetalView/MTKView) being introduced |
| Worked in version | 2.80.2 |
| Broke in version | 2.80.3 |

## Analysis

### Technical Summary

SkiaSharp 2.80.3 introduced SKMetalView, which inherits from MTKView (MetalKit framework). When building with --registrar:static, the Xamarin.iOS linker does not automatically include the MetalKit framework, causing a native link error. The fix is to explicitly reference MetalKit via mtouch arguments.

### Rationale

The issue is classified as type/question because the reporter is asking how to resolve a build configuration problem, and a clear workaround exists (confirmed in comments). The root cause — MetalKit not being auto-linked with static registrar — is external to SkiaSharp itself (it's Xamarin.iOS linker behavior). The behavior is correct: SkiaSharp now requires MetalKit, and the user must configure their project accordingly.

### Key Signals

- "linker command failed with exit code 1...native linking failed, undefined objective-c class: MTKView. The symbol '_OBJC_CLASS_$_MTKView' could not be found" — **issue body** (MetalKit framework is not being linked — MTKView is defined in MetalKit.framework)
- "I set the mtouch option "--registrar:static" for iOS Simulator. This setting is needed for the binding library used in my project." — **issue body** (Static registrar is required by the user's binding library, preventing them from removing it)
- "Starting in 2.80.3 this package brought in support for the Metal API: commit 2a87a0d947e69800cdfc8dc31ab38a6a916bf7c1" — **comment by justinimel** (The regression is directly tied to the addition of SKMetalView which depends on MetalKit)
- "I found this could be fixed by adding `-gcc_flags '-framework MetalKit'` to the Additional MTouch Arguments field in the simulator build configuration" — **comment by grahampett** (Confirmed workaround — explicitly linking MetalKit framework resolves the linker error)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKMetalView.cs` | 1-20 | direct | SKMetalView inherits from MTKView and imports 'using MetalKit'. This creates a hard dependency on the MetalKit.framework that must be linked by the iOS toolchain. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKMetalView.cs` | 35-45 | related | DepthStencilModePrivate property checks ObjCRuntime.Runtime.Arch == SIMULATOR, showing the code is simulator-aware but still requires MetalKit to be linked |

### Workarounds

- Add `-gcc_flags '-framework MetalKit'` to Additional MTouch Arguments in the iOS/Simulator build configuration in Visual Studio
- Downgrade to SkiaSharp 2.80.2 (does not include Metal support, avoids the issue but loses Metal features)
- Set Linker Behavior to 'Don't Link' in iOS Build options (may work but increases app size significantly)

### Next Questions

- Does the project actually use SKMetalView, or is it only using non-Metal views (SKCanvasView, SKGLView)?
- What Xamarin.iOS version is being used? Newer versions may handle MetalKit linking automatically.

### Resolution Proposals

**Hypothesis:** The user needs to explicitly link MetalKit.framework because the static registrar does not auto-include it when SkiaSharp 2.80.3+ is referenced.

1. **Add MetalKit framework via mtouch arguments** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - In Visual Studio Mac, go to iOS Build settings for the Simulator configuration, and add `-gcc_flags '-framework MetalKit'` to Additional MTouch Arguments.
2. **Downgrade to SkiaSharp 2.80.2** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Revert the SkiaSharp version to 2.80.2 which did not include Metal support and therefore does not require MetalKit linkage.

**Recommended proposal:** Add MetalKit framework via mtouch arguments

**Why:** Allows the user to stay on the latest SkiaSharp version while resolving the linker issue with a simple configuration change.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.80 (80%) |
| Reason | This is a build configuration issue, not a SkiaSharp defect. The MetalKit framework must be explicitly linked when using --registrar:static. A confirmed workaround exists in the comments. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply question, Views, iOS, Metal labels | labels=type/question, area/SkiaSharp.Views, os/iOS, backend/Metal, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Post answer explaining the MetalKit workaround | — |
| close-issue | medium | 0.75 (75%) | Close as answered — workaround documented in comments and confirmed | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! This is a known configuration issue when using `--registrar:static` with SkiaSharp 2.80.3+, which introduced `SKMetalView` (which depends on `MetalKit.framework`).

The workaround (confirmed by @grahampett above) is to explicitly link the MetalKit framework in your iOS/Simulator build configuration:

In Visual Studio for Mac, open your iOS project's **Build Options → Additional mtouch arguments** (for the Simulator configuration) and add:
```
-gcc_flags '-framework MetalKit'
```

This tells the native linker to include MetalKit, which resolves the `_OBJC_CLASS_$_MTKView` undefined symbol error.

**Why this happens:** SkiaSharp 2.80.3 added `SKMetalView` (which wraps `MTKView` from MetalKit). When the static registrar is used, the Xamarin.iOS linker requires this framework to be referenced explicitly. The dynamic registrar handles this automatically, but the static registrar does not.

If you're not using `SKMetalView` in your project at all and only use `SKCanvasView` or `SKGLView`, you may also consider whether the Metal views are being pulled in transitively and whether you can strip them — but the `-framework MetalKit` flag is the simplest solution.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1816,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T22:18:29Z"
  },
  "summary": "After upgrading to SkiaSharp 2.80.3, iOS Simulator builds fail with undefined MTKView symbol (MetalKit not linked) when using --registrar:static mtouch option.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.85
    },
    "platforms": [
      "os/iOS"
    ],
    "backends": [
      "backend/Metal"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Upgrade SkiaSharp from 2.80.2 to 2.80.3",
        "Use --registrar:static in iOS mtouch options (required for binding libraries)",
        "Attempt to build for iOS Simulator"
      ],
      "environmentDetails": "Visual Studio for Mac, iOS Simulator, SkiaSharp 2.80.3, --registrar:static mtouch option",
      "repoLinks": [
        {
          "url": "https://github.com/xamarin/xamarin-macios/issues/4422",
          "description": "Older iOS Simulator didn't support Metal — upstream Xamarin.iOS issue"
        },
        {
          "url": "https://github.com/xamarin/xamarin-macios/pull/4510",
          "description": "Xamarin.Macios linker fix for Metal on Simulator"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/commit/2a87a0d947e69800cdfc8dc31ab38a6a916bf7c1",
          "description": "Commit that introduced Metal (SKMetalView/MTKView) support in 2.80.3"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2",
        "2.80.3"
      ],
      "workedIn": "2.80.2",
      "brokeIn": "2.80.3",
      "currentRelevance": "unlikely",
      "relevanceReason": "SkiaSharp 2.80.x is old; modern versions use net8.0-ios TFMs. However, the MetalKit linkage requirement still applies to any app using SKMetalView with --registrar:static."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.85,
      "reason": "Builds worked with 2.80.2 and broke after upgrade to 2.80.3 due to Metal API (SKMetalView/MTKView) being introduced",
      "workedInVersion": "2.80.2",
      "brokeInVersion": "2.80.3"
    }
  },
  "analysis": {
    "summary": "SkiaSharp 2.80.3 introduced SKMetalView, which inherits from MTKView (MetalKit framework). When building with --registrar:static, the Xamarin.iOS linker does not automatically include the MetalKit framework, causing a native link error. The fix is to explicitly reference MetalKit via mtouch arguments.",
    "rationale": "The issue is classified as type/question because the reporter is asking how to resolve a build configuration problem, and a clear workaround exists (confirmed in comments). The root cause — MetalKit not being auto-linked with static registrar — is external to SkiaSharp itself (it's Xamarin.iOS linker behavior). The behavior is correct: SkiaSharp now requires MetalKit, and the user must configure their project accordingly.",
    "keySignals": [
      {
        "text": "linker command failed with exit code 1...native linking failed, undefined objective-c class: MTKView. The symbol '_OBJC_CLASS_$_MTKView' could not be found",
        "source": "issue body",
        "interpretation": "MetalKit framework is not being linked — MTKView is defined in MetalKit.framework"
      },
      {
        "text": "I set the mtouch option \"--registrar:static\" for iOS Simulator. This setting is needed for the binding library used in my project.",
        "source": "issue body",
        "interpretation": "Static registrar is required by the user's binding library, preventing them from removing it"
      },
      {
        "text": "Starting in 2.80.3 this package brought in support for the Metal API: commit 2a87a0d947e69800cdfc8dc31ab38a6a916bf7c1",
        "source": "comment by justinimel",
        "interpretation": "The regression is directly tied to the addition of SKMetalView which depends on MetalKit"
      },
      {
        "text": "I found this could be fixed by adding `-gcc_flags '-framework MetalKit'` to the Additional MTouch Arguments field in the simulator build configuration",
        "source": "comment by grahampett",
        "interpretation": "Confirmed workaround — explicitly linking MetalKit framework resolves the linker error"
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKMetalView.cs",
        "lines": "1-20",
        "finding": "SKMetalView inherits from MTKView and imports 'using MetalKit'. This creates a hard dependency on the MetalKit.framework that must be linked by the iOS toolchain.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKMetalView.cs",
        "lines": "35-45",
        "finding": "DepthStencilModePrivate property checks ObjCRuntime.Runtime.Arch == SIMULATOR, showing the code is simulator-aware but still requires MetalKit to be linked",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Add `-gcc_flags '-framework MetalKit'` to Additional MTouch Arguments in the iOS/Simulator build configuration in Visual Studio",
      "Downgrade to SkiaSharp 2.80.2 (does not include Metal support, avoids the issue but loses Metal features)",
      "Set Linker Behavior to 'Don't Link' in iOS Build options (may work but increases app size significantly)"
    ],
    "nextQuestions": [
      "Does the project actually use SKMetalView, or is it only using non-Metal views (SKCanvasView, SKGLView)?",
      "What Xamarin.iOS version is being used? Newer versions may handle MetalKit linking automatically."
    ],
    "resolution": {
      "hypothesis": "The user needs to explicitly link MetalKit.framework because the static registrar does not auto-include it when SkiaSharp 2.80.3+ is referenced.",
      "proposals": [
        {
          "title": "Add MetalKit framework via mtouch arguments",
          "description": "In Visual Studio Mac, go to iOS Build settings for the Simulator configuration, and add `-gcc_flags '-framework MetalKit'` to Additional MTouch Arguments.",
          "category": "workaround",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Downgrade to SkiaSharp 2.80.2",
          "description": "Revert the SkiaSharp version to 2.80.2 which did not include Metal support and therefore does not require MetalKit linkage.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Add MetalKit framework via mtouch arguments",
      "recommendedReason": "Allows the user to stay on the latest SkiaSharp version while resolving the linker issue with a simple configuration change."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.8,
      "reason": "This is a build configuration issue, not a SkiaSharp defect. The MetalKit framework must be explicitly linked when using --registrar:static. A confirmed workaround exists in the comments.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, Views, iOS, Metal labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/question",
          "area/SkiaSharp.Views",
          "os/iOS",
          "backend/Metal",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer explaining the MetalKit workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report! This is a known configuration issue when using `--registrar:static` with SkiaSharp 2.80.3+, which introduced `SKMetalView` (which depends on `MetalKit.framework`).\n\nThe workaround (confirmed by @grahampett above) is to explicitly link the MetalKit framework in your iOS/Simulator build configuration:\n\nIn Visual Studio for Mac, open your iOS project's **Build Options → Additional mtouch arguments** (for the Simulator configuration) and add:\n```\n-gcc_flags '-framework MetalKit'\n```\n\nThis tells the native linker to include MetalKit, which resolves the `_OBJC_CLASS_$_MTKView` undefined symbol error.\n\n**Why this happens:** SkiaSharp 2.80.3 added `SKMetalView` (which wraps `MTKView` from MetalKit). When the static registrar is used, the Xamarin.iOS linker requires this framework to be referenced explicitly. The dynamic registrar handles this automatically, but the static registrar does not.\n\nIf you're not using `SKMetalView` in your project at all and only use `SKCanvasView` or `SKGLView`, you may also consider whether the Metal views are being pulled in transitively and whether you can strip them — but the `-framework MetalKit` flag is the simplest solution."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — workaround documented in comments and confirmed",
        "risk": "medium",
        "confidence": 0.75,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
