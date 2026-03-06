# Bug Reproduction — Example Outputs

Reference examples showing valid `issue-repro` JSON output for different conclusion types.
Each example conforms to [`repro-schema.json`](repro-schema.json).

## Contents
1. [Example 1: C# API Bug — `reproduced`](#example-1-c-api-bug--reproduced)
2. [Example 2: Platform-Specific Bug — `reproduced` with cross-platform verification](#example-2-platform-specific-bug--reproduced-with-cross-platform-verification)
3. [Example 3: Enhancement — `confirmed` (feature missing)](#example-3-enhancement--confirmed-feature-missing)

---

## Example 1: C# API Bug — `reproduced`

**Scenario:** `SKPath.Op` returns an empty path instead of the correct union when one of the
input paths contains a zero-length line segment. The operation silently succeeds but produces
an incorrect result.

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1501,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-01-20T14:32:00Z"
  },
  "conclusion": "reproduced",
  "notes": "Confirmed: SKPath.Op with SKPathOp.Union produces an empty path when one input contains a degenerate line segment (zero-length MoveTo+LineTo). Reproduced on both 2.88.9 and 3.118.0. The bug persists across all tested versions.",
  "reproductionTime": "~8 minutes",
  "inputs": {
    "triageFile": "ai-triage/1501.json"
  },
  "versionResults": [
    {
      "version": "2.88.9",
      "source": "nuget",
      "result": "reproduced",
      "platform": "host-windows-x64",
      "notes": "Reporter's version. Bug confirmed."
    },
    {
      "version": "3.118.0",
      "source": "nuget",
      "result": "reproduced",
      "platform": "host-windows-x64",
      "notes": "Latest stable. Bug still present."
    }
  ],
  "reproProject": {
    "type": "console",
    "tfm": "net9.0",
    "packages": [
      { "name": "SkiaSharp", "version": "2.88.9" }
    ]
  },
  "reproductionSteps": [
    {
      "stepNumber": 1,
      "description": "Create a standalone console project with the reporter's SkiaSharp version.",
      "layer": "setup",
      "command": "dotnet new console -n ReproPathOp && cd ReproPathOp && dotnet add package SkiaSharp --version 2.88.9",
      "output": "The template \"Console App\" was created successfully.\n  Determining projects to restore...\n  Restored ReproPathOp.csproj.",
      "exitCode": 0,
      "result": "success"
    },
    {
      "stepNumber": 2,
      "description": "Write a program that creates two paths (one with a degenerate segment), performs SKPath.Op union, and checks the result bounds.",
      "layer": "csharp",
      "filesCreated": [
        {
          "filename": "Program.cs",
          "description": "Console app that creates two SKPaths, one with a zero-length LineTo, performs Op(Union), and prints result bounds.",
          "content": "using SkiaSharp;\n\nvar path1 = new SKPath();\npath1.AddRect(SKRect.Create(10, 10, 100, 100));\n\nvar path2 = new SKPath();\npath2.MoveTo(50, 50);\npath2.LineTo(50, 50); // degenerate zero-length segment\npath2.AddRect(SKRect.Create(50, 50, 100, 100));\n\nvar result = path1.Op(path2, SKPathOp.Union);\nConsole.WriteLine($\"Result bounds: {result.Bounds}\");\nConsole.WriteLine($\"Result isEmpty: {result.IsEmpty}\");\nConsole.WriteLine(result.IsEmpty ? \"BUG: Union produced empty path\" : \"PASS\");"
        }
      ],
      "exitCode": 0,
      "result": "success"
    },
    {
      "stepNumber": 3,
      "description": "Run the reproduction. Expected a non-empty union but got an empty path.",
      "layer": "csharp",
      "command": "dotnet run --project ReproPathOp",
      "output": "Result bounds: {0, 0, 0, 0}\nResult isEmpty: True\nBUG: Union produced empty path",
      "exitCode": 1,
      "result": "failure"
    },
    {
      "stepNumber": 4,
      "description": "Test with latest stable (3.118.0) to check if already fixed.",
      "layer": "setup",
      "command": "cd .. && dotnet new console -n ReproPathOp-latest && cd ReproPathOp-latest && dotnet add package SkiaSharp --version 3.118.0 && cp ../ReproPathOp/Program.cs . && dotnet run",
      "output": "Same result on 3.118.0 — Op(Union) still produces empty path with degenerate segment.",
      "exitCode": 1,
      "result": "failure"
    }
  ],
  "errorMessages": {
    "primaryError": "SKPath.Op(Union) produces empty path when input contains degenerate line segment"
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Reproduced on both the reporter's version and latest stable; Op(Union) incorrectly produces empty path for valid input."
    },
    "actions": [
      {
        "type": "add-comment",
        "description": "Post reproduction findings and version matrix",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Reproduced on 2.88.9 and 3.118.0 with a minimal console app. When one input path contains a zero-length LineTo segment, Op(Union) returns an empty path instead of the correct union."
      }
    ],
    "proposedResponse": {
      "status": "ready",
      "summary": "Reproduced SKPath.Op(Union) empty result bug on 2.88.9 and 3.118.0.",
      "body": "I was able to reproduce this on both 2.88.9 and 3.118.0 with a minimal console app. When one path contains a degenerate zero-length LineTo, `SKPath.Op(SKPathOp.Union)` returns an empty path.\n\nVersions tested:\n- 2.88.9: reproduced\n- 3.118.0: reproduced"
    }
  },
  "environment": {
    "os": "Windows 11",
    "arch": "x64",
    "dotnetVersion": "9.0.200",
    "dotnetSdkVersion": "9.0.200",
    "skiaSharpVersion": "2.88.9 (reporter), 3.118.0 (latest)",
    "dockerUsed": false
  }
}
```

### Why this is `reproduced`

- The conclusion is `reproduced` because the bug manifests as a **numerically wrong return
  value** from a C# API call (the path operation returns incorrect results).
- Step 3 has `result: "failure"` — the assertion fails, satisfying the schema constraint
  that `reproduced` requires at least one step with `failure` or `wrong-output`.
- The reproduction is isolated to a minimal C# console app, making it easy to debug in the fix step.

---

## Example 2: Platform-Specific Bug — `reproduced` with cross-platform verification

**Scenario:** SKCanvas.DrawText renders garbled Unicode on Linux when the system has no
matching font installed. The same code works on Windows and macOS because those systems
have broader font fallback chains. Build succeeds — the bug only manifests at runtime.

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2502,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-02-15T14:30:00Z"
  },
  "inputs": {
    "triageFile": "ai-triage/2502.json"
  },
  "conclusion": "reproduced",
  "scope": "platform-specific/linux",
  "notes": "SKCanvas.DrawText renders replacement characters for CJK text on Ubuntu 22.04 minimal Docker image. The same code renders correctly on Windows and macOS. Root cause is likely fontconfig not finding a CJK fallback font. Reproduced on 3.116.1 and 3.118.0.",
  "assessment": "likely-bug",
  "reproductionTime": "~15 minutes",
  "versionResults": [
    {
      "version": "3.118.0",
      "source": "nuget",
      "result": "reproduced",
      "platform": "docker-linux-x64",
      "notes": "Replacement characters rendered instead of CJK glyphs"
    },
    {
      "version": "3.116.1",
      "source": "nuget",
      "result": "reproduced",
      "platform": "docker-linux-x64",
      "notes": "Same behavior on previous stable"
    },
    {
      "version": "3.118.0",
      "source": "nuget",
      "result": "not-reproduced",
      "platform": "host-windows-x64",
      "notes": "Cross-platform: Windows renders CJK text correctly with same version"
    }
  ],
  "reproProject": {
    "type": "console",
    "tfm": "net9.0",
    "packages": [{ "name": "SkiaSharp", "version": "3.118.0" }]
  },
  "reproductionSteps": [
    {
      "stepNumber": 1,
      "description": "Create console project with SkiaSharp and native Linux binaries",
      "layer": "setup",
      "command": "dotnet new console -n ReproCJK && cd ReproCJK && dotnet add package SkiaSharp --version 3.118.0 && dotnet add package SkiaSharp.NativeAssets.Linux --version 3.118.0",
      "exitCode": 0,
      "result": "success"
    },
    {
      "stepNumber": 2,
      "description": "Write program that renders CJK text to a PNG and checks pixel content",
      "layer": "csharp",
      "filesCreated": [
        {
          "filename": "Program.cs",
          "description": "Renders CJK text to bitmap, saves as PNG, checks if non-white pixels exist in text region.",
          "content": "using SkiaSharp;\nvar info = new SKImageInfo(200, 50);\nusing var bmp = new SKBitmap(info);\nusing var canvas = new SKCanvas(bmp);\ncanvas.Clear(SKColors.White);\nusing var paint = new SKPaint { Color = SKColors.Black, TextSize = 24 };\ncanvas.DrawText(\"CJK Test\", 10, 35, paint);\nusing var img = SKImage.FromBitmap(bmp);\nusing var data = img.Encode(SKEncodedImageFormat.Png, 100);\nusing var fs = File.OpenWrite(\"output.png\");\ndata.SaveTo(fs);\nConsole.WriteLine(bmp.GetPixel(15, 25) != SKColors.White ? \"PASS: text rendered\" : \"BUG: text not rendered (replacement chars)\");"
        }
      ],
      "exitCode": 0,
      "result": "success"
    },
    {
      "stepNumber": 3,
      "description": "Run in Docker (Ubuntu minimal) — expect garbled output",
      "layer": "csharp",
      "command": "docker run --rm -v $(pwd):/app -w /app mcr.microsoft.com/dotnet/sdk:9.0 dotnet run",
      "output": "BUG: text not rendered (replacement chars)",
      "exitCode": 1,
      "result": "failure"
    },
    {
      "stepNumber": 4,
      "description": "Run on Windows host — expect correct rendering",
      "layer": "csharp",
      "command": "dotnet run",
      "output": "PASS: text rendered",
      "exitCode": 0,
      "result": "success"
    }
  ],
  "errorMessages": {
    "primaryError": "CJK text renders as replacement characters on Linux without matching fonts installed"
  },
  "environment": {
    "os": "Ubuntu 22.04 (Docker)",
    "arch": "x64",
    "dotnetVersion": "9.0.200",
    "dotnetSdkVersion": "9.0.200",
    "skiaSharpVersion": "3.118.0",
    "dockerUsed": true
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Reproduced on Linux Docker, not on Windows; likely fontconfig fallback issue."
    },
    "actions": [
      {
        "type": "add-comment",
        "description": "Post reproduction findings and platform matrix",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Reproduced on Ubuntu 22.04 Docker with 3.118.0: CJK text renders as replacement characters. Same code renders correctly on Windows. This appears to be a fontconfig fallback issue on minimal Linux installations."
      }
    ],
    "proposedResponse": {
      "status": "ready",
      "summary": "Reproduced Linux-only CJK rendering issue on 3.118.0; Windows works fine.",
      "body": "I was able to reproduce this on Ubuntu 22.04 minimal Docker with 3.118.0: CJK text renders as replacement characters.\n\nPlatform matrix:\n- 3.118.0 (Docker Linux x64): reproduced\n- 3.116.1 (Docker Linux x64): reproduced\n- 3.118.0 (Windows x64): not reproduced\n\nThis appears to be a fontconfig fallback issue — minimal Linux installations lack matching fonts."
    }
  }
}
```

### Why this is a good cross-platform reproduction

- **Cross-platform verification** — Windows works, Linux doesn't → `scope: "platform-specific/linux"`.
- **Docker for reproducibility** — uses a standard Docker image for consistent Linux environment.
- **Tested multiple versions** — 3.116.1 and 3.118.0 both fail, not a regression.
- **Visual bug detected programmatically** — pixel check instead of visual inspection.
- **`scope` field** gives the fix skill a signal: look at Linux font fallback, not general text rendering.

---

## Example 3: Enhancement — `confirmed` (feature missing)

**Scenario:** "Add touch/stylus pressure sensitivity to Android SKCanvasView"

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3503,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-03-01T10:00:00Z"
  },
  "conclusion": "confirmed",
  "notes": "The SKTouchEventArgs class has a Pressure property, but the Android SKCanvasView touch handler does not read MotionEvent.GetPressure() and always passes 1.0f. The shared infrastructure is in place — this is a gap in the Android platform implementation.",
  "reproductionTime": "~10 minutes",
  "inputs": {
    "triageFile": "ai-triage/3503.json"
  },
  "assessment": "feature-request",
  "scope": "platform-specific/android",
  "environment": {
    "os": "Android 14",
    "arch": "arm64",
    "dotnetVersion": "9.0.200",
    "skiaSharpVersion": "3.118.0",
    "dockerUsed": false
  },
  "reproductionSteps": [
    {
      "stepNumber": 1,
      "description": "Create an Android project that uses SkiaSharp touch events",
      "layer": "setup",
      "command": "dotnet new android -n PressureTest && cd PressureTest && dotnet add package SkiaSharp.Views.Maui --version 3.118.0",
      "output": "Project created and package added successfully",
      "exitCode": 0,
      "result": "success"
    },
    {
      "stepNumber": 2,
      "description": "Search for pressure reading in Android touch handler",
      "layer": "investigation",
      "command": "grep -rn 'GetPressure\\|Pressure\\|pressure' source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/",
      "output": "No matches found",
      "exitCode": 0,
      "result": "success"
    },
    {
      "stepNumber": 3,
      "description": "Verify SKTouchEventArgs.Pressure property exists in shared code",
      "layer": "investigation",
      "command": "grep -rn 'Pressure' binding/SkiaSharp/SKTouchEventArgs.cs",
      "output": "Found Pressure property at line 42, default value 1.0f",
      "exitCode": 0,
      "result": "success"
    },
    {
      "stepNumber": 4,
      "description": "Check if iOS or other platforms implement pressure for comparison",
      "layer": "investigation",
      "command": "grep -rn 'GetPressure\\|Force\\|pressure' source/SkiaSharp.Views/SkiaSharp.Views/Platform/",
      "output": "Found in iOS (SKCanvasView.cs:95 — reads Touch.Force). Not found in Android or UWP.",
      "exitCode": 0,
      "result": "success"
    }
  ],
  "versionResults": [
    {
      "version": "3.118.0",
      "source": "nuget",
      "result": "confirmed",
      "notes": "Android touch handler does not read MotionEvent.GetPressure(). Pressure always reported as 1.0f.",
      "platform": "host-macos-arm64"
    },
    {
      "version": "3.116.1",
      "source": "nuget",
      "result": "confirmed",
      "notes": "Same gap in previous version.",
      "platform": "host-macos-arm64"
    }
  ],
  "reproProject": {
    "type": "console",
    "tfm": "net9.0-android",
    "packages": [
      {
        "name": "SkiaSharp.Views.Maui",
        "version": "3.118.0"
      }
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.90,
      "reason": "Feature gap confirmed — Android touch handler lacks pressure reading despite shared infrastructure existing."
    },
    "actions": [],
    "proposedResponse": {
      "status": "ready",
      "body": "We investigated and can confirm that touch pressure is not currently forwarded from Android's `MotionEvent.GetPressure()` to `SKTouchEventArgs.Pressure`.\n\nThe shared `SKTouchEventArgs.Pressure` property exists and iOS already reads `Touch.Force` — so the infrastructure is in place. The Android touch handler just needs to call `MotionEvent.GetPressure()` and pass it through.\n\nFlagged for implementation."
    }
  }
}
```

### Why this is a good enhancement confirmation

- **Used `confirmed` not `not-reproduced`** — semantically correct for verifying a feature gap.
- **Created a project** — demonstrates the gap is real, not just source reading.
- **Version testing** — confirmed the gap on both reporter's version and latest, proving it's not already fixed.
- **Scope set** — `platform-specific/android` signals the gap is Android-only.
- **Noted related infrastructure** — Pressure property and iOS implementation help the fix skill.
- **Assessment `feature-request`** — correctly classifies the type of confirmation.

---

> **Note:** Example 1 omits a step testing against the `main` branch for brevity.
> In real reproductions, always include a main-branch test step when a locally-built SkiaSharp
> is available. See the SKILL.md Phase 3C instructions.
