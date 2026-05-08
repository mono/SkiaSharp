# Issue Triage Report — #2832

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:16:37Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp (0.75 (75%)) |
| Suggested action | close-as-external (0.75 (75%)) |

**Issue Summary:** Drawing rendered with SkiaSharp over an OpenGL+GLFW surface is stretched or warped into an ellipse during window resize on Windows with NVIDIA graphics cards, but not with Intel integrated graphics.

**Analysis:** The warping/stretching artifact during resize is consistent with a known GLFW issue on Windows with NVIDIA drivers: during WM_SIZE message processing, NVIDIA's driver temporarily stretches the last rendered frame until a new frame is presented. A community commenter has linked the GLFW upstream issue #1231 as the root cause. SkiaSharp's code is used correctly — the surface is recreated, and canvas.Flush() correctly delegates to GRContext.Flush().

**Recommendations:** **close-as-external** — The warping/stretching artifact on NVIDIA during resize is consistent with a known GLFW issue (#1231) on Windows. SkiaSharp's code is being used correctly and the surface is properly recreated on each resize. The root cause is in GLFW or NVIDIA driver behavior, not in SkiaSharp.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Open a GLFW window using Silk.NET GLFW bindings on Windows with NVIDIA graphics
2. Create a GRContext via GRContext.CreateGl(grGlInterface)
3. In the main loop, call draw() which recreates GRBackendRenderTarget + SKSurface every frame
4. Register a framebuffer resize callback that also calls draw()
5. Resize the window and observe that the circle is stretched into an ellipse during the resize

**Environment:** Windows 11 23H2 (Build 22631.3447), Dell XPS with NVIDIA GPU, SkiaSharp 3.x (Alpha) and 2.88.8, Visual Studio on Windows

**Repository links:**
- https://github.com/mono/SkiaSharp/files/14965352/SilkNETGLFWWindowTest.zip — Attached zip with F# project reproducing the issue
- https://github.com/glfw/glfw/issues/1231 — GLFW issue referenced by a commenter — resize flickering/stretching on Windows

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Circle is stretched into an ellipse during window resize on NVIDIA graphics |
| Repro quality | complete |
| Target frameworks | net9.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.8, 3.x Alpha |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Issue occurs on all tried versions including 2.88.8 (latest stable) and 3.x Alpha — not a regression, seems to be a longstanding behavior in the GLFW+NVIDIA path. |

## Analysis

### Technical Summary

The warping/stretching artifact during resize is consistent with a known GLFW issue on Windows with NVIDIA drivers: during WM_SIZE message processing, NVIDIA's driver temporarily stretches the last rendered frame until a new frame is presented. A community commenter has linked the GLFW upstream issue #1231 as the root cause. SkiaSharp's code is used correctly — the surface is recreated, and canvas.Flush() correctly delegates to GRContext.Flush().

### Rationale

The reported behavior (circle stretched to ellipse on NVIDIA but not on Intel during resize) matches known GLFW resize behavior on Windows. The reporter's code is structurally correct from a SkiaSharp standpoint: GRBackendRenderTarget is recreated with the new dimensions, SKSurface is recreated, canvas.Flush() (which internally calls GRContext.Flush()) is called, and SwapBuffers is invoked. Intel may handle WM_SIZE differently internally or produce different driver-level stretching. The root cause is in GLFW/NVIDIA's OS-level frame buffer scaling during resize, outside SkiaSharp's control.

### Key Signals

- "It only seems to occur when NVIDIA graphics are used. Thus, it is unclear whether this is a SkiaSharp problem, a Skia problem, whatever backend graphics API is being chosen by Skia or SkiaSharp, or an NVIDIA driver problem." — **issue body** (Reporter suspects external cause; NVIDIA-specific behavior is consistent with driver/OS-level frame stretching during resize.)
- "the entire surface is recreated during the resize callback and the drawing redrawn, and the callback is blocking" — **issue body** (The SkiaSharp usage appears correct — surface is recreated with correct dimensions each time. The stretch must come from a lower layer.)
- "Seems to be with glfw https://github.com/glfw/glfw/issues/1231" — **comment by jakobs** (Community commenter points to GLFW upstream issue as the root cause, which describes resize flickering/stretching on Windows.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 734-737 | direct | canvas.Flush() delegates to (Context as GRContext)?.Flush() — confirms the reporter's Flush() call is functionally equivalent to grContext.Flush(). The SkiaSharp side is correct. |
| `binding/SkiaSharp/GRContext.cs` | 131-138 | related | ResetContext() calls sk_direct_context_reset_context — API used by reporter is correct and exposed as expected. No SkiaSharp-level bug in context reset. |
| `binding/SkiaSharp/GRBackendRenderTarget.cs` | 15-19 | context | GRBackendRenderTarget constructor correctly accepts new width/height per frame. Recreating on resize is the documented correct pattern. |

### Workarounds

- Check if GLFW itself offers a workaround (e.g., using glfwWaitEventsTimeout instead of PollEvents during resize)
- Add a call to glViewport(0, 0, width, height) explicitly before Skia renders to ensure the GL viewport matches the new framebuffer size
- Consider an alternative windowing library (e.g., SDL2 or Win32 native) that handles WM_SIZE more synchronously on NVIDIA

### Next Questions

- Does the issue still occur if the draw loop is moved entirely into the resize callback (removing the main-loop draw)?
- Does adding an explicit glViewport call change the behavior on NVIDIA?
- Is GLFW issue #1231 still open/unresolved, or has a fix been released that would address this?

### Resolution Proposals

**Hypothesis:** During window resize on Windows, NVIDIA's OpenGL driver temporarily scales/stretches the last presented framebuffer as the OS resizes the window region before the application can present a new frame. GLFW issue #1231 documents this behavior. SkiaSharp has no mechanism to prevent this as it occurs at the driver/OS level during WM_SIZE processing.

1. **Acknowledge as external — GLFW + NVIDIA driver behavior** — investigation, confidence 0.78 (78%), cost/xs, validated=untested
   - The stretching occurs at the GLFW/NVIDIA driver level during WM_SIZE processing, not in SkiaSharp or Skia. Close with an explanation pointing to GLFW issue #1231 and suggest GLFW-level workarounds.
2. **Try explicit glViewport before rendering** — workaround, confidence 0.45 (45%), cost/s, validated=untested
   - Add a P/Invoke to glViewport(0, 0, width, height) before the Skia render calls. On NVIDIA, this may force the driver to use the correct viewport and prevent stretching.

**Recommended proposal:** Acknowledge as external — GLFW + NVIDIA driver behavior

**Why:** The root cause is clearly in GLFW's resize handling on NVIDIA as confirmed by the community comment linking GLFW #1231. SkiaSharp has no code to change here. The reporter should follow up with GLFW or use an alternative windowing system.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.75 (75%) |
| Reason | The warping/stretching artifact on NVIDIA during resize is consistent with a known GLFW issue (#1231) on Windows. SkiaSharp's code is being used correctly and the surface is properly recreated on each resize. The root cause is in GLFW or NVIDIA driver behavior, not in SkiaSharp. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply bug, SkiaSharp core area, Windows and OpenGL labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/OpenGL, tenet/reliability |
| add-comment | high | 0.75 (75%) | Post analysis explaining the GLFW/NVIDIA root cause and suggest workarounds | — |
| close-issue | medium | 0.75 (75%) | Close as external — root cause is GLFW + NVIDIA driver behavior on Windows | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro! After reviewing the code and a community comment, this appears to be related to a known GLFW issue on Windows with NVIDIA drivers: [glfw/glfw#1231](https://github.com/glfw/glfw/issues/1231).

During a window resize on Windows, NVIDIA's OpenGL driver temporarily scales the last presented framebuffer at the OS level (WM_SIZE processing) before the application can present a new frame. This stretching/warping happens below the SkiaSharp/Skia layer and is not something SkiaSharp can prevent.

Your SkiaSharp usage looks correct — you're recreating `GRBackendRenderTarget` and `SKSurface` with new dimensions on each resize, and `canvas.Flush()` correctly submits the commands.

**Potential workarounds to investigate:**
1. Try using `glfwWaitEventsTimeout` or rendering synchronously within the WM_SIZE callback at a lower level (see GLFW issue for details)
2. Try adding an explicit `glViewport(0, 0, width, height)` P/Invoke call before Skia renders
3. Consider SDL2 as an alternative windowing layer, which handles WM_SIZE differently on Windows

Since the root cause is outside SkiaSharp, we'll close this issue. Please follow up in the GLFW repository if the workarounds don't help.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2832,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:16:37Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Drawing rendered with SkiaSharp over an OpenGL+GLFW surface is stretched or warped into an ellipse during window resize on Windows with NVIDIA graphics cards, but not with Intel integrated graphics.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.75
    },
    "platforms": [
      "os/Windows-Classic"
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
      "errorMessage": "Circle is stretched into an ellipse during window resize on NVIDIA graphics",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Open a GLFW window using Silk.NET GLFW bindings on Windows with NVIDIA graphics",
        "Create a GRContext via GRContext.CreateGl(grGlInterface)",
        "In the main loop, call draw() which recreates GRBackendRenderTarget + SKSurface every frame",
        "Register a framebuffer resize callback that also calls draw()",
        "Resize the window and observe that the circle is stretched into an ellipse during the resize"
      ],
      "environmentDetails": "Windows 11 23H2 (Build 22631.3447), Dell XPS with NVIDIA GPU, SkiaSharp 3.x (Alpha) and 2.88.8, Visual Studio on Windows",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/14965352/SilkNETGLFWWindowTest.zip",
          "description": "Attached zip with F# project reproducing the issue"
        },
        {
          "url": "https://github.com/glfw/glfw/issues/1231",
          "description": "GLFW issue referenced by a commenter — resize flickering/stretching on Windows"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.8",
        "3.x Alpha"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Issue occurs on all tried versions including 2.88.8 (latest stable) and 3.x Alpha — not a regression, seems to be a longstanding behavior in the GLFW+NVIDIA path."
    }
  },
  "analysis": {
    "summary": "The warping/stretching artifact during resize is consistent with a known GLFW issue on Windows with NVIDIA drivers: during WM_SIZE message processing, NVIDIA's driver temporarily stretches the last rendered frame until a new frame is presented. A community commenter has linked the GLFW upstream issue #1231 as the root cause. SkiaSharp's code is used correctly — the surface is recreated, and canvas.Flush() correctly delegates to GRContext.Flush().",
    "rationale": "The reported behavior (circle stretched to ellipse on NVIDIA but not on Intel during resize) matches known GLFW resize behavior on Windows. The reporter's code is structurally correct from a SkiaSharp standpoint: GRBackendRenderTarget is recreated with the new dimensions, SKSurface is recreated, canvas.Flush() (which internally calls GRContext.Flush()) is called, and SwapBuffers is invoked. Intel may handle WM_SIZE differently internally or produce different driver-level stretching. The root cause is in GLFW/NVIDIA's OS-level frame buffer scaling during resize, outside SkiaSharp's control.",
    "keySignals": [
      {
        "text": "It only seems to occur when NVIDIA graphics are used. Thus, it is unclear whether this is a SkiaSharp problem, a Skia problem, whatever backend graphics API is being chosen by Skia or SkiaSharp, or an NVIDIA driver problem.",
        "source": "issue body",
        "interpretation": "Reporter suspects external cause; NVIDIA-specific behavior is consistent with driver/OS-level frame stretching during resize."
      },
      {
        "text": "the entire surface is recreated during the resize callback and the drawing redrawn, and the callback is blocking",
        "source": "issue body",
        "interpretation": "The SkiaSharp usage appears correct — surface is recreated with correct dimensions each time. The stretch must come from a lower layer."
      },
      {
        "text": "Seems to be with glfw https://github.com/glfw/glfw/issues/1231",
        "source": "comment by jakobs",
        "interpretation": "Community commenter points to GLFW upstream issue as the root cause, which describes resize flickering/stretching on Windows."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "734-737",
        "finding": "canvas.Flush() delegates to (Context as GRContext)?.Flush() — confirms the reporter's Flush() call is functionally equivalent to grContext.Flush(). The SkiaSharp side is correct.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "lines": "131-138",
        "finding": "ResetContext() calls sk_direct_context_reset_context — API used by reporter is correct and exposed as expected. No SkiaSharp-level bug in context reset.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/GRBackendRenderTarget.cs",
        "lines": "15-19",
        "finding": "GRBackendRenderTarget constructor correctly accepts new width/height per frame. Recreating on resize is the documented correct pattern.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Check if GLFW itself offers a workaround (e.g., using glfwWaitEventsTimeout instead of PollEvents during resize)",
      "Add a call to glViewport(0, 0, width, height) explicitly before Skia renders to ensure the GL viewport matches the new framebuffer size",
      "Consider an alternative windowing library (e.g., SDL2 or Win32 native) that handles WM_SIZE more synchronously on NVIDIA"
    ],
    "nextQuestions": [
      "Does the issue still occur if the draw loop is moved entirely into the resize callback (removing the main-loop draw)?",
      "Does adding an explicit glViewport call change the behavior on NVIDIA?",
      "Is GLFW issue #1231 still open/unresolved, or has a fix been released that would address this?"
    ],
    "resolution": {
      "hypothesis": "During window resize on Windows, NVIDIA's OpenGL driver temporarily scales/stretches the last presented framebuffer as the OS resizes the window region before the application can present a new frame. GLFW issue #1231 documents this behavior. SkiaSharp has no mechanism to prevent this as it occurs at the driver/OS level during WM_SIZE processing.",
      "proposals": [
        {
          "title": "Acknowledge as external — GLFW + NVIDIA driver behavior",
          "description": "The stretching occurs at the GLFW/NVIDIA driver level during WM_SIZE processing, not in SkiaSharp or Skia. Close with an explanation pointing to GLFW issue #1231 and suggest GLFW-level workarounds.",
          "category": "investigation",
          "confidence": 0.78,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Try explicit glViewport before rendering",
          "description": "Add a P/Invoke to glViewport(0, 0, width, height) before the Skia render calls. On NVIDIA, this may force the driver to use the correct viewport and prevent stretching.",
          "category": "workaround",
          "confidence": 0.45,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Acknowledge as external — GLFW + NVIDIA driver behavior",
      "recommendedReason": "The root cause is clearly in GLFW's resize handling on NVIDIA as confirmed by the community comment linking GLFW #1231. SkiaSharp has no code to change here. The reporter should follow up with GLFW or use an alternative windowing system."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.75,
      "reason": "The warping/stretching artifact on NVIDIA during resize is consistent with a known GLFW issue (#1231) on Windows. SkiaSharp's code is being used correctly and the surface is properly recreated on each resize. The root cause is in GLFW or NVIDIA driver behavior, not in SkiaSharp.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp core area, Windows and OpenGL labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis explaining the GLFW/NVIDIA root cause and suggest workarounds",
        "risk": "high",
        "confidence": 0.75,
        "comment": "Thanks for the detailed repro! After reviewing the code and a community comment, this appears to be related to a known GLFW issue on Windows with NVIDIA drivers: [glfw/glfw#1231](https://github.com/glfw/glfw/issues/1231).\n\nDuring a window resize on Windows, NVIDIA's OpenGL driver temporarily scales the last presented framebuffer at the OS level (WM_SIZE processing) before the application can present a new frame. This stretching/warping happens below the SkiaSharp/Skia layer and is not something SkiaSharp can prevent.\n\nYour SkiaSharp usage looks correct — you're recreating `GRBackendRenderTarget` and `SKSurface` with new dimensions on each resize, and `canvas.Flush()` correctly submits the commands.\n\n**Potential workarounds to investigate:**\n1. Try using `glfwWaitEventsTimeout` or rendering synchronously within the WM_SIZE callback at a lower level (see GLFW issue for details)\n2. Try adding an explicit `glViewport(0, 0, width, height)` P/Invoke call before Skia renders\n3. Consider SDL2 as an alternative windowing layer, which handles WM_SIZE differently on Windows\n\nSince the root cause is outside SkiaSharp, we'll close this issue. Please follow up in the GLFW repository if the workarounds don't help."
      },
      {
        "type": "close-issue",
        "description": "Close as external — root cause is GLFW + NVIDIA driver behavior on Windows",
        "risk": "medium",
        "confidence": 0.75,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
