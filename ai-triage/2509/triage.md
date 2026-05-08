# Issue Triage Report — #2509

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T17:20:00Z |
| Type | type/bug (0.72 (72%)) |
| Area | area/SkiaSharp (0.65 (65%)) |
| Suggested action | close-as-external (0.82 (82%)) |

**Issue Summary:** Reporter gets an unspecified exception during image loading in a Web API deployed on Windows Server 2019 using QuestPDF; works on localhost. The maintainer identified this as a QuestPDF issue.

**Analysis:** The exception occurs during image loading in a QuestPDF-driven Web API deployed to Windows Server 2019. The maintainer identified the root cause as a QuestPDF-level issue, not a SkiaSharp bug. The 'works locally, fails on server' pattern commonly indicates a deployment-level issue: missing native assets, missing VC++ Redistributable, or a QuestPDF-specific dependency not present on the server.

**Recommendations:** **close-as-external** — The SkiaSharp maintainer already identified this as a QuestPDF issue. No stack trace or error text was provided (screenshots only), and the reporter confirmed QuestPDF is the intermediary library. The root cause is outside SkiaSharp's scope.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Deploy a Web API using QuestPDF (which internally uses SkiaSharp) to Windows Server 2019
2. Trigger an endpoint that loads/processes an image
3. Observe exception (works fine on localhost)

**Environment:** Windows Server 2019, Web API deployment, QuestPDF dependency

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | exception |
| Error message | Unknown exception during image load (only visible in screenshots — not reproduced as text) |
| Repro quality | none |
| Target frameworks | — |

## Analysis

### Technical Summary

The exception occurs during image loading in a QuestPDF-driven Web API deployed to Windows Server 2019. The maintainer identified the root cause as a QuestPDF-level issue, not a SkiaSharp bug. The 'works locally, fails on server' pattern commonly indicates a deployment-level issue: missing native assets, missing VC++ Redistributable, or a QuestPDF-specific dependency not present on the server.

### Rationale

The maintainer explicitly said 'The exception is throwing on an image load. This looks like an issue in questpdf.' The reporter confirmed they use QuestPDF. The issue body contains only screenshot images — no error text, no stack trace, no version information. This is a QuestPDF compatibility/deployment issue; SkiaSharp itself is not at fault. The close-as-external action is appropriate.

### Key Signals

- "The exception is throwing on an image load. This looks like an issue in questpdf." — **maintainer comment (mattleibow)** (Root cause identified by maintainer as QuestPDF, not SkiaSharp.)
- "same code runned in localhost works fine" — **issue body** (Classic deployment issue — missing native dependency or runtime on Windows Server 2019.)
- "first of this issue I opened to QuestPDF but actually no one reply to me" — **reporter follow-up comment** (Reporter opened QuestPDF issue first, confirming QuestPDF involvement. Came here as fallback.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImage.cs` | 166-233 | context | SKImage.FromEncodedData() and related overloads validate arguments and delegate to native SkiaApi calls. The C# wrapper itself is straightforward — any exception at this level would propagate from the native layer or from QuestPDF's usage pattern. |
| `binding/SkiaSharp/SKImage.cs` | 166-173 | related | SKImage.FromEncodedData(SKData data) calls SkiaApi.sk_image_new_from_encoded — no Windows-specific code. If this fails on Server 2019 it is a deployment (native binary) or caller issue, not a SkiaSharp API bug. |

### Workarounds

- Ensure SkiaSharp.NativeAssets.Win32 package is referenced directly in the executable project (not only in library projects) so native binaries are included in publish output.
- Install VC++ Redistributable on Windows Server 2019 if not already present.
- Report the issue to QuestPDF with the full exception and stack trace.

### Next Questions

- What is the actual exception message and stack trace?
- Which SkiaSharp version is being used?
- Is SkiaSharp.NativeAssets.Win32 (or equivalent) deployed with the application?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.82 (82%) |
| Reason | The SkiaSharp maintainer already identified this as a QuestPDF issue. No stack trace or error text was provided (screenshots only), and the reporter confirmed QuestPDF is the intermediary library. The root cause is outside SkiaSharp's scope. |
| Suggested repro platform | windows |

### Missing Info

- Full exception type and message (text, not screenshot)
- Complete stack trace
- SkiaSharp version used
- Whether SkiaSharp.NativeAssets.Win32 is included in the deployment

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply bug, SkiaSharp core, and Windows Classic labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic |
| add-comment | high | 0.82 (82%) | Redirect to QuestPDF with guidance on native asset deployment | — |
| close-issue | medium | 0.80 (80%) | Close as external — root cause is in QuestPDF, not SkiaSharp | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thank you for reporting this. As @mattleibow noted, the exception is occurring in QuestPDF's image-loading code, which sits above SkiaSharp. Please continue tracking this with the QuestPDF team.

If you'd like to verify SkiaSharp's native binaries are correctly deployed on Windows Server 2019, check that:
1. `SkiaSharp.NativeAssets.Win32` is referenced directly in your executable/API project (not just a library).
2. The VC++ Redistributable is installed on the server.
3. The publish output contains `libSkiaSharp.dll`.

If QuestPDF's issue tracker is unresponsive, please provide a minimal repro that reproduces the error **without** QuestPDF so we can assess whether SkiaSharp itself is involved.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2509,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T17:20:00Z"
  },
  "summary": "Reporter gets an unspecified exception during image loading in a Web API deployed on Windows Server 2019 using QuestPDF; works on localhost. The maintainer identified this as a QuestPDF issue.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.72
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.65
    },
    "platforms": [
      "os/Windows-Classic"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "Unknown exception during image load (only visible in screenshots — not reproduced as text)",
      "reproQuality": "none",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Deploy a Web API using QuestPDF (which internally uses SkiaSharp) to Windows Server 2019",
        "Trigger an endpoint that loads/processes an image",
        "Observe exception (works fine on localhost)"
      ],
      "environmentDetails": "Windows Server 2019, Web API deployment, QuestPDF dependency",
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "The exception occurs during image loading in a QuestPDF-driven Web API deployed to Windows Server 2019. The maintainer identified the root cause as a QuestPDF-level issue, not a SkiaSharp bug. The 'works locally, fails on server' pattern commonly indicates a deployment-level issue: missing native assets, missing VC++ Redistributable, or a QuestPDF-specific dependency not present on the server.",
    "rationale": "The maintainer explicitly said 'The exception is throwing on an image load. This looks like an issue in questpdf.' The reporter confirmed they use QuestPDF. The issue body contains only screenshot images — no error text, no stack trace, no version information. This is a QuestPDF compatibility/deployment issue; SkiaSharp itself is not at fault. The close-as-external action is appropriate.",
    "keySignals": [
      {
        "text": "The exception is throwing on an image load. This looks like an issue in questpdf.",
        "source": "maintainer comment (mattleibow)",
        "interpretation": "Root cause identified by maintainer as QuestPDF, not SkiaSharp."
      },
      {
        "text": "same code runned in localhost works fine",
        "source": "issue body",
        "interpretation": "Classic deployment issue — missing native dependency or runtime on Windows Server 2019."
      },
      {
        "text": "first of this issue I opened to QuestPDF but actually no one reply to me",
        "source": "reporter follow-up comment",
        "interpretation": "Reporter opened QuestPDF issue first, confirming QuestPDF involvement. Came here as fallback."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "166-233",
        "finding": "SKImage.FromEncodedData() and related overloads validate arguments and delegate to native SkiaApi calls. The C# wrapper itself is straightforward — any exception at this level would propagate from the native layer or from QuestPDF's usage pattern.",
        "relevance": "context"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "166-173",
        "finding": "SKImage.FromEncodedData(SKData data) calls SkiaApi.sk_image_new_from_encoded — no Windows-specific code. If this fails on Server 2019 it is a deployment (native binary) or caller issue, not a SkiaSharp API bug.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "What is the actual exception message and stack trace?",
      "Which SkiaSharp version is being used?",
      "Is SkiaSharp.NativeAssets.Win32 (or equivalent) deployed with the application?"
    ],
    "workarounds": [
      "Ensure SkiaSharp.NativeAssets.Win32 package is referenced directly in the executable project (not only in library projects) so native binaries are included in publish output.",
      "Install VC++ Redistributable on Windows Server 2019 if not already present.",
      "Report the issue to QuestPDF with the full exception and stack trace."
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.82,
      "reason": "The SkiaSharp maintainer already identified this as a QuestPDF issue. No stack trace or error text was provided (screenshots only), and the reporter confirmed QuestPDF is the intermediary library. The root cause is outside SkiaSharp's scope.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Full exception type and message (text, not screenshot)",
      "Complete stack trace",
      "SkiaSharp version used",
      "Whether SkiaSharp.NativeAssets.Win32 is included in the deployment"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp core, and Windows Classic labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Redirect to QuestPDF with guidance on native asset deployment",
        "risk": "high",
        "confidence": 0.82,
        "comment": "Thank you for reporting this. As @mattleibow noted, the exception is occurring in QuestPDF's image-loading code, which sits above SkiaSharp. Please continue tracking this with the QuestPDF team.\n\nIf you'd like to verify SkiaSharp's native binaries are correctly deployed on Windows Server 2019, check that:\n1. `SkiaSharp.NativeAssets.Win32` is referenced directly in your executable/API project (not just a library).\n2. The VC++ Redistributable is installed on the server.\n3. The publish output contains `libSkiaSharp.dll`.\n\nIf QuestPDF's issue tracker is unresponsive, please provide a minimal repro that reproduces the error **without** QuestPDF so we can assess whether SkiaSharp itself is involved."
      },
      {
        "type": "close-issue",
        "description": "Close as external — root cause is in QuestPDF, not SkiaSharp",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
