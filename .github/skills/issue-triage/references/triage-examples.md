# Triage Examples

Full JSON examples for reference. Read once per session to calibrate output format.

## Bug Example

Bug with bugSignals, codeInvestigation, resolution proposals, and simplified actions:

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1234,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-02-08T15:00:00Z",
    "currentLabels": ["type/feature-request"]
  },
  "summary": "Crash on Android when disposing SKCanvasView",
  "classification": {
    "type": { "value": "type/bug", "confidence": 0.92 },
    "area": { "value": "area/SkiaSharp.Views", "confidence": 0.85 },
    "platforms": ["os/Android"]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "errorType": "crash",
      "errorMessage": "ObjectDisposedException at SKCanvasView.OnDetachedFromWindow",
      "reproQuality": "complete",
      "targetFrameworks": ["net8.0-android"]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a MAUI app with SKCanvasView",
        "Navigate away from the page containing the view",
        "Observe crash on Android (works fine on iOS)"
      ],
      "environmentDetails": "Android 14, net8.0-android, SkiaSharp 3.116.1",
      "relatedIssues": [1100, 1150],
      "repoLinks": [
        { "url": "https://github.com/user/repro-android-crash", "description": "Minimal MAUI repro project" }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": ["3.116.1"],
      "currentRelevance": "likely",
      "relevanceReason": "The disposal code path hasn't changed since 3.116.1."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.80,
      "reason": "Reporter states this worked in 2.88.x. View lifecycle code was rewritten for MAUI.",
      "workedInVersion": "2.88.8",
      "brokeInVersion": "3.116.1"
    }
  },
  "analysis": {
    "summary": "Crash in SKCanvasView disposal on Android. Stack trace points to native memory access after the surface was released.",
    "rationale": "Reporter describes a crash with stack trace during a normal lifecycle event (view detachment). This is clearly broken behavior, not a usage question. The crash is in view disposal, not core drawing.",
    "keySignals": [
      { "text": "ObjectDisposedException at SKCanvasView.OnDetachedFromWindow", "source": "issue body", "interpretation": "Native surface accessed after disposal — classic use-after-free." },
      { "text": "Works fine on iOS, only crashes on Android", "source": "issue body", "interpretation": "Platform-specific lifecycle difference. Android detaches on different thread." },
      { "text": "Worked in 2.88.8, broke after upgrading to 3.116.1", "source": "comment #2", "interpretation": "Regression — view lifecycle was rewritten for MAUI migration." }
    ],
    "codeInvestigation": [
      { "file": "source/SkiaSharp.Views.Maui/Platform/Android/SKCanvasView.cs", "lines": "45-78", "finding": "OnDetachedFromWindow disposes native surface without null-check — use-after-free if called on background thread", "relevance": "direct" },
      { "file": "source/SkiaSharp.Views.Maui/Platform/iOS/SKCanvasView.cs", "lines": "52-70", "finding": "iOS equivalent checks IsDisposed before accessing surface — explains why iOS doesn't crash", "relevance": "related" }
    ],
    "nextQuestions": ["Unclear if specific to Android 14+ or all versions", "Unknown if SKGLView has the same issue"],
    "resolution": {
      "hypothesis": "Android detaches views on a different thread than iOS, causing use-after-free of the native surface.",
      "proposals": [
        { "title": "Add disposal guard", "description": "Add null/disposed check before accessing native surface in OnDetachedFromWindow.", "codeSnippet": "if (IsDisposed || _surface == null) return;", "confidence": 0.75, "effort": "small" },
        { "title": "Synchronize with UI thread", "description": "Synchronize disposal with UI thread to ensure surface validity.", "confidence": 0.70, "effort": "medium" }
      ],
      "recommendedProposal": "Add disposal guard",
      "recommendedReason": "Simpler fix with high confidence. Matches the pattern already used on iOS."
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
        "type": "update-labels",
        "description": "Apply bug, views, android labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": ["type/bug", "area/SkiaSharp.Views", "os/Android"]
      },
      {
        "type": "add-comment",
        "description": "Post analysis response asking for Android version details",
        "risk": "high",
        "confidence": 0.80,
        "comment": "Thanks for the detailed stack trace. This looks like a threading issue in Android's view lifecycle. Could you confirm which Android version(s) you're seeing this on?"
      }
    ]
  }
}
```

## Question Example

Question with resolution proposals, no bugSignals, close-with-docs action:

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 5678,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-02-08T15:00:00Z"
  },
  "summary": "How to load custom fonts in SkiaSharp on Linux",
  "classification": {
    "type": { "value": "type/question", "confidence": 0.90 },
    "area": { "value": "area/SkiaSharp", "confidence": 0.80 },
    "platforms": ["os/Linux"]
  },
  "evidence": {},
  "analysis": {
    "summary": "User asking how to load custom fonts on Linux. Usage question — the API exists and works.",
    "rationale": "Asking how to accomplish a task. No broken behavior described. Docs exist for SKTypeface.FromFile() and FromData().",
    "codeInvestigation": [
      { "file": "binding/SkiaSharp/SKTypeface.cs", "lines": "45-62", "finding": "SKTypeface.FromFile() and FromData() are public, well-documented APIs — confirms this is a usage question", "relevance": "context" }
    ],
    "workarounds": ["Use SKTypeface.FromFile('/path/to/font.ttf')", "Embed font as resource and use SKTypeface.FromData(skData)"],
    "resolution": {
      "hypothesis": "User wants to render text with a custom .ttf font on Linux.",
      "proposals": [
        { "description": "Use SKTypeface.FromFile() to load font from path. Simplest approach.", "confidence": 0.90, "effort": "trivial" },
        { "description": "Embed font as resource and use SKTypeface.FromData() for portability.", "confidence": 0.90, "effort": "small" }
      ]
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-with-docs",
      "confidence": 0.85,
      "reason": "Answered by existing API documentation"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and linux labels",
        "risk": "low",
        "confidence": 0.90,
        "labels": ["type/question", "area/SkiaSharp", "os/Linux"]
      },
      {
        "type": "add-comment",
        "description": "Post answer with font loading methods",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Use `SKTypeface.FromFile(\"/path/to/font.ttf\")` or embed the font as a resource and use `SKTypeface.FromData(skData)`. Both approaches work without fontconfig."
      },
      {
        "type": "close-issue",
        "description": "Close as answered",
        "risk": "medium",
        "confidence": 0.80
      }
    ]
  }
}
```

## Duplicate Example

Duplicate with link-duplicate action and linkedIssue:

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 9999,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-02-08T15:00:00Z"
  },
  "summary": "Duplicate of #1234 — same Android disposal crash",
  "classification": {
    "type": { "value": "type/bug", "confidence": 0.95 },
    "area": { "value": "area/SkiaSharp.Views", "confidence": 0.95 },
    "platforms": ["os/Android"]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "errorType": "crash",
      "errorMessage": "ObjectDisposedException at SKCanvasView.OnDetachedFromWindow"
    }
  },
  "analysis": {
    "summary": "Identical stack trace and reproduction steps as #1234.",
    "rationale": "Same crash signature as #1234 — same component, same version, same stack trace.",
    "codeInvestigation": [
      { "file": "source/SkiaSharp.Views.Maui/Platform/Android/SKCanvasView.cs", "lines": "45-78", "finding": "Same disposal code path as #1234 — confirms duplicate", "relevance": "direct" }
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-duplicate",
      "confidence": 0.95,
      "reason": "Identical stack trace to #1234"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": ["type/bug", "area/SkiaSharp.Views", "os/Android"]
      },
      {
        "type": "link-duplicate",
        "description": "Mark as duplicate of #1234",
        "risk": "medium",
        "confidence": 0.95,
        "linkedIssue": 1234
      },
      {
        "type": "close-issue",
        "description": "Close as duplicate",
        "risk": "medium",
        "confidence": 0.95
      }
    ]
  }
}
```
