# Issue Triage Report — #1601

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T08:55:52Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | close-as-fixed (0.93 (93%)) |

**Issue Summary:** Feature request to add Metal (GPU) rendering support for tvOS, mirroring the SKMetalView API that was implemented for iOS and macOS via issue #1391 and PR #1394.

**Analysis:** tvOS Metal support (SKMetalView) was requested in 2021 and has since been implemented in SkiaSharp 3.119.0. The codebase now includes full tvOS Metal rendering through the shared Apple SKMetalView (compiled under __TVOS__) and the SkiaSharp.Views tvOS package exposes SKMetalView and SKPaintMetalSurfaceEventArgs to tvOS consumers.

**Recommendations:** **close-as-fixed** — The requested feature (SKMetalView for tvOS) was implemented and released in SkiaSharp 3.119.0. The issue can be closed with a note pointing to the version where it shipped.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.Views |
| Platforms | os/tvOS |
| Backends | backend/Metal |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** tvOS, SkiaSharp at time of filing (2021-02-01) did not include SKMetalView for tvOS

**Related issues:** #1391

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1391 — Parent feature request: Metal APIs for iOS and macOS (now closed/completed)
- https://github.com/mono/SkiaSharp/pull/1394 — PR that added Metal support for iOS and macOS
- https://bugs.chromium.org/p/skia/issues/detail?id=11250 — Upstream Skia change requested by reporter to support tvOS native Metal

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The feature was implemented and released in v3.119.0. The issue is still open only because it was never closed. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.95 (95%) |
| Reason | SKMetalView and SKPaintMetalSurfaceEventArgs for tvOS were added in the 3.119.0 changelog. The source file source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKMetalView.cs includes __TVOS__ conditional compilation guards and is compiled for tvOS TFMs via the csproj. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | 3.119.0 |

## Analysis

### Technical Summary

tvOS Metal support (SKMetalView) was requested in 2021 and has since been implemented in SkiaSharp 3.119.0. The codebase now includes full tvOS Metal rendering through the shared Apple SKMetalView (compiled under __TVOS__) and the SkiaSharp.Views tvOS package exposes SKMetalView and SKPaintMetalSurfaceEventArgs to tvOS consumers.

### Rationale

Code investigation confirms that SKMetalView.cs uses __TVOS__ conditional compilation alongside __IOS__ and __MACOS__, and the csproj compiles the Platform/Apple/ directory for tvOS TFMs. The 3.119.0 changelog for SkiaSharp.Views.tvOS explicitly lists 'New Type: SkiaSharp.Views.tvOS.SKMetalView' and 'New Type: SkiaSharp.Views.tvOS.SKPaintMetalSurfaceEventArgs', confirming the feature is shipped.

### Key Signals

- "Adding the APIs from this issue #1391 and this PR #1394, but for tvOS" — **issue body** (Reporter explicitly wants the same SKMetalView API extended to tvOS. This is now done.)
- "Metal seems to be only supported on iOS and macOS" — **issue body** (Confirmed state at time of filing; no longer true since v3.119.0.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKMetalView.cs` | 1-14 | direct | File is guarded with #if __IOS__ || __MACOS__ || __TVOS__, meaning tvOS is fully included. The SKMetalView class creates GRMtlBackendContext, GRContext.CreateMetal(), and raises PaintSurface on each frame — identical to iOS Metal behavior. |
| `source/SkiaSharp.Views/SkiaSharp.Views/SkiaSharp.Views.csproj` | 40-44 | direct | The ItemGroup for tvOS TFMs includes Platform/Apple/**/*.cs which compiles SKMetalView for tvOS. A PropertyGroup sets RootNamespace to SkiaSharp.Views.tvOS for tvOS TFMs. |
| `binding/SkiaSharp/GRMtlBackendContext.cs` | 1-53 | related | GRMtlBackendContext exposes IMTLDevice and IMTLCommandQueue properties under #if __IOS__ || __MACOS__ || __TVOS__, confirming the binding layer supports tvOS Metal contexts. |
| `changelogs/SkiaSharp.Views/3.119.0/SkiaSharp.Views.tvOS.md` | — | direct | Changelog lists 'New Type: SkiaSharp.Views.tvOS.SKMetalView' and 'New Type: SkiaSharp.Views.tvOS.SKPaintMetalSurfaceEventArgs', confirming the feature shipped in 3.119.0. |

### Next Questions

- Has the upstream Skia bug (https://bugs.chromium.org/p/skia/issues/detail?id=11250) been resolved or was it not required?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.93 (93%) |
| Reason | The requested feature (SKMetalView for tvOS) was implemented and released in SkiaSharp 3.119.0. The issue can be closed with a note pointing to the version where it shipped. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply feature-request, views, tvOS, and Metal backend labels | labels=type/feature-request, area/SkiaSharp.Views, os/tvOS, backend/Metal |
| add-comment | medium | 0.93 (93%) | Notify reporter that the feature shipped in 3.119.0 | — |
| close-issue | medium | 0.93 (93%) | Close the feature request as it has been implemented | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Great news — Metal support for tvOS (`SKMetalView` in the `SkiaSharp.Views.tvOS` namespace) shipped in **SkiaSharp 3.119.0**! 🎉

You can use `SKMetalView` on tvOS the same way you would on iOS:

```csharp
var metalView = new SKMetalView();
metalView.PaintSurface += (sender, e) =>
{
    var canvas = e.Surface.Canvas;
    canvas.Clear(SKColors.White);
    // draw here
};
```

Upgrade to 3.119.0 or later and let us know if you hit any issues. Closing as completed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1601,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T08:55:52Z",
    "currentLabels": []
  },
  "summary": "Feature request to add Metal (GPU) rendering support for tvOS, mirroring the SKMetalView API that was implemented for iOS and macOS via issue #1391 and PR #1394.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.95
    },
    "platforms": [
      "os/tvOS"
    ],
    "backends": [
      "backend/Metal"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1391",
          "description": "Parent feature request: Metal APIs for iOS and macOS (now closed/completed)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/1394",
          "description": "PR that added Metal support for iOS and macOS"
        },
        {
          "url": "https://bugs.chromium.org/p/skia/issues/detail?id=11250",
          "description": "Upstream Skia change requested by reporter to support tvOS native Metal"
        }
      ],
      "environmentDetails": "tvOS, SkiaSharp at time of filing (2021-02-01) did not include SKMetalView for tvOS",
      "relatedIssues": [
        1391
      ]
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.95,
      "reason": "SKMetalView and SKPaintMetalSurfaceEventArgs for tvOS were added in the 3.119.0 changelog. The source file source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKMetalView.cs includes __TVOS__ conditional compilation guards and is compiled for tvOS TFMs via the csproj.",
      "fixedInVersion": "3.119.0"
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "The feature was implemented and released in v3.119.0. The issue is still open only because it was never closed."
    }
  },
  "analysis": {
    "summary": "tvOS Metal support (SKMetalView) was requested in 2021 and has since been implemented in SkiaSharp 3.119.0. The codebase now includes full tvOS Metal rendering through the shared Apple SKMetalView (compiled under __TVOS__) and the SkiaSharp.Views tvOS package exposes SKMetalView and SKPaintMetalSurfaceEventArgs to tvOS consumers.",
    "rationale": "Code investigation confirms that SKMetalView.cs uses __TVOS__ conditional compilation alongside __IOS__ and __MACOS__, and the csproj compiles the Platform/Apple/ directory for tvOS TFMs. The 3.119.0 changelog for SkiaSharp.Views.tvOS explicitly lists 'New Type: SkiaSharp.Views.tvOS.SKMetalView' and 'New Type: SkiaSharp.Views.tvOS.SKPaintMetalSurfaceEventArgs', confirming the feature is shipped.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Apple/SKMetalView.cs",
        "lines": "1-14",
        "finding": "File is guarded with #if __IOS__ || __MACOS__ || __TVOS__, meaning tvOS is fully included. The SKMetalView class creates GRMtlBackendContext, GRContext.CreateMetal(), and raises PaintSurface on each frame — identical to iOS Metal behavior.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/SkiaSharp.Views.csproj",
        "lines": "40-44",
        "finding": "The ItemGroup for tvOS TFMs includes Platform/Apple/**/*.cs which compiles SKMetalView for tvOS. A PropertyGroup sets RootNamespace to SkiaSharp.Views.tvOS for tvOS TFMs.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRMtlBackendContext.cs",
        "lines": "1-53",
        "finding": "GRMtlBackendContext exposes IMTLDevice and IMTLCommandQueue properties under #if __IOS__ || __MACOS__ || __TVOS__, confirming the binding layer supports tvOS Metal contexts.",
        "relevance": "related"
      },
      {
        "file": "changelogs/SkiaSharp.Views/3.119.0/SkiaSharp.Views.tvOS.md",
        "finding": "Changelog lists 'New Type: SkiaSharp.Views.tvOS.SKMetalView' and 'New Type: SkiaSharp.Views.tvOS.SKPaintMetalSurfaceEventArgs', confirming the feature shipped in 3.119.0.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Adding the APIs from this issue #1391 and this PR #1394, but for tvOS",
        "source": "issue body",
        "interpretation": "Reporter explicitly wants the same SKMetalView API extended to tvOS. This is now done."
      },
      {
        "text": "Metal seems to be only supported on iOS and macOS",
        "source": "issue body",
        "interpretation": "Confirmed state at time of filing; no longer true since v3.119.0."
      }
    ],
    "nextQuestions": [
      "Has the upstream Skia bug (https://bugs.chromium.org/p/skia/issues/detail?id=11250) been resolved or was it not required?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.93,
      "reason": "The requested feature (SKMetalView for tvOS) was implemented and released in SkiaSharp 3.119.0. The issue can be closed with a note pointing to the version where it shipped.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, views, tvOS, and Metal backend labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.Views",
          "os/tvOS",
          "backend/Metal"
        ]
      },
      {
        "type": "add-comment",
        "description": "Notify reporter that the feature shipped in 3.119.0",
        "risk": "medium",
        "confidence": 0.93,
        "comment": "Great news — Metal support for tvOS (`SKMetalView` in the `SkiaSharp.Views.tvOS` namespace) shipped in **SkiaSharp 3.119.0**! 🎉\n\nYou can use `SKMetalView` on tvOS the same way you would on iOS:\n\n```csharp\nvar metalView = new SKMetalView();\nmetalView.PaintSurface += (sender, e) =>\n{\n    var canvas = e.Surface.Canvas;\n    canvas.Clear(SKColors.White);\n    // draw here\n};\n```\n\nUpgrade to 3.119.0 or later and let us know if you hit any issues. Closing as completed."
      },
      {
        "type": "close-issue",
        "description": "Close the feature request as it has been implemented",
        "risk": "medium",
        "confidence": 0.93,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
