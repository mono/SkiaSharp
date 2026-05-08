# Issue Triage Report — #3421

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T10:46:03Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** Calling Animation.Create() with a .lottie binary file (dotLottie ZIP container) silently returns null because the Skottie C API only accepts JSON-formatted Lottie data, not the ZIP-based .lottie format.

**Analysis:** The .lottie format (dotLottie) is a ZIP archive containing a manifest.json and one or more Lottie JSON animation files. When Animation.Create(path) reads the file and passes the bytes to the C API skottie_animation_make_from_data, the Skottie JSON parser receives a ZIP binary (starting with 'PK\x03\x04') instead of JSON, fails to parse, and returns null. SkiaSharp provides no ZIP extraction, no format detection, and no error message, causing a silent failure.

**Recommendations:** **needs-investigation** — Root cause is clear (ZIP vs JSON format mismatch) but fix scope needs decision: minimal error detection vs full dotLottie container support. The reporter needs guidance on the workaround.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Windows-Classic, area/SkiaSharp, tenet/reliability |

## Evidence

### Reproduction

1. Download a .lottie file from LottieFiles (e.g. https://app.lottiefiles.com/animation/5b206184-3c40-4a29-a6c4-0f62df06e4a8)
2. Call SkiaSharp.Skottie.Animation.Create(path) with the absolute path to the .lottie file
3. Observe that the returned animation is null and no exception is thrown

**Environment:** SkiaSharp 3.116.0, Windows 11, Visual Studio

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2166 — Related: Skottie cannot load animations with BOM (null-return pattern — same root symptom)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | missing-output |
| Error message | Animation.Create returns null with no exception for a .lottie file |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Animation.Create code path has not changed — it still passes raw file bytes directly to skottie_animation_make_from_data which expects JSON. |

## Analysis

### Technical Summary

The .lottie format (dotLottie) is a ZIP archive containing a manifest.json and one or more Lottie JSON animation files. When Animation.Create(path) reads the file and passes the bytes to the C API skottie_animation_make_from_data, the Skottie JSON parser receives a ZIP binary (starting with 'PK\x03\x04') instead of JSON, fails to parse, and returns null. SkiaSharp provides no ZIP extraction, no format detection, and no error message, causing a silent failure.

### Rationale

The .lottie format (dotLottie, file extension .lottie) is a ZIP archive introduced by LottieFiles — it is distinct from the traditional JSON Lottie format (.json). The Skottie C API (skottie_animation_make_from_data) only understands JSON. SkiaSharp currently performs no format detection and passes raw bytes to the C API regardless of file format. This is a bug because the failure is silent and the API contract (Create returns null on parse failure) does not communicate the root cause to users. A proper fix would be either: (a) detect the ZIP magic bytes and throw an informative exception, or (b) implement dotLottie container support (extract JSON from ZIP). The issue is classified as medium severity because a workaround exists (use the .json format).

### Key Signals

- "var animation = SkiaSharp.Skottie.Animation.Create(sourceFileAbsolute);" — **issue body** (Reporter is using the file-path overload of Animation.Create with a .lottie file which is a ZIP, not JSON. The C API only accepts JSON.)
- "A null animation instance is returned instead." — **issue body** (Silent failure: no exception, no error message. The failure mode is indistinguishable from passing a valid but unsupported JSON file.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.Skottie/Animation.cs` | 91-102 | direct | Animation.Create(string path) calls SKData.Create(path) then TryCreate(SKData) which passes raw bytes directly to skottie_animation_make_from_data. No format detection for ZIP magic bytes. |
| `binding/SkiaSharp.Skottie/Animation.cs` | 77-89 | direct | TryCreate(SKData) strips UTF-8 BOM preamble (via GetPreambleSize) but has no handling for binary/ZIP content — the ZIP bytes are passed raw to the C API, which returns null. |
| `binding/SkiaSharp.Skottie/AnimationBuilder.cs` | 61-75 | related | AnimationBuilder.Build(SKData) follows the same pattern — passes raw bytes to skottie_animation_builder_make_from_data with no ZIP format detection. |

### Workarounds

- Download or export the animation in traditional .json (Lottie JSON) format from LottieFiles instead of .lottie format, then load with Animation.Create(path).
- If only the .lottie file is available, extract it with any ZIP library (e.g., System.IO.Compression.ZipFile), read the animation JSON from inside the archive, then call Animation.Parse(json) or Animation.Create(stream) with that JSON content.

### Next Questions

- Does Skia's upstream Skottie C++ have any built-in .lottie (dotLottie ZIP) support not yet exposed via the C API?
- Should SkiaSharp add dotLottie ZIP extraction in the managed binding, or should a new C API function be added?

### Resolution Proposals

**Hypothesis:** The .lottie file is a ZIP archive. Animation.Create passes raw bytes to the JSON parser, which returns null for non-JSON input. Adding ZIP magic byte detection would either throw a clear error or trigger ZIP extraction.

1. **Detect .lottie ZIP and throw informative exception** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - In TryCreate(SKData), check if the first 4 bytes match the ZIP magic (PK\x03\x04). If so, throw an InvalidOperationException explaining that .lottie (dotLottie) format is not supported and the user should use the .json format.
2. **Workaround: extract JSON from .lottie ZIP manually** — workaround, confidence 0.90 (90%), cost/s, validated=untested
   - Users can use System.IO.Compression.ZipFile to open the .lottie archive, enumerate entries, find the first .json entry matching the manifest, read its contents, and pass the JSON string to Animation.Parse().

**Recommended proposal:** Detect .lottie ZIP and throw informative exception

**Why:** Minimal change that converts the silent failure into an actionable error, unblocking the reporter immediately. Full dotLottie support is a larger feature request.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Root cause is clear (ZIP vs JSON format mismatch) but fix scope needs decision: minimal error detection vs full dotLottie container support. The reporter needs guidance on the workaround. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Confirm existing labels: type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.90 (90%) | Explain .lottie vs .json format difference and provide workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report!

The issue here is a format mismatch: the `.lottie` file extension (dotLottie format, from LottieFiles) is actually a **ZIP archive** containing Lottie JSON animations. SkiaSharp's Skottie binding currently only supports the traditional **Lottie JSON** format (`.json`), so it silently returns `null` when given a ZIP binary.

**Workaround (immediate):** Export or download the animation in **Lottie JSON (.json)** format from LottieFiles, then load it:

```csharp
var animation = SkiaSharp.Skottie.Animation.Create(pathToJsonFile);
```

**Workaround (if you only have the .lottie file):** Extract the JSON from the ZIP using `System.IO.Compression`:

```csharp
using System.IO.Compression;

string animationJson = null;
using (var zip = ZipFile.OpenRead(lottiePath))
{
    // The main animation JSON is typically the first .json entry
    var entry = zip.Entries.FirstOrDefault(e => e.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
    if (entry != null)
    {
        using var reader = new StreamReader(entry.Open());
        animationJson = reader.ReadToEnd();
    }
}

var animation = animationJson != null ? SkiaSharp.Skottie.Animation.Parse(animationJson) : null;
```

We're tracking adding proper format detection so that a clearer error is thrown instead of a silent null return.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3421,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T10:46:03Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "area/SkiaSharp",
      "tenet/reliability"
    ]
  },
  "summary": "Calling Animation.Create() with a .lottie binary file (dotLottie ZIP container) silently returns null because the Skottie C API only accepts JSON-formatted Lottie data, not the ZIP-based .lottie format.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "missing-output",
      "errorMessage": "Animation.Create returns null with no exception for a .lottie file",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Download a .lottie file from LottieFiles (e.g. https://app.lottiefiles.com/animation/5b206184-3c40-4a29-a6c4-0f62df06e4a8)",
        "Call SkiaSharp.Skottie.Animation.Create(path) with the absolute path to the .lottie file",
        "Observe that the returned animation is null and no exception is thrown"
      ],
      "environmentDetails": "SkiaSharp 3.116.0, Windows 11, Visual Studio",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2166",
          "description": "Related: Skottie cannot load animations with BOM (null-return pattern — same root symptom)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Animation.Create code path has not changed — it still passes raw file bytes directly to skottie_animation_make_from_data which expects JSON."
    }
  },
  "analysis": {
    "summary": "The .lottie format (dotLottie) is a ZIP archive containing a manifest.json and one or more Lottie JSON animation files. When Animation.Create(path) reads the file and passes the bytes to the C API skottie_animation_make_from_data, the Skottie JSON parser receives a ZIP binary (starting with 'PK\\x03\\x04') instead of JSON, fails to parse, and returns null. SkiaSharp provides no ZIP extraction, no format detection, and no error message, causing a silent failure.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.Skottie/Animation.cs",
        "lines": "91-102",
        "finding": "Animation.Create(string path) calls SKData.Create(path) then TryCreate(SKData) which passes raw bytes directly to skottie_animation_make_from_data. No format detection for ZIP magic bytes.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.Skottie/Animation.cs",
        "lines": "77-89",
        "finding": "TryCreate(SKData) strips UTF-8 BOM preamble (via GetPreambleSize) but has no handling for binary/ZIP content — the ZIP bytes are passed raw to the C API, which returns null.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.Skottie/AnimationBuilder.cs",
        "lines": "61-75",
        "finding": "AnimationBuilder.Build(SKData) follows the same pattern — passes raw bytes to skottie_animation_builder_make_from_data with no ZIP format detection.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "var animation = SkiaSharp.Skottie.Animation.Create(sourceFileAbsolute);",
        "source": "issue body",
        "interpretation": "Reporter is using the file-path overload of Animation.Create with a .lottie file which is a ZIP, not JSON. The C API only accepts JSON."
      },
      {
        "text": "A null animation instance is returned instead.",
        "source": "issue body",
        "interpretation": "Silent failure: no exception, no error message. The failure mode is indistinguishable from passing a valid but unsupported JSON file."
      }
    ],
    "rationale": "The .lottie format (dotLottie, file extension .lottie) is a ZIP archive introduced by LottieFiles — it is distinct from the traditional JSON Lottie format (.json). The Skottie C API (skottie_animation_make_from_data) only understands JSON. SkiaSharp currently performs no format detection and passes raw bytes to the C API regardless of file format. This is a bug because the failure is silent and the API contract (Create returns null on parse failure) does not communicate the root cause to users. A proper fix would be either: (a) detect the ZIP magic bytes and throw an informative exception, or (b) implement dotLottie container support (extract JSON from ZIP). The issue is classified as medium severity because a workaround exists (use the .json format).",
    "workarounds": [
      "Download or export the animation in traditional .json (Lottie JSON) format from LottieFiles instead of .lottie format, then load with Animation.Create(path).",
      "If only the .lottie file is available, extract it with any ZIP library (e.g., System.IO.Compression.ZipFile), read the animation JSON from inside the archive, then call Animation.Parse(json) or Animation.Create(stream) with that JSON content."
    ],
    "nextQuestions": [
      "Does Skia's upstream Skottie C++ have any built-in .lottie (dotLottie ZIP) support not yet exposed via the C API?",
      "Should SkiaSharp add dotLottie ZIP extraction in the managed binding, or should a new C API function be added?"
    ],
    "resolution": {
      "hypothesis": "The .lottie file is a ZIP archive. Animation.Create passes raw bytes to the JSON parser, which returns null for non-JSON input. Adding ZIP magic byte detection would either throw a clear error or trigger ZIP extraction.",
      "proposals": [
        {
          "title": "Detect .lottie ZIP and throw informative exception",
          "description": "In TryCreate(SKData), check if the first 4 bytes match the ZIP magic (PK\\x03\\x04). If so, throw an InvalidOperationException explaining that .lottie (dotLottie) format is not supported and the user should use the .json format.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Workaround: extract JSON from .lottie ZIP manually",
          "description": "Users can use System.IO.Compression.ZipFile to open the .lottie archive, enumerate entries, find the first .json entry matching the manifest, read its contents, and pass the JSON string to Animation.Parse().",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Detect .lottie ZIP and throw informative exception",
      "recommendedReason": "Minimal change that converts the silent failure into an actionable error, unblocking the reporter immediately. Full dotLottie support is a larger feature request."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Root cause is clear (ZIP vs JSON format mismatch) but fix scope needs decision: minimal error detection vs full dotLottie container support. The reporter needs guidance on the workaround.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Confirm existing labels: type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain .lottie vs .json format difference and provide workaround",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed report!\n\nThe issue here is a format mismatch: the `.lottie` file extension (dotLottie format, from LottieFiles) is actually a **ZIP archive** containing Lottie JSON animations. SkiaSharp's Skottie binding currently only supports the traditional **Lottie JSON** format (`.json`), so it silently returns `null` when given a ZIP binary.\n\n**Workaround (immediate):** Export or download the animation in **Lottie JSON (.json)** format from LottieFiles, then load it:\n\n```csharp\nvar animation = SkiaSharp.Skottie.Animation.Create(pathToJsonFile);\n```\n\n**Workaround (if you only have the .lottie file):** Extract the JSON from the ZIP using `System.IO.Compression`:\n\n```csharp\nusing System.IO.Compression;\n\nstring animationJson = null;\nusing (var zip = ZipFile.OpenRead(lottiePath))\n{\n    // The main animation JSON is typically the first .json entry\n    var entry = zip.Entries.FirstOrDefault(e => e.Name.EndsWith(\".json\", StringComparison.OrdinalIgnoreCase));\n    if (entry != null)\n    {\n        using var reader = new StreamReader(entry.Open());\n        animationJson = reader.ReadToEnd();\n    }\n}\n\nvar animation = animationJson != null ? SkiaSharp.Skottie.Animation.Parse(animationJson) : null;\n```\n\nWe're tracking adding proper format detection so that a clearer error is thrown instead of a silent null return."
      }
    ]
  }
}
```

</details>
