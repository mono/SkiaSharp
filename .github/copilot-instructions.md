# SkiaSharp Documentation - Copilot Instructions

This repository contains the conceptual documentation for **SkiaSharp**, a cross-platform 2D graphics library for .NET. These docs help developers integrate SkiaSharp with **.NET MAUI** applications.

---

## Repository Overview

### Structure

```
docs/
├── index.md                    # Main entry point
├── docfx.json                  # DocFX build configuration
├── TOC.yml                     # Top-level navigation
├── xrefmaps/                   # Cross-reference maps for API linking
├── images/                     # Site-wide assets (logo, favicon)
└── docs/
    ├── basics/                 # Drawing fundamentals (8 files)
    ├── paths/                  # Lines and paths (7 files)
    ├── transforms/             # Geometric transforms (9 files)
    ├── curves/                 # Beziers, arcs, path effects (8 files)
    ├── bitmaps/                # Image handling (8 files)
    └── effects/                # Visual effects (13 files)
        ├── blend-modes/        # Compositing modes
        └── shaders/            # Gradients and patterns

contributing-guidelines/
├── voice-tone.md               # Writing style guide
└── template.md                 # Markdown conventions

samples/                        # Sample code (separate from docs)
```

### Key Files

| File | Purpose |
|------|---------|
| `docs/docfx.json` | Build configuration, xref sources, templates |
| `docs/TOC.yml` | Navigation structure |
| `.markdownlint.json` | Markdown linting rules |

---

## Writing Guidelines

### Voice and Tone

Follow the guidelines in `contributing-guidelines/voice-tone.md`:

1. **Conversational tone** - Write as if explaining to a colleague
2. **Second person** - Use "you" to address the reader directly
3. **Active voice** - Subject performs the action ("SkiaSharp draws the circle")
4. **5th grade reading level** - Simple sentences, international audience

**Example (Good):**
> You can draw a circle by calling the `DrawCircle` method on the canvas.

**Example (Avoid):**
> The DrawCircle method may be invoked on the canvas object when circle rendering is required by the application.

### Microsoft Learn Style

Follow [Microsoft Learn style guidelines](https://learn.microsoft.com/contribute/content/style-quick-start):

- Focus on the reader's intent
- Use everyday words
- Write concisely
- Make content easy to scan
- Show empathy

---

## Markdown Conventions

### YAML Frontmatter (Required)

Every article must have this frontmatter:

```yaml
---
title: "Article Title"
description: "A clear description of what readers will learn (60-160 chars)"
ms.service: dotnet-maui
ms.assetid: 00000000-0000-0000-0000-000000000000  # Unique GUID
author: authorname
ms.author: alias
ms.date: MM/DD/YYYY
---
```

| Field | Required | Description |
|-------|----------|-------------|
| `title` | Yes | Browser tab/search result title (≤60 chars) |
| `description` | Yes | Search snippet (60-160 chars) |
| `ms.service` | Yes | Always `dotnet-maui` for this repo |
| `ms.assetid` | Yes | Unique GUID for BI tracking |
| `author` | Yes | GitHub username |
| `ms.author` | Yes | Microsoft alias |
| `ms.date` | Yes | Last significant update (MM/DD/YYYY) |

> [!TIP]
> Generate GUIDs at https://www.guidgenerator.com/ or use PowerShell: `[guid]::NewGuid().ToString().ToUpper()`

### Lead Paragraph

After the H1, include an italicized summary paragraph:

```markdown
# Drawing a Simple Circle in SkiaSharp

_Learn the basics of SkiaSharp drawing, including canvases and paint objects_
```

This provides a scannable summary distinct from the `description` in frontmatter.

### Headings

- **One H1 per file** - Matches or relates to the `title` in frontmatter
- **Sentence case** - "Drawing a simple circle" not "Drawing a Simple Circle"
- **No trailing punctuation** - Except "?" for questions
- Use H2 (`##`) for main sections, H3 (`###`) for subsections

### Code Blocks

Always specify the language:

```csharp
// C# code
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;

SKCanvas canvas = surface.Canvas;
canvas.Clear(SKColors.White);
```

```xaml
<!-- XAML markup -->
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:skia="clr-namespace:SkiaSharp.Views.Maui.Controls;assembly=SkiaSharp.Views.Maui.Controls">
    <skia:SKCanvasView PaintSurface="OnPaintSurface" />
</ContentPage>
```

### Alerts

Use DocFX alert syntax for callouts:

```markdown
> [!NOTE]
> Additional information that's helpful but not critical.

> [!TIP]
> Suggestions that can improve the reader's experience.

> [!IMPORTANT]
> Critical information required for success.

> [!WARNING]
> Information about potential problems or breaking changes.
```

---

## API Cross-References (xref)

### Syntax Options

```markdown
<!-- Standard link -->
[SKCanvas](xref:SkiaSharp.SKCanvas)

<!-- Auto-link (displays full type name) -->
<xref:SkiaSharp.SKCanvas>

<!-- Method with overloads -->
[DrawCircle](xref:SkiaSharp.SKCanvas.DrawCircle*)
```

### Common SkiaSharp Types

| Type | xref |
|------|------|
| Canvas | `xref:SkiaSharp.SKCanvas` |
| Paint | `xref:SkiaSharp.SKPaint` |
| Path | `xref:SkiaSharp.SKPath` |
| Bitmap | `xref:SkiaSharp.SKBitmap` |
| Image | `xref:SkiaSharp.SKImage` |
| Surface | `xref:SkiaSharp.SKSurface` |
| Color | `xref:SkiaSharp.SKColor` |
| ImageInfo | `xref:SkiaSharp.SKImageInfo` |

### MAUI Integration Types

| Type | xref |
|------|------|
| SKCanvasView | `xref:SkiaSharp.Views.Maui.Controls.SKCanvasView` |
| SKGLView | `xref:SkiaSharp.Views.Maui.Controls.SKGLView` |
| SKPaintSurfaceEventArgs | `xref:SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs` |
| SKTouchEventArgs | `xref:SkiaSharp.Views.Maui.SKTouchEventArgs` |

### .NET MAUI Types

| Type | xref |
|------|------|
| View | `xref:Microsoft.Maui.Controls.View` |
| ContentPage | `xref:Microsoft.Maui.Controls.ContentPage` |
| TapGestureRecognizer | `xref:Microsoft.Maui.Controls.TapGestureRecognizer` |
| VisualElement | `xref:Microsoft.Maui.Controls.VisualElement` |
| Color | `xref:Microsoft.Maui.Graphics.Color` |
| Aspect | `xref:Microsoft.Maui.Aspect` |

---

## Images

### Directory Structure

Images live alongside articles in `*-images/` folders:

```
docs/basics/
├── circle.md
└── circle-images/
    ├── circleexample.png         # Main screenshot
    ├── simplecircle-large.png    # Full-size composite
    ├── simplecircle-small.png    # Thumbnail composite
    ├── simplecircle.a.png        # Android screenshot
    ├── simplecircle.i.png        # iOS screenshot
    └── simplecircle.u.png        # UWP/Windows screenshot
```

### Naming Convention

- `{name}.png` - Single screenshot
- `{name}-large.png` / `{name}-small.png` - Composite images
- `{name}.a.png` - Android
- `{name}.i.png` - iOS  
- `{name}.u.png` - Windows/UWP

### Markdown Syntax

```markdown
![A blue circle outlined in red](circle-images/circleexample.png)
```

- Use descriptive alt text
- Relative paths from the article file

### Responsive Screenshots (Triple-Platform)

For platform comparison images, use thumbnail linking to full-size:

```markdown
[![Triple screenshot of Basic Rotate](rotate-images/basicrotate-small.png)](rotate-images/basicrotate-large.png#lightbox "Triple screenshot of Basic Rotate")
```

- `#lightbox` suffix enables zoom on click
- Alt text should describe what's shown

---

## Links

### Internal Links

```markdown
<!-- Same folder -->
[next topic](integration.md)

<!-- Different folder -->
[shaders section](../effects/shaders/index.md)

<!-- Anchor link within same file -->
[transform section](#the-rotate-transform)
```

Always use relative paths with `.md` extension.

### External Links

```markdown
[SkiaSharp API Reference](https://learn.microsoft.com/dotnet/api/skiasharp)
```

---

## Sample Code Links

Link to samples in the `samples/` directory using GitHub URLs:

```markdown
[`SimpleCirclePage`](https://github.com/mono/SkiaSharp/blob/docs/samples/Demos/Demos/SkiaSharpFormsDemos/Basics/SimpleCirclePage.cs)
```

Sample structure mirrors doc sections:
- `samples/Demos/Demos/SkiaSharpFormsDemos/Basics/` → `docs/basics/`
- `samples/Demos/Demos/SkiaSharpFormsDemos/Transforms/` → `docs/transforms/`
- etc.

---

## Code Patterns

### Standard Paint Surface Handler

```csharp
void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
{
    SKImageInfo info = args.Info;
    SKSurface surface = args.Surface;
    SKCanvas canvas = surface.Canvas;

    canvas.Clear();
    
    // Drawing code here
}
```

### MAUI Initialization (MauiProgram.cs)

```csharp
using SkiaSharp.Views.Maui.Controls.Hosting;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp();  // Required!
        return builder.Build();
    }
}
```

### XAML Page Structure

```xaml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:skia="clr-namespace:SkiaSharp.Views.Maui.Controls;assembly=SkiaSharp.Views.Maui.Controls"
             x:Class="MyApp.MyPage">
    <skia:SKCanvasView PaintSurface="OnCanvasViewPaintSurface" />
</ContentPage>
```

### Color Conversion (MAUI → SkiaSharp)

```csharp
// Convert .NET MAUI Colors to SkiaSharp
SKColor skColor = Colors.Red.ToSKColor();
SKColor skColor = Colors.FromRgba(255, 128, 0, 255).ToSKColor();

// Use SkiaSharp's built-in colors
SKColor skColor = SKColors.CornflowerBlue;
```

### Disposable Objects

Wrap `SKPaint`, `SKPath`, `SKImage` in `using` statements:

```csharp
// Preferred: using statement for proper disposal
using (SKPaint paint = new SKPaint { Style = SKPaintStyle.Fill })
{
    canvas.DrawCircle(100, 100, 50, paint);
}

// Or C# 8+ using declaration:
using SKPaint paint = new SKPaint { Style = SKPaintStyle.Fill };
canvas.DrawCircle(100, 100, 50, paint);
```

---

## Article Structure

### Standard Article Template

```markdown
---
title: "Topic Title"
description: "What readers will learn (60-160 chars)"
ms.service: dotnet-maui
ms.assetid: GUID-HERE
author: username
ms.author: alias
ms.date: MM/DD/YYYY
---

# Topic Title

_Brief summary of what this article covers_

Introduction paragraph explaining the concept...

## First Major Section

Content...

### Subsection (if needed)

More content...

## Related Links

- [SkiaSharp APIs](https://learn.microsoft.com/dotnet/api/skiasharp)
- [Next Related Topic](next-topic.md)
```

### Adding Articles to Navigation

Edit the appropriate `TOC.yml`:

```yaml
# docs/docs/basics/TOC.yml
- name: Drawing a Simple Circle
  href: circle.md
- name: Integrating with .NET MAUI
  href: integration.md
```

Nested sections use `items:`:

```yaml
- name: Shaders
  href: index.md
  items:
    - name: Linear Gradients
      href: linear-gradient.md
```

---

## Building the Docs

### Prerequisites

```bash
dotnet tool install -g docfx
```

### Build Command

```bash
cd docs
dotnet docfx docfx.json
```

### Output

Built site appears in `_site/` directory (gitignored).

### Cross-Reference Resolution

The build uses xrefmap files in `docs/xrefmaps/`:

| File | Contents |
|------|----------|
| `xrefmap-dotnet.zip` | .NET BCL types (System.*, etc.) |
| `xrefmap-maui.zip` | Microsoft.Maui.* types |
| `xrefmap-skiasharp.zip` | SkiaSharp & HarfBuzzSharp APIs |
| `xrefmap-ios.zip` | UIKit, CoreGraphics, AppKit |
| `xrefmap-android.zip` | Android.*, Java.* |

If xrefs don't resolve, verify the type exists in these maps.

### Regenerating Xrefmap Files

If xrefmap files need to be updated (e.g., new .NET version, new APIs), use Microsoft's buildapi endpoint:

```
https://buildapi.docs.microsoft.com/v1/xrefmap/dotnet?site_name=Docs&branch_name=live
```

> [!IMPORTANT]
> The URLs in the response are **time-limited signed Azure blob URLs**. They expire after a few hours. Download immediately after fetching.

#### How the current xrefmaps were obtained (January 2026):

1. **Fetch the index** - The URL returns JSON with a `links` array containing ~116 xrefmap file URLs

2. **Identify needed files** - Search through the links for files containing relevant types:
   - File containing "SkiaSharp" → `xrefmap-skiasharp.zip`
   - File containing "Microsoft.Maui" → `xrefmap-maui.zip`
   - File containing "UIKit" or "CoreGraphics" → `xrefmap-ios.zip`
   - File containing "Android." namespace → `xrefmap-android.zip`
   - Main .NET file (largest, ~300MB uncompressed) → `xrefmap-dotnet.zip`

3. **Download and inspect** - Files are gzip-compressed JSON. Decompress to verify contents:
   ```bash
   curl -o temp.json.gz "SIGNED_URL_HERE"
   gunzip temp.json.gz
   grep -l "SkiaSharp" temp.json  # Check if it has what you need
   ```

4. **Create DocFX-compatible zips** - DocFX requires a specific zip structure:
   ```bash
   # Convert JSON to zip format DocFX expects
   dotnet docfx download xrefmap-skiasharp.zip --xref "file:///path/to/downloaded.json"
   ```
   
   The zip must contain a file named `xrefmap.yml` (DocFX converts JSON to YAML internally).

5. **Replace files** in `docs/xrefmaps/` and test build

#### File positions in buildapi response (as of January 2026):

These positions **will change** over time. Always search by content, not position:

| Approx Position | Content | Notes |
|-----------------|---------|-------|
| ~68 | SkiaSharp, HarfBuzzSharp | Primary SkiaSharp xref |
| ~91 | Microsoft.Maui.* | MAUI controls and types |
| ~106 | Android.*, Java.* | ~150MB uncompressed |
| ~116 | UIKit, CoreGraphics, AppKit | iOS/Mac types, ~102MB |
| First/largest | System.*, Microsoft.* | .NET BCL, ~332MB |

#### Verification

After regenerating, run a build and check for xref warnings:

```bash
cd docs
dotnet docfx docfx.json 2>&1 | grep -i "warning"
```

Common issues:
- Type moved to different namespace → Update xref in docs
- Type removed from API → Remove xref or use plain text
- Wrong xrefmap file → Search all downloaded files for the type

---

## File Naming Rules

From `contributing-guidelines/template.md`:

- **Lowercase only**
- **Hyphens for spaces** - `drawing-basics.md` not `drawing basics.md`
- **No small words** - Omit "a", "the", "and", "or"
- **Action verbs** - `displaying.md` → `display.md` (no -ing)
- **Keep short** - File names appear in URLs

---

## Common Patterns by Section

### basics/
- Simple "hello world" style tutorials
- One concept per article
- Heavy use of step-by-step instructions

### paths/
- Focus on `SKPath` class methods
- Progressive complexity (lines → polylines → fill rules)
- Touch interaction examples

### transforms/
- Mathematical concepts with visual examples
- Comparison between SkiaSharp and MAUI transforms
- Matrix math explanations

### curves/
- Bezier math with diagrams
- SVG path data syntax
- Path effects and text-on-path

### bitmaps/
- Loading from resources and URLs
- Pixel manipulation
- Platform-specific saving

### effects/
- Color filters, blur, image effects
- Blend modes with visual samples
- Gradient shaders

---

## Quality Checklist

Before submitting changes:

- [ ] YAML frontmatter is complete and valid
- [ ] Title uses sentence case
- [ ] Only one H1 heading in the file
- [ ] Code blocks have language identifiers
- [ ] Images have descriptive alt text
- [ ] Links are relative (not absolute) where possible
- [ ] xref links use correct namespace
- [ ] No trailing whitespace
- [ ] File passes markdownlint
- [ ] Article ends with "Related Links" section

---

## Common Mistakes to Avoid

### ❌ Generic code blocks

```
canvas.Clear();
```

### ✅ Always specify language

```csharp
canvas.Clear();
```

### ❌ Wrong GitHub branch

```markdown
[sample](https://github.com/mono/SkiaSharp/blob/main/samples/...)
```

### ✅ Use the docs branch

```markdown
[sample](https://github.com/mono/SkiaSharp/blob/docs/samples/...)
```

### ❌ Absolute paths for internal links

```markdown
[topic](/Users/matthew/Documents/SkiaSharp/docs/basics/circle.md)
```

### ✅ Relative paths

```markdown
[topic](circle.md)
```

### ❌ Missing alt text

```markdown
![](circle-images/example.png)
```

### ✅ Descriptive alt text

```markdown
![A blue circle with red outline](circle-images/example.png)
```

---

## Quick Reference

### Namespaces

```csharp
// Core SkiaSharp
using SkiaSharp;

// MAUI integration
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharp.Views.Maui.Controls.Hosting;
```

### Key NuGet Package

```
SkiaSharp.Views.Maui.Controls
```

### External Resources

- [SkiaSharp API Reference](https://learn.microsoft.com/dotnet/api/skiasharp)
- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
- [Skia Graphics Library](https://skia.org/)
- [DocFX Documentation](https://dotnet.github.io/docfx/)
