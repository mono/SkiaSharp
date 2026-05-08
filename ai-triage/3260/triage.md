# Issue Triage Report — #3260

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T06:55:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | needs-investigation (0.90 (90%)) |

**Issue Summary:** SKGLView crashes in Android release mode with 'no Java peer type found' for SKGLTextureView+InternalRenderer, a regression introduced in SkiaSharp 3.119 vs 3.118-preview.2.3.

**Analysis:** In Android release builds, R8/ProGuard linker strips the JNI Java peer for 'SKGLTextureView+InternalRenderer' (a private nested class extending SKGLTextureViewRenderer / Java.Lang.Object) because no [Preserve] or [Register] attribute protects it from AOT linker removal. This pattern exists in both SKGLTextureView and SKGLSurfaceView. A MacCatalyst crash is mentioned in comments (-[SKCanvasView drawRect:] unhandled exception in release mode) which appears unrelated.

**Recommendations:** **needs-investigation** — Confirmed regression with multiple reporters and clear stack trace. Root cause is likely missing [Preserve] attribute on private nested Java.Lang.Object subclasses, but the exact regression trigger between 3.118 and 3.119 needs investigation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android, os/macOS |
| Backends | — |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a MAUI app using SKGLView on Android
2. Build and deploy in Release mode
3. Observe crash on app start when SKGLView is initialized

**Environment:** Android 16 on Google Pixel 6a, Windows 11, SkiaSharp 3.119, last good: 3.118.0-preview.2

**Repository links:**
- https://github.com/dotnet/android/issues/10096 — Upstream .NET Android issue filed at maintainer request

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | crash |
| Error message | Cannot create instance of type 'SkiaSharp.Views.Android.SKGLTextureView+InternalRenderer': no Java peer type found. |
| Repro quality | partial |
| Target frameworks | net9.0-android |

**Stack trace:**

```text
Java.Interop.JniPeerMembers+JniInstanceMethods..ctor -> StartCreateInstance -> Java.Lang.Object..ctor -> SKGLTextureViewRenderer..ctor
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.119, 3.118.0-preview.2 |
| Worked in | 3.118.0-preview.2 |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Multiple reporters confirm the regression and reproduction with current 3.119 release; issue is still open. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.95 (95%) |
| Reason | Three independent users confirm it works in 3.118.0-preview.2 and fails in 3.119. The error is consistent and reproducible. |
| Worked in version | 3.118.0-preview.2 |
| Broke in version | 3.119 |

## Analysis

### Technical Summary

In Android release builds, R8/ProGuard linker strips the JNI Java peer for 'SKGLTextureView+InternalRenderer' (a private nested class extending SKGLTextureViewRenderer / Java.Lang.Object) because no [Preserve] or [Register] attribute protects it from AOT linker removal. This pattern exists in both SKGLTextureView and SKGLSurfaceView. A MacCatalyst crash is mentioned in comments (-[SKCanvasView drawRect:] unhandled exception in release mode) which appears unrelated.

### Rationale

Confirmed regression in 3.119 vs 3.118-preview.2.3, with three independent reporters. The stack trace points to JNI peer construction of a private nested class that extends Java.Lang.Object — a known .NET Android release-mode linker issue when [Preserve]/[Register] annotations are missing. The SKGLTextureView.InternalRenderer and SKGLSurfaceView.InternalRenderer classes are both affected.

### Key Signals

- "Cannot create instance of type 'SkiaSharp.Views.Android.SKGLTextureView+InternalRenderer': no Java peer type found." — **issue body - log output** (The Android R8/ProGuard linker has stripped the Java peer for this private nested Java.Lang.Object subclass in release mode.)
- "I did confirm that downgrading to 3.118 preview 2.3 resolves the issue with the same MAUI version." — **comment by reporter daltzctr** (Clear regression signal — something changed between 3.118-preview.2.3 and 3.119 that triggered linker stripping of the Java peer.)
- "Also facing this issue, when reverting to 3.118-preview2.3 it does not occur" — **comment by bcaceiro** (Third independent confirmation of the regression.)
- "TextureView doesn't support displaying a background drawable" — **issue body - second error in log** (Secondary error indicating a background was set on the TextureView — may be a separate issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureView.cs` | 56-69 | direct | InternalRenderer is a private nested class extending SKGLTextureViewRenderer (which extends Java.Lang.Object). It has no [Preserve] or [Register] attribute. In .NET Android release builds, R8/ProGuard can strip JNI peers for private nested Java.Lang.Object subclasses, causing 'no Java peer type found' at runtime. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceView.cs` | 45-58 | related | SKGLSurfaceView.InternalRenderer follows the same pattern as SKGLTextureView.InternalRenderer — private nested class extending a Java.Lang.Object subclass with no [Preserve] attribute. Likely affected by the same linker-stripping issue. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Android.cs` | 117-143 | related | MauiSKGLTextureView is a private nested class of SKGLViewHandler extending SKGLTextureView. While SKGLTextureView is not a direct Java.Lang.Object subclass, if Android linker recurses through the class hierarchy during release stripping this could also be affected. |

### Workarounds

- Downgrade to SkiaSharp 3.118.0-preview.2 (confirmed by multiple users)

### Next Questions

- What specific code change between 3.118-preview.2.3 and 3.119 triggered the linker to strip the Java peer?
- Does SKGLSurfaceView.InternalRenderer show the same crash when used directly?
- Is the MacCatalyst crash (-[SKCanvasView drawRect:]) a separate issue or related to the same 3.119 release?

### Resolution Proposals

**Hypothesis:** The private nested class InternalRenderer (extending Java.Lang.Object via SKGLTextureViewRenderer) lacks [Preserve] attribute, causing .NET Android's R8 linker to strip its JNI peer in release builds. A change in the 3.119 build configuration or Android SDK version may have tightened linking rules, exposing this pre-existing issue.

1. **Add [Preserve] attribute to InternalRenderer classes** — fix, confidence 0.80 (80%), cost/xs, validated=untested
   - Add [Android.Runtime.Preserve(AllMembers = true)] (or [Java.Interop.JniAddNativeMethodRegistrationAttribute]) to SKGLTextureView.InternalRenderer and SKGLSurfaceView.InternalRenderer to prevent R8 from stripping their JNI peers in release builds.
2. **Add R8/ProGuard keep rules** — alternative, confidence 0.70 (70%), cost/s, validated=untested
   - Add ProGuard/R8 keep rules in the Android packaging configuration to preserve all SkiaSharp Java peer types. This is a broader fix but more configuration-heavy.
3. **Investigate 3.118 vs 3.119 build diff** — investigation, confidence 0.90 (90%), cost/s, validated=untested
   - Compare the build pipeline, Android SDK version, and linking configuration between 3.118-preview.2.3 and 3.119 to identify the specific change that triggered linker stripping.

**Recommended proposal:** Add [Preserve] attribute to InternalRenderer classes

**Why:** Minimal, targeted fix that follows the standard .NET Android pattern for protecting Java.Lang.Object subclasses from R8 stripping. Should be tried first before deeper investigation.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.90 (90%) |
| Reason | Confirmed regression with multiple reporters and clear stack trace. Root cause is likely missing [Preserve] attribute on private nested Java.Lang.Object subclasses, but the exact regression trigger between 3.118 and 3.119 needs investigation. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply regression-confirmed classification labels | labels=type/bug, area/SkiaSharp.Views, os/Android, tenet/reliability, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Acknowledge regression, explain root cause hypothesis, ask for investigation help | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and upstream cross-reference.

This appears to be a regression in SkiaSharp 3.119 affecting Android release builds. The error `no Java peer type found` for `SKGLTextureView+InternalRenderer` is a classic .NET Android R8/ProGuard linker issue: the private nested class `InternalRenderer` extends `SKGLTextureViewRenderer` (which inherits from `Java.Lang.Object`) but lacks a `[Preserve]` attribute, so the release-mode AOT linker strips its JNI peer.

We're investigating what changed between 3.118-preview.2.3 and 3.119 that exposed this.

**Workaround (confirmed):** Downgrade to `SkiaSharp 3.118.0-preview.2` while a fix is prepared.

**Note on MacCatalyst crash:** If you're seeing a different crash (`-[SKCanvasView drawRect:]` unhandled exception on MacCatalyst), please file a separate issue as that appears to be an unrelated problem.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3260,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T06:55:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SKGLView crashes in Android release mode with 'no Java peer type found' for SKGLTextureView+InternalRenderer, a regression introduced in SkiaSharp 3.119 vs 3.118-preview.2.3.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.92
    },
    "platforms": [
      "os/Android",
      "os/macOS"
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
      "errorType": "crash",
      "errorMessage": "Cannot create instance of type 'SkiaSharp.Views.Android.SKGLTextureView+InternalRenderer': no Java peer type found.",
      "stackTrace": "Java.Interop.JniPeerMembers+JniInstanceMethods..ctor -> StartCreateInstance -> Java.Lang.Object..ctor -> SKGLTextureViewRenderer..ctor",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net9.0-android"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a MAUI app using SKGLView on Android",
        "Build and deploy in Release mode",
        "Observe crash on app start when SKGLView is initialized"
      ],
      "environmentDetails": "Android 16 on Google Pixel 6a, Windows 11, SkiaSharp 3.119, last good: 3.118.0-preview.2",
      "repoLinks": [
        {
          "url": "https://github.com/dotnet/android/issues/10096",
          "description": "Upstream .NET Android issue filed at maintainer request"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.119",
        "3.118.0-preview.2"
      ],
      "workedIn": "3.118.0-preview.2",
      "currentRelevance": "likely",
      "relevanceReason": "Multiple reporters confirm the regression and reproduction with current 3.119 release; issue is still open."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.95,
      "reason": "Three independent users confirm it works in 3.118.0-preview.2 and fails in 3.119. The error is consistent and reproducible.",
      "workedInVersion": "3.118.0-preview.2",
      "brokeInVersion": "3.119"
    }
  },
  "analysis": {
    "summary": "In Android release builds, R8/ProGuard linker strips the JNI Java peer for 'SKGLTextureView+InternalRenderer' (a private nested class extending SKGLTextureViewRenderer / Java.Lang.Object) because no [Preserve] or [Register] attribute protects it from AOT linker removal. This pattern exists in both SKGLTextureView and SKGLSurfaceView. A MacCatalyst crash is mentioned in comments (-[SKCanvasView drawRect:] unhandled exception in release mode) which appears unrelated.",
    "rationale": "Confirmed regression in 3.119 vs 3.118-preview.2.3, with three independent reporters. The stack trace points to JNI peer construction of a private nested class that extends Java.Lang.Object — a known .NET Android release-mode linker issue when [Preserve]/[Register] annotations are missing. The SKGLTextureView.InternalRenderer and SKGLSurfaceView.InternalRenderer classes are both affected.",
    "keySignals": [
      {
        "text": "Cannot create instance of type 'SkiaSharp.Views.Android.SKGLTextureView+InternalRenderer': no Java peer type found.",
        "source": "issue body - log output",
        "interpretation": "The Android R8/ProGuard linker has stripped the Java peer for this private nested Java.Lang.Object subclass in release mode."
      },
      {
        "text": "I did confirm that downgrading to 3.118 preview 2.3 resolves the issue with the same MAUI version.",
        "source": "comment by reporter daltzctr",
        "interpretation": "Clear regression signal — something changed between 3.118-preview.2.3 and 3.119 that triggered linker stripping of the Java peer."
      },
      {
        "text": "Also facing this issue, when reverting to 3.118-preview2.3 it does not occur",
        "source": "comment by bcaceiro",
        "interpretation": "Third independent confirmation of the regression."
      },
      {
        "text": "TextureView doesn't support displaying a background drawable",
        "source": "issue body - second error in log",
        "interpretation": "Secondary error indicating a background was set on the TextureView — may be a separate issue."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLTextureView.cs",
        "lines": "56-69",
        "finding": "InternalRenderer is a private nested class extending SKGLTextureViewRenderer (which extends Java.Lang.Object). It has no [Preserve] or [Register] attribute. In .NET Android release builds, R8/ProGuard can strip JNI peers for private nested Java.Lang.Object subclasses, causing 'no Java peer type found' at runtime.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKGLSurfaceView.cs",
        "lines": "45-58",
        "finding": "SKGLSurfaceView.InternalRenderer follows the same pattern as SKGLTextureView.InternalRenderer — private nested class extending a Java.Lang.Object subclass with no [Preserve] attribute. Likely affected by the same linker-stripping issue.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKGLView/SKGLViewHandler.Android.cs",
        "lines": "117-143",
        "finding": "MauiSKGLTextureView is a private nested class of SKGLViewHandler extending SKGLTextureView. While SKGLTextureView is not a direct Java.Lang.Object subclass, if Android linker recurses through the class hierarchy during release stripping this could also be affected.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "What specific code change between 3.118-preview.2.3 and 3.119 triggered the linker to strip the Java peer?",
      "Does SKGLSurfaceView.InternalRenderer show the same crash when used directly?",
      "Is the MacCatalyst crash (-[SKCanvasView drawRect:]) a separate issue or related to the same 3.119 release?"
    ],
    "workarounds": [
      "Downgrade to SkiaSharp 3.118.0-preview.2 (confirmed by multiple users)"
    ],
    "resolution": {
      "hypothesis": "The private nested class InternalRenderer (extending Java.Lang.Object via SKGLTextureViewRenderer) lacks [Preserve] attribute, causing .NET Android's R8 linker to strip its JNI peer in release builds. A change in the 3.119 build configuration or Android SDK version may have tightened linking rules, exposing this pre-existing issue.",
      "proposals": [
        {
          "title": "Add [Preserve] attribute to InternalRenderer classes",
          "description": "Add [Android.Runtime.Preserve(AllMembers = true)] (or [Java.Interop.JniAddNativeMethodRegistrationAttribute]) to SKGLTextureView.InternalRenderer and SKGLSurfaceView.InternalRenderer to prevent R8 from stripping their JNI peers in release builds.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Add R8/ProGuard keep rules",
          "description": "Add ProGuard/R8 keep rules in the Android packaging configuration to preserve all SkiaSharp Java peer types. This is a broader fix but more configuration-heavy.",
          "category": "alternative",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Investigate 3.118 vs 3.119 build diff",
          "description": "Compare the build pipeline, Android SDK version, and linking configuration between 3.118-preview.2.3 and 3.119 to identify the specific change that triggered linker stripping.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add [Preserve] attribute to InternalRenderer classes",
      "recommendedReason": "Minimal, targeted fix that follows the standard .NET Android pattern for protecting Java.Lang.Object subclasses from R8 stripping. Should be tried first before deeper investigation."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.9,
      "reason": "Confirmed regression with multiple reporters and clear stack trace. Root cause is likely missing [Preserve] attribute on private nested Java.Lang.Object subclasses, but the exact regression trigger between 3.118 and 3.119 needs investigation.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply regression-confirmed classification labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Android",
          "tenet/reliability",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge regression, explain root cause hypothesis, ask for investigation help",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thank you for the detailed report and upstream cross-reference.\n\nThis appears to be a regression in SkiaSharp 3.119 affecting Android release builds. The error `no Java peer type found` for `SKGLTextureView+InternalRenderer` is a classic .NET Android R8/ProGuard linker issue: the private nested class `InternalRenderer` extends `SKGLTextureViewRenderer` (which inherits from `Java.Lang.Object`) but lacks a `[Preserve]` attribute, so the release-mode AOT linker strips its JNI peer.\n\nWe're investigating what changed between 3.118-preview.2.3 and 3.119 that exposed this.\n\n**Workaround (confirmed):** Downgrade to `SkiaSharp 3.118.0-preview.2` while a fix is prepared.\n\n**Note on MacCatalyst crash:** If you're seeing a different crash (`-[SKCanvasView drawRect:]` unhandled exception on MacCatalyst), please file a separate issue as that appears to be an unrelated problem."
      }
    ]
  }
}
```

</details>
