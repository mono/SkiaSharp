# Issue Triage Report — #2396

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T21:36:39Z |
| Type | type/question (0.92 (92%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.88 (88%)) |

**Issue Summary:** Reporter gets TypeInitializationException for SKImageInfo when referencing SkiaSharp DLLs directly (not via NuGet) in a Roslyn-compiled dynamic context, because the native libSkiaSharp library is not deployed alongside the managed assemblies.

**Analysis:** The TypeInitializationException on SKImageInfo is caused by a missing native libSkiaSharp library. The SKImageInfo static constructor calls P/Invoke methods (sk_colortype_get_default_8888, sk_color_get_bit_shift) that require the native library to be present at runtime. When SkiaSharp is referenced via NuGet, NativeAssets packages handle native library deployment automatically. When referencing managed DLLs directly, the native library must be manually placed alongside the managed assemblies.

**Recommendations:** **close-as-not-a-bug** — This is expected behavior — native library deployment is handled by NuGet NativeAssets, not by the managed DLLs. The workaround (manually copying libSkiaSharp) is well-established and community-confirmed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Reference SkiaSharp.dll, LiveChartsCore.dll, LiveChartsCore.SkiaSharpView.dll directly (not via NuGet)
2. Dynamically compile and execute code using Roslyn that instantiates SKCartesianChart
3. Call SaveImage() — triggers SKImageInfo static constructor → P/Invoke into native libSkiaSharp
4. Observe TypeInitializationException for SKImageInfo

**Environment:** Roslyn dynamic compilation; DLLs referenced directly instead of via NuGet; works fine with NuGet packages

**Code snippets:**

```csharp
SKCartesianChart ch = new SKCartesianChart();
// ... add series ...
ch.SaveImage("CartesianImageFromControl.png");
```

## Analysis

### Technical Summary

The TypeInitializationException on SKImageInfo is caused by a missing native libSkiaSharp library. The SKImageInfo static constructor calls P/Invoke methods (sk_colortype_get_default_8888, sk_color_get_bit_shift) that require the native library to be present at runtime. When SkiaSharp is referenced via NuGet, NativeAssets packages handle native library deployment automatically. When referencing managed DLLs directly, the native library must be manually placed alongside the managed assemblies.

### Rationale

The [QUESTION] prefix and phrasing confirm this is a usage/support question, not a bug. The behavior is by design: NuGet NativeAssets handle native binary deployment; bypassing NuGet removes this deployment mechanism. Source confirms SKImageInfo.ctor calls P/Invoke on load.

### Key Signals

- "The type initializer for 'SkiaSharp.SKImageInfo' threw an exception." — **issue body** (SKImageInfo static constructor calls P/Invoke into libSkiaSharp. If the native library is missing, this throws TypeInitializationException.)
- "The code works fine when using NuGet packages" — **issue body** (Confirms this is a deployment/native-asset issue. NuGet NativeAssets packages automatically deploy libSkiaSharp alongside the managed DLLs.)
- "Users working with graphics libraries using SkiaSharp 2.80.2 corrected this error by manually copying the libSkiaSharp.dll file into the folder where the software is installed." — **comment by batuhanDoymaz** (Community-confirmed workaround: manually deploy the native libSkiaSharp binary.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImageInfo.cs` | 46-56 | direct | SKImageInfo has a static constructor that calls SkiaApi.sk_colortype_get_default_8888() and SkiaApi.sk_color_get_bit_shift() — both are P/Invoke calls into native libSkiaSharp. If libSkiaSharp cannot be loaded at runtime, the static constructor throws TypeInitializationException. |

### Workarounds

- Copy the native libSkiaSharp binary (libSkiaSharp.dll on Windows, libSkiaSharp.so on Linux, libSkiaSharp.dylib on macOS) from the SkiaSharp NuGet package's runtimes folder to the application's output directory.
- Instead of referencing DLLs directly, reference the SkiaSharp NuGet package (and SkiaSharp.NativeAssets.{Platform} if needed) from the host application project to ensure native assets are deployed automatically.

### Resolution Proposals

**Hypothesis:** The native libSkiaSharp binary is not present in the runtime probing path, so the P/Invoke call in SKImageInfo's static constructor fails. NuGet NativeAssets packages solve this automatically; direct DLL references do not.

1. **Manually deploy native libSkiaSharp** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Copy the native libSkiaSharp binary from the SkiaSharp.NativeAssets.{Platform} NuGet package to the folder where your application runs. On Windows x64: extract SkiaSharp.NativeAssets.Win32 and copy runtimes/win-x64/native/libSkiaSharp.dll to your output folder.
2. **Use NuGet package references instead of direct DLL references** — alternative, confidence 0.90 (90%), cost/s, validated=yes
   - Reference SkiaSharp via NuGet in the host application project. This ensures native assets are deployed automatically. For dynamically compiled code, load the assemblies from the NuGet output directory rather than copying DLLs manually.

**Recommended proposal:** Manually deploy native libSkiaSharp

**Why:** Quickest fix for the reporter's specific scenario. The community comment already confirms this resolves the issue.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.88 (88%) |
| Reason | This is expected behavior — native library deployment is handled by NuGet NativeAssets, not by the managed DLLs. The workaround (manually copying libSkiaSharp) is well-established and community-confirmed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply question and core area labels | labels=type/question, area/SkiaSharp |
| add-comment | medium | 0.88 (88%) | Answer the question with workaround and explanation | — |
| close-issue | medium | 0.85 (85%) | Close as answered — not a bug in SkiaSharp | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
The error occurs because the native `libSkiaSharp` binary is not present in your runtime output directory. SkiaSharp's managed assembly (`SkiaSharp.dll`) uses P/Invoke to call into a platform-specific native library (`libSkiaSharp.dll` on Windows, `libSkiaSharp.so` on Linux, `libSkiaSharp.dylib` on macOS). When you install SkiaSharp via NuGet, the `SkiaSharp.NativeAssets.{Platform}` package automatically deploys this native binary alongside your application. When you reference the managed DLL directly, this deployment doesn't happen.

Here's a workaround: copy the native binary from the SkiaSharp NuGet package to your output directory. You can find it in the NuGet cache or extract it from the package:

- **Windows x64:** `SkiaSharp.NativeAssets.Win32` → `runtimes/win-x64/native/libSkiaSharp.dll`
- **Linux x64:** `SkiaSharp.NativeAssets.Linux` → `runtimes/linux-x64/native/libSkiaSharp.so`
- **macOS:** `SkiaSharp.NativeAssets.macOS` → `runtimes/osx/native/libSkiaSharp.dylib`

Place the native binary in the same directory as `SkiaSharp.dll` (or in a location on the native library search path), and the `TypeInitializationException` will be resolved. The community comment above confirms this approach works.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2396,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T21:36:39Z"
  },
  "summary": "Reporter gets TypeInitializationException for SKImageInfo when referencing SkiaSharp DLLs directly (not via NuGet) in a Roslyn-compiled dynamic context, because the native libSkiaSharp library is not deployed alongside the managed assemblies.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    }
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Reference SkiaSharp.dll, LiveChartsCore.dll, LiveChartsCore.SkiaSharpView.dll directly (not via NuGet)",
        "Dynamically compile and execute code using Roslyn that instantiates SKCartesianChart",
        "Call SaveImage() — triggers SKImageInfo static constructor → P/Invoke into native libSkiaSharp",
        "Observe TypeInitializationException for SKImageInfo"
      ],
      "codeSnippets": [
        "SKCartesianChart ch = new SKCartesianChart();\n// ... add series ...\nch.SaveImage(\"CartesianImageFromControl.png\");"
      ],
      "environmentDetails": "Roslyn dynamic compilation; DLLs referenced directly instead of via NuGet; works fine with NuGet packages",
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "The TypeInitializationException on SKImageInfo is caused by a missing native libSkiaSharp library. The SKImageInfo static constructor calls P/Invoke methods (sk_colortype_get_default_8888, sk_color_get_bit_shift) that require the native library to be present at runtime. When SkiaSharp is referenced via NuGet, NativeAssets packages handle native library deployment automatically. When referencing managed DLLs directly, the native library must be manually placed alongside the managed assemblies.",
    "rationale": "The [QUESTION] prefix and phrasing confirm this is a usage/support question, not a bug. The behavior is by design: NuGet NativeAssets handle native binary deployment; bypassing NuGet removes this deployment mechanism. Source confirms SKImageInfo.ctor calls P/Invoke on load.",
    "keySignals": [
      {
        "text": "The type initializer for 'SkiaSharp.SKImageInfo' threw an exception.",
        "source": "issue body",
        "interpretation": "SKImageInfo static constructor calls P/Invoke into libSkiaSharp. If the native library is missing, this throws TypeInitializationException."
      },
      {
        "text": "The code works fine when using NuGet packages",
        "source": "issue body",
        "interpretation": "Confirms this is a deployment/native-asset issue. NuGet NativeAssets packages automatically deploy libSkiaSharp alongside the managed DLLs."
      },
      {
        "text": "Users working with graphics libraries using SkiaSharp 2.80.2 corrected this error by manually copying the libSkiaSharp.dll file into the folder where the software is installed.",
        "source": "comment by batuhanDoymaz",
        "interpretation": "Community-confirmed workaround: manually deploy the native libSkiaSharp binary."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "46-56",
        "finding": "SKImageInfo has a static constructor that calls SkiaApi.sk_colortype_get_default_8888() and SkiaApi.sk_color_get_bit_shift() — both are P/Invoke calls into native libSkiaSharp. If libSkiaSharp cannot be loaded at runtime, the static constructor throws TypeInitializationException.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Copy the native libSkiaSharp binary (libSkiaSharp.dll on Windows, libSkiaSharp.so on Linux, libSkiaSharp.dylib on macOS) from the SkiaSharp NuGet package's runtimes folder to the application's output directory.",
      "Instead of referencing DLLs directly, reference the SkiaSharp NuGet package (and SkiaSharp.NativeAssets.{Platform} if needed) from the host application project to ensure native assets are deployed automatically."
    ],
    "resolution": {
      "hypothesis": "The native libSkiaSharp binary is not present in the runtime probing path, so the P/Invoke call in SKImageInfo's static constructor fails. NuGet NativeAssets packages solve this automatically; direct DLL references do not.",
      "proposals": [
        {
          "title": "Manually deploy native libSkiaSharp",
          "description": "Copy the native libSkiaSharp binary from the SkiaSharp.NativeAssets.{Platform} NuGet package to the folder where your application runs. On Windows x64: extract SkiaSharp.NativeAssets.Win32 and copy runtimes/win-x64/native/libSkiaSharp.dll to your output folder.",
          "category": "workaround",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use NuGet package references instead of direct DLL references",
          "description": "Reference SkiaSharp via NuGet in the host application project. This ensures native assets are deployed automatically. For dynamically compiled code, load the assemblies from the NuGet output directory rather than copying DLLs manually.",
          "category": "alternative",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Manually deploy native libSkiaSharp",
      "recommendedReason": "Quickest fix for the reporter's specific scenario. The community comment already confirms this resolves the issue."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.88,
      "reason": "This is expected behavior — native library deployment is handled by NuGet NativeAssets, not by the managed DLLs. The workaround (manually copying libSkiaSharp) is well-established and community-confirmed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and core area labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Answer the question with workaround and explanation",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "The error occurs because the native `libSkiaSharp` binary is not present in your runtime output directory. SkiaSharp's managed assembly (`SkiaSharp.dll`) uses P/Invoke to call into a platform-specific native library (`libSkiaSharp.dll` on Windows, `libSkiaSharp.so` on Linux, `libSkiaSharp.dylib` on macOS). When you install SkiaSharp via NuGet, the `SkiaSharp.NativeAssets.{Platform}` package automatically deploys this native binary alongside your application. When you reference the managed DLL directly, this deployment doesn't happen.\n\nHere's a workaround: copy the native binary from the SkiaSharp NuGet package to your output directory. You can find it in the NuGet cache or extract it from the package:\n\n- **Windows x64:** `SkiaSharp.NativeAssets.Win32` → `runtimes/win-x64/native/libSkiaSharp.dll`\n- **Linux x64:** `SkiaSharp.NativeAssets.Linux` → `runtimes/linux-x64/native/libSkiaSharp.so`\n- **macOS:** `SkiaSharp.NativeAssets.macOS` → `runtimes/osx/native/libSkiaSharp.dylib`\n\nPlace the native binary in the same directory as `SkiaSharp.dll` (or in a location on the native library search path), and the `TypeInitializationException` will be resolved. The community comment above confirms this approach works."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — not a bug in SkiaSharp",
        "risk": "medium",
        "confidence": 0.85,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
