# Issue Triage Report — #736

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T06:16:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/Build (0.88 (88%)) |
| Suggested action | close-as-external (0.82 (82%)) |

**Issue Summary:** SkiaSharp.Views.Mac namespace is not resolved when a macOS Cocoa project targets Xamarin.Mac Full framework instead of Xamarin.Mac Modern, because NuGet incorrectly selects the .NET framework TFM instead of the Xamarin.Mac TFM.

**Analysis:** NuGet package resolution fails for Xamarin.Mac Full framework apps because NuGet treats them as .NET Framework apps instead of Xamarin.Mac apps, resulting in the wrong assembly being selected. The root cause is in the NuGet client, not SkiaSharp. Maintainer provided workarounds and filed a NuGet PR. Xamarin.Mac Classic/Full is now deprecated; modern SkiaSharp targets net*-macos via .NET MAUI.

**Recommendations:** **close-as-external** — Root cause is in NuGet TFM resolution for Xamarin.Mac Full projects, not in SkiaSharp. The underlying platform (Xamarin.Mac Classic/Full) is now deprecated by Microsoft. Maintainer has provided workarounds and the modern path (net*-macos) is fully supported.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/Build |
| Platforms | os/macOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug, type/enhancement, os/macOS, area/SkiaSharp.Views, area/Build, status/blocked |

## Evidence

### Reproduction

1. Create a Cocoa project in Visual Studio for Mac
2. Add SkiaSharp and SkiaSharp.Views NuGet packages (defaults to Xamarin.Mac Modern)
3. Edit project General settings and change target framework to Xamarin.Mac Full
4. Re-add NuGet packages or rebuild
5. Observe: 'using SkiaSharp.Views.Mac;' fails with namespace not found error

**Environment:** macOS 10.14, Visual Studio for Mac, SkiaSharp v1.68, Xamarin.Mac Full framework

**Related issues:** #1583

**Repository links:**
- https://github.com/NuGet/NuGet.Client/pull/2572 — NuGet PR to fix TFM resolution for Xamarin.Mac Full apps
- https://devdiv.visualstudio.com/DevDiv/_workitems/edit/753282 — Visual Studio bug report for the same issue
- https://github.com/mono/SkiaSharp/issues/1583 — Related issue: same Xamarin.Mac Full compilation error reported later

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | build-error |
| Error message | The type or namespace name 'Mac' does not exist in the namespace 'SkiaSharp.Views' |
| Repro quality | partial |
| Target frameworks | XamarinMac-Full |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | SkiaSharp now uses modern .NET TFMs (net8.0-macos, net9.0-macos). Xamarin.Mac Classic/Full framework is deprecated. The NuGet TFM resolution issue for the legacy Xamarin.Mac Full TFM is no longer relevant to current SkiaSharp versions. |

## Analysis

### Technical Summary

NuGet package resolution fails for Xamarin.Mac Full framework apps because NuGet treats them as .NET Framework apps instead of Xamarin.Mac apps, resulting in the wrong assembly being selected. The root cause is in the NuGet client, not SkiaSharp. Maintainer provided workarounds and filed a NuGet PR. Xamarin.Mac Classic/Full is now deprecated; modern SkiaSharp targets net*-macos via .NET MAUI.

### Rationale

The bug is real but its root cause is entirely in the NuGet client (wrong TFM selection for Xamarin.Mac Full projects). The maintainer confirmed this and pointed to NuGet/NuGet.Client#2572. Since then, Xamarin.Mac Classic/Full has been deprecated by Microsoft in favor of .NET MAUI + net*-macos TFMs, making this issue obsolete for current SkiaSharp versions. Closing as external is appropriate since the fix was in the NuGet toolchain and the platform itself is now deprecated.

### Key Signals

- "when I edit the project, General, and change 'target framework' to 'Xamarin.Mac Full', then it can't find namespace SkiaSharp.Views.Mac" — **issue body** (Problem is specifically triggered by switching from Xamarin.Mac Modern to Xamarin.Mac Full, pointing to a TFM resolution difference.)
- "The reason this bug shows is that when you select the full framework option, the app actually 'transitions' from a 'mac app' to a '.net framework app' and NuGet does not know that this particular app is actually both a 'mac app' AND a 'framework app'." — **maintainer comment (mattleibow)** (Root cause confirmed by maintainer — NuGet TFM resolution bug, not a SkiaSharp code issue.)
- "There has been an issue that has been opened, closed, reopened and eventually locked with a few PRs. But, not to be outdone, I have created a new PR that hopefully gets this in: https://github.com/NuGet/NuGet.Client/pull/2572" — **maintainer comment (mattleibow)** (The fix was tracked in the NuGet client repository, not SkiaSharp.)
- "you can add <MigrateToNewXMIdentifier>true</MigrateToNewXMIdentifier> in your project" — **maintainer comment (mattleibow)** (A newer workaround was identified via the NuGet PR discussion.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/SkiaSharp.Views.csproj` | 16-19 | direct | The SkiaSharp.Views project defines RootNamespace=SkiaSharp.Views.Mac and AssemblyName=SkiaSharp.Views.Mac only for TargetFramework containing '-macos' (modern .NET TFM). There is no legacy Xamarin.Mac Classic/Full TFM target in the current codebase. |
| `scripts/cake/UtilsManaged.cake` | — | related | Build utilities contain a branch for 'xamarinmac' and 'xamarin.mac' TFM prefixes alongside modern 'net*-macos' TFMs, indicating the transition from legacy Xamarin.Mac to modern .NET has been accommodated at the build level. |

### Workarounds

- Use packages.config instead of PackageReference and manually set the HintPath to the Xamarin.Mac version of SkiaSharp.Views.Mac.dll
- Don't reference the SkiaSharp.Views NuGet package; manually add a project reference to SkiaSharp.Views.Mac.dll
- Add <MigrateToNewXMIdentifier>true</MigrateToNewXMIdentifier> to the .csproj to make NuGet recognize the project as a Xamarin.Mac app (not a .NET framework app)
- Migrate to modern .NET (net8.0-macos / net9.0-macos) using .NET MAUI or native macOS project — SkiaSharp fully supports modern macOS targets

### Next Questions

- Is the reporter using Xamarin.Mac legacy? If so, they should migrate to .NET MAUI or net*-macos.
- Has the NuGet PR #2572 been merged and does it resolve the issue for Xamarin.Mac Full projects?

### Resolution Proposals

**Hypothesis:** NuGet's TFM selection logic for Xamarin.Mac Full projects was broken — it chose the .NET Framework TFM instead of the Xamarin.Mac TFM. The underlying platform (Xamarin.Mac Classic/Full) is now deprecated.

1. **Apply MigrateToNewXMIdentifier workaround** — workaround, confidence 0.75 (75%), cost/xs, validated=untested
   - Add <MigrateToNewXMIdentifier>true</MigrateToNewXMIdentifier> to the .csproj file to make NuGet correctly identify the project as a Xamarin.Mac app rather than a .NET Framework app.
2. **Migrate to modern .NET macOS target** — alternative, confidence 0.95 (95%), cost/m, validated=untested
   - Migrate from Xamarin.Mac Full framework to net8.0-macos (or later). SkiaSharp.Views fully supports modern macOS TFMs and this is the recommended path now that Xamarin.Mac is deprecated.

**Recommended proposal:** Migrate to modern .NET macOS target

**Why:** Xamarin.Mac Classic/Full is deprecated by Microsoft. The modern path (net8.0-macos) is fully supported by SkiaSharp and avoids the NuGet TFM resolution issue entirely.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.82 (82%) |
| Reason | Root cause is in NuGet TFM resolution for Xamarin.Mac Full projects, not in SkiaSharp. The underlying platform (Xamarin.Mac Classic/Full) is now deprecated by Microsoft. Maintainer has provided workarounds and the modern path (net*-macos) is fully supported. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Correct labels: remove duplicate type label, apply single type/bug and area/Build | labels=type/bug, area/Build, os/macOS, tenet/compatibility |
| add-comment | high | 0.82 (82%) | Post explanation with workarounds and migration guidance, recommending closure | — |
| link-related | low | 0.95 (95%) | Cross-reference related issue #1583 (same Xamarin.Mac Full compilation error) | linkedIssue=#1583 |
| close-issue | medium | 0.80 (80%) | Close as external — root cause in NuGet tooling, platform deprecated | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thank you for the report! The root cause of this issue is in the NuGet client's TFM resolution for Xamarin.Mac Full projects — when switching to Full framework, NuGet treats the project as a .NET Framework app rather than a Xamarin.Mac app, causing it to pick the wrong assembly.

For immediate workarounds:
1. Add `<MigrateToNewXMIdentifier>true</MigrateToNewXMIdentifier>` to your `.csproj` — this tells NuGet to correctly identify your project as a Xamarin.Mac app.
2. Alternatively, use `packages.config` and manually set the `HintPath` to the Xamarin.Mac version of `SkiaSharp.Views.Mac.dll`.

However, the recommended long-term path is to **migrate to modern .NET** (e.g., `net8.0-macos`). Xamarin.Mac Classic/Full is now deprecated by Microsoft in favor of .NET MAUI and .NET for macOS. SkiaSharp fully supports `net8.0-macos` and later, and the `SkiaSharp.Views.Mac` namespace works correctly under modern .NET TFMs.

Since the root cause is in external NuGet tooling and the underlying Xamarin.Mac Classic/Full platform is deprecated, we're going to close this issue. Please migrate to modern .NET macOS targets — if you encounter any issues there, please open a new issue!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 736,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T06:16:00Z",
    "currentLabels": [
      "type/bug",
      "type/enhancement",
      "os/macOS",
      "area/SkiaSharp.Views",
      "area/Build",
      "status/blocked"
    ]
  },
  "summary": "SkiaSharp.Views.Mac namespace is not resolved when a macOS Cocoa project targets Xamarin.Mac Full framework instead of Xamarin.Mac Modern, because NuGet incorrectly selects the .NET framework TFM instead of the Xamarin.Mac TFM.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.88
    },
    "platforms": [
      "os/macOS"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "build-error",
      "errorMessage": "The type or namespace name 'Mac' does not exist in the namespace 'SkiaSharp.Views'",
      "reproQuality": "partial",
      "targetFrameworks": [
        "XamarinMac-Full"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Cocoa project in Visual Studio for Mac",
        "Add SkiaSharp and SkiaSharp.Views NuGet packages (defaults to Xamarin.Mac Modern)",
        "Edit project General settings and change target framework to Xamarin.Mac Full",
        "Re-add NuGet packages or rebuild",
        "Observe: 'using SkiaSharp.Views.Mac;' fails with namespace not found error"
      ],
      "environmentDetails": "macOS 10.14, Visual Studio for Mac, SkiaSharp v1.68, Xamarin.Mac Full framework",
      "relatedIssues": [
        1583
      ],
      "repoLinks": [
        {
          "url": "https://github.com/NuGet/NuGet.Client/pull/2572",
          "description": "NuGet PR to fix TFM resolution for Xamarin.Mac Full apps"
        },
        {
          "url": "https://devdiv.visualstudio.com/DevDiv/_workitems/edit/753282",
          "description": "Visual Studio bug report for the same issue"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1583",
          "description": "Related issue: same Xamarin.Mac Full compilation error reported later"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "SkiaSharp now uses modern .NET TFMs (net8.0-macos, net9.0-macos). Xamarin.Mac Classic/Full framework is deprecated. The NuGet TFM resolution issue for the legacy Xamarin.Mac Full TFM is no longer relevant to current SkiaSharp versions."
    }
  },
  "analysis": {
    "summary": "NuGet package resolution fails for Xamarin.Mac Full framework apps because NuGet treats them as .NET Framework apps instead of Xamarin.Mac apps, resulting in the wrong assembly being selected. The root cause is in the NuGet client, not SkiaSharp. Maintainer provided workarounds and filed a NuGet PR. Xamarin.Mac Classic/Full is now deprecated; modern SkiaSharp targets net*-macos via .NET MAUI.",
    "rationale": "The bug is real but its root cause is entirely in the NuGet client (wrong TFM selection for Xamarin.Mac Full projects). The maintainer confirmed this and pointed to NuGet/NuGet.Client#2572. Since then, Xamarin.Mac Classic/Full has been deprecated by Microsoft in favor of .NET MAUI + net*-macos TFMs, making this issue obsolete for current SkiaSharp versions. Closing as external is appropriate since the fix was in the NuGet toolchain and the platform itself is now deprecated.",
    "keySignals": [
      {
        "text": "when I edit the project, General, and change 'target framework' to 'Xamarin.Mac Full', then it can't find namespace SkiaSharp.Views.Mac",
        "source": "issue body",
        "interpretation": "Problem is specifically triggered by switching from Xamarin.Mac Modern to Xamarin.Mac Full, pointing to a TFM resolution difference."
      },
      {
        "text": "The reason this bug shows is that when you select the full framework option, the app actually 'transitions' from a 'mac app' to a '.net framework app' and NuGet does not know that this particular app is actually both a 'mac app' AND a 'framework app'.",
        "source": "maintainer comment (mattleibow)",
        "interpretation": "Root cause confirmed by maintainer — NuGet TFM resolution bug, not a SkiaSharp code issue."
      },
      {
        "text": "There has been an issue that has been opened, closed, reopened and eventually locked with a few PRs. But, not to be outdone, I have created a new PR that hopefully gets this in: https://github.com/NuGet/NuGet.Client/pull/2572",
        "source": "maintainer comment (mattleibow)",
        "interpretation": "The fix was tracked in the NuGet client repository, not SkiaSharp."
      },
      {
        "text": "you can add <MigrateToNewXMIdentifier>true</MigrateToNewXMIdentifier> in your project",
        "source": "maintainer comment (mattleibow)",
        "interpretation": "A newer workaround was identified via the NuGet PR discussion."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/SkiaSharp.Views.csproj",
        "lines": "16-19",
        "finding": "The SkiaSharp.Views project defines RootNamespace=SkiaSharp.Views.Mac and AssemblyName=SkiaSharp.Views.Mac only for TargetFramework containing '-macos' (modern .NET TFM). There is no legacy Xamarin.Mac Classic/Full TFM target in the current codebase.",
        "relevance": "direct"
      },
      {
        "file": "scripts/cake/UtilsManaged.cake",
        "finding": "Build utilities contain a branch for 'xamarinmac' and 'xamarin.mac' TFM prefixes alongside modern 'net*-macos' TFMs, indicating the transition from legacy Xamarin.Mac to modern .NET has been accommodated at the build level.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Use packages.config instead of PackageReference and manually set the HintPath to the Xamarin.Mac version of SkiaSharp.Views.Mac.dll",
      "Don't reference the SkiaSharp.Views NuGet package; manually add a project reference to SkiaSharp.Views.Mac.dll",
      "Add <MigrateToNewXMIdentifier>true</MigrateToNewXMIdentifier> to the .csproj to make NuGet recognize the project as a Xamarin.Mac app (not a .NET framework app)",
      "Migrate to modern .NET (net8.0-macos / net9.0-macos) using .NET MAUI or native macOS project — SkiaSharp fully supports modern macOS targets"
    ],
    "nextQuestions": [
      "Is the reporter using Xamarin.Mac legacy? If so, they should migrate to .NET MAUI or net*-macos.",
      "Has the NuGet PR #2572 been merged and does it resolve the issue for Xamarin.Mac Full projects?"
    ],
    "resolution": {
      "hypothesis": "NuGet's TFM selection logic for Xamarin.Mac Full projects was broken — it chose the .NET Framework TFM instead of the Xamarin.Mac TFM. The underlying platform (Xamarin.Mac Classic/Full) is now deprecated.",
      "proposals": [
        {
          "title": "Apply MigrateToNewXMIdentifier workaround",
          "description": "Add <MigrateToNewXMIdentifier>true</MigrateToNewXMIdentifier> to the .csproj file to make NuGet correctly identify the project as a Xamarin.Mac app rather than a .NET Framework app.",
          "category": "workaround",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Migrate to modern .NET macOS target",
          "description": "Migrate from Xamarin.Mac Full framework to net8.0-macos (or later). SkiaSharp.Views fully supports modern macOS TFMs and this is the recommended path now that Xamarin.Mac is deprecated.",
          "category": "alternative",
          "confidence": 0.95,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Migrate to modern .NET macOS target",
      "recommendedReason": "Xamarin.Mac Classic/Full is deprecated by Microsoft. The modern path (net8.0-macos) is fully supported by SkiaSharp and avoids the NuGet TFM resolution issue entirely."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.82,
      "reason": "Root cause is in NuGet TFM resolution for Xamarin.Mac Full projects, not in SkiaSharp. The underlying platform (Xamarin.Mac Classic/Full) is now deprecated by Microsoft. Maintainer has provided workarounds and the modern path (net*-macos) is fully supported.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct labels: remove duplicate type label, apply single type/bug and area/Build",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/Build",
          "os/macOS",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post explanation with workarounds and migration guidance, recommending closure",
        "risk": "high",
        "confidence": 0.82,
        "comment": "Thank you for the report! The root cause of this issue is in the NuGet client's TFM resolution for Xamarin.Mac Full projects — when switching to Full framework, NuGet treats the project as a .NET Framework app rather than a Xamarin.Mac app, causing it to pick the wrong assembly.\n\nFor immediate workarounds:\n1. Add `<MigrateToNewXMIdentifier>true</MigrateToNewXMIdentifier>` to your `.csproj` — this tells NuGet to correctly identify your project as a Xamarin.Mac app.\n2. Alternatively, use `packages.config` and manually set the `HintPath` to the Xamarin.Mac version of `SkiaSharp.Views.Mac.dll`.\n\nHowever, the recommended long-term path is to **migrate to modern .NET** (e.g., `net8.0-macos`). Xamarin.Mac Classic/Full is now deprecated by Microsoft in favor of .NET MAUI and .NET for macOS. SkiaSharp fully supports `net8.0-macos` and later, and the `SkiaSharp.Views.Mac` namespace works correctly under modern .NET TFMs.\n\nSince the root cause is in external NuGet tooling and the underlying Xamarin.Mac Classic/Full platform is deprecated, we're going to close this issue. Please migrate to modern .NET macOS targets — if you encounter any issues there, please open a new issue!"
      },
      {
        "type": "link-related",
        "description": "Cross-reference related issue #1583 (same Xamarin.Mac Full compilation error)",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 1583
      },
      {
        "type": "close-issue",
        "description": "Close as external — root cause in NuGet tooling, platform deprecated",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
