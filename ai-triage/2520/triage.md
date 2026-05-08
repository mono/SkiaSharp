# Issue Triage Report — #2520

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T13:09:19Z |
| Type | type/enhancement (0.90 (90%)) |
| Area | area/Build (0.92 (92%)) |
| Suggested action | keep-open (0.88 (88%)) |

**Issue Summary:** Users want to reduce the native runtime output footprint by selecting specific RIDs at build time; currently all platforms (~132 MB) are always copied regardless of RuntimeIdentifiers settings.

**Analysis:** The NativeAssets .targets files unconditionally include all runtime-specific native binaries for their platform package. There is no MSBuild logic to filter based on <RuntimeIdentifiers>. The <RuntimeIdentifier> (singular) property can limit to a single RID, but there is no first-party way to specify a subset of RIDs. The maintainer acknowledged a future approach using the .NET runtime system feature to replace native asset NuGet packages, but it has not been implemented yet.

**Recommendations:** **keep-open** — Valid enhancement request acknowledged by maintainer. A community workaround exists. The proper fix requires changes to the NativeAssets build targets or a larger architectural change to the native packaging system.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/Build |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create a .NET desktop project referencing SkiaSharp
2. Set <RuntimeIdentifiers>win10-x64;linux-x64</RuntimeIdentifiers> in the project file
3. Build the project
4. Observe that all runtimes (Linux x86/x64/ARM/MUSL, OSX, Windows x86/x64/arm64, Win7) are still copied to the output folder (~132 MB)

**Environment:** Avalonia v0.10.21, SkiaSharp (unspecified version), .NET desktop/multi-platform

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2520#issuecomment-1626067943 — Reporter discovered that singular <RuntimeIdentifier> limits output to one RID, but plural <RuntimeIdentifiers> does not filter
- https://github.com/mono/SkiaSharp/issues/2520#issuecomment-2996347293 — Maintainer (mattleibow) confirmed runtimes are independent and noted a future .NET runtime system feature could replace native asset NuGets
- https://github.com/mono/SkiaSharp/issues/2520#issuecomment-3632669881 — Community workaround: custom MSBuild target TrimUntargetedRuntimes that filters RuntimeTargetsCopyLocalItems based on RuntimeIdentifiers

## Analysis

### Technical Summary

The NativeAssets .targets files unconditionally include all runtime-specific native binaries for their platform package. There is no MSBuild logic to filter based on <RuntimeIdentifiers>. The <RuntimeIdentifier> (singular) property can limit to a single RID, but there is no first-party way to specify a subset of RIDs. The maintainer acknowledged a future approach using the .NET runtime system feature to replace native asset NuGet packages, but it has not been implemented yet.

### Rationale

This is a valid enhancement request with multiple community reports. The current behavior of including all runtimes regardless of <RuntimeIdentifiers> is technically correct (NuGet RID fallback requires all RIDs in the package) but inconvenient for deployment scenarios. The maintainer has acknowledged the limitation. A workaround exists (custom MSBuild target from community or singular <RuntimeIdentifier>). Classified as enhancement because this improves existing packaging behavior without adding completely new functionality.

### Key Signals

- "using the singular, <RuntimeIdentifier>linux-x64</RuntimeIdentifier> limits the output to just linux-x64, but the plural <RuntimeIdentifiers> generates the subfolder runtimes for all of them" — **comment by DamianSuess** (Root cause confirmed by reporter — singular vs plural property has different behavior for native asset inclusion)
- "I know the runtime system has a feature so we can do away with all the weird native asset nugets. I need to investigate and implement that still." — **comment by mattleibow** (Maintainer acknowledged the limitation and identified a longer-term architectural solution path)
- "It is safe. The runtimes are all independent of each other." — **comment by mattleibow** (Confirms users can safely delete extra runtime folders as a manual workaround)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.Win32/buildTransitive/net4/SkiaSharp.targets` | 12-28 | direct | Unconditionally includes win-x86, win-x64, and win-arm64 native DLLs in None items with CopyToOutputDirectory=PreserveNewest. No check of RuntimeIdentifiers property — all three architectures are always copied to output. |
| `binding/IncludeNativeAssets.SkiaSharp.targets` | 9-15 | related | For non-platform-specific TFMs, includes Windows, OSX, and Linux natives from the output/native directory based on BuildArch/Platform — again no RuntimeIdentifiers filtering, so all native files are included for desktop builds. |

### Workarounds

- Use singular <RuntimeIdentifier>win-x64</RuntimeIdentifier> if targeting only a single runtime (prevents multi-RID selection but reduces output)
- Add custom MSBuild target to trim untargeted runtimes after asset resolution: <Target Name="TrimUntargetedRuntimes" AfterTargets="ResolvePackageAssets"><ItemGroup><RuntimeTargetsCopyLocalItems Remove="@(RuntimeTargetsCopyLocalItems)" Condition="!$([System.Text.RegularExpressions.Regex]::Match($(RuntimeIdentifiers),'(^|;)\s*%(RuntimeTargetsCopyLocalItems.RuntimeIdentifier)\s*(;|$)').Success)" /></ItemGroup></Target>
- Manually delete extra runtime folders from output directory after build (safe per maintainer comment)

### Next Questions

- What is the .NET runtime system feature mattleibow is referring to that could replace native asset NuGet packages?
- Should the NativeAssets targets be updated to respect <RuntimeIdentifiers> as a built-in feature?

### Resolution Proposals

**Hypothesis:** The NativeAssets .targets files need to be updated to filter native binaries based on the <RuntimeIdentifiers> MSBuild property, similar to how .NET SDK itself handles this.

1. **Add RuntimeIdentifiers filtering to NativeAssets targets** — fix, confidence 0.70 (70%), cost/m, validated=untested
   - Update all NativeAssets .targets files to conditionally include native binaries only for the RIDs listed in <RuntimeIdentifiers>. This would require modifying each platform's .targets file to check RuntimeIdentifiers before adding items to None/Content groups.
2. **Use custom MSBuild target workaround (community solution)** — workaround, confidence 0.85 (85%), cost/xs, validated=untested
   - Add a TrimUntargetedRuntimes target to the project that removes RuntimeTargetsCopyLocalItems not matching the configured RuntimeIdentifiers. This is an immediate workaround users can apply now.

```csharp
<Target Name="TrimUntargetedRuntimes" AfterTargets="ResolvePackageAssets">
  <ItemGroup>
    <RuntimeTargetsCopyLocalItems Remove="@(RuntimeTargetsCopyLocalItems)"
      Condition="!$([System.Text.RegularExpressions.Regex]::Match($(RuntimeIdentifiers),'(^|;)\s*%(RuntimeTargetsCopyLocalItems.RuntimeIdentifier)\s*(;|$)').Success)" />
  </ItemGroup>
</Target>
```

**Recommended proposal:** Use custom MSBuild target workaround (community solution)

**Why:** Immediate user relief with low effort while the longer-term architectural fix (reworking native asset packaging) is planned by the maintainer.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.88 (88%) |
| Reason | Valid enhancement request acknowledged by maintainer. A community workaround exists. The proper fix requires changes to the NativeAssets build targets or a larger architectural change to the native packaging system. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply enhancement and build labels | labels=type/enhancement, area/Build |
| add-comment | medium | 0.85 (85%) | Share community workaround and maintainer guidance | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for filing this! There are a couple of workarounds available while this is being investigated:

**Option 1 — Singular `<RuntimeIdentifier>` (single target only):**
If you need only one target platform, use the singular form:
```xml
<RuntimeIdentifier>win-x64</RuntimeIdentifier>
```

**Option 2 — Custom MSBuild target (multi-RID support):**
Add this target to your `.csproj` to trim runtimes not listed in `<RuntimeIdentifiers>`:
```xml
<Target Name="TrimUntargetedRuntimes" AfterTargets="ResolvePackageAssets">
  <ItemGroup>
    <RuntimeTargetsCopyLocalItems Remove="@(RuntimeTargetsCopyLocalItems)"
      Condition="!$([System.Text.RegularExpressions.Regex]::Match($(RuntimeIdentifiers),'(^|;)\s*%(RuntimeTargetsCopyLocalItems.RuntimeIdentifier)\s*(;|$)').Success)" />
  </ItemGroup>
</Target>
```

**Option 3 — Manual deletion:**
As confirmed by @mattleibow, all runtime folders are independent — it is safe to manually delete unneeded platform folders from the output.

The underlying issue is that the NativeAssets `.targets` files currently do not filter based on `<RuntimeIdentifiers>`. A longer-term solution may involve reworking the native asset packaging approach.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2520,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T13:09:19Z"
  },
  "summary": "Users want to reduce the native runtime output footprint by selecting specific RIDs at build time; currently all platforms (~132 MB) are always copied regardless of RuntimeIdentifiers settings.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.9
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.92
    }
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET desktop project referencing SkiaSharp",
        "Set <RuntimeIdentifiers>win10-x64;linux-x64</RuntimeIdentifiers> in the project file",
        "Build the project",
        "Observe that all runtimes (Linux x86/x64/ARM/MUSL, OSX, Windows x86/x64/arm64, Win7) are still copied to the output folder (~132 MB)"
      ],
      "environmentDetails": "Avalonia v0.10.21, SkiaSharp (unspecified version), .NET desktop/multi-platform",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2520#issuecomment-1626067943",
          "description": "Reporter discovered that singular <RuntimeIdentifier> limits output to one RID, but plural <RuntimeIdentifiers> does not filter"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2520#issuecomment-2996347293",
          "description": "Maintainer (mattleibow) confirmed runtimes are independent and noted a future .NET runtime system feature could replace native asset NuGets"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2520#issuecomment-3632669881",
          "description": "Community workaround: custom MSBuild target TrimUntargetedRuntimes that filters RuntimeTargetsCopyLocalItems based on RuntimeIdentifiers"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The NativeAssets .targets files unconditionally include all runtime-specific native binaries for their platform package. There is no MSBuild logic to filter based on <RuntimeIdentifiers>. The <RuntimeIdentifier> (singular) property can limit to a single RID, but there is no first-party way to specify a subset of RIDs. The maintainer acknowledged a future approach using the .NET runtime system feature to replace native asset NuGet packages, but it has not been implemented yet.",
    "rationale": "This is a valid enhancement request with multiple community reports. The current behavior of including all runtimes regardless of <RuntimeIdentifiers> is technically correct (NuGet RID fallback requires all RIDs in the package) but inconvenient for deployment scenarios. The maintainer has acknowledged the limitation. A workaround exists (custom MSBuild target from community or singular <RuntimeIdentifier>). Classified as enhancement because this improves existing packaging behavior without adding completely new functionality.",
    "keySignals": [
      {
        "text": "using the singular, <RuntimeIdentifier>linux-x64</RuntimeIdentifier> limits the output to just linux-x64, but the plural <RuntimeIdentifiers> generates the subfolder runtimes for all of them",
        "source": "comment by DamianSuess",
        "interpretation": "Root cause confirmed by reporter — singular vs plural property has different behavior for native asset inclusion"
      },
      {
        "text": "I know the runtime system has a feature so we can do away with all the weird native asset nugets. I need to investigate and implement that still.",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer acknowledged the limitation and identified a longer-term architectural solution path"
      },
      {
        "text": "It is safe. The runtimes are all independent of each other.",
        "source": "comment by mattleibow",
        "interpretation": "Confirms users can safely delete extra runtime folders as a manual workaround"
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.Win32/buildTransitive/net4/SkiaSharp.targets",
        "lines": "12-28",
        "finding": "Unconditionally includes win-x86, win-x64, and win-arm64 native DLLs in None items with CopyToOutputDirectory=PreserveNewest. No check of RuntimeIdentifiers property — all three architectures are always copied to output.",
        "relevance": "direct"
      },
      {
        "file": "binding/IncludeNativeAssets.SkiaSharp.targets",
        "lines": "9-15",
        "finding": "For non-platform-specific TFMs, includes Windows, OSX, and Linux natives from the output/native directory based on BuildArch/Platform — again no RuntimeIdentifiers filtering, so all native files are included for desktop builds.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use singular <RuntimeIdentifier>win-x64</RuntimeIdentifier> if targeting only a single runtime (prevents multi-RID selection but reduces output)",
      "Add custom MSBuild target to trim untargeted runtimes after asset resolution: <Target Name=\"TrimUntargetedRuntimes\" AfterTargets=\"ResolvePackageAssets\"><ItemGroup><RuntimeTargetsCopyLocalItems Remove=\"@(RuntimeTargetsCopyLocalItems)\" Condition=\"!$([System.Text.RegularExpressions.Regex]::Match($(RuntimeIdentifiers),'(^|;)\\s*%(RuntimeTargetsCopyLocalItems.RuntimeIdentifier)\\s*(;|$)').Success)\" /></ItemGroup></Target>",
      "Manually delete extra runtime folders from output directory after build (safe per maintainer comment)"
    ],
    "nextQuestions": [
      "What is the .NET runtime system feature mattleibow is referring to that could replace native asset NuGet packages?",
      "Should the NativeAssets targets be updated to respect <RuntimeIdentifiers> as a built-in feature?"
    ],
    "resolution": {
      "hypothesis": "The NativeAssets .targets files need to be updated to filter native binaries based on the <RuntimeIdentifiers> MSBuild property, similar to how .NET SDK itself handles this.",
      "proposals": [
        {
          "title": "Add RuntimeIdentifiers filtering to NativeAssets targets",
          "description": "Update all NativeAssets .targets files to conditionally include native binaries only for the RIDs listed in <RuntimeIdentifiers>. This would require modifying each platform's .targets file to check RuntimeIdentifiers before adding items to None/Content groups.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Use custom MSBuild target workaround (community solution)",
          "description": "Add a TrimUntargetedRuntimes target to the project that removes RuntimeTargetsCopyLocalItems not matching the configured RuntimeIdentifiers. This is an immediate workaround users can apply now.",
          "codeSnippet": "<Target Name=\"TrimUntargetedRuntimes\" AfterTargets=\"ResolvePackageAssets\">\n  <ItemGroup>\n    <RuntimeTargetsCopyLocalItems Remove=\"@(RuntimeTargetsCopyLocalItems)\"\n      Condition=\"!$([System.Text.RegularExpressions.Regex]::Match($(RuntimeIdentifiers),'(^|;)\\s*%(RuntimeTargetsCopyLocalItems.RuntimeIdentifier)\\s*(;|$)').Success)\" />\n  </ItemGroup>\n</Target>",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use custom MSBuild target workaround (community solution)",
      "recommendedReason": "Immediate user relief with low effort while the longer-term architectural fix (reworking native asset packaging) is planned by the maintainer."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.88,
      "reason": "Valid enhancement request acknowledged by maintainer. A community workaround exists. The proper fix requires changes to the NativeAssets build targets or a larger architectural change to the native packaging system.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement and build labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/enhancement",
          "area/Build"
        ]
      },
      {
        "type": "add-comment",
        "description": "Share community workaround and maintainer guidance",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for filing this! There are a couple of workarounds available while this is being investigated:\n\n**Option 1 — Singular `<RuntimeIdentifier>` (single target only):**\nIf you need only one target platform, use the singular form:\n```xml\n<RuntimeIdentifier>win-x64</RuntimeIdentifier>\n```\n\n**Option 2 — Custom MSBuild target (multi-RID support):**\nAdd this target to your `.csproj` to trim runtimes not listed in `<RuntimeIdentifiers>`:\n```xml\n<Target Name=\"TrimUntargetedRuntimes\" AfterTargets=\"ResolvePackageAssets\">\n  <ItemGroup>\n    <RuntimeTargetsCopyLocalItems Remove=\"@(RuntimeTargetsCopyLocalItems)\"\n      Condition=\"!$([System.Text.RegularExpressions.Regex]::Match($(RuntimeIdentifiers),'(^|;)\\s*%(RuntimeTargetsCopyLocalItems.RuntimeIdentifier)\\s*(;|$)').Success)\" />\n  </ItemGroup>\n</Target>\n```\n\n**Option 3 — Manual deletion:**\nAs confirmed by @mattleibow, all runtime folders are independent — it is safe to manually delete unneeded platform folders from the output.\n\nThe underlying issue is that the NativeAssets `.targets` files currently do not filter based on `<RuntimeIdentifiers>`. A longer-term solution may involve reworking the native asset packaging approach."
      }
    ]
  }
}
```

</details>
