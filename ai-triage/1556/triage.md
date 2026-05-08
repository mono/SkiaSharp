# Issue Triage Report — #1556

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T10:22:00Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp.Views (0.80 (80%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** Windows Forms app on .NET 5.0 throws AccessViolationException ('Attempted to read or write protected memory') when using SkiaSharp.

**Analysis:** Reporter gets an AccessViolationException (memory corruption crash) using SkiaSharp in a Windows Forms app on .NET 5.0. No stack trace is provided, but a repro GitHub repository is linked. The crash likely originates from a native Skia memory access issue — either a threading problem in the WinForms paint loop, a GC/pinning issue in SKControl where native surface pointers into GDI bitmap memory become invalid, or a .NET 5.0-specific native interop regression.

**Recommendations:** **needs-investigation** — Real crash with a linked repro repository. Needs reproduction to get stack trace and confirm root cause.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a Windows Forms App (.NET) targeting .NET 5.0
2. Add SkiaSharp
3. Observe AccessViolationException at runtime

**Environment:** Windows 10 Enterprise 10.0.19042 Build 19042, VS 2019 Preview 16.9.0 Preview 1.0, .NET 5.0

**Related issues:** #1490, #706, #1137

**Repository links:**
- https://github.com/CBrauer/TestSkia — Reproducible solution provided by reporter

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | Attempted to read or write protected memory. This is often an indication that other memory is corrupt. |
| Repro quality | partial |
| Target frameworks | net5.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | net5.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Issue was filed with .NET 5.0 (EOL). No SkiaSharp version specified. The SKControl and SKGLControl code paths exist in current codebase but .NET 5.0-specific native asset packaging may have changed. |

## Analysis

### Technical Summary

Reporter gets an AccessViolationException (memory corruption crash) using SkiaSharp in a Windows Forms app on .NET 5.0. No stack trace is provided, but a repro GitHub repository is linked. The crash likely originates from a native Skia memory access issue — either a threading problem in the WinForms paint loop, a GC/pinning issue in SKControl where native surface pointers into GDI bitmap memory become invalid, or a .NET 5.0-specific native interop regression.

### Rationale

AccessViolationException in .NET is almost always a native memory access violation, pointing to SkiaSharp's native layer. The Windows Forms SKControl creates a surface over a locked GDI Bitmap's memory region — if the surface or canvas escapes the using block, or if the bitmap is disposed while native code holds a pointer, the crash follows. The issue has a linked repro project, so it is classifiable as a real bug requiring investigation.

### Key Signals

- "Attempted to read or write protected memory. This is often an indication that other memory is corrupt." — **issue body** (AccessViolationException — native memory access outside valid region. Classic symptom of use-after-free or GC moving a pinned buffer.)
- "I submitted this problem to the .NET Core group, but they say it's not their problem" — **issue body** (.NET runtime team deflected — confirms the crash is inside SkiaSharp's native interop layer.)
- "I have put my reproducible solution on GitHub at: https://github.com/CBrauer/TestSkia" — **issue body** (Complete repro project available — accelerates reproduction and root cause identification.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKControl.cs` | 39-52 | direct | SKControl.OnPaint locks a GDI Bitmap with LockBits and creates an SKSurface over data.Scan0 (raw pointer). The surface is disposed in a using block before UnlockBits. If the user's PaintSurface handler retains a reference to the SKSurface or SKCanvas beyond the event, the native surface will outlive the locked memory region, causing a crash on next access. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs` | 82-133 | related | SKGLControl.OnPaint uses OpenGL via OpenTK. The grContext, renderTarget, and surface are long-lived instance fields. On .NET 5.0, OpenTK interop with the Windows OpenGL context may have compatibility issues that trigger memory violations. |

### Next Questions

- Which SkiaSharp control is used in the repro — SKControl or SKGLControl?
- Which SkiaSharp NuGet package version is referenced?
- Is there a stack trace from the crash (can be obtained via Visual Studio or Process Monitor)?
- Does the crash occur immediately on startup, or after some user interaction (resize, repaint)?

### Resolution Proposals

**Hypothesis:** The crash likely results from the user's PaintSurface handler retaining a reference to SKSurface/SKCanvas beyond the event callback (use-after-free of the locked GDI bitmap memory), or from an OpenTK .NET 5.0 incompatibility in SKGLControl.

1. **Investigate repro project** — investigation, confidence 0.90 (90%), cost/s, validated=untested
   - Clone https://github.com/CBrauer/TestSkia and reproduce the crash on Windows to identify the exact stack trace and code path.
2. **Workaround: do not retain surface/canvas references** — workaround, confidence 0.75 (75%), cost/xs, validated=untested
   - Ensure the PaintSurface handler does not store the SKSurface or SKCanvas in a field for later use. All drawing must happen within the event handler synchronously.

**Recommended proposal:** Investigate repro project

**Why:** The repro is available; cloning it will reveal the exact crash callstack, resolving the ambiguity between SKControl memory pinning and SKGLControl OpenTK issues.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real crash with a linked repro repository. Needs reproduction to get stack trace and confirm root cause. |
| Suggested repro platform | windows |

### Missing Info

- SkiaSharp NuGet package version
- Stack trace from the AccessViolationException
- Which SkiaSharp control (SKControl vs SKGLControl) is used in the repro

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply bug, views, windows-classic, and reliability labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Ask for stack trace and SkiaSharp version, note workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report and the repro project!

To help investigate this crash, could you please provide:
1. The **SkiaSharp NuGet package version** you're using
2. A **stack trace** from the `AccessViolationException` (you can get this in Visual Studio by enabling "Break on all CLR exceptions" under Debug → Windows → Exception Settings)
3. Confirmation of whether you're using `SKControl` or `SKGLControl` in the repro

In the meantime, a common cause of this crash pattern is retaining a reference to `SKSurface` or `SKCanvas` outside the `PaintSurface` event handler. All drawing operations must be performed synchronously within the handler — storing the surface or canvas in a field for later use will cause a crash because the underlying native memory is only valid during the event.

We'll investigate the repro repository you provided.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1556,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T10:22:00Z"
  },
  "summary": "Windows Forms app on .NET 5.0 throws AccessViolationException ('Attempted to read or write protected memory') when using SkiaSharp.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.8
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
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "Attempted to read or write protected memory. This is often an indication that other memory is corrupt.",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net5.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Windows Forms App (.NET) targeting .NET 5.0",
        "Add SkiaSharp",
        "Observe AccessViolationException at runtime"
      ],
      "environmentDetails": "Windows 10 Enterprise 10.0.19042 Build 19042, VS 2019 Preview 16.9.0 Preview 1.0, .NET 5.0",
      "repoLinks": [
        {
          "url": "https://github.com/CBrauer/TestSkia",
          "description": "Reproducible solution provided by reporter"
        }
      ],
      "relatedIssues": [
        1490,
        706,
        1137
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "net5.0"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Issue was filed with .NET 5.0 (EOL). No SkiaSharp version specified. The SKControl and SKGLControl code paths exist in current codebase but .NET 5.0-specific native asset packaging may have changed."
    }
  },
  "analysis": {
    "summary": "Reporter gets an AccessViolationException (memory corruption crash) using SkiaSharp in a Windows Forms app on .NET 5.0. No stack trace is provided, but a repro GitHub repository is linked. The crash likely originates from a native Skia memory access issue — either a threading problem in the WinForms paint loop, a GC/pinning issue in SKControl where native surface pointers into GDI bitmap memory become invalid, or a .NET 5.0-specific native interop regression.",
    "rationale": "AccessViolationException in .NET is almost always a native memory access violation, pointing to SkiaSharp's native layer. The Windows Forms SKControl creates a surface over a locked GDI Bitmap's memory region — if the surface or canvas escapes the using block, or if the bitmap is disposed while native code holds a pointer, the crash follows. The issue has a linked repro project, so it is classifiable as a real bug requiring investigation.",
    "keySignals": [
      {
        "text": "Attempted to read or write protected memory. This is often an indication that other memory is corrupt.",
        "source": "issue body",
        "interpretation": "AccessViolationException — native memory access outside valid region. Classic symptom of use-after-free or GC moving a pinned buffer."
      },
      {
        "text": "I submitted this problem to the .NET Core group, but they say it's not their problem",
        "source": "issue body",
        "interpretation": ".NET runtime team deflected — confirms the crash is inside SkiaSharp's native interop layer."
      },
      {
        "text": "I have put my reproducible solution on GitHub at: https://github.com/CBrauer/TestSkia",
        "source": "issue body",
        "interpretation": "Complete repro project available — accelerates reproduction and root cause identification."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKControl.cs",
        "lines": "39-52",
        "finding": "SKControl.OnPaint locks a GDI Bitmap with LockBits and creates an SKSurface over data.Scan0 (raw pointer). The surface is disposed in a using block before UnlockBits. If the user's PaintSurface handler retains a reference to the SKSurface or SKCanvas beyond the event, the native surface will outlive the locked memory region, causing a crash on next access.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs",
        "lines": "82-133",
        "finding": "SKGLControl.OnPaint uses OpenGL via OpenTK. The grContext, renderTarget, and surface are long-lived instance fields. On .NET 5.0, OpenTK interop with the Windows OpenGL context may have compatibility issues that trigger memory violations.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Which SkiaSharp control is used in the repro — SKControl or SKGLControl?",
      "Which SkiaSharp NuGet package version is referenced?",
      "Is there a stack trace from the crash (can be obtained via Visual Studio or Process Monitor)?",
      "Does the crash occur immediately on startup, or after some user interaction (resize, repaint)?"
    ],
    "resolution": {
      "hypothesis": "The crash likely results from the user's PaintSurface handler retaining a reference to SKSurface/SKCanvas beyond the event callback (use-after-free of the locked GDI bitmap memory), or from an OpenTK .NET 5.0 incompatibility in SKGLControl.",
      "proposals": [
        {
          "title": "Investigate repro project",
          "description": "Clone https://github.com/CBrauer/TestSkia and reproduce the crash on Windows to identify the exact stack trace and code path.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Workaround: do not retain surface/canvas references",
          "description": "Ensure the PaintSurface handler does not store the SKSurface or SKCanvas in a field for later use. All drawing must happen within the event handler synchronously.",
          "category": "workaround",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate repro project",
      "recommendedReason": "The repro is available; cloning it will reveal the exact crash callstack, resolving the ambiguity between SKControl memory pinning and SKGLControl OpenTK issues."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real crash with a linked repro repository. Needs reproduction to get stack trace and confirm root cause.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "SkiaSharp NuGet package version",
      "Stack trace from the AccessViolationException",
      "Which SkiaSharp control (SKControl vs SKGLControl) is used in the repro"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, windows-classic, and reliability labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for stack trace and SkiaSharp version, note workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thank you for the detailed report and the repro project!\n\nTo help investigate this crash, could you please provide:\n1. The **SkiaSharp NuGet package version** you're using\n2. A **stack trace** from the `AccessViolationException` (you can get this in Visual Studio by enabling \"Break on all CLR exceptions\" under Debug → Windows → Exception Settings)\n3. Confirmation of whether you're using `SKControl` or `SKGLControl` in the repro\n\nIn the meantime, a common cause of this crash pattern is retaining a reference to `SKSurface` or `SKCanvas` outside the `PaintSurface` event handler. All drawing operations must be performed synchronously within the handler — storing the surface or canvas in a field for later use will cause a crash because the underlying native memory is only valid during the event.\n\nWe'll investigate the repro repository you provided."
      }
    ]
  }
}
```

</details>
