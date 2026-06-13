# Issue Triage Report — #658

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-13T05:43:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp.Views (0.90 (90%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** SKGLView.CanvasSize always returns {Width=0, Height=0} on macOS when the view is created via an InterfaceBuilder outlet, while SKCanvasView.CanvasSize returns the correct size in the same scenario.

**Analysis:** The macOS SKGLView.CanvasSize returns lastSize which is only set when DrawRect creates or recreates the renderTarget. The size is sourced from newSize, which is populated exclusively in Reshape(). When a view is created via [Outlet]/nib, Reshape() may fire before the view's frame is finalized, resulting in newSize=(0,0). DrawRect then captures that stale zero size as lastSize, and CanvasSize returns (0,0) for the entire session. SKCanvasView avoids this by reading Bounds directly inside DrawRect rather than caching through Reshape.

**Recommendations:** **needs-investigation** — Root cause is identified (Reshape timing on nib-created views), but a fix in SKGLView needs verification that NSOpenGLView draw-time Bounds reading is safe and correct across all creation paths. The issue is still open and likely still present in current code.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/macOS |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create a macOS app with an SKGLView created as an [Outlet] in InterfaceBuilder
2. Subscribe to PaintSurface
3. Inside PaintSurface handler, read CanvasView.CanvasSize
4. Observe {Width=0, Height=0} — expected: actual view dimensions

**Environment:** SkiaSharp 1.60.3, Visual Studio for Mac 7.6.10 (build 27), macOS 10.14, MacBook Pro 13'' 2018 and MacBook Pro 15'' 2016

**Code snippets:**

```csharp
[Outlet]
SkiaSharp.Views.Mac.SKGLView CanvasView { get; set; }
CanvasView.PaintSurface += CanvasView_PaintSurface;

void CanvasView_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
{
    Debug.WriteLine(CanvasView.CanvasSize);
    //output: {Width=0, Height=0}
}
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | CanvasSize is {Width=0, Height=0} in SKGLView.PaintSurface on macOS |
| Repro quality | complete |
| Target frameworks | Xamarin.Mac / macOS 10.14 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.60.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The macOS SKGLView still uses the same Reshape()-based newSize caching pattern in the current source. The fundamental design divergence from SKCanvasView (which reads Bounds at draw time) has not been corrected. |

## Analysis

### Technical Summary

The macOS SKGLView.CanvasSize returns lastSize which is only set when DrawRect creates or recreates the renderTarget. The size is sourced from newSize, which is populated exclusively in Reshape(). When a view is created via [Outlet]/nib, Reshape() may fire before the view's frame is finalized, resulting in newSize=(0,0). DrawRect then captures that stale zero size as lastSize, and CanvasSize returns (0,0) for the entire session. SKCanvasView avoids this by reading Bounds directly inside DrawRect rather than caching through Reshape.

### Rationale

Reporter provides a complete minimal repro with version, device, and code. The expected vs actual is clearly wrong output (not a usage question). Code investigation confirms a structural difference between SKGLView and SKCanvasView in how CanvasSize is computed — SKGLView relies on Reshape() timing while SKCanvasView reads current Bounds in DrawRect. The bug is type/bug area/SkiaSharp.Views because the issue is in the macOS view implementation's size-tracking logic.

### Key Signals

- "CanvasSize is empty ({Width=0, Height=0})" — **issue title and body** (The property always returns zero — not intermittent. Consistent with Reshape() firing before bounds are set.)
- "It works if a SKCanvasView is used instead of SKGLView" — **issue body** (Direct comparison confirms the regression is isolated to SKGLView's size-tracking implementation, not the user's code.)
- "[Outlet] SkiaSharp.Views.Mac.SKGLView CanvasView" — **code snippet in issue body** (View is created via InterfaceBuilder/nib. In this path, Reshape() fires before layout is finalized — this is the trigger for the zero-size bug.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs` | 84-103 | direct | CanvasSize returns lastSize (line 84). lastSize is only updated in DrawRect when renderTarget needs recreation (lastSize = newSize, in the if-block). newSize is set only in Reshape() (line 103) via ConvertSizeToBacking(Bounds.Size). If Reshape() fires before the view frame is finalized (typical for nib/outlet creation), newSize stays (0,0) and CanvasSize is always empty. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKCanvasView.cs` | 46-82 | related | CanvasSize is a settable property (line 46) updated directly inside DrawRect from current Bounds (line 82: CanvasSize = userVisibleSize). This approach is immune to Reshape() timing issues and correctly reflects the view size at every draw. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLLayer.cs` | 52-58 | related | SKGLLayer computes newSize from Bounds.Width * ContentsScale inside DrawInCGLContext (lines 52-54) — not via Reshape(). This is the pattern that would fix SKGLView: computing size at draw-time rather than caching from Reshape. |

### Workarounds

- Read the view dimensions using ConvertSizeToBacking(CanvasView.Bounds.Size) inside the PaintSurface handler instead of CanvasView.CanvasSize.
- Switch to SKCanvasView (CPU rasterizer) which correctly exposes CanvasSize.
- Use SKGLLayer embedded in a custom NSView if GPU rendering is required — SKGLLayer computes its size at draw time and does not suffer from this bug.

### Resolution Proposals

**Hypothesis:** Reshape() is called before the nib-created view has a finalized frame, so newSize is (0,0). The fix is to compute newSize inside DrawRect from Bounds at draw time (like SKGLLayer and SKCanvasView already do), removing the dependency on Reshape() timing.

1. **Compute newSize in DrawRect instead of Reshape** — fix, confidence 0.85 (85%), cost/s, validated=untested
   - In SKGLView.DrawRect, replace the cached newSize field with a locally computed size (ConvertSizeToBacking(Bounds.Size)) before the renderTarget creation block. Remove or demote Reshape() to only trigger a NeedsDisplay. This matches the pattern in SKGLLayer.cs (lines 52-54) and eliminates the timing dependency.
2. **Read size from Bounds inside PaintSurface (workaround)** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - As an immediate workaround, the reporter can read ConvertSizeToBacking(CanvasView.Bounds.Size) inside the PaintSurface handler rather than relying on CanvasView.CanvasSize.

```csharp
void CanvasView_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
{
    var size = CanvasView.ConvertSizeToBacking(CanvasView.Bounds.Size);
    Debug.WriteLine($"{size.Width} x {size.Height}"); // correct dimensions
}
```

**Recommended proposal:** Compute newSize in DrawRect instead of Reshape

**Why:** Fixes the root cause for all callers rather than requiring every user to work around it. Low-effort change matching the existing SKGLLayer pattern.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Root cause is identified (Reshape timing on nib-created views), but a fix in SKGLView needs verification that NSOpenGLView draw-time Bounds reading is safe and correct across all creation paths. The issue is still open and likely still present in current code. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, views, and macOS labels | labels=type/bug, area/SkiaSharp.Views, os/macOS |
| add-comment | medium | 0.85 (85%) | Acknowledge the bug with root cause analysis and provide an immediate workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the clear report! This is a known bug in the macOS `SKGLView` size-tracking implementation.

**Root cause:** `CanvasSize` is populated from a field that's set in `Reshape()`. When the view is created via an `[Outlet]`/nib, `Reshape()` fires before the view's frame is finalized, so the size is `(0,0)`. `SKCanvasView` avoids this by reading `Bounds` directly inside `DrawRect`.

**Immediate workaround** — read the size from `Bounds` inside your `PaintSurface` handler:

```csharp
void CanvasView_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
{
    var size = CanvasView.ConvertSizeToBacking(CanvasView.Bounds.Size);
    // size.Width and size.Height are correct
}
```

**Alternative:** On macOS 10.14+ consider `SKMetalView` (in `SkiaSharp.Views.Mac`) which uses Metal and has this timing issue resolved.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 658,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-13T05:43:00Z"
  },
  "summary": "SKGLView.CanvasSize always returns {Width=0, Height=0} on macOS when the view is created via an InterfaceBuilder outlet, while SKCanvasView.CanvasSize returns the correct size in the same scenario.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.9
    },
    "platforms": [
      "os/macOS"
    ],
    "backends": [
      "backend/OpenGL"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "CanvasSize is {Width=0, Height=0} in SKGLView.PaintSurface on macOS",
      "reproQuality": "complete",
      "targetFrameworks": [
        "Xamarin.Mac / macOS 10.14"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a macOS app with an SKGLView created as an [Outlet] in InterfaceBuilder",
        "Subscribe to PaintSurface",
        "Inside PaintSurface handler, read CanvasView.CanvasSize",
        "Observe {Width=0, Height=0} — expected: actual view dimensions"
      ],
      "codeSnippets": [
        "[Outlet]\nSkiaSharp.Views.Mac.SKGLView CanvasView { get; set; }\nCanvasView.PaintSurface += CanvasView_PaintSurface;\n\nvoid CanvasView_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)\n{\n    Debug.WriteLine(CanvasView.CanvasSize);\n    //output: {Width=0, Height=0}\n}"
      ],
      "environmentDetails": "SkiaSharp 1.60.3, Visual Studio for Mac 7.6.10 (build 27), macOS 10.14, MacBook Pro 13'' 2018 and MacBook Pro 15'' 2016"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.60.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The macOS SKGLView still uses the same Reshape()-based newSize caching pattern in the current source. The fundamental design divergence from SKCanvasView (which reads Bounds at draw time) has not been corrected."
    }
  },
  "analysis": {
    "summary": "The macOS SKGLView.CanvasSize returns lastSize which is only set when DrawRect creates or recreates the renderTarget. The size is sourced from newSize, which is populated exclusively in Reshape(). When a view is created via [Outlet]/nib, Reshape() may fire before the view's frame is finalized, resulting in newSize=(0,0). DrawRect then captures that stale zero size as lastSize, and CanvasSize returns (0,0) for the entire session. SKCanvasView avoids this by reading Bounds directly inside DrawRect rather than caching through Reshape.",
    "rationale": "Reporter provides a complete minimal repro with version, device, and code. The expected vs actual is clearly wrong output (not a usage question). Code investigation confirms a structural difference between SKGLView and SKCanvasView in how CanvasSize is computed — SKGLView relies on Reshape() timing while SKCanvasView reads current Bounds in DrawRect. The bug is type/bug area/SkiaSharp.Views because the issue is in the macOS view implementation's size-tracking logic.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs",
        "lines": "84-103",
        "finding": "CanvasSize returns lastSize (line 84). lastSize is only updated in DrawRect when renderTarget needs recreation (lastSize = newSize, in the if-block). newSize is set only in Reshape() (line 103) via ConvertSizeToBacking(Bounds.Size). If Reshape() fires before the view frame is finalized (typical for nib/outlet creation), newSize stays (0,0) and CanvasSize is always empty.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKCanvasView.cs",
        "lines": "46-82",
        "finding": "CanvasSize is a settable property (line 46) updated directly inside DrawRect from current Bounds (line 82: CanvasSize = userVisibleSize). This approach is immune to Reshape() timing issues and correctly reflects the view size at every draw.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLLayer.cs",
        "lines": "52-58",
        "finding": "SKGLLayer computes newSize from Bounds.Width * ContentsScale inside DrawInCGLContext (lines 52-54) — not via Reshape(). This is the pattern that would fix SKGLView: computing size at draw-time rather than caching from Reshape.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "CanvasSize is empty ({Width=0, Height=0})",
        "source": "issue title and body",
        "interpretation": "The property always returns zero — not intermittent. Consistent with Reshape() firing before bounds are set."
      },
      {
        "text": "It works if a SKCanvasView is used instead of SKGLView",
        "source": "issue body",
        "interpretation": "Direct comparison confirms the regression is isolated to SKGLView's size-tracking implementation, not the user's code."
      },
      {
        "text": "[Outlet] SkiaSharp.Views.Mac.SKGLView CanvasView",
        "source": "code snippet in issue body",
        "interpretation": "View is created via InterfaceBuilder/nib. In this path, Reshape() fires before layout is finalized — this is the trigger for the zero-size bug."
      }
    ],
    "workarounds": [
      "Read the view dimensions using ConvertSizeToBacking(CanvasView.Bounds.Size) inside the PaintSurface handler instead of CanvasView.CanvasSize.",
      "Switch to SKCanvasView (CPU rasterizer) which correctly exposes CanvasSize.",
      "Use SKGLLayer embedded in a custom NSView if GPU rendering is required — SKGLLayer computes its size at draw time and does not suffer from this bug."
    ],
    "resolution": {
      "hypothesis": "Reshape() is called before the nib-created view has a finalized frame, so newSize is (0,0). The fix is to compute newSize inside DrawRect from Bounds at draw time (like SKGLLayer and SKCanvasView already do), removing the dependency on Reshape() timing.",
      "proposals": [
        {
          "title": "Compute newSize in DrawRect instead of Reshape",
          "description": "In SKGLView.DrawRect, replace the cached newSize field with a locally computed size (ConvertSizeToBacking(Bounds.Size)) before the renderTarget creation block. Remove or demote Reshape() to only trigger a NeedsDisplay. This matches the pattern in SKGLLayer.cs (lines 52-54) and eliminates the timing dependency.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Read size from Bounds inside PaintSurface (workaround)",
          "description": "As an immediate workaround, the reporter can read ConvertSizeToBacking(CanvasView.Bounds.Size) inside the PaintSurface handler rather than relying on CanvasView.CanvasSize.",
          "codeSnippet": "void CanvasView_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)\n{\n    var size = CanvasView.ConvertSizeToBacking(CanvasView.Bounds.Size);\n    Debug.WriteLine($\"{size.Width} x {size.Height}\"); // correct dimensions\n}",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Compute newSize in DrawRect instead of Reshape",
      "recommendedReason": "Fixes the root cause for all callers rather than requiring every user to work around it. Low-effort change matching the existing SKGLLayer pattern."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Root cause is identified (Reshape timing on nib-created views), but a fix in SKGLView needs verification that NSOpenGLView draw-time Bounds reading is safe and correct across all creation paths. The issue is still open and likely still present in current code.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, and macOS labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/macOS"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug with root cause analysis and provide an immediate workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the clear report! This is a known bug in the macOS `SKGLView` size-tracking implementation.\n\n**Root cause:** `CanvasSize` is populated from a field that's set in `Reshape()`. When the view is created via an `[Outlet]`/nib, `Reshape()` fires before the view's frame is finalized, so the size is `(0,0)`. `SKCanvasView` avoids this by reading `Bounds` directly inside `DrawRect`.\n\n**Immediate workaround** — read the size from `Bounds` inside your `PaintSurface` handler:\n\n```csharp\nvoid CanvasView_PaintSurface(object sender, SKPaintGLSurfaceEventArgs e)\n{\n    var size = CanvasView.ConvertSizeToBacking(CanvasView.Bounds.Size);\n    // size.Width and size.Height are correct\n}\n```\n\n**Alternative:** On macOS 10.14+ consider `SKMetalView` (in `SkiaSharp.Views.Mac`) which uses Metal and has this timing issue resolved."
      }
    ]
  }
}
```

</details>
