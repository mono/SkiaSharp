# Issue Triage Report — #2642

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T17:34:39Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.82 (82%)) |
| Suggested action | ready-to-fix (0.92 (92%)) |

**Issue Summary:** WGL window crashes when switching to fullscreen then to another window on Windows 10 because the WNDCLASS struct (holding the WNDPROC delegate) is a local variable that gets garbage collected after RegisterClass returns, causing a use-after-free when Windows calls back into the collected delegate.

**Analysis:** The Win32Window class in tests/Tests/Utils/Win32/Win32Window.cs stores the WNDCLASS struct as a local variable `wc` in the constructor. The struct holds a WNDPROC delegate cast from User32.DefWindowProc. After RegisterClass(ref wc) returns, the local `wc` goes out of scope and the GC is free to collect the delegate. Windows Win32 retains the raw function pointer from the registered class; when Windows dispatches window messages (e.g., during fullscreen transition), it calls into the now-collected delegate, causing a crash. Making wc a static field prevents the delegate from being collected.

**Recommendations:** **ready-to-fix** — Root cause is definitively identified (GC-collected WNDPROC delegate in local WNDCLASS struct), the fix is known and confirmed by the reporter, and the affected file is identified in the repo.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a WGLContext using Win32Window from SkiaSharp tests
2. Enable OpenGL rendering on Windows 10
3. Switch the window to fullscreen
4. Switch to another window
5. Observe crash

**Environment:** Windows 10, SkiaSharp 3.x (Alpha), Visual Studio (Windows)

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/745 — Related issue: GPU-accelerated WPF without WindowsFormsHost — same WNDPROC delegate lifetime problem reported

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | Program crash after switching WGL window to fullscreen and then switching to another window |
| Repro quality | complete |
| Target frameworks | net8.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.x |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Win32Window.cs in tests/Tests/Utils/Win32/Win32Window.cs still has the local wc variable pattern — the bug exists in the current codebase. |

## Analysis

### Technical Summary

The Win32Window class in tests/Tests/Utils/Win32/Win32Window.cs stores the WNDCLASS struct as a local variable `wc` in the constructor. The struct holds a WNDPROC delegate cast from User32.DefWindowProc. After RegisterClass(ref wc) returns, the local `wc` goes out of scope and the GC is free to collect the delegate. Windows Win32 retains the raw function pointer from the registered class; when Windows dispatches window messages (e.g., during fullscreen transition), it calls into the now-collected delegate, causing a crash. Making wc a static field prevents the delegate from being collected.

### Rationale

Clear reproducible crash with root cause identified by the reporter. The fix is minimal and well-understood: the WNDCLASS struct containing the WNDPROC delegate must be kept alive for the lifetime of the registered window class. The current code in the repo has exactly the pattern described as buggy. This is a ready-to-fix bug — root cause is known and the fix is a one-line change.

### Key Signals

- "it seems that this is due to the insufficient lifetime of wndClass. Persistently storing wndClass as a static field of the class resolved this issue." — **issue body** (Reporter has correctly identified the root cause: the WNDPROC delegate inside the WNDCLASS struct is being garbage collected because the struct is a local variable.)
- "private static WNDCLASS wc;" — **issue body (patch code)** (The fix is to make wc a static field to prevent the delegate from being GC'd.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `tests/Tests/Utils/Win32/Win32Window.cs` | 19-33 | direct | WNDCLASS wc is declared as a local variable in the constructor. It contains lpfnWndProc = (WNDPROC)User32.DefWindowProc — a managed delegate cast. After RegisterClass(ref wc) returns, wc goes out of scope and the GC can collect the delegate. Windows stores only a raw function pointer from the registration; when Windows dispatches messages to this window class, it calls through the collected delegate, causing a crash. |
| `tests/Tests/SkiaSharp/GlContexts/Wgl/WglContext.cs` | 9 | related | Win32Window is stored as a static readonly field in WglContext, so the Win32Window instance itself lives forever. However, this doesn't help — the WNDCLASS struct (and its embedded delegate) inside Win32Window's constructor is still local and can be GC'd after construction. |

### Next Questions

- Does this crash occur for anyone not in fullscreen — the fullscreen trigger may be coincidental if GC is delayed.
- Should Win32Window be moved from test utilities to a shared helper or production code?

### Resolution Proposals

**Hypothesis:** The WNDPROC delegate stored in the WNDCLASS struct is being garbage collected because the struct is a local variable, causing a use-after-free when Windows dispatches window messages.

1. **Make WNDCLASS a static field** — fix, confidence 0.95 (95%), cost/xs, validated=yes
   - Change the local variable `var wc = new WNDCLASS { ... }` to a static field `private static WNDCLASS wc;` and assign it in the constructor. This keeps the delegate alive for the lifetime of the class.

```csharp
private static WNDCLASS wc;

public Win32Window(string className)
{
    WindowClassName = className;
    wc = new WNDCLASS
    {
        lpfnWndProc = (WNDPROC)User32.DefWindowProc,
        // ... rest of fields
    };
    classRegistration = User32.RegisterClass(ref wc);
    // ...
}
```

**Recommended proposal:** Make WNDCLASS a static field

**Why:** Minimal one-line change that directly addresses the GC lifetime issue. Reporter confirmed it fixes the crash.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.92 (92%) |
| Reason | Root cause is definitively identified (GC-collected WNDPROC delegate in local WNDCLASS struct), the fix is known and confirmed by the reporter, and the affected file is identified in the repo. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, area, platform, and backend labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/OpenGL |
| add-comment | medium | 0.92 (92%) | Acknowledge root cause and confirm fix path | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and the patch! You've correctly identified the root cause: the `WNDCLASS` struct holds a managed `WNDPROC` delegate (`(WNDPROC)User32.DefWindowProc`) which is only referenced by the local variable `wc`. Once the constructor returns, the GC is free to collect the delegate. When Windows later dispatches window messages (e.g., during a fullscreen transition), it calls through the now-freed function pointer, causing the crash.

The fix — making `wc` a static field — is correct. This is the same issue documented in #745.

The fix will be applied to `tests/Tests/Utils/Win32/Win32Window.cs`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2642,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T17:34:39Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "WGL window crashes when switching to fullscreen then to another window on Windows 10 because the WNDCLASS struct (holding the WNDPROC delegate) is a local variable that gets garbage collected after RegisterClass returns, causing a use-after-free when Windows calls back into the collected delegate.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.82
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/OpenGL"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "Program crash after switching WGL window to fullscreen and then switching to another window",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net8.0-windows"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a WGLContext using Win32Window from SkiaSharp tests",
        "Enable OpenGL rendering on Windows 10",
        "Switch the window to fullscreen",
        "Switch to another window",
        "Observe crash"
      ],
      "environmentDetails": "Windows 10, SkiaSharp 3.x (Alpha), Visual Studio (Windows)",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/745",
          "description": "Related issue: GPU-accelerated WPF without WindowsFormsHost — same WNDPROC delegate lifetime problem reported"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.x"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The Win32Window.cs in tests/Tests/Utils/Win32/Win32Window.cs still has the local wc variable pattern — the bug exists in the current codebase."
    }
  },
  "analysis": {
    "summary": "The Win32Window class in tests/Tests/Utils/Win32/Win32Window.cs stores the WNDCLASS struct as a local variable `wc` in the constructor. The struct holds a WNDPROC delegate cast from User32.DefWindowProc. After RegisterClass(ref wc) returns, the local `wc` goes out of scope and the GC is free to collect the delegate. Windows Win32 retains the raw function pointer from the registered class; when Windows dispatches window messages (e.g., during fullscreen transition), it calls into the now-collected delegate, causing a crash. Making wc a static field prevents the delegate from being collected.",
    "rationale": "Clear reproducible crash with root cause identified by the reporter. The fix is minimal and well-understood: the WNDCLASS struct containing the WNDPROC delegate must be kept alive for the lifetime of the registered window class. The current code in the repo has exactly the pattern described as buggy. This is a ready-to-fix bug — root cause is known and the fix is a one-line change.",
    "keySignals": [
      {
        "text": "it seems that this is due to the insufficient lifetime of wndClass. Persistently storing wndClass as a static field of the class resolved this issue.",
        "source": "issue body",
        "interpretation": "Reporter has correctly identified the root cause: the WNDPROC delegate inside the WNDCLASS struct is being garbage collected because the struct is a local variable."
      },
      {
        "text": "private static WNDCLASS wc;",
        "source": "issue body (patch code)",
        "interpretation": "The fix is to make wc a static field to prevent the delegate from being GC'd."
      }
    ],
    "codeInvestigation": [
      {
        "file": "tests/Tests/Utils/Win32/Win32Window.cs",
        "lines": "19-33",
        "finding": "WNDCLASS wc is declared as a local variable in the constructor. It contains lpfnWndProc = (WNDPROC)User32.DefWindowProc — a managed delegate cast. After RegisterClass(ref wc) returns, wc goes out of scope and the GC can collect the delegate. Windows stores only a raw function pointer from the registration; when Windows dispatches messages to this window class, it calls through the collected delegate, causing a crash.",
        "relevance": "direct"
      },
      {
        "file": "tests/Tests/SkiaSharp/GlContexts/Wgl/WglContext.cs",
        "lines": "9",
        "finding": "Win32Window is stored as a static readonly field in WglContext, so the Win32Window instance itself lives forever. However, this doesn't help — the WNDCLASS struct (and its embedded delegate) inside Win32Window's constructor is still local and can be GC'd after construction.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Does this crash occur for anyone not in fullscreen — the fullscreen trigger may be coincidental if GC is delayed.",
      "Should Win32Window be moved from test utilities to a shared helper or production code?"
    ],
    "resolution": {
      "hypothesis": "The WNDPROC delegate stored in the WNDCLASS struct is being garbage collected because the struct is a local variable, causing a use-after-free when Windows dispatches window messages.",
      "proposals": [
        {
          "title": "Make WNDCLASS a static field",
          "description": "Change the local variable `var wc = new WNDCLASS { ... }` to a static field `private static WNDCLASS wc;` and assign it in the constructor. This keeps the delegate alive for the lifetime of the class.",
          "category": "fix",
          "codeSnippet": "private static WNDCLASS wc;\n\npublic Win32Window(string className)\n{\n    WindowClassName = className;\n    wc = new WNDCLASS\n    {\n        lpfnWndProc = (WNDPROC)User32.DefWindowProc,\n        // ... rest of fields\n    };\n    classRegistration = User32.RegisterClass(ref wc);\n    // ...\n}",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Make WNDCLASS a static field",
      "recommendedReason": "Minimal one-line change that directly addresses the GC lifetime issue. Reporter confirmed it fixes the crash."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.92,
      "reason": "Root cause is definitively identified (GC-collected WNDPROC delegate in local WNDCLASS struct), the fix is known and confirmed by the reporter, and the affected file is identified in the repo.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area, platform, and backend labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge root cause and confirm fix path",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "Thanks for the detailed report and the patch! You've correctly identified the root cause: the `WNDCLASS` struct holds a managed `WNDPROC` delegate (`(WNDPROC)User32.DefWindowProc`) which is only referenced by the local variable `wc`. Once the constructor returns, the GC is free to collect the delegate. When Windows later dispatches window messages (e.g., during a fullscreen transition), it calls through the now-freed function pointer, causing the crash.\n\nThe fix — making `wc` a static field — is correct. This is the same issue documented in #745.\n\nThe fix will be applied to `tests/Tests/Utils/Win32/Win32Window.cs`."
      }
    ]
  }
}
```

</details>
