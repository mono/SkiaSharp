# Issue Triage Report — #855

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T19:30:32Z |
| Type | type/question (0.82 (82%)) |
| Area | area/libSkiaSharp.native (0.85 (85%)) |
| Suggested action | close-as-not-a-bug (0.82 (82%)) |

**Issue Summary:** Xamarin.iOS build fails with MTOUCH MT0009 error when building with SkiaSharp 1.68.0+, because the win-x86/win-x64 native DLL from the NuGet package is included in the iOS build artifacts when the project has PlatformTarget set to x86 or CI uses Windows as the build host.

**Analysis:** The Xamarin.iOS mtouch linker fails with MT0009 because the Windows native DLL (from the NuGet package's runtimes/win-x86/native/ directory) is being copied into the iOS build output. This happens when the iOS .csproj has `<PlatformTarget>x86</PlatformTarget>` (or x64) in its Release|iPhone configuration, which causes NuGet to resolve the matching Windows RID assets. The same effect occurs in CI environments (App Center, Azure DevOps) that use Windows agents for the build. SkiaSharp fully supports Xamarin 4.0; the problem is a project misconfiguration.

**Recommendations:** **close-as-not-a-bug** — SkiaSharp fully supports Xamarin 4.0. The error is caused by a project configuration issue (PlatformTarget=x86 in iOS csproj) that causes NuGet to resolve Windows native assets into the iOS build. Known workarounds are well-documented in the comments. Issue #854 is the same question from the same reporter.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/libSkiaSharp.native |
| Platforms | os/iOS |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Xamarin Forms 4.0, SkiaSharp 1.68.0 (and later 2.80.1, 2.80.2), FFImageLoading, Visual Studio on Windows with Xamarin connected to Mac build host

**Repository links:**
- https://github.com/xamarin/xamarin-macios/issues/5265 — Linked upstream Xamarin.iOS issue about mtouch including Windows DLLs during iOS builds

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.0, 2.80.1, 2.80.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The issue is a project configuration problem (PlatformTarget=x86 in iOS csproj) that causes NuGet to resolve win-x86 RID assets. This is a user-side config issue acknowledged by the maintainer with known workarounds. Modern SDK-style projects typically don't have this problem. |

## Analysis

### Technical Summary

The Xamarin.iOS mtouch linker fails with MT0009 because the Windows native DLL (from the NuGet package's runtimes/win-x86/native/ directory) is being copied into the iOS build output. This happens when the iOS .csproj has `<PlatformTarget>x86</PlatformTarget>` (or x64) in its Release|iPhone configuration, which causes NuGet to resolve the matching Windows RID assets. The same effect occurs in CI environments (App Center, Azure DevOps) that use Windows agents for the build. SkiaSharp fully supports Xamarin 4.0; the problem is a project misconfiguration.

### Rationale

The reporter asks 'Do support SkiaSharp the new version of Xamarin?' — this is fundamentally a compatibility question. The error is caused by a project configuration setting that causes NuGet to include win-x86 native assets in iOS builds. The maintainer (mattleibow) commented with workarounds (ExcludeAssets=runtimes) and multiple community members confirmed that removing `<PlatformTarget>x86</PlatformTarget>` from the iOS csproj resolves the issue. Issue #854 is the same question from the same reporter filed 2 days earlier.

### Key Signals

- "MTOUCH: Error MT0009: Error while loading assemblies: /Users/hugo/.nuget/packages/skiasharp/1.68.0/runtimes/win-x86/native/libSkiaSharp.dll" — **issue body** (mtouch is encountering a Windows PE DLL (win-x86 RID asset) in the iOS build output and cannot process it. This is caused by NuGet resolving the win-x86 RID because PlatformTarget is set to x86.)
- "The workaround of #5265 to switch from x64 to Any CPU temporally resolve the issue." — **comment by AndreaGobs** (Confirms the root cause: PlatformTarget setting determines which RID NuGet resolves. AnyCPU avoids picking up win-x86 assets.)
- "Remove the <PlatformTarget>x86</PlatformTarget> and you should be good to go." — **comment by michaelstonis** (Confirmed workaround: removing the PlatformTarget element from the iOS Release configuration fixes the issue for most users.)
- "I can build locally, because I do not have the PlatformTarget line, but I cannot build via App Center." — **comment by DeerSteak** (CI environments (Windows agents) can also trigger the issue independently of the PlatformTarget setting, in which case ExcludeAssets=runtimes is needed.)
- "maybe you can try adding ExcludeAssets=runtimes" — **comment by mattleibow (maintainer)** (Official maintainer workaround for CI scenarios: add ExcludeAssets=runtimes to the SkiaSharp PackageReference to prevent Windows DLLs from being included.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj` | 9-11 | direct | The Win32 NativeAssets package explicitly includes win-x64, win-x86, and win-arm64 RID variants of libSkiaSharp.dll. When NuGet resolves RID assets for a project with PlatformTarget=x86, it picks up the win-x86 variant and copies it to the build output. |
| `binding/NativeAssets.Build.targets` | 31-32 | related | Native assets are placed under runtimes/{RID}/native/ in the NuGet package. This is standard NuGet convention; mtouch processes all DLLs in the output, including native ones it cannot handle. |

### Workarounds

- Remove `<PlatformTarget>x86</PlatformTarget>` (or x64) from the iOS csproj Release|iPhone configuration — this prevents NuGet from resolving Windows RID assets
- Add `ExcludeAssets="runtimes"` to the SkiaSharp (and HarfBuzzSharp) PackageReference in the iOS project — prevents Windows native DLLs from being included entirely
- Add `<RuntimeIdentifiers>osx;osx-x86;osx-x64</RuntimeIdentifiers>` and `<NuGetRuntimeIdentifier>osx</NuGetRuntimeIdentifier>` to the iOS csproj to force NuGet to use macOS RID assets

### Resolution Proposals

**Hypothesis:** The iOS project has PlatformTarget=x86 (or CI uses a Windows agent), causing NuGet RID resolution to copy win-x86 libSkiaSharp.dll into the iOS build output. mtouch cannot process Windows PE DLLs.

1. **Remove PlatformTarget from iOS csproj** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Remove the `<PlatformTarget>x86</PlatformTarget>` element from all iPhone/Release/Ad-Hoc/AppStore PropertyGroup configurations in the iOS .csproj file. This prevents NuGet from matching a Windows RID.
2. **Use ExcludeAssets=runtimes on SkiaSharp reference** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - Add `ExcludeAssets="runtimes"` to the SkiaSharp PackageReference in the iOS project. This is especially needed for CI builds on Windows agents.

```xml
<PackageReference Include="SkiaSharp" Version="2.88.x" ExcludeAssets="runtimes" />
```

```csharp
<PackageReference Include="SkiaSharp" Version="2.88.x" ExcludeAssets="runtimes" />
```

**Recommended proposal:** Remove PlatformTarget from iOS csproj

**Why:** Directly addresses the root cause for local builds. For CI scenarios, combine with ExcludeAssets=runtimes.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.82 (82%) |
| Reason | SkiaSharp fully supports Xamarin 4.0. The error is caused by a project configuration issue (PlatformTarget=x86 in iOS csproj) that causes NuGet to resolve Windows native assets into the iOS build. Known workarounds are well-documented in the comments. Issue #854 is the same question from the same reporter. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply type/question, area/libSkiaSharp.native, os/iOS labels | labels=type/question, area/libSkiaSharp.native, os/iOS |
| link-related | low | 0.95 (95%) | Link to related question issue #854 (same reporter, same error, closed) | linkedIssue=#854 |
| add-comment | high | 0.82 (82%) | Post answer with workarounds for the mtouch MT0009 error | — |
| close-issue | medium | 0.80 (80%) | Close as answered — SkiaSharp supports Xamarin 4.0, the issue is a project configuration problem with known workarounds | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Yes, SkiaSharp supports Xamarin Forms 4.0. The error you're seeing is caused by a project configuration issue, not a SkiaSharp incompatibility.

**Root cause:** Your iOS `.csproj` has `<PlatformTarget>x86</PlatformTarget>` (or `x64`) in the `Release|iPhone` configuration. This causes NuGet to resolve the `win-x86` (or `win-x64`) native assets for SkiaSharp — Windows DLLs that `mtouch` cannot process.

**Fix 1 (local builds):** Remove the `<PlatformTarget>` element from all `iPhone` configurations in your iOS `.csproj`:
```xml
<!-- Remove this line from Release|iPhone, Ad-Hoc, AppStore configurations -->
<PlatformTarget>x86</PlatformTarget>
```

**Fix 2 (CI / App Center):** Add `ExcludeAssets="runtimes"` to the SkiaSharp reference in your iOS project:
```xml
<PackageReference Include="SkiaSharp" Version="2.88.x" ExcludeAssets="runtimes" />
<PackageReference Include="SkiaSharp.Views.Forms" Version="2.88.x" ExcludeAssets="runtimes" />
```
Make sure to also add the reference (without `ExcludeAssets`) to the final app project so the correct native binaries are deployed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 855,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T19:30:32Z"
  },
  "summary": "Xamarin.iOS build fails with MTOUCH MT0009 error when building with SkiaSharp 1.68.0+, because the win-x86/win-x64 native DLL from the NuGet package is included in the iOS build artifacts when the project has PlatformTarget set to x86 or CI uses Windows as the build host.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.82
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.85
    },
    "platforms": [
      "os/iOS"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Xamarin Forms 4.0, SkiaSharp 1.68.0 (and later 2.80.1, 2.80.2), FFImageLoading, Visual Studio on Windows with Xamarin connected to Mac build host",
      "repoLinks": [
        {
          "url": "https://github.com/xamarin/xamarin-macios/issues/5265",
          "description": "Linked upstream Xamarin.iOS issue about mtouch including Windows DLLs during iOS builds"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.0",
        "2.80.1",
        "2.80.2"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "The issue is a project configuration problem (PlatformTarget=x86 in iOS csproj) that causes NuGet to resolve win-x86 RID assets. This is a user-side config issue acknowledged by the maintainer with known workarounds. Modern SDK-style projects typically don't have this problem."
    }
  },
  "analysis": {
    "summary": "The Xamarin.iOS mtouch linker fails with MT0009 because the Windows native DLL (from the NuGet package's runtimes/win-x86/native/ directory) is being copied into the iOS build output. This happens when the iOS .csproj has `<PlatformTarget>x86</PlatformTarget>` (or x64) in its Release|iPhone configuration, which causes NuGet to resolve the matching Windows RID assets. The same effect occurs in CI environments (App Center, Azure DevOps) that use Windows agents for the build. SkiaSharp fully supports Xamarin 4.0; the problem is a project misconfiguration.",
    "rationale": "The reporter asks 'Do support SkiaSharp the new version of Xamarin?' — this is fundamentally a compatibility question. The error is caused by a project configuration setting that causes NuGet to include win-x86 native assets in iOS builds. The maintainer (mattleibow) commented with workarounds (ExcludeAssets=runtimes) and multiple community members confirmed that removing `<PlatformTarget>x86</PlatformTarget>` from the iOS csproj resolves the issue. Issue #854 is the same question from the same reporter filed 2 days earlier.",
    "keySignals": [
      {
        "text": "MTOUCH: Error MT0009: Error while loading assemblies: /Users/hugo/.nuget/packages/skiasharp/1.68.0/runtimes/win-x86/native/libSkiaSharp.dll",
        "source": "issue body",
        "interpretation": "mtouch is encountering a Windows PE DLL (win-x86 RID asset) in the iOS build output and cannot process it. This is caused by NuGet resolving the win-x86 RID because PlatformTarget is set to x86."
      },
      {
        "text": "The workaround of #5265 to switch from x64 to Any CPU temporally resolve the issue.",
        "source": "comment by AndreaGobs",
        "interpretation": "Confirms the root cause: PlatformTarget setting determines which RID NuGet resolves. AnyCPU avoids picking up win-x86 assets."
      },
      {
        "text": "Remove the <PlatformTarget>x86</PlatformTarget> and you should be good to go.",
        "source": "comment by michaelstonis",
        "interpretation": "Confirmed workaround: removing the PlatformTarget element from the iOS Release configuration fixes the issue for most users."
      },
      {
        "text": "I can build locally, because I do not have the PlatformTarget line, but I cannot build via App Center.",
        "source": "comment by DeerSteak",
        "interpretation": "CI environments (Windows agents) can also trigger the issue independently of the PlatformTarget setting, in which case ExcludeAssets=runtimes is needed."
      },
      {
        "text": "maybe you can try adding ExcludeAssets=runtimes",
        "source": "comment by mattleibow (maintainer)",
        "interpretation": "Official maintainer workaround for CI scenarios: add ExcludeAssets=runtimes to the SkiaSharp PackageReference to prevent Windows DLLs from being included."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.Win32/SkiaSharp.NativeAssets.Win32.csproj",
        "lines": "9-11",
        "finding": "The Win32 NativeAssets package explicitly includes win-x64, win-x86, and win-arm64 RID variants of libSkiaSharp.dll. When NuGet resolves RID assets for a project with PlatformTarget=x86, it picks up the win-x86 variant and copies it to the build output.",
        "relevance": "direct"
      },
      {
        "file": "binding/NativeAssets.Build.targets",
        "lines": "31-32",
        "finding": "Native assets are placed under runtimes/{RID}/native/ in the NuGet package. This is standard NuGet convention; mtouch processes all DLLs in the output, including native ones it cannot handle.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Remove `<PlatformTarget>x86</PlatformTarget>` (or x64) from the iOS csproj Release|iPhone configuration — this prevents NuGet from resolving Windows RID assets",
      "Add `ExcludeAssets=\"runtimes\"` to the SkiaSharp (and HarfBuzzSharp) PackageReference in the iOS project — prevents Windows native DLLs from being included entirely",
      "Add `<RuntimeIdentifiers>osx;osx-x86;osx-x64</RuntimeIdentifiers>` and `<NuGetRuntimeIdentifier>osx</NuGetRuntimeIdentifier>` to the iOS csproj to force NuGet to use macOS RID assets"
    ],
    "resolution": {
      "hypothesis": "The iOS project has PlatformTarget=x86 (or CI uses a Windows agent), causing NuGet RID resolution to copy win-x86 libSkiaSharp.dll into the iOS build output. mtouch cannot process Windows PE DLLs.",
      "proposals": [
        {
          "title": "Remove PlatformTarget from iOS csproj",
          "description": "Remove the `<PlatformTarget>x86</PlatformTarget>` element from all iPhone/Release/Ad-Hoc/AppStore PropertyGroup configurations in the iOS .csproj file. This prevents NuGet from matching a Windows RID.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use ExcludeAssets=runtimes on SkiaSharp reference",
          "description": "Add `ExcludeAssets=\"runtimes\"` to the SkiaSharp PackageReference in the iOS project. This is especially needed for CI builds on Windows agents.\n\n```xml\n<PackageReference Include=\"SkiaSharp\" Version=\"2.88.x\" ExcludeAssets=\"runtimes\" />\n```",
          "codeSnippet": "<PackageReference Include=\"SkiaSharp\" Version=\"2.88.x\" ExcludeAssets=\"runtimes\" />",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Remove PlatformTarget from iOS csproj",
      "recommendedReason": "Directly addresses the root cause for local builds. For CI scenarios, combine with ExcludeAssets=runtimes."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.82,
      "reason": "SkiaSharp fully supports Xamarin 4.0. The error is caused by a project configuration issue (PlatformTarget=x86 in iOS csproj) that causes NuGet to resolve Windows native assets into the iOS build. Known workarounds are well-documented in the comments. Issue #854 is the same question from the same reporter.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/question, area/libSkiaSharp.native, os/iOS labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/question",
          "area/libSkiaSharp.native",
          "os/iOS"
        ]
      },
      {
        "type": "link-related",
        "description": "Link to related question issue #854 (same reporter, same error, closed)",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 854
      },
      {
        "type": "add-comment",
        "description": "Post answer with workarounds for the mtouch MT0009 error",
        "risk": "high",
        "confidence": 0.82,
        "comment": "Yes, SkiaSharp supports Xamarin Forms 4.0. The error you're seeing is caused by a project configuration issue, not a SkiaSharp incompatibility.\n\n**Root cause:** Your iOS `.csproj` has `<PlatformTarget>x86</PlatformTarget>` (or `x64`) in the `Release|iPhone` configuration. This causes NuGet to resolve the `win-x86` (or `win-x64`) native assets for SkiaSharp — Windows DLLs that `mtouch` cannot process.\n\n**Fix 1 (local builds):** Remove the `<PlatformTarget>` element from all `iPhone` configurations in your iOS `.csproj`:\n```xml\n<!-- Remove this line from Release|iPhone, Ad-Hoc, AppStore configurations -->\n<PlatformTarget>x86</PlatformTarget>\n```\n\n**Fix 2 (CI / App Center):** Add `ExcludeAssets=\"runtimes\"` to the SkiaSharp reference in your iOS project:\n```xml\n<PackageReference Include=\"SkiaSharp\" Version=\"2.88.x\" ExcludeAssets=\"runtimes\" />\n<PackageReference Include=\"SkiaSharp.Views.Forms\" Version=\"2.88.x\" ExcludeAssets=\"runtimes\" />\n```\nMake sure to also add the reference (without `ExcludeAssets`) to the final app project so the correct native binaries are deployed."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — SkiaSharp supports Xamarin 4.0, the issue is a project configuration problem with known workarounds",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
