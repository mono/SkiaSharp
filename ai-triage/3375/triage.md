# Issue Triage Report — #3375

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T20:44:46Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.93 (93%)) |
| Suggested action | needs-investigation (0.91 (91%)) |

**Issue Summary:** Windows x86 app crashes with stack overflow in P/Invoke (sk_bitmap_get_pixel_color) on .NET 8/9 with SkiaSharp 3.116+, works on x64 and on .NET Framework x86; regression from 2.88.9.

**Analysis:** SkiaSharp 3.x introduced [LibraryImport] for .NET 7+ builds (USE_LIBRARY_IMPORT defined in SkiaSharp.csproj when targeting net7.0+). Unlike [DllImport] which explicitly sets CallingConvention.Cdecl, the [LibraryImport] attributes in SkiaApi.generated.cs have no [UnmanagedCallConv] specifying cdecl. On x86 Windows, the default P/Invoke calling convention is StdCall (caller-cleaned-up), but libSkiaSharp native functions use Cdecl (callee-cleaned-up). This mismatch corrupts the stack on every P/Invoke call, producing a stack overflow. On x64 and ARM64 there is only one calling convention so the mismatch has no effect. On .NET Framework (USE_DELEGATES path) the delegates explicitly use [UnmanagedFunctionPointer(CallingConvention.Cdecl)] and work correctly.

**Recommendations:** **needs-investigation** — Root cause is well-understood (missing [UnmanagedCallConv] on [LibraryImport]), but a repro project and CI coverage for x86 Windows are needed before the fix is merged. The issue warrants a targeted repro to confirm the fix.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |
| Current labels | type/bug, os/Windows-Classic, area/SkiaSharp, tenet/reliability |

## Evidence

### Reproduction

1. Build a .NET 8 or 9 application targeting x86 on Windows
2. Reference a library that uses SkiaSharp 3.116+ for any image operation
3. Call any SkiaSharp API (e.g., SKBitmap.GetPixel) that triggers a P/Invoke into libSkiaSharp
4. Observe stack overflow crash

**Environment:** Windows 11 ARM64 (original) + Windows x86 .NET 8 (confirmed by second reporter). Also reproduced with SkiaSharp 3.119.1.

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | crash |
| Error message | Stack overflow at SkiaSharp.SkiaApi.sk_bitmap_get_pixel_color (and all P/Invoke calls on x86 .NET 8+) |
| Repro quality | partial |
| Target frameworks | net9.0, net8.0, netstandard2.0 |

**Stack trace:**

```text
at SkiaSharp.SkiaApi.sk_bitmap_get_pixel_color(IntPtr, Int32, Int32)
[...]
at System.RuntimeMethodHandle.InvokeMethod(System.Object, Void**, System.Signature, Boolean)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 3.119.1, 2.88.9 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | The USE_LIBRARY_IMPORT code path with missing UnmanagedCallConv still present in 3.119.x; no fix PR found. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.95 (95%) |
| Reason | Two reporters independently confirm 2.88.9 works but 3.116+ crashes on x86 .NET 8/9. The regression aligns with introduction of USE_LIBRARY_IMPORT in 3.x for .NET 7+ TFMs. |
| Worked in version | 2.88.9 |
| Broke in version | 3.116.0 |

## Analysis

### Technical Summary

SkiaSharp 3.x introduced [LibraryImport] for .NET 7+ builds (USE_LIBRARY_IMPORT defined in SkiaSharp.csproj when targeting net7.0+). Unlike [DllImport] which explicitly sets CallingConvention.Cdecl, the [LibraryImport] attributes in SkiaApi.generated.cs have no [UnmanagedCallConv] specifying cdecl. On x86 Windows, the default P/Invoke calling convention is StdCall (caller-cleaned-up), but libSkiaSharp native functions use Cdecl (callee-cleaned-up). This mismatch corrupts the stack on every P/Invoke call, producing a stack overflow. On x64 and ARM64 there is only one calling convention so the mismatch has no effect. On .NET Framework (USE_DELEGATES path) the delegates explicitly use [UnmanagedFunctionPointer(CallingConvention.Cdecl)] and work correctly.

### Rationale

The bug type is clear: crash regression between 2.88.9 and 3.116 on a specific architecture/runtime combo. The stack trace to a raw SkiaApi P/Invoke method is definitive. The calling convention mismatch hypothesis is strongly supported by the code investigation: [LibraryImport] without [UnmanagedCallConv(CallConvCdecl)] on x86 Windows, combined with a cdecl native library, causes a stack imbalance that grows into a stack overflow. This is a known .NET 7 [LibraryImport] migration pitfall on x86.

### Key Signals

- "Stack overflow at SkiaSharp.SkiaApi.sk_bitmap_get_pixel_color" — **comment by TomPMoleman** (Crash happens inside the P/Invoke wrapper itself, not in user code; points to calling convention mismatch causing stack corruption per-call.)
- "x86 + net472: working; x86 + Net8.0: ISSUE" — **comment by TomPMoleman** (net472 uses USE_DELEGATES with explicit CallingConvention.Cdecl; net8.0 uses USE_LIBRARY_IMPORT without UnmanagedCallConv — this is the exact delta that explains the regression.)
- "when we rollback the SkiaSharp nuget to version 2.88.9 then DLLs loading from subdirectory also work as expected" — **issue body** (Regression introduced in 3.x, consistent with the USE_LIBRARY_IMPORT change introduced in 3.x for .NET 7+ TFMs.)
- "x64 + Net8.0: working" — **comment by TomPMoleman** (x64 and ARM64 Windows have a single unified calling convention, so the missing [UnmanagedCallConv] has no effect there — confirms x86-specific calling convention mismatch.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.generated.cs` | 1644-1661 | direct | sk_bitmap_get_pixel_color uses [LibraryImport(SKIA)] with no [UnmanagedCallConv] for the USE_LIBRARY_IMPORT path, while the USE_DELEGATES path uses [UnmanagedFunctionPointer(CallingConvention.Cdecl)] explicitly. All generated P/Invoke entries follow this pattern — [LibraryImport] without CallingConvention. |
| `binding/SkiaSharp/SkiaSharp.csproj` | — | direct | USE_DELEGATES is defined only for net4* TFMs; USE_LIBRARY_IMPORT is defined for net7.0+. For net8.0/net9.0 (the regression target), USE_LIBRARY_IMPORT is active and [LibraryImport] is used. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 118-155 | direct | All [LibraryImport] entries (e.g., gr_backendrendertarget_delete) lack [UnmanagedCallConv]. On x64/ARM64 this is benign; on x86 Windows, stdcall is assumed by default but libSkiaSharp exports cdecl functions, causing stack corruption. |

### Workarounds

- Downgrade to SkiaSharp 2.88.9 (last known good, uses USE_DELEGATES with explicit cdecl).
- Target net472 instead of net8.0 on x86 (USE_DELEGATES path is used, which correctly specifies CallingConvention.Cdecl).
- Build the consuming library for x64 instead of x86 (no calling convention distinction on x64 Windows).

### Next Questions

- Can the code generator (utils/SkiaSharpGenerator) be updated to emit [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })] on all [LibraryImport] entries?
- Is x86 tested in CI? If not, adding an x86 Windows test runner would catch this regression class automatically.
- Does the same issue affect HarfBuzzSharp (if it has a similar [LibraryImport] path)?

### Resolution Proposals

**Hypothesis:** Add [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })] to every [LibraryImport] attribute in SkiaApi.generated.cs (and the corresponding template in the SkiaSharpGenerator tool), to explicitly match the native cdecl ABI on all platforms including x86.

1. **Add [UnmanagedCallConv(CallConvCdecl)] to all [LibraryImport] entries** — fix, confidence 0.92 (92%), cost/s, validated=untested
   - Update the SkiaSharpGenerator tool so it emits [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })] on every [LibraryImport] declaration in SkiaApi.generated.cs, then regenerate the file.
2. **Downgrade to 2.88.9 (workaround)** — workaround, confidence 0.99 (99%), cost/xs, validated=yes
   - Revert to SkiaSharp 2.88.9 which uses delegate-based P/Invoke with explicit CallingConvention.Cdecl and does not have this regression.
3. **Build consuming library/app as x64** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Switch the compilation target from x86 to x64 (AnyCPU/x64) — calling convention mismatch only affects x86.

**Recommended proposal:** Add [UnmanagedCallConv(CallConvCdecl)] to all [LibraryImport] entries

**Why:** Proper fix — restores correct calling convention explicitly for all [LibraryImport] P/Invoke paths. The generator change is small and affects only the x86 code path at the ABI level.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.91 (91%) |
| Reason | Root cause is well-understood (missing [UnmanagedCallConv] on [LibraryImport]), but a repro project and CI coverage for x86 Windows are needed before the fix is merged. The issue warrants a targeted repro to confirm the fix. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Labels are already correct — type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability. Add tenet/compatibility for the .NET version regression. | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability, tenet/compatibility |
| add-comment | medium | 0.91 (91%) | Post root cause analysis and workarounds | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and for the follow-up stack trace from @TomPMoleman — that's very helpful.

**Root cause hypothesis:** SkiaSharp 3.x introduced `[LibraryImport]` for .NET 7+ builds (`USE_LIBRARY_IMPORT`). Unlike the `[DllImport]` path which explicitly sets `CallingConvention.Cdecl`, the generated `[LibraryImport]` attributes in `SkiaApi.generated.cs` do not include `[UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]`. On **x86 Windows**, the runtime defaults to StdCall rather than Cdecl, which is a calling convention mismatch with the native `libSkiaSharp.dll` (which exports Cdecl functions). This corrupts the stack with every P/Invoke call, eventually causing the stack overflow. On x64 and ARM64 there is only one calling convention so no mismatch occurs.

This also explains why:
- `x86 + net472` works: the `USE_DELEGATES` path uses `[UnmanagedFunctionPointer(CallingConvention.Cdecl)]` explicitly.
- `x64 + net8.0` works: x64 Windows has a single calling convention.
- `2.88.9` works: that release did not use `[LibraryImport]`.

**Workarounds (while a fix is prepared):**
1. **Downgrade to SkiaSharp 2.88.9** — confirmed working by both reporters.
2. **Build as x64 (AnyCPU)** — no calling convention distinction on x64.
3. **Target net472** — uses the delegate-based P/Invoke path with explicit Cdecl.

**Proposed fix:** Add `[UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]` to every `[LibraryImport]` declaration in the SkiaSharpGenerator output template, then regenerate `SkiaApi.generated.cs`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3375,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T20:44:46Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "area/SkiaSharp",
      "tenet/reliability"
    ]
  },
  "summary": "Windows x86 app crashes with stack overflow in P/Invoke (sk_bitmap_get_pixel_color) on .NET 8/9 with SkiaSharp 3.116+, works on x64 and on .NET Framework x86; regression from 2.88.9.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.93
    },
    "platforms": [
      "os/Windows-Classic"
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
      "errorMessage": "Stack overflow at SkiaSharp.SkiaApi.sk_bitmap_get_pixel_color (and all P/Invoke calls on x86 .NET 8+)",
      "stackTrace": "at SkiaSharp.SkiaApi.sk_bitmap_get_pixel_color(IntPtr, Int32, Int32)\n[...]\nat System.RuntimeMethodHandle.InvokeMethod(System.Object, Void**, System.Signature, Boolean)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net9.0",
        "net8.0",
        "netstandard2.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Build a .NET 8 or 9 application targeting x86 on Windows",
        "Reference a library that uses SkiaSharp 3.116+ for any image operation",
        "Call any SkiaSharp API (e.g., SKBitmap.GetPixel) that triggers a P/Invoke into libSkiaSharp",
        "Observe stack overflow crash"
      ],
      "environmentDetails": "Windows 11 ARM64 (original) + Windows x86 .NET 8 (confirmed by second reporter). Also reproduced with SkiaSharp 3.119.1.",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "3.119.1",
        "2.88.9"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "The USE_LIBRARY_IMPORT code path with missing UnmanagedCallConv still present in 3.119.x; no fix PR found."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.95,
      "reason": "Two reporters independently confirm 2.88.9 works but 3.116+ crashes on x86 .NET 8/9. The regression aligns with introduction of USE_LIBRARY_IMPORT in 3.x for .NET 7+ TFMs.",
      "workedInVersion": "2.88.9",
      "brokeInVersion": "3.116.0"
    }
  },
  "analysis": {
    "summary": "SkiaSharp 3.x introduced [LibraryImport] for .NET 7+ builds (USE_LIBRARY_IMPORT defined in SkiaSharp.csproj when targeting net7.0+). Unlike [DllImport] which explicitly sets CallingConvention.Cdecl, the [LibraryImport] attributes in SkiaApi.generated.cs have no [UnmanagedCallConv] specifying cdecl. On x86 Windows, the default P/Invoke calling convention is StdCall (caller-cleaned-up), but libSkiaSharp native functions use Cdecl (callee-cleaned-up). This mismatch corrupts the stack on every P/Invoke call, producing a stack overflow. On x64 and ARM64 there is only one calling convention so the mismatch has no effect. On .NET Framework (USE_DELEGATES path) the delegates explicitly use [UnmanagedFunctionPointer(CallingConvention.Cdecl)] and work correctly.",
    "rationale": "The bug type is clear: crash regression between 2.88.9 and 3.116 on a specific architecture/runtime combo. The stack trace to a raw SkiaApi P/Invoke method is definitive. The calling convention mismatch hypothesis is strongly supported by the code investigation: [LibraryImport] without [UnmanagedCallConv(CallConvCdecl)] on x86 Windows, combined with a cdecl native library, causes a stack imbalance that grows into a stack overflow. This is a known .NET 7 [LibraryImport] migration pitfall on x86.",
    "keySignals": [
      {
        "text": "Stack overflow at SkiaSharp.SkiaApi.sk_bitmap_get_pixel_color",
        "source": "comment by TomPMoleman",
        "interpretation": "Crash happens inside the P/Invoke wrapper itself, not in user code; points to calling convention mismatch causing stack corruption per-call."
      },
      {
        "text": "x86 + net472: working; x86 + Net8.0: ISSUE",
        "source": "comment by TomPMoleman",
        "interpretation": "net472 uses USE_DELEGATES with explicit CallingConvention.Cdecl; net8.0 uses USE_LIBRARY_IMPORT without UnmanagedCallConv — this is the exact delta that explains the regression."
      },
      {
        "text": "when we rollback the SkiaSharp nuget to version 2.88.9 then DLLs loading from subdirectory also work as expected",
        "source": "issue body",
        "interpretation": "Regression introduced in 3.x, consistent with the USE_LIBRARY_IMPORT change introduced in 3.x for .NET 7+ TFMs."
      },
      {
        "text": "x64 + Net8.0: working",
        "source": "comment by TomPMoleman",
        "interpretation": "x64 and ARM64 Windows have a single unified calling convention, so the missing [UnmanagedCallConv] has no effect there — confirms x86-specific calling convention mismatch."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "1644-1661",
        "finding": "sk_bitmap_get_pixel_color uses [LibraryImport(SKIA)] with no [UnmanagedCallConv] for the USE_LIBRARY_IMPORT path, while the USE_DELEGATES path uses [UnmanagedFunctionPointer(CallingConvention.Cdecl)] explicitly. All generated P/Invoke entries follow this pattern — [LibraryImport] without CallingConvention.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaSharp.csproj",
        "finding": "USE_DELEGATES is defined only for net4* TFMs; USE_LIBRARY_IMPORT is defined for net7.0+. For net8.0/net9.0 (the regression target), USE_LIBRARY_IMPORT is active and [LibraryImport] is used.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "118-155",
        "finding": "All [LibraryImport] entries (e.g., gr_backendrendertarget_delete) lack [UnmanagedCallConv]. On x64/ARM64 this is benign; on x86 Windows, stdcall is assumed by default but libSkiaSharp exports cdecl functions, causing stack corruption.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Can the code generator (utils/SkiaSharpGenerator) be updated to emit [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })] on all [LibraryImport] entries?",
      "Is x86 tested in CI? If not, adding an x86 Windows test runner would catch this regression class automatically.",
      "Does the same issue affect HarfBuzzSharp (if it has a similar [LibraryImport] path)?"
    ],
    "workarounds": [
      "Downgrade to SkiaSharp 2.88.9 (last known good, uses USE_DELEGATES with explicit cdecl).",
      "Target net472 instead of net8.0 on x86 (USE_DELEGATES path is used, which correctly specifies CallingConvention.Cdecl).",
      "Build the consuming library for x64 instead of x86 (no calling convention distinction on x64 Windows)."
    ],
    "resolution": {
      "hypothesis": "Add [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })] to every [LibraryImport] attribute in SkiaApi.generated.cs (and the corresponding template in the SkiaSharpGenerator tool), to explicitly match the native cdecl ABI on all platforms including x86.",
      "proposals": [
        {
          "title": "Add [UnmanagedCallConv(CallConvCdecl)] to all [LibraryImport] entries",
          "description": "Update the SkiaSharpGenerator tool so it emits [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })] on every [LibraryImport] declaration in SkiaApi.generated.cs, then regenerate the file.",
          "category": "fix",
          "confidence": 0.92,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Downgrade to 2.88.9 (workaround)",
          "description": "Revert to SkiaSharp 2.88.9 which uses delegate-based P/Invoke with explicit CallingConvention.Cdecl and does not have this regression.",
          "category": "workaround",
          "confidence": 0.99,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Build consuming library/app as x64",
          "description": "Switch the compilation target from x86 to x64 (AnyCPU/x64) — calling convention mismatch only affects x86.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Add [UnmanagedCallConv(CallConvCdecl)] to all [LibraryImport] entries",
      "recommendedReason": "Proper fix — restores correct calling convention explicitly for all [LibraryImport] P/Invoke paths. The generator change is small and affects only the x86 code path at the ABI level."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.91,
      "reason": "Root cause is well-understood (missing [UnmanagedCallConv] on [LibraryImport]), but a repro project and CI coverage for x86 Windows are needed before the fix is merged. The issue warrants a targeted repro to confirm the fix.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Labels are already correct — type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability. Add tenet/compatibility for the .NET version regression.",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/reliability",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post root cause analysis and workarounds",
        "risk": "medium",
        "confidence": 0.91,
        "comment": "Thanks for the detailed report and for the follow-up stack trace from @TomPMoleman — that's very helpful.\n\n**Root cause hypothesis:** SkiaSharp 3.x introduced `[LibraryImport]` for .NET 7+ builds (`USE_LIBRARY_IMPORT`). Unlike the `[DllImport]` path which explicitly sets `CallingConvention.Cdecl`, the generated `[LibraryImport]` attributes in `SkiaApi.generated.cs` do not include `[UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]`. On **x86 Windows**, the runtime defaults to StdCall rather than Cdecl, which is a calling convention mismatch with the native `libSkiaSharp.dll` (which exports Cdecl functions). This corrupts the stack with every P/Invoke call, eventually causing the stack overflow. On x64 and ARM64 there is only one calling convention so no mismatch occurs.\n\nThis also explains why:\n- `x86 + net472` works: the `USE_DELEGATES` path uses `[UnmanagedFunctionPointer(CallingConvention.Cdecl)]` explicitly.\n- `x64 + net8.0` works: x64 Windows has a single calling convention.\n- `2.88.9` works: that release did not use `[LibraryImport]`.\n\n**Workarounds (while a fix is prepared):**\n1. **Downgrade to SkiaSharp 2.88.9** — confirmed working by both reporters.\n2. **Build as x64 (AnyCPU)** — no calling convention distinction on x64.\n3. **Target net472** — uses the delegate-based P/Invoke path with explicit Cdecl.\n\n**Proposed fix:** Add `[UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]` to every `[LibraryImport]` declaration in the SkiaSharpGenerator output template, then regenerate `SkiaApi.generated.cs`."
      }
    ]
  }
}
```

</details>
