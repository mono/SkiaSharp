# Issue Triage Report — #1690

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T18:01:25Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/Build (0.95 (95%)) |
| Suggested action | ready-to-fix (0.88 (88%)) |

**Issue Summary:** The net4 buildTransitive .targets files for Linux native assets use <Content> item type instead of <None>, causing MSBuild to copy all native binaries to the root of published web application output in addition to the expected bin folder location.

**Analysis:** The net4 .targets files for Linux native asset packages (SkiaSharp.NativeAssets.Linux, SkiaSharp.NativeAssets.Linux.NoDependencies, HarfBuzzSharp.NativeAssets.Linux) use <Content> to include native .so files with Link attributes. MSBuild treats Content items as web publishable assets, so the publish pipeline copies them to the root output directory. The Windows equivalent targets correctly use <None> instead. Changing Content to None would fix the extra copies while preserving CopyToOutputDirectory behavior.

**Recommendations:** **ready-to-fix** — Root cause is confirmed via code investigation. Fix is clear (change Content to None in Linux net4 targets), low-risk, and mirrors the already-correct Windows targets.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/Build |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create an ASP.NET Core web application referencing SkiaSharp
2. Run dotnet publish
3. Observe that all libSkiaSharp*.so binaries appear both in bin/ subdirectories and at the root of the publish output

**Environment:** ASP.NET Core web app, Linux native assets, net4 TFM targets

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | Unnecessary copies of all native binaries appear at root of published web app output |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Content item type is still present in the current Linux and Linux.NoDependencies net4 targets files, as well as HarfBuzzSharp Linux targets. |

## Analysis

### Technical Summary

The net4 .targets files for Linux native asset packages (SkiaSharp.NativeAssets.Linux, SkiaSharp.NativeAssets.Linux.NoDependencies, HarfBuzzSharp.NativeAssets.Linux) use <Content> to include native .so files with Link attributes. MSBuild treats Content items as web publishable assets, so the publish pipeline copies them to the root output directory. The Windows equivalent targets correctly use <None> instead. Changing Content to None would fix the extra copies while preserving CopyToOutputDirectory behavior.

### Rationale

Clear build behavior discrepancy between Linux and Windows targets files. Windows uses <None> (correct), Linux uses <Content> (wrong for native binaries). Reporter correctly identified the root cause and workaround. No functional breakage — binaries end up in the right bin folder too — but extra unwanted root copies occur during web publish. Low severity since the app still works.

### Key Signals

- "you end up getting copies of all binaries in the target-file in the root of the published result" — **issue body** (MSBuild Content items with Link are treated as publishable web assets, causing them to be output-copied to the root during web publish.)
- "Tweaking your targetfile to use something else, like None with a Link seemed to be enough" — **issue body** (Reporter already identified the fix: change <Content> to <None> in the targets file.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.Linux/buildTransitive/net4/SkiaSharp.targets` | 61-65 | direct | Uses <Content Include="@(_NativeSkiaSharpFile)"> with Link attribute. Content items are treated by MSBuild publish as web content, causing extra copies at root during dotnet publish for web apps. |
| `binding/SkiaSharp.NativeAssets.Win32/buildTransitive/net4/SkiaSharp.targets` | 23-27 | related | Uses <None Include="@(_NativeSkiaSharpFile)"> with Link attribute — the correct approach that does not trigger web publish copy behavior. |
| `binding/HarfBuzzSharp.NativeAssets.Linux/buildTransitive/net4/HarfBuzzSharp.targets` | 61-65 | direct | Same issue as SkiaSharp.NativeAssets.Linux — uses <Content> instead of <None> for native .so files. |
| `binding/SkiaSharp.NativeAssets.Linux.NoDependencies/buildTransitive/net4/SkiaSharp.targets` | 61-65 | direct | Same issue — uses <Content> instead of <None> for native .so files. |

### Workarounds

- Override the targets in your project by adding a Directory.Build.targets that removes the Content items and re-adds them as None items.
- Use None with a Link attribute in a consuming project's targets override instead of relying on the NuGet-injected targets.

### Resolution Proposals

**Hypothesis:** Change <Content> to <None> in all affected Linux net4 targets files. This preserves CopyToOutputDirectory behavior for bin placement while removing the web publish copy side-effect.

1. **Change Content to None in Linux net4 targets files** — fix, confidence 0.90 (90%), cost/xs, validated=untested
   - In binding/SkiaSharp.NativeAssets.Linux/buildTransitive/net4/SkiaSharp.targets, binding/SkiaSharp.NativeAssets.Linux.NoDependencies/buildTransitive/net4/SkiaSharp.targets, and binding/HarfBuzzSharp.NativeAssets.Linux/buildTransitive/net4/HarfBuzzSharp.targets, replace <Content Include="..."> with <None Include="...">. Windows targets already use None correctly.

**Recommended proposal:** Change Content to None in Linux net4 targets files

**Why:** Minimal, surgical change with high confidence. Matches the pattern already used in Windows targets. Reporter validated the fix locally.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.88 (88%) |
| Reason | Root cause is confirmed via code investigation. Fix is clear (change Content to None in Linux net4 targets), low-risk, and mirrors the already-correct Windows targets. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, build, linux labels | labels=type/bug, area/Build, os/Linux |
| add-comment | medium | 0.88 (88%) | Acknowledge the bug and confirm the fix path | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and suggested fix! You're right — the Linux net4 targets files incorrectly use `<Content>` for native assets instead of `<None>`, which causes MSBuild's web publish to copy the binaries to the root output directory. The Windows equivalent already uses `<None>` correctly.

The fix is to change `<Content>` to `<None>` in the affected targets files:
- `binding/SkiaSharp.NativeAssets.Linux/buildTransitive/net4/SkiaSharp.targets`
- `binding/SkiaSharp.NativeAssets.Linux.NoDependencies/buildTransitive/net4/SkiaSharp.targets`
- `binding/HarfBuzzSharp.NativeAssets.Linux/buildTransitive/net4/HarfBuzzSharp.targets`

As a workaround until this is patched, you can override the targets in your project with a `Directory.Build.targets` that removes the `Content` items and re-adds them as `None`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1690,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T18:01:25Z"
  },
  "summary": "The net4 buildTransitive .targets files for Linux native assets use <Content> item type instead of <None>, causing MSBuild to copy all native binaries to the root of published web application output in addition to the expected bin folder location.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.95
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
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "Unnecessary copies of all native binaries appear at root of published web app output",
      "reproQuality": "partial"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an ASP.NET Core web application referencing SkiaSharp",
        "Run dotnet publish",
        "Observe that all libSkiaSharp*.so binaries appear both in bin/ subdirectories and at the root of the publish output"
      ],
      "environmentDetails": "ASP.NET Core web app, Linux native assets, net4 TFM targets",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The Content item type is still present in the current Linux and Linux.NoDependencies net4 targets files, as well as HarfBuzzSharp Linux targets."
    }
  },
  "analysis": {
    "summary": "The net4 .targets files for Linux native asset packages (SkiaSharp.NativeAssets.Linux, SkiaSharp.NativeAssets.Linux.NoDependencies, HarfBuzzSharp.NativeAssets.Linux) use <Content> to include native .so files with Link attributes. MSBuild treats Content items as web publishable assets, so the publish pipeline copies them to the root output directory. The Windows equivalent targets correctly use <None> instead. Changing Content to None would fix the extra copies while preserving CopyToOutputDirectory behavior.",
    "rationale": "Clear build behavior discrepancy between Linux and Windows targets files. Windows uses <None> (correct), Linux uses <Content> (wrong for native binaries). Reporter correctly identified the root cause and workaround. No functional breakage — binaries end up in the right bin folder too — but extra unwanted root copies occur during web publish. Low severity since the app still works.",
    "keySignals": [
      {
        "text": "you end up getting copies of all binaries in the target-file in the root of the published result",
        "source": "issue body",
        "interpretation": "MSBuild Content items with Link are treated as publishable web assets, causing them to be output-copied to the root during web publish."
      },
      {
        "text": "Tweaking your targetfile to use something else, like None with a Link seemed to be enough",
        "source": "issue body",
        "interpretation": "Reporter already identified the fix: change <Content> to <None> in the targets file."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.Linux/buildTransitive/net4/SkiaSharp.targets",
        "lines": "61-65",
        "finding": "Uses <Content Include=\"@(_NativeSkiaSharpFile)\"> with Link attribute. Content items are treated by MSBuild publish as web content, causing extra copies at root during dotnet publish for web apps.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.Win32/buildTransitive/net4/SkiaSharp.targets",
        "lines": "23-27",
        "finding": "Uses <None Include=\"@(_NativeSkiaSharpFile)\"> with Link attribute — the correct approach that does not trigger web publish copy behavior.",
        "relevance": "related"
      },
      {
        "file": "binding/HarfBuzzSharp.NativeAssets.Linux/buildTransitive/net4/HarfBuzzSharp.targets",
        "lines": "61-65",
        "finding": "Same issue as SkiaSharp.NativeAssets.Linux — uses <Content> instead of <None> for native .so files.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.Linux.NoDependencies/buildTransitive/net4/SkiaSharp.targets",
        "lines": "61-65",
        "finding": "Same issue — uses <Content> instead of <None> for native .so files.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Override the targets in your project by adding a Directory.Build.targets that removes the Content items and re-adds them as None items.",
      "Use None with a Link attribute in a consuming project's targets override instead of relying on the NuGet-injected targets."
    ],
    "resolution": {
      "hypothesis": "Change <Content> to <None> in all affected Linux net4 targets files. This preserves CopyToOutputDirectory behavior for bin placement while removing the web publish copy side-effect.",
      "proposals": [
        {
          "title": "Change Content to None in Linux net4 targets files",
          "description": "In binding/SkiaSharp.NativeAssets.Linux/buildTransitive/net4/SkiaSharp.targets, binding/SkiaSharp.NativeAssets.Linux.NoDependencies/buildTransitive/net4/SkiaSharp.targets, and binding/HarfBuzzSharp.NativeAssets.Linux/buildTransitive/net4/HarfBuzzSharp.targets, replace <Content Include=\"...\"> with <None Include=\"...\">. Windows targets already use None correctly.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Change Content to None in Linux net4 targets files",
      "recommendedReason": "Minimal, surgical change with high confidence. Matches the pattern already used in Windows targets. Reporter validated the fix locally."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.88,
      "reason": "Root cause is confirmed via code investigation. Fix is clear (change Content to None in Linux net4 targets), low-risk, and mirrors the already-correct Windows targets.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, build, linux labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/Build",
          "os/Linux"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug and confirm the fix path",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report and suggested fix! You're right — the Linux net4 targets files incorrectly use `<Content>` for native assets instead of `<None>`, which causes MSBuild's web publish to copy the binaries to the root output directory. The Windows equivalent already uses `<None>` correctly.\n\nThe fix is to change `<Content>` to `<None>` in the affected targets files:\n- `binding/SkiaSharp.NativeAssets.Linux/buildTransitive/net4/SkiaSharp.targets`\n- `binding/SkiaSharp.NativeAssets.Linux.NoDependencies/buildTransitive/net4/SkiaSharp.targets`\n- `binding/HarfBuzzSharp.NativeAssets.Linux/buildTransitive/net4/HarfBuzzSharp.targets`\n\nAs a workaround until this is patched, you can override the targets in your project with a `Directory.Build.targets` that removes the `Content` items and re-adds them as `None`."
      }
    ]
  }
}
```

</details>
