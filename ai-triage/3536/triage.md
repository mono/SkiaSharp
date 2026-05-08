# Issue Triage Report — #3536

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T22:32:00Z |
| Type | type/enhancement (0.88 (88%)) |
| Area | area/SkiaSharp.Views.Maui (0.95 (95%)) |
| Suggested action | needs-investigation (0.90 (90%)) |

**Issue Summary:** Request to add scroll wheel support to the Apple SKTouchHandler for macOS MAUI views, overriding scrollWheel(with:) and normalizing NSEvent.scrollingDeltaY to the v120 standard (120 units per discrete notch), as a sub-task of #3533.

**Analysis:** The Apple SKTouchHandler (shared across iOS, macOS, and Mac Catalyst in SkiaSharp.Views.Maui) handles touch events only (TouchesBegan/Moved/Ended/Cancelled) and does not override scrollWheel(with:). On macOS, this means WheelChanged events are never fired. The Windows handler already implements wheel support via PointerWheelChanged. This request is well-specified with normalization logic: multiply NSEvent.scrollingDeltaY by 120 for discrete mouse wheels, and by 12 for trackpads (hasPreciseScrollingDeltas), with inversion handling for natural scrolling. The SKTouchAction.WheelChanged enum and WheelDelta field on SKTouchEventArgs are already present in the codebase.

**Recommendations:** **needs-investigation** — Well-specified enhancement with detailed implementation guidance, clear scope (macOS only), and confirmed infrastructure gap. Needs implementation and hardware validation on macOS.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/macOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | partner/maui |
| Current labels | type/enhancement, os/macOS, area/SkiaSharp |

## Evidence

### Reproduction

**Environment:** macOS MAUI, NSEvent.scrollingDeltaY, SKTouchHandler Apple platform handler

**Related issues:** #3533, #3534, #3535, #3537, #3538, #3539, #3540

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3533 — Parent issue: Standardize WheelDelta to v120 convention across all platforms

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Apple SKTouchHandler still has no scrollWheel override — confirmed by reading source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs. |

## Analysis

### Technical Summary

The Apple SKTouchHandler (shared across iOS, macOS, and Mac Catalyst in SkiaSharp.Views.Maui) handles touch events only (TouchesBegan/Moved/Ended/Cancelled) and does not override scrollWheel(with:). On macOS, this means WheelChanged events are never fired. The Windows handler already implements wheel support via PointerWheelChanged. This request is well-specified with normalization logic: multiply NSEvent.scrollingDeltaY by 120 for discrete mouse wheels, and by 12 for trackpads (hasPreciseScrollingDeltas), with inversion handling for natural scrolling. The SKTouchAction.WheelChanged enum and WheelDelta field on SKTouchEventArgs are already present in the codebase.

### Rationale

This is a type/enhancement because scroll wheel support on macOS is a new capability being added to the existing touch event system, as part of the v120 cross-platform standardization effort in #3533. The SKTouchAction.WheelChanged enum and WheelDelta field already exist but are not wired up for macOS. The issue provides a detailed, technically sound implementation spec including normalization logic, sign convention, and natural scrolling handling. Area is area/SkiaSharp.Views.Maui since this is specifically the MAUI platform handler (not the older Views.Forms handler). Platform is os/macOS only (iOS/Mac Catalyst is tracked in #3537). The suggestedReproPlatform is macos since the native NSEvent.scrollingDeltaY API is macOS-only.

### Key Signals

- "The Apple SKTouchHandler ... currently: Handles: TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled. Does NOT override: scrollWheel(with:)" — **issue body** (Confirms the feature gap — no scroll wheel handling on macOS at all.)
- "hasPreciseScrollingDeltas == false → discrete mouse wheel (line units: ~1.0 per notch). Multiply by 120 to get v120." — **issue body** (Normalization formula is provided and well-reasoned, differentiating mouse wheel vs trackpad.)
- "Fire SKTouchAction.WheelChanged with SKTouchDeviceType.Mouse" — **issue body** (Implementation note confirms which action type and device type to use, consistent with the existing SKTouchEventArgs API.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs` | 1-112 | direct | The Apple SKTouchHandler extends UIGestureRecognizer and only overrides TouchesBegan, TouchesMoved, TouchesEnded, and TouchesCancelled. No scrollWheel(with:) override exists — confirming the feature is absent on macOS. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs` | 35-44, 101-104, 122 | related | The Windows handler subscribes to PointerWheelChanged and passes MouseWheelDelta from PointerPoint.Properties to SKTouchEventArgs. This is the reference pattern for adding wheel support on other platforms. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SKTouchEventArgs.cs` | 19-34, 68 | direct | SKTouchEventArgs already has a WheelDelta (int) property and the SKTouchAction.WheelChanged enum value is defined. The infrastructure is in place; only the macOS platform handler needs to fire these events. |

### Resolution Proposals

**Hypothesis:** Add a scrollWheel(with:) override to the Apple SKTouchHandler guarded by #if __MACOS__ (or equivalent platform check), normalizing NSEvent.scrollingDeltaY to v120 units and firing SKTouchAction.WheelChanged.

1. **Add scrollWheel override to Apple SKTouchHandler with #if __MACOS__ guard** — fix, confidence 0.88 (88%), cost/s, validated=untested
   - Override scrollWheel(with:) in SKTouchHandler.cs using a platform guard. Use NSEvent.HasPreciseScrollingDeltas to branch between mouse (×120) and trackpad (×12) scaling. Handle IsDirectionInvertedFromDevice to normalize direction. Get pointer position from NSEvent.LocationInWindow converted to view coordinates. Fire SKTouchAction.WheelChanged with SKTouchDeviceType.Mouse and inContact=false.
2. **Create a separate macOS-specific SKTouchHandler subclass** — alternative, confidence 0.75 (75%), cost/m, validated=untested
   - Instead of modifying the shared Apple handler with #if guards, create a dedicated macOS handler that inherits from or replaces the base SKTouchHandler with scroll wheel support. Avoids platform-specific #if pollution in shared code.

**Recommended proposal:** Add scrollWheel override to Apple SKTouchHandler with #if __MACOS__ guard

**Why:** Follows the existing pattern in the Apple handler and keeps the implementation co-located. The issue author already describes this approach, noting the handler is shared and may need #if for macOS scroll wheel.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.90 (90%) |
| Reason | Well-specified enhancement with detailed implementation guidance, clear scope (macOS only), and confirmed infrastructure gap. Needs implementation and hardware validation on macOS. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Correct labels: change area/SkiaSharp to area/SkiaSharp.Views.Maui, add partner/maui and tenet/compatibility | labels=type/enhancement, os/macOS, area/SkiaSharp.Views.Maui, partner/maui, tenet/compatibility |
| link-related | low | 0.99 (99%) | Link to parent issue #3533 (v120 standardization umbrella) | linkedIssue=#3533 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3536,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T22:32:00Z",
    "currentLabels": [
      "type/enhancement",
      "os/macOS",
      "area/SkiaSharp"
    ]
  },
  "summary": "Request to add scroll wheel support to the Apple SKTouchHandler for macOS MAUI views, overriding scrollWheel(with:) and normalizing NSEvent.scrollingDeltaY to the v120 standard (120 units per discrete notch), as a sub-task of #3533.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.95
    },
    "platforms": [
      "os/macOS"
    ],
    "tenets": [
      "tenet/compatibility"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "macOS MAUI, NSEvent.scrollingDeltaY, SKTouchHandler Apple platform handler",
      "relatedIssues": [
        3533,
        3534,
        3535,
        3537,
        3538,
        3539,
        3540
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3533",
          "description": "Parent issue: Standardize WheelDelta to v120 convention across all platforms"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The Apple SKTouchHandler still has no scrollWheel override — confirmed by reading source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs."
    }
  },
  "analysis": {
    "summary": "The Apple SKTouchHandler (shared across iOS, macOS, and Mac Catalyst in SkiaSharp.Views.Maui) handles touch events only (TouchesBegan/Moved/Ended/Cancelled) and does not override scrollWheel(with:). On macOS, this means WheelChanged events are never fired. The Windows handler already implements wheel support via PointerWheelChanged. This request is well-specified with normalization logic: multiply NSEvent.scrollingDeltaY by 120 for discrete mouse wheels, and by 12 for trackpads (hasPreciseScrollingDeltas), with inversion handling for natural scrolling. The SKTouchAction.WheelChanged enum and WheelDelta field on SKTouchEventArgs are already present in the codebase.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Apple/SKTouchHandler.cs",
        "lines": "1-112",
        "finding": "The Apple SKTouchHandler extends UIGestureRecognizer and only overrides TouchesBegan, TouchesMoved, TouchesEnded, and TouchesCancelled. No scrollWheel(with:) override exists — confirming the feature is absent on macOS.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs",
        "lines": "35-44, 101-104, 122",
        "finding": "The Windows handler subscribes to PointerWheelChanged and passes MouseWheelDelta from PointerPoint.Properties to SKTouchEventArgs. This is the reference pattern for adding wheel support on other platforms.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SKTouchEventArgs.cs",
        "lines": "19-34, 68",
        "finding": "SKTouchEventArgs already has a WheelDelta (int) property and the SKTouchAction.WheelChanged enum value is defined. The infrastructure is in place; only the macOS platform handler needs to fire these events.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "The Apple SKTouchHandler ... currently: Handles: TouchesBegan, TouchesMoved, TouchesEnded, TouchesCancelled. Does NOT override: scrollWheel(with:)",
        "source": "issue body",
        "interpretation": "Confirms the feature gap — no scroll wheel handling on macOS at all."
      },
      {
        "text": "hasPreciseScrollingDeltas == false → discrete mouse wheel (line units: ~1.0 per notch). Multiply by 120 to get v120.",
        "source": "issue body",
        "interpretation": "Normalization formula is provided and well-reasoned, differentiating mouse wheel vs trackpad."
      },
      {
        "text": "Fire SKTouchAction.WheelChanged with SKTouchDeviceType.Mouse",
        "source": "issue body",
        "interpretation": "Implementation note confirms which action type and device type to use, consistent with the existing SKTouchEventArgs API."
      }
    ],
    "rationale": "This is a type/enhancement because scroll wheel support on macOS is a new capability being added to the existing touch event system, as part of the v120 cross-platform standardization effort in #3533. The SKTouchAction.WheelChanged enum and WheelDelta field already exist but are not wired up for macOS. The issue provides a detailed, technically sound implementation spec including normalization logic, sign convention, and natural scrolling handling. Area is area/SkiaSharp.Views.Maui since this is specifically the MAUI platform handler (not the older Views.Forms handler). Platform is os/macOS only (iOS/Mac Catalyst is tracked in #3537). The suggestedReproPlatform is macos since the native NSEvent.scrollingDeltaY API is macOS-only.",
    "resolution": {
      "hypothesis": "Add a scrollWheel(with:) override to the Apple SKTouchHandler guarded by #if __MACOS__ (or equivalent platform check), normalizing NSEvent.scrollingDeltaY to v120 units and firing SKTouchAction.WheelChanged.",
      "proposals": [
        {
          "title": "Add scrollWheel override to Apple SKTouchHandler with #if __MACOS__ guard",
          "description": "Override scrollWheel(with:) in SKTouchHandler.cs using a platform guard. Use NSEvent.HasPreciseScrollingDeltas to branch between mouse (×120) and trackpad (×12) scaling. Handle IsDirectionInvertedFromDevice to normalize direction. Get pointer position from NSEvent.LocationInWindow converted to view coordinates. Fire SKTouchAction.WheelChanged with SKTouchDeviceType.Mouse and inContact=false.",
          "category": "fix",
          "confidence": 0.88,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Create a separate macOS-specific SKTouchHandler subclass",
          "description": "Instead of modifying the shared Apple handler with #if guards, create a dedicated macOS handler that inherits from or replaces the base SKTouchHandler with scroll wheel support. Avoids platform-specific #if pollution in shared code.",
          "category": "alternative",
          "confidence": 0.75,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add scrollWheel override to Apple SKTouchHandler with #if __MACOS__ guard",
      "recommendedReason": "Follows the existing pattern in the Apple handler and keeps the implementation co-located. The issue author already describes this approach, noting the handler is shared and may need #if for macOS scroll wheel."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.9,
      "reason": "Well-specified enhancement with detailed implementation guidance, clear scope (macOS only), and confirmed infrastructure gap. Needs implementation and hardware validation on macOS.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct labels: change area/SkiaSharp to area/SkiaSharp.Views.Maui, add partner/maui and tenet/compatibility",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "os/macOS",
          "area/SkiaSharp.Views.Maui",
          "partner/maui",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to parent issue #3533 (v120 standardization umbrella)",
        "risk": "low",
        "confidence": 0.99,
        "linkedIssue": 3533
      }
    ]
  }
}
```

</details>
