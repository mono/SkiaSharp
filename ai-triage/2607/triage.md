# Issue Triage Report — #2607

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T08:49:12Z |
| Type | type/question (0.82 (82%)) |
| Area | area/libSkiaSharp.native (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** Reporter gets TypeInitializationException for SKData when deploying a .NET 6 app to GCP Linux Docker container; root cause is conflicting NativeAssets.Linux and NativeAssets.Linux.NoDependencies package references that cause libSkiaSharp.so to fail loading.

**Analysis:** The TypeInitializationException for SKData wraps a DllNotFoundException for libSkiaSharp.so. The reporter has both NativeAssets.Linux (which requires fontconfig) and NativeAssets.Linux.NoDependencies referenced simultaneously; in a minimal Docker container without fontconfig the font-dependent binary cannot load. Multiple community members confirmed the fix: use only NoDependencies, or keep NativeAssets.Linux and install libfontconfig1/libfreetype6 in the Dockerfile. This is a deployment configuration question, not a SkiaSharp code bug.

**Recommendations:** **close-as-not-a-bug** — The error is a packaging/deployment configuration issue — not a SkiaSharp code defect. NativeAssets.Linux requiring fontconfig is by-design and documented. Confirmed workarounds exist from multiple community members.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create an ASP.NET Core .NET 6 app using SKImage/SKBitmap for image resizing
2. Add both SkiaSharp.NativeAssets.Linux and SkiaSharp.NativeAssets.Linux.NoDependencies (plus unnecessary Views.Blazor and NativeAssets.WebAssembly)
3. Deploy to GCP Container Registry via Docker
4. Invoke any SkiaSharp API — TypeInitializationException for SKData is thrown on first use

**Environment:** GCP Container Registry (Docker/Linux), .NET 6, SkiaSharp 2.88.5, Windows 10 local machine works fine

**Related issues:** #1341, #2215, #2124

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.5 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The native asset selection and fontconfig dependency behavior has not changed fundamentally in recent releases; the same packaging guidance applies. |

## Analysis

### Technical Summary

The TypeInitializationException for SKData wraps a DllNotFoundException for libSkiaSharp.so. The reporter has both NativeAssets.Linux (which requires fontconfig) and NativeAssets.Linux.NoDependencies referenced simultaneously; in a minimal Docker container without fontconfig the font-dependent binary cannot load. Multiple community members confirmed the fix: use only NoDependencies, or keep NativeAssets.Linux and install libfontconfig1/libfreetype6 in the Dockerfile. This is a deployment configuration question, not a SkiaSharp code bug.

### Rationale

SKData.cctor() is the first P/Invoke entry point; DllNotFoundException from a missing/unloadable libSkiaSharp.so always surfaces as TypeInitializationException on SKData. The maintainer confirmed this in the first comment. Having conflicting NativeAssets packages is a well-known configuration mistake documented in packages.md. Community members confirmed two working workarounds. Classifying as type/question and suggesting close-as-not-a-bug because the behavior is by-design and a confirmed answer exists.

### Key Signals

- "The type initializer for 'SkiaSharp.SKData' threw an exception." — **issue body** (SKData static constructor fails due to DllNotFoundException — libSkiaSharp.so cannot be loaded in the container.)
- "this exception is saying it was unable to load the libSkiaSharp native binary" — **comment by mattleibow** (Maintainer confirmed root cause: native binary load failure, not a code bug.)
- "SkiaSharp.NativeAssets.Linux (2.88.5) ... SkiaSharp.NativeAssets.Linux.NoDependencies (2.88.5)" — **issue body** (Both conflicting packages are referenced simultaneously — the fontconfig-dependent binary from NativeAssets.Linux may take precedence in the container where fontconfig is absent.)
- "This worked for me after adding missing 2 libs libfontconfig1 libfreetype6 into the DockerFile." — **comment by atakane (confirmed by fjpging8908)** (Community-confirmed workaround: NoDependencies + optional fontconfig/freetype packages in Dockerfile resolves the issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKData.cs` | 20-28 | direct | Static constructor calls SkiaApi.sk_data_new_empty() as the first P/Invoke entry point into libSkiaSharp. When the native library fails to load, the runtime wraps the DllNotFoundException in a TypeInitializationException for SKData — precisely the error reported. |
| `documentation/dev/packages.md` | 86-87 | direct | Documents the difference: NativeAssets.Linux requires libfontconfig.so.1 for system font enumeration; NativeAssets.Linux.NoDependencies has zero external dependencies (only libc/libm/libpthread/libdl). Mixing both packages can deploy the fontconfig-dependent binary in a minimal container that lacks fontconfig. |

### Workarounds

- Remove SkiaSharp.NativeAssets.Linux and keep only SkiaSharp.NativeAssets.Linux.NoDependencies — add it as a PackageReference in the executable project (not a library project).
- Alternatively, keep SkiaSharp.NativeAssets.Linux and add 'RUN apt-get update && apt-get install -y libfontconfig1 libfreetype6 && rm -rf /var/lib/apt/lists/*' to your Dockerfile.
- Remove unnecessary packages: SkiaSharp.NativeAssets.WebAssembly and SkiaSharp.Views.Blazor are not needed for a server-side Linux app.

### Resolution Proposals

**Hypothesis:** Having both NativeAssets.Linux (fontconfig-dependent) and NoDependencies referenced together causes the wrong binary to be deployed in a minimal Docker container. The fontconfig-dependent binary then fails to load, producing a TypeInitializationException wrapped DllNotFoundException.

1. **Use NoDependencies only** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Remove SkiaSharp.NativeAssets.Linux and keep only SkiaSharp.NativeAssets.Linux.NoDependencies referenced in the executable project. Also remove NativeAssets.WebAssembly and Views.Blazor which are irrelevant for a server-side Linux app.

```csharp
<PackageReference Include="SkiaSharp" Version="2.88.5" />
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="2.88.5" />
```
2. **Keep NativeAssets.Linux and install system dependencies in Dockerfile** — alternative, confidence 0.90 (90%), cost/xs, validated=yes
   - Remove NoDependencies, keep NativeAssets.Linux, and install libfontconfig1 and libfreetype6 in the Dockerfile. Choose this if system font enumeration via SKFontManager is required.

```csharp
# In Dockerfile:
RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    libfreetype6 \
    && rm -rf /var/lib/apt/lists/*
```

**Recommended proposal:** Use NoDependencies only

**Why:** NoDependencies is specifically designed for minimal containers with zero system dependencies. Multiple community members confirmed it resolves the issue.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | The error is a packaging/deployment configuration issue — not a SkiaSharp code defect. NativeAssets.Linux requiring fontconfig is by-design and documented. Confirmed workarounds exist from multiple community members. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Correct type label from bug to question; add area and platform labels | labels=type/question, area/libSkiaSharp.native, os/Linux, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Post answer explaining the packaging conflict and confirmed workarounds | — |
| close-issue | medium | 0.80 (80%) | Close as not a bug — deployment configuration question with confirmed community answer | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report. This is a packaging configuration issue rather than a SkiaSharp code bug.

The `TypeInitializationException` for `SKData` wraps a `DllNotFoundException` — `libSkiaSharp.so` cannot be loaded in your Docker container. You have **both** `SkiaSharp.NativeAssets.Linux` (which requires `libfontconfig.so.1`) **and** `SkiaSharp.NativeAssets.Linux.NoDependencies` referenced. In a minimal Docker image without fontconfig, having both packages can cause the fontconfig-dependent binary to be deployed, which then fails to load.

**Option 1 (recommended): Use only `NoDependencies`**

In your executable project's `.csproj`:
```xml
<PackageReference Include="SkiaSharp" Version="2.88.5" />
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="2.88.5" />
```

Also remove `SkiaSharp.NativeAssets.WebAssembly` and `SkiaSharp.Views.Blazor` — these are only needed for Blazor/WASM apps, not server-side Linux apps.

**Option 2: Keep `NativeAssets.Linux` and install system deps in Dockerfile**

Choose this if you need system font enumeration via `SKFontManager`:
```dockerfile
RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    libfreetype6 \
    && rm -rf /var/lib/apt/lists/*
```

> ⚠️ Make sure the `NativeAssets` package is referenced in the **executable** project (the `.csproj` that produces the Docker image output), not in a class library project.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2607,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T08:49:12Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter gets TypeInitializationException for SKData when deploying a .NET 6 app to GCP Linux Docker container; root cause is conflicting NativeAssets.Linux and NativeAssets.Linux.NoDependencies package references that cause libSkiaSharp.so to fail loading.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.82
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.92
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an ASP.NET Core .NET 6 app using SKImage/SKBitmap for image resizing",
        "Add both SkiaSharp.NativeAssets.Linux and SkiaSharp.NativeAssets.Linux.NoDependencies (plus unnecessary Views.Blazor and NativeAssets.WebAssembly)",
        "Deploy to GCP Container Registry via Docker",
        "Invoke any SkiaSharp API — TypeInitializationException for SKData is thrown on first use"
      ],
      "environmentDetails": "GCP Container Registry (Docker/Linux), .NET 6, SkiaSharp 2.88.5, Windows 10 local machine works fine",
      "relatedIssues": [
        1341,
        2215,
        2124
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.5"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The native asset selection and fontconfig dependency behavior has not changed fundamentally in recent releases; the same packaging guidance applies."
    }
  },
  "analysis": {
    "summary": "The TypeInitializationException for SKData wraps a DllNotFoundException for libSkiaSharp.so. The reporter has both NativeAssets.Linux (which requires fontconfig) and NativeAssets.Linux.NoDependencies referenced simultaneously; in a minimal Docker container without fontconfig the font-dependent binary cannot load. Multiple community members confirmed the fix: use only NoDependencies, or keep NativeAssets.Linux and install libfontconfig1/libfreetype6 in the Dockerfile. This is a deployment configuration question, not a SkiaSharp code bug.",
    "rationale": "SKData.cctor() is the first P/Invoke entry point; DllNotFoundException from a missing/unloadable libSkiaSharp.so always surfaces as TypeInitializationException on SKData. The maintainer confirmed this in the first comment. Having conflicting NativeAssets packages is a well-known configuration mistake documented in packages.md. Community members confirmed two working workarounds. Classifying as type/question and suggesting close-as-not-a-bug because the behavior is by-design and a confirmed answer exists.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKData.cs",
        "lines": "20-28",
        "finding": "Static constructor calls SkiaApi.sk_data_new_empty() as the first P/Invoke entry point into libSkiaSharp. When the native library fails to load, the runtime wraps the DllNotFoundException in a TypeInitializationException for SKData — precisely the error reported.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "86-87",
        "finding": "Documents the difference: NativeAssets.Linux requires libfontconfig.so.1 for system font enumeration; NativeAssets.Linux.NoDependencies has zero external dependencies (only libc/libm/libpthread/libdl). Mixing both packages can deploy the fontconfig-dependent binary in a minimal container that lacks fontconfig.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "The type initializer for 'SkiaSharp.SKData' threw an exception.",
        "source": "issue body",
        "interpretation": "SKData static constructor fails due to DllNotFoundException — libSkiaSharp.so cannot be loaded in the container."
      },
      {
        "text": "this exception is saying it was unable to load the libSkiaSharp native binary",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer confirmed root cause: native binary load failure, not a code bug."
      },
      {
        "text": "SkiaSharp.NativeAssets.Linux (2.88.5) ... SkiaSharp.NativeAssets.Linux.NoDependencies (2.88.5)",
        "source": "issue body",
        "interpretation": "Both conflicting packages are referenced simultaneously — the fontconfig-dependent binary from NativeAssets.Linux may take precedence in the container where fontconfig is absent."
      },
      {
        "text": "This worked for me after adding missing 2 libs libfontconfig1 libfreetype6 into the DockerFile.",
        "source": "comment by atakane (confirmed by fjpging8908)",
        "interpretation": "Community-confirmed workaround: NoDependencies + optional fontconfig/freetype packages in Dockerfile resolves the issue."
      }
    ],
    "workarounds": [
      "Remove SkiaSharp.NativeAssets.Linux and keep only SkiaSharp.NativeAssets.Linux.NoDependencies — add it as a PackageReference in the executable project (not a library project).",
      "Alternatively, keep SkiaSharp.NativeAssets.Linux and add 'RUN apt-get update && apt-get install -y libfontconfig1 libfreetype6 && rm -rf /var/lib/apt/lists/*' to your Dockerfile.",
      "Remove unnecessary packages: SkiaSharp.NativeAssets.WebAssembly and SkiaSharp.Views.Blazor are not needed for a server-side Linux app."
    ],
    "resolution": {
      "hypothesis": "Having both NativeAssets.Linux (fontconfig-dependent) and NoDependencies referenced together causes the wrong binary to be deployed in a minimal Docker container. The fontconfig-dependent binary then fails to load, producing a TypeInitializationException wrapped DllNotFoundException.",
      "proposals": [
        {
          "title": "Use NoDependencies only",
          "description": "Remove SkiaSharp.NativeAssets.Linux and keep only SkiaSharp.NativeAssets.Linux.NoDependencies referenced in the executable project. Also remove NativeAssets.WebAssembly and Views.Blazor which are irrelevant for a server-side Linux app.",
          "category": "workaround",
          "codeSnippet": "<PackageReference Include=\"SkiaSharp\" Version=\"2.88.5\" />\n<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"2.88.5\" />",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Keep NativeAssets.Linux and install system dependencies in Dockerfile",
          "description": "Remove NoDependencies, keep NativeAssets.Linux, and install libfontconfig1 and libfreetype6 in the Dockerfile. Choose this if system font enumeration via SKFontManager is required.",
          "category": "alternative",
          "codeSnippet": "# In Dockerfile:\nRUN apt-get update && apt-get install -y \\\n    libfontconfig1 \\\n    libfreetype6 \\\n    && rm -rf /var/lib/apt/lists/*",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use NoDependencies only",
      "recommendedReason": "NoDependencies is specifically designed for minimal containers with zero system dependencies. Multiple community members confirmed it resolves the issue."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "The error is a packaging/deployment configuration issue — not a SkiaSharp code defect. NativeAssets.Linux requiring fontconfig is by-design and documented. Confirmed workarounds exist from multiple community members.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct type label from bug to question; add area and platform labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/question",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer explaining the packaging conflict and confirmed workarounds",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report. This is a packaging configuration issue rather than a SkiaSharp code bug.\n\nThe `TypeInitializationException` for `SKData` wraps a `DllNotFoundException` — `libSkiaSharp.so` cannot be loaded in your Docker container. You have **both** `SkiaSharp.NativeAssets.Linux` (which requires `libfontconfig.so.1`) **and** `SkiaSharp.NativeAssets.Linux.NoDependencies` referenced. In a minimal Docker image without fontconfig, having both packages can cause the fontconfig-dependent binary to be deployed, which then fails to load.\n\n**Option 1 (recommended): Use only `NoDependencies`**\n\nIn your executable project's `.csproj`:\n```xml\n<PackageReference Include=\"SkiaSharp\" Version=\"2.88.5\" />\n<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"2.88.5\" />\n```\n\nAlso remove `SkiaSharp.NativeAssets.WebAssembly` and `SkiaSharp.Views.Blazor` — these are only needed for Blazor/WASM apps, not server-side Linux apps.\n\n**Option 2: Keep `NativeAssets.Linux` and install system deps in Dockerfile**\n\nChoose this if you need system font enumeration via `SKFontManager`:\n```dockerfile\nRUN apt-get update && apt-get install -y \\\n    libfontconfig1 \\\n    libfreetype6 \\\n    && rm -rf /var/lib/apt/lists/*\n```\n\n> ⚠️ Make sure the `NativeAssets` package is referenced in the **executable** project (the `.csproj` that produces the Docker image output), not in a class library project."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — deployment configuration question with confirmed community answer",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
