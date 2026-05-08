# Issue Triage Report — #1001

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T09:00:15Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/libSkiaSharp.native (0.92 (92%)) |
| Suggested action | close-as-external (0.85 (85%)) |

**Issue Summary:** Azure Functions (.NET Core 3.0) cannot load the native libSkiaSharp DLL at runtime because Azure Functions deploys the runtimes/ folder alongside bin/ rather than inside it, causing the .NET runtime to fail resolving the native library.

**Analysis:** Azure Functions places the runtimes/ folder alongside bin/ during deployment (not inside bin/), so the .NET runtime cannot resolve platform-specific native assets from the expected runtimes/{rid}/native/ path. This is external to SkiaSharp but a complete workaround exists: a custom MSBuild AfterTargets step copies the required native DLL into bin/ at publish time. The maintainer already provided the full workaround and filed an upstream Azure Functions issue.

**Recommendations:** **close-as-external** — Root cause is Azure Functions' non-standard deployment layout (runtimes/ placed outside bin/), not a SkiaSharp defect. Maintainer already provided a complete working workaround. The upstream issue is tracked with the Azure Functions team.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create an Azure Functions v3 (.NET Core 3.0) app using SkiaSharp 1.68.0-preview
2. Run locally — works fine (runtimes/ folder lands inside bin/)
3. Deploy to Azure Functions — fails with DllNotFoundException (runtimes/ folder is placed at the same level as bin/, not inside it)

**Environment:** Azure Functions v3, .NET Core 3.0, SkiaSharp 1.68.0-preview, Windows (32-bit cloud runtime)

**Repository links:**
- https://github.com/mattleibow/SkiaSharpFunctions — Maintainer's sample repo demonstrating the workaround for Azure Functions
- https://github.com/Azure/Azure-Functions/issues/622 — Azure Functions upstream issue for proper runtimes/ folder placement

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | other |
| Error message | Unable to load DLL 'libSkiaSharp' or one of its dependencies: The specified module could not be found. (0x8007007E) |
| Repro quality | partial |
| Target frameworks | netcoreapp3.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Azure Functions runtimes/ folder deployment issue is an Azure Functions platform behavior, not version-specific to SkiaSharp. The workaround (custom MSBuild target) remains applicable regardless of SkiaSharp version. |

## Analysis

### Technical Summary

Azure Functions places the runtimes/ folder alongside bin/ during deployment (not inside bin/), so the .NET runtime cannot resolve platform-specific native assets from the expected runtimes/{rid}/native/ path. This is external to SkiaSharp but a complete workaround exists: a custom MSBuild AfterTargets step copies the required native DLL into bin/ at publish time. The maintainer already provided the full workaround and filed an upstream Azure Functions issue.

### Rationale

The error (0x8007007E = ERROR_MOD_NOT_FOUND on Windows) confirms the DLL file is simply not in a directory the runtime probes. The maintainer (mattleibow) confirmed in a comment that the cause is Azure Functions' non-standard deployment layout, not a bug in SkiaSharp's packaging. SkiaSharp's NativeAssets.Build.targets correctly places binaries in runtimes/{rid}/native/, which is the standard .NET convention; Azure Functions deviates from this convention. Since the root cause is external and a complete workaround exists, close-as-external is appropriate.

### Key Signals

- "locally the runtimes folder which contains libSkiaSharp.dll is within the bin and deployed on Azure it's on the same file level as bin" — **issue body** (Azure Functions' non-standard publish layout puts runtimes/ at the app root, not inside bin/, so the .NET native loader's probing paths miss it.)
- "Looks like the runtime does not load the native files like other .net core apps and websites." — **comment by mattleibow (maintainer)** (Confirms the root cause is Azure Functions platform behavior, not a SkiaSharp defect.)
- "I have created a test repo (https://github.com/mattleibow/SkiaSharpFunctions) and have something hosted for your viewing pleasure" — **comment by mattleibow (maintainer)** (Maintainer already validated the workaround end-to-end in a live deployment.)
- "I had same issue in .netcoreapp3.0 service on docker" — **comment by AlexeiScherbakov** (Same pattern appears in other non-standard hosting environments — confirms it's a deployment/hosting issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/NativeAssets.Build.targets` | 28-53 | direct | Native assets are packaged into runtimes/{rid}/native/ paths in the NuGet package. This follows the standard .NET NativeAssets convention. The issue arises when the consuming host (Azure Functions) does not place runtimes/ inside the working/bin directory where the .NET runtime probes for native libs. |
| `binding/SkiaSharp.NativeAssets.Win32` | — | related | Win32 native assets package exists for x64, x86, and arm64. At the time of the issue, Azure Functions was 32-bit on the cloud, requiring win-x86. Package structure is correct per .NET standards. |

### Workarounds

- Add a custom MSBuild target in the Functions .csproj to copy libSkiaSharp.dll from runtimes/win-x86/native/ to the bin/ publish folder after publish (AfterTargets='_FunctionsPostPublish').
- Adjust the path to win-x64 if running on a 64-bit Azure Functions plan.

### Next Questions

- Has Azure Functions fixed the runtimes/ placement issue in newer runtimes (v4, .NET 6+, .NET 8)?
- Does the MSBuild workaround still apply to newer Azure Functions SDK versions, or has a cleaner solution (e.g. app.config probing paths) become available?

### Resolution Proposals

**Hypothesis:** Azure Functions does not place the runtimes/ folder inside bin/, so the .NET runtime never probes the correct path for native binaries. Manually copying the DLL to bin/ at publish time fixes the problem.

1. **Custom MSBuild publish target (workaround)** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Add an AfterTargets='_FunctionsPostPublish' target in the Azure Functions .csproj that copies libSkiaSharp.dll from runtimes/win-x86/native/ into bin/. Adjust win-x86 to win-x64 for 64-bit plans.

```csharp
<Target Name="CopyRequiredNativeAssets" AfterTargets="_FunctionsPostPublish">
  <ItemGroup>
    <NativeAssetToCopy Include="$(PublishDir)runtimes\win-x86\native\libSkiaSharp.dll" />
  </ItemGroup>
  <Copy SourceFiles="@(NativeAssetToCopy)" DestinationFolder="$(PublishDir)bin" />
</Target>
```
2. **Upstream Azure Functions fix** — investigation, confidence 0.70 (70%), cost/xl, validated=untested
   - Vote on https://github.com/Azure/Azure-Functions/issues/622 to have Azure Functions correctly handle runtimes/ folder placement, eliminating the need for a workaround.

**Recommended proposal:** Custom MSBuild publish target (workaround)

**Why:** Immediately actionable, validated by maintainer in a live deployment, minimal effort for the reporter.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.85 (85%) |
| Reason | Root cause is Azure Functions' non-standard deployment layout (runtimes/ placed outside bin/), not a SkiaSharp defect. Maintainer already provided a complete working workaround. The upstream issue is tracked with the Azure Functions team. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/libSkiaSharp.native, os/Windows-Classic, tenet/reliability labels | labels=type/bug, area/libSkiaSharp.native, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Post workaround and explain the Azure Functions platform limitation | — |
| close-issue | medium | 0.82 (82%) | Close as external — root cause is Azure Functions deployment, workaround is documented | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report. This is caused by Azure Functions' non-standard deployment layout: it places the `runtimes/` folder *alongside* `bin/` rather than *inside* it, so the .NET runtime cannot find `libSkiaSharp.dll` in the expected probe paths.

A working workaround is to manually copy the native DLL into `bin/` as part of the publish step. Add this target to your Azure Functions `.csproj`:

```xml
<Target Name="CopyRequiredNativeAssets" AfterTargets="_FunctionsPostPublish">
  <ItemGroup>
    <NativeAssetToCopy Include="$(PublishDir)runtimes\win-x86\native\libSkiaSharp.dll" />
  </ItemGroup>
  <Copy SourceFiles="@(NativeAssetToCopy)" DestinationFolder="$(PublishDir)bin" />
</Target>
```

If you are on a 64-bit Functions plan, change `win-x86` to `win-x64`. A full working example is available at https://github.com/mattleibow/SkiaSharpFunctions.

The underlying platform issue is tracked at https://github.com/Azure/Azure-Functions/issues/622. Since the root cause is external to SkiaSharp, closing this issue. Please re-open if the workaround does not resolve it for you.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1001,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T09:00:15Z"
  },
  "summary": "Azure Functions (.NET Core 3.0) cannot load the native libSkiaSharp DLL at runtime because Azure Functions deploys the runtimes/ folder alongside bin/ rather than inside it, causing the .NET runtime to fail resolving the native library.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.92
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
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "other",
      "errorMessage": "Unable to load DLL 'libSkiaSharp' or one of its dependencies: The specified module could not be found. (0x8007007E)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "netcoreapp3.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an Azure Functions v3 (.NET Core 3.0) app using SkiaSharp 1.68.0-preview",
        "Run locally — works fine (runtimes/ folder lands inside bin/)",
        "Deploy to Azure Functions — fails with DllNotFoundException (runtimes/ folder is placed at the same level as bin/, not inside it)"
      ],
      "environmentDetails": "Azure Functions v3, .NET Core 3.0, SkiaSharp 1.68.0-preview, Windows (32-bit cloud runtime)",
      "repoLinks": [
        {
          "url": "https://github.com/mattleibow/SkiaSharpFunctions",
          "description": "Maintainer's sample repo demonstrating the workaround for Azure Functions"
        },
        {
          "url": "https://github.com/Azure/Azure-Functions/issues/622",
          "description": "Azure Functions upstream issue for proper runtimes/ folder placement"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The Azure Functions runtimes/ folder deployment issue is an Azure Functions platform behavior, not version-specific to SkiaSharp. The workaround (custom MSBuild target) remains applicable regardless of SkiaSharp version."
    }
  },
  "analysis": {
    "summary": "Azure Functions places the runtimes/ folder alongside bin/ during deployment (not inside bin/), so the .NET runtime cannot resolve platform-specific native assets from the expected runtimes/{rid}/native/ path. This is external to SkiaSharp but a complete workaround exists: a custom MSBuild AfterTargets step copies the required native DLL into bin/ at publish time. The maintainer already provided the full workaround and filed an upstream Azure Functions issue.",
    "rationale": "The error (0x8007007E = ERROR_MOD_NOT_FOUND on Windows) confirms the DLL file is simply not in a directory the runtime probes. The maintainer (mattleibow) confirmed in a comment that the cause is Azure Functions' non-standard deployment layout, not a bug in SkiaSharp's packaging. SkiaSharp's NativeAssets.Build.targets correctly places binaries in runtimes/{rid}/native/, which is the standard .NET convention; Azure Functions deviates from this convention. Since the root cause is external and a complete workaround exists, close-as-external is appropriate.",
    "keySignals": [
      {
        "text": "locally the runtimes folder which contains libSkiaSharp.dll is within the bin and deployed on Azure it's on the same file level as bin",
        "source": "issue body",
        "interpretation": "Azure Functions' non-standard publish layout puts runtimes/ at the app root, not inside bin/, so the .NET native loader's probing paths miss it."
      },
      {
        "text": "Looks like the runtime does not load the native files like other .net core apps and websites.",
        "source": "comment by mattleibow (maintainer)",
        "interpretation": "Confirms the root cause is Azure Functions platform behavior, not a SkiaSharp defect."
      },
      {
        "text": "I have created a test repo (https://github.com/mattleibow/SkiaSharpFunctions) and have something hosted for your viewing pleasure",
        "source": "comment by mattleibow (maintainer)",
        "interpretation": "Maintainer already validated the workaround end-to-end in a live deployment."
      },
      {
        "text": "I had same issue in .netcoreapp3.0 service on docker",
        "source": "comment by AlexeiScherbakov",
        "interpretation": "Same pattern appears in other non-standard hosting environments — confirms it's a deployment/hosting issue."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/NativeAssets.Build.targets",
        "lines": "28-53",
        "finding": "Native assets are packaged into runtimes/{rid}/native/ paths in the NuGet package. This follows the standard .NET NativeAssets convention. The issue arises when the consuming host (Azure Functions) does not place runtimes/ inside the working/bin directory where the .NET runtime probes for native libs.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.Win32",
        "finding": "Win32 native assets package exists for x64, x86, and arm64. At the time of the issue, Azure Functions was 32-bit on the cloud, requiring win-x86. Package structure is correct per .NET standards.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Add a custom MSBuild target in the Functions .csproj to copy libSkiaSharp.dll from runtimes/win-x86/native/ to the bin/ publish folder after publish (AfterTargets='_FunctionsPostPublish').",
      "Adjust the path to win-x64 if running on a 64-bit Azure Functions plan."
    ],
    "nextQuestions": [
      "Has Azure Functions fixed the runtimes/ placement issue in newer runtimes (v4, .NET 6+, .NET 8)?",
      "Does the MSBuild workaround still apply to newer Azure Functions SDK versions, or has a cleaner solution (e.g. app.config probing paths) become available?"
    ],
    "resolution": {
      "hypothesis": "Azure Functions does not place the runtimes/ folder inside bin/, so the .NET runtime never probes the correct path for native binaries. Manually copying the DLL to bin/ at publish time fixes the problem.",
      "proposals": [
        {
          "title": "Custom MSBuild publish target (workaround)",
          "description": "Add an AfterTargets='_FunctionsPostPublish' target in the Azure Functions .csproj that copies libSkiaSharp.dll from runtimes/win-x86/native/ into bin/. Adjust win-x86 to win-x64 for 64-bit plans.",
          "category": "workaround",
          "codeSnippet": "<Target Name=\"CopyRequiredNativeAssets\" AfterTargets=\"_FunctionsPostPublish\">\n  <ItemGroup>\n    <NativeAssetToCopy Include=\"$(PublishDir)runtimes\\win-x86\\native\\libSkiaSharp.dll\" />\n  </ItemGroup>\n  <Copy SourceFiles=\"@(NativeAssetToCopy)\" DestinationFolder=\"$(PublishDir)bin\" />\n</Target>",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Upstream Azure Functions fix",
          "description": "Vote on https://github.com/Azure/Azure-Functions/issues/622 to have Azure Functions correctly handle runtimes/ folder placement, eliminating the need for a workaround.",
          "category": "investigation",
          "confidence": 0.7,
          "effort": "cost/xl",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Custom MSBuild publish target (workaround)",
      "recommendedReason": "Immediately actionable, validated by maintainer in a live deployment, minimal effort for the reporter."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.85,
      "reason": "Root cause is Azure Functions' non-standard deployment layout (runtimes/ placed outside bin/), not a SkiaSharp defect. Maintainer already provided a complete working workaround. The upstream issue is tracked with the Azure Functions team.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/libSkiaSharp.native, os/Windows-Classic, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post workaround and explain the Azure Functions platform limitation",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the report. This is caused by Azure Functions' non-standard deployment layout: it places the `runtimes/` folder *alongside* `bin/` rather than *inside* it, so the .NET runtime cannot find `libSkiaSharp.dll` in the expected probe paths.\n\nA working workaround is to manually copy the native DLL into `bin/` as part of the publish step. Add this target to your Azure Functions `.csproj`:\n\n```xml\n<Target Name=\"CopyRequiredNativeAssets\" AfterTargets=\"_FunctionsPostPublish\">\n  <ItemGroup>\n    <NativeAssetToCopy Include=\"$(PublishDir)runtimes\\win-x86\\native\\libSkiaSharp.dll\" />\n  </ItemGroup>\n  <Copy SourceFiles=\"@(NativeAssetToCopy)\" DestinationFolder=\"$(PublishDir)bin\" />\n</Target>\n```\n\nIf you are on a 64-bit Functions plan, change `win-x86` to `win-x64`. A full working example is available at https://github.com/mattleibow/SkiaSharpFunctions.\n\nThe underlying platform issue is tracked at https://github.com/Azure/Azure-Functions/issues/622. Since the root cause is external to SkiaSharp, closing this issue. Please re-open if the workaround does not resolve it for you."
      },
      {
        "type": "close-issue",
        "description": "Close as external — root cause is Azure Functions deployment, workaround is documented",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
