# Issue Triage Report — #2837

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T19:25:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKGLElement (WPF OpenGL control based on GLWpfControl) renders a black screen on machines with Intel integrated graphics due to Intel driver incompatibility with the WGL_NV_DX_interop extension used by GLWpfControl.

**Analysis:** SKGLElement inherits from GLWpfControl (OpenTK), which uses the WGL_NV_DX_interop extension to share D3D9/OpenGL surfaces in WPF. Intel integrated graphics drivers newer than version 15.40.21.4416 (circa March 2016) have a broken implementation of this extension, causing the interop to silently fail and produce a black screen. SkiaSharp's SKGLElement.cs creates a GRContext via GRGlInterface.Create() inside the GLWpfControl Render callback, but the interop failure means no output is visible. SkiaSharp currently depends on OpenTK.GLWpfControl 4.2.3; community comments indicate version 4.3.3 includes fixes (PR #131) for some of these Intel-specific WGL issues.

**Recommendations:** **needs-investigation** — The root cause points to a GLWpfControl dependency version that is known to have Intel driver compatibility issues. Updating to GLWpfControl 4.3.3 is the likely fix, but needs to be verified. The issue is real and reproducible on Intel integrated graphics hardware.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Windows 11, HP Mini PC with Intel HDG 4600, Visual Studio (Windows)

**Repository links:**
- https://github.com/opentk/GLWpfControl/issues/73 — GLWpfControl black screen issue on Intel graphics
- https://github.com/opentk/GLWpfControl/pull/131 — GLWpfControl PR fixing Intel WGL interop issues
- https://github.com/opentk/opentk/issues/1788 — OpenTK Intel driver compatibility issue
- https://github.com/opentk/GLWpfControl/issues/149 — GLWpfControl Intel driver issues
- https://github.com/opentk/GLWpfControl/issues/138 — GLWpfControl Intel-related rendering issue
- https://github.com/opentk/GLWpfControl/issues/130 — GLWpfControl black screen reports
- https://github.com/opentk/GLWpfControl/issues/128 — GLWpfControl black screen reports

**Code snippets:**

```csharp
e.Surface.Canvas.Clear(new SKColor(255, 0, 0));
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net3.x-alpha |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.x (Alpha), 2.88.2 |
| Worked in | 2.88.2 |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SkiaSharp.Views.WPF still uses OpenTK.GLWpfControl 4.2.3 which has the Intel driver incompatibility; upgrading to 4.3.3 may address it |

## Analysis

### Technical Summary

SKGLElement inherits from GLWpfControl (OpenTK), which uses the WGL_NV_DX_interop extension to share D3D9/OpenGL surfaces in WPF. Intel integrated graphics drivers newer than version 15.40.21.4416 (circa March 2016) have a broken implementation of this extension, causing the interop to silently fail and produce a black screen. SkiaSharp's SKGLElement.cs creates a GRContext via GRGlInterface.Create() inside the GLWpfControl Render callback, but the interop failure means no output is visible. SkiaSharp currently depends on OpenTK.GLWpfControl 4.2.3; community comments indicate version 4.3.3 includes fixes (PR #131) for some of these Intel-specific WGL issues.

### Rationale

Classified as type/bug in area/SkiaSharp.Views because the defect is reproducible and traceable: SkiaSharp ships GLWpfControl 4.2.3 which has known Intel driver compatibility bugs. The root cause is in the OpenTK/GLWpfControl dependency, but SkiaSharp can address it by updating to 4.3.3. Severity is medium because it affects a specific hardware class (Intel integrated graphics) and a workaround exists (use SKGLControl via WinFormsHost). suggestedAction is needs-investigation to confirm whether upgrading GLWpfControl resolves the issue.

### Key Signals

- "works fine on a machine with an Nvidia or AMD graphic card. But, when tested on a machine with integrated Intel graphics only (or the other is disabled), it does not work" — **issue body** (Hardware-specific rendering failure points to GPU driver/interop compatibility, not a SkiaSharp core bug)
- "When using WinFormsHost with SKGLControl, it works just fine" — **issue body** (SKGLControl (WinForms) uses a different WGL path without D3D interop, bypassing the Intel driver issue)
- "Driver Version 15.40.21.4416 (03/23/2016) - No [working]" — **comment by romen-h** (Precise driver bisect confirms this is an Intel driver regression starting in early 2016, not a SkiaSharp version regression)
- "I would suggest skia update to the latest 4.3.3 version of GLWpfControl and see if that fixes the problem" — **comment by Xerxes004** (GLWpfControl 4.3.3 contains fixes for Intel WGL_NV_DX_interop issues; SkiaSharp currently ships 4.2.3)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs` | 29-65 | direct | SKGLElement inherits from GLWpfControl and creates a GRContext using GRGlInterface.Create() on the Render callback. It sets MajorVersion=2, MinorVersion=1 for the GL context. The GL rendering pipeline is functional in SkiaSharp code; the black screen issue is caused by the underlying WGL_NV_DX_interop surface sharing which GLWpfControl manages. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj` | 17-20 | direct | The .csproj references OpenTK.GLWpfControl 4.2.3 (for .NET 5+) and OpenTK.GLWpfControl 3.3.0 (for .NET 4). GLWpfControl 4.3.3 is available with Intel interop fixes per community report; upgrading could resolve this issue. |

### Workarounds

- Use SKGLControl wrapped in WindowsFormsHost (WinFormsHost) instead of SKGLElement — avoids GLWpfControl's D3D interop path entirely
- Downgrade Intel graphics driver to version 15.40.16.4364 or earlier (not practical for most users)

### Next Questions

- Does upgrading to OpenTK.GLWpfControl 4.3.3 resolve the black screen on Intel HDG 4600?
- Are there any breaking API changes between GLWpfControl 4.2.3 and 4.3.3 that would require SKGLElement code changes?

### Resolution Proposals

**Hypothesis:** Upgrading OpenTK.GLWpfControl from 4.2.3 to 4.3.3 in SkiaSharp.Views.WPF.csproj would incorporate upstream fixes for Intel WGL_NV_DX_interop incompatibility and likely resolve the black screen.

1. **Use SKGLControl in WinFormsHost as temporary workaround** — workaround, cost/s, validated=untested
   - Wrap SKGLControl (WinForms) in a WindowsFormsHost in your WPF application. This avoids GLWpfControl's WGL_NV_DX_interop path and works on Intel integrated graphics.
2. **Upgrade OpenTK.GLWpfControl dependency to 4.3.3** — fix, cost/s, validated=untested
   - Update SkiaSharp.Views.WPF.csproj to use OpenTK.GLWpfControl 4.3.3 (and potentially a matching OpenTK version), which includes GLWpfControl PR #131 that fixes Intel WGL_NV_DX_interop compatibility issues.

**Recommended proposal:** Upgrade OpenTK.GLWpfControl dependency to 4.3.3

**Why:** A dependency version bump is low-risk and likely to fix this at the source. The workaround is available for users who cannot wait.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | The root cause points to a GLWpfControl dependency version that is known to have Intel driver compatibility issues. Updating to GLWpfControl 4.3.3 is the likely fix, but needs to be verified. The issue is real and reproducible on Intel integrated graphics hardware. |
| Suggested repro platform | windows |

### Missing Info

- Confirmation of whether updating GLWpfControl from 4.2.3 to 4.3.3 resolves the issue
- Stack trace or error log output (Relevant Log Output was left empty)
- Expected vs actual behavior description (both were left as '_No response_')

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Add area/SkiaSharp.Views, os/Windows-Classic, backend/OpenGL labels | labels=area/SkiaSharp.Views, os/Windows-Classic, backend/OpenGL |
| add-comment | medium | 0.82 (82%) | Acknowledge issue, provide WinFormsHost workaround, explain GLWpfControl version dependency angle | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the driver bisect from the community.

Here's a workaround you can use while we investigate: wrap `SKGLControl` (WinForms) in a `WindowsFormsHost` in your WPF project. This avoids `GLWpfControl`'s WGL_NV_DX_interop path entirely and is known to work on Intel integrated graphics.

```xml
<WindowsFormsHost>
    <wpf:SKGLControl x:Name="skglControl" PaintSurface="OnPaintSurface" />
</WindowsFormsHost>
```

The root issue is that `SKGLElement` is based on `GLWpfControl`, which uses the `WGL_NV_DX_interop` extension to share D3D9/OpenGL surfaces with WPF's compositor. Intel integrated graphics drivers newer than version 15.40.21.4416 (March 2016) have a broken implementation of this extension. SkiaSharp currently ships `OpenTK.GLWpfControl` 4.2.3; the community suggests version 4.3.3 includes fixes for these Intel-specific issues (see [GLWpfControl PR #131](https://github.com/opentk/GLWpfControl/pull/131)). We'll investigate updating the dependency.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2837,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T19:25:00Z"
  },
  "summary": "SKGLElement (WPF OpenGL control based on GLWpfControl) renders a black screen on machines with Intel integrated graphics due to Intel driver incompatibility with the WGL_NV_DX_interop extension used by GLWpfControl.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
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
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net3.x-alpha"
      ]
    },
    "reproEvidence": {
      "codeSnippets": [
        "e.Surface.Canvas.Clear(new SKColor(255, 0, 0));"
      ],
      "repoLinks": [
        {
          "url": "https://github.com/opentk/GLWpfControl/issues/73",
          "description": "GLWpfControl black screen issue on Intel graphics"
        },
        {
          "url": "https://github.com/opentk/GLWpfControl/pull/131",
          "description": "GLWpfControl PR fixing Intel WGL interop issues"
        },
        {
          "url": "https://github.com/opentk/opentk/issues/1788",
          "description": "OpenTK Intel driver compatibility issue"
        },
        {
          "url": "https://github.com/opentk/GLWpfControl/issues/149",
          "description": "GLWpfControl Intel driver issues"
        },
        {
          "url": "https://github.com/opentk/GLWpfControl/issues/138",
          "description": "GLWpfControl Intel-related rendering issue"
        },
        {
          "url": "https://github.com/opentk/GLWpfControl/issues/130",
          "description": "GLWpfControl black screen reports"
        },
        {
          "url": "https://github.com/opentk/GLWpfControl/issues/128",
          "description": "GLWpfControl black screen reports"
        }
      ],
      "environmentDetails": "Windows 11, HP Mini PC with Intel HDG 4600, Visual Studio (Windows)"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.x (Alpha)",
        "2.88.2"
      ],
      "workedIn": "2.88.2",
      "currentRelevance": "likely",
      "relevanceReason": "SkiaSharp.Views.WPF still uses OpenTK.GLWpfControl 4.2.3 which has the Intel driver incompatibility; upgrading to 4.3.3 may address it"
    }
  },
  "analysis": {
    "summary": "SKGLElement inherits from GLWpfControl (OpenTK), which uses the WGL_NV_DX_interop extension to share D3D9/OpenGL surfaces in WPF. Intel integrated graphics drivers newer than version 15.40.21.4416 (circa March 2016) have a broken implementation of this extension, causing the interop to silently fail and produce a black screen. SkiaSharp's SKGLElement.cs creates a GRContext via GRGlInterface.Create() inside the GLWpfControl Render callback, but the interop failure means no output is visible. SkiaSharp currently depends on OpenTK.GLWpfControl 4.2.3; community comments indicate version 4.3.3 includes fixes (PR #131) for some of these Intel-specific WGL issues.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SKGLElement.cs",
        "finding": "SKGLElement inherits from GLWpfControl and creates a GRContext using GRGlInterface.Create() on the Render callback. It sets MajorVersion=2, MinorVersion=1 for the GL context. The GL rendering pipeline is functional in SkiaSharp code; the black screen issue is caused by the underlying WGL_NV_DX_interop surface sharing which GLWpfControl manages.",
        "relevance": "direct",
        "lines": "29-65"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj",
        "finding": "The .csproj references OpenTK.GLWpfControl 4.2.3 (for .NET 5+) and OpenTK.GLWpfControl 3.3.0 (for .NET 4). GLWpfControl 4.3.3 is available with Intel interop fixes per community report; upgrading could resolve this issue.",
        "relevance": "direct",
        "lines": "17-20"
      }
    ],
    "keySignals": [
      {
        "text": "works fine on a machine with an Nvidia or AMD graphic card. But, when tested on a machine with integrated Intel graphics only (or the other is disabled), it does not work",
        "source": "issue body",
        "interpretation": "Hardware-specific rendering failure points to GPU driver/interop compatibility, not a SkiaSharp core bug"
      },
      {
        "text": "When using WinFormsHost with SKGLControl, it works just fine",
        "source": "issue body",
        "interpretation": "SKGLControl (WinForms) uses a different WGL path without D3D interop, bypassing the Intel driver issue"
      },
      {
        "text": "Driver Version 15.40.21.4416 (03/23/2016) - No [working]",
        "source": "comment by romen-h",
        "interpretation": "Precise driver bisect confirms this is an Intel driver regression starting in early 2016, not a SkiaSharp version regression"
      },
      {
        "text": "I would suggest skia update to the latest 4.3.3 version of GLWpfControl and see if that fixes the problem",
        "source": "comment by Xerxes004",
        "interpretation": "GLWpfControl 4.3.3 contains fixes for Intel WGL_NV_DX_interop issues; SkiaSharp currently ships 4.2.3"
      }
    ],
    "rationale": "Classified as type/bug in area/SkiaSharp.Views because the defect is reproducible and traceable: SkiaSharp ships GLWpfControl 4.2.3 which has known Intel driver compatibility bugs. The root cause is in the OpenTK/GLWpfControl dependency, but SkiaSharp can address it by updating to 4.3.3. Severity is medium because it affects a specific hardware class (Intel integrated graphics) and a workaround exists (use SKGLControl via WinFormsHost). suggestedAction is needs-investigation to confirm whether upgrading GLWpfControl resolves the issue.",
    "resolution": {
      "hypothesis": "Upgrading OpenTK.GLWpfControl from 4.2.3 to 4.3.3 in SkiaSharp.Views.WPF.csproj would incorporate upstream fixes for Intel WGL_NV_DX_interop incompatibility and likely resolve the black screen.",
      "proposals": [
        {
          "title": "Use SKGLControl in WinFormsHost as temporary workaround",
          "description": "Wrap SKGLControl (WinForms) in a WindowsFormsHost in your WPF application. This avoids GLWpfControl's WGL_NV_DX_interop path and works on Intel integrated graphics.",
          "category": "workaround",
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Upgrade OpenTK.GLWpfControl dependency to 4.3.3",
          "description": "Update SkiaSharp.Views.WPF.csproj to use OpenTK.GLWpfControl 4.3.3 (and potentially a matching OpenTK version), which includes GLWpfControl PR #131 that fixes Intel WGL_NV_DX_interop compatibility issues.",
          "category": "fix",
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Upgrade OpenTK.GLWpfControl dependency to 4.3.3",
      "recommendedReason": "A dependency version bump is low-risk and likely to fix this at the source. The workaround is available for users who cannot wait."
    },
    "workarounds": [
      "Use SKGLControl wrapped in WindowsFormsHost (WinFormsHost) instead of SKGLElement — avoids GLWpfControl's D3D interop path entirely",
      "Downgrade Intel graphics driver to version 15.40.16.4364 or earlier (not practical for most users)"
    ],
    "nextQuestions": [
      "Does upgrading to OpenTK.GLWpfControl 4.3.3 resolve the black screen on Intel HDG 4600?",
      "Are there any breaking API changes between GLWpfControl 4.2.3 and 4.3.3 that would require SKGLElement code changes?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "The root cause points to a GLWpfControl dependency version that is known to have Intel driver compatibility issues. Updating to GLWpfControl 4.3.3 is the likely fix, but needs to be verified. The issue is real and reproducible on Intel integrated graphics hardware.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Confirmation of whether updating GLWpfControl from 4.2.3 to 4.3.3 resolves the issue",
      "Stack trace or error log output (Relevant Log Output was left empty)",
      "Expected vs actual behavior description (both were left as '_No response_')"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Add area/SkiaSharp.Views, os/Windows-Classic, backend/OpenGL labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "area/SkiaSharp.Views",
          "os/Windows-Classic",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge issue, provide WinFormsHost workaround, explain GLWpfControl version dependency angle",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report and the driver bisect from the community.\n\nHere's a workaround you can use while we investigate: wrap `SKGLControl` (WinForms) in a `WindowsFormsHost` in your WPF project. This avoids `GLWpfControl`'s WGL_NV_DX_interop path entirely and is known to work on Intel integrated graphics.\n\n```xml\n<WindowsFormsHost>\n    <wpf:SKGLControl x:Name=\"skglControl\" PaintSurface=\"OnPaintSurface\" />\n</WindowsFormsHost>\n```\n\nThe root issue is that `SKGLElement` is based on `GLWpfControl`, which uses the `WGL_NV_DX_interop` extension to share D3D9/OpenGL surfaces with WPF's compositor. Intel integrated graphics drivers newer than version 15.40.21.4416 (March 2016) have a broken implementation of this extension. SkiaSharp currently ships `OpenTK.GLWpfControl` 4.2.3; the community suggests version 4.3.3 includes fixes for these Intel-specific issues (see [GLWpfControl PR #131](https://github.com/opentk/GLWpfControl/pull/131)). We'll investigate updating the dependency."
      }
    ]
  }
}
```

</details>
