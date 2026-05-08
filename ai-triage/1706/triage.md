# Issue Triage Report — #1706

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T21:30:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | needs-info (0.88 (88%)) |

**Issue Summary:** TypeInitializationException on SKImageInfo when running SkiaSharp 2.80.2 in a Red Hat OpenShift container (ASP.NET Core 3.1 on Linux), likely caused by the native libSkiaSharp library failing to load or a missing shared library dependency.

**Analysis:** The TypeInitializationException is thrown because SKImageInfo's static constructor calls into the native libSkiaSharp library (sk_colortype_get_default_8888 via P/Invoke), and that P/Invoke fails because the native library either isn't present in the container output or one of its shared library dependencies (such as libfontconfig.so.1) is not installed. The reporter tried adding NativeAssets packages but the inner exception is not provided, making it impossible to confirm which failure mode applies.

**Recommendations:** **needs-info** — The inner exception is not provided, making it impossible to confirm whether the failure is a missing native binary, a missing shared library dependency, or a musl/glibc mismatch. Common container deployment patterns are well understood but the specific failure mode is unknown.

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

1. Deploy ASP.NET Core 3.1 application referencing SkiaSharp 2.80.2 to Red Hat OpenShift container
2. Execute any code that touches SKImageInfo (e.g., new SKBitmap(100, 100, SKImageInfo.PlatformColorType, SKAlphaType.Premul))
3. Observe TypeInitializationException on first SkiaSharp call

**Environment:** SkiaSharp 2.80.2, ASP.NET Core 3.1, Red Hat OpenShift container, Visual Studio Professional 2019 16.8.4

**Repository links:**
- https://github.com/RamarajMarimuthu/s2i-dotnetcore-ex/tree/SkApp — Reporter-provided OpenShift sample repro project

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | exception |
| Error message | The type initializer for 'SkiaSharp.SKImageInfo' threw an exception. |
| Repro quality | partial |
| Target frameworks | netcoreapp3.1 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | SkiaSharp 2.80.2 is old; current releases are 2.88.x and 3.x. The NativeAssets.Linux.NoDependencies package situation and container guidance has been significantly improved in later versions. However, the underlying pattern (native library not loading in a minimal Linux container) still applies to all versions. |

## Analysis

### Technical Summary

The TypeInitializationException is thrown because SKImageInfo's static constructor calls into the native libSkiaSharp library (sk_colortype_get_default_8888 via P/Invoke), and that P/Invoke fails because the native library either isn't present in the container output or one of its shared library dependencies (such as libfontconfig.so.1) is not installed. The reporter tried adding NativeAssets packages but the inner exception is not provided, making it impossible to confirm which failure mode applies.

### Rationale

Classified as type/bug in area/libSkiaSharp.native because the exception originates in native library loading on Linux. The SKImageInfo static constructor is the trigger but not the root cause. The root cause is the native binary not being available or its dependencies missing in the container. The reporter hasn't provided the inner exception, making exact diagnosis impossible, so needs-info is the appropriate action.

### Key Signals

- "The type initializer for 'SkiaSharp.SKImageInfo' threw an exception." — **issue body** (This is the CLR's wrapper exception when a static constructor throws. The root cause is almost always a DllNotFoundException from libSkiaSharp failing to load in the container.)
- "Already we have tried to resolve this issue by installing SkiaSharpNativeAssets.Linux, SkiaSharpNativeAssets.NoDependencies packages. But nothing helps." — **issue body** (Reporter added the native asset packages but the problem persists. The inner exception is not provided, so it's unknown whether: (a) the package is in the wrong project, (b) the container is musl-based and requires NoDependencies with musl support, or (c) there is a different root cause.)
- "#1715 seems very similar - perhaps a dupe (Windows not Linux)" — **comment by Peru-S** (Related issue for Windows; different platform but potentially same root cause pattern.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImageInfo.cs` | 46-56 | direct | SKImageInfo static constructor calls SkiaApi.sk_colortype_get_default_8888() and SkiaApi.sk_color_get_bit_shift() — both are P/Invoke calls into libSkiaSharp. If the native library fails to load (DllNotFoundException), the CLR wraps it in TypeInitializationException on first access to SKImageInfo. |
| `documentation/dev/packages.md` | 86-87 | direct | SkiaSharp.NativeAssets.Linux requires fontconfig (libfontconfig.so.1). SkiaSharp.NativeAssets.Linux.NoDependencies is the recommended package for minimal containers. Both must be referenced in the executable (application) project, not a library. Common failure: NativeAssets only added to a library project, so the binary isn't deployed to the container output. |

### Workarounds

- Use SkiaSharp.NativeAssets.Linux.NoDependencies in the executable project (not a library project) — avoids fontconfig dependency in minimal containers.
- Ensure the NativeAssets PackageReference is in the application (startup) project so the native binary is copied to the publish output.
- Run 'ldd /app/libSkiaSharp.so' in the container to identify the first missing dependency.
- For RHEL/UBI containers, fontconfig may need to be installed: RUN dnf install -y fontconfig.

### Next Questions

- What is the full inner exception and stack trace (the TypeInitializationException wraps the real error)?
- Is the NativeAssets package referenced in the executable/application project (not just a class library)?
- What is the base container image (RHEL, UBI minimal, etc.) — is it glibc or musl based?
- Is the published output a framework-dependent, self-contained, or single-file deployment?
- Does ldd on the libSkiaSharp.so in the publish output show any missing dependencies?

### Resolution Proposals

**Hypothesis:** The native libSkiaSharp.so is either not deployed into the container output (NativeAssets package in wrong project) or is present but has an unmet shared library dependency (fontconfig, or musl vs glibc mismatch).

1. **Provide inner exception and use NoDependencies in app project** — investigation, confidence 0.90 (90%), cost/xs, validated=untested
   - Ask reporter to share the full InnerException of the TypeInitializationException and confirm the NativeAssets package is referenced in the executable project. Provide the correct code snippet.
2. **Switch to NoDependencies and add to executable project** — workaround, confidence 0.82 (82%), cost/xs, validated=untested
   - Add SkiaSharp.NativeAssets.Linux.NoDependencies to the .csproj of the web application (not a shared library). This removes the fontconfig dependency and works in minimal RHEL/UBI containers.

```csharp
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="2.88.8" />
```

**Recommended proposal:** Provide inner exception and use NoDependencies in app project

**Why:** Without the inner exception we cannot confirm the exact root cause. The comment and deployment guidance should help the reporter diagnose and fix the issue themselves.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.88 (88%) |
| Reason | The inner exception is not provided, making it impossible to confirm whether the failure is a missing native binary, a missing shared library dependency, or a musl/glibc mismatch. Common container deployment patterns are well understood but the specific failure mode is unknown. |
| Suggested repro platform | linux |

### Missing Info

- Full inner exception and stack trace from the TypeInitializationException
- Whether SkiaSharp.NativeAssets.Linux.NoDependencies is referenced in the executable project vs a class library project
- Base container image name and version (RHEL UBI, Alpine, etc.)
- Output of 'ldd libSkiaSharp.so' in the container

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, native, linux labels | labels=type/bug, area/libSkiaSharp.native, os/Linux, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Ask for inner exception and container deployment details | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report. The `TypeInitializationException` on `SKImageInfo` is almost always caused by `libSkiaSharp` failing to load in the container — the static constructor P/Invokes into the native library, and if that fails, the CLR wraps it in a `TypeInitializationException`.

To help diagnose, could you provide:

1. **The full inner exception** — catch the exception and print `ex.InnerException?.ToString()` — the real error (usually a `DllNotFoundException` or `BadImageFormatException`) is in there.
2. **Which project** has the `SkiaSharp.NativeAssets.Linux.NoDependencies` reference — it must be in the **application project** (the one with the `Program.cs`/startup), not a class library project.

A common fix for OpenShift/Red Hat containers:

```xml
<!-- In your web application .csproj (not a library project) -->
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="2.88.8" />
```

If fontconfig errors appear in the inner exception, you can also install it in your Dockerfile:

```dockerfile
RUN dnf install -y fontconfig
```

You can verify which binary is loaded and what dependencies it needs by running `ldd /app/libSkiaSharp.so` inside the container.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1706,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T21:30:00Z"
  },
  "summary": "TypeInitializationException on SKImageInfo when running SkiaSharp 2.80.2 in a Red Hat OpenShift container (ASP.NET Core 3.1 on Linux), likely caused by the native libSkiaSharp library failing to load or a missing shared library dependency.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.9
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
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "The type initializer for 'SkiaSharp.SKImageInfo' threw an exception.",
      "reproQuality": "partial",
      "targetFrameworks": [
        "netcoreapp3.1"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Deploy ASP.NET Core 3.1 application referencing SkiaSharp 2.80.2 to Red Hat OpenShift container",
        "Execute any code that touches SKImageInfo (e.g., new SKBitmap(100, 100, SKImageInfo.PlatformColorType, SKAlphaType.Premul))",
        "Observe TypeInitializationException on first SkiaSharp call"
      ],
      "environmentDetails": "SkiaSharp 2.80.2, ASP.NET Core 3.1, Red Hat OpenShift container, Visual Studio Professional 2019 16.8.4",
      "repoLinks": [
        {
          "url": "https://github.com/RamarajMarimuthu/s2i-dotnetcore-ex/tree/SkApp",
          "description": "Reporter-provided OpenShift sample repro project"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "SkiaSharp 2.80.2 is old; current releases are 2.88.x and 3.x. The NativeAssets.Linux.NoDependencies package situation and container guidance has been significantly improved in later versions. However, the underlying pattern (native library not loading in a minimal Linux container) still applies to all versions."
    }
  },
  "analysis": {
    "summary": "The TypeInitializationException is thrown because SKImageInfo's static constructor calls into the native libSkiaSharp library (sk_colortype_get_default_8888 via P/Invoke), and that P/Invoke fails because the native library either isn't present in the container output or one of its shared library dependencies (such as libfontconfig.so.1) is not installed. The reporter tried adding NativeAssets packages but the inner exception is not provided, making it impossible to confirm which failure mode applies.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "46-56",
        "finding": "SKImageInfo static constructor calls SkiaApi.sk_colortype_get_default_8888() and SkiaApi.sk_color_get_bit_shift() — both are P/Invoke calls into libSkiaSharp. If the native library fails to load (DllNotFoundException), the CLR wraps it in TypeInitializationException on first access to SKImageInfo.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "86-87",
        "finding": "SkiaSharp.NativeAssets.Linux requires fontconfig (libfontconfig.so.1). SkiaSharp.NativeAssets.Linux.NoDependencies is the recommended package for minimal containers. Both must be referenced in the executable (application) project, not a library. Common failure: NativeAssets only added to a library project, so the binary isn't deployed to the container output.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "The type initializer for 'SkiaSharp.SKImageInfo' threw an exception.",
        "source": "issue body",
        "interpretation": "This is the CLR's wrapper exception when a static constructor throws. The root cause is almost always a DllNotFoundException from libSkiaSharp failing to load in the container."
      },
      {
        "text": "Already we have tried to resolve this issue by installing SkiaSharpNativeAssets.Linux, SkiaSharpNativeAssets.NoDependencies packages. But nothing helps.",
        "source": "issue body",
        "interpretation": "Reporter added the native asset packages but the problem persists. The inner exception is not provided, so it's unknown whether: (a) the package is in the wrong project, (b) the container is musl-based and requires NoDependencies with musl support, or (c) there is a different root cause."
      },
      {
        "text": "#1715 seems very similar - perhaps a dupe (Windows not Linux)",
        "source": "comment by Peru-S",
        "interpretation": "Related issue for Windows; different platform but potentially same root cause pattern."
      }
    ],
    "nextQuestions": [
      "What is the full inner exception and stack trace (the TypeInitializationException wraps the real error)?",
      "Is the NativeAssets package referenced in the executable/application project (not just a class library)?",
      "What is the base container image (RHEL, UBI minimal, etc.) — is it glibc or musl based?",
      "Is the published output a framework-dependent, self-contained, or single-file deployment?",
      "Does ldd on the libSkiaSharp.so in the publish output show any missing dependencies?"
    ],
    "workarounds": [
      "Use SkiaSharp.NativeAssets.Linux.NoDependencies in the executable project (not a library project) — avoids fontconfig dependency in minimal containers.",
      "Ensure the NativeAssets PackageReference is in the application (startup) project so the native binary is copied to the publish output.",
      "Run 'ldd /app/libSkiaSharp.so' in the container to identify the first missing dependency.",
      "For RHEL/UBI containers, fontconfig may need to be installed: RUN dnf install -y fontconfig."
    ],
    "rationale": "Classified as type/bug in area/libSkiaSharp.native because the exception originates in native library loading on Linux. The SKImageInfo static constructor is the trigger but not the root cause. The root cause is the native binary not being available or its dependencies missing in the container. The reporter hasn't provided the inner exception, making exact diagnosis impossible, so needs-info is the appropriate action.",
    "resolution": {
      "hypothesis": "The native libSkiaSharp.so is either not deployed into the container output (NativeAssets package in wrong project) or is present but has an unmet shared library dependency (fontconfig, or musl vs glibc mismatch).",
      "proposals": [
        {
          "title": "Provide inner exception and use NoDependencies in app project",
          "description": "Ask reporter to share the full InnerException of the TypeInitializationException and confirm the NativeAssets package is referenced in the executable project. Provide the correct code snippet.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Switch to NoDependencies and add to executable project",
          "description": "Add SkiaSharp.NativeAssets.Linux.NoDependencies to the .csproj of the web application (not a shared library). This removes the fontconfig dependency and works in minimal RHEL/UBI containers.",
          "codeSnippet": "<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"2.88.8\" />",
          "category": "workaround",
          "confidence": 0.82,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Provide inner exception and use NoDependencies in app project",
      "recommendedReason": "Without the inner exception we cannot confirm the exact root cause. The comment and deployment guidance should help the reporter diagnose and fix the issue themselves."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.88,
      "reason": "The inner exception is not provided, making it impossible to confirm whether the failure is a missing native binary, a missing shared library dependency, or a musl/glibc mismatch. Common container deployment patterns are well understood but the specific failure mode is unknown.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Full inner exception and stack trace from the TypeInitializationException",
      "Whether SkiaSharp.NativeAssets.Linux.NoDependencies is referenced in the executable project vs a class library project",
      "Base container image name and version (RHEL UBI, Alpine, etc.)",
      "Output of 'ldd libSkiaSharp.so' in the container"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, native, linux labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for inner exception and container deployment details",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the report. The `TypeInitializationException` on `SKImageInfo` is almost always caused by `libSkiaSharp` failing to load in the container — the static constructor P/Invokes into the native library, and if that fails, the CLR wraps it in a `TypeInitializationException`.\n\nTo help diagnose, could you provide:\n\n1. **The full inner exception** — catch the exception and print `ex.InnerException?.ToString()` — the real error (usually a `DllNotFoundException` or `BadImageFormatException`) is in there.\n2. **Which project** has the `SkiaSharp.NativeAssets.Linux.NoDependencies` reference — it must be in the **application project** (the one with the `Program.cs`/startup), not a class library project.\n\nA common fix for OpenShift/Red Hat containers:\n\n```xml\n<!-- In your web application .csproj (not a library project) -->\n<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"2.88.8\" />\n```\n\nIf fontconfig errors appear in the inner exception, you can also install it in your Dockerfile:\n\n```dockerfile\nRUN dnf install -y fontconfig\n```\n\nYou can verify which binary is loaded and what dependencies it needs by running `ldd /app/libSkiaSharp.so` inside the container."
      }
    ]
  }
}
```

</details>
