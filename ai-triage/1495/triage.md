# Issue Triage Report — #1495

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T22:00:55Z |
| Type | type/question (0.85 (85%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** Reporter gets an exception when resizing an SKBitmap using an SKImageInfo created via property initializer syntax (omitting ColorType/AlphaType), while using the parameterized constructor works fine.

**Analysis:** The issue is caused by C# struct semantics: using `new SKImageInfo { Width = w, Height = h }` leaves ColorType and AlphaType at their default zero values (SKColorType.Unknown / SKAlphaType.Unknown). The SKBitmap constructor then fails to allocate pixels because ColorType.Unknown has 0 bytes-per-pixel, causing RowBytes=0 and allocation failure. The parameterized constructor `new SKImageInfo(w, h)` correctly sets ColorType=PlatformColorType and AlphaType=Premul. This is by-design C# struct behavior.

**Recommendations:** **close-as-not-a-bug** — Behavior is by-design C# struct semantics. Maintainer has already explained this in comments. Parameterized constructor is the correct API and works as expected.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Android |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create an SKImageInfo using object initializer: new SKImageInfo { Width = w, Height = h }
2. Call bitmap.Resize(info, SKFilterQuality.High)
3. Observe exception (allocation failure)

**Environment:** SkiaSharp 1.68.3, Xamarin.Android, minSdk 23, targetSdk 28, Visual Studio simulator

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKImageInfo is a struct and the property-initializer behavior has not changed; object initializer still bypasses constructors that set ColorType/AlphaType defaults. |

## Analysis

### Technical Summary

The issue is caused by C# struct semantics: using `new SKImageInfo { Width = w, Height = h }` leaves ColorType and AlphaType at their default zero values (SKColorType.Unknown / SKAlphaType.Unknown). The SKBitmap constructor then fails to allocate pixels because ColorType.Unknown has 0 bytes-per-pixel, causing RowBytes=0 and allocation failure. The parameterized constructor `new SKImageInfo(w, h)` correctly sets ColorType=PlatformColorType and AlphaType=Premul. This is by-design C# struct behavior.

### Rationale

Classified as type/question because the behavior is by-design: the parameterless struct initializer does not invoke any constructor, so ColorType and AlphaType remain at their enum default value of 0 (Unknown). The maintainer already explained this in a comment and noted better exception messages would help. No code defect exists in SkiaSharp; the fix is user education about struct initialization and providing clearer error messages.

### Key Signals

- "bitmap.Resize(info, SKFilterQuality.High) // this line causes the exception" — **issue body** (Exception during Resize when SKImageInfo is created via property initializer without ColorType/AlphaType.)
- "doing it like this works: bitmap.Resize(new SKImageInfo((int)..., (int)...), SKFilterQuality.High)" — **issue body** (Parameterized constructor sets PlatformColorType and Premul defaults, property initializer does not.)
- "I probably need better exception messages" — **maintainer comment #707385221** (Maintainer acknowledges the poor error message is a UX issue; the underlying behavior is by-design.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImageInfo.cs` | 37-70 | direct | SKImageInfo struct has constructors SKImageInfo(int width, int height) and overloads that set ColorType=PlatformColorType and AlphaType=Premul. The parameterless struct default leaves ColorType=Unknown(0) and AlphaType=Unknown(0). Using a C# object initializer invokes default struct initialization, not any of these constructors. |
| `binding/SkiaSharp/SKBitmap.cs` | 47-57 | direct | SKBitmap(SKImageInfo info) calls TryAllocPixels(info, info.RowBytes). When ColorType=Unknown, BytesPerPixel=0 so RowBytes=0, causing TryAllocPixels to fail and throw 'Unable to allocate pixels'. |
| `binding/SkiaSharp/SKBitmap.cs` | 664-676 | related | Resize(SKImageInfo info, SKSamplingOptions sampling) checks info.IsEmpty (Width<=0 || Height<=0) but does NOT validate ColorType or AlphaType before allocating. Passes through to new SKBitmap(info) which then throws. |

### Workarounds

- Use the parameterized constructor: new SKImageInfo(width, height) which sets PlatformColorType and Premul defaults.
- Explicitly set ColorType and AlphaType in the object initializer: new SKImageInfo { Width = w, Height = h, ColorType = SKImageInfo.PlatformColorType, AlphaType = SKAlphaType.Premul }

### Resolution Proposals

**Hypothesis:** User is unaware that C# object initializer syntax on a struct doesn't invoke any constructor, leaving ColorType/AlphaType at Unknown. The use of the parameterized constructor is the correct approach.

1. **Use parameterized constructor** — workaround, confidence 0.97 (97%), cost/xs, validated=yes
   - Replace the object initializer with the two-parameter constructor which sets defaults automatically.

**Recommended proposal:** Use parameterized constructor

**Why:** One-line fix with no side effects. The constructor already sets PlatformColorType and Premul, which is what the user expects.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | Behavior is by-design C# struct semantics. Maintainer has already explained this in comments. Parameterized constructor is the correct API and works as expected. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question, SkiaSharp, Android labels | labels=type/question, area/SkiaSharp, os/Android |
| add-comment | high | 0.88 (88%) | Post answer explaining struct initialization behavior and provide workaround | — |
| close-issue | medium | 0.80 (80%) | Close as answered / not a bug | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! This is expected C# struct behaviour. When you use an object initializer like `new SKImageInfo { Width = w, Height = h }`, C# does *not* call any constructor — it uses the default struct value, which leaves `ColorType` as `SKColorType.Unknown` and `AlphaType` as `SKAlphaType.Unknown`. With `Unknown` color type, `BytesPerPixel` is 0, so pixel allocation fails.

The two-parameter constructor (`new SKImageInfo(width, height)`) automatically sets `ColorType = SKImageInfo.PlatformColorType` and `AlphaType = SKAlphaType.Premul`, which is what you need for a valid bitmap.

**Fix:** Replace the object initializer with the constructor:
```csharp
SKImageInfo info = new SKImageInfo(
    (int)Math.Floor(bitmap.Width * 0.25),
    (int)Math.Floor(bitmap.Height * 0.25)
);
bitmap = bitmap.Resize(info, SKFilterQuality.High);
```

Or, if you need the object initializer pattern:
```csharp
SKImageInfo info = new SKImageInfo
{
    Width = (int)Math.Floor(bitmap.Width * 0.25),
    Height = (int)Math.Floor(bitmap.Height * 0.25),
    ColorType = SKImageInfo.PlatformColorType,
    AlphaType = SKAlphaType.Premul
};
```
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1495,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T22:00:55Z"
  },
  "summary": "Reporter gets an exception when resizing an SKBitmap using an SKImageInfo created via property initializer syntax (omitting ColorType/AlphaType), while using the parameterized constructor works fine.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Android"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKImageInfo using object initializer: new SKImageInfo { Width = w, Height = h }",
        "Call bitmap.Resize(info, SKFilterQuality.High)",
        "Observe exception (allocation failure)"
      ],
      "environmentDetails": "SkiaSharp 1.68.3, Xamarin.Android, minSdk 23, targetSdk 28, Visual Studio simulator",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKImageInfo is a struct and the property-initializer behavior has not changed; object initializer still bypasses constructors that set ColorType/AlphaType defaults."
    }
  },
  "analysis": {
    "summary": "The issue is caused by C# struct semantics: using `new SKImageInfo { Width = w, Height = h }` leaves ColorType and AlphaType at their default zero values (SKColorType.Unknown / SKAlphaType.Unknown). The SKBitmap constructor then fails to allocate pixels because ColorType.Unknown has 0 bytes-per-pixel, causing RowBytes=0 and allocation failure. The parameterized constructor `new SKImageInfo(w, h)` correctly sets ColorType=PlatformColorType and AlphaType=Premul. This is by-design C# struct behavior.",
    "rationale": "Classified as type/question because the behavior is by-design: the parameterless struct initializer does not invoke any constructor, so ColorType and AlphaType remain at their enum default value of 0 (Unknown). The maintainer already explained this in a comment and noted better exception messages would help. No code defect exists in SkiaSharp; the fix is user education about struct initialization and providing clearer error messages.",
    "keySignals": [
      {
        "text": "bitmap.Resize(info, SKFilterQuality.High) // this line causes the exception",
        "source": "issue body",
        "interpretation": "Exception during Resize when SKImageInfo is created via property initializer without ColorType/AlphaType."
      },
      {
        "text": "doing it like this works: bitmap.Resize(new SKImageInfo((int)..., (int)...), SKFilterQuality.High)",
        "source": "issue body",
        "interpretation": "Parameterized constructor sets PlatformColorType and Premul defaults, property initializer does not."
      },
      {
        "text": "I probably need better exception messages",
        "source": "maintainer comment #707385221",
        "interpretation": "Maintainer acknowledges the poor error message is a UX issue; the underlying behavior is by-design."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "37-70",
        "finding": "SKImageInfo struct has constructors SKImageInfo(int width, int height) and overloads that set ColorType=PlatformColorType and AlphaType=Premul. The parameterless struct default leaves ColorType=Unknown(0) and AlphaType=Unknown(0). Using a C# object initializer invokes default struct initialization, not any of these constructors.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "47-57",
        "finding": "SKBitmap(SKImageInfo info) calls TryAllocPixels(info, info.RowBytes). When ColorType=Unknown, BytesPerPixel=0 so RowBytes=0, causing TryAllocPixels to fail and throw 'Unable to allocate pixels'.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "664-676",
        "finding": "Resize(SKImageInfo info, SKSamplingOptions sampling) checks info.IsEmpty (Width<=0 || Height<=0) but does NOT validate ColorType or AlphaType before allocating. Passes through to new SKBitmap(info) which then throws.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use the parameterized constructor: new SKImageInfo(width, height) which sets PlatformColorType and Premul defaults.",
      "Explicitly set ColorType and AlphaType in the object initializer: new SKImageInfo { Width = w, Height = h, ColorType = SKImageInfo.PlatformColorType, AlphaType = SKAlphaType.Premul }"
    ],
    "resolution": {
      "hypothesis": "User is unaware that C# object initializer syntax on a struct doesn't invoke any constructor, leaving ColorType/AlphaType at Unknown. The use of the parameterized constructor is the correct approach.",
      "proposals": [
        {
          "title": "Use parameterized constructor",
          "description": "Replace the object initializer with the two-parameter constructor which sets defaults automatically.",
          "category": "workaround",
          "confidence": 0.97,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use parameterized constructor",
      "recommendedReason": "One-line fix with no side effects. The constructor already sets PlatformColorType and Premul, which is what the user expects."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "Behavior is by-design C# struct semantics. Maintainer has already explained this in comments. Parameterized constructor is the correct API and works as expected.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, SkiaSharp, Android labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Android"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer explaining struct initialization behavior and provide workaround",
        "risk": "high",
        "confidence": 0.88,
        "comment": "Thanks for the report! This is expected C# struct behaviour. When you use an object initializer like `new SKImageInfo { Width = w, Height = h }`, C# does *not* call any constructor — it uses the default struct value, which leaves `ColorType` as `SKColorType.Unknown` and `AlphaType` as `SKAlphaType.Unknown`. With `Unknown` color type, `BytesPerPixel` is 0, so pixel allocation fails.\n\nThe two-parameter constructor (`new SKImageInfo(width, height)`) automatically sets `ColorType = SKImageInfo.PlatformColorType` and `AlphaType = SKAlphaType.Premul`, which is what you need for a valid bitmap.\n\n**Fix:** Replace the object initializer with the constructor:\n```csharp\nSKImageInfo info = new SKImageInfo(\n    (int)Math.Floor(bitmap.Width * 0.25),\n    (int)Math.Floor(bitmap.Height * 0.25)\n);\nbitmap = bitmap.Resize(info, SKFilterQuality.High);\n```\n\nOr, if you need the object initializer pattern:\n```csharp\nSKImageInfo info = new SKImageInfo\n{\n    Width = (int)Math.Floor(bitmap.Width * 0.25),\n    Height = (int)Math.Floor(bitmap.Height * 0.25),\n    ColorType = SKImageInfo.PlatformColorType,\n    AlphaType = SKAlphaType.Premul\n};\n```"
      },
      {
        "type": "close-issue",
        "description": "Close as answered / not a bug",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
