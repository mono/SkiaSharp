# Issue Triage Report — #1772

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T10:11:48Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-fixed (0.78 (78%)) |

**Issue Summary:** On CentOS Linux with .NET 5, SkiaSharp 2.80.2 throws EntryPointNotFoundException for 'sk_managedstream_set_procs', and 2.80.3 intermittently throws ArgumentNullException in OnReadManagedStream when the buffer parameter is null — both issues have since been resolved in current code.

**Analysis:** Two related issues in the 2.80.x series on Linux: (1) 2.80.2 lacked the 'sk_managedstream_set_procs' native export — the static constructor of SKAbstractManagedStream calls it immediately on first use of any managed stream type, causing a hard crash on CentOS. (2) 2.80.3 added the native function but the managed OnReadManagedStream did not guard against a null/zero buffer pointer passed from native code, causing an intermittent ArgumentNullException. Current code addresses both: the native entry point exists and the method explicitly checks buffer != IntPtr.Zero before copying.

**Recommendations:** **close-as-fixed** — Code investigation confirms both issues were fixed after 2.80.3: native symbol is present and the null-buffer path is guarded. The issue has been open since 2021 with no reporter activity; the fix is in production.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Run a .NET 5 application on CentOS Server using SkiaSharp 2.80.2
2. Instantiate or use any type that inherits from SKAbstractManagedStream (e.g., load an image from a Stream)
3. Observe EntryPointNotFoundException for sk_managedstream_set_procs

**Environment:** CentOS Linux, .NET 5, SkiaSharp 2.80.2 and 2.80.3

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | EntryPointNotFoundException: Unable to find an entry point named 'sk_managedstream_set_procs' in shared library 'libSkiaSharp' (2.80.2); ArgumentNullException: Value cannot be null. (Parameter 'buffer') at SKManagedStream.OnReadManagedStream (2.80.3) |
| Repro quality | partial |
| Target frameworks | net5.0 |

**Stack trace:**

```text
at SkiaSharp.SkiaApi.sk_managedstream_set_procs(SKManagedStreamDelegates procs)
at SkiaSharp.SKAbstractManagedStream..cctor()
---
at SkiaSharp.SKManagedStream.OnReadManagedStream(IntPtr buffer, IntPtr size)
at SkiaSharp.SKAbstractManagedStream.ReadInternal(IntPtr s, Void* context, Void* buffer, IntPtr size)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2, 2.80.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | Both issues are from the 2.80.x series. The current codebase explicitly guards against null buffer in OnReadManagedStream (line 85 check), and sk_managedstream_set_procs is present in the generated API. These issues have been addressed in subsequent releases. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.82 (82%) |
| Reason | 1) sk_managedstream_set_procs is present in SkiaApi.generated.cs and SKAbstractManagedStream..cctor — the native/managed mismatch in 2.80.2 is version-specific. 2) SKManagedStream.OnReadManagedStream now uses Utils.RentArray<byte> and only copies to buffer when buffer != IntPtr.Zero (line 85), directly fixing the ArgumentNullException. The fix appears to have landed between 2.80.3 and 2.88.x. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

Two related issues in the 2.80.x series on Linux: (1) 2.80.2 lacked the 'sk_managedstream_set_procs' native export — the static constructor of SKAbstractManagedStream calls it immediately on first use of any managed stream type, causing a hard crash on CentOS. (2) 2.80.3 added the native function but the managed OnReadManagedStream did not guard against a null/zero buffer pointer passed from native code, causing an intermittent ArgumentNullException. Current code addresses both: the native entry point exists and the method explicitly checks buffer != IntPtr.Zero before copying.

### Rationale

Both exceptions trace to SKAbstractManagedStream/SKManagedStream and are reproducible on Linux with 2.80.x. Code investigation shows the current codebase contains sk_managedstream_set_procs in the generated API and guards against null buffer in OnReadManagedStream, indicating both root causes were subsequently fixed. The issue is classified as type/bug area/SkiaSharp os/Linux. Suggested action is close-as-fixed because the code evidence strongly indicates resolution in versions after 2.80.3.

### Key Signals

- "EntryPointNotFoundException: Unable to find an entry point named 'sk_managedstream_set_procs' in shared library 'libSkiaSharp'" — **issue body** (Native library version 2.80.2 did not export this symbol; managed code expected it.)
- "With 2.80.3 version the behaviour is strange... sometimes it works, maybe depending on file type/size" — **issue body** (Intermittent failure suggests a race or data-dependent code path — consistent with the null-buffer path in OnReadManagedStream.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKAbstractManagedStream.cs` | 14-31 | direct | Static constructor calls SkiaApi.sk_managedstream_set_procs(delegates) unconditionally. If the native libSkiaSharp does not export this symbol (as in 2.80.2), an EntryPointNotFoundException is thrown at class initialization time, before any stream object is created. |
| `binding/SkiaSharp/SKManagedStream.cs` | 73-96 | direct | OnReadManagedStream now reads into a managed RentArray<byte> buffer and only copies to the native buffer pointer if buffer != IntPtr.Zero (line 85). This null guard was missing in 2.80.3, which could cause ArgumentNullException when Skia passed a null skip buffer. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | related | sk_managedstream_set_procs is present in the generated P/Invoke bindings, confirming the native function is expected and present in current native builds. |

### Workarounds

- Upgrade to SkiaSharp 2.88.x or later, which contains the sk_managedstream_set_procs export and the null-buffer guard in OnReadManagedStream.

### Resolution Proposals

**Hypothesis:** Both issues were version-specific 2.80.x bugs that were fixed in subsequent releases: the missing native export was added, and OnReadManagedStream was refactored to use a managed buffer with an explicit null guard before copying to native memory.

1. **Upgrade SkiaSharp to 2.88.x or later** — workaround, cost/xs, validated=yes
   - Both reported issues are in SkiaSharp 2.80.x. Upgrading to the latest stable release resolves both the EntryPointNotFoundException and the ArgumentNullException.

**Recommended proposal:** p1

**Why:** The fix is already in the codebase; upgrading is the simplest and most reliable resolution.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.78 (78%) |
| Reason | Code investigation confirms both issues were fixed after 2.80.3: native symbol is present and the null-buffer path is guarded. The issue has been open since 2021 with no reporter activity; the fix is in production. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type, area, platform, and tenet labels | labels=type/bug, area/SkiaSharp, os/Linux, tenet/reliability, tenet/compatibility |
| add-comment | medium | 0.78 (78%) | Inform reporter that both issues were fixed in versions after 2.80.3 and suggest upgrading | — |
| close-issue | medium | 0.78 (78%) | Close as fixed — both root causes are resolved in current code | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thank you for reporting this!

Both issues you described were specific to SkiaSharp 2.80.x:

1. **`EntryPointNotFoundException` for `sk_managedstream_set_procs` (2.80.2)** — this native export was missing in the 2.80.2 native library. It was added in 2.80.3+.

2. **`ArgumentNullException: Value cannot be null (Parameter 'buffer')` (2.80.3)** — this was a missing null-guard in `SKManagedStream.OnReadManagedStream`. The current code reads into a managed buffer first and only copies to the native pointer when it is non-null, preventing this crash.

**Recommended fix:** Upgrade to SkiaSharp **2.88.x** or the latest stable release. Both root causes have been addressed in the current codebase.

If you are still seeing either exception on a recent version, please reopen with the SkiaSharp version, .NET version, and CentOS version you are using.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1772,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T10:11:48Z"
  },
  "summary": "On CentOS Linux with .NET 5, SkiaSharp 2.80.2 throws EntryPointNotFoundException for 'sk_managedstream_set_procs', and 2.80.3 intermittently throws ArgumentNullException in OnReadManagedStream when the buffer parameter is null — both issues have since been resolved in current code.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/reliability",
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "EntryPointNotFoundException: Unable to find an entry point named 'sk_managedstream_set_procs' in shared library 'libSkiaSharp' (2.80.2); ArgumentNullException: Value cannot be null. (Parameter 'buffer') at SKManagedStream.OnReadManagedStream (2.80.3)",
      "stackTrace": "at SkiaSharp.SkiaApi.sk_managedstream_set_procs(SKManagedStreamDelegates procs)\nat SkiaSharp.SKAbstractManagedStream..cctor()\n---\nat SkiaSharp.SKManagedStream.OnReadManagedStream(IntPtr buffer, IntPtr size)\nat SkiaSharp.SKAbstractManagedStream.ReadInternal(IntPtr s, Void* context, Void* buffer, IntPtr size)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net5.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Run a .NET 5 application on CentOS Server using SkiaSharp 2.80.2",
        "Instantiate or use any type that inherits from SKAbstractManagedStream (e.g., load an image from a Stream)",
        "Observe EntryPointNotFoundException for sk_managedstream_set_procs"
      ],
      "environmentDetails": "CentOS Linux, .NET 5, SkiaSharp 2.80.2 and 2.80.3",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2",
        "2.80.3"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "Both issues are from the 2.80.x series. The current codebase explicitly guards against null buffer in OnReadManagedStream (line 85 check), and sk_managedstream_set_procs is present in the generated API. These issues have been addressed in subsequent releases."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.82,
      "reason": "1) sk_managedstream_set_procs is present in SkiaApi.generated.cs and SKAbstractManagedStream..cctor — the native/managed mismatch in 2.80.2 is version-specific. 2) SKManagedStream.OnReadManagedStream now uses Utils.RentArray<byte> and only copies to buffer when buffer != IntPtr.Zero (line 85), directly fixing the ArgumentNullException. The fix appears to have landed between 2.80.3 and 2.88.x."
    }
  },
  "analysis": {
    "summary": "Two related issues in the 2.80.x series on Linux: (1) 2.80.2 lacked the 'sk_managedstream_set_procs' native export — the static constructor of SKAbstractManagedStream calls it immediately on first use of any managed stream type, causing a hard crash on CentOS. (2) 2.80.3 added the native function but the managed OnReadManagedStream did not guard against a null/zero buffer pointer passed from native code, causing an intermittent ArgumentNullException. Current code addresses both: the native entry point exists and the method explicitly checks buffer != IntPtr.Zero before copying.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKAbstractManagedStream.cs",
        "lines": "14-31",
        "finding": "Static constructor calls SkiaApi.sk_managedstream_set_procs(delegates) unconditionally. If the native libSkiaSharp does not export this symbol (as in 2.80.2), an EntryPointNotFoundException is thrown at class initialization time, before any stream object is created.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKManagedStream.cs",
        "lines": "73-96",
        "finding": "OnReadManagedStream now reads into a managed RentArray<byte> buffer and only copies to the native buffer pointer if buffer != IntPtr.Zero (line 85). This null guard was missing in 2.80.3, which could cause ArgumentNullException when Skia passed a null skip buffer.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "sk_managedstream_set_procs is present in the generated P/Invoke bindings, confirming the native function is expected and present in current native builds.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "EntryPointNotFoundException: Unable to find an entry point named 'sk_managedstream_set_procs' in shared library 'libSkiaSharp'",
        "source": "issue body",
        "interpretation": "Native library version 2.80.2 did not export this symbol; managed code expected it."
      },
      {
        "text": "With 2.80.3 version the behaviour is strange... sometimes it works, maybe depending on file type/size",
        "source": "issue body",
        "interpretation": "Intermittent failure suggests a race or data-dependent code path — consistent with the null-buffer path in OnReadManagedStream."
      }
    ],
    "rationale": "Both exceptions trace to SKAbstractManagedStream/SKManagedStream and are reproducible on Linux with 2.80.x. Code investigation shows the current codebase contains sk_managedstream_set_procs in the generated API and guards against null buffer in OnReadManagedStream, indicating both root causes were subsequently fixed. The issue is classified as type/bug area/SkiaSharp os/Linux. Suggested action is close-as-fixed because the code evidence strongly indicates resolution in versions after 2.80.3.",
    "workarounds": [
      "Upgrade to SkiaSharp 2.88.x or later, which contains the sk_managedstream_set_procs export and the null-buffer guard in OnReadManagedStream."
    ],
    "resolution": {
      "hypothesis": "Both issues were version-specific 2.80.x bugs that were fixed in subsequent releases: the missing native export was added, and OnReadManagedStream was refactored to use a managed buffer with an explicit null guard before copying to native memory.",
      "proposals": [
        {
          "category": "workaround",
          "title": "Upgrade SkiaSharp to 2.88.x or later",
          "description": "Both reported issues are in SkiaSharp 2.80.x. Upgrading to the latest stable release resolves both the EntryPointNotFoundException and the ArgumentNullException.",
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "p1",
      "recommendedReason": "The fix is already in the codebase; upgrading is the simplest and most reliable resolution."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.78,
      "reason": "Code investigation confirms both issues were fixed after 2.80.3: native symbol is present and the null-buffer path is guarded. The issue has been open since 2021 with no reporter activity; the fix is in production.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type, area, platform, and tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Linux",
          "tenet/reliability",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter that both issues were fixed in versions after 2.80.3 and suggest upgrading",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thank you for reporting this!\n\nBoth issues you described were specific to SkiaSharp 2.80.x:\n\n1. **`EntryPointNotFoundException` for `sk_managedstream_set_procs` (2.80.2)** — this native export was missing in the 2.80.2 native library. It was added in 2.80.3+.\n\n2. **`ArgumentNullException: Value cannot be null (Parameter 'buffer')` (2.80.3)** — this was a missing null-guard in `SKManagedStream.OnReadManagedStream`. The current code reads into a managed buffer first and only copies to the native pointer when it is non-null, preventing this crash.\n\n**Recommended fix:** Upgrade to SkiaSharp **2.88.x** or the latest stable release. Both root causes have been addressed in the current codebase.\n\nIf you are still seeing either exception on a recent version, please reopen with the SkiaSharp version, .NET version, and CentOS version you are using."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed — both root causes are resolved in current code",
        "risk": "medium",
        "confidence": 0.78,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
