# Triage Examples

Full JSON examples for reference. Read once per session to calibrate output format.

## Bug Example

Bug with stack trace, bugSignals required, analysisNotes required, resolutionAnalysis with 3+ proposals:

```json
{
  "schemaVersion": "1.0",
  "number": 1234,
  "repo": "mono/SkiaSharp",
  "analyzedAt": "2026-02-08T15:00:00Z",
  "summary": "Crash on Android when disposing SKCanvasView",
  "type": { "value": "bug", "confidence": 0.92, "reason": "Stack trace present" },
  "area": { "value": "SkiaSharp.Views", "confidence": 0.85, "reason": "SKCanvasView is in Views" },
  "backends": null, "platforms": null, "tenets": null, "partner": null,
  "regression": null, "fixStatus": null,
  "bugSignals": {
    "hasCrash": true, "hasStackTrace": true, "reproQuality": "partial",
    "hasScreenshot": false, "hasWorkaround": false, "workaroundSummary": null,
    "targetFrameworks": ["net8.0-android"],
    "severity": "high", "severityReason": "Crash with no workaround"
  },
  "reproEvidence": null, "versionAnalysis": null,
  "actionability": { "suggestedAction": "needs-investigation", "confidence": 0.80, "reason": "Real bug with stack trace" },
  "suggestedResponse": null,
  "analysisNotes": {
    "summary": "Crash in SKCanvasView disposal on Android. Stack trace points to native memory access after the surface was released.",
    "keySignals": [
      { "text": "ObjectDisposedException at SKCanvasView.OnDetachedFromWindow", "source": "stack-trace", "interpretation": "Native surface accessed after disposal", "supportedFields": ["type", "area", "bugSignals.severity"] },
      { "text": "works on iOS, crashes on Android only", "source": "body", "interpretation": "Platform-specific disposal timing issue", "supportedFields": ["platforms"] }
    ],
    "fieldRationales": [
      { "field": "type", "chosen": "bug", "expandedReason": "Reporter describes a crash with stack trace during a normal lifecycle event (view detachment). This is clearly broken behavior, not a usage question.", "alternatives": [{ "value": "question", "whyRejected": "Not asking how to do something — reporting a crash." }] },
      { "field": "area", "chosen": "SkiaSharp.Views", "expandedReason": "SKCanvasView is in the Views package. The crash occurs in view lifecycle methods.", "alternatives": [{ "value": "SkiaSharp", "whyRejected": "Crash is in view disposal, not core drawing." }] },
      { "field": "bugSignals.severity", "chosen": "high", "expandedReason": "Hard crash with no workaround. Occurs during normal view lifecycle." }
    ],
    "docsConsulted": [
      { "path": "documentation/packages.md", "relevance": "Confirmed SKCanvasView is in SkiaSharp.Views package", "usedFor": ["area"] }
    ],
    "docsNotConsulted": "No native loading diagnostics needed — managed disposal crash.",
    "uncertainties": ["Unclear if specific to Android 14+ or all versions", "Unknown if SKGLView has the same issue"],
    "assumptions": ["Assumed latest stable NativeAssets.Android since no version conflict mentioned"]
  },
  "resolutionAnalysis": {
    "hypothesis": "Android detaches views on a different thread than iOS, causing use-after-free of the native surface.",
    "researchDone": ["Checked SkiaSharp.Views Android disposal pattern", "Searched for similar lifecycle crash issues"],
    "proposals": [
      { "title": "Guard disposal with null check", "description": "Add null/disposed check before accessing native surface in OnDetachedFromWindow.", "steps": ["Add IsDisposed check", "Run Android lifecycle tests"], "pros": ["Simple fix", "Low risk"], "cons": ["May mask deeper issue"], "confidence": 0.75, "effort": "low" },
      { "title": "Synchronize disposal with UI thread", "description": "Post disposal to UI thread to ensure surface validity.", "steps": ["Wrap surface release in RunOnUiThread", "Add thread-safety tests"], "pros": ["Addresses root cause"], "cons": ["May introduce timing issues"], "confidence": 0.70, "effort": "medium" },
      { "title": "Weak reference to native surface", "description": "Hold weak reference to prevent use-after-free.", "steps": ["Replace strong with weak reference", "Add null check", "Test across versions"], "pros": ["Prevents crash definitively"], "cons": ["More invasive"], "confidence": 0.65, "effort": "medium" }
    ],
    "recommendedProposal": "Guard disposal with null check",
    "recommendedReason": "Lowest risk, addresses the immediate crash."
  }
}
```

## Question Example

Question with resolutionAnalysis (3+ answers/approaches), bugSignals null:

```json
{
  "schemaVersion": "1.0",
  "number": 5678,
  "repo": "mono/SkiaSharp",
  "analyzedAt": "2026-02-08T15:00:00Z",
  "summary": "How to load custom fonts in SkiaSharp on Linux",
  "type": { "value": "question", "confidence": 0.90, "reason": "Asking how to do something" },
  "area": { "value": "SkiaSharp", "confidence": 0.80, "reason": "Core font loading API" },
  "backends": null, "platforms": null, "tenets": null, "partner": null,
  "regression": null, "fixStatus": null, "bugSignals": null,
  "reproEvidence": null, "versionAnalysis": null,
  "actionability": {
    "suggestedAction": "close-with-docs", "confidence": 0.85,
    "reason": "Answered by existing documentation", "closeable": true,
    "closeReason": "See SKTypeface.FromFile() docs"
  },
  "suggestedResponse": {
    "responseType": "documentation", "confidence": 0.85,
    "reason": "Direct answer exists in API docs",
    "draft": "Use `SKTypeface.FromFile()` or `SKTypeface.FromData()` to load custom fonts."
  },
  "analysisNotes": {
    "summary": "User asking how to load custom fonts on Linux. Usage question — the API exists and works.",
    "keySignals": [
      { "text": "How do I load a custom .ttf font?", "source": "body", "interpretation": "How-to question about existing API", "supportedFields": ["type"] }
    ],
    "fieldRationales": [
      { "field": "type", "chosen": "question", "expandedReason": "Asking how to accomplish a task. No broken behavior described.", "alternatives": [{ "value": "documentation", "whyRejected": "Docs exist — user just hasn't found them." }] },
      { "field": "actionability.suggestedAction", "chosen": "close-with-docs", "expandedReason": "SKTypeface.FromFile() and FromData() are documented. Can answer directly." }
    ],
    "docsConsulted": [
      { "path": "docs/SkiaSharpAPI/SkiaSharp/SKTypeface.xml", "relevance": "Confirmed FromFile and FromData methods exist", "usedFor": ["type", "actionability"] }
    ]
  },
  "resolutionAnalysis": {
    "hypothesis": "User wants to render text with a custom .ttf font on Linux where system fonts may not be available.",
    "researchDone": ["Checked SKTypeface API docs", "Reviewed Linux package selection in packages.md"],
    "proposals": [
      { "title": "SKTypeface.FromFile()", "description": "Load font directly from file path.", "steps": ["Bundle .ttf with the app", "Call SKTypeface.FromFile(\"/path/to/font.ttf\")", "Set SKPaint.Typeface"], "pros": ["Simplest approach", "No dependencies"], "cons": ["Requires knowing file path at runtime"], "confidence": 0.90, "effort": "low" },
      { "title": "SKTypeface.FromData() with embedded resource", "description": "Embed font as resource, load from byte array.", "steps": ["Add .ttf as embedded resource", "Read stream into SKData", "Call SKTypeface.FromData(skData)"], "pros": ["Font travels with assembly", "No file path concerns"], "cons": ["Slightly more code"], "confidence": 0.90, "effort": "low" },
      { "title": "SKFontManager with NativeAssets.Linux", "description": "Use system font enumeration via fontconfig.", "steps": ["Use NativeAssets.Linux instead of NoDependencies", "Install fontconfig", "Use SKFontManager.Default"], "pros": ["Access to all system fonts"], "cons": ["Requires fontconfig dependency"], "confidence": 0.75, "effort": "medium" }
    ],
    "recommendedProposal": "SKTypeface.FromFile()",
    "recommendedReason": "Simplest and most portable. Works regardless of fontconfig availability."
  }
}
```

## Minimal Example (Duplicate)

```json
{
  "schemaVersion": "1.0",
  "number": 9999,
  "repo": "mono/SkiaSharp",
  "analyzedAt": "2026-02-08T15:00:00Z",
  "summary": "Duplicate of #1234 — same Android disposal crash",
  "type": { "value": "bug", "confidence": 0.95, "reason": "Same crash as #1234" },
  "area": { "value": "SkiaSharp.Views", "confidence": 0.95, "reason": "Same area as #1234" },
  "backends": null, "platforms": null, "tenets": null, "partner": null,
  "regression": null, "fixStatus": null,
  "bugSignals": { "hasCrash": true, "hasStackTrace": true, "reproQuality": "full", "hasScreenshot": false, "hasWorkaround": false, "workaroundSummary": null, "targetFrameworks": ["net8.0-android"], "severity": "high", "severityReason": "Same as #1234" },
  "reproEvidence": null, "versionAnalysis": null,
  "actionability": { "suggestedAction": "close-as-duplicate", "confidence": 0.95, "reason": "Identical stack trace to #1234", "duplicateOf": 1234 },
  "suggestedResponse": null,
  "analysisNotes": {
    "summary": "Identical stack trace and reproduction steps as #1234. Same Android disposal crash.",
    "keySignals": [
      { "text": "ObjectDisposedException at SKCanvasView.OnDetachedFromWindow", "source": "stack-trace", "interpretation": "Same crash signature as #1234", "supportedFields": ["type", "actionability"] }
    ],
    "fieldRationales": [
      { "field": "actionability.suggestedAction", "chosen": "close-as-duplicate", "expandedReason": "Stack trace is identical to #1234. Same Android version, same view lifecycle trigger." }
    ],
    "docsConsulted": []
  },
  "resolutionAnalysis": null
}
```
