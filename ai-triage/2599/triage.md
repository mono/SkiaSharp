# Issue Triage Report — #2599

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T21:44:09Z |
| Type | type/question (0.87 (87%)) |
| Area | area/SkiaSharp (0.85 (85%)) |
| Suggested action | close-as-not-a-bug (0.87 (87%)) |

**Issue Summary:** Reporter cannot load bitmaps and fonts from the Resources/Images folder on Android (MAUI), receiving a file-not-found exception; the same code works on Windows. Community member identified this as Android resource system behavior, not a SkiaSharp issue.

**Analysis:** The reporter is attempting to load MAUI resources (images/fonts) via standard file paths, which works on Windows but fails on Android because MAUI's Resources/Images and Resources/Raw folders are packaged as Android resources/assets — not accessible via regular filesystem paths. SkiaSharp itself works correctly when given valid stream or file path inputs; the fix is to use MAUI's FileSystem.OpenAppPackageFileAsync() to obtain a stream, then pass it to SKImage.FromEncodedData(Stream) or SKTypeface.FromStream(Stream). A community member correctly identified this as Android resource system behavior, not a SkiaSharp bug.

**Recommendations:** **close-as-not-a-bug** — SkiaSharp works correctly — the issue is Android's resource packaging system. Reporter is using file-path APIs for resources only accessible via Android AssetManager or MAUI's FileSystem.OpenAppPackageFileAsync(). Community member already provided the correct answer. The regression claim (2.88.2 → 2.88.3) is not credible for this category of problem.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Android |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a MAUI app with bitmaps in Resources/Images and fonts in Resources/Fonts
2. Try to load them using file paths with File.Exists() on Android
3. Observe file-not-found exception on Android

**Environment:** SkiaSharp 2.88.3, Android Emulator, Visual Studio on Windows

**Attachments:**
- Cyclone.zip — https://github.com/mono/SkiaSharp/files/12542816/Cyclone.zip — Reporter's sample project

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.2 |
| Worked in | 2.88.2 |
| Broke in | 2.88.3 |
| Current relevance | unlikely |
| Relevance reason | The claimed regression from 2.88.2 to 2.88.3 is unlikely to be caused by SkiaSharp changes; Android resource system behavior is platform-fundamental and unaffected by SkiaSharp version. Reporter likely made a setup or project configuration change alongside the upgrade. |

## Analysis

### Technical Summary

The reporter is attempting to load MAUI resources (images/fonts) via standard file paths, which works on Windows but fails on Android because MAUI's Resources/Images and Resources/Raw folders are packaged as Android resources/assets — not accessible via regular filesystem paths. SkiaSharp itself works correctly when given valid stream or file path inputs; the fix is to use MAUI's FileSystem.OpenAppPackageFileAsync() to obtain a stream, then pass it to SKImage.FromEncodedData(Stream) or SKTypeface.FromStream(Stream). A community member correctly identified this as Android resource system behavior, not a SkiaSharp bug.

### Rationale

Reporter is confused about Android's resource packaging model. File.Exists() returns false on Android for resources stored in the APK because they are not exposed as regular filesystem paths. SKTypeface.FromFile() and SKImage.FromEncodedData(string) both require real filesystem paths. Community comment #1709260086 confirms the root cause and provides the correct approach. The regression claim (2.88.2 → 2.88.3) is not credible for this category of issue — Android resource access has not changed in SkiaSharp across those versions.

### Key Signals

- "I checked the file with File.Exist a function for the file, but I could not find the file name." — **issue body** (Using standard filesystem API on Android for packaged resources — these are not exposed as file paths in Android.)
- "It works fine with Windows." — **issue body** (Windows exposes project resources via the file system; Android does not — classic cross-platform resource access mismatch.)
- "This is not error of skiasharp, Maui deal with Resources as native resource, every platform is different" — **comment by xtuzy (comment #1709260086)** (Community member correctly identifies this as Android resource system behavior, not a SkiaSharp defect.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 89-90 | direct | SKTypeface.FromFile(string path) delegates to SKFontManager.Default.CreateTypeface(path, index) — requires a valid filesystem path; cannot directly accept Android asset paths. |
| `binding/SkiaSharp/SKImage.cs` | 223-233 | direct | SKImage.FromEncodedData(string filename) creates an SKData from the filesystem path via SKData.Create(filename) — returns null if the file is not on the real filesystem (as is the case for Android APK assets). |
| `binding/SkiaSharp/SKTypeface.cs` | 92-93 | related | SKTypeface.FromStream(Stream stream) exists as a public API accepting a managed Stream — this is the correct API to use when loading from an Android AssetManager or MAUI FileSystem stream. |
| `binding/SkiaSharp/SKImage.cs` | 211-219 | related | SKImage.FromEncodedData(Stream data) accepts a managed Stream — correct API for Android when combined with FileSystem.OpenAppPackageFileAsync() or AssetManager.Open(). |

### Workarounds

- Place images in Resources/Raw (MauiAsset) instead of Resources/Images (MauiImage), then use FileSystem.OpenAppPackageFileAsync("image.png") to get a Stream and pass to SKImage.FromEncodedData(Stream) or SKBitmap.Decode(Stream).
- For fonts placed as MauiFont (Resources/Fonts), use FileSystem.OpenAppPackageFileAsync("MyFont.ttf") and pass the resulting Stream to SKTypeface.FromStream(Stream).
- Alternatively, on Android you can use the platform-specific AssetManager.Open("image.png") to get a Stream.

### Resolution Proposals

**Hypothesis:** Reporter is using file-path APIs on Android for resources that are packaged inside the APK and not accessible via the filesystem. The correct approach is to obtain a Stream from MAUI's FileSystem API and use the Stream-based SkiaSharp overloads.

1. **Use MAUI FileSystem to load resources as streams** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Place assets in Resources/Raw (build action: MauiAsset) and use MAUI's cross-platform FileSystem.OpenAppPackageFileAsync() to open a stream, then pass it to SkiaSharp's stream-based APIs.

```csharp
// Images: place in Resources/Raw as MauiAsset
using var stream = await FileSystem.OpenAppPackageFileAsync("image.png");
using var bitmap = SKBitmap.Decode(stream);

// Fonts: place in Resources/Fonts as MauiFont
using var fontStream = await FileSystem.OpenAppPackageFileAsync("MyFont.ttf");
using var typeface = SKTypeface.FromStream(fontStream);
```
2. **Android-specific: use AssetManager** — alternative, confidence 0.85 (85%), cost/xs, validated=yes
   - On Android specifically, use the platform AssetManager to open assets placed in Resources/Raw.

```csharp
#if ANDROID
using var stream = Android.App.Application.Context.Assets.Open("image.png");
using var bitmap = SKBitmap.Decode(stream);
#endif
```

**Recommended proposal:** Use MAUI FileSystem to load resources as streams

**Why:** Cross-platform approach using MAUI's abstraction, no platform-specific code needed. The FileSystem.OpenAppPackageFileAsync() API is the official MAUI way to access packaged assets on all platforms.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.87 (87%) |
| Reason | SkiaSharp works correctly — the issue is Android's resource packaging system. Reporter is using file-path APIs for resources only accessible via Android AssetManager or MAUI's FileSystem.OpenAppPackageFileAsync(). Community member already provided the correct answer. The regression claim (2.88.2 → 2.88.3) is not credible for this category of problem. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Correct label from type/bug to type/question, add os/Android | labels=type/question, area/SkiaSharp, os/Android |
| add-comment | high | 0.87 (87%) | Post answer explaining Android resource access with SkiaSharp and stream-based APIs | — |
| close-issue | medium | 0.82 (82%) | Close as not a SkiaSharp bug — platform resource loading question answered | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for filing this! As @xtuzy pointed out, this is expected Android behavior rather than a SkiaSharp bug.

On Android, resources placed in `Resources/Images` (MauiImage) or `Resources/Raw` (MauiAsset) are packaged inside the APK and are **not** accessible via regular file-system paths. `File.Exists()` will always return `false` for these assets, and `SKTypeface.FromFile()` / `SKImage.FromEncodedData(string)` won't find them.

The correct approach is to obtain a `Stream` using MAUI's cross-platform API, then use SkiaSharp's stream-based overloads:

```csharp
// Images: place file in Resources/Raw with Build Action = MauiAsset
using var stream = await FileSystem.OpenAppPackageFileAsync("image.png");
using var bitmap = SKBitmap.Decode(stream);

// Fonts: place file in Resources/Fonts with Build Action = MauiFont
using var fontStream = await FileSystem.OpenAppPackageFileAsync("MyFont.ttf");
using var typeface = SKTypeface.FromStream(fontStream);
```

This approach works on Android, iOS, macOS, and Windows with a single code path.

Closing as not a SkiaSharp issue — if you have further questions about MAUI resource loading, the [MAUI documentation on assets](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/single-project#resource-files) has more detail.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2599,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T21:44:09Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "Reporter cannot load bitmaps and fonts from the Resources/Images folder on Android (MAUI), receiving a file-not-found exception; the same code works on Windows. Community member identified this as Android resource system behavior, not a SkiaSharp issue.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.87
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.85
    },
    "platforms": [
      "os/Android"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a MAUI app with bitmaps in Resources/Images and fonts in Resources/Fonts",
        "Try to load them using file paths with File.Exists() on Android",
        "Observe file-not-found exception on Android"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Android Emulator, Visual Studio on Windows",
      "attachments": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/12542816/Cyclone.zip",
          "description": "Reporter's sample project",
          "filename": "Cyclone.zip"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.2"
      ],
      "workedIn": "2.88.2",
      "brokeIn": "2.88.3",
      "currentRelevance": "unlikely",
      "relevanceReason": "The claimed regression from 2.88.2 to 2.88.3 is unlikely to be caused by SkiaSharp changes; Android resource system behavior is platform-fundamental and unaffected by SkiaSharp version. Reporter likely made a setup or project configuration change alongside the upgrade."
    }
  },
  "analysis": {
    "summary": "The reporter is attempting to load MAUI resources (images/fonts) via standard file paths, which works on Windows but fails on Android because MAUI's Resources/Images and Resources/Raw folders are packaged as Android resources/assets — not accessible via regular filesystem paths. SkiaSharp itself works correctly when given valid stream or file path inputs; the fix is to use MAUI's FileSystem.OpenAppPackageFileAsync() to obtain a stream, then pass it to SKImage.FromEncodedData(Stream) or SKTypeface.FromStream(Stream). A community member correctly identified this as Android resource system behavior, not a SkiaSharp bug.",
    "rationale": "Reporter is confused about Android's resource packaging model. File.Exists() returns false on Android for resources stored in the APK because they are not exposed as regular filesystem paths. SKTypeface.FromFile() and SKImage.FromEncodedData(string) both require real filesystem paths. Community comment #1709260086 confirms the root cause and provides the correct approach. The regression claim (2.88.2 → 2.88.3) is not credible for this category of issue — Android resource access has not changed in SkiaSharp across those versions.",
    "keySignals": [
      {
        "text": "I checked the file with File.Exist a function for the file, but I could not find the file name.",
        "source": "issue body",
        "interpretation": "Using standard filesystem API on Android for packaged resources — these are not exposed as file paths in Android."
      },
      {
        "text": "It works fine with Windows.",
        "source": "issue body",
        "interpretation": "Windows exposes project resources via the file system; Android does not — classic cross-platform resource access mismatch."
      },
      {
        "text": "This is not error of skiasharp, Maui deal with Resources as native resource, every platform is different",
        "source": "comment by xtuzy (comment #1709260086)",
        "interpretation": "Community member correctly identifies this as Android resource system behavior, not a SkiaSharp defect."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "89-90",
        "finding": "SKTypeface.FromFile(string path) delegates to SKFontManager.Default.CreateTypeface(path, index) — requires a valid filesystem path; cannot directly accept Android asset paths.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "223-233",
        "finding": "SKImage.FromEncodedData(string filename) creates an SKData from the filesystem path via SKData.Create(filename) — returns null if the file is not on the real filesystem (as is the case for Android APK assets).",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "92-93",
        "finding": "SKTypeface.FromStream(Stream stream) exists as a public API accepting a managed Stream — this is the correct API to use when loading from an Android AssetManager or MAUI FileSystem stream.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "211-219",
        "finding": "SKImage.FromEncodedData(Stream data) accepts a managed Stream — correct API for Android when combined with FileSystem.OpenAppPackageFileAsync() or AssetManager.Open().",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Place images in Resources/Raw (MauiAsset) instead of Resources/Images (MauiImage), then use FileSystem.OpenAppPackageFileAsync(\"image.png\") to get a Stream and pass to SKImage.FromEncodedData(Stream) or SKBitmap.Decode(Stream).",
      "For fonts placed as MauiFont (Resources/Fonts), use FileSystem.OpenAppPackageFileAsync(\"MyFont.ttf\") and pass the resulting Stream to SKTypeface.FromStream(Stream).",
      "Alternatively, on Android you can use the platform-specific AssetManager.Open(\"image.png\") to get a Stream."
    ],
    "resolution": {
      "hypothesis": "Reporter is using file-path APIs on Android for resources that are packaged inside the APK and not accessible via the filesystem. The correct approach is to obtain a Stream from MAUI's FileSystem API and use the Stream-based SkiaSharp overloads.",
      "proposals": [
        {
          "title": "Use MAUI FileSystem to load resources as streams",
          "description": "Place assets in Resources/Raw (build action: MauiAsset) and use MAUI's cross-platform FileSystem.OpenAppPackageFileAsync() to open a stream, then pass it to SkiaSharp's stream-based APIs.",
          "category": "workaround",
          "codeSnippet": "// Images: place in Resources/Raw as MauiAsset\nusing var stream = await FileSystem.OpenAppPackageFileAsync(\"image.png\");\nusing var bitmap = SKBitmap.Decode(stream);\n\n// Fonts: place in Resources/Fonts as MauiFont\nusing var fontStream = await FileSystem.OpenAppPackageFileAsync(\"MyFont.ttf\");\nusing var typeface = SKTypeface.FromStream(fontStream);",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Android-specific: use AssetManager",
          "description": "On Android specifically, use the platform AssetManager to open assets placed in Resources/Raw.",
          "category": "alternative",
          "codeSnippet": "#if ANDROID\nusing var stream = Android.App.Application.Context.Assets.Open(\"image.png\");\nusing var bitmap = SKBitmap.Decode(stream);\n#endif",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use MAUI FileSystem to load resources as streams",
      "recommendedReason": "Cross-platform approach using MAUI's abstraction, no platform-specific code needed. The FileSystem.OpenAppPackageFileAsync() API is the official MAUI way to access packaged assets on all platforms."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.87,
      "reason": "SkiaSharp works correctly — the issue is Android's resource packaging system. Reporter is using file-path APIs for resources only accessible via Android AssetManager or MAUI's FileSystem.OpenAppPackageFileAsync(). Community member already provided the correct answer. The regression claim (2.88.2 → 2.88.3) is not credible for this category of problem.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct label from type/bug to type/question, add os/Android",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Android"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer explaining Android resource access with SkiaSharp and stream-based APIs",
        "risk": "high",
        "confidence": 0.87,
        "comment": "Thanks for filing this! As @xtuzy pointed out, this is expected Android behavior rather than a SkiaSharp bug.\n\nOn Android, resources placed in `Resources/Images` (MauiImage) or `Resources/Raw` (MauiAsset) are packaged inside the APK and are **not** accessible via regular file-system paths. `File.Exists()` will always return `false` for these assets, and `SKTypeface.FromFile()` / `SKImage.FromEncodedData(string)` won't find them.\n\nThe correct approach is to obtain a `Stream` using MAUI's cross-platform API, then use SkiaSharp's stream-based overloads:\n\n```csharp\n// Images: place file in Resources/Raw with Build Action = MauiAsset\nusing var stream = await FileSystem.OpenAppPackageFileAsync(\"image.png\");\nusing var bitmap = SKBitmap.Decode(stream);\n\n// Fonts: place file in Resources/Fonts with Build Action = MauiFont\nusing var fontStream = await FileSystem.OpenAppPackageFileAsync(\"MyFont.ttf\");\nusing var typeface = SKTypeface.FromStream(fontStream);\n```\n\nThis approach works on Android, iOS, macOS, and Windows with a single code path.\n\nClosing as not a SkiaSharp issue — if you have further questions about MAUI resource loading, the [MAUI documentation on assets](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/single-project#resource-files) has more detail."
      },
      {
        "type": "close-issue",
        "description": "Close as not a SkiaSharp bug — platform resource loading question answered",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
