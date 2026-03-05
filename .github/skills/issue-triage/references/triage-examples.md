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
  "summary": "Reporter describes a crash (NullReferenceException) when calling SKBitmap.Decode with a corrupted JPEG on iOS 18 with net9.0-ios and SkiaSharp 3.118.0. The crash occurs in the native codec path and does not happen with valid JPEGs or on Windows.",
  "classification": {
    "type": { "value": "type/bug", "confidence": 0.92 },
    "area": { "value": "area/SkiaSharp", "confidence": 0.85 },
    "platforms": ["os/iOS"]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "crash",
      "errorMessage": "NullReferenceException at SKBitmap.Decode",
      "reproQuality": "complete",
      "targetFrameworks": ["net9.0-ios"]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET MAUI app with SKBitmap.Decode",
        "Pass a truncated/corrupted JPEG file",
        "Observe crash on iOS (returns null safely on Windows)"
      ],
      "environmentDetails": "iOS 18.2, net9.0-ios, SkiaSharp 3.118.0",
      "relatedIssues": [1100, 1150],
      "repoLinks": [
        { "url": "https://github.com/user/repro-decode-crash", "description": "Minimal MAUI repro project" }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": ["3.118.0"],
      "currentRelevance": "likely",
      "relevanceReason": "The codec error handling path hasn't changed since 3.118.0."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.80,
      "reason": "Reporter states this worked in 2.88.x. Codec initialization was refactored for v3.",
      "workedInVersion": "2.88.8",
      "brokeInVersion": "3.118.0"
    }
  },
  "analysis": {
    "summary": "Crash in SKBitmap.Decode when input JPEG is corrupted. The native codec returns null but the managed wrapper doesn't check before accessing the result.",
    "rationale": "Reporter describes a crash with stack trace during normal API usage. This is clearly broken behavior — a corrupted input should return null, not crash.",
    "keySignals": [
      { "text": "NullReferenceException at SKBitmap.Decode", "source": "issue body", "interpretation": "Native codec returns null for corrupted input, but managed code dereferences without null check." },
      { "text": "Works fine on Windows, only crashes on iOS", "source": "issue body", "interpretation": "Platform-specific codec behavior — iOS may report errors differently than Windows." },
      { "text": "Worked in 2.88.8, broke after upgrading to 3.118.0", "source": "comment #2", "interpretation": "Regression — codec initialization was refactored for v3." }
    ],
    "codeInvestigation": [
      { "file": "binding/SkiaSharp/SKBitmap.cs", "lines": "120-145", "finding": "Decode calls native function and dereferences result without null check — NRE if native returns null for bad input", "relevance": "direct" },
      { "file": "binding/SkiaSharp/SKCodec.cs", "lines": "80-95", "finding": "SKCodec.Create has proper null-check pattern — SKBitmap.Decode should follow the same pattern", "relevance": "related" }
    ],
    "nextQuestions": ["Does this affect all corrupted formats or only JPEG?", "Is SKImage.FromEncodedData affected similarly?"],
    "resolution": {
      "hypothesis": "The native codec returns null for corrupted input on iOS, and the managed Decode method dereferences it without checking.",
      "proposals": [
        { "title": "Add null check after native decode", "description": "Check native return value before constructing managed SKBitmap. Return null for failed decodes.", "codeSnippet": "var handle = SkiaApi.sk_bitmap_decode(...); if (handle == IntPtr.Zero) return null;", "confidence": 0.85, "effort": "small" },
        { "title": "Add try-catch with error info", "description": "Wrap native call in try-catch and include codec error details in exception.", "confidence": 0.70, "effort": "medium" }
      ],
      "recommendedProposal": "Add null check after native decode",
      "recommendedReason": "Simpler fix with high confidence. Matches the pattern already used in SKCodec.Create."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.80,
      "reason": "Real bug with stack trace, needs deeper investigation into codec error handling"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core, iOS labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": ["type/bug", "area/SkiaSharp", "os/iOS"]
      },
      {
        "type": "add-comment",
        "description": "Post analysis asking for additional format details",
        "risk": "high",
        "confidence": 0.80,
        "comment": "Thanks for the detailed stack trace. This looks like a missing null check in the decode path when the codec encounters corrupted input. Could you confirm if this also happens with corrupted PNG files, or only JPEG?"
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
  "summary": "How to render text with a custom TrueType font on Android",
  "classification": {
    "type": { "value": "type/question", "confidence": 0.90 },
    "area": { "value": "area/SkiaSharp", "confidence": 0.80 },
    "platforms": ["os/Android"]
  },
  "evidence": {},
  "analysis": {
    "summary": "User asking how to load and use custom fonts on Android. Usage question — the API exists and works.",
    "rationale": "Asking how to accomplish a task. No broken behavior described. Docs exist for SKTypeface.FromFile() and FromStream().",
    "codeInvestigation": [
      { "file": "binding/SkiaSharp/SKTypeface.cs", "lines": "45-62", "finding": "SKTypeface.FromFile() and FromStream() are public, well-documented APIs — confirms this is a usage question", "relevance": "context" }
    ],
    "workarounds": ["Use SKTypeface.FromFile('/path/to/font.ttf')", "Embed font as Android asset and use SKTypeface.FromStream(stream)"],
    "resolution": {
      "hypothesis": "User wants to render text with a custom .ttf font on Android.",
      "proposals": [
        { "title": "Load font from file", "description": "Use SKTypeface.FromFile() to load font from path. Simplest approach.", "category": "fix", "confidence": 0.90, "effort": "trivial" },
        { "title": "Embed font as resource", "description": "Embed font as Android asset and use SKTypeface.FromStream() for portability.", "category": "alternative", "confidence": 0.90, "effort": "small" }
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
        "description": "Apply question and android labels",
        "risk": "low",
        "confidence": 0.90,
        "labels": ["type/question", "area/SkiaSharp", "os/Android"]
      },
      {
        "type": "add-comment",
        "description": "Post answer with font loading methods",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Use `SKTypeface.FromFile(\"/path/to/font.ttf\")` or embed the font as an Android asset and use `SKTypeface.FromStream(stream)`. Both approaches work on Android."
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
  "summary": "Duplicate of #1234 — same iOS decode crash with corrupted input",
  "classification": {
    "type": { "value": "type/bug", "confidence": 0.95 },
    "area": { "value": "area/SkiaSharp", "confidence": 0.95 },
    "platforms": ["os/iOS"]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "errorType": "crash",
      "errorMessage": "NullReferenceException at SKBitmap.Decode"
    }
  },
  "analysis": {
    "summary": "Identical stack trace and reproduction steps as #1234.",
    "rationale": "Same crash signature as #1234 — same component, same version, same stack trace.",
    "codeInvestigation": [
      { "file": "binding/SkiaSharp/SKBitmap.cs", "lines": "120-145", "finding": "Same null-dereference code path as #1234 — confirms duplicate", "relevance": "direct" }
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
        "labels": ["type/bug", "area/SkiaSharp", "os/iOS"]
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
