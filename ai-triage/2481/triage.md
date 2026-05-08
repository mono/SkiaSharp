# Issue Triage Report — #2481

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T23:30:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/libHarfBuzzSharp.native (0.95 (95%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** libHarfBuzzSharp.dylib shipped in the NuGet package is only adhoc-signed (not with a Developer ID certificate), preventing macOS notarization of applications that bundle the library.

**Analysis:** The libHarfBuzzSharp.dylib distributed in NuGet packages is signed with an adhoc code signature rather than a valid Apple Developer ID certificate. Apple's notarization service rejects any binary not signed with a Developer ID and a secure timestamp. The build script's StripSign() function in scripts/cake/xcode.cake applies --sign - (adhoc) with --timestamp=none. The same issue previously affected libSkiaSharp.dylib (issue #1156) and was reportedly fixed in PR #2932; it is unclear whether that fix extended to libHarfBuzzSharp as well.

**Recommendations:** **needs-investigation** — The issue is real and well-documented, but it needs verification of whether PR #2932 already fixed libHarfBuzzSharp.dylib or only libSkiaSharp.dylib. If already fixed, this should be closed as fixed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libHarfBuzzSharp.native |
| Platforms | os/macOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Build a .NET 6 app referencing SkiaSharp.HarfBuzz 2.88.2 on macOS.
2. Attempt to notarize the output directory containing the app or installer.
3. Notarization fails: libHarfBuzzSharp.dylib is not signed with a valid Developer ID certificate and lacks a secure timestamp.

**Related issues:** #1156

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/2932 — PR that addressed macOS dylib signing for libSkiaSharp.dylib, referenced when closing issue #1156.

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | platform-specific |
| Error message | The binary is not signed with a valid Developer ID certificate. / The signature does not include a secure timestamp. |
| Repro quality | partial |
| Target frameworks | net6.0 |

## Analysis

### Technical Summary

The libHarfBuzzSharp.dylib distributed in NuGet packages is signed with an adhoc code signature rather than a valid Apple Developer ID certificate. Apple's notarization service rejects any binary not signed with a Developer ID and a secure timestamp. The build script's StripSign() function in scripts/cake/xcode.cake applies --sign - (adhoc) with --timestamp=none. The same issue previously affected libSkiaSharp.dylib (issue #1156) and was reportedly fixed in PR #2932; it is unclear whether that fix extended to libHarfBuzzSharp as well.

### Rationale

Classified as type/bug in area/libHarfBuzzSharp.native because the native dylib is shipped with only an adhoc signature, preventing notarization of macOS apps using SkiaSharp.HarfBuzz. Related issue #1156 (libSkiaSharp.dylib) was closed as fixed via PR #2932; this issue may be a gap if HarfBuzz was not included in that fix. Severity is medium because a workaround exists (manually codesign the dylib with your own Developer ID before notarizing).

### Key Signals

- "The binary is not signed with a valid Developer ID certificate." — **Apple notarization JSON output** (Apple requires Developer ID signing for notarization; adhoc signing is not accepted.)
- "The signature does not include a secure timestamp." — **Apple notarization JSON output** (The --timestamp=none flag in StripSign causes this; notarization requires a secure timestamp.)
- "I have now started signing the dylib for macOS: https://github.com/mono/SkiaSharp/pull/2932" — **mattleibow comment on issue #1156** (PR #2932 addressed signing for macOS dylibs, but scope (libSkiaSharp only vs both) needs verification.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `scripts/cake/xcode.cake` | 30-50 | direct | StripSign() signs with `--force --sign - --timestamp=none`, producing an adhoc signature. This is the code path used when building all macOS dylibs including libHarfBuzzSharp. |
| `native/macos/build.cake` | 56-77 | direct | libHarfBuzzSharp task builds via RunXCodeBuild for x86_64 and arm64 then calls CreateFatDylib. No additional Developer ID signing step is present beyond the StripSign adhoc path. |

### Workarounds

- Manually codesign libHarfBuzzSharp.dylib with your Developer ID before notarizing: codesign --force --sign "Developer ID Application: Your Name (TEAMID)" --timestamp runtimes/osx/native/libHarfBuzzSharp.dylib

### Next Questions

- Did PR #2932 also sign libHarfBuzzSharp.dylib, or only libSkiaSharp.dylib?
- Is a newer version of SkiaSharp.HarfBuzz available (post-2.88.2) that ships a properly signed dylib?

### Resolution Proposals

**Hypothesis:** The build pipeline signs libHarfBuzzSharp.dylib with an adhoc signature; it needs to be signed with the Microsoft/Xamarin Developer ID certificate (with secure timestamp) before packaging into the NuGet.

1. **Manually codesign libHarfBuzzSharp.dylib before notarizing** — workaround, cost/xs, validated=untested
   - The developer can re-sign the dylib with their own Developer ID certificate before notarizing their application.
2. **Sign libHarfBuzzSharp.dylib with Developer ID in CI** — fix, cost/s, validated=untested
   - Update the build pipeline to sign libHarfBuzzSharp.dylib with the Microsoft/Xamarin Developer ID certificate (with secure timestamp), similar to what PR #2932 did for libSkiaSharp.dylib.

**Recommended proposal:** Manually codesign libHarfBuzzSharp.dylib before notarizing

**Why:** The workaround unblocks the reporter immediately. The proper fix requires CI pipeline changes and a new NuGet release.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | The issue is real and well-documented, but it needs verification of whether PR #2932 already fixed libHarfBuzzSharp.dylib or only libSkiaSharp.dylib. If already fixed, this should be closed as fixed. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply classification labels | labels=type/bug, area/libHarfBuzzSharp.native, os/macOS, tenet/compatibility |
| link-related | low | 0.95 (95%) | Cross-reference related issue #1156 (same problem for libSkiaSharp.dylib) | linkedIssue=#1156 |
| add-comment | medium | 0.80 (80%) | Provide workaround and note the related fix in PR #2932 | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the report! This is the same class of issue as #1156 (which was about `libSkiaSharp.dylib`) — the `libHarfBuzzSharp.dylib` shipped in the NuGet is signed with only an adhoc signature rather than a Developer ID certificate, which Apple requires for notarization.

**Workaround (immediate):** You can re-sign the dylib with your own Developer ID certificate before notarizing:

```bash
codesign --force \
  --sign "Developer ID Application: Your Name (TEAMID)" \
  --timestamp \
  runtimes/osx/native/libHarfBuzzSharp.dylib
```

PR #2932 addressed signing for macOS dylibs — we're investigating whether `libHarfBuzzSharp.dylib` was included. If not, a follow-up fix is needed in the build pipeline. You may also want to try a newer version of `SkiaSharp.HarfBuzz` to see if the issue has been resolved in a subsequent release.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2481,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T23:30:00Z"
  },
  "summary": "libHarfBuzzSharp.dylib shipped in the NuGet package is only adhoc-signed (not with a Developer ID certificate), preventing macOS notarization of applications that bundle the library.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/libHarfBuzzSharp.native",
      "confidence": 0.95
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
      "errorType": "platform-specific",
      "errorMessage": "The binary is not signed with a valid Developer ID certificate. / The signature does not include a secure timestamp.",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Build a .NET 6 app referencing SkiaSharp.HarfBuzz 2.88.2 on macOS.",
        "Attempt to notarize the output directory containing the app or installer.",
        "Notarization fails: libHarfBuzzSharp.dylib is not signed with a valid Developer ID certificate and lacks a secure timestamp."
      ],
      "relatedIssues": [
        1156
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/2932",
          "description": "PR that addressed macOS dylib signing for libSkiaSharp.dylib, referenced when closing issue #1156."
        }
      ]
    }
  },
  "analysis": {
    "summary": "The libHarfBuzzSharp.dylib distributed in NuGet packages is signed with an adhoc code signature rather than a valid Apple Developer ID certificate. Apple's notarization service rejects any binary not signed with a Developer ID and a secure timestamp. The build script's StripSign() function in scripts/cake/xcode.cake applies --sign - (adhoc) with --timestamp=none. The same issue previously affected libSkiaSharp.dylib (issue #1156) and was reportedly fixed in PR #2932; it is unclear whether that fix extended to libHarfBuzzSharp as well.",
    "codeInvestigation": [
      {
        "file": "scripts/cake/xcode.cake",
        "lines": "30-50",
        "finding": "StripSign() signs with `--force --sign - --timestamp=none`, producing an adhoc signature. This is the code path used when building all macOS dylibs including libHarfBuzzSharp.",
        "relevance": "direct"
      },
      {
        "file": "native/macos/build.cake",
        "lines": "56-77",
        "finding": "libHarfBuzzSharp task builds via RunXCodeBuild for x86_64 and arm64 then calls CreateFatDylib. No additional Developer ID signing step is present beyond the StripSign adhoc path.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "The binary is not signed with a valid Developer ID certificate.",
        "source": "Apple notarization JSON output",
        "interpretation": "Apple requires Developer ID signing for notarization; adhoc signing is not accepted."
      },
      {
        "text": "The signature does not include a secure timestamp.",
        "source": "Apple notarization JSON output",
        "interpretation": "The --timestamp=none flag in StripSign causes this; notarization requires a secure timestamp."
      },
      {
        "text": "I have now started signing the dylib for macOS: https://github.com/mono/SkiaSharp/pull/2932",
        "source": "mattleibow comment on issue #1156",
        "interpretation": "PR #2932 addressed signing for macOS dylibs, but scope (libSkiaSharp only vs both) needs verification."
      }
    ],
    "rationale": "Classified as type/bug in area/libHarfBuzzSharp.native because the native dylib is shipped with only an adhoc signature, preventing notarization of macOS apps using SkiaSharp.HarfBuzz. Related issue #1156 (libSkiaSharp.dylib) was closed as fixed via PR #2932; this issue may be a gap if HarfBuzz was not included in that fix. Severity is medium because a workaround exists (manually codesign the dylib with your own Developer ID before notarizing).",
    "workarounds": [
      "Manually codesign libHarfBuzzSharp.dylib with your Developer ID before notarizing: codesign --force --sign \"Developer ID Application: Your Name (TEAMID)\" --timestamp runtimes/osx/native/libHarfBuzzSharp.dylib"
    ],
    "nextQuestions": [
      "Did PR #2932 also sign libHarfBuzzSharp.dylib, or only libSkiaSharp.dylib?",
      "Is a newer version of SkiaSharp.HarfBuzz available (post-2.88.2) that ships a properly signed dylib?"
    ],
    "resolution": {
      "hypothesis": "The build pipeline signs libHarfBuzzSharp.dylib with an adhoc signature; it needs to be signed with the Microsoft/Xamarin Developer ID certificate (with secure timestamp) before packaging into the NuGet.",
      "proposals": [
        {
          "title": "Manually codesign libHarfBuzzSharp.dylib before notarizing",
          "description": "The developer can re-sign the dylib with their own Developer ID certificate before notarizing their application.",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Sign libHarfBuzzSharp.dylib with Developer ID in CI",
          "description": "Update the build pipeline to sign libHarfBuzzSharp.dylib with the Microsoft/Xamarin Developer ID certificate (with secure timestamp), similar to what PR #2932 did for libSkiaSharp.dylib.",
          "category": "fix",
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Manually codesign libHarfBuzzSharp.dylib before notarizing",
      "recommendedReason": "The workaround unblocks the reporter immediately. The proper fix requires CI pipeline changes and a new NuGet release."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "The issue is real and well-documented, but it needs verification of whether PR #2932 already fixed libHarfBuzzSharp.dylib or only libSkiaSharp.dylib. If already fixed, this should be closed as fixed.",
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
          "area/libHarfBuzzSharp.native",
          "os/macOS",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference related issue #1156 (same problem for libSkiaSharp.dylib)",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 1156
      },
      {
        "type": "add-comment",
        "description": "Provide workaround and note the related fix in PR #2932",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thank you for the report! This is the same class of issue as #1156 (which was about `libSkiaSharp.dylib`) — the `libHarfBuzzSharp.dylib` shipped in the NuGet is signed with only an adhoc signature rather than a Developer ID certificate, which Apple requires for notarization.\n\n**Workaround (immediate):** You can re-sign the dylib with your own Developer ID certificate before notarizing:\n\n```bash\ncodesign --force \\\n  --sign \"Developer ID Application: Your Name (TEAMID)\" \\\n  --timestamp \\\n  runtimes/osx/native/libHarfBuzzSharp.dylib\n```\n\nPR #2932 addressed signing for macOS dylibs — we're investigating whether `libHarfBuzzSharp.dylib` was included. If not, a follow-up fix is needed in the build pipeline. You may also want to try a newer version of `SkiaSharp.HarfBuzz` to see if the issue has been resolved in a subsequent release."
      }
    ]
  }
}
```

</details>
