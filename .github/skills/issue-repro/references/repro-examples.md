# Bug Reproduction — Example Outputs

Reference examples showing valid `issue-repro` JSON output for different conclusion types.
Each example conforms to [`repro-schema.json`](repro-schema.json).

## Contents
1. [Example 1: C# API Bug — `reproduced`](#example-1-c-api-bug--reproduced)
2. [Example 2: WASM/Blazor Bug — `reproduced` with cross-platform verification](#example-2-wasmblazor-bug--reproduced-with-cross-platform-verification)

---

## Example 1: C# API Bug — `reproduced`

**Scenario:** `SKMatrix.MapRect` normalizes the output rect, sorting coordinates so that
`Left < Right` and `Top < Bottom`. This discards negative scaling information — a matrix
that flips an axis produces a rect identical to one that doesn't.

Based on [#2997](https://github.com/mono/SkiaSharp/issues/2997).

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2997,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2025-06-20T14:32:00Z"
  },
  "conclusion": "reproduced",
  "notes": "Confirmed: SKMatrix.MapRect normalizes the output rect by sorting its coordinates. When a matrix contains a negative X-scale (e.g. ScaleX = -1), the mapped rect has Left/Right swapped back to ascending order, losing the sign of the scale. Reproduced on both 2.88.9 (reporter's version) and latest 3.116.1. The bug persists across all tested versions.",
  "reproductionTime": "~10 minutes",
  "inputs": {
    "triageFile": "ai-triage/2997.json"
  },
  "versionResults": [
    {
      "version": "2.88.9",
      "source": "nuget",
      "result": "reproduced",
      "platform": "host-macos-arm64",
      "notes": "Reporter's version. Bug confirmed."
    },
    {
      "version": "3.116.1",
      "source": "nuget",
      "result": "reproduced",
      "platform": "host-macos-arm64",
      "notes": "Latest stable. Bug still present."
    }
  ],
  "reproProject": {
    "type": "console",
    "tfm": "net8.0",
    "packages": [
      { "name": "SkiaSharp", "version": "2.88.9" }
    ]
  },
  "reproductionSteps": [
    {
      "stepNumber": 1,
      "description": "Create a standalone console project with the reporter's SkiaSharp version (2.88.9).",
      "layer": "setup",
      "command": "dotnet new console -n Repro2997 && cd Repro2997 && dotnet add package SkiaSharp --version 2.88.9",
      "output": "The template \"Console App\" was created successfully.\n  Determining projects to restore...\n  Restored Repro2997.csproj.",
      "exitCode": 0,
      "result": "success"
    },
    {
      "stepNumber": 2,
      "description": "Write a program that creates an SKMatrix with negative X-scale (-2, 1), maps SKRect(0, 0, 1, 1), and prints the resulting rect. If the bug exists, the output rect will be normalized (Left=-2, Right=0) instead of preserving the flip (Left=0, Right=-2).",
      "layer": "csharp",
      "filesCreated": [
        {
          "filename": "Program.cs",
          "description": "Console app that creates SKMatrix.CreateScale(-2, 1), calls MapRect on unit rect, prints Left/Top/Right/Bottom values.",
          "content": "using SkiaSharp;\n\nvar matrix = SKMatrix.CreateScale(-2, 1);\nvar source = new SKRect(0, 0, 1, 1);\nvar mapped = matrix.MapRect(source);\n\nConsole.WriteLine($\"Matrix: ScaleX={matrix.ScaleX}, ScaleY={matrix.ScaleY}\");\nConsole.WriteLine($\"Input:  SKRect({source.Left}, {source.Top}, {source.Right}, {source.Bottom})\");\nConsole.WriteLine($\"Output: SKRect({mapped.Left}, {mapped.Top}, {mapped.Right}, {mapped.Bottom})\");\nConsole.WriteLine($\"Expected Left=0, Right=-2 (preserve flip)\");\nConsole.WriteLine(mapped.Left == 0 && mapped.Right == -2 ? \"PASS\" : \"BUG: MapRect normalized the rect\");"
        }
      ],
      "exitCode": 0,
      "result": "success"
    },
    {
      "stepNumber": 3,
      "description": "Run the reproduction. Expected Left=0 and Right=-2 but got Left=-2 and Right=0, confirming the normalization bug.",
      "layer": "csharp",
      "command": "dotnet run --project Repro2997",
      "output": "Matrix: ScaleX=-2, ScaleY=1\nInput:  SKRect(0, 0, 1, 1)\nOutput: SKRect(-2, 0, 0, 1)\nExpected Left=0, Right=-2 (preserve flip) but got Left=-2, Right=0 (normalized)\nBUG: MapRect returns sorted rect, losing negative scale orientation",
      "exitCode": 1,
      "result": "failure"
    },
    {
      "stepNumber": 4,
      "description": "Create a fresh project with latest stable (3.116.1) and run to check if fixed.",
      "layer": "setup",
      "command": "cd .. && dotnet new console -n Repro2997-latest && cd Repro2997-latest && dotnet add package SkiaSharp --version 3.116.1 && cp ../Repro2997/Program.cs . && dotnet run",
      "output": "Same result on 3.116.1 — MapRect still normalizes output rect.",
      "exitCode": 1,
      "result": "failure"
    }
  ],
  "errorMessages": {
    "primaryError": "MapRect normalizes output rect, losing negative scale orientation"
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Reproduced on both the reporter's version and latest stable; MapRect returns a normalized rect that loses flip orientation (Left/Right ordering)."
    },
    "actions": [
      {
        "type": "add-comment",
        "description": "Post reproduction findings and version matrix",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Reproduced on 2.88.9 and 3.116.1 with a minimal console app. With `SKMatrix.CreateScale(-2, 1)` mapping `SKRect(0,0,1,1)`, the mapped rect is normalized (Left=-2, Right=0) instead of preserving the flip (Left=0, Right=-2)."
      }
    ],
    "proposedResponse": {
      "status": "ready",
      "summary": "Reproduced SKMatrix.MapRect normalization behavior on 2.88.9 and 3.116.1.",
      "body": "I was able to reproduce this on both 2.88.9 and 3.116.1 with a minimal console app. With `SKMatrix.CreateScale(-2, 1)` mapping `SKRect(0,0,1,1)`, the mapped rect is normalized (Left=-2, Right=0) instead of preserving the flip (Left=0, Right=-2).\n\nVersions tested:\n- 2.88.9: reproduced\n- 3.116.1: reproduced"
    }
  },
  "environment": {
    "os": "macOS 15.3",
    "arch": "arm64",
    "dotnetVersion": "10.0.2",
    "dotnetSdkVersion": "10.0.100",
    "skiaSharpVersion": "2.88.9 (reporter), 3.116.1 (latest)",
    "dockerUsed": false
  }
}
```

### Why this is `reproduced`

- The conclusion is `reproduced` because the bug manifests as a **numerically wrong return
  value** from a C# API call (the rect coordinates are incorrect).
- Step 3 has `result: "failure"` — the assertion fails, satisfying the schema constraint
  that `reproduced` requires at least one step with `failure` or `wrong-output`.
- The reproduction is isolated to a minimal C# console app calling `SKMatrix.MapRect`, making it easy to debug in the fix step.

---

## Example 2: WASM/Blazor Bug — `reproduced` with cross-platform verification

**Scenario:** SkiaSharp 3.119.2-preview.1 crashes at runtime in Blazor WASM with
`TypeInitializationException` on .NET 10. Build succeeds — the bug only manifests in
the browser. Cross-platform verification shows it's WASM-specific (console works fine).

Based on [#3422](https://github.com/mono/SkiaSharp/issues/3422).

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3422,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2025-06-15T14:30:00Z"
  },
  "inputs": {
    "triageFile": "ai-triage/3422.json"
  },
  "conclusion": "reproduced",
  "scope": "platform-specific/wasm",
  "notes": "TypeInitializationException in browser console when using SkiaSharp 3.119.2-preview.1 on .NET 10 Blazor WASM. Build succeeds but runtime crashes. 3.116.1 (stable) works perfectly — this is a regression in 3.118+ preview WASM native binaries. Cross-platform verification: console app works fine, confirming WASM-specific issue.",
  "assessment": "likely-bug",
  "reproductionTime": "~12 minutes",
  "versionResults": [
    {
      "version": "3.119.2-preview.1",
      "source": "nuget",
      "result": "reproduced",
      "platform": "wasm-blazor",
      "notes": "TypeInitializationException in browser console"
    },
    {
      "version": "3.116.1",
      "source": "nuget",
      "result": "not-reproduced",
      "platform": "wasm-blazor",
      "notes": "Canvas renders correctly, SUCCESS in console"
    },
    {
      "version": "3.119.2-preview.1",
      "source": "nuget",
      "result": "not-reproduced",
      "platform": "host-macos-arm64",
      "notes": "Cross-platform: console app works fine with same version"
    }
  ],
  "reproProject": {
    "type": "blazor-wasm",
    "tfm": "net10.0",
    "packages": [{ "name": "SkiaSharp.Views.Blazor", "version": "3.119.2-preview.1" }]
  },
  "reproductionSteps": [
    {
      "stepNumber": 1,
      "description": "Create Blazor WASM project with SkiaSharp.Views.Blazor",
      "layer": "setup",
      "command": "dotnet new blazorwasm -n Repro --framework net10.0 && cd Repro && dotnet add package SkiaSharp.Views.Blazor --version 3.119.2-preview.1",
      "exitCode": 0,
      "result": "success"
    },
    {
      "stepNumber": 2,
      "description": "Build with WasmBuildNative",
      "layer": "deployment",
      "command": "dotnet build",
      "exitCode": 0,
      "output": "Build succeeded. 0 Warning(s) 0 Error(s)",
      "result": "success"
    },
    {
      "stepNumber": 3,
      "description": "Serve and check browser console with Playwright",
      "layer": "csharp",
      "command": "dotnet run --urls http://localhost:5111",
      "output": "Error: [TypeInitialization_Type, SKObject]",
      "result": "failure"
    }
  ],
  "errorMessages": {
    "primaryError": "TypeInitialization_Type, SKObject",
    "additionalErrors": ["crit: Microsoft.AspNetCore.Components.WebAssembly.Rendering.WebAssemblyRenderer[100]"]
  },
  "environment": {
    "os": "macOS",
    "arch": "arm64",
    "dotnetVersion": "10.0.2",
    "dotnetSdkVersion": "10.0.100",
    "skiaSharpVersion": "3.119.2-preview.1",
    "dockerUsed": false
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Reproduced in Blazor WASM runtime (browser console) on 3.119.2-preview.1, while 3.116.1 is not reproduced; likely WASM-specific regression."
    },
    "actions": [
      {
        "type": "add-comment",
        "description": "Post reproduction findings and version/platform matrix",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Reproduced on Blazor WASM with 3.119.2-preview.1: runtime TypeInitializationException appears in the browser console (build succeeds). 3.116.1 stable does not reproduce. A console app on macOS with 3.119.2-preview.1 also does not reproduce, suggesting a WASM-specific regression."
      }
    ],
    "proposedResponse": {
      "status": "ready",
      "summary": "Reproduced WASM-only runtime crash on 3.119.2-preview.1; 3.116.1 is OK.",
      "body": "I was able to reproduce this in a Blazor WASM app on 3.119.2-preview.1: the project builds, but at runtime the browser console shows a `TypeInitializationException` (`TypeInitialization_Type, SKObject`).\n\nVersion/platform matrix:\n- 3.119.2-preview.1 (wasm-blazor): reproduced\n- 3.116.1 (wasm-blazor): not reproduced\n- 3.119.2-preview.1 (host macOS console): not reproduced\n\nThis looks like a WASM-specific regression introduced in the preview builds (3.118+)."
    }
  },
  "feedback": {
    "corrections": [
      {
        "source": "triage",
        "topic": "root-cause",
        "upstream": "Triage concluded: SkiaSharp doesn't officially support .NET 10",
        "corrected": "net8.0 libraries are forward-compatible with net10.0 via TFM fallback. 3.116.1 works on net10.0. The real bug is a regression in 3.118+ preview WASM native binaries."
      }
    ]
  }
}
```

### Why this is a good WASM reproduction

- **Did not stop at build success** — build passed but the bug is runtime-only in browser.
- **Used Playwright** to navigate, read browser console errors, and verify.
- **Tested multiple versions** — found 3.116.1 works, proving it's a 3.118+ regression.
- **Cross-platform verification** — console app works fine → `scope: "platform-specific/wasm"`.
- **Corrected triage** — triage wrongly blamed missing net10.0 TFM; repro proved forward-compat works.
- **`scope` field** gives the fix skill an immediate signal: look at WASM native binaries, not TFM support.

---

> **Note:** Both examples omit a Phase 3C step (testing against the `main` branch) for brevity.
> In real reproductions, always include a main-branch test step when a locally-built SkiaSharp
> is available. See the SKILL.md Phase 3C instructions.
