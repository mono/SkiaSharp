# Issue Triage Report — #3538

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T22:06:49Z |
| Type | type/enhancement (0.97 (97%)) |
| Area | area/SkiaSharp.Views.Maui (0.97 (97%)) |
| Suggested action | ready-to-fix (0.90 (90%)) |

**Issue Summary:** Add scroll wheel support to Tizen MAUI views by handling WheelEvent in SKTouchHandler and normalizing the delta to the v120 convention (120 units = one discrete notch), as a sub-issue of #3533.

**Analysis:** The Tizen SKTouchHandler only registers a TouchEvent handler and has no WheelEvent support. Adding wheel support requires subscribing to Tizen.NUI.BaseComponents.View.WheelEvent, converting Z×−120 to v120 WheelDelta, and firing SKTouchAction.WheelChanged. The implementation pattern is already established in the Windows handler (Platform/Windows/SKTouchHandler.cs).

**Recommendations:** **ready-to-fix** — Root cause is clear (missing WheelEvent subscription), normalization formula is provided by the author, and a reference implementation exists in Platform/Windows/SKTouchHandler.cs. No additional investigation needed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Tizen |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | partner/tizen |
| Current labels | type/enhancement, area/SkiaSharp |

## Evidence

### Reproduction

1. Use SkiaSharp MAUI views on a Tizen device or emulator
2. Subscribe to the Touch event and move the mouse scroll wheel
3. Observe that no SKTouchAction.WheelChanged event is fired

**Environment:** Tizen NUI platform, SkiaSharp.Views.Maui

**Related issues:** #3533

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3533 — Parent issue: Standardize WheelDelta to v120 convention across all platforms

## Analysis

### Technical Summary

The Tizen SKTouchHandler only registers a TouchEvent handler and has no WheelEvent support. Adding wheel support requires subscribing to Tizen.NUI.BaseComponents.View.WheelEvent, converting Z×−120 to v120 WheelDelta, and firing SKTouchAction.WheelChanged. The implementation pattern is already established in the Windows handler (Platform/Windows/SKTouchHandler.cs).

### Rationale

This is a type/enhancement because the touch infrastructure already exists on Tizen (TouchEvent handled) but the wheel event is a new capability being added to an existing system. The parent issue #3533 explicitly lists this as sub-issue #3538 targeting os/Tizen. The request is well-specified with normalization formula, sign convention, expected values table, and a Windows reference implementation. area/SkiaSharp.Views.Maui is correct as the change is in Platform/Tizen/SKTouchHandler.cs within that package.

### Key Signals

- "The Tizen SKTouchHandler currently: Handles TouchEvent… Does NOT handle WheelEvent" — **issue body** (Missing wheel event subscription is the entire gap — no partial implementation to work around.)
- "int wheelDelta = -wheel.Z * 120;" — **issue body – Normalization Logic** (Reporter has already computed the exact normalization formula including sign-flip and scaling to v120.)
- "Reference: Windows handler in Platform/Windows/SKTouchHandler.cs for the pattern" — **issue body – Implementation Notes** (A complete, working reference implementation exists in the same codebase, making this straightforward.)
- "Parent issue: #3533" — **issue body** (This is one of 7 tracked sub-issues under a coordinated cross-platform effort to standardize WheelDelta.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Tizen/SKTouchHandler.cs` | 1-75 | direct | SetEnabled subscribes only to view.TouchEvent; no WheelEvent subscription or handler exists. Detach only unsubscribes TouchEvent. This confirms the gap. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs` | 36-44 | related | Windows handler subscribes to PointerWheelChanged and dispatches SKTouchAction.WheelChanged using MouseWheelDelta (already v120). Provides the reference pattern for the Tizen implementation. |

### Workarounds

- No in-library workaround available. A user could subclass the NUI view and manually forward Tizen Wheel events to their own scroll logic outside of SkiaSharp touch handling.

### Next Questions

- Should the horizontal wheel direction (wheel.Direction == 1) be handled in this issue or deferred to a follow-up?
- Does Tizen NUI BaseComponents.View.WheelEvent fire for custom/non-mouse wheel devices (e.g., rotary crown on wearables)?
- Is there a Tizen emulator available in CI to verify the implementation?

### Resolution Proposals

**Hypothesis:** Add a WheelEvent handler to SKTouchHandler that computes wheelDelta = -wheel.Z * 120 and fires SKTouchAction.WheelChanged, mirroring the Windows pattern.

1. **Add WheelEvent handler to Tizen SKTouchHandler** — fix, confidence 0.92 (92%), cost/s, validated=untested
   - In SetEnabled, subscribe/unsubscribe view.WheelEvent alongside the existing TouchEvent. In the handler, compute wheelDelta = -wheel.Z * 120, retrieve cursor position from wheel.Point, and invoke onTouchAction with SKTouchAction.WheelChanged, SKTouchDeviceType.Mouse, inContact=false, and the computed wheelDelta.

**Recommended proposal:** Add WheelEvent handler to Tizen SKTouchHandler

**Why:** Directly implements the specified behavior with a clear reference implementation available in the same codebase. Low risk, small scope.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.90 (90%) |
| Reason | Root cause is clear (missing WheelEvent subscription), normalization formula is provided by the author, and a reference implementation exists in Platform/Windows/SKTouchHandler.cs. No additional investigation needed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply correct type, area, platform, partner, and tenet labels | labels=type/enhancement, area/SkiaSharp.Views.Maui, os/Tizen, partner/tizen, tenet/compatibility, tenet/reliability |
| link-related | low | 0.99 (99%) | Cross-reference parent tracking issue #3533 | linkedIssue=#3533 |
| add-comment | medium | 0.90 (90%) | Acknowledge the well-specified request and confirm implementation readiness | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed spec! The implementation path is clear:

1. In `SetEnabled`, subscribe/unsubscribe `view.WheelEvent` alongside the existing `TouchEvent`.
2. In the wheel handler, compute `wheelDelta = -wheel.Z * 120` (negation aligns Tizen's down-positive convention with v120's up-positive standard).
3. Fire `SKTouchAction.WheelChanged` with `SKTouchDeviceType.Mouse`, `inContact = false`, and the cursor position from `wheel.Point`.

The Windows handler in `Platform/Windows/SKTouchHandler.cs` is a good reference for the overall structure. The horizontal direction (`wheel.Direction == 1`) can be a follow-up.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3538,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T22:06:49Z",
    "currentLabels": [
      "type/enhancement",
      "area/SkiaSharp"
    ]
  },
  "summary": "Add scroll wheel support to Tizen MAUI views by handling WheelEvent in SKTouchHandler and normalizing the delta to the v120 convention (120 units = one discrete notch), as a sub-issue of #3533.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.97
    },
    "platforms": [
      "os/Tizen"
    ],
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ],
    "partner": "partner/tizen"
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Use SkiaSharp MAUI views on a Tizen device or emulator",
        "Subscribe to the Touch event and move the mouse scroll wheel",
        "Observe that no SKTouchAction.WheelChanged event is fired"
      ],
      "environmentDetails": "Tizen NUI platform, SkiaSharp.Views.Maui",
      "relatedIssues": [
        3533
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3533",
          "description": "Parent issue: Standardize WheelDelta to v120 convention across all platforms"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The Tizen SKTouchHandler only registers a TouchEvent handler and has no WheelEvent support. Adding wheel support requires subscribing to Tizen.NUI.BaseComponents.View.WheelEvent, converting Z×−120 to v120 WheelDelta, and firing SKTouchAction.WheelChanged. The implementation pattern is already established in the Windows handler (Platform/Windows/SKTouchHandler.cs).",
    "rationale": "This is a type/enhancement because the touch infrastructure already exists on Tizen (TouchEvent handled) but the wheel event is a new capability being added to an existing system. The parent issue #3533 explicitly lists this as sub-issue #3538 targeting os/Tizen. The request is well-specified with normalization formula, sign convention, expected values table, and a Windows reference implementation. area/SkiaSharp.Views.Maui is correct as the change is in Platform/Tizen/SKTouchHandler.cs within that package.",
    "keySignals": [
      {
        "text": "The Tizen SKTouchHandler currently: Handles TouchEvent… Does NOT handle WheelEvent",
        "source": "issue body",
        "interpretation": "Missing wheel event subscription is the entire gap — no partial implementation to work around."
      },
      {
        "text": "int wheelDelta = -wheel.Z * 120;",
        "source": "issue body – Normalization Logic",
        "interpretation": "Reporter has already computed the exact normalization formula including sign-flip and scaling to v120."
      },
      {
        "text": "Reference: Windows handler in Platform/Windows/SKTouchHandler.cs for the pattern",
        "source": "issue body – Implementation Notes",
        "interpretation": "A complete, working reference implementation exists in the same codebase, making this straightforward."
      },
      {
        "text": "Parent issue: #3533",
        "source": "issue body",
        "interpretation": "This is one of 7 tracked sub-issues under a coordinated cross-platform effort to standardize WheelDelta."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Tizen/SKTouchHandler.cs",
        "lines": "1-75",
        "finding": "SetEnabled subscribes only to view.TouchEvent; no WheelEvent subscription or handler exists. Detach only unsubscribes TouchEvent. This confirms the gap.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs",
        "lines": "36-44",
        "finding": "Windows handler subscribes to PointerWheelChanged and dispatches SKTouchAction.WheelChanged using MouseWheelDelta (already v120). Provides the reference pattern for the Tizen implementation.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "No in-library workaround available. A user could subclass the NUI view and manually forward Tizen Wheel events to their own scroll logic outside of SkiaSharp touch handling."
    ],
    "nextQuestions": [
      "Should the horizontal wheel direction (wheel.Direction == 1) be handled in this issue or deferred to a follow-up?",
      "Does Tizen NUI BaseComponents.View.WheelEvent fire for custom/non-mouse wheel devices (e.g., rotary crown on wearables)?",
      "Is there a Tizen emulator available in CI to verify the implementation?"
    ],
    "resolution": {
      "hypothesis": "Add a WheelEvent handler to SKTouchHandler that computes wheelDelta = -wheel.Z * 120 and fires SKTouchAction.WheelChanged, mirroring the Windows pattern.",
      "proposals": [
        {
          "title": "Add WheelEvent handler to Tizen SKTouchHandler",
          "description": "In SetEnabled, subscribe/unsubscribe view.WheelEvent alongside the existing TouchEvent. In the handler, compute wheelDelta = -wheel.Z * 120, retrieve cursor position from wheel.Point, and invoke onTouchAction with SKTouchAction.WheelChanged, SKTouchDeviceType.Mouse, inContact=false, and the computed wheelDelta.",
          "category": "fix",
          "confidence": 0.92,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add WheelEvent handler to Tizen SKTouchHandler",
      "recommendedReason": "Directly implements the specified behavior with a clear reference implementation available in the same codebase. Low risk, small scope."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.9,
      "reason": "Root cause is clear (missing WheelEvent subscription), normalization formula is provided by the author, and a reference implementation exists in Platform/Windows/SKTouchHandler.cs. No additional investigation needed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply correct type, area, platform, partner, and tenet labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp.Views.Maui",
          "os/Tizen",
          "partner/tizen",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference parent tracking issue #3533",
        "risk": "low",
        "confidence": 0.99,
        "linkedIssue": 3533
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the well-specified request and confirm implementation readiness",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed spec! The implementation path is clear:\n\n1. In `SetEnabled`, subscribe/unsubscribe `view.WheelEvent` alongside the existing `TouchEvent`.\n2. In the wheel handler, compute `wheelDelta = -wheel.Z * 120` (negation aligns Tizen's down-positive convention with v120's up-positive standard).\n3. Fire `SKTouchAction.WheelChanged` with `SKTouchDeviceType.Mouse`, `inContact = false`, and the cursor position from `wheel.Point`.\n\nThe Windows handler in `Platform/Windows/SKTouchHandler.cs` is a good reference for the overall structure. The horizontal direction (`wheel.Direction == 1`) can be a follow-up."
      }
    ]
  }
}
```

</details>
