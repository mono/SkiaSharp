# Issue Triage Report — #2222

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T23:44:46Z |
| Type | type/bug (0.70 (70%)) |
| Area | area/libSkiaSharp.native (0.80 (80%)) |
| Suggested action | needs-info (0.85 (85%)) |

**Issue Summary:** CentOS 7 (.NET 5) throws TypeInitializationException in SKImageInfo static constructor when using SkiaSharp 2.80.3 or 2.88.1 on Linux, likely indicating a native library loading failure (fontconfig missing or glibc incompatibility).

**Analysis:** TypeInitializationException in SKImageInfo static constructor is caused by native libSkiaSharp.so failing to load. The static constructor calls P/Invoke methods into the native library; if the native load fails, those calls throw and surface as TypeInitializationException. Without the full InnerException chain and ldd output from the CentOS 7 server, the specific root cause — missing fontconfig dependency, glibc 2.17 ABI incompatibility, or wrong binary deployment — cannot be confirmed.

**Recommendations:** **needs-info** — TypeInitializationException is a symptom of native library load failure. The inner exception (which reveals the exact DllNotFoundException cause) and ldd output from the CentOS 7 server are not provided, preventing root cause confirmation. Both NativeAssets.Linux and NoDependencies fail, suggesting glibc ABI incompatibility, but this requires confirmation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Deploy .NET 5 application with SkiaSharp 2.80.3 or 2.88.1 and SkiaSharp.NativeAssets.Linux on CentOS 7 x86-64
2. Invoke code that constructs SKImageInfo (e.g., new SKImageInfo(width, height))
3. Observe TypeInitializationException wrapping an inner exception from the static constructor

**Environment:** CentOS 7 x86-64 (Kernel 3.10.0, glibc 2.17), .NET 5.0.408, SkiaSharp 2.80.3 and 2.88.1 tested

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | exception |
| Error message | The type initializer for 'SkiaSharp.SKImageInfo' threw an exception. |
| Repro quality | partial |
| Target frameworks | net5.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.3, 2.88.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Reporter tested 2.80.3 and 2.88.1 with both NativeAssets.Linux and NativeAssets.Linux.NoDependencies — all fail. Inner exception not provided so the exact root cause (missing fontconfig, glibc ABI, or deployment issue) is unconfirmed. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.55 (55%) |
| Reason | Reporter states the app worked previously on CentOS 7 without code changes on their part, then stopped working. |
| Worked in version | 2.80.3 (initially) |
| Broke in version | — |

## Analysis

### Technical Summary

TypeInitializationException in SKImageInfo static constructor is caused by native libSkiaSharp.so failing to load. The static constructor calls P/Invoke methods into the native library; if the native load fails, those calls throw and surface as TypeInitializationException. Without the full InnerException chain and ldd output from the CentOS 7 server, the specific root cause — missing fontconfig dependency, glibc 2.17 ABI incompatibility, or wrong binary deployment — cannot be confirmed.

### Rationale

SKImageInfo's static constructor makes P/Invoke calls (sk_colortype_get_default_8888, sk_color_get_bit_shift); TypeInitializationException from this constructor is a classic symptom of native library load failure. Reporter tried NativeAssets.Linux and NativeAssets.Linux.NoDependencies without success, which rules out a simple fontconfig fix and points toward CentOS 7 glibc 2.17 ABI incompatibility or incorrect deployment. Classified as type/bug because there is broken behavior requiring investigation.

### Key Signals

- "The type initializer for 'SkiaSharp.SKImageInfo' threw an exception." — **issue body** (SKImageInfo static constructor makes P/Invoke calls; if libSkiaSharp fails to load, these throw and are wrapped as TypeInitializationException. The inner exception would reveal the exact loading failure.)
- "CentOS 7 (x86-64 Kernel Linux 3.10.0)" — **issue body** (CentOS 7 uses glibc 2.17 (from 2013). Newer SkiaSharp native binaries compiled against a newer glibc may require symbols not present in glibc 2.17, causing a load failure.)
- "I tested [...] SkiaSharp.NativeAssets.Linux.NoDependencies [...] but nothing works" — **issue body** (NoDependencies variant has no fontconfig dependency, so a fontconfig-only issue would have been fixed by this switch. Continued failure suggests deeper incompatibility (glibc ABI or deployment problem).)
- "the project doesnt work in CentOS 7 (create PDF), even, I havent touch this part of the program" — **issue body** (Reporter believes this is a regression triggered by an external change (VPS server update or OS update), not a code change on their side.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImageInfo.cs` | 46-58 | direct | SKImageInfo static constructor calls SkiaApi.sk_colortype_get_default_8888() and SkiaApi.sk_color_get_bit_shift(). Both are P/Invoke calls into libSkiaSharp. If libSkiaSharp fails to load (DllNotFoundException or similar), the first P/Invoke call throws and the runtime wraps it as TypeInitializationException. This confirms the reported exception is a symptom of native library load failure. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 6270-6286 | direct | sk_colortype_get_default_8888 is defined as a P/Invoke into libSkiaSharp. If the native library cannot be resolved at runtime (missing file, missing dependency, or ABI mismatch), any call into SkiaApi throws DllNotFoundException, which is then wrapped by the TypeInitializationException in SKImageInfo. |

### Workarounds

- Run `ldd /publish/libSkiaSharp.so` on CentOS 7 to identify which dependency is missing — the output pinpoints the root cause
- If using NativeAssets.Linux: install fontconfig (`sudo yum install fontconfig`) — required dependency for this variant
- If NoDependencies still fails: upgrade to a newer Linux distribution (Rocky Linux 8/9, RHEL 8/9, Ubuntu 20.04+) that provides glibc >= 2.18
- Ensure NativeAssets package is referenced in the executable project, not only in a library project

### Next Questions

- What is the full InnerException chain of the TypeInitializationException? The inner DllNotFoundException shows the specific missing library or ABI error.
- What does `ldd /path/to/publish/libSkiaSharp.so` output on the CentOS 7 server?
- Is the application running in a Docker container or directly on bare-metal CentOS 7?
- What changed on the CentOS 7 VPS around the time it stopped working — OS update, .NET runtime update, or package update?

### Resolution Proposals

**Hypothesis:** The native libSkiaSharp.so fails to load on CentOS 7 because either: (a) SkiaSharp.NativeAssets.Linux requires fontconfig which is not installed, or (b) SkiaSharp 2.88.x native binaries require glibc symbols not present in CentOS 7's glibc 2.17. Since NoDependencies also fails, option (b) or a deployment issue is more likely.

1. **Request InnerException and ldd output** — investigation, confidence 0.95 (95%), cost/xs, validated=untested
   - Ask reporter to capture the full InnerException from the TypeInitializationException and run ldd on the deployed libSkiaSharp.so. This will confirm the exact root cause (missing fontconfig, glibc ABI failure, or file not found).
2. **Install fontconfig on CentOS 7** — workaround, confidence 0.70 (70%), cost/xs, validated=untested
   - When using SkiaSharp.NativeAssets.Linux (not NoDependencies), fontconfig must be present. Install it with: `sudo yum install fontconfig`. If the app is in a container, add `RUN yum install -y fontconfig` to the Dockerfile.
3. **Upgrade to newer Linux distribution** — workaround, confidence 0.65 (65%), cost/l, validated=untested
   - CentOS 7 (EOL June 2024, glibc 2.17) may be too old for newer SkiaSharp native binaries. Migrating to Rocky Linux 8/9, RHEL 8/9, or Ubuntu 20.04+ provides glibc >= 2.17 and continued security support.

**Recommended proposal:** Request InnerException and ldd output

**Why:** Without the inner exception and ldd output, the specific root cause cannot be confirmed. Getting this information first will allow a precise solution rather than guessing between fontconfig and glibc issues.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.85 (85%) |
| Reason | TypeInitializationException is a symptom of native library load failure. The inner exception (which reveals the exact DllNotFoundException cause) and ldd output from the CentOS 7 server are not provided, preventing root cause confirmation. Both NativeAssets.Linux and NoDependencies fail, suggesting glibc ABI incompatibility, but this requires confirmation. |
| Suggested repro platform | linux |

### Missing Info

- Full exception including InnerException chain — the inner exception shows the specific missing library or ABI error
- Output of `ldd /path/to/publish/libSkiaSharp.so` on the CentOS 7 server
- Whether deployment is in a Docker container or directly on bare-metal CentOS 7
- What changed on the CentOS 7 VPS around the time the issue appeared

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply type/bug, area/libSkiaSharp.native, os/Linux labels | labels=type/bug, area/libSkiaSharp.native, os/Linux |
| add-comment | medium | 0.85 (85%) | Request full InnerException and ldd output to diagnose native library load failure on CentOS 7 | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report. The `TypeInitializationException` in `SKImageInfo` is caused by the native `libSkiaSharp.so` failing to load — the actual error is hidden in the `InnerException`.

To diagnose this, please provide:

1. **Full exception with `InnerException`** — capture the complete exception including `InnerException` (e.g., with `ex.ToString()` or structured logging). The inner exception shows what specifically failed (e.g., `DllNotFoundException: libfontconfig.so.1`, or a glibc symbol error).
2. **`ldd` output** — run `ldd /path/to/publish/runtimes/linux-x64/native/libSkiaSharp.so` on the CentOS 7 server and share the output. This identifies any missing shared library dependencies.
3. **Deployment type** — are you running in a Docker container or directly on bare-metal CentOS 7?

**Quick things to try:**
- If using `SkiaSharp.NativeAssets.Linux`: install fontconfig with `sudo yum install fontconfig` — this is a required dependency.
- If still failing with `SkiaSharp.NativeAssets.Linux.NoDependencies`: CentOS 7 ships with glibc 2.17 (from 2013), which may be too old for SkiaSharp 2.88.x native binaries. In that case, migrating to a newer OS (Rocky Linux 8/9, RHEL 8/9) may be necessary.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2222,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T23:44:46Z"
  },
  "summary": "CentOS 7 (.NET 5) throws TypeInitializationException in SKImageInfo static constructor when using SkiaSharp 2.80.3 or 2.88.1 on Linux, likely indicating a native library loading failure (fontconfig missing or glibc incompatibility).",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.7
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.8
    },
    "platforms": [
      "os/Linux"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "The type initializer for 'SkiaSharp.SKImageInfo' threw an exception.",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net5.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Deploy .NET 5 application with SkiaSharp 2.80.3 or 2.88.1 and SkiaSharp.NativeAssets.Linux on CentOS 7 x86-64",
        "Invoke code that constructs SKImageInfo (e.g., new SKImageInfo(width, height))",
        "Observe TypeInitializationException wrapping an inner exception from the static constructor"
      ],
      "environmentDetails": "CentOS 7 x86-64 (Kernel 3.10.0, glibc 2.17), .NET 5.0.408, SkiaSharp 2.80.3 and 2.88.1 tested"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.3",
        "2.88.1"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Reporter tested 2.80.3 and 2.88.1 with both NativeAssets.Linux and NativeAssets.Linux.NoDependencies — all fail. Inner exception not provided so the exact root cause (missing fontconfig, glibc ABI, or deployment issue) is unconfirmed."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.55,
      "reason": "Reporter states the app worked previously on CentOS 7 without code changes on their part, then stopped working.",
      "workedInVersion": "2.80.3 (initially)"
    }
  },
  "analysis": {
    "summary": "TypeInitializationException in SKImageInfo static constructor is caused by native libSkiaSharp.so failing to load. The static constructor calls P/Invoke methods into the native library; if the native load fails, those calls throw and surface as TypeInitializationException. Without the full InnerException chain and ldd output from the CentOS 7 server, the specific root cause — missing fontconfig dependency, glibc 2.17 ABI incompatibility, or wrong binary deployment — cannot be confirmed.",
    "rationale": "SKImageInfo's static constructor makes P/Invoke calls (sk_colortype_get_default_8888, sk_color_get_bit_shift); TypeInitializationException from this constructor is a classic symptom of native library load failure. Reporter tried NativeAssets.Linux and NativeAssets.Linux.NoDependencies without success, which rules out a simple fontconfig fix and points toward CentOS 7 glibc 2.17 ABI incompatibility or incorrect deployment. Classified as type/bug because there is broken behavior requiring investigation.",
    "keySignals": [
      {
        "text": "The type initializer for 'SkiaSharp.SKImageInfo' threw an exception.",
        "source": "issue body",
        "interpretation": "SKImageInfo static constructor makes P/Invoke calls; if libSkiaSharp fails to load, these throw and are wrapped as TypeInitializationException. The inner exception would reveal the exact loading failure."
      },
      {
        "text": "CentOS 7 (x86-64 Kernel Linux 3.10.0)",
        "source": "issue body",
        "interpretation": "CentOS 7 uses glibc 2.17 (from 2013). Newer SkiaSharp native binaries compiled against a newer glibc may require symbols not present in glibc 2.17, causing a load failure."
      },
      {
        "text": "I tested [...] SkiaSharp.NativeAssets.Linux.NoDependencies [...] but nothing works",
        "source": "issue body",
        "interpretation": "NoDependencies variant has no fontconfig dependency, so a fontconfig-only issue would have been fixed by this switch. Continued failure suggests deeper incompatibility (glibc ABI or deployment problem)."
      },
      {
        "text": "the project doesnt work in CentOS 7 (create PDF), even, I havent touch this part of the program",
        "source": "issue body",
        "interpretation": "Reporter believes this is a regression triggered by an external change (VPS server update or OS update), not a code change on their side."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImageInfo.cs",
        "lines": "46-58",
        "finding": "SKImageInfo static constructor calls SkiaApi.sk_colortype_get_default_8888() and SkiaApi.sk_color_get_bit_shift(). Both are P/Invoke calls into libSkiaSharp. If libSkiaSharp fails to load (DllNotFoundException or similar), the first P/Invoke call throws and the runtime wraps it as TypeInitializationException. This confirms the reported exception is a symptom of native library load failure.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "6270-6286",
        "finding": "sk_colortype_get_default_8888 is defined as a P/Invoke into libSkiaSharp. If the native library cannot be resolved at runtime (missing file, missing dependency, or ABI mismatch), any call into SkiaApi throws DllNotFoundException, which is then wrapped by the TypeInitializationException in SKImageInfo.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Run `ldd /publish/libSkiaSharp.so` on CentOS 7 to identify which dependency is missing — the output pinpoints the root cause",
      "If using NativeAssets.Linux: install fontconfig (`sudo yum install fontconfig`) — required dependency for this variant",
      "If NoDependencies still fails: upgrade to a newer Linux distribution (Rocky Linux 8/9, RHEL 8/9, Ubuntu 20.04+) that provides glibc >= 2.18",
      "Ensure NativeAssets package is referenced in the executable project, not only in a library project"
    ],
    "nextQuestions": [
      "What is the full InnerException chain of the TypeInitializationException? The inner DllNotFoundException shows the specific missing library or ABI error.",
      "What does `ldd /path/to/publish/libSkiaSharp.so` output on the CentOS 7 server?",
      "Is the application running in a Docker container or directly on bare-metal CentOS 7?",
      "What changed on the CentOS 7 VPS around the time it stopped working — OS update, .NET runtime update, or package update?"
    ],
    "resolution": {
      "hypothesis": "The native libSkiaSharp.so fails to load on CentOS 7 because either: (a) SkiaSharp.NativeAssets.Linux requires fontconfig which is not installed, or (b) SkiaSharp 2.88.x native binaries require glibc symbols not present in CentOS 7's glibc 2.17. Since NoDependencies also fails, option (b) or a deployment issue is more likely.",
      "proposals": [
        {
          "title": "Request InnerException and ldd output",
          "description": "Ask reporter to capture the full InnerException from the TypeInitializationException and run ldd on the deployed libSkiaSharp.so. This will confirm the exact root cause (missing fontconfig, glibc ABI failure, or file not found).",
          "category": "investigation",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Install fontconfig on CentOS 7",
          "description": "When using SkiaSharp.NativeAssets.Linux (not NoDependencies), fontconfig must be present. Install it with: `sudo yum install fontconfig`. If the app is in a container, add `RUN yum install -y fontconfig` to the Dockerfile.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Upgrade to newer Linux distribution",
          "description": "CentOS 7 (EOL June 2024, glibc 2.17) may be too old for newer SkiaSharp native binaries. Migrating to Rocky Linux 8/9, RHEL 8/9, or Ubuntu 20.04+ provides glibc >= 2.17 and continued security support.",
          "category": "workaround",
          "confidence": 0.65,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Request InnerException and ldd output",
      "recommendedReason": "Without the inner exception and ldd output, the specific root cause cannot be confirmed. Getting this information first will allow a precise solution rather than guessing between fontconfig and glibc issues."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.85,
      "reason": "TypeInitializationException is a symptom of native library load failure. The inner exception (which reveals the exact DllNotFoundException cause) and ldd output from the CentOS 7 server are not provided, preventing root cause confirmation. Both NativeAssets.Linux and NoDependencies fail, suggesting glibc ABI incompatibility, but this requires confirmation.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Full exception including InnerException chain — the inner exception shows the specific missing library or ABI error",
      "Output of `ldd /path/to/publish/libSkiaSharp.so` on the CentOS 7 server",
      "Whether deployment is in a Docker container or directly on bare-metal CentOS 7",
      "What changed on the CentOS 7 VPS around the time the issue appeared"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/libSkiaSharp.native, os/Linux labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Linux"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request full InnerException and ldd output to diagnose native library load failure on CentOS 7",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the report. The `TypeInitializationException` in `SKImageInfo` is caused by the native `libSkiaSharp.so` failing to load — the actual error is hidden in the `InnerException`.\n\nTo diagnose this, please provide:\n\n1. **Full exception with `InnerException`** — capture the complete exception including `InnerException` (e.g., with `ex.ToString()` or structured logging). The inner exception shows what specifically failed (e.g., `DllNotFoundException: libfontconfig.so.1`, or a glibc symbol error).\n2. **`ldd` output** — run `ldd /path/to/publish/runtimes/linux-x64/native/libSkiaSharp.so` on the CentOS 7 server and share the output. This identifies any missing shared library dependencies.\n3. **Deployment type** — are you running in a Docker container or directly on bare-metal CentOS 7?\n\n**Quick things to try:**\n- If using `SkiaSharp.NativeAssets.Linux`: install fontconfig with `sudo yum install fontconfig` — this is a required dependency.\n- If still failing with `SkiaSharp.NativeAssets.Linux.NoDependencies`: CentOS 7 ships with glibc 2.17 (from 2013), which may be too old for SkiaSharp 2.88.x native binaries. In that case, migrating to a newer OS (Rocky Linux 8/9, RHEL 8/9) may be necessary."
      }
    ]
  }
}
```

</details>
