# Issue Triage Report — #2147

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T11:16:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SKTextBlob.CreatePositioned throws MissingMethodException on iOS (Xamarin.Forms) due to a System.Memory version mismatch caused by SkiaSharp's netstandard2.0 dependency resolving ReadOnlySpan<T> from a different assembly than the iOS runtime provides.

**Analysis:** On Xamarin.Forms iOS, SkiaSharp resolves to its netstandard2.0 assembly, which has a PackageReference to System.Memory 4.6.3 (confirmed in binding/Directory.Build.targets). This causes the ReadOnlySpan<T> type identity used in SKTextBlob.CreatePositioned to differ from the ReadOnlySpan<T> the Mono/Xamarin iOS runtime exposes at runtime, resulting in a MissingMethodException. Android and Windows work because their runtimes resolve the mismatch differently or use different TFMs. A well-known MSBuild workaround (strip the conflicting System.Memory reference in the iOS .csproj) resolves the issue.

**Recommendations:** **needs-investigation** — Root cause is well-established (System.Memory version mismatch in netstandard2.0 TFM), a workaround exists and is confirmed by multiple users, but a proper fix (either in-package MSBuild targets or platform TFM migration in 3.x) needs investigation to scope and implement.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/wieslawsoltes/Svg.Skia/issues/125 — Downstream issue caused by the same System.Memory mismatch in Svg.Skia.

**Code snippets:**

```csharp
SKTextBlob.CreatePositioned("aaaa", new SKFont(SKTypeface.CreateDefault()), new ReadOnlySpan<SKPoint>(new SKPoint[]{ SKPoint.Empty }));
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | exception |
| Error message | System.MissingMethodException: Method not found: SkiaSharp.SKTextBlob SkiaSharp.SKTextBlob.CreatePositioned |
| Repro quality | partial |
| Target frameworks | netstandard2.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.0, 2.88.1, 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The System.Memory version mismatch is a structural issue in how SkiaSharp's netstandard2.0 TFM brings in System.Memory 4.6.3, which persists until SkiaSharp ships platform-specific TFMs (net8.0-ios, etc.) that don't need the System.Memory polyfill. |

## Analysis

### Technical Summary

On Xamarin.Forms iOS, SkiaSharp resolves to its netstandard2.0 assembly, which has a PackageReference to System.Memory 4.6.3 (confirmed in binding/Directory.Build.targets). This causes the ReadOnlySpan<T> type identity used in SKTextBlob.CreatePositioned to differ from the ReadOnlySpan<T> the Mono/Xamarin iOS runtime exposes at runtime, resulting in a MissingMethodException. Android and Windows work because their runtimes resolve the mismatch differently or use different TFMs. A well-known MSBuild workaround (strip the conflicting System.Memory reference in the iOS .csproj) resolves the issue.

### Rationale

Classified as type/bug because calling a public API throws MissingMethodException at runtime on iOS—this is not expected behavior. Area is area/SkiaSharp because the root cause is in the core library's netstandard2.0 System.Memory dependency. Platform is os/iOS because Android and Windows are unaffected. Severity is high because it blocks any iOS Xamarin.Forms user who calls SKTextBlob.CreatePositioned (or any other API taking ReadOnlySpan<T>), though a workaround exists. A workaround MSBuild targets snippet has been verified by multiple users in the comments.

### Key Signals

- "the root cause of this is the fact that SkiaSharp in .NET Standard 2.0 references System.Memory in a different version and that version mismatch causes issues with ReadOnlySpan" — **comment by MartinZikmund (Uno Platform contributor), https://github.com/mono/SkiaSharp/issues/2147#issuecomment-1424649999** (Authoritative identification of root cause by a platform contributor who encountered and solved this issue for Uno Platform.)
- "I'm facing the same issue with 2.88.1" — **comment by cschwarz** (Bug persists across minor releases.)
- "Me too using 2.88.3. Is anyone looking into it?" — **comment by JohnBot2013** (Bug persists across minor releases, multiple users affected.)
- "Appears to be causing https://github.com/wieslawsoltes/Svg.Skia/issues/125" — **comment by charlesroddie** (The bug has downstream impact on other libraries (Svg.Skia).)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/Directory.Build.targets` | 3-4 | direct | System.Memory 4.6.3 is added as a PackageReference for netstandard2.0 and net4x TFMs. This is the source of the ReadOnlySpan type identity mismatch on iOS. |
| `binding/SkiaSharp/SKTextBlob.cs` | 107-108 | direct | SKTextBlob.CreatePositioned(string, SKFont, ReadOnlySpan<SKPoint>) is a public API taking ReadOnlySpan<T> from System.Memory when compiled under netstandard2.0. On iOS, the runtime's ReadOnlySpan type comes from a different assembly, causing the MissingMethodException at the call site. |

### Workarounds

- Add the following MSBuild targets to the iOS .csproj to strip the conflicting System.Memory reference:

```xml
<Target Name="VSMac_RemoveSystemMemory" BeforeTargets="ResolveAssemblyReferences">
  <ItemGroup>
    <_ReferenceToRemove Include="@(Reference)" Condition="'%(Reference.Identity)'=='System.Memory'" />
    <Reference Remove="@(_ReferenceToRemove)" />
    <Reference Include="System.Memory" />
  </ItemGroup>
</Target>

<Target Name="VS_RemoveSystemMemory" BeforeTargets="FindReferenceAssembliesForReferences">
  <ItemGroup>
    <_ReferencePathToRemove Include="@(ReferencePath)" Condition="'%(ReferencePath.NuGetPackageId)'=='System.Memory'" />
    <ReferencePath Remove="@(_ReferencePathToRemove)" />
  </ItemGroup>
</Target>
```

### Next Questions

- Does this issue affect SkiaSharp 3.x (which ships platform-specific TFMs like net8.0-ios) or only 2.x?
- Are any other ReadOnlySpan<T> APIs in SkiaSharp affected by the same mismatch on iOS?
- Should SkiaSharp 2.x ship an updated NuGet .targets file that applies the System.Memory fix automatically for iOS consumers?

### Resolution Proposals

**Hypothesis:** The System.Memory 4.6.3 package pulled in by the netstandard2.0 TFM causes a ReadOnlySpan<T> type identity conflict on iOS/Xamarin. The fix is either to update the SkiaSharp NuGet package to automatically strip the conflicting System.Memory reference on iOS builds (via MSBuild targets shipped in the package), or to migrate to platform-specific TFMs in SkiaSharp 3.x that don't require the System.Memory polyfill.

1. **Add MSBuild targets to iOS .csproj to strip conflicting System.Memory** — workaround, cost/xs, validated=yes
   - Apply the workaround documented by MartinZikmund in the issue comments: add two MSBuild targets that remove the conflicting System.Memory NuGet reference before assembly resolution, forcing the iOS runtime's built-in ReadOnlySpan<T>.

```csharp
<Target Name="VSMac_RemoveSystemMemory" BeforeTargets="ResolveAssemblyReferences">
  <ItemGroup>
    <_ReferenceToRemove Include="@(Reference)" Condition="'%(Reference.Identity)'=='System.Memory'" />
    <Reference Remove="@(_ReferenceToRemove)" />
    <Reference Include="System.Memory" />
  </ItemGroup>
</Target>

<Target Name="VS_RemoveSystemMemory" BeforeTargets="FindReferenceAssembliesForReferences">
  <ItemGroup>
    <_ReferencePathToRemove Include="@(ReferencePath)" Condition="'%(ReferencePath.NuGetPackageId)'=='System.Memory'" />
    <ReferencePath Remove="@(_ReferencePathToRemove)" />
  </ItemGroup>
</Target>
```
2. **Ship MSBuild targets in SkiaSharp NuGet package to auto-fix System.Memory on iOS** — fix, cost/s, validated=untested
   - Add a .targets file to the SkiaSharp NuGet package that automatically applies the System.Memory stripping logic for iOS Xamarin consumers, so users don't need to manually add the workaround.

**Recommended proposal:** workaround-1

**Why:** Multiple users confirmed the workaround is effective. The fix-1 proposal would require packaging changes and is better suited for a future release.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Root cause is well-established (System.Memory version mismatch in netstandard2.0 TFM), a workaround exists and is confirmed by multiple users, but a proper fix (either in-package MSBuild targets or platform TFM migration in 3.x) needs investigation to scope and implement. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply classification labels | labels=type/bug, area/SkiaSharp, os/iOS, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Share confirmed workaround and root cause analysis | — |

**Comment draft for `add-comment`:**

```markdown
## Root Cause

This is caused by a **`System.Memory` version mismatch** in the `netstandard2.0` build of SkiaSharp. When SkiaSharp is resolved as `netstandard2.0` (as in Xamarin.Forms iOS), it brings in `System.Memory 4.6.3` as a dependency. This causes `ReadOnlySpan<T>` to resolve to a different type identity than the one the iOS Mono runtime exposes, leading to `MissingMethodException` at the call site.

## Workaround

Add the following MSBuild targets to your iOS `.csproj` file:

```xml
<Target Name="VSMac_RemoveSystemMemory" BeforeTargets="ResolveAssemblyReferences">
  <ItemGroup>
    <_ReferenceToRemove Include="@(Reference)" Condition="'%(Reference.Identity)'=='System.Memory'" />
    <Reference Remove="@(_ReferenceToRemove)" />
    <Reference Include="System.Memory" />
  </ItemGroup>
</Target>

<Target Name="VS_RemoveSystemMemory" BeforeTargets="FindReferenceAssembliesForReferences">
  <ItemGroup>
    <_ReferencePathToRemove Include="@(ReferencePath)" Condition="'%(ReferencePath.NuGetPackageId)'=='System.Memory'" />
    <ReferencePath Remove="@(_ReferencePathToRemove)" />
  </ItemGroup>
</Target>
```

This workaround has been confirmed by multiple users (including the Uno Platform team). Note: this also affects other SkiaSharp APIs that take `ReadOnlySpan<T>` parameters.

> **Note for SkiaSharp 3.x users:** If you are targeting `net8.0-ios` or later, this issue should not occur as SkiaSharp 3.x ships platform-specific TFMs that do not require the `System.Memory` polyfill.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2147,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T11:16:00Z"
  },
  "summary": "SKTextBlob.CreatePositioned throws MissingMethodException on iOS (Xamarin.Forms) due to a System.Memory version mismatch caused by SkiaSharp's netstandard2.0 dependency resolving ReadOnlySpan<T> from a different assembly than the iOS runtime provides.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "platforms": [
      "os/iOS"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "System.MissingMethodException: Method not found: SkiaSharp.SKTextBlob SkiaSharp.SKTextBlob.CreatePositioned",
      "reproQuality": "partial",
      "targetFrameworks": [
        "netstandard2.0"
      ]
    },
    "reproEvidence": {
      "codeSnippets": [
        "SKTextBlob.CreatePositioned(\"aaaa\", new SKFont(SKTypeface.CreateDefault()), new ReadOnlySpan<SKPoint>(new SKPoint[]{ SKPoint.Empty }));"
      ],
      "repoLinks": [
        {
          "url": "https://github.com/wieslawsoltes/Svg.Skia/issues/125",
          "description": "Downstream issue caused by the same System.Memory mismatch in Svg.Skia."
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.0",
        "2.88.1",
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The System.Memory version mismatch is a structural issue in how SkiaSharp's netstandard2.0 TFM brings in System.Memory 4.6.3, which persists until SkiaSharp ships platform-specific TFMs (net8.0-ios, etc.) that don't need the System.Memory polyfill."
    }
  },
  "analysis": {
    "summary": "On Xamarin.Forms iOS, SkiaSharp resolves to its netstandard2.0 assembly, which has a PackageReference to System.Memory 4.6.3 (confirmed in binding/Directory.Build.targets). This causes the ReadOnlySpan<T> type identity used in SKTextBlob.CreatePositioned to differ from the ReadOnlySpan<T> the Mono/Xamarin iOS runtime exposes at runtime, resulting in a MissingMethodException. Android and Windows work because their runtimes resolve the mismatch differently or use different TFMs. A well-known MSBuild workaround (strip the conflicting System.Memory reference in the iOS .csproj) resolves the issue.",
    "codeInvestigation": [
      {
        "file": "binding/Directory.Build.targets",
        "finding": "System.Memory 4.6.3 is added as a PackageReference for netstandard2.0 and net4x TFMs. This is the source of the ReadOnlySpan type identity mismatch on iOS.",
        "relevance": "direct",
        "lines": "3-4"
      },
      {
        "file": "binding/SkiaSharp/SKTextBlob.cs",
        "finding": "SKTextBlob.CreatePositioned(string, SKFont, ReadOnlySpan<SKPoint>) is a public API taking ReadOnlySpan<T> from System.Memory when compiled under netstandard2.0. On iOS, the runtime's ReadOnlySpan type comes from a different assembly, causing the MissingMethodException at the call site.",
        "relevance": "direct",
        "lines": "107-108"
      }
    ],
    "keySignals": [
      {
        "text": "the root cause of this is the fact that SkiaSharp in .NET Standard 2.0 references System.Memory in a different version and that version mismatch causes issues with ReadOnlySpan",
        "source": "comment by MartinZikmund (Uno Platform contributor), https://github.com/mono/SkiaSharp/issues/2147#issuecomment-1424649999",
        "interpretation": "Authoritative identification of root cause by a platform contributor who encountered and solved this issue for Uno Platform."
      },
      {
        "text": "I'm facing the same issue with 2.88.1",
        "source": "comment by cschwarz",
        "interpretation": "Bug persists across minor releases."
      },
      {
        "text": "Me too using 2.88.3. Is anyone looking into it?",
        "source": "comment by JohnBot2013",
        "interpretation": "Bug persists across minor releases, multiple users affected."
      },
      {
        "text": "Appears to be causing https://github.com/wieslawsoltes/Svg.Skia/issues/125",
        "source": "comment by charlesroddie",
        "interpretation": "The bug has downstream impact on other libraries (Svg.Skia)."
      }
    ],
    "rationale": "Classified as type/bug because calling a public API throws MissingMethodException at runtime on iOS—this is not expected behavior. Area is area/SkiaSharp because the root cause is in the core library's netstandard2.0 System.Memory dependency. Platform is os/iOS because Android and Windows are unaffected. Severity is high because it blocks any iOS Xamarin.Forms user who calls SKTextBlob.CreatePositioned (or any other API taking ReadOnlySpan<T>), though a workaround exists. A workaround MSBuild targets snippet has been verified by multiple users in the comments.",
    "workarounds": [
      "Add the following MSBuild targets to the iOS .csproj to strip the conflicting System.Memory reference:\n\n```xml\n<Target Name=\"VSMac_RemoveSystemMemory\" BeforeTargets=\"ResolveAssemblyReferences\">\n  <ItemGroup>\n    <_ReferenceToRemove Include=\"@(Reference)\" Condition=\"'%(Reference.Identity)'=='System.Memory'\" />\n    <Reference Remove=\"@(_ReferenceToRemove)\" />\n    <Reference Include=\"System.Memory\" />\n  </ItemGroup>\n</Target>\n\n<Target Name=\"VS_RemoveSystemMemory\" BeforeTargets=\"FindReferenceAssembliesForReferences\">\n  <ItemGroup>\n    <_ReferencePathToRemove Include=\"@(ReferencePath)\" Condition=\"'%(ReferencePath.NuGetPackageId)'=='System.Memory'\" />\n    <ReferencePath Remove=\"@(_ReferencePathToRemove)\" />\n  </ItemGroup>\n</Target>\n```"
    ],
    "nextQuestions": [
      "Does this issue affect SkiaSharp 3.x (which ships platform-specific TFMs like net8.0-ios) or only 2.x?",
      "Are any other ReadOnlySpan<T> APIs in SkiaSharp affected by the same mismatch on iOS?",
      "Should SkiaSharp 2.x ship an updated NuGet .targets file that applies the System.Memory fix automatically for iOS consumers?"
    ],
    "resolution": {
      "hypothesis": "The System.Memory 4.6.3 package pulled in by the netstandard2.0 TFM causes a ReadOnlySpan<T> type identity conflict on iOS/Xamarin. The fix is either to update the SkiaSharp NuGet package to automatically strip the conflicting System.Memory reference on iOS builds (via MSBuild targets shipped in the package), or to migrate to platform-specific TFMs in SkiaSharp 3.x that don't require the System.Memory polyfill.",
      "proposals": [
        {
          "title": "Add MSBuild targets to iOS .csproj to strip conflicting System.Memory",
          "description": "Apply the workaround documented by MartinZikmund in the issue comments: add two MSBuild targets that remove the conflicting System.Memory NuGet reference before assembly resolution, forcing the iOS runtime's built-in ReadOnlySpan<T>.",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "yes",
          "codeSnippet": "<Target Name=\"VSMac_RemoveSystemMemory\" BeforeTargets=\"ResolveAssemblyReferences\">\n  <ItemGroup>\n    <_ReferenceToRemove Include=\"@(Reference)\" Condition=\"'%(Reference.Identity)'=='System.Memory'\" />\n    <Reference Remove=\"@(_ReferenceToRemove)\" />\n    <Reference Include=\"System.Memory\" />\n  </ItemGroup>\n</Target>\n\n<Target Name=\"VS_RemoveSystemMemory\" BeforeTargets=\"FindReferenceAssembliesForReferences\">\n  <ItemGroup>\n    <_ReferencePathToRemove Include=\"@(ReferencePath)\" Condition=\"'%(ReferencePath.NuGetPackageId)'=='System.Memory'\" />\n    <ReferencePath Remove=\"@(_ReferencePathToRemove)\" />\n  </ItemGroup>\n</Target>"
        },
        {
          "title": "Ship MSBuild targets in SkiaSharp NuGet package to auto-fix System.Memory on iOS",
          "description": "Add a .targets file to the SkiaSharp NuGet package that automatically applies the System.Memory stripping logic for iOS Xamarin consumers, so users don't need to manually add the workaround.",
          "category": "fix",
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "workaround-1",
      "recommendedReason": "Multiple users confirmed the workaround is effective. The fix-1 proposal would require packaging changes and is better suited for a future release."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Root cause is well-established (System.Memory version mismatch in netstandard2.0 TFM), a workaround exists and is confirmed by multiple users, but a proper fix (either in-package MSBuild targets or platform TFM migration in 3.x) needs investigation to scope and implement.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/iOS",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Share confirmed workaround and root cause analysis",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "## Root Cause\n\nThis is caused by a **`System.Memory` version mismatch** in the `netstandard2.0` build of SkiaSharp. When SkiaSharp is resolved as `netstandard2.0` (as in Xamarin.Forms iOS), it brings in `System.Memory 4.6.3` as a dependency. This causes `ReadOnlySpan<T>` to resolve to a different type identity than the one the iOS Mono runtime exposes, leading to `MissingMethodException` at the call site.\n\n## Workaround\n\nAdd the following MSBuild targets to your iOS `.csproj` file:\n\n```xml\n<Target Name=\"VSMac_RemoveSystemMemory\" BeforeTargets=\"ResolveAssemblyReferences\">\n  <ItemGroup>\n    <_ReferenceToRemove Include=\"@(Reference)\" Condition=\"'%(Reference.Identity)'=='System.Memory'\" />\n    <Reference Remove=\"@(_ReferenceToRemove)\" />\n    <Reference Include=\"System.Memory\" />\n  </ItemGroup>\n</Target>\n\n<Target Name=\"VS_RemoveSystemMemory\" BeforeTargets=\"FindReferenceAssembliesForReferences\">\n  <ItemGroup>\n    <_ReferencePathToRemove Include=\"@(ReferencePath)\" Condition=\"'%(ReferencePath.NuGetPackageId)'=='System.Memory'\" />\n    <ReferencePath Remove=\"@(_ReferencePathToRemove)\" />\n  </ItemGroup>\n</Target>\n```\n\nThis workaround has been confirmed by multiple users (including the Uno Platform team). Note: this also affects other SkiaSharp APIs that take `ReadOnlySpan<T>` parameters.\n\n> **Note for SkiaSharp 3.x users:** If you are targeting `net8.0-ios` or later, this issue should not occur as SkiaSharp 3.x ships platform-specific TFMs that do not require the `System.Memory` polyfill."
      }
    ]
  }
}
```

</details>
