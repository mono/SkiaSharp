# Bug Reproduction — Example Outputs

Reference examples showing valid `bug-repro` JSON output for different conclusion types.
Each example conforms to [`repro-schema.json`](repro-schema.json).

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
  "triageFile": "ai-triage/2997.json",
  "triageNotes": "Triage correctly identified this as a C# wrapper issue in SKMatrix. The suggested area (binding/SkiaSharp/SKMatrix.cs) was accurate.",
  "versionResults": [
    {
      "version": "2.88.9",
      "source": "nuget",
      "result": "reproduced",
      "notes": "Reporter's version. Bug confirmed."
    },
    {
      "version": "3.116.1",
      "source": "nuget",
      "result": "reproduced",
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
      "result": "success"
    },
    {
      "stepNumber": 2,
      "description": "Write a program that creates an SKMatrix with negative X-scale (-2, 1), maps SKRect(0, 0, 1, 1), and prints the resulting rect. If the bug exists, Left will be 0 instead of -2.",
      "layer": "csharp",
      "filesCreated": [
        {
          "filename": "Program.cs",
          "description": "Console app that creates SKMatrix.CreateScale(-2, 1), calls MapRect on unit rect, prints Left/Top/Right/Bottom values."
        }
      ],
      "result": null
    },
    {
      "stepNumber": 3,
      "description": "Run the reproduction. Expected Left=-2 but got Left=0, confirming the normalization bug.",
      "layer": "csharp",
      "command": "dotnet run --project Repro2997",
      "output": "Matrix: ScaleX=-2, ScaleY=1\nInput:  SKRect(0, 0, 1, 1)\nOutput: SKRect(0, 0, -2, 1) → normalized to SKRect(-2, 0, 0, 1)\nExpected Left=-2, Right=0 but got Left=-2, Right=0\nBUG: MapRect returns sorted rect, losing negative scale orientation",
      "result": "failure"
    },
    {
      "stepNumber": 4,
      "description": "Update to latest stable (3.116.1) and re-run to check if fixed.",
      "layer": "setup",
      "command": "dotnet add Repro2997 package SkiaSharp --version 3.116.1 && dotnet run --project Repro2997",
      "output": "Same result on 3.116.1 — MapRect still normalizes output rect.",
      "result": "failure"
    }
  ],
  "artifacts": null,
  "errorMessages": {
    "primaryError": "MapRect normalizes output rect, losing negative scale orientation",
    "stackTrace": null,
    "additionalErrors": null
  },
  "environment": {
    "os": "macOS 15.3",
    "arch": "arm64",
    "dotnetVersion": "10.0.100",
    "skiaSharpVersion": "2.88.9 (reporter), 3.116.1 (latest)",
    "dockerUsed": false
  },
  "blockers": null
}
```

### Why this is `reproduced`

- The conclusion is `reproduced` because the bug manifests as a **numerically wrong return
  value** from a C# API call (the rect coordinates are incorrect).
- Step 2 has `result: "failure"` — the assertion fails, satisfying the schema constraint
  that `reproduced` requires at least one step with `failure` or `wrong-output`.
- Step 3 narrows the bug to the C# layer by showing the C API returns correct values.

---

## Example 2: Rendering Bug — `wrong-output`

**Scenario (hypothetical):** An image decoded with `SKBitmap.Decode` has its colors inverted
compared to the source file. The process does not crash — it produces a valid PNG, but the
colors are wrong.

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3100,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2025-07-01T09:15:00Z"
  },
  "conclusion": "wrong-output",
  "notes": "Reproduced color inversion when decoding the reporter's test image (srgb-palette.png). The decoded bitmap has R and B channels swapped compared to the source. Saved the decoded output as decoded-output.png for comparison. This affects only palette-based PNGs with an embedded sRGB profile; JPEG and non-palette PNG decode correctly.",
  "reproductionTime": "~12 minutes",
  "triageFile": null,
  "triageNotes": null,
  "versionResults": [
    {
      "version": "3.119.0-preview.2",
      "source": "nuget",
      "result": "wrong-output",
      "notes": "Reporter's version. Color inversion confirmed."
    }
  ],
  "reproProject": {
    "type": "console",
    "tfm": "net10.0",
    "packages": [
      { "name": "SkiaSharp", "version": "3.119.0-preview.2" }
    ]
  },
  "reproductionSteps": [
    {
      "stepNumber": 1,
      "description": "Create a .NET 10 console project and add the SkiaSharp NuGet package.",
      "layer": "setup",
      "command": "dotnet new console -n ReproColorInversion && cd ReproColorInversion && dotnet add package SkiaSharp --version 3.119.0-preview.2",
      "output": "The template \"Console App\" was created successfully.\n  Determining projects to restore...\n  Restored ReproColorInversion.csproj.",
      "result": "success"
    },
    {
      "stepNumber": 2,
      "description": "Write a program that decodes the reporter's test image (srgb-palette.png) using SKBitmap.Decode and saves the result as decoded-output.png.",
      "layer": "csharp",
      "filesCreated": [
        {
          "filename": "Program.cs",
          "description": "Console app that loads srgb-palette.png with SKBitmap.Decode, then encodes to PNG and saves as decoded-output.png."
        }
      ],
      "result": null
    },
    {
      "stepNumber": 3,
      "description": "Run the console app. It should produce decoded-output.png without errors.",
      "layer": "csharp",
      "command": "dotnet run --project ReproColorInversion",
      "output": "Decoded image: 256x256, ColorType=Bgra8888\nSaved to decoded-output.png",
      "result": "success"
    },
    {
      "stepNumber": 4,
      "description": "Compare pixel values between the source and decoded images. Read the top-left pixel from each file — the source has R=200, G=50, B=30 but the decoded output has R=30, G=50, B=200, confirming R/B channel swap.",
      "layer": "csharp",
      "command": "dotnet run --project ReproColorInversion -- --compare",
      "output": "Source pixel (0,0): R=200 G=50 B=30 A=255\nDecoded pixel (0,0): R=30 G=50 B=200 A=255\nChannels R and B are swapped!",
      "result": "wrong-output"
    }
  ],
  "artifacts": [
    {
      "filename": "srgb-palette.png",
      "description": "Test image from issue reporter that triggers the color inversion. 256x256 palette-based PNG with embedded sRGB profile.",
      "source": "issue-attachment",
      "url": "https://github.com/mono/SkiaSharp/files/00000001/srgb-palette.png",
      "available": true
    },
    {
      "filename": "decoded-output.png",
      "description": "Output produced by the reproduction program showing inverted colors.",
      "source": "generated",
      "url": null,
      "available": true
    }
  ],
  "errorMessages": null,
  "environment": {
    "os": "macOS 15.3",
    "arch": "arm64",
    "dotnetVersion": "10.0.100",
    "skiaSharpVersion": "3.119.0-preview.2",
    "dockerUsed": false,
    "gpu": {
      "backend": "CPU",
      "available": false
    }
  },
  "blockers": null
}
```

### Why this is `wrong-output`

- The process **does not crash** — every command succeeds and exits cleanly.
- The bug is that the **decoded pixel data is incorrect** (R/B channels swapped).
- Step 4 has `result: "wrong-output"`, satisfying the schema constraint that `wrong-output`
  conclusions require at least one step with that result.
- The `errorMessages` field is `null` because there is no exception — just incorrect data.

---

## Example 3: Platform-Specific — `partial`

**Scenario (hypothetical):** A user reports that loading a specific `.otf` font crashes with
`AccessViolationException` on Linux but works fine on macOS. The reproduction environment
is macOS only — Linux is unavailable.

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3200,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2025-07-10T16:45:00Z"
  },
  "conclusion": "partial",
  "notes": "Tested on macOS arm64 — the font loads and renders successfully with no crash. The reporter's crash occurs only on Linux x64 (Ubuntu 22.04). Docker is not available in this environment so the Linux reproduction path could not be attempted. The font file itself is valid (FreeType and macOS CoreText both parse it), suggesting the crash may be related to Linux-specific font backend behavior in Skia.",
  "reproductionTime": "~15 minutes",
  "triageFile": null,
  "triageNotes": null,
  "versionResults": [
    {
      "version": "3.116.1",
      "source": "nuget",
      "result": "not-reproduced",
      "notes": "Works fine on macOS — crash is Linux-specific."
    }
  ],
  "reproProject": {
    "type": "console",
    "tfm": "net9.0",
    "packages": [
      { "name": "SkiaSharp", "version": "3.116.1" }
    ]
  },
  "reproductionSteps": [
    {
      "stepNumber": 1,
      "description": "Create a standalone console project with the reporter's SkiaSharp version.",
      "layer": "setup",
      "command": "dotnet new console -n Repro3200 && cd Repro3200 && dotnet add package SkiaSharp --version 3.116.1",
      "output": "The template \"Console App\" was created successfully.\n  Restored Repro3200.csproj.",
      "result": "success"
    },
    {
      "stepNumber": 2,
      "description": "Write a program that loads the problematic font file (CustomSerif.otf) using SKTypeface.FromFile, then draws text with it.",
      "layer": "csharp",
      "filesCreated": [
        {
          "filename": "Program.cs",
          "description": "Console app that loads CustomSerif.otf with SKTypeface.FromFile, draws 'Hello World' to a bitmap, and saves as output.png."
        }
      ],
      "result": null
    },
    {
      "stepNumber": 3,
      "description": "Run the reproduction on macOS. The font loads and renders correctly with no crash.",
      "layer": "csharp",
      "command": "dotnet run --project Repro3200",
      "output": "Typeface loaded: FamilyName=Custom Serif, GlyphCount=312\nRendered 'Hello World' at 48px to 400x100 bitmap.\nSaved to output.png — no errors.",
      "result": "success"
    },
    {
      "stepNumber": 4,
      "description": "Attempt to reproduce on Linux via Docker. Docker is not available in this environment.",
      "layer": "setup",
      "command": "docker --version",
      "output": "bash: docker: command not found",
      "result": "skip"
    }
  ],
  "artifacts": [
    {
      "filename": "CustomSerif.otf",
      "description": "OpenType font file from the issue reporter that triggers AccessViolationException on Linux. Contains 312 glyphs with CFF outlines.",
      "source": "issue-attachment",
      "url": "https://github.com/mono/SkiaSharp/files/00000002/CustomSerif.otf",
      "available": false
    }
  ],
  "errorMessages": null,
  "environment": {
    "os": "macOS 15.3",
    "arch": "arm64",
    "dotnetVersion": "10.0.100",
    "skiaSharpVersion": "3.116.1",
    "dockerUsed": false
  },
  "blockers": [
    "Linux environment not available for testing — Docker is not installed and no Linux host is accessible",
    "Reporter's font file (CustomSerif.otf) could not be downloaded from the issue attachment (link expired)"
  ]
}
```

### Why this is `partial`

- The bug was **not reproduced on macOS** — all steps passed on the available platform.
- The reporter's crash is **Linux-specific**, and Linux could not be tested (no Docker).
- The `blockers` array is **required** by the schema for `partial` conclusions and lists
  the specific reasons reproduction was incomplete.
- Step 4 uses `result: "skip"` to indicate the Linux path was attempted but could not run.
- The font artifact has `available: false` because the attachment link had expired.
