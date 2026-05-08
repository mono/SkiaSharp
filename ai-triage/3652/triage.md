# Issue Triage Report — #3652

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-27T14:20:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | needs-info (0.85 (85%)) |

**Issue Summary:** ASP.NET Core app on AWS EC2 (Linux) returns 502/503 proxy errors when invoking SkiaSharp drawing code using SkiaSharp.NativeAssets.Linux v3.119.1; worked correctly in v3.116.1.

**Analysis:** The process hosting the ASP.NET Core application is likely crashing or failing to initialize SkiaSharp due to a missing or incompatible native dependency introduced in the v3.119 native binary. The 502/503 proxy errors are consistent with the worker process crashing before returning a response. SKTypeface.FromFamilyName() calls SKFontManager.Default.MatchFamily() which requires fontconfig on Linux; although the reporter installed fontconfig, the v3.119.1 native binary may require a different version or an additional system library not present on the AWS EC2 instance. No server-side logs are available to confirm the exact exception.

**Recommendations:** **needs-info** — The regression claim is credible and the failure pattern is consistent with a native dependency issue in v3.119.1. However, no server logs exist to identify the exact exception, and the reporter has not yet tried the NoDependencies workaround suggested by the maintainer. Server logs and EC2 distro details are required to confirm root cause.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create an ASP.NET Core app targeting net8.0 on Linux (AWS EC2)
2. Reference SkiaSharp.NativeAssets.Linux v3.119.1
3. Call SKBitmap, SKCanvas, SKTypeface.FromFamilyName, SKPaint and SKImage APIs
4. Observe 502/503 proxy error from the server

**Environment:** AWS EC2 instance, Linux, ASP.NET Core .NET 8.0, SkiaSharp.NativeAssets.Linux v3.119.1

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3386 — Similar DllNotFoundException with fontconfig on Azure Container App (Aspire) – same pattern: libfontconfig.so.1 not resolved, causing app crash

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | crash |
| Error message | Proxy Error: The proxy server received an invalid response from an upstream server. Reason: Error reading from remote server |
| Repro quality | partial |
| Target frameworks | net8.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.1, 3.119.1 |
| Worked in | 3.116.1 |
| Broke in | 3.119.1 |
| Current relevance | likely |
| Relevance reason | Reporter confirmed regression between these two specific versions; no server logs yet to confirm exact failure mode. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.85 (85%) |
| Reason | Reporter explicitly states it works in v3.116.1 and fails in v3.119.1 with the same code and environment. The Skia milestone bump from m116 to m119 may have introduced new native dependencies or changed the fontconfig linkage. |
| Worked in version | 3.116.1 |
| Broke in version | 3.119.1 |

## Analysis

### Technical Summary

The process hosting the ASP.NET Core application is likely crashing or failing to initialize SkiaSharp due to a missing or incompatible native dependency introduced in the v3.119 native binary. The 502/503 proxy errors are consistent with the worker process crashing before returning a response. SKTypeface.FromFamilyName() calls SKFontManager.Default.MatchFamily() which requires fontconfig on Linux; although the reporter installed fontconfig, the v3.119.1 native binary may require a different version or an additional system library not present on the AWS EC2 instance. No server-side logs are available to confirm the exact exception.

### Rationale

Classified as type/bug and area/libSkiaSharp.native because the failure is a native library load or initialization crash (proxy errors indicate server process death), not a code logic error. It is a regression between 3.116.1 and 3.119.1, pointing to a native binary dependency change in the m119 Skia milestone bump. The tenet/compatibility label reflects the version regression. Needs-info is suggested because no server logs exist to identify the exact failure mode, AWS EC2 distro and version are unknown, and the reporter has not tried NoDependencies as suggested by maintainer.

### Key Signals

- "This issue does not occur in v3.116.1" — **issue body** (Clear regression — native binary or dependency requirements changed between Skia milestone m116 and m119.)
- "Proxy Error: Error reading from remote server" — **issue body log output** (The upstream ASP.NET Core server process is crashing/dying, not returning an HTTP error. This is consistent with an unhandled exception during native library initialization (e.g., DllNotFoundException or TypeInitializationException).)
- "The server logs were not printed" — **comment by Akash26Arul** (Confirms the process is dying before logging — likely a TypeInitializationException or process-level crash rather than a handled exception.)
- "We installed fontconfig correctly in our application, but we are still facing the issue" — **comment by Akash26Arul** (Fontconfig version mismatch or additional undeclared native dependencies added in v3.119.1 are likely causing the failure, not a simple missing fontconfig package.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 74-80 | direct | SKTypeface.FromFamilyName() delegates to SKFontManager.Default.MatchFamily() — the default font manager on Linux uses fontconfig to enumerate and match system fonts. If fontconfig or a transitive native dependency cannot be loaded, this call will trigger a DllNotFoundException or similar crash at the point SkiaSharp first accesses the native library. |
| `binding/SkiaSharp/SKFontManager.cs` | 39 | direct | SKFontManager.Default is a static singleton. Its first access initializes the native font manager. On Linux this is backed by libSkiaSharp.so which dynamically links to libfontconfig.so.1. If fontconfig is present but at an incompatible ABI version, dlopen will fail. |
| `documentation/dev/packages.md` | — | related | SkiaSharp.NativeAssets.Linux requires libfontconfig.so.1 as a runtime dependency. The NoDependencies variant strips all external dependencies. If the v3.119 binary added new native deps or updated fontconfig linkage beyond what was in v3.116, environments with older or differently-configured fontconfig will fail. |

### Workarounds

- Switch from SkiaSharp.NativeAssets.Linux to SkiaSharp.NativeAssets.Linux.NoDependencies — this variant has no external dependencies (only libc/libm/libpthread/libdl) and avoids fontconfig entirely.
- Load fonts explicitly with SKTypeface.FromFile('/path/to/font.ttf') instead of SKTypeface.FromFamilyName() to avoid triggering the fontconfig-backed font manager.
- Run 'ldd /path/to/publish/libSkiaSharp.so' on the EC2 instance to identify which shared library is missing or incompatible.
- Enable ASP.NET Core stdout logging or set ASPNETCORE_ENVIRONMENT=Development to capture the full exception before the process crashes.

### Next Questions

- What is the AWS EC2 Linux distro and version (Amazon Linux 2, Amazon Linux 2023, Ubuntu, etc.)?
- What is the exact exception in the server logs when ASPNETCORE_ENVIRONMENT=Development is set?
- Does the issue reproduce with SkiaSharp.NativeAssets.Linux.NoDependencies?
- What does 'ldd libSkiaSharp.so' show on the EC2 instance — are there any 'not found' dependencies?
- Is the application running inside a Docker container on the EC2 instance?

### Resolution Proposals

**Hypothesis:** The v3.119.1 native binary for SkiaSharp.NativeAssets.Linux has a native dependency (likely fontconfig or a transitive library) that is present at a different version or not present at all on the AWS EC2 environment, causing a crash at library load time which manifests as a 502/503 proxy error.

1. **Switch to NoDependencies package** — workaround, confidence 0.85 (85%), cost/xs, validated=untested
   - Replace SkiaSharp.NativeAssets.Linux with SkiaSharp.NativeAssets.Linux.NoDependencies. This variant has zero external dependencies and works in any Linux environment. Fonts must be loaded explicitly using SKTypeface.FromFile() instead of SKTypeface.FromFamilyName().

```csharp
// In your .csproj, replace:
// <PackageReference Include="SkiaSharp.NativeAssets.Linux" Version="3.119.1" />
// with:
// <PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="3.119.1" />

// And load fonts explicitly:
using var typeface = SKTypeface.FromFile("/usr/share/fonts/truetype/arial.ttf")
    ?? SKTypeface.Default;
```
2. **Diagnose and install missing native dependency** — investigation, confidence 0.75 (75%), cost/s, validated=untested
   - Run 'ldd' on the deployed libSkiaSharp.so to identify the exact missing library, then install it. This preserves system font enumeration via fontconfig.

```csharp
# On the EC2 instance, find the deployed .so and check deps:
find /var/app -name 'libSkiaSharp.so' 2>/dev/null
ldd /path/to/libSkiaSharp.so | grep 'not found'
```

**Recommended proposal:** Switch to NoDependencies package

**Why:** Immediately unblocks the reporter without needing to debug native dependency chains. The NoDependencies variant is specifically designed for server/container deployments where fontconfig system integration isn't critical.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.85 (85%) |
| Reason | The regression claim is credible and the failure pattern is consistent with a native dependency issue in v3.119.1. However, no server logs exist to identify the exact exception, and the reporter has not yet tried the NoDependencies workaround suggested by the maintainer. Server logs and EC2 distro details are required to confirm root cause. |
| Suggested repro platform | linux |

### Missing Info

- Server logs with full exception stack trace (set ASPNETCORE_ENVIRONMENT=Development or check stdout logs)
- AWS EC2 Linux distribution and version (Amazon Linux 2, Amazon Linux 2023, Ubuntu, etc.)
- Output of 'ldd libSkiaSharp.so' on the EC2 instance to identify missing native dependencies
- Whether the application runs inside a Docker container on EC2
- Whether SkiaSharp.NativeAssets.Linux.NoDependencies resolves the issue

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, linux, native, compatibility labels | labels=type/bug, area/libSkiaSharp.native, os/Linux, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Request server logs and offer NoDependencies workaround | — |
| link-related | low | 0.80 (80%) | Cross-reference similar native loading issue on Azure Container Apps | linkedIssue=#3386 |

**Comment draft for `add-comment`:**

```markdown
Thanks for following up. The 502/503 proxy error pattern strongly suggests the ASP.NET Core worker process is crashing before returning a response — typically caused by a native library load failure.

A few things that would help diagnose this:

1. **Server logs**: Set `ASPNETCORE_ENVIRONMENT=Development` (or enable stdout logging) to capture the full exception stack trace before the process dies. Look for `DllNotFoundException`, `TypeInitializationException`, or similar.

2. **Check native dependencies**: On the EC2 instance, find and inspect the deployed native binary:
   ```bash
   find /var/app -name 'libSkiaSharp.so' 2>/dev/null
   ldd /path/to/libSkiaSharp.so | grep 'not found'
   ```
   This will show exactly which dependency is missing.

3. **AWS EC2 details**: What Linux distro/version is your EC2 instance running (Amazon Linux 2, Amazon Linux 2023, Ubuntu 20.04/22.04, etc.)?

**Workaround while investigating**: Switch to `SkiaSharp.NativeAssets.Linux.NoDependencies` — this variant has zero external dependencies and works on any Linux without fontconfig:
```xml
<!-- Replace NativeAssets.Linux with NoDependencies -->
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="3.119.1" />
```
Note: With `NoDependencies`, you must load fonts explicitly using `SKTypeface.FromFile("/path/to/font.ttf")` instead of `SKTypeface.FromFamilyName("Arial")`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3652,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-27T14:20:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "ASP.NET Core app on AWS EC2 (Linux) returns 502/503 proxy errors when invoking SkiaSharp drawing code using SkiaSharp.NativeAssets.Linux v3.119.1; worked correctly in v3.116.1.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.9
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "crash",
      "errorMessage": "Proxy Error: The proxy server received an invalid response from an upstream server. Reason: Error reading from remote server",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an ASP.NET Core app targeting net8.0 on Linux (AWS EC2)",
        "Reference SkiaSharp.NativeAssets.Linux v3.119.1",
        "Call SKBitmap, SKCanvas, SKTypeface.FromFamilyName, SKPaint and SKImage APIs",
        "Observe 502/503 proxy error from the server"
      ],
      "environmentDetails": "AWS EC2 instance, Linux, ASP.NET Core .NET 8.0, SkiaSharp.NativeAssets.Linux v3.119.1",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3386",
          "description": "Similar DllNotFoundException with fontconfig on Azure Container App (Aspire) – same pattern: libfontconfig.so.1 not resolved, causing app crash"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.1",
        "3.119.1"
      ],
      "workedIn": "3.116.1",
      "brokeIn": "3.119.1",
      "currentRelevance": "likely",
      "relevanceReason": "Reporter confirmed regression between these two specific versions; no server logs yet to confirm exact failure mode."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.85,
      "reason": "Reporter explicitly states it works in v3.116.1 and fails in v3.119.1 with the same code and environment. The Skia milestone bump from m116 to m119 may have introduced new native dependencies or changed the fontconfig linkage.",
      "workedInVersion": "3.116.1",
      "brokeInVersion": "3.119.1"
    }
  },
  "analysis": {
    "summary": "The process hosting the ASP.NET Core application is likely crashing or failing to initialize SkiaSharp due to a missing or incompatible native dependency introduced in the v3.119 native binary. The 502/503 proxy errors are consistent with the worker process crashing before returning a response. SKTypeface.FromFamilyName() calls SKFontManager.Default.MatchFamily() which requires fontconfig on Linux; although the reporter installed fontconfig, the v3.119.1 native binary may require a different version or an additional system library not present on the AWS EC2 instance. No server-side logs are available to confirm the exact exception.",
    "rationale": "Classified as type/bug and area/libSkiaSharp.native because the failure is a native library load or initialization crash (proxy errors indicate server process death), not a code logic error. It is a regression between 3.116.1 and 3.119.1, pointing to a native binary dependency change in the m119 Skia milestone bump. The tenet/compatibility label reflects the version regression. Needs-info is suggested because no server logs exist to identify the exact failure mode, AWS EC2 distro and version are unknown, and the reporter has not tried NoDependencies as suggested by maintainer.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "74-80",
        "finding": "SKTypeface.FromFamilyName() delegates to SKFontManager.Default.MatchFamily() — the default font manager on Linux uses fontconfig to enumerate and match system fonts. If fontconfig or a transitive native dependency cannot be loaded, this call will trigger a DllNotFoundException or similar crash at the point SkiaSharp first accesses the native library.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "39",
        "finding": "SKFontManager.Default is a static singleton. Its first access initializes the native font manager. On Linux this is backed by libSkiaSharp.so which dynamically links to libfontconfig.so.1. If fontconfig is present but at an incompatible ABI version, dlopen will fail.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "finding": "SkiaSharp.NativeAssets.Linux requires libfontconfig.so.1 as a runtime dependency. The NoDependencies variant strips all external dependencies. If the v3.119 binary added new native deps or updated fontconfig linkage beyond what was in v3.116, environments with older or differently-configured fontconfig will fail.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "This issue does not occur in v3.116.1",
        "source": "issue body",
        "interpretation": "Clear regression — native binary or dependency requirements changed between Skia milestone m116 and m119."
      },
      {
        "text": "Proxy Error: Error reading from remote server",
        "source": "issue body log output",
        "interpretation": "The upstream ASP.NET Core server process is crashing/dying, not returning an HTTP error. This is consistent with an unhandled exception during native library initialization (e.g., DllNotFoundException or TypeInitializationException)."
      },
      {
        "text": "The server logs were not printed",
        "source": "comment by Akash26Arul",
        "interpretation": "Confirms the process is dying before logging — likely a TypeInitializationException or process-level crash rather than a handled exception."
      },
      {
        "text": "We installed fontconfig correctly in our application, but we are still facing the issue",
        "source": "comment by Akash26Arul",
        "interpretation": "Fontconfig version mismatch or additional undeclared native dependencies added in v3.119.1 are likely causing the failure, not a simple missing fontconfig package."
      }
    ],
    "workarounds": [
      "Switch from SkiaSharp.NativeAssets.Linux to SkiaSharp.NativeAssets.Linux.NoDependencies — this variant has no external dependencies (only libc/libm/libpthread/libdl) and avoids fontconfig entirely.",
      "Load fonts explicitly with SKTypeface.FromFile('/path/to/font.ttf') instead of SKTypeface.FromFamilyName() to avoid triggering the fontconfig-backed font manager.",
      "Run 'ldd /path/to/publish/libSkiaSharp.so' on the EC2 instance to identify which shared library is missing or incompatible.",
      "Enable ASP.NET Core stdout logging or set ASPNETCORE_ENVIRONMENT=Development to capture the full exception before the process crashes."
    ],
    "nextQuestions": [
      "What is the AWS EC2 Linux distro and version (Amazon Linux 2, Amazon Linux 2023, Ubuntu, etc.)?",
      "What is the exact exception in the server logs when ASPNETCORE_ENVIRONMENT=Development is set?",
      "Does the issue reproduce with SkiaSharp.NativeAssets.Linux.NoDependencies?",
      "What does 'ldd libSkiaSharp.so' show on the EC2 instance — are there any 'not found' dependencies?",
      "Is the application running inside a Docker container on the EC2 instance?"
    ],
    "resolution": {
      "hypothesis": "The v3.119.1 native binary for SkiaSharp.NativeAssets.Linux has a native dependency (likely fontconfig or a transitive library) that is present at a different version or not present at all on the AWS EC2 environment, causing a crash at library load time which manifests as a 502/503 proxy error.",
      "proposals": [
        {
          "title": "Switch to NoDependencies package",
          "description": "Replace SkiaSharp.NativeAssets.Linux with SkiaSharp.NativeAssets.Linux.NoDependencies. This variant has zero external dependencies and works in any Linux environment. Fonts must be loaded explicitly using SKTypeface.FromFile() instead of SKTypeface.FromFamilyName().",
          "category": "workaround",
          "codeSnippet": "// In your .csproj, replace:\n// <PackageReference Include=\"SkiaSharp.NativeAssets.Linux\" Version=\"3.119.1\" />\n// with:\n// <PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"3.119.1\" />\n\n// And load fonts explicitly:\nusing var typeface = SKTypeface.FromFile(\"/usr/share/fonts/truetype/arial.ttf\")\n    ?? SKTypeface.Default;",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Diagnose and install missing native dependency",
          "description": "Run 'ldd' on the deployed libSkiaSharp.so to identify the exact missing library, then install it. This preserves system font enumeration via fontconfig.",
          "category": "investigation",
          "codeSnippet": "# On the EC2 instance, find the deployed .so and check deps:\nfind /var/app -name 'libSkiaSharp.so' 2>/dev/null\nldd /path/to/libSkiaSharp.so | grep 'not found'",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Switch to NoDependencies package",
      "recommendedReason": "Immediately unblocks the reporter without needing to debug native dependency chains. The NoDependencies variant is specifically designed for server/container deployments where fontconfig system integration isn't critical."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.85,
      "reason": "The regression claim is credible and the failure pattern is consistent with a native dependency issue in v3.119.1. However, no server logs exist to identify the exact exception, and the reporter has not yet tried the NoDependencies workaround suggested by the maintainer. Server logs and EC2 distro details are required to confirm root cause.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Server logs with full exception stack trace (set ASPNETCORE_ENVIRONMENT=Development or check stdout logs)",
      "AWS EC2 Linux distribution and version (Amazon Linux 2, Amazon Linux 2023, Ubuntu, etc.)",
      "Output of 'ldd libSkiaSharp.so' on the EC2 instance to identify missing native dependencies",
      "Whether the application runs inside a Docker container on EC2",
      "Whether SkiaSharp.NativeAssets.Linux.NoDependencies resolves the issue"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, linux, native, compatibility labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request server logs and offer NoDependencies workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for following up. The 502/503 proxy error pattern strongly suggests the ASP.NET Core worker process is crashing before returning a response — typically caused by a native library load failure.\n\nA few things that would help diagnose this:\n\n1. **Server logs**: Set `ASPNETCORE_ENVIRONMENT=Development` (or enable stdout logging) to capture the full exception stack trace before the process dies. Look for `DllNotFoundException`, `TypeInitializationException`, or similar.\n\n2. **Check native dependencies**: On the EC2 instance, find and inspect the deployed native binary:\n   ```bash\n   find /var/app -name 'libSkiaSharp.so' 2>/dev/null\n   ldd /path/to/libSkiaSharp.so | grep 'not found'\n   ```\n   This will show exactly which dependency is missing.\n\n3. **AWS EC2 details**: What Linux distro/version is your EC2 instance running (Amazon Linux 2, Amazon Linux 2023, Ubuntu 20.04/22.04, etc.)?\n\n**Workaround while investigating**: Switch to `SkiaSharp.NativeAssets.Linux.NoDependencies` — this variant has zero external dependencies and works on any Linux without fontconfig:\n```xml\n<!-- Replace NativeAssets.Linux with NoDependencies -->\n<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"3.119.1\" />\n```\nNote: With `NoDependencies`, you must load fonts explicitly using `SKTypeface.FromFile(\"/path/to/font.ttf\")` instead of `SKTypeface.FromFamilyName(\"Arial\")`."
      },
      {
        "type": "link-related",
        "description": "Cross-reference similar native loading issue on Azure Container Apps",
        "risk": "low",
        "confidence": 0.8,
        "linkedIssue": 3386
      }
    ]
  }
}
```

</details>
