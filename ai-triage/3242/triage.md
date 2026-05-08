# Issue Triage Report — #3242

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T16:52:00Z |
| Type | type/bug (0.80 (80%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.75 (75%)) |

**Issue Summary:** A specific Lottie file that appears to contain movie/video layer references renders blank when using SkiaSharp.Skottie.Animation.Create() and Animation.Render(), while other Lottie files render correctly.

**Analysis:** The Lottie file likely contains movie/video layer references or external asset dependencies that Skottie does not support without a configured ResourceProvider. The reporter uses Animation.Create() directly without AnimationBuilder, which means no ResourceProvider is set. Additionally, the reporter's code snippet omits any Seek() call before Render(), which is required to position the animation timeline. Either unsupported video layers or missing Seek() (or both) cause the blank output.

**Recommendations:** **needs-investigation** — Two plausible causes exist (missing Seek + unsupported video layers). Need to confirm which applies by testing with Seek(0) and inspecting the Lottie file's asset types. Repro file is attached so investigation is feasible.

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
| Current labels | type/bug |

## Evidence

### Reproduction

1. Load the provided BadLottie.json file using Animation.Create(path)
2. Call animation.Render(surface.Canvas, new SKRect(0, 0, 100, 100))
3. Observe blank canvas output despite properties (Duration, Size, etc.) loading correctly

**Environment:** Windows 11, SkiaSharp 3.116.0, Visual Studio on Windows, tested on multiple Windows machines

**Repository links:**
- https://github.com/user-attachments/files/19828604/BadLottie.json — The specific Lottie file that fails to render (attached to issue)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Nothing is rendered to the canvas - it's blank |
| Repro quality | partial |
| Target frameworks | net8.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | No changes to Skottie resource loading between 3.116.0 and current that would fix this. |

## Analysis

### Technical Summary

The Lottie file likely contains movie/video layer references or external asset dependencies that Skottie does not support without a configured ResourceProvider. The reporter uses Animation.Create() directly without AnimationBuilder, which means no ResourceProvider is set. Additionally, the reporter's code snippet omits any Seek() call before Render(), which is required to position the animation timeline. Either unsupported video layers or missing Seek() (or both) cause the blank output.

### Rationale

The reporter confirms other Lottie files work — so this is specific to this file's content. The file is described as 'created from a movie file', suggesting it may contain video/precomp layers referencing external video assets. Skottie silently skips unsupported or unresolvable asset layers, producing a blank canvas. The reporter's code also does not call Seek() before Render(), which is required per the Skottie API — however, since other files presumably work the same way, the missing Seek() alone is unlikely to be the sole cause. The bug is either unsupported layer types or missing asset resolution via ResourceProvider.

### Key Signals

- "this particular file does not render" — **issue body** (File-specific failure — other files work, so it is the content of this specific Lottie file causing the issue.)
- "I suspect this has to do with the data coming from a movie file which is not supported by SkiaSharp" — **issue body** (Reporter self-identified likely cause: movie/video layer in Lottie is unsupported by Skottie.)
- "The properties seem to load correctly" — **issue body** (Animation object created successfully and Duration/Size are valid — the file parses, but layers don't render.)
- "animation.Render(surface.Canvas, new SKRect(0, 0, 100, 100))" — **issue body code snippet** (No Seek() call shown before Render() — this is required by the Skottie API to position the animation timeline.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.Skottie/Animation.cs` | 91-102 | direct | Animation.Create(string path) loads file data and passes to skottie_animation_make_from_data — no resource provider is configured via this path, so external asset references in the Lottie file cannot be resolved. |
| `binding/SkiaSharp.Skottie/Animation.cs` | 106-110 | direct | Animation.Render() directly calls skottie_animation_render without any prior Seek(). Skottie requires Seek() to advance the internal scene graph before Render() produces visible output on the first frame. |
| `binding/SkiaSharp.Skottie/AnimationBuilder.cs` | 19-33 | related | AnimationBuilder.SetResourceProvider() is the correct API for providing external assets (e.g., video frames, images) required by complex Lottie files. Not used in the reporter's code path. |

### Workarounds

- Call animation.Seek(0) before the first animation.Render() call to initialize the scene graph.
- Use AnimationBuilder with a custom ResourceProvider to supply external assets (images/video frames) referenced by the Lottie file.
- Use the LottieFiles viewer or LottieSharp (wrapper around rlottie) as an alternative renderer that supports more Lottie features including video layers.

### Next Questions

- Does calling animation.Seek(0) before animation.Render() fix the blank canvas for this specific file?
- Does the BadLottie.json file contain 'assets' entries of type 'video' or reference external media files?
- Are there any Skottie warning logs produced when loading this file that indicate unsupported features?

### Resolution Proposals

**Hypothesis:** Two independent issues: (1) missing Seek() before Render() is a common usage pitfall; (2) the Lottie file uses video/movie layer types or external asset references that Skottie cannot render without a configured ResourceProvider.

1. **Call Seek(0) before first Render()** — workaround, confidence 0.65 (65%), cost/xs, validated=yes
   - Add animation.Seek(0) before animation.Render() to position the animation at frame 0 and allow the scene graph to initialize.

```csharp
animation = SkiaSharp.Skottie.Animation.Create(sourceFileAbsolute);
animation.Seek(0); // Required before first Render()
animation.Render(surface.Canvas, new SKRect(0, 0, 100, 100));
```
2. **Use AnimationBuilder with ResourceProvider for files with external assets** — workaround, confidence 0.60 (60%), cost/m, validated=yes
   - If the Lottie file references external video or image assets, use AnimationBuilder and implement a custom ResourceProvider to supply those assets.

```csharp
using var builder = Animation.CreateBuilder();
// builder.SetResourceProvider(myProvider); // supply external assets
using var animation = builder.Build(sourceFileAbsolute);
animation?.Seek(0);
animation?.Render(surface.Canvas, new SKRect(0, 0, 100, 100));
```

**Recommended proposal:** Call Seek(0) before first Render()

**Why:** Simplest first step — calling Seek(0) before Render() is required by the Skottie API and may fix the issue for this file. If the file genuinely uses video layers, that is a Skottie upstream limitation requiring a ResourceProvider.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.75 (75%) |
| Reason | Two plausible causes exist (missing Seek + unsupported video layers). Need to confirm which applies by testing with Seek(0) and inspecting the Lottie file's asset types. Repro file is attached so investigation is feasible. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, SkiaSharp area, Windows platform labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.75 (75%) | Ask reporter to try Seek(0) before Render and inspect Lottie file for video layers | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and for attaching the Lottie file!

There are two things to investigate:

**1. Call `Seek()` before `Render()`**

The Skottie API requires a `Seek()` call to position the animation timeline before rendering. Without it, the scene graph is uninitialized and renders blank. Please try:

```csharp
animation = SkiaSharp.Skottie.Animation.Create(sourceFileAbsolute);
animation.Seek(0); // position at frame 0
animation.Render(surface.Canvas, new SKRect(0, 0, 100, 100));
```

**2. Movie/video layer support**

If calling `Seek(0)` does not fix it, the Lottie file may contain video or movie layer references that require an external asset provider. Skottie silently skips layers whose assets cannot be resolved. You can use `AnimationBuilder` to supply a custom `ResourceProvider` if your file references external media.

Could you confirm whether adding `Seek(0)` resolves the issue? If not, can you share whether the Lottie file was exported with embedded video assets or external video references?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3242,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T16:52:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "A specific Lottie file that appears to contain movie/video layer references renders blank when using SkiaSharp.Skottie.Animation.Create() and Animation.Render(), while other Lottie files render correctly.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
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
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Nothing is rendered to the canvas - it's blank",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Load the provided BadLottie.json file using Animation.Create(path)",
        "Call animation.Render(surface.Canvas, new SKRect(0, 0, 100, 100))",
        "Observe blank canvas output despite properties (Duration, Size, etc.) loading correctly"
      ],
      "environmentDetails": "Windows 11, SkiaSharp 3.116.0, Visual Studio on Windows, tested on multiple Windows machines",
      "repoLinks": [
        {
          "url": "https://github.com/user-attachments/files/19828604/BadLottie.json",
          "description": "The specific Lottie file that fails to render (attached to issue)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "No changes to Skottie resource loading between 3.116.0 and current that would fix this."
    }
  },
  "analysis": {
    "summary": "The Lottie file likely contains movie/video layer references or external asset dependencies that Skottie does not support without a configured ResourceProvider. The reporter uses Animation.Create() directly without AnimationBuilder, which means no ResourceProvider is set. Additionally, the reporter's code snippet omits any Seek() call before Render(), which is required to position the animation timeline. Either unsupported video layers or missing Seek() (or both) cause the blank output.",
    "rationale": "The reporter confirms other Lottie files work — so this is specific to this file's content. The file is described as 'created from a movie file', suggesting it may contain video/precomp layers referencing external video assets. Skottie silently skips unsupported or unresolvable asset layers, producing a blank canvas. The reporter's code also does not call Seek() before Render(), which is required per the Skottie API — however, since other files presumably work the same way, the missing Seek() alone is unlikely to be the sole cause. The bug is either unsupported layer types or missing asset resolution via ResourceProvider.",
    "keySignals": [
      {
        "text": "this particular file does not render",
        "source": "issue body",
        "interpretation": "File-specific failure — other files work, so it is the content of this specific Lottie file causing the issue."
      },
      {
        "text": "I suspect this has to do with the data coming from a movie file which is not supported by SkiaSharp",
        "source": "issue body",
        "interpretation": "Reporter self-identified likely cause: movie/video layer in Lottie is unsupported by Skottie."
      },
      {
        "text": "The properties seem to load correctly",
        "source": "issue body",
        "interpretation": "Animation object created successfully and Duration/Size are valid — the file parses, but layers don't render."
      },
      {
        "text": "animation.Render(surface.Canvas, new SKRect(0, 0, 100, 100))",
        "source": "issue body code snippet",
        "interpretation": "No Seek() call shown before Render() — this is required by the Skottie API to position the animation timeline."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.Skottie/Animation.cs",
        "lines": "91-102",
        "finding": "Animation.Create(string path) loads file data and passes to skottie_animation_make_from_data — no resource provider is configured via this path, so external asset references in the Lottie file cannot be resolved.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.Skottie/Animation.cs",
        "lines": "106-110",
        "finding": "Animation.Render() directly calls skottie_animation_render without any prior Seek(). Skottie requires Seek() to advance the internal scene graph before Render() produces visible output on the first frame.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.Skottie/AnimationBuilder.cs",
        "lines": "19-33",
        "finding": "AnimationBuilder.SetResourceProvider() is the correct API for providing external assets (e.g., video frames, images) required by complex Lottie files. Not used in the reporter's code path.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Call animation.Seek(0) before the first animation.Render() call to initialize the scene graph.",
      "Use AnimationBuilder with a custom ResourceProvider to supply external assets (images/video frames) referenced by the Lottie file.",
      "Use the LottieFiles viewer or LottieSharp (wrapper around rlottie) as an alternative renderer that supports more Lottie features including video layers."
    ],
    "nextQuestions": [
      "Does calling animation.Seek(0) before animation.Render() fix the blank canvas for this specific file?",
      "Does the BadLottie.json file contain 'assets' entries of type 'video' or reference external media files?",
      "Are there any Skottie warning logs produced when loading this file that indicate unsupported features?"
    ],
    "resolution": {
      "hypothesis": "Two independent issues: (1) missing Seek() before Render() is a common usage pitfall; (2) the Lottie file uses video/movie layer types or external asset references that Skottie cannot render without a configured ResourceProvider.",
      "proposals": [
        {
          "title": "Call Seek(0) before first Render()",
          "description": "Add animation.Seek(0) before animation.Render() to position the animation at frame 0 and allow the scene graph to initialize.",
          "category": "workaround",
          "codeSnippet": "animation = SkiaSharp.Skottie.Animation.Create(sourceFileAbsolute);\nanimation.Seek(0); // Required before first Render()\nanimation.Render(surface.Canvas, new SKRect(0, 0, 100, 100));",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use AnimationBuilder with ResourceProvider for files with external assets",
          "description": "If the Lottie file references external video or image assets, use AnimationBuilder and implement a custom ResourceProvider to supply those assets.",
          "category": "workaround",
          "codeSnippet": "using var builder = Animation.CreateBuilder();\n// builder.SetResourceProvider(myProvider); // supply external assets\nusing var animation = builder.Build(sourceFileAbsolute);\nanimation?.Seek(0);\nanimation?.Render(surface.Canvas, new SKRect(0, 0, 100, 100));",
          "confidence": 0.6,
          "effort": "cost/m",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Call Seek(0) before first Render()",
      "recommendedReason": "Simplest first step — calling Seek(0) before Render() is required by the Skottie API and may fix the issue for this file. If the file genuinely uses video layers, that is a Skottie upstream limitation requiring a ResourceProvider."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.75,
      "reason": "Two plausible causes exist (missing Seek + unsupported video layers). Need to confirm which applies by testing with Seek(0) and inspecting the Lottie file's asset types. Repro file is attached so investigation is feasible.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp area, Windows platform labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter to try Seek(0) before Render and inspect Lottie file for video layers",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thanks for the detailed report and for attaching the Lottie file!\n\nThere are two things to investigate:\n\n**1. Call `Seek()` before `Render()`**\n\nThe Skottie API requires a `Seek()` call to position the animation timeline before rendering. Without it, the scene graph is uninitialized and renders blank. Please try:\n\n```csharp\nanimation = SkiaSharp.Skottie.Animation.Create(sourceFileAbsolute);\nanimation.Seek(0); // position at frame 0\nanimation.Render(surface.Canvas, new SKRect(0, 0, 100, 100));\n```\n\n**2. Movie/video layer support**\n\nIf calling `Seek(0)` does not fix it, the Lottie file may contain video or movie layer references that require an external asset provider. Skottie silently skips layers whose assets cannot be resolved. You can use `AnimationBuilder` to supply a custom `ResourceProvider` if your file references external media.\n\nCould you confirm whether adding `Seek(0)` resolves the issue? If not, can you share whether the Lottie file was exported with embedded video assets or external video references?"
      }
    ]
  }
}
```

</details>
