# Issue Triage Report — #4259

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-06-27T05:29:07Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/libSkiaSharp.native (0.92 (92%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** SkiaSharp 4.148.0 includes native PDB files (225 MB total) in the Windows MAUI unpackaged build output because NuGet packages starting from 3.119.2 ship PDB files under runtimes/win-xxx/native/, which get deployed as native assets in unpackaged mode.

**Analysis:** SkiaSharp's Win32 NativeAssets package (from 3.119.2+) ships native PDB debug symbols inside the main .nupkg under runtimes/win-xxx/native/. When building a MAUI Windows app in unpackaged mode, MSBuild resolves all files under runtimes/win-xxx/native/ as native copy-local items and deploys them to the output directory — including the large PDB files. Packaged (MSIX) mode and other platforms are not affected because their deployment pipelines filter or process native assets differently.

**Recommendations:** **needs-investigation** — Confirmed regression with clear root cause hypothesis (PDB files in main nupkg runtimes/ from 3.119.2+). Needs investigation to confirm the packaging mechanism and implement a proper fix in the build targets. Workaround available for the reporter.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Windows-WinUI |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | partner/maui |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a .NET MAUI app using SkiaSharp 4.148.0 targeting Windows in unpackaged mode
2. Build in Release configuration or run dotnet publish with -f net10.0-windows10.0.19041.0 -c Release
3. Inspect the output folder: four large .pdb files (~225 MB total) appear

**Environment:** SkiaSharp 4.148.0, .NET MAUI 10.0.71, Windows 11, Visual Studio, net10.0-windows10.0.19041.0 (unpackaged)

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3519 — Related issue #3519: Sudden disk-usage increase in 3.119.2 — confirms PDB files were added to main NuGet packages from 3.119.2 onwards, with workarounds
- https://github.com/mono/SkiaSharp/issues/3286 — Related issue #3286: Debug symbols missing from symbol servers for 3.119.0

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | Four large PDB files (225 MB total) appear in the Windows unpackaged build/publish output folder in Release configuration. |
| Repro quality | partial |
| Target frameworks | net10.0-windows10.0.19041.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 4.148.0, 3.119.1 |
| Worked in | 3.119.1 |
| Broke in | 3.119.2 |
| Current relevance | likely |
| Relevance reason | Related issue #3519 confirms PDB files were introduced into main NuGet packages starting in 3.119.2. The SkiaSharp 4.148.0 packages continue this behavior. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | Reporter confirms this worked with SkiaSharp 3.119.1 and broke with 4.148.0. Issue #3519 confirms PDB files were added to NuGet packages starting from 3.119.2. |
| Worked in version | 3.119.1 |
| Broke in version | 3.119.2 |

## Analysis

### Technical Summary

SkiaSharp's Win32 NativeAssets package (from 3.119.2+) ships native PDB debug symbols inside the main .nupkg under runtimes/win-xxx/native/. When building a MAUI Windows app in unpackaged mode, MSBuild resolves all files under runtimes/win-xxx/native/ as native copy-local items and deploys them to the output directory — including the large PDB files. Packaged (MSIX) mode and other platforms are not affected because their deployment pipelines filter or process native assets differently.

### Rationale

The behavior is clearly a regression: PDB files in the main NuGet package were introduced in 3.119.2 (confirmed by related issue #3519). The NativeAssets.Build.targets attempts to separate PDB files into the symbols package (_TargetPathsToSymbols), but the PDB files are present in the main package's runtimes/ folder, causing MSBuild's native asset resolution to copy them to the output. The unpackaged-specific behavior is because MSIX packaging filters content while unpackaged builds copy all native assets verbatim.

### Key Signals

- "four large pdb files are added (totaling 225 MB)" — **issue body** (Four native PDB files — likely libSkiaSharp.pdb for x64/x86/arm64 and libHarfBuzzSharp.pdb for one arch — are being deployed as native assets.)
- "On other platforms or in packaged mode the large pdb files do not show up" — **issue body** (Packaged (MSIX) mode strips these files; unpackaged mode copies everything from runtimes/ to the output.)
- "It seems that the new version includes pdb files in package" — **issue #3519 comment** (Confirmed: PDB files entered the main NuGet package from 3.119.2, not just the .snupkg symbols package.)
- "SkiaSharp.NativeAssets.Win32 >= 3.119.2 ships libSkiaSharp.pdb" — **issue #3519 comment** (Direct confirmation that libSkiaSharp.pdb is present in the runtimes/ folder of the main package.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj` | 9-11 | direct | Uses glob `libSkiaSharp*` when declaring NativeAssetPackageFile items, which matches both libSkiaSharp.dll and libSkiaSharp.pdb if the pdb exists in the output/native/windows directories. |
| `binding/NativeAssets.Build.targets` | 50-52 | direct | _IncludeRuntimesPackageFiles target attempts to split files: non-PDB goes to _BuildOutputInPackage (main package) and PDB goes to _TargetPathsToSymbols (symbols package). However, related issue #3519 confirms PDB files end up in the main nupkg runtimes/ path regardless. |
| `binding/NativeAssets.Build.targets` | 28-32 | related | IsNativeAssetsProject=true sets IncludeSymbols=true and BuildOutputTargetFolder=runtimes. This controls how native assets are packaged. |
| `binding/SkiaSharp.NativeAssets.WinUI/SkiaSharp.NativeAssets.WinUI.csproj` | 9-17 | related | WinUI package also uses wildcard `*` for NativeAssetPackageFile items, meaning any PDB files in winui/* directories would also be captured. |

### Workarounds

- Add an MSBuild target to remove PDB files after build/publish:

<Target Name="RemoveUnnecessaryPdbFilesAfterBuild" AfterTargets="Build">
  <ItemGroup>
    <PdbFilesToRemoveBuild Include="$(OutputPath)runtimes\**\*.pdb" />
  </ItemGroup>
  <Delete Files="@(PdbFilesToRemoveBuild)" />
</Target>
<Target Name="RemoveUnnecessaryPdbFilesAfterPublish" AfterTargets="Publish">
  <ItemGroup>
    <PdbFilesToRemovePublish Include="$(PublishDir)runtimes\**\*.pdb" />
  </ItemGroup>
  <Delete Files="@(PdbFilesToRemovePublish)" />
</Target>
- For .NET 7+ (Core), prevent PDB files from entering deps.json by removing them from native copy-local items:

<Target Name="RemovePackageNativePdbsFromDeps" AfterTargets="ResolvePackageAssets">
  <ItemGroup>
    <RuntimeTargetsCopyLocalItems Remove="@(RuntimeTargetsCopyLocalItems)" Condition="'%(Extension)' == '.pdb'" />
    <NativeCopyLocalItems Remove="@(NativeCopyLocalItems)" Condition="'%(Extension)' == '.pdb'" />
  </ItemGroup>
</Target>

### Next Questions

- Do the PDB files appear in the main .nupkg runtimes/ folder or only in the .snupkg? (Check NuGet package contents with NuGet Package Explorer)
- Does the _IncludeRuntimesPackageFiles target in NativeAssets.Build.targets fail to run for some TFMs, allowing PDB files into the main package?
- Should SkiaSharp ship native PDB files at all in the main package, or should they be symbol-server-only?

### Resolution Proposals

**Hypothesis:** The NativeAssets.Build.targets _IncludeRuntimesPackageFiles target is supposed to route PDB files to _TargetPathsToSymbols (symbols package), but PDB files are ending up in the main nupkg's runtimes/ folder. This causes MSBuild to treat them as normal native copy-local items that get deployed to the output directory.

1. **MSBuild workaround in app project** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - Add targets to strip PDB files from native copy-local items after ResolvePackageAssets (for .NET 7+) or delete them post-build/publish.

```csharp
<Target Name="RemovePackageNativePdbsFromDeps" AfterTargets="ResolvePackageAssets">
  <ItemGroup>
    <RuntimeTargetsCopyLocalItems Remove="@(RuntimeTargetsCopyLocalItems)" Condition="'%(Extension)' == '.pdb'" />
    <NativeCopyLocalItems Remove="@(NativeCopyLocalItems)" Condition="'%(Extension)' == '.pdb'" />
  </ItemGroup>
</Target>
```
2. **Fix packaging to exclude PDB from main nupkg** — fix, confidence 0.75 (75%), cost/s, validated=untested
   - Investigate why _IncludeRuntimesPackageFiles in NativeAssets.Build.targets fails to keep PDB files out of the main package runtimes/ folder and fix the build targets.
3. **Add exclusion filter to buildTransitive targets** — fix, confidence 0.80 (80%), cost/xs, validated=untested
   - Update the buildTransitive targets (SkiaSharp.targets for Win32 and WinUI) to explicitly exclude *.pdb files when declaring native assets for copy to output, preventing them from being deployed regardless of package structure.

**Recommended proposal:** MSBuild workaround in app project

**Why:** Immediately actionable for the reporter while the underlying packaging issue is investigated. The .NET 7+ approach via ResolvePackageAssets is cleaner than a post-build delete as it prevents PDB files from entering deps.json as well.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Confirmed regression with clear root cause hypothesis (PDB files in main nupkg runtimes/ from 3.119.2+). Needs investigation to confirm the packaging mechanism and implement a proper fix in the build targets. Workaround available for the reporter. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, native area, Windows WinUI platform, reliability tenet, and MAUI partner labels | labels=type/bug, area/libSkiaSharp.native, os/Windows-WinUI, tenet/reliability, partner/maui |
| link-related | low | 0.95 (95%) | Link to related issue #3519 which is the same root cause (PDB files in main NuGet package) with a more general scope | linkedIssue=#3519 |
| add-comment | medium | 0.85 (85%) | Post analysis with confirmed root cause and workaround from issue #3519 | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this! This is related to #3519 — starting from SkiaSharp 3.119.2, the Win32 (and HarfBuzzSharp) NativeAssets packages began shipping native PDB debug symbols inside the main `.nupkg` under `runtimes/win-xxx/native/`. When building a MAUI Windows app in **unpackaged** mode, MSBuild treats all files under `runtimes/*/native/` as native copy-local items and deploys them to the output directory — including the large PDB files. Packaged (MSIX) mode isn't affected because the MSIX pipeline processes native assets differently.

**Workaround (add to your `.csproj` or `Directory.Build.targets`):**

For .NET 7+, this prevents PDB files from entering the copy-local list and `deps.json` entirely:
```xml
<Target Name="RemovePackageNativePdbsFromDeps" AfterTargets="ResolvePackageAssets">
  <ItemGroup>
    <RuntimeTargetsCopyLocalItems Remove="@(RuntimeTargetsCopyLocalItems)" Condition="'%(Extension)' == '.pdb'" />
    <NativeCopyLocalItems Remove="@(NativeCopyLocalItems)" Condition="'%(Extension)' == '.pdb'" />
  </ItemGroup>
</Target>
```

A proper fix in SkiaSharp's build targets to prevent PDB files from being deployed as native assets is being tracked.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4259,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-06-27T05:29:07Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SkiaSharp 4.148.0 includes native PDB files (225 MB total) in the Windows MAUI unpackaged build output because NuGet packages starting from 3.119.2 ship PDB files under runtimes/win-xxx/native/, which get deployed as native assets in unpackaged mode.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.92
    },
    "platforms": [
      "os/Windows-WinUI"
    ],
    "tenets": [
      "tenet/reliability"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "Four large PDB files (225 MB total) appear in the Windows unpackaged build/publish output folder in Release configuration.",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net10.0-windows10.0.19041.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET MAUI app using SkiaSharp 4.148.0 targeting Windows in unpackaged mode",
        "Build in Release configuration or run dotnet publish with -f net10.0-windows10.0.19041.0 -c Release",
        "Inspect the output folder: four large .pdb files (~225 MB total) appear"
      ],
      "environmentDetails": "SkiaSharp 4.148.0, .NET MAUI 10.0.71, Windows 11, Visual Studio, net10.0-windows10.0.19041.0 (unpackaged)",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3519",
          "description": "Related issue #3519: Sudden disk-usage increase in 3.119.2 — confirms PDB files were added to main NuGet packages from 3.119.2 onwards, with workarounds"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3286",
          "description": "Related issue #3286: Debug symbols missing from symbol servers for 3.119.0"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "4.148.0",
        "3.119.1"
      ],
      "workedIn": "3.119.1",
      "brokeIn": "3.119.2",
      "currentRelevance": "likely",
      "relevanceReason": "Related issue #3519 confirms PDB files were introduced into main NuGet packages starting in 3.119.2. The SkiaSharp 4.148.0 packages continue this behavior."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "Reporter confirms this worked with SkiaSharp 3.119.1 and broke with 4.148.0. Issue #3519 confirms PDB files were added to NuGet packages starting from 3.119.2.",
      "workedInVersion": "3.119.1",
      "brokeInVersion": "3.119.2"
    }
  },
  "analysis": {
    "summary": "SkiaSharp's Win32 NativeAssets package (from 3.119.2+) ships native PDB debug symbols inside the main .nupkg under runtimes/win-xxx/native/. When building a MAUI Windows app in unpackaged mode, MSBuild resolves all files under runtimes/win-xxx/native/ as native copy-local items and deploys them to the output directory — including the large PDB files. Packaged (MSIX) mode and other platforms are not affected because their deployment pipelines filter or process native assets differently.",
    "rationale": "The behavior is clearly a regression: PDB files in the main NuGet package were introduced in 3.119.2 (confirmed by related issue #3519). The NativeAssets.Build.targets attempts to separate PDB files into the symbols package (_TargetPathsToSymbols), but the PDB files are present in the main package's runtimes/ folder, causing MSBuild's native asset resolution to copy them to the output. The unpackaged-specific behavior is because MSIX packaging filters content while unpackaged builds copy all native assets verbatim.",
    "keySignals": [
      {
        "text": "four large pdb files are added (totaling 225 MB)",
        "source": "issue body",
        "interpretation": "Four native PDB files — likely libSkiaSharp.pdb for x64/x86/arm64 and libHarfBuzzSharp.pdb for one arch — are being deployed as native assets."
      },
      {
        "text": "On other platforms or in packaged mode the large pdb files do not show up",
        "source": "issue body",
        "interpretation": "Packaged (MSIX) mode strips these files; unpackaged mode copies everything from runtimes/ to the output."
      },
      {
        "text": "It seems that the new version includes pdb files in package",
        "source": "issue #3519 comment",
        "interpretation": "Confirmed: PDB files entered the main NuGet package from 3.119.2, not just the .snupkg symbols package."
      },
      {
        "text": "SkiaSharp.NativeAssets.Win32 >= 3.119.2 ships libSkiaSharp.pdb",
        "source": "issue #3519 comment",
        "interpretation": "Direct confirmation that libSkiaSharp.pdb is present in the runtimes/ folder of the main package."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj",
        "lines": "9-11",
        "finding": "Uses glob `libSkiaSharp*` when declaring NativeAssetPackageFile items, which matches both libSkiaSharp.dll and libSkiaSharp.pdb if the pdb exists in the output/native/windows directories.",
        "relevance": "direct"
      },
      {
        "file": "binding/NativeAssets.Build.targets",
        "lines": "50-52",
        "finding": "_IncludeRuntimesPackageFiles target attempts to split files: non-PDB goes to _BuildOutputInPackage (main package) and PDB goes to _TargetPathsToSymbols (symbols package). However, related issue #3519 confirms PDB files end up in the main nupkg runtimes/ path regardless.",
        "relevance": "direct"
      },
      {
        "file": "binding/NativeAssets.Build.targets",
        "lines": "28-32",
        "finding": "IsNativeAssetsProject=true sets IncludeSymbols=true and BuildOutputTargetFolder=runtimes. This controls how native assets are packaged.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.WinUI/SkiaSharp.NativeAssets.WinUI.csproj",
        "lines": "9-17",
        "finding": "WinUI package also uses wildcard `*` for NativeAssetPackageFile items, meaning any PDB files in winui/* directories would also be captured.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Add an MSBuild target to remove PDB files after build/publish:\n\n<Target Name=\"RemoveUnnecessaryPdbFilesAfterBuild\" AfterTargets=\"Build\">\n  <ItemGroup>\n    <PdbFilesToRemoveBuild Include=\"$(OutputPath)runtimes\\**\\*.pdb\" />\n  </ItemGroup>\n  <Delete Files=\"@(PdbFilesToRemoveBuild)\" />\n</Target>\n<Target Name=\"RemoveUnnecessaryPdbFilesAfterPublish\" AfterTargets=\"Publish\">\n  <ItemGroup>\n    <PdbFilesToRemovePublish Include=\"$(PublishDir)runtimes\\**\\*.pdb\" />\n  </ItemGroup>\n  <Delete Files=\"@(PdbFilesToRemovePublish)\" />\n</Target>",
      "For .NET 7+ (Core), prevent PDB files from entering deps.json by removing them from native copy-local items:\n\n<Target Name=\"RemovePackageNativePdbsFromDeps\" AfterTargets=\"ResolvePackageAssets\">\n  <ItemGroup>\n    <RuntimeTargetsCopyLocalItems Remove=\"@(RuntimeTargetsCopyLocalItems)\" Condition=\"'%(Extension)' == '.pdb'\" />\n    <NativeCopyLocalItems Remove=\"@(NativeCopyLocalItems)\" Condition=\"'%(Extension)' == '.pdb'\" />\n  </ItemGroup>\n</Target>"
    ],
    "nextQuestions": [
      "Do the PDB files appear in the main .nupkg runtimes/ folder or only in the .snupkg? (Check NuGet package contents with NuGet Package Explorer)",
      "Does the _IncludeRuntimesPackageFiles target in NativeAssets.Build.targets fail to run for some TFMs, allowing PDB files into the main package?",
      "Should SkiaSharp ship native PDB files at all in the main package, or should they be symbol-server-only?"
    ],
    "resolution": {
      "hypothesis": "The NativeAssets.Build.targets _IncludeRuntimesPackageFiles target is supposed to route PDB files to _TargetPathsToSymbols (symbols package), but PDB files are ending up in the main nupkg's runtimes/ folder. This causes MSBuild to treat them as normal native copy-local items that get deployed to the output directory.",
      "proposals": [
        {
          "title": "MSBuild workaround in app project",
          "description": "Add targets to strip PDB files from native copy-local items after ResolvePackageAssets (for .NET 7+) or delete them post-build/publish.",
          "category": "workaround",
          "codeSnippet": "<Target Name=\"RemovePackageNativePdbsFromDeps\" AfterTargets=\"ResolvePackageAssets\">\n  <ItemGroup>\n    <RuntimeTargetsCopyLocalItems Remove=\"@(RuntimeTargetsCopyLocalItems)\" Condition=\"'%(Extension)' == '.pdb'\" />\n    <NativeCopyLocalItems Remove=\"@(NativeCopyLocalItems)\" Condition=\"'%(Extension)' == '.pdb'\" />\n  </ItemGroup>\n</Target>",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Fix packaging to exclude PDB from main nupkg",
          "description": "Investigate why _IncludeRuntimesPackageFiles in NativeAssets.Build.targets fails to keep PDB files out of the main package runtimes/ folder and fix the build targets.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Add exclusion filter to buildTransitive targets",
          "description": "Update the buildTransitive targets (SkiaSharp.targets for Win32 and WinUI) to explicitly exclude *.pdb files when declaring native assets for copy to output, preventing them from being deployed regardless of package structure.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "MSBuild workaround in app project",
      "recommendedReason": "Immediately actionable for the reporter while the underlying packaging issue is investigated. The .NET 7+ approach via ResolvePackageAssets is cleaner than a post-build delete as it prevents PDB files from entering deps.json as well."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Confirmed regression with clear root cause hypothesis (PDB files in main nupkg runtimes/ from 3.119.2+). Needs investigation to confirm the packaging mechanism and implement a proper fix in the build targets. Workaround available for the reporter.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, native area, Windows WinUI platform, reliability tenet, and MAUI partner labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Windows-WinUI",
          "tenet/reliability",
          "partner/maui"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to related issue #3519 which is the same root cause (PDB files in main NuGet package) with a more general scope",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 3519
      },
      {
        "type": "add-comment",
        "description": "Post analysis with confirmed root cause and workaround from issue #3519",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for reporting this! This is related to #3519 — starting from SkiaSharp 3.119.2, the Win32 (and HarfBuzzSharp) NativeAssets packages began shipping native PDB debug symbols inside the main `.nupkg` under `runtimes/win-xxx/native/`. When building a MAUI Windows app in **unpackaged** mode, MSBuild treats all files under `runtimes/*/native/` as native copy-local items and deploys them to the output directory — including the large PDB files. Packaged (MSIX) mode isn't affected because the MSIX pipeline processes native assets differently.\n\n**Workaround (add to your `.csproj` or `Directory.Build.targets`):**\n\nFor .NET 7+, this prevents PDB files from entering the copy-local list and `deps.json` entirely:\n```xml\n<Target Name=\"RemovePackageNativePdbsFromDeps\" AfterTargets=\"ResolvePackageAssets\">\n  <ItemGroup>\n    <RuntimeTargetsCopyLocalItems Remove=\"@(RuntimeTargetsCopyLocalItems)\" Condition=\"'%(Extension)' == '.pdb'\" />\n    <NativeCopyLocalItems Remove=\"@(NativeCopyLocalItems)\" Condition=\"'%(Extension)' == '.pdb'\" />\n  </ItemGroup>\n</Target>\n```\n\nA proper fix in SkiaSharp's build targets to prevent PDB files from being deployed as native assets is being tracked."
      }
    ]
  }
}
```

</details>
