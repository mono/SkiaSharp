# Issue Triage Report — #2018

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-03T20:33:15Z |
| Type | type/feature-request (0.80 (80%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-info (0.75 (75%)) |

**Issue Summary:** Reporter asks how to expose the Android-only SkAndroidCodec Skia API in C# via SkiaSharp.

**Analysis:** The reporter wants to use the Android-specific SkAndroidCodec Skia class from C#. SkiaSharp currently only exposes SKCodec (wrapping SkCodec), not SkAndroidCodec. SkAndroidCodec provides Android-specific features like getting scaled dimensions from Android's BitmapFactory options. This API is not wrapped in SkiaSharp's C API shim or C# bindings.

**Recommendations:** **needs-info** — The issue is a bare one-line question with no details about what specific SkAndroidCodec functionality is needed. A comment answering the question and asking what specific features are required would clarify whether this is a valid feature request or can be closed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | os/Android |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

## Analysis

### Technical Summary

The reporter wants to use the Android-specific SkAndroidCodec Skia class from C#. SkiaSharp currently only exposes SKCodec (wrapping SkCodec), not SkAndroidCodec. SkAndroidCodec provides Android-specific features like getting scaled dimensions from Android's BitmapFactory options. This API is not wrapped in SkiaSharp's C API shim or C# bindings.

### Rationale

The issue title includes [QUESTION] and the body is a single question with no repro code. SkAndroidCodec is an Android-only Skia class that is not currently wrapped in the SkiaSharp C API or C# bindings. Classifying as a feature-request because the reporter is ultimately asking for this API to be available — answering the question requires either implementing the binding or documenting that it doesn't exist. No prior issues or PRs about SkAndroidCodec were found.

### Key Signals

- "how do i expose SkAndroidCodec in C# ?" — **issue body** (The reporter is asking if or how SkAndroidCodec can be accessed from C#, not reporting a bug.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCodec.cs` | — | direct | SkiaSharp exposes SKCodec wrapping the Skia SkCodec class. No SKAndroidCodec class exists anywhere in the binding directory. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | direct | The generated C# P/Invoke layer contains sk_codec_* functions but no sk_android_codec_* functions, confirming SkAndroidCodec is not exposed via the C API shim. |

### Resolution Proposals

**Hypothesis:** SkAndroidCodec is not exposed in SkiaSharp. To expose it, a new C API shim (sk_android_codec_t) and C# class (SKAndroidCodec) would need to be added. For now, the reporter can use SKCodec which wraps the base SkCodec class.

1. **Use SKCodec instead of SKAndroidCodec** — workaround, cost/xs, validated=yes
   - SKCodec (wrapping SkCodec) is available cross-platform including Android. Most functionality of SkAndroidCodec is accessible via SKCodec.

```csharp
using var stream = new SKFileStream("image.jpg");
using var codec = SKCodec.Create(stream);
var info = codec.Info;
var scaledDims = codec.GetScaledDimensions(0.5f);
```
2. **Add SKAndroidCodec binding** — fix, cost/l, validated=untested
   - Add a C API shim (sk_android_codec_*) in externals/skia/src/c/ and a C# wrapper class SKAndroidCodec in binding/SkiaSharp/. This is a non-trivial effort requiring native build changes.

**Recommended proposal:** workaround-1

**Why:** SKCodec covers the most common use cases and is available now without any changes.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.75 (75%) |
| Reason | The issue is a bare one-line question with no details about what specific SkAndroidCodec functionality is needed. A comment answering the question and asking what specific features are required would clarify whether this is a valid feature request or can be closed. |
| Suggested repro platform | linux |

### Missing Info

- What specific SkAndroidCodec functionality does the reporter need that is not available via SKCodec?
- What is the use case / Android API level target?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type and area labels | labels=type/feature-request, area/SkiaSharp, os/Android |
| add-comment | medium | 0.80 (80%) | Answer the question and ask for more detail about what specific SkAndroidCodec features are needed | — |

**Comment draft for `add-comment`:**

```markdown
Hi @mgood7123,

SkAndroidCodec is not currently exposed in SkiaSharp. The library wraps SkCodec as [`SKCodec`](https://learn.microsoft.com/dotnet/api/skiasharp.skcodec), which provides the base codec functionality across all platforms including Android.

**Workaround using SKCodec:**

```csharp
using var stream = new SKFileStream("image.jpg");
using var codec = SKCodec.Create(stream);
var info = codec.Info;
// Get scaled dimensions (like SkAndroidCodec.getSampledDimensions)
var scaledDims = codec.GetScaledDimensions(0.5f);
```

Could you share what specific functionality from `SkAndroidCodec` you need that `SKCodec` does not provide? That would help determine whether a new binding is worth pursuing.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2018,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-03T20:33:15Z"
  },
  "summary": "Reporter asks how to expose the Android-only SkAndroidCodec Skia API in C# via SkiaSharp.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Android"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [],
      "attachments": []
    }
  },
  "analysis": {
    "summary": "The reporter wants to use the Android-specific SkAndroidCodec Skia class from C#. SkiaSharp currently only exposes SKCodec (wrapping SkCodec), not SkAndroidCodec. SkAndroidCodec provides Android-specific features like getting scaled dimensions from Android's BitmapFactory options. This API is not wrapped in SkiaSharp's C API shim or C# bindings.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "finding": "SkiaSharp exposes SKCodec wrapping the Skia SkCodec class. No SKAndroidCodec class exists anywhere in the binding directory.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "The generated C# P/Invoke layer contains sk_codec_* functions but no sk_android_codec_* functions, confirming SkAndroidCodec is not exposed via the C API shim.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "how do i expose SkAndroidCodec in C# ?",
        "source": "issue body",
        "interpretation": "The reporter is asking if or how SkAndroidCodec can be accessed from C#, not reporting a bug."
      }
    ],
    "rationale": "The issue title includes [QUESTION] and the body is a single question with no repro code. SkAndroidCodec is an Android-only Skia class that is not currently wrapped in the SkiaSharp C API or C# bindings. Classifying as a feature-request because the reporter is ultimately asking for this API to be available — answering the question requires either implementing the binding or documenting that it doesn't exist. No prior issues or PRs about SkAndroidCodec were found.",
    "resolution": {
      "hypothesis": "SkAndroidCodec is not exposed in SkiaSharp. To expose it, a new C API shim (sk_android_codec_t) and C# class (SKAndroidCodec) would need to be added. For now, the reporter can use SKCodec which wraps the base SkCodec class.",
      "proposals": [
        {
          "title": "Use SKCodec instead of SKAndroidCodec",
          "description": "SKCodec (wrapping SkCodec) is available cross-platform including Android. Most functionality of SkAndroidCodec is accessible via SKCodec.",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "yes",
          "codeSnippet": "using var stream = new SKFileStream(\"image.jpg\");\nusing var codec = SKCodec.Create(stream);\nvar info = codec.Info;\nvar scaledDims = codec.GetScaledDimensions(0.5f);"
        },
        {
          "title": "Add SKAndroidCodec binding",
          "description": "Add a C API shim (sk_android_codec_*) in externals/skia/src/c/ and a C# wrapper class SKAndroidCodec in binding/SkiaSharp/. This is a non-trivial effort requiring native build changes.",
          "category": "fix",
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "workaround-1",
      "recommendedReason": "SKCodec covers the most common use cases and is available now without any changes."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.75,
      "reason": "The issue is a bare one-line question with no details about what specific SkAndroidCodec functionality is needed. A comment answering the question and asking what specific features are required would clarify whether this is a valid feature request or can be closed.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "What specific SkAndroidCodec functionality does the reporter need that is not available via SKCodec?",
      "What is the use case / Android API level target?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type and area labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "os/Android"
        ]
      },
      {
        "type": "add-comment",
        "description": "Answer the question and ask for more detail about what specific SkAndroidCodec features are needed",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Hi @mgood7123,\n\nSkAndroidCodec is not currently exposed in SkiaSharp. The library wraps SkCodec as [`SKCodec`](https://learn.microsoft.com/dotnet/api/skiasharp.skcodec), which provides the base codec functionality across all platforms including Android.\n\n**Workaround using SKCodec:**\n\n```csharp\nusing var stream = new SKFileStream(\"image.jpg\");\nusing var codec = SKCodec.Create(stream);\nvar info = codec.Info;\n// Get scaled dimensions (like SkAndroidCodec.getSampledDimensions)\nvar scaledDims = codec.GetScaledDimensions(0.5f);\n```\n\nCould you share what specific functionality from `SkAndroidCodec` you need that `SKCodec` does not provide? That would help determine whether a new binding is worth pursuing."
      }
    ]
  }
}
```

</details>
