# Issue Triage Report — #801

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T11:36:00Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp.Views.Maui (0.82 (82%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** Feature request to expose keyboard modifier state (Ctrl, Shift, Command) in SKTouchEventArgs so desktop apps can differentiate modified mouse clicks.

**Analysis:** SKTouchEventArgs currently exposes Id, ActionType, DeviceType, MouseButton, Location, InContact, WheelDelta, and Pressure but has no property for keyboard modifier state (Ctrl/Shift/Alt/Command). On desktop platforms each platform handler (Windows WinUI, Apple, Android, Tizen) constructs SKTouchEventArgs directly; none pass modifier information because the type has no field for it. The Windows handler does have access to PointerRoutedEventArgs which can expose modifier keys via CoreWindow.GetForCurrentThread().GetKeyState(), and Apple UIKit/AppKit expose UIKeyModifierFlags / NSEvent.modifierFlags. This is a genuine capability gap for desktop canvas apps.

**Recommendations:** **needs-investigation** — Valid feature request acknowledged by maintainer. The gap is confirmed by code investigation. A reproduction is not needed (no bug), but investigation is warranted to design the API change and assess effort across all platform handlers.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Windows-Classic, os/macOS, os/Linux |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SKTouchEventArgs class in the current MAUI views codebase still has no key modifier property; the feature has never been implemented. |

## Analysis

### Technical Summary

SKTouchEventArgs currently exposes Id, ActionType, DeviceType, MouseButton, Location, InContact, WheelDelta, and Pressure but has no property for keyboard modifier state (Ctrl/Shift/Alt/Command). On desktop platforms each platform handler (Windows WinUI, Apple, Android, Tizen) constructs SKTouchEventArgs directly; none pass modifier information because the type has no field for it. The Windows handler does have access to PointerRoutedEventArgs which can expose modifier keys via CoreWindow.GetForCurrentThread().GetKeyState(), and Apple UIKit/AppKit expose UIKeyModifierFlags / NSEvent.modifierFlags. This is a genuine capability gap for desktop canvas apps.

### Rationale

Classified as type/feature-request because the existing SKTouchEventArgs API simply lacks a modifier property — the behavior is not wrong, it is incomplete. Area is area/SkiaSharp.Views.Maui because the Xamarin.Forms views layer has been superseded by MAUI views and that is where SKTouchEventArgs now lives. Desktop platforms (Windows, macOS, Linux) are the primary consumers since mobile platforms rarely expose modifier key state to touch events. tenet/compatibility is flagged because adding a new constructor overload must remain ABI-compatible with existing callers.

### Key Signals

- "it not possible to know if a mouse click has any key modifier as this information is not provided in the SKTouchEventArgs" — **issue body** (Reporter has confirmed the limitation — no workaround exists within the SkiaSharp views API.)
- "VS bug #805949" — **issue body** (Internally tracked; indicates the request was considered seriously when filed.)
- "This is something good to have." — **comment by mattleibow** (Maintainer validated the request as worthwhile.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SKTouchEventArgs.cs` | 1-86 | direct | SKTouchEventArgs constructor chain has parameters: id, type, mouseButton, deviceType, location, inContact, wheelDelta, pressure. No keyboard modifier parameter exists. The public surface exposes 8 properties with no modifier-related one. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs` | 106-127 | direct | CommonHandler creates SKTouchEventArgs with (id, touchActionType, mouse, device, skPoint, isInContact, wheelDelta) — no modifier state passed. PointerRoutedEventArgs is available and provides access to modifier keys but is not queried. |

### Resolution Proposals

**Hypothesis:** Add an optional `SKKeyModifiers` flags enum and expose it as a property on SKTouchEventArgs, then plumb the value through each desktop platform handler (Windows WinUI, macOS/iOS AppKit/UIKit, Linux/GTK).

1. **Add SKKeyModifiers enum and extend SKTouchEventArgs** — fix, cost/m, validated=untested
   - Define a [Flags] SKKeyModifiers enum (None, Shift, Control, Alt, Meta/Command). Add a new constructor overload to SKTouchEventArgs that includes modifiers (keeping all existing overloads for ABI compatibility). Plumb modifiers in each platform SKTouchHandler: Windows via CoreWindow key state, Apple via NSEvent.modifierFlags / UIKeyModifierFlags, Android/Tizen can pass None as modifiers are not applicable.

**Recommended proposal:** fix-1

**Why:** This is the minimal change needed: a new flags enum, one new constructor overload, and per-platform handler updates. All existing callers are unaffected.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Valid feature request acknowledged by maintainer. The gap is confirmed by code investigation. A reproduction is not needed (no bug), but investigation is warranted to design the API change and assess effort across all platform handlers. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/feature-request, area/SkiaSharp.Views.Maui, os/Windows-Classic, os/macOS, os/Linux, tenet/compatibility | labels=type/feature-request, area/SkiaSharp.Views.Maui, os/Windows-Classic, os/macOS, os/Linux, tenet/compatibility |
| add-comment | medium | 0.82 (82%) | Acknowledge the request, confirm the gap exists in current MAUI views, outline the proposed design (SKKeyModifiers enum + new constructor overload + platform handler updates). | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for this request! This is still relevant in the SkiaSharp MAUI views layer.

`SKTouchEventArgs` currently has no keyboard modifier property. Adding this would require:

1. A new `[Flags] SKKeyModifiers` enum (`None`, `Shift`, `Control`, `Alt`, `Meta`)
2. A new constructor overload on `SKTouchEventArgs` (existing overloads must remain for ABI compatibility)
3. Per-platform plumbing in each `SKTouchHandler`:
   - **Windows (WinUI):** query `CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.*)`
   - **macOS / iOS:** read `NSEvent.modifierFlags` / `UIKeyModifierFlags`
   - **Android / Tizen:** pass `SKKeyModifiers.None` (modifier keys are not applicable to touch)

Contributions are welcome!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 801,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T11:36:00Z"
  },
  "summary": "Feature request to expose keyboard modifier state (Ctrl, Shift, Command) in SKTouchEventArgs so desktop apps can differentiate modified mouse clicks.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.82
    },
    "platforms": [
      "os/Windows-Classic",
      "os/macOS",
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The SKTouchEventArgs class in the current MAUI views codebase still has no key modifier property; the feature has never been implemented."
    }
  },
  "analysis": {
    "summary": "SKTouchEventArgs currently exposes Id, ActionType, DeviceType, MouseButton, Location, InContact, WheelDelta, and Pressure but has no property for keyboard modifier state (Ctrl/Shift/Alt/Command). On desktop platforms each platform handler (Windows WinUI, Apple, Android, Tizen) constructs SKTouchEventArgs directly; none pass modifier information because the type has no field for it. The Windows handler does have access to PointerRoutedEventArgs which can expose modifier keys via CoreWindow.GetForCurrentThread().GetKeyState(), and Apple UIKit/AppKit expose UIKeyModifierFlags / NSEvent.modifierFlags. This is a genuine capability gap for desktop canvas apps.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/SKTouchEventArgs.cs",
        "finding": "SKTouchEventArgs constructor chain has parameters: id, type, mouseButton, deviceType, location, inContact, wheelDelta, pressure. No keyboard modifier parameter exists. The public surface exposes 8 properties with no modifier-related one.",
        "relevance": "direct",
        "lines": "1-86"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Platform/Windows/SKTouchHandler.cs",
        "finding": "CommonHandler creates SKTouchEventArgs with (id, touchActionType, mouse, device, skPoint, isInContact, wheelDelta) — no modifier state passed. PointerRoutedEventArgs is available and provides access to modifier keys but is not queried.",
        "relevance": "direct",
        "lines": "106-127"
      }
    ],
    "keySignals": [
      {
        "text": "it not possible to know if a mouse click has any key modifier as this information is not provided in the SKTouchEventArgs",
        "source": "issue body",
        "interpretation": "Reporter has confirmed the limitation — no workaround exists within the SkiaSharp views API."
      },
      {
        "text": "VS bug #805949",
        "source": "issue body",
        "interpretation": "Internally tracked; indicates the request was considered seriously when filed."
      },
      {
        "text": "This is something good to have.",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer validated the request as worthwhile."
      }
    ],
    "rationale": "Classified as type/feature-request because the existing SKTouchEventArgs API simply lacks a modifier property — the behavior is not wrong, it is incomplete. Area is area/SkiaSharp.Views.Maui because the Xamarin.Forms views layer has been superseded by MAUI views and that is where SKTouchEventArgs now lives. Desktop platforms (Windows, macOS, Linux) are the primary consumers since mobile platforms rarely expose modifier key state to touch events. tenet/compatibility is flagged because adding a new constructor overload must remain ABI-compatible with existing callers.",
    "resolution": {
      "hypothesis": "Add an optional `SKKeyModifiers` flags enum and expose it as a property on SKTouchEventArgs, then plumb the value through each desktop platform handler (Windows WinUI, macOS/iOS AppKit/UIKit, Linux/GTK).",
      "proposals": [
        {
          "title": "Add SKKeyModifiers enum and extend SKTouchEventArgs",
          "category": "fix",
          "effort": "cost/m",
          "validated": "untested",
          "description": "Define a [Flags] SKKeyModifiers enum (None, Shift, Control, Alt, Meta/Command). Add a new constructor overload to SKTouchEventArgs that includes modifiers (keeping all existing overloads for ABI compatibility). Plumb modifiers in each platform SKTouchHandler: Windows via CoreWindow key state, Apple via NSEvent.modifierFlags / UIKeyModifierFlags, Android/Tizen can pass None as modifiers are not applicable."
        }
      ],
      "recommendedProposal": "fix-1",
      "recommendedReason": "This is the minimal change needed: a new flags enum, one new constructor overload, and per-platform handler updates. All existing callers are unaffected."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Valid feature request acknowledged by maintainer. The gap is confirmed by code investigation. A reproduction is not needed (no bug), but investigation is warranted to design the API change and assess effort across all platform handlers.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/feature-request, area/SkiaSharp.Views.Maui, os/Windows-Classic, os/macOS, os/Linux, tenet/compatibility",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp.Views.Maui",
          "os/Windows-Classic",
          "os/macOS",
          "os/Linux",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request, confirm the gap exists in current MAUI views, outline the proposed design (SKKeyModifiers enum + new constructor overload + platform handler updates).",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for this request! This is still relevant in the SkiaSharp MAUI views layer.\n\n`SKTouchEventArgs` currently has no keyboard modifier property. Adding this would require:\n\n1. A new `[Flags] SKKeyModifiers` enum (`None`, `Shift`, `Control`, `Alt`, `Meta`)\n2. A new constructor overload on `SKTouchEventArgs` (existing overloads must remain for ABI compatibility)\n3. Per-platform plumbing in each `SKTouchHandler`:\n   - **Windows (WinUI):** query `CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.*)`\n   - **macOS / iOS:** read `NSEvent.modifierFlags` / `UIKeyModifierFlags`\n   - **Android / Tizen:** pass `SKKeyModifiers.None` (modifier keys are not applicable to touch)\n\nContributions are welcome!"
      }
    ]
  }
}
```

</details>
