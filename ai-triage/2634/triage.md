# Issue Triage Report — #2634

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T13:45:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-investigation (0.90 (90%)) |

**Issue Summary:** SKBitmap.Decode called with a Span<byte> and an SKImageInfo with Width=0 and Height=0 causes a native access violation (process crash) in SkiaSharp 2.88.6; the same call returned null in 2.88.5.

**Analysis:** In 2.88.6, SKBitmap.Decode(byte[], SKImageInfo) was changed to delegate to a new Decode(ReadOnlySpan<byte>, SKImageInfo) overload. That overload uses 'fixed' + SKData.Create(IntPtr, int) (wrapping, not copying) to avoid a data copy. When the caller passes an SKImageInfo with Width=0 and Height=0 (default-initialised), SKBitmap is allocated with a 0-byte pixel buffer, and codec.GetPixels is called with an invalid zero-dimension info, triggering a native access violation. Additionally, a related crash is reported in SKCodec.GetScaledDimensions in 2.88.6, suggesting a broader issue with the native codec handling edge-case inputs introduced in this release.

**Recommendations:** **needs-investigation** — Clear regression with complete repro code on Windows and Linux. Root cause hypothesis is strong but exact mechanism (zero-dimension vs fixed-pointer lifetime) needs confirmation via reproduction before fixing.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic, os/Linux |
| Backends | — |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create a console app targeting .NET 6
2. Add SkiaSharp 2.88.6 NuGet package
3. Decode a base64-encoded JPEG into a ReadOnlySpan<byte>
4. Call SKBitmap.Decode(bytes, new SKImageInfo { AlphaType=Premul, ColorType=Bgra8888 }) — note Width and Height default to 0
5. Observe process crash (AccessViolationException / SIGSEGV)

**Environment:** .NET 6, SkiaSharp 2.88.6, Windows 11 and Ubuntu

**Related issues:** #2645, #2681

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2634#issuecomment-1758090955 — Comment: same crash from SKCodec.GetScaledDimensions(0.4f) on Windows, 2.88.6 only
- https://github.com/mono/SkiaSharp/issues/2634#issuecomment-1838523311 — Comment: SIGSEGV on Linux .NET Core 8 docker and Windows, intermittent, links to #2681 and #2645

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | crash |
| Error message | System.AccessViolationException / SIGSEGV during SKBitmap.Decode or SKCodec.GetScaledDimensions |
| Repro quality | complete |
| Target frameworks | net6.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.1, 2.88.5, 2.88.6 |
| Worked in | 2.88.5 |
| Broke in | 2.88.6 |
| Current relevance | likely |
| Relevance reason | Issue is still open, no fix PR found; Span-based overloads introduced in 2.88.6 remain in current codebase. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.97 (97%) |
| Reason | Reporter explicitly tested 2.88.5 vs 2.88.6. Multiple users confirm the same behavior. The Span-based Decode overloads were introduced in 2.88.6. |
| Worked in version | 2.88.5 |
| Broke in version | 2.88.6 |

## Analysis

### Technical Summary

In 2.88.6, SKBitmap.Decode(byte[], SKImageInfo) was changed to delegate to a new Decode(ReadOnlySpan<byte>, SKImageInfo) overload. That overload uses 'fixed' + SKData.Create(IntPtr, int) (wrapping, not copying) to avoid a data copy. When the caller passes an SKImageInfo with Width=0 and Height=0 (default-initialised), SKBitmap is allocated with a 0-byte pixel buffer, and codec.GetPixels is called with an invalid zero-dimension info, triggering a native access violation. Additionally, a related crash is reported in SKCodec.GetScaledDimensions in 2.88.6, suggesting a broader issue with the native codec handling edge-case inputs introduced in this release.

### Rationale

This is unambiguously a regression bug: it worked in 2.88.5 and crashes in 2.88.6, reproduced by multiple users on Windows and Linux. The root code path is SKBitmap.Decode(ReadOnlySpan<byte>, SKImageInfo), which lacks validation for degenerate (zero-dimension) SKImageInfo. The fix is to validate bitmapInfo dimensions before attempting to decode, mirroring the behaviour of pre-2.88.6 which would return null. Classified as area/SkiaSharp because the issue is in the managed binding layer.

### Key Signals

- "the application will die on bitmapNoLongerWorks" — **issue body** (Native crash (not a managed exception) during or after the decode attempt.)
- "If you go back to 2.88.5 or earlier, it will continue with bitmapNoLongerWorks as null." — **issue body** (Pre-2.88.6 code returned null gracefully for invalid bitmapInfo; 2.88.6 crashes instead.)
- "SKCodec.GetScaledDimensions(float) ... package 2.88.6 crashes with the error System.AccessViolationException" — **issue comment #1758090955 by mpbraithwaite** (Same 2.88.5→2.88.6 regression manifesting in a different SKCodec method, pointing to a native-layer change.)
- "SIGSEGV in Net Core 8 docker image (mcr.microsoft.com/dotnet/aspnet:8.0) as well as Windows" — **issue comment #1838523311 by MateuszBogdan** (Crash is cross-platform and cross-runtime, confirming native root cause.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 580-587 | direct | Decode(ReadOnlySpan<byte>, SKImageInfo) uses 'fixed' to pin the span and SKData.Create (no copy) to wrap the pointer, then calls Decode(codec, bitmapInfo) without validating bitmapInfo dimensions. When Width=0 and Height=0, a 0-byte bitmap is created and the codec is given a null/invalid pixel pointer. |
| `binding/SkiaSharp/SKBitmap.cs` | 446-459 | direct | Decode(SKCodec, SKImageInfo): creates 'new SKBitmap(bitmapInfo)' which succeeds even for 0x0 info, then calls codec.GetPixels with the potentially zero/invalid pixel pointer. No guard against degenerate bitmapInfo. |
| `binding/SkiaSharp/SKCodec.cs` | 119-136 | related | GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) checks for IntPtr.Zero and throws ArgumentNullException — however, for a 0x0 bitmap the native allocator may return a non-null but invalid pointer, bypassing this check and crashing in native code. |
| `binding/SkiaSharp/SKCodec.cs` | 42-47 | related | GetScaledDimensions(float) delegates directly to sk_codec_get_scaled_dimensions with no validation; related crash reported in #2645 also regressed from 2.88.5 to 2.88.6, suggesting a common root cause in the native codec. |

**Error fingerprint:** `SKBitmap.Decode+zero-dimension-SKImageInfo+AccessViolation+2.88.6`

### Next Questions

- Does passing Width>0 and Height>0 but mismatched to the image also crash, or only Width=Height=0?
- Was SKData.Create (wrapping, no copy) introduced in 2.88.6 replacing SKData.CreateCopy?
- Is SKCodec.GetScaledDimensions crash (#2645) the same root cause or a separate native issue?
- Does the crash occur during the Decode call or only at process teardown (finalizer thread)?

### Resolution Proposals

**Hypothesis:** SKBitmap.Decode(SKCodec, SKImageInfo) does not guard against degenerate (zero-dimension) bitmapInfo. When Width=0 or Height=0, the native codec is invoked with an invalid pixel buffer, causing a crash. The fix is to validate bitmapInfo before allocating and decoding.

1. **Guard against zero-dimension bitmapInfo in Decode(SKCodec, SKImageInfo)** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - Add a check at the top of Decode(SKCodec, SKImageInfo): if bitmapInfo.Width <= 0 or bitmapInfo.Height <= 0, return null immediately, restoring pre-2.88.6 behaviour.

```csharp
if (bitmapInfo.Width <= 0 || bitmapInfo.Height <= 0)
    return null;
```
2. **Use SKData.CreateCopy instead of SKData.Create in Span-based overloads** — fix, confidence 0.75 (75%), cost/xs, validated=untested
   - Replace SKData.Create((IntPtr)b, buffer.Length) with SKData.CreateCopy in Decode(ReadOnlySpan<byte>) and Decode(ReadOnlySpan<byte>, SKImageInfo) to eliminate the unsafe pointer-wrapping pattern introduced in 2.88.6. This removes the fixed-block lifetime concern at the cost of one extra copy.
3. **Workaround: always pass explicit non-zero dimensions** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Until a fix is released, callers should always pass the image's actual dimensions in the SKImageInfo, retrieved first via SKBitmap.DecodeBounds().

```csharp
var info = SKBitmap.DecodeBounds(bytes);
if (info.IsEmpty) return null;
var bitmap = SKBitmap.Decode(bytes, info);
```

**Recommended proposal:** Guard against zero-dimension bitmapInfo in Decode(SKCodec, SKImageInfo)

**Why:** Smallest targeted fix that restores the pre-2.88.6 null-return contract. Does not change memory semantics.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.90 (90%) |
| Reason | Clear regression with complete repro code on Windows and Linux. Root cause hypothesis is strong but exact mechanism (zero-dimension vs fixed-pointer lifetime) needs confirmation via reproduction before fixing. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/bug, area/SkiaSharp, os/Windows, os/Linux, tenet/reliability, tenet/compatibility labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, os/Linux, tenet/reliability, tenet/compatibility |
| link-related | low | 0.85 (85%) | Link to related issue #2645 (same SKCodec crash, same regression) | linkedIssue=#2645 |
| link-related | low | 0.75 (75%) | Link to related issue #2681 (AccessViolationException, possibly same root cause) | linkedIssue=#2681 |
| add-comment | medium | 0.85 (85%) | Acknowledge regression, share workaround, request confirmation of zero-dimension theory | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro!

This looks like a regression introduced in 2.88.6 in the `SKBitmap.Decode(ReadOnlySpan<byte>, SKImageInfo)` overload. The crash is likely triggered because the `SKImageInfo` in your repro has `Width = 0` and `Height = 0` (default values — you only set `AlphaType` and `ColorType`). The codec then tries to write pixel data into a zero-size buffer, causing a native access violation.

**Workaround until a fix is released:**

```csharp
// First get the actual dimensions
var imageInfo = SKBitmap.DecodeBounds(bytes);
if (imageInfo.IsEmpty)
    return null;

// Then decode with full info (or return null if you expected null)
var bitmap = SKBitmap.Decode(bytes, imageInfo);
```

This crash also appears to be related to #2645 (SKCodec.GetScaledDimensions crash) — both are regressions in 2.88.6.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2634,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T13:45:00Z"
  },
  "summary": "SKBitmap.Decode called with a Span<byte> and an SKImageInfo with Width=0 and Height=0 causes a native access violation (process crash) in SkiaSharp 2.88.6; the same call returned null in 2.88.5.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic",
      "os/Linux"
    ],
    "tenets": [
      "tenet/reliability",
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "crash",
      "errorMessage": "System.AccessViolationException / SIGSEGV during SKBitmap.Decode or SKCodec.GetScaledDimensions",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a console app targeting .NET 6",
        "Add SkiaSharp 2.88.6 NuGet package",
        "Decode a base64-encoded JPEG into a ReadOnlySpan<byte>",
        "Call SKBitmap.Decode(bytes, new SKImageInfo { AlphaType=Premul, ColorType=Bgra8888 }) — note Width and Height default to 0",
        "Observe process crash (AccessViolationException / SIGSEGV)"
      ],
      "environmentDetails": ".NET 6, SkiaSharp 2.88.6, Windows 11 and Ubuntu",
      "relatedIssues": [
        2645,
        2681
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2634#issuecomment-1758090955",
          "description": "Comment: same crash from SKCodec.GetScaledDimensions(0.4f) on Windows, 2.88.6 only"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2634#issuecomment-1838523311",
          "description": "Comment: SIGSEGV on Linux .NET Core 8 docker and Windows, intermittent, links to #2681 and #2645"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.1",
        "2.88.5",
        "2.88.6"
      ],
      "workedIn": "2.88.5",
      "brokeIn": "2.88.6",
      "currentRelevance": "likely",
      "relevanceReason": "Issue is still open, no fix PR found; Span-based overloads introduced in 2.88.6 remain in current codebase."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.97,
      "reason": "Reporter explicitly tested 2.88.5 vs 2.88.6. Multiple users confirm the same behavior. The Span-based Decode overloads were introduced in 2.88.6.",
      "workedInVersion": "2.88.5",
      "brokeInVersion": "2.88.6"
    }
  },
  "analysis": {
    "summary": "In 2.88.6, SKBitmap.Decode(byte[], SKImageInfo) was changed to delegate to a new Decode(ReadOnlySpan<byte>, SKImageInfo) overload. That overload uses 'fixed' + SKData.Create(IntPtr, int) (wrapping, not copying) to avoid a data copy. When the caller passes an SKImageInfo with Width=0 and Height=0 (default-initialised), SKBitmap is allocated with a 0-byte pixel buffer, and codec.GetPixels is called with an invalid zero-dimension info, triggering a native access violation. Additionally, a related crash is reported in SKCodec.GetScaledDimensions in 2.88.6, suggesting a broader issue with the native codec handling edge-case inputs introduced in this release.",
    "rationale": "This is unambiguously a regression bug: it worked in 2.88.5 and crashes in 2.88.6, reproduced by multiple users on Windows and Linux. The root code path is SKBitmap.Decode(ReadOnlySpan<byte>, SKImageInfo), which lacks validation for degenerate (zero-dimension) SKImageInfo. The fix is to validate bitmapInfo dimensions before attempting to decode, mirroring the behaviour of pre-2.88.6 which would return null. Classified as area/SkiaSharp because the issue is in the managed binding layer.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "580-587",
        "finding": "Decode(ReadOnlySpan<byte>, SKImageInfo) uses 'fixed' to pin the span and SKData.Create (no copy) to wrap the pointer, then calls Decode(codec, bitmapInfo) without validating bitmapInfo dimensions. When Width=0 and Height=0, a 0-byte bitmap is created and the codec is given a null/invalid pixel pointer.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "446-459",
        "finding": "Decode(SKCodec, SKImageInfo): creates 'new SKBitmap(bitmapInfo)' which succeeds even for 0x0 info, then calls codec.GetPixels with the potentially zero/invalid pixel pointer. No guard against degenerate bitmapInfo.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "119-136",
        "finding": "GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) checks for IntPtr.Zero and throws ArgumentNullException — however, for a 0x0 bitmap the native allocator may return a non-null but invalid pointer, bypassing this check and crashing in native code.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKCodec.cs",
        "lines": "42-47",
        "finding": "GetScaledDimensions(float) delegates directly to sk_codec_get_scaled_dimensions with no validation; related crash reported in #2645 also regressed from 2.88.5 to 2.88.6, suggesting a common root cause in the native codec.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "the application will die on bitmapNoLongerWorks",
        "source": "issue body",
        "interpretation": "Native crash (not a managed exception) during or after the decode attempt."
      },
      {
        "text": "If you go back to 2.88.5 or earlier, it will continue with bitmapNoLongerWorks as null.",
        "source": "issue body",
        "interpretation": "Pre-2.88.6 code returned null gracefully for invalid bitmapInfo; 2.88.6 crashes instead."
      },
      {
        "text": "SKCodec.GetScaledDimensions(float) ... package 2.88.6 crashes with the error System.AccessViolationException",
        "source": "issue comment #1758090955 by mpbraithwaite",
        "interpretation": "Same 2.88.5→2.88.6 regression manifesting in a different SKCodec method, pointing to a native-layer change."
      },
      {
        "text": "SIGSEGV in Net Core 8 docker image (mcr.microsoft.com/dotnet/aspnet:8.0) as well as Windows",
        "source": "issue comment #1838523311 by MateuszBogdan",
        "interpretation": "Crash is cross-platform and cross-runtime, confirming native root cause."
      }
    ],
    "nextQuestions": [
      "Does passing Width>0 and Height>0 but mismatched to the image also crash, or only Width=Height=0?",
      "Was SKData.Create (wrapping, no copy) introduced in 2.88.6 replacing SKData.CreateCopy?",
      "Is SKCodec.GetScaledDimensions crash (#2645) the same root cause or a separate native issue?",
      "Does the crash occur during the Decode call or only at process teardown (finalizer thread)?"
    ],
    "errorFingerprint": "SKBitmap.Decode+zero-dimension-SKImageInfo+AccessViolation+2.88.6",
    "resolution": {
      "hypothesis": "SKBitmap.Decode(SKCodec, SKImageInfo) does not guard against degenerate (zero-dimension) bitmapInfo. When Width=0 or Height=0, the native codec is invoked with an invalid pixel buffer, causing a crash. The fix is to validate bitmapInfo before allocating and decoding.",
      "proposals": [
        {
          "title": "Guard against zero-dimension bitmapInfo in Decode(SKCodec, SKImageInfo)",
          "description": "Add a check at the top of Decode(SKCodec, SKImageInfo): if bitmapInfo.Width <= 0 or bitmapInfo.Height <= 0, return null immediately, restoring pre-2.88.6 behaviour.",
          "category": "fix",
          "codeSnippet": "if (bitmapInfo.Width <= 0 || bitmapInfo.Height <= 0)\n    return null;",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Use SKData.CreateCopy instead of SKData.Create in Span-based overloads",
          "description": "Replace SKData.Create((IntPtr)b, buffer.Length) with SKData.CreateCopy in Decode(ReadOnlySpan<byte>) and Decode(ReadOnlySpan<byte>, SKImageInfo) to eliminate the unsafe pointer-wrapping pattern introduced in 2.88.6. This removes the fixed-block lifetime concern at the cost of one extra copy.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Workaround: always pass explicit non-zero dimensions",
          "description": "Until a fix is released, callers should always pass the image's actual dimensions in the SKImageInfo, retrieved first via SKBitmap.DecodeBounds().",
          "category": "workaround",
          "codeSnippet": "var info = SKBitmap.DecodeBounds(bytes);\nif (info.IsEmpty) return null;\nvar bitmap = SKBitmap.Decode(bytes, info);",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Guard against zero-dimension bitmapInfo in Decode(SKCodec, SKImageInfo)",
      "recommendedReason": "Smallest targeted fix that restores the pre-2.88.6 null-return contract. Does not change memory semantics."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.9,
      "reason": "Clear regression with complete repro code on Windows and Linux. Root cause hypothesis is strong but exact mechanism (zero-dimension vs fixed-pointer lifetime) needs confirmation via reproduction before fixing.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Windows, os/Linux, tenet/reliability, tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "os/Linux",
          "tenet/reliability",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to related issue #2645 (same SKCodec crash, same regression)",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 2645
      },
      {
        "type": "link-related",
        "description": "Link to related issue #2681 (AccessViolationException, possibly same root cause)",
        "risk": "low",
        "confidence": 0.75,
        "linkedIssue": 2681
      },
      {
        "type": "add-comment",
        "description": "Acknowledge regression, share workaround, request confirmation of zero-dimension theory",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed repro!\n\nThis looks like a regression introduced in 2.88.6 in the `SKBitmap.Decode(ReadOnlySpan<byte>, SKImageInfo)` overload. The crash is likely triggered because the `SKImageInfo` in your repro has `Width = 0` and `Height = 0` (default values — you only set `AlphaType` and `ColorType`). The codec then tries to write pixel data into a zero-size buffer, causing a native access violation.\n\n**Workaround until a fix is released:**\n\n```csharp\n// First get the actual dimensions\nvar imageInfo = SKBitmap.DecodeBounds(bytes);\nif (imageInfo.IsEmpty)\n    return null;\n\n// Then decode with full info (or return null if you expected null)\nvar bitmap = SKBitmap.Decode(bytes, imageInfo);\n```\n\nThis crash also appears to be related to #2645 (SKCodec.GetScaledDimensions crash) — both are regressions in 2.88.6."
      }
    ]
  }
}
```

</details>
