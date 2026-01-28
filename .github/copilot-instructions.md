# SkiaSharp Documentation Migration: Xamarin.Forms to .NET MAUI

## Migration Status Dashboard

**Total Markdown Files**: 54
**Migration Status**: ✅ COMPLETE (2026-01-28)

### Progress Tracking

| Section | Files | Status |
|---------|-------|--------|
| Root docs | 1 (index.md) | ✅ Complete |
| basics/ | 7 files | ✅ Complete |
| paths/ | 7 files | ✅ Complete |
| transforms/ | 10 files | ✅ Complete |
| curves/ | 9 files | ✅ Complete |
| bitmaps/ | 9 files | ✅ Complete |
| effects/ | 11 files | ✅ Complete |

### Remaining "Xamarin.Forms" References
✅ **None** - All Xamarin references have been removed or updated.

### Sample Links Migration (Completed 2026-01-28)
All links pointing to `https://github.com/xamarin/xamarin-forms-samples/blob/master/SkiaSharpForms/...` have been updated to use local relative paths (`../../../samples/...`).

Book references to "Creating Mobile Apps with Xamarin.Forms" have been removed, with conceptual content preserved.

---

## Key Migration Patterns

### 1. YAML Frontmatter Changes

**Change `ms.service`:**
```yaml
# FROM:
ms.service: xamarin
ms.subservice: xamarin-skiasharp

# TO:
ms.service: dotnet-maui
ms.subservice: skiasharp
```

**Remove `no-loc` directive:**
```yaml
# REMOVE this line:
no-loc: [Xamarin.Forms, Xamarin.Essentials]
```

**Update descriptions (title, description fields) - replace "Xamarin.Forms" with ".NET MAUI"**

### 2. C# Namespace Changes

```csharp
// FROM:
using SkiaSharp.Views.Forms;

// TO:
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
```

### 3. XAML Namespace Changes

```xml
<!-- FROM: -->
xmlns="http://xamarin.com/schemas/2014/forms"
xmlns:skia="clr-namespace:SkiaSharp.Views.Forms;assembly=SkiaSharp.Views.Forms"

<!-- TO: -->
xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
xmlns:skia="clr-namespace:SkiaSharp.Views.Maui.Controls;assembly=SkiaSharp.Views.Maui.Controls"
```

### 4. NuGet Package References

```text
FROM: SkiaSharp.Views.Forms
TO:   SkiaSharp.Views.Maui.Controls
```

### 5. Color API Changes (in code examples)

```csharp
// FROM (Xamarin.Forms Colors):
Color.Red.ToSKColor()

// TO (.NET MAUI Colors):
Colors.Red.ToSKColor()
```

Note: `SKColors.Red` (SkiaSharp native) remains unchanged.

### 6. Initialization (for intro docs)

```csharp
// .NET MAUI requires explicit initialization in MauiProgram.cs:
using SkiaSharp.Views.Maui.Controls.Hosting;

public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiApp<App>()
        .UseSkiaSharp()  // <-- Required for .NET MAUI
        ...
}
```

### 7. Text/Prose Replacements

| From | To |
|------|-----|
| Xamarin.Forms | .NET MAUI |
| Xamarin.Forms application(s) | .NET MAUI application(s) |
| Xamarin.Forms app(s) | .NET MAUI app(s) |
| Xamarin.Forms solution | .NET MAUI solution |
| Visual Studio for Mac | Visual Studio (Mac support dropped in .NET MAUI 8+) |
| Xamarin.Essentials | (integrated into .NET MAUI, remove reference) |

### 8. Link/URL Updates

```markdown
<!-- FROM: -->
[xamarin-forms-samples](https://github.com/xamarin/xamarin-forms-samples/...)

<!-- TO: -->
[maui-samples](https://github.com/dotnet/maui-samples/...)
```

**Note**: Sample links may need verification - samples may not exist yet in MAUI format.

### 9. Cross-Reference (xref) Updates

```markdown
<!-- FROM: -->
xref:Xamarin.Forms.View
xref:Xamarin.Forms.TapGestureRecognizer
xref:SkiaSharp.Views.Forms.SKCanvasView

<!-- TO: -->
xref:Microsoft.Maui.Controls.View
xref:Microsoft.Maui.Controls.TapGestureRecognizer
xref:SkiaSharp.Views.Maui.Controls.SKCanvasView
```

---

## Files By Section

### Root (docs/)
- [ ] `index.md` - Main overview, heavy Xamarin.Forms references

### basics/ (7 files)
- [ ] `index.md` - Section index
- [ ] `circle.md` - Drawing basics, has XAML/C# examples
- [ ] `integration.md` - **CRITICAL** - Heavy Xamarin.Forms integration content
- [ ] `pixels.md` - Coordinate systems
- [ ] `animation.md` - Animation patterns
- [ ] `text.md` - Text rendering
- [ ] `bitmaps.md` - Bitmap basics
- [ ] `transparency.md` - Alpha/transparency

### paths/ (7 files)
- [ ] `index.md` - Section index
- [ ] `paths.md` - Path fundamentals
- [ ] `lines.md` - Line drawing
- [ ] `polylines.md` - Polyline patterns
- [ ] `dots.md` - Dotted lines
- [ ] `fill-types.md` - Fill rules
- [ ] `finger-paint.md` - Touch painting example

### transforms/ (10 files)
- [ ] `index.md` - Section index (**heavy Xamarin.Forms references**)
- [ ] `translate.md` - Translation transforms
- [ ] `scale.md` - Scale transforms
- [ ] `rotate.md` - Rotation transforms
- [ ] `skew.md` - Skew transforms
- [ ] `matrix.md` - Matrix transforms
- [ ] `3d-rotation.md` - 3D rotation
- [ ] `non-affine.md` - Non-affine transforms
- [ ] `touch.md` - **CRITICAL** - Touch manipulation (**heavy Xamarin.Forms**)

### curves/ (9 files)
- [ ] `index.md` - Section index
- [ ] `arcs.md` - Arc drawing
- [ ] `beziers.md` - Bezier curves
- [ ] `path-data.md` - SVG path data
- [ ] `clipping.md` - Clipping paths
- [ ] `effects.md` - Path effects
- [ ] `information.md` - Path information
- [ ] `text-paths.md` - Text on paths

### bitmaps/ (9 files)
- [ ] `index.md` - Section index
- [ ] `displaying.md` - Display bitmaps
- [ ] `drawing.md` - Draw on bitmaps
- [ ] `cropping.md` - Crop bitmaps (**heavy Xamarin.Forms**)
- [ ] `animating.md` - Animate bitmaps
- [ ] `pixel-bits.md` - Pixel manipulation
- [ ] `saving.md` - Save bitmaps
- [ ] `segmented.md` - Nine-patch/segmented display

### effects/ (11 files)
- [ ] `index.md` - Section index
- [ ] `color-filters.md` - Color filters
- [ ] `mask-filters.md` - Mask/blur filters
- [ ] `image-filters.md` - Image filters (**multiple XAML examples**)
- [ ] `blend-modes/index.md` - Blend modes overview
- [ ] `blend-modes/porter-duff.md` - Porter-Duff modes
- [ ] `blend-modes/separable.md` - Separable blend modes
- [ ] `blend-modes/non-separable.md` - Non-separable blend modes
- [ ] `shaders/index.md` - Shaders overview
- [ ] `shaders/linear-gradient.md` - Linear gradients
- [ ] `shaders/circular-gradients.md` - Radial gradients
- [ ] `shaders/bitmap-tiling.md` - Bitmap tiling
- [ ] `shaders/noise.md` - Perlin noise

---

## Migration Strategy

### Phase 1: Research & Preparation (CURRENT)
- [x] Inventory all files
- [x] Document patterns to change
- [x] Research .NET MAUI equivalents via mslearn
- [ ] Create migration script/checklist per file

### Phase 2: Migrate High-Impact Files First
1. `docs/index.md` - Sets the tone for entire docs
2. `docs/docs/basics/index.md` - Entry point for users
3. `docs/docs/basics/integration.md` - Critical Xamarin.Forms content

### Phase 3: Section-by-Section Migration
Order by complexity (simplest first):
1. basics/ - Foundation content
2. paths/ - Core drawing
3. transforms/ - Transform operations
4. curves/ - Curve operations  
5. bitmaps/ - Bitmap handling
6. effects/ - Advanced effects

### Phase 4: Verification & Cleanup
- [ ] Verify all links work
- [ ] Check code samples compile conceptually
- [ ] Update TOC.yml if needed
- [ ] Update contributing-guidelines if needed

---

## Agent Task Distribution Strategy

For managing context window limits:

### Use `task` agents with `agent_type: "general-purpose"` for:
- Files with >500 lines of complex content
- Files requiring deep understanding of both old and new APIs

### Use batch processing:
- Process 3-5 simple files per task agent
- Each agent gets clear instructions + migration patterns

### Model Selection:
- **claude-sonnet-4** (default): Good balance for most migration tasks
- **claude-opus-4.5**: Complex files with nuanced content decisions
- **claude-haiku-4.5**: Simple metadata-only changes

---

## Quality Checklist Per File

- [ ] YAML frontmatter updated (ms.service, description, etc.)
- [ ] C# `using` statements updated
- [ ] XAML namespaces updated  
- [ ] Color API calls updated (Color.X → Colors.X)
- [ ] All "Xamarin.Forms" text → ".NET MAUI"
- [ ] Sample repository links updated or marked for update
- [ ] xref links updated
- [ ] No broken internal links
- [ ] Code examples are syntactically correct

---

## Notes & Decisions Log

*Record important decisions and findings here during migration*

- **2024-XX-XX**: Migration plan created
- Sample links: Many point to xamarin-forms-samples which may not have MAUI equivalents. Decision: Update URL pattern but may need to note "samples being migrated" if not available.

---

## Reference Links

- [.NET MAUI SkiaSharp Migration Guide](https://learn.microsoft.com/en-us/dotnet/maui/migration/skiasharp)
- [Xamarin.Forms to .NET MAUI Migration](https://learn.microsoft.com/en-us/dotnet/maui/migration/)
- [SkiaSharp.Views.Maui.Controls NuGet](https://www.nuget.org/packages/SkiaSharp.Views.Maui.Controls/)
