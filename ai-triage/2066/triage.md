# Issue Triage Report — #2066

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T16:08:00Z |
| Type | type/enhancement (0.98 (98%)) |
| Area | area/SkiaSharp.Views.Forms (0.90 (90%)) |
| Suggested action | keep-open (0.92 (92%)) |

**Issue Summary:** Enable Nullable Reference Types (NRT) annotations in the SkiaSharp.Views.Forms.GTK (SkiaSharp.Views.Gtk3) package to improve null-safety for consumers.

**Analysis:** The SkiaSharp.Views.Gtk3 project (formerly SkiaSharp.Views.Forms.GTK) does not have Nullable Reference Types enabled. The csproj lacks `<Nullable>enable</Nullable>` and source files like SKDrawingArea.cs have fields (e.g., `private ImageSurface pix`, `private SKSurface surface`) that are non-nullable but assigned null, which would require `?` annotations once NRT is enabled. This is part of a broader initiative to enable NRT across all SkiaSharp packages.

**Recommendations:** **keep-open** — This is a valid long-term enhancement tracked by the maintainer. The work is clear but has been intentionally deferred. No repro needed — it is a code improvement task.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views.Forms |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2064 — Sibling issue: Enable NRT for SkiaSharp.Views.Forms
- https://github.com/mono/SkiaSharp/issues/2065 — Sibling issue: Enable NRT for SkiaSharp.Views.Forms.WPF

## Analysis

### Technical Summary

The SkiaSharp.Views.Gtk3 project (formerly SkiaSharp.Views.Forms.GTK) does not have Nullable Reference Types enabled. The csproj lacks `<Nullable>enable</Nullable>` and source files like SKDrawingArea.cs have fields (e.g., `private ImageSurface pix`, `private SKSurface surface`) that are non-nullable but assigned null, which would require `?` annotations once NRT is enabled. This is part of a broader initiative to enable NRT across all SkiaSharp packages.

### Rationale

This is a maintainer-filed enhancement (filed by mattleibow, core maintainer) to track enabling C# Nullable Reference Types in the SkiaSharp.Views.Gtk3 (formerly Views.Forms.GTK) package. Code investigation confirms NRT is not enabled. Classified as type/enhancement because it improves existing functionality (adds null annotations) without changing behavior. Area is SkiaSharp.Views.Forms as it relates to the Xamarin.Forms/GTK UI layer. The tenet/compatibility applies because enabling NRT changes the public API surface (adding ? to parameter/return types) which affects source compatibility for consumers.

### Key Signals

- "Enable Nullable Reference Types (SkiaSharp.Views.Forms.GTK)" — **issue title** (Maintainer-filed tracking issue for enabling NRT annotations in the GTK Views.Forms package)
- "status/long-term" — **existing label** (Maintainer has marked this as a long-term quality improvement, not urgent)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Gtk3/SkiaSharp.Views.Gtk3.csproj` | — | direct | No <Nullable>enable</Nullable> property present in PropertyGroup. NRT is not currently enabled for this project. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Gtk3/SKDrawingArea.cs` | 11-12 | direct | Fields `private ImageSurface pix` and `private SKSurface surface` are used as nullable (null-checked via `?.` and assigned `null`) but declared without `?` suffix. Enabling NRT would require these to be `ImageSurface?` and `SKSurface?`. The event `PaintSurface` is also nullable-invoked. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Gtk4/SkiaSharp.Views.Gtk4.csproj` | — | related | GTK4 sister project also lacks <Nullable>enable</Nullable>, indicating the same NRT enablement work is needed there too. |

### Resolution Proposals

**Hypothesis:** Add `<Nullable>enable</Nullable>` to the GTK3 (and GTK4) project files and annotate all public APIs and private fields with appropriate nullable reference type annotations.

1. **Enable NRT in SkiaSharp.Views.Gtk3 and SkiaSharp.Views.Gtk4** — fix, cost/s, validated=untested
   - Add `<Nullable>enable</Nullable>` to both .csproj files. In SKDrawingArea.cs, change `private ImageSurface pix` to `private ImageSurface? pix` and `private SKSurface surface` to `private SKSurface? surface`. Review all public method signatures for nullable correctness.

**Recommended proposal:** proposal-1

**Why:** Straightforward change: enable Nullable in csproj and fix field declarations. Low risk as it only adds null annotations without changing runtime behavior.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.92 (92%) |
| Reason | This is a valid long-term enhancement tracked by the maintainer. The work is clear but has been intentionally deferred. No repro needed — it is a code improvement task. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Add type/enhancement, area/SkiaSharp.Views.Forms, os/Linux, tenet/compatibility labels | labels=type/enhancement, area/SkiaSharp.Views.Forms, os/Linux, tenet/compatibility |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2066,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T16:08:00Z"
  },
  "summary": "Enable Nullable Reference Types (NRT) annotations in the SkiaSharp.Views.Forms.GTK (SkiaSharp.Views.Gtk3) package to improve null-safety for consumers.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.98
    },
    "area": {
      "value": "area/SkiaSharp.Views.Forms",
      "confidence": 0.9
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2064",
          "description": "Sibling issue: Enable NRT for SkiaSharp.Views.Forms"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2065",
          "description": "Sibling issue: Enable NRT for SkiaSharp.Views.Forms.WPF"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The SkiaSharp.Views.Gtk3 project (formerly SkiaSharp.Views.Forms.GTK) does not have Nullable Reference Types enabled. The csproj lacks `<Nullable>enable</Nullable>` and source files like SKDrawingArea.cs have fields (e.g., `private ImageSurface pix`, `private SKSurface surface`) that are non-nullable but assigned null, which would require `?` annotations once NRT is enabled. This is part of a broader initiative to enable NRT across all SkiaSharp packages.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Gtk3/SkiaSharp.Views.Gtk3.csproj",
        "finding": "No <Nullable>enable</Nullable> property present in PropertyGroup. NRT is not currently enabled for this project.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Gtk3/SKDrawingArea.cs",
        "finding": "Fields `private ImageSurface pix` and `private SKSurface surface` are used as nullable (null-checked via `?.` and assigned `null`) but declared without `?` suffix. Enabling NRT would require these to be `ImageSurface?` and `SKSurface?`. The event `PaintSurface` is also nullable-invoked.",
        "relevance": "direct",
        "lines": "11-12"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Gtk4/SkiaSharp.Views.Gtk4.csproj",
        "finding": "GTK4 sister project also lacks <Nullable>enable</Nullable>, indicating the same NRT enablement work is needed there too.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Enable Nullable Reference Types (SkiaSharp.Views.Forms.GTK)",
        "source": "issue title",
        "interpretation": "Maintainer-filed tracking issue for enabling NRT annotations in the GTK Views.Forms package"
      },
      {
        "text": "status/long-term",
        "source": "existing label",
        "interpretation": "Maintainer has marked this as a long-term quality improvement, not urgent"
      }
    ],
    "rationale": "This is a maintainer-filed enhancement (filed by mattleibow, core maintainer) to track enabling C# Nullable Reference Types in the SkiaSharp.Views.Gtk3 (formerly Views.Forms.GTK) package. Code investigation confirms NRT is not enabled. Classified as type/enhancement because it improves existing functionality (adds null annotations) without changing behavior. Area is SkiaSharp.Views.Forms as it relates to the Xamarin.Forms/GTK UI layer. The tenet/compatibility applies because enabling NRT changes the public API surface (adding ? to parameter/return types) which affects source compatibility for consumers.",
    "resolution": {
      "hypothesis": "Add `<Nullable>enable</Nullable>` to the GTK3 (and GTK4) project files and annotate all public APIs and private fields with appropriate nullable reference type annotations.",
      "proposals": [
        {
          "title": "Enable NRT in SkiaSharp.Views.Gtk3 and SkiaSharp.Views.Gtk4",
          "description": "Add `<Nullable>enable</Nullable>` to both .csproj files. In SKDrawingArea.cs, change `private ImageSurface pix` to `private ImageSurface? pix` and `private SKSurface surface` to `private SKSurface? surface`. Review all public method signatures for nullable correctness.",
          "category": "fix",
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "proposal-1",
      "recommendedReason": "Straightforward change: enable Nullable in csproj and fix field declarations. Low risk as it only adds null annotations without changing runtime behavior."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.92,
      "reason": "This is a valid long-term enhancement tracked by the maintainer. The work is clear but has been intentionally deferred. No repro needed — it is a code improvement task.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Add type/enhancement, area/SkiaSharp.Views.Forms, os/Linux, tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views.Forms",
          "os/Linux",
          "tenet/compatibility"
        ]
      }
    ]
  }
}
```

</details>
