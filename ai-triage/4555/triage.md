# Issue Triage Report — #4555

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-28T05:20:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Graphite/Metal recorder.Snap() returns null for the GradientBlend scene on the iOS simulator — isolated to the iOS-simulator Metal shader compiler rejecting a Graphite gradient+Multiply pipeline, while Ganesh/Metal and macOS Graphite/Metal render the same scene correctly.

**Analysis:** The iOS simulator's MTLCompilerService fails to compile the Metal render pipeline that Graphite emits for a 3-stop linear gradient combined with a Multiply-blend circle. recorder.Snap() returns null when any pipeline compilation fails. Ganesh/Metal renders the same scene on the same simulator successfully, and Graphite/Metal renders it on macOS — so the failure is isolated to the iOS-simulator Metal shader compiler handling of the specific pipeline Graphite emits.

**Recommendations:** **keep-open** — Known Graphite-Metal-on-iOS-simulator limitation tracked to remove a temporary skip once the upstream shader pipeline issue is resolved. Workaround is in place.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/iOS |
| Backends | backend/Metal |
| Tenets | — |
| Perf | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Build and run SkiaSharp.Tests.Devices on an iOS simulator (iOS 26.2, Apple Silicon host).
2. Run SkiaSharp.Tests.Visual.Tests.VisualMatrixTests.RenderMatchesGolden.
3. Observe 'graphite-metal' × 'GradientBlend' cell throws InvalidOperationException: Recorder.Snap() returned null.

**Environment:** iOS 26.2 simulator on Apple-silicon macOS; CI build xamarin/public SkiaSharp (Public) build 159072; nightly CI branch (PR #3968).

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/3968 — PR #3968 — Graphite Metal backend (source of this issue)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | exception |
| Error message | System.InvalidOperationException: Recorder.Snap() returned null. |
| Repro quality | complete |
| Target frameworks | net10.0-ios |

**Stack trace:**

```text
at SkiaSharp.Tests.Visual.GraphiteMetalRenderer.RenderAsync(...) line 87
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | nightly/CI |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Graphite backend is new in PR #3968 and has never shipped; this is a known limitation tracked to be addressed. |

## Analysis

### Technical Summary

The iOS simulator's MTLCompilerService fails to compile the Metal render pipeline that Graphite emits for a 3-stop linear gradient combined with a Multiply-blend circle. recorder.Snap() returns null when any pipeline compilation fails. Ganesh/Metal renders the same scene on the same simulator successfully, and Graphite/Metal renders it on macOS — so the failure is isolated to the iOS-simulator Metal shader compiler handling of the specific pipeline Graphite emits.

### Rationale

Clear bug: a backend path (Graphite/Metal on iOS simulator) produces an exception rather than pixels for a valid scene. Root cause is confirmed by the device console: 'Compiler failed to build request' from MTLCompilerService. Not a SkiaSharp binding bug — it is the upstream Metal shader Graphite emits for the gradient/Multiply combination that the iOS simulator compiler rejects. Severity is low because it affects only the iOS simulator (not real hardware) and a skip workaround is in place in PR #3968.

### Key Signals

- "System.InvalidOperationException: Recorder.Snap() returned null." — **issue body / CI log** (Graphite records a null snap when a Metal pipeline compilation fails — the exception is the SkiaSharp-level symptom.)
- "(Metal) Compiler failed to build request" — **comment by mattleibow (device console)** (iOS-sim MTLCompilerService rejects one of the Metal pipelines Graphite emits for this scene — root cause is in the Metal shader/pipeline Graphite generates.)
- "Ganesh/Metal renders the same scene on the same simulator; Graphite/Metal renders it on macOS" — **issue body** (Narrows root cause to the specific pipeline Graphite emits for gradient+Multiply on the iOS simulator's Metal compiler.)
- "In #3968 the cell is skipped on the simulator by converting null Snap() to RendererUnavailableException" — **issue body — Current handling / workaround section** (Workaround is in place; this issue tracks removing the skip once the pipeline issue is resolved.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `tests/Tests/SkiaSharp/Visual/Scenes/GradientBlendScene.cs` | 1-40 | direct | Scene draws a 3-stop linear gradient (Orange→DeepPink→Indigo) + a translucent Cyan circle with SKBlendMode.Multiply. This combination exercises gradient shaders and premultiplied-alpha blending — confirmed to be the exact pipeline that the iOS sim's Metal compiler rejects. |
| `tests/Tests/SkiaSharp/Visual/Renderers/GaneshMetalRenderer.cs` | 1-110 | related | Ganesh/Metal renderer uses GRContext.CreateMetal + SKSurface.Create. Draws via surface.Canvas directly. No Snap() step — confirms the Snap() null path is Graphite-specific, not Ganesh. |
| `tests/Tests/SkiaSharp/Visual/Tests/VisualMatrixTestsBase.cs` | 103-133 | context | RenderAsync throws RendererUnavailableException → cell is skipped (Assert.Skip). The PR #3968 workaround converts null Snap() into RendererUnavailableException, which flows through this path as a sanctioned skip. |

### Workarounds

- PR #3968 (commit bd78469) skips the GradientBlend cell on the iOS simulator by catching null Snap() and throwing RendererUnavailableException — this is the current in-tree workaround that prevents CI failure without masking real-hardware regressions.

### Next Questions

- Does the failure reproduce on a physical iOS device (expected not to, per issue description)?
- Which specific gradient pipeline step triggers the MTLCompilerService rejection — the 3-stop gradient interpolation, the Multiply blend, or their combination?
- Is there a Skia upstream issue or ANGLE workaround for this simulator Metal shader limitation?
- Will a future iOS simulator update (iOS 26.x → later) fix the compiler limitation?

### Resolution Proposals

**Hypothesis:** The iOS-simulator Metal shader compiler cannot compile the pipeline Graphite emits for a 3-stop linear gradient combined with Multiply-blend alpha compositing. This is an upstream Skia/Metal compatibility issue, not a SkiaSharp binding error.

1. **Keep skip, file upstream Skia issue** — workaround, confidence 0.80 (80%), cost/s, validated=untested
   - Retain the per-cell simulator skip in place (PR #3968) and file an issue against Skia's Graphite backend to simplify or work around the pipeline the simulator's Metal compiler rejects.
2. **Investigate simplifying the GradientBlend scene on the simulator** — investigation, confidence 0.65 (65%), cost/m, validated=untested
   - Try reducing the scene (2-stop gradient, or SrcOver blend) to identify the exact pipeline feature the iOS-sim compiler rejects, then report a targeted issue upstream.

**Recommended proposal:** Keep skip, file upstream Skia issue

**Why:** The workaround is already in place and does not mask real-hardware regressions. The root cause is in the iOS simulator's Metal compiler, which is outside SkiaSharp's control. Filing upstream is the correct next step.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Known Graphite-Metal-on-iOS-simulator limitation tracked to remove a temporary skip once the upstream shader pipeline issue is resolved. Workaround is in place. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, area/SkiaSharp, os/iOS, backend/Metal labels | labels=type/bug, area/SkiaSharp, os/iOS, backend/Metal |
| add-comment | medium | 0.88 (88%) | Acknowledge the confirmed root cause and next steps | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed investigation. Root cause confirmed: the iOS-simulator `MTLCompilerService` rejects the Metal render pipeline Graphite emits for the 3-stop gradient + `Multiply` blend combination — the `Compiler failed to build request` line in the device console is the smoking gun.

The in-PR workaround (skip the `GradientBlend` × `graphite-metal` cell on the simulator via `RendererUnavailableException`) is the right short-term fix: it keeps CI green without masking regressions on real hardware.

Next steps to eventually close this issue:
1. Verify the failure does **not** reproduce on a physical iOS device (as expected).
2. Narrow down which pipeline feature the sim compiler rejects (3-stop gradient? Multiply blend? their combination?) by simplifying the scene.
3. Consider filing a Skia upstream issue if the pipeline Graphite emits for this scene can be simplified.

cc @ramezgerges
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4555,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-28T05:20:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Graphite/Metal recorder.Snap() returns null for the GradientBlend scene on the iOS simulator — isolated to the iOS-simulator Metal shader compiler rejecting a Graphite gradient+Multiply pipeline, while Ganesh/Metal and macOS Graphite/Metal render the same scene correctly.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
    },
    "platforms": [
      "os/iOS"
    ],
    "backends": [
      "backend/Metal"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "System.InvalidOperationException: Recorder.Snap() returned null.",
      "stackTrace": "at SkiaSharp.Tests.Visual.GraphiteMetalRenderer.RenderAsync(...) line 87",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net10.0-ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Build and run SkiaSharp.Tests.Devices on an iOS simulator (iOS 26.2, Apple Silicon host).",
        "Run SkiaSharp.Tests.Visual.Tests.VisualMatrixTests.RenderMatchesGolden.",
        "Observe 'graphite-metal' × 'GradientBlend' cell throws InvalidOperationException: Recorder.Snap() returned null."
      ],
      "environmentDetails": "iOS 26.2 simulator on Apple-silicon macOS; CI build xamarin/public SkiaSharp (Public) build 159072; nightly CI branch (PR #3968).",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/3968",
          "description": "PR #3968 — Graphite Metal backend (source of this issue)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "nightly/CI"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The Graphite backend is new in PR #3968 and has never shipped; this is a known limitation tracked to be addressed."
    }
  },
  "analysis": {
    "summary": "The iOS simulator's MTLCompilerService fails to compile the Metal render pipeline that Graphite emits for a 3-stop linear gradient combined with a Multiply-blend circle. recorder.Snap() returns null when any pipeline compilation fails. Ganesh/Metal renders the same scene on the same simulator successfully, and Graphite/Metal renders it on macOS — so the failure is isolated to the iOS-simulator Metal shader compiler handling of the specific pipeline Graphite emits.",
    "rationale": "Clear bug: a backend path (Graphite/Metal on iOS simulator) produces an exception rather than pixels for a valid scene. Root cause is confirmed by the device console: 'Compiler failed to build request' from MTLCompilerService. Not a SkiaSharp binding bug — it is the upstream Metal shader Graphite emits for the gradient/Multiply combination that the iOS simulator compiler rejects. Severity is low because it affects only the iOS simulator (not real hardware) and a skip workaround is in place in PR #3968.",
    "keySignals": [
      {
        "text": "System.InvalidOperationException: Recorder.Snap() returned null.",
        "source": "issue body / CI log",
        "interpretation": "Graphite records a null snap when a Metal pipeline compilation fails — the exception is the SkiaSharp-level symptom."
      },
      {
        "text": "(Metal) Compiler failed to build request",
        "source": "comment by mattleibow (device console)",
        "interpretation": "iOS-sim MTLCompilerService rejects one of the Metal pipelines Graphite emits for this scene — root cause is in the Metal shader/pipeline Graphite generates."
      },
      {
        "text": "Ganesh/Metal renders the same scene on the same simulator; Graphite/Metal renders it on macOS",
        "source": "issue body",
        "interpretation": "Narrows root cause to the specific pipeline Graphite emits for gradient+Multiply on the iOS simulator's Metal compiler."
      },
      {
        "text": "In #3968 the cell is skipped on the simulator by converting null Snap() to RendererUnavailableException",
        "source": "issue body — Current handling / workaround section",
        "interpretation": "Workaround is in place; this issue tracks removing the skip once the pipeline issue is resolved."
      }
    ],
    "codeInvestigation": [
      {
        "file": "tests/Tests/SkiaSharp/Visual/Scenes/GradientBlendScene.cs",
        "lines": "1-40",
        "finding": "Scene draws a 3-stop linear gradient (Orange→DeepPink→Indigo) + a translucent Cyan circle with SKBlendMode.Multiply. This combination exercises gradient shaders and premultiplied-alpha blending — confirmed to be the exact pipeline that the iOS sim's Metal compiler rejects.",
        "relevance": "direct"
      },
      {
        "file": "tests/Tests/SkiaSharp/Visual/Renderers/GaneshMetalRenderer.cs",
        "lines": "1-110",
        "finding": "Ganesh/Metal renderer uses GRContext.CreateMetal + SKSurface.Create. Draws via surface.Canvas directly. No Snap() step — confirms the Snap() null path is Graphite-specific, not Ganesh.",
        "relevance": "related"
      },
      {
        "file": "tests/Tests/SkiaSharp/Visual/Tests/VisualMatrixTestsBase.cs",
        "lines": "103-133",
        "finding": "RenderAsync throws RendererUnavailableException → cell is skipped (Assert.Skip). The PR #3968 workaround converts null Snap() into RendererUnavailableException, which flows through this path as a sanctioned skip.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "PR #3968 (commit bd78469) skips the GradientBlend cell on the iOS simulator by catching null Snap() and throwing RendererUnavailableException — this is the current in-tree workaround that prevents CI failure without masking real-hardware regressions."
    ],
    "nextQuestions": [
      "Does the failure reproduce on a physical iOS device (expected not to, per issue description)?",
      "Which specific gradient pipeline step triggers the MTLCompilerService rejection — the 3-stop gradient interpolation, the Multiply blend, or their combination?",
      "Is there a Skia upstream issue or ANGLE workaround for this simulator Metal shader limitation?",
      "Will a future iOS simulator update (iOS 26.x → later) fix the compiler limitation?"
    ],
    "resolution": {
      "hypothesis": "The iOS-simulator Metal shader compiler cannot compile the pipeline Graphite emits for a 3-stop linear gradient combined with Multiply-blend alpha compositing. This is an upstream Skia/Metal compatibility issue, not a SkiaSharp binding error.",
      "proposals": [
        {
          "title": "Keep skip, file upstream Skia issue",
          "description": "Retain the per-cell simulator skip in place (PR #3968) and file an issue against Skia's Graphite backend to simplify or work around the pipeline the simulator's Metal compiler rejects.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Investigate simplifying the GradientBlend scene on the simulator",
          "description": "Try reducing the scene (2-stop gradient, or SrcOver blend) to identify the exact pipeline feature the iOS-sim compiler rejects, then report a targeted issue upstream.",
          "category": "investigation",
          "confidence": 0.65,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Keep skip, file upstream Skia issue",
      "recommendedReason": "The workaround is already in place and does not mask real-hardware regressions. The root cause is in the iOS simulator's Metal compiler, which is outside SkiaSharp's control. Filing upstream is the correct next step."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Known Graphite-Metal-on-iOS-simulator limitation tracked to remove a temporary skip once the upstream shader pipeline issue is resolved. Workaround is in place.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area/SkiaSharp, os/iOS, backend/Metal labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/iOS",
          "backend/Metal"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the confirmed root cause and next steps",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed investigation. Root cause confirmed: the iOS-simulator `MTLCompilerService` rejects the Metal render pipeline Graphite emits for the 3-stop gradient + `Multiply` blend combination — the `Compiler failed to build request` line in the device console is the smoking gun.\n\nThe in-PR workaround (skip the `GradientBlend` × `graphite-metal` cell on the simulator via `RendererUnavailableException`) is the right short-term fix: it keeps CI green without masking regressions on real hardware.\n\nNext steps to eventually close this issue:\n1. Verify the failure does **not** reproduce on a physical iOS device (as expected).\n2. Narrow down which pipeline feature the sim compiler rejects (3-stop gradient? Multiply blend? their combination?) by simplifying the scene.\n3. Consider filing a Skia upstream issue if the pipeline Graphite emits for this scene can be simplified.\n\ncc @ramezgerges"
      }
    ]
  }
}
```

</details>
