# Issue Triage Report — #1320

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T11:18:58Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.78 (78%)) |

**Issue Summary:** On .NET Framework 4.8 with SkiaSharp 1.68.3, calling SKPixmap.Encode() throws System.IO.FileLoadException because SkiaSharp was compiled against System.Buffers 4.0.2.0 but the runtime finds a different version (4.0.3.0+), caused by .NET Framework strict assembly version binding.

**Analysis:** SkiaSharp 1.68.3's NuGet package encoded a hard dependency on System.Buffers 4.0.2.0 in its assembly references. On .NET Framework, the CLR checks assembly identity strictly and rejects 4.0.3.0 as a substitute unless assembly binding redirects are configured. The reporter's project had SixLabors.ImageSharp pulling in System.Buffers 4.0.3.0, causing the mismatch at runtime when SKManagedWStream.OnWrite tried to use ArrayPool<T>.

**Recommendations:** **needs-investigation** — The issue is clearly reproducible in 1.68.3 and a workaround exists. However, the issue is from 2020 and current SkiaSharp has changed its dependency structure. Repro against current version (3.x) is needed to determine if the root cause still exists.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create a .NET 4.8 unit test project
2. Reference SkiaSharp 1.68.3 and SixLabors.ImageSharp (which pulls System.Buffers 4.0.3.0)
3. Call SKPixmap.Encode() saving as JPEG or PNG
4. Observe System.IO.FileLoadException for System.Buffers Version=4.0.2.0

**Environment:** .NET Framework 4.8, Windows 10 2004, Visual Studio 2019, SkiaSharp 1.68.3

**Repository links:**
- https://github.com/dotnet/runtime/issues/27117 — Upstream dotnet/runtime issue about System.Buffers version conflicts on .NET Framework

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | System.IO.FileLoadException: Could not load file or assembly 'System.Buffers, Version=4.0.2.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference. |
| Repro quality | partial |
| Target frameworks | net48 |

**Stack trace:**

```text
at SkiaSharp.SKManagedWStream.OnWrite(IntPtr buffer, IntPtr size)
   at SkiaSharp.SKAbstractManagedWStream.WriteInternal(IntPtr s, Void* context, Void* buffer, IntPtr size)
   at SkiaSharp.SkiaApi.sk_jpegencoder_encode(IntPtr dst, IntPtr src, SKJpegEncoderOptions* options)
   at SkiaSharp.SKPixmap.Encode(SKWStream dst, SKJpegEncoderOptions options)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Issue filed against 1.68.3 (2020). Current codebase still uses ArrayPool<T> from System.Buffers in Util.cs and SKManagedWStream.cs, but the SkiaSharp.csproj no longer has an explicit System.Buffers PackageReference. Unclear whether the NuGet package metadata still locks to a specific version. |

## Analysis

### Technical Summary

SkiaSharp 1.68.3's NuGet package encoded a hard dependency on System.Buffers 4.0.2.0 in its assembly references. On .NET Framework, the CLR checks assembly identity strictly and rejects 4.0.3.0 as a substitute unless assembly binding redirects are configured. The reporter's project had SixLabors.ImageSharp pulling in System.Buffers 4.0.3.0, causing the mismatch at runtime when SKManagedWStream.OnWrite tried to use ArrayPool<T>.

### Rationale

Stack trace clearly points to SKManagedWStream.OnWrite failing at load time due to a .NET Framework assembly version mismatch, not a logic error. The HRESULT 0x80131040 (COR_E_FILELOAD / FUSLOGVW manifest mismatch) is characteristic of this class of problem. The current codebase still uses System.Buffers in the same code path (Util.cs/ArrayPool<T>.Shared.Rent), so the issue may still affect .NET Framework consumers if the NuGet package metadata has not been updated.

### Key Signals

- "System.IO.FileLoadException: Could not load file or assembly 'System.Buffers, Version=4.0.2.0' ... manifest definition does not match the assembly reference" — **issue body** (.NET Framework CLR found System.Buffers with a different assembly version than SkiaSharp was compiled against. Binding redirects would normally fix this but apparently did not take effect.)
- "the System.Buffers assembly in my build directory has assembly version 4.0.3.0, so it is very strange that this minor revision is not picked up automatically" — **comment by ziriax** (Confirms the mismatch direction: user has 4.0.3.0, SkiaSharp embedded ref to 4.0.2.0. Unlike .NET Core, .NET Framework does not auto-redirect minor assembly versions.)
- "My issue turned out to be a corrupted dll in my NuGet cache... I had to remove the NuGet package, delete all bin folders, delete NuGet Cache. Then re-add buffers 4.5.1. It finally worked." — **comment by erebuswolf (2023)** (A separate root cause — corrupted cache — mimics the same error. Suggests issue body and later comments describe distinct problems with the same symptom.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKManagedWStream.cs` | 4,45-55 | direct | File imports System.Buffers (line 4) and uses Utils.RentArray<byte>(count) in OnWrite — this is the exact method in the reporter's stack trace. The ArrayPool<T> call will fail if System.Buffers assembly cannot be loaded. |
| `binding/SkiaSharp/Util.cs` | 4,77-103 | direct | RentedArray<T> struct directly calls ArrayPool<T>.Shared.Rent(length) and ArrayPool<T>.Shared.Return(Array). System.Buffers is a required assembly for this code path. The csproj does NOT list System.Buffers as an explicit PackageReference, meaning in current SkiaSharp it likely arrives as a transitive dependency. |
| `binding/SkiaSharp/SkiaSharp.csproj` | — | related | No explicit PackageReference for System.Buffers found. The project uses $(AllTargetFrameworks) and has net4x-specific conditions. Without an explicit System.Buffers reference, newer NuGet resolution may handle version selection differently than 1.68.3. |

### Workarounds

- Add AppDomain.CurrentDomain.AssemblyResolve handler to redirect System.Buffers 4.0.2.0 requests to the installed version (provided by reporter).
- Add or correct assembly binding redirects in App.config/packages.config for System.Buffers to cover the range up to 4.0.3.0 or higher.
- Clear the NuGet cache (`%USERPROFILE%\.nuget\packages\system.buffers`) and reinstall System.Buffers 4.5.1 explicitly in the project.

### Next Questions

- Does this error still occur with current SkiaSharp (2.88.x or 3.x) on .NET Framework 4.8?
- Does the SkiaSharp NuGet package still emit a System.Buffers version-pinned dependency in its nuspec for .NET Framework targets?
- Does adding an explicit <PackageReference Include='System.Buffers' Version='4.5.1'/> to the project resolve the issue without the AssemblyResolve hack?

### Resolution Proposals

**Hypothesis:** SkiaSharp 1.68.3's NuGet package encoded System.Buffers 4.0.2.0 in its dependency metadata, and no binding redirect was generated to handle the version difference. Current SkiaSharp may have improved handling through updated NuGet metadata or transitive resolution.

1. **AssemblyResolve workaround (immediate)** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Register an AppDomain.AssemblyResolve handler that redirects System.Buffers 4.0.2.0 requests to the installed version. This is the reporter's own confirmed workaround.

```csharp
AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
{
    switch (args.Name)
    {
        case "System.Buffers, Version=4.0.2.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51":
            return typeof(System.Buffers.ArrayPool<>).Assembly;
        default:
            return null;
    }
};
```
2. **Explicit System.Buffers package reference** — workaround, confidence 0.75 (75%), cost/xs, validated=untested
   - Add System.Buffers 4.5.1 as a direct PackageReference in the consuming project. This forces NuGet to resolve the highest version and should generate the correct binding redirects.

```csharp
<PackageReference Include="System.Buffers" Version="4.5.1" />
```
3. **Investigate whether current SkiaSharp still requires version-pinned System.Buffers** — investigation, confidence 0.70 (70%), cost/s, validated=untested
   - Examine the generated NuGet .nupkg for the .NET Framework target to confirm whether System.Buffers is listed as a versioned dependency. If so, update the NuGet metadata to allow a range (e.g., [4.0.1,)) instead of an exact version.

**Recommended proposal:** AssemblyResolve workaround (immediate)

**Why:** Confirmed working by the original reporter. The investigation proposal is worth pursuing to determine if the underlying issue is fixed in newer SkiaSharp.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.78 (78%) |
| Reason | The issue is clearly reproducible in 1.68.3 and a workaround exists. However, the issue is from 2020 and current SkiaSharp has changed its dependency structure. Repro against current version (3.x) is needed to determine if the root cause still exists. |
| Suggested repro platform | windows |

### Missing Info

- Standalone minimal repro project (reporter acknowledged this was not ready yet)
- Confirmation whether the issue reproduces with current SkiaSharp (2.88.x / 3.x) on .NET Framework 4.8

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/bug, area/SkiaSharp, os/Windows-Classic, tenet/compatibility | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/compatibility |
| add-comment | medium | 0.78 (78%) | Acknowledge the issue, provide confirmed workaround, ask for repro against current version | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and for providing the `AssemblyResolve` workaround.

This is a classic .NET Framework assembly binding issue: SkiaSharp 1.68.3 was compiled against `System.Buffers 4.0.2.0` and the CLR won't automatically redirect to `4.0.3.0`.

**Confirmed workaround (from issue body):** Register an `AssemblyResolve` handler as shown.

**Alternative — add an explicit package reference:**
```xml
<PackageReference Include="System.Buffers" Version="4.5.1" />
```
This forces NuGet to select a higher version and should generate the correct binding redirects in `app.config`.

Since this was filed against 1.68.3, could you or anyone on this thread confirm whether the issue still reproduces with the current SkiaSharp (3.x) on .NET Framework 4.8? That would help us determine whether a fix is still needed in the NuGet package metadata.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1320,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T11:18:58Z"
  },
  "summary": "On .NET Framework 4.8 with SkiaSharp 1.68.3, calling SKPixmap.Encode() throws System.IO.FileLoadException because SkiaSharp was compiled against System.Buffers 4.0.2.0 but the runtime finds a different version (4.0.3.0+), caused by .NET Framework strict assembly version binding.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "System.IO.FileLoadException: Could not load file or assembly 'System.Buffers, Version=4.0.2.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference.",
      "stackTrace": "at SkiaSharp.SKManagedWStream.OnWrite(IntPtr buffer, IntPtr size)\n   at SkiaSharp.SKAbstractManagedWStream.WriteInternal(IntPtr s, Void* context, Void* buffer, IntPtr size)\n   at SkiaSharp.SkiaApi.sk_jpegencoder_encode(IntPtr dst, IntPtr src, SKJpegEncoderOptions* options)\n   at SkiaSharp.SKPixmap.Encode(SKWStream dst, SKJpegEncoderOptions options)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net48"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET 4.8 unit test project",
        "Reference SkiaSharp 1.68.3 and SixLabors.ImageSharp (which pulls System.Buffers 4.0.3.0)",
        "Call SKPixmap.Encode() saving as JPEG or PNG",
        "Observe System.IO.FileLoadException for System.Buffers Version=4.0.2.0"
      ],
      "environmentDetails": ".NET Framework 4.8, Windows 10 2004, Visual Studio 2019, SkiaSharp 1.68.3",
      "repoLinks": [
        {
          "url": "https://github.com/dotnet/runtime/issues/27117",
          "description": "Upstream dotnet/runtime issue about System.Buffers version conflicts on .NET Framework"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.3"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Issue filed against 1.68.3 (2020). Current codebase still uses ArrayPool<T> from System.Buffers in Util.cs and SKManagedWStream.cs, but the SkiaSharp.csproj no longer has an explicit System.Buffers PackageReference. Unclear whether the NuGet package metadata still locks to a specific version."
    }
  },
  "analysis": {
    "summary": "SkiaSharp 1.68.3's NuGet package encoded a hard dependency on System.Buffers 4.0.2.0 in its assembly references. On .NET Framework, the CLR checks assembly identity strictly and rejects 4.0.3.0 as a substitute unless assembly binding redirects are configured. The reporter's project had SixLabors.ImageSharp pulling in System.Buffers 4.0.3.0, causing the mismatch at runtime when SKManagedWStream.OnWrite tried to use ArrayPool<T>.",
    "rationale": "Stack trace clearly points to SKManagedWStream.OnWrite failing at load time due to a .NET Framework assembly version mismatch, not a logic error. The HRESULT 0x80131040 (COR_E_FILELOAD / FUSLOGVW manifest mismatch) is characteristic of this class of problem. The current codebase still uses System.Buffers in the same code path (Util.cs/ArrayPool<T>.Shared.Rent), so the issue may still affect .NET Framework consumers if the NuGet package metadata has not been updated.",
    "keySignals": [
      {
        "text": "System.IO.FileLoadException: Could not load file or assembly 'System.Buffers, Version=4.0.2.0' ... manifest definition does not match the assembly reference",
        "source": "issue body",
        "interpretation": ".NET Framework CLR found System.Buffers with a different assembly version than SkiaSharp was compiled against. Binding redirects would normally fix this but apparently did not take effect."
      },
      {
        "text": "the System.Buffers assembly in my build directory has assembly version 4.0.3.0, so it is very strange that this minor revision is not picked up automatically",
        "source": "comment by ziriax",
        "interpretation": "Confirms the mismatch direction: user has 4.0.3.0, SkiaSharp embedded ref to 4.0.2.0. Unlike .NET Core, .NET Framework does not auto-redirect minor assembly versions."
      },
      {
        "text": "My issue turned out to be a corrupted dll in my NuGet cache... I had to remove the NuGet package, delete all bin folders, delete NuGet Cache. Then re-add buffers 4.5.1. It finally worked.",
        "source": "comment by erebuswolf (2023)",
        "interpretation": "A separate root cause — corrupted cache — mimics the same error. Suggests issue body and later comments describe distinct problems with the same symptom."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKManagedWStream.cs",
        "lines": "4,45-55",
        "finding": "File imports System.Buffers (line 4) and uses Utils.RentArray<byte>(count) in OnWrite — this is the exact method in the reporter's stack trace. The ArrayPool<T> call will fail if System.Buffers assembly cannot be loaded.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/Util.cs",
        "lines": "4,77-103",
        "finding": "RentedArray<T> struct directly calls ArrayPool<T>.Shared.Rent(length) and ArrayPool<T>.Shared.Return(Array). System.Buffers is a required assembly for this code path. The csproj does NOT list System.Buffers as an explicit PackageReference, meaning in current SkiaSharp it likely arrives as a transitive dependency.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaSharp.csproj",
        "finding": "No explicit PackageReference for System.Buffers found. The project uses $(AllTargetFrameworks) and has net4x-specific conditions. Without an explicit System.Buffers reference, newer NuGet resolution may handle version selection differently than 1.68.3.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Add AppDomain.CurrentDomain.AssemblyResolve handler to redirect System.Buffers 4.0.2.0 requests to the installed version (provided by reporter).",
      "Add or correct assembly binding redirects in App.config/packages.config for System.Buffers to cover the range up to 4.0.3.0 or higher.",
      "Clear the NuGet cache (`%USERPROFILE%\\.nuget\\packages\\system.buffers`) and reinstall System.Buffers 4.5.1 explicitly in the project."
    ],
    "nextQuestions": [
      "Does this error still occur with current SkiaSharp (2.88.x or 3.x) on .NET Framework 4.8?",
      "Does the SkiaSharp NuGet package still emit a System.Buffers version-pinned dependency in its nuspec for .NET Framework targets?",
      "Does adding an explicit <PackageReference Include='System.Buffers' Version='4.5.1'/> to the project resolve the issue without the AssemblyResolve hack?"
    ],
    "resolution": {
      "hypothesis": "SkiaSharp 1.68.3's NuGet package encoded System.Buffers 4.0.2.0 in its dependency metadata, and no binding redirect was generated to handle the version difference. Current SkiaSharp may have improved handling through updated NuGet metadata or transitive resolution.",
      "proposals": [
        {
          "title": "AssemblyResolve workaround (immediate)",
          "description": "Register an AppDomain.AssemblyResolve handler that redirects System.Buffers 4.0.2.0 requests to the installed version. This is the reporter's own confirmed workaround.",
          "category": "workaround",
          "codeSnippet": "AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>\n{\n    switch (args.Name)\n    {\n        case \"System.Buffers, Version=4.0.2.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51\":\n            return typeof(System.Buffers.ArrayPool<>).Assembly;\n        default:\n            return null;\n    }\n};",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Explicit System.Buffers package reference",
          "description": "Add System.Buffers 4.5.1 as a direct PackageReference in the consuming project. This forces NuGet to resolve the highest version and should generate the correct binding redirects.",
          "category": "workaround",
          "codeSnippet": "<PackageReference Include=\"System.Buffers\" Version=\"4.5.1\" />",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate whether current SkiaSharp still requires version-pinned System.Buffers",
          "description": "Examine the generated NuGet .nupkg for the .NET Framework target to confirm whether System.Buffers is listed as a versioned dependency. If so, update the NuGet metadata to allow a range (e.g., [4.0.1,)) instead of an exact version.",
          "category": "investigation",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "AssemblyResolve workaround (immediate)",
      "recommendedReason": "Confirmed working by the original reporter. The investigation proposal is worth pursuing to determine if the underlying issue is fixed in newer SkiaSharp."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.78,
      "reason": "The issue is clearly reproducible in 1.68.3 and a workaround exists. However, the issue is from 2020 and current SkiaSharp has changed its dependency structure. Repro against current version (3.x) is needed to determine if the root cause still exists.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Standalone minimal repro project (reporter acknowledged this was not ready yet)",
      "Confirmation whether the issue reproduces with current SkiaSharp (2.88.x / 3.x) on .NET Framework 4.8"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Windows-Classic, tenet/compatibility",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the issue, provide confirmed workaround, ask for repro against current version",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for the detailed report and for providing the `AssemblyResolve` workaround.\n\nThis is a classic .NET Framework assembly binding issue: SkiaSharp 1.68.3 was compiled against `System.Buffers 4.0.2.0` and the CLR won't automatically redirect to `4.0.3.0`.\n\n**Confirmed workaround (from issue body):** Register an `AssemblyResolve` handler as shown.\n\n**Alternative — add an explicit package reference:**\n```xml\n<PackageReference Include=\"System.Buffers\" Version=\"4.5.1\" />\n```\nThis forces NuGet to select a higher version and should generate the correct binding redirects in `app.config`.\n\nSince this was filed against 1.68.3, could you or anyone on this thread confirm whether the issue still reproduces with the current SkiaSharp (3.x) on .NET Framework 4.8? That would help us determine whether a fix is still needed in the NuGet package metadata."
      }
    ]
  }
}
```

</details>
