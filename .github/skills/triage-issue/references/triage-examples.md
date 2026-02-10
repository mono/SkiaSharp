# Triage Examples

Full JSON examples for reference. Read once per session to calibrate output format.

## Bug Example

Bug with stack trace, bugSignals required, analysis with fieldRationales and resolution proposals, actions array:

```json
{
  "meta": {
    "schemaVersion": "2.0",
    "number": 1234,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-02-08T15:00:00Z",
    "currentLabels": ["type/feature-request"]
  },
  "summary": "Crash on Android when disposing SKCanvasView",
  "classification": {
    "type": { "value": "type/bug", "confidence": 0.92 },
    "area": { "value": "area/SkiaSharp.Views", "confidence": 0.85 },
    "backends": null,
    "platforms": [{ "value": "os/Android", "confidence": 0.95 }],
    "tenets": [{ "value": "tenet/reliability", "confidence": 0.90 }],
    "partner": null
  },
  "evidence": {
    "bugSignals": {
      "hasCrash": true, "hasStackTrace": true, "reproQuality": "partial",
      "hasWorkaround": false, "workaroundSummary": null,
      "targetFrameworks": ["net8.0-android"],
      "severity": "high", "severityReason": "Crash with no workaround"
    },
    "reproEvidence": null,
    "versionAnalysis": null,
    "regression": null,
    "fixStatus": null
  },
  "analysis": {
    "summary": "Crash in SKCanvasView disposal on Android. Stack trace points to native memory access after the surface was released.",
    "keySignals": [
      { "text": "ObjectDisposedException at SKCanvasView.OnDetachedFromWindow", "source": "stack-trace", "interpretation": "Native surface accessed after disposal" },
      { "text": "works on iOS, crashes on Android only", "source": "body", "interpretation": "Platform-specific disposal timing issue" }
    ],
    "fieldRationales": [
      { "field": "type", "chosen": "type/bug", "expandedReason": "Reporter describes a crash with stack trace during a normal lifecycle event (view detachment). This is clearly broken behavior, not a usage question.", "alternatives": [{ "value": "type/question", "whyRejected": "Not asking how to do something — reporting a crash." }] },
      { "field": "area", "chosen": "area/SkiaSharp.Views", "expandedReason": "SKCanvasView is in the Views package. The crash occurs in view lifecycle methods.", "alternatives": [{ "value": "area/SkiaSharp", "whyRejected": "Crash is in view disposal, not core drawing." }] },
      { "field": "bugSignals.severity", "chosen": "high", "expandedReason": "Hard crash with no workaround. Occurs during normal view lifecycle." },
      { "field": "platforms", "chosen": "os/Android", "expandedReason": "Reporter states crash only happens on Android, not iOS. Platform-specific disposal timing." },
      { "field": "tenets", "chosen": "tenet/reliability", "expandedReason": "Application crash during normal view lifecycle is a reliability issue." },
      { "field": "actionability.suggestedAction", "chosen": "needs-investigation", "expandedReason": "Root cause is unclear — could be threading, lifecycle ordering, or native surface management. Needs code investigation." }
    ],
    "uncertainties": ["Unclear if specific to Android 14+ or all versions", "Unknown if SKGLView has the same issue"],
    "assumptions": ["Assumed latest stable NativeAssets.Android since no version conflict mentioned"],
    "resolution": {
      "hypothesis": "Android detaches views on a different thread than iOS, causing use-after-free of the native surface.",
      "proposals": [
        { "title": "Guard disposal with null check", "description": "Add null/disposed check before accessing native surface in OnDetachedFromWindow. Simple fix, low risk, but may mask deeper issue.", "confidence": 0.75, "effort": "low" },
        { "title": "Synchronize disposal with UI thread", "description": "Post disposal to UI thread to ensure surface validity. Addresses root cause but may introduce timing issues.", "confidence": 0.70, "effort": "medium" },
        { "title": "Weak reference to native surface", "description": "Hold weak reference to prevent use-after-free. More invasive but prevents crash definitively.", "confidence": 0.65, "effort": "medium" }
      ],
      "recommendedProposal": "Guard disposal with null check",
      "recommendedReason": "Lowest risk, addresses the immediate crash."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.80,
      "reason": "Real bug with stack trace, needs deeper investigation into Android lifecycle threading"
    },
    "actions": [
      {
        "id": "labels-1", "type": "update-labels", "risk": "low",
        "description": "Apply bug, views, android, reliability labels",
        "reason": "Matches classification",
        "confidence": 0.95, "dependsOn": null,
        "payload": {
          "labelsToAdd": ["type/bug", "area/SkiaSharp.Views", "os/Android", "tenet/reliability"],
          "labelsToRemove": ["type/feature-request"]
        }
      },
      {
        "id": "comment-1", "type": "add-comment", "risk": "high",
        "description": "Post analysis response asking for Android version details",
        "reason": "Need more info to narrow down the issue",
        "confidence": 0.80, "dependsOn": "labels-1",
        "payload": {
          "commentType": "request-info",
          "draftBody": "Thanks for the detailed stack trace.\n\nThis looks like a threading issue in Android's view lifecycle — the native surface may be released before OnDetachedFromWindow completes.\n\nWould you be able to confirm which Android version(s) you're seeing this on? And whether you see the same crash with SKGLView instead of SKCanvasView?",
          "requiresHumanEdit": true,
          "finalBody": null,
          "dedupeToken": "triage-1234-comment-1"
        }
      }
    ]
  }
}
```

## Question Example

Question with resolution proposals, bugSignals null, close-with-docs action:

```json
{
  "meta": {
    "schemaVersion": "2.0",
    "number": 5678,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-02-08T15:00:00Z",
    "currentLabels": []
  },
  "summary": "How to load custom fonts in SkiaSharp on Linux",
  "classification": {
    "type": { "value": "type/question", "confidence": 0.90 },
    "area": { "value": "area/SkiaSharp", "confidence": 0.80 },
    "backends": null,
    "platforms": [{ "value": "os/Linux", "confidence": 0.85 }],
    "tenets": null,
    "partner": null
  },
  "evidence": {
    "bugSignals": null,
    "reproEvidence": null,
    "versionAnalysis": null,
    "regression": null,
    "fixStatus": null
  },
  "analysis": {
    "summary": "User asking how to load custom fonts on Linux. Usage question — the API exists and works.",
    "keySignals": [
      { "text": "How do I load a custom .ttf font?", "source": "body", "interpretation": "How-to question about existing API" }
    ],
    "fieldRationales": [
      { "field": "type", "chosen": "type/question", "expandedReason": "Asking how to accomplish a task. No broken behavior described.", "alternatives": [{ "value": "type/documentation", "whyRejected": "Docs exist — user just hasn't found them." }] },
      { "field": "area", "chosen": "area/SkiaSharp", "expandedReason": "Core font loading API (SKTypeface) is in the SkiaSharp package, not Views." },
      { "field": "platforms", "chosen": "os/Linux", "expandedReason": "Reporter specifically asks about Linux where system fonts may not be available." },
      { "field": "actionability.suggestedAction", "chosen": "close-with-docs", "expandedReason": "SKTypeface.FromFile() and FromData() are documented. Can answer directly." }
    ],
    "uncertainties": ["Whether user needs system font enumeration or just custom font loading"],
    "resolution": {
      "hypothesis": "User wants to render text with a custom .ttf font on Linux where system fonts may not be available.",
      "proposals": [
        { "title": "SKTypeface.FromFile()", "description": "Load font directly from file path. Simplest approach, no dependencies, but requires knowing file path at runtime.", "confidence": 0.90, "effort": "low" },
        { "title": "SKTypeface.FromData() with embedded resource", "description": "Embed font as resource, load from byte array. Font travels with assembly, no file path concerns.", "confidence": 0.90, "effort": "low" },
        { "title": "SKFontManager with NativeAssets.Linux", "description": "Use system font enumeration via fontconfig. Access to all system fonts but requires fontconfig dependency.", "confidence": 0.75, "effort": "medium" }
      ],
      "recommendedProposal": "SKTypeface.FromFile()",
      "recommendedReason": "Simplest and most portable. Works regardless of fontconfig availability."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-with-docs",
      "confidence": 0.85,
      "reason": "Answered by existing documentation"
    },
    "actions": [
      {
        "id": "labels-1", "type": "update-labels", "risk": "low",
        "description": "Apply question and linux labels",
        "reason": "Matches classification",
        "confidence": 0.90, "dependsOn": null,
        "payload": {
          "labelsToAdd": ["type/question", "area/SkiaSharp", "os/Linux"],
          "labelsToRemove": []
        }
      },
      {
        "id": "comment-1", "type": "add-comment", "risk": "high",
        "description": "Post answer with font loading methods",
        "reason": "Direct answer available from API docs",
        "confidence": 0.85, "dependsOn": "labels-1",
        "payload": {
          "commentType": "documentation",
          "draftBody": "Use `SKTypeface.FromFile(\"/path/to/font.ttf\")` or embed the font as a resource and use `SKTypeface.FromData(skData)`. Both approaches work without fontconfig.",
          "requiresHumanEdit": true,
          "finalBody": null,
          "dedupeToken": "triage-5678-comment-1"
        }
      },
      {
        "id": "close-1", "type": "close-issue", "risk": "medium",
        "description": "Close as answered",
        "reason": "Question answered with documentation pointer",
        "confidence": 0.80, "dependsOn": "comment-1",
        "payload": { "reason": "completed", "comment": null }
      }
    ]
  }
}
```

## Duplicate Example

Minimal triage for a duplicate issue — close + link actions:

```json
{
  "meta": {
    "schemaVersion": "2.0",
    "number": 9999,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-02-08T15:00:00Z",
    "currentLabels": []
  },
  "summary": "Duplicate of #1234 — same Android disposal crash",
  "classification": {
    "type": { "value": "type/bug", "confidence": 0.95 },
    "area": { "value": "area/SkiaSharp.Views", "confidence": 0.95 },
    "backends": null,
    "platforms": [{ "value": "os/Android", "confidence": 0.95 }],
    "tenets": null,
    "partner": null
  },
  "evidence": {
    "bugSignals": {
      "hasCrash": true, "hasStackTrace": true, "reproQuality": "complete",
      "hasWorkaround": false, "workaroundSummary": null,
      "targetFrameworks": ["net8.0-android"],
      "severity": "high", "severityReason": "Same as #1234"
    },
    "reproEvidence": { "relatedIssues": [1234] },
    "versionAnalysis": null,
    "regression": null,
    "fixStatus": null
  },
  "analysis": {
    "summary": "Identical stack trace and reproduction steps as #1234. Same Android disposal crash.",
    "keySignals": [
      { "text": "ObjectDisposedException at SKCanvasView.OnDetachedFromWindow", "source": "stack-trace", "interpretation": "Same crash signature as #1234" }
    ],
    "fieldRationales": [
      { "field": "type", "chosen": "type/bug", "expandedReason": "Same crash as #1234 — ObjectDisposedException during view detachment." },
      { "field": "area", "chosen": "area/SkiaSharp.Views", "expandedReason": "Crash in SKCanvasView lifecycle, same as canonical issue #1234." },
      { "field": "bugSignals.severity", "chosen": "high", "expandedReason": "Hard crash with no workaround, same severity as #1234." },
      { "field": "platforms", "chosen": "os/Android", "expandedReason": "Android-only crash, identical to #1234." },
      { "field": "actionability.suggestedAction", "chosen": "close-as-duplicate", "expandedReason": "Stack trace is identical to #1234. Same Android version, same view lifecycle trigger." }
    ],
    "uncertainties": ["Whether reporter's environment is identical to #1234"],
    "resolution": null
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-duplicate",
      "confidence": 0.95,
      "reason": "Identical stack trace to #1234"
    },
    "actions": [
      {
        "id": "labels-1", "type": "update-labels", "risk": "low",
        "description": "Apply bug and android labels",
        "reason": "Matches classification",
        "confidence": 0.95, "dependsOn": null,
        "payload": {
          "labelsToAdd": ["type/bug", "area/SkiaSharp.Views", "os/Android"],
          "labelsToRemove": []
        }
      },
      {
        "id": "dup-1", "type": "link-duplicate", "risk": "medium",
        "description": "Mark as duplicate of #1234",
        "reason": "Identical crash signature",
        "confidence": 0.95, "dependsOn": null,
        "payload": { "duplicateOf": 1234, "comment": null }
      },
      {
        "id": "comment-1", "type": "add-comment", "risk": "high",
        "description": "Notify reporter of duplicate",
        "reason": "Let reporter know their issue is tracked",
        "confidence": 0.90, "dependsOn": "dup-1",
        "payload": {
          "commentType": "duplicate-notice",
          "draftBody": "This appears to be the same Android disposal crash tracked in #1234. Closing as duplicate — any updates will be posted there.",
          "requiresHumanEdit": true,
          "finalBody": null,
          "dedupeToken": "triage-9999-comment-1"
        }
      },
      {
        "id": "close-1", "type": "close-issue", "risk": "medium",
        "description": "Close as duplicate",
        "reason": "Duplicate of #1234",
        "confidence": 0.95, "dependsOn": "comment-1",
        "payload": { "reason": "not_planned", "comment": null }
      }
    ]
  }
}
```
