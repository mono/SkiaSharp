# Issue Triage Report — #1282

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T11:30:00Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-info (0.75 (75%)) |

**Issue Summary:** App crashes with MissingMethodException when calling SKCanvas.DrawText(string, float, float, SKPaint) on Android using SkiaSharp 1.68.0–1.68.2.1 with Visual Studio 2017; works with 1.60.3 and VS 2019.

**Analysis:** The MissingMethodException at runtime on Android suggests the compiled app references a method that cannot be found in the deployed SkiaSharp assembly. Between 1.60.3 and 1.68.x, DrawText(string, float, float, SKPaint) was refactored to internally call paint.GetFont() and delegate to a new SKFont-based overload. VS 2017 may have produced a stale build or incorrect TFM resolution for the Xamarin.Android platform assembly.

**Recommendations:** **needs-info** — Issue is from 2020 with SkiaSharp 1.68.x and VS 2017 (EOL). Needs confirmation whether this still reproduces on current SkiaSharp 3.x. The error screenshot is not readable from the issue body. VS 2019 workaround already exists per the reporter.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Android |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Forms project targeting .NET Standard 2.0
2. Install SkiaSharp.Views.Forms version 1.68.0 or later (up to 1.68.2.1)
3. Add SKCanvasView and call canvas.DrawText("text", 15, 15, paint) in PaintSurface handler
4. Build and deploy to Android 8.1 or 9.0 using Visual Studio 2017
5. Observe MissingMethodException at runtime on device

**Environment:** VS 2017, .NET Standard 2.0, Android 8.1/9.0 (API 27/28), SkiaSharp.Views.Forms 1.68.0–1.68.2.1

**Repository links:**
- https://github.com/leocdi/skia01 — Reporter's minimal reproduction project

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | exception |
| Error message | System.MissingMethodException when calling canvas.DrawText(string, float, float, SKPaint) |
| Repro quality | partial |
| Target frameworks | netstandard2.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.60.3, 1.68.0, 1.68.2.1 |
| Worked in | 1.60.3 |
| Broke in | 1.68.0 |
| Current relevance | unlikely |
| Relevance reason | Issue is from 2020 targeting SkiaSharp 1.68.x. Current SkiaSharp is 3.x; DrawText(string,float,float,SKPaint) still exists today as a deprecated method. The exact runtime assembly deployment bug on VS 2017 is unlikely to be relevant to modern tooling. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.85 (85%) |
| Reason | Reporter explicitly states 1.60.3 worked and 1.68.0+ breaks. Between these versions, SKCanvas.DrawText was refactored to delegate through SKFont (introduced in 1.68), potentially causing assembly version mismatches with VS 2017's NuGet resolution. |
| Worked in version | 1.60.3 |
| Broke in version | 1.68.0 |

## Analysis

### Technical Summary

The MissingMethodException at runtime on Android suggests the compiled app references a method that cannot be found in the deployed SkiaSharp assembly. Between 1.60.3 and 1.68.x, DrawText(string, float, float, SKPaint) was refactored to internally call paint.GetFont() and delegate to a new SKFont-based overload. VS 2017 may have produced a stale build or incorrect TFM resolution for the Xamarin.Android platform assembly.

### Rationale

Classified as type/bug because this is a runtime crash with a reproducible code path and a known-good baseline version. The area is area/SkiaSharp since it involves SKCanvas.DrawText core API. Platform is os/Android because VS 2019 works (implying toolchain/NuGet packaging interaction). The tenet/compatibility label applies because upgrading from 1.60.3 to 1.68.x triggers the crash. Severity is medium — crash exists but has a clear workaround (use VS 2019 or downgrade).

### Key Signals

- "above 1.68.0 to 1.68.2.1 / Last known good version: 1.60.3" — **issue body** (Confirmed regression introduced in 1.68.0; implies API or packaging change broke the method resolution at runtime.)
- "correct behavior on vs 2019" — **issue body** (VS 2019 resolves the NuGet package correctly; VS 2017 likely picks a wrong platform DLL or fails to update the linked SkiaSharp assembly on the Android target.)
- "canvas.DrawText("I am text", 15, 15, paint)" — **issue body** (Standard usage of the existing overload DrawText(string, float, float, SKPaint), which still exists in current code as a deprecated method delegating to SKFont-based path.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 631-637 | direct | DrawText(string, float, float, SKPaint) still exists (marked [Obsolete]) and delegates to DrawText(text, x, y, paint.TextAlign, paint.GetFont(), paint). In 1.68.x this delegate call was added; in 1.60.3 the implementation likely used a direct native call. The refactoring means the compiled call site matches the API surface, but if the deployed DLL is a stale older build without this overload, MissingMethodException results. |
| `binding/SkiaSharp/SKPaint.cs` | 885 | related | SKPaint.GetFont() is an internal method used by the DrawText delegation chain. This method was introduced alongside SKFont in 1.68.x. If VS 2017 deploys an old 1.60.3 Android assembly instead of 1.68.x, GetFont() and new overloads are missing, causing MissingMethodException. |

### Workarounds

- Use Visual Studio 2019 or later to build and deploy the project
- Clean the Android project, delete bin/obj folders, and re-deploy (may resolve stale assembly deployment in VS 2017)
- Downgrade SkiaSharp.Views.Forms to 1.60.3 if VS 2017 must be used

### Next Questions

- Does the issue reproduce on current SkiaSharp 3.x with a modern IDE?
- Is the screenshot showing MissingMethodException for DrawText itself or an internal method it calls (GetFont, etc.)?
- Does cleaning bin/obj and doing a full rebuild in VS 2017 resolve the issue?

### Resolution Proposals

**Hypothesis:** VS 2017 NuGet/MSBuild deploys a stale or incorrect platform assembly for the Xamarin.Android target, causing a mismatch between the compile-time method resolution and the runtime assembly. A clean rebuild or switch to VS 2019 resolves it.

1. **Clean rebuild workaround** — workaround, confidence 0.70 (70%), cost/xs, validated=untested
   - Delete bin/ and obj/ directories in both the Android project and the shared library, then rebuild and redeploy. This forces VS 2017 to deploy the correct SkiaSharp platform assembly.
2. **Upgrade to VS 2019 or later** — workaround, confidence 0.90 (90%), cost/m, validated=untested
   - Visual Studio 2019 is confirmed to work by the reporter. VS 2017 is end-of-life. Upgrading resolves the NuGet deployment issue.
3. **Downgrade SkiaSharp to 1.60.3** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - If VS 2017 must be used, pinning SkiaSharp.Views.Forms to 1.60.3 avoids the issue. Note: 1.60.3 is very old and lacks many features and bug fixes.

**Recommended proposal:** Upgrade to VS 2019 or later

**Why:** VS 2017 is end-of-life; the issue is specific to its toolchain. The reporter confirmed VS 2019 works.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.75 (75%) |
| Reason | Issue is from 2020 with SkiaSharp 1.68.x and VS 2017 (EOL). Needs confirmation whether this still reproduces on current SkiaSharp 3.x. The error screenshot is not readable from the issue body. VS 2019 workaround already exists per the reporter. |
| Suggested repro platform | linux |

### Missing Info

- Does the issue reproduce with current SkiaSharp 3.x?
- What exact method does the MissingMethodException reference (full exception message)?
- Does a clean rebuild in VS 2017 (delete bin/obj) fix the issue?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, area/SkiaSharp, os/Android, and tenet/compatibility labels | labels=type/bug, area/SkiaSharp, os/Android, tenet/compatibility |
| add-comment | medium | 0.75 (75%) | Ask reporter if this reproduces on current SkiaSharp and provide workarounds | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! A few questions to help us investigate:

1. **Does this still happen with the latest SkiaSharp (3.x)?** This issue was filed against 1.68.x; a lot has changed since then.
2. **Can you share the full exception message?** The screenshot isn't readable — specifically, which method does the `MissingMethodException` reference?
3. **Have you tried a clean rebuild?** Delete the `bin/` and `obj/` folders in both your Android project and shared library, then rebuild in VS 2017. Stale assemblies can cause this type of error.

In the meantime, if you must use VS 2017, you can work around this by either:
- Switching to **Visual Studio 2019** (confirmed to work per your report)
- Downgrading to **SkiaSharp 1.60.3**
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1282,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T11:30:00Z"
  },
  "summary": "App crashes with MissingMethodException when calling SKCanvas.DrawText(string, float, float, SKPaint) on Android using SkiaSharp 1.68.0–1.68.2.1 with Visual Studio 2017; works with 1.60.3 and VS 2019.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Android"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "System.MissingMethodException when calling canvas.DrawText(string, float, float, SKPaint)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "netstandard2.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms project targeting .NET Standard 2.0",
        "Install SkiaSharp.Views.Forms version 1.68.0 or later (up to 1.68.2.1)",
        "Add SKCanvasView and call canvas.DrawText(\"text\", 15, 15, paint) in PaintSurface handler",
        "Build and deploy to Android 8.1 or 9.0 using Visual Studio 2017",
        "Observe MissingMethodException at runtime on device"
      ],
      "environmentDetails": "VS 2017, .NET Standard 2.0, Android 8.1/9.0 (API 27/28), SkiaSharp.Views.Forms 1.68.0–1.68.2.1",
      "repoLinks": [
        {
          "url": "https://github.com/leocdi/skia01",
          "description": "Reporter's minimal reproduction project"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.60.3",
        "1.68.0",
        "1.68.2.1"
      ],
      "workedIn": "1.60.3",
      "brokeIn": "1.68.0",
      "currentRelevance": "unlikely",
      "relevanceReason": "Issue is from 2020 targeting SkiaSharp 1.68.x. Current SkiaSharp is 3.x; DrawText(string,float,float,SKPaint) still exists today as a deprecated method. The exact runtime assembly deployment bug on VS 2017 is unlikely to be relevant to modern tooling."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.85,
      "reason": "Reporter explicitly states 1.60.3 worked and 1.68.0+ breaks. Between these versions, SKCanvas.DrawText was refactored to delegate through SKFont (introduced in 1.68), potentially causing assembly version mismatches with VS 2017's NuGet resolution.",
      "workedInVersion": "1.60.3",
      "brokeInVersion": "1.68.0"
    }
  },
  "analysis": {
    "summary": "The MissingMethodException at runtime on Android suggests the compiled app references a method that cannot be found in the deployed SkiaSharp assembly. Between 1.60.3 and 1.68.x, DrawText(string, float, float, SKPaint) was refactored to internally call paint.GetFont() and delegate to a new SKFont-based overload. VS 2017 may have produced a stale build or incorrect TFM resolution for the Xamarin.Android platform assembly.",
    "rationale": "Classified as type/bug because this is a runtime crash with a reproducible code path and a known-good baseline version. The area is area/SkiaSharp since it involves SKCanvas.DrawText core API. Platform is os/Android because VS 2019 works (implying toolchain/NuGet packaging interaction). The tenet/compatibility label applies because upgrading from 1.60.3 to 1.68.x triggers the crash. Severity is medium — crash exists but has a clear workaround (use VS 2019 or downgrade).",
    "keySignals": [
      {
        "text": "above 1.68.0 to 1.68.2.1 / Last known good version: 1.60.3",
        "source": "issue body",
        "interpretation": "Confirmed regression introduced in 1.68.0; implies API or packaging change broke the method resolution at runtime."
      },
      {
        "text": "correct behavior on vs 2019",
        "source": "issue body",
        "interpretation": "VS 2019 resolves the NuGet package correctly; VS 2017 likely picks a wrong platform DLL or fails to update the linked SkiaSharp assembly on the Android target."
      },
      {
        "text": "canvas.DrawText(\"I am text\", 15, 15, paint)",
        "source": "issue body",
        "interpretation": "Standard usage of the existing overload DrawText(string, float, float, SKPaint), which still exists in current code as a deprecated method delegating to SKFont-based path."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "631-637",
        "finding": "DrawText(string, float, float, SKPaint) still exists (marked [Obsolete]) and delegates to DrawText(text, x, y, paint.TextAlign, paint.GetFont(), paint). In 1.68.x this delegate call was added; in 1.60.3 the implementation likely used a direct native call. The refactoring means the compiled call site matches the API surface, but if the deployed DLL is a stale older build without this overload, MissingMethodException results.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "885",
        "finding": "SKPaint.GetFont() is an internal method used by the DrawText delegation chain. This method was introduced alongside SKFont in 1.68.x. If VS 2017 deploys an old 1.60.3 Android assembly instead of 1.68.x, GetFont() and new overloads are missing, causing MissingMethodException.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use Visual Studio 2019 or later to build and deploy the project",
      "Clean the Android project, delete bin/obj folders, and re-deploy (may resolve stale assembly deployment in VS 2017)",
      "Downgrade SkiaSharp.Views.Forms to 1.60.3 if VS 2017 must be used"
    ],
    "nextQuestions": [
      "Does the issue reproduce on current SkiaSharp 3.x with a modern IDE?",
      "Is the screenshot showing MissingMethodException for DrawText itself or an internal method it calls (GetFont, etc.)?",
      "Does cleaning bin/obj and doing a full rebuild in VS 2017 resolve the issue?"
    ],
    "resolution": {
      "hypothesis": "VS 2017 NuGet/MSBuild deploys a stale or incorrect platform assembly for the Xamarin.Android target, causing a mismatch between the compile-time method resolution and the runtime assembly. A clean rebuild or switch to VS 2019 resolves it.",
      "proposals": [
        {
          "title": "Clean rebuild workaround",
          "description": "Delete bin/ and obj/ directories in both the Android project and the shared library, then rebuild and redeploy. This forces VS 2017 to deploy the correct SkiaSharp platform assembly.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Upgrade to VS 2019 or later",
          "description": "Visual Studio 2019 is confirmed to work by the reporter. VS 2017 is end-of-life. Upgrading resolves the NuGet deployment issue.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Downgrade SkiaSharp to 1.60.3",
          "description": "If VS 2017 must be used, pinning SkiaSharp.Views.Forms to 1.60.3 avoids the issue. Note: 1.60.3 is very old and lacks many features and bug fixes.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Upgrade to VS 2019 or later",
      "recommendedReason": "VS 2017 is end-of-life; the issue is specific to its toolchain. The reporter confirmed VS 2019 works."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.75,
      "reason": "Issue is from 2020 with SkiaSharp 1.68.x and VS 2017 (EOL). Needs confirmation whether this still reproduces on current SkiaSharp 3.x. The error screenshot is not readable from the issue body. VS 2019 workaround already exists per the reporter.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Does the issue reproduce with current SkiaSharp 3.x?",
      "What exact method does the MissingMethodException reference (full exception message)?",
      "Does a clean rebuild in VS 2017 (delete bin/obj) fix the issue?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area/SkiaSharp, os/Android, and tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Android",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter if this reproduces on current SkiaSharp and provide workarounds",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thanks for the report! A few questions to help us investigate:\n\n1. **Does this still happen with the latest SkiaSharp (3.x)?** This issue was filed against 1.68.x; a lot has changed since then.\n2. **Can you share the full exception message?** The screenshot isn't readable — specifically, which method does the `MissingMethodException` reference?\n3. **Have you tried a clean rebuild?** Delete the `bin/` and `obj/` folders in both your Android project and shared library, then rebuild in VS 2017. Stale assemblies can cause this type of error.\n\nIn the meantime, if you must use VS 2017, you can work around this by either:\n- Switching to **Visual Studio 2019** (confirmed to work per your report)\n- Downgrading to **SkiaSharp 1.60.3**"
      }
    ]
  }
}
```

</details>
