# Issue Triage Report — #4349

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-06T05:45:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** SKCanvasView crashes with FileNotFoundException on _Microsoft.Android.Resource.Designer when publishing a MAUI Android app with PublishAot=true (NativeAOT), because the Android resource designer shim uses Assembly.Load which is unsupported in NativeAOT.

**Analysis:** SKCanvasView.Initialize references Resource.Styleable.SKCanvasView (from attrs.xml) which triggers Android.Runtime.ResourceIdManager.UpdateIdValues() -> Assembly.Load('_Microsoft.Android.Resource.Designer') at static initialization time. In NativeAOT, Assembly.Load is unsupported, causing a crash. The NativeAOT compiler may eagerly initialize the Resource.Styleable type even when attrs is null, placing the failure at the start of Initialize (offset +0x18).

**Recommendations:** **needs-investigation** — Confirmed bug in SkiaSharp.Views.Android with clear stack trace and root cause. Fix needs investigation into whether attrs.xml removal is safe (backward compat) and whether the Resource.Styleable static init can be deferred or guarded in NativeAOT.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | partner/maui |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a MAUI app that uses SKCanvasView (directly or via LottieView)
2. Publish for Android with PublishAot=true: dotnet publish -f net10.0-android -r android-arm64 -c Release /p:PublishAot=true
3. Launch the app on Android device/emulator
4. Observe crash when SKCanvasView is instantiated

**Environment:** Android 16, net10.0-android, SkiaSharp 3.116.0, NativeAOT (PublishAot=true), arm64

**Repository links:**
- https://github.com/dotnet/android/issues/10852 — Related dotnet/android issue: Assembly.Load failure in NativeAOT for _Microsoft.Android.Resource.Designer

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | exception |
| Error message | System.IO.FileNotFoundException: IO_FileNotFound_FileName, _Microsoft.Android.Resource.Designer |
| Repro quality | partial |
| Target frameworks | net10.0-android |

**Stack trace:**

```text
at Internal.Runtime.TypeLoaderExceptionHelper.CreateFileNotFoundException(ExceptionStringID, String) + 0x50
at SkiaSharp.Views.Android.SKCanvasView.Initialize(IAttributeSet) + 0x18
at SkiaSharp.Views.Maui.Handlers.SKCanvasViewHandler.CreatePlatformView() + 0x38
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Resource.Styleable usage in SKCanvasView.Initialize has not changed; NativeAOT incompatibility persists in current codebase. |

## Analysis

### Technical Summary

SKCanvasView.Initialize references Resource.Styleable.SKCanvasView (from attrs.xml) which triggers Android.Runtime.ResourceIdManager.UpdateIdValues() -> Assembly.Load('_Microsoft.Android.Resource.Designer') at static initialization time. In NativeAOT, Assembly.Load is unsupported, causing a crash. The NativeAOT compiler may eagerly initialize the Resource.Styleable type even when attrs is null, placing the failure at the start of Initialize (offset +0x18).

### Rationale

The stack trace clearly points to SKCanvasView.Initialize at offset +0x18, and the exception is FileNotFoundException for _Microsoft.Android.Resource.Designer — this is the well-known .NET for Android resource designer shim pattern that fails under NativeAOT. The crash only manifests with PublishAot=true. The code path via SKCanvasViewHandler.CreatePlatformView calls new SKCanvasView(Context) (no attrs), but the Resource.Styleable type's static constructor is triggered either eagerly or by the JIT's preparation of Initialize, which calls Assembly.Load and fails.

### Key Signals

- "System.IO.FileNotFoundException: IO_FileNotFound_FileName, _Microsoft.Android.Resource.Designer" — **stack trace in issue body** (Assembly.Load('_Microsoft.Android.Resource.Designer') is called and fails because NativeAOT does not support runtime assembly loading.)
- "at SkiaSharp.Views.Android.SKCanvasView.Initialize(IAttributeSet) + 0x18" — **stack trace** (Crash occurs very early in Initialize (offset 24 bytes), consistent with eager static-type initialization in NativeAOT of the Resource.Styleable class referenced in that method.)
- "dotnet publish -f net10.0-android -r android-arm64 -c Release /p:PublishAot=true" — **build command in issue body** (Confirms NativeAOT publish mode, which disables Assembly.Load.)
- "Here is a similar issue but posted on .Net repo: https://github.com/dotnet/android/issues/10852" — **comment by reporter** (This is a cross-cutting issue between SkiaSharp and dotnet/android toolchain; fix may need to come from both sides.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs` | 48-60 | direct | Initialize(IAttributeSet) accesses Resource.Styleable.SKCanvasView and Resource.Styleable.SKCanvasView_ignorePixelScaling inside an if (attrs != null) guard. However, in NativeAOT the Resource.Styleable static constructor may run eagerly before the null check is evaluated, triggering Assembly.Load('_Microsoft.Android.Resource.Designer') which is unsupported in NativeAOT. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/Resources/values/attrs.xml` | 1-6 | direct | Declares a custom styleable SKCanvasView with an ignorePixelScaling attribute. This attrs.xml causes the .NET for Android build to generate a Resource.designer.cs with a static initializer that calls ResourceIdManager.UpdateIdValues(), which uses Assembly.Load. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Android.cs` | 14 | related | CreatePlatformView() calls new SKCanvasView(Context) (no IAttributeSet). With attrs=null the attrs processing block is guarded, but the Resource.Styleable type initialization still occurs in NativeAOT. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKSurfaceView.cs` | 12-29 | context | SKSurfaceView does not use Resource.Styleable and its Initialize() takes no IAttributeSet — it is not affected by this NativeAOT incompatibility. |

### Workarounds

- Build without NativeAOT (remove PublishAot=true) — not suitable for release size optimization goals.
- Set SKCanvasView.IgnorePixelScaling programmatically after view creation rather than via XML attributes — avoids the attrs processing entirely but does not prevent the static init crash.
- Wait for dotnet/android to fix the Assembly.Load issue in NativeAOT (tracked at https://github.com/dotnet/android/issues/10852).

### Next Questions

- Does the crash also occur with SKGLView or other SkiaSharp Android views?
- Is this crash reproducible on net8.0-android or net9.0-android with NativeAOT, or only net10.0-android?
- Can the attrs.xml / Resource.Styleable usage be removed from SKCanvasView without breaking existing apps (the ignorePixelScaling attribute can still be set programmatically)?

### Resolution Proposals

**Hypothesis:** SkiaSharp.Views.Android declares an attrs.xml causing the .NET for Android build to generate a Resource.designer.cs whose static constructor calls Assembly.Load. In NativeAOT this fails. The fix is to remove or guard the Resource.Styleable usage to make SKCanvasView NativeAOT-compatible.

1. **Wrap attrs processing in try-catch** — workaround, confidence 0.75 (75%), cost/xs, validated=untested
   - Wrap the entire attrs processing block in a try-catch(Exception) so that if the Resource.Styleable initialization fails in NativeAOT, the view continues to work with default settings (ignorePixelScaling=false).
2. **Remove attrs.xml and Resource.Styleable usage** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Remove the Platform/Android/Resources/values/attrs.xml file and the associated Resource.Styleable usage from SKCanvasView.Initialize. The IgnorePixelScaling property remains settable programmatically. This is a minor breaking change for the very rare case of setting ignorePixelScaling via XML layout attributes.

**Recommended proposal:** Remove attrs.xml and Resource.Styleable usage

**Why:** Setting IgnorePixelScaling via XML attributes is a rarely-used feature; removing it eliminates the NativeAOT incompatibility cleanly without requiring ongoing maintenance of the try-catch approach.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Confirmed bug in SkiaSharp.Views.Android with clear stack trace and root cause. Fix needs investigation into whether attrs.xml removal is safe (backward compat) and whether the Resource.Styleable static init can be deferred or guarded in NativeAOT. |
| Suggested repro platform | linux |

### Missing Info

- Minimal repro project (e.g. GitHub repo) to confirm the crash is reproducible and validate fixes.
- Confirmation whether the crash also occurs without LottieView (plain SKCanvasView in MAUI with NativeAOT).
- Affected SkiaSharp versions: is 3.119.x affected or was this fixed?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, views, android, maui, and compatibility labels | labels=type/bug, area/SkiaSharp.Views, os/Android, partner/maui, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Acknowledge bug, explain root cause, request minimal repro, provide workaround information | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and for pointing to the dotnet/android issue!

This is a confirmed bug in `SkiaSharp.Views.Android`. The root cause is that `SKCanvasView` declares a custom `attrs.xml` attribute (`ignorePixelScaling`), which causes the .NET for Android build toolchain to generate a `Resource.designer.cs` whose static constructor calls `Assembly.Load("_Microsoft.Android.Resource.Designer")` to resolve resource IDs at runtime. In NativeAOT mode (`PublishAot=true`), `Assembly.Load` is unsupported, causing the crash.

**Workarounds while we investigate a fix:**
1. If you're setting `IgnorePixelScaling` via XML layout attributes, switch to setting it programmatically after the view is created instead.
2. If the crash persists (i.e. occurs even without XML attributes), it means the static initializer is being eagerly triggered by NativeAOT — in this case, there's no user-side workaround short of disabling NativeAOT.

The upstream tracking issue in dotnet/android is https://github.com/dotnet/android/issues/10852 — a fix there may also resolve this.

Could you share a minimal repro project? It would help us validate a fix on our side. Also, could you confirm: does the crash occur with a plain `SKCanvasView` (without LottieView), e.g. in a fresh MAUI app with just an `<SkiaSharp:SKCanvasView />` in the page?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4349,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-06T05:45:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKCanvasView crashes with FileNotFoundException on _Microsoft.Android.Resource.Designer when publishing a MAUI Android app with PublishAot=true (NativeAOT), because the Android resource designer shim uses Assembly.Load which is unsupported in NativeAOT.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.92
    },
    "platforms": [
      "os/Android"
    ],
    "tenets": [
      "tenet/compatibility"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "System.IO.FileNotFoundException: IO_FileNotFound_FileName, _Microsoft.Android.Resource.Designer",
      "stackTrace": "at Internal.Runtime.TypeLoaderExceptionHelper.CreateFileNotFoundException(ExceptionStringID, String) + 0x50\nat SkiaSharp.Views.Android.SKCanvasView.Initialize(IAttributeSet) + 0x18\nat SkiaSharp.Views.Maui.Handlers.SKCanvasViewHandler.CreatePlatformView() + 0x38",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net10.0-android"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a MAUI app that uses SKCanvasView (directly or via LottieView)",
        "Publish for Android with PublishAot=true: dotnet publish -f net10.0-android -r android-arm64 -c Release /p:PublishAot=true",
        "Launch the app on Android device/emulator",
        "Observe crash when SKCanvasView is instantiated"
      ],
      "environmentDetails": "Android 16, net10.0-android, SkiaSharp 3.116.0, NativeAOT (PublishAot=true), arm64",
      "repoLinks": [
        {
          "url": "https://github.com/dotnet/android/issues/10852",
          "description": "Related dotnet/android issue: Assembly.Load failure in NativeAOT for _Microsoft.Android.Resource.Designer"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The Resource.Styleable usage in SKCanvasView.Initialize has not changed; NativeAOT incompatibility persists in current codebase."
    }
  },
  "analysis": {
    "summary": "SKCanvasView.Initialize references Resource.Styleable.SKCanvasView (from attrs.xml) which triggers Android.Runtime.ResourceIdManager.UpdateIdValues() -> Assembly.Load('_Microsoft.Android.Resource.Designer') at static initialization time. In NativeAOT, Assembly.Load is unsupported, causing a crash. The NativeAOT compiler may eagerly initialize the Resource.Styleable type even when attrs is null, placing the failure at the start of Initialize (offset +0x18).",
    "rationale": "The stack trace clearly points to SKCanvasView.Initialize at offset +0x18, and the exception is FileNotFoundException for _Microsoft.Android.Resource.Designer — this is the well-known .NET for Android resource designer shim pattern that fails under NativeAOT. The crash only manifests with PublishAot=true. The code path via SKCanvasViewHandler.CreatePlatformView calls new SKCanvasView(Context) (no attrs), but the Resource.Styleable type's static constructor is triggered either eagerly or by the JIT's preparation of Initialize, which calls Assembly.Load and fails.",
    "keySignals": [
      {
        "text": "System.IO.FileNotFoundException: IO_FileNotFound_FileName, _Microsoft.Android.Resource.Designer",
        "source": "stack trace in issue body",
        "interpretation": "Assembly.Load('_Microsoft.Android.Resource.Designer') is called and fails because NativeAOT does not support runtime assembly loading."
      },
      {
        "text": "at SkiaSharp.Views.Android.SKCanvasView.Initialize(IAttributeSet) + 0x18",
        "source": "stack trace",
        "interpretation": "Crash occurs very early in Initialize (offset 24 bytes), consistent with eager static-type initialization in NativeAOT of the Resource.Styleable class referenced in that method."
      },
      {
        "text": "dotnet publish -f net10.0-android -r android-arm64 -c Release /p:PublishAot=true",
        "source": "build command in issue body",
        "interpretation": "Confirms NativeAOT publish mode, which disables Assembly.Load."
      },
      {
        "text": "Here is a similar issue but posted on .Net repo: https://github.com/dotnet/android/issues/10852",
        "source": "comment by reporter",
        "interpretation": "This is a cross-cutting issue between SkiaSharp and dotnet/android toolchain; fix may need to come from both sides."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs",
        "lines": "48-60",
        "finding": "Initialize(IAttributeSet) accesses Resource.Styleable.SKCanvasView and Resource.Styleable.SKCanvasView_ignorePixelScaling inside an if (attrs != null) guard. However, in NativeAOT the Resource.Styleable static constructor may run eagerly before the null check is evaluated, triggering Assembly.Load('_Microsoft.Android.Resource.Designer') which is unsupported in NativeAOT.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/Resources/values/attrs.xml",
        "lines": "1-6",
        "finding": "Declares a custom styleable SKCanvasView with an ignorePixelScaling attribute. This attrs.xml causes the .NET for Android build to generate a Resource.designer.cs with a static initializer that calls ResourceIdManager.UpdateIdValues(), which uses Assembly.Load.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Android.cs",
        "lines": "14",
        "finding": "CreatePlatformView() calls new SKCanvasView(Context) (no IAttributeSet). With attrs=null the attrs processing block is guarded, but the Resource.Styleable type initialization still occurs in NativeAOT.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKSurfaceView.cs",
        "lines": "12-29",
        "finding": "SKSurfaceView does not use Resource.Styleable and its Initialize() takes no IAttributeSet — it is not affected by this NativeAOT incompatibility.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Build without NativeAOT (remove PublishAot=true) — not suitable for release size optimization goals.",
      "Set SKCanvasView.IgnorePixelScaling programmatically after view creation rather than via XML attributes — avoids the attrs processing entirely but does not prevent the static init crash.",
      "Wait for dotnet/android to fix the Assembly.Load issue in NativeAOT (tracked at https://github.com/dotnet/android/issues/10852)."
    ],
    "nextQuestions": [
      "Does the crash also occur with SKGLView or other SkiaSharp Android views?",
      "Is this crash reproducible on net8.0-android or net9.0-android with NativeAOT, or only net10.0-android?",
      "Can the attrs.xml / Resource.Styleable usage be removed from SKCanvasView without breaking existing apps (the ignorePixelScaling attribute can still be set programmatically)?"
    ],
    "resolution": {
      "hypothesis": "SkiaSharp.Views.Android declares an attrs.xml causing the .NET for Android build to generate a Resource.designer.cs whose static constructor calls Assembly.Load. In NativeAOT this fails. The fix is to remove or guard the Resource.Styleable usage to make SKCanvasView NativeAOT-compatible.",
      "proposals": [
        {
          "title": "Wrap attrs processing in try-catch",
          "description": "Wrap the entire attrs processing block in a try-catch(Exception) so that if the Resource.Styleable initialization fails in NativeAOT, the view continues to work with default settings (ignorePixelScaling=false).",
          "category": "workaround",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Remove attrs.xml and Resource.Styleable usage",
          "description": "Remove the Platform/Android/Resources/values/attrs.xml file and the associated Resource.Styleable usage from SKCanvasView.Initialize. The IgnorePixelScaling property remains settable programmatically. This is a minor breaking change for the very rare case of setting ignorePixelScaling via XML layout attributes.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Remove attrs.xml and Resource.Styleable usage",
      "recommendedReason": "Setting IgnorePixelScaling via XML attributes is a rarely-used feature; removing it eliminates the NativeAOT incompatibility cleanly without requiring ongoing maintenance of the try-catch approach."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Confirmed bug in SkiaSharp.Views.Android with clear stack trace and root cause. Fix needs investigation into whether attrs.xml removal is safe (backward compat) and whether the Resource.Styleable static init can be deferred or guarded in NativeAOT.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Minimal repro project (e.g. GitHub repo) to confirm the crash is reproducible and validate fixes.",
      "Confirmation whether the crash also occurs without LottieView (plain SKCanvasView in MAUI with NativeAOT).",
      "Affected SkiaSharp versions: is 3.119.x affected or was this fixed?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, android, maui, and compatibility labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Android",
          "partner/maui",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge bug, explain root cause, request minimal repro, provide workaround information",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report and for pointing to the dotnet/android issue!\n\nThis is a confirmed bug in `SkiaSharp.Views.Android`. The root cause is that `SKCanvasView` declares a custom `attrs.xml` attribute (`ignorePixelScaling`), which causes the .NET for Android build toolchain to generate a `Resource.designer.cs` whose static constructor calls `Assembly.Load(\"_Microsoft.Android.Resource.Designer\")` to resolve resource IDs at runtime. In NativeAOT mode (`PublishAot=true`), `Assembly.Load` is unsupported, causing the crash.\n\n**Workarounds while we investigate a fix:**\n1. If you're setting `IgnorePixelScaling` via XML layout attributes, switch to setting it programmatically after the view is created instead.\n2. If the crash persists (i.e. occurs even without XML attributes), it means the static initializer is being eagerly triggered by NativeAOT — in this case, there's no user-side workaround short of disabling NativeAOT.\n\nThe upstream tracking issue in dotnet/android is https://github.com/dotnet/android/issues/10852 — a fix there may also resolve this.\n\nCould you share a minimal repro project? It would help us validate a fix on our side. Also, could you confirm: does the crash occur with a plain `SKCanvasView` (without LottieView), e.g. in a fresh MAUI app with just an `<SkiaSharp:SKCanvasView />` in the page?"
      }
    ]
  }
}
```

</details>
