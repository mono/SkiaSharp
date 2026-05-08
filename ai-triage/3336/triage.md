# Issue Triage Report — #3336

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:50:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp.Views (0.97 (97%)) |
| Suggested action | ready-to-fix (0.90 (90%)) |

**Issue Summary:** EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE constant in Egl.cs has value 0x33AA (same as COPY), but the correct ANGLE spec value for FAST is 0x33A9, causing all display attribute arrays in GlesContext to silently use the COPY present path instead of the FAST path on Windows WinUI.

**Analysis:** The constant EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE is defined as 0x33AA in Egl.cs — identical to EGL_EXPERIMENTAL_PRESENT_PATH_COPY_ANGLE — when the ANGLE specification defines it as 0x33A9. All three display initialization attribute arrays in GlesContext.cs reference FAST_ANGLE, so they all silently send the COPY value (0x33AA) to EGL. This means the D3D11-backed ANGLE renderer always uses the copy present path, foregoing the potential performance benefit of the fast path.

**Recommendations:** **ready-to-fix** — Root cause is clear — wrong hex constant. Recommended fix (use COPY explicitly) is a low-risk one-liner. Full FAST path enablement needs maintainer sign-off due to surface orientation side-effects.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-WinUI |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, area/SkiaSharp.Views, backend/OpenGL, os/Windows-WinUI, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Open source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/Egl.cs
2. Check line 62: EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE = 0x33AA
3. Check line 63: EGL_EXPERIMENTAL_PRESENT_PATH_COPY_ANGLE = 0x33AA
4. Verify ANGLE spec: FAST should be 0x33A9, COPY should be 0x33AA
5. Observe both constants have same value — FAST silently behaves as COPY in GlesContext.cs

**Environment:** Windows WinUI, SkiaSharp 3.0.119, Visual Studio (Windows)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE = 0x33AA (should be 0x33A9); 0x33AA is the COPY path value |
| Repro quality | partial |
| Target frameworks | net8.0-windows10.0.19041.0, net9.0-windows10.0.19041.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.0.119, 2.88.9 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Code still has identical wrong value in current main branch. Commenter confirmed the bug also existed in 2.88.9, so this is not a regression but a long-standing defect. |

## Analysis

### Technical Summary

The constant EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE is defined as 0x33AA in Egl.cs — identical to EGL_EXPERIMENTAL_PRESENT_PATH_COPY_ANGLE — when the ANGLE specification defines it as 0x33A9. All three display initialization attribute arrays in GlesContext.cs reference FAST_ANGLE, so they all silently send the COPY value (0x33AA) to EGL. This means the D3D11-backed ANGLE renderer always uses the copy present path, foregoing the potential performance benefit of the fast path.

### Rationale

The bug is straightforward: a wrong hex literal. Both FAST and COPY constants share 0x33AA; FAST should be 0x33A9 per the ANGLE EGL extension specification. The fix path is clear, though there is a design decision (fix the FAST value vs. explicitly use COPY everywhere) because enabling FAST causes surfaces to render vertically flipped, requiring a GRSurfaceOrigin.TopLeft change to compensate. The issue is not a regression from 2.88.9; commenter confirmed 2.88.9 had the same wrong value.

### Key Signals

- "correct value is 0x33A9 => current value in code is 0x33AA => NOK" — **issue body** (Reporter correctly identifies the wrong hex literal for EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE.)
- "It was also the wrong value in 2.88.9 though... part of the perf improvement is that it's not conformant (the surfaces are vertically flipped)." — **comment by molesmoke** (Not a regression; long-standing defect. Enabling the true FAST path introduces surface orientation change as a side effect.)
- "Might want to comment on whether he'd prefer a fix that explicitly uses the copy path, or whether the fast path should be correctly enabled as a breaking change?" — **comment by molesmoke** (Two valid fix strategies: (1) fix constant to 0x33A9 and adjust surface origin, or (2) rename usages to COPY_ANGLE to be explicit about the current behavior.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/Egl.cs` | 61-63 | direct | EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE is defined as 0x33AA (line 62) and EGL_EXPERIMENTAL_PRESENT_PATH_COPY_ANGLE is also 0x33AA (line 63). The ANGLE spec defines FAST as 0x33A9 and COPY as 0x33AA, so FAST has the wrong value. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs` | 168, 184, 195 | direct | All three display attribute arrays (defaultDisplayAttributes, fl9_3DisplayAttributes, warpDisplayAttributes) set EGL_EXPERIMENTAL_PRESENT_PATH_ANGLE to EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE. Because of the wrong constant value, they all pass 0x33AA (COPY) instead of 0x33A9 (FAST) to the ANGLE EGL driver. |

### Workarounds

- No workaround needed for current behavior — the code silently operates correctly using the COPY path; the issue is a missing performance optimization (FAST path) that was never actually enabled.
- Custom control authors accessing GRContext directly from SKSwapChainPanel.GRContext who want the FAST path should set surface origin to GRSurfaceOrigin.TopLeft when using the true FAST path value.

### Next Questions

- Should the fix correct FAST to 0x33A9 (enabling fast path with potential surface flip side-effect), or rename FAST usages to COPY to make the current behavior explicit?
- If FAST path is correctly enabled, does SKSwapChainPanel.GRContext still render right-side-up, or does it require a surface origin change?

### Resolution Proposals

**Hypothesis:** Wrong hex literal in Egl.cs: FAST constant should be 0x33A9, not 0x33AA. Two fix approaches exist depending on whether the fast path behavior change is acceptable.

1. **Correct FAST constant to 0x33A9** — fix, confidence 0.75 (75%), cost/s, validated=untested
   - Change EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE from 0x33AA to 0x33A9 to match the ANGLE EGL spec. This enables the true fast path but may cause surfaces to render vertically flipped. The rendering code in SKSwapChainPanel may need to be updated to use GRSurfaceOrigin.TopLeft.
2. **Explicitly use COPY path constant** — fix, confidence 0.90 (90%), cost/xs, validated=untested
   - Replace all references to EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE in GlesContext.cs with EGL_EXPERIMENTAL_PRESENT_PATH_COPY_ANGLE. This makes the current behavior explicit and avoids any surface orientation side-effects. The FAST constant can still be fixed for completeness.

**Recommended proposal:** Explicitly use COPY path constant

**Why:** Lowest risk: preserves current rendering behavior while making it explicit. The FAST path change would be a behavioral breaking change requiring additional investigation of surface orientation impact on SKSwapChainPanel users.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.90 (90%) |
| Reason | Root cause is clear — wrong hex constant. Recommended fix (use COPY explicitly) is a low-risk one-liner. Full FAST path enablement needs maintainer sign-off due to surface orientation side-effects. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply confirmed labels: bug, WinUI views, OpenGL backend, reliability tenet | labels=type/bug, area/SkiaSharp.Views, backend/OpenGL, os/Windows-WinUI, tenet/reliability |
| add-comment | medium | 0.90 (90%) | Post analysis confirming the wrong constant and proposing two fix options | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The bug is confirmed: in `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/Egl.cs`, `EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE` is incorrectly defined as `0x33AA` — which is actually the value for `EGL_EXPERIMENTAL_PRESENT_PATH_COPY_ANGLE`. The correct ANGLE spec value for FAST is `0x33A9`.

As a result, all three display initialization paths in `GlesContext.cs` silently use the COPY present path instead of the FAST path.

As noted in the thread, there are two fix options:
1. **Correct FAST to `0x33A9`** — enables the true fast path but surfaces may render vertically flipped (callers using `SKSwapChainPanel.GRContext` may need `GRSurfaceOrigin.TopLeft`).
2. **Use `EGL_EXPERIMENTAL_PRESENT_PATH_COPY_ANGLE` explicitly** — makes the current behavior explicit with no rendering change.

@mattleibow, could you confirm which approach you'd prefer?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3336,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:50:00Z",
    "currentLabels": [
      "type/bug",
      "area/SkiaSharp.Views",
      "backend/OpenGL",
      "os/Windows-WinUI",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE constant in Egl.cs has value 0x33AA (same as COPY), but the correct ANGLE spec value for FAST is 0x33A9, causing all display attribute arrays in GlesContext to silently use the COPY present path instead of the FAST path on Windows WinUI.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.97
    },
    "platforms": [
      "os/Windows-WinUI"
    ],
    "backends": [
      "backend/OpenGL"
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
      "errorMessage": "EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE = 0x33AA (should be 0x33A9); 0x33AA is the COPY path value",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-windows10.0.19041.0",
        "net9.0-windows10.0.19041.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Open source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/Egl.cs",
        "Check line 62: EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE = 0x33AA",
        "Check line 63: EGL_EXPERIMENTAL_PRESENT_PATH_COPY_ANGLE = 0x33AA",
        "Verify ANGLE spec: FAST should be 0x33A9, COPY should be 0x33AA",
        "Observe both constants have same value — FAST silently behaves as COPY in GlesContext.cs"
      ],
      "environmentDetails": "Windows WinUI, SkiaSharp 3.0.119, Visual Studio (Windows)"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.0.119",
        "2.88.9"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Code still has identical wrong value in current main branch. Commenter confirmed the bug also existed in 2.88.9, so this is not a regression but a long-standing defect."
    }
  },
  "analysis": {
    "summary": "The constant EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE is defined as 0x33AA in Egl.cs — identical to EGL_EXPERIMENTAL_PRESENT_PATH_COPY_ANGLE — when the ANGLE specification defines it as 0x33A9. All three display initialization attribute arrays in GlesContext.cs reference FAST_ANGLE, so they all silently send the COPY value (0x33AA) to EGL. This means the D3D11-backed ANGLE renderer always uses the copy present path, foregoing the potential performance benefit of the fast path.",
    "rationale": "The bug is straightforward: a wrong hex literal. Both FAST and COPY constants share 0x33AA; FAST should be 0x33A9 per the ANGLE EGL extension specification. The fix path is clear, though there is a design decision (fix the FAST value vs. explicitly use COPY everywhere) because enabling FAST causes surfaces to render vertically flipped, requiring a GRSurfaceOrigin.TopLeft change to compensate. The issue is not a regression from 2.88.9; commenter confirmed 2.88.9 had the same wrong value.",
    "keySignals": [
      {
        "text": "correct value is 0x33A9 => current value in code is 0x33AA => NOK",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies the wrong hex literal for EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE."
      },
      {
        "text": "It was also the wrong value in 2.88.9 though... part of the perf improvement is that it's not conformant (the surfaces are vertically flipped).",
        "source": "comment by molesmoke",
        "interpretation": "Not a regression; long-standing defect. Enabling the true FAST path introduces surface orientation change as a side effect."
      },
      {
        "text": "Might want to comment on whether he'd prefer a fix that explicitly uses the copy path, or whether the fast path should be correctly enabled as a breaking change?",
        "source": "comment by molesmoke",
        "interpretation": "Two valid fix strategies: (1) fix constant to 0x33A9 and adjust surface origin, or (2) rename usages to COPY_ANGLE to be explicit about the current behavior."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/Egl.cs",
        "lines": "61-63",
        "finding": "EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE is defined as 0x33AA (line 62) and EGL_EXPERIMENTAL_PRESENT_PATH_COPY_ANGLE is also 0x33AA (line 63). The ANGLE spec defines FAST as 0x33A9 and COPY as 0x33AA, so FAST has the wrong value.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/GlesContext.cs",
        "lines": "168, 184, 195",
        "finding": "All three display attribute arrays (defaultDisplayAttributes, fl9_3DisplayAttributes, warpDisplayAttributes) set EGL_EXPERIMENTAL_PRESENT_PATH_ANGLE to EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE. Because of the wrong constant value, they all pass 0x33AA (COPY) instead of 0x33A9 (FAST) to the ANGLE EGL driver.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "No workaround needed for current behavior — the code silently operates correctly using the COPY path; the issue is a missing performance optimization (FAST path) that was never actually enabled.",
      "Custom control authors accessing GRContext directly from SKSwapChainPanel.GRContext who want the FAST path should set surface origin to GRSurfaceOrigin.TopLeft when using the true FAST path value."
    ],
    "nextQuestions": [
      "Should the fix correct FAST to 0x33A9 (enabling fast path with potential surface flip side-effect), or rename FAST usages to COPY to make the current behavior explicit?",
      "If FAST path is correctly enabled, does SKSwapChainPanel.GRContext still render right-side-up, or does it require a surface origin change?"
    ],
    "resolution": {
      "hypothesis": "Wrong hex literal in Egl.cs: FAST constant should be 0x33A9, not 0x33AA. Two fix approaches exist depending on whether the fast path behavior change is acceptable.",
      "proposals": [
        {
          "title": "Correct FAST constant to 0x33A9",
          "description": "Change EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE from 0x33AA to 0x33A9 to match the ANGLE EGL spec. This enables the true fast path but may cause surfaces to render vertically flipped. The rendering code in SKSwapChainPanel may need to be updated to use GRSurfaceOrigin.TopLeft.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Explicitly use COPY path constant",
          "description": "Replace all references to EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE in GlesContext.cs with EGL_EXPERIMENTAL_PRESENT_PATH_COPY_ANGLE. This makes the current behavior explicit and avoids any surface orientation side-effects. The FAST constant can still be fixed for completeness.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Explicitly use COPY path constant",
      "recommendedReason": "Lowest risk: preserves current rendering behavior while making it explicit. The FAST path change would be a behavioral breaking change requiring additional investigation of surface orientation impact on SKSwapChainPanel users."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.9,
      "reason": "Root cause is clear — wrong hex constant. Recommended fix (use COPY explicitly) is a low-risk one-liner. Full FAST path enablement needs maintainer sign-off due to surface orientation side-effects.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply confirmed labels: bug, WinUI views, OpenGL backend, reliability tenet",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "backend/OpenGL",
          "os/Windows-WinUI",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis confirming the wrong constant and proposing two fix options",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the report! The bug is confirmed: in `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/GlesInterop/Egl.cs`, `EGL_EXPERIMENTAL_PRESENT_PATH_FAST_ANGLE` is incorrectly defined as `0x33AA` — which is actually the value for `EGL_EXPERIMENTAL_PRESENT_PATH_COPY_ANGLE`. The correct ANGLE spec value for FAST is `0x33A9`.\n\nAs a result, all three display initialization paths in `GlesContext.cs` silently use the COPY present path instead of the FAST path.\n\nAs noted in the thread, there are two fix options:\n1. **Correct FAST to `0x33A9`** — enables the true fast path but surfaces may render vertically flipped (callers using `SKSwapChainPanel.GRContext` may need `GRSurfaceOrigin.TopLeft`).\n2. **Use `EGL_EXPERIMENTAL_PRESENT_PATH_COPY_ANGLE` explicitly** — makes the current behavior explicit with no rendering change.\n\n@mattleibow, could you confirm which approach you'd prefer?"
      }
    ]
  }
}
```

</details>
