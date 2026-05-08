# Issue Triage Report — #2438

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T12:52:17Z |
| Type | type/bug (0.85 (85%)) |
| Area | area/libSkiaSharp.native (0.93 (93%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** DllNotFoundException for libSkiaSharp when running an Azure Function inside a Docker Linux container, even though SkiaSharp.NativeAssets.Linux.NoDependencies is installed — the native .so is not found because the publish command lacks an explicit RID.

**Analysis:** The libSkiaSharp.so native binary is not being copied to the publish output because the Dockerfile's dotnet publish command does not specify a Runtime Identifier (RID). Without `-r linux-x64`, the .NET SDK may not place the RID-specific native asset under runtimes/linux-x64/native/ in the publish directory. The NativeAssets.Linux.NoDependencies package ships the .so correctly for linux-x64/linux-arm64/musl variants, but the binary must land in the output for the Azure Functions runtime to find it.

**Recommendations:** **needs-investigation** — Complete repro with Dockerfile provided. Root cause is likely the missing RID in dotnet publish, but needs confirmation. Well-known Linux native loading issue pattern documented in packages.md.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a default Azure Function project in Visual Studio (enable Docker container)
2. Install SkiaSharp.NativeAssets.Linux.NoDependencies NuGet package
3. Add basic SkiaSharp drawing code
4. Build and run using Docker with mcr.microsoft.com/azure-functions/dotnet:4 base image

**Environment:** Azure Functions v4, .NET 6, Docker Linux container (mcr.microsoft.com/azure-functions/dotnet:4), SkiaSharp.NativeAssets.Linux.NoDependencies installed

**Related issues:** #1341

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | DllNotFoundException: Unable to load shared library 'libSkiaSharp' or one of its dependencies. liblibSkiaSharp: cannot open shared object file: No such file or directory |
| Repro quality | complete |
| Target frameworks | net6.0 |

## Analysis

### Technical Summary

The libSkiaSharp.so native binary is not being copied to the publish output because the Dockerfile's dotnet publish command does not specify a Runtime Identifier (RID). Without `-r linux-x64`, the .NET SDK may not place the RID-specific native asset under runtimes/linux-x64/native/ in the publish directory. The NativeAssets.Linux.NoDependencies package ships the .so correctly for linux-x64/linux-arm64/musl variants, but the binary must land in the output for the Azure Functions runtime to find it.

### Rationale

The error 'liblibSkiaSharp: cannot open shared object file' matches the 'file not found' pattern in packages.md — the .so was never placed in the publish output. The reporter installed the correct package (NoDependencies), and the Dockerfile is otherwise standard. The missing RID in the publish step is the most likely root cause. Issue #1341 is a highly-related prior report for the same error in Linux Azure App Service containers.

### Key Signals

- "liblibSkiaSharp: cannot open shared object file: No such file or directory" — **issue body** (The .so file is entirely absent from the runtime's search path — a 'not found' failure, not a dependency failure. The double 'lib' prefix is the .NET runtime fallback path probe.)
- "Install SkiaSharp.NativeAssets.Linux.NoDependencies nuget" — **issue body (Steps to reproduce)** (Reporter installed the correct package for minimal containers, so the package selection is not the issue.)
- "RUN dotnet publish "FunctionApp1.csproj" -c Release -o /app/publish /p:UseAppHost=false" — **issue body (Dockerfile)** (No -r (RID) flag is passed to dotnet publish. Without an explicit RID, the SDK may not copy the linux-x64 native asset to the publish output directory.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.Linux.NoDependencies/SkiaSharp.NativeAssets.Linux.NoDependencies.csproj` | 20-35 | direct | The NoDependencies package ships libSkiaSharp.so under runtimes/linux-x64/native/, linux-arm64, linux-musl-x64, etc. The package correctly ships for all common Linux RIDs. The binary should be present if the package is referenced in the executable project and the RID resolves correctly at publish time. |
| `binding/NativeAssets.Build.targets` | 39-48 | related | The NativeAssets targets use RuntimeIdentifier metadata to place binaries under runtimes/{rid}/native/ in the package. This is the standard NuGet native assets convention — the .NET runtime resolves these at load time when the output contains the correct runtimes/ subfolder structure. |
| `documentation/dev/packages.md` | 183-215 | direct | packages.md explicitly documents this exact scenario: 'libSkiaSharp.so: cannot open shared object file: No such file or directory' means the .so was not found at any search path. Fix: add NativeAssets.Linux.NoDependencies as a direct PackageReference in the executable project, and ensure publish includes the correct RID. |

### Workarounds

- Add `-r linux-x64 --self-contained false` to the `dotnet publish` command in the Dockerfile to force inclusion of the linux-x64 native asset.
- Add `<RuntimeIdentifier>linux-x64</RuntimeIdentifier>` to the .csproj file so the RID is always set for publish.
- Verify that SkiaSharp.NativeAssets.Linux.NoDependencies is referenced in the application project (FunctionApp1.csproj), not only in a class library — native assets must be in the executable project.

### Next Questions

- Is the NativeAssets package referenced in FunctionApp1.csproj (the executable) or only in a library project?
- Does adding -r linux-x64 to the dotnet publish command resolve the issue?

### Resolution Proposals

**Hypothesis:** The dotnet publish step lacks an explicit RID (-r linux-x64), so the linux-x64 native binary is not placed in the publish output. The Azure Functions runtime cannot find libSkiaSharp.so at load time.

1. **Add explicit RID to dotnet publish in Dockerfile** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - Change the publish step to include the linux-x64 RID so the native binary is copied to the publish output.

```csharp
RUN dotnet publish "FunctionApp1.csproj" -c Release -o /app/publish /p:UseAppHost=false -r linux-x64 --self-contained false
```
2. **Add RuntimeIdentifier to the project file** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - Set <RuntimeIdentifier>linux-x64</RuntimeIdentifier> in FunctionApp1.csproj so every publish automatically targets linux-x64.

```csharp
<PropertyGroup>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
</PropertyGroup>
```

**Recommended proposal:** Add explicit RID to dotnet publish in Dockerfile

**Why:** Minimal change to the Dockerfile; no project file modification needed. Consistent with how Azure Functions Docker deployments are typically set up.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Complete repro with Dockerfile provided. Root cause is likely the missing RID in dotnet publish, but needs confirmation. Well-known Linux native loading issue pattern documented in packages.md. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.93 (93%) | Apply bug, native, linux labels | labels=type/bug, area/libSkiaSharp.native, os/Linux, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Post analysis with likely fix and workaround | — |
| link-related | low | 0.90 (90%) | Cross-reference related Linux native loading issue | linkedIssue=#1341 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed Dockerfile! This is a known Linux native asset deployment issue.

The error `liblibSkiaSharp: cannot open shared object file` means the `libSkiaSharp.so` binary was **not found** in the publish output at all — not that it has a missing dependency.

You have the right package (`SkiaSharp.NativeAssets.Linux.NoDependencies`), but the `dotnet publish` step in your Dockerfile doesn't specify a Runtime Identifier (RID). Without `-r linux-x64`, the .NET SDK may not copy the `runtimes/linux-x64/native/libSkiaSharp.so` file to the publish directory.

**Fix 1 — Add RID to the Dockerfile publish step:**
```dockerfile
RUN dotnet publish "FunctionApp1.csproj" -c Release -o /app/publish /p:UseAppHost=false -r linux-x64 --self-contained false
```

**Fix 2 — Add `RuntimeIdentifier` to your `.csproj`:**
```xml
<PropertyGroup>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
</PropertyGroup>
```

Also make sure `SkiaSharp.NativeAssets.Linux.NoDependencies` is referenced in the **executable project** (your Functions app), not only in a class library — native assets must be in the app that produces the output.

See also: #1341 for a thorough discussion of this issue on Linux Azure environments.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2438,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T12:52:17Z"
  },
  "summary": "DllNotFoundException for libSkiaSharp when running an Azure Function inside a Docker Linux container, even though SkiaSharp.NativeAssets.Linux.NoDependencies is installed — the native .so is not found because the publish command lacks an explicit RID.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.85
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.93
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "DllNotFoundException: Unable to load shared library 'libSkiaSharp' or one of its dependencies. liblibSkiaSharp: cannot open shared object file: No such file or directory",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a default Azure Function project in Visual Studio (enable Docker container)",
        "Install SkiaSharp.NativeAssets.Linux.NoDependencies NuGet package",
        "Add basic SkiaSharp drawing code",
        "Build and run using Docker with mcr.microsoft.com/azure-functions/dotnet:4 base image"
      ],
      "environmentDetails": "Azure Functions v4, .NET 6, Docker Linux container (mcr.microsoft.com/azure-functions/dotnet:4), SkiaSharp.NativeAssets.Linux.NoDependencies installed",
      "relatedIssues": [
        1341
      ],
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "The libSkiaSharp.so native binary is not being copied to the publish output because the Dockerfile's dotnet publish command does not specify a Runtime Identifier (RID). Without `-r linux-x64`, the .NET SDK may not place the RID-specific native asset under runtimes/linux-x64/native/ in the publish directory. The NativeAssets.Linux.NoDependencies package ships the .so correctly for linux-x64/linux-arm64/musl variants, but the binary must land in the output for the Azure Functions runtime to find it.",
    "rationale": "The error 'liblibSkiaSharp: cannot open shared object file' matches the 'file not found' pattern in packages.md — the .so was never placed in the publish output. The reporter installed the correct package (NoDependencies), and the Dockerfile is otherwise standard. The missing RID in the publish step is the most likely root cause. Issue #1341 is a highly-related prior report for the same error in Linux Azure App Service containers.",
    "keySignals": [
      {
        "text": "liblibSkiaSharp: cannot open shared object file: No such file or directory",
        "source": "issue body",
        "interpretation": "The .so file is entirely absent from the runtime's search path — a 'not found' failure, not a dependency failure. The double 'lib' prefix is the .NET runtime fallback path probe."
      },
      {
        "text": "Install SkiaSharp.NativeAssets.Linux.NoDependencies nuget",
        "source": "issue body (Steps to reproduce)",
        "interpretation": "Reporter installed the correct package for minimal containers, so the package selection is not the issue."
      },
      {
        "text": "RUN dotnet publish \"FunctionApp1.csproj\" -c Release -o /app/publish /p:UseAppHost=false",
        "source": "issue body (Dockerfile)",
        "interpretation": "No -r (RID) flag is passed to dotnet publish. Without an explicit RID, the SDK may not copy the linux-x64 native asset to the publish output directory."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.Linux.NoDependencies/SkiaSharp.NativeAssets.Linux.NoDependencies.csproj",
        "lines": "20-35",
        "finding": "The NoDependencies package ships libSkiaSharp.so under runtimes/linux-x64/native/, linux-arm64, linux-musl-x64, etc. The package correctly ships for all common Linux RIDs. The binary should be present if the package is referenced in the executable project and the RID resolves correctly at publish time.",
        "relevance": "direct"
      },
      {
        "file": "binding/NativeAssets.Build.targets",
        "lines": "39-48",
        "finding": "The NativeAssets targets use RuntimeIdentifier metadata to place binaries under runtimes/{rid}/native/ in the package. This is the standard NuGet native assets convention — the .NET runtime resolves these at load time when the output contains the correct runtimes/ subfolder structure.",
        "relevance": "related"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "183-215",
        "finding": "packages.md explicitly documents this exact scenario: 'libSkiaSharp.so: cannot open shared object file: No such file or directory' means the .so was not found at any search path. Fix: add NativeAssets.Linux.NoDependencies as a direct PackageReference in the executable project, and ensure publish includes the correct RID.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Add `-r linux-x64 --self-contained false` to the `dotnet publish` command in the Dockerfile to force inclusion of the linux-x64 native asset.",
      "Add `<RuntimeIdentifier>linux-x64</RuntimeIdentifier>` to the .csproj file so the RID is always set for publish.",
      "Verify that SkiaSharp.NativeAssets.Linux.NoDependencies is referenced in the application project (FunctionApp1.csproj), not only in a class library — native assets must be in the executable project."
    ],
    "nextQuestions": [
      "Is the NativeAssets package referenced in FunctionApp1.csproj (the executable) or only in a library project?",
      "Does adding -r linux-x64 to the dotnet publish command resolve the issue?"
    ],
    "resolution": {
      "hypothesis": "The dotnet publish step lacks an explicit RID (-r linux-x64), so the linux-x64 native binary is not placed in the publish output. The Azure Functions runtime cannot find libSkiaSharp.so at load time.",
      "proposals": [
        {
          "title": "Add explicit RID to dotnet publish in Dockerfile",
          "description": "Change the publish step to include the linux-x64 RID so the native binary is copied to the publish output.",
          "category": "workaround",
          "codeSnippet": "RUN dotnet publish \"FunctionApp1.csproj\" -c Release -o /app/publish /p:UseAppHost=false -r linux-x64 --self-contained false",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Add RuntimeIdentifier to the project file",
          "description": "Set <RuntimeIdentifier>linux-x64</RuntimeIdentifier> in FunctionApp1.csproj so every publish automatically targets linux-x64.",
          "category": "workaround",
          "codeSnippet": "<PropertyGroup>\n  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>\n</PropertyGroup>",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Add explicit RID to dotnet publish in Dockerfile",
      "recommendedReason": "Minimal change to the Dockerfile; no project file modification needed. Consistent with how Azure Functions Docker deployments are typically set up."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Complete repro with Dockerfile provided. Root cause is likely the missing RID in dotnet publish, but needs confirmation. Well-known Linux native loading issue pattern documented in packages.md.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, native, linux labels",
        "risk": "low",
        "confidence": 0.93,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis with likely fix and workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed Dockerfile! This is a known Linux native asset deployment issue.\n\nThe error `liblibSkiaSharp: cannot open shared object file` means the `libSkiaSharp.so` binary was **not found** in the publish output at all — not that it has a missing dependency.\n\nYou have the right package (`SkiaSharp.NativeAssets.Linux.NoDependencies`), but the `dotnet publish` step in your Dockerfile doesn't specify a Runtime Identifier (RID). Without `-r linux-x64`, the .NET SDK may not copy the `runtimes/linux-x64/native/libSkiaSharp.so` file to the publish directory.\n\n**Fix 1 — Add RID to the Dockerfile publish step:**\n```dockerfile\nRUN dotnet publish \"FunctionApp1.csproj\" -c Release -o /app/publish /p:UseAppHost=false -r linux-x64 --self-contained false\n```\n\n**Fix 2 — Add `RuntimeIdentifier` to your `.csproj`:**\n```xml\n<PropertyGroup>\n  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>\n</PropertyGroup>\n```\n\nAlso make sure `SkiaSharp.NativeAssets.Linux.NoDependencies` is referenced in the **executable project** (your Functions app), not only in a class library — native assets must be in the app that produces the output.\n\nSee also: #1341 for a thorough discussion of this issue on Linux Azure environments."
      },
      {
        "type": "link-related",
        "description": "Cross-reference related Linux native loading issue",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 1341
      }
    ]
  }
}
```

</details>
