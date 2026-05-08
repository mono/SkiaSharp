# Issue Triage Report — #2033

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T22:32:20Z |
| Type | type/bug (0.93 (93%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** Visual Studio designer crashes when SKControl or SKGLControl from SkiaSharp.Views.WindowsForms is used in a WinForms project that multi-targets net6.0-windows and net462.

**Analysis:** The VS designer crashes when a WinForms project multi-targets net6.0-windows and net462 with SKControl or SKGLControl on the form. SKControl.OnPaint guards against DesignMode but the designer crash likely occurs at assembly load or constructor time due to VS resolving the wrong TFM assembly, or SKGLControl's OpenTK base class (GLControl) failing to initialize in design-time context. No fix has been identified in the current codebase.

**Recommendations:** **needs-investigation** — Complete repro steps and screenshot provided. The bug is real and reproducible, but root cause (assembly resolution vs GLControl native init) needs deeper investigation into the multi-target designer hosting behavior.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create a new Windows Forms App project targeting net6.0-windows
2. Add net462 to TargetFrameworks so the project multi-targets net6.0-windows;net462
3. Install SkiaSharp.Views.WindowsForms v2.80.3
4. Open the Form1.cs designer or drag an SKControl onto the form, save and restart VS
5. Observe designer crash

**Environment:** Visual Studio 2022 17.1.2, SkiaSharp.Views.WindowsForms 2.80.3, net6.0-windows + net462

**Repository links:**
- https://user-images.githubusercontent.com/1202288/167926737-690c626e-5539-4acb-9470-10cb31dc9413.png — Screenshot of the designer error

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | platform-specific |
| Error message | Visual Studio designer crashes when opening Form1.cs in a multi-targeted (net6.0-windows;net462) WinForms project containing SKControl or SKGLControl |
| Repro quality | complete |
| Target frameworks | net6.0-windows, net462 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The DesignMode guard exists in SKControl.OnPaint, but the issue may be in assembly resolution or constructor execution during multi-target design-time hosting. The code has not changed in a way that would fix this. |

## Analysis

### Technical Summary

The VS designer crashes when a WinForms project multi-targets net6.0-windows and net462 with SKControl or SKGLControl on the form. SKControl.OnPaint guards against DesignMode but the designer crash likely occurs at assembly load or constructor time due to VS resolving the wrong TFM assembly, or SKGLControl's OpenTK base class (GLControl) failing to initialize in design-time context. No fix has been identified in the current codebase.

### Rationale

This is a bug because the DesignMode guards in OnPaint exist to prevent rendering issues but do not address assembly resolution ambiguity in multi-TFM projects. SKGLControl inherits from OpenTK.GLControl which may trigger native OpenGL context creation even in design mode. The issue is reproducible with clear steps and a screenshot confirming the crash.

### Key Signals

- "The designer crashes if the application targets .NET Framework and .NET 6" — **issue body** (The multi-targeting combination (net6.0-windows + net462) is the trigger — design-time host may load the wrong TFM assembly or fail to resolve dependencies)
- "If you don't get an error at step 8, drag an SKControl onto the form, save, and restart Visual Studio" — **issue body** (The crash is persistent after first use — the control's type registration or the assembly reference is persisted in the designer state)
- "Last known good version: Unknown, probably none" — **issue body** (This may have always been broken for multi-target projects; not a regression from a specific version)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKControl.cs` | 27-29 | direct | OnPaint has a DesignMode guard that returns early — prevents SkiaSharp drawing in design mode, but constructor still runs (sets DoubleBuffered=true, ControlStyles). Does not protect against assembly resolution failures at design-time load in multi-target context. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs` | 73-77 | direct | SKGLControl inherits from OpenTK.GLControl and has a DesignMode guard in OnPaint. However, the base class GLControl constructor may attempt OpenGL context creation or P/Invoke into OpenGL native libraries, which would fail in design-time. The #if WINDOWS conditional compilation also suggests TFM-sensitive behavior. |

### Next Questions

- What exact error message does the designer show (the screenshot is not textually available)?
- Does the crash happen with SKControl only, SKGLControl only, or both?
- Does adding a DesignMode guard in the SKGLControl constructor (before GLControl base call) help?
- Is this reproduced with newer SkiaSharp versions (2.88.x, 3.x)?

### Resolution Proposals

**Hypothesis:** The VS 2022 designer for multi-target WinForms projects runs in a context where assembly resolution for the mixed TFM setup fails, or SKGLControl's OpenTK base class triggers native initialization that crashes the design-time host.

1. **Add designer-safe attribute to SKGLControl** — investigation, confidence 0.60 (60%), cost/m, validated=untested
   - Apply [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] or a custom designer that skips OpenGL initialization. This prevents the designer from invoking GLControl's native initialization.
2. **Investigate multi-target TFM assembly resolution** — investigation, confidence 0.70 (70%), cost/m, validated=untested
   - Check if SkiaSharp.Views.WindowsForms NuGet package provides correct design-time assemblies for both net462 and net6.0-windows TFMs. The issue may be a packaging problem where the wrong assembly is loaded by the designer.

**Recommended proposal:** Investigate multi-target TFM assembly resolution

**Why:** Assembly resolution issues in multi-target projects are a common root cause for designer crashes and are fixable at the package level without changing control code.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Complete repro steps and screenshot provided. The bug is real and reproducible, but root cause (assembly resolution vs GLControl native init) needs deeper investigation into the multi-target designer hosting behavior. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.93 (93%) | Apply type/bug, area/SkiaSharp.Views, os/Windows-Classic, tenet/compatibility | labels=type/bug, area/SkiaSharp.Views, os/Windows-Classic, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Ask reporter for the exact error message and whether both SKControl and SKGLControl are affected | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed reproduction steps and screenshot!

To help investigate further:
1. Could you share the exact error message shown in the designer error dialog? The screenshot shows the crash but the text would help identify the root cause.
2. Does the crash happen with **SKControl only**, **SKGLControl only**, or both?
3. Does this also reproduce with a newer version of SkiaSharp (2.88.x or 3.x)?

This appears to be related to how Visual Studio's designer host resolves assemblies for multi-targeted projects, or possibly OpenTK's native initialization inside SKGLControl's base class being triggered at design-time.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2033,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T22:32:20Z"
  },
  "summary": "Visual Studio designer crashes when SKControl or SKGLControl from SkiaSharp.Views.WindowsForms is used in a WinForms project that multi-targets net6.0-windows and net462.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.93
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "platform-specific",
      "errorMessage": "Visual Studio designer crashes when opening Form1.cs in a multi-targeted (net6.0-windows;net462) WinForms project containing SKControl or SKGLControl",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net6.0-windows",
        "net462"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a new Windows Forms App project targeting net6.0-windows",
        "Add net462 to TargetFrameworks so the project multi-targets net6.0-windows;net462",
        "Install SkiaSharp.Views.WindowsForms v2.80.3",
        "Open the Form1.cs designer or drag an SKControl onto the form, save and restart VS",
        "Observe designer crash"
      ],
      "environmentDetails": "Visual Studio 2022 17.1.2, SkiaSharp.Views.WindowsForms 2.80.3, net6.0-windows + net462",
      "repoLinks": [
        {
          "url": "https://user-images.githubusercontent.com/1202288/167926737-690c626e-5539-4acb-9470-10cb31dc9413.png",
          "description": "Screenshot of the designer error"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The DesignMode guard exists in SKControl.OnPaint, but the issue may be in assembly resolution or constructor execution during multi-target design-time hosting. The code has not changed in a way that would fix this."
    }
  },
  "analysis": {
    "summary": "The VS designer crashes when a WinForms project multi-targets net6.0-windows and net462 with SKControl or SKGLControl on the form. SKControl.OnPaint guards against DesignMode but the designer crash likely occurs at assembly load or constructor time due to VS resolving the wrong TFM assembly, or SKGLControl's OpenTK base class (GLControl) failing to initialize in design-time context. No fix has been identified in the current codebase.",
    "rationale": "This is a bug because the DesignMode guards in OnPaint exist to prevent rendering issues but do not address assembly resolution ambiguity in multi-TFM projects. SKGLControl inherits from OpenTK.GLControl which may trigger native OpenGL context creation even in design mode. The issue is reproducible with clear steps and a screenshot confirming the crash.",
    "keySignals": [
      {
        "text": "The designer crashes if the application targets .NET Framework and .NET 6",
        "source": "issue body",
        "interpretation": "The multi-targeting combination (net6.0-windows + net462) is the trigger — design-time host may load the wrong TFM assembly or fail to resolve dependencies"
      },
      {
        "text": "If you don't get an error at step 8, drag an SKControl onto the form, save, and restart Visual Studio",
        "source": "issue body",
        "interpretation": "The crash is persistent after first use — the control's type registration or the assembly reference is persisted in the designer state"
      },
      {
        "text": "Last known good version: Unknown, probably none",
        "source": "issue body",
        "interpretation": "This may have always been broken for multi-target projects; not a regression from a specific version"
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKControl.cs",
        "lines": "27-29",
        "finding": "OnPaint has a DesignMode guard that returns early — prevents SkiaSharp drawing in design mode, but constructor still runs (sets DoubleBuffered=true, ControlStyles). Does not protect against assembly resolution failures at design-time load in multi-target context.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs",
        "lines": "73-77",
        "finding": "SKGLControl inherits from OpenTK.GLControl and has a DesignMode guard in OnPaint. However, the base class GLControl constructor may attempt OpenGL context creation or P/Invoke into OpenGL native libraries, which would fail in design-time. The #if WINDOWS conditional compilation also suggests TFM-sensitive behavior.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "What exact error message does the designer show (the screenshot is not textually available)?",
      "Does the crash happen with SKControl only, SKGLControl only, or both?",
      "Does adding a DesignMode guard in the SKGLControl constructor (before GLControl base call) help?",
      "Is this reproduced with newer SkiaSharp versions (2.88.x, 3.x)?"
    ],
    "resolution": {
      "hypothesis": "The VS 2022 designer for multi-target WinForms projects runs in a context where assembly resolution for the mixed TFM setup fails, or SKGLControl's OpenTK base class triggers native initialization that crashes the design-time host.",
      "proposals": [
        {
          "title": "Add designer-safe attribute to SKGLControl",
          "description": "Apply [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] or a custom designer that skips OpenGL initialization. This prevents the designer from invoking GLControl's native initialization.",
          "category": "investigation",
          "confidence": 0.6,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Investigate multi-target TFM assembly resolution",
          "description": "Check if SkiaSharp.Views.WindowsForms NuGet package provides correct design-time assemblies for both net462 and net6.0-windows TFMs. The issue may be a packaging problem where the wrong assembly is loaded by the designer.",
          "category": "investigation",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate multi-target TFM assembly resolution",
      "recommendedReason": "Assembly resolution issues in multi-target projects are a common root cause for designer crashes and are fixable at the package level without changing control code."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Complete repro steps and screenshot provided. The bug is real and reproducible, but root cause (assembly resolution vs GLControl native init) needs deeper investigation into the multi-target designer hosting behavior.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views, os/Windows-Classic, tenet/compatibility",
        "risk": "low",
        "confidence": 0.93,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter for the exact error message and whether both SKControl and SKGLControl are affected",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed reproduction steps and screenshot!\n\nTo help investigate further:\n1. Could you share the exact error message shown in the designer error dialog? The screenshot shows the crash but the text would help identify the root cause.\n2. Does the crash happen with **SKControl only**, **SKGLControl only**, or both?\n3. Does this also reproduce with a newer version of SkiaSharp (2.88.x or 3.x)?\n\nThis appears to be related to how Visual Studio's designer host resolves assemblies for multi-targeted projects, or possibly OpenTK's native initialization inside SKGLControl's base class being triggered at design-time."
      }
    ]
  }
}
```

</details>
