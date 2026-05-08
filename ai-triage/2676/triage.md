# Issue Triage Report — #2676

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T12:20:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** SKGLControl throws NullReferenceException in OnPaint on systems without OpenGL support because GRContext.CreateGl() returns null but is not null-checked before use.

**Analysis:** When GRContext.CreateGl(glInterface) is called on a system without GL support it returns null, but the code immediately calls grContext.GetMaxSurfaceSampleCount(colorType) on line 103 without checking for null, producing a NullReferenceException. There is also no way for callers to detect GL unavailability before constructing SKGLControl.

**Recommendations:** **needs-investigation** — Bug is confirmed by code inspection — grContext null after CreateGl() is not guarded. Root cause is clear; fix path is straightforward but needs a repro to confirm behavior on a no-GL system.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Windows-Classic, area/SkiaSharp.Views, backend/OpenGL |

## Evidence

### Reproduction

1. Create a Windows Forms app with SKGLControl
2. Run on a system that does not support OpenGL (e.g., a VM or system without GL drivers)
3. Observe NullReferenceException when OnPaint is called

**Environment:** Windows, SkiaSharp 2.88.3, Visual Studio

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | Object reference not set to an instance of an object in SKGLControl.OnPaint |
| Repro quality | partial |
| Target frameworks | — |

**Stack trace:**

```text
at SkiaSharp.Views.Desktop.SKGLControl.OnPaint(PaintEventArgs e)
at System.Windows.Forms.Control.PaintWithErrorHandling(PaintEventArgs e, Int16 layer)
at System.Windows.Forms.Control.WmPaint(Message& m)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The OnPaint code in SKGLControl.cs still lacks a null check for grContext after GRContext.CreateGl() — the bug is present in the current source. |

## Analysis

### Technical Summary

When GRContext.CreateGl(glInterface) is called on a system without GL support it returns null, but the code immediately calls grContext.GetMaxSurfaceSampleCount(colorType) on line 103 without checking for null, producing a NullReferenceException. There is also no way for callers to detect GL unavailability before constructing SKGLControl.

### Rationale

Stack trace pinpoints the crash to SKGLControl.OnPaint. Code inspection confirms that GRContext.CreateGl() can return null on systems without GL support and that the null is never checked before dereferencing grContext. The reporter's question about detection is secondary: the primary issue is that the control does not gracefully handle a missing GL context.

### Key Signals

- "Object reference not set to an instance of an object in SKGLControl.OnPaint" — **issue body** (grContext is null after GRContext.CreateGl() on a no-GL system; next dereference is grContext.GetMaxSurfaceSampleCount())
- "I expected exception when I create SKGLControl, not later... How can I detect if I have to create SKControl instead of SKGLControl?" — **issue body** (Reporter wants early failure or a detection API; the current failure is a confusing late NRE)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs` | 85-103 | direct | grContext is created via GRContext.CreateGl(glInterface) (line 88) which can return null when GL is unavailable. On the very next resize/first paint grContext.GetMaxSurfaceSampleCount(colorType) is called (line 103) without any null guard, causing NullReferenceException. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs` | 119-122 | related | SKSurface.Create(grContext, ...) is also called without null-checking grContext. If null, surface remains null and canvas = surface.Canvas (line 122) would NPE too. |

### Workarounds

- Wrap SKGLControl instantiation in a try/catch for the first paint cycle
- Create a GRGlInterface and GRContext manually before constructing SKGLControl to probe GL availability, then fall back to SKControl

### Next Questions

- Should SKGLControl expose a static/instance method to check GL availability before construction?
- Should a meaningful InvalidOperationException be thrown instead of silently returning?

### Resolution Proposals

**Hypothesis:** GRContext.CreateGl() returns null on systems without GL support; all subsequent uses of grContext should be guarded with a null check, and ideally a descriptive InvalidOperationException should be thrown when the context cannot be created.

1. **Add null check for grContext after creation** — fix, confidence 0.90 (90%), cost/xs, validated=untested
   - After calling GRContext.CreateGl(glInterface) in OnPaint, check if grContext is null and throw an InvalidOperationException with a descriptive message, or return early to prevent the NullReferenceException from propagating.
2. **Expose static GL availability check** — fix, confidence 0.75 (75%), cost/s, validated=untested
   - Add a static method or property (e.g., SKGLControl.IsSupported) that attempts to create a GRGlInterface and GRContext to probe for GL support, allowing callers to decide whether to use SKGLControl or SKControl at startup.

**Recommended proposal:** Add null check for grContext after creation

**Why:** Smallest safe change that converts the confusing NRE into a clear, actionable exception and prevents cascading null dereferences further down OnPaint.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Bug is confirmed by code inspection — grContext null after CreateGl() is not guarded. Root cause is clear; fix path is straightforward but needs a repro to confirm behavior on a no-GL system. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply existing labels (already present); add tenet/reliability | labels=type/bug, area/SkiaSharp.Views, os/Windows-Classic, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Acknowledge bug, explain root cause, provide workaround, and note planned fix | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The root cause is in `SKGLControl.OnPaint`: `GRContext.CreateGl()` returns `null` when OpenGL is not available on the host system, but the code immediately dereferences `grContext` without a null check, producing the `NullReferenceException`.

**Workaround (until a fix is released):**

You can probe for OpenGL support before constructing `SKGLControl`:

```csharp
private static bool IsOpenGLSupported()
{
    try
    {
        // Attempt to create a temporary GL context
        var glInterface = GRGlInterface.Create();
        if (glInterface == null) return false;
        using var ctx = GRContext.CreateGl(glInterface);
        return ctx != null;
    }
    catch
    {
        return false;
    }
}
```

Then in your form:
```csharp
Control skControl = IsOpenGLSupported() ? (Control)new SKGLControl() : new SKControl();
```

A proper fix will add a null guard in `OnPaint` so a clear `InvalidOperationException` is raised instead of a confusing NRE.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2676,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T12:20:00Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "area/SkiaSharp.Views",
      "backend/OpenGL"
    ]
  },
  "summary": "SKGLControl throws NullReferenceException in OnPaint on systems without OpenGL support because GRContext.CreateGl() returns null but is not null-checked before use.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.95
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
      "errorType": "exception",
      "errorMessage": "Object reference not set to an instance of an object in SKGLControl.OnPaint",
      "stackTrace": "at SkiaSharp.Views.Desktop.SKGLControl.OnPaint(PaintEventArgs e)\nat System.Windows.Forms.Control.PaintWithErrorHandling(PaintEventArgs e, Int16 layer)\nat System.Windows.Forms.Control.WmPaint(Message& m)",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Windows Forms app with SKGLControl",
        "Run on a system that does not support OpenGL (e.g., a VM or system without GL drivers)",
        "Observe NullReferenceException when OnPaint is called"
      ],
      "environmentDetails": "Windows, SkiaSharp 2.88.3, Visual Studio"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The OnPaint code in SKGLControl.cs still lacks a null check for grContext after GRContext.CreateGl() — the bug is present in the current source."
    }
  },
  "analysis": {
    "summary": "When GRContext.CreateGl(glInterface) is called on a system without GL support it returns null, but the code immediately calls grContext.GetMaxSurfaceSampleCount(colorType) on line 103 without checking for null, producing a NullReferenceException. There is also no way for callers to detect GL unavailability before constructing SKGLControl.",
    "rationale": "Stack trace pinpoints the crash to SKGLControl.OnPaint. Code inspection confirms that GRContext.CreateGl() can return null on systems without GL support and that the null is never checked before dereferencing grContext. The reporter's question about detection is secondary: the primary issue is that the control does not gracefully handle a missing GL context.",
    "keySignals": [
      {
        "text": "Object reference not set to an instance of an object in SKGLControl.OnPaint",
        "source": "issue body",
        "interpretation": "grContext is null after GRContext.CreateGl() on a no-GL system; next dereference is grContext.GetMaxSurfaceSampleCount()"
      },
      {
        "text": "I expected exception when I create SKGLControl, not later... How can I detect if I have to create SKControl instead of SKGLControl?",
        "source": "issue body",
        "interpretation": "Reporter wants early failure or a detection API; the current failure is a confusing late NRE"
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs",
        "lines": "85-103",
        "finding": "grContext is created via GRContext.CreateGl(glInterface) (line 88) which can return null when GL is unavailable. On the very next resize/first paint grContext.GetMaxSurfaceSampleCount(colorType) is called (line 103) without any null guard, causing NullReferenceException.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs",
        "lines": "119-122",
        "finding": "SKSurface.Create(grContext, ...) is also called without null-checking grContext. If null, surface remains null and canvas = surface.Canvas (line 122) would NPE too.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Should SKGLControl expose a static/instance method to check GL availability before construction?",
      "Should a meaningful InvalidOperationException be thrown instead of silently returning?"
    ],
    "workarounds": [
      "Wrap SKGLControl instantiation in a try/catch for the first paint cycle",
      "Create a GRGlInterface and GRContext manually before constructing SKGLControl to probe GL availability, then fall back to SKControl"
    ],
    "resolution": {
      "hypothesis": "GRContext.CreateGl() returns null on systems without GL support; all subsequent uses of grContext should be guarded with a null check, and ideally a descriptive InvalidOperationException should be thrown when the context cannot be created.",
      "proposals": [
        {
          "title": "Add null check for grContext after creation",
          "description": "After calling GRContext.CreateGl(glInterface) in OnPaint, check if grContext is null and throw an InvalidOperationException with a descriptive message, or return early to prevent the NullReferenceException from propagating.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Expose static GL availability check",
          "description": "Add a static method or property (e.g., SKGLControl.IsSupported) that attempts to create a GRGlInterface and GRContext to probe for GL support, allowing callers to decide whether to use SKGLControl or SKControl at startup.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add null check for grContext after creation",
      "recommendedReason": "Smallest safe change that converts the confusing NRE into a clear, actionable exception and prevents cascading null dereferences further down OnPaint."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Bug is confirmed by code inspection — grContext null after CreateGl() is not guarded. Root cause is clear; fix path is straightforward but needs a repro to confirm behavior on a no-GL system.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply existing labels (already present); add tenet/reliability",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge bug, explain root cause, provide workaround, and note planned fix",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the report! The root cause is in `SKGLControl.OnPaint`: `GRContext.CreateGl()` returns `null` when OpenGL is not available on the host system, but the code immediately dereferences `grContext` without a null check, producing the `NullReferenceException`.\n\n**Workaround (until a fix is released):**\n\nYou can probe for OpenGL support before constructing `SKGLControl`:\n\n```csharp\nprivate static bool IsOpenGLSupported()\n{\n    try\n    {\n        // Attempt to create a temporary GL context\n        var glInterface = GRGlInterface.Create();\n        if (glInterface == null) return false;\n        using var ctx = GRContext.CreateGl(glInterface);\n        return ctx != null;\n    }\n    catch\n    {\n        return false;\n    }\n}\n```\n\nThen in your form:\n```csharp\nControl skControl = IsOpenGLSupported() ? (Control)new SKGLControl() : new SKControl();\n```\n\nA proper fix will add a null guard in `OnPaint` so a clear `InvalidOperationException` is raised instead of a confusing NRE."
      }
    ]
  }
}
```

</details>
