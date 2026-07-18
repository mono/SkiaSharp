# Issue Triage Report — #4374

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-18T05:06:00Z |
| Type | type/bug (0.75 (75%)) |
| Area | area/libSkiaSharp.native (0.85 (85%)) |
| Suggested action | needs-info (0.82 (82%)) |

**Issue Summary:** Reporter gets EntryPointNotFoundException for 'sk_compatpaint_new' after upgrading to v4.148.0 on Linux (Kubernetes pod); root cause is a version mismatch between the managed SkiaSharp package and the native libSkiaSharp assets.

**Analysis:** EntryPointNotFoundException for 'sk_compatpaint_new' is caused by a mismatch between the managed SkiaSharp package (which was updated to call sk_compatpaint_new_with_font in v4) and the native libSkiaSharp.so in the Kubernetes pod (which may still be v3.x from a cached layer or incomplete upgrade). The current codebase has no reference to the bare 'sk_compatpaint_new' symbol — it was replaced by 'sk_compatpaint_new_with_font'.

**Recommendations:** **needs-info** — Maintainer has diagnosed this as a version mismatch but reporter has not confirmed. The version info in the issue is contradictory. We need the reporter to verify the exact package versions deployed in the pod.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | — |
| Perf | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Upgrade SkiaSharp and SkiaSharp.NativeAssets.Linux from 3.x to 4.148.0
2. Deploy application to a Linux-based Kubernetes pod
3. Run the application — EntryPointNotFoundException is thrown

**Environment:** Linux Kubernetes pod, SkiaSharp 4.148.0 (title), but form fields indicate 3.116.0 as current version — version info is contradictory

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | exception |
| Error message | System.EntryPointNotFoundException: Unable to find an entry point named 'sk_compatpaint_new' in shared library 'libSkiaSharp'. |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9, 4.148.0 |
| Worked in | 3.x |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | sk_compatpaint_new was removed and replaced by sk_compatpaint_new_with_font in the 4.x C API; if the managed package version and native library version do not match, this exact error occurs. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.60 (60%) |
| Reason | Reporter says the app works on 3.x but fails on 4.148.0. However the form fields state the current version is 3.116.0, indicating potentially inconsistent version reporting. Most likely a deployment cache or version mismatch during upgrade. |
| Worked in version | 3.116.0 |
| Broke in version | 4.148.0 |

## Analysis

### Technical Summary

EntryPointNotFoundException for 'sk_compatpaint_new' is caused by a mismatch between the managed SkiaSharp package (which was updated to call sk_compatpaint_new_with_font in v4) and the native libSkiaSharp.so in the Kubernetes pod (which may still be v3.x from a cached layer or incomplete upgrade). The current codebase has no reference to the bare 'sk_compatpaint_new' symbol — it was replaced by 'sk_compatpaint_new_with_font'.

### Rationale

Code investigation confirms that sk_compatpaint_new (without _with_font suffix) no longer exists in the current C# or C API. The current SKPaint constructor uses sk_compatpaint_new_with_font. This means the reporter's running environment has mismatched package versions — an older native lib with a newer C# wrapper or vice versa. The maintainer (mattleibow) already confirmed this diagnosis in a comment.

### Key Signals

- "Unable to find an entry point named 'sk_compatpaint_new' in shared library 'libSkiaSharp'" — **issue body** (The C# binding is calling sk_compatpaint_new which no longer exists in the v4.x native library — classic version mismatch.)
- "After upgrading SkiaSharp and SkiaSharp.NativeAssets.Linux from 3.x to 4.148.0" — **issue body** (Reporter believes they upgraded both packages, but the Kubernetes pod may have a cached/stale native layer.)
- "We removed that method, so this sounds like you have an older SkiaSharp package compared to the native assets." — **maintainer comment by mattleibow** (Maintainer has confirmed this is a version mismatch, not a genuine defect in SkiaSharp itself.)
- "Version of SkiaSharp: 3.116.0 (Current)" — **issue form fields** (The version form fields are inconsistent with the title claiming v4.148.0 — reporter may have filled the form incorrectly, or may still be running 3.116.0 managed package against 4.x native.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | direct | The current generated API has sk_compatpaint_new_with_font but no bare sk_compatpaint_new symbol — the bare function was removed in the v4 API surface. |
| `binding/SkiaSharp/SKPaint.cs` | — | direct | SKPaint constructor calls SkiaApi.sk_compatpaint_new_with_font — confirms the v4.x C# layer expects the new symbol, not the old one. Any environment loading a v3.x native lib against v4 managed code will throw EntryPointNotFoundException. |

### Workarounds

- Ensure both the SkiaSharp managed package and SkiaSharp.NativeAssets.Linux are exactly the same version (e.g., both 4.148.0).
- Rebuild the Docker/Kubernetes pod image from scratch (no layer caching) to avoid stale native artifacts.
- Run `dotnet nuget locals all --clear` to clear the NuGet package cache before building.

### Next Questions

- What exact versions of SkiaSharp and SkiaSharp.NativeAssets.Linux are actually deployed in the pod (not just the project reference)?
- Is the Kubernetes pod image rebuilt from scratch or does it use a cached layer that may contain an older libSkiaSharp.so?
- Does `dotnet nuget locals all --clear` and a full pod image rebuild resolve the issue?

### Resolution Proposals

**Hypothesis:** The reporter has a version mismatch: either the managed SkiaSharp C# package is older than the native library (or vice versa). The Kubernetes container image likely has a cached layer with an older libSkiaSharp.so while the app was compiled against newer bindings.

1. **Verify and align package versions** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Confirm both SkiaSharp and SkiaSharp.NativeAssets.Linux packages are at exactly the same version (4.148.0). Run `dotnet list package` inside the container to verify. Rebuild the container image from scratch without cache.

**Recommended proposal:** Verify and align package versions

**Why:** This is the root cause diagnosed by the maintainer — ensuring version alignment will resolve the issue.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.82 (82%) |
| Reason | Maintainer has diagnosed this as a version mismatch but reporter has not confirmed. The version info in the issue is contradictory. We need the reporter to verify the exact package versions deployed in the pod. |
| Suggested repro platform | linux |

### Missing Info

- Exact SkiaSharp managed package version actually deployed in the pod (output of `dotnet list package`)
- Exact SkiaSharp.NativeAssets.Linux version in the pod
- Whether rebuilding the container image from scratch (no cache) resolves the issue

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, native, linux labels | labels=type/bug, area/libSkiaSharp.native, os/Linux |
| add-comment | medium | 0.82 (82%) | Ask reporter to confirm actual deployed package versions and whether a clean image rebuild resolves the issue | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report. As @mattleibow noted, the `sk_compatpaint_new` symbol was removed in v4 and replaced by `sk_compatpaint_new_with_font`. This error typically means the **managed SkiaSharp C# package** and the **native `SkiaSharp.NativeAssets.Linux` package** are different versions.

The version fields in the issue also appear contradictory (title says v4.148.0, form says 3.116.0). Could you please:

1. Run `dotnet list package` inside your Kubernetes pod and share the output — we need to see the exact resolved versions of both `SkiaSharp` and `SkiaSharp.NativeAssets.Linux`.
2. Try rebuilding your container image **from scratch** (disable Docker layer caching: `docker build --no-cache`) to ensure no stale native library is cached.
3. Run `dotnet nuget locals all --clear` before rebuilding.

Both packages must be **exactly the same version**. If they match and the issue persists after a clean rebuild, please reopen with the `dotnet list package` output.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4374,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-18T05:06:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter gets EntryPointNotFoundException for 'sk_compatpaint_new' after upgrading to v4.148.0 on Linux (Kubernetes pod); root cause is a version mismatch between the managed SkiaSharp package and the native libSkiaSharp assets.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.75
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.85
    },
    "platforms": [
      "os/Linux"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "System.EntryPointNotFoundException: Unable to find an entry point named 'sk_compatpaint_new' in shared library 'libSkiaSharp'.",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Upgrade SkiaSharp and SkiaSharp.NativeAssets.Linux from 3.x to 4.148.0",
        "Deploy application to a Linux-based Kubernetes pod",
        "Run the application — EntryPointNotFoundException is thrown"
      ],
      "environmentDetails": "Linux Kubernetes pod, SkiaSharp 4.148.0 (title), but form fields indicate 3.116.0 as current version — version info is contradictory"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9",
        "4.148.0"
      ],
      "workedIn": "3.x",
      "currentRelevance": "likely",
      "relevanceReason": "sk_compatpaint_new was removed and replaced by sk_compatpaint_new_with_font in the 4.x C API; if the managed package version and native library version do not match, this exact error occurs."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.6,
      "reason": "Reporter says the app works on 3.x but fails on 4.148.0. However the form fields state the current version is 3.116.0, indicating potentially inconsistent version reporting. Most likely a deployment cache or version mismatch during upgrade.",
      "workedInVersion": "3.116.0",
      "brokeInVersion": "4.148.0"
    }
  },
  "analysis": {
    "summary": "EntryPointNotFoundException for 'sk_compatpaint_new' is caused by a mismatch between the managed SkiaSharp package (which was updated to call sk_compatpaint_new_with_font in v4) and the native libSkiaSharp.so in the Kubernetes pod (which may still be v3.x from a cached layer or incomplete upgrade). The current codebase has no reference to the bare 'sk_compatpaint_new' symbol — it was replaced by 'sk_compatpaint_new_with_font'.",
    "rationale": "Code investigation confirms that sk_compatpaint_new (without _with_font suffix) no longer exists in the current C# or C API. The current SKPaint constructor uses sk_compatpaint_new_with_font. This means the reporter's running environment has mismatched package versions — an older native lib with a newer C# wrapper or vice versa. The maintainer (mattleibow) already confirmed this diagnosis in a comment.",
    "keySignals": [
      {
        "text": "Unable to find an entry point named 'sk_compatpaint_new' in shared library 'libSkiaSharp'",
        "source": "issue body",
        "interpretation": "The C# binding is calling sk_compatpaint_new which no longer exists in the v4.x native library — classic version mismatch."
      },
      {
        "text": "After upgrading SkiaSharp and SkiaSharp.NativeAssets.Linux from 3.x to 4.148.0",
        "source": "issue body",
        "interpretation": "Reporter believes they upgraded both packages, but the Kubernetes pod may have a cached/stale native layer."
      },
      {
        "text": "We removed that method, so this sounds like you have an older SkiaSharp package compared to the native assets.",
        "source": "maintainer comment by mattleibow",
        "interpretation": "Maintainer has confirmed this is a version mismatch, not a genuine defect in SkiaSharp itself."
      },
      {
        "text": "Version of SkiaSharp: 3.116.0 (Current)",
        "source": "issue form fields",
        "interpretation": "The version form fields are inconsistent with the title claiming v4.148.0 — reporter may have filled the form incorrectly, or may still be running 3.116.0 managed package against 4.x native."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "The current generated API has sk_compatpaint_new_with_font but no bare sk_compatpaint_new symbol — the bare function was removed in the v4 API surface.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "finding": "SKPaint constructor calls SkiaApi.sk_compatpaint_new_with_font — confirms the v4.x C# layer expects the new symbol, not the old one. Any environment loading a v3.x native lib against v4 managed code will throw EntryPointNotFoundException.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "What exact versions of SkiaSharp and SkiaSharp.NativeAssets.Linux are actually deployed in the pod (not just the project reference)?",
      "Is the Kubernetes pod image rebuilt from scratch or does it use a cached layer that may contain an older libSkiaSharp.so?",
      "Does `dotnet nuget locals all --clear` and a full pod image rebuild resolve the issue?"
    ],
    "workarounds": [
      "Ensure both the SkiaSharp managed package and SkiaSharp.NativeAssets.Linux are exactly the same version (e.g., both 4.148.0).",
      "Rebuild the Docker/Kubernetes pod image from scratch (no layer caching) to avoid stale native artifacts.",
      "Run `dotnet nuget locals all --clear` to clear the NuGet package cache before building."
    ],
    "resolution": {
      "hypothesis": "The reporter has a version mismatch: either the managed SkiaSharp C# package is older than the native library (or vice versa). The Kubernetes container image likely has a cached layer with an older libSkiaSharp.so while the app was compiled against newer bindings.",
      "proposals": [
        {
          "title": "Verify and align package versions",
          "description": "Confirm both SkiaSharp and SkiaSharp.NativeAssets.Linux packages are at exactly the same version (4.148.0). Run `dotnet list package` inside the container to verify. Rebuild the container image from scratch without cache.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Verify and align package versions",
      "recommendedReason": "This is the root cause diagnosed by the maintainer — ensuring version alignment will resolve the issue."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.82,
      "reason": "Maintainer has diagnosed this as a version mismatch but reporter has not confirmed. The version info in the issue is contradictory. We need the reporter to verify the exact package versions deployed in the pod.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Exact SkiaSharp managed package version actually deployed in the pod (output of `dotnet list package`)",
      "Exact SkiaSharp.NativeAssets.Linux version in the pod",
      "Whether rebuilding the container image from scratch (no cache) resolves the issue"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, native, linux labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Linux"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter to confirm actual deployed package versions and whether a clean image rebuild resolves the issue",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the report. As @mattleibow noted, the `sk_compatpaint_new` symbol was removed in v4 and replaced by `sk_compatpaint_new_with_font`. This error typically means the **managed SkiaSharp C# package** and the **native `SkiaSharp.NativeAssets.Linux` package** are different versions.\n\nThe version fields in the issue also appear contradictory (title says v4.148.0, form says 3.116.0). Could you please:\n\n1. Run `dotnet list package` inside your Kubernetes pod and share the output — we need to see the exact resolved versions of both `SkiaSharp` and `SkiaSharp.NativeAssets.Linux`.\n2. Try rebuilding your container image **from scratch** (disable Docker layer caching: `docker build --no-cache`) to ensure no stale native library is cached.\n3. Run `dotnet nuget locals all --clear` before rebuilding.\n\nBoth packages must be **exactly the same version**. If they match and the issue persists after a clean rebuild, please reopen with the `dotnet list package` output."
      }
    ]
  }
}
```

</details>
