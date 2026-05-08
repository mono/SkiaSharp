# Issue Triage Report — #2420

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T15:18:03Z |
| Type | type/question (0.82 (82%)) |
| Area | area/SkiaSharp.Views.Maui (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** Reporter encountered a Frame.NavigationFailed crash on Windows WinUI when using SKCanvasView without calling UseSkiaSharp() in MauiProgram.cs; reporter self-resolved and is asking if this setup requirement is documented.

**Analysis:** The crash is caused by missing .UseSkiaSharp() registration in MauiProgram.cs, a required setup step for MAUI apps using SKCanvasView. Without it, MAUI has no registered handler for SKCanvasView, so navigating to a page containing it fails. The reporter self-resolved and is asking for better docs. Documentation does exist in the official guides but may not be prominently linked from MAUI migration materials.

**Recommendations:** **close-as-not-a-bug** — The crash is caused by a missing required setup step (UseSkiaSharp()) that is documented. The reporter self-found the fix. This is by-design MAUI handler registration behavior, not a bug in SkiaSharp.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Windows-WinUI |
| Backends | — |
| Tenets | — |
| Partner | partner/maui |

## Evidence

### Reproduction

1. Create a .NET MAUI app targeting net7.0-windows10.0.19041.0
2. Add SKCanvasView to a page in XAML
3. Do NOT call .UseSkiaSharp() in MauiProgram.cs
4. Run the app on Windows and navigate to the page containing SKCanvasView
5. Observe: Microsoft.UI.Xaml.Controls.Frame.NavigationFailed was unhandled

**Environment:** Windows 11 Pro Version 10.0.22621, .NET 7 (net7.0-windows10.0.19041.0), SkiaSharp 2.88.3, Visual Studio 2022

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2139 — Related issue #2139 — same crash root cause (UseSkiaSharp not called in MAUI, same Frame.NavigationFailed on Windows)

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | UseSkiaSharp() registration requirement applies to all MAUI-based versions of SkiaSharp and has not changed. |

## Analysis

### Technical Summary

The crash is caused by missing .UseSkiaSharp() registration in MauiProgram.cs, a required setup step for MAUI apps using SKCanvasView. Without it, MAUI has no registered handler for SKCanvasView, so navigating to a page containing it fails. The reporter self-resolved and is asking for better docs. Documentation does exist in the official guides but may not be prominently linked from MAUI migration materials.

### Rationale

The behavior is by-design in MAUI — handler registration is required for custom views. The reporter self-resolved and found the answer. They are now asking if it is documented. The official docfx guide (documentation/docfx/guides/index.md) already has an [IMPORTANT] block about UseSkiaSharp(). This is a setup/onboarding question, not a defect in SkiaSharp itself.

### Key Signals

- "In MauiProgram.cs add .UseSkiaSharp() to the builder" — **comment #2 by reporter** (Reporter found the answer themselves — the crash was caused by missing UseSkiaSharp() handler registration.)
- "Is this documented anywhere? ... it would be nice to have it documented somewhere that's easy to find." — **comment #2 by reporter** (Reporter's actual ask is about documentation discoverability; the technical problem is resolved.)
- "This works fine on Xamarin.Forms." — **comment #1 by reporter** (Xamarin.Forms doesn't require explicit handler registration — MAUI's handler model is different, causing confusion for migrators.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/AppHostBuilderExtensions.cs` | 11-24 | direct | UseSkiaSharp() registers SKCanvasViewHandler and SKGLViewHandler via ConfigureMauiHandlers. Without this call, MAUI has no handler for SKCanvasView and crashes during navigation. |
| `documentation/docfx/guides/index.md` | 12-14 | related | An [!IMPORTANT] block explicitly states that UseSkiaSharp() must be called in MauiProgram.cs with the SkiaSharp.Views.Maui.Controls.Hosting namespace. Documentation exists but may not be discovered from MAUI migration guides. |

### Workarounds

- Add `.UseSkiaSharp()` call in MauiProgram.cs after `.UseMauiApp<App>()`. Requires `using SkiaSharp.Views.Maui.Controls.Hosting;` namespace. This registers the SKCanvasViewHandler and SKGLViewHandler with MAUI's handler infrastructure.

### Resolution Proposals

**Hypothesis:** The crash is entirely caused by missing UseSkiaSharp() handler registration. The reporter found the fix. Documentation exists at documentation/docfx/guides/index.md but is not prominently linked from MAUI migration docs.

1. **Answer with setup snippet and point to documentation** — workaround, confidence 0.88 (88%), cost/xs, validated=yes
   - Confirm that UseSkiaSharp() is a required MAUI setup step. Note that .NET 7 is EOL; recommend upgrading to .NET 9 with current SkiaSharp versions.

**Recommended proposal:** Answer with setup snippet and point to documentation

**Why:** The reporter self-resolved, the behavior is by-design, and official documentation exists. A clear response confirming the fix and pointing to docs closes the loop.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | The crash is caused by a missing required setup step (UseSkiaSharp()) that is documented. The reporter self-found the fix. This is by-design MAUI handler registration behavior, not a bug in SkiaSharp. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question, MAUI views, Windows WinUI, and partner/maui labels | labels=type/question, area/SkiaSharp.Views.Maui, os/Windows-WinUI, partner/maui |
| add-comment | medium | 0.88 (88%) | Confirm the fix, provide the setup snippet, and link to documentation | — |
| close-issue | medium | 0.85 (85%) | Close as not a bug — missing required setup step, reporter self-resolved | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for following up with the solution!

You're correct — `.UseSkiaSharp()` is required to register the SkiaSharp view handlers with MAUI's handler infrastructure. Without it, MAUI has no registered handler for `SKCanvasView`, so navigating to a page that contains one fails.

```csharp
// MauiProgram.cs
using SkiaSharp.Views.Maui.Controls.Hosting;

public static MauiApp CreateMauiApp() =>
    MauiApp
        .CreateBuilder()
        .UseMauiApp<App>()
        .UseSkiaSharp() // Required!
        .Build();
```

This requirement is documented in the [SkiaSharp Graphics in .NET MAUI](https://learn.microsoft.com/dotnet/api/skiasharp.views.maui.controls.hosting.apphostbuilderextensions.useskiasharp) guide with an important note at the top. You're right that it's easy to miss when migrating from Xamarin.Forms — handler registration is part of MAUI's architecture and wasn't required in Xamarin.Forms.

Note: .NET 7 has reached end-of-life. If you continue with this project, upgrading to .NET 9 and a current SkiaSharp version (3.x) is recommended.

Closing as not-a-bug since the behavior is by design. Feel free to re-open if you encounter other issues.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2420,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T15:18:03Z"
  },
  "summary": "Reporter encountered a Frame.NavigationFailed crash on Windows WinUI when using SKCanvasView without calling UseSkiaSharp() in MauiProgram.cs; reporter self-resolved and is asking if this setup requirement is documented.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-WinUI"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET MAUI app targeting net7.0-windows10.0.19041.0",
        "Add SKCanvasView to a page in XAML",
        "Do NOT call .UseSkiaSharp() in MauiProgram.cs",
        "Run the app on Windows and navigate to the page containing SKCanvasView",
        "Observe: Microsoft.UI.Xaml.Controls.Frame.NavigationFailed was unhandled"
      ],
      "environmentDetails": "Windows 11 Pro Version 10.0.22621, .NET 7 (net7.0-windows10.0.19041.0), SkiaSharp 2.88.3, Visual Studio 2022",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2139",
          "description": "Related issue #2139 — same crash root cause (UseSkiaSharp not called in MAUI, same Frame.NavigationFailed on Windows)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "UseSkiaSharp() registration requirement applies to all MAUI-based versions of SkiaSharp and has not changed."
    }
  },
  "analysis": {
    "summary": "The crash is caused by missing .UseSkiaSharp() registration in MauiProgram.cs, a required setup step for MAUI apps using SKCanvasView. Without it, MAUI has no registered handler for SKCanvasView, so navigating to a page containing it fails. The reporter self-resolved and is asking for better docs. Documentation does exist in the official guides but may not be prominently linked from MAUI migration materials.",
    "rationale": "The behavior is by-design in MAUI — handler registration is required for custom views. The reporter self-resolved and found the answer. They are now asking if it is documented. The official docfx guide (documentation/docfx/guides/index.md) already has an [IMPORTANT] block about UseSkiaSharp(). This is a setup/onboarding question, not a defect in SkiaSharp itself.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/AppHostBuilderExtensions.cs",
        "lines": "11-24",
        "finding": "UseSkiaSharp() registers SKCanvasViewHandler and SKGLViewHandler via ConfigureMauiHandlers. Without this call, MAUI has no handler for SKCanvasView and crashes during navigation.",
        "relevance": "direct"
      },
      {
        "file": "documentation/docfx/guides/index.md",
        "lines": "12-14",
        "finding": "An [!IMPORTANT] block explicitly states that UseSkiaSharp() must be called in MauiProgram.cs with the SkiaSharp.Views.Maui.Controls.Hosting namespace. Documentation exists but may not be discovered from MAUI migration guides.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "In MauiProgram.cs add .UseSkiaSharp() to the builder",
        "source": "comment #2 by reporter",
        "interpretation": "Reporter found the answer themselves — the crash was caused by missing UseSkiaSharp() handler registration."
      },
      {
        "text": "Is this documented anywhere? ... it would be nice to have it documented somewhere that's easy to find.",
        "source": "comment #2 by reporter",
        "interpretation": "Reporter's actual ask is about documentation discoverability; the technical problem is resolved."
      },
      {
        "text": "This works fine on Xamarin.Forms.",
        "source": "comment #1 by reporter",
        "interpretation": "Xamarin.Forms doesn't require explicit handler registration — MAUI's handler model is different, causing confusion for migrators."
      }
    ],
    "workarounds": [
      "Add `.UseSkiaSharp()` call in MauiProgram.cs after `.UseMauiApp<App>()`. Requires `using SkiaSharp.Views.Maui.Controls.Hosting;` namespace. This registers the SKCanvasViewHandler and SKGLViewHandler with MAUI's handler infrastructure."
    ],
    "resolution": {
      "hypothesis": "The crash is entirely caused by missing UseSkiaSharp() handler registration. The reporter found the fix. Documentation exists at documentation/docfx/guides/index.md but is not prominently linked from MAUI migration docs.",
      "proposals": [
        {
          "title": "Answer with setup snippet and point to documentation",
          "description": "Confirm that UseSkiaSharp() is a required MAUI setup step. Note that .NET 7 is EOL; recommend upgrading to .NET 9 with current SkiaSharp versions.",
          "category": "workaround",
          "confidence": 0.88,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Answer with setup snippet and point to documentation",
      "recommendedReason": "The reporter self-resolved, the behavior is by-design, and official documentation exists. A clear response confirming the fix and pointing to docs closes the loop."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "The crash is caused by a missing required setup step (UseSkiaSharp()) that is documented. The reporter self-found the fix. This is by-design MAUI handler registration behavior, not a bug in SkiaSharp.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, MAUI views, Windows WinUI, and partner/maui labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp.Views.Maui",
          "os/Windows-WinUI",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm the fix, provide the setup snippet, and link to documentation",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for following up with the solution!\n\nYou're correct — `.UseSkiaSharp()` is required to register the SkiaSharp view handlers with MAUI's handler infrastructure. Without it, MAUI has no registered handler for `SKCanvasView`, so navigating to a page that contains one fails.\n\n```csharp\n// MauiProgram.cs\nusing SkiaSharp.Views.Maui.Controls.Hosting;\n\npublic static MauiApp CreateMauiApp() =>\n    MauiApp\n        .CreateBuilder()\n        .UseMauiApp<App>()\n        .UseSkiaSharp() // Required!\n        .Build();\n```\n\nThis requirement is documented in the [SkiaSharp Graphics in .NET MAUI](https://learn.microsoft.com/dotnet/api/skiasharp.views.maui.controls.hosting.apphostbuilderextensions.useskiasharp) guide with an important note at the top. You're right that it's easy to miss when migrating from Xamarin.Forms — handler registration is part of MAUI's architecture and wasn't required in Xamarin.Forms.\n\nNote: .NET 7 has reached end-of-life. If you continue with this project, upgrading to .NET 9 and a current SkiaSharp version (3.x) is recommended.\n\nClosing as not-a-bug since the behavior is by design. Feel free to re-open if you encounter other issues."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — missing required setup step, reporter self-resolved",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
