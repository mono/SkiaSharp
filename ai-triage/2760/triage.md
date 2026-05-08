# Issue Triage Report — #2760

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T22:25:37Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.93 (93%)) |
| Suggested action | ready-to-fix (0.90 (90%)) |

**Issue Summary:** SKDrawable.Snapshot() crashes intermittently on Apple platforms (macOS, iOS, Mac Catalyst) due to a memory-management mismatch in the C++ shim that overrides sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() for the managed drawable.

**Analysis:** The C++ shim for the managed drawable overrides sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot(). The managed proxy (SKManagedDrawableMakePictureSnapshotProxyImplementation) returns a raw IntPtr handle from OnSnapshot(). When Skia wraps that raw pointer into a sk_sp<>, the ref-count mechanics differ from the plain-pointer return path (SkPicture* SkDrawable::onMakePictureSnapshot()), leading to a crash — likely a use-after-free or double-free on Apple platforms where the ABI or ref-counting behavior differs. The tests explicitly skip on macOS, iOS, and Mac Catalyst to avoid this crash.

**Recommendations:** **ready-to-fix** — Root cause is clearly identified by the maintainer: the C++ sk_sp<SkPicture> override in the managed drawable shim causes a ref-count mismatch crash on Apple platforms. The fix direction is known (use plain SkPicture* instead). Tests already document the skipped platforms.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/macOS, os/iOS |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a subclass of SKDrawable
2. Call drawable.Snapshot() on macOS or iOS
3. Observe intermittent crash

**Environment:** macOS (locally), not reproducible on CI

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/2630#discussion_r1493718896 — PR #2630 discussion where the maintainer (mattleibow) first identified the crash

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | crash when sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() is overridden for the managed drawable |
| Repro quality | partial |
| Target frameworks | net8.0-macos, net8.0-ios |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Tests still skip on Mac/iOS with this exact message; no fix has been merged. |

## Analysis

### Technical Summary

The C++ shim for the managed drawable overrides sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot(). The managed proxy (SKManagedDrawableMakePictureSnapshotProxyImplementation) returns a raw IntPtr handle from OnSnapshot(). When Skia wraps that raw pointer into a sk_sp<>, the ref-count mechanics differ from the plain-pointer return path (SkPicture* SkDrawable::onMakePictureSnapshot()), leading to a crash — likely a use-after-free or double-free on Apple platforms where the ABI or ref-counting behavior differs. The tests explicitly skip on macOS, iOS, and Mac Catalyst to avoid this crash.

### Rationale

The reporter (mattleibow, the maintainer) directly identified the C++ override as the root cause. Tests confirm it by skipping on Apple platforms. The crash is intermittent because memory corruption is non-deterministic. Switching from the sk_sp<> override to the plain SkPicture* override stops the crash.

### Key Signals

- "I override the sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() function in C++ for the managed drawable is causing a crash. If i change it to SkPicture* SkDrawable::onMakePictureSnapshot() stops the crash." — **issue body** (Root cause is clear: the sk_sp<> override introduces incorrect ref-counting when returning a raw pointer from the managed callback.)
- "SkipOnPlatform(IsMac || IsIOS || IsMacCatalyst, "sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() is causing issues on Apple platforms")" — **tests/Tests/SkiaSharp/SKDrawableTest.cs:39,52** (Tests are already aware of the bug and skip on Apple platforms — confirming the issue is known and unresolved.)
- "Not sure why it passes on CI" — **issue body** (Intermittency suggests a memory corruption / race in ref-count teardown, which is non-deterministic.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/DelegateProxies.drawable.cs` | 38-42 | direct | SKManagedDrawableMakePictureSnapshotProxyImplementation returns drawable.OnSnapshot()?.Handle — a raw IntPtr. This raw pointer is returned across the P/Invoke boundary to the C shim that wraps it in sk_sp<SkPicture>, but no extra ref is added, causing a use-after-free when the managed SKPicture is GC'd. |
| `binding/SkiaSharp/SKDrawable.cs` | 97-103 | direct | SKDrawable.OnSnapshot() creates a new SKPicture via SKPictureRecorder.EndRecording() and returns it. The returned SKPicture is not ref-incremented before being passed to the native sk_sp<>, so Skia's smart pointer could outlive the managed wrapper. |
| `tests/Tests/SkiaSharp/SKDrawableTest.cs` | 37-47 | direct | CanCreateSnapshot and CanUseAllMembers both skip on IsMac, IsIOS, and IsMacCatalyst explicitly due to this bug, confirming it is a known unresolved issue. |

### Next Questions

- Does changing the C shim from sk_sp<SkPicture> override to SkPicture* override fully resolve the crash on all Apple platforms?
- Should the managed callback add an explicit ref (sk_ref_sp) on the returned handle before transferring ownership to sk_sp<>?
- Does the crash occur on tvOS or watchOS as well?

### Resolution Proposals

**Hypothesis:** The fix is to change the C++ shim override from sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() to SkPicture* SkDrawable::onMakePictureSnapshot() — as the maintainer already identified. This avoids the ref-count mismatch.

1. **Change C shim override from sk_sp<> to plain pointer** — fix, confidence 0.90 (90%), cost/s, validated=untested
   - In the ManagedDrawable C++ shim, override SkPicture* onMakePictureSnapshot() instead of sk_sp<SkPicture>, matching the raw pointer that the managed callback returns.
2. **Remove SkipOnPlatform guards once fix is applied** — fix, confidence 0.90 (90%), cost/xs, validated=untested
   - After the C shim fix is verified on Apple platforms, remove the SkipOnPlatform calls in SKDrawableTest.cs so the tests run on macOS/iOS/Mac Catalyst.

**Recommended proposal:** Change C shim override from sk_sp<> to plain pointer

**Why:** This is the root-cause fix identified by the maintainer. The plain pointer return avoids the sk_sp ref-count mismatch.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.90 (90%) |
| Reason | Root cause is clearly identified by the maintainer: the C++ sk_sp<SkPicture> override in the managed drawable shim causes a ref-count mismatch crash on Apple platforms. The fix direction is known (use plain SkPicture* instead). Tests already document the skipped platforms. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply bug, SkiaSharp core, macOS and iOS platform, reliability labels | labels=type/bug, area/SkiaSharp, os/macOS, os/iOS, tenet/reliability |
| add-comment | medium | 0.90 (90%) | Confirm root cause and outline the fix | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for filing this! The root cause is clear: the C++ shim overrides `sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot()`, but the managed callback returns a raw `IntPtr` handle without incrementing the ref count. When Skia wraps that raw pointer into a `sk_sp<>`, it assumes ownership without an extra ref, causing a use-after-free when the managed `SKPicture` wrapper is GC'd.

As noted, switching the C++ override to `SkPicture* SkDrawable::onMakePictureSnapshot()` (plain pointer, no `sk_sp<>`) avoids the crash. Once that native change is in place, the `SkipOnPlatform` guards in `SKDrawableTest.cs` can be removed so the tests run on macOS/iOS/Mac Catalyst.

This requires rebuilding the native libraries after the C++ shim change (`dotnet cake --target=externals-macos`).
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2760,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T22:25:37Z"
  },
  "summary": "SKDrawable.Snapshot() crashes intermittently on Apple platforms (macOS, iOS, Mac Catalyst) due to a memory-management mismatch in the C++ shim that overrides sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() for the managed drawable.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.93
    },
    "platforms": [
      "os/macOS",
      "os/iOS"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "crash when sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() is overridden for the managed drawable",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-macos",
        "net8.0-ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a subclass of SKDrawable",
        "Call drawable.Snapshot() on macOS or iOS",
        "Observe intermittent crash"
      ],
      "environmentDetails": "macOS (locally), not reproducible on CI",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/2630#discussion_r1493718896",
          "description": "PR #2630 discussion where the maintainer (mattleibow) first identified the crash"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "Tests still skip on Mac/iOS with this exact message; no fix has been merged."
    }
  },
  "analysis": {
    "summary": "The C++ shim for the managed drawable overrides sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot(). The managed proxy (SKManagedDrawableMakePictureSnapshotProxyImplementation) returns a raw IntPtr handle from OnSnapshot(). When Skia wraps that raw pointer into a sk_sp<>, the ref-count mechanics differ from the plain-pointer return path (SkPicture* SkDrawable::onMakePictureSnapshot()), leading to a crash — likely a use-after-free or double-free on Apple platforms where the ABI or ref-counting behavior differs. The tests explicitly skip on macOS, iOS, and Mac Catalyst to avoid this crash.",
    "rationale": "The reporter (mattleibow, the maintainer) directly identified the C++ override as the root cause. Tests confirm it by skipping on Apple platforms. The crash is intermittent because memory corruption is non-deterministic. Switching from the sk_sp<> override to the plain SkPicture* override stops the crash.",
    "keySignals": [
      {
        "text": "I override the sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() function in C++ for the managed drawable is causing a crash. If i change it to SkPicture* SkDrawable::onMakePictureSnapshot() stops the crash.",
        "source": "issue body",
        "interpretation": "Root cause is clear: the sk_sp<> override introduces incorrect ref-counting when returning a raw pointer from the managed callback."
      },
      {
        "text": "SkipOnPlatform(IsMac || IsIOS || IsMacCatalyst, \"sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() is causing issues on Apple platforms\")",
        "source": "tests/Tests/SkiaSharp/SKDrawableTest.cs:39,52",
        "interpretation": "Tests are already aware of the bug and skip on Apple platforms — confirming the issue is known and unresolved."
      },
      {
        "text": "Not sure why it passes on CI",
        "source": "issue body",
        "interpretation": "Intermittency suggests a memory corruption / race in ref-count teardown, which is non-deterministic."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/DelegateProxies.drawable.cs",
        "lines": "38-42",
        "finding": "SKManagedDrawableMakePictureSnapshotProxyImplementation returns drawable.OnSnapshot()?.Handle — a raw IntPtr. This raw pointer is returned across the P/Invoke boundary to the C shim that wraps it in sk_sp<SkPicture>, but no extra ref is added, causing a use-after-free when the managed SKPicture is GC'd.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKDrawable.cs",
        "lines": "97-103",
        "finding": "SKDrawable.OnSnapshot() creates a new SKPicture via SKPictureRecorder.EndRecording() and returns it. The returned SKPicture is not ref-incremented before being passed to the native sk_sp<>, so Skia's smart pointer could outlive the managed wrapper.",
        "relevance": "direct"
      },
      {
        "file": "tests/Tests/SkiaSharp/SKDrawableTest.cs",
        "lines": "37-47",
        "finding": "CanCreateSnapshot and CanUseAllMembers both skip on IsMac, IsIOS, and IsMacCatalyst explicitly due to this bug, confirming it is a known unresolved issue.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Does changing the C shim from sk_sp<SkPicture> override to SkPicture* override fully resolve the crash on all Apple platforms?",
      "Should the managed callback add an explicit ref (sk_ref_sp) on the returned handle before transferring ownership to sk_sp<>?",
      "Does the crash occur on tvOS or watchOS as well?"
    ],
    "resolution": {
      "hypothesis": "The fix is to change the C++ shim override from sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() to SkPicture* SkDrawable::onMakePictureSnapshot() — as the maintainer already identified. This avoids the ref-count mismatch.",
      "proposals": [
        {
          "title": "Change C shim override from sk_sp<> to plain pointer",
          "description": "In the ManagedDrawable C++ shim, override SkPicture* onMakePictureSnapshot() instead of sk_sp<SkPicture>, matching the raw pointer that the managed callback returns.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Remove SkipOnPlatform guards once fix is applied",
          "description": "After the C shim fix is verified on Apple platforms, remove the SkipOnPlatform calls in SKDrawableTest.cs so the tests run on macOS/iOS/Mac Catalyst.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Change C shim override from sk_sp<> to plain pointer",
      "recommendedReason": "This is the root-cause fix identified by the maintainer. The plain pointer return avoids the sk_sp ref-count mismatch."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.9,
      "reason": "Root cause is clearly identified by the maintainer: the C++ sk_sp<SkPicture> override in the managed drawable shim causes a ref-count mismatch crash on Apple platforms. The fix direction is known (use plain SkPicture* instead). Tests already document the skipped platforms.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, SkiaSharp core, macOS and iOS platform, reliability labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/macOS",
          "os/iOS",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm root cause and outline the fix",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for filing this! The root cause is clear: the C++ shim overrides `sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot()`, but the managed callback returns a raw `IntPtr` handle without incrementing the ref count. When Skia wraps that raw pointer into a `sk_sp<>`, it assumes ownership without an extra ref, causing a use-after-free when the managed `SKPicture` wrapper is GC'd.\n\nAs noted, switching the C++ override to `SkPicture* SkDrawable::onMakePictureSnapshot()` (plain pointer, no `sk_sp<>`) avoids the crash. Once that native change is in place, the `SkipOnPlatform` guards in `SKDrawableTest.cs` can be removed so the tests run on macOS/iOS/Mac Catalyst.\n\nThis requires rebuilding the native libraries after the C++ shim change (`dotnet cake --target=externals-macos`)."
      }
    ]
  }
}
```

</details>
