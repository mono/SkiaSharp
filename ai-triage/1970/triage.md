# Issue Triage Report — #1970

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T22:10:00Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp.Views.Uno (0.90 (90%)) |
| Suggested action | needs-reproduction (0.80 (80%)) |

**Issue Summary:** SKXamlCanvas subclass in an Uno Platform project renders correctly on Android but draws nothing on the iOS simulator (iPhone 13, iOS 15.2), with SkiaSharp 2.88.0-preview.209.

**Analysis:** On iOS, SKXamlCanvas.Draw() guards on `drawable == null` and returns early if the backing SKCGSurfaceFactory was never initialized. The factory is initialized only in DoLoaded() which fires from the Loaded event. On Android, the SurfaceFactory is initialized directly in the constructor, making it unconditionally available. If Uno Platform on iOS does not fire the Loaded event for a custom SKXamlCanvas subclass before the first Draw() call, drawable stays null and nothing is rendered.

**Recommendations:** **needs-reproduction** — The issue has enough context to classify as a bug (platform asymmetry in surface factory initialization) but no minimal repro project was provided. Cannot confirm root cause without a runnable reproduction.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Uno |
| Platforms | os/iOS |
| Backends | — |
| Tenets | — |
| Partner | partner/unoplatform |

## Evidence

### Reproduction

1. Create a custom SKXamlCanvas subclass in an Uno Platform project
2. Wire the PaintSurface event (or override OnPaintSurface) to draw an image and some paths
3. Run the project on iOS simulator (iPhone 13, iOS 15.2)
4. Observe that nothing is drawn — canvas stays blue as defined in XAML

**Environment:** Visual Studio Community 2022 17.0.6, Xamarin iOS 15.4.0.0, SkiaSharp 2.88.0-preview.209, iPhone 13 Simulator, iOS 15.2

**Related issues:** #2107

**Repository links:**
- https://github.com/unoplatform/uno/issues/7255 — Uno platform issue referenced by reporter related to SKXamlCanvas rendering size on iOS and Android

**Code snippets:**

```csharp
Render method drawing SKImage and SKPath elements via SKCanvas; XAML shows MarkerCanvas with IgnorePixelScaling=True and Background=Blue
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | missing-output |
| Error message | Nothing is drawn on the canvas, it remains blue as set in XAML |
| Repro quality | partial |
| Target frameworks | ios15.2 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.0-preview.209 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Reported against a preview build; it is unknown whether the current stable release addressed the lifecycle difference between iOS and Android in SKXamlCanvas. |

## Analysis

### Technical Summary

On iOS, SKXamlCanvas.Draw() guards on `drawable == null` and returns early if the backing SKCGSurfaceFactory was never initialized. The factory is initialized only in DoLoaded() which fires from the Loaded event. On Android, the SurfaceFactory is initialized directly in the constructor, making it unconditionally available. If Uno Platform on iOS does not fire the Loaded event for a custom SKXamlCanvas subclass before the first Draw() call, drawable stays null and nothing is rendered.

### Rationale

The symptom (blue background unchanged, no output) matches a null drawable guard in the iOS Draw() path. Android initializes the factory in the constructor, iOS defers to Loaded. This lifecycle asymmetry is a plausible platform-specific bug in SkiaSharp.Views.Uno on iOS. No reproduction project was provided, so root cause cannot be confirmed.

### Key Signals

- "On my Android phone the canvas works fine, but on my iOS emulator nothing is drawn" — **issue body** (Platform asymmetry — Android initializes the surface factory unconditionally in constructor; iOS depends on Loaded event which may not fire reliably in Uno.)
- "it remains blue as set in xaml" — **issue body** (Canvas background is never cleared to white, matching the early-return path in Draw() when drawable is null.)
- "Reproduction Link (empty)" — **issue body** (No minimal repro project was provided, making it impossible to confirm the root cause.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI/SKXamlCanvas.AppleiOS.cs` | 18-29 | direct | SKCGSurfaceFactory 'drawable' is initialized only in DoLoaded(). Constructor does NOT create it. The Draw() method checks 'drawable == null' and returns early — so if Loaded event doesn't fire before first draw, nothing is rendered. |
| `source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI/SKXamlCanvas.Android.cs` | 19-23 | direct | Android SurfaceFactory is initialized directly in the constructor — not dependent on the Loaded event. This explains why Android works while iOS may not if Loaded fires late or not at all in Uno. |
| `source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Shared/SKXamlCanvas.cs` | 100-113 | related | OnLoaded increments loadUnloadCounter and calls DoLoaded() only when counter reaches 1. If Uno on iOS fires Loaded multiple times or not at all for custom subclasses, drawable may never be initialized. |

### Workarounds

- Ensure Invalidate() is called after the page is fully loaded via a delay or OnAppearing equivalent so Loaded fires before the first draw.
- Override DoLoaded partial method is not available externally; instead, manually call Invalidate() after wiring up the PaintSurface event.

### Next Questions

- Does subscribing to the PaintSurface event fire at all on iOS? (Confirms if DoLoaded / drawable is the issue)
- Does the stock SKXamlCanvas (non-subclassed) also fail to draw on iOS with Uno?
- What version of Uno Platform is used?
- Is there a minimal repro project that can be shared?

### Resolution Proposals

**Hypothesis:** The iOS implementation of SKXamlCanvas defers surface factory initialization to the Loaded event, whereas Android initializes it in the constructor. If Uno Platform on iOS does not consistently fire Loaded before the first draw cycle for custom subclasses, the guard `drawable == null` causes all drawing to be silently skipped.

1. **Initialize drawable in constructor on iOS (parity with Android)** — fix, confidence 0.65 (65%), cost/s, validated=untested
   - Move SKCGSurfaceFactory initialization from DoLoaded() into the constructor in SKXamlCanvas.AppleiOS.cs, matching the Android pattern. DoLoaded() would then be a no-op or handle other setup.
2. **Request minimal repro and Uno Platform version from reporter** — investigation, confidence 0.90 (90%), cost/xs, validated=untested
   - Ask reporter to provide: (1) Uno Platform NuGet version, (2) a minimal repro project, (3) confirmation whether a plain (non-subclassed) SKXamlCanvas also fails to draw on iOS.

**Recommended proposal:** Request minimal repro and Uno Platform version from reporter

**Why:** No repro project was provided. Before fixing, confirm if this is a SkiaSharp bug or an Uno lifecycle issue, and whether it still occurs in the current stable release.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-reproduction |
| Confidence | 0.80 (80%) |
| Reason | The issue has enough context to classify as a bug (platform asymmetry in surface factory initialization) but no minimal repro project was provided. Cannot confirm root cause without a runnable reproduction. |
| Suggested repro platform | macos |

### Missing Info

- A minimal repro project reproducing the issue
- Uno Platform NuGet package version
- Whether stock (non-subclassed) SKXamlCanvas also fails to draw on iOS

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, Uno views, iOS, and Uno partner labels | labels=type/bug, area/SkiaSharp.Views.Uno, os/iOS, partner/unoplatform |
| add-comment | medium | 0.85 (85%) | Ask reporter for minimal repro and additional info | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! We can see a potential cause: on iOS, `SKXamlCanvas` initializes its drawing surface only when the `Loaded` event fires, whereas on Android it's initialized in the constructor. If Uno Platform on iOS doesn't fire `Loaded` before the first draw, nothing gets rendered.

To confirm and fix this, we need:
1. A **minimal repro project** (the Reproduction Link is empty)
2. The **Uno Platform NuGet package version** you're using
3. Whether a **plain (non-subclassed) `SKXamlCanvas`** with a `PaintSurface` handler also fails to draw on iOS

Thank you!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1970,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T22:10:00Z"
  },
  "summary": "SKXamlCanvas subclass in an Uno Platform project renders correctly on Android but draws nothing on the iOS simulator (iPhone 13, iOS 15.2), with SkiaSharp 2.88.0-preview.209.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views.Uno",
      "confidence": 0.9
    },
    "platforms": [
      "os/iOS"
    ],
    "partner": "partner/unoplatform"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "missing-output",
      "errorMessage": "Nothing is drawn on the canvas, it remains blue as set in XAML",
      "reproQuality": "partial",
      "targetFrameworks": [
        "ios15.2"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a custom SKXamlCanvas subclass in an Uno Platform project",
        "Wire the PaintSurface event (or override OnPaintSurface) to draw an image and some paths",
        "Run the project on iOS simulator (iPhone 13, iOS 15.2)",
        "Observe that nothing is drawn — canvas stays blue as defined in XAML"
      ],
      "codeSnippets": [
        "Render method drawing SKImage and SKPath elements via SKCanvas; XAML shows MarkerCanvas with IgnorePixelScaling=True and Background=Blue"
      ],
      "environmentDetails": "Visual Studio Community 2022 17.0.6, Xamarin iOS 15.4.0.0, SkiaSharp 2.88.0-preview.209, iPhone 13 Simulator, iOS 15.2",
      "relatedIssues": [
        2107
      ],
      "repoLinks": [
        {
          "url": "https://github.com/unoplatform/uno/issues/7255",
          "description": "Uno platform issue referenced by reporter related to SKXamlCanvas rendering size on iOS and Android"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.0-preview.209"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Reported against a preview build; it is unknown whether the current stable release addressed the lifecycle difference between iOS and Android in SKXamlCanvas."
    }
  },
  "analysis": {
    "summary": "On iOS, SKXamlCanvas.Draw() guards on `drawable == null` and returns early if the backing SKCGSurfaceFactory was never initialized. The factory is initialized only in DoLoaded() which fires from the Loaded event. On Android, the SurfaceFactory is initialized directly in the constructor, making it unconditionally available. If Uno Platform on iOS does not fire the Loaded event for a custom SKXamlCanvas subclass before the first Draw() call, drawable stays null and nothing is rendered.",
    "rationale": "The symptom (blue background unchanged, no output) matches a null drawable guard in the iOS Draw() path. Android initializes the factory in the constructor, iOS defers to Loaded. This lifecycle asymmetry is a plausible platform-specific bug in SkiaSharp.Views.Uno on iOS. No reproduction project was provided, so root cause cannot be confirmed.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI/SKXamlCanvas.AppleiOS.cs",
        "lines": "18-29",
        "finding": "SKCGSurfaceFactory 'drawable' is initialized only in DoLoaded(). Constructor does NOT create it. The Draw() method checks 'drawable == null' and returns early — so if Loaded event doesn't fire before first draw, nothing is rendered.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI/SKXamlCanvas.Android.cs",
        "lines": "19-23",
        "finding": "Android SurfaceFactory is initialized directly in the constructor — not dependent on the Loaded event. This explains why Android works while iOS may not if Loaded fires late or not at all in Uno.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Uno/SkiaSharp.Views.Uno.WinUI.Shared/SKXamlCanvas.cs",
        "lines": "100-113",
        "finding": "OnLoaded increments loadUnloadCounter and calls DoLoaded() only when counter reaches 1. If Uno on iOS fires Loaded multiple times or not at all for custom subclasses, drawable may never be initialized.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "On my Android phone the canvas works fine, but on my iOS emulator nothing is drawn",
        "source": "issue body",
        "interpretation": "Platform asymmetry — Android initializes the surface factory unconditionally in constructor; iOS depends on Loaded event which may not fire reliably in Uno."
      },
      {
        "text": "it remains blue as set in xaml",
        "source": "issue body",
        "interpretation": "Canvas background is never cleared to white, matching the early-return path in Draw() when drawable is null."
      },
      {
        "text": "Reproduction Link (empty)",
        "source": "issue body",
        "interpretation": "No minimal repro project was provided, making it impossible to confirm the root cause."
      }
    ],
    "nextQuestions": [
      "Does subscribing to the PaintSurface event fire at all on iOS? (Confirms if DoLoaded / drawable is the issue)",
      "Does the stock SKXamlCanvas (non-subclassed) also fail to draw on iOS with Uno?",
      "What version of Uno Platform is used?",
      "Is there a minimal repro project that can be shared?"
    ],
    "workarounds": [
      "Ensure Invalidate() is called after the page is fully loaded via a delay or OnAppearing equivalent so Loaded fires before the first draw.",
      "Override DoLoaded partial method is not available externally; instead, manually call Invalidate() after wiring up the PaintSurface event."
    ],
    "resolution": {
      "hypothesis": "The iOS implementation of SKXamlCanvas defers surface factory initialization to the Loaded event, whereas Android initializes it in the constructor. If Uno Platform on iOS does not consistently fire Loaded before the first draw cycle for custom subclasses, the guard `drawable == null` causes all drawing to be silently skipped.",
      "proposals": [
        {
          "title": "Initialize drawable in constructor on iOS (parity with Android)",
          "description": "Move SKCGSurfaceFactory initialization from DoLoaded() into the constructor in SKXamlCanvas.AppleiOS.cs, matching the Android pattern. DoLoaded() would then be a no-op or handle other setup.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Request minimal repro and Uno Platform version from reporter",
          "description": "Ask reporter to provide: (1) Uno Platform NuGet version, (2) a minimal repro project, (3) confirmation whether a plain (non-subclassed) SKXamlCanvas also fails to draw on iOS.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Request minimal repro and Uno Platform version from reporter",
      "recommendedReason": "No repro project was provided. Before fixing, confirm if this is a SkiaSharp bug or an Uno lifecycle issue, and whether it still occurs in the current stable release."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-reproduction",
      "confidence": 0.8,
      "reason": "The issue has enough context to classify as a bug (platform asymmetry in surface factory initialization) but no minimal repro project was provided. Cannot confirm root cause without a runnable reproduction.",
      "suggestedReproPlatform": "macos"
    },
    "missingInfo": [
      "A minimal repro project reproducing the issue",
      "Uno Platform NuGet package version",
      "Whether stock (non-subclassed) SKXamlCanvas also fails to draw on iOS"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, Uno views, iOS, and Uno partner labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Uno",
          "os/iOS",
          "partner/unoplatform"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter for minimal repro and additional info",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the report! We can see a potential cause: on iOS, `SKXamlCanvas` initializes its drawing surface only when the `Loaded` event fires, whereas on Android it's initialized in the constructor. If Uno Platform on iOS doesn't fire `Loaded` before the first draw, nothing gets rendered.\n\nTo confirm and fix this, we need:\n1. A **minimal repro project** (the Reproduction Link is empty)\n2. The **Uno Platform NuGet package version** you're using\n3. Whether a **plain (non-subclassed) `SKXamlCanvas`** with a `PaintSurface` handler also fails to draw on iOS\n\nThank you!"
      }
    ]
  }
}
```

</details>
