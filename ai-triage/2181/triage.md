# Issue Triage Report — #2181

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T16:46:00Z |
| Type | type/bug (0.80 (80%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-info (0.85 (85%)) |

**Issue Summary:** SkiaSharp crashes in Unity Editor due to lock issues in HandleDictionary; reporter worked around by replacing the default PlatformLock.Factory with a ReaderWriterLockSlim-based implementation, but the actual stack trace and crash details are missing.

**Analysis:** Reporter experiences a crash in SkiaSharp when used within Unity Editor and discovered a workaround via Discord: replacing PlatformLock.Factory with a custom IPlatformLock backed by ReaderWriterLockSlim. The workaround bypasses the NonAlertableWin32Lock (Win32 CRITICAL_SECTION path introduced in #1383) that SkiaSharp uses on Windows. The actual exception type, message, and stack trace were not provided — the linked gist was confirmed empty by maintainer @mattleibow. No SkiaSharp or Unity version info was provided and no response was given to the maintainer's follow-up.

**Recommendations:** **needs-info** — Stack trace is missing (gist is empty), no SkiaSharp or Unity version provided, and reporter did not respond to maintainer's follow-up request. Cannot investigate further without crash details.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://gist.github.com/craftercis/fe2f72c59d3bbe742b2f94d7cc7ce74f — Linked gist (empty/not a stack trace per maintainer comment)
- https://github.com/mono/SkiaSharp/issues/1383 — Original issue that introduced IPlatformLock / NonAlertableWin32Lock to fix Windows STA alertable-lock deadlock

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | crash |
| Error message | — |
| Repro quality | none |
| Target frameworks | — |

## Analysis

### Technical Summary

Reporter experiences a crash in SkiaSharp when used within Unity Editor and discovered a workaround via Discord: replacing PlatformLock.Factory with a custom IPlatformLock backed by ReaderWriterLockSlim. The workaround bypasses the NonAlertableWin32Lock (Win32 CRITICAL_SECTION path introduced in #1383) that SkiaSharp uses on Windows. The actual exception type, message, and stack trace were not provided — the linked gist was confirmed empty by maintainer @mattleibow. No SkiaSharp or Unity version info was provided and no response was given to the maintainer's follow-up.

### Rationale

Classified as type/bug because the reporter describes a crash, provides a workaround, and the title has [BUG]. Area is area/SkiaSharp because the crash is in the core HandleDictionary/PlatformLock layer. Platform is os/Windows-Classic because the workaround replaces the NonAlertableWin32Lock (Windows-only codepath). Suggested action is needs-info because the stack trace is missing, no version details were given, and the reporter did not respond to the maintainer's request for the stack trace.

### Key Signals

- "On Discord, we fixed this bug by adding this code to my script" — **issue body** (Reporter already has a working workaround — replacing PlatformLock.Factory with a ReaderWriterLockSlim-based IPlatformLock. Suggests the Win32 CRITICAL_SECTION path crashes in Unity Editor context.)
- "Here is my stacktrace: https://gist.github.com/craftercis/fe2f72c59d3bbe742b2f94d7cc7ce74f" — **issue body** (Gist URL is broken/does not contain a stack trace (confirmed by maintainer). The actual exception type and crash location are unknown.)
- "@craftercis do you still have that stack trace? Seems the gist is not a stack..." — **comment by mattleibow** (Maintainer confirmed the gist does not contain the stack trace. No follow-up from the reporter.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/PlatformLock.cs` | 70-77 | direct | DefaultFactory() returns NonAlertableWin32Lock on Windows (using Win32 CRITICAL_SECTION), and ReadWriteLock (ReaderWriterLockSlim) on non-Windows. Reporter's workaround replicates the internal ReadWriteLock class, indicating the crash is in the NonAlertableWin32Lock code path used on Windows. |
| `binding/SkiaSharp/PlatformLock.cs` | 99-175 | direct | NonAlertableWin32Lock uses Win32 CRITICAL_SECTION via Kernel32.dll P/Invoke (InitializeCriticalSectionEx, EnterCriticalSection, LeaveCriticalSection). Unity Editor may restrict Kernel32.dll access or have a scripting environment where this crashes. |
| `binding/SkiaSharp/HandleDictionary.cs` | 27 | related | instancesLock is created once via PlatformLock.Create(). The lock is used for all SKObject handle registration/deregistration. The PlatformLock.Factory must be set before any SkiaSharp objects are created. |

### Workarounds

- Set PlatformLock.Factory before using SkiaSharp: SkiaSharp.Internals.PlatformLock.Factory = () => new ReadWriteLock(); where ReadWriteLock wraps System.Threading.ReaderWriterLockSlim and implements IPlatformLock.

### Next Questions

- What is the actual crash/exception (type, message, stack trace)?
- What version of SkiaSharp is being used?
- What version of Unity Editor is being used (and on which OS)?
- Does the crash occur with the Win32 NonAlertableWin32Lock specifically, or also on macOS/Linux Unity Editor?
- Is it reproducible without the factory workaround after upgrading to a newer SkiaSharp version?

### Resolution Proposals

**Hypothesis:** The NonAlertableWin32Lock (Win32 CRITICAL_SECTION via Kernel32.dll P/Invoke) crashes in Unity Editor on Windows, likely because Unity's scripting environment restricts direct Win32 API access or has threading constraints that conflict with CRITICAL_SECTION initialization.

1. **Use PlatformLock.Factory workaround** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - Set PlatformLock.Factory before initializing SkiaSharp to use a ReaderWriterLockSlim-based lock instead of the Win32 path.

```csharp
// Set this BEFORE any SkiaSharp usage
SkiaSharp.Internals.PlatformLock.Factory = () => new ReadWriteLock();

class ReadWriteLock : SkiaSharp.Internals.IPlatformLock
{
    public void EnterReadLock() => _lock.EnterReadLock();
    public void ExitReadLock() => _lock.ExitReadLock();
    public void EnterWriteLock() => _lock.EnterWriteLock();
    public void ExitWriteLock() => _lock.ExitWriteLock();
    public void EnterUpgradeableReadLock() => _lock.EnterUpgradeableReadLock();
    public void ExitUpgradeableReadLock() => _lock.ExitUpgradeableReadLock();
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
}
```
2. **Investigate NonAlertableWin32Lock in Unity context** — investigation, confidence 0.60 (60%), cost/m, validated=untested
   - Obtain the full stack trace and determine why Kernel32.dll CRITICAL_SECTION calls crash in Unity Editor. May need to detect Unity runtime and fall back to ReaderWriterLockSlim.

**Recommended proposal:** Use PlatformLock.Factory workaround

**Why:** The workaround is confirmed to work by the reporter and is low-effort. Full investigation requires the missing stack trace.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.85 (85%) |
| Reason | Stack trace is missing (gist is empty), no SkiaSharp or Unity version provided, and reporter did not respond to maintainer's follow-up request. Cannot investigate further without crash details. |
| Suggested repro platform | windows |

### Missing Info

- Full stack trace / exception message for the crash
- SkiaSharp version number
- Unity Editor version and OS
- Does the crash reproduce without the PlatformLock.Factory workaround on latest SkiaSharp?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply bug, core, Windows, and reliability labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Ask for missing details and share the confirmed workaround | — |
| link-related | low | 0.95 (95%) | Cross-reference with #1383 which introduced the IPlatformLock abstraction | linkedIssue=#1383 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The gist link doesn't appear to contain a stack trace — could you share the actual exception message and stack trace?

To help investigate, please also provide:
- SkiaSharp version
- Unity Editor version and host OS

In the meantime, the workaround you found (setting `PlatformLock.Factory` before any SkiaSharp usage) is the right approach. Make sure the factory is set **before** any SkiaSharp objects are created:

```csharp
// Must be called before any SkiaSharp usage
SkiaSharp.Internals.PlatformLock.Factory = () => new ReadWriteLock();

class ReadWriteLock : SkiaSharp.Internals.IPlatformLock
{
    public void EnterReadLock() => _lock.EnterReadLock();
    public void ExitReadLock() => _lock.ExitReadLock();
    public void EnterWriteLock() => _lock.EnterWriteLock();
    public void ExitWriteLock() => _lock.ExitWriteLock();
    public void EnterUpgradeableReadLock() => _lock.EnterUpgradeableReadLock();
    public void ExitUpgradeableReadLock() => _lock.ExitUpgradeableReadLock();
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
}
```

This replaces the default Windows lock (a Win32 CRITICAL_SECTION) with a pure .NET `ReaderWriterLockSlim`. The related issue for context is #1383.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2181,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T16:46:00Z"
  },
  "summary": "SkiaSharp crashes in Unity Editor due to lock issues in HandleDictionary; reporter worked around by replacing the default PlatformLock.Factory with a ReaderWriterLockSlim-based implementation, but the actual stack trace and crash details are missing.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "crash",
      "reproQuality": "none"
    },
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://gist.github.com/craftercis/fe2f72c59d3bbe742b2f94d7cc7ce74f",
          "description": "Linked gist (empty/not a stack trace per maintainer comment)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1383",
          "description": "Original issue that introduced IPlatformLock / NonAlertableWin32Lock to fix Windows STA alertable-lock deadlock"
        }
      ]
    }
  },
  "analysis": {
    "summary": "Reporter experiences a crash in SkiaSharp when used within Unity Editor and discovered a workaround via Discord: replacing PlatformLock.Factory with a custom IPlatformLock backed by ReaderWriterLockSlim. The workaround bypasses the NonAlertableWin32Lock (Win32 CRITICAL_SECTION path introduced in #1383) that SkiaSharp uses on Windows. The actual exception type, message, and stack trace were not provided — the linked gist was confirmed empty by maintainer @mattleibow. No SkiaSharp or Unity version info was provided and no response was given to the maintainer's follow-up.",
    "rationale": "Classified as type/bug because the reporter describes a crash, provides a workaround, and the title has [BUG]. Area is area/SkiaSharp because the crash is in the core HandleDictionary/PlatformLock layer. Platform is os/Windows-Classic because the workaround replaces the NonAlertableWin32Lock (Windows-only codepath). Suggested action is needs-info because the stack trace is missing, no version details were given, and the reporter did not respond to the maintainer's request for the stack trace.",
    "keySignals": [
      {
        "text": "On Discord, we fixed this bug by adding this code to my script",
        "source": "issue body",
        "interpretation": "Reporter already has a working workaround — replacing PlatformLock.Factory with a ReaderWriterLockSlim-based IPlatformLock. Suggests the Win32 CRITICAL_SECTION path crashes in Unity Editor context."
      },
      {
        "text": "Here is my stacktrace: https://gist.github.com/craftercis/fe2f72c59d3bbe742b2f94d7cc7ce74f",
        "source": "issue body",
        "interpretation": "Gist URL is broken/does not contain a stack trace (confirmed by maintainer). The actual exception type and crash location are unknown."
      },
      {
        "text": "@craftercis do you still have that stack trace? Seems the gist is not a stack...",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer confirmed the gist does not contain the stack trace. No follow-up from the reporter."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/PlatformLock.cs",
        "lines": "70-77",
        "finding": "DefaultFactory() returns NonAlertableWin32Lock on Windows (using Win32 CRITICAL_SECTION), and ReadWriteLock (ReaderWriterLockSlim) on non-Windows. Reporter's workaround replicates the internal ReadWriteLock class, indicating the crash is in the NonAlertableWin32Lock code path used on Windows.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/PlatformLock.cs",
        "lines": "99-175",
        "finding": "NonAlertableWin32Lock uses Win32 CRITICAL_SECTION via Kernel32.dll P/Invoke (InitializeCriticalSectionEx, EnterCriticalSection, LeaveCriticalSection). Unity Editor may restrict Kernel32.dll access or have a scripting environment where this crashes.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/HandleDictionary.cs",
        "lines": "27",
        "finding": "instancesLock is created once via PlatformLock.Create(). The lock is used for all SKObject handle registration/deregistration. The PlatformLock.Factory must be set before any SkiaSharp objects are created.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Set PlatformLock.Factory before using SkiaSharp: SkiaSharp.Internals.PlatformLock.Factory = () => new ReadWriteLock(); where ReadWriteLock wraps System.Threading.ReaderWriterLockSlim and implements IPlatformLock."
    ],
    "nextQuestions": [
      "What is the actual crash/exception (type, message, stack trace)?",
      "What version of SkiaSharp is being used?",
      "What version of Unity Editor is being used (and on which OS)?",
      "Does the crash occur with the Win32 NonAlertableWin32Lock specifically, or also on macOS/Linux Unity Editor?",
      "Is it reproducible without the factory workaround after upgrading to a newer SkiaSharp version?"
    ],
    "resolution": {
      "hypothesis": "The NonAlertableWin32Lock (Win32 CRITICAL_SECTION via Kernel32.dll P/Invoke) crashes in Unity Editor on Windows, likely because Unity's scripting environment restricts direct Win32 API access or has threading constraints that conflict with CRITICAL_SECTION initialization.",
      "proposals": [
        {
          "title": "Use PlatformLock.Factory workaround",
          "description": "Set PlatformLock.Factory before initializing SkiaSharp to use a ReaderWriterLockSlim-based lock instead of the Win32 path.",
          "category": "workaround",
          "codeSnippet": "// Set this BEFORE any SkiaSharp usage\nSkiaSharp.Internals.PlatformLock.Factory = () => new ReadWriteLock();\n\nclass ReadWriteLock : SkiaSharp.Internals.IPlatformLock\n{\n    public void EnterReadLock() => _lock.EnterReadLock();\n    public void ExitReadLock() => _lock.ExitReadLock();\n    public void EnterWriteLock() => _lock.EnterWriteLock();\n    public void ExitWriteLock() => _lock.ExitWriteLock();\n    public void EnterUpgradeableReadLock() => _lock.EnterUpgradeableReadLock();\n    public void ExitUpgradeableReadLock() => _lock.ExitUpgradeableReadLock();\n    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();\n}",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Investigate NonAlertableWin32Lock in Unity context",
          "description": "Obtain the full stack trace and determine why Kernel32.dll CRITICAL_SECTION calls crash in Unity Editor. May need to detect Unity runtime and fall back to ReaderWriterLockSlim.",
          "category": "investigation",
          "confidence": 0.6,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use PlatformLock.Factory workaround",
      "recommendedReason": "The workaround is confirmed to work by the reporter and is low-effort. Full investigation requires the missing stack trace."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.85,
      "reason": "Stack trace is missing (gist is empty), no SkiaSharp or Unity version provided, and reporter did not respond to maintainer's follow-up request. Cannot investigate further without crash details.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Full stack trace / exception message for the crash",
      "SkiaSharp version number",
      "Unity Editor version and OS",
      "Does the crash reproduce without the PlatformLock.Factory workaround on latest SkiaSharp?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core, Windows, and reliability labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for missing details and share the confirmed workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the report! The gist link doesn't appear to contain a stack trace — could you share the actual exception message and stack trace?\n\nTo help investigate, please also provide:\n- SkiaSharp version\n- Unity Editor version and host OS\n\nIn the meantime, the workaround you found (setting `PlatformLock.Factory` before any SkiaSharp usage) is the right approach. Make sure the factory is set **before** any SkiaSharp objects are created:\n\n```csharp\n// Must be called before any SkiaSharp usage\nSkiaSharp.Internals.PlatformLock.Factory = () => new ReadWriteLock();\n\nclass ReadWriteLock : SkiaSharp.Internals.IPlatformLock\n{\n    public void EnterReadLock() => _lock.EnterReadLock();\n    public void ExitReadLock() => _lock.ExitReadLock();\n    public void EnterWriteLock() => _lock.EnterWriteLock();\n    public void ExitWriteLock() => _lock.ExitWriteLock();\n    public void EnterUpgradeableReadLock() => _lock.EnterUpgradeableReadLock();\n    public void ExitUpgradeableReadLock() => _lock.ExitUpgradeableReadLock();\n    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();\n}\n```\n\nThis replaces the default Windows lock (a Win32 CRITICAL_SECTION) with a pure .NET `ReaderWriterLockSlim`. The related issue for context is #1383."
      },
      {
        "type": "link-related",
        "description": "Cross-reference with #1383 which introduced the IPlatformLock abstraction",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 1383
      }
    ]
  }
}
```

</details>
