# Issue Triage Report — #1371

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T04:04:21Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.75 (75%)) |

**Issue Summary:** Release build for Android fails with LinkAssemblies error when Xamarin.Android 9.1.7.0 linker cannot resolve modreq(InAttribute) on ReadOnlySpan<byte>.GetPinnableReference(), a regression introduced by SKBitmap.DecodeBounds(ReadOnlySpan<byte>) added in SkiaSharp 1.68.2.

**Analysis:** Xamarin.Android Mono.Linker 9.1.7.0 cannot resolve the modreq(InAttribute) modifier on ReadOnlySpan<T>.GetPinnableReference(), which is emitted by the C# compiler when using 'fixed (byte* b = buffer)' on ReadOnlySpan<byte>. SkiaSharp 1.68.2 introduced SKBitmap.DecodeBounds(ReadOnlySpan<byte>) using this pattern, triggering the incompatibility. The root cause is in the old Xamarin.Android linker; upgrading Xamarin.Android or downgrading SkiaSharp resolves it.

**Recommendations:** **needs-investigation** — Root cause is in the old Xamarin.Android linker. Needs verification of whether current SkiaSharp 2.x/3.x is still affected and whether upgrading Xamarin.Android SDK is sufficient.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Android |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Android or Xamarin.Forms project
2. Add SkiaSharp 1.68.3 (or 1.68.2+) NuGet reference
3. Configure Linker to 'Sdk Assemblies Only' in Android build options
4. Build the project in Release mode to generate APK
5. Observe LinkAssemblies task failure

**Environment:** Android 9 (Pie), Visual Studio 2017 v15.9.24, SkiaSharp 1.68.3, Xamarin.Android SDK 9.1.7.0, Xamarin.Forms 4.5.0.725

**Repository links:**
- https://github.com/mono/SkiaSharp/files/4856763/App5.zip — Reporter repro project (App5.zip)
- https://github.com/xamarin/Xamarin.Forms/issues/11295 — Related Xamarin.Forms issue linked by maintainer — same linker compatibility problem

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | build-error |
| Error message | Mono.Linker.MarkException: Error processing method: 'SkiaSharp.SKImageInfo SkiaSharp.SKBitmap::DecodeBounds(System.ReadOnlySpan`1)' --- Mono.Cecil.ResolutionException: Failed to resolve !0& modreq(System.Runtime.InteropServices.InAttribute) System.ReadOnlySpan`1::GetPinnableReference() |
| Repro quality | complete |
| Target frameworks | monoandroid90 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.3, 1.68.2.0, 1.68.1.1 |
| Worked in | 1.68.1.1 |
| Broke in | 1.68.2.0 |
| Current relevance | unlikely |
| Relevance reason | Issue is specific to SkiaSharp 1.68.x with Xamarin.Android 9.1.7.0. Current SkiaSharp 2.x/3.x targets modern Android SDK with a fixed linker. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.95 (95%) |
| Reason | A separate user confirmed the error appears starting from SkiaSharp 1.68.2.0 and downgrading to 1.68.1.1 resolves it. |
| Worked in version | 1.68.1.1 |
| Broke in version | 1.68.2.0 |

## Analysis

### Technical Summary

Xamarin.Android Mono.Linker 9.1.7.0 cannot resolve the modreq(InAttribute) modifier on ReadOnlySpan<T>.GetPinnableReference(), which is emitted by the C# compiler when using 'fixed (byte* b = buffer)' on ReadOnlySpan<byte>. SkiaSharp 1.68.2 introduced SKBitmap.DecodeBounds(ReadOnlySpan<byte>) using this pattern, triggering the incompatibility. The root cause is in the old Xamarin.Android linker; upgrading Xamarin.Android or downgrading SkiaSharp resolves it.

### Rationale

The error is a confirmed regression from 1.68.1.1 to 1.68.2.0 per a second user. The root cause is a Xamarin.Android linker limitation with modreq types on newer Span APIs. The maintainer linked to a Xamarin.Forms issue tracking the same external problem, indicating the fix belongs in Xamarin.Android rather than SkiaSharp. Alternative DecodeBounds overloads avoid the Span code path entirely.

### Key Signals

- "The error appears from version of Skiasharp 1.68.2.0." — **comment by MarieR64** (Confirms regression introduced in 1.68.2.0, not present in 1.68.1.1.)
- "Failed to resolve !0& modreq(System.Runtime.InteropServices.InAttribute) System.ReadOnlySpan`1::GetPinnableReference()" — **issue body stack trace** (Xamarin.Android Mono.Linker cannot process the modreq modifier on GetPinnableReference — a known limitation of older Xamarin.Android linker versions.)
- "Linking https://github.com/xamarin/Xamarin.Forms/issues/11295" — **maintainer comment by mattleibow** (Root cause is external — the Xamarin.Android linker has a bug that is tracked upstream, not in SkiaSharp.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 423-430 | direct | DecodeBounds(ReadOnlySpan<byte>) uses 'fixed (byte* b = buffer)' which emits a call to ReadOnlySpan<T>.GetPinnableReference() with modreq(InAttribute). This is the exact method the old Xamarin.Android linker (9.1.7.0) fails to resolve, directly causing the build error. |
| `binding/SkiaSharp/SKBitmap.cs` | 400-408 | related | DecodeBounds(SKData data) is an available overload that bypasses ReadOnlySpan<byte> entirely, providing a viable workaround by wrapping byte[] in SKData.CreateCopy() first. |

### Workarounds

- Downgrade SkiaSharp to 1.68.1.1 (confirmed working by a second user).
- Upgrade Xamarin.Android SDK to a version where the linker supports modreq(InAttribute) on ReadOnlySpan<T>.
- Use SKBitmap.DecodeBounds(SKData) instead of DecodeBounds(byte[]): create an SKData via SKData.CreateCopy(buffer) and pass that to avoid the ReadOnlySpan code path.

### Next Questions

- Does upgrading Xamarin.Android to 10.x or 11.x resolve the issue without downgrading SkiaSharp?
- Is this issue still present in SkiaSharp 2.x/3.x with modern .NET Android tooling?

### Resolution Proposals

**Hypothesis:** The Xamarin.Android Mono.Linker in version 9.1.7.0 does not support modreq(InAttribute) used by C# code for ReadOnlySpan<T>.GetPinnableReference(). SkiaSharp 1.68.2 triggered this latent linker bug by introducing ReadOnlySpan<byte> overloads compiled with a newer Roslyn that emits the modreq.

1. **Use SKData overload as immediate workaround** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Replace calls to SKBitmap.DecodeBounds(byte[]) with SKBitmap.DecodeBounds(SKData) to avoid the ReadOnlySpan code path that triggers the linker error.

```csharp
byte[] imageBytes = ...;
using var skData = SKData.CreateCopy(imageBytes);
var info = SKBitmap.DecodeBounds(skData);
```
2. **Upgrade Xamarin.Android SDK** — fix, confidence 0.85 (85%), cost/s, validated=untested
   - Upgrade to a newer Xamarin.Android SDK version that includes a fixed Mono.Linker capable of handling modreq(InAttribute). This addresses the root cause.

**Recommended proposal:** Use SKData overload as immediate workaround

**Why:** Immediate code change that avoids the problematic ReadOnlySpan code path without requiring a downgrade of SkiaSharp or waiting for an SDK upgrade.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.75 (75%) |
| Reason | Root cause is in the old Xamarin.Android linker. Needs verification of whether current SkiaSharp 2.x/3.x is still affected and whether upgrading Xamarin.Android SDK is sufficient. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, core SkiaSharp area, Android platform, and compatibility tenet labels | labels=type/bug, area/SkiaSharp, os/Android, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Post regression analysis with workaround options | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and repro project. This is a known compatibility issue between SkiaSharp 1.68.2+ and older Xamarin.Android SDK versions.

The `DecodeBounds(ReadOnlySpan<byte>)` method introduced in 1.68.2 uses `fixed (byte* b = buffer)` which the old Xamarin.Android 9.1.7.0 Mono.Linker cannot process — it fails to resolve the `modreq(System.Runtime.InteropServices.InAttribute)` modifier on `GetPinnableReference()`. This issue has been confirmed by another user (regression from 1.68.1.1 → 1.68.2.0).

**Workarounds:**
1. **Downgrade SkiaSharp** to 1.68.1.1 (confirmed working)
2. **Upgrade Xamarin.Android SDK** to a newer version with a fixed linker
3. **Use the SKData overload** to avoid the ReadOnlySpan code path:
```csharp
byte[] imageBytes = ...;
using var skData = SKData.CreateCopy(imageBytes);
var info = SKBitmap.DecodeBounds(skData);
```

The root cause is tracked upstream: https://github.com/xamarin/Xamarin.Forms/issues/11295
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1371,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T04:04:21Z"
  },
  "summary": "Release build for Android fails with LinkAssemblies error when Xamarin.Android 9.1.7.0 linker cannot resolve modreq(InAttribute) on ReadOnlySpan<byte>.GetPinnableReference(), a regression introduced by SKBitmap.DecodeBounds(ReadOnlySpan<byte>) added in SkiaSharp 1.68.2.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Android"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "build-error",
      "errorMessage": "Mono.Linker.MarkException: Error processing method: 'SkiaSharp.SKImageInfo SkiaSharp.SKBitmap::DecodeBounds(System.ReadOnlySpan`1)' --- Mono.Cecil.ResolutionException: Failed to resolve !0& modreq(System.Runtime.InteropServices.InAttribute) System.ReadOnlySpan`1::GetPinnableReference()",
      "reproQuality": "complete",
      "targetFrameworks": [
        "monoandroid90"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Android or Xamarin.Forms project",
        "Add SkiaSharp 1.68.3 (or 1.68.2+) NuGet reference",
        "Configure Linker to 'Sdk Assemblies Only' in Android build options",
        "Build the project in Release mode to generate APK",
        "Observe LinkAssemblies task failure"
      ],
      "environmentDetails": "Android 9 (Pie), Visual Studio 2017 v15.9.24, SkiaSharp 1.68.3, Xamarin.Android SDK 9.1.7.0, Xamarin.Forms 4.5.0.725",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/4856763/App5.zip",
          "description": "Reporter repro project (App5.zip)"
        },
        {
          "url": "https://github.com/xamarin/Xamarin.Forms/issues/11295",
          "description": "Related Xamarin.Forms issue linked by maintainer — same linker compatibility problem"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.3",
        "1.68.2.0",
        "1.68.1.1"
      ],
      "workedIn": "1.68.1.1",
      "brokeIn": "1.68.2.0",
      "currentRelevance": "unlikely",
      "relevanceReason": "Issue is specific to SkiaSharp 1.68.x with Xamarin.Android 9.1.7.0. Current SkiaSharp 2.x/3.x targets modern Android SDK with a fixed linker."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.95,
      "reason": "A separate user confirmed the error appears starting from SkiaSharp 1.68.2.0 and downgrading to 1.68.1.1 resolves it.",
      "workedInVersion": "1.68.1.1",
      "brokeInVersion": "1.68.2.0"
    }
  },
  "analysis": {
    "summary": "Xamarin.Android Mono.Linker 9.1.7.0 cannot resolve the modreq(InAttribute) modifier on ReadOnlySpan<T>.GetPinnableReference(), which is emitted by the C# compiler when using 'fixed (byte* b = buffer)' on ReadOnlySpan<byte>. SkiaSharp 1.68.2 introduced SKBitmap.DecodeBounds(ReadOnlySpan<byte>) using this pattern, triggering the incompatibility. The root cause is in the old Xamarin.Android linker; upgrading Xamarin.Android or downgrading SkiaSharp resolves it.",
    "rationale": "The error is a confirmed regression from 1.68.1.1 to 1.68.2.0 per a second user. The root cause is a Xamarin.Android linker limitation with modreq types on newer Span APIs. The maintainer linked to a Xamarin.Forms issue tracking the same external problem, indicating the fix belongs in Xamarin.Android rather than SkiaSharp. Alternative DecodeBounds overloads avoid the Span code path entirely.",
    "keySignals": [
      {
        "text": "The error appears from version of Skiasharp 1.68.2.0.",
        "source": "comment by MarieR64",
        "interpretation": "Confirms regression introduced in 1.68.2.0, not present in 1.68.1.1."
      },
      {
        "text": "Failed to resolve !0& modreq(System.Runtime.InteropServices.InAttribute) System.ReadOnlySpan`1::GetPinnableReference()",
        "source": "issue body stack trace",
        "interpretation": "Xamarin.Android Mono.Linker cannot process the modreq modifier on GetPinnableReference — a known limitation of older Xamarin.Android linker versions."
      },
      {
        "text": "Linking https://github.com/xamarin/Xamarin.Forms/issues/11295",
        "source": "maintainer comment by mattleibow",
        "interpretation": "Root cause is external — the Xamarin.Android linker has a bug that is tracked upstream, not in SkiaSharp."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "423-430",
        "finding": "DecodeBounds(ReadOnlySpan<byte>) uses 'fixed (byte* b = buffer)' which emits a call to ReadOnlySpan<T>.GetPinnableReference() with modreq(InAttribute). This is the exact method the old Xamarin.Android linker (9.1.7.0) fails to resolve, directly causing the build error.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "400-408",
        "finding": "DecodeBounds(SKData data) is an available overload that bypasses ReadOnlySpan<byte> entirely, providing a viable workaround by wrapping byte[] in SKData.CreateCopy() first.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Downgrade SkiaSharp to 1.68.1.1 (confirmed working by a second user).",
      "Upgrade Xamarin.Android SDK to a version where the linker supports modreq(InAttribute) on ReadOnlySpan<T>.",
      "Use SKBitmap.DecodeBounds(SKData) instead of DecodeBounds(byte[]): create an SKData via SKData.CreateCopy(buffer) and pass that to avoid the ReadOnlySpan code path."
    ],
    "nextQuestions": [
      "Does upgrading Xamarin.Android to 10.x or 11.x resolve the issue without downgrading SkiaSharp?",
      "Is this issue still present in SkiaSharp 2.x/3.x with modern .NET Android tooling?"
    ],
    "resolution": {
      "hypothesis": "The Xamarin.Android Mono.Linker in version 9.1.7.0 does not support modreq(InAttribute) used by C# code for ReadOnlySpan<T>.GetPinnableReference(). SkiaSharp 1.68.2 triggered this latent linker bug by introducing ReadOnlySpan<byte> overloads compiled with a newer Roslyn that emits the modreq.",
      "proposals": [
        {
          "title": "Use SKData overload as immediate workaround",
          "description": "Replace calls to SKBitmap.DecodeBounds(byte[]) with SKBitmap.DecodeBounds(SKData) to avoid the ReadOnlySpan code path that triggers the linker error.",
          "category": "workaround",
          "codeSnippet": "byte[] imageBytes = ...;\nusing var skData = SKData.CreateCopy(imageBytes);\nvar info = SKBitmap.DecodeBounds(skData);",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Upgrade Xamarin.Android SDK",
          "description": "Upgrade to a newer Xamarin.Android SDK version that includes a fixed Mono.Linker capable of handling modreq(InAttribute). This addresses the root cause.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use SKData overload as immediate workaround",
      "recommendedReason": "Immediate code change that avoids the problematic ReadOnlySpan code path without requiring a downgrade of SkiaSharp or waiting for an SDK upgrade."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.75,
      "reason": "Root cause is in the old Xamarin.Android linker. Needs verification of whether current SkiaSharp 2.x/3.x is still affected and whether upgrading Xamarin.Android SDK is sufficient.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core SkiaSharp area, Android platform, and compatibility tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Android",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post regression analysis with workaround options",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report and repro project. This is a known compatibility issue between SkiaSharp 1.68.2+ and older Xamarin.Android SDK versions.\n\nThe `DecodeBounds(ReadOnlySpan<byte>)` method introduced in 1.68.2 uses `fixed (byte* b = buffer)` which the old Xamarin.Android 9.1.7.0 Mono.Linker cannot process — it fails to resolve the `modreq(System.Runtime.InteropServices.InAttribute)` modifier on `GetPinnableReference()`. This issue has been confirmed by another user (regression from 1.68.1.1 → 1.68.2.0).\n\n**Workarounds:**\n1. **Downgrade SkiaSharp** to 1.68.1.1 (confirmed working)\n2. **Upgrade Xamarin.Android SDK** to a newer version with a fixed linker\n3. **Use the SKData overload** to avoid the ReadOnlySpan code path:\n```csharp\nbyte[] imageBytes = ...;\nusing var skData = SKData.CreateCopy(imageBytes);\nvar info = SKBitmap.DecodeBounds(skData);\n```\n\nThe root cause is tracked upstream: https://github.com/xamarin/Xamarin.Forms/issues/11295"
      }
    ]
  }
}
```

</details>
