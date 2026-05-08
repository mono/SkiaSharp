# Issue Triage Report — #2759

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T22:02:17Z |
| Type | type/bug (0.80 (80%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-info (0.85 (85%)) |

**Issue Summary:** Reporter (same author as issue #933) claims SKBitmap.Decode returns null for a PNG image that previously decoded correctly, citing a regression between 2.88.2 (last good) and 2.88.3 on Windows, but provides no failing image and leaves expected/actual behavior blank.

**Analysis:** SKBitmap.Decode(string) internally creates an SKCodec; if SKCodec.Create returns null (i.e., the codec cannot be created for the given file), Decode returns null. For certain PNG files (non-standard color profiles, unusual chunk layouts, or libpng compatibility issues), SKCodec.Create fails silently. The reporter does not attach the failing image, making it impossible to reproduce or confirm the regression claim.

**Recommendations:** **needs-info** — Report is missing the failing PNG image, expected/actual behavior details, and log output; same symptom is tracked in related open issues #2429 and #2874

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Call SKBitmap.Decode with a specific PNG file path
2. Assert result is not null — assert fails (null returned)

**Environment:** Visual Studio on Windows, SkiaSharp 2.88.3

**Related issues:** #933, #1981, #2429, #2801, #2874

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/933 — Original issue filed by same reporter; closed as duplicate of #1981 in v2.88.1 milestone
- https://github.com/mono/SkiaSharp/issues/1981 — Root issue #933 was a duplicate of
- https://github.com/mono/SkiaSharp/issues/2429 — Similar report: SKBitmap.Decode returns null for specific images; open and triaged
- https://github.com/mono/SkiaSharp/issues/2801 — Similar report: SKBitmap.Decode(string) returns null for some PNGs (FBX-embedded texture)
- https://github.com/mono/SkiaSharp/issues/2874 — Similar report: Failed to decode specific PNG images; open and triaged on Windows and Linux

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | missing-output |
| Error message | SKBitmap.Decode returns null |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.2, 2.88.3, 2.88.6 |
| Worked in | 2.88.2 |
| Broke in | 2.88.3 |
| Current relevance | likely |
| Relevance reason | Related open issues #2429 and #2874 report the same symptom on current versions (2.88.8, 3.x), suggesting the root cause persists. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.65 (65%) |
| Reason | Reporter explicitly states 2.88.2 is last good version and 2.88.3 broke it, though no attachment or log is provided to verify |
| Worked in version | 2.88.2 |
| Broke in version | 2.88.3 |

## Analysis

### Technical Summary

SKBitmap.Decode(string) internally creates an SKCodec; if SKCodec.Create returns null (i.e., the codec cannot be created for the given file), Decode returns null. For certain PNG files (non-standard color profiles, unusual chunk layouts, or libpng compatibility issues), SKCodec.Create fails silently. The reporter does not attach the failing image, making it impossible to reproduce or confirm the regression claim.

### Rationale

The issue type is bug because the reporter claims correct images that load in other applications fail with SkiaSharp. The regression from 2.88.2→2.88.3 is claimed but unverifiable without the image. The symptom clusters with several other open issues (#2429, #2801, #2874) all reporting the same null-return behavior for specific PNG files, suggesting a systemic issue with libpng or the codec layer handling of non-standard PNG variants. The issue is classified needs-info because the failing image is absent and expected/actual behavior sections are blank.

### Key Signals

- "This is follow-up of #933 issue, which is closed as resolved, but the problem is still there 2.88.6 version" — **issue body** (Reporter believes the fix from v2.88.1 (or related fix) did not fully address the problem; claims persistence into 2.88.6.)
- "Last Known Good: 2.88.2, Current (broken): 2.88.3" — **issue form fields** (Explicit regression claim between patch versions; aligns with libpng or codec changes between these releases.)
- "Expected Behavior: _No response_ / Actual Behavior: _No response_" — **issue body** (Reporter did not fill in key sections; no log output, no screenshot, no failing image attached — limits reproducibility.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 539-549 | direct | SKBitmap.Decode(string) creates an SKCodec via SKCodec.Create(filename) and returns null if the codec is null — no error message or exception is thrown, making silent failures hard to diagnose |
| `binding/SkiaSharp/SKBitmap.cs` | 434-459 | related | SKBitmap.Decode(SKCodec) and its bitmapInfo overload perform the actual pixel decode; if codec is null the outer method short-circuits before reaching this code |

### Workarounds

- Try SKImage.FromEncodedData(SKData.Create(filePath)) as an alternative decode path
- Use SKCodec.Create(filename) first to get an error result / null check before calling Decode

### Next Questions

- What specific PNG file triggers the null return? (Reporter should attach the file)
- Does SKCodec.Create(filename) also return null for the same file (to isolate codec vs bitmap decode)?
- Does the issue reproduce with SKImage.FromEncodedData or SKCodec.Create directly?
- Has the reporter tested on 2.88.8 or a 3.x build to confirm the issue persists?

### Resolution Proposals

**Hypothesis:** A specific PNG's metadata or chunk layout causes SKCodec.Create to fail silently; this may be the same class of issue as #2429/#2874 (non-standard PNG variants rejected by libpng).

1. **Request the failing image and repro details** — investigation, confidence 0.90 (90%), cost/xs, validated=untested
   - Ask reporter to attach the specific PNG that fails and to confirm whether SKCodec.Create also returns null for it, and whether the issue persists in the latest version.
2. **Link to related open issues** — investigation, confidence 0.85 (85%), cost/xs, validated=untested
   - Cross-reference #2429 and #2874 which describe the same symptom with test images attached; consolidate investigations.

**Recommended proposal:** Request the failing image and repro details

**Why:** Without the failing image the report cannot be reproduced; gathering that data is the highest-value next step.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.85 (85%) |
| Reason | Report is missing the failing PNG image, expected/actual behavior details, and log output; same symptom is tracked in related open issues #2429 and #2874 |
| Suggested repro platform | linux |

### Missing Info

- Attach the specific PNG file that fails to decode
- Confirm whether SKCodec.Create(filename) also returns null
- Provide expected vs actual behavior description
- Confirm whether the issue persists in the latest version (2.88.8 or 3.x)

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Add area/SkiaSharp, tenet/compatibility labels (type/bug already present) | labels=type/bug, area/SkiaSharp, tenet/compatibility |
| link-related | low | 0.85 (85%) | Cross-reference related open issue #2874 (same symptom, has test image) | linkedIssue=#2874 |
| link-related | low | 0.80 (80%) | Cross-reference related open issue #2429 (same symptom on Windows and Linux) | linkedIssue=#2429 |
| add-comment | medium | 0.85 (85%) | Request the failing PNG image and additional repro details | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the follow-up report! To investigate the regression, could you please:

1. **Attach the specific PNG file** that returns null (the one from `C:\Temp\image1.png` in your code).
2. Confirm whether `SKCodec.Create(filename)` also returns null for the same file.
3. Let us know whether the issue still occurs on **2.88.8** or a **3.x** build.

Note: there are a few related open issues (#2429, #2874) describing the same `SKBitmap.Decode` null return for specific PNG files — your image may help link these reports together. Without the file we are unable to reproduce the issue.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2759,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T22:02:17Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter (same author as issue #933) claims SKBitmap.Decode returns null for a PNG image that previously decoded correctly, citing a regression between 2.88.2 (last good) and 2.88.3 on Windows, but provides no failing image and leaves expected/actual behavior blank.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "missing-output",
      "errorMessage": "SKBitmap.Decode returns null",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Call SKBitmap.Decode with a specific PNG file path",
        "Assert result is not null — assert fails (null returned)"
      ],
      "environmentDetails": "Visual Studio on Windows, SkiaSharp 2.88.3",
      "relatedIssues": [
        933,
        1981,
        2429,
        2801,
        2874
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/933",
          "description": "Original issue filed by same reporter; closed as duplicate of #1981 in v2.88.1 milestone"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1981",
          "description": "Root issue #933 was a duplicate of"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2429",
          "description": "Similar report: SKBitmap.Decode returns null for specific images; open and triaged"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2801",
          "description": "Similar report: SKBitmap.Decode(string) returns null for some PNGs (FBX-embedded texture)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2874",
          "description": "Similar report: Failed to decode specific PNG images; open and triaged on Windows and Linux"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.2",
        "2.88.3",
        "2.88.6"
      ],
      "workedIn": "2.88.2",
      "brokeIn": "2.88.3",
      "currentRelevance": "likely",
      "relevanceReason": "Related open issues #2429 and #2874 report the same symptom on current versions (2.88.8, 3.x), suggesting the root cause persists."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.65,
      "reason": "Reporter explicitly states 2.88.2 is last good version and 2.88.3 broke it, though no attachment or log is provided to verify",
      "workedInVersion": "2.88.2",
      "brokeInVersion": "2.88.3"
    }
  },
  "analysis": {
    "summary": "SKBitmap.Decode(string) internally creates an SKCodec; if SKCodec.Create returns null (i.e., the codec cannot be created for the given file), Decode returns null. For certain PNG files (non-standard color profiles, unusual chunk layouts, or libpng compatibility issues), SKCodec.Create fails silently. The reporter does not attach the failing image, making it impossible to reproduce or confirm the regression claim.",
    "rationale": "The issue type is bug because the reporter claims correct images that load in other applications fail with SkiaSharp. The regression from 2.88.2→2.88.3 is claimed but unverifiable without the image. The symptom clusters with several other open issues (#2429, #2801, #2874) all reporting the same null-return behavior for specific PNG files, suggesting a systemic issue with libpng or the codec layer handling of non-standard PNG variants. The issue is classified needs-info because the failing image is absent and expected/actual behavior sections are blank.",
    "keySignals": [
      {
        "text": "This is follow-up of #933 issue, which is closed as resolved, but the problem is still there 2.88.6 version",
        "source": "issue body",
        "interpretation": "Reporter believes the fix from v2.88.1 (or related fix) did not fully address the problem; claims persistence into 2.88.6."
      },
      {
        "text": "Last Known Good: 2.88.2, Current (broken): 2.88.3",
        "source": "issue form fields",
        "interpretation": "Explicit regression claim between patch versions; aligns with libpng or codec changes between these releases."
      },
      {
        "text": "Expected Behavior: _No response_ / Actual Behavior: _No response_",
        "source": "issue body",
        "interpretation": "Reporter did not fill in key sections; no log output, no screenshot, no failing image attached — limits reproducibility."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "539-549",
        "finding": "SKBitmap.Decode(string) creates an SKCodec via SKCodec.Create(filename) and returns null if the codec is null — no error message or exception is thrown, making silent failures hard to diagnose",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "434-459",
        "finding": "SKBitmap.Decode(SKCodec) and its bitmapInfo overload perform the actual pixel decode; if codec is null the outer method short-circuits before reaching this code",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "What specific PNG file triggers the null return? (Reporter should attach the file)",
      "Does SKCodec.Create(filename) also return null for the same file (to isolate codec vs bitmap decode)?",
      "Does the issue reproduce with SKImage.FromEncodedData or SKCodec.Create directly?",
      "Has the reporter tested on 2.88.8 or a 3.x build to confirm the issue persists?"
    ],
    "workarounds": [
      "Try SKImage.FromEncodedData(SKData.Create(filePath)) as an alternative decode path",
      "Use SKCodec.Create(filename) first to get an error result / null check before calling Decode"
    ],
    "resolution": {
      "hypothesis": "A specific PNG's metadata or chunk layout causes SKCodec.Create to fail silently; this may be the same class of issue as #2429/#2874 (non-standard PNG variants rejected by libpng).",
      "proposals": [
        {
          "title": "Request the failing image and repro details",
          "description": "Ask reporter to attach the specific PNG that fails and to confirm whether SKCodec.Create also returns null for it, and whether the issue persists in the latest version.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Link to related open issues",
          "description": "Cross-reference #2429 and #2874 which describe the same symptom with test images attached; consolidate investigations.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Request the failing image and repro details",
      "recommendedReason": "Without the failing image the report cannot be reproduced; gathering that data is the highest-value next step."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.85,
      "reason": "Report is missing the failing PNG image, expected/actual behavior details, and log output; same symptom is tracked in related open issues #2429 and #2874",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Attach the specific PNG file that fails to decode",
      "Confirm whether SKCodec.Create(filename) also returns null",
      "Provide expected vs actual behavior description",
      "Confirm whether the issue persists in the latest version (2.88.8 or 3.x)"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Add area/SkiaSharp, tenet/compatibility labels (type/bug already present)",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference related open issue #2874 (same symptom, has test image)",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 2874
      },
      {
        "type": "link-related",
        "description": "Cross-reference related open issue #2429 (same symptom on Windows and Linux)",
        "risk": "low",
        "confidence": 0.8,
        "linkedIssue": 2429
      },
      {
        "type": "add-comment",
        "description": "Request the failing PNG image and additional repro details",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the follow-up report! To investigate the regression, could you please:\n\n1. **Attach the specific PNG file** that returns null (the one from `C:\\Temp\\image1.png` in your code).\n2. Confirm whether `SKCodec.Create(filename)` also returns null for the same file.\n3. Let us know whether the issue still occurs on **2.88.8** or a **3.x** build.\n\nNote: there are a few related open issues (#2429, #2874) describing the same `SKBitmap.Decode` null return for specific PNG files — your image may help link these reports together. Without the file we are unable to reproduce the issue."
      }
    ]
  }
}
```

</details>
