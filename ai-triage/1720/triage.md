# Issue Triage Report — #1720

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T11:12:21Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.HarfBuzz (0.90 (90%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** HarfBuzz Blob creation with a ReleaseDelegate crashes with a Mono JIT assertion in Uno WASM applications on SkiaSharp 2.80.3.

**Analysis:** When BlobExtensions.ToHarfBuzzBlob creates a Blob with a managed ReleaseDelegate on Uno WASM, the HarfBuzz C API (hb_blob_create) receives a native function pointer (DestroyProxy) that must call back into the Mono managed runtime. The Mono WASM JIT at the version used (Uno.DotnetRuntime.WebAssembly 3.0.0-dev.87) hits an unsupported code path (mini.c:4231 assertion 'should not be reached'), suggesting the WASM AOT/JIT runtime could not handle this native-to-managed callback trampoline at that point in time. A secondary code issue was also found: Blob.FromStream passes a pointer from a fixed block to the Blob constructor, but the fixed block exits before the Blob's lifetime ends, which is a dangling-pointer bug on all platforms.

**Recommendations:** **needs-investigation** — A complete repro repo is provided and the crash site is identified. However, the issue was filed against outdated pre-release WASM tooling (SkiaSharp 2.80.3, Uno dev bootstrapper 3.0.0-dev.87) and has not been confirmed on current versions. Investigation should verify whether the crash still occurs with current SkiaSharp and Uno WASM.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.HarfBuzz |
| Platforms | os/WASM |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | partner/unoplatform |

## Evidence

### Reproduction

1. Create an Uno.Wasm application using SkiaSharp 2.80.3
2. Use TopTen.RichTextKit to render a TextBlock
3. RichTextKit calls BlobExtensions.ToHarfBuzzBlob on a SKStreamAsset
4. BlobExtensions.cs line 24 creates Blob with ReleaseDelegate pointing to asset.Dispose()
5. hb_blob_create is called with DestroyProxy function pointer
6. Mono WASM runtime hits assertion at mini.c:4231 during native callback setup

**Environment:** SkiaSharp 2.80.3, Uno.DotnetRuntime.WebAssembly 3.0.0-dev.87, IDE: VS Code / VS4Mac, Target: Browser (WASM)

**Repository links:**
- https://github.com/nor0x/skiasharp-1720 — Reporter's WASM reproduction repository

**Screenshots:**
- https://user-images.githubusercontent.com/3210391/121664278-9c908c00-caa7-11eb-903d-abf642b2d93c.png — Browser console showing the Mono assertion crash

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | Assertion: should not be reached at /__w/Uno.DotnetRuntime.WebAssembly/Uno.DotnetRuntime.WebAssembly/runtime/src/mono/mono/mini/mini.c:4231 |
| Repro quality | complete |
| Target frameworks | netstandard2.0 |

**Stack trace:**

```text
mono_wasm_invoke_method -> managed__Uno__Windows_UI_Core_CoreDispatcher_DispatcherCallback (via dotnet.js WASM trampoline)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Reported against SkiaSharp 2.80.3 and Uno.DotnetRuntime.WebAssembly 3.0.0-dev.87 (pre-release) in mid-2021. Mono/WASM runtime has evolved significantly since then; the issue may have been resolved in newer runtime versions. |

## Analysis

### Technical Summary

When BlobExtensions.ToHarfBuzzBlob creates a Blob with a managed ReleaseDelegate on Uno WASM, the HarfBuzz C API (hb_blob_create) receives a native function pointer (DestroyProxy) that must call back into the Mono managed runtime. The Mono WASM JIT at the version used (Uno.DotnetRuntime.WebAssembly 3.0.0-dev.87) hits an unsupported code path (mini.c:4231 assertion 'should not be reached'), suggesting the WASM AOT/JIT runtime could not handle this native-to-managed callback trampoline at that point in time. A secondary code issue was also found: Blob.FromStream passes a pointer from a fixed block to the Blob constructor, but the fixed block exits before the Blob's lifetime ends, which is a dangling-pointer bug on all platforms.

### Rationale

Classified as type/bug in area/SkiaSharp.HarfBuzz because the crash is triggered by SkiaSharp's BlobExtensions.ToHarfBuzzBlob API when used on WASM. The root cause is an incompatibility between the managed ReleaseDelegate callback mechanism and the Uno WASM Mono runtime version. The action is needs-investigation because a complete repro repo is provided but the issue was filed against old pre-release tooling and may be fixed in current WASM runtimes.

### Key Signals

- "Assertion: should not be reached at /__w/Uno.DotnetRuntime.WebAssembly/.../mono/mini/mini.c:4231" — **browser console log in issue body** (Mono JIT/AOT hit an internal assertion — an unsupported code path. This is a runtime-level failure, not a SkiaSharp API logic error.)
- "it happens on creation of the HarfBuzzSharp Blob for the Face" — **issue body** (Reporter has narrowed the crash to Blob creation, pointing directly at BlobExtensions.cs line 24.)
- "Uno.Wasm Bootstrapper: 3.0.0-dev.87" — **Basic Information section of issue body** (Pre-release development build of the Uno WASM bootstrapper from mid-2021 — WASM Mono runtime was still maturing and native-to-managed callbacks may not have been fully supported.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/BlobExtensions.cs` | 10-38 | direct | ToHarfBuzzBlob creates a Blob with `new Blob(memoryBase, size, MemoryMode.ReadOnly, () => asset.Dispose())` on the GetMemoryBase path (line 24). This passes a managed lambda as the ReleaseDelegate. The lambda is marshalled via DelegateProxies.DestroyProxy (a static native function pointer), which is what HarfBuzz's hb_blob_create receives as the destroy callback. |
| `binding/HarfBuzzSharp/Blob.cs` | 84-89 | direct | Blob.Create calls DelegateProxies.Create to wrap the ReleaseDelegate in a GCHandle, then passes DelegateProxies.DestroyProxy (a function pointer to a static [MonoPInvokeCallback]-decorated method) to hb_blob_create. The [MonoPInvokeCallback] attribute is present in the generated code, but the WASM runtime version used may not have fully supported native-to-managed callbacks at the time. |
| `binding/HarfBuzzSharp/Blob.cs` | 71-82 | related | Blob.FromStream contains a separate dangling-pointer bug: it pins `data` with `fixed (byte* dataPtr = data)` and passes the resulting pointer to the Blob constructor, but the fixed block exits immediately after the return. Once the fixed block exits, the GC may move the array, leaving a dangling pointer in the native Blob. This is a separate defect from the WASM assertion crash but affects all platforms. |
| `binding/HarfBuzzSharp/HarfBuzzApi.generated.cs` | — | context | DestroyProxy is declared with [MonoPInvokeCallback(typeof(DestroyProxyDelegate))]. The attribute is present and correct, suggesting the codegen was aware of WASM callback requirements, but the older WASM Mono runtime (3.0.0-dev.87) may not have processed it correctly. |

**Error fingerprint:** `Mono-WASM-assertion-mini.c-HarfBuzz-Blob-ReleaseDelegate`

### Next Questions

- Does the crash reproduce on current SkiaSharp (2.88+) with current Uno WASM bootstrapper?
- Does passing null as the ReleaseDelegate (and manually managing memory) avoid the crash on 2.80.3?
- Is the Blob.FromStream dangling-pointer bug worth filing as a separate issue?

### Resolution Proposals

**Hypothesis:** The crash is caused by the Mono WASM runtime (version 3.0.0-dev.87) failing to handle a native-to-managed callback (DestroyProxy) when invoked from hb_blob_create. Newer WASM runtimes may handle this correctly. A SkiaSharp-side mitigation is to avoid passing a ReleaseDelegate on WASM or to copy the data with Marshal.AllocCoTaskMem.

1. **Verify reproduction on current SkiaSharp + Uno WASM versions** — investigation, cost/s, validated=untested
   - Build the reporter's repro repo against current SkiaSharp (2.88+) and current Uno WASM bootstrapper to determine if the issue has been resolved upstream in the Mono/WASM runtime.
2. **Use Marshal.AllocCoTaskMem path to avoid managed delegate callback** — workaround, cost/s, validated=untested
   - Load font data into unmanaged memory explicitly so the destroy callback is a simple Marshal.FreeCoTaskMem lambda, which avoids the managed closure that triggers the Mono WASM assertion.

```csharp
using var ms = new MemoryStream();
stream.CopyTo(ms);
var fontData = ms.ToArray();
var ptr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontData.Length);
System.Runtime.InteropServices.Marshal.Copy(fontData, 0, ptr, fontData.Length);
var blob = new HarfBuzzSharp.Blob(ptr, fontData.Length,
    HarfBuzzSharp.MemoryMode.ReadOnly,
    () => System.Runtime.InteropServices.Marshal.FreeCoTaskMem(ptr));
blob.MakeImmutable();
```

**Recommended proposal:** Verify reproduction on current SkiaSharp + Uno WASM versions

**Why:** The crash was reported against pre-release WASM tooling from mid-2021. Verifying on current versions is the lowest-effort path and may show the issue is already resolved.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | A complete repro repo is provided and the crash site is identified. However, the issue was filed against outdated pre-release WASM tooling (SkiaSharp 2.80.3, Uno dev bootstrapper 3.0.0-dev.87) and has not been confirmed on current versions. Investigation should verify whether the crash still occurs with current SkiaSharp and Uno WASM. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply classification labels | labels=type/bug, area/SkiaSharp.HarfBuzz, os/WASM, tenet/reliability, partner/unoplatform |
| add-comment | medium | 0.80 (80%) | Acknowledge the crash, explain the likely cause, and request verification on current versions | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the repro repo!

The assertion `should not be reached at mini.c:4231` originates from the Mono WASM JIT/runtime, not from SkiaSharp/HarfBuzz directly. What happens is:

1. `BlobExtensions.ToHarfBuzzBlob` creates a `HarfBuzzSharp.Blob` with a managed `ReleaseDelegate` (a lambda that calls `asset.Dispose()`).
2. This delegate is passed to `hb_blob_create` as a native destroy callback via `DelegateProxies.DestroyProxy`.
3. On the Uno WASM runtime version you were using (`3.0.0-dev.87` — a pre-release from mid-2021), the Mono JIT hit an unsupported code path when trying to set up the native-to-managed callback trampoline.

Could you check if this still reproduces on a current version of SkiaSharp (2.88+) and the latest Uno WASM bootstrapper? The WASM Mono runtime has matured significantly since 2021.

**Temporary workaround** (if you need to unblock now): Load the font data manually into unmanaged memory to avoid the managed delegate callback:

```csharp
using var ms = new MemoryStream();
stream.CopyTo(ms);
var fontData = ms.ToArray();
var ptr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontData.Length);
System.Runtime.InteropServices.Marshal.Copy(fontData, 0, ptr, fontData.Length);
var blob = new HarfBuzzSharp.Blob(ptr, fontData.Length,
    HarfBuzzSharp.MemoryMode.ReadOnly,
    () => System.Runtime.InteropServices.Marshal.FreeCoTaskMem(ptr));
blob.MakeImmutable();
```

This uses `Marshal.AllocCoTaskMem` (unmanaged memory) so the destroy callback only calls `FreeCoTaskMem`, which is simpler and less likely to trigger the WASM trampoline issue.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1720,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T11:12:21Z"
  },
  "summary": "HarfBuzz Blob creation with a ReleaseDelegate crashes with a Mono JIT assertion in Uno WASM applications on SkiaSharp 2.80.3.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.HarfBuzz",
      "confidence": 0.9
    },
    "platforms": [
      "os/WASM"
    ],
    "tenets": [
      "tenet/reliability"
    ],
    "partner": "partner/unoplatform"
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "Assertion: should not be reached at /__w/Uno.DotnetRuntime.WebAssembly/Uno.DotnetRuntime.WebAssembly/runtime/src/mono/mono/mini/mini.c:4231",
      "stackTrace": "mono_wasm_invoke_method -> managed__Uno__Windows_UI_Core_CoreDispatcher_DispatcherCallback (via dotnet.js WASM trampoline)",
      "reproQuality": "complete",
      "targetFrameworks": [
        "netstandard2.0"
      ]
    },
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/nor0x/skiasharp-1720",
          "description": "Reporter's WASM reproduction repository"
        }
      ],
      "screenshots": [
        {
          "url": "https://user-images.githubusercontent.com/3210391/121664278-9c908c00-caa7-11eb-903d-abf642b2d93c.png",
          "description": "Browser console showing the Mono assertion crash"
        }
      ],
      "stepsToReproduce": [
        "Create an Uno.Wasm application using SkiaSharp 2.80.3",
        "Use TopTen.RichTextKit to render a TextBlock",
        "RichTextKit calls BlobExtensions.ToHarfBuzzBlob on a SKStreamAsset",
        "BlobExtensions.cs line 24 creates Blob with ReleaseDelegate pointing to asset.Dispose()",
        "hb_blob_create is called with DestroyProxy function pointer",
        "Mono WASM runtime hits assertion at mini.c:4231 during native callback setup"
      ],
      "environmentDetails": "SkiaSharp 2.80.3, Uno.DotnetRuntime.WebAssembly 3.0.0-dev.87, IDE: VS Code / VS4Mac, Target: Browser (WASM)"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.3"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Reported against SkiaSharp 2.80.3 and Uno.DotnetRuntime.WebAssembly 3.0.0-dev.87 (pre-release) in mid-2021. Mono/WASM runtime has evolved significantly since then; the issue may have been resolved in newer runtime versions."
    }
  },
  "analysis": {
    "summary": "When BlobExtensions.ToHarfBuzzBlob creates a Blob with a managed ReleaseDelegate on Uno WASM, the HarfBuzz C API (hb_blob_create) receives a native function pointer (DestroyProxy) that must call back into the Mono managed runtime. The Mono WASM JIT at the version used (Uno.DotnetRuntime.WebAssembly 3.0.0-dev.87) hits an unsupported code path (mini.c:4231 assertion 'should not be reached'), suggesting the WASM AOT/JIT runtime could not handle this native-to-managed callback trampoline at that point in time. A secondary code issue was also found: Blob.FromStream passes a pointer from a fixed block to the Blob constructor, but the fixed block exits before the Blob's lifetime ends, which is a dangling-pointer bug on all platforms.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/BlobExtensions.cs",
        "lines": "10-38",
        "finding": "ToHarfBuzzBlob creates a Blob with `new Blob(memoryBase, size, MemoryMode.ReadOnly, () => asset.Dispose())` on the GetMemoryBase path (line 24). This passes a managed lambda as the ReleaseDelegate. The lambda is marshalled via DelegateProxies.DestroyProxy (a static native function pointer), which is what HarfBuzz's hb_blob_create receives as the destroy callback.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp/Blob.cs",
        "lines": "84-89",
        "finding": "Blob.Create calls DelegateProxies.Create to wrap the ReleaseDelegate in a GCHandle, then passes DelegateProxies.DestroyProxy (a function pointer to a static [MonoPInvokeCallback]-decorated method) to hb_blob_create. The [MonoPInvokeCallback] attribute is present in the generated code, but the WASM runtime version used may not have fully supported native-to-managed callbacks at the time.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp/Blob.cs",
        "lines": "71-82",
        "finding": "Blob.FromStream contains a separate dangling-pointer bug: it pins `data` with `fixed (byte* dataPtr = data)` and passes the resulting pointer to the Blob constructor, but the fixed block exits immediately after the return. Once the fixed block exits, the GC may move the array, leaving a dangling pointer in the native Blob. This is a separate defect from the WASM assertion crash but affects all platforms.",
        "relevance": "related"
      },
      {
        "file": "binding/HarfBuzzSharp/HarfBuzzApi.generated.cs",
        "finding": "DestroyProxy is declared with [MonoPInvokeCallback(typeof(DestroyProxyDelegate))]. The attribute is present and correct, suggesting the codegen was aware of WASM callback requirements, but the older WASM Mono runtime (3.0.0-dev.87) may not have processed it correctly.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Assertion: should not be reached at /__w/Uno.DotnetRuntime.WebAssembly/.../mono/mini/mini.c:4231",
        "source": "browser console log in issue body",
        "interpretation": "Mono JIT/AOT hit an internal assertion — an unsupported code path. This is a runtime-level failure, not a SkiaSharp API logic error."
      },
      {
        "text": "it happens on creation of the HarfBuzzSharp Blob for the Face",
        "source": "issue body",
        "interpretation": "Reporter has narrowed the crash to Blob creation, pointing directly at BlobExtensions.cs line 24."
      },
      {
        "text": "Uno.Wasm Bootstrapper: 3.0.0-dev.87",
        "source": "Basic Information section of issue body",
        "interpretation": "Pre-release development build of the Uno WASM bootstrapper from mid-2021 — WASM Mono runtime was still maturing and native-to-managed callbacks may not have been fully supported."
      }
    ],
    "rationale": "Classified as type/bug in area/SkiaSharp.HarfBuzz because the crash is triggered by SkiaSharp's BlobExtensions.ToHarfBuzzBlob API when used on WASM. The root cause is an incompatibility between the managed ReleaseDelegate callback mechanism and the Uno WASM Mono runtime version. The action is needs-investigation because a complete repro repo is provided but the issue was filed against old pre-release tooling and may be fixed in current WASM runtimes.",
    "errorFingerprint": "Mono-WASM-assertion-mini.c-HarfBuzz-Blob-ReleaseDelegate",
    "nextQuestions": [
      "Does the crash reproduce on current SkiaSharp (2.88+) with current Uno WASM bootstrapper?",
      "Does passing null as the ReleaseDelegate (and manually managing memory) avoid the crash on 2.80.3?",
      "Is the Blob.FromStream dangling-pointer bug worth filing as a separate issue?"
    ],
    "resolution": {
      "hypothesis": "The crash is caused by the Mono WASM runtime (version 3.0.0-dev.87) failing to handle a native-to-managed callback (DestroyProxy) when invoked from hb_blob_create. Newer WASM runtimes may handle this correctly. A SkiaSharp-side mitigation is to avoid passing a ReleaseDelegate on WASM or to copy the data with Marshal.AllocCoTaskMem.",
      "proposals": [
        {
          "title": "Verify reproduction on current SkiaSharp + Uno WASM versions",
          "description": "Build the reporter's repro repo against current SkiaSharp (2.88+) and current Uno WASM bootstrapper to determine if the issue has been resolved upstream in the Mono/WASM runtime.",
          "category": "investigation",
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Use Marshal.AllocCoTaskMem path to avoid managed delegate callback",
          "description": "Load font data into unmanaged memory explicitly so the destroy callback is a simple Marshal.FreeCoTaskMem lambda, which avoids the managed closure that triggers the Mono WASM assertion.",
          "category": "workaround",
          "codeSnippet": "using var ms = new MemoryStream();\nstream.CopyTo(ms);\nvar fontData = ms.ToArray();\nvar ptr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontData.Length);\nSystem.Runtime.InteropServices.Marshal.Copy(fontData, 0, ptr, fontData.Length);\nvar blob = new HarfBuzzSharp.Blob(ptr, fontData.Length,\n    HarfBuzzSharp.MemoryMode.ReadOnly,\n    () => System.Runtime.InteropServices.Marshal.FreeCoTaskMem(ptr));\nblob.MakeImmutable();",
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Verify reproduction on current SkiaSharp + Uno WASM versions",
      "recommendedReason": "The crash was reported against pre-release WASM tooling from mid-2021. Verifying on current versions is the lowest-effort path and may show the issue is already resolved."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "A complete repro repo is provided and the crash site is identified. However, the issue was filed against outdated pre-release WASM tooling (SkiaSharp 2.80.3, Uno dev bootstrapper 3.0.0-dev.87) and has not been confirmed on current versions. Investigation should verify whether the crash still occurs with current SkiaSharp and Uno WASM.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels",
        "labels": [
          "type/bug",
          "area/SkiaSharp.HarfBuzz",
          "os/WASM",
          "tenet/reliability",
          "partner/unoplatform"
        ],
        "risk": "low",
        "confidence": 0.95
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the crash, explain the likely cause, and request verification on current versions",
        "comment": "Thanks for the detailed report and the repro repo!\n\nThe assertion `should not be reached at mini.c:4231` originates from the Mono WASM JIT/runtime, not from SkiaSharp/HarfBuzz directly. What happens is:\n\n1. `BlobExtensions.ToHarfBuzzBlob` creates a `HarfBuzzSharp.Blob` with a managed `ReleaseDelegate` (a lambda that calls `asset.Dispose()`).\n2. This delegate is passed to `hb_blob_create` as a native destroy callback via `DelegateProxies.DestroyProxy`.\n3. On the Uno WASM runtime version you were using (`3.0.0-dev.87` — a pre-release from mid-2021), the Mono JIT hit an unsupported code path when trying to set up the native-to-managed callback trampoline.\n\nCould you check if this still reproduces on a current version of SkiaSharp (2.88+) and the latest Uno WASM bootstrapper? The WASM Mono runtime has matured significantly since 2021.\n\n**Temporary workaround** (if you need to unblock now): Load the font data manually into unmanaged memory to avoid the managed delegate callback:\n\n```csharp\nusing var ms = new MemoryStream();\nstream.CopyTo(ms);\nvar fontData = ms.ToArray();\nvar ptr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontData.Length);\nSystem.Runtime.InteropServices.Marshal.Copy(fontData, 0, ptr, fontData.Length);\nvar blob = new HarfBuzzSharp.Blob(ptr, fontData.Length,\n    HarfBuzzSharp.MemoryMode.ReadOnly,\n    () => System.Runtime.InteropServices.Marshal.FreeCoTaskMem(ptr));\nblob.MakeImmutable();\n```\n\nThis uses `Marshal.AllocCoTaskMem` (unmanaged memory) so the destroy callback only calls `FreeCoTaskMem`, which is simpler and less likely to trigger the WASM trampoline issue.",
        "risk": "medium",
        "confidence": 0.8
      }
    ]
  }
}
```

</details>
